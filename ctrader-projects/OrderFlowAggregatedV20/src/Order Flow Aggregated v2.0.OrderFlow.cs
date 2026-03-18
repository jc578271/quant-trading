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
        private void OrderFlow(int iStart)
        {
            // ==== Highest and Lowest ====
            double highest = Bars.HighPrices[iStart];
            double lowest = Bars.LowPrices[iStart];
            double open = Bars.OpenPrices[iStart];

            if (isRenkoChart && ShowWicks)
            {
                bool isUp = Bars.ClosePrices[iStart] > Bars.OpenPrices[iStart];
                DateTime currentOpenTime = Bars.OpenTimes[iStart];
                DateTime nextOpenTime = Bars.OpenTimes[iStart + 1];

                double[] wicks = GetWicks(currentOpenTime, nextOpenTime);

                if (IsLastBar && !BooleanUtils.isPriceBased_NewBar)
                {
                    lowest = wicks[0];
                    highest = wicks[1];
                    open = Bars.ClosePrices[iStart - 1];
                }
                else
                {
                    if (isUp)
                        lowest = wicks[0];
                    else
                        highest = wicks[1];
                }
            }

            // ==== Segments ====
            List<double> barSegments = new();

            lowest -= rowHeight;
            highest += rowHeight;

            for (int i = 0; i < Segments_VP.Count; i++)
            {
                double row = Segments_VP[i];
                if (lowest <= row)
                    barSegments.Add(row);
                if (highest < row)
                    break;
            }
            Segments_Bar = barSegments.OrderBy(x => x).ToList();

            // Lock features/design, if applicable.
            LockODFTemplate();
            
            // ==== Volume on Tick ====
            VP_Tick(iStart);

            // Do not populate series if the current bar is empty (like bars before TickObjs.firstTickTime)
            if (Segments_Bar.Count == 0 || !VolumesRank.Any())
                return;

            // Series for [Strength, Tick Spike, Bubbles Chart] filters
            PopulateSeries(iStart);
            
            // Tick Spike => strength of each row
            Dictionary<double, double> spikeProfile = GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta ? CreateSpikeProfile(iStart) : new();

            // ==== Drawing ====
            DateTime xBar = Bars.OpenTimes[iStart];

            double maxLength_LeftSide = xBar.Subtract(Bars[iStart - 1].OpenTime).TotalMilliseconds;
            double proportion_LeftSide = maxLength_LeftSide / 3;

            double maxLength_RightSide = (!IsLastBar || BooleanUtils.isPriceBased_NewBar) ?
                                         Bars[iStart + 1].OpenTime.Subtract(xBar).TotalMilliseconds :
                                         maxLength_LeftSide;
            double proportion_RightSide = maxLength_RightSide / 3;

            bool gapWeekday = xBar.DayOfWeek == DayOfWeek.Sunday && Bars.OpenTimes[iStart - 1].DayOfWeek == DayOfWeek.Friday;
            bool priceGap = xBar == Bars[iStart - 1].OpenTime || Bars[iStart - 2].OpenTime == Bars[iStart - 1].OpenTime;
            bool isBullish = Bars.ClosePrices[iStart] > Bars.OpenPrices[iStart];
            bool avoidStretching = IsLastBar && !BooleanUtils.isPriceBased_NewBar; // For real-time => Avoid stretching the histograms away ad infinitum

            // (micro)Optimization for all modes
            int maxValue = GeneralParams.VolumeMode_Input switch {
                VolumeMode_Data.Normal => VolumesRank.Any() ? VolumesRank.Values.Max() : 0,
                VolumeMode_Data.Delta => DeltaRank.Any() ? DeltaRank.Values.Max() : 0,
                _ => 0
            };

            int buyMax = 0;
            int sellMax = 0;
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell) {
                buyMax = VolumesRank_Up.Any() ? VolumesRank_Up.Values.Max() : 0;
                sellMax = VolumesRank_Down.Any() ? VolumesRank_Down.Values.Max() : 0;
            }

            IEnumerable<int> negativeList = new List<int>();
            double finalMaxForIntensity = 1;
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta)
            {
                negativeList = DeltaRank.Values.Where(n => n < 0);
                
                int posMax = DeltaRank.Values.Any(v => v > 0) ? DeltaRank.Values.Max() : 0;
                int negMax = negativeList.Any() ? Math.Abs(negativeList.Min()) : 0;
                double currentBarMaxDelta = Math.Max(posMax, negMax);
                
                GeneralParams.BarMaxDeltaCache[iStart] = currentBarMaxDelta;
                finalMaxForIntensity = currentBarMaxDelta;

                if (GeneralParams.IntensityMode_Input != IntensityMode_Data.Per_Bar)
                {
                    if (GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_N_Days) {
                        DateTime cutoffTime = Bars.OpenTimes[iStart].AddDays(-GeneralParams.IntensityNDays_Input);
                        foreach (var kvp in GeneralParams.BarMaxDeltaCache) {
                            if (Bars.OpenTimes[kvp.Key] >= cutoffTime && kvp.Value > finalMaxForIntensity) {
                                finalMaxForIntensity = kvp.Value;
                            }
                        }
                    } else if (GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_Lookback) {
                        foreach (var val in GeneralParams.BarMaxDeltaCache.Values) {
                            if (val > finalMaxForIntensity) {
                                finalMaxForIntensity = val;
                            }
                        }
                    }
                }
            }
            else if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Normal)
            {
                double currentBarMax = maxValue;
                GeneralParams.BarMaxVolumeCache[iStart] = currentBarMax;
                finalMaxForIntensity = currentBarMax;

                if (GeneralParams.IntensityMode_Input != IntensityMode_Data.Per_Bar)
                {
                    if (GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_N_Days) {
                        DateTime cutoffTime = Bars.OpenTimes[iStart].AddDays(-GeneralParams.IntensityNDays_Input);
                        foreach (var kvp in GeneralParams.BarMaxVolumeCache) {
                            if (Bars.OpenTimes[kvp.Key] >= cutoffTime && kvp.Value > finalMaxForIntensity) {
                                finalMaxForIntensity = kvp.Value;
                            }
                        }
                    } else if (GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_Lookback) {
                        foreach (var val in GeneralParams.BarMaxVolumeCache.Values) {
                            if (val > finalMaxForIntensity) {
                                finalMaxForIntensity = val;
                            }
                        }
                    }
                }
            }
            else if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell)
            {
                double currentBarMax = Math.Max(buyMax, sellMax);
                GeneralParams.BarMaxBuySellCache[iStart] = currentBarMax;
                finalMaxForIntensity = currentBarMax;

                if (GeneralParams.IntensityMode_Input != IntensityMode_Data.Per_Bar)
                {
                    if (GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_N_Days) {
                        DateTime cutoffTime = Bars.OpenTimes[iStart].AddDays(-GeneralParams.IntensityNDays_Input);
                        foreach (var kvp in GeneralParams.BarMaxBuySellCache) {
                            if (Bars.OpenTimes[kvp.Key] >= cutoffTime && kvp.Value > finalMaxForIntensity) {
                                finalMaxForIntensity = kvp.Value;
                            }
                        }
                    } else if (GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_Lookback) {
                        foreach (var val in GeneralParams.BarMaxBuySellCache.Values) {
                            if (val > finalMaxForIntensity) {
                                finalMaxForIntensity = val;
                            }
                        }
                    }
                }
            }

            // Manual Refactoring.
            // LLM allucinates.
            // ODF_Agg => Refactored again
            double loopPrevSegment = 0;
            for (int i = 0; i < Segments_Bar.Count; i++)
            {
                if (loopPrevSegment == 0)
                    loopPrevSegment = Segments_Bar[i];

                double priceKey = Segments_Bar[i];
                if (!VolumesRank.ContainsKey(priceKey))
                    continue;

                // ====  HISTOGRAMs + Texts  ====
                /*
                  Indeed, the value of X-Axis is simply a rule of three,
                  where the maximum value of the respective side (Volume/Buy/Sell) will be the maxLength (in Milliseconds),
                  from there the math adjusts the histograms.
                      MaxValue    maxLength(ms)
                      x             ?(ms)
                  The values 1.50 and 3 are the manually set values like the size of the Bar body in any timeframe
                  (Candle, Ticks, Renko, Range)

                  NEW IN ODF_AGG => To avoid unnecessary workarounds for others timeframes/charts,
                  as well as improve readability, instead of just one maxLength,
                  that works great in Candles Charts(timebased) only, now we have:
                  - maxLengthLeft / maxLengthRight
                  - Like in Divided View, for Profile view we should add one more step:
                      - Calculate the Middle-to-Left & Middle-to-Right
                      - Set Left side as starting point (this)
                      - Add Left-to-Middle proportion
                      - Add Middle-to-Right proportion to current (BarLeft + Left = Middle) total milliseconds.
                */

                double lowerSegmentY1 = loopPrevSegment;
                double upperSegmentY2 = Segments_Bar[i];

                void DrawRectangle_Normal(int currentVolume, int maxVolume, bool profileInMiddle = false)
                {
                    double proportion_ToMiddle = currentVolume * proportion_LeftSide;
                    double dynLength_ToMiddle = proportion_ToMiddle / maxVolume;

                    double proportion_ToRight = currentVolume * proportion_RightSide;
                    double dynLength_ToRight = proportion_ToRight / maxVolume;

                    bool dividedCondition = GeneralParams.VolumeView_Input == VolumeView_Data.Profile && profileInMiddle; // Profile View - Half Proportion

                    DateTime x1 = dividedCondition ? xBar : xBar.AddMilliseconds(-proportion_LeftSide);
                    DateTime x2;
                    if (dividedCondition)
                        x2 = x1.AddMilliseconds(dynLength_ToRight);
                    else
                        x2 = x1.AddMilliseconds(dynLength_ToMiddle).AddMilliseconds(dynLength_ToRight);

                    if (isPriceBased_Chart)
                    {
                        if (avoidStretching)
                            x2 = x1.AddMilliseconds(dynLength_ToMiddle).AddMilliseconds(0);

                        if (priceGap)
                        {
                            x1 = xBar;
                            x2 = x1.AddMilliseconds(dynLength_ToRight);
                        }
                    }

                    Color colorHist = currentVolume != maxVolume ? VolumeColor : VolumeLargeColor;

                    if (GeneralParams.ColoringIntensity && finalMaxForIntensity > 0)
                    {
                        double ratio = (double)currentVolume / finalMaxForIntensity;
                        if (ratio > 1) ratio = 1.0;
                        double minAlpha = 30;
                        byte newAlpha = (byte)(minAlpha + (255 - minAlpha) * ratio);
                        colorHist = Color.FromArgb(newAlpha, colorHist.R, colorHist.G, colorHist.B);
                    }

                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = iStart,
                        Type = DrawType.Rectangle,
                        Id = $"{iStart}_{i}_Normal",
                        X1 = x1,
                        Y1 = lowerSegmentY1,
                        X2 = x2,
                        Y2 = upperSegmentY2,
                        Color = colorHist
                    });
                }

                void DrawRectangle_BuySell(
                    int currentBuy, int maxBuy,
                    int currentSell, int maxSell)
                {

                    // Right Side - Divided View
                    double proportionBuy_RightSide = currentBuy * proportion_RightSide;
                    double dynLengthBuy_RightSide = proportionBuy_RightSide / maxBuy;

                    // Left Side - Divided View
                    double proportionSell_LeftSide = currentSell * proportion_LeftSide;
                    double dynLengthSell_LeftSide = proportionSell_LeftSide / maxSell;

                    // Profile View - Complete Proportion
                    int profileMaxVolume = maxBuy > maxSell ? maxBuy : maxSell;

                    double proportionBuy_ToMiddle = currentBuy * proportion_LeftSide;
                    double dynLengthBuy_ToMiddle = proportionBuy_ToMiddle / profileMaxVolume;

                    double proportionSell_ToMiddle = currentSell * proportion_LeftSide;
                    double dynLengthSell_ToMiddle = proportionSell_ToMiddle / profileMaxVolume;

                    double proportionSell_RightSide = currentSell * proportion_RightSide;
                    double dynLengthSell_RightSide = proportionSell_RightSide / profileMaxVolume;
                    // ========

                    bool dividedCondition = GeneralParams.VolumeView_Input == VolumeView_Data.Divided;
                    DateTime x1 = dividedCondition || gapWeekday ? xBar : xBar.AddMilliseconds(-proportion_LeftSide);

                    DateTime x2_Buy = x1.AddMilliseconds(dividedCondition ? dynLengthBuy_RightSide : dynLengthBuy_ToMiddle);
                    DateTime x2_Sell;
                    if (dividedCondition || gapWeekday)
                        x2_Sell = x1.AddMilliseconds(-dynLengthSell_LeftSide);
                    else
                        x2_Sell = x1.AddMilliseconds(dynLengthSell_ToMiddle).AddMilliseconds(dynLengthSell_RightSide);

                    if (isPriceBased_Chart)
                    {
                        if (avoidStretching)
                        {
                            dynLengthSell_RightSide = 0;
                            dynLengthBuy_ToMiddle /= 2;

                            x2_Buy = x1.AddMilliseconds(dynLengthBuy_ToMiddle);
                            x2_Sell = x1.AddMilliseconds(dynLengthSell_ToMiddle).AddMilliseconds(dynLengthSell_RightSide);
                        }

                        if (priceGap)
                        {
                            proportionBuy_ToMiddle = currentBuy * (proportion_RightSide / 2);
                            dynLengthBuy_ToMiddle = proportionBuy_ToMiddle / profileMaxVolume;

                            proportionSell_RightSide = currentSell * proportion_RightSide;
                            dynLengthSell_RightSide = proportionSell_RightSide / profileMaxVolume;

                            x2_Buy = x1.AddMilliseconds(dynLengthBuy_ToMiddle);
                            x2_Sell = x1.AddMilliseconds(dynLengthSell_RightSide);
                        }
                    }

                    Color buyDividedColor = currentBuy != maxBuy ? BuyColor : BuyLargeColor;
                    Color sellDividedColor = currentSell != maxSell ? SellColor : SellLargeColor;
                    if (GeneralParams.ColoringOnlyLarguest)
                    {
                        buyDividedColor = maxBuy > maxSell && currentBuy == maxBuy ?
                            BuyLargeColor : BuyColor;
                        sellDividedColor = maxSell > maxBuy && currentSell == maxSell ?
                            SellLargeColor : SellColor;
                    }

                    Color buyColor = dividedCondition ? buyDividedColor : BuyColor;
                    Color sellColor = dividedCondition ? sellDividedColor : SellColor;

                    if (GeneralParams.ColoringIntensity && finalMaxForIntensity > 0)
                    {
                        double buyRatio = (double)currentBuy / finalMaxForIntensity;
                        if (buyRatio > 1) buyRatio = 1.0;
                        double sellRatio = (double)currentSell / finalMaxForIntensity;
                        if (sellRatio > 1) sellRatio = 1.0;
                        double minAlpha = 30;
                        byte newBuyAlpha = (byte)(minAlpha + (255 - minAlpha) * buyRatio);
                        byte newSellAlpha = (byte)(minAlpha + (255 - minAlpha) * sellRatio);
                        buyColor = Color.FromArgb(newBuyAlpha, buyColor.R, buyColor.G, buyColor.B);
                        sellColor = Color.FromArgb(newSellAlpha, sellColor.R, sellColor.G, sellColor.B);
                    }

                    // Sell histogram first, Buy histogram to override it.
                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = iStart,
                        Type = DrawType.Rectangle,
                        Id = $"{iStart}_{i}_Sell",
                        X1 = x1,
                        Y1 = lowerSegmentY1,
                        X2 = x2_Sell,
                        Y2 = upperSegmentY2,
                        Color = sellColor
                    });

                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = iStart,
                        Type = DrawType.Rectangle,
                        Id = $"{iStart}_{i}_Buy",
                        X1 = x1,
                        Y1 = lowerSegmentY1,
                        X2 = x2_Buy,
                        Y2 = upperSegmentY2,
                        Color = buyColor
                    });
                }

                void DrawRectangle_Delta(int currentDelta, int positiveDeltaMax, IEnumerable<int> negativeDeltaList, double finalMaxForIntensity)
                {
                    int negativeDeltaMax = negativeDeltaList.Any() ? Math.Abs(negativeDeltaList.Min()) : 0;

                    // Divided View
                    double dynLengthDelta_Divided = 0;
                    if (currentDelta > 0)
                    {
                        double proportionDelta_Positive = currentDelta * proportion_RightSide;
                        dynLengthDelta_Divided = proportionDelta_Positive / positiveDeltaMax;
                    }
                    else
                    {
                        double proportionDelta_Negative = Math.Abs(currentDelta) * proportion_LeftSide;
                        dynLengthDelta_Divided = proportionDelta_Negative / negativeDeltaMax;
                        dynLengthDelta_Divided = -dynLengthDelta_Divided;
                    }

                    // Profile View - Complete Proportion
                    int deltaMax = positiveDeltaMax > Math.Abs(negativeDeltaMax) ? positiveDeltaMax : Math.Abs(negativeDeltaMax);

                    double proportion_ToMiddle = Math.Abs(currentDelta) * proportion_LeftSide;
                    double dynLength_ToMiddle = proportion_ToMiddle / deltaMax;

                    double proportion_ToRight = Math.Abs(currentDelta) * proportion_RightSide;
                    double dynLength_ToRight = proportion_ToRight / deltaMax;
                    // ========

                    bool dividedCondition = GeneralParams.VolumeView_Input == VolumeView_Data.Divided;
                    DateTime x1 = dividedCondition || gapWeekday ? xBar : xBar.AddMilliseconds(-proportion_LeftSide);

                    DateTime x2;
                    if (dividedCondition || gapWeekday)
                        x2 = x1.AddMilliseconds(dynLengthDelta_Divided);
                    else
                        x2 = x1.AddMilliseconds(dynLength_ToMiddle).AddMilliseconds(dynLength_ToRight);

                    if (isPriceBased_Chart && GeneralParams.VolumeView_Input == VolumeView_Data.Profile)
                    {
                        if (avoidStretching)
                            x2 = x1.AddMilliseconds(dynLength_ToMiddle).AddMilliseconds(0);

                        if (priceGap)
                        {
                            x1 = xBar;
                            x2 = x1.AddMilliseconds(dynLengthDelta_Divided);
                        }
                    }

                    Color buyDividedColor = currentDelta != positiveDeltaMax ? BuyColor : BuyLargeColor;
                    Color sellDividedColor = currentDelta != negativeDeltaMax ? SellColor : SellLargeColor;
                    if (GeneralParams.ColoringOnlyLarguest)
                    {
                        buyDividedColor = positiveDeltaMax > Math.Abs(negativeDeltaMax) && currentDelta == positiveDeltaMax ?
                            BuyLargeColor : BuyColor;
                        sellDividedColor = Math.Abs(negativeDeltaMax) > positiveDeltaMax && currentDelta == negativeDeltaMax ?
                            SellLargeColor : SellColor;
                    }

                    Color buyColorWithFilter = GeneralParams.VolumeView_Input == VolumeView_Data.Divided ? buyDividedColor : BuyColor;
                    Color sellColorWithFilter = GeneralParams.VolumeView_Input == VolumeView_Data.Divided ? sellDividedColor : SellColor;

                    Color colorHist = currentDelta > 0 ? buyColorWithFilter : sellColorWithFilter;

                    if (GeneralParams.ColoringIntensity && finalMaxForIntensity > 0)
                    {
                        double ratio = Math.Abs((double)currentDelta) / finalMaxForIntensity;
                        if (ratio > 1) ratio = 1.0;

                        double minAlpha = 30; // 30/255 = ~12% minimum opacity
                        byte newAlpha = (byte)(minAlpha + (255 - minAlpha) * ratio);
                        colorHist = Color.FromArgb(newAlpha, colorHist.R, colorHist.G, colorHist.B);
                    }


                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = iStart,
                        Type = DrawType.Rectangle,
                        Id = $"{iStart}_{i}_Delta",
                        X1 = x1,
                        Y1 = lowerSegmentY1,
                        X2 = x2,
                        Y2 = upperSegmentY2,
                        Color = colorHist
                    });
                }

                switch (GeneralParams.VolumeMode_Input)
                {
                    case VolumeMode_Data.Normal:
                    {
                        int normalValue = VolumesRank[priceKey];

                        if (MiscParams.ShowHist)
                            DrawRectangle_Normal(normalValue, maxValue);

                        if (MiscParams.ShowNumbers)
                        {
                            string valueFmtd = FormatNumbers ? FormatBigNumber(normalValue) : $"{normalValue}";

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_{i}_NormalNumber",
                                Text = valueFmtd,
                                X1 = Bars.OpenTimes[iStart],
                                Y1 = priceKey,
                                horizontalAlignment = HorizontalAlignment.Center,
                                verticalAlignment = VerticalAlignment.Bottom,
                                FontSize = FontSizeNumbers,
                                Color = RtnbFixedColor
                            });
                        }
                        break;
                    }
                    case VolumeMode_Data.Buy_Sell:
                    {
                        int buyValue = VolumesRank_Up[priceKey];
                        int sellValue = VolumesRank_Down[priceKey];

                        if (MiscParams.ShowHist)
                            DrawRectangle_BuySell(buyValue, buyMax, sellValue, sellMax);

                        if (MiscParams.ShowNumbers)
                        {
                            string buyValueFmt = FormatNumbers ? FormatBigNumber(buyValue) : $"{buyValue}";
                            string sellValueFmt = FormatNumbers ? FormatBigNumber(sellValue) : $"{sellValue}";

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_{i}_BuyNumber",
                                Text = buyValueFmt,
                                X1 = xBar,
                                Y1 = priceKey,
                                horizontalAlignment = HorizontalAlignment.Right,
                                verticalAlignment = VerticalAlignment.Bottom,
                                FontSize = FontSizeNumbers,
                                Color = RtnbFixedColor
                            });

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_{i}_SellNumber",
                                Text = sellValueFmt,
                                X1 = xBar,
                                Y1 = priceKey,
                                horizontalAlignment = HorizontalAlignment.Left,
                                verticalAlignment = VerticalAlignment.Bottom,
                                FontSize = FontSizeNumbers,
                                Color = RtnbFixedColor
                            });
                        }
                        break;
                    }
                    default:
                    {
                        int deltaValue = DeltaRank[priceKey];
                        if (MiscParams.ShowHist)
                            DrawRectangle_Delta(deltaValue, maxValue, negativeList, finalMaxForIntensity);

                        if (MiscParams.ShowNumbers)
                        {
                            string deltaValueFmtd = deltaValue > 0 ? FormatBigNumber(deltaValue) : $"-{FormatBigNumber(Math.Abs(deltaValue))}";
                            string deltaFmtd = FormatNumbers ? deltaValueFmtd : $"{deltaValue}";

                            HorizontalAlignment horizontalAligh;
                            if (GeneralParams.VolumeView_Input == VolumeView_Data.Divided)
                                horizontalAligh = deltaValue > 0 ? HorizontalAlignment.Right : deltaValue < 0 ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                            else
                                horizontalAligh = HorizontalAlignment.Center;

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_{i}_DeltaNumber",
                                Text = deltaFmtd,
                                X1 = xBar,
                                Y1 = priceKey,
                                horizontalAlignment = horizontalAligh,
                                verticalAlignment = VerticalAlignment.Bottom,
                                FontSize = FontSizeNumbers,
                                Color = RtnbFixedColor
                            });
                        }

                        // Tick Delta = Spike Filter
                        if (SpikeFilterParams.EnableSpikeFilter)
                        {
                            double rowStrength = spikeProfile[priceKey];

                            // Ratios
                            double lowestValue = SpikeRatioParams.Lowest_PctValue;
                            double lowValue = SpikeRatioParams.Low_PctValue;
                            double averageValue = SpikeRatioParams.Average_PctValue;
                            double highValue = SpikeRatioParams.High_PctValue;
                            double ultraValue = SpikeRatioParams.Ultra_PctValue;

                            if (SpikeRatioParams.SpikeRatio_Input == SpikeRatio_Data.Fixed) 
                            {
                                lowestValue = SpikeRatioParams.Lowest_FixedValue;
                                lowValue = SpikeRatioParams.Low_FixedValue;
                                averageValue = SpikeRatioParams.Average_FixedValue;
                                highValue = SpikeRatioParams.High_FixedValue;
                                ultraValue = SpikeRatioParams.Ultra_FixedValue;
                            }

                            Color spikeHeatColor = rowStrength < lowestValue ? SpikeLowest_Color :
                                                   rowStrength < lowValue ? SpikeLow_Color :
                                                   rowStrength < averageValue ? SpikeAverage_Color :
                                                   rowStrength < highValue ? SpikeHigh_Color :
                                                   rowStrength >= ultraValue ? SpikeUltra_Color : SpikeUltra_Color;

                            Color spikeBySideColor = deltaValue > 0 ? BuyColor : SellColor;
                            
                            // For real-time - "repaint/update" the spike price level.
                            if (IsLastBar) {
                                Chart.RemoveObject($"{iStart}_{i}_Spike");
                                if (DrawingStrategy_Input == DrawingStrategy_Data.Redraw_Fastest)
                                    PerfDrawingObjs.currentToRedraw.Clear();
                                else 
                                    PerfDrawingObjs.currentToHidden.Clear();
                            }

                            bool isSpikeAverage = rowStrength > lowValue;
                            if (isSpikeAverage || SpikeFilterParams.EnableSpikeChart)
                            {
                                double proportion_ToMiddle = 1 * proportion_LeftSide;
                                double dynLength_ToMiddle = proportion_ToMiddle / 1;

                                double proportion_ToRight = 1 * proportion_RightSide;
                                double dynLength_ToRight = proportion_ToRight / 1;

                                DateTime X1 = xBar.AddMilliseconds(-proportion_LeftSide);
                                DateTime X2 = X1.AddMilliseconds(dynLength_ToMiddle).AddMilliseconds(
                                    (avoidStretching && isPriceBased_Chart || gapWeekday) ? 0 : dynLength_ToRight
                                );

                                double Y1 = priceKey;
                                double Y2 = priceKey - rowHeight;

                                if (SpikeFilterParams.SpikeView_Input == SpikeView_Data.Bubbles || SpikeFilterParams.EnableSpikeChart)
                                {
                                    Color spikeHeat_WithOpacity = Color.FromArgb((int)(2.55 * SpikeChart_Opacity), spikeHeatColor.R, spikeHeatColor.G, spikeHeatColor.B);
                                    Color spikeBySide_WithOpacity = Color.FromArgb((int)(2.55 * SpikeChart_Opacity), spikeBySideColor.R, spikeBySideColor.G, spikeBySideColor.B);
                                    Color spikeChartColor = SpikeFilterParams.SpikeChartColoring_Input == SpikeChartColoring_Data.Heatmap ?
                                                            spikeHeat_WithOpacity : spikeBySide_WithOpacity;


                                    if (SpikeFilterParams.SpikeChartColoring_Input == SpikeChartColoring_Data.PlusMinus_Highlight_Heatmap)
                                        spikeChartColor = isSpikeAverage ? spikeHeat_WithOpacity : spikeChartColor;

                                    Color bubbleColor = !SpikeFilterParams.EnableSpikeChart ? spikeHeatColor : spikeChartColor;
                                    DrawOrCache(new DrawInfo
                                    {
                                        BarIndex = iStart,
                                        Type = DrawType.Ellipse,
                                        Id = $"{iStart}_{i}_Spike",
                                        X1 = X1,
                                        Y1 = Y1,
                                        X2 = X2,
                                        Y2 = Y2,
                                        Color = bubbleColor
                                    });
                                }
                                else
                                {
                                    DateTime positionX = GeneralParams.VolumeView_Input == VolumeView_Data.Divided ? xBar : X2;
                                    double positionY = (Y1 + Y2) / 2;
                                    ChartIcon icon = Chart.DrawIcon($"{iStart}_{i}_Spike", SpikeFilterParams.IconView_Input, positionX, positionY, spikeHeatColor);
                                    DrawOrCache(new DrawInfo
                                    {
                                        BarIndex = iStart,
                                        Type = DrawType.Icon,
                                        Id = $"{iStart}_{i}_Spike",
                                        IconType = SpikeFilterParams.IconView_Input,
                                        X1 = positionX,
                                        Y1 = positionY,
                                        Color = spikeHeatColor
                                    });
                                }

                                bool notifyLater = SpikeFilterParams.SpikeFilter_Input == SpikeFilter_Data.L1Norm ||
                                                   SpikeFilterParams.SpikeFilter_Input == SpikeFilter_Data.SoftMax_Power;

                                bool notifyBool = !notifyLater ? isSpikeAverage : (BooleanLocks.lastIsAvg && BooleanLocks.spikeNotify_NewBar);
                                
                                if (SpikeFilterParams.EnableSpikeNotification && IsLastBar && !BooleanLocks.spikeNotify && notifyBool)
                                {
                                    string symbolName = $"{Symbol.Name} ({Chart.TimeFrame.ShortName})";
                                    string popupText = $"{symbolName} => Tick Spike at {Server.Time}";

                                    switch (SpikeFilterParams.NotificationType_Input) {
                                        case NotificationType_Data.Sound:
                                            Notifications.PlaySound(SpikeFilterParams.Spike_SoundType);
                                            break;
                                        case NotificationType_Data.Popup:
                                            Notifications.ShowPopup(NOTIFY_CAPTION, popupText, PopupNotificationState.Information);
                                            break;
                                        default:
                                            Notifications.PlaySound(SpikeFilterParams.Spike_SoundType);
                                            Notifications.ShowPopup(NOTIFY_CAPTION, popupText, PopupNotificationState.Information);
                                            break;
                                    }
                                    BooleanLocks.spikeNotify = true;
                                    BooleanLocks.spikeNotify_NewBar = false;
                                }
                            }

                            // At the final loop when the bar is closed, if "isSpikeAverage", notify in the next bar.
                            // When Backtesting in Price-Based Charts, this condition doesn't seem to be triggered,
                            // Works fine in real-time market though.
                            if (isSpikeAverage) {
                                BooleanLocks.spikeNotify_NewBar = false;
                                BooleanLocks.lastIsAvg = true;
                            }
                            else
                                BooleanLocks.lastIsAvg = false;

                            if (SpikeRatioParams.ShowStrengthValue)
                            {
                                string suffix = SpikeRatioParams.SpikeRatio_Input == SpikeRatio_Data.Percentage ? "%" : "";
                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_{i}_TickStrengthValue",
                                    Text = $"   <= {rowStrength}{suffix}",
                                    X1 = xBar,
                                    Y1 = priceKey,
                                    horizontalAlignment = HorizontalAlignment.Right,
                                    verticalAlignment = VerticalAlignment.Bottom,
                                    FontSize = FontSizeNumbers,
                                    Color = RtnbFixedColor
                                });
                            }

                            // === Spike Levels ====
                            if (SpikeLevelParams.ShowSpikeLevels)
                            {
                                string spikeDictKey = $"{iStart}_{i}_SpikeLevel";

                                // For real-time - "repaint/update" the spike price level.
                                if (IsLastBar)
                                {
                                    try { Chart.RemoveObject($"{iStart}_{i}_SpikeLevelRectangle"); } catch { };
                                    if (SpikeLevels_ShowValue) {
                                        try { Chart.RemoveObject($"{iStart}_{i}_SpikeLevelText"); } catch { };
                                    }
                                    spikeRectangles.Remove(spikeDictKey);
                                }

                                // 'open' already declared.
                                double close = Bars.ClosePrices[iStart];
                                double high = (isRenkoChart && ShowWicks) ?
                                                highest : Bars.HighPrices[iStart];
                                double low = (isRenkoChart && ShowWicks) ?
                                                lowest : Bars.LowPrices[iStart];

                                // Check touches for all active rectangles
                                // Historical Data || Real-time Market
                                // This one gave more headache than Ultra Bubbles
                                if (!BooleanLocks.spikeLevels || IsLastBar)
                                {
                                    foreach (var rect in spikeRectangles.Values)
                                    {
                                        if (!rect.isActive)
                                            continue;

                                        // Avoid "touch counting" on the current bar rectangles
                                        if (rect.LastBarIndex == iStart && IsLastBar)
                                            continue;

                                        double top = Math.Max(rect.Y1, rect.Y2);
                                        double bottom = Math.Min(rect.Y1, rect.Y2);

                                        // Check OHLC one by one
                                        if (TouchesRect_Spike(open, high, low, close, top, bottom))
                                        {
                                            rect.Touches++;
                                            // Current forming bar already touched that rectangle.
                                            // So, lock it until a new LastBarIndex appear.
                                            rect.LastBarIndex = iStart;

                                            if (SpikeLevels_ShowValue)
                                                UpdateLabel_Spike(rect, Bars.OpenTimes[iStart]);

                                            if (rect.Touches >= SpikeLevelParams.MaxCount)
                                            {
                                                rect.isActive = false;

                                                // Stop extension → fix rectangle to current bar
                                                rect.Rectangle.Time2 = Bars.OpenTimes[iStart];
                                                rect.Rectangle.Color = Color.FromArgb(50, rect.Rectangle.Color);

                                                // Finalize label
                                                if (SpikeLevels_ShowValue)
                                                {
                                                    rect.Text.Text = $"{rect.Touches}";
                                                    rect.Text.Color = RtnbFixedColor;
                                                }
                                            }
                                        }
                                    }

                                    BooleanLocks.spikeLevels = true;
                                }

                                // Stretch
                                foreach (var rect in spikeRectangles.Values)
                                {
                                    if (!rect.isActive)
                                        continue;
                                    // Historical not desactivated yet;
                                    if (SpikeLevels_ShowValue)
                                        rect.Text.Time = Bars.LastBar.OpenTime;

                                    if (rect.Rectangle.Time2 == Bars.LastBar.OpenTime)
                                        continue;
                                    rect.Rectangle.Time2 = Bars.LastBar.OpenTime;
                                }

                                // Create new rectangle for each Tick Spike
                                if (isSpikeAverage)
                                {
                                    Color spikeHeat_WithOpacity = Color.FromArgb((int)(2.55 * SpikeChart_Opacity), spikeHeatColor.R, spikeHeatColor.G, spikeHeatColor.B);
                                    Color SpikeBySide_WithOpacity = Color.FromArgb((int)(2.55 * SpikeChart_Opacity), spikeBySideColor.R, spikeBySideColor.G, spikeBySideColor.B);
                                    Color spikeLevelColor = SpikeLevelParams.SpikeLevelsColoring_Input == SpikeLevelsColoring_Data.Heatmap ?
                                                            spikeHeat_WithOpacity : SpikeBySide_WithOpacity;

                                    double Y1 = priceKey;
                                    double Y2 = priceKey - rowHeight;
                                    CreateRect_Spike(Y1, Y2, iStart, i, spikeDictKey, spikeLevelColor);
                                }
                            }
                        }
                        break;
                    }
                }

                loopPrevSegment = Segments_Bar[i];
            }
            
            // Drawings that don't require each segment-price as y-axis
            // It can/should be outside SegmentsLoop for better performance.
            
            double rowHeightHalf = (rowHeight + rowHeight) / 2;
            double highestHalf = Bars.HighPrices[iStart] + rowHeightHalf;
            double lowestHalf = Bars.LowPrices[iStart] - rowHeightHalf;
            if (isRenkoChart && ShowWicks)
            {
                lowest += rowHeight;
                highest -= rowHeight;
                highestHalf = highest + rowHeightHalf;
                lowestHalf = lowest - rowHeightHalf;
            }

            // Results
            switch (GeneralParams.VolumeMode_Input)
            {
                case VolumeMode_Data.Normal:
                {
                    if (!ResultParams.ShowResults)
                        break;

                    double sumValue = Dynamic_Series[iStart];
                    string valueFmtd = FormatResults ? FormatBigNumber(sumValue) : $"{sumValue}";
                    Color resultColor = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : VolumeColor;

                    if (ResultParams.EnableLargeFilter)
                    {
                        // ====== Strength Filter ======
                        double filterValue = 0;
                        if (UseCustomMAs)
                            filterValue = CustomMAs(sumValue, iStart, ResultParams.MAperiod, CustomMAType.Large);
                        else
                            filterValue = MADynamic_LargeFilter.Result[iStart];

                        double volumeStrength = sumValue / filterValue;
                        Color filterColor = volumeStrength >= ResultParams.LargeRatio ? ColorLargeResult : resultColor;

                        resultColor = filterColor;
                        if (LargeFilter_ColoringBars && filterColor == ColorLargeResult)
                            Chart.SetBarFillColor(iStart, ColorLargeResult);
                        else
                            Chart.SetBarFillColor(iStart, isBullish ? Chart.ColorSettings.BullFillColor : Chart.ColorSettings.BearFillColor);
                    }

                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = iStart,
                        Type = DrawType.Text,
                        Id = $"{iStart}_NormalSum",
                        Text = $"\n{valueFmtd}",
                        X1 = xBar,
                        Y1 = lowestHalf,
                        horizontalAlignment = HorizontalAlignment.Center,
                        FontSize = FontSizeResults,
                        Color = resultColor
                    });

                    break;
                }
                case VolumeMode_Data.Buy_Sell:
                {
                    if (!ResultParams.ShowResults)
                        break;
                        
                    int volBuy = VolumesRank_Up.Values.Sum();
                    int volSell = VolumesRank_Down.Values.Sum();

                    if (ResultParams.ShowSideTotal)
                    {
                        Color colorLeft = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : SellColor;
                        Color colorRight = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : BuyColor;

                        int percentBuy = (volBuy * 100) / (volBuy + volSell);
                        int percentSell = (volSell * 100) / (volBuy + volSell);

                        string volBuyFmtd = FormatResults ? FormatBigNumber(volBuy) : $"{volBuy}";
                        string volSellFmtd = FormatResults ? FormatBigNumber(volSell) : $"{volSell}";

                        string strBuy = ResultParams.ResultsView_Input switch {
                            ResultsView_Data.Percentage => $"\n{percentBuy}%",
                            ResultsView_Data.Value => $"\n{volBuyFmtd}",
                            _ => $"\n{percentBuy}%\n({volBuyFmtd})"
                        };
                        string strSell = ResultParams.ResultsView_Input switch {
                            ResultsView_Data.Percentage => $"\n{percentSell}%",
                            ResultsView_Data.Value => $"\n{volSellFmtd}",
                            _ => $"\n{percentSell}%\n({volSellFmtd})"
                        };

                        DrawOrCache(new DrawInfo
                        {
                            BarIndex = iStart,
                            Type = DrawType.Text,
                            Id = $"{iStart}_SellSideSum",
                            Text = strSell,
                            X1 = xBar,
                            Y1 = lowestHalf,
                            horizontalAlignment = HorizontalAlignment.Left,
                            FontSize = FontSizeResults,
                            Color = colorLeft
                        });

                        DrawOrCache(new DrawInfo
                        {
                            BarIndex = iStart,
                            Type = DrawType.Text,
                            Id = $"{iStart}_BuySideSum",
                            Text = strBuy,
                            X1 = xBar,
                            Y1 = lowestHalf,
                            horizontalAlignment = HorizontalAlignment.Right,
                            FontSize = FontSizeResults,
                            Color = colorRight
                        });
                    }

                    double sumValue = volBuy + volSell;
                    double subtValue = volBuy - volSell;

                    string sumFmtd = FormatResults ? FormatBigNumber(sumValue) : $"{sumValue}";

                    string subtValueFmtd = subtValue > 0 ? FormatBigNumber(subtValue) : $"-{FormatBigNumber(Math.Abs(subtValue))}";
                    string subtFmtd = FormatResults ? subtValueFmtd : $"{subtValue}";

                    string strFormated = ResultParams.OperatorBuySell_Input == OperatorBuySell_Data.Sum ? sumFmtd : subtFmtd;

                    Color compareColor = volBuy > volSell ? BuyColor : volBuy < volSell ? SellColor : RtnbFixedColor;
                    Color colorCenter = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : compareColor;

                    ResultsView_Data selectedView = ResultParams.ResultsView_Input;
                    bool showSide_notBoth = ResultParams.ShowSideTotal && (selectedView == ResultsView_Data.Percentage || selectedView == ResultsView_Data.Value);
                    bool showSide_Both = ResultParams.ShowSideTotal && selectedView == ResultsView_Data.Both;
                    string dynSpaceSum = showSide_notBoth ? $"\n\n\n" :
                                         showSide_Both ? $"\n\n\n\n" : "\n";

                    if (ResultParams.EnableLargeFilter)
                    {
                        double seriesValue = Dynamic_Series[iStart];
                        // ====== Strength Filter ======
                        double filterValue = 0;
                        if (UseCustomMAs)
                            filterValue = CustomMAs(seriesValue, iStart, ResultParams.MAperiod, CustomMAType.Large);
                        else
                            filterValue = MADynamic_LargeFilter.Result[iStart];

                        double bsStrength = seriesValue / filterValue;
                        Color filterColor = bsStrength >= ResultParams.LargeRatio ? ColorLargeResult : colorCenter;

                        colorCenter = filterColor;
                        if (LargeFilter_ColoringBars && filterColor == ColorLargeResult)
                            Chart.SetBarFillColor(iStart, ColorLargeResult);
                        else
                            Chart.SetBarFillColor(iStart, isBullish ? Chart.ColorSettings.BullFillColor : Chart.ColorSettings.BearFillColor);
                    }

                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = iStart,
                        Type = DrawType.Text,
                        Id = $"{iStart}_BSResultOperator",
                        Text = $"{dynSpaceSum}{strFormated}",
                        X1 = xBar,
                        Y1 = lowestHalf,
                        horizontalAlignment = HorizontalAlignment.Center,
                        FontSize = FontSizeResults,
                        Color = colorCenter
                    });

                    break;
                }
                default:
                {
                    int deltaTotal = Delta_Results[iStart];
                    int prevDeltaTotal = Delta_Results[iStart - 1];

                    int deltaChange = DeltaChange_Results[iStart];
                    int prevDeltaChange = DeltaChange_Results[iStart - 1];

                    int deltaBuySell_Sum = DeltaBuySell_Sum_Results[iStart];
                    int prevDelta_BuySell_Sum = DeltaBuySell_Sum_Results[iStart - 1];

                    int minDelta = MinMaxDelta[0];
                    int maxDelta = MinMaxDelta[1];

                    int subtDelta = 0;
                    int prevSubtDelta = 0;
                    int sumDelta = 0;
                    int prevSumDelta = 0;
                    if (ResultParams.ShowMinMaxDelta)
                    {
                        subtDelta = SubtractDelta_Results[iStart];
                        prevSubtDelta = SubtractDelta_Results[iStart - 1];
                        sumDelta = SumDelta_Results[iStart];
                        prevSumDelta = SumDelta_Results[iStart - 1];
                    }

                    if (ResultParams.ShowResults)
                    {
                        if (ResultParams.ShowSideTotal)
                        {
                            int deltaBuy = DeltaRank.Values.Where(n => n > 0).Sum();
                            int deltaSell = DeltaRank.Values.Where(n => n < 0).Sum();

                            int percentBuy = 0;
                            int percentSell = 0;
                            try { percentBuy = (deltaBuy * 100) / (deltaBuy + Math.Abs(deltaSell)); } catch { }
                            try { percentSell = (deltaSell * 100) / (deltaBuy + Math.Abs(deltaSell)); } catch { }

                            string deltaBuyFmtd = FormatResults ? FormatBigNumber(deltaBuy) : $"{deltaBuy}";
                            string deltaSellFmtd = FormatResults ? FormatBigNumber(deltaSell) : $"{deltaSell}";
                            
                            string strBuy = ResultParams.ResultsView_Input switch {
                                ResultsView_Data.Percentage => $"\n{percentBuy}%",
                                ResultsView_Data.Value => $"\n{deltaBuyFmtd}",
                                _ => $"\n{percentBuy}%\n({deltaBuyFmtd})"
                            };
                            string strSell = ResultParams.ResultsView_Input switch {
                                ResultsView_Data.Percentage => $"\n{percentSell}%",
                                ResultsView_Data.Value => $"\n{deltaSellFmtd}",
                                _ => $"\n{percentSell}%\n({deltaSellFmtd})"
                            };

                            Color colorLeft = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : SellColor;
                            Color colorRight = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : BuyColor;

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_DeltaSellSideSum",
                                Text = strSell,
                                X1 = xBar,
                                Y1 = lowestHalf,
                                horizontalAlignment = HorizontalAlignment.Left,
                                FontSize = FontSizeResults,
                                Color = colorLeft
                            });

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_DeltaBuySideSum",
                                Text = strBuy,
                                X1 = xBar,
                                Y1 = lowestHalf,
                                horizontalAlignment = HorizontalAlignment.Right,
                                FontSize = FontSizeResults,
                                Color = colorRight
                            });
                        }

                        string deltaValueFmtd = deltaTotal > 0 ? FormatBigNumber(deltaTotal) : $"-{FormatBigNumber(Math.Abs(deltaTotal))}";
                        string deltaFmtd = FormatResults ? deltaValueFmtd : $"{deltaTotal}";

                        ResultsView_Data selectedView = ResultParams.ResultsView_Input;
                        bool showSide_notBoth = ResultParams.ShowSideTotal && (selectedView == ResultsView_Data.Percentage || selectedView == ResultsView_Data.Value);
                        bool showSide_Both = ResultParams.ShowSideTotal && selectedView == ResultsView_Data.Both;
                        string dynSpaceSum = showSide_notBoth ? $"\n\n\n" :
                                             showSide_Both ? $"\n\n\n\n" : "\n";

                        Color compareSum = deltaTotal > 0 ? BuyColor : deltaTotal < 0 ? SellColor : RtnbFixedColor;
                        Color colorCenter = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : compareSum;

                        if (ResultParams.ShowMinMaxDelta)
                        {
                            string buysellsumValueFmtd = FormatBigNumber(deltaBuySell_Sum);

                            string minValueFmtd = minDelta > 0 ? FormatBigNumber(minDelta) : $"-{FormatBigNumber(Math.Abs(minDelta))}";
                            string maxValueFmtd = maxDelta > 0 ? FormatBigNumber(maxDelta) : $"-{FormatBigNumber(Math.Abs(maxDelta))}";
                            string subtValueFmtd = subtDelta > 0 ? FormatBigNumber(subtDelta) : $"-{FormatBigNumber(Math.Abs(subtDelta))}";
                            string sumValueFmtd = FormatBigNumber(sumDelta);
                            
                            string buysellsumFmtd = FormatResults ? buysellsumValueFmtd : $"{deltaBuySell_Sum}";
                            
                            string minDeltaFmtd = FormatResults ? minValueFmtd : $"{minDelta}";
                            string maxDeltaFmtd = FormatResults ? maxValueFmtd : $"{maxDelta}";
                            string subtDeltaFmtd = FormatResults ? subtValueFmtd : $"{subtDelta}";
                            string sumDeltaFmtd = FormatResults ? sumValueFmtd : $"{sumDelta}";

                            Color subtractColor = colorCenter;
                            if (ResultParams.EnableLargeFilter)
                            {
                                double absSubtValue = SubtractDelta_Series[iStart];
                                // ====== Strength Filter ======
                                double filterValue = 0;
                                if (UseCustomMAs)
                                    filterValue = CustomMAs(
                                        absSubtValue,
                                        iStart, ResultParams.MAperiod,
                                        CustomMAType.Large, DeltaSwitch.Subtract
                                    );
                                else
                                    filterValue = MASubtract_LargeFilter.Result[iStart];

                                double subtractLargeStrength = absSubtValue / filterValue;
                                Color filterColor = subtractLargeStrength >= ResultParams.LargeRatio ? ColorLargeResult : colorCenter;
                                subtractColor = filterColor;
                            }

                            HorizontalAlignment hAligh = HorizontalAlignment.Center;
                            int fontSize = FontSizeResults - 1;
                            if (!ResultParams.ShowOnlySubtDelta)
                            {
                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_Delta_BuySellSum_Result",
                                    Text = $"\n\n{dynSpaceSum}buy_sell:{buysellsumFmtd}",
                                    X1 = xBar,
                                    Y1 = lowestHalf,
                                    horizontalAlignment = hAligh,
                                    FontSize = fontSize,
                                    Color = colorCenter
                                });
                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_MinDeltaResult",
                                    Text = $"\n\n\n\n{dynSpaceSum}min:{minDeltaFmtd}",
                                    X1 = xBar,
                                    Y1 = lowestHalf,
                                    horizontalAlignment = hAligh,
                                    FontSize = fontSize,
                                    Color = colorCenter
                                });

                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_MaxDeltaResult",
                                    Text = $"\n\n\n\n\n\n{dynSpaceSum}max:{maxDeltaFmtd}",
                                    X1 = xBar,
                                    Y1 = lowestHalf,
                                    horizontalAlignment = hAligh,
                                    FontSize = fontSize,
                                    Color = colorCenter
                                });

                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_SubtDeltaResult",
                                    Text = $"\n\n\n\n\n\n\n\n{dynSpaceSum}subt:{subtDeltaFmtd}",
                                    X1 = xBar,
                                    Y1 = lowestHalf,
                                    horizontalAlignment = hAligh,
                                    FontSize = fontSize,
                                    Color = subtractColor
                                });

                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_SumDeltaResult",
                                    Text = $"\n\n\n\n\n\n\n\n\n\n{dynSpaceSum}sum:{sumDeltaFmtd}",
                                    X1 = xBar,
                                    Y1 = lowestHalf,
                                    horizontalAlignment = hAligh,
                                    FontSize = fontSize,
                                    Color = colorCenter
                                });
                            }
                            else
                            {
                                DrawOrCache(new DrawInfo
                                {
                                    BarIndex = iStart,
                                    Type = DrawType.Text,
                                    Id = $"{iStart}_SubtDeltaResult",
                                    Text = $"\n\n{dynSpaceSum}subt:{subtDeltaFmtd}",
                                    X1 = xBar,
                                    Y1 = lowestHalf,
                                    horizontalAlignment = hAligh,
                                    FontSize = fontSize,
                                    Color = subtractColor
                                });
                            }
                        }

                        string changeValueFmtd = deltaChange > 0 ? FormatBigNumber(deltaChange) : $"-{FormatBigNumber(Math.Abs(deltaChange))}";
                        string changeFmtd = FormatResults ? changeValueFmtd : $"{deltaChange}";

                        Color compareChange = deltaChange > prevDeltaChange ? BuyColor : deltaChange < prevDeltaChange ? SellColor : RtnbFixedColor;
                        Color colorChange = ResultsColoring_Input == ResultsColoring_Data.Fixed ? RtnbFixedColor : compareChange;

                        if (ResultParams.EnableLargeFilter)
                        {
                            double seriesValue = Dynamic_Series[iStart];
                            // ====== Strength Filter ======
                            double filterValue = 0;
                            if (UseCustomMAs)
                                filterValue = CustomMAs(seriesValue, iStart, ResultParams.MAperiod, CustomMAType.Large);
                            else
                                filterValue = MADynamic_LargeFilter.Result[iStart];

                            double deltaLargeStrength = seriesValue / filterValue;
                            Color filterColor = deltaLargeStrength >= ResultParams.LargeRatio ? ColorLargeResult : colorCenter;

                            colorCenter = filterColor;
                            if (LargeFilter_ColoringBars && filterColor == ColorLargeResult)
                                Chart.SetBarFillColor(iStart, ColorLargeResult);
                            else
                                Chart.SetBarFillColor(iStart, isBullish ? Chart.ColorSettings.BullFillColor : Chart.ColorSettings.BearFillColor);

                            if (LargeFilter_ColoringCD)
                                colorChange = filterColor == ColorLargeResult ? filterColor : colorChange;
                        }

                        DrawOrCache(new DrawInfo
                        {
                            BarIndex = iStart,
                            Type = DrawType.Text,
                            Id = $"{iStart}_DeltaTotal",
                            Text = $"{dynSpaceSum}{deltaFmtd}",
                            X1 = xBar,
                            Y1 = lowestHalf,
                            horizontalAlignment = HorizontalAlignment.Center,
                            FontSize = FontSizeResults,
                            Color = colorCenter
                        });

                        DrawOrCache(new DrawInfo
                        {
                            BarIndex = iStart,
                            Type = DrawType.Text,
                            Id = $"{iStart}_DeltaChange",
                            Text = $"{changeFmtd}",
                            X1 = xBar,
                            Y1 = highestHalf,
                            horizontalAlignment = HorizontalAlignment.Center,
                            verticalAlignment = VerticalAlignment.Top,
                            FontSize = FontSizeResults,
                            Color = colorChange
                        });
                    }

                    // ====== Delta Bubbles Chart ======
                    if (BubblesChartParams.EnableBubblesChart)
                    {
                        IndicatorDataSeries sourceSeries = BubblesChartParams.UseChangeSeries ? DeltaChange_Series :
                        BubblesChartParams.BubblesSource_Input switch {
                            BubblesSource_Data.Delta_BuySell_Sum => DeltaBuySell_Sum_Series,
                            BubblesSource_Data.Subtract_Delta => SubtractDelta_Series,
                            BubblesSource_Data.Sum_Delta => SumDelta_Series,
                            _ => Dynamic_Series
                        };

                        double sourceValue = sourceSeries[iStart];

                        DeltaSwitch deltaSwitch = BubblesChartParams.UseChangeSeries ? DeltaSwitch.DeltaChange : 
                        BubblesChartParams.BubblesSource_Input switch {
                            BubblesSource_Data.Delta_BuySell_Sum => DeltaSwitch.DeltaBuySell_Sum,
                            BubblesSource_Data.Subtract_Delta => DeltaSwitch.Subtract,
                            BubblesSource_Data.Sum_Delta => DeltaSwitch.Sum,
                            _ => DeltaSwitch.None
                        };

                        double[] window = new double[BubblesChartParams.MAperiod];
                        if (BubblesChartParams.BubblesFilter_Input == BubblesFilter_Data.SoftMax_Power ||
                            BubblesChartParams.BubblesFilter_Input == BubblesFilter_Data.L2Norm ||
                            BubblesChartParams.BubblesFilter_Input == BubblesFilter_Data.MinMax)
                        {
                            for (int k = 0; k < BubblesChartParams.MAperiod; k++)
                                window[k] = sourceSeries[iStart - BubblesChartParams.MAperiod + 1 + k];
                        }

                        double deltaStrength = 0.0;
                        double filterValue = 1.0;
                        switch (BubblesChartParams.BubblesFilter_Input)
                        {
                            case BubblesFilter_Data.MA:
                                if (UseCustomMAs)
                                    filterValue = CustomMAs(sourceValue, iStart,
                                        BubblesChartParams.MAperiod, CustomMAType.Bubbles,
                                        deltaSwitch, MASwitch.Bubbles, false
                                    );
                                else
                                    filterValue = BubblesChartParams.UseChangeSeries ? MABubbles_DeltaChange.Result[iStart] :
                                    BubblesChartParams.BubblesSource_Input switch {
                                        BubblesSource_Data.Delta_BuySell_Sum => MABubbles_DeltaBuySell_Sum.Result[iStart],
                                        BubblesSource_Data.Subtract_Delta => MABubbles_SubtractDelta.Result[iStart],
                                        BubblesSource_Data.Sum_Delta => MABubbles_SumDelta.Result[iStart],
                                        _ => MABubbles_Delta.Result[iStart]
                                    };
                                deltaStrength = sourceValue / filterValue;
                                break;
                            case BubblesFilter_Data.Standard_Deviation:
                                if (UseCustomMAs)
                                    filterValue = CustomMAs(sourceValue, iStart,
                                        BubblesChartParams.MAperiod, CustomMAType.Bubbles,
                                        deltaSwitch, MASwitch.Bubbles, true
                                    );
                                else
                                    filterValue = BubblesChartParams.UseChangeSeries ? StdDevBubbles_DeltaChange.Result[iStart] :
                                    BubblesChartParams.BubblesSource_Input switch {
                                        BubblesSource_Data.Delta_BuySell_Sum => StdDevBubbles_DeltaBuySell_Sum.Result[iStart],
                                        BubblesSource_Data.Subtract_Delta => StdDevBubbles_SubtractDelta.Result[iStart],
                                        BubblesSource_Data.Sum_Delta => StdDevBubbles_SumDelta.Result[iStart],
                                        _ => StdDevBubbles_Delta.Result[iStart]
                                    };
                                deltaStrength = sourceValue / filterValue;
                                break;
                            case BubblesFilter_Data.Both:
                                double ma;
                                if (UseCustomMAs)
                                    ma = CustomMAs(sourceValue, iStart,
                                        BubblesChartParams.MAperiod, CustomMAType.Bubbles,
                                        deltaSwitch, MASwitch.Bubbles, false
                                    );
                                else
                                    ma = BubblesChartParams.UseChangeSeries ? MABubbles_DeltaChange.Result[iStart] :
                                    BubblesChartParams.BubblesSource_Input switch {
                                        BubblesSource_Data.Delta_BuySell_Sum => MABubbles_DeltaBuySell_Sum.Result[iStart],
                                        BubblesSource_Data.Subtract_Delta => MABubbles_SubtractDelta.Result[iStart],
                                        BubblesSource_Data.Sum_Delta => MABubbles_SumDelta.Result[iStart],
                                        _ => MABubbles_Delta.Result[iStart]
                                    };

                                double stddev;
                                if (UseCustomMAs)
                                    stddev = CustomMAs(sourceValue, iStart,
                                        BubblesChartParams.MAperiod, CustomMAType.Bubbles,
                                        deltaSwitch, MASwitch.Bubbles, true
                                    );
                                else
                                    stddev = BubblesChartParams.UseChangeSeries ? StdDevBubbles_DeltaChange.Result[iStart] :
                                    BubblesChartParams.BubblesSource_Input switch {
                                        BubblesSource_Data.Delta_BuySell_Sum => StdDevBubbles_DeltaBuySell_Sum.Result[iStart],
                                        BubblesSource_Data.Subtract_Delta => StdDevBubbles_SubtractDelta.Result[iStart],
                                        BubblesSource_Data.Sum_Delta => StdDevBubbles_SumDelta.Result[iStart],
                                        _ => StdDevBubbles_Delta.Result[iStart]
                                    };

                                deltaStrength = (sourceValue - ma) / stddev;
                                break;
                            case BubblesFilter_Data.SoftMax_Power:
                                deltaStrength = Filters.PowerSoftmax_Strength(window);
                                break;
                            case BubblesFilter_Data.L2Norm:
                                deltaStrength = Filters.L2Norm_Strength(window);
                                break;
                            case BubblesFilter_Data.MinMax:
                                deltaStrength = Filters.MinMax_Strength(window);
                                break;
                        }

                        deltaStrength = Math.Round(deltaStrength, 2);

                        if (BubblesRatioParams.BubblesRatio_Input == BubblesRatio_Data.Percentile)
                        {
                            PercentileRatio_Series[iStart] = deltaStrength;

                            double[] windowRatio = new double[BubblesRatioParams.PctilePeriod];
                            for (int i = 0; i < BubblesRatioParams.PctilePeriod; i++) {
                                windowRatio[i] = PercentileRatio_Series[iStart - BubblesRatioParams.PctilePeriod + 1 + i];
                            }

                            deltaStrength = Filters.RollingPercentile(windowRatio);
                            deltaStrength = Math.Round(deltaStrength, 1);
                        }

                        // Ratios
                        double lowestValue = BubblesRatioParams.Lowest_PctileValue;
                        double lowValue = BubblesRatioParams.Low_PctileValue;
                        double averageValue = BubblesRatioParams.Average_PctileValue;
                        double highValue = BubblesRatioParams.High_PctileValue;
                        double ultraValue = BubblesRatioParams.Ultra_PctileValue;

                        if (BubblesRatioParams.BubblesRatio_Input == BubblesRatio_Data.Fixed) 
                        {
                            lowestValue = BubblesRatioParams.Lowest_FixedValue;
                            lowValue = BubblesRatioParams.Low_FixedValue;
                            averageValue = BubblesRatioParams.Average_FixedValue;
                            highValue = BubblesRatioParams.High_FixedValue;
                            ultraValue = BubblesRatioParams.Ultra_FixedValue;
                        }

                        // Filter + Size for Bubbles
                        double filterSize = deltaStrength < lowestValue ? 2 :   // 1 = too small
                                            deltaStrength < lowValue ? 2.5 :
                                            deltaStrength < averageValue ? 3 :
                                            deltaStrength < highValue ? 4 :
                                            deltaStrength >= ultraValue ? 5 : 5;

                        // Coloring
                        Color heatColor = filterSize == 2 ? HeatmapLowest_Color :
                                          filterSize == 2.5 ? HeatmapLow_Color :
                                          filterSize == 3 ? HeatmapAverage_Color :
                                          filterSize == 4 ? HeatmapHigh_Color : HeatmapUltra_Color;

                        bool sourceFading = BubblesChartParams.UseChangeSeries ? deltaChange > prevDeltaChange :
                        BubblesChartParams.BubblesSource_Input switch {
                            BubblesSource_Data.Delta_BuySell_Sum => deltaBuySell_Sum > prevDelta_BuySell_Sum,
                            BubblesSource_Data.Subtract_Delta => subtDelta > prevSubtDelta,
                            BubblesSource_Data.Sum_Delta => sumDelta > prevSumDelta,
                            _ => deltaTotal > prevDeltaTotal
                        };
                        bool sourcePositiveNegative = BubblesChartParams.UseChangeSeries ? deltaChange > 0 :
                        BubblesChartParams.BubblesSource_Input switch {
                            BubblesSource_Data.Delta_BuySell_Sum => deltaBuySell_Sum > 0,
                            BubblesSource_Data.Subtract_Delta => subtDelta > 0,
                            BubblesSource_Data.Sum_Delta => sumDelta > 0,
                            _ => deltaTotal > 0
                        };

                        Color fadingColor = sourceFading ? BuyColor : SellColor;
                        Color positiveNegativeColor = sourcePositiveNegative ? BuyColor : SellColor;

                        Color momentumColor = BubblesChartParams.BubblesMomentum_Input == BubblesMomentum_Data.Fading ? fadingColor : positiveNegativeColor;
                        Color colorMode = BubblesChartParams.BubblesColoring_Input == BubblesColoring_Data.Heatmap ? heatColor : momentumColor;

                        // X-value
                        (double x1Position, double dynLength) CalculateX1X2(double maxLength)
                        {
                            double maxLengthUltra = maxLength * 1.4 * BubblesChartParams.BubblesSizeMultiplier; // Slightly bigger than Bar Body
                            double maxLengthBubble = maxLength * BubblesChartParams.BubblesSizeMultiplier;

                            double dynMaxProportion = filterSize == 5 ? maxLengthUltra : maxLengthBubble;
                            double proportion = filterSize * (dynMaxProportion / 3);

                            double dynMaxLength = filterSize == 5 ? 5 : 4;
                            double dynLength = proportion / dynMaxLength;

                            // X1 position from LeftSide
                            double x1Position = filterSize == 5 ? -(maxLengthUltra / 3) :
                                                filterSize == 4 ? -(maxLengthBubble / 3) :
                                                filterSize == 3 ? -(maxLengthBubble / 4) :
                                                filterSize == 2.5 ? -(maxLengthBubble / 5) :
                                                                    -(maxLengthBubble / 6);

                            return (x1Position, dynLength);
                        }
                        // X1 to Left / x2 to Middle
                        var (x1Position, dynLength_ToMiddle) = CalculateX1X2(maxLength_LeftSide);

                        // x2 from Middle to Right
                        var (_, dynLength_ToRight) = CalculateX1X2(maxLength_RightSide);

                        bool isPriceToAvoid = isPriceBased_Chart && avoidStretching;

                        DateTime x1 = xBar.AddMilliseconds(x1Position);
                        DateTime x2 = x1.AddMilliseconds(dynLength_ToMiddle).AddMilliseconds(isPriceToAvoid || gapWeekday ? 0 : dynLength_ToRight);

                        // Y-Value
                        double maxHeightBubble = heightPips * BubblesChartParams.BubblesSizeMultiplier;
                        double proportionHeight = filterSize * maxHeightBubble;
                        double dynHeight = proportionHeight / 5;

                        double y1 = Bars.ClosePrices[iStart] + (Symbol.PipSize * dynHeight);
                        double y2 = Bars.ClosePrices[iStart] - (Symbol.PipSize * dynHeight);

                        // Draw
                        Color colorModeWithAlpha = Color.FromArgb((int)(2.55 * BubblesOpacity), colorMode.R, colorMode.G, colorMode.B);
                        DrawOrCache(new DrawInfo
                        {
                            BarIndex = iStart,
                            Type = DrawType.Ellipse,
                            Id = $"{iStart}_Bubble",
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            Color = colorModeWithAlpha
                        });

                        if (MiscParams.ShowBubbleValue)
                        {
                            string deltaFmtd = deltaTotal > 0 ? FormatBigNumber(deltaTotal) : $"-{FormatBigNumber(Math.Abs(deltaTotal))}";
                            string changeFmtd = deltaChange > 0 ? FormatBigNumber(deltaChange) : $"-{FormatBigNumber(Math.Abs(deltaChange))}";
                            string buysellsum_Fmtd = FormatBigNumber(deltaBuySell_Sum);
                            string subtFmtd = subtDelta > 0 ? FormatBigNumber(subtDelta) : $"-{FormatBigNumber(Math.Abs(subtDelta))}";
                            string sumFmtd = FormatBigNumber(sumDelta);

                            string dynBubbleValue =  BubblesChartParams.UseChangeSeries ? changeFmtd :
                            BubblesChartParams.BubblesSource_Input switch {
                                BubblesSource_Data.Delta_BuySell_Sum => buysellsum_Fmtd,
                                BubblesSource_Data.Subtract_Delta => subtFmtd,
                                BubblesSource_Data.Sum_Delta => sumFmtd,
                                _ => deltaFmtd
                            };

                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_BubbleValue",
                                Text = dynBubbleValue,
                                X1 = xBar,
                                Y1 = Bars[iStart].Close,
                                horizontalAlignment = isPriceToAvoid ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                                verticalAlignment = VerticalAlignment.Center,
                                FontSize = FontSizeResults,
                                Color = RtnbFixedColor
                            });
                        }
                        if (BubblesRatioParams.ShowStrengthValue)
                        {
                            DrawOrCache(new DrawInfo
                            {
                                BarIndex = iStart,
                                Type = DrawType.Text,
                                Id = $"{iStart}_BubbleStrengthValue",
                                Text = $"{deltaStrength}",
                                X1 = xBar,
                                Y1 = y2, // bottom of bubble
                                horizontalAlignment = isPriceToAvoid ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                                verticalAlignment = VerticalAlignment.Center,
                                FontSize = FontSizeNumbers,
                                Color = RtnbFixedColor
                            });
                        }

                        if (BubblesLevelParams.EnableUltraNotification && BooleanLocks.lastIsUltra && !BooleanLocks.ultraNotify && BooleanLocks.ultraNotify_NewBar)
                        {
                            string symbolName = $"{Symbol.Name} ({Chart.TimeFrame.ShortName})";
                            string sourceString = BubblesChartParams.BubblesSource_Input.ToString();
                            string popupText = $"{symbolName} => Ultra {sourceString} at {Server.Time}";
                            
                            switch (BubblesLevelParams.NotificationType_Input) {
                                case NotificationType_Data.Sound:
                                    Notifications.PlaySound(BubblesLevelParams.Ultra_SoundType);
                                    break;
                                case NotificationType_Data.Popup:
                                    Notifications.ShowPopup(NOTIFY_CAPTION, popupText, PopupNotificationState.Information);
                                    break;
                                default:
                                    Notifications.PlaySound(BubblesLevelParams.Ultra_SoundType);
                                    Notifications.ShowPopup(NOTIFY_CAPTION, popupText, PopupNotificationState.Information);
                                    break;
                            }
                            
                            BooleanLocks.ultraNotify = true;
                            BooleanLocks.ultraNotify_NewBar = false;
                        }
                        // At the final loop when the bar is closed, if filterSize == 5, notify in the next bar.
                        // When Backtesting in Price-Based Charts, this condition doesn't seem to be triggered,
                        // Works fine in real-time market though.
                        if (filterSize == 5) {
                            BooleanLocks.ultraNotify_NewBar = false;
                            BooleanLocks.lastIsUltra = true;
                        }
                        else
                            BooleanLocks.lastIsUltra = false;

                        // === Ultra Bubbles Levels ====
                        if (BubblesLevelParams.ShowUltraLevels)
                        {
                            // Main logic by LLM
                            // Fixed and modified for the desired behavior
                            /*
                               The idea (count bars that pass or touch it to break it)
                               was made by human creativity => aka cheap copy of:
                               - Shved Supply and Demand indicator without (verified, untested, etc..) info.
                               Yes, I was a MT4 enjoyer.
                            */
                            // 'open' already declared.
                            double close = Bars.ClosePrices[iStart];
                            double high = Bars.HighPrices[iStart];
                            double low = Bars.LowPrices[iStart];

                            // Check touches for all active rectangles
                            if (!BooleanLocks.ultraLevels)
                            {
                                foreach (var rect in ultraRectangles.Values)
                                {
                                    if (!rect.isActive)
                                        continue;

                                    double top = Math.Max(rect.Y1, rect.Y2);
                                    double bottom = Math.Min(rect.Y1, rect.Y2);

                                    // Check OHLC one by one
                                    if (TouchesRect_Bubbles(open, high, low, close, top, bottom, BubblesLevelParams.UltraBubblesBreak_Input))
                                    {
                                        rect.Touches++;

                                        // Update label
                                        if (UltraBubbles_ShowValue)
                                            UpdateLabel_Bubbles(rect, top, Bars.OpenTimes[iStart]);

                                        if (rect.Touches >= BubblesLevelParams.MaxCount)
                                        {
                                            rect.isActive = false;

                                            // Stop extension → fix rectangle to current bar
                                            rect.Rectangle.Time2 = Bars.OpenTimes[iStart];
                                            rect.Rectangle.Color = Color.FromArgb(50, rect.Rectangle.Color);

                                            // Finalize label
                                            if (UltraBubbles_ShowValue)
                                            {
                                                rect.Text.Text = $"{rect.Touches}";
                                                rect.Text.Color = RtnbFixedColor;
                                            }
                                        }
                                    }
                                }

                                BooleanLocks.ultraLevels = true;
                            }

                            // Stretch
                            foreach (var rect in ultraRectangles.Values)
                            {
                                if (!rect.isActive)
                                    continue;
                                // Historical not desactivated yet;
                                if (UltraBubbles_ShowValue)
                                    rect.Text.Time = Bars.LastBar.OpenTime;

                                if (rect.Rectangle.Time2 == Bars.LastBar.OpenTime)
                                    continue;
                                rect.Rectangle.Time2 = Bars.LastBar.OpenTime;
                            }

                            // Create new rectangle for each Ultra Bubble
                            if (filterSize == 5)
                            {
                                bool isUltraColor = BubblesLevelParams.UltraBubblesColoring_Input == UltraBubblesColoring_Data.Bubble_Color;

                                if (BubblesLevelParams.UltraBubbles_RectSizeInput == UltraBubbles_RectSizeData.High_Low)
                                    CreateRect_Bubbles(high, low, iStart, isUltraColor ? HeatmapUltra_Color : positiveNegativeColor);
                                else if (BubblesLevelParams.UltraBubbles_RectSizeInput == UltraBubbles_RectSizeData.HighOrLow_Close)
                                    CreateRect_Bubbles(close > open ? high : low, close, iStart, isUltraColor ? HeatmapUltra_Color : positiveNegativeColor);
                                else
                                    CreateRect_Bubbles(y1, y2, iStart, isUltraColor ? HeatmapUltra_Color : positiveNegativeColor);
                            }
                        }
                    }

                    break;
                }
            }

            // TCP SOCKET EXPORT LOGIC 
            if (ExportHistory)
            {
                ExportCsvData(iStart);
            }
            else if (IsLastBar)
            {
                SendSocketData(iStart);
            }
        }

        private void PopulateSeries(int iStart)
        {
            switch (GeneralParams.VolumeMode_Input)
            {
                case VolumeMode_Data.Normal:
                    Dynamic_Series[iStart] = VolumesRank.Values.Sum();
                    break;
                case VolumeMode_Data.Buy_Sell:
                    double sumValue = VolumesRank_Up.Values.Sum() + VolumesRank_Down.Values.Sum();
                    double subtValue = VolumesRank_Up.Values.Sum() - VolumesRank_Down.Values.Sum();
                    Dynamic_Series[iStart] = ResultParams.OperatorBuySell_Input == OperatorBuySell_Data.Sum ? sumValue : Math.Abs(subtValue);
                    break;
                default:
                {
                        
                    int deltaTotal = DeltaRank.Values.Sum();

                    int deltaBuy = DeltaRank.Values.Where(n => n > 0).Sum();
                    int deltaSell = DeltaRank.Values.Where(n => n < 0).Sum();
                    int deltaBuySell_Sum = deltaBuy + Math.Abs(deltaSell);
                    deltaBuySell_Sum = Math.Max(1, deltaBuySell_Sum);

                    int minDelta = MinMaxDelta[0];
                    int maxDelta = MinMaxDelta[1];
                    int subtDelta = minDelta - maxDelta;
                    int sumDelta = Math.Abs(minDelta) + Math.Abs(maxDelta);

                    bool isNoDraw_MinMax = SpikeFilterParams.SpikeSource_Input == SpikeSource_Data.Sum_Delta || 
                        BubblesChartParams.BubblesSource_Input switch {
                            BubblesSource_Data.Subtract_Delta =>  true,
                            BubblesSource_Data.Sum_Delta => true,
                            _ => false
                        };

                    // _Results = > original values for plus/minus checker (later)
                    if (!Delta_Results.ContainsKey(iStart))
                        Delta_Results.Add(iStart, deltaTotal);
                    else
                        Delta_Results[iStart] = deltaTotal;
                    
                    // Delta Sum (BuySell)
                    if (!DeltaBuySell_Sum_Results.ContainsKey(iStart))
                        DeltaBuySell_Sum_Results.Add(iStart, deltaBuySell_Sum);
                    else
                        DeltaBuySell_Sum_Results[iStart] = deltaBuySell_Sum;

                    // [Subtract, Sum] Delta => MinMax
                    if (ResultParams.ShowMinMaxDelta || isNoDraw_MinMax)
                    {
                        if (!SubtractDelta_Results.ContainsKey(iStart))
                            SubtractDelta_Results.Add(iStart, subtDelta);
                        else
                            SubtractDelta_Results[iStart] = subtDelta;

                        if (!SumDelta_Results.ContainsKey(iStart))
                            SumDelta_Results.Add(iStart, sumDelta);
                        else
                            SumDelta_Results[iStart] = sumDelta;
                    }

                    // Any Delta => Change
                    // Keep previous "Change" implementation for Delta(only)
                    int deltaChange = Delta_Results.Keys.Count <= 1 ? Delta_Results[iStart] : (Delta_Results[iStart] - Delta_Results[iStart - 1]);
                    
                    if (BubblesChartParams.EnableBubblesChart && BubblesChartParams.UseChangeSeries) {
                        deltaChange = BubblesChartParams.BubblesSource_Input switch {
                            BubblesSource_Data.Delta_BuySell_Sum => WindowChange(DeltaBuySell_Sum_Results, iStart),
                            BubblesSource_Data.Subtract_Delta => WindowChange(SubtractDelta_Results, iStart),
                            BubblesSource_Data.Sum_Delta => WindowChange(SumDelta_Results, iStart),
                            _ => WindowChange(Delta_Results, iStart)
                        };
                    }

                    if (!DeltaChange_Results.ContainsKey(iStart))
                        DeltaChange_Results.Add(iStart, deltaChange);
                    else
                        DeltaChange_Results[iStart] = deltaChange;
                    
                    // _Series => always use absolute values (positive)
                    Dynamic_Series[iStart] = Math.Abs(deltaTotal);
                    DeltaChange_Series[iStart] = Math.Abs(deltaChange);
                    DeltaBuySell_Sum_Series[iStart] = Math.Abs(deltaBuySell_Sum);
                    if (ResultParams.ShowMinMaxDelta || isNoDraw_MinMax)
                    {
                        SubtractDelta_Series[iStart] = Math.Abs(subtDelta);
                        SumDelta_Series[iStart] = Math.Abs(sumDelta);
                    }
                    break;
                }
            }
            int WindowChange(Dictionary<int, int> source, int index)
            {
                int period = BubblesChartParams.changePeriod;
                int result = source[index];

                if (period <= 1 || index == 0)
                    return result;

                int available = Math.Min(period - 1, index);

                for (int i = 1; i <= available; i++)
                {
                    switch (BubblesChartParams.ChangeOperator_Input) {
                        case ChangeOperator_Data.Plus_KeepSign:
                            result += source[index - i]; break;
                        case ChangeOperator_Data.Minus_KeepSign:
                            result -= source[index - i]; break;
                        case ChangeOperator_Data.Plus_Absolute:
                            result += Math.Abs(source[index - i]); break;
                        case ChangeOperator_Data.Minus_Absolue:
                            result -= Math.Abs(source[index - i]); break;
                    }
                    
                }
                
                return result;
            }
        }

    }
}
