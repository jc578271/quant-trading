package com.bookmap.alertlistener;

import velox.api.layer1.Layer1ApiAdminAdapter;
import velox.api.layer1.Layer1ApiFinishable;
import velox.api.layer1.Layer1ApiProvider;
import velox.api.layer1.annotations.Layer1ApiVersion;
import velox.api.layer1.annotations.Layer1ApiVersionValue;
import velox.api.layer1.annotations.Layer1Attachable;
import velox.api.layer1.annotations.Layer1StrategyName;
import velox.api.layer1.common.ListenableHelper;
import velox.api.layer1.messages.Layer1ApiSoundAlertMessage;
import velox.api.layer1.Layer1ApiDataListener;
import velox.api.layer1.data.MarketMode;
import velox.api.layer1.data.TradeInfo;
import velox.api.layer1.layers.utils.OrderBook;

import javax.swing.*;
import java.awt.*;
import java.util.*;
import java.util.concurrent.*;
import java.io.*;
import java.net.Socket;
import java.nio.file.*;
import java.time.format.DateTimeFormatter;
import java.time.*;

@Layer1Attachable
@Layer1StrategyName("Alert Listener")
@Layer1ApiVersion(Layer1ApiVersionValue.VERSION2)
public class AlertListener implements
        Layer1ApiAdminAdapter,
        Layer1ApiFinishable,
        velox.api.layer1.Layer1CustomPanelsGetter,
        Layer1ApiDataListener {

    private final Layer1ApiProvider provider;
    private final Map<String, java.util.List<String>> alertLogsMap = new ConcurrentHashMap<>();
    private final Map<String, JTextArea> logAreas = new ConcurrentHashMap<>();
    private JLabel countLabel;
    private int alertCount = 0;
    private static final int MAX_LOG_LINES = 500;
    private static final DateTimeFormatter TIME_FMT = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'");

    // Socket for Python AI Analyzer
    private Socket socket;
    private PrintWriter socketWriter;
    private static final String HOST = "127.0.0.1";
    private static final int PORT = 5555;
    private final Object socketLock = new Object();

    // DOM State Tracking: alias -> OrderBook
    // Note: Bookmap automatically backfills historical data from the chart into
    // onDepth/onTrade when the addon starts.
    private final Map<String, OrderBook> domState = new ConcurrentHashMap<>();

    // Files for persistence
    private final File configFile;
    // history files are now per-alias: AlertListener_history_[alias].log

    // Per-instrument settings
    private static class InstrumentSettings {
        int minDotVol = 20;
        int minWallSizeAdded = 100;
        int minWallSizeRemoved = 50;
        int minWallDur = 5;
        int aggWindowMs = 50;
        int maxAggMs = 1000;
    }

    private final Map<String, InstrumentSettings> settingsMap = new ConcurrentHashMap<>();
    private final InstrumentSettings defaultSettings = new InstrumentSettings();

    // Trade Aggregation
    private final ScheduledExecutorService aggregationExecutor = Executors.newSingleThreadScheduledExecutor();
    private final Map<String, AggregatedTrade> tradeBuffer = new ConcurrentHashMap<>();

    // Wall tracking: Key = alias + side + price
    private static class WallState {
        String alias;
        boolean isBid;
        int price;
        long firstSeenTime = 0;
        int accumulatedSize = 0;
        int currentDomSize = 0;
        boolean isLogged = false;

        WallState(String alias, boolean isBid, int price) {
            this.alias = alias;
            this.isBid = isBid;
            this.price = price;
        }
    }
    private final Map<String, WallState> wallStates = new ConcurrentHashMap<>();

    private static class AggregatedTrade {
        String alias;
        double minPrice;
        double maxPrice;
        int buySize;
        int sellSize;
        long lastTime;
        long startTime;
        double sumPriceSize; // To calculate VWAP of all trades in this window

        AggregatedTrade(String alias, double price, boolean isBuy, int size, long currentTimeMs) {
            this.alias = alias;
            this.minPrice = price;
            this.maxPrice = price;
            if (isBuy) {
                this.buySize = size;
            } else {
                this.sellSize = size;
            }
            this.sumPriceSize = price * size;
            this.lastTime = currentTimeMs;
            this.startTime = currentTimeMs;
        }

        void add(double price, boolean isBuy, int size, long currentTimeMs) {
            if (isBuy) {
                this.buySize += size;
            } else {
                this.sellSize += size;
            }
            this.sumPriceSize += price * size;
            this.minPrice = Math.min(this.minPrice, price);
            this.maxPrice = Math.max(this.maxPrice, price);
            this.lastTime = currentTimeMs;
        }

        int getTotalAbsSize() {
            return buySize + sellSize;
        }

        int getDelta() {
            return buySize - sellSize;
        }

        double getVwap() {
            int total = getTotalAbsSize();
            return total == 0 ? 0 : sumPriceSize / total;
        }
    }

    public AlertListener(Layer1ApiProvider provider) {
        this.provider = provider;
        this.configFile = new File(System.getProperty("user.home"), "AlertListener.properties");

        ListenableHelper.addListeners(provider, this);
        System.out.println("=== Alert Listener Started ===");
        System.out.println("Listening for Bookmap Alert Messages from all addons...");

        loadConfig();

        // Start aggregation flusher (check every 10ms)
        aggregationExecutor.scheduleAtFixedRate(() -> {
            flushTrades();
            checkWalls();
        }, 10, 10, TimeUnit.MILLISECONDS);

        reconnectSocket();
        System.out.println("==============================");
    }

    private long getCurrentTimeMs() {
        return provider.getCurrentTime() / 1_000_000L;
    }

    private void reconnectSocket() {
        new Thread(() -> {
            synchronized (socketLock) {
                try {
                    // Close existing if open
                    if (socketWriter != null)
                        socketWriter.close();
                    if (socket != null)
                        socket.close();

                    socket = new Socket(HOST, PORT);
                    socketWriter = new PrintWriter(socket.getOutputStream(), true);
                    String msg = "Successfully connected to Python Socket (ai_analyzer)";
                    System.out.println(msg);
                    logToUI(null, "SYSTEM: " + msg);
                } catch (IOException e) {
                    String err = "Could not connect to Python Socket: " + e.getMessage();
                    System.err.println(err);
                    logToUI(null, "ERROR: " + err);
                    socketWriter = null;
                    socket = null;
                }
            }
        }).start();
    }

    private String logToUI(String alias, String message) {
        long currentMs = getCurrentTimeMs();
        String time = Instant.ofEpochMilli(currentMs).atZone(ZoneOffset.UTC).format(TIME_FMT);
        String fullMsg = "[" + time + "] " + message;
        updateUI(alias, fullMsg);
        appendLogToFile(alias, fullMsg);
        return fullMsg;
    }

    private void appendLogToFile(String alias, String fullMsg) {
        String fileName = alias == null ? "AlertListener_history_system.log"
                : "AlertListener_history_" + sanitize(alias) + ".log";
        File file = new File(System.getProperty("user.home"), fileName);
        try (PrintWriter out = new PrintWriter(new BufferedWriter(new FileWriter(file, true)))) {
            out.println(fullMsg);
        } catch (IOException e) {
            System.err.println("Error writing to history log (" + fileName + "): " + e.getMessage());
        }
    }

    private String sanitize(String alias) {
        return alias.replaceAll("[^a-zA-Z0-9.-]", "_");
    }

    private void updateUI(String alias, String message) {
        SwingUtilities.invokeLater(() -> {
            if (alias == null) {
                // System message: broadcast to all active log areas
                for (JTextArea area : logAreas.values()) {
                    area.append(message + "\n");
                    area.setCaretPosition(area.getDocument().getLength());
                }
            } else {
                JTextArea area = logAreas.get(alias);
                if (area != null) {
                    area.append(message + "\n");
                    area.setCaretPosition(area.getDocument().getLength());
                }
            }
            if (countLabel != null) {
                countLabel.setText("Total Alerts: " + alertCount);
            }
        });
    }

    @Override
    public void onDepth(String alias, boolean isBid, int price, int size) {
        // Get or create OrderBook for this instrument
        OrderBook orderBook = domState.computeIfAbsent(alias, k -> new OrderBook());

        // Get previous size before update
        long prevSize = orderBook.getSizeFor(isBid, price);

        // Update the OrderBook state using the correct Bookmap utility method
        orderBook.onUpdate(isBid, price, size);

        // Detect Add/Remove for general DOM events (size is now the NEW size)
        String action = null;
        if (prevSize == 0 && size > 0) {
            action = "add";
        } else if (prevSize > 0 && size == 0) {
            action = "remove";
        }

        // Send to Socket if action detected (simple add/remove to new price level)
        if (action != null) {
            sendDomEvent(action, alias, isBid, price, size);
        }

        // Logic for Liquidity Wall Detection
        String wallKey = alias + (isBid ? "BID" : "ASK") + price;
        int delta = (int) (size - prevSize);

        InstrumentSettings settings = settingsMap.getOrDefault(alias, defaultSettings);

        WallState state = wallStates.computeIfAbsent(wallKey, k -> new WallState(alias, isBid, price));
        synchronized (state) {
            state.accumulatedSize += delta;
            if (state.accumulatedSize < 0) {
                state.accumulatedSize = 0;
            }
            state.currentDomSize = size;

            // Wall Addition Detection: Threshold reached and some liquidity exists
            if (state.accumulatedSize >= settings.minWallSizeAdded && size > 0) {
                if (state.firstSeenTime == 0) {
                    state.firstSeenTime = getCurrentTimeMs();
                }
            }

            // Wall Removal Detection: size drops below threshold
            if (size < settings.minWallSizeRemoved) {
                if (state.isLogged) {
                    String wallMsg = String.format("[WALL REMOVED] %s | %s | Price: %d | Current Size: %d | Threshold: %d",
                            alias, isBid ? "BID" : "ASK", price, size, settings.minWallSizeRemoved);
                    logToUI(alias, wallMsg);
                    sendWallEvent("removed", alias, isBid, price, size, 0);
                }
                wallStates.remove(wallKey);
            }
        }
    }

    private void sendDomEvent(String action, String alias, boolean isBid, int price, int size) {
        synchronized (socketLock) {
            if (socketWriter != null) {
                String json = String.format(
                        "{\"type\":\"dom\", \"action\":\"%s\", \"alias\":\"%s\", \"isBid\":%b, \"price\":%d, \"size\":%d}",
                        action, alias, isBid, price, size);
                socketWriter.println(json);
            }
        }
    }

    private void sendWallEvent(String action, String alias, boolean isBid, int price, int size, long duration) {
        synchronized (socketLock) {
            if (socketWriter != null) {
                String json = String.format(
                        "{\"type\":\"wall\", \"action\":\"%s\", \"alias\":\"%s\", \"isBid\":%b, \"price\":%d, \"size\":%d, \"duration\":%d}",
                        action, alias, isBid, price, size, duration);
                socketWriter.println(json);
            }
        }
    }

    @Override
    public void onMarketMode(String alias, MarketMode mode) {
    }

    @Override
    public void onTrade(String alias, double price, int size, TradeInfo tradeInfo) {
        // Bookmap API definition: isBidAggressor actually means the BUYER was the aggressor (Green Bubble).
        // When tradeInfo.isBidAggressor is true, it represents a BUY order.
        boolean isBuy = tradeInfo.isBidAggressor;
        // Aggregation is now per-alias only to calculate Delta
        String key = alias;
        long currentMs = getCurrentTimeMs();

        tradeBuffer.compute(key, (k, existing) -> {
            if (existing == null) {
                return new AggregatedTrade(alias, price, isBuy, size, currentMs);
            } else {
                existing.add(price, isBuy, size, currentMs);
                return existing;
            }
        });
    }

    private void checkWalls() {
        long now = getCurrentTimeMs();
        for (WallState state : wallStates.values()) {
            synchronized (state) {
                if (state.firstSeenTime > 0 && !state.isLogged) {
                    InstrumentSettings settings = settingsMap.getOrDefault(state.alias, defaultSettings);
                    
                    // Prevent race condition (state removed concurrently by onDepth)
                    if (state.currentDomSize < settings.minWallSizeRemoved) {
                        continue;
                    }

                    long elapsedSeconds = (now - state.firstSeenTime) / 1000;
                    if (elapsedSeconds >= settings.minWallDur) {
                        state.isLogged = true;
                        String wallMsg = String.format(
                                "[WALL ADDED] %s | %s | Price: %d | Size: %d | Net Added: %d | Dur: %ds",
                                state.alias, state.isBid ? "BID" : "ASK", state.price, state.currentDomSize, state.accumulatedSize, elapsedSeconds);
                        logToUI(state.alias, wallMsg);
                        sendWallEvent("added", state.alias, state.isBid, state.price, state.currentDomSize, elapsedSeconds);
                    }
                }
            }
        }
    }

    private void flushTrades() {
        long now = getCurrentTimeMs();
        java.util.List<AggregatedTrade> readyToFlush = new ArrayList<>();

        for (String key : tradeBuffer.keySet()) {
            tradeBuffer.computeIfPresent(key, (k, trade) -> {
                InstrumentSettings settings = settingsMap.getOrDefault(trade.alias, defaultSettings);
                boolean idleTimeout = now - trade.lastTime >= settings.aggWindowMs;
                boolean maxDurationReached = now - trade.startTime >= settings.maxAggMs;

                if (idleTimeout || maxDurationReached) {
                    readyToFlush.add(trade);
                    return null; // Atomic remove
                }
                return trade;
            });
        }

        for (AggregatedTrade trade : readyToFlush) {
            InstrumentSettings settings = settingsMap.getOrDefault(trade.alias, defaultSettings);
            // Apply threshold to the TOTAL volume of the bubble, as is standard
            if (trade.getTotalAbsSize() >= settings.minDotVol) {
                processFlush(trade, trade.getDelta());
            }
        }
    }

    private void processFlush(AggregatedTrade trade, int delta) {
        String priceStr;
        if (trade.minPrice == trade.maxPrice) {
            priceStr = String.format("%.2f", trade.minPrice);
        } else {
            // It's a sweep! Show the range or average
            priceStr = String.format("%.2f-%.2f (Avg: %.2f)", trade.minPrice, trade.maxPrice, trade.getVwap());
        }

        String direction = delta > 0 ? "BUY" : (delta < 0 ? "SELL" : "NEUTRAL");
        boolean isNetBuy = delta > 0;
        String dotMsg = String.format("[DOT] %s | %s | Price: %s | Delta: %+d | (B:%d S:%d)",
                trade.alias, direction, priceStr, delta, trade.buySize, trade.sellSize);
        logToUI(trade.alias, dotMsg);
        sendDotEvent(trade.alias, isNetBuy, trade.getVwap(), Math.abs(delta));
    }

    private void sendDotEvent(String alias, boolean isBuy, double price, int size) {
        synchronized (socketLock) {
            if (socketWriter != null) {
                String json = String.format(
                        "{\"type\":\"dot\", \"alias\":\"%s\", \"isBuy\":%b, \"price\":%.2f, \"size\":%d}",
                        alias, isBuy, price, size);
                socketWriter.println(json);
            }
        }
    }

    @Override
    public void onUserMessage(Object data) {
        // Log ALL message types to discover what each addon sends
        String className = data.getClass().getName();
        System.out.println("MSG >> [" + className + "] " + truncate(data.toString(), 200));

        if (data instanceof Layer1ApiSoundAlertMessage) {
            Layer1ApiSoundAlertMessage alert = (Layer1ApiSoundAlertMessage) data;
            handleAlert(alert);
        }
    }

    private String truncate(String s, int maxLen) {
        if (s == null)
            return "null";
        return s.length() <= maxLen ? s : s.substring(0, maxLen) + "...";
    }

    private void handleAlert(Layer1ApiSoundAlertMessage alert) {
        alertCount++;
        String text = alert.textInfo != null ? alert.textInfo : "(no text)";
        String textLower = text.toLowerCase();

        String logEntry = "[ALERT] " + text + (alert.showPopup ? " [POPUP]" : "");

        // Print to console
        System.out.println("ALERT >> " + logEntry);

        // Track active instrument tabs
        Set<String> activeAliases = logAreas.keySet();

        for (String alias : activeAliases) {
            String aliasLower = alias.toLowerCase();
            // Try matching the full alias (e.g., GCJ6.COMEX) or just the root (GCJ6)
            String root = aliasLower.contains(".") ? aliasLower.split("\\.")[0] : aliasLower;

            if (textLower.contains(aliasLower) || textLower.contains(root)) {
                // Update UI & Disk for this specific tab
                String timestampedMsg = logToUI(alias, logEntry);
                // Add to memory log for this alias (now with timestamp)
                java.util.List<String> logs = alertLogsMap.computeIfAbsent(alias, k -> new ArrayList<>());
                synchronized (logs) {
                    logs.add(timestampedMsg);
                    if (logs.size() > MAX_LOG_LINES)
                        logs.remove(0);
                }
            }
        }

        // Export to Socket (using first match or Unknown)
        String firstMatchedAlias = activeAliases.stream()
                .filter(a -> textLower.contains(a.toLowerCase()) || textLower.contains(a.toLowerCase().split("\\.")[0]))
                .findFirst().orElse("Unknown");

        long currentMs = getCurrentTimeMs();
        String timeStr = Instant.ofEpochMilli(currentMs).atZone(ZoneOffset.UTC).format(TIME_FMT);
        exportAlert(timeStr, alertCount, firstMatchedAlias, text, alert.showPopup);
    }

    private void exportAlert(String time, int count, String symbol, String text, boolean popup) {
        // Send to Socket (JSON)
        synchronized (socketLock) {
            if (socketWriter != null) {
                String json = String.format(
                        "{\"type\":\"alert\", \"timestamp\":\"%s\", \"symbol\":\"%s\", \"text\":\"%s\", \"count\":%d, \"popup\":%b}",
                        time, symbol, text.replace("\"", "\\\""), count, popup);
                socketWriter.println(json);
                if (socketWriter.checkError()) {
                    System.err.println("Socket write error detected.");
                    logToUI(null, "ERROR: Socket write failed. Please click Reconnect.");
                }
            } else {
                System.err.println("Internal Error: Socket not connected. Alert not sent.");
                // logToUI(null, "WARNING: Not connected. Alert #" + count + " missed.");
            }
        }
    }

    @Override
    public velox.gui.StrategyPanel[] getCustomGuiFor(String indicatorName, String indicatorFullName) {
        // Use a container panel with BorderLayout
        JPanel panel = new JPanel(new BorderLayout(5, 5));
        panel.setBorder(BorderFactory.createEmptyBorder(5, 5, 5, 5));

        // Create a top section using GridBagLayout
        JPanel topPanel = new JPanel(new GridBagLayout());
        GridBagConstraints gbc = new GridBagConstraints();
        gbc.fill = GridBagConstraints.HORIZONTAL;
        gbc.insets = new Insets(2, 2, 2, 2);

        // Row 0: Status and Reconnect Button
        countLabel = new JLabel("Total Alerts: " + alertCount);
        countLabel.setFont(new Font("Arial", Font.BOLD, 14));
        countLabel.setForeground(new Color(0, 200, 100));
        gbc.gridx = 0;
        gbc.gridy = 0;
        gbc.weightx = 1.0;
        topPanel.add(countLabel, gbc);

        JButton reconnectBtn = new JButton("Reconnect");
        reconnectBtn.setMargin(new Insets(2, 5, 2, 5));
        reconnectBtn.addActionListener(e -> reconnectSocket());
        gbc.gridx = 1;
        gbc.gridy = 0;
        gbc.weightx = 0;
        topPanel.add(reconnectBtn, gbc);

        // Row 1: Filters Title
        JLabel filterTitle = new JLabel("Filters:");
        filterTitle.setFont(new Font("Arial", Font.BOLD, 12));
        gbc.gridx = 0;
        gbc.gridy = 1;
        gbc.gridwidth = 2;
        gbc.insets = new Insets(10, 2, 2, 2);
        topPanel.add(filterTitle, gbc);

        // Row 2: Filter Area (GridBag for pr // Row 2 of Filters
        JPanel filterArea = new JPanel(new GridBagLayout());
        GridBagConstraints fgbc = new GridBagConstraints();
        fgbc.fill = GridBagConstraints.HORIZONTAL;
        fgbc.insets = new Insets(2, 4, 2, 4);

        InstrumentSettings settings = settingsMap.computeIfAbsent(indicatorName, k -> new InstrumentSettings());

        // COLUMN 1: DOT SETTINGS (Left)
        fgbc.gridx = 0;
        fgbc.gridy = 0;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Dot Vol:"), fgbc);
        fgbc.gridx = 1;
        fgbc.gridy = 0;
        fgbc.weightx = 0.7;
        JTextField dotVolField = new JTextField(String.valueOf(settings.minDotVol), 4);
        filterArea.add(dotVolField, fgbc);

        fgbc.gridx = 0;
        fgbc.gridy = 1;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Agg Win(ms):"), fgbc);
        fgbc.gridx = 1;
        fgbc.gridy = 1;
        fgbc.weightx = 0.7;
        JTextField aggWinField = new JTextField(String.valueOf(settings.aggWindowMs), 4);
        filterArea.add(aggWinField, fgbc);

        fgbc.gridx = 0;
        fgbc.gridy = 2;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Max Dur(ms):"), fgbc);
        fgbc.gridx = 1;
        fgbc.gridy = 2;
        fgbc.weightx = 0.7;
        JTextField maxAggField = new JTextField(String.valueOf(settings.maxAggMs), 4);
        filterArea.add(maxAggField, fgbc);

        // COLUMN 2: WALL SETTINGS (Right)
        fgbc.gridx = 2;
        fgbc.gridy = 0;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Wall Added:"), fgbc);
        fgbc.gridx = 3;
        fgbc.gridy = 0;
        fgbc.weightx = 0.7;
        JTextField wallAddedField = new JTextField(String.valueOf(settings.minWallSizeAdded), 4);
        filterArea.add(wallAddedField, fgbc);

        fgbc.gridx = 2;
        fgbc.gridy = 1;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Wall Removed:"), fgbc);
        fgbc.gridx = 3;
        fgbc.gridy = 1;
        fgbc.weightx = 0.7;
        JTextField wallRemovedField = new JTextField(String.valueOf(settings.minWallSizeRemoved), 4);
        filterArea.add(wallRemovedField, fgbc);

        fgbc.gridx = 2;
        fgbc.gridy = 2;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Wall Dur(s):"), fgbc);
        fgbc.gridx = 3;
        fgbc.gridy = 2;
        fgbc.weightx = 0.7;
        JTextField wallDurField = new JTextField(String.valueOf(settings.minWallDur), 4);
        filterArea.add(wallDurField, fgbc);

        gbc.gridx = 0;
        gbc.gridy = 2;
        gbc.gridwidth = 2;
        gbc.insets = new Insets(5, 2, 5, 2);
        topPanel.add(filterArea, gbc);

        // Row 3: Apply Button
        JButton applyBtn = new JButton("Apply Settings [" + indicatorName + "]");
        applyBtn.setMargin(new Insets(2, 10, 2, 10));
        applyBtn.addActionListener(e -> {
            try {
                settings.minDotVol = Integer.parseInt(dotVolField.getText().trim());
                settings.minWallSizeAdded = Integer.parseInt(wallAddedField.getText().trim());
                settings.minWallSizeRemoved = Integer.parseInt(wallRemovedField.getText().trim());
                settings.minWallDur = Integer.parseInt(wallDurField.getText().trim());
                settings.aggWindowMs = Integer.parseInt(aggWinField.getText().trim());
                settings.maxAggMs = Integer.parseInt(maxAggField.getText().trim());
                saveConfig();
                logToUI(indicatorName,
                        "SYSTEM: Filters saved for " + indicatorName + ". Dot:" + settings.minDotVol + " Added:"
                                + settings.minWallSizeAdded + " Removed:" + settings.minWallSizeRemoved + " Dur:"
                                + settings.minWallDur + "s Agg:" + settings.aggWindowMs + "ms MaxDur:"
                                + settings.maxAggMs + "ms");
            } catch (NumberFormatException ex) {
                logToUI(indicatorName, "ERROR: Invalid filter values for " + indicatorName + ". Please enter numbers.");
            }
        });
        gbc.gridy = 3;
        gbc.insets = new Insets(2, 2, 10, 2);
        topPanel.add(applyBtn, gbc);

        panel.add(topPanel, BorderLayout.NORTH);

        // Log area
        JTextArea areaForThisTab = new JTextArea(15, 40);
        areaForThisTab.setEditable(false);
        areaForThisTab.setFont(new Font("Consolas", Font.PLAIN, 11));
        areaForThisTab.setBackground(new Color(30, 30, 30));
        areaForThisTab.setForeground(new Color(200, 200, 200));
        areaForThisTab.setCaretColor(Color.WHITE);

        // Register this log area
        logAreas.put(indicatorName, areaForThisTab);

        // Load log history from disk
        loadLogHistory(indicatorName, areaForThisTab);

        JScrollPane scrollPane = new JScrollPane(areaForThisTab);
        scrollPane.setPreferredSize(new Dimension(400, 300));
        panel.add(scrollPane, BorderLayout.CENTER);

        // Bottom panel buttons
        JPanel bottomPanel = new JPanel(new GridLayout(1, 2, 5, 5));

        JButton clearButton = new JButton("Clear Log");
        clearButton.addActionListener(e -> {
            // Clear memory
            java.util.List<String> currentLogs = alertLogsMap.get(indicatorName);
            if (currentLogs != null) {
                synchronized (currentLogs) {
                    currentLogs.clear();
                }
            }
            areaForThisTab.setText("");

            // Clear disk (delete the file)
            File file = new File(System.getProperty("user.home"),
                    "AlertListener_history_" + sanitize(indicatorName) + ".log");
            if (file.exists()) {
                file.delete();
            }
        });

        JButton exportButton = new JButton("Export Log");
        exportButton.addActionListener(e -> {
            String timestamp = LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyyMMdd_HHmmss"));
            String fileName = "AlertListener_Export_" + sanitize(indicatorName) + "_" + timestamp + ".txt";
            File exportFile = new File(System.getProperty("user.home"), fileName);
            try (PrintWriter out = new PrintWriter(new BufferedWriter(new FileWriter(exportFile)))) {
                out.print(areaForThisTab.getText());
                logToUI(indicatorName, "SUCCESS: Log exported to " + exportFile.getAbsolutePath());
            } catch (IOException ex) {
                logToUI(indicatorName, "ERROR: Failed to export log: " + ex.getMessage());
            }
        });

        bottomPanel.add(clearButton);
        bottomPanel.add(exportButton);

        panel.add(bottomPanel, BorderLayout.SOUTH);

        velox.gui.StrategyPanel strategyPanel = new velox.gui.StrategyPanel("Alert Listener", true);
        strategyPanel.add(panel);
        return new velox.gui.StrategyPanel[] { strategyPanel };
    }

    private void loadConfig() {
        if (configFile.exists()) {
            try (InputStream input = new FileInputStream(configFile)) {
                Properties prop = new Properties();
                prop.load(input);

                // Collect all instrument names from keys (keys look like INSTRUMENT.minDotVol)
                Set<String> aliases = new HashSet<>();
                for (String key : prop.stringPropertyNames()) {
                    if (key.contains(".")) {
                        aliases.add(key.substring(0, key.lastIndexOf('.')));
                    }
                }

                for (String alias : aliases) {
                    InstrumentSettings s = new InstrumentSettings();
                    s.minDotVol = Integer.parseInt(prop.getProperty(alias + ".minDotVol", "20"));
                    s.minWallSizeAdded = Integer.parseInt(prop.getProperty(alias + ".minWallSizeAdded", "100"));
                    s.minWallSizeRemoved = Integer.parseInt(prop.getProperty(alias + ".minWallSizeRemoved", "50"));
                    s.minWallDur = Integer.parseInt(prop.getProperty(alias + ".minWallDur", "5"));
                    s.aggWindowMs = Integer.parseInt(prop.getProperty(alias + ".aggWindowMs", "50"));
                    s.maxAggMs = Integer.parseInt(prop.getProperty(alias + ".maxAggMs", "1000"));
                    settingsMap.put(alias, s);
                }

                System.out.println("Config loaded for " + settingsMap.size() + " instruments.");
            } catch (IOException | NumberFormatException ex) {
                System.err.println("Error loading config: " + ex.getMessage());
            }
        }
    }

    private void saveConfig() {
        try (OutputStream output = new FileOutputStream(configFile)) {
            Properties prop = new Properties();
            for (Map.Entry<String, InstrumentSettings> entry : settingsMap.entrySet()) {
                String alias = entry.getKey();
                InstrumentSettings s = entry.getValue();
                prop.setProperty(alias + ".minDotVol", String.valueOf(s.minDotVol));
                prop.setProperty(alias + ".minWallSizeAdded", String.valueOf(s.minWallSizeAdded));
                prop.setProperty(alias + ".minWallSizeRemoved", String.valueOf(s.minWallSizeRemoved));
                prop.setProperty(alias + ".minWallDur", String.valueOf(s.minWallDur));
                prop.setProperty(alias + ".aggWindowMs", String.valueOf(s.aggWindowMs));
                prop.setProperty(alias + ".maxAggMs", String.valueOf(s.maxAggMs));
            }
            prop.store(output, null);
        } catch (IOException io) {
            System.err.println("Error saving config: " + io.getMessage());
        }
    }

    private void loadLogHistory(String alias, JTextArea area) {
        // 1. Load instrument specific logs ONLY (Internal system logs are managed
        // separately)
        File instrumentFile = new File(System.getProperty("user.home"),
                "AlertListener_history_" + sanitize(alias) + ".log");
        loadLines(instrumentFile, area);
    }

    private void loadLines(File file, JTextArea area) {
        if (file.exists()) {
            try {
                java.util.List<String> lines = Files.readAllLines(file.toPath());
                int start = Math.max(0, lines.size() - 250); // Show more lines from specific history
                for (int i = start; i < lines.size(); i++) {
                    final String line = lines.get(i);
                    SwingUtilities.invokeLater(() -> {
                        if (area != null) {
                            area.append(line + "\n");
                            area.setCaretPosition(area.getDocument().getLength());
                        }
                    });
                }
            } catch (IOException e) {
                System.err.println("Error loading log history from " + file.getName() + ": " + e.getMessage());
            }
        }
    }

    @Override
    public void finish() {
        saveConfig();
        System.out.println("Alert Listener stopped. Total alerts received: " + alertCount);
        aggregationExecutor.shutdown();
        ListenableHelper.removeListeners(provider, this);

        // Close Socket
        try {
            if (socketWriter != null)
                socketWriter.close();
            if (socket != null)
                socket.close();
        } catch (IOException e) {
            // Ignore
        }
    }
}
