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
        private void WyckoffAnalysis(int index)
        {
            if (index < 2)
                return;

            // ==== Time Filter ====
            DateTime openTime = Bars.OpenTimes[index];
            DateTime closeTime = Bars.OpenTimes[index + 1];
            if (IsLastBar)
                closeTime = UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart ? Server.Time : TicksOHLC.OpenTimes.LastValue;
            TimeSpan interval = closeTime.Subtract(openTime);
            double interval_ms = interval.TotalMilliseconds;

            // Dynamic TimeLapse Format
            string[] interval_timelapse = GetTimeLapse(interval_ms);
            double timelapse_Value = Convert.ToDouble(interval_timelapse[0]);
            string timelapse_Suffix = interval_timelapse[1];

            TimeSeries[index] = timelapse_Value;

            // ==== Strength Filter ====
            double volume = VolumeSeries[index];
            double time = TimeSeries[index];
            double volumeStrength = 0;
            double timeStrength = 0;
            switch (ColoringParams.StrengthFilter_Input)
            {
                case StrengthFilter_Data.MA: {
                    double maValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume) : MAVol.Result[index];
                    volumeStrength = volume / maValue;
                    // ========
                    maValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time) : MATime.Result[index];
                    timeStrength = time / maValue;
                    break;
                }
                case StrengthFilter_Data.Standard_Deviation: {
                    double  stddevValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume, true) : stdDev_Vol.Result[index];
                    volumeStrength = volume / stddevValue;
                    // ========
                    stddevValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time, true) : stdDev_Time.Result[index];
                    timeStrength = time / stddevValue;
                    break;
                }
                case StrengthFilter_Data.Both: {
                    double maValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume) : MAVol.Result[index];
                    double stddevValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume, true) : stdDev_Vol.Result[index];
                    volumeStrength = (volume - maValue) / stddevValue;
                    // ========
                    maValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time) : MATime.Result[index];
                    stddevValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time, true) : stdDev_Time.Result[index];
                    timeStrength = (time - maValue) / stddevValue;
                    break;
                }
                case StrengthFilter_Data.Normalized_Emphasized:
                    double Normalization(bool isTime = false) {
                        /*
                        ==== References for Normalized_Emphasized ====
                        (Normalized Volume Oscillator 2008/2014) (https://www.mql5.com/en/code/8208)
                        // (The key idea for normalized volume by average volume period)
                        (Volumes Emphasized.mq4) (???)
                        // (improvement of above indicator)

                        It seems to be... the most suitable filter approach for Time-Based Charts, without Candle Spread Analysis.
                        Since CFD's Volume can be very flat at higher Tick activity,
                        - the slightest value change will be highlighted... as in ODF_Ticks/AGG.
                        */
                        if (index < ColoringParams.NormalizePeriod)
                            return 0;

                        double avg = 0;
                        for (int j = index; j > index - ColoringParams.NormalizePeriod; j--) {
                            if (isTime)
                                avg += TimeSeries[j];
                            else
                                avg += VolumeSeries[j];
                        }

                        avg /= ColoringParams.NormalizePeriod;

                        double normalizedValue = isTime ? (time / avg) : (volume / avg);
                        double normalizedPercentage = (normalizedValue * 100) - 100;
                        normalizedPercentage *= ColoringParams.NormalizeMultiplier; // I've added this to get "less but meaningful" coloring

                        return normalizedPercentage;
                    }
                    volumeStrength = Normalization();
                    timeStrength = Normalization(true);
                    break;
                case StrengthFilter_Data.L1Norm:
                    double[] windowVolume = new double[ColoringParams.MAperiod];
                    double[] windowTime = new double[ColoringParams.MAperiod];

                    for (int i = 0; i < ColoringParams.MAperiod; i++) {
                        windowVolume[i] = VolumeSeries[index - ColoringParams.MAperiod + 1 + i];
                        windowTime[i] = TimeSeries[index - ColoringParams.MAperiod + 1 + i];
                    }

                    volumeStrength = Filters.L1NormStrength(windowVolume);
                    timeStrength = Filters.L1NormStrength(windowTime);
                    break;
            }

            // Keep negative values of Normalized_Emphasized
            if (ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized) {
                volumeStrength = Math.Abs(volumeStrength);
                timeStrength = Math.Abs(timeStrength);
            }

            volumeStrength = Math.Round(volumeStrength, 2);
            timeStrength = Math.Round(timeStrength, 2);

            if (ColoringParams.StrengthRatio_Input == StrengthRatio_Data.Percentile &&
                ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized)
            {
                StrengthSeries_Volume[index] = volumeStrength;
                StrengthSeries_Time[index] = timeStrength;

                double[] windowVolume = new double[ColoringParams.Pctile_Period];
                double[] windowTime = new double[ColoringParams.Pctile_Period];

                for (int i = 0; i < ColoringParams.Pctile_Period; i++) {
                    windowVolume[i] = StrengthSeries_Volume[index - ColoringParams.Pctile_Period + 1 + i];
                    windowTime[i] = StrengthSeries_Time[index - ColoringParams.Pctile_Period + 1 + i];
                }

                volumeStrength = Filters.RollingPercentile(windowVolume);
                volumeStrength = Math.Round(volumeStrength, 1);
                // ========
                timeStrength = Filters.RollingPercentile(windowTime);
                timeStrength = Math.Round(timeStrength, 1);
            }

            // ==== Drawing ====
            // Y-Axis
            bool isBullish = Bars.ClosePrices[index] > Bars.OpenPrices[index];
            double y_Close = Bars.ClosePrices[index];
            double y_Open = Bars.OpenPrices[index];

            // Coloring
            double colorTypeNumbers = WyckoffParams.NumbersColor_Input == NumbersColor_Data.Time ? timeStrength : volumeStrength;
            double colorTypeBars = WyckoffParams.BarsColor_Input== BarsColor_Data.Time ? timeStrength : volumeStrength;
            bool isNumbersOutside = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside || WyckoffParams.Numbers_Input == Numbers_Data.None;

            int alpha = (int)(2.55 * HeatmapBars_Opacity);
            Color lowestColor = isNumbersOutside ? HeatmapLowest_Color : Color.FromArgb(alpha, HeatmapLowest_Color);
            Color lowColor = isNumbersOutside ? HeatmapLow_Color : Color.FromArgb(alpha, HeatmapLow_Color);
            Color averageColor = isNumbersOutside ? HeatmapAverage_Color : Color.FromArgb(alpha, HeatmapAverage_Color);

            Color highColorUp = isNumbersOutside ? HeatmapHighUp_Color : Color.FromArgb(alpha, HeatmapHighUp_Color);
            Color highColorDown = isNumbersOutside ? HeatmapHighDown_Color : Color.FromArgb(alpha, HeatmapHighDown_Color);
            Color highColor = isBullish ? highColorUp : highColorDown;

            Color ultraColorUp = isNumbersOutside ? HeatmapUltraUp_Color : Color.FromArgb(alpha, HeatmapUltraUp_Color);
            Color ultraColorDown = isNumbersOutside ? HeatmapUltraDown_Color : Color.FromArgb(alpha, HeatmapUltraDown_Color);
            Color ultraColor = isBullish ? ultraColorUp : ultraColorDown;

            if (ColoringParams.StrengthFilter_Input == StrengthFilter_Data.Normalized_Emphasized) {
                // if negative, just to be sure.
                colorTypeBars = colorTypeBars < 0 ? 0 : colorTypeBars;
                colorTypeNumbers = colorTypeNumbers < 0 ? 0 : colorTypeNumbers;
            }

            // Ratio
            bool isFixed = ColoringParams.StrengthRatio_Input == StrengthRatio_Data.Fixed;

            double lowest = isFixed ? ColoringParams.Lowest_FixedValue : ColoringParams.Lowest_PctileValue;
            double low = isFixed ? ColoringParams.Low_FixedValue : ColoringParams.Low_PctileValue;
            double average = isFixed ? ColoringParams.Average_FixedValue : ColoringParams.Average_PctileValue;
            double high = isFixed ? ColoringParams.High_FixedValue : ColoringParams.High_PctileValue;
            double ultra = isFixed ? ColoringParams.Ultra_FixedValue : ColoringParams.Ultra_PctileValue;

            if (ColoringParams.StrengthFilter_Input == StrengthFilter_Data.Normalized_Emphasized) {
                lowest = ColoringParams.Lowest_PctValue;
                low = ColoringParams.Low_PctValue;
                average = ColoringParams.Average_PctValue;
                high = ColoringParams.High_PctValue;
                ultra = ColoringParams.Ultra_PctValue;
            }

            Color barColor = colorTypeBars < lowest ? lowestColor :
                             colorTypeBars < low ? lowColor :
                             colorTypeBars < average ? averageColor :
                             colorTypeBars < high ? highColor :
                             colorTypeBars >= ultra ? ultraColor : lowestColor;

            highColor = isBullish ? HeatmapHighUp_Color : HeatmapHighDown_Color;
            ultraColor = isBullish ? HeatmapUltraUp_Color : HeatmapUltraDown_Color;
            Color numberColor = colorTypeNumbers < lowest ? HeatmapLowest_Color :
                                colorTypeNumbers < low ? HeatmapLow_Color :
                                colorTypeNumbers < average ? HeatmapAverage_Color :
                                colorTypeNumbers < high ? highColor :
                                colorTypeNumbers >= ultra ? ultraColor : HeatmapLowest_Color;

            // Numbers
            timelapse_Value = Math.Round(timelapse_Value);
            string onlyTime = WyckoffParams.ShowOnlyLargeNumbers ?
                              (timeStrength > low ? timelapse_Value + timelapse_Suffix : "") :
                              timelapse_Value + timelapse_Suffix;

            string onlyVol = WyckoffParams.ShowOnlyLargeNumbers ?
                             (volumeStrength > low ? FormatBigNumber(volume) : "") :
                             FormatBigNumber(volume);

            string bothVolTime = WyckoffParams.NumbersBothPosition_Input == NumbersBothPosition_Data.Default ?
                                 $"{onlyTime}\n{onlyVol}" : $"{onlyVol}\n{onlyTime}";

            string numbersFmtd = WyckoffParams.Numbers_Input switch {
                Numbers_Data.Time => onlyTime,
                Numbers_Data.Volume => onlyVol,
                Numbers_Data.Both => bothVolTime,
                _ => ""
            };
            
            if (numbersFmtd != "")
            {
                double y1 = isBullish ? y_Close : y_Open;
                VerticalAlignment v_align = VerticalAlignment.Bottom;
                HorizontalAlignment h_align;

                if (Chart.ChartType != ChartType.Bars && Chart.ChartType != ChartType.Hlc)
                {
                    if (WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside && isBullish)
                        v_align = VerticalAlignment.Top;

                    if (!isBullish) {
                        v_align = VerticalAlignment.Bottom;
                        if (WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside)
                            y1 = y_Close;
                    }

                    h_align = HorizontalAlignment.Center;
                }
                else {
                    h_align = HorizontalAlignment.Stretch;
                    if (!isBullish) {
                        v_align = VerticalAlignment.Top;
                        y1 = y_Close;
                    }
                }

                DrawOrCache(new DrawInfo
                {
                    BarIndex = index,
                    Type = DrawType.Text,
                    Id = $"{index}_wyckoff",
                    Text = numbersFmtd,
                    X1 = Bars[index].OpenTime,
                    Y1 = y1,
                    horizontalAlignment = h_align,
                    verticalAlignment = v_align,
                    FontSize = FontSizeNumbers,
                    Color = WyckoffParams.NumbersColor_Input == NumbersColor_Data.CustomColor ? CustomNumbersColor : numberColor
                });
            }

            // Fill + Outline Settings
            if (!WyckoffParams.FillBars && !WyckoffParams.KeepOutline) {
                Chart.SetBarFillColor(index, Color.Transparent);
                Chart.SetBarOutlineColor(index, barColor);
                if (isBullish) UpWickColor = barColor;
                else DownWickColor = barColor;
            }
            else if (WyckoffParams.FillBars && WyckoffParams.KeepOutline) {
                Chart.SetBarFillColor(index, barColor);
                if (isBullish) UpWickColor = Chart.ColorSettings.BullOutlineColor;
                else DownWickColor = Chart.ColorSettings.BullOutlineColor;
            }
            else if (!WyckoffParams.FillBars && WyckoffParams.KeepOutline) {
                Chart.SetBarFillColor(index, Color.Transparent);
                if (isBullish) UpWickColor = Chart.ColorSettings.BullOutlineColor;
                else DownWickColor = Chart.ColorSettings.BullOutlineColor;
            }
            else {
                Chart.SetBarColor(index, barColor);
                if (isBullish) UpWickColor = barColor;
                else DownWickColor = barColor;
            }

            if (ColoringParams.ShowStrengthValue) {
                if (WyckoffParams.Numbers_Input == Numbers_Data.Volume || WyckoffParams.Numbers_Input == Numbers_Data.Both) {
                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = index,
                        Type = DrawType.Text,
                        Id = $"{index}_strengthVol",
                        Text = $"{volumeStrength}v",
                        X1 = Bars[index].OpenTime,
                        Y1 = isBullish ? Bars[index].High : Bars[index].Low,
                        horizontalAlignment = HorizontalAlignment.Center,
                        verticalAlignment = isBullish ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                        FontSize = FontSizeNumbers,
                        Color = CustomNumbersColor
                    });
                }
                if (WyckoffParams.Numbers_Input == Numbers_Data.Time || WyckoffParams.Numbers_Input == Numbers_Data.Both) {
                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = index,
                        Type = DrawType.Text,
                        Id = $"{index}_strengthTime",
                        Text = $"{timeStrength}ts",
                        X1 = Bars[index].OpenTime,
                        Y1 = isBullish ? Bars[index].Low : Bars[index].High,
                        horizontalAlignment = HorizontalAlignment.Center,
                        verticalAlignment = isBullish ? VerticalAlignment.Bottom : VerticalAlignment.Top,
                        FontSize = FontSizeNumbers,
                        Color = CustomNumbersColor
                    });
                }

                DrawOnScreen("v => volume \n ts => time");
            }

        }

        private double CustomMAs(double seriesValue, int index, int maPeriod, MAType_Data maType, SourceSwitch sourceSwitch, bool isStdDev = false) 
        {
            Dictionary<int, double> buffer = sourceSwitch switch {
                SourceSwitch.Time => _customBuffer.Time,
                _ => _customBuffer.Volume
            };
            Dictionary<int, double> prevMA_Dict = sourceSwitch switch {
                SourceSwitch.Time => _customBuffer.MATime,
                _ => _customBuffer.MAVolume
            };

            if (!buffer.ContainsKey(index))
                buffer.Add(index, seriesValue);
            else
                buffer[index] = seriesValue;

            double maValue = maType switch
            {
                MAType_Data.Simple => CustomMA.SMA(index, maPeriod, buffer),
                MAType_Data.Exponential => CustomMA.EMA(index, maPeriod, buffer, prevMA_Dict),
                MAType_Data.Weighted => CustomMA.WMA(index, maPeriod, buffer),
                MAType_Data.Triangular => CustomMA.TMA(index, maPeriod, buffer),
                MAType_Data.Hull => CustomMA.Hull(index, maPeriod, buffer),
                MAType_Data.VIDYA => CustomMA.VIDYA(index, maPeriod, buffer, prevMA_Dict),
                MAType_Data.WilderSmoothing => CustomMA.Wilder(index, maPeriod, buffer, prevMA_Dict),
                MAType_Data.KaufmanAdaptive => CustomMA.KAMA(index, maPeriod, 2, 30, buffer, prevMA_Dict),
                _ => double.NaN
            };

            return isStdDev ? CustomMA.StdDev(index, maPeriod, maValue, buffer) : maValue;
        }

        private string FormatBigNumber(double num)
        {
            if (double.IsNaN(num) || num.ToString().Length == 1)
                return num.ToString();

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

        private static string[] GetTimeLapse(double interval_ms)
        {
            // Dynamic TimeLapse Format
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

        // *********** VOLUME RENKO/RANGE ***********
        /*
            Original source code by srlcarlg (me) (https://ctrader.com/algos/indicators/show/3045)
            Uses Ticks Data to make the calculation of volume, just like Candles.

            Refactored in Order Flow Ticks v2.0 revision 1.5
            Improved in Order Flow Aggregated v2.0
        */
    }
}
