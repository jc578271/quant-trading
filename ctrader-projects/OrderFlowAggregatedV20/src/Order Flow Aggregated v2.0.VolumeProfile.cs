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
        private void VolumeProfile(int iStart, int index, ExtraProfiles extraProfiles = ExtraProfiles.No, bool isLoop = false, bool drawOnly = false, string fixedKey = "", double fixedLowest = 0)
        {
            // Weekly/Monthly on Buy_Sell is a waste of time
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell && (extraProfiles == ExtraProfiles.Weekly || extraProfiles == ExtraProfiles.Monthly))
               return;
               
            // ==== VP ====
            if (!drawOnly)
                VP_Tick(index, true, extraProfiles, fixedKey);
                
            // ==== Drawing ====
            if (Segments_VP.Count == 0 || isLoop)
                return;

            // Results or Fixed Range
            Bars TF_Bars = extraProfiles switch {
                ExtraProfiles.MiniVP => MiniVPs_Bars,
                ExtraProfiles.Weekly => WeeklyBars,
                ExtraProfiles.Monthly => MonthlyBars,
                _ => MiscParams.ODFInterval_Input == ODFInterval_Data.Daily ? DailyBars : WeeklyBars
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
            bool isRightSide = ProfileParams.HistogramSide_Input == HistSide_Data.Right;

            // Profile Selection
            IDictionary<double, double> vpNormal = new Dictionary<double, double>();
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

            IDictionary<double, double> vpBuy = new Dictionary<double, double>();
            IDictionary<double, double> vpSell = new Dictionary<double, double>();
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

            IDictionary<double, double> vpDelta = new Dictionary<double, double>();
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
            int segmentIdx = extraProfiles == ExtraProfiles.Fixed ? GetSegmentIndex(index) : index;
            List<double> whichSegment = extraProfiles == ExtraProfiles.Fixed ? segmentsDict[segmentIdx] : Segments_VP;

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
                double y1_text = priceKey;

                void DrawRectangle_Normal(double currentVolume, double maxVolume, bool intradayProfile = false)
                {
                    double proportion = currentVolume * proportion_VP;
                    double dynLength = proportion / maxVolume;

                    DateTime x2 = x1_Start.AddMilliseconds(dynLength);

                    Color histogramColor = extraProfiles switch
                    {
                        ExtraProfiles.Monthly => MonthlyColor,
                        ExtraProfiles.Weekly => WeeklyColor,
                        _ => VolumeColor,
                    };

                    ChartRectangle volHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Normal", x1_Start, lowerSegmentY1, x2, upperSegmentY2, histogramColor);

                    if (ProfileParams.FillHist_VP)
                        volHist.IsFilled = true;

                    if (isRightSide)
                    {
                        volHist.Time1 = xBar;
                        volHist.Time2 = xBar.AddMilliseconds(-dynLength);
                    }

                    if (ProfileParams.ShowHistoricalNumbers) {
                        double volumeNumber = currentVolume;
                        string volumeNumberFmtd = FormatNumbers ? FormatBigNumber(volumeNumber) : $"{volumeNumber}";

                        ChartText Center = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Normal", volumeNumberFmtd, isRightSide ? xBar : x1_Start, y1_text, RtnbFixedColor);
                        Center.HorizontalAlignment = isRightSide ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                        Center.FontSize = FontSizeNumbers;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                            Center.Time = xBar;
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

                        if (ProfileParams.ShowIntradayNumbers) {
                            double volumeNumber = currentVolume;
                            string volumeNumberFmtd = FormatNumbers ? FormatBigNumber(volumeNumber) : $"{volumeNumber}";

                            ChartText Center = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Normal",
                                volumeNumberFmtd, volHist.Time1, y1_text, RtnbFixedColor);
                            Center.FontSize = FontSizeResults;

                            Center.HorizontalAlignment = HorizontalAlignment.Left;
                            if (extraProfiles == ExtraProfiles.Weekly) {
                                if (!ProfileParams.EnableMonthlyProfile && ProfileParams.FillIntradaySpace)
                                    Center.HorizontalAlignment = HorizontalAlignment.Right;
                            }
                            if (extraProfiles == ExtraProfiles.Monthly) {
                                if (ProfileParams.FillIntradaySpace)
                                    Center.HorizontalAlignment = HorizontalAlignment.Right;
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

                    if (ProfileParams.ShowHistoricalNumbers) {
                        double buyNumber = currentBuy;
                        string buyNumberFmtd = FormatNumbers ? FormatBigNumber(buyNumber) : $"{buyNumber}";
                        double sellNumber = currentSell;
                        string sellNumberFmtd = FormatNumbers ? FormatBigNumber(sellNumber) : $"{sellNumber}";

                        ChartText Left, Right;
                        Left = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Sell", sellNumberFmtd, x1_Start, y1_text, RtnbFixedColor);
                        Right = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Buy", buyNumberFmtd, x1_Start, y1_text, RtnbFixedColor);

                        Left.HorizontalAlignment = HorizontalAlignment.Left;
                        Right.HorizontalAlignment = HorizontalAlignment.Right;

                        Left.FontSize = FontSizeNumbers;
                        Right.FontSize = FontSizeNumbers;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right) {
                            Left.Time = xBar;
                            Right.Time = xBar;
                        }
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

                        if (ProfileParams.ShowIntradayNumbers) {
                            double volumeNumber = VP_VolumesRank_Subt[priceKey];
                            string volumeNumberFmtd = volumeNumber > 0 ? FormatBigNumber(volumeNumber) : $"-{FormatBigNumber(Math.Abs(volumeNumber))}";

                            ChartText Center;
                            Center = Chart.DrawText($"{iStart}_{i}_VP_Number_Subt", volumeNumberFmtd, x1, y1_text, RtnbFixedColor);
                            Center.HorizontalAlignment = HorizontalAlignment.Left;
                            Center.FontSize = FontSizeResults;
                        }

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

                        if (ProfileParams.ShowIntradayNumbers) {
                            double buyNumber = currentBuy;
                            string buyNumberFmtd = FormatNumbers ? FormatBigNumber(buyNumber) : $"{buyNumber}";
                            double sellNumber = currentSell;
                            string sellNumberFmtd = FormatNumbers ? FormatBigNumber(sellNumber) : $"{sellNumber}";

                            ChartText Left, Right;
                            Left = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Sell", sellNumberFmtd, dateOffset, y1_text, RtnbFixedColor);
                            Right = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Buy", buyNumberFmtd, dateOffset, y1_text, RtnbFixedColor);

                            Left.HorizontalAlignment = HorizontalAlignment.Left;
                            Right.HorizontalAlignment = HorizontalAlignment.Right;

                            Left.FontSize = FontSizeResults;
                            Right.FontSize = FontSizeResults;
                        }
                    }
                }

                void DrawRectangle_Delta(double currentDelta, double positiveDeltaMax, IEnumerable<double> negativeDeltaList, bool intradayProfile = false)
                {
                    double negativeDeltaMax = negativeDeltaList.Any() ? Math.Abs(negativeDeltaList.Min()) : 0;

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

                    if (ProfileParams.ShowHistoricalNumbers) {
                        double deltaNumber = currentDelta;
                        string deltaNumberFmtd = deltaNumber > 0 ? FormatBigNumber(deltaNumber) : $"-{FormatBigNumber(Math.Abs(deltaNumber))}";
                        string deltaString = FormatNumbers ? deltaNumberFmtd : $"{deltaNumber}";

                        ChartText Center = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Delta", deltaString, x1_Start, y1_text, RtnbFixedColor);
                        Center.HorizontalAlignment = isRightSide ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                        Center.FontSize = FontSizeNumbers;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                            Center.Time = xBar;
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

                        if (ProfileParams.ShowIntradayNumbers) {
                            double deltaNumber = currentDelta;
                            string deltaNumberFmtd = deltaNumber > 0 ? FormatBigNumber(deltaNumber) : $"-{FormatBigNumber(Math.Abs(deltaNumber))}";
                            string deltaString = FormatNumbers ? deltaNumberFmtd : $"{deltaNumber}";

                            ChartText Center = Chart.DrawText($"{prefix}_{i}_VP_{extraProfiles}_Number_Delta", deltaString, deltaHist.Time1, y1_text, RtnbFixedColor);
                            Center.FontSize = FontSizeResults;

                            Center.HorizontalAlignment = HorizontalAlignment.Left;
                            if (extraProfiles == ExtraProfiles.Weekly) {
                                if (!ProfileParams.EnableMonthlyProfile && ProfileParams.FillIntradaySpace)
                                    Center.HorizontalAlignment = HorizontalAlignment.Right;
                            }
                            if (extraProfiles == ExtraProfiles.Monthly) {
                                if (ProfileParams.FillIntradaySpace)
                                    Center.HorizontalAlignment = HorizontalAlignment.Right;
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

            double _lowest = TF_Bars.LowPrices[TF_idx];
            if (double.IsNaN(_lowest)) // Mini VPs avoid crash after recalculating
                _lowest = TF_Bars.LowPrices.LastValue;
            double y1_lowest = extraProfiles == ExtraProfiles.Fixed ? fixedLowest : _lowest;

            if (extraProfiles == ExtraProfiles.MiniVP && ProfileParams.ShowMiniResults ||
                extraProfiles != ExtraProfiles.MiniVP && ResultParams.ShowResults)
            {
                switch (GeneralParams.VolumeMode_Input)
                {
                    case VolumeMode_Data.Normal:
                    {
                        double sum = Math.Round(vpNormal.Values.Sum());
                        string strValue = FormatResults ? FormatBigNumber(sum) : $"{sum}";

                        ChartText Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Normal_Result", $"\n{strValue}", x1_Start, y1_lowest, VolumeColor);
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
            IDictionary<double, double> vpDict = GeneralParams.VolumeMode_Input switch
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
                // HVN/LVN
                DrawVolumeNodes(vpDict, iStart, x1_Start, xBar, extraProfiles, isIntraday, intraDate, fixedKey);
            }

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

        // *********** MWM PROFILES ***********
    }
}
