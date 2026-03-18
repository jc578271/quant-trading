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
        private void CreateMiniVPs(int index, bool loopStart = false, bool isLoop = false, bool isConcurrent = false) {
            if (ProfileParams.EnableMiniProfiles)
            {
                int miniIndex = MiniVPs_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                int miniStart = Bars.OpenTimes.GetIndexByTime(MiniVPs_Bars.OpenTimes[miniIndex]);

                if (index == miniStart ||
                    (index - 1) == miniStart && isPriceBased_Chart ||
                    (index - 1) == miniStart && (index - 1) != ClearIdx.Mini || loopStart
                ) {
                    if (!IsLastBar)
                        PerformanceTick.startIdx_Mini = PerformanceTick.lastIdx_Mini;

                    MiniRank.ClearAllModes();
                    ClearIdx.Mini = index == miniStart ? index : (index - 1);
                }
                if (!isConcurrent)
                    VolumeProfile(miniStart, index, ExtraProfiles.MiniVP, isLoop);
                else
                {
                    _Tasks.MiniVP ??= Task.Run(() => LiveVP_Worker(ExtraProfiles.MiniVP, _Tasks.cts.Token));

                    LiveVPIndexes.Mini = miniStart;

                    if (index != miniStart) {
                        lock (_Locks.MiniVP)
                        VolumeProfile(miniStart, index, ExtraProfiles.MiniVP, false, true);
                    }
                }
            }
        }
        private void CreateWeeklyVP(int index, bool loopStart = false, bool isLoop = false, bool isConcurrent = false) {
            if (ProfileParams.EnableWeeklyProfile)
            {
                // Avoid recalculating the same period.
                if (MiscParams.ODFInterval_Input == ODFInterval_Data.Weekly)
                    return;

                int weekIndex = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                int weekStart = Bars.OpenTimes.GetIndexByTime(WeeklyBars.OpenTimes[weekIndex]);

                if (index == weekStart ||
                    (index - 1) == weekStart && isPriceBased_Chart || loopStart
                ) {
                    if (!IsLastBar)
                        PerformanceTick.startIdx_Weekly = PerformanceTick.lastIdx_Weekly;
                    WeeklyRank.ClearAllModes();
                }

                if (!isConcurrent)
                    VolumeProfile(weekStart, index, ExtraProfiles.Weekly, isLoop);
                else
                {
                    _Tasks.WeeklyVP ??= Task.Run(() => LiveVP_Worker(ExtraProfiles.Weekly, _Tasks.cts.Token));

                    LiveVPIndexes.Weekly = weekStart;

                    if (index != weekStart) {
                        lock (_Locks.WeeklyVP)
                            VolumeProfile(weekStart, index, ExtraProfiles.Weekly, false, true);
                    }

                    DateTime weekStartDate = WeeklyBars.OpenTimes[weekIndex];
                    TickObjs.firstTickTime = TickObjs.firstTickTime > weekStartDate ? TicksOHLC.OpenTimes.FirstOrDefault() : TickObjs.firstTickTime;
                    if (TickObjs.firstTickTime > weekStartDate)
                    {
                        DrawOnScreen("Not enough Tick data to calculate Weekly Profile \n Zoom out to see the vertical Aqua line");
                        Chart.DrawVerticalLine("WeekStart", weekStartDate, Color.Aqua);
                        ChartText text = Chart.DrawText("WeekStartText", "Target Weekly Tick Data", weekStartDate,
                                    WeeklyBars.HighPrices[weekIndex], Color.Aqua);
                        text.HorizontalAlignment = HorizontalAlignment.Right;
                        text.VerticalAlignment = VerticalAlignment.Top;
                        text.FontSize = 8;
                    }
                }
            }
        }
        private void CreateMonthlyVP(int index, bool loopStart = false, bool isLoop = false, bool isConcurrent = false) {
            if (ProfileParams.EnableMonthlyProfile)
            {
                int monthIndex = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                int monthStart = Bars.OpenTimes.GetIndexByTime(MonthlyBars.OpenTimes[monthIndex]);

                if (index == monthStart ||
                    (index - 1) == monthStart && isPriceBased_Chart || loopStart
                ) {
                    if (!IsLastBar)
                        PerformanceTick.startIdx_Monthly = PerformanceTick.lastIdx_Monthly;
                    MonthlyRank.ClearAllModes();
                }
                if (!isConcurrent)
                    VolumeProfile(monthStart, index, ExtraProfiles.Monthly, isLoop);
                else
                {
                    _Tasks.MonthlyVP ??= Task.Run(() => LiveVP_Worker(ExtraProfiles.Monthly, _Tasks.cts.Token));

                    LiveVPIndexes.Monthly = monthStart;

                    if (index != monthStart) {
                        lock (_Locks.MonthlyVP)
                            VolumeProfile(monthStart, index, ExtraProfiles.Monthly, false, true);
                    }

                    DateTime monthStartDate = MonthlyBars.OpenTimes[monthIndex];
                    TickObjs.firstTickTime = TickObjs.firstTickTime > monthStartDate ? TicksOHLC.OpenTimes.FirstOrDefault() : TickObjs.firstTickTime;
                    if (TickObjs.firstTickTime > monthStartDate)
                    {
                        Second_DrawOnScreen("Not enough Tick data to calculate Monthly Profile \n- Zoom out to see the vertical Aqua line");
                        Chart.DrawVerticalLine("MonthStart", monthStartDate, Color.Aqua);
                        ChartText text = Chart.DrawText("MonthStartText", "Target Monthly Tick Data", monthStartDate,
                                    MonthlyBars.HighPrices[monthIndex], Color.Aqua);
                        text.HorizontalAlignment = HorizontalAlignment.Right;
                        text.VerticalAlignment = VerticalAlignment.Top;
                        text.FontSize = 8;
                    }
                }
            }
        }

        // *********** LIVE PROFILE UPDATE ***********
        private void LiveVP_Update(int indexStart, int index, bool onlyMini = false) {
            double price = Bars.ClosePrices[index];

            bool updateStrategy = ProfileParams.UpdateProfile_Input switch {
                UpdateProfile_Data.ThroughSegments_Balanced => Math.Abs(price - prevUpdatePrice) >= rowHeight,
                UpdateProfile_Data.Through_2_Segments_Best => Math.Abs(price - prevUpdatePrice) >= (rowHeight + rowHeight),
                _ => true
            };

            if (updateStrategy || BooleanUtils.isUpdateVP || BooleanUtils.configHasChanged)
            {
                if (!onlyMini)
                {
                    if (ProfileParams.EnableMonthlyProfile) {

                        int monthIndex = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                        DateTime monthStartDate = MonthlyBars.OpenTimes[monthIndex];
                        int monthStart = Bars.OpenTimes.GetIndexByTime(MonthlyBars.OpenTimes[monthIndex]);

                        if (TickObjs.firstTickTime > monthStartDate) {
                            Second_DrawOnScreen("Not enough Tick data to calculate Monthly Profile \n- Zoom out to see the vertical Aqua line");
                            Chart.DrawVerticalLine("MonthStart", monthStartDate, Color.Aqua);
                            ChartText text =Chart.DrawText("MonthStartText", "Target Monthly Tick Data", monthStartDate,
                                           MonthlyBars.HighPrices[monthIndex], Color.Aqua);
                            text.HorizontalAlignment = HorizontalAlignment.Right;
                            text.VerticalAlignment = VerticalAlignment.Top;
                            text.FontSize = 8;
                        }

                        if (index != monthStart)
                        {
                            bool loopStart = true;
                            for (int i = monthStart; i <= index; i++) {
                                if (i < index)
                                    CreateMonthlyVP(i, loopStart, true); // Update only
                                else
                                    CreateMonthlyVP(i, loopStart, false); // Update and Draw
                                loopStart = false;
                            }

                        }
                    }

                    if (ProfileParams.EnableWeeklyProfile && MiscParams.ODFInterval_Input != ODFInterval_Data.Weekly)
                    {
                        int weekIndex = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                        DateTime weekStartDate = WeeklyBars.OpenTimes[weekIndex];
                        int weekStart = Bars.OpenTimes.GetIndexByTime(WeeklyBars.OpenTimes[weekIndex]);

                        TickObjs.firstTickTime = TickObjs.firstTickTime > weekStartDate ? TicksOHLC.OpenTimes.FirstOrDefault() : TickObjs.firstTickTime;
                        if (TickObjs.firstTickTime > weekStartDate) {
                            DrawOnScreen("Not enough Tick data to calculate Weekly Profile \n Zoom out to see the vertical Aqua line");
                            Chart.DrawVerticalLine("WeekStart", weekStartDate, Color.Aqua);
                            ChartText text = Chart.DrawText("WeekStartText", "Target Weekly Tick Data", weekStartDate,
                                           WeeklyBars.HighPrices[weekIndex], Color.Aqua);
                            text.HorizontalAlignment = HorizontalAlignment.Right;
                            text.VerticalAlignment = VerticalAlignment.Top;
                            text.FontSize = 8;
                        }

                        if (index != weekStart)
                        {
                            bool loopStart = true;
                            for (int i = weekStart; i <= index; i++) {
                                if (i < index)
                                    CreateWeeklyVP(i, loopStart, true); // Update only
                                else
                                    CreateWeeklyVP(i, loopStart, false); // Update and Draw
                                loopStart = false;
                            }
                        }
                    }

                    if (ProfileParams.EnableMiniProfiles) {
                        int miniIndex = MiniVPs_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                        int miniStart = Bars.OpenTimes.GetIndexByTime(MiniVPs_Bars.OpenTimes[miniIndex]);

                        if (index != miniStart)
                        {
                            bool loopStart = true;
                            for (int i = miniStart; i <= index; i++)
                            {
                                if (i < index)
                                    CreateMiniVPs(i, loopStart, true); // Update only
                                else
                                    CreateMiniVPs(i, loopStart, false); // Update and Draw
                                loopStart = false;
                            }
                        }
                    }

                    if (index != indexStart)
                    {
                        for (int i = indexStart; i <= index; i++)
                        {
                            if (i == indexStart) {
                                VP_VolumesRank.Clear();
                                VP_VolumesRank_Up.Clear();
                                VP_VolumesRank_Down.Clear();
                                VP_VolumesRank_Subt.Clear();
                                VP_DeltaRank.Clear();
                            }
                            if (i < index)
                                VolumeProfile(indexStart, i, ExtraProfiles.No, true); // Update only
                            else
                                VolumeProfile(indexStart, i, ExtraProfiles.No, false); // Update and Draw
                        }
                    }
                }
                else
                {
                    int miniIndex = MiniVPs_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                    int miniStart = Bars.OpenTimes.GetIndexByTime(MiniVPs_Bars.OpenTimes[miniIndex]);

                    if (index != miniStart)
                    {
                        bool loopStart = true;
                        for (int i = miniStart; i <= index; i++)
                        {
                            if (i < index)
                                CreateMiniVPs(i, loopStart, true); // Update only
                            else
                                CreateMiniVPs(i, loopStart, false); // Update and Draw
                            loopStart = false;
                        }
                    }
                }
            }

            BooleanUtils.isUpdateVP = false;
            BooleanUtils.configHasChanged = false;

            if (ProfileParams.UpdateProfile_Input != UpdateProfile_Data.EveryTick_CPU_Workout)
                prevUpdatePrice = price;
        }

        private void LiveVP_Concurrent(int index, int indexStart)
        {
            if (!ProfileParams.EnableMainVP && !ProfileParams.EnableMiniProfiles)
                return;

            double price = Bars.ClosePrices[index];
            bool updateStrategy = ProfileParams.UpdateProfile_Input switch {
                UpdateProfile_Data.ThroughSegments_Balanced => Math.Abs(price - prevUpdatePrice) >= rowHeight,
                UpdateProfile_Data.Through_2_Segments_Best => Math.Abs(price - prevUpdatePrice) >= (rowHeight + rowHeight),
                _ => true
            };

            if (updateStrategy || BooleanUtils.isUpdateVP || BooleanUtils.configHasChanged)
            {
                if (Bars.Count > BarTimes_Array.Length)
                {
                    lock (_Locks.Bar)
                        BarTimes_Array = Bars.OpenTimes.ToArray();
                }
                lock (_Locks.Tick) {
                    int startFrom = ProfileParams.EnableMonthlyProfile ? 
                                    PerformanceTick.startIdx_Monthly :
                                    (ProfileParams.EnableWeeklyProfile && MiscParams.ODFInterval_Input != ODFInterval_Data.Weekly) ?
                                    PerformanceTick.startIdx_Weekly :
                                    ProfileParams.EnableMainVP ? 
                                    PerformanceTick.startIdx_MainVP :
                                    (ProfileParams.MiniVPs_Timeframe >= TimeFrame.Hour4 ? PerformanceTick.startIdx_Mini : PerformanceTick.startIdx_MainVP);

                    TickBars_List = new List<Bar>(TicksOHLC.Skip(startFrom - 1));
                }

                liveVP_RunWorker = true;
            }
            _Tasks.cts ??= new CancellationTokenSource();

            CreateMonthlyVP(index, isConcurrent: true);
            CreateWeeklyVP(index, isConcurrent: true);
            CreateMiniVPs(index, isConcurrent: true);

            if (ProfileParams.EnableMainVP)
            {
                _Tasks.MainVP ??= Task.Run(() => LiveVP_Worker(ExtraProfiles.No, _Tasks.cts.Token));
                LiveVPIndexes.MainVP = indexStart;
                if (index != indexStart) {
                    lock (_Locks.MainVP)
                        VolumeProfile(indexStart, index, ExtraProfiles.No, false, true);
                }
            }
        }

        private void LiveVP_Worker(ExtraProfiles extraID, CancellationToken token)
        {
            /*
            It's quite simple, but gave headaches mostly due to GetByInvoke() unexpected behavior and debugging it.
             - GetByInvoke() will slowdown loops due to accumulative Bars[index] => "0.xx ms" operations
            The major reason why Copy of Time/Bars are used.

            I've tried:
                TimesCopy = GetByInvoke(() => Bars.OpenTimes.Skip(startIndex))
                TicksCopy = GetByInvoke(() => TicksOHLC.Skip(startTickIndex))
            With or without ToArray or ToList; leads to RAM spíkes at startup.
            */

            Dictionary<double, double> Worker_VolumesRank = new();
            Dictionary<double, double> Worker_VolumesRank_Up = new();
            Dictionary<double, double> Worker_VolumesRank_Down = new();
            Dictionary<double, double> Worker_VolumesRank_Subt = new();
            Dictionary<double, double> Worker_DeltaRank = new();
            double[] Worker_MinMaxDelta = { 0, 0 };

            DateTime lastTime = new();
            IEnumerable<DateTime> TimesCopy = Array.Empty<DateTime>();
            IEnumerable<Bar> TicksCopy;

            while (!token.IsCancellationRequested)
            {
                if (!liveVP_RunWorker) {
                    // Stop itself
                    if (extraID == ExtraProfiles.No && !ProfileParams.EnableMainVP) {
                        _Tasks.MainVP = null;
                        return;
                    }
                    if (extraID == ExtraProfiles.MiniVP && !ProfileParams.EnableMiniProfiles) {
                        _Tasks.MiniVP = null;
                        return;
                    }
                    if (extraID == ExtraProfiles.Weekly && !ProfileParams.EnableWeeklyProfile) {
                        _Tasks.WeeklyVP = null;
                        return;
                    }
                    if (extraID == ExtraProfiles.Monthly && !ProfileParams.EnableMonthlyProfile) {
                        _Tasks.MonthlyVP = null;
                        return;
                    }

                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    Worker_VolumesRank = new();
                    Worker_VolumesRank_Up = new();
                    Worker_VolumesRank_Down = new();
                    Worker_VolumesRank_Subt = new();
                    Worker_DeltaRank = new();
                    double[] resetDelta = {0, 0};
                    Worker_MinMaxDelta = resetDelta;

                    // Chart Bars
                    int startIndex = extraID switch {
                        ExtraProfiles.MiniVP => LiveVPIndexes.Mini,
                        ExtraProfiles.Weekly => LiveVPIndexes.Weekly,
                        ExtraProfiles.Monthly => LiveVPIndexes.Monthly,
                        _ => LiveVPIndexes.MainVP
                    };
                    DateTime lastBarTime = GetByInvoke(() => Bars.LastBar.OpenTime);

                    // Replace only when needed
                    if (lastTime != lastBarTime) {
                        lock (_Locks.Bar)
                            TimesCopy = BarTimes_Array.Skip(startIndex);
                        lastTime = lastBarTime;
                    }
                    int endIndex = TimesCopy.Count();

                    // 
                    // Tick => Always replace
                    // The ".Skip(startTickIndex)" is already done in LiveVP_Concurrent()
                    lock (_Locks.Tick)
                        TicksCopy = TickBars_List;
                    
                    for (int i = 0; i < endIndex; i++)
                    {
                        Worker_VP_Tick(i, extraID, i == (endIndex - 1));
                    }
                                     
                    object whichLock = extraID switch {
                        ExtraProfiles.MiniVP => _Locks.MiniVP,
                        ExtraProfiles.Weekly => _Locks.WeeklyVP,
                        ExtraProfiles.Monthly => _Locks.MonthlyVP,
                        _ => _Locks.MainVP
                    };
  
                    lock (whichLock) {
                        switch (extraID)
                        {
                            case ExtraProfiles.MiniVP:
                                MiniRank.Normal = Worker_VolumesRank;
                                MiniRank.Up = Worker_VolumesRank_Up;
                                MiniRank.Down = Worker_VolumesRank_Down;
                                MiniRank.Delta = Worker_DeltaRank;
                                MiniRank.MinMaxDelta = Worker_MinMaxDelta;
                                break;
                            case ExtraProfiles.Weekly:
                                WeeklyRank.Normal = Worker_VolumesRank;
                                WeeklyRank.Up = Worker_VolumesRank_Up;
                                WeeklyRank.Down = Worker_VolumesRank_Down;
                                WeeklyRank.Delta = Worker_DeltaRank;
                                WeeklyRank.MinMaxDelta = Worker_MinMaxDelta;
                                break;
                            case ExtraProfiles.Monthly:
                                MonthlyRank.Normal = Worker_VolumesRank;
                                MonthlyRank.Up = Worker_VolumesRank_Up;
                                MonthlyRank.Down = Worker_VolumesRank_Down;
                                MonthlyRank.Delta = Worker_DeltaRank;
                                MonthlyRank.MinMaxDelta = Worker_MinMaxDelta;
                                break;
                            default:
                                VP_VolumesRank = Worker_VolumesRank;
                                VP_VolumesRank_Up = Worker_VolumesRank_Up;
                                VP_VolumesRank_Down = Worker_VolumesRank_Down;
                                VP_VolumesRank_Subt = Worker_VolumesRank_Subt;
                                VP_DeltaRank = Worker_DeltaRank;
                                VP_MinMaxDelta = Worker_MinMaxDelta;
                                break;
                        }

                        BooleanUtils.isUpdateVP = false;
                        BooleanUtils.configHasChanged = false;

                        if (ProfileParams.UpdateProfile_Input != UpdateProfile_Data.EveryTick_CPU_Workout)
                            prevUpdatePrice = TicksCopy.Last().Close;
                    }
                }
                catch (Exception e) { Print($"CRASH at LiveVP_Worker => {extraID}: {e}"); }

                liveVP_RunWorker = false;
            }

            void Worker_VP_Tick(int index, ExtraProfiles extraVP = ExtraProfiles.No, bool isLastBarLoop = false)
            {
                DateTime startTime = TimesCopy.ElementAt(index);
                DateTime endTime = !isLastBarLoop ? TimesCopy.ElementAt(index + 1) : TicksCopy.Last().OpenTime;
                
                double prevLoopTick = 0;
                for (int tickIndex = 0; tickIndex < TicksCopy.Count(); tickIndex++)
                {
                    Bar tickBar = TicksCopy.ElementAt(tickIndex);

                    if (tickBar.OpenTime < startTime || tickBar.OpenTime > endTime)
                    {
                        if (tickBar.OpenTime > endTime)
                            break;
                        else
                            continue;
                    }

                    if (prevLoopTick != 0)
                        RankVolume(tickBar.Close, prevLoopTick);

                    prevLoopTick = tickBar.Close;
                }

                // =======================
                void RankVolume(double tickPrice, double prevTick)
                {
                    bool modeIsBuySell = GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell; 
                    bool modeIsDelta = GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta;
                    
                    List<double> segmentsSource = Segments_VP;

                    double prevSegmentValue = 0.0;
                    for (int i = 0; i < segmentsSource.Count; i++)
                    {
                        if (prevSegmentValue != 0 && tickPrice >= prevSegmentValue && tickPrice <= segmentsSource[i])
                        {
                            double priceKey = segmentsSource[i];

                            double prevDelta = 0;
                            if (modeIsDelta && ResultParams.ShowMinMaxDelta)
                                prevDelta = Worker_DeltaRank.Values.Sum();

                            if (Worker_VolumesRank.ContainsKey(priceKey))
                            {
                                Worker_VolumesRank[priceKey] += 1;
                                
                                if (modeIsBuySell || modeIsDelta) 
                                {
                                    if (tickPrice > prevTick)
                                        Worker_VolumesRank_Up[priceKey] += 1;
                                    else if (tickPrice < prevTick)
                                        Worker_VolumesRank_Down[priceKey] += 1;
                                    else if (tickPrice == prevTick)
                                    {
                                        Worker_VolumesRank_Up[priceKey] += 1;
                                        Worker_VolumesRank_Down[priceKey] += 1;
                                    }
                                    
                                    Worker_VolumesRank_Subt[priceKey] = Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey];
                                }

                                if (modeIsDelta)
                                    Worker_DeltaRank[priceKey] += (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]);
                            }
                            else
                            {
                                Worker_VolumesRank.Add(priceKey, 1);
                                if (modeIsBuySell || modeIsDelta) 
                                {
                                    if (!Worker_VolumesRank_Up.ContainsKey(priceKey))
                                        Worker_VolumesRank_Up.Add(priceKey, 1);
                                    else
                                        Worker_VolumesRank_Up[priceKey] += 1;

                                    if (!Worker_VolumesRank_Down.ContainsKey(priceKey))
                                        Worker_VolumesRank_Down.Add(priceKey, 1);
                                    else
                                        Worker_VolumesRank_Down[priceKey] += 1;

                                    double value = Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey];
                                    if (!Worker_VolumesRank_Subt.ContainsKey(priceKey))
                                        Worker_VolumesRank_Subt.Add(priceKey, value);
                                    else
                                        Worker_VolumesRank_Subt[priceKey] = value;
                                }

                                if (modeIsDelta) 
                                {
                                    if (!Worker_DeltaRank.ContainsKey(priceKey))
                                        Worker_DeltaRank.Add(priceKey, (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]));
                                    else
                                        Worker_DeltaRank[priceKey] += (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]);
                                }
                            }

                            if (modeIsDelta && ResultParams.ShowMinMaxDelta)
                            {
                                double currentDelta = Worker_DeltaRank.Values.Sum();
                                if (prevDelta > currentDelta)
                                    Worker_MinMaxDelta[0] = prevDelta; // Min
                                if (prevDelta < currentDelta)
                                    Worker_MinMaxDelta[1] = prevDelta; // Max before final delta
                            }

                            break;
                        }
                        prevSegmentValue = segmentsSource[i];
                    }
                }
            }

        }

    }
}
