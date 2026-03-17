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
    // ================ PARAMS PANEL ================
    /*
    What I've done since bringing it from ODF Ticks, by order:
        - Add remaining Result parameters (Large Filter)
        - Chang some hard-coded Widths "// ParamsPanel"
        - Add remaining and new "Tick Spike" parameters (Spike Chart/Levels)
        - Add textInputLabelMap
        - Add remaining and new "Bubbles Chart" parameters (Ultra Bubbles Levels/Notify)
        - Increase grid rows from 4 to 6 => CreateContentPanel
            - Reorganize Tick Spike / Bubbles Chart Parametrs
        - Add Volume Profile parameters
        - Add Segments/ODF intervals parameters
            - Fix RegionOrder between Results <=> Misc
        - Add Crimson foreground-color for parameters that will eat up RAM (RefreshHighlighting())
            - Or are better to leave at default, unless Higher Timeframes(Bars >= h2) are used.
        - Add ReplaceByATR (override loaded row config by ATR in LoadParams())
        - Add IsVisible conditions to specific parameters
        - Add OnChanged to all new parameters added so far.
            - Revision of Static Update for new parameters
        - Change prefix for LocalStorage to ODFT-AGG (GetStorageKey())
        - Add Outside.UseCustomMAs condition to every "MA Type" inputs (3)
    */

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
    public enum Supported_Timeframes {
        m5, m10, m15, m30, m45, h1, h2, h3, h4, h6, h8, h12, D1, D2, D3
    }

    public class ParamsPanel : CustomControl
    {
        private readonly OrderFlowTicksV20 Outside;
        private readonly IndicatorParams FirstParams;
        private Button ModeBtn;
        private Button SaveBtn;
        private Button ApplyBtn;
        private Button RangeBtn;
        private ProgressBar _progressBar;
        private bool isLoadingParams;

        private readonly Dictionary<string, TextBox> textInputMap = new();
        private readonly Dictionary<string, TextBlock> textInputLabelMap = new();

        private readonly Dictionary<string, TextBlock> checkBoxTextMap = new();
        private readonly Dictionary<string, CheckBox> checkBoxMap = new();

        private readonly Dictionary<string, ComboBox> comboBoxMap = new();
        private readonly Dictionary<string, TextBlock> comboBoxTextMap = new();

        private readonly List<ParamDefinition> _paramDefinitions;
        private readonly Dictionary<string, RegionSection> _regionSections = new();
        private readonly Dictionary<string, object> _originalValues = new();
        private ColorTheme ApplicationTheme => Outside.Application.ColorTheme;

        // For deferred inputs
        private int _pendingIntensityNDays = 0;

        public ParamsPanel(OrderFlowTicksV20 indicator, IndicatorParams defaultParams)
        {
            Outside = indicator;
            FirstParams = defaultParams;
            _paramDefinitions = DefineParams();

            AddChild(CreateTradingPanel());

            LoadParams(); // If not present, use defaults params.
            RefreshVisibility(); // Refresh UI with the current values.
        }

        private List<ParamDefinition> DefineParams()
        {
            bool isPanel_VP() => Outside.PanelSwitch_Input != PanelSwitch_Data.Order_Flow_Ticks;
            bool isPanelOnly_VP() => Outside.PanelSwitch_Input == PanelSwitch_Data.Volume_Profile;
            bool isPanel_ODF() => Outside.PanelSwitch_Input != PanelSwitch_Data.Volume_Profile;
            
            bool isIntraday_VP() => Outside.ProfileParams.ShowIntradayProfile;
            bool isEnable_AnyVP() => Outside.ProfileParams.EnableMainVP || Outside.ProfileParams.EnableMiniProfiles ||
                                     Outside.ProfileParams.EnableWeeklyProfile || Outside.ProfileParams.EnableMonthlyProfile ||
                                     Outside.ProfileParams.EnableFixedRange;
            bool isDeltaMode() => Outside.GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta;

            bool isNodeBand() => (
                Outside.NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands ||
                Outside.NodesParams.ShowNode_Input == ShowNode_Data.LVN_With_Bands
            ) && Outside.NodesParams.ProfileNode_Input != ProfileNode_Data.Percentile;
            bool isStrongHVN() => (
                Outside.NodesParams.ShowNode_Input == ShowNode_Data.HVN_Raw ||
                Outside.NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile && Outside.NodesParams.ShowNode_Input == ShowNode_Data.HVN_With_Bands
            );
            bool isStrongLVN() => (
                Outside.NodesParams.ShowNode_Input != ShowNode_Data.HVN_Raw && Outside.NodesParams.ProfileNode_Input != ProfileNode_Data.Percentile ||
                Outside.NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile &&
                (Outside.NodesParams.ShowNode_Input == ShowNode_Data.LVN_With_Bands || Outside.NodesParams.ShowNode_Input == ShowNode_Data.LVN_Raw)
            );
            
            bool isSpikeFilter() => Outside.SpikeFilterParams.EnableSpikeFilter;
            bool isSpikePercentage() => Outside.SpikeRatioParams.SpikeRatio_Input == SpikeRatio_Data.Percentage;
            bool isSpikeFixed() => Outside.SpikeRatioParams.SpikeRatio_Input == SpikeRatio_Data.Fixed;
            bool isSpike_NoMAType() => !(Outside.SpikeFilterParams.SpikeFilter_Input == SpikeFilter_Data.L1Norm || 
                                      Outside.SpikeFilterParams.SpikeFilter_Input == SpikeFilter_Data.SoftMax_Power);
            
            bool isBubblesChart() => Outside.BubblesChartParams.EnableBubblesChart;
            bool isBubblesPercentile() => Outside.BubblesRatioParams.BubblesRatio_Input == BubblesRatio_Data.Percentile;
            bool isBubblesFixed() => Outside.BubblesRatioParams.BubblesRatio_Input == BubblesRatio_Data.Fixed;
            bool isBubblesChange() => Outside.BubblesChartParams.UseChangeSeries;
            bool isBubbles_NoMAType() => !(Outside.BubblesChartParams.BubblesFilter_Input == BubblesFilter_Data.L2Norm || 
                                      Outside.BubblesChartParams.BubblesFilter_Input == BubblesFilter_Data.SoftMax_Power ||
                                      Outside.BubblesChartParams.BubblesFilter_Input == BubblesFilter_Data.MinMax);
            
            bool isNot_NormalMode() => Outside.GeneralParams.VolumeMode_Input != VolumeMode_Data.Normal;
            bool IsNot_BubblesChart() => !Outside.BubblesChartParams.EnableBubblesChart;
            bool IsNot_SpikeChart() =>  !Outside.SpikeFilterParams.EnableSpikeChart;
            
            return new List<ParamDefinition>
            {
                new()
                {
                    Region = "General",
                    RegionOrder = 1,
                    Key = "DaysToShowKey",
                    Label = "Nº Days",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.GeneralParams.Lookback,
                    OnChanged = _ => UpdateDaysToShow()
                },
                new()
                {
                    Region = "General",
                    RegionOrder = 1,
                    Key = "RowHeightKey",
                    Label = "Row(pips)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.RowHeightInPips.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateRowHeight()
                },
                new()
                {
                    Region = "General",
                    RegionOrder = 1,
                    Key = "VolumeViewKey",
                    Label = "Volume View",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.GeneralParams.VolumeView_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(VolumeView_Data)),
                    OnChanged = _ => UpdateVolumeView(),
                    IsVisible = () => isNot_NormalMode() && IsNot_BubblesChart() && isPanel_ODF()
                },

                new()
                {
                    Region = "Coloring",
                    RegionOrder = 2,
                    Key = "LargestDividedKey",
                    Label = "Largest?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.GeneralParams.ColoringOnlyLarguest,
                    OnChanged = _ => UpdateCheckbox("LargestDividedKey", val => Outside.GeneralParams.ColoringOnlyLarguest = val, true),
                    IsVisible = () => isNot_NormalMode() && Outside.GeneralParams.VolumeView_Input == VolumeView_Data.Divided && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 2,
                    Key = "IntensityKey",
                    Label = "Intensity?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.GeneralParams.ColoringIntensity,
                    OnChanged = _ => UpdateCheckbox("IntensityKey", val => Outside.GeneralParams.ColoringIntensity = val, true),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 2,
                    Key = "IntensityModeKey",
                    Label = "Intensity Mode",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.GeneralParams.IntensityMode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(IntensityMode_Data)),
                    OnChanged = _ => UpdateIntensityMode(),
                    IsVisible = () => Outside.GeneralParams.ColoringIntensity && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 2,
                    Key = "IntensityNDaysKey",
                    Label = "Intensity(Days)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.GeneralParams.IntensityNDays_Input.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateIntensityNDays(),
                    IsVisible = () => Outside.GeneralParams.ColoringIntensity && Outside.GeneralParams.IntensityMode_Input == IntensityMode_Data.Global_N_Days && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "EnableVPKey",
                    Label = "Main VP?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableMainVP,
                    OnChanged = _ => UpdateCheckbox("EnableVPKey", val => Outside.ProfileParams.EnableMainVP = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "UpdateVPKey",
                    Label = "Update At",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.UpdateProfile_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(UpdateProfile_Data)),
                    OnChanged = _ => UpdateVP(),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "FillVPKey",
                    Label = "Fill Histogram?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.FillHist_VP,
                    OnChanged = _ => UpdateCheckbox("FillVPKey", val => Outside.ProfileParams.FillHist_VP = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "SideVPKey",
                    Label = "Side",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.HistogramSide_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(HistSide_Data)),
                    OnChanged = _ => UpdateSideVP(),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "WidthVPKey",
                    Label = "Width",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.HistogramWidth_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(HistWidth_Data)),
                    OnChanged = _ => UpdateWidthVP(),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "NumbersVPKey",
                    Label = "Historical Nºs?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.ShowHistoricalNumbers,
                    OnChanged = _ => UpdateCheckbox("NumbersVPKey", val => Outside.ProfileParams.ShowHistoricalNumbers = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "IntradayVPKey",
                    Label = "Intraday?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.ShowIntradayProfile,
                    OnChanged = _ => UpdateCheckbox("IntradayVPKey", val => Outside.ProfileParams.ShowIntradayProfile = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 5,
                    Key = "IntraOffsetKey",
                    Label = "Offset(bars)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ProfileParams.OffsetBarsInput.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateIntradayOffset(),
                    IsVisible = () => isIntraday_VP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "IntraTFKey",
                    Label = "Offset(time)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.OffsetTimeframeInput.ShortName,
                    EnumOptions = () => Enum.GetNames(typeof(Supported_Timeframes)),
                    OnChanged = _ => UpdateIntradayTimeframe(),
                    IsVisible = () => isIntraday_VP() && Outside.isPriceBased_Chart && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "MiniVPsKey",
                    Label = "Mini-VPs?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableMiniProfiles,
                    OnChanged = _ => UpdateCheckbox("MiniVPsKey", val => Outside.ProfileParams.EnableMiniProfiles = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "MiniTFKey",
                    Label = "Mini-Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.MiniVPs_Timeframe.ShortName.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Supported_Timeframes)),
                    OnChanged = _ => UpdateMiniVPTimeframe(),
                    IsVisible = () => Outside.ProfileParams.EnableMiniProfiles && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "MiniResultKey",
                    Label = "Mini-Result?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.ShowMiniResults,
                    OnChanged = _ => UpdateCheckbox("MiniResultKey", val => Outside.ProfileParams.ShowMiniResults = val),
                    IsVisible = () => Outside.ProfileParams.EnableMiniProfiles && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "FixedRangeKey",
                    Label = "Fixed Range?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableFixedRange,
                    OnChanged = _ => UpdateCheckbox("FixedRangeKey", val => Outside.ProfileParams.EnableFixedRange = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP()
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "WeeklyVPKey",
                    Label = "Weekly VP?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableWeeklyProfile,
                    OnChanged = _ => UpdateCheckbox("WeeklyVPKey", val => Outside.ProfileParams.EnableWeeklyProfile = val),
                    IsVisible = () => Outside.GeneralParams.VolumeMode_Input != VolumeMode_Data.Buy_Sell && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "MonthlyVPKey",
                    Label = "Monthly VP?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableMonthlyProfile,
                    OnChanged = _ => UpdateCheckbox("MonthlyVPKey", val => Outside.ProfileParams.EnableMonthlyProfile = val),
                    IsVisible = () => Outside.GeneralParams.VolumeMode_Input != VolumeMode_Data.Buy_Sell && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "IntraNumbersKey",
                    Label = "Intra-Nºs?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.ShowIntradayNumbers,
                    OnChanged = _ => UpdateCheckbox("IntraNumbersKey", val => Outside.ProfileParams.ShowIntradayNumbers = val),
                    IsVisible = () => isIntraday_VP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "FillIntraVPKey",
                    Label = "Intra-Space?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.FillIntradaySpace,
                    OnChanged = _ => UpdateCheckbox("FillIntraVPKey", val => Outside.ProfileParams.FillIntradaySpace = val),
                    IsVisible = () => isIntraday_VP() && (Outside.ProfileParams.EnableWeeklyProfile || Outside.ProfileParams.EnableMonthlyProfile) && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },


                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "EnableNodeKey",
                    Label = "Enable?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.EnableNodeDetection,
                    OnChanged = _ => UpdateCheckbox("EnableNodeKey", val => Outside.NodesParams.EnableNodeDetection = val),
                    IsVisible = () => isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "NodeSmoothKey",
                    Label = "Smooth",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.NodesParams.ProfileSmooth_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ProfileSmooth_Data)),
                    OnChanged = _ => UpdateNodeSmooth(),
                    IsVisible = () => isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "NodeTypeKey",
                    Label = "Nodes",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.NodesParams.ProfileNode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ProfileNode_Data)),
                    OnChanged = _ => UpdateNodeType(),
                    IsVisible = () => isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "ShowNodeKey",
                    Label = "Show",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.NodesParams.ShowNode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ShowNode_Data)),
                    OnChanged = _ => UpdateShowNode(),
                    IsVisible = () => isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "HvnBandPctKey",
                    Label = "HVN Band(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.bandHVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHVN_Band(),
                    IsVisible = () => isNodeBand() && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "LvnBandPctKey",
                    Label = "LVN Band(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.bandLVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLVN_Band(),
                    IsVisible = () => isNodeBand() && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "NodeStrongKey",
                    Label = "Only Strong?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.onlyStrongNodes,
                    OnChanged = _ => UpdateCheckbox("NodeStrongKey", val => Outside.NodesParams.onlyStrongNodes = val),
                    IsVisible = () => isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                // 'Strong HVN' for HVN_Raw(only) on [LocalMinMax, Topology]
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "StrongHvnPctKey",
                    Label = "(%) >= POC",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.strongHVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHVN_Strong(),
                    IsVisible = () => Outside.NodesParams.onlyStrongNodes && isStrongHVN() && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                // 'Strong LVN' should be used by HVN_With_Bands, since the POCs are derived from LVN Split.
                // on [LocalMinMax, Topology]
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "StrongLvnPctKey",
                    Label = "(%) <= POC",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.strongLVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLVN_Strong(),
                    IsVisible = () => Outside.NodesParams.onlyStrongNodes && isStrongLVN() && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "ExtendNodeKey",
                    Label = "Extend?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.extendNodes,
                    OnChanged = _ => UpdateCheckbox("ExtendNodeKey", val => Outside.NodesParams.extendNodes = val),
                    IsVisible = () => isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "ExtNodesCountKey",
                    Label = "Extend(count)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.extendNodes_Count.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateExtendNodesCount(),
                    IsVisible = () => Outside.NodesParams.extendNodes && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "ExtBandsKey",
                    Label = "Ext.(bands)?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.extendNodes_WithBands,
                    OnChanged = _ => UpdateCheckbox("ExtBandsKey", val => Outside.NodesParams.extendNodes_WithBands = val),
                    IsVisible = () => Outside.NodesParams.extendNodes && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "HvnPctileKey",
                    Label = "HVN(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.pctileHVN_Value.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHVN_Pctile(),
                    IsVisible = () => Outside.NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "LvnPctileKey",
                    Label = "LVN(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.pctileLVN_Value.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLVN_Pctile(),
                    IsVisible = () => Outside.NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 3,
                    Key = "ExtNodeStartKey",
                    Label = "From start?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.extendNodes_FromStart,
                    OnChanged = _ => UpdateCheckbox("ExtNodeStartKey", val => Outside.NodesParams.extendNodes_FromStart = val),
                    IsVisible = () => Outside.NodesParams.extendNodes && isEnable_AnyVP() && (IsNot_BubblesChart() && isPanel_VP() || isPanelOnly_VP())
                },


                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "EnableSpikeKey",
                    Label = "Enable?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.SpikeFilterParams.EnableSpikeFilter,
                    OnChanged = _ => UpdateCheckbox("EnableSpikeKey", val => Outside.SpikeFilterParams.EnableSpikeFilter = val),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeSourceKey",
                    Label = "Source",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.SpikeSource_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SpikeSource_Data)),
                    OnChanged = _ => UpdateSpikeSource(),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeViewKey",
                    Label = "View",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.SpikeView_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SpikeView_Data)),
                    OnChanged = _ => UpdateSpikeView(),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeFilterKey",
                    Label = "Filter",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.SpikeFilter_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SpikeFilter_Data)),
                    OnChanged = _ => UpdateSpikeFilter(),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikePeriodKey",
                    Label = "Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeFilterParams.MAperiod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateSpikeMAPeriod(),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeMATypeKey",
                    Label = "MA Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Outside.UseCustomMAs ? Outside.CustomMAType.Spike.ToString() : p.SpikeFilterParams.MAtype.ToString(),
                    EnumOptions = () => Outside.UseCustomMAs ? Enum.GetNames(typeof(MAType_Data)) : Enum.GetNames(typeof(MovingAverageType)),
                    OnChanged = _ => UpdateSpikeMAType(),
                    IsVisible = () => isDeltaMode() && isSpike_NoMAType() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "EnableNotifyKey",
                    Label = "Notify?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.SpikeFilterParams.EnableSpikeNotification,
                    OnChanged = _ => UpdateCheckbox("EnableNotifyKey", val => Outside.SpikeFilterParams.EnableSpikeNotification = val),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeTypeKey",
                    Label = "Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.NotificationType_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(NotificationType_Data)),
                    OnChanged = _ => UpdateSpikeNotifyType(),
                    IsVisible = () => isDeltaMode() && Outside.SpikeFilterParams.EnableSpikeNotification && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeSoundKey",
                    Label = "Sound",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.Spike_SoundType.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SoundType)),
                    OnChanged = _ => UpdateSpikeSound(),
                    IsVisible = () => isDeltaMode() && Outside.SpikeFilterParams.EnableSpikeNotification && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeLevelsKey",
                    Label = "Levels?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.SpikeLevelParams.ShowSpikeLevels,
                    OnChanged = _ => UpdateCheckbox("SpikeLevelsKey", val => Outside.SpikeLevelParams.ShowSpikeLevels = val),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeLvsTouchKey",
                    Label = "Max Touch",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeLevelParams.MaxCount.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateSpikeLevels_MaxCount(),
                    IsVisible = () => isDeltaMode() && Outside.SpikeLevelParams.ShowSpikeLevels && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeLvsColorKey",
                    Label = "Coloring",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeLevelParams.SpikeLevelsColoring_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SpikeLevelsColoring_Data)),
                    OnChanged = _ => UpdateSpikeLevels_Coloring(),
                    IsVisible = () => isDeltaMode() && Outside.SpikeLevelParams.ShowSpikeLevels && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeChartKey",
                    Label = "Chart?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.SpikeFilterParams.EnableSpikeChart,
                    OnChanged = _ => UpdateCheckbox("SpikeChartKey", val => Outside.SpikeFilterParams.EnableSpikeChart = val),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeColorKey",
                    Label = "Coloring",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.SpikeChartColoring_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SpikeChartColoring_Data)),
                    OnChanged = _ => UpdateSpikeChart_Coloring(),
                    IsVisible = () => isDeltaMode() && Outside.SpikeFilterParams.EnableSpikeChart && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "SpikeLvsResetKey",
                    Label = "Reset Daily?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.SpikeLevelParams.ResetDaily,
                    OnChanged = _ => UpdateCheckbox("SpikeLvsResetKey", val => Outside.SpikeLevelParams.ResetDaily = val),
                    IsVisible = () => isDeltaMode() && Outside.SpikeLevelParams.ShowSpikeLevels && IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Tick Spike",
                    RegionOrder = 4,
                    Key = "IconViewKey",
                    Label = "Icon",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeFilterParams.IconView_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ChartIconType)),
                    OnChanged = _ => UpdateIconView(),
                    IsVisible = () => isDeltaMode() && Outside.SpikeFilterParams.SpikeView_Input == SpikeView_Data.Icon && IsNot_BubblesChart() && isPanel_ODF()
                },

                // Ratio
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "SpikeRatioKey",
                    Label = "Ratio",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.SpikeRatioParams.SpikeRatio_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SpikeRatio_Data)),
                    OnChanged = _ => UpdateSpikeRatio(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isPanel_ODF()
                },
                // Percentage => Period + MA type
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "PctPeriodKey",
                    Label = "Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.MAperiod_PctSpike.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdatePctPeriod_Spike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "PctMATypeKey",
                    Label = "MA Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Outside.UseCustomMAs ? Outside.CustomMAType.SpikePctRatio.ToString() : p.SpikeRatioParams.MAtype_PctSpike.ToString(),
                    EnumOptions = () => Outside.UseCustomMAs ? Enum.GetNames(typeof(MAType_Data)) : Enum.GetNames(typeof(MovingAverageType)),
                    OnChanged = _ => UpdateMAType_PctSpike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                // Percentage
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "LowestPctKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Lowest_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowest_Pct(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "LowPctKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Low_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLow_Pct(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "AveragePctKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Average_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverage_Pct(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "HighPctKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.High_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHigh_Pct(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "UltraPctKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Ultra_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltra_Pct(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikePercentage() && isPanel_ODF()
                },
                // [Debug] Show Strength
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "DebugSpikeKey",
                    Label = "Debug?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.SpikeRatioParams.ShowStrengthValue,
                    OnChanged = _ => UpdateCheckbox("DebugSpikeKey", val => Outside.SpikeRatioParams.ShowStrengthValue = val),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isPanel_ODF()
                },
                // Fixed
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "LowestFixedSpikeKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Lowest_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowestFixed_Spike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikeFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "LowFixedSpikeKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Low_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowFixed_Spike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikeFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "AverageFixedSpikeKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Average_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverageFixed_Spike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikeFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "HighFixedSpikeKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.High_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHighFixed_Spike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikeFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Spike(ratio)",
                    RegionOrder = 5,
                    Key = "UltraFixedSpikeKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.SpikeRatioParams.Ultra_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltraFixed_Spike(),
                    IsVisible = () => isDeltaMode() && isSpikeFilter() && IsNot_BubblesChart() && isSpikeFixed() && isPanel_ODF()
                },


                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "EnableBubblesKey",
                    Label = "Enable?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.BubblesChartParams.EnableBubblesChart,
                    OnChanged = _ => UpdateCheckbox("EnableBubblesKey", val => Outside.BubblesChartParams.EnableBubblesChart = val),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubblesSizeKey",
                    Label = "Size Multiplier",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesChartParams.BubblesSizeMultiplier.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateBubblesSize(),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubblesSourceKey",
                    Label = "Source",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesChartParams.BubblesSource_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(BubblesSource_Data)),
                    OnChanged = _ => UpdateBubblesSource(),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubblesChangeKey",
                    Label = "Change?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.BubblesChartParams.UseChangeSeries,
                    OnChanged = _ => UpdateCheckbox("BubblesChangeKey", val => Outside.BubblesChartParams.UseChangeSeries = val),
                    IsVisible = () => isDeltaMode() &&IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "ChangePeriodKey",
                    Label = "Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesChartParams.changePeriod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateChangePeriod(),
                    IsVisible = () => isDeltaMode() && isBubblesChange() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "ChangeOperatorKey",
                    Label = "Operator",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesChartParams.ChangeOperator_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ChangeOperator_Data)),
                    OnChanged = _ => UpdateChangeOperator(),
                    IsVisible = () => isDeltaMode() && isBubblesChange() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubblesFilterKey",
                    Label = "Filter",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesChartParams.BubblesFilter_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(BubblesFilter_Data)),
                    OnChanged = _ => UpdateBubblesFilter(),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubbMAPeriodKey",
                    Label = "MA Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesChartParams.MAperiod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateBubblesMAPeriod(),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubbMATypeKey",
                    Label = "MA Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Outside.UseCustomMAs ? Outside.CustomMAType.Bubbles.ToString() : p.BubblesChartParams.MAtype.ToString(),
                    EnumOptions = () => Outside.UseCustomMAs ? Enum.GetNames(typeof(MAType_Data)) : Enum.GetNames(typeof(MovingAverageType)),
                    OnChanged = _ => UpdateBubblesMAType(),
                    IsVisible = () => isDeltaMode() && isBubbles_NoMAType() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubblesColoringKey",
                    Label = "Coloring",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesChartParams.BubblesColoring_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(BubblesColoring_Data)),
                    OnChanged = _ => UpdateBubblesColoring(),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "BubblesMomentumKey",
                    Label = "Strategy",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesChartParams.BubblesMomentum_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(BubblesMomentum_Data)),
                    OnChanged = _ => UpdateBubblesMomentum(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesChartParams.BubblesColoring_Input == BubblesColoring_Data.Momentum && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraNotifyKey",
                    Label = "Ultra Notify?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.BubblesLevelParams.EnableUltraNotification,
                    OnChanged = _ => UpdateCheckbox("UltraNotifyKey", val => Outside.BubblesLevelParams.EnableUltraNotification = val),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraTypeKey",
                    Label = "Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesLevelParams.NotificationType_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(NotificationType_Data)),
                    OnChanged = _ => UpdateUltraNotifyType(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.EnableUltraNotification && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraSoundKey",
                    Label = "Sound",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesLevelParams.Ultra_SoundType.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SoundType)),
                    OnChanged = _ => UpdateUltraSound(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.EnableUltraNotification && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraLevelskey",
                    Label = "Ultra Levels?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.BubblesLevelParams.ShowUltraLevels,
                    OnChanged = _ => UpdateCheckbox("UltraLevelskey", val => Outside.BubblesLevelParams.ShowUltraLevels = val),
                    IsVisible = () => isDeltaMode() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraCountKey",
                    Label = "Max Touch",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesLevelParams.MaxCount,
                    OnChanged = _ => UpdateUltraLevels_MaxCount(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.ShowUltraLevels && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraBreakKey",
                    Label = "Touch from",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesLevelParams.UltraBubblesBreak_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(UltraBubblesBreak_Data)),
                    OnChanged = _ => UpdateUltraBreakStrategy(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.ShowUltraLevels && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraResetKey",
                    Label = "Reset Daily?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.BubblesLevelParams.ResetDaily,
                    OnChanged = _ => UpdateCheckbox("UltraResetKey", val => Outside.BubblesLevelParams.ResetDaily = val),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.ShowUltraLevels && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraRectSizeKey",
                    Label = "Level Size",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesLevelParams.UltraBubbles_RectSizeInput.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(UltraBubbles_RectSizeData)),
                    OnChanged = _ => UpdateUltraRectangleSize(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.ShowUltraLevels && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles Chart",
                    RegionOrder = 6,
                    Key = "UltraColoringKey",
                    Label = "Coloring",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesLevelParams.UltraBubblesColoring_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(UltraBubblesColoring_Data)),
                    OnChanged = _ => UpdateUltraColoring(),
                    IsVisible = () => isDeltaMode() && Outside.BubblesLevelParams.ShowUltraLevels && IsNot_SpikeChart() && isPanel_ODF()
                },


                // Ratio
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "BubblesRatioKey",
                    Label = "Ratio",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.BubblesRatioParams.BubblesRatio_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(BubblesRatio_Data)),
                    OnChanged = _ => UpdateBubblesRatio(),
                    IsVisible = () => isBubblesChart() && isPanel_ODF()
                },
                // Percentile => Period
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "PctilePeriodKey",
                    Label = "Pctile Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.PctilePeriod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdatePctilePeriod_Bubbles(),
                    IsVisible = () => isBubblesChart() && isBubblesPercentile() && isPanel_ODF()
                },
                // [Debug] Show Strength
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "DebugBubblesKey",
                    Label = "Debug?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.BubblesRatioParams.ShowStrengthValue,
                    OnChanged = _ => UpdateCheckbox("DebugBubblesKey", val => Outside.BubblesRatioParams.ShowStrengthValue = val),
                    IsVisible = () => isBubblesChart() && isPanel_ODF()
                },
                // Percentile
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "LowestPctileKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Lowest_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowest_Pctile(),
                    IsVisible = () => isBubblesChart() && isBubblesPercentile() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "LowPctileKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Low_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLow_Pctile(),
                    IsVisible = () => isBubblesChart() && isBubblesPercentile() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "AveragePctileKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Average_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverage_Pctile(),
                    IsVisible = () => isBubblesChart() && isBubblesPercentile() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "HighPctileKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.High_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHigh_Pctile(),
                    IsVisible = () => isBubblesChart() && isBubblesPercentile() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "UltraPctileKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Ultra_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltra_Pctile(),
                    IsVisible = () => isBubblesChart() && isBubblesPercentile() && isPanel_ODF()
                },
                // Fixed
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "LowestFixedBubblesKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Lowest_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowestFixed_Bubbles(),
                    IsVisible = () => isBubblesChart() && isBubblesFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "LowFixedBubblesKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Low_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowFixed_Bubbles(),
                    IsVisible = () => isBubblesChart() && isBubblesFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "AverageFixedBubblesKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Average_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverageFixed_Bubbles(),
                    IsVisible = () => isBubblesChart() && isBubblesFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "HighFixedBubblesKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.High_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHighFixed_Bubbles(),
                    IsVisible = () => isBubblesChart() && isBubblesFixed() && isPanel_ODF()
                },
                new()
                {
                    Region = "Bubbles(ratio)",
                    RegionOrder = 7,
                    Key = "UltraFixedBubblesKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.BubblesRatioParams.Ultra_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltraFixed_Bubbles(),
                    IsVisible = () => isBubblesChart() && isBubblesFixed() && isPanel_ODF()
                }, 


                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "ShowResultsKey",
                    Label = "Show?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowResults,
                    OnChanged = _ => UpdateCheckbox("ShowResultsKey", val => Outside.ResultParams.ShowResults = val),
                    IsVisible = () => IsNot_BubblesChart() && IsNot_SpikeChart()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "EnableLargeKey",
                    Label = "Enable Filter?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.EnableLargeFilter,
                    OnChanged = _ => UpdateCheckbox("EnableLargeKey", val => Outside.ResultParams.EnableLargeFilter = val),
                    IsVisible = () => IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "ShowMinMaxKey",
                    Label = "Min/Max?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowMinMaxDelta,
                    OnChanged = _ => UpdateCheckbox("ShowMinMaxKey", val => Outside.ResultParams.ShowMinMaxDelta = val),
                    IsVisible = () => isDeltaMode() && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "LargeMATypeKey",
                    Label = "MA Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Outside.UseCustomMAs ? Outside.CustomMAType.Large.ToString() : p.ResultParams.MAtype.ToString(),
                    EnumOptions = () => Outside.UseCustomMAs ? Enum.GetNames(typeof(MAType_Data)) : Enum.GetNames(typeof(MovingAverageType)),
                    OnChanged = _ => UpdateLargeMAType(),
                    IsVisible = () => Outside.ResultParams.EnableLargeFilter && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "LargePeriodKey",
                    Label = "MA Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ResultParams.MAperiod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLargeMAPeriod(),
                    IsVisible = () => Outside.ResultParams.EnableLargeFilter && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "LargeRatioKey",
                    Label = "Ratio",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ResultParams.LargeRatio.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLargeRatio(),
                    IsVisible = () => Outside.ResultParams.EnableLargeFilter && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "ShowSideTotalKey",
                    Label = "Side(total)?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowSideTotal,
                    OnChanged = _ => UpdateCheckbox("ShowSideTotalKey", val => Outside.ResultParams.ShowSideTotal = val),
                    IsVisible = () => isNot_NormalMode() && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "ResultViewKey",
                    Label = "Side(view)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ResultParams.ResultsView_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ResultsView_Data)),
                    OnChanged = _ => UpdateResultView(),
                    IsVisible = () => isNot_NormalMode() && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "OnlySubtKey",
                    Label = "Only Subtract?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowOnlySubtDelta,
                    OnChanged = _ => UpdateCheckbox("OnlySubtKey", val => Outside.ResultParams.ShowOnlySubtDelta = val),
                    IsVisible = () => isDeltaMode() && Outside.ResultParams.ShowMinMaxDelta && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Results",
                    RegionOrder = 8,
                    Key = "OperatorKey",
                    Label = "Operator",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ResultParams.OperatorBuySell_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(OperatorBuySell_Data)),
                    OnChanged = _ => UpdateOperator(),
                    IsVisible = () => Outside.GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell && IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },

                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "ShowHistKey",
                    Label = "Histogram?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.MiscParams.ShowHist,
                    OnChanged = _ => UpdateCheckbox("ShowHistKey", val => Outside.MiscParams.ShowHist = val),
                    IsVisible = () => IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "FillHistKey",
                    Label = "Fill Hist?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.MiscParams.FillHist,
                    OnChanged = _ => UpdateCheckbox("FillHistKey", val => Outside.MiscParams.FillHist = val),
                    IsVisible = () => IsNot_BubblesChart() && IsNot_SpikeChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "ShowNumbersKey",
                    Label = "Numbers?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.MiscParams.ShowNumbers,
                    OnChanged = _ => UpdateCheckbox("ShowNumbersKey", val => Outside.MiscParams.ShowNumbers = val),
                    IsVisible = () => IsNot_BubblesChart() && isPanel_ODF()
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "DrawAtKey",
                    Label = "Draw at Zoom",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.MiscParams.DrawAtZoom_Value.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateDrawAtZoom()
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "SegmentsKey",
                    Label = "Segments",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.MiscParams.SegmentsInterval_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SegmentsInterval_Data)),
                    OnChanged = _ => UpdateSegmentsInterval(),
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "ODFIntervalKey",
                    Label = "ODF + VP",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.MiscParams.ODFInterval_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ODFInterval_Data)),
                    OnChanged = _ => UpdateODFInterval(),
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 9,
                    Key = "BubbleValueKey",
                    Label = "Bubbles-V?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.MiscParams.ShowBubbleValue,
                    OnChanged = _ => UpdateCheckbox("BubbleValueKey", val => Outside.MiscParams.ShowBubbleValue = val),
                    IsVisible = () => isBubblesChart() && isPanel_ODF()
                },
            };
        }

        private ControlBase CreateTradingPanel()
        {
            // Replace StackPanel to Grid
            // So the Footer stays pinned at the bottom, always visible.
            Grid mainPanel = new(3, 1);

            mainPanel.Rows[0].SetHeightToAuto();
            mainPanel.AddChild(CreateHeader(), 0, 0);

            mainPanel.Rows[1].SetHeightInStars(1); // Takes remaining space
            mainPanel.AddChild(CreateContentPanel(), 1, 0);

            mainPanel.Rows[2].SetHeightToAuto();
            mainPanel.AddChild(CreateFooter(), 2, 0);

            return mainPanel;
        }

        private static ControlBase CreateHeader()
        {
            var grid = new Grid(0, 0);
            grid.AddChild(new TextBlock
            {
                Text = "Order Flow Ticks\nAggregated",
                Margin = "10 7",
                Style = Styles.CreateHeaderStyle(),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center

            });
            var border = new Border
            {
                BorderThickness = "0 0 0 1",
                Style = Styles.CreateCommonBorderStyle(),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 250, // ParamsPanel Width
                Child = grid
            };
            return border;
        }

        private ControlBase CreateFooter()
        {
            var footerGrid = new Grid(2, 3)
            {
                Margin = 8,
                VerticalAlignment = VerticalAlignment.Center
            };

            footerGrid.Columns[0].SetWidthInStars(1);
            footerGrid.Columns[1].SetWidthInPixels(8);
            footerGrid.Columns[2].SetWidthToAuto();

            // Fix MacOS => small size button (save)
            footerGrid.Rows[0].SetHeightInPixels(35);

            var saveButton = CreateSaveButton();
            footerGrid.AddChild(saveButton, 0, 2);

            _progressBar = new ProgressBar {
                Height = 12,
                Margin = "0 2 0 0"
            };
            footerGrid.AddChild(_progressBar, 0, 0);

            footerGrid.AddChild(CreateApplyButton_TextInput(), 1, 0, 1, 3);
            footerGrid.AddChild(CreateFixedRangeButton(), 1, 0, 1, 3);

            return footerGrid;
        }

        private ScrollViewer CreateContentPanel()
        {
            var contentPanel = new StackPanel
            {
                Margin = 10,
                // Fix MacOS => large string increase column and hidden others
                Width = 250, // ParamsPanel Width
                // Fix MacOS(maybe) => panel is cut short/half the size
                VerticalAlignment = VerticalAlignment.Top,
            };

            // --- Mode controls at the top ---
            var grid = new Grid(2, 5);
            grid.Columns[1].SetWidthInPixels(5);
            grid.Columns[3].SetWidthInPixels(5);

            // Fix MacOS => small size button (modeinfo)
            grid.Rows[0].SetHeightInPixels(45);

            grid.AddChild(CreatePassButton("<"), 0, 0);
            grid.AddChild(CreateModeInfo_Button(FirstParams.GeneralParams.VolumeMode_Input.ToString()), 0, 1, 1, 3);
            grid.AddChild(CreatePassButton(">"), 0, 4);

            contentPanel.AddChild(grid);

            // --- Create region sections ---
            var groups = _paramDefinitions
                .GroupBy(p => p.Region)
                .OrderBy(g => g.FirstOrDefault().RegionOrder);
            // With g.FirstOrDefault().Key => Worked as expected until 2x "Enable[...]Key" appear

            foreach (var group in groups)
            {
                var section = new RegionSection(group.Key, group);
                _regionSections[group.Key] = section;

                // param grid inside section
                var groupGrid = new Grid(9, 5);
                groupGrid.Columns[1].SetWidthInPixels(5);
                groupGrid.Columns[3].SetWidthInPixels(5);

                int row = 0, col = 0;
                foreach (var param in group)
                {
                    var control = CreateParamControl(param);
                    groupGrid.AddChild(control, row, col);
                    col += 2;
                    if (col > 4) { row++; col = 0; }
                }

                section.AddParamControl(groupGrid);
                contentPanel.AddChild(section.Container);
            }

            ScrollViewer scroll = new() {
                Content = contentPanel,
                Style = Styles.CreateScrollViewerTransparentStyle(),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };

            return scroll;
        }

        private ControlBase CreateParamControl(ParamDefinition param)
        {
            return param.InputType switch
            {
                ParamInputType.Text => CreateInputWithLabel(param.Label, param.GetDefault(FirstParams).ToString(), param.Key, param.OnChanged),
                ParamInputType.Checkbox => CreateCheckboxWithLabel(param.Label, (bool)param.GetDefault(FirstParams), param.Key, param.OnChanged),
                ParamInputType.ComboBox => CreateComboBoxWithLabel(param.Label, param.Key, (string)param.GetDefault(FirstParams), param.EnumOptions(), param.OnChanged),
                _ => throw new NotSupportedException()
            };
        }

        private Button CreatePassButton(string label)
        {
            Button button = new()
            {
                Text = label,
                Padding = 0,
                Width = 30,
                Height = 20,
                Margin = 0,
                BackgroundColor = Color.FromHex("#7F808080"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            button.Click += label == ">" ? NextModeEvent : PrevModeEvent;
            return button;
        }

        private Button CreateModeInfo_Button(string label)
        {
            Button button = new()
            {
                Text = label,
                Padding = 0,
                Width = 70,
                Height = 30,
                Margin = 4,
                Style = Styles.CreateButtonStyle(),
                HorizontalAlignment = HorizontalAlignment.Center

            };
            button.Click += _ => ResetParamsEvent();
            ModeBtn = button;
            return button;
        }

        private Button CreateSaveButton()
        {
            Button button = new()
            {
                Text = "💾 Save",
                Margin = 5,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            button.Click += (_) => SaveParams();
            SaveBtn = button;
            return button;
        }
        private Button CreateApplyButton_TextInput()
        {
            Button button = new() {
                Text = "Apply ✓",
                Padding = 0,
                Width = 50,
                Height = 20,
                Margin = 0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_) => RecalculateOutsideWithMsg();
            ApplyBtn = button;
            return button;
        }

        private void SetApplyVisibility() {
            ApplyBtn.IsVisible = true;
            RangeBtn.IsVisible = false;
        }
        private Button CreateFixedRangeButton()
        {
            Button button = new() {
                Text = "➕ Range",
                Padding = 0,
                Width = 50,
                Height = 20,
                Margin = 0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_) => Outside.CreateNewRange();
            RangeBtn = button;
            return button;
        }

        private Panel CreateInputWithLabel(string label, string defaultValue, string key, Action<string> onChanged)
        {
            var input = new TextBox
            {
                Text = defaultValue,
                Style = Styles.CreateInputStyle(),
                TextAlignment = TextAlignment.Center,
                Margin = "0 5 0 0"
            };
            input.TextChanged += _ => onChanged?.Invoke(key);
            textInputMap[key] = input;

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 10 0 0",
            };

            var text = new TextBlock { Text = label, TextAlignment = TextAlignment.Center };
            textInputLabelMap[key] = text;

            stack.AddChild(text);
            stack.AddChild(input);
            return stack;
        }

        private Panel CreateComboBoxWithLabel(string label, string key, string selected, IEnumerable<string> options, Action<string> onChanged)
        {
            var combo = new ComboBox
            {
                Style = Styles.CreateInputStyle(),
                Margin = "0 5 0 0",

            };
            foreach (var option in options)
                combo.AddItem(option);
            combo.SelectedItem = selected;
            combo.SelectedItemChanged += _ => onChanged?.Invoke(key);
            comboBoxMap[key] = combo;

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 10 0 0",
            };

            var text = new TextBlock { Text = label, TextAlignment = TextAlignment.Center };
            comboBoxTextMap[key] = text;

            stack.AddChild(text);
            stack.AddChild(combo);

            return stack;
        }

        private ControlBase CreateCheckboxWithLabel(string label, bool defaultValue, string key, Action<string> onChanged)
        {
            var checkbox = new CheckBox {
                Margin = "0 0 5 0",
                IsChecked = defaultValue,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            checkbox.Click += _ => onChanged?.Invoke(key);
            checkBoxMap[key] = checkbox;

            var text = new TextBlock { Text = label, TextAlignment = TextAlignment.Center };
            checkBoxTextMap[key] = text;

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 10 0 10",
            };

            stack.AddChild(text);
            stack.AddChild(checkbox);

            return stack;
        }

        private void ResetParamsEvent() => ChangeParams(FirstParams);

        private void ChangeParams(IndicatorParams p)
        {
            foreach (var param in _paramDefinitions)
            {
                switch (param.InputType)
                {
                    case ParamInputType.Text:
                        textInputMap[param.Key].Text = param.GetDefault(p).ToString();
                        break;
                    case ParamInputType.Checkbox:
                        checkBoxMap[param.Key].IsChecked = (bool)param.GetDefault(p);
                        break;
                    case ParamInputType.ComboBox:
                        comboBoxMap[param.Key].SelectedItem = param.GetDefault(p).ToString();
                        break;
                }
            }
        }

        private void UpdateCheckbox(string key, Action<bool> applyAction, bool updateImmediately = false)
        {
            bool value = checkBoxMap[key].IsChecked ?? false;
            applyAction(value);
            if (updateImmediately) {
                RecalculateOutsideWithMsg();
            } else {
                CheckboxHandler(key, value);
            }
        }
        private void CheckboxHandler(string key, bool value)
        {
            switch (key) {
                case "EnableBubblesKey":
                    if (value)
                        Outside.Chart.ChartType = ChartType.Line;
                    else if (!value && _originalValues.ContainsKey("ShowHistKey")) {
                        // ContainsKey avoids crash when loading
                        Outside.MiscParams.ShowHist = (bool)_originalValues["ShowHistKey"];
                        Outside.MiscParams.ShowNumbers = (bool)_originalValues["ShowNumbersKey"];
                        Outside.ResultParams.ShowResults = (bool)_originalValues["ShowResultsKey"];
                        Outside.SpikeFilterParams.EnableSpikeFilter = (bool)_originalValues["EnableSpikeKey"];
                        Outside.ProfileParams.EnableMainVP = (bool)_originalValues["EnableVPKey"];
                        Outside.ProfileParams.EnableMiniProfiles = (bool)_originalValues["MiniVPsKey"];
                        Outside.Chart.ChartType = ChartType.Hlc;
                    }
                    break;
                case "SpikeChartKey":
                    if (value)
                        Outside.Chart.ChartType = ChartType.Hlc;
                    else if (!value && _originalValues.ContainsKey("ShowHistKey")) {
                        // ContainsKey avoids crash when loading
                        Outside.SpikeFilterParams.EnableSpikeFilter = (bool)_originalValues["EnableSpikeKey"];
                        Outside.MiscParams.ShowHist = (bool)_originalValues["ShowHistKey"];
                        Outside.ResultParams.ShowResults = (bool)_originalValues["ShowResultsKey"];
                        Outside.ResultParams.ShowMinMaxDelta = (bool)_originalValues["ShowMinMaxKey"];
                        Outside.Chart.ChartType = ChartType.Hlc;
                    }
                    break;
                case "IntradayVPKey":
                    RecalculateOutsideWithMsg(Outside.ProfileParams.ShowIntradayNumbers);
                    return;
                case "FillIntraVPKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "EnableNotifyKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "SpikeLvsResetKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "UltraNotifyKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "UltraResetKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "FillHistKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "FixedRangeKey":
                    RangeBtn.IsVisible = value;
                    return;
                case "NodeStrongKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "ExtendNodeKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "ExtBandsKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "ExtNodeStartKey":
                    RecalculateOutsideWithMsg(false);
                    return;
            }

            RecalculateOutsideWithMsg();
        }

        // ==== General ====
        private void UpdateDaysToShow()
        {
            int value = int.TryParse(textInputMap["DaysToShowKey"].Text, out var n) ? n : -2;
            if (value >= -1 && value != Outside.GetLookback())
            {
                Outside.SetLookback(value);
                SetApplyVisibility();
            }
        }
        private void UpdateRowHeight()
        {
            if (double.TryParse(textInputMap["RowHeightKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0.1)
            {
                double height = Outside.Symbol.PipSize * value;
                if (height != Outside.GetRowHeight())
                {
                    Outside.SetRowHeight(height);
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateVolumeView()
        {
            var selected = comboBoxMap["VolumeViewKey"].SelectedItem;
            if (Enum.TryParse(selected, out VolumeView_Data viewType) && viewType != Outside.GeneralParams.VolumeView_Input)
            {
                Outside.GeneralParams.VolumeView_Input = viewType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateIntensityMode()
        {
            var selected = comboBoxMap["IntensityModeKey"].SelectedItem;
            if (Enum.TryParse(selected, out IntensityMode_Data modeType) && modeType != Outside.GeneralParams.IntensityMode_Input)
            {
                Outside.GeneralParams.IntensityMode_Input = modeType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateIntensityNDays()
        {
            int value = int.TryParse(textInputMap["IntensityNDaysKey"].Text, out var n) ? n : 1;
            if (value >= 1 && value != Outside.GeneralParams.IntensityNDays_Input)
            {
                _pendingIntensityNDays = value;
                SetApplyVisibility();
            }
        }

        // ==== Volume Profile ====
        private void UpdateVP()
        {
            var selected = comboBoxMap["UpdateVPKey"].SelectedItem;
            if (Enum.TryParse(selected, out UpdateProfile_Data updateType) && updateType != Outside.ProfileParams.UpdateProfile_Input)
            {
                Outside.ProfileParams.UpdateProfile_Input = updateType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateSideVP()
        {
            var selected = comboBoxMap["SideVPKey"].SelectedItem;
            if (Enum.TryParse(selected, out HistSide_Data sideType) && sideType != Outside.ProfileParams.HistogramSide_Input)
            {
                Outside.ProfileParams.HistogramSide_Input = sideType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateWidthVP()
        {
            var selected = comboBoxMap["WidthVPKey"].SelectedItem;
            if (Enum.TryParse(selected, out HistWidth_Data widthType) && widthType != Outside.ProfileParams.HistogramWidth_Input)
            {
                Outside.ProfileParams.HistogramWidth_Input = widthType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateIntradayOffset()
        {
            int value = int.TryParse(textInputMap["IntraOffsetKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.ProfileParams.OffsetBarsInput)
            {
                Outside.ProfileParams.OffsetBarsInput = value;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateIntradayTimeframe()
        {
            var selected = comboBoxMap["IntraTFKey"].SelectedItem;
            TimeFrame value = StringToTimeframe(selected);
            if (value != TimeFrame.Minute && value != Outside.ProfileParams.OffsetTimeframeInput)
            {
                Outside.ProfileParams.OffsetTimeframeInput = value;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateMiniVPTimeframe()
        {
            var selected = comboBoxMap["MiniTFKey"].SelectedItem;
            TimeFrame value = StringToTimeframe(selected);
            if (value != TimeFrame.Minute && value != Outside.ProfileParams.MiniVPs_Timeframe)
            {
                Outside.ProfileParams.MiniVPs_Timeframe = value;
                Outside.SetMiniVPsBars();
                RecalculateOutsideWithMsg();
            }
        }
        private static TimeFrame StringToTimeframe(string inputTF)
        {
            TimeFrame ifWrong = TimeFrame.Minute;
            switch (inputTF)
            {
                // Candles
                case "m5": return TimeFrame.Minute5;
                case "m10": return TimeFrame.Minute10;
                case "m15": return TimeFrame.Minute15;
                case "m30": return TimeFrame.Minute30;
                case "m45": return TimeFrame.Minute45;
                case "h1": return TimeFrame.Hour;
                case "h2": return TimeFrame.Hour2;
                case "h3": return TimeFrame.Hour3;
                case "h4": return TimeFrame.Hour4;
                case "h6": return TimeFrame.Hour6;
                case "h8": return TimeFrame.Hour8;
                case "h12": return TimeFrame.Hour12;
                case "D1": return TimeFrame.Daily;
                case "D2": return TimeFrame.Day2;
                case "D3": return TimeFrame.Day3;
                case "W1": return TimeFrame.Weekly;
                case "Month1": return TimeFrame.Monthly;
                default:
                    break;
            }
            return ifWrong;
        }


        // ==== HVN + LVN ====
        private void UpdateNodeSmooth()
        {
            var selected = comboBoxMap["NodeSmoothKey"].SelectedItem;
            if (Enum.TryParse(selected, out ProfileSmooth_Data smoothType) && smoothType != Outside.NodesParams.ProfileSmooth_Input)
            {
                Outside.NodesParams.ProfileSmooth_Input = smoothType;
                Outside.nodesKernel = null;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateNodeType()
        {
            var selected = comboBoxMap["NodeTypeKey"].SelectedItem;
            if (Enum.TryParse(selected, out ProfileNode_Data nodeType) && nodeType != Outside.NodesParams.ProfileNode_Input)
            {
                Outside.NodesParams.ProfileNode_Input = nodeType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateShowNode()
        {
            var selected = comboBoxMap["ShowNodeKey"].SelectedItem;
            if (Enum.TryParse(selected, out ShowNode_Data showNodeType) && showNodeType != Outside.NodesParams.ShowNode_Input)
            {
                Outside.NodesParams.ShowNode_Input = showNodeType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateHVN_Band()
        {
            if (double.TryParse(textInputMap["HvnBandPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0.9)
            {
                if (value != Outside.NodesParams.bandHVN_Pct)
                {
                    Outside.NodesParams.bandHVN_Pct = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateLVN_Band()
        {
            if (double.TryParse(textInputMap["LvnBandPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0.9)
            {
                if (value != Outside.NodesParams.bandLVN_Pct)
                {
                    Outside.NodesParams.bandLVN_Pct = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateHVN_Strong()
        {
            if (double.TryParse(textInputMap["StrongHvnPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0.9)
            {
                if (value != Outside.NodesParams.strongHVN_Pct)
                {
                    Outside.NodesParams.strongHVN_Pct = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateLVN_Strong()
        {
            if (double.TryParse(textInputMap["StrongLvnPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0.9)
            {
                if (value != Outside.NodesParams.strongLVN_Pct)
                {
                    Outside.NodesParams.strongLVN_Pct = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateExtendNodesCount()
        {
            int value = int.TryParse(textInputMap["ExtNodesCountKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.NodesParams.extendNodes_Count)
            {
                Outside.NodesParams.extendNodes_Count = value;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateHVN_Pctile()
        {
            int value = int.TryParse(textInputMap["HvnPctileKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.NodesParams.pctileHVN_Value)
            {
                Outside.NodesParams.pctileHVN_Value = value;
                SetApplyVisibility();
            }
        }
        private void UpdateLVN_Pctile()
        {
            int value = int.TryParse(textInputMap["LvnPctileKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.NodesParams.pctileLVN_Value)
            {
                Outside.NodesParams.pctileLVN_Value = value;
                SetApplyVisibility();
            }
        }

        // ==== Spike Filter ====
        private void UpdateSpikeSource()
        {
            var selected = comboBoxMap["SpikeSourceKey"].SelectedItem;
            if (Enum.TryParse(selected, out SpikeSource_Data sourceType) && sourceType != Outside.SpikeFilterParams.SpikeSource_Input)
            {
                Outside.SpikeFilterParams.SpikeSource_Input = sourceType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateSpikeView()
        {
            var selected = comboBoxMap["SpikeViewKey"].SelectedItem;
            if (Enum.TryParse(selected, out SpikeView_Data viewType) && viewType != Outside.SpikeFilterParams.SpikeView_Input)
            {
                Outside.SpikeFilterParams.SpikeView_Input = viewType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateIconView()
        {
            var selected = comboBoxMap["IconViewKey"].SelectedItem;
            if (Enum.TryParse(selected, out ChartIconType viewType) && viewType != Outside.SpikeFilterParams.IconView_Input)
            {
                Outside.SpikeFilterParams.IconView_Input = viewType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateSpikeFilter()
        {
            var selected = comboBoxMap["SpikeFilterKey"].SelectedItem;
            if (Enum.TryParse(selected, out SpikeFilter_Data filterType) && filterType != Outside.SpikeFilterParams.SpikeFilter_Input)
            {
                Outside.SpikeFilterParams.SpikeFilter_Input = filterType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateSpikeMAType()
        {
            var selected = comboBoxMap["SpikeMATypeKey"].SelectedItem;
            if (Outside.UseCustomMAs) {
                if (Enum.TryParse(selected, out MAType_Data MAType) && MAType != Outside.CustomMAType.Spike)
                {
                    Outside.CustomMAType.Spike = MAType;
                    RecalculateOutsideWithMsg();
                }
            } else {
                if (Enum.TryParse(selected, out MovingAverageType MAType) && MAType != Outside.SpikeFilterParams.MAtype)
                {
                    Outside.SpikeFilterParams.MAtype = MAType;
                    RecalculateOutsideWithMsg();
                }
            }
        }
        private void UpdateSpikeMAPeriod()
        {
            if (int.TryParse(textInputMap["SpikePeriodKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.SpikeFilterParams.MAperiod)
                {
                    Outside.SpikeFilterParams.MAperiod = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateSpikeNotifyType()
        {
            var selected = comboBoxMap["SpikeTypeKey"].SelectedItem;
            if (Enum.TryParse(selected, out NotificationType_Data notifyType) && notifyType != Outside.SpikeFilterParams.NotificationType_Input)
            {
                Outside.SpikeFilterParams.NotificationType_Input = notifyType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateSpikeSound()
        {
            var selected = comboBoxMap["SpikeSoundKey"].SelectedItem;
            if (Enum.TryParse(selected, out SoundType soundType) && soundType != Outside.SpikeFilterParams.Spike_SoundType)
            {
                Outside.SpikeFilterParams.Spike_SoundType = soundType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateSpikeLevels_MaxCount()
        {
            if (int.TryParse(textInputMap["SpikeLvsTouchKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.SpikeLevelParams.MaxCount)
                {
                    Outside.SpikeLevelParams.MaxCount = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateSpikeLevels_Coloring()
        {
            var selected = comboBoxMap["SpikeLvsColorKey"].SelectedItem;
            if (Enum.TryParse(selected, out SpikeLevelsColoring_Data coloringType) && coloringType != Outside.SpikeLevelParams.SpikeLevelsColoring_Input)
            {
                Outside.SpikeLevelParams.SpikeLevelsColoring_Input = coloringType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateSpikeChart_Coloring()
        {
            var selected = comboBoxMap["SpikeColorKey"].SelectedItem;
            if (Enum.TryParse(selected, out SpikeChartColoring_Data coloringType) && coloringType != Outside.SpikeFilterParams.SpikeChartColoring_Input)
            {
                Outside.SpikeFilterParams.SpikeChartColoring_Input = coloringType;
                RecalculateOutsideWithMsg(false);
            }
        }

        // ==== Spike(ratio) ====

        private void UpdateSpikeRatio()
        {
            var selected = comboBoxMap["SpikeRatioKey"].SelectedItem;
            if (Enum.TryParse(selected, out SpikeRatio_Data ratioType) && ratioType != Outside.SpikeRatioParams.SpikeRatio_Input)
            {
                Outside.SpikeRatioParams.SpikeRatio_Input = ratioType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdatePctPeriod_Spike() {
            if (int.TryParse(textInputMap["PctPeriodKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.SpikeRatioParams.MAperiod_PctSpike)
                {
                    Outside.SpikeRatioParams.MAperiod_PctSpike = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateMAType_PctSpike()
        {
            var selected = comboBoxMap["PctMATypeKey"].SelectedItem;
            if (Outside.UseCustomMAs) {
                if (Enum.TryParse(selected, out MAType_Data MAType) && MAType != Outside.CustomMAType.SpikePctRatio)
                {
                    Outside.CustomMAType.SpikePctRatio = MAType;
                    RecalculateOutsideWithMsg();
                }
            } else {
                if (Enum.TryParse(selected, out MovingAverageType MAType) && MAType != Outside.SpikeRatioParams.MAtype_PctSpike)
                {
                    Outside.SpikeRatioParams.MAtype_PctSpike = MAType;
                    RecalculateOutsideWithMsg();
                }
            }
        }
        // Percentage
        private void UpdateLowest_Pct()
        {
            if (double.TryParse(textInputMap["LowestPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Lowest_PctValue)
                {
                    Outside.SpikeRatioParams.Lowest_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLow_Pct()
        {
            if (double.TryParse(textInputMap["LowPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Low_PctValue)
                {
                    Outside.SpikeRatioParams.Low_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverage_Pct()
        {
            if (double.TryParse(textInputMap["AveragePctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Average_PctValue)
                {
                    Outside.SpikeRatioParams.Average_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHigh_Pct()
        {
            if (double.TryParse(textInputMap["HighPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.High_PctValue)
                {
                    Outside.SpikeRatioParams.High_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltra_Pct()
        {
            if (double.TryParse(textInputMap["UltraPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Ultra_PctValue)
                {
                    Outside.SpikeRatioParams.Ultra_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        // Fixed
        private void UpdateLowestFixed_Spike()
        {
            if (double.TryParse(textInputMap["LowestFixedSpikeKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Lowest_FixedValue)
                {
                    Outside.SpikeRatioParams.Lowest_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLowFixed_Spike()
        {
            if (double.TryParse(textInputMap["LowFixedSpikeKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Low_FixedValue)
                {
                    Outside.SpikeRatioParams.Low_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverageFixed_Spike()
        {
            if (double.TryParse(textInputMap["AverageFixedSpikeKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Average_FixedValue)
                {
                    Outside.SpikeRatioParams.Average_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHighFixed_Spike()
        {
            if (double.TryParse(textInputMap["HighFixedSpikeKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.High_FixedValue)
                {
                    Outside.SpikeRatioParams.High_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltraFixed_Spike()
        {
            if (double.TryParse(textInputMap["UltraFixedSpikeKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.SpikeRatioParams.Ultra_FixedValue)
                {
                    Outside.SpikeRatioParams.Ultra_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }


        // ==== Bubbles Chart ====
        private void UpdateBubblesSize()
        {
            int value = int.TryParse(textInputMap["BubblesSizeKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.BubblesChartParams.BubblesSizeMultiplier)
            {
                Outside.BubblesChartParams.BubblesSizeMultiplier = value;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateBubblesSource()
        {
            var selected = comboBoxMap["BubblesSourceKey"].SelectedItem;
            if (Enum.TryParse(selected, out BubblesSource_Data sourceType) && sourceType != Outside.BubblesChartParams.BubblesSource_Input)
            {
                Outside.BubblesChartParams.BubblesSource_Input = sourceType;
                RecalculateOutsideWithMsg(Outside.BubblesLevelParams.ShowUltraLevels);
            }
        }
        private void UpdateChangePeriod()
        {
            int value = int.TryParse(textInputMap["ChangePeriodKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.BubblesChartParams.changePeriod)
            {
                Outside.BubblesChartParams.changePeriod = value;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateChangeOperator()
        {
            var selected = comboBoxMap["ChangeOperatorKey"].SelectedItem;
            if (Enum.TryParse(selected, out ChangeOperator_Data operatorType) && operatorType != Outside.BubblesChartParams.ChangeOperator_Input)
            {
                Outside.BubblesChartParams.ChangeOperator_Input = operatorType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateBubblesFilter()
        {
            var selected = comboBoxMap["BubblesFilterKey"].SelectedItem;
            if (Enum.TryParse(selected, out BubblesFilter_Data filterType) && filterType != Outside.BubblesChartParams.BubblesFilter_Input)
            {
                Outside.BubblesChartParams.BubblesFilter_Input = filterType;
                RecalculateOutsideWithMsg(Outside.BubblesLevelParams.ShowUltraLevels);
            }
        }
        private void UpdateBubblesMAType() {
            var selected = comboBoxMap["BubbMATypeKey"].SelectedItem;

            if (Outside.UseCustomMAs) {
                if (Enum.TryParse(selected, out MAType_Data MAType) && MAType != Outside.CustomMAType.Bubbles)
                {
                    Outside.CustomMAType.Bubbles = MAType;
                    RecalculateOutsideWithMsg();
                }
            } else {
                if (Enum.TryParse(selected, out MovingAverageType MAType) && MAType != Outside.BubblesChartParams.MAtype)
                {
                    Outside.BubblesChartParams.MAtype = MAType;
                    RecalculateOutsideWithMsg();
                }
            }
        }

        private void UpdateBubblesMAPeriod() {
            if (int.TryParse(textInputMap["BubbMAPeriodKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.BubblesChartParams.MAperiod)
                {
                    Outside.BubblesChartParams.MAperiod = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateBubblesColoring()
        {
            var selected = comboBoxMap["BubblesColoringKey"].SelectedItem;
            if (Enum.TryParse(selected, out BubblesColoring_Data coloringType) && coloringType != Outside.BubblesChartParams.BubblesColoring_Input)
            {
                Outside.BubblesChartParams.BubblesColoring_Input = coloringType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateBubblesMomentum()
        {
            var selected = comboBoxMap["BubblesMomentumKey"].SelectedItem;
            if (Enum.TryParse(selected, out BubblesMomentum_Data strategyType) && strategyType != Outside.BubblesChartParams.BubblesMomentum_Input)
            {
                Outside.BubblesChartParams.BubblesMomentum_Input = strategyType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateUltraNotifyType()
        {
            var selected = comboBoxMap["UltraTypeKey"].SelectedItem;
            if (Enum.TryParse(selected, out NotificationType_Data notifyType) && notifyType != Outside.BubblesLevelParams.NotificationType_Input)
            {
                Outside.BubblesLevelParams.NotificationType_Input = notifyType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateUltraSound()
        {
            var selected = comboBoxMap["UltraSoundKey"].SelectedItem;
            if (Enum.TryParse(selected, out SoundType soundType) && soundType != Outside.BubblesLevelParams.Ultra_SoundType)
            {
                Outside.SpikeFilterParams.Spike_SoundType = soundType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateUltraLevels_MaxCount()
        {
            if (int.TryParse(textInputMap["UltraCountKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.BubblesLevelParams.MaxCount)
                {
                    Outside.BubblesLevelParams.MaxCount = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateUltraBreakStrategy()
        {
            var selected = comboBoxMap["UltraBreakKey"].SelectedItem;
            if (Enum.TryParse(selected, out UltraBubblesBreak_Data breakType) && breakType != Outside.BubblesLevelParams.UltraBubblesBreak_Input)
            {
                Outside.BubblesLevelParams.UltraBubblesBreak_Input = breakType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateUltraRectangleSize()
        {
            var selected = comboBoxMap["UltraRectSizeKey"].SelectedItem;
            if (Enum.TryParse(selected, out UltraBubbles_RectSizeData rectSizeType) && rectSizeType != Outside.BubblesLevelParams.UltraBubbles_RectSizeInput)
            {
                Outside.BubblesLevelParams.UltraBubbles_RectSizeInput = rectSizeType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateUltraColoring()
        {
            var selected = comboBoxMap["UltraColoringKey"].SelectedItem;
            if (Enum.TryParse(selected, out UltraBubblesColoring_Data coloringType) && coloringType != Outside.BubblesLevelParams.UltraBubblesColoring_Input)
            {
                Outside.BubblesLevelParams.UltraBubblesColoring_Input = coloringType;
                RecalculateOutsideWithMsg(false);
            }
        }

        // ==== Bubbles(ratio)

        private void UpdateBubblesRatio()
        {
            var selected = comboBoxMap["BubblesRatioKey"].SelectedItem;
            if (Enum.TryParse(selected, out BubblesRatio_Data ratioType) && ratioType != Outside.BubblesRatioParams.BubblesRatio_Input)
            {
                Outside.BubblesRatioParams.BubblesRatio_Input = ratioType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdatePctilePeriod_Bubbles() {
            if (int.TryParse(textInputMap["PctilePeriodKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.BubblesRatioParams.PctilePeriod)
                {
                    Outside.BubblesRatioParams.PctilePeriod = value;
                    SetApplyVisibility();
                }
            }
        }
        // Percentile
        private void UpdateLowest_Pctile()
        {
            if (int.TryParse(textInputMap["LowestPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Lowest_PctileValue)
                {
                    Outside.BubblesRatioParams.Lowest_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLow_Pctile()
        {
            if (int.TryParse(textInputMap["LowPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Low_PctileValue)
                {
                    Outside.BubblesRatioParams.Low_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverage_Pctile()
        {
            if (int.TryParse(textInputMap["AveragePctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Average_PctileValue)
                {
                    Outside.BubblesRatioParams.Average_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHigh_Pctile()
        {
            if (int.TryParse(textInputMap["HighPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.High_PctileValue)
                {
                    Outside.BubblesRatioParams.High_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltra_Pctile()
        {
            if (int.TryParse(textInputMap["UltraPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Ultra_PctileValue)
                {
                    Outside.BubblesRatioParams.Ultra_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        // Fixed
        private void UpdateLowestFixed_Bubbles()
        {
            if (double.TryParse(textInputMap["LowestFixedBubblesKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Lowest_FixedValue)
                {
                    Outside.BubblesRatioParams.Lowest_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLowFixed_Bubbles()
        {
            if (double.TryParse(textInputMap["LowFixedBubblesKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Low_FixedValue)
                {
                    Outside.BubblesRatioParams.Low_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverageFixed_Bubbles()
        {
            if (double.TryParse(textInputMap["AverageFixedBubblesKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Average_FixedValue)
                {
                    Outside.BubblesRatioParams.Average_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHighFixed_Bubbles()
        {
            if (double.TryParse(textInputMap["HighFixedBubblesKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.High_FixedValue)
                {
                    Outside.BubblesRatioParams.High_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltraFixed_Bubbles()
        {
            if (double.TryParse(textInputMap["UltraFixedBubblesKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.BubblesRatioParams.Ultra_FixedValue)
                {
                    Outside.BubblesRatioParams.Ultra_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }

        // ==== Results ====
        private void UpdateResultView()
        {
            var selected = comboBoxMap["ResultViewKey"].SelectedItem;
            if (Enum.TryParse(selected, out ResultsView_Data viewType) && viewType != Outside.ResultParams.ResultsView_Input)
            {
                Outside.ResultParams.ResultsView_Input = viewType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateLargeMAType()
        {
            var selected = comboBoxMap["LargeMATypeKey"].SelectedItem;

            if (Outside.UseCustomMAs) {
                if (Enum.TryParse(selected, out MAType_Data MAType) && MAType != Outside.CustomMAType.Large)
                {
                    Outside.CustomMAType.Large = MAType;
                    RecalculateOutsideWithMsg();
                }
            } else {
                if (Enum.TryParse(selected, out MovingAverageType MAType) && MAType != Outside.ResultParams.MAtype)
                {
                    Outside.ResultParams.MAtype = MAType;
                    RecalculateOutsideWithMsg();
                }
            }
        }
        private void UpdateLargeMAPeriod()
        {
            if (int.TryParse(textInputMap["LargePeriodKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.ResultParams.MAperiod)
                {
                    Outside.ResultParams.MAperiod = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateLargeRatio()
        {
            if (double.TryParse(textInputMap["LargeRatioKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.ResultParams.LargeRatio)
                {
                    Outside.ResultParams.LargeRatio = value;
                    SetApplyVisibility();
                }
            }
        }
        private void UpdateOperator()
        {
            var selected = comboBoxMap["OperatorKey"].SelectedItem;
            if (Enum.TryParse(selected, out OperatorBuySell_Data op) && op != Outside.ResultParams.OperatorBuySell_Input)
            {
                Outside.ResultParams.OperatorBuySell_Input = op;
                RecalculateOutsideWithMsg();
            }
        }

        // ==== Misc ====
        private void UpdateDrawAtZoom()
        {
            if (int.TryParse(textInputMap["DrawAtKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.MiscParams.DrawAtZoom_Value)
                {
                    Outside.MiscParams.DrawAtZoom_Value = value;
                }
            }
        }
        private void UpdateSegmentsInterval()
        {
            var selected = comboBoxMap["SegmentsKey"].SelectedItem;
            if (Enum.TryParse(selected, out SegmentsInterval_Data segmentsType) && segmentsType != Outside.MiscParams.SegmentsInterval_Input)
            {
                Outside.MiscParams.SegmentsInterval_Input = segmentsType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateODFInterval()
        {
            var selected = comboBoxMap["ODFIntervalKey"].SelectedItem;
            if (Enum.TryParse(selected, out ODFInterval_Data intervalType) && intervalType != Outside.MiscParams.ODFInterval_Input)
            {
                Outside.MiscParams.ODFInterval_Input = intervalType;
                RecalculateOutsideWithMsg();
            }
        }

        private void RecalculateOutsideWithMsg(bool reset = true)
        {
            // Avoid multiples calls when loading parameters from LocalStorage
            if (isLoadingParams)
                return;

            if (_pendingIntensityNDays > 0)
            {
                Outside.GeneralParams.IntensityNDays_Input = _pendingIntensityNDays;
                _pendingIntensityNDays = 0;
            }

            string current = ModeBtn.Text;
            ModeBtn.Text = $"{current}\nCalculating...";
            Outside.BeginInvokeOnMainThread(() => {
                try { _progressBar.IsIndeterminate = true; } catch { }
            });

            if (reset) {
                Outside.BeginInvokeOnMainThread(() =>
                {
                    Outside.Chart.RemoveAllObjects();
                    Outside.Chart.ResetBarColors();
                    Outside.ResetFixedRange_Dicts();
                });
            }

            Outside.BeginInvokeOnMainThread(() =>
            {
                Outside.ClearAndRecalculate();
                ModeBtn.Text = current;
            });

            // Slow down a bit, avoid crash.
            Thread.Sleep(200);

            Outside.BeginInvokeOnMainThread(() => {
                try { _progressBar.IsIndeterminate = false; } catch { }
            });

            // Update UI every OnChange()
            RefreshVisibility();
            // Highlight any modified/unsaved parameter
            // The reset of _originalValues only happens in Load/Save methods
            RefreshHighlighting();
        }

        private void NextModeEvent(ButtonClickEventArgs e)
        {
            PopupNotification  cleaningProgress = Outside.Notifications.ShowPopup(
                Outside.NOTIFY_CAPTION,
                "Cleaning up the chart...",
                PopupNotificationState.InProgress
            );

            Outside.GeneralParams.VolumeMode_Input = Outside.GeneralParams.VolumeMode_Input switch
            {
                VolumeMode_Data.Normal => VolumeMode_Data.Buy_Sell,
                VolumeMode_Data.Buy_Sell => VolumeMode_Data.Delta,
                _ => VolumeMode_Data.Normal
            };
            ModeBtn.Text = Outside.GeneralParams.VolumeMode_Input.ToString();
            RefreshVisibility();
            RecalculateOutsideWithMsg();

            cleaningProgress.Complete(PopupNotificationState.Success);
        }

        private void PrevModeEvent(ButtonClickEventArgs e)
        {
            PopupNotification  cleaningProgress = Outside.Notifications.ShowPopup(
                Outside.NOTIFY_CAPTION,
                "Cleaning up the chart...",
                PopupNotificationState.InProgress
            );

            Outside.GeneralParams.VolumeMode_Input = Outside.GeneralParams.VolumeMode_Input switch
            {
                VolumeMode_Data.Delta => VolumeMode_Data.Buy_Sell,
                VolumeMode_Data.Buy_Sell => VolumeMode_Data.Normal,
                _ => VolumeMode_Data.Delta
            };
            ModeBtn.Text = Outside.GeneralParams.VolumeMode_Input.ToString();
            RefreshVisibility();
            RecalculateOutsideWithMsg();

            cleaningProgress.Complete(PopupNotificationState.Success);
        }
        private void RefreshVisibility()
        {
            foreach (var param in _paramDefinitions)
            {
                bool isVisible = param.IsVisible();
                switch (param.InputType)
                {
                    case ParamInputType.Text:
                        textInputMap[param.Key].IsVisible = isVisible;
                        textInputLabelMap[param.Key].IsVisible = isVisible;
                        break;
                    case ParamInputType.ComboBox:
                        comboBoxMap[param.Key].IsVisible = isVisible;
                        comboBoxTextMap[param.Key].IsVisible = isVisible;
                        break;
                    case ParamInputType.Checkbox:
                        checkBoxMap[param.Key].IsVisible = isVisible;
                        checkBoxTextMap[param.Key].IsVisible = isVisible;
                        break;
                }
            }

            // Hide regions if all params are invisible
            foreach (var section in _regionSections.Values)
            {
                bool anyVisible = section.Params.Any(p =>
                {
                    return p.InputType switch
                    {
                        ParamInputType.Text => textInputMap[p.Key].IsVisible || textInputLabelMap[p.Key].IsVisible,
                        ParamInputType.ComboBox => comboBoxMap[p.Key].IsVisible || comboBoxTextMap[p.Key].IsVisible,
                        ParamInputType.Checkbox => checkBoxMap[p.Key].IsVisible || checkBoxTextMap[p.Key].IsVisible,
                        _ => false
                    };
                });

                section.SetVisible(anyVisible);
            }

            // Manually hidden Apply Button
            ApplyBtn.IsVisible = false;
            RangeBtn.IsVisible = Outside.ProfileParams.EnableFixedRange;
        }

        private void RefreshHighlighting()
        {
            bool anyChange = false;
            foreach (var param in _paramDefinitions)
            {
                object currentValue = param.InputType switch
                {
                    ParamInputType.Text => (object)textInputMap[param.Key].Text,
                    ParamInputType.Checkbox => (object)(checkBoxMap[param.Key].IsChecked ?? false),
                    ParamInputType.ComboBox => (object)comboBoxMap[param.Key].SelectedItem,
                    _ => null
                };

                // Save original value if not already saved
                if (!_originalValues.ContainsKey(param.Key))
                    _originalValues[param.Key] = currentValue;

                bool isChanged = !Equals(currentValue, _originalValues[param.Key]);
                if (!anyChange && isChanged)
                    anyChange = isChanged;

                Color darkColorButton = Styles.ColorDarkTheme_PanelBorder;
                Color darkColor = Styles.ColorDarkTheme_Input;
                Color darkHover = Styles.ColorDarkTheme_ButtonHover;

                Color whiteColor = Styles.ColorLightTheme_Input;
                Color whiteHover = Styles.ColorLightTheme_InputHover;

                Color backgroundThemeColor = ApplicationTheme == ColorTheme.Dark ? darkColor : whiteColor;
                Color highlightThemeColor = ApplicationTheme == ColorTheme.Dark ? darkHover : whiteHover;

                SaveBtn.BackgroundColor = anyChange ? Color.FromHex("#D4D6262A") : (backgroundThemeColor == darkColor ? darkColorButton : whiteColor);
                FontStyle fontStyle = isChanged ? FontStyle.Oblique : FontStyle.Normal;

                switch (param.InputType)
                {
                    case ParamInputType.Text:
                        textInputMap[param.Key].BackgroundColor = isChanged ? highlightThemeColor : backgroundThemeColor;
                        break;
                    case ParamInputType.Checkbox:
                        if (param.Key == "WeeklyVPKey")
                            checkBoxTextMap[param.Key].ForegroundColor = Color.Crimson;
                        if (param.Key == "MonthlyVPKey")
                            checkBoxTextMap[param.Key].ForegroundColor = Color.Crimson;
                        checkBoxTextMap[param.Key].FontStyle = fontStyle;
                        break;
                    case ParamInputType.ComboBox:
                        if (param.Key == "SegmentsKey")
                            comboBoxTextMap[param.Key].ForegroundColor = Color.Crimson;
                        if (param.Key == "ODFIntervalKey")
                            comboBoxTextMap[param.Key].ForegroundColor = Color.Crimson;
                        comboBoxTextMap[param.Key].FontStyle = fontStyle;
                        comboBoxMap[param.Key].FontStyle = fontStyle;
                        break;
                }
            }
        }

        public class ParamStorage
        {
            public Dictionary<string, object> Values { get; set; } = new();
        }


        private async void AnimateProgressBar()
        {
            for (int i = 0; i <= 150; i += 25)
            {
                Outside.BeginInvokeOnMainThread(() => _progressBar.Value = i);
                await Task.Delay(100);
            }

            await Task.Delay(700);

            Outside.BeginInvokeOnMainThread(() => _progressBar.Value = 0);
        }

        private string GetStorageKey()
        {
            string SymbolPrefix = Outside.SymbolName;
            string BrokerPrefix = Outside.Account.BrokerName;
            string TimeframePrefix = Outside.TimeFrame.ShortName;

            BrokerPrefix = BrokerPrefix.ToLowerInvariant();
            SymbolPrefix = SymbolPrefix.ToUpperInvariant();

            bool selectbyBroker = Outside.StorageKeyConfig_Input == StorageKeyConfig_Data.Broker_Symbol_Timeframe;
            return selectbyBroker
                ? $"ODFT-AGG {BrokerPrefix} {SymbolPrefix} {TimeframePrefix}"
                : $"ODFT-AGG {SymbolPrefix} {TimeframePrefix}";
        }

        private class ParamStorageModel
        {
            public Dictionary<string, object> Params { get; set; } = new();
        }

        private void SaveParams()
        {
            var storageModel = new ParamStorageModel();

            foreach (var param in _paramDefinitions)
            {
                object value = param.InputType switch
                {
                    ParamInputType.Text => textInputMap[param.Key].Text,
                    ParamInputType.Checkbox => checkBoxMap[param.Key].IsChecked ?? false,
                    ParamInputType.ComboBox => comboBoxMap[param.Key].SelectedItem,
                    _ => null
                };

                if (value != null)
                    storageModel.Params[param.Key] = value;

                // Reset highlighting tracking
                _originalValues[param.Key] = value;
            }

            // Save current volume mode to start from there later.
            storageModel.Params["PanelMode"] = Outside.GeneralParams.VolumeMode_Input;

            Outside.LocalStorage.SetObject(GetStorageKey(), storageModel, LocalStorageScope.Device);
            Outside.LocalStorage.Flush(LocalStorageScope.Device);

            // Use loaded params as _originalValues
            RefreshHighlighting();
            // Some fancy fake progress
            AnimateProgressBar();
        }

        private void LoadParams()
        {
            isLoadingParams = true;

            Outside.LocalStorage.Reload(LocalStorageScope.Device);
            var storageModel = Outside.LocalStorage.GetObject<ParamStorageModel>(GetStorageKey(), LocalStorageScope.Device);

            if (storageModel == null) {
                // Add keys and use default parameters as _originalValues;
                RefreshHighlighting();
                isLoadingParams = false;
                return;
            }

            foreach (var param in _paramDefinitions)
            {
                if (!storageModel.Params.TryGetValue(param.Key, out var storedValue))
                    continue;

                switch (param.InputType)
                {
                    case ParamInputType.Text:
                        textInputMap[param.Key].Text = storedValue.ToString();
                        if (param.Key == "RowHeightKey") {
                            if (Outside.ReplaceByATR && Outside.RowConfig_Input == RowConfig_Data.ATR) {
                                textInputMap[param.Key].Text = Outside.heightATR.ToString();
                            }
                        }
                        param.OnChanged?.Invoke(param.Key);
                        break;
                    case ParamInputType.Checkbox:
                        if (storedValue is bool b)
                            checkBoxMap[param.Key].IsChecked = b;
                        param.OnChanged?.Invoke(param.Key);
                        break;
                    case ParamInputType.ComboBox:
                        if (comboBoxMap.ContainsKey(param.Key))
                            comboBoxMap[param.Key].SelectedItem = storedValue.ToString();
                        param.OnChanged?.Invoke(param.Key);
                        break;
                }

                // Reset highlighting tracking
                _originalValues[param.Key] = storedValue;
            }

            // Load the previously saved volume mode.
            string volModeText = storageModel.Params["PanelMode"].ToString();
            _ = Enum.TryParse(volModeText, out VolumeMode_Data volMode);
            Outside.GeneralParams.VolumeMode_Input = volMode;
            ModeBtn.Text = volModeText;

            // Use loaded params as _originalValues
            RefreshHighlighting();

            isLoadingParams = false;
        }

        public class RegionSection
        {
            public string Name { get; }
            public StackPanel Container { get; }
            public ControlBase Header { get; }
            public List<ParamDefinition> Params { get; }

            private bool _isExpanded = false;
            // Fix MacOS => MissingMethodException <cAlgo.API.Panel.get_Children()>
            private readonly List<ControlBase> _panelChildren = new();

            public RegionSection(string name, IEnumerable<ParamDefinition> parameters)
            {
                Name = name;
                Params = parameters.ToList();

                Container = new StackPanel { Margin = "0 0 0 10" };

                // Only expand General region by default
                _isExpanded = name == "General";

                Header = CreateToggleHeader(name);
                Container.AddChild(Header);
            }

            private ControlBase CreateToggleHeader(string text)
            {
                var btn = new Button
                {
                    Text = (_isExpanded ? "▼ " : "► ") + text, // ▼ expanded / ► collapsed
                    Padding = 0,
                    // Width = 200,
                    Width = 250, // ParamsPanel Width
                    Height = 25,
                    Margin = "0 10 0 0",
                    Style = Styles.CreateButtonStyle(),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                btn.Click += _ => ToggleExpandCollapse(btn);
                return btn;
            }

            private void ToggleExpandCollapse(Button btn)
            {
                _isExpanded = !_isExpanded;
                btn.Text = (_isExpanded ? "▼ " : "► ") + Name;

                foreach (var child in _panelChildren)
                    child.IsVisible = _isExpanded;
            }

            public void AddParamControl(ControlBase control)
            {
                control.IsVisible = _isExpanded;
                Container.AddChild(control);
                _panelChildren.Add(control);
            }

            public void SetVisible(bool visible)
            {
                Container.IsVisible = visible;
            }
        }
    }
}
