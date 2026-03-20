/*
--------------------------------------------------------------------------------------------------------------------------------
                        Order Flow Agreggated v2.0
                                revision 2

From srl-python-indicators/notebooks/REAME.MD:
    Actually, it's a conjunction of Volume Profile (Ticks) + Order Flow Ticks indicators.
    - Volume Profile (intervals/values) for Aggregate Order Flow data.
    - Volume Profile (segmentation) to calculate the Order Flow of each bar.

    This 'combination' gives the quality that others footprint/order-flow software have:
    - Aligned Rows for all bars on the chart, or - in our case, at the given interval.
    - Possibility to create a truly (Volume, Delta) Bubbles chart.

    It's means that Order Flow Ticks is wrong / no longer useful? Absolutely not! Think about it:

    - With ODF_Ticks you get -> exactly <- what happened inside a bar, it's like looking at:
        a microstructure (ticks) through a microscope (bar segments) using optical zoom (bar).

    - With ODF_Aggregated you get a -> structured view <- of what happened inside the bars, it's like looking at:
        a microstructure (ticks) through a filter lens (VP segments) of a microscope (VP values) using digital zoom (VP interval).

    In other words:
    - Order Flow Ticks - raw detail.
    - Order Flow Aggregated - compressed detail.

===========================

What's new in rev. 2? (2026)
"Percentages Everywhere"

(ODF) Spike(ratio) / Bubbles(ratio):
  - [Fixed / Percentage or Percentile] type
  - Independent Ratios on Params-Panel
  - Move "[Debug] Show Strength Value?" standard input to Params-Panel
(ODF) Tick Spike:
  - [Delta, Delta_BuySell_Sum, Sum_Delta] sources alternatives
  - [L1Norm, SoftMax_Power] filters alternatives
    - These filters are not suitable for real-time notification (strength values are unstable until the bar is closed)
    - So, the spike notification will be processed after its strength confirmation (bar closed) in these filters
(ODF) Bubbles Chart:
  - "Change?" for any source.
  - [Delta_BuySell_Sum, Sum_Delta] sources alternatives
  - [SoftMax_Power, L2Norm, MinMax] filters alternatives

(VP) HVN + LVN:
  - Detection:
    - Smoothing => [Gaussian, Savitzky_Golay]
    - Nodes => [LocalMinMax, Topology, Percentile]
    - (Tip) Use "Percentile" for "Savitzky_Golay".
  - Levels(bands)
    - VA-like (set by percentage)
    - (Tip) Use 'LineStyles = [Solid, Lines, LinesDots]" if any stuttering/lagging occurs when scrolling at profiles on chart (Reduce GPU workload).
(VP-Fix) Concurrent Live VP always crashing.

(cTrader Inputs) Add "Panel Mode" input:
  - 'Volume_Profile' => Only related VP inputs will be show and used.
  - 'Order_Flow_Ticks' => Only related ODF inputs will be show and used.
  - 'Both' => Self-explanatory
  
  - Just use "Both
    - if "MiniVPs <= Daily" or "Main VP?" are used.
  - Or run 2 instances of ODF_AGG 
    - On the same chart with distinct PanelMode

(CODE) Improved Performance of:
  - (ODF) Tick Spike
  - (ODF) Bubbles Chart
  - (VP) Fixed Range
  - (VP) Main VP (uses "ODF + VP" input)
  - (BOTH) 'Results'
(CODE) Massive refactor/restructure of the entire code...(finally!)
  - It's still "all-in-one" .cs file, though.

(Off-topic) Python version finally shows its advantage! hehehe
(Off-topic) New features developed in C# version
    - "Change" for any delta-result => value from nº bars instead of previous bar.

===========================
'Sprint of ODF_Agg development'

Days since ODF_Ticks rev.1.5 => 18 Days

Days of fine-tuning as final step - 6 days
    - Price Based Charts (better support)
    - Concurrent Volume Profile (performance)
    - Custom MAs (performance)

New ODF_Ticks (not ODF_AGG exclusive) features after ODF_Ticks rev.1.5 (27/08/2025)
    - Perfomance Drawing - No more "Nº Bars to Show"!
    - High-performance VP_Tick()
    - Asynchronous Tick Data Collection
    - Bubbles Chart - Ultra Bubbles Levels
    - Tick Spike Filter - Spikes Levels
    - Subtract Delta
        - Large Filter
        - As source to Bubbles Chart
    - Custom MAs for performance.
    - Another OrderFlow() Loop refactor!

New "Free Volume Profile v2.0" features after rev.1.2 (12/08/2025), but developed only in ODF_Agg (xx/09/2025).
    - Concurrent Live VP Update
    - Daily/Monthly/Weekly Shared Segments
    - Mini-VPs that uses the current shared Segments.
    - Show Any or All (Mini-VPs/Daily/Weekly/Monthly) Profiles at once!

Fix => Custom MAs:
- Always coloring yellow bars
- EMA, KAMA, Wilder, VIDYA using wrongly previous values
- Replace "MA Period" checker => from index-based to avaiable values count.
Fix => Concurrent Live VP:
- Refactor duplicated code

==========================

Why 'v2.0' suffix?
- Coming from 'v2.0 revision 1.x' indicators.
- It has Params Panel (main reason).

AUTHOR: srlcarlg

==========================

== DON"T BE an ASSHOLE SELLING this FREE and OPEN-SOURCE indicator ==
----------------------------------------------------------------------------------------------------------------------------
*/

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
    // Appended _Exporter
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public partial class OrderFlowTicksV20 : Indicator
    {
        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;
        private Button _exportButton;
        private bool _isManualCsvExportInProgress;
        private const string DefaultCsvOutputFolder = @"D:\projects\quant-trading";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private static readonly string[] ExportCsvHeaders =
        {
            "type",
            "symbol",
            "timeframe",
            "timestamp",
            "open",
            "high",
            "low",
            "close",
            "volumesRank",
            "volumesRankUp",
            "volumesRankDown",
            "deltaRank",
            "minMaxDelta",
            "spread"
        };
        private const string EventContractSchema = "event-contract/v1";
        private const string EventSource = "ctrader";
        private const string SourceInstanceName = "OrderFlowAggregatedV20";
        private const string ExportEventName = "order_flow_aggregated";

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

        [Parameter("Export History Data", DefaultValue = true, Group = "==== Python AI Export ====")]
        public bool ExportHistory { get; set; }

        [Parameter("Direct CSV Export", DefaultValue = true, Group = "==== Python AI Export ====")]
        public bool DirectCsvExport { get; set; }

        [Parameter("CSV Output Folder", DefaultValue = DefaultCsvOutputFolder, Group = "==== Python AI Export ====")]
        public string CsvOutputFolder { get; set; }

        public enum LoadTickFrom_Data
        {
            Today,
            Yesterday,
            Before_Yesterday,
            One_Week,
            Two_Week,
            Monthly,
            Custom
        }
        [Parameter("Load From:", DefaultValue = LoadTickFrom_Data.Today, Group = "==== Tick Volume Settings ====")]
        public LoadTickFrom_Data LoadTickFrom_Input { get; set; }

        public enum LoadTickStrategy_Data
        {
            At_Startup_Sync,
            On_ChartStart_Sync,
            On_ChartEnd_Async
        }
        [Parameter("Load Type:", DefaultValue = LoadTickStrategy_Data.On_ChartEnd_Async, Group = "==== Tick Volume Settings ====")]
        public LoadTickStrategy_Data LoadTickStrategy_Input { get; set; }

        [Parameter("Custom (dd/mm/yyyy):", DefaultValue = "00/00/0000", Group = "==== Tick Volume Settings ====")]
        public string StringDate { get; set; }

        public enum LoadTickNotify_Data
        {
            Minimal,
            Detailed,
        }
        [Parameter("Notifications Type:", DefaultValue = LoadTickNotify_Data.Minimal, Group = "==== Tick Volume Settings ====")]
        public LoadTickNotify_Data LoadTickNotify_Input { get; set; }

        public enum PanelSwitch_Data
        {
            Volume_Profile,
            Order_Flow_Ticks,
            Both
        }
        [Parameter("Panel Mode:", DefaultValue = PanelSwitch_Data.Both, Group = "==== Order Flow Aggregated v2.0 ====")]
        public PanelSwitch_Data PanelSwitch_Input { get; set; }
        
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
        [Parameter("Panel Position:", DefaultValue = PanelAlign_Data.Bottom_Left, Group = "==== Order Flow Aggregated v2.0 ====")]
        public PanelAlign_Data PanelAlign_Input { get; set; }

        public enum StorageKeyConfig_Data
        {
            Symbol_Timeframe,
            Broker_Symbol_Timeframe
        }
        [Parameter("Storage By:", DefaultValue = StorageKeyConfig_Data.Broker_Symbol_Timeframe, Group = "==== Order Flow Aggregated v2.0 ====")]
        public StorageKeyConfig_Data StorageKeyConfig_Input { get; set; }

        public enum RowConfig_Data
        {
            ATR,
            Custom,
        }
        [Parameter("Row Config:", DefaultValue = RowConfig_Data.ATR, Group = "==== Order Flow Aggregated v2.0 ====")]
        public RowConfig_Data RowConfig_Input { get; set; }

        [Parameter("Custom Row(pips):", DefaultValue = 0.2, MinValue = 0.2, Group = "==== Order Flow Aggregated v2.0 ====")]
        public double CustomHeightInPips { get; set; }


        [Parameter("ATR Period:", DefaultValue = 5, MinValue = 1, Group = "==== ATR Row Config ====")]
        public int ATRPeriod { get; set; }

        [Parameter("Row Detail(%):", DefaultValue = 70, MinValue = 20, MaxValue = 100, Group = "==== ATR Row Config ====")]
        public int RowDetailATR { get; set; }

        [Parameter("Replace Loaded Row?", DefaultValue = false, Group = "==== ATR Row Config ====")]
        public bool ReplaceByATR { get; set; }


        public enum DrawingStrategy_Data
        {
            Hidden_Slowest,
            Redraw_Fastest
        }
        [Parameter("Drawing Strategy", DefaultValue = DrawingStrategy_Data.Redraw_Fastest, Group = "==== Performance Drawing ====")]
        public DrawingStrategy_Data DrawingStrategy_Input { get; set; }

        [Parameter("[Debug] Show Count?:", DefaultValue = false , Group = "==== Performance Drawing ====")]
        public bool ShowDrawingInfo { get; set; }



        [Parameter("[ODF] Use Custom MAs?", DefaultValue = true, Group = "==== Specific Parameters ====")]
        public bool UseCustomMAs { get; set; }

        public enum UpdateVPStrategy_Data
        {
            Concurrent,
            SameThread_MayFreeze
        }
        [Parameter("[VP] Update Strategy", DefaultValue = UpdateVPStrategy_Data.Concurrent, Group = "==== Specific Parameters ====")]
        public UpdateVPStrategy_Data UpdateVPStrategy_Input { get; set; }

        [Parameter("[Renko] Show Wicks?", DefaultValue = true, Group = "==== Specific Parameters ====")]
        public bool ShowWicks { get; set; }


        [Parameter("Show Controls at Zoom(%):", DefaultValue = 10, Group = "==== Fixed Range ====")]
        public int FixedHiddenZoom { get; set; }

        [Parameter("Show Info?", DefaultValue = true, Group = "==== Fixed Range ====")]
        public bool ShowFixedInfo { get; set; }

        [Parameter("Rectangle Color:", DefaultValue = "#6087CEEB", Group = "==== Fixed Range ====")]
        public Color FixedColor { get; set; }


        public enum FormatMaxDigits_Data
        {
            Zero,
            One,
            Two,
        }
        [Parameter("Format Max Digits:", DefaultValue = FormatMaxDigits_Data.One, Group = "==== Big Numbers ====")]
        public FormatMaxDigits_Data FormatMaxDigits_Input { get; set; }

        [Parameter("Format Numbers?", DefaultValue = true, Group = "==== Big Numbers ====")]
        public bool FormatNumbers { get; set; }

        [Parameter("Format Results?", DefaultValue = true, Group = "==== Big Numbers ====")]
        public bool FormatResults { get; set; }


        [Parameter("Font Size Numbers:", DefaultValue = 8, MinValue = 1, MaxValue = 80, Group = "==== Font Size ====")]
        public int FontSizeNumbers { get; set; }

        [Parameter("Font Size Results:", DefaultValue = 10, MinValue = 1, MaxValue = 80, Group = "==== Font Size ====")]
        public int FontSizeResults { get; set; }


        public enum ResultsColoring_Data
        {
            bySide,
            Fixed,
        }
        [Parameter("Results Coloring:", DefaultValue = ResultsColoring_Data.bySide, Group = "==== Results/Numbers ====")]
        public ResultsColoring_Data ResultsColoring_Input { get; set; }

        [Parameter("Fixed Color RT/NB:", DefaultValue = "#CCFFFFFF", Group = "==== Results/Numbers ====")]
        public Color RtnbFixedColor { get; set; }


        [Parameter("Large R. Color", DefaultValue = "Gold", Group = "==== Large Result Filter ====")]
        public Color ColorLargeResult { get; set; }

        [Parameter("Coloring Bar?", DefaultValue = true, Group = "==== Large Result Filter ====")]
        public bool LargeFilter_ColoringBars { get; set; }

        [Parameter("[Delta] Coloring Cumulative?", DefaultValue = true, Group = "==== Large Result Filter ====")]
        public bool LargeFilter_ColoringCD { get; set; }


        [Parameter("[Levels][Spike] Show Touch Value?", DefaultValue = false, Group = "==== Debug(both) ====")]
        public bool SpikeLevels_ShowValue { get; set; }

        [Parameter("[Levels][Bubbles] Show Touch Value?", DefaultValue = false, Group = "==== Debug(both) ====")]
        public bool UltraBubbles_ShowValue { get; set; }


        [Parameter("Bubbles Chart Opacity(%):", DefaultValue = 40, MinValue = 1, MaxValue = 100, Group = "==== Spike HeatMap Coloring ====")]
        public int SpikeChart_Opacity { get; set; }

        [Parameter("Lowest Color:", DefaultValue = "Aqua", Group = "==== Spike HeatMap Coloring ====")]
        public Color SpikeLowest_Color { get; set; }

        [Parameter("Low Color:", DefaultValue = "White", Group = "==== Spike HeatMap Coloring ====")]
        public Color SpikeLow_Color { get; set; }

        [Parameter("Average Color:", DefaultValue = "#DAFFFF00", Group = "==== Spike HeatMap Coloring ====")]
        public Color SpikeAverage_Color { get; set; }

        [Parameter("High Color:", DefaultValue = "#DAFFC000", Group = "==== Spike HeatMap Coloring ====")]
        public Color SpikeHigh_Color { get; set; }

        [Parameter("Ultra Color:", DefaultValue = "#DAFF0000", Group = "==== Spike HeatMap Coloring ====")]
        public Color SpikeUltra_Color { get; set; }


        [Parameter("Opacity(%):", DefaultValue = 70, MinValue = 1, Step = 1, MaxValue = 100, Group = "==== Bubbles HeatMap Coloring ====")]
        public int BubblesOpacity { get; set; }

        [Parameter("Lowest Color:", DefaultValue = "Aqua", Group = "==== Bubbles HeatMap Coloring ====")]
        public Color HeatmapLowest_Color { get; set; }

        [Parameter("Low Color:", DefaultValue = "White", Group = "==== Bubbles HeatMap Coloring ====")]
        public Color HeatmapLow_Color { get; set; }

        [Parameter("Average Color:", DefaultValue = "Yellow", Group = "==== Bubbles HeatMap Coloring ====")]
        public Color HeatmapAverage_Color { get; set; }

        [Parameter("High Color:", DefaultValue = "Goldenrod", Group = "==== Bubbles HeatMap Coloring ====")]
        public Color HeatmapHigh_Color { get; set; }

        [Parameter("Ultra Color:", DefaultValue = "Red", Group = "==== Bubbles HeatMap Coloring ====")]
        public Color HeatmapUltra_Color { get; set; }


        [Parameter("Color Volume:", DefaultValue = "#B287CEEB", Group = "==== Volume ====")]
        public Color VolumeColor { get; set; }

        [Parameter("Color Largest Volume:", DefaultValue = "#B2FFD700", Group = "==== Volume ====")]
        public Color VolumeLargeColor { get; set; }


        [Parameter("Color Buy:", DefaultValue = "#B200BFFF", Group = "==== Buy ====")]
        public Color BuyColor { get; set; }

        [Parameter("Color Largest Buy:", DefaultValue = "#B2FFD700", Group = "==== Buy ====")]
        public Color BuyLargeColor { get; set; }


        [Parameter("Color Sell:", DefaultValue = "#B2DC143C", Group = "==== Sell ====")]
        public Color SellColor { get; set; }

        [Parameter("Color Largest Sell:", DefaultValue = "#B2DAA520", Group = "==== Sell ====")]
        public Color SellLargeColor { get; set; }


        [Parameter("Color Weekly:", DefaultValue = "#B2FFD700", Group = "==== WM Profiles ====")]
        public Color WeeklyColor { get; set; }

        [Parameter("Color Monthly:", DefaultValue = "#920071C1", Group = "==== WM Profiles ====")]
        public Color MonthlyColor { get; set; }



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


        // ========= Moved from cTrader Input to Params Panel =========

        // ==== General ====
        public enum VolumeMode_Data
        {
            Normal,
            Buy_Sell,
            Delta,
        }
        public enum VolumeView_Data
        {
            Divided,
            Profile,
        }

        public enum IntensityMode_Data
        {
            Per_Bar,
            Global_Lookback,
            Global_N_Days
        }

        public class GeneralParams_Info {
            public int Lookback = 1;
            public VolumeMode_Data VolumeMode_Input = VolumeMode_Data.Delta;
            public VolumeView_Data VolumeView_Input = VolumeView_Data.Profile;

            // Coloring region - only for VolumeView_Data.Divided
            public bool ColoringOnlyLarguest = true;
            public bool ColoringIntensity = false;
            public IntensityMode_Data IntensityMode_Input = IntensityMode_Data.Per_Bar;
            public int IntensityNDays_Input = 1;
            public Dictionary<int, double> BarMaxDeltaCache = new Dictionary<int, double>();
            public Dictionary<int, double> BarMaxVolumeCache = new Dictionary<int, double>();
            public Dictionary<int, double> BarMaxBuySellCache = new Dictionary<int, double>();
        }
        public GeneralParams_Info GeneralParams = new();


        // ==== Volume Profile ====
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
        public class ProfileParams_Info {
            public bool EnableMainVP = false;

            // View
            public UpdateProfile_Data UpdateProfile_Input = UpdateProfile_Data.Through_2_Segments_Best;
            public bool FillHist_VP = false;
            public bool ShowHistoricalNumbers = false;
            public HistSide_Data HistogramSide_Input = HistSide_Data.Left;
            public HistWidth_Data HistogramWidth_Input = HistWidth_Data._70;

            // FWM Profiles
            public bool EnableFixedRange = false;
            public bool EnableWeeklyProfile = false;
            public bool EnableMonthlyProfile = false;

            // Intraday Profiles
            public bool ShowIntradayProfile = false;
            public bool ShowIntradayNumbers = false;
            public int OffsetBarsInput = 1;
            public TimeFrame OffsetTimeframeInput = TimeFrame.Hour;
            public bool FillIntradaySpace = false;

            // Mini VPs
            public bool EnableMiniProfiles = false;
            public TimeFrame MiniVPs_Timeframe = TimeFrame.Hour4;
            public bool ShowMiniResults = true;
        }
        public ProfileParams_Info ProfileParams = new();


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

            public bool EnableNodeDetection = false;

            public ProfileSmooth_Data ProfileSmooth_Input = ProfileSmooth_Data.Gaussian;
            public ProfileNode_Data ProfileNode_Input = ProfileNode_Data.LocalMinMax;

            public ShowNode_Data ShowNode_Input = ShowNode_Data.HVN_With_Bands;
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

        // ==== Spike Filter ====
        public enum SpikeView_Data
        {
            Bubbles,
            Icon,
        }
        public enum SpikeSource_Data
        {
            Delta,
            Delta_BuySell_Sum,
            Sum_Delta,
        }
        public enum SpikeFilter_Data
        {
            MA,
            Standard_Deviation,
            L1Norm,
            SoftMax_Power
        }
        public enum NotificationType_Data
        {
            Popup,
            Sound,
            Both
        }
        public enum SpikeChartColoring_Data
        {
            Heatmap,
            Positive_Negative,
            PlusMinus_Highlight_Heatmap,
        }

        public class SpikeFilterParams_Info {
            public bool EnableSpikeFilter = true;
            public SpikeView_Data SpikeView_Input = SpikeView_Data.Icon;
            public ChartIconType IconView_Input = ChartIconType.Square;

            // Filter Settings
            public SpikeSource_Data SpikeSource_Input = SpikeSource_Data.Delta;
            public SpikeFilter_Data SpikeFilter_Input = SpikeFilter_Data.MA;

            public MovingAverageType MAtype = MovingAverageType.Simple;
            public int MAperiod = 20;

            // Notifications
            public bool EnableSpikeNotification = true;
            public NotificationType_Data NotificationType_Input = NotificationType_Data.Both;
            public SoundType Spike_SoundType = SoundType.Confirmation;

            // Chart
            public bool EnableSpikeChart = false;
            public SpikeChartColoring_Data SpikeChartColoring_Input = SpikeChartColoring_Data.Heatmap;
        }
        public SpikeFilterParams_Info SpikeFilterParams = new();


        // ==== Spike Levels ====
        public enum SpikeLevelsColoring_Data
        {
            Heatmap,
            Positive_Negative
        }
        public class SpikeLevelParams_Info {
            public bool ShowSpikeLevels = false;
            public bool ResetDaily = true;
            public int MaxCount = 2;

            public SpikeLevelsColoring_Data SpikeLevelsColoring_Input = SpikeLevelsColoring_Data.Positive_Negative;
        }
        public SpikeLevelParams_Info SpikeLevelParams = new();


        // ==== Spike Ratio ====
        public enum SpikeRatio_Data
        {
            Fixed,
            Percentage,
        }
        public class SpikeRatioParams_Info {
            public SpikeRatio_Data SpikeRatio_Input = SpikeRatio_Data.Percentage;
            public MovingAverageType MAtype_PctSpike = MovingAverageType.Simple;
            public int MAperiod_PctSpike = 20;
            public bool ShowStrengthValue = false;

            // Fixed Ratio
            public double Lowest_FixedValue = 0.5;
            public double Low_FixedValue = 1;
            public double Average_FixedValue = 1.5;
            public double High_FixedValue = 2;
            public double Ultra_FixedValue = 2.01;

            // Percentage Ratio
            public double Lowest_PctValue = 38.2;
            public double Low_PctValue = 61.8;
            public double Average_PctValue = 78.6;
            public double High_PctValue = 100;
            public double Ultra_PctValue = 101;
        }
        public SpikeRatioParams_Info SpikeRatioParams = new();


        // ==== Bubbles Chart ====
        public enum BubblesSource_Data
        {
            Delta,
            Delta_BuySell_Sum,
            Subtract_Delta,
            Sum_Delta,
        }
        public enum ChangeOperator_Data {
            Plus_KeepSign,
            Minus_KeepSign,
            Plus_Absolute,
            Minus_Absolue
        }
        public enum BubblesFilter_Data
        {
            MA,
            Standard_Deviation,
            Both,
            SoftMax_Power,
            L2Norm,
            MinMax,
        }
        public enum BubblesColoring_Data
        {
            Heatmap,
            Momentum,
        }
        public enum BubblesMomentum_Data
        {
            Fading,
            Positive_Negative,
        }

        public class BubblesChartParams_Info {
            public bool EnableBubblesChart = false;

            // Filter Settings
            public BubblesSource_Data BubblesSource_Input = BubblesSource_Data.Delta;
            
            public bool UseChangeSeries = false;
            public int changePeriod = 4;
            public ChangeOperator_Data ChangeOperator_Input = ChangeOperator_Data.Plus_KeepSign;

            public BubblesFilter_Data BubblesFilter_Input = BubblesFilter_Data.MA;
            public MovingAverageType MAtype = MovingAverageType.Exponential;
            public int MAperiod = 20;

            // View
            public double BubblesSizeMultiplier = 2;
            public BubblesColoring_Data BubblesColoring_Input = BubblesColoring_Data.Heatmap;
            public BubblesMomentum_Data BubblesMomentum_Input = BubblesMomentum_Data.Fading;
        }
        public BubblesChartParams_Info BubblesChartParams = new();



        // ==== Ultra Bubbles Levels ====
        public enum UltraBubbles_RectSizeData
        {
            High_Low,
            HighOrLow_Close,
            Bubble_Size,
        }
        public enum UltraBubblesBreak_Data
        {
            Close_Only,
            Close_plus_BarBody,
            OHLC_plus_BarBody,
        }
        public enum UltraBubblesColoring_Data
        {
            Bubble_Color,
            Positive_Negative
        }

        public class BubblesLevelParams_Info {
            public bool ShowUltraLevels = false;

            // Notification
            public bool EnableUltraNotification = true;
            public NotificationType_Data NotificationType_Input = NotificationType_Data.Both;
            public SoundType Ultra_SoundType = SoundType.PositiveNotification;

            // Levels settings
            public bool ResetDaily = true;
            public int MaxCount = 5;
            public UltraBubbles_RectSizeData UltraBubbles_RectSizeInput = UltraBubbles_RectSizeData.Bubble_Size;
            public UltraBubblesBreak_Data UltraBubblesBreak_Input = UltraBubblesBreak_Data.Close_Only;

            // View
            public UltraBubblesColoring_Data UltraBubblesColoring_Input = UltraBubblesColoring_Data.Positive_Negative;
        }
        public BubblesLevelParams_Info BubblesLevelParams = new();


        public enum BubblesRatio_Data
        {
            Fixed,
            Percentile,
        }
        public class BubblesRatioParams_Info {
            public BubblesRatio_Data BubblesRatio_Input = BubblesRatio_Data.Percentile;
            public int PctilePeriod = 20;
            public bool ShowStrengthValue = false;

            // Fixed Ratio
            public double Lowest_FixedValue = 0.5;
            public double Low_FixedValue = 1;
            public double Average_FixedValue = 1.5;
            public double High_FixedValue = 2;
            public double Ultra_FixedValue = 2.01;

            // Percentile Ratio
            public int Lowest_PctileValue = 40;
            public int Low_PctileValue = 70;
            public int Average_PctileValue = 90;
            public int High_PctileValue = 97;
            public int Ultra_PctileValue = 99;
        }
        public BubblesRatioParams_Info BubblesRatioParams = new();


        // ==== Results ====
        public enum ResultsView_Data
        {
            Percentage,
            Value,
            Both
        }
        public enum OperatorBuySell_Data
        {
            Sum,
            Subtraction,
        }

        public class ResultParams_Info {
            public bool ShowResults = true;

            // Large Filter
            public bool EnableLargeFilter = true;
            public MovingAverageType MAtype = MovingAverageType.Exponential;
            public int MAperiod = 5;
            public double LargeRatio = 1.5;

            // Buy_Sell / Delta
            public ResultsView_Data ResultsView_Input = ResultsView_Data.Percentage;
            public bool ShowSideTotal = true;
            public OperatorBuySell_Data OperatorBuySell_Input = OperatorBuySell_Data.Subtraction;

            // Delta
            public bool ShowMinMaxDelta = false;
            public bool ShowOnlySubtDelta = true;
        }
        public ResultParams_Info ResultParams = new();


        // ==== Misc ====
        public enum SegmentsInterval_Data
        {
            Daily,
            Weekly,
            Monthly
        }
        public enum ODFInterval_Data
        {
            Daily,
            Weekly,
        }

        public class MiscParams_Info {
            public bool ShowHist = true;
            public bool FillHist = true;
            public bool ShowNumbers = true;
            public int DrawAtZoom_Value = 80;

            public SegmentsInterval_Data SegmentsInterval_Input = SegmentsInterval_Data.Weekly;
            public ODFInterval_Data ODFInterval_Input = ODFInterval_Data.Daily;

            public bool ShowBubbleValue = true;
        }
        public MiscParams_Info MiscParams = new();

        // ======================================================

        public readonly string NOTIFY_CAPTION = "Order Flow Ticks \n    Aɢɢʀᴇɢᴀᴛᴇᴅ";

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

        // Order Flow Ticks
        private List<double> Segments_Bar = new();
        private readonly Dictionary<double, int> VolumesRank = new();
        private readonly Dictionary<double, int> VolumesRank_Up = new();
        private readonly Dictionary<double, int> VolumesRank_Down = new();
        private readonly Dictionary<double, int> DeltaRank = new();
        private int[] MinMaxDelta = { 0, 0 };

        // Volume Profile Ticks
        private List<double> Segments_VP = new();
        private Dictionary<double, double> VP_VolumesRank = new();
        private Dictionary<double, double> VP_VolumesRank_Up = new();
        private Dictionary<double, double> VP_VolumesRank_Down = new();
        private Dictionary<double, double> VP_VolumesRank_Subt = new();
        private Dictionary<double, double> VP_DeltaRank = new();
        private double[] VP_MinMaxDelta = { 0, 0 };

        // Weekly, Monthly and Mini VPs
        public class VolumeRankType
        {
            public Dictionary<double, double> Normal = new();
            public Dictionary<double, double> Up = new();
            public Dictionary<double, double> Down = new();
            public Dictionary<double, double> Delta = new();
            public double[] MinMaxDelta = new double[2];

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
            public readonly object Bar = new();
            public readonly object Tick = new();
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

        private DateTime[] BarTimes_Array = Array.Empty<DateTime>();
        private IEnumerable<Bar> TickBars_List;

        // High-Performance VP_Tick()
        private class PerfTickIndex {
            public int startIdx_MainVP = 0;
            public int startIdx_Mini = 0;
            public int startIdx_Weekly = 0;
            public int startIdx_Monthly = 0;

            public int lastIdx_MainVP = 0;
            public int lastIdx_Mini = 0;
            public int lastIdx_Weekly = 0;
            public int lastIdx_Monthly = 0;

            public int lastIdx_Bars = 0;
            public int lastIdx_Wicks = 0;

            public Dictionary<DateTime, int> IndexesByDate = new();

            public void ResetAll() {
                lastIdx_MainVP = 0;
                lastIdx_Mini = 0;
                lastIdx_Weekly = 0;
                lastIdx_Monthly = 0;
                lastIdx_Bars = 0;
                lastIdx_Wicks = 0;
            }
        }
        private readonly PerfTickIndex PerformanceTick = new();

        // Tick Volume
        public class TickObjs_Info {
            public DateTime firstTickTime;
            public DateTime fromDateTime;
            public ProgressBar syncProgressBar = null;
            public PopupNotification asyncPopup = null;
            public bool startAsyncLoading = false;
            public bool isLoadingComplete = false;
        }
        private readonly TickObjs_Info TickObjs = new();

        private Bars TicksOHLC;

        // Timer
        private class TimerHandler {
            public bool isAsyncLoading = false;
        }
        private readonly TimerHandler timerHandler = new();

        // Shared rowHeight
        private double heightPips = 4;
        public double heightATR = 4;
        private double rowHeight = 0;

        private double prevUpdatePrice;
        public bool isPriceBased_Chart = false;
        public bool isRenkoChart = false;

        // Some required utils
        public class BooleanUtils_Info {
            public bool segmentsConflict = false;
            public bool configHasChanged = false;
            public bool isPriceBased_NewBar = false;

            public bool isUpdateVP = false;
        }
        private readonly BooleanUtils_Info BooleanUtils = new();
        
        public class BooleanLocks_Info {
            // lock[...] mainly because of the Segments loop
            // Avoid Historical Data
            public bool spikeNotify = true;
            public bool ultraNotify = true;
            
            public bool ultraNotify_NewBar = false;
            public bool spikeNotify_NewBar = false;

            public bool lastIsUltra = false;
            public bool lastIsAvg = false;

            // Allow Historical Data
            // Although it needs to be redefined to false before each OrderFlow() call in Historical Data.
            public bool ultraLevels = false;
            public bool spikeLevels = false;

            public void SetAllToFalse() {
                spikeNotify = false;
                ultraNotify = false;
                
                ultraLevels = false;
                spikeLevels = false;
            }
            public void LevelsToFalse() {                
                ultraLevels = false;
                spikeLevels = false;
            }
            public void SetAllNewBar() {
                ultraNotify_NewBar = true;
                spikeNotify_NewBar = true;
            }
        }
        private readonly BooleanLocks_Info BooleanLocks = new();
        
        // For [Ultra Bubbles, Spike] Levels
        private class RectInfo
        {
            public int LastBarIndex;
            public bool isActive;
            public ChartRectangle Rectangle;
            public ChartText Text;
            public int Touches;
            public double Y1;
            public double Y2;
        }
        private readonly Dictionary<double, RectInfo> ultraRectangles = new ();
        private readonly Dictionary<string, RectInfo> spikeRectangles = new ();

        // Filters
        // DynamicSeries can be Normal, Buy_Sell or Delta Volume
        private IndicatorDataSeries Dynamic_Series, DeltaChange_Series, DeltaBuySell_Sum_Series,
                                    SubtractDelta_Series, SumDelta_Series,
                                    PercentageRatio_Series, PercentileRatio_Series;
        private MovingAverage MABubbles_Delta, MABubbles_DeltaChange, MABubbles_DeltaBuySell_Sum,
                              MABubbles_SubtractDelta, MABubbles_SumDelta,
                              MARatio_Percentage,
                              MASpike_Delta, MASpike_DeltaBuySell_Sum, MASpike_SumDelta,
                              MADynamic_LargeFilter, MASubtract_LargeFilter;
        private StandardDeviation StdDevBubbles_Delta, StdDevBubbles_DeltaChange, StdDevBubbles_DeltaBuySell_Sum,
                                  StdDevBubbles_SubtractDelta, StdDevBubbles_SumDelta,
                                  StdDevSpike_Delta, StdDevSpike_DeltaBuySell_Sum, StdDevSpike_SumDelta;

        // _Results => Raw Values
        private readonly Dictionary<int, int> Delta_Results = new();
        private readonly Dictionary<int, int> DeltaChange_Results = new();
        private readonly Dictionary<int, int> DeltaBuySell_Sum_Results = new();
        private readonly Dictionary<int, int> SubtractDelta_Results = new();
        private readonly Dictionary<int, int> SumDelta_Results = new();
        
        
        // Performance Drawing
        public class DrawInfo
        {
            public int BarIndex;
            public DrawType Type;
            public string Id;
            public DateTime X1;
            public double Y1;
            public DateTime X2;
            public double Y2;
            public string Text;
            public HorizontalAlignment horizontalAlignment;
            public VerticalAlignment verticalAlignment;
            public int FontSize;
            public ChartIconType IconType;
            public Color Color;
        }
        public enum DrawType
        {
            Text,
            Icon,
            Ellipse,
            Rectangle
        }

        public class PerfDrawingObjs_Info {
            /*
              Redraw should use another dict as value,
              to avoid creating previous Volume Modes objects
              or previous objects from Static Update.
              - intKey is the Bar index
              - stringKey is the DrawInfo.Id (object name)
              - DrawInfo is the current Bar object info.
            */
            public Dictionary<int, Dictionary<string, DrawInfo>> redrawInfos = new();
            /*
              For real-time market:
              - intKey is always [0]
              - stringKey is the DrawInfo.Id (object name)
              - DrawInfo is the current Bar object info.
            */
            public Dictionary<int, Dictionary<string, DrawInfo>> currentToRedraw = new();

            // It's fine to just keep the objects name as keys,
            // since hiddenInfos is populated/updated at each drawing.
            public Dictionary<string, ChartObject> hiddenInfos = new();
            /*
              For real-time market:
              - intKey is always [0]
              - stringKey is the DrawInfo.Id (object name)
              - DrawInfo is the current Bar object.
            */
            public Dictionary<int, Dictionary<string, ChartObject>> currentToHidden = new();
            public ChartStaticText staticText_DebugPerfDraw;

            public void ClearAll() {
                hiddenInfos.Clear();
                redrawInfos.Clear();
                currentToHidden.Clear();
                currentToRedraw.Clear();
            }
        }
        private readonly PerfDrawingObjs_Info PerfDrawingObjs = new();

        // Custom MAs
        public enum MAType_Data
        {
            Simple,
            Exponential,
            Weighted,
            Triangular,
            Hull,
            VIDYA,
            WilderSmoothing,
            KaufmanAdaptive,
        }

        public class CustomMAObjs {
            public MAType_Data Large = MAType_Data.Exponential;
            public MAType_Data Bubbles = MAType_Data.Exponential;
            public MAType_Data Spike = MAType_Data.Simple;
            public MAType_Data SpikePctRatio = MAType_Data.Simple;
        }
        public CustomMAObjs CustomMAType = new();

        private readonly Dictionary<int, double> _dynamicBuffer = new();
        private readonly Dictionary<int, double> _maDynamic = new();

        private class DeltaBuffer {
            public Dictionary<int, double> Change = new();
            public Dictionary<int, double> BuySell_Sum = new();
            public Dictionary<int, double> Subtract = new();
            public Dictionary<int, double> Sum = new();
            public Dictionary<int, double> Spike_PctRatio = new();

            public Dictionary<int, double> MASubtract_Large = new();

            public Dictionary<int, double> MAChange_Bubbles = new();
            public Dictionary<int, double> MABuySellSum_Bubbles = new();
            public Dictionary<int, double> MASubtract_Bubbles = new();
            public Dictionary<int, double> MASum_Bubbles = new();

            public Dictionary<int, double> MABuySellSum_Spike = new();
            public Dictionary<int, double> MASum_Spike = new();

            public Dictionary<int, double> MASpike_PctRatio = new();

            public void ClearAll()
            {
                Dictionary<int, double>[] _all = new[] {
                    Change, BuySell_Sum, Subtract, Sum, Spike_PctRatio,
                    MASubtract_Large,
                    MAChange_Bubbles, MABuySellSum_Bubbles, MASubtract_Bubbles, MASum_Bubbles,
                    MABuySellSum_Spike, MASum_Spike, MASpike_PctRatio
                };

                foreach (var dict in _all)
                    dict.Clear();
            }
        }
        private readonly DeltaBuffer _deltaBuffer = new();

        private enum MASwitch {
            Large,
            Bubbles,
            Spike,
        }
        private enum DeltaSwitch {
            None,
            DeltaChange,
            DeltaBuySell_Sum,
            Subtract,
            Sum,
            Spike_PctRatio
        }

        // Params Panel
        private Border ParamBorder;

        public class IndicatorParams
        {
            public GeneralParams_Info GeneralParams { get; set; }
            public double RowHeightInPips { get; set; }
            public ProfileParams_Info ProfileParams { get; set; }
            public NodesParams_Info NodesParams { get; set; }

            public SpikeFilterParams_Info SpikeFilterParams { get; set; }
            public SpikeLevelParams_Info SpikeLevelParams { get; set; }
            public SpikeRatioParams_Info SpikeRatioParams { get; set; }

            public BubblesChartParams_Info BubblesChartParams { get; set; }
            public BubblesLevelParams_Info BubblesLevelParams { get; set; }
            public BubblesRatioParams_Info BubblesRatioParams { get; set; }

            public ResultParams_Info ResultParams { get; set; }
            public MiscParams_Info MiscParams { get; set; }
        }

    }

}
