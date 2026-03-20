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
        public void ClearAndRecalculate()
        {
            Thread.Sleep(300);
            LoadMoreHistory_IfNeeded();

            // LookBack from VP
            Bars vpBars = GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? DailyBars :
                           GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? WeeklyBars : MonthlyBars;
            int firstIndex = Bars.OpenTimes.GetIndexByTime(vpBars.OpenTimes.FirstOrDefault());

            // Get index of VP Interval to continue only in Lookback
            int iVerify = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
            while (vpBars.ClosePrices.Count - iVerify > GeneralParams.Lookback) {
                firstIndex++;
                iVerify = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
            }

            // Daily or Weekly VP
            int TF_idx = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
            int startIndex = Bars.OpenTimes.GetIndexByTime(vpBars.OpenTimes[TF_idx]);

            // Weekly Profile but Daily VP
            bool extraWeekly = ProfileParams.EnableWeeklyProfile && GeneralParams.VPInterval_Input == VPInterval_Data.Daily;
            if (extraWeekly) {
                TF_idx = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
                startIndex = Bars.OpenTimes.GetIndexByTime(WeeklyBars.OpenTimes[TF_idx]);
            }

            // Monthly Profile
            bool extraMonthly = ProfileParams.EnableMonthlyProfile;
            if (extraMonthly) {
                TF_idx = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstIndex]);
                startIndex = Bars.OpenTimes.GetIndexByTime(MonthlyBars.OpenTimes[TF_idx]);
            }

            // Reset VOL_Bars/Source Index.
            PerformanceSource.ResetAll();
            // Reset Segments
            Segments_VP.Clear();
            segmentInfo.Clear();
            // Reset last update
            ClearIdx.ResetAll();
            // Reset Fixed Range
            foreach (ChartRectangle rect in RangeObjs.rectangles)
            {
                DateTime end = rect.Time1 < rect.Time2 ? rect.Time2 : rect.Time1;
                ResetFixedRange(rect.Name, end);
            }

            // Historical data
            for (int index = startIndex; index < Bars.Count; index++)
            {
                CreateSegments(index);

                CreateMonthlyVP(index);
                CreateWeeklyVP(index);

                // Calculate VP only in lookback
                if (extraWeekly || extraMonthly) {
                    iVerify = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                    if (vpBars.ClosePrices.Count - iVerify > GeneralParams.Lookback)
                        continue;
                }

                TF_idx = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                startIndex = Bars.OpenTimes.GetIndexByTime(vpBars.OpenTimes[TF_idx]);

                if (index == startIndex ||
                   (index - 1) == startIndex && isPriceBased_Chart ||
                   (index - 1) == startIndex && (index - 1) != ClearIdx.MainVP)
                    CleanUp_MainVP(startIndex, index);

                try { if (ProfileParams.EnableMainVP) VolumeProfile(startIndex, index); } catch { }
                
                CreateMiniVPs(index);

                // Export recalculated history
                if (_isManualCsvExportInProgress)
                {
                    if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                        ExportCsvData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                    if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                        ExportCsvData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
                }
            }

            configHasChanged = true;
            DrawStartVolumeLine();
        }

        public void DrawStartVolumeLine() {
            try {
                DateTime firstVolDate = Source_Bars.OpenTimes.FirstOrDefault();
                double firstVolPrice = Source_Bars.HighPrices.FirstOrDefault();
                ChartVerticalLine lineInfo = Chart.DrawVerticalLine("Volume_Start", firstVolDate, Color.Red);
                lineInfo.LineStyle = LineStyle.Lines;
                ChartText textInfo = Chart.DrawText($"Volume_Start_Text", $"{ProfileParams.Source_Timeframe.ShortName} Volume Data \n ends here", firstVolDate, firstVolPrice, Color.Red);
                textInfo.FontSize = 8;
            }
            catch { };

            try {
                Bar firstInterval_Bar = GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? DailyBars.FirstOrDefault() :
                                       GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? WeeklyBars.FirstOrDefault() : MonthlyBars.FirstOrDefault();
                DateTime firstInterval_Date = firstInterval_Bar.OpenTime;
                double firstInterval_Price = firstInterval_Bar.High;

                ChartVerticalLine lineInfo = Chart.DrawVerticalLine("Lookback_Start", firstInterval_Date, Color.Gray);
                lineInfo.LineStyle = LineStyle.Lines;
                ChartText textInfo = Chart.DrawText($"Lookback_Start_Text", $"{GeneralParams.VPInterval_Input} Interval Data \n ends here", firstInterval_Date, firstInterval_Price, Color.Gray);
                textInfo.FontSize = 8;
            }
            catch { };
        }
        public void DrawTargetDateLine() {
            try
            {
                Bars VPInterval_Bars = GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? DailyBars :
                                       GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? WeeklyBars : MonthlyBars;
                DateTime TargetVolDate = VPInterval_Bars.OpenTimes[VPInterval_Bars.ClosePrices.Count - GeneralParams.Lookback];
                TargetVolDate = ProfileParams.EnableWeeklyProfile && !ProfileParams.EnableMonthlyProfile ? WeeklyBars.LastBar.OpenTime.Date :
                                ProfileParams.EnableMonthlyProfile ? MonthlyBars.LastBar.OpenTime.Date :
                                TargetVolDate;
                ChartVerticalLine lineInfo = Chart.DrawVerticalLine("VolumeTarget", TargetVolDate, Color.Yellow);
                lineInfo.LineStyle = LineStyle.Lines;
                ChartText textInfo = Chart.DrawText($"VolumeTargetText", $"Target Volume Data", TargetVolDate, Source_Bars.HighPrices.FirstOrDefault(), Color.Red);
                textInfo.FontSize = 8;
            }
            catch { }
        }

        public void LoadMoreHistory_IfNeeded() {
            Bars VPInterval_Bars = GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? DailyBars :
                                   GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? WeeklyBars : MonthlyBars;

            DateTime sourceDate = ProfileParams.EnableWeeklyProfile && !ProfileParams.EnableMonthlyProfile ? WeeklyBars.LastBar.OpenTime.Date :
                                  ProfileParams.EnableMonthlyProfile ? MonthlyBars.LastBar.OpenTime.Date :
                                  VPInterval_Bars.OpenTimes[VPInterval_Bars.ClosePrices.Count - GeneralParams.Lookback];

            if (LoadBarsStrategy_Input == LoadBarsStrategy_Data.Async)
            {
                if (VPInterval_Bars.ClosePrices.Count < GeneralParams.Lookback || Source_Bars.OpenTimes.FirstOrDefault() > sourceDate) {
                    SourceObjs.startAsyncLoading = false;
                    SourceObjs.isLoadingComplete = false;
                    timerHandler.isAsyncLoading = true;
                    Timer.Start(TimeSpan.FromSeconds(0.5));
                }
                return;
            }

            // Lookback
            if (VPInterval_Bars.ClosePrices.Count < GeneralParams.Lookback)
            {
                PopupNotification notifyProgress = Notifications.ShowPopup(NOTIFY_CAPTION, $"Loading Sync => {VPInterval_Bars} Lookback Bars", PopupNotificationState.InProgress);

                while (VPInterval_Bars.ClosePrices.Count < GeneralParams.Lookback)
                {
                    int loadedCount = VPInterval_Bars.LoadMoreHistory();
                    if (loadedCount == 0)
                        break;
                }

                notifyProgress.Complete(PopupNotificationState.Success);
            }

            DateTime lookbackDate = VPInterval_Bars.OpenTimes[VPInterval_Bars.ClosePrices.Count - GeneralParams.Lookback];

            sourceDate = ProfileParams.EnableWeeklyProfile && !ProfileParams.EnableMonthlyProfile ? WeeklyBars.LastBar.OpenTime.Date :
                         ProfileParams.EnableMonthlyProfile ? MonthlyBars.LastBar.OpenTime.Date :
                         VPInterval_Bars.OpenTimes[VPInterval_Bars.ClosePrices.Count - GeneralParams.Lookback];

            if (ProfileParams.EnableMiniProfiles && MiniVPs_Bars.OpenTimes.FirstOrDefault() > lookbackDate)
            {
                PopupNotification notifyProgress = Notifications.ShowPopup(NOTIFY_CAPTION, $"Loading Sync => {ProfileParams.MiniVPs_Timeframe} Lookback Bars", PopupNotificationState.InProgress);

                while (MiniVPs_Bars.OpenTimes.FirstOrDefault() > lookbackDate)
                {
                    int loadedCount = MiniVPs_Bars.LoadMoreHistory();
                    if (loadedCount == 0)
                        break;
                }

                notifyProgress.Complete(PopupNotificationState.Success);
            }

            // Source
            if (Source_Bars.OpenTimes.FirstOrDefault() > sourceDate)
            {
                PopupNotification notifyProgress_Two = Notifications.ShowPopup(NOTIFY_CAPTION, $"Loading Sync => {ProfileParams.Source_Timeframe.ShortName} Bars", PopupNotificationState.InProgress);

                while (Source_Bars.OpenTimes.FirstOrDefault() > sourceDate)
                {
                    int loadedCount = Source_Bars.LoadMoreHistory();
                    if (loadedCount == 0)
                        break;
                }

                notifyProgress_Two.Complete(PopupNotificationState.Success);
            }
        }

        protected override void OnTimer()
        {
            if (timerHandler.isAsyncLoading)
            {
                if (!SourceObjs.startAsyncLoading)
                {
                    string volumeLineInfo = "=> Zoom out and follow the Vertical Line";
                    SourceObjs.asyncBarsPopup = Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        $"[{Symbol.Name}] Loading Async {ProfileParams.Source_Timeframe.ShortName} Bars \n{volumeLineInfo}",
                        PopupNotificationState.InProgress
                    );
                }

                if (!SourceObjs.isLoadingComplete)
                {
                    Bars VPInterval_Bars = GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? DailyBars :
                                           GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? WeeklyBars : MonthlyBars;
                    if (VPInterval_Bars.ClosePrices.Count < GeneralParams.Lookback)
                    {
                        while (VPInterval_Bars.ClosePrices.Count < GeneralParams.Lookback)
                        {
                            int loadedCount = VPInterval_Bars.LoadMoreHistory();
                            if (loadedCount == 0)
                                break;
                        }
                    }

                    DateTime lookbackDate = VPInterval_Bars.OpenTimes[VPInterval_Bars.ClosePrices.Count - GeneralParams.Lookback];
                    if (ProfileParams.EnableMiniProfiles && MiniVPs_Bars.OpenTimes.FirstOrDefault() > lookbackDate) {
                        while (MiniVPs_Bars.OpenTimes.FirstOrDefault() > lookbackDate)
                        {
                            int loadedCount = MiniVPs_Bars.LoadMoreHistory();
                            if (loadedCount == 0)
                                break;
                        }
                    }

                    DrawTargetDateLine();

                    DateTime sourceDate = ProfileParams.EnableWeeklyProfile && !ProfileParams.EnableMonthlyProfile ? WeeklyBars.LastBar.OpenTime.Date :
                                          ProfileParams.EnableMonthlyProfile ? MonthlyBars.LastBar.OpenTime.Date :
                                          lookbackDate;

                    Source_Bars.LoadMoreHistoryAsync((_) => {
                        DateTime currentDate = _.Bars.FirstOrDefault().OpenTime;

                        DrawStartVolumeLine();

                        if (currentDate != default && currentDate < sourceDate) {
                            if (SourceObjs.asyncBarsPopup.State != PopupNotificationState.Success)
                                SourceObjs.asyncBarsPopup.Complete(PopupNotificationState.Success);

                            SourceObjs.isLoadingComplete = true;
                        }
                    });

                    SourceObjs.startAsyncLoading = true;
                }
                else {
                    ClearAndRecalculate();
                    timerHandler.isAsyncLoading = false;
                    Timer.Stop();
                }
            }

        }

        public int GetLookback() {
            return GeneralParams.Lookback;
        }
        public double GetRowHeight() {
            return rowHeight;
        }

        public void SetRowHeight(double number) {
            rowHeight = number;
        }
        public void SetLookback(int number) {
            GeneralParams.Lookback = number;
            LoadMoreHistory_IfNeeded();
        }
        public void SetMiniVPsBars() {
            MiniVPs_Bars = MarketData.GetBars(ProfileParams.MiniVPs_Timeframe);
        }
        public void SetVPBars() {
            Source_Bars = MarketData.GetBars(ProfileParams.Source_Timeframe);
            LoadMoreHistory_IfNeeded();
        }

    }
}
