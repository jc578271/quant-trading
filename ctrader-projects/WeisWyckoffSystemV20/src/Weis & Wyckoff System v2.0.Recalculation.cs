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
        public void ClearAndRecalculate()
        {
            Thread.Sleep(300);

            Design_Templates();
            SpecificChart_Templates(false);
            DrawingConflict();

            if (!ShowTrendLines) {
                for (int i = 0; i < Bars.Count; i++)
                {
                    if (!double.IsNaN(ZigZagBuffer[i]))
                        ZigZagBuffer[i] = double.NaN;
                }
            }
            // Reset Zigzag.
            ZigZagObjs.extremumPrice = 0;
            lockMTFNotify = false;
            // Reset Tick Index.
            PerformanceTick.ResetAll();
            // Reset Drawings
            PerfDrawingObjs.ClearAll();

            int firstLoadedTick = Bars.OpenTimes.GetIndexByTime(TicksOHLC.OpenTimes.FirstOrDefault());
            int startIndex = UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart ? 0 : firstLoadedTick;
            int endIndex = Bars.Count;
            for (int index = startIndex; index < endIndex; index++)
            {
                if (!UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart || BooleanUtils.isPriceBased_Chart) {
                    if (index < firstLoadedTick) {
                        Chart.SetBarColor(index, HeatmapLowest_Color);
                        continue;
                    }
                }

                if (UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart)
                    VolumeSeries[index] = Bars.TickVolumes[index];
                else
                    VolumeSeries[index] = Get_Volume_or_Wicks(index, true)[2];

                if (WyckoffParams.EnableWyckoff)
                    WyckoffAnalysis(index);

                // Catch MTF ZigZag < Current timeframe (ArgumentOutOfRangeException, index)
                try { WeisWaveAnalysis(index); } catch {
                    if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF && !lockMTFNotify) {
                        Notifications.ShowPopup(
                            NOTIFY_CAPTION,
                            $"ERROR => ZigZag MTF(source): \nCannot use {ZigZagParams.MTFSource_TimeFrame.ShortName} interval for {Chart.TimeFrame.ShortName} chart \nThe interval is probably too short?",
                            PopupNotificationState.Error
                        );
                        lockMTFNotify = true;
                    }
                }

                if (ShowWicks && BooleanUtils.isRenkoChart)
                    RenkoWicks(index);

                if (ExportHistory)
                    ExportCsvData(index);
            }

            if (!UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart || BooleanUtils.isPriceBased_Chart)
                DrawStartVolumeLine();
            try { PerformanceDrawing(true); } catch { } // Draw without scroll or zoom
        }

        public void SetMTFSource_TimeFrame(TimeFrame timeFrame) {
            ZigZagParams.MTFSource_TimeFrame = timeFrame;
            MTFSource_Bars = MarketData.GetBars(timeFrame);
        }
    }
}
