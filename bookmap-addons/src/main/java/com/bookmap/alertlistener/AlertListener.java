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
import java.util.regex.Matcher;
import java.util.regex.Pattern;

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
    private final java.util.List<JButton> socketStatusButtons = Collections.synchronizedList(new ArrayList<>());
    private JLabel countLabel;
    private int alertCount = 0;
    private static final int MAX_LOG_LINES = 500;
    private static final DateTimeFormatter TIME_FMT = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'");
    static final String CSV_HEADER = String.join(",",
            "timestamp",
            "alias",
            "type",
            "side",
            "price",
            "value",
            "price_min",
            "price_max",
            "bid_size",
            "ask_size",
            "duration_sec",
            "raw_text");

    private static final Pattern ALERT_SOURCE_PREFIX_PATTERN = Pattern.compile("^(?:([^:]+):\\s+)?(.+)$");
    private static final Pattern HIDDEN_DEVELOPMENT_PATTERN = Pattern.compile(
            "^HIDDEN\\s+(BID|ASK)\\s+DEVELOPMENT,\\s+V:\\s*([-+]?[0-9]+(?:\\.[0-9]+)?)\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern HIDDEN_N_PATTERN = Pattern.compile(
            "^HIDDEN\\s+(BID|ASK)-N,\\s+V:\\s*([-+]?[0-9]+(?:\\.[0-9]+)?)(?:\\s*\\([^)]*\\))?\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern ABSORPTION_PATTERN = Pattern.compile(
            "^ABSORPTION\\s+(BUY|SELL),\\s+V:\\s*([-+]?[0-9]+(?:\\.[0-9]+)?)\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern SWEEP_PATTERN = Pattern.compile(
            "^SWEEP\\s+(BUY|SELL),\\s+V:\\s*([-+]?[0-9]+(?:\\.[0-9]+)?)(?:,\\s*P:\\s*[-+]?[0-9]+(?:\\.[0-9]+)?)?\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern IMBALANCE_ZERO_CROSS_PATTERN = Pattern.compile(
            "^Imbalance\\s+(BID|ASK)\\s+zero-cross\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern IMBALANCE_RATIO_PATTERN = Pattern.compile(
            "^Imbalance\\s+(BID|ASK)\\s+([-+]?[0-9]+)%\\s*>\\s*(?:BID|ASK)\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern STOP_PATTERN = Pattern.compile(
            "^Stop\\s+.+?\\s+(buy|sell)\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)\\s+volume\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)(?:\\s+\\[POPUP\\])?$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern MARKET_VOLUME_STOP_PATTERN = Pattern.compile(
            "^Market\\s+Volume\\s+Stop\\s+(BUY|SELL)\\s+of\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)\\s+reached\\s+at\\s+([-+]?[0-9]+(?:\\.[0-9]+)?)$",
            Pattern.CASE_INSENSITIVE);
    private static final Pattern INCONSISTENT_MBO_PATTERN = Pattern.compile(
            "^Inconsistent\\s+MBO\\s+data.*$",
            Pattern.CASE_INSENSITIVE);

    // Socket for Python AI Analyzer
    private Socket socket;
    private PrintWriter socketWriter;
    private static final String HOST = "127.0.0.1";
    private static final int PORT = 5555;
    private static final String DEFAULT_CSV_OUTPUT_PATH = "D:\\projects\\quant-trading\\logs";
    private final Object socketLock = new Object();
    private final ScheduledExecutorService socketReconnectExecutor = Executors.newSingleThreadScheduledExecutor();
    private static final long SOCKET_RECONNECT_DELAY_SECONDS = 5L;
    private static final String EVENT_CONTRACT_SCHEMA = "event-contract/v1";
    private static final String EVENT_SOURCE = "bookmap";
    private static final String EVENT_SOURCE_INSTANCE = "AlertListener";
    private String socketConnectionState = "socket disconnected";
    private boolean hasConnectedOnce = false;
    private long reconnectCount = 0L;
    private long droppedEventsTotal = 0L;

    // DOM State Tracking: alias -> OrderBook
    // Note: Bookmap automatically backfills historical data from the chart into
    // onDepth/onTrade when the addon starts.
    private final Map<String, OrderBook> domState = new ConcurrentHashMap<>();

    // Files for persistence
    private final File configFile;
    private String csvOutputPath = DEFAULT_CSV_OUTPUT_PATH;

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

    static class AggregatedTrade {
        String alias;
        double minPrice;
        double maxPrice;
        int bidSize;
        int askSize;
        long lastTime;
        long startTime;
        double sumPriceSize; // To calculate VWAP of all trades in this window

        AggregatedTrade(String alias, double price, boolean isBidAggressor, int size, long currentTimeMs) {
            this.alias = alias;
            this.minPrice = price;
            this.maxPrice = price;
            if (isBidAggressor) {
                this.bidSize = size;
            } else {
                this.askSize = size;
            }
            this.sumPriceSize = price * size;
            this.lastTime = currentTimeMs;
            this.startTime = currentTimeMs;
        }

        void add(double price, boolean isBidAggressor, int size, long currentTimeMs) {
            if (isBidAggressor) {
                this.bidSize += size;
            } else {
                this.askSize += size;
            }
            this.sumPriceSize += price * size;
            this.minPrice = Math.min(this.minPrice, price);
            this.maxPrice = Math.max(this.maxPrice, price);
            this.lastTime = currentTimeMs;
        }

        int getTotalAbsSize() {
            return bidSize + askSize;
        }

        int getDelta() {
            return askSize - bidSize;
        }

        double getVwap() {
            int total = getTotalAbsSize();
            return total == 0 ? 0 : sumPriceSize / total;
        }
    }

    static class CsvLogRow {
        String timestamp = "";
        String alias = "";
        String type = "";
        String side = "";
        String price = "";
        String value = "";
        String priceMin = "";
        String priceMax = "";
        String bidSize = "";
        String askSize = "";
        String durationSec = "";
        String rawText = "";
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

        socketReconnectExecutor.scheduleWithFixedDelay(
                this::maintainSocketConnection,
                0,
                SOCKET_RECONNECT_DELAY_SECONDS,
                TimeUnit.SECONDS);
        System.out.println("==============================");
    }

    private long getCurrentTimeMs() {
        return provider.getCurrentTime() / 1_000_000L;
    }

    private void maintainSocketConnection() {
        synchronized (socketLock) {
            if (socket != null && socketWriter != null && !socket.isClosed()) {
                sendConnectionHeartbeat();
                return;
            }
        }
        ensureSocketConnected();
    }

    private void ensureSocketConnected() {
        synchronized (socketLock) {
            if (socket != null && socketWriter != null && !socket.isClosed()) {
                return;
            }

            logSocketStateTransition("socket reconnecting");

            try {
                closeSocketSilently();
                socket = new Socket(HOST, PORT);
                socketWriter = new PrintWriter(socket.getOutputStream(), true);
                long helloReconnectCount = hasConnectedOnce ? reconnectCount + 1L : reconnectCount;
                if (!sendConnectionHello(helloReconnectCount)) {
                    closeSocketSilently();
                    return;
                }

                reconnectCount = helloReconnectCount;
                hasConnectedOnce = true;
                logSocketStateTransition("socket connected");
            } catch (IOException ignored) {
                markSocketDisconnected();
            }
        }
    }

    private void reconnectSocket() {
        synchronized (socketLock) {
            closeSocketSilently();
        }
        logSocketStateTransition("socket disconnected");
        ensureSocketConnected();
    }

    private void closeSocketSilently() {
        try {
            if (socketWriter != null) {
                socketWriter.close();
            }
        } catch (Exception ignored) {
        }

        try {
            if (socket != null) {
                socket.close();
            }
        } catch (Exception ignored) {
        }

        socketWriter = null;
        socket = null;
    }

    private void markSocketDisconnected() {
        closeSocketSilently();
        logSocketStateTransition("socket disconnected");
    }

    private Color socketStatusColor() {
        return "socket connected".equals(socketConnectionState)
                ? new Color(0, 170, 90)
                : new Color(200, 60, 60);
    }

    private void updateSocketStatusButtons() {
        Color color = socketStatusColor();
        String tooltip = "Python socket: " + socketConnectionState;
        synchronized (socketStatusButtons) {
            for (JButton button : socketStatusButtons) {
                SwingUtilities.invokeLater(() -> {
                    button.setText("");
                    button.setBackground(color);
                    button.setForeground(Color.WHITE);
                    button.setOpaque(true);
                    button.setBorderPainted(false);
                    button.setFocusPainted(false);
                    button.setToolTipText(tooltip);
                });
            }
        }
    }

    private void logSocketStateTransition(String nextState) {
        if (Objects.equals(socketConnectionState, nextState)) {
            return;
        }

        socketConnectionState = nextState;
        updateSocketStatusButtons();
        System.out.println(nextState);
        logToUI(null, "SYSTEM: " + nextState);
    }

    private void recordDroppedEvent() {
        droppedEventsTotal++;
        markSocketDisconnected();
    }

    private String logToUI(String alias, String message) {
        return logCsvRow(alias, createSystemCsvRow(currentTimestamp(), alias, message));
    }

    private String logCsvRow(String alias, CsvLogRow row) {
        String csvLine = toCsvLine(row);
        updateUI(alias, csvLine);
        return csvLine;
    }

    private void appendLogToFile(String alias, String csvLine) {
        // Phase 2 runtime artifacts are owned by the Python bridge.
    }

    private File getCsvOutputDirectory() {
        String configuredPath = csvOutputPath == null ? "" : csvOutputPath.trim();
        File directory = configuredPath.isEmpty()
                ? new File(DEFAULT_CSV_OUTPUT_PATH)
                : new File(configuredPath);

        if (!directory.exists()) {
            directory.mkdirs();
        }

        return directory;
    }

    private int migrateCsvOutputFiles(File sourceDirectory, File targetDirectory) {
        if (sourceDirectory == null || targetDirectory == null || sourceDirectory.equals(targetDirectory) || !sourceDirectory.exists()) {
            return 0;
        }

        File[] filesToMove = sourceDirectory.listFiles((dir, name) ->
                name.startsWith("AlertListener_") && (name.endsWith(".csv") || name.endsWith(".log")));
        if (filesToMove == null || filesToMove.length == 0) {
            return 0;
        }

        int movedCount = 0;
        for (File sourceFile : filesToMove) {
            File targetFile = new File(targetDirectory, sourceFile.getName());
            if (targetFile.exists()) {
                continue;
            }

            try {
                Files.move(sourceFile.toPath(), targetFile.toPath());
                movedCount++;
            } catch (IOException e) {
                System.err.println("Error migrating file " + sourceFile.getName() + ": " + e.getMessage());
            }
        }

        return movedCount;
    }

    private void openCsvOutputDirectory(String alias) {
        File directory = getCsvOutputDirectory();
        try {
            if (!Desktop.isDesktopSupported()) {
                logToUI(alias, "ERROR: Desktop integration is not supported on this machine.");
                return;
            }

            Desktop.getDesktop().open(directory);
        } catch (IOException e) {
            logToUI(alias, "ERROR: Failed to open CSV folder: " + e.getMessage());
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
                countLabel.setText("Alerts: " + alertCount);
            }
        });
    }

    private String currentTimestamp() {
        long currentMs = getCurrentTimeMs();
        return Instant.ofEpochMilli(currentMs).atZone(ZoneOffset.UTC).format(TIME_FMT);
    }

    private void sendContractEvent(String event, String instrument, String timestamp, Map<String, Object> payload,
            Map<String, Object> sourceMeta) {
        synchronized (socketLock) {
            if (socketWriter == null) {
                droppedEventsTotal++;
                return;
            }

            Map<String, Object> envelope = new LinkedHashMap<>();
            envelope.put("schema", EVENT_CONTRACT_SCHEMA);
            envelope.put("source", EVENT_SOURCE);
            envelope.put("source_instance", EVENT_SOURCE_INSTANCE);
            envelope.put("event", event);
            envelope.put("event_id", buildEventId(event, instrument, timestamp));
            envelope.put("instrument", instrument);
            envelope.put("timestamp", timestamp);
            envelope.put("payload", payload);
            envelope.put("source_meta", sourceMeta);

            socketWriter.println(toJson(envelope));
            if (socketWriter.checkError()) {
                recordDroppedEvent();
            }
        }
    }

    private boolean sendConnectionHello(long helloReconnectCount) {
        if (socketWriter == null) {
            return false;
        }

        Map<String, Object> hello = new LinkedHashMap<>();
        hello.put("kind", "connection_hello");
        hello.put("source", EVENT_SOURCE);
        hello.put("source_instance", EVENT_SOURCE_INSTANCE);
        hello.put("instrument", "bookmap");
        hello.put("timestamp", currentTimestamp());
        hello.put("reconnect_count", helloReconnectCount);
        hello.put("dropped_events_total", droppedEventsTotal);

        socketWriter.println(toJson(hello));
        if (socketWriter.checkError()) {
            markSocketDisconnected();
            return false;
        }
        return true;
    }

    private boolean sendConnectionHeartbeat() {
        if (socketWriter == null) {
            return false;
        }

        Map<String, Object> heartbeat = new LinkedHashMap<>();
        heartbeat.put("kind", "connection_heartbeat");
        heartbeat.put("source", EVENT_SOURCE);
        heartbeat.put("source_instance", EVENT_SOURCE_INSTANCE);
        heartbeat.put("instrument", "bookmap");
        heartbeat.put("timestamp", currentTimestamp());
        heartbeat.put("reconnect_count", reconnectCount);
        heartbeat.put("dropped_events_total", droppedEventsTotal);

        socketWriter.println(toJson(heartbeat));
        if (socketWriter.checkError()) {
            markSocketDisconnected();
            return false;
        }
        return true;
    }

    private String buildEventId(String event, String instrument, String timestamp) {
        return String.format("%s-%s-%s-%s",
                EVENT_SOURCE,
                sanitizeEventIdPart(event),
                sanitizeEventIdPart(instrument),
                sanitizeEventIdPart(timestamp));
    }

    private String sanitizeEventIdPart(String value) {
        return nonNull(value).replaceAll("[^a-zA-Z0-9._-]", "_");
    }

    @SuppressWarnings("unchecked")
    private String toJson(Object value) {
        if (value == null) {
            return "null";
        }
        if (value instanceof String) {
            return "\"" + escapeJson((String) value) + "\"";
        }
        if (value instanceof Number || value instanceof Boolean) {
            return String.valueOf(value);
        }
        if (value instanceof Map<?, ?>) {
            StringBuilder builder = new StringBuilder("{");
            boolean first = true;
            for (Map.Entry<String, Object> entry : ((Map<String, Object>) value).entrySet()) {
                if (!first) {
                    builder.append(", ");
                }
                builder.append(toJson(entry.getKey()));
                builder.append(":");
                builder.append(toJson(entry.getValue()));
                first = false;
            }
            builder.append("}");
            return builder.toString();
        }
        if (value instanceof Collection<?>) {
            StringBuilder builder = new StringBuilder("[");
            boolean first = true;
            for (Object item : (Collection<?>) value) {
                if (!first) {
                    builder.append(", ");
                }
                builder.append(toJson(item));
                first = false;
            }
            builder.append("]");
            return builder.toString();
        }
        return toJson(String.valueOf(value));
    }

    private String escapeJson(String value) {
        StringBuilder builder = new StringBuilder();
        for (char c : value.toCharArray()) {
            switch (c) {
                case '\\':
                    builder.append("\\\\");
                    break;
                case '"':
                    builder.append("\\\"");
                    break;
                case '\b':
                    builder.append("\\b");
                    break;
                case '\f':
                    builder.append("\\f");
                    break;
                case '\n':
                    builder.append("\\n");
                    break;
                case '\r':
                    builder.append("\\r");
                    break;
                case '\t':
                    builder.append("\\t");
                    break;
                default:
                    if (c < 0x20) {
                        builder.append(String.format("\\u%04x", (int) c));
                    } else {
                        builder.append(c);
                    }
                    break;
            }
        }
        return builder.toString();
    }

    static CsvLogRow createSystemCsvRow(String timestamp, String alias, String message) {
        CsvLogRow row = new CsvLogRow();
        row.timestamp = nonNull(timestamp);
        row.alias = nonNull(alias);
        row.rawText = nonNull(message);

        if (message != null && message.startsWith("ERROR:")) {
            row.type = "ERROR";
        } else if (message != null && message.startsWith("SYSTEM:")) {
            row.type = "SYSTEM";
        } else {
            row.type = "INFO";
        }
        return row;
    }

    static CsvLogRow createDotCsvRow(String timestamp, AggregatedTrade trade) {
        int dotDelta = trade.getDelta();
        CsvLogRow row = new CsvLogRow();
        row.timestamp = nonNull(timestamp);
        row.alias = nonNull(trade.alias);
        row.type = "DOT";
        row.side = normalizeDotSide(dotDelta);
        row.price = formatNumber(trade.getVwap());
        row.value = String.valueOf(Math.abs(dotDelta));
        row.priceMin = formatNumber(trade.minPrice);
        row.priceMax = formatNumber(trade.maxPrice);
        row.bidSize = String.valueOf(trade.bidSize);
        row.askSize = String.valueOf(trade.askSize);
        row.rawText = buildDotRawText(trade, dotDelta);
        return row;
    }

    static CsvLogRow createWallCsvRow(String timestamp, String alias, boolean isBid, int price, int size, long durationSec,
            String type, String rawText) {
        CsvLogRow row = new CsvLogRow();
        row.timestamp = nonNull(timestamp);
        row.alias = nonNull(alias);
        row.type = nonNull(type);
        row.side = isBid ? "BID" : "ASK";
        row.price = String.valueOf(price);
        row.value = String.valueOf(size);
        row.durationSec = durationSec > 0 ? String.valueOf(durationSec) : "";
        row.rawText = nonNull(rawText);
        return row;
    }

    static CsvLogRow parseAlertCsvRow(String timestamp, String alias, String text, boolean popup) {
        CsvLogRow row = new CsvLogRow();
        row.timestamp = nonNull(timestamp);
        row.alias = nonNull(alias);
        row.type = "UNKNOWN";
        row.rawText = nonNull(text);

        String body = text == null ? "" : text.trim();
        Matcher sourceMatcher = ALERT_SOURCE_PREFIX_PATTERN.matcher(body);
        if (sourceMatcher.matches()) {
            String prefix = sourceMatcher.group(1);
            String possibleBody = sourceMatcher.group(2);
            if (prefix != null && prefix.matches("[A-Za-z0-9._@-]+")) {
                body = possibleBody.trim();
            }
        }

        if (fillAlertMatch(row, HIDDEN_DEVELOPMENT_PATTERN.matcher(body), "HIDDEN DEVELOPMENT", 1, 3, 2)) {
            return row;
        }
        if (fillAlertMatch(row, HIDDEN_N_PATTERN.matcher(body), "HIDDEN", 1, 3, 2)) {
            return row;
        }
        if (fillAlertMatch(row, ABSORPTION_PATTERN.matcher(body), "ABSORPTION", 1, 3, 2)) {
            return row;
        }
        if (fillAlertMatch(row, SWEEP_PATTERN.matcher(body), "SWEEP", 1, 3, 2)) {
            return row;
        }

        Matcher imbalanceZeroCross = IMBALANCE_ZERO_CROSS_PATTERN.matcher(body);
        if (imbalanceZeroCross.matches()) {
            row.type = "IMBALANCE ZERO-CROSS";
            row.side = normalizeSide(imbalanceZeroCross.group(1));
            row.price = imbalanceZeroCross.group(2);
            return row;
        }

        Matcher imbalanceRatio = IMBALANCE_RATIO_PATTERN.matcher(body);
        if (imbalanceRatio.matches()) {
            row.type = "IMBALANCE RATIO";
            row.side = normalizeSide(imbalanceRatio.group(1));
            row.value = imbalanceRatio.group(2);
            row.price = imbalanceRatio.group(3);
            return row;
        }

        Matcher stopMatcher = STOP_PATTERN.matcher(body);
        if (stopMatcher.matches()) {
            row.type = "STOP";
            row.side = normalizeSide(stopMatcher.group(1));
            row.price = stopMatcher.group(2);
            row.value = stopMatcher.group(3);
            return row;
        }

        Matcher marketVolumeStop = MARKET_VOLUME_STOP_PATTERN.matcher(body);
        if (marketVolumeStop.matches()) {
            row.type = "MARKET VOLUME STOP";
            row.side = normalizeSide(marketVolumeStop.group(1));
            row.value = marketVolumeStop.group(2);
            row.price = marketVolumeStop.group(3);
            return row;
        }

        if (INCONSISTENT_MBO_PATTERN.matcher(body).matches()) {
            row.type = "INCONSISTENT MBO DATA";
        }

        return row;
    }

    private static boolean fillAlertMatch(CsvLogRow row, Matcher matcher, String type, int sideGroup, int priceGroup,
            int valueGroup) {
        if (!matcher.matches()) {
            return false;
        }

        row.type = type;
        row.side = normalizeSide(matcher.group(sideGroup));
        row.price = matcher.group(priceGroup);
        row.value = matcher.group(valueGroup);
        return true;
    }

    static String csvHeader() {
        return CSV_HEADER;
    }

    static String toCsvLine(CsvLogRow row) {
        return String.join(",",
                csvEscape(row.timestamp),
                csvEscape(row.alias),
                csvEscape(row.type),
                csvEscape(row.side),
                csvEscape(row.price),
                csvEscape(row.value),
                csvEscape(row.priceMin),
                csvEscape(row.priceMax),
                csvEscape(row.bidSize),
                csvEscape(row.askSize),
                csvEscape(row.durationSec),
                csvEscape(row.rawText));
    }

    static String normalizeSide(String rawSide) {
        String side = nonNull(rawSide).trim().toUpperCase(Locale.ROOT);
        switch (side) {
            case "BID":
            case "ASK":
                return side;
            case "BUY":
                return "ASK";
            case "SELL":
                return "BID";
            default:
                return "";
        }
    }

    static String normalizeDotSide(int delta) {
        if (delta > 0) {
            return "ASK";
        }
        if (delta < 0) {
            return "BID";
        }
        return "";
    }

    static void appendCsvLine(File file, String csvLine) throws IOException {
        boolean writeHeader = !file.exists() || file.length() == 0;
        try (PrintWriter out = new PrintWriter(new BufferedWriter(new FileWriter(file, true)))) {
            if (writeHeader) {
                out.println(csvHeader());
            }
            out.println(csvLine);
        }
    }

    private static String csvEscape(String value) {
        String safe = nonNull(value);
        boolean needsQuotes = safe.contains(",") || safe.contains("\"") || safe.contains("\n") || safe.contains("\r");
        if (!needsQuotes) {
            return safe;
        }
        return "\"" + safe.replace("\"", "\"\"") + "\"";
    }

    private static String formatNumber(double value) {
        if (Math.rint(value) == value) {
            return String.format(Locale.US, "%.0f", value);
        }
        return java.math.BigDecimal.valueOf(value).stripTrailingZeros().toPlainString();
    }

    private static String nonNull(String value) {
        return value == null ? "" : value;
    }

    private static String buildDotRawText(AggregatedTrade trade, int delta) {
        String priceStr;
        if (trade.minPrice == trade.maxPrice) {
            priceStr = String.format(Locale.US, "%.2f", trade.minPrice);
        } else {
            priceStr = String.format(Locale.US, "%.2f-%.2f (Avg: %.2f)", trade.minPrice, trade.maxPrice, trade.getVwap());
        }

        String direction = delta > 0 ? "BUY" : (delta < 0 ? "SELL" : "NEUTRAL");
        return String.format(Locale.US, "[DOT] %s | %s | Price: %s | Delta: %+d | (Bid:%d Ask:%d)",
                trade.alias, direction, priceStr, delta, trade.bidSize, trade.askSize);
    }

    private static String buildWallRawText(String type, String alias, boolean isBid, int price, int size, int extraValue,
            long durationSec) {
        if ("WALL ADDED".equals(type)) {
            return String.format(Locale.US,
                    "[WALL ADDED] %s | %s | Price: %d | Size: %d | Net Added: %d | Dur: %ds",
                    alias, isBid ? "BID" : "ASK", price, size, extraValue, durationSec);
        }
        return String.format(Locale.US,
                "[WALL REMOVED] %s | %s | Price: %d | Current Size: %d | Threshold: %d",
                alias, isBid ? "BID" : "ASK", price, size, extraValue);
    }

    @Override
    public void onDepth(String alias, boolean isBid, int price, int size) {
        // Get or create OrderBook for this instrument
        OrderBook orderBook = domState.computeIfAbsent(alias, k -> new OrderBook());

        // Get previous size before update
        long prevSize = orderBook.getSizeFor(isBid, price);

        // Update the OrderBook state using the correct Bookmap utility method
        orderBook.onUpdate(isBid, price, size);

        // Emit realtime DOM updates for every size change at this price level.
        String action = null;
        if (prevSize == 0 && size > 0) {
            action = "add";
        } else if (prevSize > 0 && size == 0) {
            action = "remove";
        } else if (prevSize != size) {
            action = "update";
        }

        // Send every DOM state transition to the realtime socket.
        if (action != null) {
            sendDomEvent(action, alias, isBid, price, size, prevSize);
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
                    String timestamp = currentTimestamp();
                    String wallMsg = buildWallRawText("WALL REMOVED", alias, isBid, price, size,
                            settings.minWallSizeRemoved, 0);
                    logCsvRow(alias, createWallCsvRow(timestamp, alias, isBid, price, size, 0, "WALL REMOVED", wallMsg));
                    sendWallEvent("removed", alias, isBid, price, size, 0);
                }
                wallStates.remove(wallKey);
            }
        }
    }

    private void sendDomEvent(String action, String alias, boolean isBid, int price, int size, long prevSize) {
        Map<String, Object> payload = new LinkedHashMap<>();
        payload.put("action", action);
        payload.put("isBid", isBid);
        payload.put("price", price);
        payload.put("size", size);
        payload.put("prevSize", prevSize);
        payload.put("delta", size - prevSize);

        Map<String, Object> sourceMeta = new LinkedHashMap<>();
        sourceMeta.put("alias", alias);

        sendContractEvent("dom", alias, currentTimestamp(), payload, sourceMeta);
    }

    private void sendWallEvent(String action, String alias, boolean isBid, int price, int size, long duration) {
        Map<String, Object> payload = new LinkedHashMap<>();
        payload.put("action", action);
        payload.put("isBid", isBid);
        payload.put("price", price);
        payload.put("size", size);
        payload.put("duration", duration);

        Map<String, Object> sourceMeta = new LinkedHashMap<>();
        sourceMeta.put("alias", alias);

        sendContractEvent("wall", alias, currentTimestamp(), payload, sourceMeta);
    }

    @Override
    public void onMarketMode(String alias, MarketMode mode) {
    }

    @Override
    public void onTrade(String alias, double price, int size, TradeInfo tradeInfo) {
        // Bookmap API: true means the aggressive trade happened on the bid side.
        boolean isBidAggressor = tradeInfo.isBidAggressor;
        sendDotEvent(alias, !isBidAggressor, price, size);

        // Aggregation is per-alias and keeps bid/ask aggressor volume separately.
        String key = alias;
        long currentMs = getCurrentTimeMs();

        tradeBuffer.compute(key, (k, existing) -> {
            if (existing == null) {
                return new AggregatedTrade(alias, price, isBidAggressor, size, currentMs);
            } else {
                existing.add(price, isBidAggressor, size, currentMs);
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
                        String timestamp = currentTimestamp();
                        String wallMsg = buildWallRawText("WALL ADDED", state.alias, state.isBid, state.price,
                                state.currentDomSize, state.accumulatedSize, elapsedSeconds);
                        logCsvRow(state.alias,
                                createWallCsvRow(timestamp, state.alias, state.isBid, state.price, state.currentDomSize,
                                        elapsedSeconds, "WALL ADDED", wallMsg));
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
                processFlush(trade);
            }
        }
    }

    private void processFlush(AggregatedTrade trade) {
        logCsvRow(trade.alias, createDotCsvRow(currentTimestamp(), trade));
    }

    private void sendDotEvent(String alias, boolean isBuy, double price, int size) {
        Map<String, Object> payload = new LinkedHashMap<>();
        payload.put("isBuy", isBuy);
        payload.put("price", price);
        payload.put("size", size);

        Map<String, Object> sourceMeta = new LinkedHashMap<>();
        sourceMeta.put("alias", alias);

        sendContractEvent("dot", alias, currentTimestamp(), payload, sourceMeta);
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
                String timestampedMsg = logCsvRow(alias, parseAlertCsvRow(currentTimestamp(), alias, text, alert.showPopup));
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
        Map<String, Object> payload = new LinkedHashMap<>();
        payload.put("symbol", symbol);
        payload.put("text", text);
        payload.put("count", count);
        payload.put("popup", popup);

        Map<String, Object> sourceMeta = new LinkedHashMap<>();
        sourceMeta.put("symbol", symbol);

        sendContractEvent("alert", symbol, time, payload, sourceMeta);
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
        countLabel = new JLabel("Alerts: " + alertCount);
        countLabel.setFont(new Font("Arial", Font.BOLD, 12));
        countLabel.setForeground(new Color(0, 200, 100));
        gbc.gridx = 0;
        gbc.gridy = 0;
        gbc.weightx = 1.0;
        topPanel.add(countLabel, gbc);

        JButton socketStatusBtn = new JButton("");
        socketStatusBtn.setMargin(new Insets(0, 0, 0, 0));
        socketStatusBtn.setPreferredSize(new Dimension(12, 12));
        socketStatusBtn.setMinimumSize(new Dimension(12, 12));
        socketStatusBtn.setMaximumSize(new Dimension(12, 12));
        socketStatusButtons.add(socketStatusBtn);
        updateSocketStatusButtons();
        gbc.gridx = 1;
        gbc.gridy = 0;
        gbc.weightx = 0;
        topPanel.add(socketStatusBtn, gbc);

        JButton reconnectBtn = new JButton("Reconnect");
        reconnectBtn.setMargin(new Insets(2, 5, 2, 5));
        reconnectBtn.addActionListener(e -> reconnectSocket());
        gbc.gridx = 2;
        gbc.gridy = 0;
        gbc.weightx = 0;
        topPanel.add(reconnectBtn, gbc);

        // Row 1: Filters Title
        JLabel filterTitle = new JLabel("Filters:");
        filterTitle.setFont(new Font("Arial", Font.BOLD, 12));
        gbc.gridx = 0;
        gbc.gridy = 1;
        gbc.gridwidth = 3;
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
        filterArea.add(new JLabel("Dot:"), fgbc);
        fgbc.gridx = 1;
        fgbc.gridy = 0;
        fgbc.weightx = 0.7;
        JTextField dotVolField = new JTextField(String.valueOf(settings.minDotVol), 3);
        filterArea.add(dotVolField, fgbc);

        fgbc.gridx = 0;
        fgbc.gridy = 1;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Agg(ms):"), fgbc);
        fgbc.gridx = 1;
        fgbc.gridy = 1;
        fgbc.weightx = 0.7;
        JTextField aggWinField = new JTextField(String.valueOf(settings.aggWindowMs), 3);
        filterArea.add(aggWinField, fgbc);

        fgbc.gridx = 0;
        fgbc.gridy = 2;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Max(ms):"), fgbc);
        fgbc.gridx = 1;
        fgbc.gridy = 2;
        fgbc.weightx = 0.7;
        JTextField maxAggField = new JTextField(String.valueOf(settings.maxAggMs), 3);
        filterArea.add(maxAggField, fgbc);

        // COLUMN 2: WALL SETTINGS (Right)
        fgbc.gridx = 2;
        fgbc.gridy = 0;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Added:"), fgbc);
        fgbc.gridx = 3;
        fgbc.gridy = 0;
        fgbc.weightx = 0.7;
        JTextField wallAddedField = new JTextField(String.valueOf(settings.minWallSizeAdded), 3);
        filterArea.add(wallAddedField, fgbc);

        fgbc.gridx = 2;
        fgbc.gridy = 1;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Removed:"), fgbc);
        fgbc.gridx = 3;
        fgbc.gridy = 1;
        fgbc.weightx = 0.7;
        JTextField wallRemovedField = new JTextField(String.valueOf(settings.minWallSizeRemoved), 3);
        filterArea.add(wallRemovedField, fgbc);

        fgbc.gridx = 2;
        fgbc.gridy = 2;
        fgbc.weightx = 0.3;
        filterArea.add(new JLabel("Dur(s):"), fgbc);
        fgbc.gridx = 3;
        fgbc.gridy = 2;
        fgbc.weightx = 0.7;
        JTextField wallDurField = new JTextField(String.valueOf(settings.minWallDur), 3);
        filterArea.add(wallDurField, fgbc);

        gbc.gridx = 0;
        gbc.gridy = 2;
        gbc.gridwidth = 3;
        gbc.insets = new Insets(5, 2, 5, 2);
        topPanel.add(filterArea, gbc);

        // Row 3: CSV Output Path
        JPanel csvPathPanel = new JPanel(new BorderLayout(0, 2));
        csvPathPanel.add(new JLabel("CSV Path:"), BorderLayout.NORTH);
        JTextField csvOutputPathField = new JTextField(csvOutputPath, 18);
        csvPathPanel.add(csvOutputPathField, BorderLayout.CENTER);

        gbc.gridy = 3;
        gbc.insets = new Insets(2, 2, 5, 2);
        topPanel.add(csvPathPanel, gbc);

        // Row 4: Apply Button
        JButton applyBtn = new JButton("Apply");
        applyBtn.setMargin(new Insets(2, 10, 2, 10));
        applyBtn.addActionListener(e -> {
            try {
                File previousOutputDirectory = getCsvOutputDirectory();
                settings.minDotVol = Integer.parseInt(dotVolField.getText().trim());
                settings.minWallSizeAdded = Integer.parseInt(wallAddedField.getText().trim());
                settings.minWallSizeRemoved = Integer.parseInt(wallRemovedField.getText().trim());
                settings.minWallDur = Integer.parseInt(wallDurField.getText().trim());
                settings.aggWindowMs = Integer.parseInt(aggWinField.getText().trim());
                settings.maxAggMs = Integer.parseInt(maxAggField.getText().trim());
                csvOutputPath = csvOutputPathField.getText().trim();
                File currentOutputDirectory = getCsvOutputDirectory();
                int migratedFiles = migrateCsvOutputFiles(previousOutputDirectory, currentOutputDirectory);
                saveConfig();
                logToUI(indicatorName,
                        "SYSTEM: Filters saved for " + indicatorName + ". Dot:" + settings.minDotVol + " Added:"
                                + settings.minWallSizeAdded + " Removed:" + settings.minWallSizeRemoved + " Dur:"
                                + settings.minWallDur + "s Agg:" + settings.aggWindowMs + "ms MaxDur:"
                                + settings.maxAggMs + "ms CSV Path:" + currentOutputDirectory.getAbsolutePath()
                                + " Migrated:" + migratedFiles);
            } catch (NumberFormatException ex) {
                logToUI(indicatorName, "ERROR: Invalid filter values for " + indicatorName + ". Please enter numbers.");
            }
        });
        gbc.gridy = 4;
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
            setAreaToHeader(areaForThisTab);

        });

        JButton openFolderButton = new JButton("Open Folder");
        openFolderButton.addActionListener(e -> openCsvOutputDirectory(indicatorName));

        bottomPanel.add(clearButton);
        bottomPanel.add(openFolderButton);

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
                    if (key.contains(".") && !key.startsWith("global.")) {
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

                csvOutputPath = prop.getProperty("global.csvOutputPath", DEFAULT_CSV_OUTPUT_PATH);

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
            prop.setProperty("global.csvOutputPath", getCsvOutputDirectory().getAbsolutePath());
            prop.store(output, null);
        } catch (IOException io) {
            System.err.println("Error saving config: " + io.getMessage());
        }
    }

    private void loadLogHistory(String alias, JTextArea area) {
        setAreaToHeader(area);
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

    private void setAreaToHeader(JTextArea area) {
        area.setText(csvHeader() + "\n");
    }

    @Override
    public void finish() {
        saveConfig();
        System.out.println("Alert Listener stopped. Total alerts received: " + alertCount);
        aggregationExecutor.shutdown();
        socketReconnectExecutor.shutdownNow();
        ListenableHelper.removeListeners(provider, this);

        closeSocketSilently();
    }
}
