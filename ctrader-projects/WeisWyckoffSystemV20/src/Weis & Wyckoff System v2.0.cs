/*
--------------------------------------------------------------------------------------------------------------------------------
                    [Renko] Weis & Wyckoff System v2.0
                                revision 2
showcases the concepts of David H. Weis and Richard Wyckoff on Renko Chart

It's just a way of visualizing the Waves and Volume numerically, it's not an original idea.
You can find this way of visualization first at
(www.youtube.com/watch?v=uzISUr1itWg, most recent www.vimeo.com/394541866)

This uses the code concepts of (Numbers-Renko 数字練行足 https://www.tradingview.com/script/9BKOIhdl-Numbers-Renko/ in PineScript),
=> Cheers to the akutsusho!

I IMPROVED IT and BROUGHT IT to cTrader/C#.
I added many other features based on the original design and my personal taste, like:

(Make your favorite design template yourself): 14 design parameters with a total of 32 sub-options
(Non-Repaint and Repaint Weis Waves Option): You can choose whether to see the Current Trend Wave value.
(Dynamic TimeLapse): Time Waves showed the difference in milliseconds, seconds, minutes, hours, days!
And many others...

What's new in rev. 1? (after ODF_AGG)
- Support to [Candles, Heikin-Ash, Tick, Range] Charts
- Improved ZigZag => MTF support + [ATR, Percentage, Pips, NoLag_HighLow] Modes
- Includes all "Order Flow Aggregated" related improvements
    - Custom MAs
    - Performance Drawing
    - Strength Filters (MA/StdDev/Both)
    - High-performance VP_Tick()
    - High-performance GetWicks()
    - Asynchronous Tick Data Collection

Last update => 21/01/2026
===========================

What's new in rev.2 (2026)?

New features for 'Wyckoff Bars' => 'Coloring':
  - "L1Norm" to Strength Filter
  - "Percentile" Ratio
  - Independent Ratios on Params-Panel
    - for [Fixed, Percentile] types
    - and [Normalized_Emphasized] filter
Params-Panel
  - Move '[Wyckoff] Show Strength?' debug parameter to Params-Panel
  - Move '[ZigZag] Show Turning Point Bar?' debug parameter to Params-Panel
Fixes:
  - (params-panel) => Normalized_Emphasized parameters (Period, Multiplier) doesn't set new values.
  - (perf-drawing) => "Wyckoff Bars => Numbers" in live-market are always changed to '2' (Redraw_Fastest only)
  - (perf-drawing) => "Weis Waves => "Show Current Wave?" often flickering in live-market (Redraw_Fastest only)
    - It should/still flicks on the first developing wave after recalculating (switching parameters)

========================================================================

              Transcribed & Improved for cTrader/C#
                          by srlcarlg

        Original Code Concepts in TradingView/Pinescript
                          by akutsusho

=========================================================================

== DON"T BE an ASSHOLE SELLING this FREE and OPEN-SOURCE indicator ==
----------------------------------------------------------------------------------------------------------------------------
*/

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
    [Indicator(IsOverlay = true, AutoRescale = true, AccessRights = AccessRights.FullAccess)]
    public partial class WeisWyckoffSystemV20 : Indicator
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Button _exportButton;
        private bool _isManualCsvExportInProgress;
        private const string DefaultCsvOutputFolder = @"D:\projects\quant-trading";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private static readonly string[] ExportCsvHeaders =
        {
            "symbol",
            "timeframe",
            "timestamp",
            "open",
            "high",
            "low",
            "close",
            "wyckoffVolume",
            "wyckoffTime",
            "zigZag",
            "waveVolume",
            "wavePrice",
            "waveVolPrice",
            "waveDirection",
            "spread"
        };
        private const string EventContractSchema = "event-contract/v1";
        private const string EventSource = "ctrader";
        private const string SourceInstanceName = "WeisWyckoffSystemV20";
        private const string ExportEventName = "wyckoff_state";

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

        private double _expCumulVolume;
        private double _expCumulPrice;
        private double _expCumulVolPrice;
        private string _expWaveDirection = "None";

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
        [Parameter("Panel Position:", DefaultValue = PanelAlign_Data.Bottom_Left, Group = "==== Weis & Wyckoff System v2.0 ====")]
        public PanelAlign_Data PanelAlign_Input { get; set; }


        [Parameter("[Candles] Use 'Tick Volume' from bars?", DefaultValue = true, Group = "==== Specific Parameters ====")]
        public bool UseTimeBasedVolume { get; set; }

        [Parameter("[Wyckoff] Use Custom MAs?", DefaultValue = true, Group = "==== Specific Parameters ====")]
        public bool UseCustomMAs { get; set; }

        [Parameter("[Renko] Show Wicks?", DefaultValue = true, Group = "==== Specific Parameters ====")]
        public bool ShowWicks { get; set; }

        [Parameter("[Renko] Wicks Thickness:", DefaultValue = 1, MaxValue = 5, Group = "==== Specific Parameters ====")]
        public int RenkoThickness { get; set; }

        [Parameter("[ZZ] ATR Multiplier", DefaultValue = 2, MinValue = 0, MaxValue = 10, Group = "==== Specific Parameters ====")]
        public double ATR_Multiplier { get; set; }

        [Parameter("[ZZ] ATR Period", DefaultValue = 10, MinValue = 1, Group = "==== Specific Parameters ====")]
        public int ATR_Period { get; set; }

        public enum StorageKeyConfig_Data
        {
            Symbol_Timeframe,
            Broker_Symbol_Timeframe
        }
        [Parameter("Storage By:", DefaultValue = StorageKeyConfig_Data.Broker_Symbol_Timeframe, Group = "==== Weis & Wyckoff System v2.0 ====")]
        public StorageKeyConfig_Data StorageKeyConfig_Input { get; set; }

        [Parameter("Draw At Zoom(%)", DefaultValue = 40, Group = "==== Performance Drawing ====")]
        public int DrawAtZoom_Value { get; set; }
        public enum DrawingStrategy_Data
        {
            Hidden_Slowest,
            Redraw_Fastest
        }
        [Parameter("Drawing Strategy", DefaultValue = DrawingStrategy_Data.Redraw_Fastest, Group = "==== Performance Drawing ====")]
        public DrawingStrategy_Data DrawingStrategy_Input { get; set; }

        [Parameter("[Debug] Show Count?:", DefaultValue = false , Group = "==== Performance Drawing ====")]
        public bool ShowDrawingInfo { get; set; }


        [Parameter("Format Numbers?", DefaultValue = true, Group = "==== Numbers ====")]
        public bool FormatNumbers { get; set; }

        public enum FormatMaxDigits_Data
        {
            Zero,
            One,
            Two,
        }
        [Parameter("Format Max Digits:", DefaultValue = FormatMaxDigits_Data.One, Group = "==== Numbers ====")]
        public FormatMaxDigits_Data FormatMaxDigits_Input { get; set; }

        [Parameter("Font Size [Bars]:", DefaultValue = 11, MinValue = 1, MaxValue = 80, Group = "==== Numbers ====")]
        public int FontSizeNumbers { get; set; }

        [Parameter("Font Size [Waves]:", DefaultValue = 12, MinValue = 1, MaxValue = 80, Group = "==== Numbers ====")]
        public int FontSizeWaves { get; set; }

        [Parameter("Custom Color:", DefaultValue = "White", Group = "==== Numbers ====")]
        public Color CustomNumbersColor { get; set; }


        [Parameter("Show TrendLines?", DefaultValue = true, Group = "==== Trend Lines Settings ====")]
        public bool ShowTrendLines { get; set; }

        [Parameter("Up/Down Coloring?", DefaultValue = false, Group = "==== Trend Lines Settings ====")]
        public bool ColorfulTrendLines { get; set; }

        [Parameter("Large Wave Coloring?", DefaultValue = true, Group = "==== Trend Lines Settings ====")]
        public bool ShowYellowTrendLines { get; set; }

        [Parameter("Thickness", DefaultValue = 3, MinValue = 1, Group = "==== Trend Lines Settings ====")]
        public int TrendThickness { get; set; }

        [Parameter("NoTrend Line Color", DefaultValue = "SteelBlue", Group = "==== Trend Lines Settings ====")]
        public Color NoTrendColor { get; set; }

        [Parameter("UpTrend Line Color", DefaultValue = "Green", Group = "==== Trend Lines Settings ====")]
        public Color UpLineColor { get; set; }

        [Parameter("DownTrend Line Color", DefaultValue = "Red", Group = "==== Trend Lines Settings ====")]
        public Color DownLineColor { get; set; }


        [Parameter("[ZigZag] Invert Turning Color?", DefaultValue = true, Group = "==== Debug ====")]
        public bool InvertTurningColor { get; set; }
        [Parameter("[Weis] Show Wave Ratio?", DefaultValue = false, Group = "==== Debug ====")]
        public bool ShowRatioValue { get; set; }


        [Parameter("Opacity(%) [Nº Inside]:", DefaultValue = 70, MinValue = 1, MaxValue = 100, Group = "==== HeatMap Coloring ====")]
        public int HeatmapBars_Opacity { get; set; }

        [Parameter("Lowest Color:", DefaultValue = "#FF737373", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapLowest_Color { get; set; }

        [Parameter("Low Color:", DefaultValue = "#8F9092", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapLow_Color { get; set; }

        [Parameter("Average Color:", DefaultValue = "#D9D9D9", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapAverage_Color { get; set; }

        [Parameter("High Color [Up]:", DefaultValue = "#A1F6A1", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapHighUp_Color { get; set; }
        [Parameter("High Color [Down]:", DefaultValue = "#FA6681", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapHighDown_Color { get; set; }

        [Parameter("Ultra Color[Up]:", DefaultValue = "#1D8934", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapUltraUp_Color { get; set; }
        [Parameter("Ultra Color[Down]:", DefaultValue = "#E00106", Group = "==== HeatMap Coloring ====")]
        public Color HeatmapUltraDown_Color { get; set; }


        [Parameter("Up Wave Color", DefaultValue = "SeaGreen", Group = "==== Waves Color ====")]
        public Color UpWaveColor { get; set; }

        [Parameter("Down Wave Color", DefaultValue = "OrangeRed", Group = "==== Waves Color ====")]
        public Color DownWaveColor { get; set; }

        [Parameter("Large Wave Color", DefaultValue = "Yellow", Group = "==== Waves Color ====")]
        public Color LargeWaveColor { get; set; }


        [Parameter("Transcribed & Improved", DefaultValue = "for cTrader/C# by srlcarlg", Group = "==== Credits ====")]
        public string Credits { get; set; }
        [Parameter("Original Code Concepts", DefaultValue = "in TDV/Pinescript by akutsusho", Group = "==== Credits ====")]
        public string Credits_2 { get; set; }


        // Moved from cTrader Input to Params Panel
        public enum Template_Data
        {
            Insider,
            Volume,
            Time,
            BigBrain,
            Custom
        }
        public Template_Data Template_Input = Template_Data.Insider;

        // Wyckoff Bars
        public enum Numbers_Data
        {
            Both,
            Volume,
            Time,
            None
        }
        public enum NumbersPosition_Data
        {
            Inside,
            Outside,
        }
        public enum NumbersBothPosition_Data
        {
            Default,
            Invert,
        }
        public enum NumbersColor_Data
        {
            Volume,
            Time,
            CustomColor
        }
        public enum BarsColor_Data
        {
            Volume,
            Time,
        }

        public class WyckoffParams_Info {
            public bool EnableWyckoff = true;
            public Numbers_Data Numbers_Input = Numbers_Data.Both;
            public NumbersPosition_Data NumbersPosition_Input = NumbersPosition_Data.Inside;

            public NumbersBothPosition_Data NumbersBothPosition_Input = NumbersBothPosition_Data.Default;
            public NumbersColor_Data NumbersColor_Input = NumbersColor_Data.Volume;

            public BarsColor_Data BarsColor_Input = BarsColor_Data.Volume;

            public bool FillBars = true;
            public bool KeepOutline = false;
            public bool ShowOnlyLargeNumbers = false;
        }
        public WyckoffParams_Info WyckoffParams = new();

        // Coloring (Wyckoff filter/ratio)
        public enum StrengthFilter_Data
        {
            MA,
            Standard_Deviation,
            Both,
            Normalized_Emphasized,
            L1Norm
        }
        public enum StrengthRatio_Data
        {
            Fixed,
            Percentile,
        }

        public class ColoringParams_Info {
            public StrengthFilter_Data StrengthFilter_Input = StrengthFilter_Data.MA;
            public StrengthRatio_Data StrengthRatio_Input = StrengthRatio_Data.Percentile;
            public MovingAverageType MAtype = MovingAverageType.Exponential;
            public int MAperiod = 5;
            public int Pctile_Period = 20;
            public int NormalizePeriod = 5;
            public int NormalizeMultiplier = 10;
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

            // Normalized_Emphasized ratio
            public double Lowest_PctValue = 23.6;
            public double Low_PctValue = 38.2;
            public double Average_PctValue = 61.8;
            public double High_PctValue = 100;
            public double Ultra_PctValue = 101;
        }
        public ColoringParams_Info ColoringParams = new();


        // Weis Waves
        public enum ShowWaves_Data
        {
            No,
            Both,
            Volume,
            EffortvsResult
        }
        public enum ShowOtherWaves_Data
        {
            No,
            Both,
            Price,
            Time
        }
        public enum ShowMarks_Data
        {
            No,
            Both,
            Left,
            Right
        }
        public enum WavesMode_Data
        {
            Reversal,
            ZigZag,
        }
        public enum YellowZigZag_Data
        {
            UsePrev_SameWave,
            UsePrev_InvertWave,
            UseCurrent
        }

        public class WavesParams_Info {
            public WavesMode_Data WavesMode_Input = WavesMode_Data.ZigZag;
            public bool ShowCurrentWave = true;
            public bool YellowRenko_IgnoreRanging = false;

            public ShowWaves_Data ShowWaves_Input = ShowWaves_Data.EffortvsResult;
            public ShowOtherWaves_Data ShowOtherWaves_Input = ShowOtherWaves_Data.Both;
            public ShowMarks_Data ShowMarks_Input = ShowMarks_Data.No;
            public YellowZigZag_Data YellowZigZag_Input = YellowZigZag_Data.UseCurrent;

            public double EvsR_Ratio = 1.5;
            public double WW_Ratio = 1.7;
        }
        public WavesParams_Info WavesParams = new();

        // ZigZag
        public enum ZigZagMode_Data
        {
            ATR,
            Percentage,
            Pips,
            NoLag_HighLow
        }
        public enum Priority_Data {
            None,
            Auto,
            Skip
        }
        public enum ZigZagSource_Data {
            Current,
            MultiTF
        }

        public class ZigZagParams_Info {
            public ZigZagMode_Data ZigZagMode_Input = ZigZagMode_Data.NoLag_HighLow;
            public Priority_Data Priority_Input = Priority_Data.None;
            public bool ShowTurningPoint = false;

            public double PercentageZZ = 0.01;
            public double PipsZZ = 0.1;

            public ZigZagSource_Data ZigZagSource_Input = ZigZagSource_Data.Current;
            public TimeFrame MTFSource_TimeFrame = TimeFrame.Minute30;
            public MTF_Sources MTFSource_Panel = MTF_Sources.Standard;
        }
        public ZigZagParams_Info ZigZagParams = new();


        // ==== Weis Wave & Wyckoff System ====        
        public readonly string NOTIFY_CAPTION = "Weis & Wyckoff System \n    v2.0";

        private IndicatorDataSeries TimeSeries;
        private IndicatorDataSeries StrengthSeries_Volume;
        private IndicatorDataSeries StrengthSeries_Time;
        private MovingAverage MATime, MAVol;
        private StandardDeviation stdDev_Time, stdDev_Vol;

        double[] prevWave_Up = { 0, 0 };
        double[] prevWave_Down = { 0, 0 };
        // Volume/Cumulative (Renko or Price) = EvsR
        double[] prevWaves_EvsR = { 0, 0, 0, 0 };
        // onlyVolume = Large WW
        double[] prevWaves_Volume = { 0, 0, 0, 0 };
        
        public class BooleanUtils_Info {
            public bool isRenkoChart = false;
            public bool isTickChart = false;
            public bool isPriceBased_Chart = false;
            public bool isPriceBased_NewBar = false;
        }
        public BooleanUtils_Info BooleanUtils = new();

        private bool isLargeWave_EvsR = false;
        private bool lockMTFNotify = false;
        private ChartTrendLine PrevWave_TrendLine;


        // Zig Zag
        public enum Direction
        {
            UP,
            DOWN
        }

        public class ZigZagObjs_Info {
            public Direction direction = Direction.DOWN;
            public double extremumPrice = 0.0;
            public int extremumIndex = 0;
        }
        public ZigZagObjs_Info ZigZagObjs = new();

        private int trendStartIndex = 0;
        private Bars MTFSource_Bars;
        private Bars _m1Bars;
        private AverageTrueRange _ATR;

        [Output("ZigZag", LineColor = "DeepSkyBlue", Thickness = 2, PlotType = PlotType.Line)]
        public IndicatorDataSeries ZigZagBuffer { get; set; }


        // High-Performance VP_Tick()
        private class PerfTickIndex {
            public int lastIdx_Bars = 0;
            public int lastIdx_Wicks = 0;

            public void ResetAll() {
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

        private IndicatorDataSeries VolumeSeries;
        private Bars TicksOHLC;

        // Timer
        private class TimerHandler {
            public bool isAsyncLoading = false;
        }
        private readonly TimerHandler timerHandler = new();
        

        // ==== Renko Wicks ====
        private Color UpWickColor;
        private Color DownWickColor;


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
        public MAType_Data customMAtype = MAType_Data.Triangular;

        private enum SourceSwitch {
            Volume,
            Time
        }
        private class SourceBuffer {
            public Dictionary<int, double> Volume = new();
            public Dictionary<int, double> Time = new();

            public Dictionary<int, double> MAVolume = new();
            public Dictionary<int, double> MATime = new();

            public void ClearAll()
            {
                Dictionary<int, double>[] _all = new[] {
                    Volume, Time, MAVolume, MATime
                };

                foreach (var dict in _all)
                    dict.Clear();
            }
        }
        private readonly SourceBuffer _customBuffer = new();


        // Performance Drawing
        // Disable X2, Y2 and IconType "never assigned" warning
        #pragma warning disable CS0649
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
        

        // Params Panel
        private Border ParamBorder;

        public class IndicatorParams
        {
            public Template_Data Template { get; set; }
            public WyckoffParams_Info WyckoffParams { get; set; }
            public ColoringParams_Info ColoringParams { get; set; }
            public WavesParams_Info WavesParams { get; set; }
            public ZigZagParams_Info ZigZagParams { get; set; }
        }

    }

}
