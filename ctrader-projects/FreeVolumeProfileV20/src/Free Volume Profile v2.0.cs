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
    public class FreeVolumeProfileV20 : Indicator
    {
        private NetworkStream _networkStream;
        private Button _exportButton;
        private TcpClient _tcpClient;
        private bool _isManualCsvExportInProgress;
        private const string DefaultCsvOutputFolder = @"D:\projects\quant-trading";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private static readonly string[] ExportCsvHeaders =
        {
            "type",
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
            "volumesRank",
            "volumesRankUp",
            "volumesRankDown",
            "deltaRank",
            "minMaxDelta",
            "spread"
        };

        [Parameter("Export History Data", DefaultValue = true, Group = "==== Python AI Export ====")]
        public bool ExportHistory { get; set; }

        [Parameter("Direct CSV Export", DefaultValue = true, Group = "==== Python AI Export ====")]
        public bool DirectCsvExport { get; set; }

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

        private void AddHiddenButton(Panel panel, Color btnColor)
        {
            Button button = new()
            {
                Text = "VP",
                Padding = 0,
                Height = 22,
                Width = 30, // Fix MacOS => stretching button when StackPanel is used.
                Margin = 2,
                BackgroundColor = btnColor
            };
            button.Click += HiddenEvent;
            panel.AddChild(button);
        }

        private void AddExportButton(Panel panel, Color btnColor)
        {
            _exportButton = new Button()
            {
                Text = "Export",
                Padding = 0,
                Height = 22,
                Width = 50,
                Margin = 2,
                BackgroundColor = btnColor
            };
            _exportButton.Click += ExportEvent;
            panel.AddChild(_exportButton);
        }

        private void HiddenEvent(ButtonClickEventArgs obj)
        {
            if (ParamBorder.IsVisible)
                ParamBorder.IsVisible = false;
            else
                ParamBorder.IsVisible = true;
        }

        private void ConnectSocket()
        {
            if (_tcpClient != null && _tcpClient.Connected) return;

            try {
                if (_tcpClient != null) {
                    try { _networkStream?.Close(); } catch {}
                    try { _tcpClient.Close(); } catch {}
                }
                _tcpClient = new TcpClient("127.0.0.1", 5555);
                _networkStream = _tcpClient.GetStream();
                Print("Successfully connected to Python Socket (OrderFlow Exporter)");
            } catch (Exception ex) {
                Print("Socket Error: " + ex.Message);
            }
        }

        private void ExportEvent(ButtonClickEventArgs obj)
        {
            _exportButton.IsEnabled = false;
            try
            {
                ConnectSocket();

                bool originalExport = ExportHistory;
                ExportHistory = true;
                _isManualCsvExportInProgress = true;

                Print("Starting Volume Profile Export...");
                ClearAndRecalculate();
                Print("Volume Profile Export Finished.");

                ExportHistory = originalExport;
            }
            catch (Exception ex)
            {
                Print("Export Error: " + ex.Message);
            }
            finally
            {
                _isManualCsvExportInProgress = false;
                _exportButton.IsEnabled = true;
            }
        }

        protected override void Initialize()
        {
            ConnectSocket();

            // ========== Predefined Config ==========
            if (RowConfig_Input == RowConfig_Data.ATR && (Chart.TimeFrame >= TimeFrame.Minute && Chart.TimeFrame <= TimeFrame.Day3))
            {
                if (Chart.TimeFrame >= TimeFrame.Minute && Chart.TimeFrame <= TimeFrame.Minute4)
                {
                    if (Chart.TimeFrame == TimeFrame.Minute)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour;
                    else if (Chart.TimeFrame == TimeFrame.Minute2)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour2;
                    else if (Chart.TimeFrame <= TimeFrame.Minute4)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour3;
                }
                else if (Chart.TimeFrame >= TimeFrame.Minute5 && Chart.TimeFrame <= TimeFrame.Minute10)
                {
                    if (Chart.TimeFrame == TimeFrame.Minute5)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour4;
                    else if (Chart.TimeFrame == TimeFrame.Minute6)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour6;
                    else if (Chart.TimeFrame <= TimeFrame.Minute8)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour8;
                    else if (Chart.TimeFrame <= TimeFrame.Minute10)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Hour12;
                }
                else if (Chart.TimeFrame >= TimeFrame.Minute15 && Chart.TimeFrame <= TimeFrame.Hour8)
                {
                    if (Chart.TimeFrame >= TimeFrame.Minute15 && Chart.TimeFrame <= TimeFrame.Hour)
                        ProfileParams.MiniVPs_Timeframe = TimeFrame.Daily;

                    else if (Chart.TimeFrame <= TimeFrame.Hour8) {
                        ProfileParams.EnableMainVP = true;
                        ProfileParams.EnableMiniProfiles = false;
                        GeneralParams.VPInterval_Input = VPInterval_Data.Weekly;
                    }
                }
                else if (Chart.TimeFrame >= TimeFrame.Hour12 && Chart.TimeFrame <= TimeFrame.Weekly) {
                    ProfileParams.EnableMainVP = true;
                    ProfileParams.EnableMiniProfiles = false;
                    GeneralParams.VPInterval_Input = VPInterval_Data.Monthly;
                }
            }

            if (RowConfig_Input == RowConfig_Data.Custom)
                heightPips = CustomHeightInPips;
            else {
                // Math Formulas by LLM
                // Manual coding with adaptations for cTrader Algo API.
                // The idea is => Set the rowHeight for any symbol with [1, 2, 5] digits with fewer hard-coded values.
                AverageTrueRange atr = Indicators.AverageTrueRange(ATRPeriod, MovingAverageType.Exponential);
                double atrInTick = atr.Result.LastValue / Symbol.TickSize;
                double priceInTick = Bars.LastBar.Close / Symbol.TickSize;

                // Original => (smaATRInTick * targetRows) / smaPriceInTick;
                // However, Initialize() already has a lot of heavy things to start (Tick / Filters / Panel),
                // Plus, the current approach is good enough and gives slightly/better higher numbers.
                double K_Factor = (atrInTick * RowDetailATR) / priceInTick;
                double rowSizeInTick = (atrInTick * atrInTick) / (K_Factor * priceInTick);

                // Original => Math.Max(1, Math.Round(rowSizeInTick, 2)) * (Symbol.TickSize / Symbol.PipSize)
                // Should 'never' go bellow 0.3 pips.
                double rowSizePips = Math.Max(0.3, Math.Round(rowSizeInTick, 2));
                heightPips = rowSizePips;
                heightATR = rowSizePips;
            }

            // Define rowHeight by Pips
            rowHeight = Symbol.PipSize * heightPips;

            // Load all at once, mostly due to:
            // Loading parameters that have it
            DailyBars = MarketData.GetBars(TimeFrame.Daily);
            WeeklyBars = MarketData.GetBars(TimeFrame.Weekly);
            MonthlyBars = MarketData.GetBars(TimeFrame.Monthly);
            MiniVPs_Bars = MarketData.GetBars(ProfileParams.MiniVPs_Timeframe);
            Source_Bars = MarketData.GetBars(ProfileParams.Source_Timeframe);

            // Concurrent Live VP
            Bars.BarOpened += (_) => {
                isUpdateVP = true;
                if (ProfileParams.UpdateProfile_Input != UpdateProfile_Data.EveryTick_CPU_Workout)
                    prevUpdatePrice = _.Bars.LastBar.Close;
            };

            // Chart
            string currentTimeframe = Chart.TimeFrame.ToString();
            isPriceBased_Chart = currentTimeframe.Contains("Renko") || currentTimeframe.Contains("Range") || currentTimeframe.Contains("Tick");
            isRenkoChart = Chart.TimeFrame.ToString().Contains("Renko");

            DrawStartVolumeLine();

            DrawOnScreen("Calculating...");
            Second_DrawOnScreen($"Taking too long? You can: \n 1) Increase the rowHeight \n 2) Disable the Value Area (High Performance)");

            // Fixed Range Profiles
            RangeInitialize();

            // PARAMS PANEL
            VerticalAlignment vAlign = VerticalAlignment.Bottom;
            HorizontalAlignment hAlign = HorizontalAlignment.Right;

            switch (PanelAlign_Input)
            {
                case PanelAlign_Data.Bottom_Left:
                    hAlign = HorizontalAlignment.Left;
                    break;
                case PanelAlign_Data.Top_Left:
                    vAlign = VerticalAlignment.Top;
                    hAlign = HorizontalAlignment.Left;
                    break;
                case PanelAlign_Data.Top_Right:
                    vAlign = VerticalAlignment.Top;
                    hAlign = HorizontalAlignment.Right;
                    break;
                case PanelAlign_Data.Center_Right:
                    vAlign = VerticalAlignment.Center;
                    hAlign = HorizontalAlignment.Right;
                    break;
                case PanelAlign_Data.Center_Left:
                    vAlign = VerticalAlignment.Center;
                    hAlign = HorizontalAlignment.Left;
                    break;
                case PanelAlign_Data.Top_Center:
                    vAlign = VerticalAlignment.Top;
                    hAlign = HorizontalAlignment.Center;
                    break;
                case PanelAlign_Data.Bottom_Center:
                    vAlign = VerticalAlignment.Bottom;
                    hAlign = HorizontalAlignment.Center;
                    break;
            }

            IndicatorParams DefaultParams = new()
            {
                GeneralParams = GeneralParams,
                RowHeightInPips = heightPips,
                ProfileParams = ProfileParams,
                VAParams = VAParams,
                NodesParams = NodesParams,
                ResultParams = ResultParams,
            };

            ParamsPanel ParamPanel = new(this, DefaultParams);

            ParamBorder = new()
            {
                VerticalAlignment = vAlign,
                HorizontalAlignment = hAlign,
                Style = Styles.CreatePanelBackgroundStyle(),
                Margin = "20 40 20 20",
                // ParamsPanel - Lock Width
                Width = 290,
                Child = ParamPanel
            };
            Chart.AddControl(ParamBorder);

            StackPanel stackPanel = new() {
                VerticalAlignment = vAlign,
                HorizontalAlignment = hAlign,
            };
            AddHiddenButton(stackPanel, Color.FromHex("#7F808080"));
            AddExportButton(stackPanel, Color.FromHex("#7F808080"));
            Chart.AddControl(stackPanel);
        }

        public override void Calculate(int index)
        {
            // Removing Messages
            if (!IsLastBar) {
                DrawOnScreen("");
                Second_DrawOnScreen("");
            }

            // Chart Segmentation
            CreateSegments(index);

            // WM
            if (!IsLastBar) {
                CreateMonthlyVP(index);
                CreateWeeklyVP(index);
            }

            // LookBack
            Bars vpBars = GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? DailyBars :
                           GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? WeeklyBars : MonthlyBars;

            // Get Index of VP Interval to continue only in Lookback
            int iVerify = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
            if (vpBars.ClosePrices.Count - iVerify > GeneralParams.Lookback)
                return;

            int TF_idx = vpBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
            int startIndex = Bars.OpenTimes.GetIndexByTime(vpBars.OpenTimes[TF_idx]);

            // === Clean Dicts/others ===
            if (index == startIndex ||
                (index - 1) == startIndex && isPriceBased_Chart ||
                (index - 1) == startIndex && (index - 1) != ClearIdx.MainVP
            )
                CleanUp_MainVP(index, startIndex);

            // Historical data
            if (!IsLastBar)
            {
                // Allows MiniVPs if (!EnableVP)
                CreateMiniVPs(index);

                if (ProfileParams.EnableMainVP)
                    VolumeProfile(startIndex, index);

                isUpdateVP = true; // chart end
            }
            else
            {
                if (UpdateStrategy_Input == UpdateStrategy_Data.SameThread_MayFreeze)
                {
                    if (ProfileParams.EnableMainVP)
                        LiveVP_Update(startIndex, index);
                    else if (!ProfileParams.EnableMainVP && ProfileParams.EnableMiniProfiles)
                        LiveVP_Update(startIndex, index, true);
                }
                else
                    LiveVP_Concurrent(index, startIndex);

                if (!isEndChart) {
                    LoadMoreHistory_IfNeeded();
                    isEndChart = true;
                }
            }

            // === Export Volume Profile data to Python ===
            if (ExportHistory)
            {
                if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                    ExportCsvData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                    ExportCsvData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
            }
            else if (IsLastBar)
            {
                if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                    SendSocketData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                    SendSocketData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
            }
        }

        public void SendSocketData(int index, string profileType, Dictionary<double, double> volRank, Dictionary<double, double> volUp, Dictionary<double, double> volDown, Dictionary<double, double> deltaRank, double[] minMaxDelta)
        {
            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, profileType, volRank, volUp, volDown, deltaRank, minMaxDelta);

                string jsonString = JsonSerializer.Serialize(exportData);
                byte[] data = Encoding.UTF8.GetBytes(jsonString + "\n");
                _networkStream?.Write(data, 0, data.Length);
            }
            catch { }
        }

        private Dictionary<string, object> BuildExportPayload(int index, string profileType, Dictionary<double, double> volRank, Dictionary<double, double> volUp, Dictionary<double, double> volDown, Dictionary<double, double> deltaRank, double[] minMaxDelta)
        {
            double pocPrice = 0;
            double vahPrice = 0;
            double valPrice = 0;
            double totalVolume = 0;

            if (volRank.Count > 0)
            {
                totalVolume = volRank.Values.Sum();
                double maxVol = volRank.Values.Max();
                pocPrice = volRank.FirstOrDefault(kv => kv.Value == maxVol).Key;

                double[] vaResult = VA_Calculation(volRank);
                if (vaResult.Length >= 3)
                {
                    valPrice = vaResult[0];
                    vahPrice = vaResult[1];
                    pocPrice = vaResult[2];
                }
            }

            return new Dictionary<string, object>
            {
                ["type"] = "volume_profile",
                ["profile_type"] = profileType,
                ["symbol"] = Symbol.Name,
                ["timeframe"] = Chart.TimeFrame.ShortName,
                ["timestamp"] = Bars.OpenTimes[index].ToString("o"),
                ["open"] = Bars.OpenPrices[index],
                ["high"] = Bars.HighPrices[index],
                ["low"] = Bars.LowPrices[index],
                ["close"] = Bars.ClosePrices[index],
                ["vpPOC"] = pocPrice,
                ["vpVAH"] = vahPrice,
                ["vpVAL"] = valPrice,
                ["vpTotalVolume"] = totalVolume,
                ["volumesRank"] = volRank,
                ["volumesRankUp"] = volUp,
                ["volumesRankDown"] = volDown,
                ["deltaRank"] = deltaRank,
                ["minMaxDelta"] = minMaxDelta,
                ["spread"] = Symbol.Spread
            };
        }

        private void AppendDirectCsv(Dictionary<string, object> exportData)
        {
            if (!DirectCsvExport || !_isManualCsvExportInProgress)
                return;

            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder.Trim();
            Directory.CreateDirectory(outputFolder);

            string filePath = Path.Combine(outputFolder, "history_volumeprofile.csv");
            bool writeHeader = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;

            using (StreamWriter writer = new StreamWriter(filePath, true, Utf8NoBom))
            {
                if (writeHeader)
                    writer.WriteLine(string.Join(",", ExportCsvHeaders));

                string[] rowValues = new string[ExportCsvHeaders.Length];

                for (int i = 0; i < ExportCsvHeaders.Length; i++)
                {
                    string key = ExportCsvHeaders[i];
                    object value;
                    exportData.TryGetValue(key, out value);
                    rowValues[i] = EscapeCsvValue(ConvertExportValue(value));
                }

                writer.WriteLine(string.Join(",", rowValues));
            }
        }

        private string ConvertExportValue(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is string stringValue)
                return stringValue;

            Type valueType = value.GetType();
            if (valueType.IsArray || (value is System.Collections.IEnumerable && !(value is string)))
                return JsonSerializer.Serialize(value);

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private string EscapeCsvValue(string value)
        {
            if (value == null)
                return string.Empty;

            bool mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!mustQuote)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void ExportCsvData(int index, string profileType, Dictionary<double, double> volRank, Dictionary<double, double> volUp, Dictionary<double, double> volDown, Dictionary<double, double> deltaRank, double[] minMaxDelta)
        {
            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, profileType, volRank, volUp, volDown, deltaRank, minMaxDelta);
                AppendDirectCsv(exportData);
            }
            catch (Exception ex)
            {
                Print("CSV Export Error: " + ex.Message);
            }
        }


        private void CleanUp_MainVP(int index, int startIndex)
        {
            // Reset VP
            // Segments are identified by TF_idx(start)
            // No need to clean up even if it's Daily Interval
            if (!IsLastBar)
                PerformanceSource.startIdx_MainVP = PerformanceSource.lastIdx_MainVP;
            VP_VolumesRank.Clear();
            VP_VolumesRank_Up.Clear();
            VP_VolumesRank_Down.Clear();
            VP_VolumesRank_Subt.Clear();
            VP_DeltaRank.Clear();

            double[] resetDelta = { 0, 0 };
            VP_MinMaxDelta = resetDelta;

            ClearIdx.MainVP = index == startIndex ? index : (index - 1);
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


        // *********** VOLUME PROFILE BARS ***********
        private void VolumeProfile(int iStart, int index, ExtraProfiles extraProfiles = ExtraProfiles.No, bool isLoop = false, bool drawOnly = false, string fixedKey = "", double fixedLowest = 0, double fixedHighest = 0)
        {
            // Weekly/Monthly on Buy_Sell is a waste of time
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell && (extraProfiles == ExtraProfiles.Weekly || extraProfiles == ExtraProfiles.Monthly))
               return;
               
            if (extraProfiles == ExtraProfiles.Fixed && ProfileParams.SegmentsFixedRange_Input == SegmentsFixedRange_Data.From_Profile)
                CreateSegments_FromFixedRange(Bars.OpenPrices[iStart], fixedLowest, fixedHighest, fixedKey);
                
            // ==== VP ====
            if (!drawOnly)
                VP_Bars(index, extraProfiles, fixedKey);

            // ==== Drawing ====
            if (Segments_VP.Count == 0 || isLoop)
                return;

            // Results or Fixed Range
            
            Bars mainTF = GeneralParams.VPInterval_Input switch {
                VPInterval_Data.Weekly => WeeklyBars,
                VPInterval_Data.Monthly => MonthlyBars,
                _ => DailyBars
            };                           
            Bars TF_Bars = extraProfiles switch {
                ExtraProfiles.MiniVP => MiniVPs_Bars,
                ExtraProfiles.Weekly => WeeklyBars,
                ExtraProfiles.Monthly => MonthlyBars,
                // Fixed should use Monthly Bars, so TF_idx can be used by "whichSegment" variable
                ExtraProfiles.Fixed => MonthlyBars,
                _ => mainTF
            };
            int TF_idx = TF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

            bool gapWeekend = Bars.OpenTimes[iStart].DayOfWeek == DayOfWeek.Friday && Bars.OpenTimes[iStart].Hour < 2;
            DateTime x1_Start = Bars.OpenTimes[iStart + (gapWeekend ? 1 : 0)];
            DateTime xBar = Bars.OpenTimes[index];

            bool isIntraday = ProfileParams.ShowIntradayProfile && index == Chart.LastVisibleBarIndex && !isLoop;
            DateTime intraDate = xBar;
            
            // Any Volume Mode
            double maxLength = xBar.Subtract(x1_Start).TotalMilliseconds;
            
            HistWidth_Data selectedWidth = ProfileParams.HistogramWidth_Input;
            double maxWidth = ProfileParams.HistogramWidth_Input switch {
                HistWidth_Data._15 => 1.25,
                HistWidth_Data._30 => 1.50,
                HistWidth_Data._50 => 2,
                _ => 3
            };
            double maxHalfWidth = ProfileParams.HistogramWidth_Input switch {
                HistWidth_Data._15 => 1.12,
                HistWidth_Data._30 => 1.25,
                HistWidth_Data._50 => 1.40,
                _ => 1.75
            };

            double proportion_VP = maxLength - (maxLength / maxWidth);
            if (selectedWidth == HistWidth_Data._100)
                proportion_VP = maxLength;

            string prefix = extraProfiles == ExtraProfiles.Fixed ? fixedKey : $"{iStart}";
            bool histRightSide = ProfileParams.HistogramSide_Input == HistSide_Data.Right;

            // Profile Selection
            Dictionary<double, double> vpNormal = new();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Normal) {
                vpNormal = extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Normal,
                    ExtraProfiles.Weekly => WeeklyRank.Normal,
                    ExtraProfiles.MiniVP => MiniRank.Normal,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Normal,
                    _ => VP_VolumesRank
                };
            }

            Dictionary<double, double> vpBuy = new();
            Dictionary<double, double> vpSell = new();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell) {
                vpBuy = extraProfiles switch
                {
                    ExtraProfiles.MiniVP => MiniRank.Up,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Up,
                    _ => VP_VolumesRank_Up
                };
                vpSell = extraProfiles switch
                {
                    ExtraProfiles.MiniVP => MiniRank.Down,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Down,
                    _ => VP_VolumesRank_Down
                };
            }
            
            Dictionary<double, double> vpDelta = new();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta) {
                vpDelta = extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Delta,
                    ExtraProfiles.Weekly => WeeklyRank.Delta,
                    ExtraProfiles.MiniVP => MiniRank.Delta,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Delta,
                    _ => VP_DeltaRank
                };
            }
            
            // Same for all
            bool intraBool = extraProfiles switch
            {
                ExtraProfiles.Monthly => isIntraday,
                ExtraProfiles.Weekly => isIntraday,
                ExtraProfiles.MiniVP => false,
                ExtraProfiles.Fixed => false,
                _ => isIntraday
            };

            // (micro)Optimization for all modes
            double maxValue = GeneralParams.VolumeMode_Input switch {
                VolumeMode_Data.Normal => vpNormal.Any() ? vpNormal.Values.Max() : 0,
                VolumeMode_Data.Delta => vpDelta.Any() ? vpDelta.Values.Max() : 0,
                _ => 0
            };

            double buyMax = 0;
            double sellMax = 0;
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell) {
                buyMax = vpBuy.Any() ? vpBuy.Values.Max() : 0;
                sellMax = vpSell.Any() ? vpSell.Values.Max() : 0;
            }

            IEnumerable<double> negativeList = new List<double>();
            if (GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta)
                negativeList = vpDelta.Values.Where(n => n < 0);

            // Segments selection
            List<double> whichSegment = extraProfiles == ExtraProfiles.Fixed ? GetRangeSegments(TF_idx, fixedKey) : Segments_VP;
            
            // Manual Refactoring.
            // LLM allucinates.
            for (int i = 0; i < whichSegment.Count; i++)
            {
                double priceKey = whichSegment[i];

                bool skip = extraProfiles switch
                {
                    ExtraProfiles.Monthly => !MonthlyRank.Normal.ContainsKey(priceKey),
                    ExtraProfiles.Weekly => !WeeklyRank.Normal.ContainsKey(priceKey),
                    ExtraProfiles.MiniVP => !MiniRank.Normal.ContainsKey(priceKey),
                    ExtraProfiles.Fixed => !FixedRank[fixedKey].Normal.ContainsKey(priceKey),
                    _ => !VP_VolumesRank.ContainsKey(priceKey),
                };
                if (skip)
                    continue;

                /*
                Indeed, the value of X-Axis is simply a rule of three,
                where the maximum value will be the maxLength (in Milliseconds),
                from there the math adjusts the histograms.
                    MaxValue    maxLength(ms)
                       x             ?(ms)
                The values 1.25 and 4 are the manually set values
                ===================
                NEW IN ODF_AGG => To avoid histograms unexpected behavior that occurs in historical data
                - on Price-Based Charts (sometimes in candles too) where interval goes through weekend
                  We'll skip 1 bar (friday) since Bar Index as X-axis didn't resolve the problem.
                */

                double lowerSegmentY1 = whichSegment[i] - rowHeight;
                double upperSegmentY2 = whichSegment[i];
                
                void DrawRectangle_Normal(double currentVolume, double maxVolume, bool intradayProfile = false)
                {
                    double proportion = currentVolume * proportion_VP;
                    double dynLength = proportion / maxVolume;

                    DateTime x2 = x1_Start.AddMilliseconds(dynLength);

                    Color histogramColor = extraProfiles switch
                    {
                        ExtraProfiles.Monthly => MonthlyColor,
                        ExtraProfiles.Weekly => WeeklyColor,
                        _ => HistColor,
                    };

                    if (ProfileParams.EnableGradient)
                    {
                        Color minColor = extraProfiles switch
                        {
                            ExtraProfiles.Monthly => MonthlyGrandient_Min,
                            ExtraProfiles.Weekly => WeeklyGrandient_Min,
                            _ => ColorGrandient_Min,
                        };

                        Color maxColor = extraProfiles switch
                        {
                            ExtraProfiles.Monthly => MonthlyGrandient_Max,
                            ExtraProfiles.Weekly => WeeklyGrandient_Max,
                            _ => ColorGrandient_Max,
                        };

                        double Intensity = (currentVolume * 100 / maxVolume) / 100;
                        double stepR = (maxColor.R - minColor.R) * Intensity;
                        double stepG = (maxColor.G - minColor.G) * Intensity;
                        double stepB = (maxColor.B - minColor.B) * Intensity;

                        int A = (int)(2.55 * OpacityHistInput);
                        int R = (int)Math.Round(minColor.R + stepR);
                        int G = (int)Math.Round(minColor.G + stepG);
                        int B = (int)Math.Round(minColor.B + stepB);

                        Color dynColor = Color.FromArgb(A, R, G, B);

                        histogramColor = dynColor;
                    }

                    ChartRectangle volHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Normal", x1_Start, lowerSegmentY1, x2, upperSegmentY2, histogramColor);

                    if (ProfileParams.FillHist_VP)
                        volHist.IsFilled = true;

                    if (histRightSide)
                    {
                        volHist.Time1 = xBar;
                        volHist.Time2 = xBar.AddMilliseconds(-dynLength);
                    }

                    if (intradayProfile && extraProfiles != ExtraProfiles.MiniVP)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        DateTime dateOffset_Duo = TimeBasedOffset(dateOffset, true);
                        DateTime dateOffset_Triple = TimeBasedOffset(dateOffset_Duo, true);

                        double maxLength_Intraday = dateOffset.Subtract(xBar).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Weekly)
                            maxLength_Intraday = dateOffset_Duo.Subtract(dateOffset).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Monthly)
                            maxLength_Intraday = dateOffset_Triple.Subtract(dateOffset_Duo).TotalMilliseconds;

                        // Recalculate histograms 'X' position
                        double proportion_Intraday = currentVolume * (maxLength_Intraday - (maxLength_Intraday / maxWidth));
                        if (selectedWidth == HistWidth_Data._100)
                            proportion_Intraday = currentVolume * maxLength_Intraday;

                        double dynLength_Intraday = proportion_Intraday / maxVolume;

                        // Set 'X'
                        volHist.Time1 = dateOffset;
                        volHist.Time2 = dateOffset.AddMilliseconds(-dynLength_Intraday);

                        if (extraProfiles == ExtraProfiles.Weekly)
                        {
                            volHist.Time1 = dateOffset_Duo;
                            volHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Intraday);
                            if (!ProfileParams.EnableMonthlyProfile && ProfileParams.FillIntradaySpace)
                            {
                                volHist.Time1 = dateOffset;
                                volHist.Time2 = dateOffset.AddMilliseconds(dynLength_Intraday);
                            }
                        }
                        if (extraProfiles == ExtraProfiles.Monthly)
                        {
                            if (ProfileParams.EnableWeeklyProfile) {
                                // Show after
                                volHist.Time1 = dateOffset_Triple;
                                volHist.Time2 = dateOffset_Triple.AddMilliseconds(-dynLength_Intraday);
                                // Show after together
                                if (ProfileParams.FillIntradaySpace) {
                                    volHist.Time1 = dateOffset_Duo;
                                    volHist.Time2 = dateOffset_Duo.AddMilliseconds(dynLength_Intraday);
                                }
                            }
                            else {
                                // Use Weekly position
                                volHist.Time1 = dateOffset_Duo;
                                volHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Intraday);
                                if (ProfileParams.FillIntradaySpace) {
                                    volHist.Time1 = dateOffset;
                                    volHist.Time2 = dateOffset.AddMilliseconds(dynLength_Intraday);
                                }
                            }
                        }

                        intraDate = volHist.Time1;
                    }
                }

                void DrawRectangle_BuySell(
                    double currentBuy, double currentSell,
                    double buyMax, double sellMax,
                    bool intradayProfile = false)
                {
                    // Buy vs Sell - already
                    double maxBuyVolume = buyMax;
                    double maxSellVolume = sellMax;

                    double maxSideVolume = maxBuyVolume > maxSellVolume ? maxBuyVolume : maxSellVolume;

                    double proportionBuy = 0;
                    try { proportionBuy = currentBuy * (maxLength - (maxLength / maxHalfWidth)); } catch { };
                    if (selectedWidth == HistWidth_Data._100)
                        try { proportionBuy = currentBuy * (maxLength - (maxLength / 3)); } catch { };

                    double dynLengthBuy = proportionBuy / maxSideVolume; ;

                    double proportionSell = 0;
                    try { proportionSell = currentSell * proportion_VP; } catch { };
                    double dynLengthSell = proportionSell / maxSideVolume;

                    DateTime x2_Sell = x1_Start.AddMilliseconds(dynLengthSell);
                    DateTime x2_Buy = x1_Start.AddMilliseconds(dynLengthBuy);

                    ChartRectangle buyHist, sellHist;
                    sellHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Sell", x1_Start, lowerSegmentY1, x2_Sell, upperSegmentY2, SellColor);
                    buyHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Buy", x1_Start, lowerSegmentY1, x2_Buy, upperSegmentY2, BuyColor);
                    if (ProfileParams.FillHist_VP)
                    {
                        buyHist.IsFilled = true;
                        sellHist.IsFilled = true;
                    }
                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                    {
                        sellHist.Time1 = xBar;
                        sellHist.Time2 = xBar.AddMilliseconds(-dynLengthSell);
                        buyHist.Time1 = xBar;
                        buyHist.Time2 = xBar.AddMilliseconds(-dynLengthBuy);
                    }

                    // Intraday Right Profile
                    if (intradayProfile && extraProfiles != ExtraProfiles.MiniVP)
                    {
                        // ==== Subtract Profile / Plain Delta - Profile View ====
                        // Recalculate histograms 'X' position
                        DateTime dateOffset_Subt = TimeBasedOffset(xBar);

                        double maxPositive = VP_VolumesRank_Subt.Values.Max();
                        IEnumerable<double> negativeVolumeList = VP_VolumesRank_Subt.Values.Where(n => n < 0);
                        double maxNegative = 0;
                        try { maxNegative = Math.Abs(negativeVolumeList.Min()); } catch { }

                        double subtMax = maxPositive > maxNegative ? maxPositive : maxNegative;

                        double maxLength_Intraday = dateOffset_Subt.Subtract(xBar).TotalMilliseconds;
                        double proportion_Intraday = VP_VolumesRank_Subt[priceKey] * (maxLength_Intraday - (maxLength_Intraday / maxWidth));
                        double dynLength = proportion_Intraday / subtMax;

                        // Set 'X'
                        DateTime x1 = dateOffset_Subt;
                        DateTime x2 = x1.AddMilliseconds(dynLength);

                        Color colorHist = dynLength > 0 ? BuyColor : SellColor;
                        ChartRectangle subtHist = Chart.DrawRectangle($"{iStart}_{i}_VP_Subt", x1, lowerSegmentY1, x2, upperSegmentY2, colorHist);

                        dynLength = -Math.Abs(dynLength);
                        subtHist.Time1 = dateOffset_Subt;
                        subtHist.Time2 = subtHist.Time2 != dateOffset_Subt ? dateOffset_Subt.AddMilliseconds(dynLength) : dateOffset_Subt;

                        if (ProfileParams.FillHist_VP)
                            subtHist.IsFilled = true;

                        intraDate = subtHist.Time1;

                        // ==== Buy_Sell - Divided View - Half Width ====
                        // Recalculate histograms 'X' position
                        DateTime dateOffset = TimeBasedOffset(dateOffset_Subt, true);
                        maxLength_Intraday = dateOffset.Subtract(dateOffset_Subt).TotalMilliseconds;

                        // Replaced maxHalfWidth to maxWidth since it's Divided View
                        proportionBuy = 0;
                        try { proportionBuy = currentBuy * (maxLength_Intraday - (maxLength_Intraday / maxHalfWidth)); } catch { };
                        if (selectedWidth == HistWidth_Data._100)
                            try { proportionBuy = currentBuy * maxLength_Intraday; } catch { };

                        dynLengthBuy = proportionBuy / maxBuyVolume; ;

                        proportionSell = 0;
                        try { proportionSell = currentSell * (maxLength_Intraday - (maxLength_Intraday / maxHalfWidth)); } catch { };
                        if (selectedWidth == HistWidth_Data._100)
                            try { proportionSell = currentSell * maxLength_Intraday; } catch { };

                        dynLengthSell = proportionSell / maxSellVolume;

                        // Set 'X'
                        sellHist.Time1 = dateOffset;
                        sellHist.Time2 = dateOffset.AddMilliseconds(-dynLengthSell);
                        buyHist.Time1 = dateOffset;
                        buyHist.Time2 = dateOffset.AddMilliseconds(dynLengthBuy);
                    }
                }

                void DrawRectangle_Delta(double currentDelta, double positiveDeltaMax, IEnumerable<double> negativeDeltaList, bool intradayProfile = false)
                {
                    double negativeDeltaMax = 0;
                    try { negativeDeltaMax = Math.Abs(negativeDeltaList.Min()); } catch { }

                    double deltaMax = positiveDeltaMax > negativeDeltaMax ? positiveDeltaMax : negativeDeltaMax;

                    double proportion_Delta = Math.Abs(currentDelta) * proportion_VP;
                    double dynLength_Delta = proportion_Delta / deltaMax;

                    Color colorHist = currentDelta >= 0 ? BuyColor : SellColor;
                    DateTime x2 = x1_Start.AddMilliseconds(dynLength_Delta);

                    ChartRectangle deltaHist = Chart.DrawRectangle($"{prefix}_{i}_VP_{extraProfiles}_Delta", x1_Start, lowerSegmentY1, x2, upperSegmentY2, colorHist);

                    if (ProfileParams.FillHist_VP)
                        deltaHist.IsFilled = true;

                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                    {
                        deltaHist.Time1 = xBar;
                        deltaHist.Time2 = deltaHist.Time2 != x1_Start ? xBar.AddMilliseconds(-dynLength_Delta) : x1_Start;
                    }

                    // Intraday Right Profile
                    if (intradayProfile && extraProfiles != ExtraProfiles.MiniVP)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        DateTime dateOffset_Duo = TimeBasedOffset(dateOffset, true);
                        DateTime dateOffset_Triple = TimeBasedOffset(dateOffset_Duo, true);
                        double maxLength_Intraday = dateOffset.Subtract(xBar).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Weekly)
                            maxLength_Intraday = dateOffset_Duo.Subtract(dateOffset).TotalMilliseconds;

                        if (extraProfiles == ExtraProfiles.Monthly)
                            maxLength_Intraday = dateOffset_Triple.Subtract(dateOffset_Duo).TotalMilliseconds;

                        // Recalculate histograms 'X' position
                        proportion_Delta = currentDelta * (maxLength_Intraday - (maxLength_Intraday / maxWidth));
                        if (selectedWidth == HistWidth_Data._100)
                            proportion_Delta = currentDelta * maxLength_Intraday;
                        dynLength_Delta = proportion_Delta / deltaMax;

                        colorHist = dynLength_Delta > 0 ? BuyColor : SellColor;
                        dynLength_Delta = Math.Abs(dynLength_Delta); // Profile view only

                        // Set 'X'
                        deltaHist.Time1 = dateOffset;
                        deltaHist.Time2 = deltaHist.Time2 != dateOffset ? dateOffset.AddMilliseconds(-dynLength_Delta) : dateOffset;
                        deltaHist.Color = colorHist;

                        if (extraProfiles == ExtraProfiles.Weekly) {
                            deltaHist.Time1 = dateOffset_Duo;
                            deltaHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Delta);
                            if (!ProfileParams.EnableMonthlyProfile && ProfileParams.FillIntradaySpace) {
                                deltaHist.Time1 = dateOffset;
                                deltaHist.Time2 = dateOffset.AddMilliseconds(dynLength_Delta);
                            }
                        }

                        if (extraProfiles == ExtraProfiles.Monthly) {
                            if (ProfileParams.EnableWeeklyProfile) {
                                // Show after
                                deltaHist.Time1 = dateOffset_Triple;
                                deltaHist.Time2 = dateOffset_Triple.AddMilliseconds(-dynLength_Delta);
                                // Show after together
                                if (ProfileParams.FillIntradaySpace) {
                                    deltaHist.Time1 = dateOffset_Duo;
                                    deltaHist.Time2 = dateOffset_Duo.AddMilliseconds(dynLength_Delta);
                                }
                            }
                            else {
                                // Use Weekly position
                                deltaHist.Time1 = dateOffset_Duo;
                                deltaHist.Time2 = dateOffset_Duo.AddMilliseconds(-dynLength_Delta);
                                if (ProfileParams.FillIntradaySpace) {
                                    deltaHist.Time1 = dateOffset;
                                    deltaHist.Time2 = dateOffset.AddMilliseconds(dynLength_Delta);
                                }
                            }
                        }

                        intraDate = deltaHist.Time1;
                    }
                }
                
                switch (GeneralParams.VolumeMode_Input) 
                {
                    case VolumeMode_Data.Normal:
                    {
                        double value = vpNormal[priceKey];
                        // Draw histograms and update 'intraDate', if applicable
                        DrawRectangle_Normal(value, maxValue, intraBool);
                        break;
                    }
                    case VolumeMode_Data.Buy_Sell:             
                    {
                        if (vpBuy.ContainsKey(priceKey) && vpSell.ContainsKey(priceKey))
                            DrawRectangle_BuySell(vpBuy[priceKey], vpSell[priceKey], buyMax, sellMax, isIntraday);
                        break;
                    }
                    default:
                    {
                        double value = vpDelta[priceKey];
                        // Draw histograms and update 'intraDate', if applicable
                        DrawRectangle_Delta(value, maxValue, negativeList, intraBool);
                        break;
                    }   
                }
            }

            // Drawings that don't require each segment-price as y-axis
            // It can/should be outside SegmentsLoop for better performance.
            
            double lowest = TF_Bars.LowPrices[TF_idx];
            double highest = TF_Bars.HighPrices[TF_idx];
            if (double.IsNaN(lowest)) { // Mini VPs avoid crash after recalculating
                lowest = TF_Bars.LowPrices.LastValue;
                highest = TF_Bars.HighPrices.LastValue;
            }
            double y1_lowest = extraProfiles == ExtraProfiles.Fixed ? fixedLowest : lowest;

            if (extraProfiles == ExtraProfiles.MiniVP && ProfileParams.ShowMiniResults || 
                extraProfiles != ExtraProfiles.MiniVP && ResultParams.ShowResults)
            {
                switch (GeneralParams.VolumeMode_Input) 
                {
                    case VolumeMode_Data.Normal:
                    {
                        double sum = Math.Round(vpNormal.Values.Sum());
                        string strValue = FormatResults ? FormatBigNumber(sum) : $"{sum}";

                        ChartText Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Normal_Result", $"\n{strValue}", x1_Start, y1_lowest, ProfileParams.EnableGradient ? ColorGrandient_Min : HistColor);
                        Center.HorizontalAlignment = HorizontalAlignment.Center;
                        Center.FontSize = FontSizeResults - 1;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                            Center.Time = xBar;

                        // Intraday Right Profile
                        if (isIntraday && extraProfiles == ExtraProfiles.No) {
                            DateTime dateOffset = TimeBasedOffset(xBar);
                            Center.Time = dateOffset;
                        }     
                        break;
                    }
                    case VolumeMode_Data.Buy_Sell:
                    {
                        double volBuy = vpBuy.Values.Sum();
                        double volSell = vpSell.Values.Sum();

                        double percentBuy = (volBuy * 100) / (volBuy + volSell);
                        double percentSell = (volSell * 100) / (volBuy + volSell);
                        percentBuy = Math.Round(percentBuy);
                        percentSell = Math.Round(percentSell);

                        ChartText Left, Right;
                        Left = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Sell_Sum", $"{percentSell}%", x1_Start, y1_lowest, SellColor);
                        Right = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Buy_Sum", $"{percentBuy}%", x1_Start, y1_lowest, BuyColor);
                        Left.HorizontalAlignment = HorizontalAlignment.Left;
                        Right.HorizontalAlignment = HorizontalAlignment.Right;
                        Left.FontSize = FontSizeResults;
                        Right.FontSize = FontSizeResults;

                        ChartText Center;
                        double sum = Math.Round(volBuy + volSell);
                        double subtract = Math.Round(volBuy - volSell);
                        double divide = 0;
                        if (volBuy != 0 && volSell != 0)
                            divide = Math.Round(volBuy / volSell, 3);

                        string sumFmtd = FormatResults ? FormatBigNumber(sum) : $"{sum}";
                        string subtractValueFmtd = subtract > 0 ? FormatBigNumber(subtract) : $"-{FormatBigNumber(Math.Abs(subtract))}";
                        string subtractFmtd = FormatResults ? subtractValueFmtd : $"{subtract}";

                        string strFormated = ResultParams.OperatorBuySell_Input == OperatorBuySell_Data.Sum ? sumFmtd :
                                             ResultParams.OperatorBuySell_Input == OperatorBuySell_Data.Subtraction ? subtractFmtd : $"{divide}";

                        Color centerColor = Math.Round(percentBuy) > Math.Round(percentSell) ? BuyColor : SellColor;

                        Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_BuySell_Result", $"\n{strFormated}", x1_Start, y1_lowest, centerColor);
                        Center.HorizontalAlignment = HorizontalAlignment.Center;
                        Center.FontSize = FontSizeResults - 1;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                        {
                            Right.Time = xBar;
                            Left.Time = xBar;
                            Center.Time = xBar;
                        }

                        // Intraday Right Profile
                        if (isIntraday) {
                            DateTime dateOffset = TimeBasedOffset(xBar);
                            Right.Time = dateOffset;
                            Left.Time = dateOffset;
                            Center.Time = dateOffset;
                        }
                        break;
                    }
                    default: {
                        double deltaBuy = vpDelta.Values.Where(n => n > 0).Sum();
                        double deltaSell = vpDelta.Values.Where(n => n < 0).Sum();
                        double totalDelta = vpDelta.Values.Sum();

                        double percentBuy = 0;
                        double percentSell = 0;
                        try { percentBuy = (deltaBuy * 100) / (deltaBuy + Math.Abs(deltaSell)); } catch { };
                        try { percentSell = (deltaSell * 100) / (deltaBuy + Math.Abs(deltaSell)); } catch { }
                        percentBuy = Math.Round(percentBuy);
                        percentSell = Math.Round(percentSell);

                        ChartText Left, Right;
                        Right = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_BuySum", $"{percentBuy}%", x1_Start, y1_lowest, BuyColor);
                        Left = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_SellSum", $"{percentSell}%", x1_Start, y1_lowest, SellColor);
                        Left.HorizontalAlignment = HorizontalAlignment.Left; Left.FontSize = FontSizeResults;
                        Right.HorizontalAlignment = HorizontalAlignment.Right; Right.FontSize = FontSizeResults;
                        
                        ChartText Center;
                        string totalDeltaFmtd = totalDelta > 0 ? FormatBigNumber(totalDelta) : $"-{FormatBigNumber(Math.Abs(totalDelta))}";
                        string totalDeltaString = FormatResults ? totalDeltaFmtd : $"{totalDelta}";

                        Color centerColor = totalDelta > 0 ? BuyColor : SellColor;
                        Center = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_Result", $"\n{totalDeltaString}", x1_Start, y1_lowest, centerColor);
                        Center.HorizontalAlignment = HorizontalAlignment.Center; Center.FontSize = FontSizeResults - 1;

                        if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                        {
                            Right.Time = xBar;
                            Left.Time = xBar;
                            Center.Time = xBar;
                        }

                        // Intraday Right Profile
                        if (isIntraday && extraProfiles == ExtraProfiles.No) {
                            DateTime dateOffset = TimeBasedOffset(xBar);
                            Right.Time = dateOffset;
                            Left.Time = dateOffset;
                            Center.Time = dateOffset;
                        }

                        if (ResultParams.ShowMinMaxDelta)
                            Draw_MinMaxDelta(extraProfiles, fixedKey, y1_lowest, x1_Start, xBar, isIntraday, prefix);
                        
                        break;
                    }
                }
            }
            
            // For [Normal, Delta] only
            Dictionary<double, double> vpDict = GeneralParams.VolumeMode_Input switch
            {
                VolumeMode_Data.Normal => extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Normal,
                    ExtraProfiles.Weekly => WeeklyRank.Normal,
                    ExtraProfiles.MiniVP => MiniRank.Normal,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Normal,
                    _ => VP_VolumesRank
                },
                VolumeMode_Data.Delta => extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.Delta,
                    ExtraProfiles.Weekly => WeeklyRank.Delta,
                    ExtraProfiles.MiniVP => MiniRank.Delta,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].Delta,
                    _ => VP_DeltaRank
                },
                _ => new Dictionary<double, double>(),
            };
            
            if (vpDict.Count > 0) {
                // VA + POC
                Draw_VA_POC(vpDict, iStart, x1_Start, xBar, extraProfiles, isIntraday, intraDate, fixedKey);

                // HVN/LVN
                DrawVolumeNodes(vpDict, iStart, x1_Start, xBar, extraProfiles, isIntraday, intraDate, fixedKey);   
            }
            
            if (!ProfileParams.ShowOHLC || extraProfiles == ExtraProfiles.Fixed)
                return;

            DateTime OHLC_Date = TF_Bars.OpenTimes[TF_idx];

            ChartText iconOpenSession =  Chart.DrawText($"{OHLC_Date}_OHLC_Start", "▂", OHLC_Date, TF_Bars.OpenPrices[TF_idx], ColorOHLC);
            iconOpenSession.VerticalAlignment = VerticalAlignment.Center;
            iconOpenSession.HorizontalAlignment = HorizontalAlignment.Left;
            iconOpenSession.FontSize = 14;

            ChartText iconCloseSession =  Chart.DrawText($"{OHLC_Date}_OHLC_End", "▂", OHLC_Date, TF_Bars.ClosePrices[TF_idx], ColorOHLC);
            iconCloseSession.VerticalAlignment = VerticalAlignment.Center;
            iconCloseSession.HorizontalAlignment = HorizontalAlignment.Right;
            iconCloseSession.FontSize = 14;

            ChartTrendLine Session = Chart.DrawTrendLine($"{OHLC_Date}_OHLC_Body", OHLC_Date, lowest, OHLC_Date, highest, ColorOHLC);
            Session.Thickness = 3;

            void Draw_MinMaxDelta(ExtraProfiles extraProfiles, string fixedKey, double lowest, DateTime x1_Start, DateTime xBar, bool isIntraday, string prefix)
            {
                ChartText MinText, MaxText, SubText;

                double[] vpMinMax = extraProfiles switch
                {
                    ExtraProfiles.Monthly => MonthlyRank.MinMaxDelta,
                    ExtraProfiles.Weekly => WeeklyRank.MinMaxDelta,
                    ExtraProfiles.MiniVP => MiniRank.MinMaxDelta,
                    ExtraProfiles.Fixed => FixedRank[fixedKey].MinMaxDelta,
                    _ => VP_MinMaxDelta
                };

                double minDelta = Math.Round(vpMinMax[0]);
                double maxDelta = Math.Round(vpMinMax[1]);
                double subDelta = Math.Round(minDelta - maxDelta);

                string minDeltaFmtd = minDelta > 0 ? FormatBigNumber(minDelta) : $"-{FormatBigNumber(Math.Abs(minDelta))}";
                string maxDeltaFmtd = maxDelta > 0 ? FormatBigNumber(maxDelta) : $"-{FormatBigNumber(Math.Abs(maxDelta))}";
                string subDeltaFmtd = subDelta > 0 ? FormatBigNumber(subDelta) : $"-{FormatBigNumber(Math.Abs(subDelta))}";

                string minDeltaString = FormatResults ? minDeltaFmtd : $"{minDelta}";
                string maxDeltaString = FormatResults ? maxDeltaFmtd : $"{maxDelta}";
                string subDeltaString = FormatResults ? subDeltaFmtd : $"{subDelta}";

                Color subColor = subDelta > 0 ? BuyColor : SellColor;

                if (!ResultParams.ShowOnlySubtDelta)
                {
                    MinText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_MinResult", $"\n\nMin: {minDeltaString}", x1_Start, lowest, SellColor);
                    MaxText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_MaxResult", $"\n\n\nMax: {maxDeltaString}", x1_Start, lowest, BuyColor);
                    SubText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_SubResult", $"\n\n\n\nSub: {subDeltaString}", x1_Start, lowest, subColor);
                    MinText.HorizontalAlignment = HorizontalAlignment.Center;
                    MaxText.HorizontalAlignment = HorizontalAlignment.Center;
                    SubText.HorizontalAlignment = HorizontalAlignment.Center;
                    MinText.FontSize = FontSizeResults - 1;
                    MaxText.FontSize = FontSizeResults - 1;
                    SubText.FontSize = FontSizeResults - 1;

                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                    {
                        MinText.Time = xBar;
                        MaxText.Time = xBar;
                        SubText.Time = xBar;
                    }

                    // Intraday Right Profile
                    if (isIntraday && extraProfiles == ExtraProfiles.No)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        MinText.Time = dateOffset;
                        MaxText.Time = dateOffset;
                        SubText.Time = dateOffset;
                    }
                }
                else
                {
                    SubText = Chart.DrawText($"{prefix}_VP_{extraProfiles}_Delta_SubResult", $"\n\nSub: {subDeltaString}", x1_Start, lowest, subColor);
                    SubText.HorizontalAlignment = HorizontalAlignment.Center;
                    SubText.FontSize = FontSizeResults - 1;

                    if (ProfileParams.HistogramSide_Input == HistSide_Data.Right)
                        SubText.Time = xBar;
                    // Intraday Right Profile
                    if (isIntraday && extraProfiles == ExtraProfiles.No)
                    {
                        DateTime dateOffset = TimeBasedOffset(xBar);
                        SubText.Time = dateOffset;
                    }
                }
            }
        }

        private void VP_Bars(int index, ExtraProfiles extraVP = ExtraProfiles.No, string fixedKey = "")
        {
            DateTime startTime = Bars.OpenTimes[index];
            DateTime endTime = Bars.OpenTimes[index + 1];

            // For real-time market - VP
            // Run conditional only in the last bar of repaint loop
            if (IsLastBar && Bars.OpenTimes[index] == Bars.LastBar.OpenTime)
                endTime = Source_Bars.LastBar.OpenTime;

            int startIndex = extraVP switch
            {
                ExtraProfiles.Monthly => !IsLastBar ? PerformanceSource.lastIdx_Monthly : PerformanceSource.startIdx_Monthly,
                ExtraProfiles.Weekly => !IsLastBar ? PerformanceSource.lastIdx_Weekly : PerformanceSource.startIdx_Weekly,
                ExtraProfiles.MiniVP => !IsLastBar ? PerformanceSource.lastIdx_Mini : PerformanceSource.startIdx_Mini,
                _ => !IsLastBar ? PerformanceSource.lastIdx_MainVP : PerformanceSource.startIdx_MainVP
            };
            if (extraVP == ExtraProfiles.Fixed) {
                ChartRectangle rect = RangeObjs.rectangles.Where(x => x.Name == fixedKey).FirstOrDefault();
                DateTime start = rect.Time1 < rect.Time2 ? rect.Time1 : rect.Time2;
                startIndex = Bars.OpenTimes.GetIndexByTime(start);
            }

            int TF_idx = extraVP == ExtraProfiles.Fixed ? MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]) : index;
            List<double> whichSegment = extraVP == ExtraProfiles.Fixed ? GetRangeSegments(TF_idx, fixedKey) : Segments_VP;

            // Keep shared VOL_Bars since 1min bars
            // are quite cheap in terms of RAM, even for 1 year.
            for (int k = startIndex; k < Source_Bars.Count; ++k)
            {
                Bar volBar;
                volBar = Source_Bars[k];

                if (volBar.OpenTime < startTime || volBar.OpenTime > endTime)
                {
                    if (volBar.OpenTime > endTime) {
                        _ = extraVP switch
                        {
                            ExtraProfiles.Monthly => PerformanceSource.lastIdx_Monthly = k,
                            ExtraProfiles.Weekly => PerformanceSource.lastIdx_Weekly = k,
                            ExtraProfiles.MiniVP => PerformanceSource.lastIdx_Mini = k,
                            _ => PerformanceSource.lastIdx_MainVP = k
                        };
                        break;
                    }
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
                if (extraVP != ExtraProfiles.No)
                {
                    VolumeRankType extraRank = extraVP switch
                    {
                        ExtraProfiles.Monthly => MonthlyRank,
                        ExtraProfiles.Weekly => WeeklyRank,
                        ExtraProfiles.Fixed => FixedRank[fixedKey],
                        _ => MiniRank
                    };
                    UpdateExtraProfiles(extraRank, priceKey, vol, isBullish);
                    return;
                }

                if (!VP_VolumesRank.ContainsKey(priceKey))
                    VP_VolumesRank.Add(priceKey, vol);
                else
                    VP_VolumesRank[priceKey] += vol;

                bool condition = GeneralParams.VolumeMode_Input != VolumeMode_Data.Normal;
                if (condition)
                    Add_BuySell(priceKey, vol, isBullish);
            }
            void Add_BuySell(double priceKey, double vol, bool isBullish)
            {
                if (isBullish)
                {
                    if (!VP_VolumesRank_Up.ContainsKey(priceKey))
                        VP_VolumesRank_Up.Add(priceKey, vol);
                    else
                        VP_VolumesRank_Up[priceKey] += vol;
                }
                else
                {
                    if (!VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Down.Add(priceKey, vol);
                    else
                        VP_VolumesRank_Down[priceKey] += vol;
                }

                // Subtract Profile - Plain Delta
                if (!VP_VolumesRank_Subt.ContainsKey(priceKey))
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt.Add(priceKey, (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]));
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt.Add(priceKey, (VP_VolumesRank_Up[priceKey]));
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt.Add(priceKey, (-VP_VolumesRank_Down[priceKey]));
                    else
                        VP_VolumesRank_Subt.Add(priceKey, 0);
                }
                else
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt[priceKey] = (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]);
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt[priceKey] = (VP_VolumesRank_Up[priceKey]);
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_VolumesRank_Subt[priceKey] = (-VP_VolumesRank_Down[priceKey]);
                }

                if (GeneralParams.VolumeMode_Input != VolumeMode_Data.Delta)
                    return;
                    
                double prevDelta = VP_DeltaRank.Values.Sum();
                if (!VP_DeltaRank.ContainsKey(priceKey))
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank.Add(priceKey, (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]));
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank.Add(priceKey, (VP_VolumesRank_Up[priceKey]));
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank.Add(priceKey, (-VP_VolumesRank_Down[priceKey]));
                    else
                        VP_DeltaRank.Add(priceKey, 0);
                }
                else
                {
                    if (VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank[priceKey] += (VP_VolumesRank_Up[priceKey] - VP_VolumesRank_Down[priceKey]);
                    else if (VP_VolumesRank_Up.ContainsKey(priceKey) && !VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank[priceKey] += (VP_VolumesRank_Up[priceKey]);
                    else if (!VP_VolumesRank_Up.ContainsKey(priceKey) && VP_VolumesRank_Down.ContainsKey(priceKey))
                        VP_DeltaRank[priceKey] += (-VP_VolumesRank_Down[priceKey]);

                }

                double currentDelta = VP_DeltaRank.Values.Sum();
                if (prevDelta > currentDelta)
                    VP_MinMaxDelta[0] = prevDelta; // Min
                if (prevDelta < currentDelta)
                    VP_MinMaxDelta[1] = prevDelta; // Max before final delta
            }

            void UpdateExtraProfiles(VolumeRankType volRank, double priceKey, double vol, bool isBullish) {
                if (!volRank.Normal.ContainsKey(priceKey))
                    volRank.Normal.Add(priceKey, vol);
                else
                    volRank.Normal[priceKey] += vol;

                bool condition = GeneralParams.VolumeMode_Input != VolumeMode_Data.Normal;
                if (condition)
                    Add_BuySell_Extra(volRank, priceKey, vol, isBullish);
            }

            void Add_BuySell_Extra(VolumeRankType volRank, double priceKey, double vol, bool isBullish)
            {
                if (isBullish)
                {
                    if (!volRank.Up.ContainsKey(priceKey))
                        volRank.Up.Add(priceKey, vol);
                    else
                        volRank.Up[priceKey] += vol;
                }
                else
                {
                    if (!volRank.Down.ContainsKey(priceKey))
                        volRank.Down.Add(priceKey, vol);
                    else
                        volRank.Down[priceKey] += vol;
                }

                if (GeneralParams.VolumeMode_Input != VolumeMode_Data.Delta)
                    return;

                double prevDelta = volRank.Delta.Values.Sum();
                if (!volRank.Delta.ContainsKey(priceKey))
                {
                    if (volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta.Add(priceKey, (volRank.Up[priceKey] - volRank.Down[priceKey]));
                    else if (volRank.Up.ContainsKey(priceKey) && !volRank.Down.ContainsKey(priceKey))
                        volRank.Delta.Add(priceKey, (volRank.Up[priceKey]));
                    else if (!volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta.Add(priceKey, (-volRank.Down[priceKey]));
                    else
                        volRank.Delta.Add(priceKey, 0);
                }
                else
                {
                    if (volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta[priceKey] += (volRank.Up[priceKey] - volRank.Down[priceKey]);
                    else if (volRank.Up.ContainsKey(priceKey) && !volRank.Down.ContainsKey(priceKey))
                        volRank.Delta[priceKey] += (volRank.Up[priceKey]);
                    else if (!volRank.Up.ContainsKey(priceKey) && volRank.Down.ContainsKey(priceKey))
                        volRank.Delta[priceKey] += (-volRank.Down[priceKey]);

                }

                double currentDelta = volRank.Delta.Values.Sum();
                if (prevDelta > currentDelta)
                    volRank.MinMaxDelta[0] = prevDelta; // Min
                if (prevDelta < currentDelta)
                    volRank.MinMaxDelta[1] = prevDelta; // Max before final delta
            }
        }

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

        protected override void OnDestroy()
        {
            _Tasks.cts.Cancel();
            if (ProfileParams.EnableFixedRange) {
                foreach (ChartRectangle item in RangeObjs.rectangles)
                    Chart.RemoveObject(item.Name);
            }
        }

        // Code generated by LLM.
        /*
            From my attempts, it should never be declared/invoked in the main thread,
                - ManualResetEventSlim(false) locks the indicator's Initialize, no matter the field or location it's on.

            The idea is "Get any cTrader's object by running BeginInvokeOnMainThread on it"
            The downside is calling it at every cTrader related objects (obviously) (Bars, Chart, etc..)

            A small price to pay to avoid freezes and lags.
        */
        public T GetByInvoke<T>(Func<T> func, string label = null)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            T result = default;
            var done = new ManualResetEventSlim(false);

            Stopwatch sw = null;
            if (!string.IsNullOrEmpty(label))
                sw = Stopwatch.StartNew();

            BeginInvokeOnMainThread(() =>
            {
                try {
                    result = func();
                }
                finally {
                    if (!string.IsNullOrEmpty(label)) {
                        sw.Stop();
                        Print($"[GetByInvoke] {label} took {sw.Elapsed.TotalMilliseconds:F2} ms");
                    }
                    done.Set();
                }
            });

            done.Wait(); // wait for main thread to finish
            return result;
        }

        // *********** FIXED RANGE PROFILE ***********
        // LLM code generating was used to quickly get the Drawings (Rectangles/Texts/ControlGrid) logic.

        void RangeInitialize()
        {
            Chart.ObjectsUpdated += OnObjectsUpdated;
            Chart.ZoomChanged += HiddenRangeControls;
        }

        private void OnObjectsUpdated(ChartObjectsEventArgs args)
        {
            if (!ProfileParams.EnableFixedRange)
                return;

            foreach (var rect in RangeObjs.rectangles.ToArray())
            {
                if (rect == null) continue;

                if (rect.IsInteractive)
                    UpdateRectangle(rect);

                if (ShowFixedInfo)
                    UpdateInfoBox(rect);

                UpdateControlGrid(rect);
            }
        }
        private void HiddenRangeControls(ChartZoomEventArgs args)
        {
            foreach (var control in RangeObjs.controlGrids.Values)
                control.IsVisible = args.Chart.ZoomLevel >= FixedHiddenZoom;
        }

        public void CreateNewRange()
        {
            // Use Mini Interval as first X/Y axis
            DateTime lastBarDate = Bars.LastBar.OpenTime;
            int miniIndex = MiniVPs_Bars.OpenTimes.GetIndexByTime(lastBarDate);
            int miniStart = Bars.OpenTimes.GetIndexByTime(MiniVPs_Bars.OpenTimes[miniIndex]);

            string nameKey = $"FixedRange_{DateTime.UtcNow.Ticks}";
            ChartRectangle rect = Chart.DrawRectangle(
                nameKey,
                Bars.OpenTimes[miniStart],
                MiniVPs_Bars.LowPrices[miniIndex],
                lastBarDate,
                MiniVPs_Bars.HighPrices[miniIndex],
                FixedColor,
                2,
                LineStyle.Lines
            );

            rect.IsInteractive = true;
            RangeObjs.rectangles.Add(rect);

            FixedRank.Add(nameKey, new VolumeRankType());

            if (ShowFixedInfo)
                CreateInfoBox(rect);

            CreateControlGrid(rect);
        }

        private void CreateInfoBox(ChartRectangle rect)
        {
            string prefixName = $"{rect.Name}_InfoBox";

            List<ChartText> list = new();
            ChartText fromTxt = Chart.DrawText(prefixName + "_From", "", rect.Time1, rect.Y1, FixedColor);
            ChartText toTxt = Chart.DrawText(prefixName + "_To", "", rect.Time1, rect.Y1, FixedColor);
            ChartText spanTxt = Chart.DrawText(prefixName + "_Span", "", rect.Time1, rect.Y1, FixedColor);

            foreach (ChartText t in new[] { fromTxt, toTxt, spanTxt }) {
                t.FontSize = 11;
                t.VerticalAlignment = VerticalAlignment.Bottom;
                list.Add(t);
            }

            RangeObjs.infoObjects[rect.Name] = list;
            UpdateInfoBox(rect);
        }

        private void CreateControlGrid(ChartRectangle rect)
        {
            Grid grid = new(2, 1)
            {
                Style = Styles.CreateButtonStyle(),
                Margin = 0,
                Height = 75,
                Width = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            CheckBox fixCheck = new()
            {
                IsChecked = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            fixCheck.Click += (s) =>
            {
                bool isFixed = (bool)fixCheck.IsChecked;
                rect.IsInteractive = !isFixed;
                rect.LineStyle = isFixed ? LineStyle.Solid : LineStyle.Lines;
            };

            Button delBtn = new()
            {
                Text = "🗑️",
                Width = 20,
                Height = 20,
                FontSize = 11,
                Padding = 0,
                Margin = "0 0 0 0",
                BackgroundColor = Color.Crimson,
                ForegroundColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            delBtn.Click += (_) => DeleteRectangle(rect);

            grid.AddChild(fixCheck, 0, 0);
            grid.AddChild(delBtn, 1, 0);

            Border border = new()
            {
                Child = grid
            };

            Chart.AddControl(border, rect.Time2, rect.Y2);
            RangeObjs.controlGrids[rect.Name] = border;
        }

        private void UpdateRectangle(ChartRectangle rect)
        {
            DateTime start = rect.Time1 < rect.Time2 ? rect.Time1 : rect.Time2;
            DateTime end = rect.Time1 < rect.Time2 ? rect.Time2 : rect.Time1;

            int startIdx = Bars.OpenTimes.GetIndexByTime(start);
            int endIdx = Bars.OpenTimes.GetIndexByTime(end);
            if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) return;

            double high = double.MinValue;
            double low = double.MaxValue;
            for (int i = startIdx; i <= endIdx; i++)
            {
                if (Bars.HighPrices[i] > high) high = Bars.HighPrices[i];
                if (Bars.LowPrices[i] < low) low = Bars.LowPrices[i];
            }

            rect.Y1 = high;
            rect.Y2 = low;
            rect.Time1 = Bars.OpenTimes[startIdx];
            rect.Time2 = Bars.OpenTimes[endIdx];

            // Update/Draw
            double bottomY = Math.Min(rect.Y1, rect.Y2);
            double topY = Math.Max(rect.Y1, rect.Y2);

            ResetFixedRange(rect.Name, end);

            for (int i = startIdx; i <= endIdx; i++)
                VolumeProfile(startIdx, i, ExtraProfiles.Fixed, fixedKey: rect.Name, fixedLowest: bottomY, fixedHighest: topY);
        }

        private void UpdateInfoBox(ChartRectangle rect)
        {
            if (!RangeObjs.infoObjects.TryGetValue(rect.Name, out var objs)) return;
            if (objs.Count < 3) return;

            ChartText fromTxt = objs[0];
            ChartText toTxt = objs[1];
            ChartText spanTxt = objs[2];

            DateTime start = rect.Time1 < rect.Time2 ? rect.Time1 : rect.Time2;
            DateTime end = rect.Time1 < rect.Time2 ? rect.Time2 : rect.Time1;
            TimeSpan interval = end.Subtract(start);
            double interval_ms = interval.TotalMilliseconds;

            // Dynamic TimeLapse Format
            string[] interval_timelapse = GetTimeLapse(interval_ms);
            string timelapse_Fmtd = interval_timelapse[0] + interval_timelapse[1];

            int startIdx = Bars.OpenTimes.GetIndexByTime(start);
            int endIdx = Bars.OpenTimes.GetIndexByTime(end);
            if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) return;

            fromTxt.Text = $"{start:MM/dd HH:mm}";
            toTxt.Text = $"{end:MM/dd HH:mm}";
            spanTxt.Text = timelapse_Fmtd;

            double maxLength = end.Subtract(start).TotalMilliseconds;
            DateTime midTime = start.AddMilliseconds(maxLength / 2);
            double textY = Math.Max(rect.Y1, rect.Y2);

            fromTxt.Time = rect.Time1;
            fromTxt.Y = textY;

            spanTxt.Time = midTime;
            spanTxt.Y = textY;
            spanTxt.HorizontalAlignment = HorizontalAlignment.Center;

            toTxt.Time = rect.Time2;
            toTxt.Y = textY;
            toTxt.HorizontalAlignment = HorizontalAlignment.Left;
        }

        private void UpdateControlGrid(ChartRectangle rect)
        {
            if (!RangeObjs.controlGrids.TryGetValue(rect.Name, out var grid)) return;
            double topY = Math.Max(rect.Y1, rect.Y2);
            DateTime rightTime = rect.Time1 > rect.Time2 ? rect.Time1 : rect.Time2;
            Chart.MoveControl(grid, rightTime, topY);
        }

        public void DeleteRectangle(ChartRectangle rect)
        {
            if (rect == null) return;
            Chart.RemoveObject(rect.Name);
            RangeObjs.rectangles.Remove(rect);

            // remove info objects
            if (RangeObjs.infoObjects.TryGetValue(rect.Name, out var objs))
            {
                foreach (var o in objs)
                    Chart.RemoveObject(o.Name);
                RangeObjs.infoObjects.Remove(rect.Name);
            }

            // remove control grid
            if (RangeObjs.controlGrids.TryGetValue(rect.Name, out var grid))
            {
                Chart.RemoveControl(grid);
                RangeObjs.controlGrids.Remove(rect.Name);
            }

            // remove histograms/lines drawings
            DateTime end = rect.Time1 < rect.Time2 ? rect.Time2 : rect.Time1;
            ResetFixedRange(rect.Name, end);
        }

        private void ResetFixedRange(string fixedKey, DateTime end)
        {
            FixedRank[fixedKey].Normal.Clear();
            FixedRank[fixedKey].Up.Clear();
            FixedRank[fixedKey].Down.Clear();
            FixedRank[fixedKey].Delta.Clear();
            FixedRank[fixedKey].MinMaxDelta = new double[2];

            List<double> whichSegment;
            if (ProfileParams.SegmentsFixedRange_Input == SegmentsFixedRange_Data.Monthly_Aligned) {
                int endIdx = Bars.OpenTimes.GetIndexByTime(end);
                int TF_idx = MonthlyBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[endIdx]); //Segments are always monthly
                whichSegment = segmentsDict[TF_idx];
            }
            else {
                if (!segmentsFromProfile.ContainsKey(fixedKey))
                    segmentsFromProfile.Add(fixedKey, new List<double>());
                whichSegment = segmentsFromProfile[fixedKey];   
            }
            
            for (int i = 0; i < whichSegment.Count; i++)
            {
                // Histograms
                Chart.RemoveObject($"{fixedKey}_{i}_VP_Fixed_Normal");

                Chart.RemoveObject($"{fixedKey}_{i}_VP_Fixed_Sell");
                Chart.RemoveObject($"{fixedKey}_{i}_VP_Fixed_Buy");

                Chart.RemoveObject($"{fixedKey}_{i}_VP_Fixed_Delta");

                // HVN + LVN
                Chart.RemoveObject($"{fixedKey}_LVN_Low_{i}_Fixed");
                Chart.RemoveObject($"{fixedKey}_LVN_{i}_Fixed");
                Chart.RemoveObject($"{fixedKey}_LVN_High_{i}_Fixed");
                Chart.RemoveObject($"{fixedKey}_LVN_Band_{i}_Fixed");

                Chart.RemoveObject($"{fixedKey}_HVN_Low_{i}_Fixed");
                Chart.RemoveObject($"{fixedKey}_HVN_{i}_Fixed");
                Chart.RemoveObject($"{fixedKey}_HVN_High_{i}_Fixed");
                Chart.RemoveObject($"{fixedKey}_HVN_Band_{i}_Fixed");
            }

            string[] objsNames = new string[14] {
                $"{fixedKey}_VP_Fixed_Normal_Result",

                $"{fixedKey}_VP_Fixed_Sell_Sum",
                $"{fixedKey}_VP_Fixed_Buy_Sum",
                $"{fixedKey}_VP_Fixed_BuySell_Result",

                $"{fixedKey}_VP_Fixed_Delta_BuySum",
                $"{fixedKey}_VP_Fixed_Delta_SellSum",
                $"{fixedKey}_VP_Fixed_Delta_Result",

                $"{fixedKey}_VP_Fixed_Delta_MinResult",
                $"{fixedKey}_VP_Fixed_Delta_MaxResult",
                $"{fixedKey}_VP_Fixed_Delta_SubResult",

                $"{fixedKey}_POC_Fixed",
                $"{fixedKey}_VAH_Fixed",
                $"{fixedKey}_VAL_Fixed",
                $"{fixedKey}_RectVA_Fixed",
            };

            foreach (string name in objsNames)
                Chart.RemoveObject(name);
        }

        public void ResetFixedRange_Dicts() {
            RangeObjs.rectangles.Clear();
            RangeObjs.infoObjects.Clear();
            RangeObjs.controlGrids.Clear();
        }

        // ====== Functions Area ======
        public string FormatBigNumber(double num)
        {
            /*
                MaxDigits = 2
                123        ->  123
                1234       ->  1.23k
                12345      ->  12.35k
                123456     ->  123.45k
                1234567    ->  1.23M
                12345678   ->  12.35M
                123456789  ->  123.56M
            */
            FormatMaxDigits_Data selected = FormatMaxDigits_Input;
            string digitsThousand = selected == FormatMaxDigits_Data.Two ? "0.##k" : selected == FormatMaxDigits_Data.One ? "0.#k" : "0.k";
            string digitsMillion = selected == FormatMaxDigits_Data.Two ? "0.##M" : selected == FormatMaxDigits_Data.One ? "0.#M" : "0.M";

            if (num >= 100000000) {
                return (num / 1000000D).ToString(digitsMillion);
            }
            if (num >= 1000000) {
                return (num / 1000000D).ToString(digitsMillion);
            }
            if (num >= 100000) {
                return (num / 1000D).ToString(digitsThousand);
            }
            if (num >= 10000) {
                return (num / 1000D).ToString(digitsThousand);
            }
            if (num >= 1000) {
                return (num / 1000D).ToString(digitsThousand);
            }

            return num.ToString("#,0");
        }

        private DateTime TimeBasedOffset(DateTime dateBar, bool isSubt = false) {
            // Offset by timebased timeframe (15m bar * nº bars of 15m)
            string[] timesBased = { "Minute", "Hour", "Daily", "Day", "Weekly", "Monthly" };
            string currentTimeframe = Chart.TimeFrame.ToString();

            // Required for Price-Based Charts for manual offset
            string tfName;
            if (timesBased.Any(currentTimeframe.Contains))
                tfName = Chart.TimeFrame.ShortName.ToString();
            else
                tfName = ProfileParams.OffsetTimeframeInput.ShortName.ToString();

            // Get the time-based interval value
            string tfString = string.Join("", tfName.Where(char.IsDigit));
            int tfValue = int.TryParse(tfString, out int value) ? value : 1;

            DateTime dateToReturn = dateBar;
            int offsetCondiditon = !isSubt ? (ProfileParams.OffsetBarsInput + 1) : Math.Max(2, ProfileParams.OffsetBarsInput - 1);
            if (tfName.Contains('m'))
                dateToReturn = dateBar.AddMinutes(tfValue * offsetCondiditon);
            else if (tfName.Contains('h'))
                dateToReturn = dateBar.AddHours(tfValue * offsetCondiditon);
            else if (tfName.Contains('D'))
                dateToReturn = dateBar.AddDays(tfValue * offsetCondiditon);
            else if (tfName.Contains('W'))
                dateToReturn = dateBar.AddDays(7 * offsetCondiditon);
            else if (tfName.Contains("Month1"))
                dateToReturn = dateBar.AddMonths(tfValue * offsetCondiditon);

            return dateToReturn;
        }

        private static string[] GetTimeLapse(double interval_ms)
        {
            // Dynamic TimeLapse Format
            // from Weis & Wykoff System
            TimeSpan ts = TimeSpan.FromMilliseconds(interval_ms);

            string timelapse_Suffix = "";
            double timelapse_Value = 0;

            double[] dividedTimestamp = { ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds };
            for (int i = 0; i < dividedTimestamp.Length; i++)
            {
                if (dividedTimestamp[i] != 0)
                {
                    string suffix = i switch {
                        4 => "ms",
                        3 => "s",
                        2 => "m",
                        1 => "h",
                        _ => "d"
                    };
                    timelapse_Value = suffix switch {
                        "ms" => ts.TotalMilliseconds,
                        "s" => ts.TotalSeconds,
                        "m" => ts.TotalMinutes,
                        "h" => ts.TotalHours,
                        _ => ts.TotalDays
                    };
                    timelapse_Suffix = suffix;
                    break;
                }
            }
            string[] interval_timelapse = { $"{Math.Round(timelapse_Value, 1)}", timelapse_Suffix };
            return interval_timelapse;
        }

        private void DrawOnScreen(string Msg)
        {
            Chart.DrawStaticText("txt", $"{Msg}", V_Align, H_Align, Color.LightBlue);
        }
        private void Second_DrawOnScreen(string Msg)
        {
            Chart.DrawStaticText("txt2", $"{Msg}", VerticalAlignment.Top, HorizontalAlignment.Left, Color.LightBlue);
        }

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

        
        // *********** HVN + LVN ***********
        private void DrawVolumeNodes(Dictionary<double, double> profileDict, int iStart, DateTime x1_Start, DateTime xBar, ExtraProfiles extraTPO = ExtraProfiles.No, bool isIntraday = false, DateTime intraX1 = default, string fixedKey = "") 
        { 
            if (!NodesParams.EnableNodeDetection)
                return;
                
            string prefix = extraTPO == ExtraProfiles.Fixed ? fixedKey : $"{iStart}";
            /*
                Alternatives for ordering:
                - "SortedDictionary<>()" 
                    - for [TPO_Rank_Histogram, TPORankType.TPO_Histogram] dicts
                - tpoDict.OrderBy(x => x.key).ToDictionary(kv => kv.Key, kv => kv.Value);
                    - Then .ToArray()
                - https://dotnettips.wordpress.com/2018/01/30/performance-sorteddictionary-vs-dictionary/
            */
            
            // This approach seems more efficient.
            double[] profilePrices = profileDict.Keys.ToArray();
            Array.Sort(profilePrices);
            double[] profileValues = profilePrices.Select(key => profileDict[key]).ToArray();
            /*
            // Alternative, no LINQ
            double[] profileValues = new double[profilePrices.Length];
            for (int i = 0; i < profilePrices.Length; i++)
                profileValues[i] = tpoDict[profilePrices[i]];
            */
            
            // Calculate Kernels/Coefficientes only once.
            nodesKernel ??= NodesParams.ProfileSmooth_Input == ProfileSmooth_Data.Gaussian ?
                            NodesAnalizer.FixedKernel() : NodesAnalizer.FixedCoefficients();
            
            // Smooth values
            double[] profileSmoothed = NodesParams.ProfileSmooth_Input == ProfileSmooth_Data.Gaussian ?
                                       NodesAnalizer.GaussianSmooth(profileValues, nodesKernel) : NodesAnalizer.SavitzkyGolay(profileValues, nodesKernel);
            
            // Get indexes of LVNs/HVNs
            var (hvnsRaw, lvnsRaw) = NodesParams.ProfileNode_Input switch {
                ProfileNode_Data.LocalMinMax => NodesAnalizer.FindLocalMinMax(profileSmoothed),
                ProfileNode_Data.Topology => NodesAnalizer.ProfileTopology(profileSmoothed),
                _ => NodesAnalizer.PercentileNodes(profileSmoothed, NodesParams.pctileHVN_Value, NodesParams.pctileLVN_Value)
            };
            
            // Filter it
            if (NodesParams.onlyStrongNodes)
            {
                double globalPoc = profileSmoothed.Max();

                double hvnPct = Math.Round(NodesParams.strongHVN_Pct / 100.0, 3);
                double lvnPct = Math.Round(NodesParams.strongLVN_Pct / 100.0, 3);

                var strongHvns = new List<int>();
                var strongLvns = new List<int>();

                foreach (int idx in hvnsRaw)
                {
                    if (profileSmoothed[idx] >= hvnPct * globalPoc)
                        strongHvns.Add(idx);
                }

                foreach (int idx in lvnsRaw)
                {
                    if (profileSmoothed[idx] <= lvnPct * globalPoc)
                        strongLvns.Add(idx);
                }

                hvnsRaw = strongHvns;
                lvnsRaw = strongLvns;
            }
                
            bool isRaw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw || NodesParams.ShowNode_Input == ShowNode_Data.LVN_Raw;
            bool isBands = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands || NodesParams.ShowNode_Input == ShowNode_Data.LVN_With_Bands;
            
            if (NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile) 
            {
                ClearOldNodes();                                               
                
                if (isBands)
                {
                    Color _nodeColor = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ColorHVN : ColorLVN;

                    var hvnsGroups = NodesAnalizer.GroupConsecutiveIndexes(hvnsRaw);
                    var lvnsGroups = NodesAnalizer.GroupConsecutiveIndexes(lvnsRaw);
                    List<List<int>> nodeGroups = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnsGroups : lvnsGroups;
                    
                    string nodeName = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? "HVN" : "LVN";   
                    foreach (var group in nodeGroups) 
                    {
                        int idxLow = group[0];
                        int idxCenter = group[group.Count / 2];
                        int idxHigh = group[group.Count - 1];
                        
                        double lowPrice = profilePrices[idxLow];
                        double centerPrice = profilePrices[idxCenter];
                        double highPrice = profilePrices[idxHigh];
                        
                        ChartTrendLine low = Chart.DrawTrendLine($"{prefix}_{nodeName}_Low_{idxLow}_{extraTPO}", x1_Start, lowPrice, xBar, lowPrice, ColorBand_Lower);
                        ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{nodeName}_{idxCenter}_{extraTPO}", x1_Start, centerPrice, xBar, centerPrice, _nodeColor);
                        ChartTrendLine high = Chart.DrawTrendLine($"{prefix}_{nodeName}_High_{idxHigh}_{extraTPO}", x1_Start, highPrice, xBar, highPrice, ColorBand_Upper);   
                        ChartRectangle rectBand = Chart.DrawRectangle($"{prefix}_{nodeName}_Band_{idxCenter}_{extraTPO}", x1_Start,  lowPrice, xBar, highPrice, ColorBand);
                        
                        FinalizeBands(low, center, high, rectBand);
                    }
                } 
                else 
                    DrawRawNodes();
                
                return;
            }

            // Draw raw-nodes, if applicable
            if (isRaw)  {
                ClearOldNodes();
                DrawRawNodes();
                return;
            }
                        
            // Split profile by LVNs
            var areasBetween = new List<(int Start, int End)>();
            int start = 0;
            foreach (int lvn in lvnsRaw)
            {
                areasBetween.Add((start, lvn));
                start = lvn;
            }
            areasBetween.Add((start, profileSmoothed.Length - 1));

            // Extract mini-bells
            var bells = new List<(int Start, int End, int Poc)>();
            foreach (var (Start, End) in areasBetween)
            {
                int startIndex = Start;
                int endIndex = End;

                if (endIndex <= startIndex)
                    continue;

                int pocIdx = startIndex;
                double maxVol = profileSmoothed[startIndex];

                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    if (profileSmoothed[i] > maxVol)
                    {
                        maxVol = profileSmoothed[i];
                        pocIdx = i;
                    }
                }

                bells.Add((startIndex, endIndex, pocIdx));
            }
            
            // Extract HVN/LVN/POC + Levels
            // [(low, center, high), ...]
            var hvnLevels = new List<(double Low, double Center, double High)>();
            var hvnIndexes = new List<(int Low, int Center, int High)>();

            var lvnLevels = new List<(double Low, double Center, double High)>();
            var lvnIndexes = new List<(int Low, int Center, int High)>();

            double hvnBandPct = Math.Round(NodesParams.bandHVN_Pct / 100.0, 3);
            double lvnBandPct = Math.Round(NodesParams.bandLVN_Pct / 100.0, 3);

            foreach (var (startIdx, endIdx, pocIdx) in bells)
            {
                // HVNs/POCs + levels
                var (hvnLow, hvnHigh) = NodesAnalizer.HVN_SymmetricVA(startIdx, endIdx, pocIdx, hvnBandPct);

                hvnLevels.Add( (profilePrices[hvnLow], profilePrices[pocIdx], profilePrices[hvnHigh]) );
                hvnIndexes.Add( (hvnLow, pocIdx, hvnHigh) );

                // LVNs + Levels
                var (lvnLow, lvnHigh) = NodesAnalizer.LVN_SymmetricBand( startIdx, endIdx, lvnBandPct);

                lvnIndexes.Add( (lvnLow, startIdx, lvnHigh) );
                lvnLevels.Add( (profilePrices[lvnLow], profilePrices[startIdx], profilePrices[lvnHigh]) );
            }
            
            // Let's draw
            ClearOldNodes();

            string node = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? "HVN" : "LVN";
            Color nodeColor = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ColorHVN : ColorLVN;
            
            var nodeLvls = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnLevels : lvnLevels;
            var nodeIdxes = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? hvnIndexes : lvnIndexes;
            
            for (int i = 0; i < nodeLvls.Count; i++)
            {
                var level = nodeLvls[i];
                var index = nodeIdxes[i];
                
                ChartTrendLine low = Chart.DrawTrendLine($"{prefix}_{node}_Low_{index.Low}_{extraTPO}", x1_Start, level.Low, xBar, level.Low, ColorBand_Lower);   
                ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{node}_{index.Center}_{extraTPO}", x1_Start, level.Center, xBar, level.Center, nodeColor);   
                ChartTrendLine high = Chart.DrawTrendLine($"{prefix}_{node}_High_{index.High}_{extraTPO}", x1_Start, level.High, xBar, level.High, ColorBand_Upper);   
                ChartRectangle rectBand = Chart.DrawRectangle($"{prefix}_{node}_Band_{index.Center}_{extraTPO}", x1_Start, level.Low, xBar, level.High, ColorBand);
                
                FinalizeBands(low, center, high, rectBand);
            }
            
            // Local
            void FinalizeBands(ChartTrendLine low, ChartTrendLine center, ChartTrendLine high, ChartRectangle rectBand) 
            {
                LineStyle nodeStyle = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? LineStyleHVN : LineStyleLVN;
                int  nodeThick = NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ? ThicknessHVN : ThicknessLVN;
            
                rectBand.IsFilled = true; 
                
                low.LineStyle = LineStyleBands; high.Thickness = ThicknessBands;
                center.LineStyle = nodeStyle; center.Thickness = nodeThick;
                high.LineStyle = LineStyleBands; high.Thickness = ThicknessBands;

                DateTime extDate = extraTPO == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
                if (NodesParams.extendNodes) 
                {
                    if (!NodesParams.extendNodes_FromStart) {
                        low.Time1 = xBar;
                        center.Time1 = xBar;
                        high.Time1 = xBar;
                        rectBand.Time1 = xBar;
                    }
                    
                    center.Time2 = extDate;
                    if (NodesParams.extendNodes_WithBands) {
                        low.Time2 = extDate;
                        high.Time2 = extDate;
                        rectBand.Time2 = extDate;
                    }
                }
                
                if (isIntraday && extraTPO != ExtraProfiles.MiniVP) {
                    low.Time1 = intraX1;
                    center.Time1 = intraX1;
                    high.Time1 = intraX1;
                    rectBand.Time1 = intraX1;
                }
            }
            void DrawRawNodes() 
            {
                string nodeRaw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? "HVN" : "LVN";
                List<int> nodeIndexes = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? hvnsRaw : lvnsRaw;
                
                LineStyle nodeStyle_Raw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? LineStyleHVN : LineStyleLVN;
                int  nodeThick_Raw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? ThicknessHVN : ThicknessLVN;
                Color nodeColor_Raw = NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ? ColorHVN : ColorLVN;

                foreach (int idx in nodeIndexes) 
                {
                    double nodePrice = profilePrices[idx];
                    ChartTrendLine center = Chart.DrawTrendLine($"{prefix}_{nodeRaw}_{idx}_{extraTPO}", x1_Start, nodePrice, xBar, nodePrice, nodeColor_Raw);
                    center.LineStyle = nodeStyle_Raw; center.Thickness = nodeThick_Raw;
                                        
                    DateTime extDate = extraTPO == ExtraProfiles.Fixed ? Bars[Bars.OpenTimes.GetIndexByTime(Server.Time)].OpenTime : extendDate();
                    if (NodesParams.extendNodes) {
                        if (!NodesParams.extendNodes_FromStart)
                            center.Time1 = xBar;
                        center.Time2 = extDate;
                    }
                    
                    if (isIntraday && extraTPO != ExtraProfiles.MiniVP)
                        center.Time1 = intraX1;
                }
            }
            void ClearOldNodes() {
                // 1º remove old price levels
                // 2º allow static-update of Params-Panel
                for (int i = 0; i < profilePrices.Length; i++)
                {
                    Chart.RemoveObject($"{prefix}_LVN_Low_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_LVN_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_LVN_High_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_LVN_Band_{i}_{extraTPO}");

                    Chart.RemoveObject($"{prefix}_HVN_Low_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_HVN_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_HVN_High_{i}_{extraTPO}");
                    Chart.RemoveObject($"{prefix}_HVN_Band_{i}_{extraTPO}");
                }
            }
            DateTime extendDate() {
                string tfName = extraTPO == ExtraProfiles.No ?
                (GeneralParams.VPInterval_Input == VPInterval_Data.Daily ? "D1" :
                    GeneralParams.VPInterval_Input == VPInterval_Data.Weekly ? "W1" : "Month1" ) :
                extraTPO == ExtraProfiles.MiniVP ? ProfileParams.MiniVPs_Timeframe.ShortName.ToString() :
                extraTPO == ExtraProfiles.Weekly ?  "W1" :  "Month1";

                // Get the time-based interval value
                string tfString = string.Join("", tfName.Where(char.IsDigit));
                int tfValue = int.TryParse(tfString, out int value) ? value : 1;

                DateTime dateToReturn = xBar;
                if (tfName.Contains('m'))
                    dateToReturn = xBar.AddMinutes(tfValue * NodesParams.extendNodes_Count);
                else if (tfName.Contains('h'))
                    dateToReturn = xBar.AddHours(tfValue * NodesParams.extendNodes_Count);
                else if (tfName.Contains('D'))
                    dateToReturn = xBar.AddDays(tfValue * NodesParams.extendNodes_Count);
                else if (tfName.Contains('W'))
                    dateToReturn = xBar.AddDays(7 * NodesParams.extendNodes_Count);
                else if (tfName.Contains("Month1"))
                    dateToReturn = xBar.AddMonths(tfValue * NodesParams.extendNodes_Count);

                return dateToReturn;
            }            
        }

        // ========= ========== ==========

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
                if (ExportHistory)
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
