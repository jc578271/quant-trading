using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using static cAlgo.WeisWyckoffSystemV20;
using System;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    public partial class WeisWyckoffSystemV20 : Indicator
    {

        // *********** PERFORMANCE DRAWING ***********
        /*
            An simple idea that came up during the development of ODF_AGG.
            LLM code generating was used to quickly test the idea concepts.

            - Re-draw => Objects are deleted and recreated each time,
                - Fastest approach
                - Removes only objects outside the visible chart range
                - when cleaning up the chart with Chart.RemoveAllObjects()
                    it takes only 1/0.5 seconds.

            - Hidden => Objects are never deleted, just .IsHidden = True.
                - Slowest approach
                - IsHidden = false, only in visibles objects.
                - when cleaning up the chart with Chart.RemoveAllObjects()
                    it lags/freezes the chart/panel UI,
                    the waiting time scales with the drawings count.
                - Lags at scrolling at MASSIVE hidden drawings count.
        */
        private void PerformanceDrawing(object obj)
        {
            int first = Chart.FirstVisibleBarIndex;
            int last = Chart.LastVisibleBarIndex;
            int visible = 0;

            // ==== Drawing at Zoom ====
            int Zoom = Chart.ZoomLevel;
            // Keep rectangles from Filters or VPs
            if (Zoom < DrawAtZoom_Value) {
                HiddenOrRemove(true);
                return;
            }

            void HiddenOrRemove(bool hiddenAll)
            {
                if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest && hiddenAll)
                {
                    foreach (var kvp in PerfDrawingObjs.hiddenInfos)
                    {
                        string drawName = kvp.Key;
                        ChartObject drawObj = kvp.Value;

                        // Extract index from name
                        string[] parts = drawName.Split('_');
                        if (parts.Length < 2) continue;
                        if (!int.TryParse(parts.FirstOrDefault(), out _)) continue;

                        drawObj.IsHidden = hiddenAll;
                    }
                }
                else if (DrawingStrategy_Input == DrawingStrategy_Data.Redraw_Fastest && hiddenAll) {
                    // Remove everything
                    foreach (var kvp in PerfDrawingObjs.redrawInfos.Values)
                    {
                        var drawInfoList = kvp.Values;
                        foreach (DrawInfo drawInfo in drawInfoList)
                            Chart.RemoveObject(drawInfo.Id);
                    }
                }

                DebugPerfDraw();
            }

            // ==== Drawing at scroll ====
            if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest) {
                // Display the hidden ones
                foreach (var kvp in PerfDrawingObjs.hiddenInfos)
                {
                    string drawName = kvp.Key;
                    ChartObject drawObj = kvp.Value;

                    // Extract index from name
                    string[] parts = drawName.Split('_');
                    if (parts.Length < 2) continue;
                    if (!int.TryParse(parts.FirstOrDefault(), out int idx)) continue;

                    bool isVis = idx >= first && idx <= last;
                    drawObj.IsHidden = !isVis;

                    if (ShowDrawingInfo) {
                        if (isVis) visible++;
                    }
                }
            }
            else {
                // Clean up
                foreach (var kvp in PerfDrawingObjs.redrawInfos)
                {
                    var drawInfoList = kvp.Value.Values;
                    foreach (DrawInfo drawInfo in drawInfoList)
                    {
                        // The actual lazy cleanup.
                        if (kvp.Key < first || kvp.Key > last)
                            Chart.RemoveObject(drawInfo.Id);
                    }
                }

                // Draw visible
                for (int i = first; i <= last; i++)
                {
                    if (!PerfDrawingObjs.redrawInfos.ContainsKey(i))
                        continue;

                    var drawInfoList = PerfDrawingObjs.redrawInfos[i].Values;
                    foreach (DrawInfo info in drawInfoList)
                    {
                        CreateDraw(info);
                        if (ShowDrawingInfo)
                            visible++;
                    }
                }
            }

            DebugPerfDraw();

            void DebugPerfDraw() {
                if (ShowDrawingInfo) {
                    PerfDrawingObjs.staticText_DebugPerfDraw ??= Chart.DrawStaticText("Debug_Perf_Draw", "", VerticalAlignment.Top, HorizontalAlignment.Left, Color.Lime);
                    bool IsHidden = DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest;
                    int cached = 0;
                    if (!IsHidden) {
                        foreach (var list in PerfDrawingObjs.redrawInfos.Values) {
                            cached += list.Count;
                        }
                    }
                    PerfDrawingObjs.staticText_DebugPerfDraw.Text = IsHidden ?
                        $"Hidden Mode\n Total Objects: {FormatBigNumber(PerfDrawingObjs.hiddenInfos.Values.Count)}\n Visible: {FormatBigNumber(visible)}" :
                        $"Redraw Mode\n Cached: {FormatBigNumber(PerfDrawingObjs.redrawInfos.Count)} bars\n Cached: {FormatBigNumber(cached)} objects\n Drawn: {FormatBigNumber(visible)}";
                }
            }
        }
        private ChartObject CreateDraw(DrawInfo info)
        {
            switch (info.Type)
            {
                case DrawType.Text:
                    ChartText text = Chart.DrawText(info.Id, info.Text, info.X1, info.Y1, info.Color);
                    text.HorizontalAlignment = info.horizontalAlignment;
                    text.VerticalAlignment = info.verticalAlignment;
                    text.FontSize = info.FontSize;
                    return text;
                case DrawType.Icon:
                    return Chart.DrawIcon(info.Id, info.IconType, info.X1, info.Y1, info.Color);

                case DrawType.Ellipse:
                    ChartEllipse ellipse = Chart.DrawEllipse(info.Id, info.X1, info.Y1, info.X2, info.Y2, info.Color);
                    ellipse.IsFilled = true;
                    return ellipse;

                case DrawType.Rectangle:
                    ChartRectangle rectangle = Chart.DrawRectangle(info.Id, info.X1, info.Y1, info.X2, info.Y2, info.Color);
                    return rectangle;

                default:
                    return null;
            }
        }
        private void DrawOrCache(DrawInfo info) {
            if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest)
            {
                if (!IsLastBar || BooleanUtils.isPriceBased_NewBar) {
                    ChartObject obj = CreateDraw(info);
                    obj.IsHidden = true;
                    PerfDrawingObjs.hiddenInfos[info.Id] = obj;
                } else {
                    ChartObject obj = CreateDraw(info);
                    // Replace current obj
                    if (!PerfDrawingObjs.currentToHidden.ContainsKey(0))
                        PerfDrawingObjs.currentToHidden[0] = new Dictionary<string, ChartObject>();
                    else
                        PerfDrawingObjs.currentToHidden[0][info.Id] = obj;
                }
            }
            else
            {
                // Add Keys if not present
                if (!PerfDrawingObjs.redrawInfos.ContainsKey(info.BarIndex)) {
                    PerfDrawingObjs.redrawInfos[info.BarIndex] = new Dictionary<string, DrawInfo> { { info.Id, info } };
                }
                else {
                    // Add/Replace drawing
                    if (!IsLastBar || BooleanUtils.isPriceBased_NewBar)
                        PerfDrawingObjs.redrawInfos[info.BarIndex][info.Id] = info;
                    else 
                    {
                        // Fix PerfDrawing => "Weis Waves => "Show Current Wave?" often flickering in live-market (Redraw_Fastest only)
                        if (WavesParams.ShowCurrentWave) 
                        {
                            string[] toReplace = {
                                $"{trendStartIndex}_WavesMisc",
                                $"{trendStartIndex}_WavesVolume",
                                $"{trendStartIndex}_WavesEvsR"
                            };
                            if (toReplace.Contains(info.Id)) {
                                int lastKey = info.BarIndex - 1;
                                foreach (string id in toReplace)
                                {
                                    if (PerfDrawingObjs.redrawInfos[lastKey].ContainsKey(id)) {
                                        if (info.Id == id)
                                            PerfDrawingObjs.redrawInfos[lastKey][id] = info;
                                    }
                                }
                            }
                        }
                        
                        // Create drawing and replace current infos
                        CreateDraw(info);
                        if (!PerfDrawingObjs.currentToRedraw.ContainsKey(0))
                            PerfDrawingObjs.currentToRedraw[0] = new Dictionary<string, DrawInfo>();
                        else
                            PerfDrawingObjs.currentToRedraw[0][info.Id] = info;
                    }
                }
            }

            // IMPORTANT! => set isPriceBased_NewBar to 'false' after using it
            BooleanUtils.isPriceBased_NewBar = false;
        }
        private void LiveDrawing(BarOpenedEventArgs obj) {
            // Working with Lists in Calculate() is painful.
            if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest) {
                List<ChartObject> objList = PerfDrawingObjs.currentToHidden[0].Values.ToList();

                foreach (var drawObj in objList)
                    PerfDrawingObjs.hiddenInfos[drawObj.Name] = drawObj;

                PerfDrawingObjs.currentToHidden.Clear();
            }
            else {
                List<DrawInfo> drawList = PerfDrawingObjs.currentToRedraw[0].Values.ToList();
                foreach (DrawInfo info in drawList) {
                    // Fix PerfDrawing => Wyckoff Bars => Numbers in live-market are always changed to '2'
                    PerfDrawingObjs.redrawInfos[info.BarIndex][info.Id] = info;
                    // previous  => The redrawInfos[drawList.FirstOrDefault().BarIndex] 
                }

                PerfDrawingObjs.currentToRedraw.Clear();
            }
        }

        // ************************ WEIS WAVE SYSTEM **************************
        /*
                                   Improved Weis Waves
                                           by
                                        srlcarlg

                          ====== References for Studies ======
        (Numbers-Renko 数字練行足) by akutsusho (https://www.tradingview.com/script/9BKOIhdl-Numbers-Renko) (Code concepts in PineScript)
        (ZigZag) by mike.ourednik (https://ctrader.com/algos/indicators/show/1419) (decreased a lot of code, base for any ZigZag)
        (Swing Gann) by TradeExperto (https://ctrader.com/algos/indicators/show/2521) (helped to make the structure of waves calculation)

        =========================================

        NEW IN Revision 1 (after ODF_AGG):
        - Instead of using the ZigZag, the DirectionChanged() method was doing the heavy job...
            - In order to use WWSystem on [Ticks, Range and time-based charts], the proper use of zigzag is needed.
        - Add [ATR, Pips] to Standard ZigZag.
        - Add simple Multi-Timeframe Price lookup.

                        ==== References for NoLag-HighLow ZigZag ===
        (Absolute ZigZag - 2024/2025) (https://tradingview.com/script/lRY74dha-Absolute-ZigZag-Lib/)
        // (The key idea for high/low bars analysis)
        (Professional ZigZag - 2011/2016) https://www.mql5.com/en/code/263
        // (The idea of High/Low order formation by looking at lower timeframes, seems to be the first one)

        I needed to simplify the High/Low Bars analysis because I wanted to keep the current ZigZag structure,
        which is quite optimized and easy to understand.
        Compared to "Absolute ZigZag" logic, I did:
            - Remove [High or Low] Priority, keep the Auto (lower timeframe order formation) for Time-Based charts only.
            - Add [Skip or None] Priority for "bars that have both a higher high and a higher low"
        */

    }
}
