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
    public class WeisWyckoffSystemV20 : Indicator
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

        private double _expCumulVolume;
        private double _expCumulPrice;
        private double _expCumulVolPrice;
        private string _expWaveDirection = "None";

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
        private void AddHiddenButton(Panel panel, Color btnColor)
        {
            Button button = new()
            {
                Text = "WWS",
                Padding = 0,
                Height = 22,
                Width = 40, // Fix MacOS => stretching button when StackPanel is used.
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

                Print("Starting Weis Wave & Wyckoff Export...");
                ClearAndRecalculate();
                Print("Weis Wave & Wyckoff Export Finished.");

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

            string currentTimeframe = Chart.TimeFrame.ToString();
            BooleanUtils.isRenkoChart = currentTimeframe.Contains("Renko");
            BooleanUtils.isTickChart = currentTimeframe.Contains("Tick");
            BooleanUtils.isPriceBased_Chart = currentTimeframe.Contains("Renko") || currentTimeframe.Contains("Range") || currentTimeframe.Contains("Tick");
            if (BooleanUtils.isPriceBased_Chart) {
                Bars.BarOpened += (_) => BooleanUtils.isPriceBased_NewBar = true;
            }
            // Performance Drawing
            Chart.ZoomChanged += PerformanceDrawing;
            Chart.ScrollChanged += PerformanceDrawing;
            Bars.BarOpened += LiveDrawing;

            // Predefined Config
            Design_Templates();
            SpecificChart_Templates();
            DrawingConflict();

            // VolumeRenkoRange / Renko Wicks
            TicksOHLC = MarketData.GetBars(TimeFrame.Tick);

            if (!UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart || BooleanUtils.isPriceBased_Chart) {
                if (LoadTickStrategy_Input != LoadTickStrategy_Data.At_Startup_Sync)
                {
                    if (LoadTickStrategy_Input == LoadTickStrategy_Data.On_ChartStart_Sync) {
                        StackPanel panel = new() {
                            Width = 200,
                            Orientation = Orientation.Vertical,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        TickObjs.syncProgressBar = new ProgressBar { IsIndeterminate = true, Height = 12 };
                        panel.AddChild(TickObjs.syncProgressBar);
                        Chart.AddControl(panel);
                    } else
                        Timer.Start(TimeSpan.FromSeconds(0.5));

                    VolumeInitialize(true);
                }
                else
                    VolumeInitialize();
            }

            // Renko Wicks
            UpWickColor = Chart.ColorSettings.BullOutlineColor;
            DownWickColor = Chart.ColorSettings.BearOutlineColor;

            // WyckoffAnalysis()
            VolumeSeries = CreateDataSeries();
            TimeSeries = CreateDataSeries();
            StrengthSeries_Volume = CreateDataSeries();
            StrengthSeries_Time = CreateDataSeries();

            if (!UseCustomMAs) {
                MATime = Indicators.MovingAverage(TimeSeries, ColoringParams.MAperiod, ColoringParams.MAtype);
                MAVol = Indicators.MovingAverage(VolumeSeries, ColoringParams.MAperiod, ColoringParams.MAtype);
                stdDev_Vol = Indicators.StandardDeviation(VolumeSeries, ColoringParams.MAperiod, ColoringParams.MAtype);
                stdDev_Time = Indicators.StandardDeviation(TimeSeries, ColoringParams.MAperiod, ColoringParams.MAtype);
            }

            // WeisWaveAnalysis()
            _ATR = Indicators.AverageTrueRange(ATR_Period, MovingAverageType.Weighted);
            _m1Bars = MarketData.GetBars(TimeFrame.Minute);
            MTFSource_Bars = MarketData.GetBars(ZigZagParams.MTFSource_TimeFrame);

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
                Template = Template_Input,
                WyckoffParams = WyckoffParams,
                ColoringParams = ColoringParams,
                WavesParams = WavesParams,
                ZigZagParams = ZigZagParams,
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

            var stackPanel = new StackPanel
            {
                VerticalAlignment = vAlign,
                HorizontalAlignment = hAlign,
            };
            AddHiddenButton(stackPanel, Color.FromHex("#7F808080"));
            AddExportButton(stackPanel, Color.FromHex("#7F808080"));
            Chart.AddControl(stackPanel);
        }

        public override void Calculate(int index)
        {
            if (!UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart || BooleanUtils.isPriceBased_Chart) {
                // Tick Data Collection on chart
                bool isOnChart = LoadTickStrategy_Input != LoadTickStrategy_Data.At_Startup_Sync;
                if (isOnChart && !TickObjs.isLoadingComplete)
                    LoadMoreTicksOnChart();

                bool isOnChartAsync = LoadTickStrategy_Input == LoadTickStrategy_Data.On_ChartEnd_Async;
                if (isOnChartAsync && !TickObjs.isLoadingComplete)
                    return;

                if (index < Bars.OpenTimes.GetIndexByTime(TicksOHLC.OpenTimes.FirstOrDefault())) {
                    Chart.SetBarColor(index, HeatmapLowest_Color);
                    return;
                }
            }

            // Removing Messages
            if (!IsLastBar)
                DrawOnScreen("");

            // VolumeRR
            if (UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart)
                VolumeSeries[index] = Bars.TickVolumes[index];
            else
                VolumeSeries[index] = Get_Volume_or_Wicks(index, true)[2];

            // ==== Wyckoff ====
            if (WyckoffParams.EnableWyckoff)
                WyckoffAnalysis(index);

            // ==== Weis Wave ====
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

            // ==== Renko Wicks ====
            if (ShowWicks && BooleanUtils.isRenkoChart)
                RenkoWicks(index);

            if (ExportHistory)
            {
                ExportCsvData(index);
            }
            else if (IsLastBar)
            {
                SendSocketData(index);
            }
        }

        public void SendSocketData(int index)
        {
            double vol = double.IsNaN(VolumeSeries[index]) ? 0 : VolumeSeries[index];
            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, vol);

                string jsonString = JsonSerializer.Serialize(exportData) + "\n";
                byte[] data = Encoding.UTF8.GetBytes(jsonString);
                _networkStream.Write(data, 0, data.Length);
            }
            catch { }
        }

        private Dictionary<string, object> BuildExportPayload(int index, double vol)
        {
            double time = double.IsNaN(TimeSeries[index]) ? 0 : TimeSeries[index];
            double zigzag = double.IsNaN(ZigZagBuffer[index]) ? 0 : ZigZagBuffer[index];

            return new Dictionary<string, object>
            {
                ["symbol"] = Symbol.Name,
                ["timeframe"] = Chart.TimeFrame.ShortName,
                ["timestamp"] = Bars.OpenTimes[index].ToString("o"),
                ["open"] = Bars.OpenPrices[index],
                ["high"] = Bars.HighPrices[index],
                ["low"] = Bars.LowPrices[index],
                ["close"] = Bars.ClosePrices[index],
                ["wyckoffVolume"] = vol,
                ["wyckoffTime"] = time,
                ["zigZag"] = zigzag,
                ["waveVolume"] = _expCumulVolume,
                ["wavePrice"] = _expCumulPrice,
                ["waveVolPrice"] = _expCumulVolPrice,
                ["waveDirection"] = _expWaveDirection,
                ["spread"] = Symbol.Spread
            };
        }

        private void AppendDirectCsv(Dictionary<string, object> exportData)
        {
            if (!DirectCsvExport || !_isManualCsvExportInProgress)
                return;

            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder.Trim();
            Directory.CreateDirectory(outputFolder);

            string filePath = Path.Combine(outputFolder, "history_wyckoff.csv");
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

        private void ExportCsvData(int index)
        {
            double vol = double.IsNaN(VolumeSeries[index]) ? 0 : VolumeSeries[index];

            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, vol);
                AppendDirectCsv(exportData);
            }
            catch (Exception ex)
            {
                Print("CSV Export Error: " + ex.Message);
            }
        }

        private void Design_Templates() {
            switch (Template_Input)
            {
                case Template_Data.Insider:
                    WyckoffParams.Numbers_Input = Numbers_Data.Both;
                    WyckoffParams.NumbersColor_Input = NumbersColor_Data.Volume;
                    WyckoffParams.NumbersPosition_Input = NumbersPosition_Data.Inside;
                    WyckoffParams.BarsColor_Input = BarsColor_Data.Volume;

                    WavesParams.ShowWaves_Input = ShowWaves_Data.EffortvsResult;
                    WavesParams.ShowOtherWaves_Input = ShowOtherWaves_Data.Both;
                    WavesParams.ShowMarks_Input = ShowMarks_Data.No;

                    WyckoffParams.EnableWyckoff = true;
                    WavesParams.ShowCurrentWave = true;
                    WyckoffParams.FillBars = true;
                    WyckoffParams.KeepOutline = false;
                    Chart.ChartType = ChartType.Candlesticks;
                    break;
                case Template_Data.Volume:
                    WyckoffParams.Numbers_Input = Numbers_Data.Volume;
                    WyckoffParams.NumbersColor_Input = NumbersColor_Data.Volume;
                    WyckoffParams.NumbersPosition_Input = NumbersPosition_Data.Inside;
                    WyckoffParams.BarsColor_Input = BarsColor_Data.Volume;

                    WavesParams.ShowWaves_Input = ShowWaves_Data.Volume;
                    WavesParams.ShowOtherWaves_Input = ShowOtherWaves_Data.Price;
                    WavesParams.ShowMarks_Input = ShowMarks_Data.No;

                    WyckoffParams.EnableWyckoff = true;
                    WavesParams.ShowCurrentWave = true;
                    WyckoffParams.FillBars = true;
                    WyckoffParams.KeepOutline = false;
                    Chart.ChartType = ChartType.Candlesticks;
                    break;
                case Template_Data.Time:
                    WyckoffParams.Numbers_Input = Numbers_Data.Time;
                    WyckoffParams.NumbersColor_Input = NumbersColor_Data.Time;
                    WyckoffParams.NumbersPosition_Input = NumbersPosition_Data.Inside;
                    WyckoffParams.BarsColor_Input = BarsColor_Data.Time;

                    WavesParams.ShowWaves_Input = ShowWaves_Data.EffortvsResult;
                    WavesParams.ShowOtherWaves_Input = ShowOtherWaves_Data.Time;
                    WavesParams.ShowMarks_Input = ShowMarks_Data.No;

                    WyckoffParams.EnableWyckoff = true;
                    WavesParams.ShowCurrentWave = true;
                    WyckoffParams.FillBars = true;
                    WyckoffParams.KeepOutline = false;
                    Chart.ChartType = ChartType.Candlesticks;
                    break;
                case Template_Data.BigBrain:
                    WyckoffParams.Numbers_Input = Numbers_Data.Both;
                    WyckoffParams.NumbersColor_Input = NumbersColor_Data.Volume;
                    WyckoffParams.NumbersPosition_Input = NumbersPosition_Data.Inside;
                    WyckoffParams.BarsColor_Input = BarsColor_Data.Time;

                    WavesParams.ShowWaves_Input = ShowWaves_Data.Both;
                    WavesParams.ShowOtherWaves_Input = ShowOtherWaves_Data.Both;
                    WavesParams.ShowMarks_Input = ShowMarks_Data.Both;

                    WyckoffParams.EnableWyckoff = true;
                    WavesParams.ShowCurrentWave = true;
                    WyckoffParams.FillBars = true;
                    WyckoffParams.KeepOutline = false;
                    Chart.ChartType = ChartType.Hlc;
                    break;
                default: break;
            }
        }
        private void SpecificChart_Templates(bool isInit = true) {
            if (Template_Input == Template_Data.Custom)
                return;
            // Tick / Time-Based Chart (Standard Candles/Heikin-Ash)
            if (BooleanUtils.isTickChart || !BooleanUtils.isPriceBased_Chart) {
                if (BooleanUtils.isTickChart) {
                    if (isInit) WyckoffParams.Numbers_Input = Numbers_Data.Time;
                    ZigZagParams.MTFSource_TimeFrame = TimeFrame.Tick100;
                    ZigZagParams.MTFSource_Panel = MTF_Sources.Tick;
                } else {
                    if (isInit) WyckoffParams.Numbers_Input = Numbers_Data.Volume;
                    ZigZagParams.MTFSource_TimeFrame = TimeFrame.Minute30;
                    ZigZagParams.MTFSource_Panel = MTF_Sources.Standard;
                }
            }
            // Range
            if (BooleanUtils.isPriceBased_Chart && !BooleanUtils.isRenkoChart && !BooleanUtils.isTickChart) {
                if (isInit) WyckoffParams.Numbers_Input = Numbers_Data.Volume;
                ColoringParams.StrengthFilter_Input = StrengthFilter_Data.MA;
                ColoringParams.MAperiod = 20;
                ColoringParams.MAtype = MovingAverageType.Triangular;
                ColoringParams.Lowest_FixedValue = 0.5;
                ColoringParams.Low_FixedValue = 1.2;
                ColoringParams.Average_FixedValue = 2.5;
                ColoringParams.High_FixedValue = 3.5;
                ColoringParams.Ultra_FixedValue = 3.51;

                ZigZagParams.MTFSource_TimeFrame = TimeFrame.Range10;
                ZigZagParams.MTFSource_Panel = MTF_Sources.Range;
            }
            if (BooleanUtils.isRenkoChart) {
                ZigZagParams.MTFSource_TimeFrame = TimeFrame.Renko5;
                ZigZagParams.MTFSource_Panel = MTF_Sources.Renko;
            }
        }
        private void DrawingConflict() {
            if (WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside)
            {
                /* It's a combination that won't be used,
                   and btw drawing this combination is a little tiring,
                   I left it out. */
                if (WyckoffParams.Numbers_Input == Numbers_Data.Both && (WavesParams.ShowWaves_Input == ShowWaves_Data.Both || WavesParams.ShowWaves_Input == ShowWaves_Data.Volume || WavesParams.ShowWaves_Input == ShowWaves_Data.EffortvsResult))
                {
                    Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        "WAVES POSITIONS are not optimized for BOTH/OUTSIDE NUMBERS, setting to VOLUME/OUTSIDE NUMBERS instead",
                        PopupNotificationState.Error
                    );
                    WyckoffParams.Numbers_Input = Numbers_Data.Volume;
                }
                else if (WyckoffParams.Numbers_Input == Numbers_Data.Both && (WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Both || WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price || WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Time))
                {
                    Notifications.ShowPopup(
                        NOTIFY_CAPTION,
                        "WAVES POSITIONS are not optimized for BOTH/OUTSIDE NUMBERS, setting to VOLUME/OUTSIDE NUMBERS instead",
                        PopupNotificationState.Error
                    );
                    WyckoffParams.Numbers_Input = Numbers_Data.Volume;
                }
            }
        }
        
        private void WyckoffAnalysis(int index)
        {
            if (index < 2)
                return;

            // ==== Time Filter ====
            DateTime openTime = Bars.OpenTimes[index];
            DateTime closeTime = Bars.OpenTimes[index + 1];
            if (IsLastBar)
                closeTime = UseTimeBasedVolume && !BooleanUtils.isPriceBased_Chart ? Server.Time : TicksOHLC.OpenTimes.LastValue;
            TimeSpan interval = closeTime.Subtract(openTime);
            double interval_ms = interval.TotalMilliseconds;

            // Dynamic TimeLapse Format
            string[] interval_timelapse = GetTimeLapse(interval_ms);
            double timelapse_Value = Convert.ToDouble(interval_timelapse[0]);
            string timelapse_Suffix = interval_timelapse[1];

            TimeSeries[index] = timelapse_Value;

            // ==== Strength Filter ====
            double volume = VolumeSeries[index];
            double time = TimeSeries[index];
            double volumeStrength = 0;
            double timeStrength = 0;
            switch (ColoringParams.StrengthFilter_Input)
            {
                case StrengthFilter_Data.MA: {
                    double maValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume) : MAVol.Result[index];
                    volumeStrength = volume / maValue;
                    // ========
                    maValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time) : MATime.Result[index];
                    timeStrength = time / maValue;
                    break;
                }
                case StrengthFilter_Data.Standard_Deviation: {
                    double  stddevValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume, true) : stdDev_Vol.Result[index];
                    volumeStrength = volume / stddevValue;
                    // ========
                    stddevValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time, true) : stdDev_Time.Result[index];
                    timeStrength = time / stddevValue;
                    break;
                }
                case StrengthFilter_Data.Both: {
                    double maValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume) : MAVol.Result[index];
                    double stddevValue = UseCustomMAs ? CustomMAs(volume, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Volume, true) : stdDev_Vol.Result[index];
                    volumeStrength = (volume - maValue) / stddevValue;
                    // ========
                    maValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time) : MATime.Result[index];
                    stddevValue = UseCustomMAs ? CustomMAs(time, index, ColoringParams.MAperiod, customMAtype, SourceSwitch.Time, true) : stdDev_Time.Result[index];
                    timeStrength = (time - maValue) / stddevValue;
                    break;
                }
                case StrengthFilter_Data.Normalized_Emphasized:
                    double Normalization(bool isTime = false) {
                        /*
                        ==== References for Normalized_Emphasized ====
                        (Normalized Volume Oscillator 2008/2014) (https://www.mql5.com/en/code/8208)
                        // (The key idea for normalized volume by average volume period)
                        (Volumes Emphasized.mq4) (???)
                        // (improvement of above indicator)

                        It seems to be... the most suitable filter approach for Time-Based Charts, without Candle Spread Analysis.
                        Since CFD's Volume can be very flat at higher Tick activity,
                        - the slightest value change will be highlighted... as in ODF_Ticks/AGG.
                        */
                        if (index < ColoringParams.NormalizePeriod)
                            return 0;

                        double avg = 0;
                        for (int j = index; j > index - ColoringParams.NormalizePeriod; j--) {
                            if (isTime)
                                avg += TimeSeries[j];
                            else
                                avg += VolumeSeries[j];
                        }

                        avg /= ColoringParams.NormalizePeriod;

                        double normalizedValue = isTime ? (time / avg) : (volume / avg);
                        double normalizedPercentage = (normalizedValue * 100) - 100;
                        normalizedPercentage *= ColoringParams.NormalizeMultiplier; // I've added this to get "less but meaningful" coloring

                        return normalizedPercentage;
                    }
                    volumeStrength = Normalization();
                    timeStrength = Normalization(true);
                    break;
                case StrengthFilter_Data.L1Norm:
                    double[] windowVolume = new double[ColoringParams.MAperiod];
                    double[] windowTime = new double[ColoringParams.MAperiod];

                    for (int i = 0; i < ColoringParams.MAperiod; i++) {
                        windowVolume[i] = VolumeSeries[index - ColoringParams.MAperiod + 1 + i];
                        windowTime[i] = TimeSeries[index - ColoringParams.MAperiod + 1 + i];
                    }

                    volumeStrength = Filters.L1NormStrength(windowVolume);
                    timeStrength = Filters.L1NormStrength(windowTime);
                    break;
            }

            // Keep negative values of Normalized_Emphasized
            if (ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized) {
                volumeStrength = Math.Abs(volumeStrength);
                timeStrength = Math.Abs(timeStrength);
            }

            volumeStrength = Math.Round(volumeStrength, 2);
            timeStrength = Math.Round(timeStrength, 2);

            if (ColoringParams.StrengthRatio_Input == StrengthRatio_Data.Percentile &&
                ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized)
            {
                StrengthSeries_Volume[index] = volumeStrength;
                StrengthSeries_Time[index] = timeStrength;

                double[] windowVolume = new double[ColoringParams.Pctile_Period];
                double[] windowTime = new double[ColoringParams.Pctile_Period];

                for (int i = 0; i < ColoringParams.Pctile_Period; i++) {
                    windowVolume[i] = StrengthSeries_Volume[index - ColoringParams.Pctile_Period + 1 + i];
                    windowTime[i] = StrengthSeries_Time[index - ColoringParams.Pctile_Period + 1 + i];
                }

                volumeStrength = Filters.RollingPercentile(windowVolume);
                volumeStrength = Math.Round(volumeStrength, 1);
                // ========
                timeStrength = Filters.RollingPercentile(windowTime);
                timeStrength = Math.Round(timeStrength, 1);
            }

            // ==== Drawing ====
            // Y-Axis
            bool isBullish = Bars.ClosePrices[index] > Bars.OpenPrices[index];
            double y_Close = Bars.ClosePrices[index];
            double y_Open = Bars.OpenPrices[index];

            // Coloring
            double colorTypeNumbers = WyckoffParams.NumbersColor_Input == NumbersColor_Data.Time ? timeStrength : volumeStrength;
            double colorTypeBars = WyckoffParams.BarsColor_Input== BarsColor_Data.Time ? timeStrength : volumeStrength;
            bool isNumbersOutside = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside || WyckoffParams.Numbers_Input == Numbers_Data.None;

            int alpha = (int)(2.55 * HeatmapBars_Opacity);
            Color lowestColor = isNumbersOutside ? HeatmapLowest_Color : Color.FromArgb(alpha, HeatmapLowest_Color);
            Color lowColor = isNumbersOutside ? HeatmapLow_Color : Color.FromArgb(alpha, HeatmapLow_Color);
            Color averageColor = isNumbersOutside ? HeatmapAverage_Color : Color.FromArgb(alpha, HeatmapAverage_Color);

            Color highColorUp = isNumbersOutside ? HeatmapHighUp_Color : Color.FromArgb(alpha, HeatmapHighUp_Color);
            Color highColorDown = isNumbersOutside ? HeatmapHighDown_Color : Color.FromArgb(alpha, HeatmapHighDown_Color);
            Color highColor = isBullish ? highColorUp : highColorDown;

            Color ultraColorUp = isNumbersOutside ? HeatmapUltraUp_Color : Color.FromArgb(alpha, HeatmapUltraUp_Color);
            Color ultraColorDown = isNumbersOutside ? HeatmapUltraDown_Color : Color.FromArgb(alpha, HeatmapUltraDown_Color);
            Color ultraColor = isBullish ? ultraColorUp : ultraColorDown;

            if (ColoringParams.StrengthFilter_Input == StrengthFilter_Data.Normalized_Emphasized) {
                // if negative, just to be sure.
                colorTypeBars = colorTypeBars < 0 ? 0 : colorTypeBars;
                colorTypeNumbers = colorTypeNumbers < 0 ? 0 : colorTypeNumbers;
            }

            // Ratio
            bool isFixed = ColoringParams.StrengthRatio_Input == StrengthRatio_Data.Fixed;

            double lowest = isFixed ? ColoringParams.Lowest_FixedValue : ColoringParams.Lowest_PctileValue;
            double low = isFixed ? ColoringParams.Low_FixedValue : ColoringParams.Low_PctileValue;
            double average = isFixed ? ColoringParams.Average_FixedValue : ColoringParams.Average_PctileValue;
            double high = isFixed ? ColoringParams.High_FixedValue : ColoringParams.High_PctileValue;
            double ultra = isFixed ? ColoringParams.Ultra_FixedValue : ColoringParams.Ultra_PctileValue;

            if (ColoringParams.StrengthFilter_Input == StrengthFilter_Data.Normalized_Emphasized) {
                lowest = ColoringParams.Lowest_PctValue;
                low = ColoringParams.Low_PctValue;
                average = ColoringParams.Average_PctValue;
                high = ColoringParams.High_PctValue;
                ultra = ColoringParams.Ultra_PctValue;
            }

            Color barColor = colorTypeBars < lowest ? lowestColor :
                             colorTypeBars < low ? lowColor :
                             colorTypeBars < average ? averageColor :
                             colorTypeBars < high ? highColor :
                             colorTypeBars >= ultra ? ultraColor : lowestColor;

            highColor = isBullish ? HeatmapHighUp_Color : HeatmapHighDown_Color;
            ultraColor = isBullish ? HeatmapUltraUp_Color : HeatmapUltraDown_Color;
            Color numberColor = colorTypeNumbers < lowest ? HeatmapLowest_Color :
                                colorTypeNumbers < low ? HeatmapLow_Color :
                                colorTypeNumbers < average ? HeatmapAverage_Color :
                                colorTypeNumbers < high ? highColor :
                                colorTypeNumbers >= ultra ? ultraColor : HeatmapLowest_Color;

            // Numbers
            timelapse_Value = Math.Round(timelapse_Value);
            string onlyTime = WyckoffParams.ShowOnlyLargeNumbers ?
                              (timeStrength > low ? timelapse_Value + timelapse_Suffix : "") :
                              timelapse_Value + timelapse_Suffix;

            string onlyVol = WyckoffParams.ShowOnlyLargeNumbers ?
                             (volumeStrength > low ? FormatBigNumber(volume) : "") :
                             FormatBigNumber(volume);

            string bothVolTime = WyckoffParams.NumbersBothPosition_Input == NumbersBothPosition_Data.Default ?
                                 $"{onlyTime}\n{onlyVol}" : $"{onlyVol}\n{onlyTime}";

            string numbersFmtd = WyckoffParams.Numbers_Input switch {
                Numbers_Data.Time => onlyTime,
                Numbers_Data.Volume => onlyVol,
                Numbers_Data.Both => bothVolTime,
                _ => ""
            };
            
            if (numbersFmtd != "")
            {
                double y1 = isBullish ? y_Close : y_Open;
                VerticalAlignment v_align = VerticalAlignment.Bottom;
                HorizontalAlignment h_align;

                if (Chart.ChartType != ChartType.Bars && Chart.ChartType != ChartType.Hlc)
                {
                    if (WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside && isBullish)
                        v_align = VerticalAlignment.Top;

                    if (!isBullish) {
                        v_align = VerticalAlignment.Bottom;
                        if (WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside)
                            y1 = y_Close;
                    }

                    h_align = HorizontalAlignment.Center;
                }
                else {
                    h_align = HorizontalAlignment.Stretch;
                    if (!isBullish) {
                        v_align = VerticalAlignment.Top;
                        y1 = y_Close;
                    }
                }

                DrawOrCache(new DrawInfo
                {
                    BarIndex = index,
                    Type = DrawType.Text,
                    Id = $"{index}_wyckoff",
                    Text = numbersFmtd,
                    X1 = Bars[index].OpenTime,
                    Y1 = y1,
                    horizontalAlignment = h_align,
                    verticalAlignment = v_align,
                    FontSize = FontSizeNumbers,
                    Color = WyckoffParams.NumbersColor_Input == NumbersColor_Data.CustomColor ? CustomNumbersColor : numberColor
                });
            }

            // Fill + Outline Settings
            if (!WyckoffParams.FillBars && !WyckoffParams.KeepOutline) {
                Chart.SetBarFillColor(index, Color.Transparent);
                Chart.SetBarOutlineColor(index, barColor);
                if (isBullish) UpWickColor = barColor;
                else DownWickColor = barColor;
            }
            else if (WyckoffParams.FillBars && WyckoffParams.KeepOutline) {
                Chart.SetBarFillColor(index, barColor);
                if (isBullish) UpWickColor = Chart.ColorSettings.BullOutlineColor;
                else DownWickColor = Chart.ColorSettings.BullOutlineColor;
            }
            else if (!WyckoffParams.FillBars && WyckoffParams.KeepOutline) {
                Chart.SetBarFillColor(index, Color.Transparent);
                if (isBullish) UpWickColor = Chart.ColorSettings.BullOutlineColor;
                else DownWickColor = Chart.ColorSettings.BullOutlineColor;
            }
            else {
                Chart.SetBarColor(index, barColor);
                if (isBullish) UpWickColor = barColor;
                else DownWickColor = barColor;
            }

            if (ColoringParams.ShowStrengthValue) {
                if (WyckoffParams.Numbers_Input == Numbers_Data.Volume || WyckoffParams.Numbers_Input == Numbers_Data.Both) {
                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = index,
                        Type = DrawType.Text,
                        Id = $"{index}_strengthVol",
                        Text = $"{volumeStrength}v",
                        X1 = Bars[index].OpenTime,
                        Y1 = isBullish ? Bars[index].High : Bars[index].Low,
                        horizontalAlignment = HorizontalAlignment.Center,
                        verticalAlignment = isBullish ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                        FontSize = FontSizeNumbers,
                        Color = CustomNumbersColor
                    });
                }
                if (WyckoffParams.Numbers_Input == Numbers_Data.Time || WyckoffParams.Numbers_Input == Numbers_Data.Both) {
                    DrawOrCache(new DrawInfo
                    {
                        BarIndex = index,
                        Type = DrawType.Text,
                        Id = $"{index}_strengthTime",
                        Text = $"{timeStrength}ts",
                        X1 = Bars[index].OpenTime,
                        Y1 = isBullish ? Bars[index].Low : Bars[index].High,
                        horizontalAlignment = HorizontalAlignment.Center,
                        verticalAlignment = isBullish ? VerticalAlignment.Bottom : VerticalAlignment.Top,
                        FontSize = FontSizeNumbers,
                        Color = CustomNumbersColor
                    });
                }

                DrawOnScreen("v => volume \n ts => time");
            }

        }

        private double CustomMAs(double seriesValue, int index, int maPeriod, MAType_Data maType, SourceSwitch sourceSwitch, bool isStdDev = false) 
        {
            Dictionary<int, double> buffer = sourceSwitch switch {
                SourceSwitch.Time => _customBuffer.Time,
                _ => _customBuffer.Volume
            };
            Dictionary<int, double> prevMA_Dict = sourceSwitch switch {
                SourceSwitch.Time => _customBuffer.MATime,
                _ => _customBuffer.MAVolume
            };

            if (!buffer.ContainsKey(index))
                buffer.Add(index, seriesValue);
            else
                buffer[index] = seriesValue;

            double maValue = maType switch
            {
                MAType_Data.Simple => CustomMA.SMA(index, maPeriod, buffer),
                MAType_Data.Exponential => CustomMA.EMA(index, maPeriod, buffer, prevMA_Dict),
                MAType_Data.Weighted => CustomMA.WMA(index, maPeriod, buffer),
                MAType_Data.Triangular => CustomMA.TMA(index, maPeriod, buffer),
                MAType_Data.Hull => CustomMA.Hull(index, maPeriod, buffer),
                MAType_Data.VIDYA => CustomMA.VIDYA(index, maPeriod, buffer, prevMA_Dict),
                MAType_Data.WilderSmoothing => CustomMA.Wilder(index, maPeriod, buffer, prevMA_Dict),
                MAType_Data.KaufmanAdaptive => CustomMA.KAMA(index, maPeriod, 2, 30, buffer, prevMA_Dict),
                _ => double.NaN
            };

            return isStdDev ? CustomMA.StdDev(index, maPeriod, maValue, buffer) : maValue;
        }

        private string FormatBigNumber(double num)
        {
            if (double.IsNaN(num) || num.ToString().Length == 1)
                return num.ToString();

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

        private static string[] GetTimeLapse(double interval_ms)
        {
            // Dynamic TimeLapse Format
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

        // *********** VOLUME RENKO/RANGE ***********
        /*
            Original source code by srlcarlg (me) (https://ctrader.com/algos/indicators/show/3045)
            Uses Ticks Data to make the calculation of volume, just like Candles.

            Refactored in Order Flow Ticks v2.0 revision 1.5
            Improved in Order Flow Aggregated v2.0
        */
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
                    Timer.Stop();
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

        // *********** PERFORMANCE DRAWING ***********
        /*
            An simple idea that came up during the development of ODF_AGG.
            LLM code generating was used to quickly test the idea concepts.

            - Re-draw => Objects are deleted and recreated each time,
                - Fastest approach
                - Removes only objects outside the visible chart range
                - when cleaning up the chart with Chart.RemoveAllObjects()
                    it takes only 1/0.5 seconds.

            - Hidden => Objects are never deleted, just .IsHidden = True.
                - Slowest approach
                - IsHidden = false, only in visibles objects.
                - when cleaning up the chart with Chart.RemoveAllObjects()
                    it lags/freezes the chart/panel UI,
                    the waiting time scales with the drawings count.
                - Lags at scrolling at MASSIVE hidden drawings count.
        */
        private void PerformanceDrawing(object obj)
        {
            int first = Chart.FirstVisibleBarIndex;
            int last = Chart.LastVisibleBarIndex;
            int visible = 0;

            // ==== Drawing at Zoom ====
            int Zoom = Chart.ZoomLevel;
            // Keep rectangles from Filters or VPs
            if (Zoom < DrawAtZoom_Value) {
                HiddenOrRemove(true);
                return;
            }

            void HiddenOrRemove(bool hiddenAll)
            {
                if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest && hiddenAll)
                {
                    foreach (var kvp in PerfDrawingObjs.hiddenInfos)
                    {
                        string drawName = kvp.Key;
                        ChartObject drawObj = kvp.Value;

                        // Extract index from name
                        string[] parts = drawName.Split('_');
                        if (parts.Length < 2) continue;
                        if (!int.TryParse(parts.FirstOrDefault(), out _)) continue;

                        drawObj.IsHidden = hiddenAll;
                    }
                }
                else if (DrawingStrategy_Input == DrawingStrategy_Data.Redraw_Fastest && hiddenAll) {
                    // Remove everything
                    foreach (var kvp in PerfDrawingObjs.redrawInfos.Values)
                    {
                        var drawInfoList = kvp.Values;
                        foreach (DrawInfo drawInfo in drawInfoList)
                            Chart.RemoveObject(drawInfo.Id);
                    }
                }

                DebugPerfDraw();
            }

            // ==== Drawing at scroll ====
            if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest) {
                // Display the hidden ones
                foreach (var kvp in PerfDrawingObjs.hiddenInfos)
                {
                    string drawName = kvp.Key;
                    ChartObject drawObj = kvp.Value;

                    // Extract index from name
                    string[] parts = drawName.Split('_');
                    if (parts.Length < 2) continue;
                    if (!int.TryParse(parts.FirstOrDefault(), out int idx)) continue;

                    bool isVis = idx >= first && idx <= last;
                    drawObj.IsHidden = !isVis;

                    if (ShowDrawingInfo) {
                        if (isVis) visible++;
                    }
                }
            }
            else {
                // Clean up
                foreach (var kvp in PerfDrawingObjs.redrawInfos)
                {
                    var drawInfoList = kvp.Value.Values;
                    foreach (DrawInfo drawInfo in drawInfoList)
                    {
                        // The actual lazy cleanup.
                        if (kvp.Key < first || kvp.Key > last)
                            Chart.RemoveObject(drawInfo.Id);
                    }
                }

                // Draw visible
                for (int i = first; i <= last; i++)
                {
                    if (!PerfDrawingObjs.redrawInfos.ContainsKey(i))
                        continue;

                    var drawInfoList = PerfDrawingObjs.redrawInfos[i].Values;
                    foreach (DrawInfo info in drawInfoList)
                    {
                        CreateDraw(info);
                        if (ShowDrawingInfo)
                            visible++;
                    }
                }
            }

            DebugPerfDraw();

            void DebugPerfDraw() {
                if (ShowDrawingInfo) {
                    PerfDrawingObjs.staticText_DebugPerfDraw ??= Chart.DrawStaticText("Debug_Perf_Draw", "", VerticalAlignment.Top, HorizontalAlignment.Left, Color.Lime);
                    bool IsHidden = DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest;
                    int cached = 0;
                    if (!IsHidden) {
                        foreach (var list in PerfDrawingObjs.redrawInfos.Values) {
                            cached += list.Count;
                        }
                    }
                    PerfDrawingObjs.staticText_DebugPerfDraw.Text = IsHidden ?
                        $"Hidden Mode\n Total Objects: {FormatBigNumber(PerfDrawingObjs.hiddenInfos.Values.Count)}\n Visible: {FormatBigNumber(visible)}" :
                        $"Redraw Mode\n Cached: {FormatBigNumber(PerfDrawingObjs.redrawInfos.Count)} bars\n Cached: {FormatBigNumber(cached)} objects\n Drawn: {FormatBigNumber(visible)}";
                }
            }
        }
        private ChartObject CreateDraw(DrawInfo info)
        {
            switch (info.Type)
            {
                case DrawType.Text:
                    ChartText text = Chart.DrawText(info.Id, info.Text, info.X1, info.Y1, info.Color);
                    text.HorizontalAlignment = info.horizontalAlignment;
                    text.VerticalAlignment = info.verticalAlignment;
                    text.FontSize = info.FontSize;
                    return text;
                case DrawType.Icon:
                    return Chart.DrawIcon(info.Id, info.IconType, info.X1, info.Y1, info.Color);

                case DrawType.Ellipse:
                    ChartEllipse ellipse = Chart.DrawEllipse(info.Id, info.X1, info.Y1, info.X2, info.Y2, info.Color);
                    ellipse.IsFilled = true;
                    return ellipse;

                case DrawType.Rectangle:
                    ChartRectangle rectangle = Chart.DrawRectangle(info.Id, info.X1, info.Y1, info.X2, info.Y2, info.Color);
                    return rectangle;

                default:
                    return null;
            }
        }
        private void DrawOrCache(DrawInfo info) {
            if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest)
            {
                if (!IsLastBar || BooleanUtils.isPriceBased_NewBar) {
                    ChartObject obj = CreateDraw(info);
                    obj.IsHidden = true;
                    PerfDrawingObjs.hiddenInfos[info.Id] = obj;
                } else {
                    ChartObject obj = CreateDraw(info);
                    // Replace current obj
                    if (!PerfDrawingObjs.currentToHidden.ContainsKey(0))
                        PerfDrawingObjs.currentToHidden[0] = new Dictionary<string, ChartObject>();
                    else
                        PerfDrawingObjs.currentToHidden[0][info.Id] = obj;
                }
            }
            else
            {
                // Add Keys if not present
                if (!PerfDrawingObjs.redrawInfos.ContainsKey(info.BarIndex)) {
                    PerfDrawingObjs.redrawInfos[info.BarIndex] = new Dictionary<string, DrawInfo> { { info.Id, info } };
                }
                else {
                    // Add/Replace drawing
                    if (!IsLastBar || BooleanUtils.isPriceBased_NewBar)
                        PerfDrawingObjs.redrawInfos[info.BarIndex][info.Id] = info;
                    else 
                    {
                        // Fix PerfDrawing => "Weis Waves => "Show Current Wave?" often flickering in live-market (Redraw_Fastest only)
                        if (WavesParams.ShowCurrentWave) 
                        {
                            string[] toReplace = {
                                $"{trendStartIndex}_WavesMisc",
                                $"{trendStartIndex}_WavesVolume",
                                $"{trendStartIndex}_WavesEvsR"
                            };
                            if (toReplace.Contains(info.Id)) {
                                int lastKey = info.BarIndex - 1;
                                foreach (string id in toReplace)
                                {
                                    if (PerfDrawingObjs.redrawInfos[lastKey].ContainsKey(id)) {
                                        if (info.Id == id)
                                            PerfDrawingObjs.redrawInfos[lastKey][id] = info;
                                    }
                                }
                            }
                        }
                        
                        // Create drawing and replace current infos
                        CreateDraw(info);
                        if (!PerfDrawingObjs.currentToRedraw.ContainsKey(0))
                            PerfDrawingObjs.currentToRedraw[0] = new Dictionary<string, DrawInfo>();
                        else
                            PerfDrawingObjs.currentToRedraw[0][info.Id] = info;
                    }
                }
            }

            // IMPORTANT! => set isPriceBased_NewBar to 'false' after using it
            BooleanUtils.isPriceBased_NewBar = false;
        }
        private void LiveDrawing(BarOpenedEventArgs obj) {
            // Working with Lists in Calculate() is painful.
            if (DrawingStrategy_Input == DrawingStrategy_Data.Hidden_Slowest) {
                List<ChartObject> objList = PerfDrawingObjs.currentToHidden[0].Values.ToList();

                foreach (var drawObj in objList)
                    PerfDrawingObjs.hiddenInfos[drawObj.Name] = drawObj;

                PerfDrawingObjs.currentToHidden.Clear();
            }
            else {
                List<DrawInfo> drawList = PerfDrawingObjs.currentToRedraw[0].Values.ToList();
                foreach (DrawInfo info in drawList) {
                    // Fix PerfDrawing => Wyckoff Bars => Numbers in live-market are always changed to '2'
                    PerfDrawingObjs.redrawInfos[info.BarIndex][info.Id] = info;
                    // previous  => The redrawInfos[drawList.FirstOrDefault().BarIndex] 
                }

                PerfDrawingObjs.currentToRedraw.Clear();
            }
        }

        // ************************ WEIS WAVE SYSTEM **************************
        /*
                                   Improved Weis Waves
                                           by
                                        srlcarlg

                          ====== References for Studies ======
        (Numbers-Renko 数字練行足) by akutsusho (https://www.tradingview.com/script/9BKOIhdl-Numbers-Renko) (Code concepts in PineScript)
        (ZigZag) by mike.ourednik (https://ctrader.com/algos/indicators/show/1419) (decreased a lot of code, base for any ZigZag)
        (Swing Gann) by TradeExperto (https://ctrader.com/algos/indicators/show/2521) (helped to make the structure of waves calculation)

        =========================================

        NEW IN Revision 1 (after ODF_AGG):
        - Instead of using the ZigZag, the DirectionChanged() method was doing the heavy job...
            - In order to use WWSystem on [Ticks, Range and time-based charts], the proper use of zigzag is needed.
        - Add [ATR, Pips] to Standard ZigZag.
        - Add simple Multi-Timeframe Price lookup.

                        ==== References for NoLag-HighLow ZigZag ===
        (Absolute ZigZag - 2024/2025) (https://tradingview.com/script/lRY74dha-Absolute-ZigZag-Lib/)
        // (The key idea for high/low bars analysis)
        (Professional ZigZag - 2011/2016) https://www.mql5.com/en/code/263
        // (The idea of High/Low order formation by looking at lower timeframes, seems to be the first one)

        I needed to simplify the High/Low Bars analysis because I wanted to keep the current ZigZag structure,
        which is quite optimized and easy to understand.
        Compared to "Absolute ZigZag" logic, I did:
            - Remove [High or Low] Priority, keep the Auto (lower timeframe order formation) for Time-Based charts only.
            - Add [Skip or None] Priority for "bars that have both a higher high and a higher low"
        */

        private void WeisWaveAnalysis(int rawIndex)
        {
            int index = rawIndex - 1;

            if (index < 2)
                return;

            if (WavesParams.WavesMode_Input == WavesMode_Data.Reversal && BooleanUtils.isRenkoChart) {
                if (IsLastBar) // IsLastBar=false at each new BarOpened
                    return;
                bool isUp = Bars.ClosePrices[index] > Bars.OpenPrices[index];

                if (WavesParams.ShowCurrentWave)
                    CalculateWaves(isUp ? Direction.UP : Direction.DOWN, trendStartIndex, index, false);

                if (ShowTrendLines) {
                    ChartTrendLine trendLine = Chart.DrawTrendLine($"TrendLine_{trendStartIndex}",
                                   trendStartIndex, Bars.OpenPrices[trendStartIndex],
                                   index, Bars.OpenPrices[index], isUp ? UpLineColor : DownLineColor);
                    trendLine.Thickness = TrendThickness;
                }

                if (!Reversal_DirectionChanged(index))
                    return;

                CalculateWaves(isUp ? Direction.UP : Direction.DOWN, trendStartIndex, index, true);

                if (ShowTrendLines) {
                    ChartTrendLine trendLine = Chart.DrawTrendLine($"TrendLine_NO{index}",
                                               index, Bars.OpenPrices[index],
                                               index + 1, Bars.OpenPrices[index], NoTrendColor);
                    trendLine.Thickness = TrendThickness;
                }

                trendStartIndex = index + 1;
            }
            else
                ZigZag(index);
        }
        private bool Reversal_DirectionChanged(int index)
        {
            bool isUp = Bars.ClosePrices[index] > Bars.OpenPrices[index];

            bool prevIsUp = Bars.ClosePrices[index - 1] > Bars.OpenPrices[index - 1];
            bool nextIsUp = Bars.ClosePrices[index + 1] > Bars.OpenPrices[index + 1];
            bool prevIsDown = Bars.ClosePrices[index - 1] < Bars.OpenPrices[index - 1];
            bool nextIsDown = Bars.ClosePrices[index + 1] < Bars.OpenPrices[index + 1];

            return prevIsUp && isUp && nextIsDown || prevIsDown && isUp && nextIsDown ||
                   prevIsDown && !isUp && nextIsUp || prevIsUp && !isUp && nextIsUp;
        }

        private bool ZigZag_DirectionChanged(int index, double low, double high, double prevLow, double prevHigh)
        {
            switch (ZigZagParams.ZigZagMode_Input)
            {
                case ZigZagMode_Data.Percentage:
                    if (ZigZagObjs.direction == Direction.DOWN)
                        return high >= ZigZagObjs.extremumPrice * (1.0 + ZigZagParams.PercentageZZ * 0.01);
                    else
                        return low <= ZigZagObjs.extremumPrice * (1.0 - ZigZagParams.PercentageZZ * 0.01);
                case ZigZagMode_Data.NoLag_HighLow:
                    bool bothIsPivot = high > prevHigh && low < prevLow;
                    bool highIsPivot = high > prevHigh && low >= prevLow;
                    bool lowIsPivot = low < prevLow && high <= prevHigh;
                    if (bothIsPivot)
                        return false;
                    return ZigZagObjs.direction == Direction.UP ? lowIsPivot : highIsPivot;
                default:
                    bool isATR = ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.ATR;
                    double value = isATR ? (_ATR.Result[index] * ATR_Multiplier) : (ZigZagParams.PipsZZ * Symbol.PipSize);
                    if (ZigZagObjs.direction == Direction.DOWN)
                        return Math.Abs(ZigZagObjs.extremumPrice - high) >= value;
                    else
                        return Math.Abs(low - ZigZagObjs.extremumPrice) >= value;
            }
        }
        private void ZigZag(int index) {
            double prevHigh = Bars.HighPrices[index - 1];
            double prevLow = Bars.LowPrices[index - 1];
            double high = Bars.HighPrices[index];
            double low = Bars.LowPrices[index];
            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                DateTime prevBarDate = Bars.OpenTimes[index - 1];
                DateTime barDate = Bars.OpenTimes[index];

                int TF_PrevIdx = MTFSource_Bars.OpenTimes.GetIndexByTime(prevBarDate);
                int TF_idx = MTFSource_Bars.OpenTimes.GetIndexByTime(barDate);

                prevHigh = MTFSource_Bars.HighPrices[TF_PrevIdx];
                prevLow = MTFSource_Bars.LowPrices[TF_PrevIdx];
                high = MTFSource_Bars.HighPrices[TF_idx];
                low = MTFSource_Bars.LowPrices[TF_idx];
            }

            if (ZigZagObjs.extremumPrice == 0) {
                ZigZagObjs.extremumPrice = high;
                ZigZagObjs.extremumIndex = index;
            }

            if (ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.NoLag_HighLow && ZigZagParams.Priority_Input != Priority_Data.None && !BooleanUtils.isPriceBased_Chart) {
                if (NoLag_BothIsPivot(index, low, high, prevLow, prevHigh) || ZigZagParams.Priority_Input == Priority_Data.Skip)
                    return;
            }
            bool directionChanged = ZigZag_DirectionChanged(index, low, high, prevLow, prevHigh);
            if (ZigZagObjs.direction == Direction.DOWN)
            {
                if (low <= ZigZagObjs.extremumPrice)
                    MoveExtremum(index, low);
                else if (directionChanged) {
                    SetExtremum(index, high, false);
                    ZigZagObjs.direction = Direction.UP;
                }
            }
            else
            {
                if (high >= ZigZagObjs.extremumPrice)
                    MoveExtremum(index, high);
                else if (directionChanged) {
                    SetExtremum(index, low, false);
                    ZigZagObjs.direction = Direction.DOWN;
                }
            }
        }
        private void MoveExtremum(int index, double price)
        {
            if (!ShowTrendLines)
                ZigZagBuffer[ZigZagObjs.extremumIndex] = double.NaN;
            SetExtremum(index, price, true);
        }
        private void SetExtremum(int index, double price, bool isMove)
        {
            if (!isMove) {
                // End of direction
                CalculateWaves(ZigZagObjs.direction, trendStartIndex, ZigZagObjs.extremumIndex, true);
                trendStartIndex = ZigZagObjs.extremumIndex + 1;

                DateTime extremeDate = Bars[ZigZagObjs.extremumIndex].OpenTime;
                double extremePrice = ZigZagObjs.direction == Direction.UP ? Bars[ZigZagObjs.extremumIndex].High : Bars[ZigZagObjs.extremumIndex].Low;
                if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                    int TF_idx = MTFSource_Bars.OpenTimes.GetIndexByTime(extremeDate);
                    extremePrice = ZigZagObjs.direction == Direction.UP ? MTFSource_Bars[TF_idx].High : MTFSource_Bars[TF_idx].Low;
                }
                if (ZigZagParams.ShowTurningPoint) {
                    Color turningColor = InvertTurningColor ?
                                        (ZigZagObjs.direction == Direction.UP ? DownLineColor : UpLineColor) :
                                        (ZigZagObjs.direction == Direction.UP ? UpLineColor : DownLineColor);

                    Chart.DrawTrendLine($"{ZigZagObjs.extremumIndex}_horizontal",
                                        extremeDate,
                                        extremePrice,
                                        Bars[index].OpenTime,
                                        extremePrice, turningColor);
                    Chart.DrawTrendLine($"{ZigZagObjs.extremumIndex}_vertical",
                                        Bars[index].OpenTime,
                                        extremePrice,
                                        Bars[index].OpenTime,
                                        ZigZagObjs.direction == Direction.UP ? Bars[index].High : Bars[index].Low, turningColor);
                }

                if (ShowTrendLines) {
                    PrevWave_TrendLine.LineStyle = LineStyle.Solid;
                    if (isLargeWave_EvsR && ShowYellowTrendLines)
                        PrevWave_TrendLine.Color = LargeWaveColor;

                    Color lineColor = ColorfulTrendLines ?
                                      (ZigZagObjs.direction == Direction.UP ? DownLineColor : UpLineColor) :
                                      NoTrendColor;
                    double trendEndPrice = ZigZagObjs.direction == Direction.UP ? Bars[index].Low : Bars[index].High;
                    PrevWave_TrendLine = Chart.DrawTrendLine($"TrendLine_{trendStartIndex}",
                                                            extremeDate,
                                                            extremePrice,
                                                            Bars[index].OpenTime,
                                                            trendEndPrice, lineColor);
                    PrevWave_TrendLine.Thickness = TrendThickness;
                }
            }
            else if (isMove && WavesParams.ShowCurrentWave)
                CalculateWaves(ZigZagObjs.direction, trendStartIndex, ZigZagObjs.extremumIndex, false);

            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF && isMove) {
                // Workaround to remove the behavior of shift(nº) when moving the extremum at custom timeframe price source
                double extremePrice = ZigZagObjs.direction == Direction.UP ? Bars[ZigZagObjs.extremumIndex].High : Bars[ZigZagObjs.extremumIndex].Low;
                double currentPrice = ZigZagObjs.direction == Direction.UP ? Bars[index].High : Bars[index].Low;
                bool condition = ZigZagObjs.direction == Direction.UP ? currentPrice <= extremePrice : currentPrice >= extremePrice;
                ZigZagObjs.extremumIndex = condition ? ZigZagObjs.extremumIndex : index;
            }
            else
                ZigZagObjs.extremumIndex = index;

            ZigZagObjs.extremumPrice = price;

            if (!ShowTrendLines)
                ZigZagBuffer[ZigZagObjs.extremumIndex] = ZigZagObjs.extremumPrice;

            if (isMove)
                MovingTrendLine(Bars[ZigZagObjs.extremumIndex].OpenTime, price);
        }

        private void MovingTrendLine(DateTime endDate, double endPrice)
        {
            if (ShowTrendLines)
            {
                int startIndex = trendStartIndex - 1;
                // Yeah... index jumps are quite annoying to debug.
                try { _ = Bars[startIndex].OpenTime; } catch { startIndex = trendStartIndex; }

                DateTime startDate = Bars[startIndex].OpenTime;
                double startPrice = ZigZagObjs.direction == Direction.UP ? Bars[startIndex].Low : Bars[startIndex].High;

                Color lineColor = ColorfulTrendLines ? (ZigZagObjs.direction == Direction.UP ? DownLineColor : UpLineColor) : NoTrendColor;
                PrevWave_TrendLine = Chart.DrawTrendLine($"TrendLine_{trendStartIndex}",
                                     startDate,
                                     startPrice,
                                     endDate,
                                     endPrice, lineColor);
                PrevWave_TrendLine.Thickness = TrendThickness;
                PrevWave_TrendLine.LineStyle = LineStyle.Dots;
            }
        }
        private bool NoLag_BothIsPivot(int  index, double low, double high, double prevLow, double prevHigh) {
            bool bothIsPivot = high > prevHigh && low < prevLow;
            if (!bothIsPivot || ZigZagParams.Priority_Input != Priority_Data.Auto)
                return false;

            bool HighIsFirst = AutoPriority(index, prevLow, prevHigh, low, high);
            if (HighIsFirst) {
                // Chart.DrawText($"{index}_First", "First(High)", Bars[index].OpenTime, high, Color.White);
                // Chart.DrawText($"{index}_Last", "Last(Low)", Bars[index].OpenTime, low, Color.White);
                // Chart.DrawText($"{index}_DIRECTION", direction.ToString(), Bars[index].OpenTime, Bars.OpenPrices[index], Color.White);
                if (ZigZagObjs.direction == Direction.UP)
                {
                    // Fix => C# version was using ZigZagBuffer['extremumIndex'] instead of ZigZagBuffer['index'],
                    if (high > ZigZagObjs.extremumPrice && !ShowTrendLines)
                        ZigZagBuffer[index] = high;

                    SetExtremum(index, low, true);
                    ZigZagObjs.direction = Direction.DOWN;
                }
            }
            else {
                // Chart.DrawText($"{index}_First", "First(Low)", Bars[index].OpenTime, low, Color.White);
                // Chart.DrawText($"{index}_Last", "Last(High)", Bars[index].OpenTime, high, Color.White);
                // Chart.DrawText($"{index}_DIRECTION", direction.ToString(), Bars[index].OpenTime, Bars.OpenPrices[index], Color.White);
                if (ZigZagObjs.direction == Direction.DOWN)
                {
                    if (low < ZigZagObjs.extremumPrice && !ShowTrendLines)
                        ZigZagBuffer[index] = low;

                    SetExtremum(index, high, true);
                    ZigZagObjs.direction = Direction.UP;
                }
            }

            return true;
        }

        private bool AutoPriority(int index, double prevLow, double prevHigh, double low, double high)
        {
            DateTime barStart = Bars.OpenTimes[index];
            DateTime barEnd = Bars.OpenTimes[index + 1];
            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                int TF_idxStart = MTFSource_Bars.OpenTimes.GetIndexByTime(barStart);
                int TF_idxEnd = MTFSource_Bars.OpenTimes.GetIndexByTime(barEnd);

                barStart = MTFSource_Bars.OpenTimes[TF_idxStart];
                barEnd = MTFSource_Bars.OpenTimes[TF_idxEnd];
            }
            if (IsLastBar)
                barEnd = _m1Bars.LastBar.OpenTime;

            bool firstIsHigh = false;
            bool atLeastOne = false;

            int startM1 = _m1Bars.OpenTimes.GetIndexByTime(barStart);
            for (int i = startM1; i < _m1Bars.OpenTimes.Count; i++)
            {
                if (_m1Bars.OpenTimes[i] > barEnd)
                    break;

                if (_m1Bars.HighPrices[i] > prevHigh) {
                    firstIsHigh = true;
                    atLeastOne = true;
                    break;
                }
                // Fix => C# version sets True for first_is_high
                if (_m1Bars.LowPrices[i] < prevLow) {
                    firstIsHigh = false;
                    atLeastOne = true;
                    break;
                }
            }

            if (!atLeastOne) {
                double subtHigh = Math.Abs(high - prevHigh);
                double subtLow = Math.Abs(prevLow - low);
                return subtHigh >= subtLow;
            }

            return firstIsHigh;
        }

        private double GetY1_Waves(int extremeIndex) {
            if (WavesParams.WavesMode_Input == WavesMode_Data.Reversal && BooleanUtils.isRenkoChart)
                return Bars.ClosePrices[extremeIndex];

            DateTime extremeDate = Bars[extremeIndex].OpenTime;
            double extremePrice = ZigZagObjs.direction == Direction.UP ? Bars[extremeIndex].High : Bars[extremeIndex].Low;
            if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF) {
                int TF_idx = MTFSource_Bars.OpenTimes.GetIndexByTime(extremeDate);
                extremePrice = ZigZagObjs.direction == Direction.UP ? MTFSource_Bars[TF_idx].High : MTFSource_Bars[TF_idx].Low;
            }
            return extremePrice;
        }

        private void CalculateWaves(Direction direction, int firstCandleIdx, int lastCandleIdx, bool directionChanged = false)
        {
            double cumulVolume()
            {
                double volume = 0.0;
                for (int i = firstCandleIdx; i <= lastCandleIdx; i++)
                    volume += VolumeSeries[i];

                return volume;
            }
            double cumulRenko()
            {
                double renkoCount = 0;
                for (int i = firstCandleIdx; i <= lastCandleIdx; i++)
                    renkoCount += 1;

                return renkoCount;
            }
            double cumulativePrice(bool isUp)
            {
                double price;
                if (isUp)
                    price = Bars.HighPrices[lastCandleIdx] - Bars.LowPrices[firstCandleIdx];
                else
                    price = Bars.HighPrices[firstCandleIdx] - Bars.LowPrices[lastCandleIdx];

                if (ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF && WavesParams.WavesMode_Input == WavesMode_Data.ZigZag) {
                    int TF_idxLast = MTFSource_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[lastCandleIdx]);
                    int TF_idxFirst = MTFSource_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[firstCandleIdx]);
                    if (isUp)
                        price = MTFSource_Bars.HighPrices[TF_idxLast] - MTFSource_Bars.LowPrices[TF_idxFirst];
                    else
                        price = MTFSource_Bars.HighPrices[TF_idxFirst] - MTFSource_Bars.LowPrices[TF_idxLast];
                }
                price /= Symbol.PipSize;

                return Math.Round(price, 2);
            }
            double cumulativeTime()
            {
                DateTime openTime = Bars.OpenTimes[firstCandleIdx];
                DateTime closeTime = Bars.OpenTimes[lastCandleIdx + 1];
                TimeSpan interval = closeTime.Subtract(openTime);
                double interval_ms = interval.TotalMilliseconds;
                return interval_ms;
            }
            bool directionIsUp = direction == Direction.UP;
            if (WavesParams.ShowWaves_Input == ShowWaves_Data.No)
            {
                // Other Waves
                if (!WavesParams.ShowCurrentWave && directionChanged || WavesParams.ShowCurrentWave)
                    OthersWaves(directionIsUp);
                return;
            }

            double cumlVolume = cumulVolume();
            double cumlRenkoOrPrice = BooleanUtils.isRenkoChart ? cumulRenko() : cumulativePrice(directionIsUp);
            double cumlVolPrice = Math.Round(cumlVolume / cumlRenkoOrPrice, 1);

            _expCumulVolume = cumlVolume;
            _expCumulPrice = cumlRenkoOrPrice;
            _expCumulVolPrice = cumlVolPrice;
            _expWaveDirection = directionIsUp ? "Up" : "Down";

            // Standard Waves
            if (!WavesParams.ShowCurrentWave && directionChanged || WavesParams.ShowCurrentWave) {
                EvsR_Analysis(cumlVolPrice, directionChanged, directionIsUp);
                WW_Analysis(cumlVolume, directionChanged, directionIsUp);
            }
            // Other Waves
            if (!WavesParams.ShowCurrentWave && directionChanged || WavesParams.ShowCurrentWave)
                OthersWaves(directionIsUp);

            // Prev Waves Analysis
            if (directionIsUp) {
                bool prevIsDown = Bars.ClosePrices[lastCandleIdx - 1] < Bars.OpenPrices[lastCandleIdx - 1];
                bool nextIsDown = Bars.ClosePrices[lastCandleIdx + 1] < Bars.OpenPrices[lastCandleIdx + 1];
                // Set Previous Bullish Wave Accumulated
                SetPrevWaves(cumlVolume, cumlVolPrice, prevIsDown, nextIsDown, true, directionChanged);
            } else {
                bool prevIsUp = Bars.ClosePrices[lastCandleIdx - 1] > Bars.OpenPrices[lastCandleIdx - 1];
                bool nextIsUp = Bars.ClosePrices[lastCandleIdx + 1] > Bars.OpenPrices[lastCandleIdx + 1];
                // Set Previous Downish Wave Accumulated
                SetPrevWaves(cumlVolume, cumlVolPrice, prevIsUp, nextIsUp, false, directionChanged);
            }

            void OthersWaves(bool isUp)
            {
                if (WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.No)
                    return;

                double cumulPrice = cumulativePrice(isUp);
                string cumulPriceFmtd = cumulPrice > 1000 ? FormatBigNumber(cumulPrice) : cumulPrice.ToString();
                double cumlTime = cumulativeTime();

                if (cumlTime == 0 || double.IsNaN(cumlTime))
                    return;

                string[] interval_timelapse = GetTimeLapse(cumlTime);

                ShowWaves_Data selectedWave = WavesParams.ShowWaves_Input;
                double timelapse_Value = Convert.ToDouble(interval_timelapse[0]);
                string timelapseString = Math.Round(timelapse_Value) + interval_timelapse[1];

                string waveInfo;
                if (isUp)
                {
                    string spacingUp = WyckoffParams.NumbersPosition_Input switch {
                        NumbersPosition_Data.Outside => selectedWave switch {
                            ShowWaves_Data.No => "\n\n",
                            ShowWaves_Data.Both => "\n\n\n\n",
                            _ => "\n\n\n"
                        },
                        _ => selectedWave switch {
                            ShowWaves_Data.No => "",
                            ShowWaves_Data.Both => "\n\n\n",
                            _ => "\n\n"
                        },
                    };
                    string sourceWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? cumulPriceFmtd : timelapseString;
                    string suffixWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? "p" : "";
                    
                    waveInfo = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Both ?
                                $"{timelapseString} ⎪ {cumulPriceFmtd}p{spacingUp}" :
                                $"{sourceWave}{suffixWave}{spacingUp}";
                }
                else
                {
                    string spacingDown = WyckoffParams.NumbersPosition_Input switch {
                        NumbersPosition_Data.Outside => selectedWave switch {
                            ShowWaves_Data.No => "\n",
                            ShowWaves_Data.Both => "\n\n\n",
                            _ => "\n\n"
                        },
                        _ => selectedWave switch {
                            ShowWaves_Data.No => "",
                            ShowWaves_Data.Both => "\n\n",
                            _ => "\n"
                        },
                    };

                    string sourceWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? cumulPriceFmtd : timelapseString;
                    string suffixWave = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Price ? "p" : "";

                    waveInfo = WavesParams.ShowOtherWaves_Input == ShowOtherWaves_Data.Both ?
                                $"{spacingDown}{timelapseString} ⎪ {cumulPriceFmtd}p" :
                                $"{spacingDown}{sourceWave}{suffixWave}";
                }

                double y1 = GetY1_Waves(lastCandleIdx);
                DrawOrCache(new DrawInfo
                {
                    BarIndex = lastCandleIdx,
                    Type = DrawType.Text,
                    Id = $"{firstCandleIdx}_WavesMisc",
                    Text = waveInfo,
                    X1 = Bars.OpenTimes[lastCandleIdx],
                    Y1 = y1,
                    horizontalAlignment = HorizontalAlignment.Center,
                    verticalAlignment = isUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    FontSize = FontSizeWaves,
                    Color = isUp ? UpWaveColor : DownWaveColor
                });
            }

            void WW_Analysis(double cumlVolume, bool endWave, bool isUp)
            {
                if (WavesParams.ShowWaves_Input == ShowWaves_Data.No || WavesParams.ShowWaves_Input == ShowWaves_Data.EffortvsResult)
                    return;
                string leftMark = "";
                string rightMark = "";
                string volFmtd = FormatBigNumber(cumlVolume);

                string waveInfo;
                if (isUp)
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolume > prevWave_Up[0] ? "⮝" : "⮟"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolume > prevWave_Down[0] ? "🡩" : "🡫"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolume > prevWave_Up[0] ? "⮝" : "⮟";
                            rightMark = cumlVolume > prevWave_Down[0] ? "" : leftMark == "⮟" ? "" : "🡫";
                            break;
                        default: break;
                    }
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                     "\n\n" :
                                     "";
                    
                    waveInfo = $"({leftMark}{volFmtd}{rightMark}){spacing}";
                }
                else
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolume > prevWave_Down[0] ? "⮟" : "⮝"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolume > prevWave_Up[0] ? "🡫" : "🡩"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolume > prevWave_Down[0] ? "⮟" : "⮝";
                            rightMark = cumlVolume > prevWave_Up[0] ? "" : leftMark == "⮝" ? "" : "🡩";
                            break;
                        default: break;
                    }
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                     "\n" :
                                     "";
                    waveInfo = $"{spacing}({leftMark}{volFmtd}{rightMark})";
                }

                double y1 = GetY1_Waves(lastCandleIdx);
                bool largeVol = endWave && Volume_Large();
                Color waveColor = largeVol ? LargeWaveColor : (isUp ? UpWaveColor : DownWaveColor);

                if (ShowRatioValue) {
                    double ratio = (cumlVolume + prevWaves_Volume[0] + prevWaves_Volume[1] + prevWaves_Volume[2] + prevWaves_Volume[3]) / 5 * WavesParams.WW_Ratio;
                    ratio = Math.Round(ratio, 2);
                    waveInfo = $"{waveInfo} > {ratio}? {cumlVolume > ratio} ";
                }

                DrawOrCache(new DrawInfo
                {
                    BarIndex = lastCandleIdx,
                    Type = DrawType.Text,
                    Id = $"{firstCandleIdx}_WavesVolume",
                    Text = waveInfo,
                    X1 = Bars.OpenTimes[lastCandleIdx],
                    Y1 = y1,
                    horizontalAlignment = HorizontalAlignment.Center,
                    verticalAlignment = isUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    FontSize = FontSizeWaves,
                    Color = waveColor
                });

                bool Volume_Large()
                {
                    bool haveZero = false;
                    foreach (double value in prevWaves_Volume)
                    {
                        if (value == 0) {
                            haveZero = true;
                            break;
                        }
                    }
                    if (haveZero)
                        return false;

                    return (cumlVolume + prevWaves_Volume[0] + prevWaves_Volume[1] + prevWaves_Volume[2] + prevWaves_Volume[3]) / 5 * WavesParams.WW_Ratio < cumlVolume;
                }
            }

            void EvsR_Analysis(double cumlVolPrice, bool endWave, bool isUp)
            {
                if (WavesParams.ShowWaves_Input == ShowWaves_Data.No || WavesParams.ShowWaves_Input == ShowWaves_Data.Volume)
                    return;

                string leftMark = "";
                string rightMark = "";
                string effortFmtd = FormatBigNumber(cumlVolPrice);
                
                string waveInfo;
                if (isUp)
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolPrice > prevWave_Up[1] ? "⮝" : "⮟"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolPrice > prevWave_Down[1] ? "🡩" : "🡫"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolPrice > prevWave_Up[1] ? "⮝" : "⮟";
                            rightMark = cumlVolPrice > prevWave_Down[1] ? "" : leftMark == "⮟" ? "" : "🡫";
                            break;
                        default: break;
                    }
                    bool isBoth =  WavesParams.ShowWaves_Input == ShowWaves_Data.Both;
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                    (isBoth ? "\n\n\n" : "\n\n") :
                                    (isBoth ? "\n\n" : "");
                    
                    waveInfo = $"[{leftMark}{effortFmtd}{rightMark}]{spacing}";
                }
                else
                {
                    switch (WavesParams.ShowMarks_Input) {
                        case ShowMarks_Data.Left:
                            leftMark = cumlVolPrice > prevWave_Down[1] ? "⮟" : "⮝"; break;
                        case ShowMarks_Data.Right:
                            rightMark = cumlVolPrice > prevWave_Up[1] ? "🡫" : "🡩"; break;
                        case ShowMarks_Data.Both:
                            leftMark = cumlVolPrice > prevWave_Down[1] ? "⮟" : "⮝";
                            rightMark = cumlVolPrice > prevWave_Up[1] ? "" : leftMark == "⮝" ? "" : "🡩";
                            break;
                        default: break;
                    }
                    
                    bool isBoth =  WavesParams.ShowWaves_Input == ShowWaves_Data.Both;
                    string spacing = WyckoffParams.NumbersPosition_Input == NumbersPosition_Data.Outside ?
                                    (isBoth ? "\n\n" : "\n") :
                                    (isBoth ? "\n" : "");
                    
                    waveInfo = $"{spacing}[{leftMark}{effortFmtd}{rightMark}]";
                }

                double y1 = GetY1_Waves(lastCandleIdx);
                bool largeEffort = endWave && EvsR_Large();
                Color waveColor = largeEffort ? LargeWaveColor : (isUp ? UpWaveColor : DownWaveColor);

                if (ShowRatioValue) {
                    double ratio = (cumlVolPrice + prevWaves_EvsR[0] + prevWaves_EvsR[1] + prevWaves_EvsR[2] + prevWaves_EvsR[3]) / 5 * WavesParams.EvsR_Ratio;
                    ratio = Math.Round(ratio, 2);
                    waveInfo = $"{waveInfo} > {ratio}? {cumlVolPrice > ratio}";
                }

                DrawOrCache(new DrawInfo
                {
                    BarIndex = lastCandleIdx,
                    Type = DrawType.Text,
                    Id = $"{firstCandleIdx}_WavesEvsR",
                    Text = waveInfo,
                    X1 = Bars.OpenTimes[lastCandleIdx],
                    Y1 = y1,
                    horizontalAlignment = HorizontalAlignment.Center,
                    verticalAlignment = isUp ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    FontSize = FontSizeWaves,
                    Color = waveColor
                });

                isLargeWave_EvsR = false;
                if (!largeEffort)
                    return;
                isLargeWave_EvsR = true;

                if (!WyckoffParams.FillBars && !WyckoffParams.KeepOutline) {
                    Chart.SetBarFillColor(lastCandleIdx, Color.Transparent);
                    Chart.SetBarOutlineColor(lastCandleIdx, LargeWaveColor);
                }
                else if (WyckoffParams.FillBars && WyckoffParams.KeepOutline)
                    Chart.SetBarFillColor(lastCandleIdx, LargeWaveColor);
                else if (!WyckoffParams.FillBars && WyckoffParams.KeepOutline)
                    Chart.SetBarFillColor(lastCandleIdx, Color.Transparent);
                else if (WyckoffParams.FillBars && !WyckoffParams.KeepOutline)
                    Chart.SetBarColor(lastCandleIdx, LargeWaveColor);

                // Large EvsR [Yellow]
                bool EvsR_Large()
                {
                    bool haveZero = false;
                    foreach (double value in prevWaves_EvsR)
                    {
                        if (value == 0) {
                            haveZero = true;
                            break;
                        }
                    }
                    if (haveZero)
                        return false;

                    return (cumlVolPrice + prevWaves_EvsR[0] + prevWaves_EvsR[1] + prevWaves_EvsR[2] + prevWaves_EvsR[3]) / 5 * WavesParams.EvsR_Ratio < cumlVolPrice;
                }
            }
        }

        private void SetPrevWaves(double cumlVolume, double cumlVolPrice, bool prevIs_UpDown, bool nextIs_UpDown, bool isUp, bool directionChanged)
        {
            // Exclude the most old wave, keep the 3 others and add current Wave value for most recent Wave
            /*
                The previous "wrongly" implementation turns out to be a good filter,
                with the correct implementation of 5 waves, it gives too many yellow bars.
                Since it's useful, keep it.
            */
            double[] cumul = { cumlVolume, cumlVolPrice };

            if (WavesParams.WavesMode_Input == WavesMode_Data.ZigZag) {
                if (!directionChanged) return;
                setTrend();
                return;
            }

            bool conditionRanging = prevIs_UpDown && directionChanged && nextIs_UpDown;
            bool conditionTrend = !prevIs_UpDown && directionChanged && nextIs_UpDown;

            if (isUp) {
                // (prevIsDown && DirectionChanged && nextIsDown);
                if (conditionRanging)
                    setRanging();
                // (!prevIsDown && DirectionChanged && nextIsDown);
                else if (conditionTrend)
                    setTrend();
            } else {
                // (prevIsUp && DirectionChanged && nextIsUp)
                if (conditionRanging)
                    setRanging();
                // (!prevIsUp && DirectionChanged && nextIsUp);
                else if (conditionTrend)
                    setTrend();
            }

            // Ranging or 1 renko trend pullback
            void setRanging() {
                // Volume Wave Analysis
                double[] newWave_Vol = { prevWaves_Volume[1], prevWaves_Volume[2], prevWaves_Volume[3], cumlVolume };
                prevWaves_Volume = newWave_Vol;

                // Effort vs Result Analysis
                double[] newWave_EvsR = { prevWaves_EvsR[1], prevWaves_EvsR[2], prevWaves_EvsR[3], cumlVolPrice };
                prevWaves_EvsR = newWave_EvsR;

                if (!WavesParams.YellowRenko_IgnoreRanging) {
                    if (isUp) prevWave_Up = cumul;
                    else prevWave_Down = cumul;
                }
            }
            void setTrend() {
                if (isUp) {
                    // Volume Wave Analysis
                    // Fix => C# version is using _prev_wave_down for UsePrev_SameWave condition
                    double volumeValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolume,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Up[0],
                        _ => prevWave_Down[0]
                    };
                    double[] newWave_Vol = { prevWaves_Volume[1], prevWaves_Volume[2], prevWaves_Volume[3], volumeValue };
                    prevWaves_Volume = newWave_Vol;

                    // Effort vs Result Analysis
                    double evsrValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolPrice,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Up[1],
                        _ => prevWave_Down[1]
                    };
                    double[] newWave_EvsR = { prevWaves_EvsR[1], prevWaves_EvsR[2], prevWaves_EvsR[3], evsrValue };
                    prevWaves_EvsR = newWave_EvsR;

                    // Prev Wave
                    prevWave_Up = cumul;
                }
                else {
                    // Volume Wave Analysis
                    double volumeValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolume,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Down[0],
                        _ => prevWave_Up[0]
                    };
                    double[] newWave_Vol = { prevWaves_Volume[1], prevWaves_Volume[2], prevWaves_Volume[3], volumeValue };
                    prevWaves_Volume = newWave_Vol;

                    // Effort vs Result Analysis
                    double evsrValue = WavesParams.YellowZigZag_Input switch {
                        YellowZigZag_Data.UseCurrent => cumlVolPrice,
                        YellowZigZag_Data.UsePrev_SameWave => prevWave_Down[1],
                        _ => prevWave_Up[1]
                    };
                    double[] newWave_EvsR = { prevWaves_EvsR[1], prevWaves_EvsR[2], prevWaves_EvsR[3], evsrValue };
                    prevWaves_EvsR = newWave_EvsR;

                    // Prev Wave
                    prevWave_Down = cumul;
                }
            }
        }

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

    public enum ParamInputType { Text, Checkbox, ComboBox }

    public class ParamDefinition
    {
        public string Region { get; init; }
        public int RegionOrder { get; init; }
        public string Key { get; init; }
        public string Label { get; init; }
        public ParamInputType InputType { get; init; }
        public Func<IndicatorParams, object> GetDefault { get; init; }
        public Action<string> OnChanged { get; init; }
        public Func<IEnumerable<string>> EnumOptions { get; init; } = null;
        public Func<bool> IsVisible { get; set; } = () => true;
    }
    public enum MTF_Sources {
        Standard, Tick, Renko, Range, Heikin_Ash
    }
    public enum Standard_Sources {
        m1, m2, m3, m4, m5, m6, m7, m8, m9, m10,
        m15, m30, m45, h1, h2, h3, h4, h6, h8, h12,
        D1, D2, D3, W1, Month1
    }
    public enum Tick_Sources {
        t1, t2, t3, t4, t5, t6, t7, t8, t9, t10,
        t15, t20, t25, t30, t40, t50, t60, t80, t90, t100,
        t150, t200, t250, t300, t500, t750, t1000
    }
    public enum Renko_Sources {
        Re1, Re2, Re3, Re4, Re5, Re6, Re7, Re8, Re9, Re10,
        Re15, Re20, Re25, Re30, Re35, Re40, Re45, Re50,
        Re100, Re150, Re200, Re300, Re500, Re800, Re1000, Re2000
    }
    public enum Range_Sources {
        Ra1, Ra2, Ra3, Ra4, Ra5, Ra8, Ra10,
        Ra20, Ra30, Ra50, Ra80,
        Ra100, Ra150, Ra200, Ra300, Ra500, Ra800,
        Ra1000, Ra2000, Ra5000, Ra7500, Ra10000
    }



}
