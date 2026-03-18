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
        // *********** HVN + LVN ***********
        private void DrawVolumeNodes(IDictionary<double, double> profileDict, int iStart, DateTime x1_Start, DateTime xBar, ExtraProfiles extraVP = ExtraProfiles.No, bool isIntraday = false, DateTime intraX1 = default, string fixedKey = "")
        {
            if (!NodesParams.EnableNodeDetection)
                return;

            string prefix = extraVP == ExtraProfiles.Fixed ? fixedKey : $"{iStart}";
            /*
                Alternatives for ordering:
                - "SortedDictionary<>()"
                    - for [TPO_Rank_Histogram, TPORankType.TPO_Histogram] dicts
                - tpoDict.OrderBy(x => x.key).ToDictionary(kv => kv.Key, kv => kv.Value);
                    - Then .ToArray()
                - https://dotnettips.wordpress.com/2018/01/30/performance-sorteddictionary-vs-dictionary/
            */

            // This approach seems more efficient.
            double[] profilePrices = profileDict.Keys.ToArray();
            Array.Sort(profilePrices);
            double[] profileValues = profilePrices.Select(key => profileDict[key]).ToArray();
            /*
            // Alternative, no LINQ
            double[] profileValues = new double[profilePrices.Length];
            for (int i = 0; i < profilePrices.Length; i++)
                profileValues[i] = tpoDict[profilePrices[i]];
            */

            // Calculate Kernels/Coefficientes only once.
            // nodesKernel should be null (params-panel)
            nodesKernel ??= NodesParams.ProfileSmooth_Input == ProfileSmooth_Data.Gaussian ?
                            NodesAnalizer.FixedKernel() :
                            NodesAnalizer.FixedCoefficients();

            // Smooth values
            double[] profileSmoothed = NodesParams.ProfileSmooth_Input == ProfileSmooth_Data.Gaussian ?
                                       NodesAnalizer.GaussianSmooth(profileValues, nodesKernel) :
                                       NodesAnalizer.SavitzkyGolay(profileValues, nodesKernel);

            // Get indexes of LVNs/HVNs
            var (hvnsRaw, lvnsRaw) = NodesParams.ProfileNode_Input switch {
                ProfileNode_Data.LocalMinMax => NodesAnalizer.FindLocalMinMax(profileSmoothed),
                ProfileNode_Data.Topology => NodesAnalizer.ProfileTopology(profileSmoothed),
                _ => NodesAnalizer.PercentileNodes(profileSmoothed, NodesParams.pctileHVN_Value, NodesParams.pctileLVN_Value)
            };

            // Filter it
            if (NodesParams.onlyStrongNodes)
                (hvnsRaw, lvnsRaw) = NodesAnalizer.GetStrongNodes(profileSmoothed, hvnsRaw, lvnsRaw, NodesParams.strongHVN_Pct, NodesParams.strongLVN_Pct);

            bool isRaw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw || NodesParams.ShowNode_Input == ShowNode_Data.LVN_Raw;
            bool isBands = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands || NodesParams.ShowNode_Input == ShowNode_Data.LVN_With_Bands;

            // Let's draw if ProfileNode_Data.Percentile
            if (NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile)
            {
                ClearOldNodes();

                if (isBands)
                {
                    Color _nodeColor = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ColorHVN : ColorLVN;

                    var hvnsGroups = NodesAnalizer.GroupConsecutiveIndexes(hvnsRaw);
                    var lvnsGroups = NodesAnalizer.GroupConsecutiveIndexes(lvnsRaw);
                    List<List<int>> nodeGroups = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnsGroups : lvnsGroups;

                    string nodeName = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? "HVN" : "LVN";
                    foreach (var group in nodeGroups)
                    {
                        int idxLow = group[0];
                        int idxCenter = group[group.Count / 2];
                        int idxHigh = group[group.Count - 1];

                        double lowPrice = profilePrices[idxLow];
                        double centerPrice = profilePrices[idxCenter];
                        double highPrice = profilePrices[idxHigh];

                        ChartTrendLine low = Chart.DrawTrendLine($"{prefix}_{nodeName}_Low_{idxLow}_{extraVP}", x1_Start, lowPrice, xBar, lowPrice, ColorBand_Lower);
                        ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{nodeName}_{idxCenter}_{extraVP}", x1_Start, centerPrice, xBar, centerPrice, _nodeColor);
                        ChartTrendLine high = Chart.DrawTrendLine($"{prefix}_{nodeName}_High_{idxHigh}_{extraVP}", x1_Start, highPrice, xBar, highPrice, ColorBand_Upper);
                        ChartRectangle rectBand = Chart.DrawRectangle($"{prefix}_{nodeName}_Band_{idxCenter}_{extraVP}", x1_Start,  lowPrice, xBar, highPrice, ColorBand);

                        FinalizeBands(low, center, high, rectBand);
                    }
                }
                else
                    DrawRawNodes();

                return;
            }

            // Draw raw-nodes, if applicable
            if (isRaw)  {
                ClearOldNodes();
                DrawRawNodes();
                return;
            }

            // Get Bands
            var (hvnLevels, hvnIndexes, lvnLevels, lvnIndexes) = NodesAnalizer.
            GetBandsTuples(profileSmoothed, profilePrices, lvnsRaw, NodesParams.bandHVN_Pct, NodesParams.bandLVN_Pct);

            // Let's draw
            ClearOldNodes();

            string node = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? "HVN" : "LVN";
            Color nodeColor = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ColorHVN : ColorLVN;

            var nodeLvls = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnLevels : lvnLevels;
            var nodeIdxes = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnIndexes : lvnIndexes;

            for (int i = 0; i < nodeLvls.Count; i++)
            {
                var (lvlLow, lvlCenter, lvlHigh) = nodeLvls[i];
                var (idxLow, idxCenter, idxHigh) = nodeIdxes[i];

                ChartTrendLine low = Chart.DrawTrendLine($"{prefix}_{node}_Low_{idxLow}_{extraVP}", x1_Start, lvlLow, xBar, lvlLow, ColorBand_Lower);
                ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{node}_{idxCenter}_{extraVP}", x1_Start, lvlCenter, xBar, lvlCenter, nodeColor);
                ChartTrendLine high = Chart.DrawTrendLine($"{prefix}_{node}_High_{idxHigh}_{extraVP}", x1_Start, lvlHigh, xBar, lvlHigh, ColorBand_Upper);
                ChartRectangle rectBand = Chart.DrawRectangle($"{prefix}_{node}_Band_{idxCenter}_{extraVP}", x1_Start, lvlLow, xBar, lvlHigh, ColorBand);

                FinalizeBands(low, center, high, rectBand);
            }

            // Local
            void FinalizeBands(ChartTrendLine low, ChartTrendLine center, ChartTrendLine high, ChartRectangle rectBand)
            {
                LineStyle nodeStyle = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? LineStyleHVN : LineStyleLVN;
                int  nodeThick = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ThicknessHVN : ThicknessLVN;

                rectBand.IsFilled = true;

                low.LineStyle = LineStyleBands; high.Thickness = ThicknessBands;
                center.LineStyle = nodeStyle; center.Thickness = nodeThick;
                high.LineStyle = LineStyleBands; high.Thickness = ThicknessBands;

                DateTime extDate = extraVP == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
                if (NodesParams.extendNodes)
                {
                    if (!NodesParams.extendNodes_FromStart) {
                        low.Time1 = xBar;
                        center.Time1 = xBar;
                        high.Time1 = xBar;
                        rectBand.Time1 = xBar;
                    }

                    center.Time2 = extDate;
                    if (NodesParams.extendNodes_WithBands) {
                        low.Time2 = extDate;
                        high.Time2 = extDate;
                        rectBand.Time2 = extDate;
                    }
                }

                if (isIntraday && extraVP != ExtraProfiles.MiniVP) {
                    low.Time1 = intraX1;
                    center.Time1 = intraX1;
                    high.Time1 = intraX1;
                    rectBand.Time1 = intraX1;
                }
            }
            void DrawRawNodes()
            {
                string nodeRaw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? "HVN" : "LVN";
                List<int> nodeIndexes = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? hvnsRaw : lvnsRaw;

                LineStyle nodeStyle_Raw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? LineStyleHVN : LineStyleLVN;
                int  nodeThick_Raw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? ThicknessHVN : ThicknessLVN;
                Color nodeColor_Raw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? ColorHVN : ColorLVN;

                foreach (int idx in nodeIndexes)
                {
                    double nodePrice = profilePrices[idx];
                    ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{nodeRaw}_{idx}_{extraVP}", x1_Start, nodePrice, xBar, nodePrice, nodeColor_Raw);
                    center.LineStyle = nodeStyle_Raw; center.Thickness = nodeThick_Raw;

                    DateTime extDate = extraVP == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
                    if (NodesParams.extendNodes) {
                        if (!NodesParams.extendNodes_FromStart)
                            center.Time1 = xBar;
                        center.Time2 = extDate;
                    }

                    if (isIntraday && extraVP != ExtraProfiles.MiniVP)
                        center.Time1 = intraX1;
                }
            }
            void ClearOldNodes() {
                // 1º remove old price levels
                // 2º allow static-update of Params-Panel
                for (int i = 0; i < profilePrices.Length; i++)
                {
                    Chart.RemoveObject($"{prefix}_LVN_Low_{i}_{extraVP}");
                    Chart.RemoveObject($"{prefix}_LVN_{i}_{extraVP}");
                    Chart.RemoveObject($"{prefix}_LVN_High_{i}_{extraVP}");
                    Chart.RemoveObject($"{prefix}_LVN_Band_{i}_{extraVP}");

                    Chart.RemoveObject($"{prefix}_HVN_Low_{i}_{extraVP}");
                    Chart.RemoveObject($"{prefix}_HVN_{i}_{extraVP}");
                    Chart.RemoveObject($"{prefix}_HVN_High_{i}_{extraVP}");
                    Chart.RemoveObject($"{prefix}_HVN_Band_{i}_{extraVP}");
                }
            }
            DateTime extendDate() {
                string tfName = extraVP == ExtraProfiles.No ?
                (MiscParams.ODFInterval_Input == ODFInterval_Data.Daily ? "D1" :
                MiscParams.ODFInterval_Input == ODFInterval_Data.Weekly ? "W1" : "Month1" ) :
                extraVP == ExtraProfiles.MiniVP ? ProfileParams.MiniVPs_Timeframe.ShortName.ToString() :
                extraVP == ExtraProfiles.Weekly ?  "W1" :  "Month1";

                // Get the time-based interval value
                string tfString = string.Join("", tfName.Where(char.IsDigit));
                int tfValue = int.TryParse(tfString, out int value) ? value : 1;

                DateTime dateToReturn = xBar;
                if (tfName.Contains('m'))
                    dateToReturn = xBar.AddMinutes(tfValue * NodesParams.extendNodes_Count);
                else if (tfName.Contains('h'))
                    dateToReturn = xBar.AddHours(tfValue * NodesParams.extendNodes_Count);
                else if (tfName.Contains('D'))
                    dateToReturn = xBar.AddDays(tfValue * NodesParams.extendNodes_Count);
                else if (tfName.Contains('W'))
                    dateToReturn = xBar.AddDays(7 * NodesParams.extendNodes_Count);
                else if (tfName.Contains("Month1"))
                    dateToReturn = xBar.AddMonths(tfValue * NodesParams.extendNodes_Count);

                return dateToReturn;
            }
        }

        // **********************
        // The chart should already be clear, with no objects and bar colors.
        // Unless it's a static update.
        public void ClearAndRecalculate()
        {
            // The plot (sometimes in some options, like Volume View) is too fast, slow down a bit.
            Thread.Sleep(300);

            // Avoid it
            VerifyConflict();
            if (BooleanUtils.segmentsConflict)
                return;

            // LookBack from VP
            Bars ODF_Bars = MiscParams.ODFInterval_Input == ODFInterval_Data.Daily ? DailyBars : WeeklyBars;
            int firstIndex = Bars.OpenTimes.GetIndexByTime(ODF_Bars.OpenTimes.FirstOrDefault());

            // Get Index of ODF Interval to continue only in Lookback
            int iVerify = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
            while (ODF_Bars.ClosePrices.Count - iVerify > GeneralParams.Lookback) {
                firstIndex++;
                iVerify = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
            }

            // Daily or Weekly ODF
            int TF_idx = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
            int indexStart = Bars.OpenTimes.GetIndexByTime(ODF_Bars.OpenTimes[TF_idx]);

            // Weekly Profile but Daily ODF
            bool extraWeekly = ProfileParams.EnableWeeklyProfile && MiscParams.ODFInterval_Input == ODFInterval_Data.Daily;
            if (extraWeekly) {
                TF_idx = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
                indexStart = Bars.OpenTimes.GetIndexByTime(WeeklyBars.OpenTimes[TF_idx]);
            }

            // Monthly Profile
            bool extraMonthly = ProfileParams.EnableMonthlyProfile;
            if (extraMonthly) {
                TF_idx = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
                indexStart = Bars.OpenTimes.GetIndexByTime(MonthlyBars.OpenTimes[TF_idx]);
            }

            // Reset Tick Index.
            PerformanceTick.ResetAll();
            // Reset Drawings
            PerfDrawingObjs.ClearAll();
            // Reset last update
            ClearIdx.ResetAll();
            // Reset Segments
            // It's needed since TF_idx(start) changes if SegmentsInterval_Input is switched on the panel
            Segments_VP.Clear();
            GeneralParams.BarMaxDeltaCache.Clear();
            GeneralParams.BarMaxVolumeCache.Clear();
            GeneralParams.BarMaxBuySellCache.Clear();
            segmentInfo.Clear(); 
            // Reset Fixed Range
            foreach (ChartRectangle rect in RangeObjs.rectangles)
            {
                DateTime end = rect.Time1 < rect.Time2 ? rect.Time2 : rect.Time1;
                ResetFixedRange(rect.Name, end);
            }

            // Historical data
            for (int index = indexStart; index < Bars.Count; index++)
            {
                CreateSegments(index);

                if (PanelSwitch_Input != PanelSwitch_Data.Order_Flow_Ticks) {
                    CreateMonthlyVP(index);
                    CreateWeeklyVP(index);
                }
                // Calculate ODF only in lookback
                if (extraWeekly || extraMonthly) {
                    iVerify = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                    if (ODF_Bars.ClosePrices.Count - iVerify > GeneralParams.Lookback)
                        continue;
                }

                TF_idx = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                indexStart = Bars.OpenTimes.GetIndexByTime(ODF_Bars.OpenTimes[TF_idx]);

                if (index == indexStart ||
                   (index - 1) == indexStart && isPriceBased_Chart ||
                   (index - 1) == indexStart && (index - 1) != ClearIdx.MainVP)
                    MassiveCleanUp(indexStart, index);

                if (PanelSwitch_Input != PanelSwitch_Data.Order_Flow_Ticks) {
                    if (ProfileParams.EnableMainVP)
                        VolumeProfile(indexStart, index);
                    CreateMiniVPs(index);
                }
            
                if (PanelSwitch_Input != PanelSwitch_Data.Volume_Profile) {
                    try { CreateOrderflow(index); } catch { }
                }

            }

            BooleanUtils.configHasChanged = true;

            DrawStartVolumeLine();
            try { PerformanceDrawing(true); } catch { } // Draw without scroll or zoom

            void CreateOrderflow(int i) {
                // Required for Ultra Bubbles Levels in Historical Data
                BooleanLocks.LevelsToFalse();
                VolumesRank.Clear();
                VolumesRank_Up.Clear();
                VolumesRank_Down.Clear();
                DeltaRank.Clear();
                int[] resetDelta = {0, 0};
                MinMaxDelta = resetDelta;
                OrderFlow(i);
            }
        }
        private void VerifyConflict() {
            // Timeframes Conflict
            if (ProfileParams.EnableWeeklyProfile && MiscParams.SegmentsInterval_Input == SegmentsInterval_Data.Daily) {
                DrawOnScreen("Misc >> Segments should be set to 'Weekly' or 'Monthly' \n to calculate Weekly Profile");
                BooleanUtils.segmentsConflict = true;
                return;
            }
            if (ProfileParams.EnableMonthlyProfile && MiscParams.SegmentsInterval_Input != SegmentsInterval_Data.Monthly) {
                DrawOnScreen("Misc >> Segments should be set to 'Monthly' \n to calculate Monthly Profile");
                BooleanUtils.segmentsConflict = true;
                return;
            }
            if (MiscParams.ODFInterval_Input == ODFInterval_Data.Weekly && MiscParams.SegmentsInterval_Input == SegmentsInterval_Data.Daily) {
                DrawOnScreen("Misc >> Segments should be set to 'Weekly' or 'Monthly' \n to calculate Order Flow weekly");
                BooleanUtils.segmentsConflict = true;
                return;
            }
            BooleanUtils.segmentsConflict = false;
        }

        public void SetRowHeight(double number) {
            rowHeight = number;
        }
        public void SetLookback(int number) {
            GeneralParams.Lookback = number;
        }
        public void SetMiniVPsBars() {
            MiniVPs_Bars = MarketData.GetBars(ProfileParams.MiniVPs_Timeframe);
        }
        public double GetRowHeight() {
            return rowHeight;
        }
        public double GetLookback() {
            return GeneralParams.Lookback;
        }
    }
}
