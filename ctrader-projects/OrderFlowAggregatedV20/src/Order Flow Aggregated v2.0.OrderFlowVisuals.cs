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
        private Dictionary<double, double> CreateSpikeProfile(int iStart)
        {
            if (!SpikeFilterParams.EnableSpikeFilter)
                return new();

            // Segments_Bar already sorted
            double[] validSegments = Segments_Bar.Where(key => VolumesRank.ContainsKey(key)).ToArray();

            double[] absProfile = validSegments.Select(key => (double)Math.Abs(DeltaRank[key])).ToArray();
            double[] normProfile = Array.Empty<double>();

            IndicatorDataSeries sourceSeries = SpikeFilterParams.SpikeSource_Input switch {
                SpikeSource_Data.Delta_BuySell_Sum => DeltaBuySell_Sum_Series,
                SpikeSource_Data.Sum_Delta => SumDelta_Series,
                _ => Dynamic_Series
            };
            double sourceValue = sourceSeries[iStart];

            DeltaSwitch deltaSwitch = SpikeFilterParams.SpikeSource_Input switch {
                SpikeSource_Data.Delta_BuySell_Sum => DeltaSwitch.DeltaBuySell_Sum,
                SpikeSource_Data.Sum_Delta => DeltaSwitch.Sum,
                _ => DeltaSwitch.None
            };

            double filterValue = 0;
            switch (SpikeFilterParams.SpikeFilter_Input)
            {
                case SpikeFilter_Data.MA:
                {
                    if (UseCustomMAs)
                        filterValue = CustomMAs(sourceValue, iStart, SpikeFilterParams.MAperiod, CustomMAType.Spike, deltaSwitch, MASwitch.Spike, false);
                    else
                        filterValue = SpikeFilterParams.SpikeSource_Input switch {
                            SpikeSource_Data.Delta_BuySell_Sum => MASpike_DeltaBuySell_Sum.Result[iStart],
                            SpikeSource_Data.Sum_Delta => MASpike_SumDelta.Result[iStart],
                            _ => MASpike_Delta.Result[iStart]
                        };
                    break;
                }
                case SpikeFilter_Data.Standard_Deviation:
                {
                    if (UseCustomMAs)
                        filterValue = CustomMAs(sourceValue, iStart, SpikeFilterParams.MAperiod, CustomMAType.Spike, deltaSwitch, MASwitch.Spike, true);
                    else
                        filterValue = SpikeFilterParams.SpikeSource_Input switch {
                            SpikeSource_Data.Delta_BuySell_Sum => StdDevSpike_DeltaBuySell_Sum.Result[iStart],
                            SpikeSource_Data.Sum_Delta => StdDevSpike_SumDelta.Result[iStart],
                            _ => StdDevSpike_Delta.Result[iStart]
                        };
                    break;
                }
                case SpikeFilter_Data.L1Norm:
                {
                    // Filter on Results
                    double[] window = new double[SpikeFilterParams.MAperiod];

                    for (int k = 0; k < SpikeFilterParams.MAperiod; k++)
                        window[k] = sourceSeries[iStart - SpikeFilterParams.MAperiod + 1 + k];

                    filterValue = Filters.L1Norm_Strength(window);
                    filterValue *= 100;

                    // Filter on Profile
                    normProfile = Filters.L1Norm_Profile(absProfile);
                    break;
                }
                case SpikeFilter_Data.SoftMax_Power:
                {
                    // Filter on Results
                    double[] window = new double[SpikeFilterParams.MAperiod];

                    for (int k = 0; k < SpikeFilterParams.MAperiod; k++)
                        window[k] = sourceSeries[iStart - SpikeFilterParams.MAperiod + 1 + k];

                    filterValue = Filters.PowerSoftmax_Strength(window);
                    filterValue *= 100;

                    // Filter on Profile
                    normProfile = Filters.PowerSoftmax_Profile(absProfile);
                    break;
                }
            }

            // Required for [L1Norm, SoftMax_Power]
            int normLength = normProfile.Length;
            if (normLength > 0)
            {
                double[] filterProfile = new double[normLength];
                for (int k = 0; k < normLength; k++)
                    filterProfile[k] = Math.Round(normProfile[k] * 100, 2);

                normProfile = filterProfile;
            }

            // Final step => rowStrength
            double[] whichProfile = normLength > 0 ? normProfile : absProfile;
            int length = whichProfile.Length;

            double[] strengthProfile = new double[length];
            for (int k = 0; k < length; k++)
            {
                double rowStrength = Math.Abs(whichProfile[k] / filterValue);
                strengthProfile[k] = Math.Round(rowStrength, 2);
            }

            if (SpikeRatioParams.SpikeRatio_Input == SpikeRatio_Data.Percentage)
            {
                // From srl-python-indicators/order_flow_ticks.py
                /*
                   simple math, normalize the values to 0~1, just:
                       - calculate the sum of all elements absolute value
                       - divide each element by the sum
                       - aka L1 normalization
                   added MA to get the values >= 100%, as well as, percentile-like behavior of bubbles chart.
                */
                double sumTotal = strengthProfile.Sum();
                PercentageRatio_Series[iStart] = sumTotal;

                double maTotal = UseCustomMAs ?
                                 CustomMAs(sumTotal, iStart, SpikeRatioParams.MAperiod_PctSpike, CustomMAType.SpikePctRatio, DeltaSwitch.Spike_PctRatio, MASwitch.Spike, false) :
                                 MARatio_Percentage.Result[iStart];

                for (int k = 0; k < length; k++)
                {
                    double rowStrength_Pct = strengthProfile[k] / maTotal;
                    strengthProfile[k] = Math.Round(rowStrength_Pct * 100, 1);
                }
            }
            
            Dictionary<double, double> dict = new();
            for (int i = 0; i < validSegments.Length; i++)
                dict[validSegments[i]] = strengthProfile[i];
                
            return dict;
        }

        // Spike Levels
        private void CreateRect_Spike(double p1, double p2, int index, int i, string spikeDictKey, Color color)
        {
            ChartRectangle rectangle = Chart.DrawRectangle(
                $"{index}_{i}_SpikeLevelRectangle",
                Bars.OpenTimes[index],
                p1,
                Bars.OpenTimes[index + 1],
                p2,
                Color.FromArgb(80, color),
                1,
                LineStyle.Solid
            );
            rectangle.IsFilled = MiscParams.FillHist;

            ChartText label = null;
            if (SpikeLevels_ShowValue)
            {
                label = Chart.DrawText(
                    $"{index}_{i}_SpikeLevelText",
                    "0",
                    Bars.OpenTimes[index],
                    (p1 + p2) / 2,
                    Color.Yellow
                );
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Center;
                label.FontSize = FontSizeResults;
            }

            RectInfo rectangleInfo = new()
            {
                Rectangle = rectangle,
                Text = label,
                Touches = 0,
                Y1 = p1,
                Y2 = p2,
                isActive = true,
                // Real-time Market
                // The current bar Spike Rectangle should not be used.
                LastBarIndex = !IsLastBar ? -1 : index,
            };

            if (spikeRectangles.ContainsKey(spikeDictKey))
                spikeRectangles[spikeDictKey] = rectangleInfo;
            else
                spikeRectangles.Add(spikeDictKey, rectangleInfo);
        }
        private static void UpdateLabel_Spike(RectInfo rect, DateTime time)
        {
            rect.Text.Text = $"{rect.Touches}";
            rect.Text.Time = time;
        }
        private static bool TouchesRect_Spike(double o, double h, double l, double c, double top, double bottom)
        {
            // If any OHLC inside rectangle
            if ((o >= bottom && o <= top) ||
                (h >= bottom && h <= top) ||
                (l >= bottom && l <= top) ||
                (c >= bottom && c <= top))
                return true;

            // If bar fully crosses rectangle (high above and low below)
            if (h >= top && l <= bottom)
                return true;

            return false;
        }

        // Ultra Bubbles Levels
        private void CreateRect_Bubbles(double p1, double p2, int index, Color color)
        {
            ChartRectangle rectangle = Chart.DrawRectangle(
                $"{index}_UltraBubbleRectangle",
                Bars.OpenTimes[index],
                p1,
                Bars.OpenTimes[index + 1],
                p2,
                Color.FromArgb(80, color),
                1,
                LineStyle.Solid
            );
            rectangle.IsFilled = MiscParams.FillHist;

            ChartText label = null;
            if (UltraBubbles_ShowValue)
            {
                label = Chart.DrawText(
                    $"{index}_UltraBubbleText",
                    "0",
                    Bars.OpenTimes[index],
                    p2,
                    Color.Yellow
                );
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.FontSize = FontSizeResults;
            }

            RectInfo rectangleInfo = new()
            {
                Rectangle = rectangle,
                Text = label,
                Touches = 0,
                Y1 = p1,
                Y2 = p2,
                isActive = true
            };

            if (ultraRectangles.ContainsKey(index))
                ultraRectangles[index] = rectangleInfo;
            else
                ultraRectangles.Add(index, rectangleInfo);

        }
        private static bool TouchesRect_Bubbles(double o, double h, double l, double c, double top, double bottom, UltraBubblesBreak_Data selectedBreak)
        {
            if (selectedBreak == UltraBubblesBreak_Data.Close_Only || selectedBreak == UltraBubblesBreak_Data.Close_plus_BarBody)
            {
                if (o >= bottom && o <= top)
                    return true;

                if (selectedBreak == UltraBubblesBreak_Data.Close_plus_BarBody)
                {
                    // If bar fully crosses rectangle (high above and low below)
                    if (h > top && l < bottom)
                        return true;
                }
            }
            else if (selectedBreak == UltraBubblesBreak_Data.OHLC_plus_BarBody)
            {
                // If any OHLC inside rectangle
                if ((o >= bottom && o <= top) ||
                    (h >= bottom && h <= top) ||
                    (l >= bottom && l <= top) ||
                    (c >= bottom && c <= top))
                    return true;

                // If bar fully crosses rectangle (high above and low below)
                if (h > top && l < bottom)
                    return true;
            }

            return false;
        }
        private static void UpdateLabel_Bubbles(RectInfo rect, double top, DateTime time)
        {
            rect.Text.Text = $"{rect.Touches}";
            rect.Text.Time = time;
            rect.Text.Y = top;
        }

        private double CustomMAs(double seriesValue, int index, int maPeriod,
                                 MAType_Data maType, DeltaSwitch deltaSwitch = DeltaSwitch.None, MASwitch maSwitch = MASwitch.Large,
                                 bool isStdDev = false
                                )
        {
            Dictionary<int, double> buffer = deltaSwitch switch {
                DeltaSwitch.DeltaChange => _deltaBuffer.Change,
                DeltaSwitch.DeltaBuySell_Sum => _deltaBuffer.BuySell_Sum,
                DeltaSwitch.Subtract => _deltaBuffer.Subtract,
                DeltaSwitch.Sum => _deltaBuffer.Sum,
                DeltaSwitch.Spike_PctRatio => _deltaBuffer.Spike_PctRatio,
                _ => _dynamicBuffer
            };

            if (!buffer.ContainsKey(index))
                buffer.Add(index, seriesValue);
            else
                buffer[index] = seriesValue;

            Dictionary<int, double> prevMA_Dict = maSwitch switch
            {
                MASwitch.Bubbles => deltaSwitch switch {
                    DeltaSwitch.DeltaChange => _deltaBuffer.MAChange_Bubbles,
                    DeltaSwitch.DeltaBuySell_Sum => _deltaBuffer.MABuySellSum_Bubbles,
                    DeltaSwitch.Subtract => _deltaBuffer.MASubtract_Bubbles,
                    DeltaSwitch.Sum => _deltaBuffer.MASum_Bubbles,
                    _ => _maDynamic
                },
                MASwitch.Spike => deltaSwitch switch {
                    DeltaSwitch.DeltaBuySell_Sum => _deltaBuffer.MABuySellSum_Spike,
                    DeltaSwitch.Sum => _deltaBuffer.MASum_Spike,
                    DeltaSwitch.Spike_PctRatio => _deltaBuffer.MASpike_PctRatio,
                    _ => _maDynamic
                },
                // Large
                _ => deltaSwitch switch {
                    DeltaSwitch.Subtract => _deltaBuffer.MASubtract_Large,
                    _ => _maDynamic
                }
            };

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

        // *********** INTERVAL SEGMENTS ***********
        /*
            In order to optimize Volume Profile and reduce CPU worload
            as well as create the possiblity to:
                - See Weekly and/or Monthly "Intraday" Profile
                - use Aligned Segments at Higher Timeframes (D1 to D3)
            Segments will be calculated outside VolumeProfile()
            and updated at new High/Low of its interval [D1, W1, M1]
        */
    }
}
