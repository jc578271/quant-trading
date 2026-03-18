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
        private void WeisWaveAnalysis(int rawIndex)
        {
            int index = rawIndex - 1;

            if (index < 2)
                return;

            if (WavesParams.WavesMode_Input == WavesMode_Data.Reversal && BooleanUtils.isRenkoChart) {
                if (IsLastBar) // IsLastBar=false at each new BarOpened
                    return;
                bool isUp = Bars.ClosePrices[index] > Bars.OpenPrices[index];

                if (WavesParams.ShowCurrentWave)
                    CalculateWaves(isUp ? Direction.UP : Direction.DOWN, trendStartIndex, index, false);

                if (ShowTrendLines) {
                    ChartTrendLine trendLine = Chart.DrawTrendLine($"TrendLine_{trendStartIndex}",
                                   trendStartIndex, Bars.OpenPrices[trendStartIndex],
                                   index, Bars.OpenPrices[index], isUp ? UpLineColor : DownLineColor);
                    trendLine.Thickness = TrendThickness;
                }

                if (!Reversal_DirectionChanged(index))
                    return;

                CalculateWaves(isUp ? Direction.UP : Direction.DOWN, trendStartIndex, index, true);

                if (ShowTrendLines) {
                    ChartTrendLine trendLine = Chart.DrawTrendLine($"TrendLine_NO{index}",
                                               index, Bars.OpenPrices[index],
                                               index + 1, Bars.OpenPrices[index], NoTrendColor);
                    trendLine.Thickness = TrendThickness;
                }

                trendStartIndex = index + 1;
            }
            else
                ZigZag(index);
        }
        private bool Reversal_DirectionChanged(int index)
        {
            bool isUp = Bars.ClosePrices[index] > Bars.OpenPrices[index];

            bool prevIsUp = Bars.ClosePrices[index - 1] > Bars.OpenPrices[index - 1];
            bool nextIsUp = Bars.ClosePrices[index + 1] > Bars.OpenPrices[index + 1];
            bool prevIsDown = Bars.ClosePrices[index - 1] < Bars.OpenPrices[index - 1];
            bool nextIsDown = Bars.ClosePrices[index + 1] < Bars.OpenPrices[index + 1];

            return prevIsUp && isUp && nextIsDown || prevIsDown && isUp && nextIsDown ||
                   prevIsDown && !isUp && nextIsUp || prevIsUp && !isUp && nextIsUp;
        }

        private bool ZigZag_DirectionChanged(int index, double low, double high, double prevLow, double prevHigh)
        {
            switch (ZigZagParams.ZigZagMode_Input)
            {
                case ZigZagMode_Data.Percentage:
                    if (ZigZagObjs.direction == Direction.DOWN)
                        return high >= ZigZagObjs.extremumPrice * (1.0 + ZigZagParams.PercentageZZ * 0.01);
                    else
                        return low <= ZigZagObjs.extremumPrice * (1.0 - ZigZagParams.PercentageZZ * 0.01);
                case ZigZagMode_Data.NoLag_HighLow:
                    bool bothIsPivot = high > prevHigh && low < prevLow;
                    bool highIsPivot = high > prevHigh && low >= prevLow;
                    bool lowIsPivot = low < prevLow && high <= prevHigh;
                    if (bothIsPivot)
                        return false;
                    return ZigZagObjs.direction == Direction.UP ? lowIsPivot : highIsPivot;
                default:
                    bool isATR = ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.ATR;
                    double value = isATR ? (_ATR.Result[index] * ATR_Multiplier) : (ZigZagParams.PipsZZ * Symbol.PipSize);
                    if (ZigZagObjs.direction == Direction.DOWN)
                        return Math.Abs(ZigZagObjs.extremumPrice - high) >= value;
                    else
                        return Math.Abs(low - ZigZagObjs.extremumPrice) >= value;
            }
        }
        private void ZigZag(int index) {
            double prevHigh = Bars.HighPrices[index - 1];
            double prevLow = Bars.LowPrices[index - 1];
            double high = Bars.HighPrices[index];
            double low = Bars.LowPrices[index];
            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                DateTime prevBarDate = Bars.OpenTimes[index - 1];
                DateTime barDate = Bars.OpenTimes[index];

                int TF_PrevIdx = MTFSource_Bars.OpenTimes.GetIndexByTime(prevBarDate);
                int TF_idx = MTFSource_Bars.OpenTimes.GetIndexByTime(barDate);

                prevHigh = MTFSource_Bars.HighPrices[TF_PrevIdx];
                prevLow = MTFSource_Bars.LowPrices[TF_PrevIdx];
                high = MTFSource_Bars.HighPrices[TF_idx];
                low = MTFSource_Bars.LowPrices[TF_idx];
            }

            if (ZigZagObjs.extremumPrice == 0) {
                ZigZagObjs.extremumPrice = high;
                ZigZagObjs.extremumIndex = index;
            }

            if (ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.NoLag_HighLow && ZigZagParams.Priority_Input != Priority_Data.None && !BooleanUtils.isPriceBased_Chart) {
                if (NoLag_BothIsPivot(index, low, high, prevLow, prevHigh) || ZigZagParams.Priority_Input == Priority_Data.Skip)
                    return;
            }
            bool directionChanged = ZigZag_DirectionChanged(index, low, high, prevLow, prevHigh);
            if (ZigZagObjs.direction == Direction.DOWN)
            {
                if (low <= ZigZagObjs.extremumPrice)
                    MoveExtremum(index, low);
                else if (directionChanged) {
                    SetExtremum(index, high, false);
                    ZigZagObjs.direction = Direction.UP;
                }
            }
            else
            {
                if (high >= ZigZagObjs.extremumPrice)
                    MoveExtremum(index, high);
                else if (directionChanged) {
                    SetExtremum(index, low, false);
                    ZigZagObjs.direction = Direction.DOWN;
                }
            }
        }
        private void MoveExtremum(int index, double price)
        {
            if (!ShowTrendLines)
                ZigZagBuffer[ZigZagObjs.extremumIndex] = double.NaN;
            SetExtremum(index, price, true);
        }
        private void SetExtremum(int index, double price, bool isMove)
        {
            if (!isMove) {
                // End of direction
                CalculateWaves(ZigZagObjs.direction, trendStartIndex, ZigZagObjs.extremumIndex, true);
                trendStartIndex = ZigZagObjs.extremumIndex + 1;

                DateTime extremeDate = Bars[ZigZagObjs.extremumIndex].OpenTime;
                double extremePrice = ZigZagObjs.direction == Direction.UP ? Bars[ZigZagObjs.extremumIndex].High : Bars[ZigZagObjs.extremumIndex].Low;
                if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                    int TF_idx = MTFSource_Bars.OpenTimes.GetIndexByTime(extremeDate);
                    extremePrice = ZigZagObjs.direction == Direction.UP ? MTFSource_Bars[TF_idx].High : MTFSource_Bars[TF_idx].Low;
                }
                if (ZigZagParams.ShowTurningPoint) {
                    Color turningColor = InvertTurningColor ?
                                        (ZigZagObjs.direction == Direction.UP ? DownLineColor : UpLineColor) :
                                        (ZigZagObjs.direction == Direction.UP ? UpLineColor : DownLineColor);

                    Chart.DrawTrendLine($"{ZigZagObjs.extremumIndex}_horizontal",
                                        extremeDate,
                                        extremePrice,
                                        Bars[index].OpenTime,
                                        extremePrice, turningColor);
                    Chart.DrawTrendLine($"{ZigZagObjs.extremumIndex}_vertical",
                                        Bars[index].OpenTime,
                                        extremePrice,
                                        Bars[index].OpenTime,
                                        ZigZagObjs.direction == Direction.UP ? Bars[index].High : Bars[index].Low, turningColor);
                }

                if (ShowTrendLines) {
                    PrevWave_TrendLine.LineStyle = LineStyle.Solid;
                    if (isLargeWave_EvsR && ShowYellowTrendLines)
                        PrevWave_TrendLine.Color = LargeWaveColor;

                    Color lineColor = ColorfulTrendLines ?
                                      (ZigZagObjs.direction == Direction.UP ? DownLineColor : UpLineColor) :
                                      NoTrendColor;
                    double trendEndPrice = ZigZagObjs.direction == Direction.UP ? Bars[index].Low : Bars[index].High;
                    PrevWave_TrendLine = Chart.DrawTrendLine($"TrendLine_{trendStartIndex}",
                                                            extremeDate,
                                                            extremePrice,
                                                            Bars[index].OpenTime,
                                                            trendEndPrice, lineColor);
                    PrevWave_TrendLine.Thickness = TrendThickness;
                }
            }
            else if (isMove && WavesParams.ShowCurrentWave)
                CalculateWaves(ZigZagObjs.direction, trendStartIndex, ZigZagObjs.extremumIndex, false);

            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF && isMove) {
                // Workaround to remove the behavior of shift(nº) when moving the extremum at custom timeframe price source
                double extremePrice = ZigZagObjs.direction == Direction.UP ? Bars[ZigZagObjs.extremumIndex].High : Bars[ZigZagObjs.extremumIndex].Low;
                double currentPrice = ZigZagObjs.direction == Direction.UP ? Bars[index].High : Bars[index].Low;
                bool condition = ZigZagObjs.direction == Direction.UP ? currentPrice <= extremePrice : currentPrice >= extremePrice;
                ZigZagObjs.extremumIndex = condition ? ZigZagObjs.extremumIndex : index;
            }
            else
                ZigZagObjs.extremumIndex = index;

            ZigZagObjs.extremumPrice = price;

            if (!ShowTrendLines)
                ZigZagBuffer[ZigZagObjs.extremumIndex] = ZigZagObjs.extremumPrice;

            if (isMove)
                MovingTrendLine(Bars[ZigZagObjs.extremumIndex].OpenTime, price);
        }

        private void MovingTrendLine(DateTime endDate, double endPrice)
        {
            if (ShowTrendLines)
            {
                int startIndex = trendStartIndex - 1;
                // Yeah... index jumps are quite annoying to debug.
                try { _ = Bars[startIndex].OpenTime; } catch { startIndex = trendStartIndex; }

                DateTime startDate = Bars[startIndex].OpenTime;
                double startPrice = ZigZagObjs.direction == Direction.UP ? Bars[startIndex].Low : Bars[startIndex].High;

                Color lineColor = ColorfulTrendLines ? (ZigZagObjs.direction == Direction.UP ? DownLineColor : UpLineColor) : NoTrendColor;
                PrevWave_TrendLine = Chart.DrawTrendLine($"TrendLine_{trendStartIndex}",
                                     startDate,
                                     startPrice,
                                     endDate,
                                     endPrice, lineColor);
                PrevWave_TrendLine.Thickness = TrendThickness;
                PrevWave_TrendLine.LineStyle = LineStyle.Dots;
            }
        }
        private bool NoLag_BothIsPivot(int  index, double low, double high, double prevLow, double prevHigh) {
            bool bothIsPivot = high > prevHigh && low < prevLow;
            if (!bothIsPivot || ZigZagParams.Priority_Input != Priority_Data.Auto)
                return false;

            bool HighIsFirst = AutoPriority(index, prevLow, prevHigh, low, high);
            if (HighIsFirst) {
                // Chart.DrawText($"{index}_First", "First(High)", Bars[index].OpenTime, high, Color.White);
                // Chart.DrawText($"{index}_Last", "Last(Low)", Bars[index].OpenTime, low, Color.White);
                // Chart.DrawText($"{index}_DIRECTION", direction.ToString(), Bars[index].OpenTime, Bars.OpenPrices[index], Color.White);
                if (ZigZagObjs.direction == Direction.UP)
                {
                    // Fix => C# version was using ZigZagBuffer['extremumIndex'] instead of ZigZagBuffer['index'],
                    if (high > ZigZagObjs.extremumPrice && !ShowTrendLines)
                        ZigZagBuffer[index] = high;

                    SetExtremum(index, low, true);
                    ZigZagObjs.direction = Direction.DOWN;
                }
            }
            else {
                // Chart.DrawText($"{index}_First", "First(Low)", Bars[index].OpenTime, low, Color.White);
                // Chart.DrawText($"{index}_Last", "Last(High)", Bars[index].OpenTime, high, Color.White);
                // Chart.DrawText($"{index}_DIRECTION", direction.ToString(), Bars[index].OpenTime, Bars.OpenPrices[index], Color.White);
                if (ZigZagObjs.direction == Direction.DOWN)
                {
                    if (low < ZigZagObjs.extremumPrice && !ShowTrendLines)
                        ZigZagBuffer[index] = low;

                    SetExtremum(index, high, true);
                    ZigZagObjs.direction = Direction.UP;
                }
            }

            return true;
        }

        private bool AutoPriority(int index, double prevLow, double prevHigh, double low, double high)
        {
            DateTime barStart = Bars.OpenTimes[index];
            DateTime barEnd = Bars.OpenTimes[index + 1];
            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                int TF_idxStart = MTFSource_Bars.OpenTimes.GetIndexByTime(barStart);
                int TF_idxEnd = MTFSource_Bars.OpenTimes.GetIndexByTime(barEnd);

                barStart = MTFSource_Bars.OpenTimes[TF_idxStart];
                barEnd = MTFSource_Bars.OpenTimes[TF_idxEnd];
            }
            if (IsLastBar)
                barEnd = _m1Bars.LastBar.OpenTime;

            bool firstIsHigh = false;
            bool atLeastOne = false;

            int startM1 = _m1Bars.OpenTimes.GetIndexByTime(barStart);
            for (int i = startM1; i < _m1Bars.OpenTimes.Count; i++)
            {
                if (_m1Bars.OpenTimes[i] > barEnd)
                    break;

                if (_m1Bars.HighPrices[i] > prevHigh) {
                    firstIsHigh = true;
                    atLeastOne = true;
                    break;
                }
                // Fix => C# version sets True for first_is_high
                if (_m1Bars.LowPrices[i] < prevLow) {
                    firstIsHigh = false;
                    atLeastOne = true;
                    break;
                }
            }

            if (!atLeastOne) {
                double subtHigh = Math.Abs(high - prevHigh);
                double subtLow = Math.Abs(prevLow - low);
                return subtHigh >= subtLow;
            }

            return firstIsHigh;
        }

        private double GetY1_Waves(int extremeIndex) {
            if (WavesParams.WavesMode_Input == WavesMode_Data.Reversal && BooleanUtils.isRenkoChart)
                return Bars.ClosePrices[extremeIndex];

            DateTime extremeDate = Bars[extremeIndex].OpenTime;
            double extremePrice = ZigZagObjs.direction == Direction.UP ? Bars[extremeIndex].High : Bars[extremeIndex].Low;
            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                int TF_idx = MTFSource_Bars.OpenTimes.GetIndexByTime(extremeDate);
                extremePrice = ZigZagObjs.direction == Direction.UP ? MTFSource_Bars[TF_idx].High : MTFSource_Bars[TF_idx].Low;
            }
            return extremePrice;
        }

        private void CalculateWaves(Direction direction, int firstCandleIdx, int lastCandleIdx, bool directionChanged = false)
        {
            double cumulVolume()
            {
                double volume = 0.0;
                for (int i = firstCandleIdx; i <= lastCandleIdx; i++)
                    volume += VolumeSeries[i];

                return volume;
            }
            double cumulRenko()
            {
                double renkoCount = 0;
                for (int i = firstCandleIdx; i <= lastCandleIdx; i++)
                    renkoCount += 1;

                return renkoCount;
            }
            double cumulativePrice(bool isUp)
            {
                double price;
                if (isUp)
                    price = Bars.HighPrices[lastCandleIdx] - Bars.LowPrices[firstCandleIdx];
                else
                    price = Bars.HighPrices[firstCandleIdx] - Bars.LowPrices[lastCandleIdx];

                if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF && WavesParams.WavesMode_Input == WavesMode_Data.ZigZag) {
                    int TF_idxLast = MTFSource_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[lastCandleIdx]);
                    int TF_idxFirst = MTFSource_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstCandleIdx]);
                    if (isUp)
                        price = MTFSource_Bars.HighPrices[TF_idxLast] - MTFSource_Bars.LowPrices[TF_idxFirst];
                    else
                        price = MTFSource_Bars.HighPrices[TF_idxFirst] - MTFSource_Bars.LowPrices[TF_idxLast];
                }
                price /= Symbol.PipSize;

                return Math.Round(price, 2);
            }
            double cumulativeTime()
            {
                DateTime openTime = Bars.OpenTimes[firstCandleIdx];
                DateTime closeTime = Bars.OpenTimes[lastCandleIdx + 1];
                TimeSpan interval = closeTime.Subtract(openTime);
                double interval_ms = interval.TotalMilliseconds;
                return interval_ms;
            }
            bool directionIsUp = direction == Direction.UP;
            if (WavesParams.ShowWaves_Input == ShowWaves_Data.No)
            {
                // Other Waves
                if (!WavesParams.ShowCurrentWave && directionChanged || WavesParams.ShowCurrentWave)
                    OthersWaves(directionIsUp);
                return;
            }

            double cumlVolume = cumulVolume();
            double cumlRenkoOrPrice = BooleanUtils.isRenkoChart ? cumulRenko() : cumulativePrice(directionIsUp);
            double cumlVolPrice = Math.Round(cumlVolume / cumlRenkoOrPrice, 1);

            _expCumulVolume = cumlVolume;
            _expCumulPrice = cumlRenkoOrPrice;
            _expCumulVolPrice = cumlVolPrice;
            _expWaveDirection = directionIsUp ? "Up" : "Down";

            // Standard Waves
            if (!WavesParams.ShowCurrentWave && directionChanged || WavesParams.ShowCurrentWave) {
                EvsR_Analysis(cumlVolPrice, directionChanged, directionIsUp);
                WW_Analysis(cumlVolume, directionChanged, directionIsUp);
            }
            // Other Waves
            if (!WavesParams.ShowCurrentWave && directionChanged || WavesParams.ShowCurrentWave)
                OthersWaves(directionIsUp);

            // Prev Waves Analysis
            if (directionIsUp) {
                bool prevIsDown = Bars.ClosePrices[lastCandleIdx - 1] < Bars.OpenPrices[lastCandleIdx - 1];
                bool nextIsDown = Bars.ClosePrices[lastCandleIdx + 1] < Bars.OpenPrices[lastCandleIdx + 1];
                // Set Previous Bullish Wave Accumulated
                SetPrevWaves(cumlVolume, cumlVolPrice, prevIsDown, nextIsDown, true, directionChanged);
            } else {
                bool prevIsUp = Bars.ClosePrices[lastCandleIdx - 1] > Bars.OpenPrices[lastCandleIdx - 1];
                bool nextIsUp = Bars.ClosePrices[lastCandleIdx + 1] > Bars.OpenPrices[lastCandleIdx + 1];
                // Set Previous Downish Wave Accumulated
                SetPrevWaves(cumlVolume, cumlVolPrice, prevIsUp, nextIsUp, false, directionChanged);
            }

            void OthersWaves(bool isUp)
            {
                if (WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.No)
                    return;

                double cumulPrice = cumulativePrice(isUp);
                string cumulPriceFmtd = cumulPrice > 1000 ? FormatBigNumber(cumulPrice) : cumulPrice.ToString();
                double cumlTime = cumulativeTime();

                if (cumlTime == 0 || double.IsNaN(cumlTime))
                    return;

                string[] interval_timelapse = GetTimeLapse(cumlTime);

                ShowWaves_Data selectedWave = WavesParams.ShowWaves_Input;
                double timelapse_Value = Convert.ToDouble(interval_timelapse[0]);
                string timelapseString = Math.Round(timelapse_Value) + interval_timelapse[1];

                string waveInfo;
                if (isUp)
                {
                    string spacingUp = WyckoffParams.NumbersPosition_Input switch {
                        NumbersPosition_Data.Outside => selectedWave switch {
                            ShowWaves_Data.No => "\n\n",
                            ShowWaves_Data.Both => "\n\n\n\n",
                            _ => "\n\n\n"
                        },
                        _ => selectedWave switch {
                            ShowWaves_Data.No => "",
                            ShowWaves_Data.Both => "\n\n\n",
                            _ => "\n\n"
                        },
                    };
                    string sourceWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? cumulPriceFmtd : timelapseString;
                    string suffixWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? "p" : "";
                    
                    waveInfo = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Both ?
                                $"{timelapseString} ⎪ {cumulPriceFmtd}p{spacingUp}" :
                                $"{sourceWave}{suffixWave}{spacingUp}";
                }
                else
                {
                    string spacingDown = WyckoffParams.NumbersPosition_Input switch {
                        NumbersPosition_Data.Outside => selectedWave switch {
                            ShowWaves_Data.No => "\n",
                            ShowWaves_Data.Both => "\n\n\n",
                            _ => "\n\n"
                        },
                        _ => selectedWave switch {
                            ShowWaves_Data.No => "",
                            ShowWaves_Data.Both => "\n\n",
                            _ => "\n"
                        },
                    };

                    string sourceWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? cumulPriceFmtd : timelapseString;
                    string suffixWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? "p" : "";

                    waveInfo = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Both ?
                                $"{spacingDown}{timelapseString} ⎪ {cumulPriceFmtd}p" :
                                $"{spacingDown}{sourceWave}{suffixWave}";
                }

                double y1 = GetY1_Waves(lastCandleIdx);
                DrawOrCache(new DrawInfo
                {
                    BarIndex = lastCandleIdx,
                    Type = DrawType.Text,
                    Id = $"{firstCandleIdx}_WavesMisc",
                    Text = waveInfo,
                    X1 = Bars.OpenTimes[lastCandleIdx],
                    Y1 = y1,
                    horizontalAlignment = HorizontalAlignment.Center,
                    verticalAlignment = isUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    FontSize = FontSizeWaves,
                    Color = isUp ? UpWaveColor : DownWaveColor
                });
            }

            void WW_Analysis(double cumlVolume, bool endWave, bool isUp)
            {
                if (WavesParams.ShowWaves_Input == ShowWaves_Data.No || WavesParams.ShowWaves_Input == ShowWaves_Data.EffortvsResult)
                    return;
                string leftMark = "";
                string rightMark = "";
                string volFmtd = FormatBigNumber(cumlVolume);

                string waveInfo;
                if (isUp)
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolume > prevWave_Up[0] ? "⮝" : "⮟"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolume > prevWave_Down[0] ? "🡩" : "🡫"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolume > prevWave_Up[0] ? "⮝" : "⮟";
                            rightMark = cumlVolume > prevWave_Down[0] ? "" : leftMark == "⮟" ? "" : "🡫";
                            break;
                        default: break;
                    }
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                     "\n\n" :
                                     "";
                    
                    waveInfo = $"({leftMark}{volFmtd}{rightMark}){spacing}";
                }
                else
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolume > prevWave_Down[0] ? "⮟" : "⮝"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolume > prevWave_Up[0] ? "🡫" : "🡩"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolume > prevWave_Down[0] ? "⮟" : "⮝";
                            rightMark = cumlVolume > prevWave_Up[0] ? "" : leftMark == "⮝" ? "" : "🡩";
                            break;
                        default: break;
                    }
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                     "\n" :
                                     "";
                    waveInfo = $"{spacing}({leftMark}{volFmtd}{rightMark})";
                }

                double y1 = GetY1_Waves(lastCandleIdx);
                bool largeVol = endWave && Volume_Large();
                Color waveColor = largeVol ? LargeWaveColor : (isUp ? UpWaveColor : DownWaveColor);

                if (ShowRatioValue) {
                    double ratio = (cumlVolume + prevWaves_Volume[0] + prevWaves_Volume[1] + prevWaves_Volume[2] + prevWaves_Volume[3]) / 5 * WavesParams.WW_Ratio;
                    ratio = Math.Round(ratio, 2);
                    waveInfo = $"{waveInfo} > {ratio}? {cumlVolume > ratio} ";
                }

                DrawOrCache(new DrawInfo
                {
                    BarIndex = lastCandleIdx,
                    Type = DrawType.Text,
                    Id = $"{firstCandleIdx}_WavesVolume",
                    Text = waveInfo,
                    X1 = Bars.OpenTimes[lastCandleIdx],
                    Y1 = y1,
                    horizontalAlignment = HorizontalAlignment.Center,
                    verticalAlignment = isUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    FontSize = FontSizeWaves,
                    Color = waveColor
                });

                bool Volume_Large()
                {
                    bool haveZero = false;
                    foreach (double value in prevWaves_Volume)
                    {
                        if (value == 0) {
                            haveZero = true;
                            break;
                        }
                    }
                    if (haveZero)
                        return false;

                    return (cumlVolume + prevWaves_Volume[0] + prevWaves_Volume[1] + prevWaves_Volume[2] + prevWaves_Volume[3]) / 5 * WavesParams.WW_Ratio < cumlVolume;
                }
            }

            void EvsR_Analysis(double cumlVolPrice, bool endWave, bool isUp)
            {
                if (WavesParams.ShowWaves_Input == ShowWaves_Data.No || WavesParams.ShowWaves_Input == ShowWaves_Data.Volume)
                    return;

                string leftMark = "";
                string rightMark = "";
                string effortFmtd = FormatBigNumber(cumlVolPrice);
                
                string waveInfo;
                if (isUp)
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolPrice > prevWave_Up[1] ? "⮝" : "⮟"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolPrice > prevWave_Down[1] ? "🡩" : "🡫"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolPrice > prevWave_Up[1] ? "⮝" : "⮟";
                            rightMark = cumlVolPrice > prevWave_Down[1] ? "" : leftMark == "⮟" ? "" : "🡫";
                            break;
                        default: break;
                    }
                    bool isBoth =  WavesParams.ShowWaves_Input == ShowWaves_Data.Both;
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                    (isBoth ? "\n\n\n" : "\n\n") :
                                    (isBoth ? "\n\n" : "");
                    
                    waveInfo = $"[{leftMark}{effortFmtd}{rightMark}]{spacing}";
                }
                else
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolPrice > prevWave_Down[1] ? "⮟" : "⮝"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolPrice > prevWave_Up[1] ? "🡫" : "🡩"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolPrice > prevWave_Down[1] ? "⮟" : "⮝";
                            rightMark = cumlVolPrice > prevWave_Up[1] ? "" : leftMark == "⮝" ? "" : "🡩";
                            break;
                        default: break;
                    }
                    
                    bool isBoth =  WavesParams.ShowWaves_Input == ShowWaves_Data.Both;
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                    (isBoth ? "\n\n" : "\n") :
                                    (isBoth ? "\n" : "");
                    
                    waveInfo = $"{spacing}[{leftMark}{effortFmtd}{rightMark}]";
                }

                double y1 = GetY1_Waves(lastCandleIdx);
                bool largeEffort = endWave && EvsR_Large();
                Color waveColor = largeEffort ? LargeWaveColor : (isUp ? UpWaveColor : DownWaveColor);

                if (ShowRatioValue) {
                    double ratio = (cumlVolPrice + prevWaves_EvsR[0] + prevWaves_EvsR[1] + prevWaves_EvsR[2] + prevWaves_EvsR[3]) / 5 * WavesParams.EvsR_Ratio;
                    ratio = Math.Round(ratio, 2);
                    waveInfo = $"{waveInfo} > {ratio}? {cumlVolPrice > ratio}";
                }

                DrawOrCache(new DrawInfo
                {
                    BarIndex = lastCandleIdx,
                    Type = DrawType.Text,
                    Id = $"{firstCandleIdx}_WavesEvsR",
                    Text = waveInfo,
                    X1 = Bars.OpenTimes[lastCandleIdx],
                    Y1 = y1,
                    horizontalAlignment = HorizontalAlignment.Center,
                    verticalAlignment = isUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    FontSize = FontSizeWaves,
                    Color = waveColor
                });

                isLargeWave_EvsR = false;
                if (!largeEffort)
                    return;
                isLargeWave_EvsR = true;

                if (!WyckoffParams.FillBars && !WyckoffParams.KeepOutline) {
                    Chart.SetBarFillColor(lastCandleIdx, Color.Transparent);
                    Chart.SetBarOutlineColor(lastCandleIdx, LargeWaveColor);
                }
                else if (WyckoffParams.FillBars && WyckoffParams.KeepOutline)
                    Chart.SetBarFillColor(lastCandleIdx, LargeWaveColor);
                else if (!WyckoffParams.FillBars && WyckoffParams.KeepOutline)
                    Chart.SetBarFillColor(lastCandleIdx, Color.Transparent);
                else if (WyckoffParams.FillBars && !WyckoffParams.KeepOutline)
                    Chart.SetBarColor(lastCandleIdx, LargeWaveColor);

                // Large EvsR [Yellow]
                bool EvsR_Large()
                {
                    bool haveZero = false;
                    foreach (double value in prevWaves_EvsR)
                    {
                        if (value == 0) {
                            haveZero = true;
                            break;
                        }
                    }
                    if (haveZero)
                        return false;

                    return (cumlVolPrice + prevWaves_EvsR[0] + prevWaves_EvsR[1] + prevWaves_EvsR[2] + prevWaves_EvsR[3]) / 5 * WavesParams.EvsR_Ratio < cumlVolPrice;
                }
            }
        }

        private void SetPrevWaves(double cumlVolume, double cumlVolPrice, bool prevIs_UpDown, bool nextIs_UpDown, bool isUp, bool directionChanged)
        {
            // Exclude the most old wave, keep the 3 others and add current Wave value for most recent Wave
            /*
                The previous "wrongly" implementation turns out to be a good filter,
                with the correct implementation of 5 waves, it gives too many yellow bars.
                Since it's useful, keep it.
            */
            double[] cumul = { cumlVolume, cumlVolPrice };

            if (WavesParams.WavesMode_Input == WavesMode_Data.ZigZag) {
                if (!directionChanged) return;
                setTrend();
                return;
            }

            bool conditionRanging = prevIs_UpDown && directionChanged && nextIs_UpDown;
            bool conditionTrend = !prevIs_UpDown && directionChanged && nextIs_UpDown;

            if (isUp) {
                // (prevIsDown && DirectionChanged && nextIsDown);
                if (conditionRanging)
                    setRanging();
                // (!prevIsDown && DirectionChanged && nextIsDown);
                else if (conditionTrend)
                    setTrend();
            } else {
                // (prevIsUp && DirectionChanged && nextIsUp)
                if (conditionRanging)
                    setRanging();
                // (!prevIsUp && DirectionChanged && nextIsUp);
                else if (conditionTrend)
                    setTrend();
            }

            // Ranging or 1 renko trend pullback
            void setRanging() {
                // Volume Wave Analysis
                double[] newWave_Vol = { prevWaves_Volume[1], prevWaves_Volume[2], prevWaves_Volume[3], cumlVolume };
                prevWaves_Volume = newWave_Vol;

                // Effort vs Result Analysis
                double[] newWave_EvsR = { prevWaves_EvsR[1], prevWaves_EvsR[2], prevWaves_EvsR[3], cumlVolPrice };
                prevWaves_EvsR = newWave_EvsR;

                if (!WavesParams.YellowRenko_IgnoreRanging) {
                    if (isUp) prevWave_Up = cumul;
                    else prevWave_Down = cumul;
                }
            }
            void setTrend() {
                if (isUp) {
                    // Volume Wave Analysis
                    // Fix => C# version is using _prev_wave_down for UsePrev_SameWave condition
                    double volumeValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolume,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Up[0],
                        _ => prevWave_Down[0]
                    };
                    double[] newWave_Vol = { prevWaves_Volume[1], prevWaves_Volume[2], prevWaves_Volume[3], volumeValue };
                    prevWaves_Volume = newWave_Vol;

                    // Effort vs Result Analysis
                    double evsrValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolPrice,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Up[1],
                        _ => prevWave_Down[1]
                    };
                    double[] newWave_EvsR = { prevWaves_EvsR[1], prevWaves_EvsR[2], prevWaves_EvsR[3], evsrValue };
                    prevWaves_EvsR = newWave_EvsR;

                    // Prev Wave
                    prevWave_Up = cumul;
                }
                else {
                    // Volume Wave Analysis
                    double volumeValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolume,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Down[0],
                        _ => prevWave_Up[0]
                    };
                    double[] newWave_Vol = { prevWaves_Volume[1], prevWaves_Volume[2], prevWaves_Volume[3], volumeValue };
                    prevWaves_Volume = newWave_Vol;

                    // Effort vs Result Analysis
                    double evsrValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolPrice,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Down[1],
                        _ => prevWave_Up[1]
                    };
                    double[] newWave_EvsR = { prevWaves_EvsR[1], prevWaves_EvsR[2], prevWaves_EvsR[3], evsrValue };
                    prevWaves_EvsR = newWave_EvsR;

                    // Prev Wave
                    prevWave_Down = cumul;
                }
            }
        }

    }
}
