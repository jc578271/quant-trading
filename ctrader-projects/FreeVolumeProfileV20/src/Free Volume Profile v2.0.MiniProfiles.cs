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
        // *********** MWM PROFILES ***********
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
                        PerformanceSource.startIdx_Mini = PerformanceSource.lastIdx_Mini;
                    
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
                if (GeneralParams.VPInterval_Input == VPInterval_Data.Weekly)
                    return;

                int weekIndex = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                int weekStart = Bars.OpenTimes.GetIndexByTime(WeeklyBars.OpenTimes[weekIndex]);

                if (index == weekStart || (index - 1) == weekStart && isPriceBased_Chart || loopStart)
                {
                    if (!IsLastBar)
                        PerformanceSource.startIdx_Weekly = PerformanceSource.lastIdx_Weekly;
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
                }
            }
        }
        private void CreateMonthlyVP(int index, bool loopStart = false, bool isLoop = false, bool isConcurrent = false) {
            // Avoid recalculating the same period.
            if (GeneralParams.VPInterval_Input == VPInterval_Data.Monthly)
                return;

            if (ProfileParams.EnableMonthlyProfile)
            {
                int monthIndex = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                int monthStart = Bars.OpenTimes.GetIndexByTime(MonthlyBars.OpenTimes[monthIndex]);

                if (index == monthStart || (index - 1) == monthStart && isPriceBased_Chart || loopStart) {
                    if (!IsLastBar)
                        PerformanceSource.startIdx_Monthly = PerformanceSource.lastIdx_Monthly;
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
                }
            }
        }

        // *********** LIVE PROFILE UPDATE ***********
        private void LiveVP_Update(int startIndex, int index, bool onlyMini = false) {
            double price = Bars.ClosePrices[index];

            bool updateStrategy = ProfileParams.UpdateProfile_Input switch {
                UpdateProfile_Data.ThroughSegments_Balanced => Math.Abs(price - prevUpdatePrice) >= rowHeight,
                UpdateProfile_Data.Through_2_Segments_Best => Math.Abs(price - prevUpdatePrice) >= (rowHeight + rowHeight),
                _ => true
            };

            if (updateStrategy || isUpdateVP || configHasChanged)
            {
                if (!onlyMini)
                {
                    if (ProfileParams.EnableMonthlyProfile && GeneralParams.VPInterval_Input != VPInterval_Data.Monthly)
                    {
                        int monthIndex = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                        int monthStart = Bars.OpenTimes.GetIndexByTime(MonthlyBars.OpenTimes[monthIndex]);

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

                    if (ProfileParams.EnableWeeklyProfile && GeneralParams.VPInterval_Input != VPInterval_Data.Weekly)
                    {
                        int weekIndex = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
                        int weekStart = Bars.OpenTimes.GetIndexByTime(WeeklyBars.OpenTimes[weekIndex]);

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

                    if (ProfileParams.EnableMiniProfiles)
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

                    if (index != startIndex)
                    {
                        for (int i = startIndex; i <= index; i++)
                        {
                            if (i == startIndex) {
                                VP_VolumesRank.Clear();
                                VP_VolumesRank_Up.Clear();
                                VP_VolumesRank_Down.Clear();
                                VP_VolumesRank_Subt.Clear();
                                VP_DeltaRank.Clear();
                            }
                            if (i < index)
                                VolumeProfile(startIndex, i, ExtraProfiles.No, true); // Update only
                            else
                                VolumeProfile(startIndex, i, ExtraProfiles.No, false); // Update and Draw
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

            configHasChanged = false;
            isUpdateVP = false;
            if (ProfileParams.UpdateProfile_Input != UpdateProfile_Data.EveryTick_CPU_Workout)
                prevUpdatePrice = price;
        }

        private void LiveVP_Concurrent(int index, int startIndex)
        {
            if (!ProfileParams.EnableMainVP && !ProfileParams.EnableMiniProfiles)
                return;

            double price = Bars.ClosePrices[index];
            bool updateStrategy = ProfileParams.UpdateProfile_Input switch {
                UpdateProfile_Data.ThroughSegments_Balanced => Math.Abs(price - prevUpdatePrice) >= rowHeight,
                UpdateProfile_Data.Through_2_Segments_Best => Math.Abs(price - prevUpdatePrice) >= (rowHeight + rowHeight),
                _ => true
            };

            if (updateStrategy || isUpdateVP || configHasChanged)
            {
                if (Bars.Count > BarsTime_ChartArray.Length)
                {
                    lock (_Locks.Bar)
                        BarsTime_ChartArray = Bars.OpenTimes.ToArray();
                }

                lock (_Locks.Source)
                    BarsSource_List = new List<Bar>(Source_Bars);

                liveVP_RunWorker = true;
            }
            _Tasks.cts ??= new CancellationTokenSource();

            CreateMonthlyVP(index, isConcurrent: true);
            CreateWeeklyVP(index, isConcurrent: true);
            CreateMiniVPs(index, isConcurrent: true);

            if (ProfileParams.EnableMainVP)
            {
                _Tasks.MainVP ??= Task.Run(() => LiveVP_Worker(ExtraProfiles.No, _Tasks.cts.Token));
                LiveVPIndexes.MainVP = startIndex;
                if (index != startIndex) {
                    lock (_Locks.MainVP)
                        VolumeProfile(startIndex, index, ExtraProfiles.No, false, true);
                }
            }
        }

        private void LiveVP_Worker(ExtraProfiles extraID, CancellationToken token)
        {
            /*
            It's quite simple, but gave headaches mostly due to GetByInvoke() unexpected behavior and debugging it.
             - GetByInvoke() will slowdown loops due to accumulative Bars[index] => "0.xx ms" operations
            The major reason why Copy of Time/Bars are used.
            */

            Dictionary<double, double> Worker_VolumesRank = new();
            Dictionary<double, double> Worker_VolumesRank_Up = new();
            Dictionary<double, double> Worker_VolumesRank_Down = new();
            Dictionary<double, double> Worker_VolumesRank_Subt = new();
            Dictionary<double, double> Worker_DeltaRank = new();
            double[] Worker_MinMaxDelta = { 0, 0 };

            DateTime lastTime = new();
            IEnumerable<DateTime> TimesCopy = Array.Empty<DateTime>();
            IEnumerable<Bar> BarsCopy = new List<Bar>();

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
                            TimesCopy = BarsTime_ChartArray.Skip(startIndex);
                        lastTime = lastBarTime;
                    }
                    int endIndex = TimesCopy.Count();

                    // Source Bars => Always replace
                    int startSourceIndex = extraID switch {
                        ExtraProfiles.MiniVP => PerformanceSource.startIdx_Mini,
                        ExtraProfiles.Weekly => PerformanceSource.startIdx_Weekly,
                        ExtraProfiles.Monthly => PerformanceSource.startIdx_Monthly,
                        _ => PerformanceSource.startIdx_MainVP
                    };

                    lock (_Locks.Source)
                        BarsCopy = BarsSource_List.Skip(startSourceIndex);

                    for (int i = 0; i < endIndex; i++)
                    {
                        Worker_VP_Bars(i, extraID, i == (endIndex - 1));
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

                        configHasChanged = false;
                        isUpdateVP = false;

                        if (ProfileParams.UpdateProfile_Input != UpdateProfile_Data.EveryTick_CPU_Workout)
                            prevUpdatePrice = BarsCopy.Last().Close;
                    }
                }
                catch (Exception e) { Print($"CRASH at LiveVP_Worker => {extraID}: {e}"); }

                liveVP_RunWorker = false;
            }

            void Worker_VP_Bars(int index, ExtraProfiles extraVP = ExtraProfiles.No, bool isLastBarLoop = false)
            {
                DateTime startTime = TimesCopy.ElementAt(index);
                DateTime endTime = !isLastBarLoop ? TimesCopy.ElementAt(index + 1) : BarsCopy.Last().OpenTime;
                List<double> whichSegment = Segments_VP;
                
                for (int k = 0; k < BarsCopy.Count(); ++k)
                {
                    Bar volBar = BarsCopy.ElementAt(k);

                    if (volBar.OpenTime < startTime || volBar.OpenTime > endTime)
                    {
                        if (volBar.OpenTime > endTime)
                            break;
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
                    if (!Worker_VolumesRank.ContainsKey(priceKey))
                        Worker_VolumesRank.Add(priceKey, vol);
                    else
                        Worker_VolumesRank[priceKey] += vol;

                    bool condition = GeneralParams.VolumeMode_Input != VolumeMode_Data.Normal;
                    if (condition)
                        Add_BuySell(priceKey, vol, isBullish);
                }
                void Add_BuySell(double priceKey, double vol, bool isBullish)
                {
                    if (isBullish)
                    {
                        if (!Worker_VolumesRank_Up.ContainsKey(priceKey))
                            Worker_VolumesRank_Up.Add(priceKey, vol);
                        else
                            Worker_VolumesRank_Up[priceKey] += vol;
                    }
                    else
                    {
                        if (!Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Down.Add(priceKey, vol);
                        else
                            Worker_VolumesRank_Down[priceKey] += vol;
                    }

                    // Subtract Profile - Plain Delta
                    if (!Worker_VolumesRank_Subt.ContainsKey(priceKey))
                    {
                        if (Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Subt.Add(priceKey, (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]));
                        else if (Worker_VolumesRank_Up.ContainsKey(priceKey) && !Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Subt.Add(priceKey, (Worker_VolumesRank_Up[priceKey]));
                        else if (!Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Subt.Add(priceKey, (-Worker_VolumesRank_Down[priceKey]));
                        else
                            Worker_VolumesRank_Subt.Add(priceKey, 0);
                    }
                    else
                    {
                        if (Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Subt[priceKey] = (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]);
                        else if (Worker_VolumesRank_Up.ContainsKey(priceKey) && !Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Subt[priceKey] = (Worker_VolumesRank_Up[priceKey]);
                        else if (!Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_VolumesRank_Subt[priceKey] = (-Worker_VolumesRank_Down[priceKey]);
                    }

                    if (GeneralParams.VolumeMode_Input != VolumeMode_Data.Delta)
                        return;
                        
                    double prevDelta = Worker_DeltaRank.Values.Sum();
                    if (!Worker_DeltaRank.ContainsKey(priceKey))
                    {
                        if (Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_DeltaRank.Add(priceKey, (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]));
                        else if (Worker_VolumesRank_Up.ContainsKey(priceKey) && !Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_DeltaRank.Add(priceKey, (Worker_VolumesRank_Up[priceKey]));
                        else if (!Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_DeltaRank.Add(priceKey, (-Worker_VolumesRank_Down[priceKey]));
                        else
                            Worker_DeltaRank.Add(priceKey, 0);
                    }
                    else
                    {
                        if (Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_DeltaRank[priceKey] += (Worker_VolumesRank_Up[priceKey] - Worker_VolumesRank_Down[priceKey]);
                        else if (Worker_VolumesRank_Up.ContainsKey(priceKey) && !Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_DeltaRank[priceKey] += (Worker_VolumesRank_Up[priceKey]);
                        else if (!Worker_VolumesRank_Up.ContainsKey(priceKey) && Worker_VolumesRank_Down.ContainsKey(priceKey))
                            Worker_DeltaRank[priceKey] += (-Worker_VolumesRank_Down[priceKey]);

                    }

                    double currentDelta = Worker_DeltaRank.Values.Sum();
                    if (prevDelta > currentDelta)
                        Worker_MinMaxDelta[0] = prevDelta; // Min
                    if (prevDelta < currentDelta)
                        Worker_MinMaxDelta[1] = prevDelta; // Max before final delta
                }
            }

        }

    }
}
