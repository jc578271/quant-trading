using cAlgo.API;
using cAlgo.API.Indicators;
using static cAlgo.FreeVolumeProfileV20;
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    public partial class FreeVolumeProfileV20 : Indicator
    {
        // *********** HVN + LVN ***********
        private void DrawVolumeNodes(Dictionary<double, double> profileDict, int iStart, DateTime x1_Start, DateTime xBar, ExtraProfiles extraTPO = ExtraProfiles.No, bool isIntraday = false, DateTime intraX1 = default, string fixedKey = "") 
        { 
            if (!NodesParams.EnableNodeDetection)
                return;
                
            string prefix = extraTPO == ExtraProfiles.Fixed ? fixedKey : $"{iStart}";
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
            nodesKernel ??= NodesParams.ProfileSmooth_Input == ProfileSmooth_Data.Gaussian ?
                            NodesAnalizer.FixedKernel() : NodesAnalizer.FixedCoefficients();
            
            // Smooth values
            double[] profileSmoothed = NodesParams.ProfileSmooth_Input == ProfileSmooth_Data.Gaussian ?
                                       NodesAnalizer.GaussianSmooth(profileValues, nodesKernel) : NodesAnalizer.SavitzkyGolay(profileValues, nodesKernel);
            
            // Get indexes of LVNs/HVNs
            var (hvnsRaw, lvnsRaw) = NodesParams.ProfileNode_Input switch {
                ProfileNode_Data.LocalMinMax => NodesAnalizer.FindLocalMinMax(profileSmoothed),
                ProfileNode_Data.Topology => NodesAnalizer.ProfileTopology(profileSmoothed),
                _ => NodesAnalizer.PercentileNodes(profileSmoothed, NodesParams.pctileHVN_Value, NodesParams.pctileLVN_Value)
            };
            
            // Filter it
            if (NodesParams.onlyStrongNodes)
            {
                double globalPoc = profileSmoothed.Max();

                double hvnPct = Math.Round(NodesParams.strongHVN_Pct / 100.0, 3);
                double lvnPct = Math.Round(NodesParams.strongLVN_Pct / 100.0, 3);

                var strongHvns = new List<int>();
                var strongLvns = new List<int>();

                foreach (int idx in hvnsRaw)
                {
                    if (profileSmoothed[idx] >= hvnPct * globalPoc)
                        strongHvns.Add(idx);
                }

                foreach (int idx in lvnsRaw)
                {
                    if (profileSmoothed[idx] <= lvnPct * globalPoc)
                        strongLvns.Add(idx);
                }

                hvnsRaw = strongHvns;
                lvnsRaw = strongLvns;
            }
                
            bool isRaw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw || NodesParams.ShowNode_Input == ShowNode_Data.LVN_Raw;
            bool isBands = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands || NodesParams.ShowNode_Input == ShowNode_Data.LVN_With_Bands;
            
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
                        
                        ChartTrendLine low = Chart.DrawTrendLine($"{prefix}_{nodeName}_Low_{idxLow}_{extraTPO}", x1_Start, lowPrice, xBar, lowPrice, ColorBand_Lower);
                        ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{nodeName}_{idxCenter}_{extraTPO}", x1_Start, centerPrice, xBar, centerPrice, _nodeColor);
                        ChartTrendLine high = Chart.DrawTrendLine($"{prefix}_{nodeName}_High_{idxHigh}_{extraTPO}", x1_Start, highPrice, xBar, highPrice, ColorBand_Upper);   
                        ChartRectangle rectBand = Chart.DrawRectangle($"{prefix}_{nodeName}_Band_{idxCenter}_{extraTPO}", x1_Start,  lowPrice, xBar, highPrice, ColorBand);
                        
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
                        
            // Split profile by LVNs
            var areasBetween = new List<(int Start, int End)>();
            int start = 0;
            foreach (int lvn in lvnsRaw)
            {
                areasBetween.Add((start, lvn));
                start = lvn;
            }
            areasBetween.Add((start, profileSmoothed.Length - 1));

            // Extract mini-bells
            var bells = new List<(int Start, int End, int Poc)>();
            foreach (var (Start, End) in areasBetween)
            {
                int startIndex = Start;
                int endIndex = End;

                if (endIndex <= startIndex)
                    continue;

                int pocIdx = startIndex;
                double maxVol = profileSmoothed[startIndex];

                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    if (profileSmoothed[i] > maxVol)
                    {
                        maxVol = profileSmoothed[i];
                        pocIdx = i;
                    }
                }

                bells.Add((startIndex, endIndex, pocIdx));
            }
            
            // Extract HVN/LVN/POC + Levels
            // [(low, center, high), ...]
            var hvnLevels = new List<(double Low, double Center, double High)>();
            var hvnIndexes = new List<(int Low, int Center, int High)>();

            var lvnLevels = new List<(double Low, double Center, double High)>();
            var lvnIndexes = new List<(int Low, int Center, int High)>();

            double hvnBandPct = Math.Round(NodesParams.bandHVN_Pct / 100.0, 3);
            double lvnBandPct = Math.Round(NodesParams.bandLVN_Pct / 100.0, 3);

            foreach (var (startIdx, endIdx, pocIdx) in bells)
            {
                // HVNs/POCs + levels
                var (hvnLow, hvnHigh) = NodesAnalizer.HVN_SymmetricVA(startIdx, endIdx, pocIdx, hvnBandPct);

                hvnLevels.Add( (profilePrices[hvnLow], profilePrices[pocIdx], profilePrices[hvnHigh]) );
                hvnIndexes.Add( (hvnLow, pocIdx, hvnHigh) );

                // LVNs + Levels
                var (lvnLow, lvnHigh) = NodesAnalizer.LVN_SymmetricBand( startIdx, endIdx, lvnBandPct);

                lvnIndexes.Add( (lvnLow, startIdx, lvnHigh) );
                lvnLevels.Add( (profilePrices[lvnLow], profilePrices[startIdx], profilePrices[lvnHigh]) );
            }
            
            // Let's draw
            ClearOldNodes();

            string node = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? "HVN" : "LVN";
            Color nodeColor = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ColorHVN : ColorLVN;
            
            var nodeLvls = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnLevels : lvnLevels;
            var nodeIdxes = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnIndexes : lvnIndexes;
            
            for (int i = 0; i < nodeLvls.Count; i++)
            {
                var level = nodeLvls[i];
                var index = nodeIdxes[i];
                
                ChartTrendLine low = Chart.DrawTrendLine($"{prefix}_{node}_Low_{index.Low}_{extraTPO}", x1_Start, level.Low, xBar, level.Low, ColorBand_Lower);   
                ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{node}_{index.Center}_{extraTPO}", x1_Start, level.Center, xBar, level.Center, nodeColor);   
                ChartTrendLine high = Chart.DrawTrendLine($"{prefix}_{node}_High_{index.High}_{extraTPO}", x1_Start, level.High, xBar, level.High, ColorBand_Upper);   
                ChartRectangle rectBand = Chart.DrawRectangle($"{prefix}_{node}_Band_{index.Center}_{extraTPO}", x1_Start, level.Low, xBar, level.High, ColorBand);
                
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

                DateTime extDate = extraTPO == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
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
                
                if (isIntraday && extraTPO != ExtraProfiles.MiniVP) {
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
                    ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{nodeRaw}_{idx}_{extraTPO}", x1_Start, nodePrice, xBar, nodePrice, nodeColor_Raw);
                    center.LineStyle = nodeStyle_Raw; center.Thickness = nodeThick_Raw;
                                        
                    DateTime extDate = extraTPO == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
                    if (NodesParams.extendNodes) {
                        if (!NodesParams.extendNodes_FromStart)
                            center.Time1 = xBar;
                        center.Time2 = extDate;
                    }
                    
                    if (isIntraday && extraTPO != ExtraProfiles.MiniVP)
                        center.Time1 = intraX1;
                }
            }
            void ClearOldNodes() {
                // 1º remove old price levels
                // 2º allow static-update of Params-Panel
                for (int i = 0; i < profilePrices.Length; i++)
                {
                    Chart.RemoveObject($"{prefix}_LVN_Low_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_LVN_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_LVN_High_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_LVN_Band_{i}_{extraTPO}");

                    Chart.RemoveObject($"{prefix}_HVN_Low_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_HVN_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_HVN_High_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_HVN_Band_{i}_{extraTPO}");
                }
            }
            DateTime extendDate() {
                string tfName = extraTPO == ExtraProfiles.No ?
                (GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? "D1" :
                    GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? "W1" : "Month1" ) :
                extraTPO == ExtraProfiles.MiniVP ? ProfileParams.MiniVPs_Timeframe.ShortName.ToString() :
                extraTPO == ExtraProfiles.Weekly ?  "W1" :  "Month1";

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

        // ========= ========== ==========

    }
}
