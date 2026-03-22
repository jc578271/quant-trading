/*
--------------------------------------------------------------------------------------------------------------------------------
                        Volume Profile v2.0
                            revision 2

All core features of TPO Profile but in VOLUME
It also has the features of Order Flow Ticks

=== Volume Modes ===
*Normal/Gradient Mode = Volume Profile with Fixed/Gradient Color
*Buy vs Sell Mode = The name explains itself
*Delta Mode = Volume Delta Profile

The Volume Calculation(in Bars Volume Source)
is exported, with adaptations, from the BEST VP I have see/used for MT4/MT5,
of Russian FXcoder's https://gitlab.com/fxcoder-mql/vp (VP 10.1), author of the famous (Volume Profile + Range v6.0)
a BIG THANKS to HIM!

All parameters are self-explanatory.

.NET 6.0+ is Required

What's new in rev. 1? (after ODF_AGG)
- Rewritten using related improvements of ODF_AGG/Volume Profile.
- High-Performance VP_Bars
- Concurrent Live VP Update
- Show Any or All (Mini-VPs/Daily/Weekly/Monthly) Profiles at once!

Last update => 19/01/2026
===========================

What's new in rev. 2? (2026)
- HVN + LVN:
    - Detection:
      - Smoothing => [Gaussian, Savitzky_Golay]
      - Nodes => [LocalMinMax, Topology, Percentile]
      - (Tip) Use "Percentile" for "Savitzky_Golay".
    - Levels(bands)
      - VA-like, set by percentage.
      - (Important!) The "mini-pocs" shown in 'HVN_With_Bands' are derived from LVN splits!
        - Decrease the "(%) <= POC" input of "Only Strong?" when filtering the LVNs or HVN_With_Bands.
        - This 'rule' apply only to [LocalMinMax, Topology].
    - (Tip) Use 'LineStyles = Solid" if any stuttering/lagging occurs when scrolling at profiles on chart (Reduce GPU workload). 
      
- Improved Performance of (all modes):
    - 'VA + POC'
    - 'Results'
    - 'Misc' => 'Distribution' (all options with less O(1) operations)
    
- Add "Segments" to "Volume Profile" => "Fixed Range?" (params-panel):
    - Monthly_Aligned (limited to the current Month)
    - From_Profile (available to any period without the 'bug' between months)
    
===========================

AUTHOR: srlcarlg

== DON"T BE an ASSHOLE SELLING this FREE and OPEN-SOURCE indicator ==
----------------------------------------------------------------------------------------------------------------------------
*/

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
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public partial class FreeVolumeProfileV20 : Indicator
    {
        private NetworkStream _networkStream;
        private Button _exportButton;
        private TcpClient _tcpClient;
        private bool _isManualCsvExportInProgress;
        private const string DefaultCsvOutputFolder = @"D:\projects\quant-trading\logs";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private static readonly string[] ExportCsvHeaders =
        {
            "profile_type",
            "symbol",
            "timeframe",
            "timestamp",
            "open",
            "high",
            "low",
            "close",
            "vpPOC",
            "vpVAH",
            "vpVAL",
            "vpTotalVolume",
            "spread",
            "price_level",
            "volume_total",
            "volume_buy",
            "volume_sell",
            "delta",
            "min_delta",
            "max_delta"
        };
        private const string EventContractSchema = "event-contract/v1";
        private const string EventSource = "ctrader";
        private const string SourceInstanceName = "FreeVolumeProfileV20";
        private const string ExportEventName = "volume_profile";

        private string BuildExportEventId(int index)
        {
            return $"ctrader-{ExportEventName}-{Symbol.Name}-{Bars.OpenTimes[index]:o}";
        }

        private Dictionary<string, object> BuildContractEnvelope(int index, Dictionary<string, object> payload, Dictionary<string, object> sourceMeta)
        {
            return new Dictionary<string, object>
            {
                ["schema"] = EventContractSchema,
                ["source"] = EventSource,
                ["source_instance"] = SourceInstanceName,
                ["event"] = ExportEventName,
                ["event_id"] = BuildExportEventId(index),
                ["instrument"] = Symbol.Name,
                ["timestamp"] = Bars.OpenTimes[index].ToString("o"),
                ["payload"] = payload,
                ["source_meta"] = sourceMeta
            };
        }

        [Parameter("CSV Output Folder", DefaultValue = DefaultCsvOutputFolder, Group = "==== Python AI Export ====")]
        public string CsvOutputFolder { get; set; }

        public enum PanelAlign_Data
        {
            Top_Left,
            Top_Center,
            Top_Right,
            Center_Left,
            Center_Right,
            Bottom_Left,
            Bottom_Center,
            Bottom_Right,
        }
        [Parameter("Panel Position:", DefaultValue = PanelAlign_Data.Bottom_Left, Group = "==== Volume Profile v2.0 ====")]
        public PanelAlign_Data PanelAlign_Input { get; set; }

        public enum StorageKeyConfig_Data
        {
            Symbol_Timeframe,
            Broker_Symbol_Timeframe
        }
        [Parameter("Storage By:", DefaultValue = StorageKeyConfig_Data.Broker_Symbol_Timeframe, Group = "==== Volume Profile v2.0 ====")]
        public StorageKeyConfig_Data StorageKeyConfig_Input { get; set; }

        public enum RowConfig_Data
        {
            ATR,
            Custom,
        }
        [Parameter("Row Config:", DefaultValue = RowConfig_Data.ATR, Group = "==== Volume Profile v2.0 ====")]
        public RowConfig_Data RowConfig_Input { get; set; }

        [Parameter("Custom Row(pips):", DefaultValue = 0.2, MinValue = 0.2, Group = "==== Volume Profile v2.0 ====")]
        public double CustomHeightInPips { get; set; }


        [Parameter("ATR Period:", DefaultValue = 5, MinValue = 1, Group = "==== ATR Row Config ====")]
        public int ATRPeriod { get; set; }

        [Parameter("Row Detail(%):", DefaultValue = 60, MinValue = 20, MaxValue = 100, Group = "==== ATR Row Config ====")]
        public int RowDetailATR { get; set; }

        [Parameter("Replace Loaded Row?", DefaultValue = false, Group = "==== ATR Row Config ====")]
        public bool ReplaceByATR { get; set; }


        public enum UpdateStrategy_Data
        {
            Concurrent,
            SameThread_MayFreeze
        }
        [Parameter("[VP] Update Strategy", DefaultValue = UpdateStrategy_Data.Concurrent, Group = "==== Specific Parameters ====")]
        public UpdateStrategy_Data UpdateStrategy_Input { get; set; }

        public enum LoadBarsStrategy_Data
        {
            Sync,
            Async
        }
        [Parameter("[Source] Load Type:", DefaultValue = LoadBarsStrategy_Data.Async, Group = "==== Specific Parameters ====")]
        public LoadBarsStrategy_Data LoadBarsStrategy_Input { get; set; }

        [Parameter("[Gradient] Opacity:", DefaultValue = 60, MinValue = 5, MaxValue = 100, Group = "==== Specific Parameters ====")]
        public int OpacityHistInput { get; set; }


        [Parameter("Show Controls at Zoom(%):", DefaultValue = 10, Group = "==== Fixed Range ====")]
        public int FixedHiddenZoom { get; set; }

        [Parameter("Show Info?", DefaultValue = true, Group = "==== Fixed Range ====")]
        public bool ShowFixedInfo { get; set; }

        [Parameter("Rectangle Color:", DefaultValue = "#6087CEEB", Group = "==== Fixed Range ====")]
        public Color FixedColor { get; set; }


        [Parameter("Font Size Results:", DefaultValue = 10, MinValue = 1, MaxValue = 80, Group = "==== Results ====")]
        public int FontSizeResults { get; set; }

        [Parameter("Format Results?", DefaultValue = true, Group = "==== Results ====")]
        public bool FormatResults { get; set; }

        public enum FormatMaxDigits_Data
        {
            Zero,
            One,
            Two,
        }
        [Parameter("Format Max Digits:", DefaultValue = FormatMaxDigits_Data.One, Group = "==== Results ====")]
        public FormatMaxDigits_Data FormatMaxDigits_Input { get; set; }


        [Parameter("Normal Color", DefaultValue = "#B287CEEB", Group = "==== Colors Histogram ====")]
        public Color HistColor  { get; set; }

        [Parameter("Gradient Color Min. Vol:", DefaultValue = "RoyalBlue", Group = "==== Colors Histogram ====")]
        public Color ColorGrandient_Min { get; set; }

        [Parameter("Gradient Color Max. Vol:", DefaultValue = "OrangeRed", Group = "==== Colors Histogram ====")]
        public Color ColorGrandient_Max { get; set; }

        [Parameter("Color Buy:", DefaultValue = "#9900BFFF", Group = "==== Colors Histogram ====")]
        public Color BuyColor  { get; set; }

        [Parameter("Color Sell:", DefaultValue = "#99DC143C", Group = "==== Colors Histogram ====")]
        public Color SellColor  { get; set; }

        [Parameter("OHLC Bar Color:", DefaultValue = "Gray", Group = "==== Colors Histogram ====")]
        public Color ColorOHLC { get; set; }


        [Parameter("Weekly Color:", DefaultValue = "#B2FFD700", Group = "==== WM Profiles ====")]
        public Color WeeklyColor { get; set; }

        [Parameter("Monthly Color:", DefaultValue = "#920071C1", Group = "==== WM Profiles ====")]
        public Color MonthlyColor { get; set; }

        [Parameter("Weekly Gradient Min. Vol:", DefaultValue = "#FFFF9900", Group = "==== WM Profiles ====")]
        public Color WeeklyGrandient_Min { get; set; }

        [Parameter("Weekly Color Max. Vol:", DefaultValue = "#FFE42226", Group = "==== WM Profiles ====")]
        public Color WeeklyGrandient_Max { get; set; }

        [Parameter("Monthly Gradient Min. Vol:", DefaultValue = "#FF090979", Group = "==== WM Profiles ====")]
        public Color MonthlyGrandient_Min { get; set; }

        [Parameter("Monthly Color Max. Vol:", DefaultValue = "#FF33C1F3", Group = "==== WM Profiles ====")]
        public Color MonthlyGrandient_Max { get; set; }


        [Parameter("Color POC:", DefaultValue = "D0FFD700", Group = "==== Point of Control ====")]
        public Color ColorPOC { get; set; }

        [Parameter("LineStyle POC:", DefaultValue = LineStyle.Lines, Group = "==== Point of Control ====")]
        public LineStyle LineStylePOC { get; set; }

        [Parameter("Thickness POC:", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "==== Point of Control ====")]
        public int ThicknessPOC { get; set; }


        [Parameter("Color VA:", DefaultValue = "#19F0F8FF", Group = "==== Value Area ====")]
        public Color VAColor  { get; set; }

        [Parameter("Color VAH:", DefaultValue = "PowderBlue", Group = "==== Value Area ====")]
        public Color ColorVAH { get; set; }

        [Parameter("Color VAL:", DefaultValue = "PowderBlue", Group = "==== Value Area ====")]
        public Color ColorVAL { get; set; }

        [Parameter("Opacity VA", DefaultValue = 10, MinValue = 5, MaxValue = 100, Group = "==== Value Area ====")]
        public int OpacityVA { get; set; }

        [Parameter("LineStyle VA:", DefaultValue = LineStyle.LinesDots, Group = "==== Value Area ====")]
        public LineStyle LineStyleVA { get; set; }

        [Parameter("Thickness VA:", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "==== Value Area ====")]
        public int ThicknessVA { get; set; }

        
        [Parameter("Color HVN:", DefaultValue = "#DFFFD700" , Group = "==== HVN/LVN ====")]
        public Color ColorHVN { get; set; }
        
        [Parameter("LineStyle HVN:", DefaultValue = LineStyle.LinesDots, Group = "==== HVN/LVN ====")]
        public LineStyle LineStyleHVN { get; set; }

        [Parameter("Thickness HVN:", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "==== HVN/LVN ====")]
        public int ThicknessHVN { get; set; }

        [Parameter("Color LVN:", DefaultValue = "#DFDC143C", Group = "==== HVN/LVN ====")]
        public Color ColorLVN { get; set; }

        [Parameter("LineStyle LVN:", DefaultValue = LineStyle.LinesDots, Group = "==== HVN/LVN ====")]
        public LineStyle LineStyleLVN { get; set; }

        [Parameter("Thickness LVN:", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "==== HVN/LVN ====")]
        public int ThicknessLVN { get; set; }


        [Parameter("Color Band:", DefaultValue = "#19F0F8FF",  Group = "==== Symmetric Bands (HVN/LVN) ====")]
        public Color ColorBand { get; set; }
        
        [Parameter("Color Lower:", DefaultValue = "#6CB0E0E6",  Group = "==== Symmetric Bands (HVN/LVN) ====")]
        public Color ColorBand_Lower { get; set; }

        [Parameter("Color Upper:", DefaultValue = "#6CB0E0E6",  Group = "==== Symmetric Bands (HVN/LVN) ====")]
        public Color ColorBand_Upper { get; set; }

        [Parameter("LineStyle Bands:", DefaultValue = LineStyle.DotsVeryRare, Group = "==== Symmetric Bands (HVN/LVN) ====")]
        public LineStyle LineStyleBands { get; set; }

        [Parameter("Thickness Bands:", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "==== Symmetric Bands (HVN/LVN) ====")]
        public int ThicknessBands { get; set; }
        

        [Parameter("Developed for cTrader/C#", DefaultValue = "by srlcarlg", Group = "==== Credits ====")]
        public string Credits { get; set; }

        // Moved from cTrader Input to Params Panel

        // ==== General ====
        public enum VolumeMode_Data
        {
            Normal,
            Buy_Sell,
            Delta,
        }
        public enum VPInterval_Data
        {
            Daily,
            Weekly,
            Monthly
        }

        public class GeneralParams_Info {
            public int Lookback = 1;
            public VolumeMode_Data VolumeMode_Input = VolumeMode_Data.Normal;
            public VPInterval_Data VPInterval_Input = VPInterval_Data.Daily;
        }
        public GeneralParams_Info GeneralParams = new();


        // ==== Volume Profile ====
        public enum Distribution_Data
        {
            OHLC,
            OHLC_No_Avg,
            Open,
            High,
            Low,
            Close,
            Uniform_Distribution,
            Uniform_Presence,
            Parabolic_Distribution,
            Triangular_Distribution,
        }
        public enum UpdateProfile_Data
        {
            EveryTick_CPU_Workout,
            ThroughSegments_Balanced,
            Through_2_Segments_Best,
        }
        public enum HistSide_Data
        {
            Left,
            Right,
        }
        public enum HistWidth_Data
        {
            _15,
            _30,
            _50,
            _70,
            _100
        }
        // Allow "old" segmentation "From_Profile", 
        // so the "Fixed Range" doesn't "bug" => remains on chart between months (end/start of each month)
        public enum SegmentsFixedRange_Data
        {
            Monthly_Aligned,
            From_Profile
        }       
        
        public class ProfileParams_Info {
            public bool EnableMainVP = false;
            public Distribution_Data Distribution_Input = Distribution_Data.OHLC;
            public TimeFrame Source_Timeframe = TimeFrame.Minute;

            // View
            public UpdateProfile_Data UpdateProfile_Input = UpdateProfile_Data.Through_2_Segments_Best;
            public bool FillHist_VP = true;
            public HistSide_Data HistogramSide_Input = HistSide_Data.Left;
            public HistWidth_Data HistogramWidth_Input = HistWidth_Data._70;
            public bool EnableGradient = true;
            public bool ShowOHLC = false;

            // FWM Profiles
            public bool EnableFixedRange = false;
            public SegmentsFixedRange_Data SegmentsFixedRange_Input = SegmentsFixedRange_Data.From_Profile;
            public bool EnableWeeklyProfile = false;
            public bool EnableMonthlyProfile = false;

            // Intraday Profiles
            public bool ShowIntradayProfile = false;
            public bool ShowIntradayNumbers = false;
            public int OffsetBarsInput = 1;
            public TimeFrame OffsetTimeframeInput = TimeFrame.Hour;
            public bool FillIntradaySpace = false;

            // Mini VPs
            public bool EnableMiniProfiles = true;
            public TimeFrame MiniVPs_Timeframe = TimeFrame.Hour4;
            public bool ShowMiniResults = true;
        }
        public ProfileParams_Info ProfileParams = new();


        // ==== VA + POC ====
        public class VAParams_Info {
            public bool ShowVA = false;
            public int PercentVA = 65;
            public bool KeepPOC = false;
            public bool ExtendPOC = false;
            public bool ExtendVA = false;
            public int ExtendCount = 1;
        }
        public VAParams_Info VAParams = new();
        
        
        // ==== HVN + LVN ====
        public enum ProfileSmooth_Data
        {
            Gaussian,
            Savitzky_Golay
        }
        public enum ProfileNode_Data
        {
            LocalMinMax,
            Topology,
            Percentile
        }
        public enum ShowNode_Data
        {
            HVN_With_Bands,
            HVN_Raw,
            LVN_With_Bands,
            LVN_Raw
        }
        public class NodesParams_Info {

            public bool EnableNodeDetection = true;

            public ProfileSmooth_Data ProfileSmooth_Input = ProfileSmooth_Data.Gaussian;
            public ProfileNode_Data ProfileNode_Input = ProfileNode_Data.LocalMinMax;

            public ShowNode_Data ShowNode_Input = ShowNode_Data.LVN_With_Bands;
            public int pctileHVN_Value = 90;
            public int pctileLVN_Value = 25;

            public bool onlyStrongNodes = false;
            public double strongHVN_Pct = 23.6;
            public double strongLVN_Pct = 55.3;

            public double bandHVN_Pct = 61.8;
            public double bandLVN_Pct = 23.6;

            public bool extendNodes = false;
            public int extendNodes_Count = 1;
            public bool extendNodes_WithBands = false;
            public bool extendNodes_FromStart = true;
        }
        public NodesParams_Info NodesParams = new();


        // ==== Results ====
        public enum OperatorBuySell_Data
        {
            Sum,
            Subtraction,
        }

        public class ResultParams_Info {
            public bool ShowResults = true;

            public OperatorBuySell_Data OperatorBuySell_Input = OperatorBuySell_Data.Subtraction;

            public bool ShowMinMaxDelta = false;
            public bool ShowOnlySubtDelta = true;
        }
        public ResultParams_Info ResultParams = new();
        

        // Always Monthly
        public enum SegmentsInterval_Data
        {
            Daily,
            Weekly,
            Monthly
        }
        public SegmentsInterval_Data SegmentsInterval_Input = SegmentsInterval_Data.Monthly;

        // ======================================================

        public readonly string NOTIFY_CAPTION = "Free Volume Profile \n    v2.0";

        private readonly VerticalAlignment V_Align = VerticalAlignment.Top;
        private readonly HorizontalAlignment H_Align = HorizontalAlignment.Center;

        // Segments
        private class SegmentsExtremumInfo
        {
            public double LastHighest;
            public double LastLowest;
        }
        // intKey is the intervalIndex
        // value is the last updated Highest/Lowest
        private readonly Dictionary<int, SegmentsExtremumInfo> segmentInfo = new();
        private readonly Dictionary<int, List<double>> segmentsDict = new();
        private readonly Dictionary<string, List<double>> segmentsFromProfile = new();
        private List<double> Segments_VP = new();

        // Volume Profile Bars
        private Dictionary<double, double> VP_VolumesRank = new();
        private Dictionary<double, double> VP_VolumesRank_Up = new();
        private Dictionary<double, double> VP_VolumesRank_Down = new();
        private Dictionary<double, double> VP_VolumesRank_Subt = new();
        private Dictionary<double, double> VP_DeltaRank = new();
        private double[] VP_MinMaxDelta = { 0, 0 };

        // Weekly, Monthly and Mini VPs
        public class VolumeRankType
        {
            public Dictionary<double, double> Normal { get; set; } = new();
            public Dictionary<double, double> Up { get; set; } = new();
            public Dictionary<double, double> Down { get; set; } = new();
            public Dictionary<double, double> Delta { get; set;  } = new();
            public double[] MinMaxDelta { get; set; } = new double[2];

            public void ClearAllModes() {

                Dictionary<double, double>[] _all = new[] {
                    Normal, Up, Down, Delta,
                };

                foreach (var dict in _all)
                    dict.Clear();

                double[] resetDelta = {0, 0};
                MinMaxDelta = resetDelta;
            }
        }
        private readonly VolumeRankType MonthlyRank = new();
        private readonly VolumeRankType WeeklyRank = new();
        private readonly VolumeRankType MiniRank = new();
        private readonly Dictionary<string, VolumeRankType> FixedRank = new();

        // Fixed Range Profile
        public class RangeObjs_Info {
            public List<ChartRectangle> rectangles = new();
            public Dictionary<string, List<ChartText>> infoObjects = new();
            public Dictionary<string, Border> controlGrids = new();
        }
        private readonly RangeObjs_Info RangeObjs = new();

        // HVN + LVN => Performance
        public double[] nodesKernel = null;

        private Bars MiniVPs_Bars;
        private Bars DailyBars;
        private Bars WeeklyBars;
        private Bars MonthlyBars;

        public enum ExtraProfiles {
            No,
            MiniVP,
            Weekly,
            Monthly,
            Fixed
        }

        /*
          Its a annoying behavior that happens even in Candles Chart (Time-Based) on any symbol/broker.
          where it's jump/pass +1 index when .GetIndexByTime is used... the exactly behavior of Price-Based Charts
          Seems to happen only in Lower Timeframes (<=´Daily)
          So, to ensure that it works flawless, an additional verification is needed.
        */
        public class CleanedIndex {
            public int MainVP = 0;
            public int Mini = 0;
            public void ResetAll() {
                MainVP = 0;
                Mini = 0;
            }
        }
        private readonly CleanedIndex ClearIdx = new();


        // Concurrent Live VP Update
        private class LockObjs_Info {
            public readonly object Source = new();
            public readonly object Bar = new();
            public readonly object MainVP = new();
            public readonly object WeeklyVP = new();
            public readonly object MonthlyVP = new();
            public readonly object MiniVP = new();
        }
        private readonly LockObjs_Info _Locks = new();


        private class TaskObjs_Info {
            public CancellationTokenSource cts;
            public Task MainVP;
            public Task WeeklyVP;
            public Task MonthlyVP;
            public Task MiniVP;
        }
        private readonly TaskObjs_Info _Tasks = new();
        
        private bool liveVP_RunWorker = false;

        public class LiveVPIndex {
            public int MainVP { get; set; }
            public int Mini { get; set; }
            public int Weekly { get; set; }
            public int Monthly { get; set; }
        }
        private readonly LiveVPIndex LiveVPIndexes = new();
        private List<Bar> BarsSource_List = new();
        private DateTime[] BarsTime_ChartArray = Array.Empty<DateTime>();

        // High-Performance VP_Bars()
        public class PerfSourceIndex {
            public int startIdx_MainVP = 0;
            public int startIdx_Mini = 0;
            public int startIdx_Weekly = 0;
            public int startIdx_Monthly = 0;

            public int lastIdx_MainVP = 0;
            public int lastIdx_Mini = 0;
            public int lastIdx_Weekly = 0;
            public int lastIdx_Monthly = 0;

            public void ResetAll() {
                lastIdx_MainVP = 0;
                lastIdx_Mini = 0;
                lastIdx_Weekly = 0;
                lastIdx_Monthly = 0;
            }
        }
        private readonly PerfSourceIndex PerformanceSource = new();
        
        private Bars Source_Bars;

        // Source Volume
        public class SourceObjs_Info {
            public PopupNotification asyncBarsPopup = null;
            public bool startAsyncLoading = false;
            public bool isLoadingComplete = false;
        }
        private readonly SourceObjs_Info SourceObjs = new();
        
        // Timer
        private class TimerHandler {
            public bool isAsyncLoading = false;
        }
        private readonly TimerHandler timerHandler = new();

        // Shared rowHeight
        private double rowHeight = 0;
        private double heightPips = 4;
        public double heightATR = 4;

        // Some required utils
        private double prevUpdatePrice;
        private bool configHasChanged = false;
        private bool isUpdateVP = false;
        private bool isEndChart = false;
        public bool isPriceBased_Chart = false;
        public bool isRenkoChart = false;

        // Params Panel
        private Border ParamBorder;
        public class IndicatorParams
        {
            public GeneralParams_Info GeneralParams { get; set; }
            public double RowHeightInPips { get; set; }
            public ProfileParams_Info ProfileParams { get; set; }
            public VAParams_Info VAParams { get; set; }
            public NodesParams_Info NodesParams { get; set; }
            public ResultParams_Info ResultParams { get; set; }
        }

    }

}
