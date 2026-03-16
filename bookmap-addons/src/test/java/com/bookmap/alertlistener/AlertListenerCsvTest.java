package com.bookmap.alertlistener;

import org.junit.Test;

import java.io.File;
import java.nio.file.Files;
import java.util.List;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

public class AlertListenerCsvTest {

    private static final String TS = "2026-03-16T00:00:00.0000000Z";

    @Test
    public void parsesHiddenBidDevelopment() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: HIDDEN BID DEVELOPMENT, V: 25 at 6775.25",
                false);

        assertEquals("HIDDEN DEVELOPMENT", row.type);
        assertEquals("BID", row.side);
        assertEquals("6775.25", row.price);
        assertEquals("25", row.value);
    }

    @Test
    public void parsesHiddenAskN() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: HIDDEN ASK-N, V: 300 (300) at 6778",
                false);

        assertEquals("HIDDEN", row.type);
        assertEquals("ASK", row.side);
        assertEquals("6778", row.price);
        assertEquals("300", row.value);
    }

    @Test
    public void parsesAbsorptionSell() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: ABSORPTION SELL, V: 911 at 6783.75",
                false);

        assertEquals("ABSORPTION", row.type);
        assertEquals("BID", row.side);
        assertEquals("6783.75", row.price);
        assertEquals("911", row.value);
    }

    @Test
    public void parsesSweepBuy() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: SWEEP BUY, V: 1546, P: 12 at 6783.75",
                false);

        assertEquals("SWEEP", row.type);
        assertEquals("ASK", row.side);
        assertEquals("6783.75", row.price);
        assertEquals("1546", row.value);
    }

    @Test
    public void parsesImbalanceZeroCross() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: Imbalance ASK zero-cross at 6764.5",
                false);

        assertEquals("IMBALANCE ZERO-CROSS", row.type);
        assertEquals("ASK", row.side);
        assertEquals("6764.5", row.price);
        assertEquals("", row.value);
    }

    @Test
    public void parsesImbalanceRatio() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: Imbalance ASK 21% > BID at 6778.5",
                false);

        assertEquals("IMBALANCE RATIO", row.type);
        assertEquals("ASK", row.side);
        assertEquals("6778.5", row.price);
        assertEquals("21", row.value);
    }

    @Test
    public void parsesStopPopup() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME",
                "Stop ESH6.CME buy at 6765.76 volume 127 [POPUP]",
                true);

        assertEquals("STOP", row.type);
        assertEquals("ASK", row.side);
        assertEquals("6765.76", row.price);
        assertEquals("127", row.value);
    }

    @Test
    public void parsesMarketVolumeStop() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "ESH6.CME@RITHMIC: Market Volume Stop SELL of 1136 reached at 6778.25",
                false);

        assertEquals("MARKET VOLUME STOP", row.type);
        assertEquals("BID", row.side);
        assertEquals("6778.25", row.price);
        assertEquals("1136", row.value);
    }

    @Test
    public void parsesInconsistentMboData() {
        AlertListener.CsvLogRow row = AlertListener.parseAlertCsvRow(
                TS,
                "ESH6.CME@RITHMIC",
                "Inconsistent MBO data for ESH6.CME@RITHMIC. Stops and icebergs detection may be inaccurate. [POPUP]",
                true);

        assertEquals("INCONSISTENT MBO DATA", row.type);
        assertEquals("", row.side);
        assertEquals("", row.price);
        assertEquals("", row.value);
    }

    @Test
    public void serializesCsvWithEscaping() {
        AlertListener.CsvLogRow row = new AlertListener.CsvLogRow();
        row.timestamp = TS;
        row.alias = "ESH6.CME@RITHMIC";
        row.type = "UNKNOWN";
        row.rawText = "Text with, comma and \"quote\"";

        String csv = AlertListener.toCsvLine(row);

        assertTrue(csv.contains("\"Text with, comma and \"\"quote\"\"\""));
    }

    @Test
    public void normalizesDotSideFromDelta() {
        AlertListener.AggregatedTrade bidTrade = new AlertListener.AggregatedTrade("ESH6.CME@RITHMIC", 100.0, true, 50, 0);
        AlertListener.AggregatedTrade askTrade = new AlertListener.AggregatedTrade("ESH6.CME@RITHMIC", 100.0, false, 50, 0);
        AlertListener.AggregatedTrade flatTrade = new AlertListener.AggregatedTrade("ESH6.CME@RITHMIC", 100.0, true, 50, 0);
        flatTrade.askSize = 50;

        assertEquals("BID", AlertListener.createDotCsvRow(TS, bidTrade).side);
        assertEquals("ASK", AlertListener.createDotCsvRow(TS, askTrade).side);
        assertEquals("", AlertListener.createDotCsvRow(TS, flatTrade).side);
    }

    @Test
    public void keepsDotBidAskSizesInBookmapOrder() {
        AlertListener.AggregatedTrade trade = new AlertListener.AggregatedTrade("ESH6.CME@RITHMIC", 100.0, true, 154, 0);
        trade.askSize = 248;

        AlertListener.CsvLogRow row = AlertListener.createDotCsvRow(TS, trade);

        assertEquals("154", row.bidSize);
        assertEquals("248", row.askSize);
        assertEquals("ASK", row.side);
    }

    @Test
    public void keepsWallSideFromDepthSide() {
        AlertListener.CsvLogRow bidWall = AlertListener.createWallCsvRow(TS, "ESH6.CME@RITHMIC", true, 6700, 100, 5, "WALL ADDED", "raw");
        AlertListener.CsvLogRow askWall = AlertListener.createWallCsvRow(TS, "ESH6.CME@RITHMIC", false, 6701, 120, 5, "WALL ADDED", "raw");

        assertEquals("BID", bidWall.side);
        assertEquals("ASK", askWall.side);
    }

    @Test
    public void writesHeaderOnlyOnce() throws Exception {
        File file = File.createTempFile("alert-listener", ".csv");
        assertTrue(file.delete());

        AlertListener.CsvLogRow row = new AlertListener.CsvLogRow();
        row.timestamp = TS;
        row.type = "SYSTEM";
        row.rawText = "hello";

        String csvLine = AlertListener.toCsvLine(row);
        AlertListener.appendCsvLine(file, csvLine);
        AlertListener.appendCsvLine(file, csvLine);

        List<String> lines = Files.readAllLines(file.toPath());
        assertEquals("timestamp,alias,type,side,price,value,price_min,price_max,bid_size,ask_size,duration_sec,raw_text", lines.get(0));
        assertEquals(3, lines.size());
        assertEquals(1, lines.stream().filter(AlertListener.csvHeader()::equals).count());
    }
}
