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
    // ================ PARAMS PANEL ================
    /*
    What I've done since bringing it from ODF Aggregated, by order:
        Remove all unrelated Volume Profile inputs
        Add remaining VP settings
        Reogarnize inputs
        Add "VA + POC"
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
    public enum Supported_Sources {
        m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m15, m30, m45, h1, h2, h3, h4, h6, h8, h12, D1, D2, D3
    }

    public class ParamsPanel : CustomControl
    {
        private readonly FreeVolumeProfileV20 Outside;
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

        public ParamsPanel(FreeVolumeProfileV20 indicator, IndicatorParams defaultParams)
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
            bool isWeekly() => Outside.GeneralParams.VolumeMode_Input != VolumeMode_Data.Buy_Sell && Outside.GeneralParams.VPInterval_Input != VPInterval_Data.Weekly;
            bool isMonthly() => Outside.GeneralParams.VolumeMode_Input != VolumeMode_Data.Buy_Sell && Outside.GeneralParams.VPInterval_Input != VPInterval_Data.Monthly;
            
            bool isOnlySubt() => Outside.GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta && Outside.ResultParams.ShowMinMaxDelta && Outside.ResultParams.ShowResults;
            bool isOperator() => Outside.GeneralParams.VolumeMode_Input == VolumeMode_Data.Buy_Sell && Outside.ResultParams.ShowResults;

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

            return new List<ParamDefinition>
            {
                new()
                {
                    Region = "General",
                    RegionOrder = 1,
                    Key = "LookbackKey",
                    Label = "Lookback",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.GeneralParams.Lookback,
                    OnChanged = _ => UpdateLookback()
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
                    Key = "VPIntervalKey",
                    Label = "Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.GeneralParams.VPInterval_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(VPInterval_Data)),
                    OnChanged = _ => UpdateVPInterval(),
                },

                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "EnableVPKey",
                    Label = "Main Profile?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableMainVP,
                    OnChanged = _ => UpdateCheckbox("EnableVPKey", val => Outside.ProfileParams.EnableMainVP = val),
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
                    IsVisible = () => isWeekly()
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
                    IsVisible = () => isMonthly()
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
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "IntraOffsetKey",
                    Label = "Offset(bars)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ProfileParams.OffsetBarsInput.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateIntradayOffset(),
                    IsVisible = () => Outside.ProfileParams.ShowIntradayProfile
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
                    IsVisible = () => Outside.ProfileParams.ShowIntradayProfile && Outside.isPriceBased_Chart
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
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "FixedSegmentsKey",
                    Label = "Segments",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.SegmentsFixedRange_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(SegmentsFixedRange_Data)),
                    OnChanged = _ => UpdateRangeSegments(),
                    IsVisible = () => Outside.ProfileParams.EnableFixedRange
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "ShowOHLCKey",
                    Label = "OHLC Body?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.ShowOHLC,
                    OnChanged = _ => UpdateCheckbox("ShowOHLCKey", val => Outside.ProfileParams.ShowOHLC = val),
                },
                new()
                {
                    Region = "Volume Profile",
                    RegionOrder = 2,
                    Key = "GradientKey",
                    Label = "Gradient?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableGradient,
                    OnChanged = _ => UpdateCheckbox("GradientKey", val => Outside.ProfileParams.EnableGradient = val),
                    IsVisible = () => Outside.GeneralParams.VolumeMode_Input == VolumeMode_Data.Normal
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
                    IsVisible = () => Outside.ProfileParams.ShowIntradayProfile && (Outside.ProfileParams.EnableWeeklyProfile || Outside.ProfileParams.EnableMonthlyProfile)
                },

                new()
                {
                    Region = "Mini VPs",
                    RegionOrder = 3,
                    Key = "MiniVPsKey",
                    Label = "Enable?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.EnableMiniProfiles,
                    OnChanged = _ => UpdateCheckbox("MiniVPsKey", val => Outside.ProfileParams.EnableMiniProfiles = val)
                },
                new()
                {
                    Region = "Mini VPs",
                    RegionOrder = 3,
                    Key = "MiniTFKey",
                    Label = "Mini-Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.MiniVPs_Timeframe.ShortName.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Supported_Timeframes)),
                    OnChanged = _ => UpdateMiniVPTimeframe()
                },
                new()
                {
                    Region = "Mini VPs",
                    RegionOrder = 3,
                    Key = "MiniResultKey",
                    Label = "Mini-Result?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ProfileParams.ShowMiniResults,
                    OnChanged = _ => UpdateCheckbox("MiniResultKey", val => Outside.ProfileParams.ShowMiniResults = val)
                },

                new()
                {
                    Region = "VA + POC",
                    RegionOrder = 4,
                    Key = "EnableVAKey",
                    Label = "Enable VA?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.VAParams.ShowVA,
                    OnChanged = _ => UpdateCheckbox("EnableVAKey", val => Outside.VAParams.ShowVA = val)
                },
                new()
                {
                    Region = "VA + POC",
                    RegionOrder = 4,
                    Key = "VAValueKey",
                    Label = "VA(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.VAParams.PercentVA.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdatePercentVA(),
                    IsVisible = () => Outside.VAParams.ShowVA
                },
                new()
                {
                    Region = "VA + POC",
                    RegionOrder = 4,
                    Key = "OnlyPOCKey",
                    Label = "Only POC?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.VAParams.KeepPOC,
                    OnChanged = _ => UpdateCheckbox("OnlyPOCKey", val => Outside.VAParams.KeepPOC = val)
                },
                new()
                {
                    Region = "VA + POC",
                    RegionOrder = 4,
                    Key = "ExtendVAKey",
                    Label = "Extend VA?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.VAParams.ExtendVA,
                    OnChanged = _ => UpdateCheckbox("ExtendVAKey", val => Outside.VAParams.ExtendVA = val)
                },
                new()
                {
                    Region = "VA + POC",
                    RegionOrder = 4,
                    Key = "ExtendCountKey",
                    Label = "Extend(count))",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.VAParams.ExtendCount.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateExtendCount(),
                    IsVisible = () => Outside.VAParams.ExtendVA || Outside.VAParams.ExtendPOC
                },
                new()
                {
                    Region = "VA + POC",
                    RegionOrder = 4,
                    Key = "ExtendPOCKey",
                    Label = "Extend POC?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.VAParams.ExtendPOC,
                    OnChanged = _ => UpdateCheckbox("ExtendPOCKey", val => Outside.VAParams.ExtendPOC = val)
                },

                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "EnableNodeKey",
                    Label = "Enable?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.EnableNodeDetection,
                    OnChanged = _ => UpdateCheckbox("EnableNodeKey", val => Outside.NodesParams.EnableNodeDetection = val)
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "NodeSmoothKey",
                    Label = "Smooth",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.NodesParams.ProfileSmooth_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ProfileSmooth_Data)),
                    OnChanged = _ => UpdateNodeSmooth()
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "NodeTypeKey",
                    Label = "Nodes",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.NodesParams.ProfileNode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ProfileNode_Data)),
                    OnChanged = _ => UpdateNodeType()
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "ShowNodeKey",
                    Label = "Show",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.NodesParams.ShowNode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ShowNode_Data)),
                    OnChanged = _ => UpdateShowNode(),
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "HvnBandPctKey",
                    Label = "HVN Band(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.bandHVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHVN_Band(),
                    IsVisible = () => isNodeBand()
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "LvnBandPctKey",
                    Label = "LVN Band(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.bandLVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLVN_Band(),
                    IsVisible = () => isNodeBand()
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "NodeStrongKey",
                    Label = "Only Strong?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.onlyStrongNodes, 
                    OnChanged = _ => UpdateCheckbox("NodeStrongKey", val => Outside.NodesParams.onlyStrongNodes = val)
                },
                // 'Strong HVN' for HVN_Raw(only) on [LocalMinMax, Topology]
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "StrongHvnPctKey",
                    Label = "(%) >= POC",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.strongHVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHVN_Strong(),
                    IsVisible = () => Outside.NodesParams.onlyStrongNodes && isStrongHVN()
                },
                // 'Strong LVN' should be used by HVN_With_Bands, since the POCs are derived from LVN Split.
                // on [LocalMinMax, Topology] 
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "StrongLvnPctKey",
                    Label = "(%) <= POC",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.strongLVN_Pct.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLVN_Strong(),
                    IsVisible = () => Outside.NodesParams.onlyStrongNodes && isStrongLVN()
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "ExtendNodeKey",
                    Label = "Extend?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.extendNodes,
                    OnChanged = _ => UpdateCheckbox("ExtendNodeKey", val => Outside.NodesParams.extendNodes = val)
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "ExtNodesCountKey",
                    Label = "Extend(count)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.extendNodes_Count.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateExtendNodesCount(),
                    IsVisible = () => Outside.NodesParams.extendNodes
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "ExtBandsKey",
                    Label = "Ext.(bands)?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.extendNodes_WithBands,
                    OnChanged = _ => UpdateCheckbox("ExtBandsKey", val => Outside.NodesParams.extendNodes_WithBands = val),
                    IsVisible = () => Outside.NodesParams.extendNodes
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "HvnPctileKey",
                    Label = "HVN(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.pctileHVN_Value.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHVN_Pctile(),
                    IsVisible = () => Outside.NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "LvnPctileKey",
                    Label = "LVN(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.NodesParams.pctileLVN_Value.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLVN_Pctile(),
                    IsVisible = () => Outside.NodesParams.ProfileNode_Input == ProfileNode_Data.Percentile
                },
                new()
                {
                    Region = "HVN + LVN",
                    RegionOrder = 5,
                    Key = "ExtNodeStartKey",
                    Label = "From start?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.NodesParams.extendNodes_FromStart,
                    OnChanged = _ => UpdateCheckbox("ExtNodeStartKey", val => Outside.NodesParams.extendNodes_FromStart = val),
                    IsVisible = () => Outside.NodesParams.extendNodes
                },

                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "UpdateVPKey",
                    Label = "Update At",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.UpdateProfile_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(UpdateProfile_Data)),
                    OnChanged = _ => UpdateVP(),
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "SourceVPKey",
                    Label = "Source",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => "m1",
                    EnumOptions = () => Enum.GetNames(typeof(Supported_Sources)),
                    OnChanged = _ => UpdateSourceVP(),
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "DistributionKey",
                    Label = "Distribution",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ProfileParams.Distribution_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Distribution_Data)),
                    OnChanged = _ => UpdateDistribution(),
                },

                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "ShowResultsKey",
                    Label = "Results?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowResults,
                    OnChanged = _ => UpdateCheckbox("ShowResultsKey", val => Outside.ResultParams.ShowResults = val),
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "ShowMinMaxKey",
                    Label = "Min/Max?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowMinMaxDelta,
                    OnChanged = _ => UpdateCheckbox("ShowMinMaxKey", val => Outside.ResultParams.ShowMinMaxDelta = val),
                    IsVisible = () => Outside.GeneralParams.VolumeMode_Input == VolumeMode_Data.Delta && Outside.ResultParams.ShowResults
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "OnlySubtKey",
                    Label = "Only Subtract?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ResultParams.ShowOnlySubtDelta,
                    OnChanged = _ => UpdateCheckbox("OnlySubtKey", val => Outside.ResultParams.ShowOnlySubtDelta = val),
                    IsVisible = () => isOnlySubt()
                },
                new()
                {
                    Region = "Misc",
                    RegionOrder = 6,
                    Key = "OperatorKey",
                    Label = "Operator",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ResultParams.OperatorBuySell_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(OperatorBuySell_Data)),
                    OnChanged = _ => UpdateOperator(),
                    IsVisible = () => isOperator()
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
                Text = "Free Volume Profile",
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
                var groupGrid = new Grid(6, 5);
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

        private void UpdateCheckbox(string key, Action<bool> applyAction)
        {
            bool value = checkBoxMap[key].IsChecked ?? false;
            applyAction(value);
            CheckboxHandler(key, value);
        }
        private void CheckboxHandler(string key, bool value)
        {
            switch (key) {
                case "IntradayVPKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "FillIntraVPKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "GradientKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "ExtendVAKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "ExtendPOCKey":
                    RecalculateOutsideWithMsg(false);
                    return;
                case "FixedRangeKey":
                    RangeBtn.IsVisible = value;
                    RefreshVisibility();
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
        private void UpdateLookback()
        {
            int value = int.TryParse(textInputMap["LookbackKey"].Text, out var n) ? n : -2;
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
        private void UpdateVPInterval()
        {
            var selected = comboBoxMap["VPIntervalKey"].SelectedItem;
            if (Enum.TryParse(selected, out VPInterval_Data intervalType) && intervalType != Outside.GeneralParams.VPInterval_Input)
            {
                Outside.GeneralParams.VPInterval_Input = intervalType;
                Outside.LoadMoreHistory_IfNeeded();
                RecalculateOutsideWithMsg();
            }
        }

        // ==== Volume Profile ====
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
        private void UpdateRangeSegments() {
            var selected = comboBoxMap["FixedSegmentsKey"].SelectedItem;
            if (Enum.TryParse(selected, out SegmentsFixedRange_Data segmentsType) && segmentsType != Outside.ProfileParams.SegmentsFixedRange_Input)
            {
                Outside.ProfileParams.SegmentsFixedRange_Input = segmentsType;
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
                case "m1": return TimeFrame.Minute;
                case "m2": return TimeFrame.Minute2;
                case "m3": return TimeFrame.Minute3;
                case "m4": return TimeFrame.Minute4;
                case "m5": return TimeFrame.Minute5;
                case "m6": return TimeFrame.Minute6;
                case "m7": return TimeFrame.Minute7;
                case "m8": return TimeFrame.Minute8;
                case "m9": return TimeFrame.Minute9;
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

        // ==== POC + VA ====
        private void UpdatePercentVA()
        {
            int value = int.TryParse(textInputMap["VAValueKey"].Text, out var n) ? n : -1;
            if (value > 0 && value <= 100 && value != Outside.VAParams.PercentVA)
            {
                Outside.VAParams.PercentVA = value;
                SetApplyVisibility();
            }
        }
        private void UpdateExtendCount()
        {
            int value = int.TryParse(textInputMap["ExtendCountKey"].Text, out var n) ? n : -1;
            if (value > 0 && value != Outside.VAParams.ExtendCount)
            {
                Outside.VAParams.ExtendCount = value;
                RecalculateOutsideWithMsg(false);
            }
        }

        // ==== Results ====
        private void UpdateOperator()
        {
            var selected = comboBoxMap["OperatorKey"].SelectedItem;
            if (Enum.TryParse(selected, out OperatorBuySell_Data op) && op != Outside.ResultParams.OperatorBuySell_Input)
            {
                Outside.ResultParams.OperatorBuySell_Input = op;
                RecalculateOutsideWithMsg(false);
            }
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

        // ==== Misc ====
        private void UpdateVP()
        {
            var selected = comboBoxMap["UpdateVPKey"].SelectedItem;
            if (Enum.TryParse(selected, out UpdateProfile_Data updateType) && updateType != Outside.ProfileParams.UpdateProfile_Input)
            {
                Outside.ProfileParams.UpdateProfile_Input = updateType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateSourceVP()
        {
            var selected = comboBoxMap["SourceVPKey"].SelectedItem;
            TimeFrame value = StringToTimeframe(selected);
            if (value != Outside.ProfileParams.Source_Timeframe)
            {
                Outside.ProfileParams.Source_Timeframe = value;
                Outside.SetVPBars();
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateDistribution()
        {
            var selected = comboBoxMap["DistributionKey"].SelectedItem;
            if (Enum.TryParse(selected, out Distribution_Data distributionType) && distributionType != Outside.ProfileParams.Distribution_Input)
            {
                Outside.ProfileParams.Distribution_Input = distributionType;
                RecalculateOutsideWithMsg(false);
            }
        }

        private void RecalculateOutsideWithMsg(bool reset = true)
        {
            // Avoid multiples calls when loading parameters from LocalStorage
            if (isLoadingParams)
                return;

            string current = ModeBtn.Text;
            ModeBtn.Text = $"{current}\nCalculating...";
            Outside.BeginInvokeOnMainThread(() => {
                try { _progressBar.IsIndeterminate = true; } catch { }
            });

            if (reset) {
                Outside.BeginInvokeOnMainThread(() =>
                {
                    Outside.Chart.RemoveAllObjects();
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
                        checkBoxTextMap[param.Key].FontStyle = fontStyle;
                        break;
                    case ParamInputType.ComboBox:
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
                ? $"VP {BrokerPrefix} {SymbolPrefix} {TimeframePrefix}"
                : $"VP {SymbolPrefix} {TimeframePrefix}";
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
