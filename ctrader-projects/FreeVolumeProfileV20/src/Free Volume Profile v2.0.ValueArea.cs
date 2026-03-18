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
        // *********** VA + POC ***********
        private void Draw_VA_POC(Dictionary<double, double> vpDict, int iStart, DateTime x1_Start, DateTime xBar, ExtraProfiles extraVP = ExtraProfiles.No, bool isIntraday = false, DateTime intraX1 = default, string fixedKey = "")
        {
            string prefix = extraVP == ExtraProfiles.Fixed ? fixedKey : $"{iStart}";

            if (VAParams.ShowVA) {
                double[] VAL_VAH_POC = VA_Calculation(vpDict);

                if (!VAL_VAH_POC.Any())
                    return;

                ChartTrendLine poc = Chart.DrawTrendLine($"{prefix}_POC_{extraVP}", x1_Start, VAL_VAH_POC[2] - rowHeight, xBar, VAL_VAH_POC[2] - rowHeight, ColorPOC);
                ChartTrendLine vah = Chart.DrawTrendLine($"{prefix}_VAH_{extraVP}", x1_Start, VAL_VAH_POC[1] + rowHeight, xBar, VAL_VAH_POC[1] + rowHeight, ColorVAH);
                ChartTrendLine val = Chart.DrawTrendLine($"{prefix}_VAL_{extraVP}", x1_Start, VAL_VAH_POC[0], xBar, VAL_VAH_POC[0], ColorVAL);

                poc.LineStyle = LineStylePOC; poc.Thickness = ThicknessPOC; poc.Comment = "POC";
                vah.LineStyle = LineStyleVA; vah.Thickness = ThicknessVA; vah.Comment = "VAH";
                val.LineStyle = LineStyleVA; val.Thickness = ThicknessVA; val.Comment = "VAL";

                ChartRectangle rectVA;
                rectVA = Chart.DrawRectangle($"{prefix}_RectVA_{extraVP}", x1_Start, VAL_VAH_POC[0], xBar, VAL_VAH_POC[1] + rowHeight, VAColor);
                rectVA.IsFilled = true;

                DateTime extDate = extraVP == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
                if (VAParams.ExtendVA) {
                    vah.Time2 = extDate;
                    val.Time2 = extDate;
                    rectVA.Time2 = extDate;
                }
                if (VAParams.ExtendPOC)
                    poc.Time2 = extDate;

                if (isIntraday && extraVP != ExtraProfiles.MiniVP) {
                    poc.Time1 = intraX1;
                    vah.Time1 = intraX1;
                    val.Time1 = intraX1;
                    rectVA.Time1 = intraX1;
                }
            }
            else if (!VAParams.ShowVA && VAParams.KeepPOC)
            {
                double positiveMax = Math.Abs(vpDict.Values.Max());
                double negativeMax = 0;
                try { negativeMax = Math.Abs(vpDict.Values.Where(n => n < 0).Min()); } catch { }

                double largestVOL = positiveMax > negativeMax ? positiveMax : negativeMax;

                double priceLVOL = 0;
                foreach (var kv in vpDict)
                {
                    if (Math.Abs(kv.Value) == largestVOL) { priceLVOL = kv.Key; break; }
                }
                ChartTrendLine poc = Chart.DrawTrendLine($"{prefix}_POC_{extraVP}", x1_Start, priceLVOL - rowHeight, xBar, priceLVOL - rowHeight, ColorPOC);
                poc.LineStyle = LineStylePOC; poc.Thickness = ThicknessPOC; poc.Comment = "POC";

                if (VAParams.ExtendPOC)
                    poc.Time2 = extraVP == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();

                if (isIntraday && extraVP != ExtraProfiles.MiniVP)
                    poc.Time1 = intraX1;
            }

            DateTime extendDate() {
                string tfName = extraVP == ExtraProfiles.No ?
                (GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? "D1" :
                    GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? "W1" : "Month1" ) :
                extraVP == ExtraProfiles.MiniVP ? ProfileParams.MiniVPs_Timeframe.ShortName.ToString() :
                extraVP == ExtraProfiles.Weekly ?  "W1" :  "Month1";

                // Get the time-based interval value
                string tfString = string.Join("", tfName.Where(char.IsDigit));
                int tfValue = int.TryParse(tfString, out int value) ? value : 1;

                DateTime dateToReturn = xBar;
                if (tfName.Contains('m'))
                    dateToReturn = xBar.AddMinutes(tfValue * VAParams.ExtendCount);
                else if (tfName.Contains('h'))
                    dateToReturn = xBar.AddHours(tfValue * VAParams.ExtendCount);
                else if (tfName.Contains('D'))
                    dateToReturn = xBar.AddDays(tfValue * VAParams.ExtendCount);
                else if (tfName.Contains('W'))
                    dateToReturn = xBar.AddDays(7 * VAParams.ExtendCount);
                else if (tfName.Contains("Month1"))
                    dateToReturn = xBar.AddMonths(tfValue * VAParams.ExtendCount);

                return dateToReturn;
            }
        }

        private double[] VA_Calculation(Dictionary<double, double> vpDict)
        {
            /*  https://onlinelibrary.wiley.com/doi/pdf/10.1002/9781118659724.app1
                https://www.mypivots.com/dictionary/definition/40/calculating-market-profile-value-area
                Same of TPO Profile(https://ctrader.com/algos/indicators/show/3074)  */

            if (vpDict.Values.Count < 4)
                return Array.Empty<double>();

            double positiveMax = Math.Abs(vpDict.Values.Max());
            double negativeMax = 0;
            try { negativeMax = Math.Abs(vpDict.Values.Where(n => n < 0).Min()); } catch { }

            double largestVOL = positiveMax > negativeMax ? positiveMax : negativeMax;

            double totalvol = Math.Abs(vpDict.Values.Sum());
            double _70percent = Math.Round((VAParams.PercentVA * totalvol) / 100);

            double priceLVOL = 0;
            foreach (var kv in vpDict)
            {
                if (Math.Abs(kv.Value) == largestVOL) { priceLVOL = kv.Key; break; }
            }
            double priceVAH = 0;
            double priceVAL = 0;

            double sumVA = largestVOL;

            List<double> upKeys = new();
            List<double> downKeys = new();
            for (int i = 0; i < Segments_VP.Count; i++)
            {
                double priceKey = Segments_VP[i];

                if (vpDict.ContainsKey(priceKey))
                {
                    if (priceKey < priceLVOL)
                        downKeys.Add(priceKey);
                    else if (priceKey > priceLVOL)
                        upKeys.Add(priceKey);
                }
            }

            double[] withoutVA = { priceLVOL - (rowHeight * 2), priceLVOL + (rowHeight / 2), priceLVOL };
            if (!upKeys.Any() || !downKeys.Any())
                return withoutVA;

            upKeys.Sort();
            if (upKeys.Count > 2)
                upKeys.Remove(upKeys.LastOrDefault());
            downKeys.Sort();
            downKeys.Reverse();

            double[] prev2UP = { 0, 0 };
            double[] prev2Down = { 0, 0 };

            bool lockAbove = false;
            double[] aboveKV = { 0, 0 };

            bool lockBelow = false;
            double[] belowKV = { 0, 0 };

            for (int i = 0; i < vpDict.Keys.Count; i++)
            {
                if (sumVA >= _70percent)
                    break;

                double sumUp = 0;
                double sumDown = 0;

                // ==== Above of POC ====
                double prevUPkey = upKeys.First();
                double keyUP = 0;
                foreach (double key in upKeys)
                {
                    if (upKeys.Count == 1 || prev2UP[0] != 0 && prev2UP[1] != 0 && key == upKeys.Last())
                    {
                        sumDown = Math.Abs(vpDict[key]);
                        keyUP = key;
                        break;
                    }
                    if (lockAbove)
                    {
                        keyUP = aboveKV[0];
                        sumUp = aboveKV[1];
                        break;
                    }
                    if (prev2UP[0] == 0 && prev2UP[1] == 0 && key != prevUPkey
                    || prev2UP[0] != 0 && prev2UP[1] != 0 && prevUPkey > aboveKV[0] && key > aboveKV[0])
                    {
                        double upVOL = Math.Abs(vpDict[key]);
                        double up2VOL = Math.Abs(vpDict[prevUPkey]);

                        keyUP = key;

                        double[] _2up = { prevUPkey, keyUP };
                        prev2UP = _2up;

                        double[] _above = { keyUP, upVOL + up2VOL };
                        aboveKV = _above;

                        sumUp = upVOL + up2VOL;
                        break;
                    }
                    prevUPkey = key;
                }

                // ==== Below of POC ====
                double prevDownkey = downKeys.First();
                double keyDw = 0;
                foreach (double key in downKeys)
                {
                    if (downKeys.Count == 1 || prev2Down[0] != 0 && prev2Down[1] != 0 && key == downKeys.Last())
                    {
                        sumDown = Math.Abs(vpDict[key]);
                        keyDw = key;
                        break;
                    }
                    if (lockBelow)
                    {
                        keyDw = belowKV[0];
                        sumDown = belowKV[1];
                        break;
                    }
                    if (prev2Down[0] == 0 && prev2Down[1] == 0 && key != prevDownkey
                    || prev2Down[0] != 0 && prev2Down[1] != 0 && prevDownkey < aboveKV[0] && key < belowKV[0])
                    {
                        double downVOL = Math.Abs(vpDict[key]);
                        double down2VOL = Math.Abs(vpDict[prevDownkey]);

                        keyDw = key;

                        double[] _2down = { prevDownkey, keyDw };
                        prev2Down = _2down;

                        double[] _below = { keyDw, downVOL + down2VOL };
                        belowKV = _below;

                        sumDown = downVOL + down2VOL;
                        break;
                    }
                    prevDownkey = key;
                }

                // ==== VA rating ====
                if (sumUp > sumDown)
                {
                    sumVA += sumUp;
                    priceVAH = keyUP;
                    priceVAL = keyDw;

                    lockBelow = true;
                    lockAbove = false;
                }
                else if (sumDown > sumUp)
                {
                    sumVA += sumDown;
                    priceVAH = keyUP;
                    priceVAL = keyDw;

                    lockBelow = false;
                    lockAbove = true;
                }
                else if (sumUp == sumDown)
                {
                    double[] _2up = { prevUPkey, keyUP };
                    prev2UP = _2up;
                    double[] _2down = { prevDownkey, keyDw };
                    prev2Down = _2down;

                    sumVA += (sumUp + sumDown);
                    priceVAH = keyUP;
                    priceVAL = keyDw;

                    lockBelow = false;
                    lockAbove = false;
                }
            }

            double[] VA = { priceVAL, priceVAH, priceLVOL };

            return VA;
        }

        
    }
}
