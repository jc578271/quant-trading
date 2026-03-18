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
        // *********** VOLUME PROFILE BARS ***********
        private void VolumeProfile(int iStart, int index, ExtraProfiles extraProfiles = ExtraProfiles.No, bool isLoop = false, bool drawOnly = false, string fixedKey = "", double fixedLowest = 0, double fixedHighest = 0)
        {
            // Weekly/Monthly on Buy_Sell is a waste of time
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell && (extraProfiles == ExtraProfiles.Weekly || extraProfiles == ExtraProfiles.Monthly))
               return;
               
            if (extraProfiles == ExtraProfiles.Fixed && ProfileParams.SegmentsFixedRange_Input == SegmentsFixedRange_Data.From_Profile)
                CreateSegments_FromFixedRange(Bars.OpenPrices[iStart], fixedLowest, fixedHighest, fixedKey);
                
            // ==== VP ====
            if (!drawOnly)
                VP_Bars(index, extraProfiles, fixedKey);

            // ==== Drawing ====
            if (Segments_VP.Count == 0 || isLoop)
                return;

            // Results or Fixed Range
            
            Bars mainTF = GeneralParams.VPInterval_Input switch {
                VPInterval_Data.Weekly => WeeklyBars,
                VPInterval_Data.Monthly => MonthlyBars,
                _ => DailyBars
            };                           
            Bars TF_Bars = extraProfiles switch {
                ExtraProfiles.MiniVP => MiniVPs_Bars,
                ExtraProfiles.Weekly => WeeklyBars,
                ExtraProfiles.Monthly => MonthlyBars,
                // Fixed should use Monthly Bars, so TF_idx can be used by "whichSegment" variable
                ExtraProfiles.Fixed => MonthlyBars,
                _ => mainTF
            };
            int TF_idx = TF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

            bool gapWeekend = Bars.OpenTimes[iStart].DayOfWeek == DayOfWeek.Friday && Bars.OpenTimes[iStart].Hour < 2;
            DateTime x1_Start = Bars.OpenTimes[iStart + (gapWeekend ? 1 : 0)];
            DateTime xBar = Bars.OpenTimes[index];

            bool isIntraday = ProfileParams.ShowIntradayProfile && index == Chart.LastVisibleBarIndex && !isLoop;
            DateTime intraDate = xBar;
            
            // Any Volume Mode
            double maxLength = xBar.Subtract(x1_Start).TotalMilliseconds;
            
            HistWidth_Data selectedWidth = ProfileParams.HistogramWidth_Input;
            double maxWidth = ProfileParams.HistogramWidth_Input switch {
                HistWidth_Data._15 => 1.25,
                HistWidth_Data._30 => 1.50,
                HistWidth_Data._50 => 2,
                _ => 3
            };
            double maxHalfWidth = ProfileParams.HistogramWidth_Input switch {
                HistWidth_Data._15 => 1.12,
                HistWidth_Data._30 => 1.25,
                HistWidth_Data._50 => 1.40,
                _ => 1.75
            };

            double proportion_VP = maxLength - (maxLength / maxWidth);
            if (selectedWidth == HistWidth_Data._100)
                proportion_VP = maxLength;

            string prefix = extraProfiles == ExtraProfiles.Fixed ? fixedKey : $"{iStart}";
            bool histRightSide = ProfileParams.HistogramSide_Input == HistSide_Data.Right;

            // Profile Selection
            Dictionary<double, double> vpNormal = new();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Normal) {
                vpNormal = extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Normal,
                    ExtraProfiles.Weekly => WeeklyRank.Normal,
                    ExtraProfiles.MiniVP => MiniRank.Normal,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Normal,
                    _ => VP_VolumesRank
                };
            }

            Dictionary<double, double> vpBuy = new();
            Dictionary<double, double> vpSell = new();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell) {
                vpBuy = extraProfiles switch
                {
                    ExtraProfiles.MiniVP => MiniRank.Up,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Up,
                    _ => VP_VolumesRank_Up
                };
                vpSell = extraProfiles switch
                {
                    ExtraProfiles.MiniVP => MiniRank.Down,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Down,
                    _ => VP_VolumesRank_Down
                };
            }
            
            Dictionary<double, double> vpDelta = new();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta) {
                vpDelta = extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Delta,
                    ExtraProfiles.Weekly => WeeklyRank.Delta,
                    ExtraProfiles.MiniVP => MiniRank.Delta,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Delta,
                    _ => VP_DeltaRank
                };
            }
            
            // Same for all
            bool intraBool = extraProfiles switch
            {
                ExtraProfiles.Monthly => isIntraday,
                ExtraProfiles.Weekly => isIntraday,
                ExtraProfiles.MiniVP => false,
                ExtraProfiles.Fixed => false,
                _ => isIntraday
            };

            // (micro)Optimization for all modes
            double maxValue = GeneralParams.VolumeMode_Input switch {
                VolumeMode_Data.Normal => vpNormal.Any() ? vpNormal.Values.Max() : 0,
                VolumeMode_Data.Delta => vpDelta.Any() ? vpDelta.Values.Max() : 0,
                _ => 0
            };

            double buyMax = 0;
            double sellMax = 0;
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell) {
                buyMax = vpBuy.Any() ? vpBuy.Values.Max() : 0;
                sellMax = vpSell.Any() ? vpSell.Values.Max() : 0;
            }

            IEnumerable<double> negativeList = new List<double>();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta)
                negativeList = vpDelta.Values.Where(n => n < 0);

            // Segments selection
            List<double> whichSegment = extraProfiles == ExtraProfiles.Fixed ? GetRangeSegments(TF_idx, fixedKey) : Segments_VP;
            
            // Manual Refactoring.
            // LLM allucinates.
            for (int i = 0; i < whichSegment.Count; i++)
            {
                double priceKey = whichSegment[i];

                bool skip = extraProfiles switch
                {
                    ExtraProfiles.Monthly => !MonthlyRank.Normal.ContainsKey(priceKey),
                    ExtraProfiles.Weekly => !WeeklyRank.Normal.ContainsKey(priceKey),
                    ExtraProfiles.MiniVP => !MiniRank.Normal.ContainsKey(priceKey),
                    ExtraProfiles.Fixed => !FixedRank[fixedKey].Normal.ContainsKey(priceKey),
                    _ => !VP_VolumesRank.ContainsKey(priceKey),
                };
                if (skip)
                    continue;

                /*
                Indeed, the value of X-Axis is simply a rule of three,
                where the maximum value will be the maxLength (in Milliseconds),
                from there the math adjusts the histograms.
                    MaxValue    maxLength(ms)
                       x             ?(ms)
                The values 1.25 and 4 are the manually set values
                ===================
                NEW IN ODF_AGG => To avoid histograms unexpected behavior that occurs in historical data
                - on Price-Based Charts (sometimes in candles too) where interval goes through weekend
                  We'll skip 1 bar (friday) since Bar Index as X-axis didn't resolve the problem.
                */

                double lowerSegmentY1 = whichSegment[i] - rowHeight;
                double upperSegmentY2 = whichSegment[i];
                
                void DrawRectangle_Normal(double currentVolume, double maxVolume, bool intradayProfile = false)
                {
                    double proportion = currentVolume * proportion_VP;
                    double dynLength = proportion / maxVolume;

                    DateTime x2 = x1_Start.AddMilliseconds(dynLength);

                    Color histogramColor = extraProfiles switch
                    {
                        ExtraProfiles.Monthly => MonthlyColor,
                        ExtraProfiles.Weekly => WeeklyColor,
                        _ => HistColor,
                    };

                    if (ProfileParams.EnableGradient)
                    {
                        Color minColor = extraProfiles switch
                        {
                            ExtraProfiles.Monthly => MonthlyGrandient_Min,
                            ExtraProfiles.Weekly => WeeklyGrandient_Min,
                            _ => ColorGrandient_Min,
                        };

                        Color maxColor = extraProfiles switch
                        {
                            ExtraProfiles.Monthly => MonthlyGrandient_Max,
                            ExtraProfiles.Weekly => WeeklyGrandient_Max,
                            _ => ColorGrandient_Max,
                        };

                        double Intensity = (currentVolume * 100 / maxVolume) / 100;
                        double stepR = (maxColor.R - minColor.R) * Intensity;
                        double stepG = (maxColor.G - minColor.G) * Intensity;
                        double stepB = (maxColor.B - minColor.B) * Intensity;

                        int A = (int)(2.55 * OpacityHistInput);
                        int R = (int)Math.Round(minColor.R + stepR);
                        int G = (int)Math.Round(minColor.G + stepG);
                        int B = (int)Math.Round(minColor.B + stepB);

                        Color dynColor = Color.FromArgb(A, R, G, B);

                        histogramColor = dynColor;
                    }

                    ChartRectangle volHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Normal", x1_Start, lowerSegmentY1, x2, upperSegmentY2, histogramColor);

                    if (ProfileParams.FillHist_VP)
                        volHist.IsFilled = true;

                    if (histRightSide)
                    {
                        volHist.Time1 = xBar;
                        volHist.Time2 = xBar.AddMilliseconds(-dynLength);
                    }

                    if (intradayProfile && extraProfiles != ExtraProfiles.MiniVP)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        DateTime dateOffset_Duo = TimeBasedOffset(dateOffset, true);
                        DateTime dateOffset_Triple = TimeBasedOffset(dateOffset_Duo, true);

                        double maxLength_Intraday = dateOffset.Subtract(xBar).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Weekly)
                            maxLength_Intraday = dateOffset_Duo.Subtract(dateOffset).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Monthly)
                            maxLength_Intraday = dateOffset_Triple.Subtract(dateOffset_Duo).TotalMilliseconds;

                        // Recalculate histograms 'X' position
                        double proportion_Intraday = currentVolume * (maxLength_Intraday - (maxLength_Intraday / maxWidth));
                        if (selectedWidth == HistWidth_Data._100)
                            proportion_Intraday = currentVolume * maxLength_Intraday;

                        double dynLength_Intraday = proportion_Intraday / maxVolume;

                        // Set 'X'
                        volHist.Time1 = dateOffset;
                        volHist.Time2 = dateOffset.AddMilliseconds(-dynLength_Intraday);

                        if (extraProfiles == ExtraProfiles.Weekly)
                        {
                            volHist.Time1 = dateOffset_Duo;
                            volHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Intraday);
                            if (!ProfileParams.EnableMonthlyProfile && ProfileParams.FillIntradaySpace)
                            {
                                volHist.Time1 = dateOffset;
                                volHist.Time2 = dateOffset.AddMilliseconds(dynLength_Intraday);
                            }
                        }
                        if (extraProfiles == ExtraProfiles.Monthly)
                        {
                            if (ProfileParams.EnableWeeklyProfile) {
                                // Show after
                                volHist.Time1 = dateOffset_Triple;
                                volHist.Time2 = dateOffset_Triple.AddMilliseconds(-dynLength_Intraday);
                                // Show after together
                                if (ProfileParams.FillIntradaySpace) {
                                    volHist.Time1 = dateOffset_Duo;
                                    volHist.Time2 = dateOffset_Duo.AddMilliseconds(dynLength_Intraday);
                                }
                            }
                            else {
                                // Use Weekly position
                                volHist.Time1 = dateOffset_Duo;
                                volHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Intraday);
                                if (ProfileParams.FillIntradaySpace) {
                                    volHist.Time1 = dateOffset;
                                    volHist.Time2 = dateOffset.AddMilliseconds(dynLength_Intraday);
                                }
                            }
                        }

                        intraDate = volHist.Time1;
                    }
                }

                void DrawRectangle_BuySell(
                    double currentBuy, double currentSell,
                    double buyMax, double sellMax,
                    bool intradayProfile = false)
                {
                    // Buy vs Sell - already
                    double maxBuyVolume = buyMax;
                    double maxSellVolume = sellMax;

                    double maxSideVolume = maxBuyVolume > maxSellVolume ? maxBuyVolume : maxSellVolume;

                    double proportionBuy = 0;
                    try { proportionBuy = currentBuy * (maxLength - (maxLength / maxHalfWidth)); } catch { };
                    if (selectedWidth == HistWidth_Data._100)
                        try { proportionBuy = currentBuy * (maxLength - (maxLength / 3)); } catch { };

                    double dynLengthBuy = proportionBuy / maxSideVolume; ;

                    double proportionSell = 0;
                    try { proportionSell = currentSell * proportion_VP; } catch { };
                    double dynLengthSell = proportionSell / maxSideVolume;

                    DateTime x2_Sell = x1_Start.AddMilliseconds(dynLengthSell);
                    DateTime x2_Buy = x1_Start.AddMilliseconds(dynLengthBuy);

                    ChartRectangle buyHist, sellHist;
                    sellHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Sell", x1_Start, lowerSegmentY1, x2_Sell, upperSegmentY2, SellColor);
                    buyHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Buy", x1_Start, lowerSegmentY1, x2_Buy, upperSegmentY2, BuyColor);
                    if (ProfileParams.FillHist_VP)
                    {
                        buyHist.IsFilled = true;
                        sellHist.IsFilled = true;
                    }
                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                    {
                        sellHist.Time1 = xBar;
                        sellHist.Time2 = xBar.AddMilliseconds(-dynLengthSell);
                        buyHist.Time1 = xBar;
                        buyHist.Time2 = xBar.AddMilliseconds(-dynLengthBuy);
                    }

                    // Intraday Right Profile
                    if (intradayProfile && extraProfiles != ExtraProfiles.MiniVP)
                    {
                        // ==== Subtract Profile / Plain Delta - Profile View ====
                        // Recalculate histograms 'X' position
                        DateTime dateOffset_Subt = TimeBasedOffset(xBar);

                        double maxPositive = VP_VolumesRank_Subt.Values.Max();
                        IEnumerable<double> negativeVolumeList = VP_VolumesRank_Subt.Values.Where(n => n < 0);
                        double maxNegative = 0;
                        try { maxNegative = Math.Abs(negativeVolumeList.Min()); } catch { }

                        double subtMax = maxPositive > maxNegative ? maxPositive : maxNegative;

                        double maxLength_Intraday = dateOffset_Subt.Subtract(xBar).TotalMilliseconds;
                        double proportion_Intraday = VP_VolumesRank_Subt[priceKey] * (maxLength_Intraday - (maxLength_Intraday / maxWidth));
                        double dynLength = proportion_Intraday / subtMax;

                        // Set 'X'
                        DateTime x1 = dateOffset_Subt;
                        DateTime x2 = x1.AddMilliseconds(dynLength);

                        Color colorHist = dynLength > 0 ? BuyColor : SellColor;
                        ChartRectangle subtHist = Chart.DrawRectangle($"{iStart}_{i}_VP_Subt", x1, lowerSegmentY1, x2, upperSegmentY2, colorHist);

                        dynLength = -Math.Abs(dynLength);
                        subtHist.Time1 = dateOffset_Subt;
                        subtHist.Time2 = subtHist.Time2 != dateOffset_Subt ? dateOffset_Subt.AddMilliseconds(dynLength) : dateOffset_Subt;

                        if (ProfileParams.FillHist_VP)
                            subtHist.IsFilled = true;

                        intraDate = subtHist.Time1;

                        // ==== Buy_Sell - Divided View - Half Width ====
                        // Recalculate histograms 'X' position
                        DateTime dateOffset = TimeBasedOffset(dateOffset_Subt, true);
                        maxLength_Intraday = dateOffset.Subtract(dateOffset_Subt).TotalMilliseconds;

                        // Replaced maxHalfWidth to maxWidth since it's Divided View
                        proportionBuy = 0;
                        try { proportionBuy = currentBuy * (maxLength_Intraday - (maxLength_Intraday / maxHalfWidth)); } catch { };
                        if (selectedWidth == HistWidth_Data._100)
                            try { proportionBuy = currentBuy * maxLength_Intraday; } catch { };

                        dynLengthBuy = proportionBuy / maxBuyVolume; ;

                        proportionSell = 0;
                        try { proportionSell = currentSell * (maxLength_Intraday - (maxLength_Intraday / maxHalfWidth)); } catch { };
                        if (selectedWidth == HistWidth_Data._100)
                            try { proportionSell = currentSell * maxLength_Intraday; } catch { };

                        dynLengthSell = proportionSell / maxSellVolume;

                        // Set 'X'
                        sellHist.Time1 = dateOffset;
                        sellHist.Time2 = dateOffset.AddMilliseconds(-dynLengthSell);
                        buyHist.Time1 = dateOffset;
                        buyHist.Time2 = dateOffset.AddMilliseconds(dynLengthBuy);
                    }
                }

                void DrawRectangle_Delta(double currentDelta, double positiveDeltaMax, IEnumerable<double> negativeDeltaList, bool intradayProfile = false)
                {
                    double negativeDeltaMax = 0;
                    try { negativeDeltaMax = Math.Abs(negativeDeltaList.Min()); } catch { }

                    double deltaMax = positiveDeltaMax > negativeDeltaMax ? positiveDeltaMax : negativeDeltaMax;

                    double proportion_Delta = Math.Abs(currentDelta) * proportion_VP;
                    double dynLength_Delta = proportion_Delta / deltaMax;

                    Color colorHist = currentDelta >= 0 ? BuyColor : SellColor;
                    DateTime x2 = x1_Start.AddMilliseconds(dynLength_Delta);

                    ChartRectangle deltaHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Delta", x1_Start, lowerSegmentY1, x2, upperSegmentY2, colorHist);

                    if (ProfileParams.FillHist_VP)
                        deltaHist.IsFilled = true;

                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                    {
                        deltaHist.Time1 = xBar;
                        deltaHist.Time2 = deltaHist.Time2 != x1_Start ? xBar.AddMilliseconds(-dynLength_Delta) : x1_Start;
                    }

                    // Intraday Right Profile
                    if (intradayProfile && extraProfiles != ExtraProfiles.MiniVP)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        DateTime dateOffset_Duo = TimeBasedOffset(dateOffset, true);
                        DateTime dateOffset_Triple = TimeBasedOffset(dateOffset_Duo, true);
                        double maxLength_Intraday = dateOffset.Subtract(xBar).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Weekly)
                            maxLength_Intraday = dateOffset_Duo.Subtract(dateOffset).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Monthly)
                            maxLength_Intraday = dateOffset_Triple.Subtract(dateOffset_Duo).TotalMilliseconds;

                        // Recalculate histograms 'X' position
                        proportion_Delta = currentDelta * (maxLength_Intraday - (maxLength_Intraday / maxWidth));
                        if (selectedWidth == HistWidth_Data._100)
                            proportion_Delta = currentDelta * maxLength_Intraday;
                        dynLength_Delta = proportion_Delta / deltaMax;

                        colorHist = dynLength_Delta > 0 ? BuyColor : SellColor;
                        dynLength_Delta = Math.Abs(dynLength_Delta); // Profile view only

                        // Set 'X'
                        deltaHist.Time1 = dateOffset;
                        deltaHist.Time2 = deltaHist.Time2 != dateOffset ? dateOffset.AddMilliseconds(-dynLength_Delta) : dateOffset;
                        deltaHist.Color = colorHist;

                        if (extraProfiles == ExtraProfiles.Weekly) {
                            deltaHist.Time1 = dateOffset_Duo;
                            deltaHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Delta);
                            if (!ProfileParams.EnableMonthlyProfile && ProfileParams.FillIntradaySpace) {
                                deltaHist.Time1 = dateOffset;
                                deltaHist.Time2 = dateOffset.AddMilliseconds(dynLength_Delta);
                            }
                        }

                        if (extraProfiles == ExtraProfiles.Monthly) {
                            if (ProfileParams.EnableWeeklyProfile) {
                                // Show after
                                deltaHist.Time1 = dateOffset_Triple;
                                deltaHist.Time2 = dateOffset_Triple.AddMilliseconds(-dynLength_Delta);
                                // Show after together
                                if (ProfileParams.FillIntradaySpace) {
                                    deltaHist.Time1 = dateOffset_Duo;
                                    deltaHist.Time2 = dateOffset_Duo.AddMilliseconds(dynLength_Delta);
                                }
                            }
                            else {
                                // Use Weekly position
                                deltaHist.Time1 = dateOffset_Duo;
                                deltaHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Delta);
                                if (ProfileParams.FillIntradaySpace) {
                                    deltaHist.Time1 = dateOffset;
                                    deltaHist.Time2 = dateOffset.AddMilliseconds(dynLength_Delta);
                                }
                            }
                        }

                        intraDate = deltaHist.Time1;
                    }
                }
                
                switch (GeneralParams.VolumeMode_Input) 
                {
                    case VolumeMode_Data.Normal:
                    {
                        double value = vpNormal[priceKey];
                        // Draw histograms and update 'intraDate', if applicable
                        DrawRectangle_Normal(value, maxValue, intraBool);
                        break;
                    }
                    case VolumeMode_Data.Buy_Sell:             
                    {
                        if (vpBuy.ContainsKey(priceKey) && vpSell.ContainsKey(priceKey))
                            DrawRectangle_BuySell(vpBuy[priceKey], vpSell[priceKey], buyMax, sellMax, isIntraday);
                        break;
                    }
                    default:
                    {
                        double value = vpDelta[priceKey];
                        // Draw histograms and update 'intraDate', if applicable
                        DrawRectangle_Delta(value, maxValue, negativeList, intraBool);
                        break;
                    }   
                }
            }

            // Drawings that don't require each segment-price as y-axis
            // It can/should be outside SegmentsLoop for better performance.
            
            double lowest = TF_Bars.LowPrices[TF_idx];
            double highest = TF_Bars.HighPrices[TF_idx];
            if (double.IsNaN(lowest)) { // Mini VPs avoid crash after recalculating
                lowest = TF_Bars.LowPrices.LastValue;
                highest = TF_Bars.HighPrices.LastValue;
            }
            double y1_lowest = extraProfiles == ExtraProfiles.Fixed ? fixedLowest : lowest;

            if (extraProfiles == ExtraProfiles.MiniVP && ProfileParams.ShowMiniResults || 
                extraProfiles != ExtraProfiles.MiniVP && ResultParams.ShowResults)
            {
                switch (GeneralParams.VolumeMode_Input) 
                {
                    case VolumeMode_Data.Normal:
                    {
                        double sum = Math.Round(vpNormal.Values.Sum());
                        string strValue = FormatResults ? FormatBigNumber(sum) : $"{sum}";

                        ChartText Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Normal_Result", $"\n{strValue}", x1_Start, y1_lowest, ProfileParams.EnableGradient ? ColorGrandient_Min : HistColor);
                        Center.HorizontalAlignment = HorizontalAlignment.Center;
                        Center.FontSize = FontSizeResults - 1;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                            Center.Time = xBar;

                        // Intraday Right Profile
                        if (isIntraday && extraProfiles == ExtraProfiles.No) {
                            DateTime dateOffset = TimeBasedOffset(xBar);
                            Center.Time = dateOffset;
                        }     
                        break;
                    }
                    case VolumeMode_Data.Buy_Sell:
                    {
                        double volBuy = vpBuy.Values.Sum();
                        double volSell = vpSell.Values.Sum();

                        double percentBuy = (volBuy * 100) / (volBuy + volSell);
                        double percentSell = (volSell * 100) / (volBuy + volSell);
                        percentBuy = Math.Round(percentBuy);
                        percentSell = Math.Round(percentSell);

                        ChartText Left, Right;
                        Left = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Sell_Sum", $"{percentSell}%", x1_Start, y1_lowest, SellColor);
                        Right = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Buy_Sum", $"{percentBuy}%", x1_Start, y1_lowest, BuyColor);
                        Left.HorizontalAlignment = HorizontalAlignment.Left;
                        Right.HorizontalAlignment = HorizontalAlignment.Right;
                        Left.FontSize = FontSizeResults;
                        Right.FontSize = FontSizeResults;

                        ChartText Center;
                        double sum = Math.Round(volBuy + volSell);
                        double subtract = Math.Round(volBuy - volSell);
                        double divide = 0;
                        if (volBuy != 0 && volSell != 0)
                            divide = Math.Round(volBuy / volSell, 3);

                        string sumFmtd = FormatResults ? FormatBigNumber(sum) : $"{sum}";
                        string subtractValueFmtd = subtract > 0 ? FormatBigNumber(subtract) : $"-{FormatBigNumber(Math.Abs(subtract))}";
                        string subtractFmtd = FormatResults ? subtractValueFmtd : $"{subtract}";

                        string strFormated = ResultParams.OperatorBuySell_Input == OperatorBuySell_Data.Sum ? sumFmtd :
                                             ResultParams.OperatorBuySell_Input == OperatorBuySell_Data.Subtraction ? subtractFmtd : $"{divide}";

                        Color centerColor = Math.Round(percentBuy) > Math.Round(percentSell) ? BuyColor : SellColor;

                        Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_BuySell_Result", $"\n{strFormated}", x1_Start, y1_lowest, centerColor);
                        Center.HorizontalAlignment = HorizontalAlignment.Center;
                        Center.FontSize = FontSizeResults - 1;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                        {
                            Right.Time = xBar;
                            Left.Time = xBar;
                            Center.Time = xBar;
                        }

                        // Intraday Right Profile
                        if (isIntraday) {
                            DateTime dateOffset = TimeBasedOffset(xBar);
                            Right.Time = dateOffset;
                            Left.Time = dateOffset;
                            Center.Time = dateOffset;
                        }
                        break;
                    }
                    default: {
                        double deltaBuy = vpDelta.Values.Where(n => n > 0).Sum();
                        double deltaSell = vpDelta.Values.Where(n => n < 0).Sum();
                        double totalDelta = vpDelta.Values.Sum();

                        double percentBuy = 0;
                        double percentSell = 0;
                        try { percentBuy = (deltaBuy * 100) / (deltaBuy + Math.Abs(deltaSell)); } catch { };
                        try { percentSell = (deltaSell * 100) / (deltaBuy + Math.Abs(deltaSell)); } catch { }
                        percentBuy = Math.Round(percentBuy);
                        percentSell = Math.Round(percentSell);

                        ChartText Left, Right;
                        Right = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_BuySum", $"{percentBuy}%", x1_Start, y1_lowest, BuyColor);
                        Left = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_SellSum", $"{percentSell}%", x1_Start, y1_lowest, SellColor);
                        Left.HorizontalAlignment = HorizontalAlignment.Left; Left.FontSize = FontSizeResults;
                        Right.HorizontalAlignment = HorizontalAlignment.Right; Right.FontSize = FontSizeResults;
                        
                        ChartText Center;
                        string totalDeltaFmtd = totalDelta > 0 ? FormatBigNumber(totalDelta) : $"-{FormatBigNumber(Math.Abs(totalDelta))}";
                        string totalDeltaString = FormatResults ? totalDeltaFmtd : $"{totalDelta}";

                        Color centerColor = totalDelta > 0 ? BuyColor : SellColor;
                        Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_Result", $"\n{totalDeltaString}", x1_Start, y1_lowest, centerColor);
                        Center.HorizontalAlignment = HorizontalAlignment.Center; Center.FontSize = FontSizeResults - 1;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                        {
                            Right.Time = xBar;
                            Left.Time = xBar;
                            Center.Time = xBar;
                        }

                        // Intraday Right Profile
                        if (isIntraday && extraProfiles == ExtraProfiles.No) {
                            DateTime dateOffset = TimeBasedOffset(xBar);
                            Right.Time = dateOffset;
                            Left.Time = dateOffset;
                            Center.Time = dateOffset;
                        }

                        if (ResultParams.ShowMinMaxDelta)
                            Draw_MinMaxDelta(extraProfiles, fixedKey, y1_lowest, x1_Start, xBar, isIntraday, prefix);
                        
                        break;
                    }
                }
            }
            
            // For [Normal, Delta] only
            Dictionary<double, double> vpDict = GeneralParams.VolumeMode_Input switch
            {
                VolumeMode_Data.Normal => extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Normal,
                    ExtraProfiles.Weekly => WeeklyRank.Normal,
                    ExtraProfiles.MiniVP => MiniRank.Normal,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Normal,
                    _ => VP_VolumesRank
                },
                VolumeMode_Data.Delta => extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Delta,
                    ExtraProfiles.Weekly => WeeklyRank.Delta,
                    ExtraProfiles.MiniVP => MiniRank.Delta,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Delta,
                    _ => VP_DeltaRank
                },
                _ => new Dictionary<double, double>(),
            };
            
            if (vpDict.Count > 0) {
                // VA + POC
                Draw_VA_POC(vpDict, iStart, x1_Start, xBar, extraProfiles, isIntraday, intraDate, fixedKey);

                // HVN/LVN
                DrawVolumeNodes(vpDict, iStart, x1_Start, xBar, extraProfiles, isIntraday, intraDate, fixedKey);   
            }
            
            if (!ProfileParams.ShowOHLC || extraProfiles == ExtraProfiles.Fixed)
                return;

            DateTime OHLC_Date = TF_Bars.OpenTimes[TF_idx];

            ChartText iconOpenSession =  Chart.DrawText($"{OHLC_Date}_OHLC_Start", "▂", OHLC_Date, TF_Bars.OpenPrices[TF_idx], ColorOHLC);
            iconOpenSession.VerticalAlignment = VerticalAlignment.Center;
            iconOpenSession.HorizontalAlignment = HorizontalAlignment.Left;
            iconOpenSession.FontSize = 14;

            ChartText iconCloseSession =  Chart.DrawText($"{OHLC_Date}_OHLC_End", "▂", OHLC_Date, TF_Bars.ClosePrices[TF_idx], ColorOHLC);
            iconCloseSession.VerticalAlignment = VerticalAlignment.Center;
            iconCloseSession.HorizontalAlignment = HorizontalAlignment.Right;
            iconCloseSession.FontSize = 14;

            ChartTrendLine Session = Chart.DrawTrendLine($"{OHLC_Date}_OHLC_Body", OHLC_Date, lowest, OHLC_Date, highest, ColorOHLC);
            Session.Thickness = 3;

            void Draw_MinMaxDelta(ExtraProfiles extraProfiles, string fixedKey, double lowest, DateTime x1_Start, DateTime xBar, bool isIntraday, string prefix)
            {
                ChartText MinText, MaxText, SubText;

                double[] vpMinMax = extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.MinMaxDelta,
                    ExtraProfiles.Weekly => WeeklyRank.MinMaxDelta,
                    ExtraProfiles.MiniVP => MiniRank.MinMaxDelta,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].MinMaxDelta,
                    _ => VP_MinMaxDelta
                };

                double minDelta = Math.Round(vpMinMax[0]);
                double maxDelta = Math.Round(vpMinMax[1]);
                double subDelta = Math.Round(minDelta - maxDelta);

                string minDeltaFmtd = minDelta > 0 ? FormatBigNumber(minDelta) : $"-{FormatBigNumber(Math.Abs(minDelta))}";
                string maxDeltaFmtd = maxDelta > 0 ? FormatBigNumber(maxDelta) : $"-{FormatBigNumber(Math.Abs(maxDelta))}";
                string subDeltaFmtd = subDelta > 0 ? FormatBigNumber(subDelta) : $"-{FormatBigNumber(Math.Abs(subDelta))}";

                string minDeltaString = FormatResults ? minDeltaFmtd : $"{minDelta}";
                string maxDeltaString = FormatResults ? maxDeltaFmtd : $"{maxDelta}";
                string subDeltaString = FormatResults ? subDeltaFmtd : $"{subDelta}";

                Color subColor = subDelta > 0 ? BuyColor : SellColor;

                if (!ResultParams.ShowOnlySubtDelta)
                {
                    MinText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_MinResult", $"\n\nMin: {minDeltaString}", x1_Start, lowest, SellColor);
                    MaxText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_MaxResult", $"\n\n\nMax: {maxDeltaString}", x1_Start, lowest, BuyColor);
                    SubText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_SubResult", $"\n\n\n\nSub: {subDeltaString}", x1_Start, lowest, subColor);
                    MinText.HorizontalAlignment = HorizontalAlignment.Center;
                    MaxText.HorizontalAlignment = HorizontalAlignment.Center;
                    SubText.HorizontalAlignment = HorizontalAlignment.Center;
                    MinText.FontSize = FontSizeResults - 1;
                    MaxText.FontSize = FontSizeResults - 1;
                    SubText.FontSize = FontSizeResults - 1;

                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                    {
                        MinText.Time = xBar;
                        MaxText.Time = xBar;
                        SubText.Time = xBar;
                    }

                    // Intraday Right Profile
                    if (isIntraday && extraProfiles == ExtraProfiles.No)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        MinText.Time = dateOffset;
                        MaxText.Time = dateOffset;
                        SubText.Time = dateOffset;
                    }
                }
                else
                {
                    SubText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_SubResult", $"\n\nSub: {subDeltaString}", x1_Start, lowest, subColor);
                    SubText.HorizontalAlignment = HorizontalAlignment.Center;
                    SubText.FontSize = FontSizeResults - 1;

                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                        SubText.Time = xBar;
                    // Intraday Right Profile
                    if (isIntraday && extraProfiles == ExtraProfiles.No)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        SubText.Time = dateOffset;
                    }
                }
            }
        }

        private void VP_Bars(int index, ExtraProfiles extraVP = ExtraProfiles.No, string fixedKey = "")
        {
            DateTime startTime = Bars.OpenTimes[index];
            DateTime endTime = Bars.OpenTimes[index + 1];

            // For real-time market - VP
            // Run conditional only in the last bar of repaint loop
            if (IsLastBar && Bars.OpenTimes[index] == Bars.LastBar.OpenTime)
                endTime = Source_Bars.LastBar.OpenTime;

            int startIndex = extraVP switch
            {
                ExtraProfiles.Monthly => !IsLastBar ? PerformanceSource.lastIdx_Monthly : PerformanceSource.startIdx_Monthly,
                ExtraProfiles.Weekly => !IsLastBar ? PerformanceSource.lastIdx_Weekly : PerformanceSource.startIdx_Weekly,
                ExtraProfiles.MiniVP => !IsLastBar ? PerformanceSource.lastIdx_Mini : PerformanceSource.startIdx_Mini,
                _ => !IsLastBar ? PerformanceSource.lastIdx_MainVP : PerformanceSource.startIdx_MainVP
            };
            if (extraVP == ExtraProfiles.Fixed) {
                ChartRectangle rect = RangeObjs.rectangles.Where(x => x.Name == fixedKey).FirstOrDefault();
                DateTime start = rect.Time1 < rect.Time2 ? rect.Time1 : rect.Time2;
                startIndex = Bars.OpenTimes.GetIndexByTime(start);
            }

            int TF_idx = extraVP == ExtraProfiles.Fixed ? MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]) : index;
            List<double> whichSegment = extraVP == ExtraProfiles.Fixed ? GetRangeSegments(TF_idx, fixedKey) : Segments_VP;

            // Keep shared VOL_Bars since 1min bars
            // are quite cheap in terms of RAM, even for 1 year.
            for (int k = startIndex; k < Source_Bars.Count; ++k)
            {
                Bar volBar;
                volBar = Source_Bars[k];

                if (volBar.OpenTime < startTime || volBar.OpenTime > endTime)
                {
                    if (volBar.OpenTime > endTime) {
                        _ = extraVP switch
                        {
                            ExtraProfiles.Monthly => PerformanceSource.lastIdx_Monthly = k,
                            ExtraProfiles.Weekly => PerformanceSource.lastIdx_Weekly = k,
                            ExtraProfiles.MiniVP => PerformanceSource.lastIdx_Mini = k,
                            _ => PerformanceSource.lastIdx_MainVP = k
                        };
                        break;
                    }
                    else
                        continue;
                }

                /*
                The Volume Calculation(in Bars Volume Source) is exported, with adaptations, from the BEST VP I have see/used for MT4/MT5,
                    of Russian FXcoder's https://gitlab.com/fxcoder-mql/vp (VP 10.1), author of the famous (Volume Profile + Range v6.0)

                I tried to reproduce as close as possible from the original,
                I would say it was very good approximation in most core options, except the:
                    - "Triangular", witch I had to interpret it my way, and it turned out different, of course.
                    - "Parabolic", but the result turned out good
                */

                bool isBullish = volBar.Close >= volBar.Open;
                if (ProfileParams.Distribution_Input == Distribution_Data.OHLC || ProfileParams.Distribution_Input == Distribution_Data.OHLC_No_Avg)
                {
                    bool isAvg = ProfileParams.Distribution_Input == Distribution_Data.OHLC;
                    // ========= Tick Simulation =========
                    // Bull/Buy/Up bar
                    if (volBar.Close >= volBar.Open)
                    {
                        // Average Tick Volume
                        double avgVol = isAvg ?
                        volBar.TickVolume / (volBar.Open + volBar.High + volBar.Low + volBar.Close / 4) :
                        volBar.TickVolume;

                        for (int i = 0; i < whichSegment.Count; i++)
                        {
                            double priceKey = whichSegment[i];
                            double currentSegment = priceKey;
                            if (currentSegment <= volBar.Open && currentSegment >= volBar.Low)
                                AddVolume(priceKey, avgVol, isBullish);
                            if (currentSegment <= volBar.High && currentSegment >= volBar.Low)
                                AddVolume(priceKey, avgVol, isBullish);
                            if (currentSegment <= volBar.High && currentSegment >= volBar.Close)
                                AddVolume(priceKey, avgVol, isBullish);
                        }
                    }
                    // Bear/Sell/Down bar
                    else
                    {
                        // Average Tick Volume
                        double avgVol = isAvg ? volBar.TickVolume / (volBar.Open + volBar.High + volBar.Low + volBar.Close / 4) : volBar.TickVolume;
                        for (int i = 0; i < whichSegment.Count; i++)
                        {
                            double priceKey = whichSegment[i];
                            double currentSegment = priceKey;
                            if (currentSegment >= volBar.Open && currentSegment <= volBar.High)
                                AddVolume(priceKey, avgVol, isBullish);
                            if (currentSegment <= volBar.High && currentSegment >= volBar.Low)
                                AddVolume(priceKey, avgVol, isBullish);
                            if (currentSegment >= volBar.Low && currentSegment <= volBar.Close)
                                AddVolume(priceKey, avgVol, isBullish);
                        }
                    }
                }
                else if (ProfileParams.Distribution_Input == Distribution_Data.High || ProfileParams.Distribution_Input == Distribution_Data.Low || ProfileParams.Distribution_Input == Distribution_Data.Close)
                {
                    var selected = ProfileParams.Distribution_Input;
                    if (selected == Distribution_Data.High)
                    {
                        double prevSegment = 0;
                        for (int i = 0; i < whichSegment.Count; i++)
                        {
                            double currentSegment = whichSegment[i];
                            if (currentSegment >= volBar.High && prevSegment <= volBar.High)
                                AddVolume(currentSegment, volBar.TickVolume, isBullish);
                            prevSegment = whichSegment[i];
                        }
                    }
                    else if (selected == Distribution_Data.Low)
                    {
                        double prevSegment = 0;
                        for (int i = 0; i < whichSegment.Count; i++)
                        {
                            double currentSegment = whichSegment[i];
                            if (currentSegment >= volBar.Low && prevSegment <= volBar.Low)
                                AddVolume(currentSegment, volBar.TickVolume, isBullish);
                            prevSegment = whichSegment[i];
                        }
                    }
                    else
                    {
                        double prevSegment = 0;
                        for (int i = 0; i < whichSegment.Count; i++)
                        {
                            double currentSegment = whichSegment[i];
                            if (currentSegment >= volBar.Close && prevSegment <= volBar.Close)
                                AddVolume(currentSegment, volBar.TickVolume, isBullish);
                            prevSegment = whichSegment[i];
                        }
                    }
                }
                else if (ProfileParams.Distribution_Input == Distribution_Data.Uniform_Distribution)
                {
                    double HL = Math.Abs(volBar.High - volBar.Low);
                    double uniVol = volBar.TickVolume / HL;
                    for (int i = 0; i < whichSegment.Count; i++)
                    {
                        double currentSegment = whichSegment[i];
                        if (currentSegment >= volBar.Low && currentSegment <= volBar.High)
                            AddVolume(currentSegment, uniVol, isBullish);
                    }
                }
                else if (ProfileParams.Distribution_Input == Distribution_Data.Uniform_Presence)
                {
                    double uniP_Vol = 1;
                    for (int i = 0; i < whichSegment.Count; i++)
                    {
                        double currentSegment = whichSegment[i];
                        if (currentSegment >= volBar.Low && currentSegment <= volBar.High)
                            AddVolume(currentSegment, uniP_Vol, isBullish);
                    }
                }
                else if (ProfileParams.Distribution_Input == Distribution_Data.Parabolic_Distribution)
                {
                    double HL2 = Math.Abs(volBar.High - volBar.Low) / 2;
                    double hl2SQRT = Math.Sqrt(HL2);
                    double final = hl2SQRT / HL2;

                    double parabolicVol = volBar.TickVolume / final;

                    for (int i = 0; i < whichSegment.Count; i++)
                    {
                        double currentSegment = whichSegment[i];
                        if (currentSegment >= volBar.Low && currentSegment <= volBar.High)
                            AddVolume(currentSegment, parabolicVol, isBullish);
                    }
                }
                else if (ProfileParams.Distribution_Input == Distribution_Data.Triangular_Distribution)
                {
                    double HL = Math.Abs(volBar.High - volBar.Low);
                    double HL2 = HL / 2;
                    double HL_minus = HL - HL2;
                    // =====================================
                    double oneStep = HL2 * HL_minus / 2;
                    double secondStep = HL_minus * HL / 2;
                    double final = oneStep + secondStep;

                    double triangularVol = volBar.TickVolume / final;

                    for (int i = 0; i < whichSegment.Count; i++)
                    {
                        double currentSegment = whichSegment[i];
                        if (currentSegment >= volBar.Low && currentSegment <= volBar.High)
                            AddVolume(currentSegment, triangularVol, isBullish);
                    }
                }
            }

            void AddVolume(double priceKey, double vol, bool isBullish)
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
                    UpdateExtraProfiles(extraRank, priceKey, vol, isBullish);
                    return;
                }

                if (!VP_VolumesRank.ContainsKey(priceKey))
                    VP_VolumesRank.Add(priceKey, vol);
                else
                    VP_VolumesRank[priceKey] += vol;

                bool condition = GeneralParams.VolumeMode_Input != VolumeMode_Data.Normal;
                if (condition)
                    Add_BuySell(priceKey, vol, isBullish);
            }
            void Add_BuySell(double priceKey, double vol, bool isBullish)
            {
                if (isBullish)
                {
                    if (!VP_VolumesRank_Up.ContainsKey(priceKey))
                        VP_VolumesRank_Up.Add(priceKey, vol);
                    else
                        VP_VolumesRank_Up[priceKey] += vol;
                }
                else
                {
                    if (!VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Down.Add(priceKey, vol);
                    else
                        VP_VolumesRank_Down[priceKey] += vol;
                }

                // Subtract Profile - Plain Delta
                if (!VP_VolumesRank_Subt.ContainsKey(priceKey))
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt.Add(priceKey, (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]));
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt.Add(priceKey, (VP_VolumesRank_Up[priceKey]));
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt.Add(priceKey, (-VP_VolumesRank_Down[priceKey]));
                    else
                        VP_VolumesRank_Subt.Add(priceKey, 0);
                }
                else
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt[priceKey] = (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]);
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt[priceKey] = (VP_VolumesRank_Up[priceKey]);
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt[priceKey] = (-VP_VolumesRank_Down[priceKey]);
                }

                if (GeneralParams.VolumeMode_Input != VolumeMode_Data.Delta)
                    return;
                    
                double prevDelta = VP_DeltaRank.Values.Sum();
                if (!VP_DeltaRank.ContainsKey(priceKey))
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank.Add(priceKey, (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]));
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank.Add(priceKey, (VP_VolumesRank_Up[priceKey]));
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank.Add(priceKey, (-VP_VolumesRank_Down[priceKey]));
                    else
                        VP_DeltaRank.Add(priceKey, 0);
                }
                else
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank[priceKey] += (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]);
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank[priceKey] += (VP_VolumesRank_Up[priceKey]);
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank[priceKey] += (-VP_VolumesRank_Down[priceKey]);

                }

                double currentDelta = VP_DeltaRank.Values.Sum();
                if (prevDelta > currentDelta)
                    VP_MinMaxDelta[0] = prevDelta; // Min
                if (prevDelta < currentDelta)
                    VP_MinMaxDelta[1] = prevDelta; // Max before final delta
            }

            void UpdateExtraProfiles(VolumeRankType volRank, double priceKey, double vol, bool isBullish) {
                if (!volRank.Normal.ContainsKey(priceKey))
                    volRank.Normal.Add(priceKey, vol);
                else
                    volRank.Normal[priceKey] += vol;

                bool condition = GeneralParams.VolumeMode_Input != VolumeMode_Data.Normal;
                if (condition)
                    Add_BuySell_Extra(volRank, priceKey, vol, isBullish);
            }

            void Add_BuySell_Extra(VolumeRankType volRank, double priceKey, double vol, bool isBullish)
            {
                if (isBullish)
                {
                    if (!volRank.Up.ContainsKey(priceKey))
                        volRank.Up.Add(priceKey, vol);
                    else
                        volRank.Up[priceKey] += vol;
                }
                else
                {
                    if (!volRank.Down.ContainsKey(priceKey))
                        volRank.Down.Add(priceKey, vol);
                    else
                        volRank.Down[priceKey] += vol;
                }

                if (GeneralParams.VolumeMode_Input != VolumeMode_Data.Delta)
                    return;

                double prevDelta = volRank.Delta.Values.Sum();
                if (!volRank.Delta.ContainsKey(priceKey))
                {
                    if (volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta.Add(priceKey, (volRank.Up[priceKey] - volRank.Down[priceKey]));
                    else if (volRank.Up.ContainsKey(priceKey) && !volRank.Down.ContainsKey(priceKey))
                        volRank.Delta.Add(priceKey, (volRank.Up[priceKey]));
                    else if (!volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta.Add(priceKey, (-volRank.Down[priceKey]));
                    else
                        volRank.Delta.Add(priceKey, 0);
                }
                else
                {
                    if (volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta[priceKey] += (volRank.Up[priceKey] - volRank.Down[priceKey]);
                    else if (volRank.Up.ContainsKey(priceKey) && !volRank.Down.ContainsKey(priceKey))
                        volRank.Delta[priceKey] += (volRank.Up[priceKey]);
                    else if (!volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta[priceKey] += (-volRank.Down[priceKey]);

                }

                double currentDelta = volRank.Delta.Values.Sum();
                if (prevDelta > currentDelta)
                    volRank.MinMaxDelta[0] = prevDelta; // Min
                if (prevDelta < currentDelta)
                    volRank.MinMaxDelta[1] = prevDelta; // Max before final delta
            }
        }

    }
}
