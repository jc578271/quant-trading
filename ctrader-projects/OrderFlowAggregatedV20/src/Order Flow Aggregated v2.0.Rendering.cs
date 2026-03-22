using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using static cAlgo.OrderFlowTicksV20;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    public partial class OrderFlowTicksV20 : Indicator
    {
        // *********** SOME SHARED FUCTIONS ***********
        private void VP_Tick(int index, bool isVP = false, ExtraProfiles extraVP = ExtraProfiles.No, string fixedKey = "")
        {
            DateTime startTime = Bars.OpenTimes[index];
            DateTime endTime = Bars.OpenTimes[index + 1];

            // For real-time market - ODF
            if (IsLastBar && !isVP && !BooleanUtils.isPriceBased_NewBar)
                endTime = TicksOHLC.LastBar.OpenTime;

            // For real-time market - VP
            // Run conditional only in the last bar of repaint loop
            if (IsLastBar && isVP && Bars.OpenTimes[index] == Bars.LastBar.OpenTime)
                endTime = TicksOHLC.LastBar.OpenTime;

            /*
                TicksOHLC.OpenTimes => .GetIndexByExactTime() and .GetIndexByTime() returns -1 for historical data
                So, the VP/Wicks loop can't be optimized like the ODF_Ticks/Agg' or VP' Python version.

                NEW IN ODF_AGG => Trying to get only 3 days of Order Flow was painfully/extremely slow...
                Just doing a simple thing, which is keeping the last used tickIndex for both VP/ODF.
                Performs the calculations/drawings at the speed of light, even for 1 month of ticks!
            */
            int startIndex = extraVP switch
            {
                ExtraProfiles.Monthly => !IsLastBar ? PerformanceTick.lastIdx_Monthly : PerformanceTick.startIdx_Monthly,
                ExtraProfiles.Weekly => !IsLastBar ? PerformanceTick.lastIdx_Weekly : PerformanceTick.startIdx_Weekly,
                ExtraProfiles.MiniVP => !IsLastBar ? PerformanceTick.lastIdx_Mini : PerformanceTick.startIdx_Mini,
                _ => !isVP ? PerformanceTick.lastIdx_Bars : (!IsLastBar ? PerformanceTick.lastIdx_MainVP : PerformanceTick.startIdx_MainVP)
            };

            // For real-time market - ODF
            if (IsLastBar && !isVP) {
                while (TicksOHLC.OpenTimes[startIndex] < startTime)
                    startIndex++;

                PerformanceTick.lastIdx_Bars = startIndex;
            }
            
            if (extraVP == ExtraProfiles.Fixed) {
                ChartRectangle rect = RangeObjs.rectangles.Where(x => x.Name == fixedKey).FirstOrDefault();
                DateTime start = rect.Time1 < rect.Time2 ? rect.Time1 : rect.Time2;
                DateTime normalizedStart = start.Date;
                
                // We should normalize this for O(1) operations
                startIndex = PerformanceTick.IndexesByDate.Any() ? PerformanceTick.IndexesByDate[normalizedStart] : 0;
            }
            
            int TF_idx = extraVP == ExtraProfiles.Fixed ? GetSegmentIndex(index) : index;
            List<double> whichSegment_VP = extraVP == ExtraProfiles.Fixed ? segmentsDict[TF_idx] : Segments_VP;

            // =======================
            bool modeIsBuySell = GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell; 
            bool modeIsDelta = GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta;            
            bool isNoDraw_MinMax = SpikeFilterParams.SpikeSource_Input == SpikeSource_Data.Sum_Delta || 
                BubblesChartParams.BubblesSource_Input switch {
                    BubblesSource_Data.Subtract_Delta =>  true,
                    BubblesSource_Data.Sum_Delta => true,
                    _ => false
                }; 
            
            double prevLoopTick = 0;
            for (int tickIndex = startIndex; tickIndex < TicksOHLC.Count; tickIndex++)
            {
                Bar tickBar;
                tickBar = TicksOHLC[tickIndex];
                
                // Fixed Range => Performance
                if (extraVP == ExtraProfiles.Fixed && startIndex == 0) {
                    // Just add the first tickIndex of current date.
                    DateTime normalizedDate = tickBar.OpenTime.Date;
                    if (!PerformanceTick.IndexesByDate.ContainsKey(normalizedDate))
                        PerformanceTick.IndexesByDate.Add(normalizedDate, tickIndex);
                }
                
                if (tickBar.OpenTime < startTime || tickBar.OpenTime > endTime)
                {
                    if (tickBar.OpenTime > endTime) {
                        // ODF
                        PerformanceTick.lastIdx_Bars = !isVP ? tickIndex : PerformanceTick.lastIdx_Bars;
                        // VP
                        if (isVP) {
                            _ = extraVP switch
                            {
                                ExtraProfiles.Monthly => PerformanceTick.lastIdx_Monthly = tickIndex,
                                ExtraProfiles.Weekly => PerformanceTick.lastIdx_Weekly = tickIndex,
                                ExtraProfiles.MiniVP => PerformanceTick.lastIdx_Mini = tickIndex,
                                ExtraProfiles.Fixed => 0,
                                _ => PerformanceTick.lastIdx_MainVP = tickIndex
                            };
                        }
                        break;
                    } else
                        continue;
                }

                if (prevLoopTick != 0)
                    RankVolume(tickBar.Close, prevLoopTick);

                prevLoopTick = tickBar.Close;
            }

            // =======================
            void RankVolume(double tickPrice, double prevTick)
            {
                List<double> segmentsSource = isVP ? whichSegment_VP : Segments_Bar;

                double prevSegmentValue = 0.0;
                for (int i = 0; i < segmentsSource.Count; i++)
                {
                    if (prevSegmentValue != 0 && tickPrice >= prevSegmentValue && tickPrice <= segmentsSource[i])
                    {
                        double priceKey = segmentsSource[i];

                        if (isVP)
                        {
                            if (extraVP != ExtraProfiles.No)
                            {
                                VolumeRankType extraRank = extraVP switch
                                {
                                    ExtraProfiles.Monthly => MonthlyRank,
                                    ExtraProfiles.Weekly => WeeklyRank,
                                    ExtraProfiles.Fixed => FixedRank[fixedKey],
                                    _ => MiniRank
                                };
                                UpdateExtraProfiles(extraRank, priceKey, tickPrice, prevTick);
                                return;
                            }

                            double prevDelta = 0;
                            if (modeIsDelta && ResultParams.ShowMinMaxDelta)
                                prevDelta = VP_DeltaRank.Values.Sum();

                            if (VP_VolumesRank.ContainsKey(priceKey))
                            {
                                VP_VolumesRank[priceKey] += 1;

                                if (modeIsBuySell || modeIsDelta) 
                                {
                                    if (tickPrice > prevTick)
                                        VP_VolumesRank_Up[priceKey] += 1;
                                    else if (tickPrice < prevTick)
                                        VP_VolumesRank_Down[priceKey] += 1;
                                    else if (tickPrice == prevTick)
                                    {
                                        VP_VolumesRank_Up[priceKey] += 1;
                                        VP_VolumesRank_Down[priceKey] += 1;
                                    }
                                    
                                    VP_VolumesRank_Subt[priceKey] = VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey];
                                }

                                if (modeIsDelta)
                                    VP_DeltaRank[priceKey] += (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]);
                            }
                            else
                            {
                                VP_VolumesRank.Add(priceKey, 1);

                                if (modeIsBuySell || modeIsDelta) 
                                {
                                    if (!VP_VolumesRank_Up.ContainsKey(priceKey))
                                        VP_VolumesRank_Up.Add(priceKey, 1);
                                    else
                                        VP_VolumesRank_Up[priceKey] += 1;

                                    if (!VP_VolumesRank_Down.ContainsKey(priceKey))
                                        VP_VolumesRank_Down.Add(priceKey, 1);
                                    else
                                        VP_VolumesRank_Down[priceKey] += 1;

                                    double value = VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey];
                                    if (!VP_VolumesRank_Subt.ContainsKey(priceKey))
                                        VP_VolumesRank_Subt.Add(priceKey, value);
                                    else
                                        VP_VolumesRank_Subt[priceKey] = value;
                                }

                                if (modeIsDelta) 
                                {
                                    if (!VP_DeltaRank.ContainsKey(priceKey))
                                        VP_DeltaRank.Add(priceKey, (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]));
                                    else
                                        VP_DeltaRank[priceKey] += (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]);
                                }
                            }

                            if (modeIsDelta && ResultParams.ShowMinMaxDelta)
                            {
                                double currentDelta = VP_DeltaRank.Values.Sum();
                                if (prevDelta > currentDelta)
                                    VP_MinMaxDelta[0] = prevDelta; // Min
                                if (prevDelta < currentDelta)
                                    VP_MinMaxDelta[1] = prevDelta; // Max before final delta
                            }
                        }
                        else
                        {
                            int prevDelta = 0;
                            if (modeIsDelta && (ResultParams.ShowMinMaxDelta || isNoDraw_MinMax))
                                prevDelta = DeltaRank.Values.Sum();                                

                            if (VolumesRank.ContainsKey(priceKey))
                            {
                                VolumesRank[priceKey] += 1;

                                if (modeIsBuySell || modeIsDelta) 
                                {
                                    if (tickPrice > prevTick)
                                        VolumesRank_Up[priceKey] += 1;
                                    else if (tickPrice < prevTick)
                                        VolumesRank_Down[priceKey] += 1;
                                    else if (tickPrice == prevTick)
                                    {
                                        VolumesRank_Up[priceKey] += 1;
                                        VolumesRank_Down[priceKey] += 1;
                                    }
                                }

                                if (modeIsDelta)
                                    DeltaRank[priceKey] += (VolumesRank_Up[priceKey] - VolumesRank_Down[priceKey]);
                            }
                            else
                            {
                                VolumesRank.Add(priceKey, 1);

                                if (modeIsBuySell || modeIsDelta) 
                                {
                                    if (!VolumesRank_Up.ContainsKey(priceKey))
                                        VolumesRank_Up.Add(priceKey, 1);
                                    else
                                        VolumesRank_Up[priceKey] += 1;

                                    if (!VolumesRank_Down.ContainsKey(priceKey))
                                        VolumesRank_Down.Add(priceKey, 1);
                                    else
                                        VolumesRank_Down[priceKey] += 1;
                                }

                                if (modeIsDelta) {
                                    if (!DeltaRank.ContainsKey(priceKey))
                                        DeltaRank.Add(priceKey, (VolumesRank_Up[priceKey] - VolumesRank_Down[priceKey]));
                                    else
                                        DeltaRank[priceKey] += (VolumesRank_Up[priceKey] - VolumesRank_Down[priceKey]);
                                }
                            }

                            if (modeIsDelta && (ResultParams.ShowMinMaxDelta || isNoDraw_MinMax))
                            {
                                int currentDelta = DeltaRank.Values.Sum();
                                if (prevDelta > currentDelta)
                                    MinMaxDelta[0] = prevDelta; // Min
                                if (prevDelta < currentDelta)
                                    MinMaxDelta[1] = prevDelta; // Max before final delta
                            }
                        }

                        break;
                    }
                    prevSegmentValue = segmentsSource[i];
                }
            }

            void UpdateExtraProfiles(VolumeRankType volRank, double priceKey, double tickPrice, double prevTick) {
                double prevDelta = 0;
                if (modeIsDelta && ResultParams.ShowMinMaxDelta)
                    prevDelta = volRank.Delta.Values.Sum();

                if (volRank.Normal.ContainsKey(priceKey))
                {
                    volRank.Normal[priceKey] += 1;
                    if (modeIsBuySell || modeIsDelta) 
                    {
                        if (tickPrice > prevTick)
                            volRank.Up[priceKey] += 1;
                        else if (tickPrice < prevTick)
                            volRank.Down[priceKey] += 1;
                        else if (tickPrice == prevTick)
                        {
                            volRank.Up[priceKey] += 1;
                            volRank.Down[priceKey] += 1;
                        }
                    }

                    if (modeIsDelta)
                        volRank.Delta[priceKey] += (volRank.Up[priceKey] - volRank.Down[priceKey]);
                }
                else
                {
                    volRank.Normal.Add(priceKey, 1);

                    if (modeIsBuySell || modeIsDelta) 
                    {
                        if (!volRank.Up.ContainsKey(priceKey))
                            volRank.Up.Add(priceKey, 1);
                        else
                            volRank.Up[priceKey] += 1;

                        if (!volRank.Down.ContainsKey(priceKey))
                            volRank.Down.Add(priceKey, 1);
                        else
                            volRank.Down[priceKey] += 1;
                    }
                    
                    if (modeIsDelta) 
                    {
                        if (!volRank.Delta.ContainsKey(priceKey))
                            volRank.Delta.Add(priceKey, (volRank.Up[priceKey] - volRank.Down[priceKey]));
                        else
                            volRank.Delta[priceKey] += (volRank.Up[priceKey] - volRank.Down[priceKey]);
                    }
                }

                if (modeIsDelta && ResultParams.ShowMinMaxDelta)
                {
                    double currentDelta = volRank.Delta.Values.Sum();
                    if (prevDelta > currentDelta)
                        volRank.MinMaxDelta[0] = prevDelta; // Min
                    if (prevDelta < currentDelta)
                        volRank.MinMaxDelta[1] = prevDelta; // Max before final delta
                }
            }
        }

        private double[] GetWicks(DateTime startTime, DateTime endTime)
        {
            double min = Int32.MaxValue;
            double max = 0;

            if (IsLastBar && !BooleanUtils.isPriceBased_NewBar)
                endTime = TicksOHLC.LastBar.OpenTime;

            for (int tickIndex = PerformanceTick.lastIdx_Wicks; tickIndex < TicksOHLC.Count; tickIndex++)
            {
                Bar tickBar = TicksOHLC[tickIndex];

                if (tickBar.OpenTime < startTime || tickBar.OpenTime > endTime) {
                    if (tickBar.OpenTime > endTime) {
                        PerformanceTick.lastIdx_Wicks = tickIndex;
                        break;
                    }
                    else
                        continue;
                }

                if (tickBar.Close < min)
                    min = tickBar.Close;
                else if (tickBar.Close > max)
                    max = tickBar.Close;
            }

            double[] toReturn = { min, max };
            return toReturn;
        }

        public string FormatBigNumber(double num)
        {
            /*
                MaxDigits = 2
                123        ->  123
                1234       ->  1.23k
                12345      ->  12.35k
                123456     ->  123.45k
                1234567    ->  1.23M
                12345678   ->  12.35M
                123456789  ->  123.56M
            */
            FormatMaxDigits_Data selected = FormatMaxDigits_Input;
            string digitsThousand = selected == FormatMaxDigits_Data.Two ? "0.##k" : selected == FormatMaxDigits_Data.One ? "0.#k" : "0.k";
            string digitsMillion = selected == FormatMaxDigits_Data.Two ? "0.##M" : selected == FormatMaxDigits_Data.One ? "0.#M" : "0.M";

            if (num >= 100000000) {
                return (num / 1000000D).ToString(digitsMillion);
            }
            if (num >= 1000000) {
                return (num / 1000000D).ToString(digitsMillion);
            }
            if (num >= 100000) {
                return (num / 1000D).ToString(digitsThousand);
            }
            if (num >= 10000) {
                return (num / 1000D).ToString(digitsThousand);
            }
            if (num >= 1000) {
                return (num / 1000D).ToString(digitsThousand);
            }

            return num.ToString("#,0");
        }

        private DateTime TimeBasedOffset(DateTime dateBar, bool isSubt = false) {
            // Offset by timebased timeframe (15m bar * nº bars of 15m)
            string[] timesBased = { "Minute", "Hour", "Daily", "Day", "Weekly", "Monthly" };
            string currentTimeframe = Chart.TimeFrame.ToString();

            // Required for Price-Based Charts for manual offset
            string tfName;
            if (timesBased.Any(currentTimeframe.Contains))
                tfName = Chart.TimeFrame.ShortName.ToString();
            else
                tfName = ProfileParams.OffsetTimeframeInput.ShortName.ToString();

            // Get the time-based interval value
            string tfString = string.Join("", tfName.Where(char.IsDigit));
            int tfValue = int.TryParse(tfString, out int value) ? value : 1;

            DateTime dateToReturn = dateBar;
            int offsetCondiditon = !isSubt ? (ProfileParams.OffsetBarsInput + 1) : Math.Max(2, ProfileParams.OffsetBarsInput - 1);
            if (tfName.Contains('m'))
                dateToReturn = dateBar.AddMinutes(tfValue * offsetCondiditon);
            else if (tfName.Contains('h'))
                dateToReturn = dateBar.AddHours(tfValue * offsetCondiditon);
            else if (tfName.Contains('D'))
                dateToReturn = dateBar.AddDays(tfValue * offsetCondiditon);
            else if (tfName.Contains('W'))
                dateToReturn = dateBar.AddDays(7 * offsetCondiditon);
            else if (tfName.Contains("Month1"))
                dateToReturn = dateBar.AddMonths(tfValue * offsetCondiditon);

            return dateToReturn;
        }

        private static string[] GetTimeLapse(double interval_ms)
        {
            // Dynamic TimeLapse Format
            // from Weis & Wykoff System
            TimeSpan ts = TimeSpan.FromMilliseconds(interval_ms);

            string timelapse_Suffix = "";
            double timelapse_Value = 0;

            double[] dividedTimestamp = { ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds };
            for (int i = 0; i < dividedTimestamp.Length; i++)
            {
                if (dividedTimestamp[i] != 0)
                {
                    string suffix = i switch {
                        4 => "ms",
                        3 => "s",
                        2 => "m",
                        1 => "h",
                        _ => "d"
                    };
                    timelapse_Value = suffix switch {
                        "ms" => ts.TotalMilliseconds,
                        "s" => ts.TotalSeconds,
                        "m" => ts.TotalMinutes,
                        "h" => ts.TotalHours,
                        _ => ts.TotalDays
                    };
                    timelapse_Suffix = suffix;
                    break;
                }
            }
            string[] interval_timelapse = { $"{Math.Round(timelapse_Value, 1)}", timelapse_Suffix };
            return interval_timelapse;
        }

        private void DrawOnScreen(string msg)
        {
            Chart.DrawStaticText("txt", $"{msg}", VerticalAlignment.Top, HorizontalAlignment.Center, Color.LightBlue);
        }

        private void Second_DrawOnScreen(string msg)
        {
            Chart.DrawStaticText("txt2", $"{msg}", VerticalAlignment.Top, HorizontalAlignment.Left, Color.LightBlue);
        }


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
            if (Zoom < MiscParams.DrawAtZoom_Value) {
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
                    rectangle.IsFilled = MiscParams.FillHist;
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
                    else {
                        // Create drawing and replace current infos
                        CreateDraw(info);
                        if (!PerfDrawingObjs.currentToRedraw.ContainsKey(0))
                            PerfDrawingObjs.currentToRedraw[0] = new Dictionary<string, DrawInfo>();
                        else
                            PerfDrawingObjs.currentToRedraw[0][info.Id] = info;
                    }
                }
            }
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
                    PerfDrawingObjs.redrawInfos[drawList.FirstOrDefault().BarIndex][info.Id] = info;
                }

                PerfDrawingObjs.currentToRedraw.Clear();
            }
        }

        // *********** VOLUME RENKO/RANGE ***********
        /*
            Original source code by srlcarlg (me) (https://ctrader.com/algos/indicators/show/3045)
            Uses Ticks Data to make the calculation of volume, just like Candles.

            Refactored in Order Flow Ticks v2.0 revision 1.5
            Improved in Order Flow Aggregated v2.0
        */
        private void VolumeInitialize(bool onlyDate = false)
        {
            DateTime lastBarDate = Bars.LastBar.OpenTime.Date;

            if (LoadTickFrom_Input == LoadTickFrom_Data.Custom) {
                // ==== Get datetime to load from: dd/mm/yyyy ====
                if (DateTime.TryParseExact(StringDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out TickObjs.fromDateTime)) {
                    if (TickObjs.fromDateTime > lastBarDate) {
                        TickObjs.fromDateTime = lastBarDate;
                        Notifications.ShowPopup(
                            NOTIFY_CAPTION,
                            $"Invalid DateTime '{StringDate}'. \nUsing '{TickObjs.fromDateTime.ToShortDateString()}",
                            PopupNotificationState.Error
                        );
                    }
                } else {
                    TickObjs.fromDateTime = lastBarDate;
                    Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        $"Invalid DateTime '{StringDate}'. \nUsing '{TickObjs.fromDateTime.ToShortDateString()}",
                        PopupNotificationState.Error
                    );
                }
            }
            else {
                TickObjs.fromDateTime = LoadTickFrom_Input switch {
                    LoadTickFrom_Data.Yesterday => MarketData.GetBars(TimeFrame.Daily).LastBar.OpenTime.Date,
                    LoadTickFrom_Data.Before_Yesterday => MarketData.GetBars(TimeFrame.Daily).Last(1).OpenTime.Date,
                    LoadTickFrom_Data.One_Week => MarketData.GetBars(TimeFrame.Weekly).LastBar.OpenTime.Date,
                    LoadTickFrom_Data.Two_Week => MarketData.GetBars(TimeFrame.Weekly).Last(1).OpenTime.Date,
                    LoadTickFrom_Data.Monthly => MarketData.GetBars(TimeFrame.Monthly).LastBar.OpenTime.Date,
                    _ => lastBarDate,
                };
            }

            if (onlyDate) {
                DrawStartVolumeLine();
                return;
            }

            // ==== Check if existing ticks data on the chart really needs more data ====
            TickObjs.firstTickTime = TicksOHLC.OpenTimes.FirstOrDefault();
            if (TickObjs.firstTickTime >= TickObjs.fromDateTime) {

                PopupNotification progressPopup = null;
                bool notifyIsMinimal = LoadTickNotify_Input == LoadTickNotify_Data.Minimal;
                if (notifyIsMinimal)
                    progressPopup = Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        $"[{Symbol.Name}] Loading Tick Data Synchronously...",
                        PopupNotificationState.InProgress
                    );

                while (TicksOHLC.OpenTimes.FirstOrDefault() > TickObjs.fromDateTime)
                {
                    int loadedCount = TicksOHLC.LoadMoreHistory();
                    if (LoadTickNotify_Input == LoadTickNotify_Data.Detailed) {
                        Notifications.ShowPopup(
                            NOTIFY_CAPTION,
                            $"[{Symbol.Name}] Loaded {loadedCount} Ticks. \nCurrent Tick Date: {TicksOHLC.OpenTimes.FirstOrDefault()}",
                            PopupNotificationState.Partial
                        );
                    }
                    if (loadedCount == 0)
                        break;
                }

                if (notifyIsMinimal)
                    progressPopup.Complete(PopupNotificationState.Success);
                else {
                    Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        $"[{Symbol.Name}] Synchronous Tick Data Collection Finished.",
                        PopupNotificationState.Success
                    );
                }
            }

            DrawStartVolumeLine();
        }

        private void DrawStartVolumeLine() {
            try {
                DateTime firstTickDate = TicksOHLC.OpenTimes.FirstOrDefault();
                ChartVerticalLine lineInfo = Chart.DrawVerticalLine("VolumeStart", firstTickDate, Color.Red);
                lineInfo.LineStyle = LineStyle.Lines;
                ChartText textInfo = Chart.DrawText("VolumeStartText", "Tick Volume Data \n ends here", firstTickDate, Bars.HighPrices[Bars.OpenTimes.GetIndexByTime(firstTickDate)], Color.Red);
                textInfo.HorizontalAlignment = HorizontalAlignment.Right;
                textInfo.VerticalAlignment = VerticalAlignment.Top;
                textInfo.FontSize = 8;
            } catch { };
        }
        private void DrawFromDateLine() {
            try {
                ChartVerticalLine lineInfo = Chart.DrawVerticalLine("FromDate", TickObjs.fromDateTime, Color.Yellow);
                lineInfo.LineStyle = LineStyle.Lines;
                ChartText textInfo = Chart.DrawText("FromDateText", "Target Tick Data", TickObjs.fromDateTime, Bars.HighPrices[Bars.OpenTimes.GetIndexByTime(TickObjs.fromDateTime)], Color.Yellow);
                textInfo.HorizontalAlignment = HorizontalAlignment.Left;
                textInfo.VerticalAlignment = VerticalAlignment.Center;
                textInfo.FontSize = 8;
            } catch { };
        }

        private void LoadMoreTicksOnChart()
        {
            /*
                At the moment, LoadMoreHistoryAsync() doesn't work
                while Calculate() is invoked for historical data (!IsLastBar)
                and loading at each price update (IsLastBar) isn't wanted.
                - Plus, LoadMoreHistory() performance seems better.

                NEW IN ODF_AGG => "Seems better"... famous last words.
                    - Asynchronous Tick Data loading has been added.
            */

            TickObjs.firstTickTime = TicksOHLC.OpenTimes.FirstOrDefault();
            if (TickObjs.firstTickTime > TickObjs.fromDateTime)
            {
                bool notifyIsMinimal = LoadTickNotify_Input == LoadTickNotify_Data.Minimal;
                PopupNotification progressPopup = null;

                if (LoadTickStrategy_Input == LoadTickStrategy_Data.On_ChartStart_Sync) {

                    if (notifyIsMinimal)
                        progressPopup = Notifications.ShowPopup(
                            NOTIFY_CAPTION,
                            $"[{Symbol.Name}] Loading Tick Data Synchronously...",
                            PopupNotificationState.InProgress
                        );

                    // "Freeze" the Chart at the beginning of Calculate()
                    while (TicksOHLC.OpenTimes.FirstOrDefault() > TickObjs.fromDateTime)
                    {
                        int loadedCount = TicksOHLC.LoadMoreHistory();
                        if (LoadTickNotify_Input == LoadTickNotify_Data.Detailed) {
                            Notifications.ShowPopup(
                                NOTIFY_CAPTION,
                                $"[{Symbol.Name}] Loaded {loadedCount} Ticks. \nCurrent Tick Date: {TicksOHLC.OpenTimes.FirstOrDefault()}",
                                PopupNotificationState.Partial
                            );
                        }
                        if (loadedCount == 0)
                            break;
                    }

                    if (notifyIsMinimal)
                        progressPopup.Complete(PopupNotificationState.Success);
                    else
                    {
                        Notifications.ShowPopup(
                            NOTIFY_CAPTION,
                            $"[{Symbol.Name}] Synchronous Tick Data Collection Finished.",
                            PopupNotificationState.Success
                        );
                    }
                    unlockChart();
                }
                else {
                    if (IsLastBar && !TickObjs.startAsyncLoading)
                        timerHandler.isAsyncLoading = true;
                }
            }
            else
                unlockChart();


            void unlockChart() {
                if (TickObjs.syncProgressBar != null) {
                    TickObjs.syncProgressBar.IsIndeterminate = false;
                    TickObjs.syncProgressBar.IsVisible = false;
                }
                TickObjs.syncProgressBar = null;
                TickObjs.isLoadingComplete = true;
                DrawStartVolumeLine();
            }
        }

        protected override void OnTimer()
        {
            RunSocketHeartbeat();

            if (timerHandler.isAsyncLoading)
            {
                if (!TickObjs.startAsyncLoading) {
                    string volumeLineInfo = "=> Zoom out and follow the Vertical Line";
                    TickObjs.asyncPopup = Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        $"[{Symbol.Name}] Loading Tick Data Asynchronously every 0.5 second...\n{volumeLineInfo}",
                        PopupNotificationState.InProgress
                    );
                    // Draw target date.
                    DrawFromDateLine();
                }

                if (!TickObjs.isLoadingComplete) {
                    TicksOHLC.LoadMoreHistoryAsync((_) => {
                        DateTime currentDate = _.Bars.FirstOrDefault().OpenTime;

                        DrawStartVolumeLine();

                        if (currentDate <= TickObjs.fromDateTime) {

                            if (TickObjs.asyncPopup.State != PopupNotificationState.Success)
                                TickObjs.asyncPopup.Complete(PopupNotificationState.Success);

                            if (LoadTickNotify_Input == LoadTickNotify_Data.Detailed) {
                                Notifications.ShowPopup(
                                    NOTIFY_CAPTION,
                                    $"[{Symbol.Name}] Asynchronous Tick Data Collection Finished.",
                                    PopupNotificationState.Success
                                );
                            }

                            TickObjs.isLoadingComplete = true;
                        }
                    });

                    TickObjs.startAsyncLoading = true;
                }
                else {
                    DrawOnScreen("");
                    Second_DrawOnScreen("");
                    timerHandler.isAsyncLoading = false;
                    ClearAndRecalculate();
                    Timer.Start(TimeSpan.FromSeconds(1));
                }
            }
        }


    }
}
