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
        // *********** INTERVAL SEGMENTS ***********
        /*
            In order to optimize Volume Profile and reduce CPU worload
            as well as create the possiblity to:
                - See Weekly and/or Monthly "Intraday" Profile
                - use Aligned Segments at Higher Timeframes (D1 to D3)
            Segments will be calculated outside VolumeProfile()
            and updated at new High/Low of its interval [D1, W1, M1]
        */
        private void CreateSegments(int index) {

            // ==== Highest and Lowest ====
            int TF_idx;
            double open, highest, lowest;

            switch (SegmentsInterval_Input)
            {
                case SegmentsInterval_Data.Weekly:
                    TF_idx = WeeklyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

                    highest = WeeklyBars.HighPrices[TF_idx];
                    lowest = WeeklyBars.LowPrices[TF_idx];
                    open = WeeklyBars.OpenPrices[TF_idx];
                    break;
                case SegmentsInterval_Data.Monthly:
                    TF_idx = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

                    highest = MonthlyBars.HighPrices[TF_idx];
                    lowest = MonthlyBars.LowPrices[TF_idx];
                    open = MonthlyBars.OpenPrices[TF_idx];
                    break;
                default:
                    TF_idx = DailyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

                    highest = DailyBars.HighPrices[TF_idx];
                    lowest = DailyBars.LowPrices[TF_idx];
                    open = DailyBars.OpenPrices[TF_idx];
                    break;
            }

            // Add indexKey if not present
            int startKey = TF_idx;
            if (!segmentInfo.ContainsKey(startKey)) {
                segmentInfo.Add(startKey, new SegmentsExtremumInfo {
                    LastHighest = highest,
                    LastLowest = lowest
                });
                updateSegments();
            }
            else {
                // Update the entirely Segments
                // when a new High/Low is made.
                if (segmentInfo[startKey].LastHighest != highest) {
                    updateSegments();
                    segmentInfo[startKey].LastHighest = highest;
                }

                if (segmentInfo[startKey].LastLowest != lowest) {
                    updateSegments();
                    segmentInfo[startKey].LastLowest = lowest;
                }

                if (!segmentsDict.ContainsKey(startKey))
                    segmentsDict.Add(startKey, Segments_VP);
                else
                    segmentsDict[startKey] = Segments_VP;
            }

            void updateSegments() {
                List<double> currentSegments = new();

                // ==== Chart Segmentation ====
                double prev_segment = open;
                while (prev_segment >= (lowest - rowHeight))
                {
                    currentSegments.Add(prev_segment);
                    prev_segment = Math.Abs(prev_segment - rowHeight);
                }
                prev_segment = open;
                while (prev_segment <= (highest + rowHeight))
                {
                    currentSegments.Add(prev_segment);
                    prev_segment = Math.Abs(prev_segment + rowHeight);
                }

                Segments_VP = currentSegments.OrderBy(x => x).ToList();
            }
        }
        private void CreateSegments_FromFixedRange(double open, double lowest, double highest, string fixedKey) {
            List<double> currentSegments = new();

            // ==== Chart Segmentation ====
            double prev_segment = open;
            while (prev_segment >= (lowest - rowHeight))
            {
                currentSegments.Add(prev_segment);
                prev_segment = Math.Abs(prev_segment - rowHeight);
            }
            prev_segment = open;
            while (prev_segment <= (highest + rowHeight))
            {
                currentSegments.Add(prev_segment);
                prev_segment = Math.Abs(prev_segment + rowHeight);
            }

            currentSegments = currentSegments.OrderBy(x => x).ToList();
        
            if (!segmentsFromProfile.ContainsKey(fixedKey))
                segmentsFromProfile.Add(fixedKey, currentSegments);
            else
                segmentsFromProfile[fixedKey] = currentSegments;
        }
        private List<double> GetRangeSegments(int TF_idx, string fixedKey) 
        {
            if (ProfileParams.SegmentsFixedRange_Input == SegmentsFixedRange_Data.From_Profile)
                return segmentsFromProfile[fixedKey];
            else
                return segmentsDict[TF_idx];
        }


    }
}
