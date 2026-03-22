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
                    timerHandler.isAsyncLoading = false;
                    ClearAndRecalculate();
                    Timer.Start(TimeSpan.FromSeconds(1));
                }
            }
        }

        private double[] Get_Volume_or_Wicks(int index, bool isVolume)
        {
            DateTime startTime = Bars.OpenTimes[index];
            DateTime endTime = Bars.OpenTimes[index + 1];
            // For real-time market
            if (IsLastBar)
                endTime = TicksOHLC.LastBar.OpenTime;

            int volume = 0;
            double min = Int32.MaxValue;
            double max = 0;

            int startIndex = isVolume ? PerformanceTick.lastIdx_Bars : PerformanceTick.lastIdx_Wicks;
            if (IsLastBar) {
                while (TicksOHLC.OpenTimes[startIndex] < startTime)
                    startIndex++;
                if (isVolume)
                    PerformanceTick.lastIdx_Bars = startIndex;
                else
                    PerformanceTick.lastIdx_Wicks = startIndex;
            }

            for (int tickIndex = startIndex; tickIndex < TicksOHLC.Count; tickIndex++)
            {
                Bar tickBar = TicksOHLC[tickIndex];

                if (tickBar.OpenTime < startTime || tickBar.OpenTime > endTime) {
                    if (tickBar.OpenTime > endTime) {
                        PerformanceTick.lastIdx_Bars = isVolume ? tickIndex : PerformanceTick.lastIdx_Bars;
                        PerformanceTick.lastIdx_Wicks = !isVolume ? tickIndex : PerformanceTick.lastIdx_Wicks;
                        break;
                    }
                    else
                        continue;
                }
                if (isVolume)
                    volume += 1;
                else {
                    if (tickBar.Close < min)
                        min = tickBar.Close;
                    else if (tickBar.Close > max)
                        max = tickBar.Close;
                }
            }

            double[] toReturn = { min, max, volume };
            return toReturn;
        }

        // *********** RENKO WICKS ***********
        /*
            Original source code by srlcarlg (me) (https://ctrader.com/algos/indicators/show/3046)
            Improved after Order Flow Aggregated v2.0
        */
        private void RenkoWicks(int index)
        {
            double highest = Bars.HighPrices[index];
            double lowest = Bars.LowPrices[index];
            double open = Bars.OpenPrices[index];

            bool isBullish = Bars.ClosePrices[index] > Bars.OpenPrices[index];
            bool prevIsBullish = Bars.ClosePrices[index - 1] > Bars.OpenPrices[index - 1];
            bool priceGap = Bars.OpenTimes[index] == Bars[index - 1].OpenTime || Bars[index - 2].OpenTime == Bars[index - 1].OpenTime;
            DateTime currentOpenTime = Bars.OpenTimes[index];

            double[] wicks = Get_Volume_or_Wicks(index, false);
            if (IsLastBar) {
                lowest = wicks[0];
                highest = wicks[1];
                open = Bars.ClosePrices[index - 1];
            } else {
                if (isBullish)
                    lowest = wicks[0];
                else
                    highest = wicks[1];
            }

            if (isBullish)
            {
                if (lowest < open && !priceGap) {
                    if (IsLastBar && !prevIsBullish && Bars.ClosePrices[index] > open)
                        open = Bars.OpenPrices[index];
                    ChartTrendLine trendlineUp = Chart.DrawTrendLine($"UpWick_{index}", currentOpenTime, open, currentOpenTime, lowest, UpWickColor);
                    trendlineUp.Thickness = RenkoThickness;
                    Chart.RemoveObject($"DownWick_{index}");
                }
            }
            else
            {
                if (highest > open && !priceGap) {
                    if (IsLastBar && prevIsBullish && Bars.ClosePrices[index] < open)
                        open = Bars.OpenPrices[index];
                    ChartTrendLine trendlineDown = Chart.DrawTrendLine($"DownWick_{index}", currentOpenTime, open, currentOpenTime, highest, DownWickColor);
                    trendlineDown.Thickness = RenkoThickness;
                    Chart.RemoveObject($"UpWick_{index}");
                }
            }
        }

        private void DrawOnScreen(string msg)
        {
            Chart.DrawStaticText("txt", $"{msg}", VerticalAlignment.Top, HorizontalAlignment.Center, Color.LightBlue);
        }
    }
}
