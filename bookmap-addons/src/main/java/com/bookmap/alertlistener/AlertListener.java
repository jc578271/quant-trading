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

import javax.swing.*;
import java.awt.*;
import java.time.ZoneOffset;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;

@Layer1Attachable
@Layer1StrategyName("Alert Listener")
@Layer1ApiVersion(Layer1ApiVersionValue.VERSION2)
public class AlertListener implements
        Layer1ApiAdminAdapter,
        Layer1ApiFinishable,
        velox.api.layer1.Layer1CustomPanelsGetter {

    private final Layer1ApiProvider provider;
    private final List<String> alertLog = new ArrayList<>();
    private JTextArea logArea;
    private JLabel countLabel;
    private int alertCount = 0;
    private static final int MAX_LOG_LINES = 500;
    private static final DateTimeFormatter TIME_FMT = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'");

    public AlertListener(Layer1ApiProvider provider) {
        this.provider = provider;
        ListenableHelper.addListeners(provider, this);
        System.out.println("=== Alert Listener Started ===");
        System.out.println("Listening for Bookmap Alert Messages from all addons...");
        System.out.println("==============================");
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
        String time = ZonedDateTime.now(ZoneOffset.UTC).format(TIME_FMT);

        StringBuilder sb = new StringBuilder();
        sb.append("[").append(time).append("] ");
        sb.append("#").append(alertCount).append(" ");

        if (alert.textInfo != null && !alert.textInfo.isEmpty()) {
            sb.append(alert.textInfo);
        } else {
            sb.append("(no text)");
        }

        if (alert.showPopup) {
            sb.append(" [POPUP]");
        }

        String logEntry = sb.toString();

        // Print to console
        System.out.println("ALERT >> " + logEntry);

        // Add to log
        synchronized (alertLog) {
            alertLog.add(logEntry);
            // Trim old entries
            while (alertLog.size() > MAX_LOG_LINES) {
                alertLog.remove(0);
            }
        }

        // Update UI
        updateUI(logEntry);
    }

    private void updateUI(String logEntry) {
        SwingUtilities.invokeLater(() -> {
            if (logArea != null) {
                logArea.append(logEntry + "\n");
                // Auto-scroll to bottom
                logArea.setCaretPosition(logArea.getDocument().getLength());
            }
            if (countLabel != null) {
                countLabel.setText("Total Alerts: " + alertCount);
            }
        });
    }

    @Override
    public velox.gui.StrategyPanel[] getCustomGuiFor(String indicatorName, String indicatorFullName) {
        JPanel panel = new JPanel(new BorderLayout(5, 5));
        panel.setBorder(BorderFactory.createEmptyBorder(5, 5, 5, 5));

        // Header
        countLabel = new JLabel("Total Alerts: " + alertCount);
        countLabel.setFont(new Font("Arial", Font.BOLD, 14));
        countLabel.setForeground(new Color(0, 200, 100));
        panel.add(countLabel, BorderLayout.NORTH);

        // Log area
        logArea = new JTextArea(15, 40);
        logArea.setEditable(false);
        logArea.setFont(new Font("Consolas", Font.PLAIN, 11));
        logArea.setBackground(new Color(30, 30, 30));
        logArea.setForeground(new Color(200, 200, 200));
        logArea.setCaretColor(Color.WHITE);

        // Load existing log entries
        synchronized (alertLog) {
            for (String entry : alertLog) {
                logArea.append(entry + "\n");
            }
        }

        JScrollPane scrollPane = new JScrollPane(logArea);
        scrollPane.setPreferredSize(new Dimension(400, 300));
        panel.add(scrollPane, BorderLayout.CENTER);

        // Clear button
        JButton clearButton = new JButton("Clear Log");
        clearButton.addActionListener(e -> {
            synchronized (alertLog) {
                alertLog.clear();
            }
            logArea.setText("");
            alertCount = 0;
            countLabel.setText("Total Alerts: 0");
        });
        panel.add(clearButton, BorderLayout.SOUTH);

        velox.gui.StrategyPanel strategyPanel = new velox.gui.StrategyPanel("Alert Listener", true);
        strategyPanel.add(panel);
        return new velox.gui.StrategyPanel[] { strategyPanel };
    }

    @Override
    public void finish() {
        System.out.println("Alert Listener stopped. Total alerts received: " + alertCount);
        ListenableHelper.removeListeners(provider, this);
    }
}
