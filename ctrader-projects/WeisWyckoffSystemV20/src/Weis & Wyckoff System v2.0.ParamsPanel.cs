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
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace cAlgo
{
    public class ParamsPanel : CustomControl
    {
        private readonly WeisWyckoffSystemV20 Outside;
        private readonly IndicatorParams FirstParams;
        private Button ModeBtn;
        private Button SaveBtn;
        private Button ApplyBtn;
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

        public ParamsPanel(WeisWyckoffSystemV20 indicator, IndicatorParams defaultParams)
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
            bool isPctile() => Outside.ColoringParams.StrengthRatio_Input == StrengthRatio_Data.Percentile && Outside.ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized;
            bool isFixed() => Outside.ColoringParams.StrengthRatio_Input == StrengthRatio_Data.Fixed && Outside.ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized;
            bool isPct() => Outside.ColoringParams.StrengthFilter_Input == StrengthFilter_Data.Normalized_Emphasized;

            bool isNot_NmlzPct() => Outside.ColoringParams.StrengthFilter_Input != StrengthFilter_Data.Normalized_Emphasized;
            bool isMTF() => Outside.ZigZagParams.ZigZagSource_Input == ZigZagSource_Data.MultiTF;

            return new List<ParamDefinition>
            {
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "EnableWyckoffKey",
                    Label = "Enable?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.WyckoffParams.EnableWyckoff,
                    OnChanged = _ => UpdateCheckbox("EnableWyckoffKey", val => Outside.WyckoffParams.EnableWyckoff = val),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "ShowNumbersKey",
                    Label = "Numbers",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WyckoffParams.Numbers_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Numbers_Data)),
                    OnChanged = _ => UpdateNumbers(),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "NumbersPositionKey",
                    Label = "Position",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WyckoffParams.NumbersPosition_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(NumbersPosition_Data)),
                    OnChanged = _ => UpdateNumbersPosition(),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "NumbersColorKey",
                    Label = "Coloring[nº]",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WyckoffParams.NumbersColor_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(NumbersColor_Data)),
                    OnChanged = _ => UpdateNumbersColoring(),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "BarsColorKey",
                    Label = "Coloring[bars]",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WyckoffParams.BarsColor_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(BarsColor_Data)),
                    OnChanged = _ => UpdateBarsColoring(),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "BothPositionKey",
                    Label = "Position[Both]",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WyckoffParams.NumbersBothPosition_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(NumbersBothPosition_Data)),
                    OnChanged = _ => UpdateNumbersBothPosition(),
                    IsVisible = () => Outside.WyckoffParams.Numbers_Input == Numbers_Data.Both
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "FillBarsKey",
                    Label = "Fill?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.WyckoffParams.FillBars,
                    OnChanged = _ => UpdateCheckbox("FillBarsKey", val => Outside.WyckoffParams.FillBars = val),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "OutlineKey",
                    Label = "Outline?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.WyckoffParams.KeepOutline,
                    OnChanged = _ => UpdateCheckbox("OutlineKey", val => Outside.WyckoffParams.KeepOutline = val),
                },
                new()
                {
                    Region = "Wyckoff Bars",
                    RegionOrder = 0,
                    Key = "NumbersLargeKey",
                    Label = "Only Avg[nº]?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.WyckoffParams.ShowOnlyLargeNumbers,
                    OnChanged = _ => UpdateCheckbox("NumbersLargeKey", val => Outside.WyckoffParams.ShowOnlyLargeNumbers = val),
                },

                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "StrengthFilterKey",
                    Label = "Filter",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ColoringParams.StrengthFilter_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(StrengthFilter_Data)),
                    OnChanged = _ => UpdateStrengthFilter(),
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "MAPeriodKey",
                    Label = "Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.MAperiod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateMAPeriod(),
                    IsVisible = () => isNot_NmlzPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "MATypeKey",
                    Label = "MA Type",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Outside.UseCustomMAs ? Outside.customMAtype.ToString() : p.ColoringParams.MAtype.ToString(),
                    EnumOptions = () => Outside.UseCustomMAs ? Enum.GetNames(typeof(MAType_Data)) : Enum.GetNames(typeof(MovingAverageType)),
                    OnChanged = _ => UpdateMAType(),
                    IsVisible = () => Outside.ColoringParams.StrengthFilter_Input != StrengthFilter_Data.L1Norm && isNot_NmlzPct()
                },
                
                // Ratio
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "StrengthRatioKey",
                    Label = "Ratio",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ColoringParams.StrengthRatio_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(StrengthRatio_Data)),
                    OnChanged = _ => UpdateStrengthRatio(),
                    IsVisible = () => isNot_NmlzPct()
                },
                // Ratio => Percentile => Period
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "PctilePeriodKey",
                    Label = "Pctile Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Pctile_Period.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdatePercentilePeriod(),
                    IsVisible = () => isPctile()
                },
                // [Debug] Show Strength
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "DebugStrengthKey",
                    Label = "Debug?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ColoringParams.ShowStrengthValue,
                    OnChanged = _ => UpdateCheckbox("DebugStrengthKey", val => Outside.ColoringParams.ShowStrengthValue = val),
                },

                // Percentile
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "LowestPctileKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Lowest_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowest_Pctile(),
                    IsVisible = () => isPctile()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "LowPctileKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Low_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLow_Pctile(),
                    IsVisible = () => isPctile()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "AveragePctileKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Average_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverage_Pctile(),
                    IsVisible = () => isPctile()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "HighPctileKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.High_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHigh_Pctile(),
                    IsVisible = () => isPctile()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "UltraPctileKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Ultra_PctileValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltra_Pctile(),
                    IsVisible = () => isPctile()
                },

                // Percentage => Normalized_Emphasized
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "NmlzPeriodKey",
                    Label = "Period",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.NormalizePeriod.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateNormalizePeriod(),
                    IsVisible = () => isPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "LowestPctKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Lowest_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowest_Pct(),
                    IsVisible = () => isPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "LowPctKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Low_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLow_Pct(),
                    IsVisible = () => isPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "AveragePctKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Average_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverage_Pct(),
                    IsVisible = () => isPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "HighPctKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.High_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHigh_Pct(),
                    IsVisible = () => isPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "UltraPctKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Ultra_PctValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltra_Pct(),
                    IsVisible = () => isPct()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "NmlzMultipKey",
                    Label = "Multiplier",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.NormalizeMultiplier.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateNormalizeMultiplier(),
                    IsVisible = () => isPct()
                },

                // Fixed
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "LowestFixedKey",
                    Label = "Lowest(<)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Lowest_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLowest_Fixed(),
                    IsVisible = () => isFixed()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "LowFixedKey",
                    Label = "Low",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Low_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateLow_Fixed(),
                    IsVisible = () => isFixed()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "AverageFixedKey",
                    Label = "Average",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Average_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateAverage_Fixed(),
                    IsVisible = () => isFixed()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "HighFixedKey",
                    Label = "High",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.High_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateHigh_Fixed(),
                    IsVisible = () => isFixed()
                },
                new()
                {
                    Region = "Coloring",
                    RegionOrder = 1,
                    Key = "UltraFixedKey",
                    Label = "Ultra(>=)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ColoringParams.Ultra_FixedValue.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateUltra_Fixed(),
                    IsVisible = () => isFixed()
                },


                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "CurrentWaveKey",
                    Label = "Current?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.WavesParams.ShowCurrentWave,
                    OnChanged = _ => UpdateCheckbox("CurrentWaveKey", val => Outside.WavesParams.ShowCurrentWave = val),
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "ShowWavesKey",
                    Label = "Waves",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WavesParams.ShowWaves_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ShowWaves_Data)),
                    OnChanged = _ => UpdateWaves(),
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "OtherWavesKey",
                    Label = "Waves(misc)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WavesParams.ShowOtherWaves_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ShowOtherWaves_Data)),
                    OnChanged = _ => UpdateOtherWaves(),
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "MarksKey",
                    Label = "Marks",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WavesParams.ShowMarks_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ShowMarks_Data)),
                    OnChanged = _ => UpdateMarks(),
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "RatioVolumeKey",
                    Label = "Ratio(volume)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.WavesParams.WW_Ratio.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateVolumeRatio()
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "RatioEvsRKey",
                    Label = "Ratio(EvsR)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.WavesParams.EvsR_Ratio.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdateEvsRRatio()
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "WavesModeKey",
                    Label = "Waves(mode)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WavesParams.WavesMode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(WavesMode_Data)),
                    OnChanged = _ => UpdateWavesMode(),
                    IsVisible = () => Outside.BooleanUtils.isRenkoChart
                },
                //
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "YellowZZKey",
                    Label = "Last(ratio)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.WavesParams.YellowZigZag_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(YellowZigZag_Data)),
                    OnChanged = _ => UpdateYellowZZ(),
                },
                new()
                {
                    Region = "Weis Waves",
                    RegionOrder = 2,
                    Key = "YellowRenkoKey",
                    Label = "Ranging?(ratio)",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.WavesParams.YellowRenko_IgnoreRanging,
                    OnChanged = _ => UpdateCheckbox("YellowRenkoKey", val => Outside.WavesParams.YellowRenko_IgnoreRanging = val),
                    IsVisible = () => Outside.WavesParams.WavesMode_Input == WavesMode_Data.Reversal
                },

                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "ZigZagModeKey",
                    Label = "Mode",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ZigZagParams.ZigZagMode_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ZigZagMode_Data)),
                    OnChanged = _ => UpdateZigZagMode()
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "PercentageZZKey",
                    Label = "Value(%)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ZigZagParams.PercentageZZ.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdatePercentage(),
                    IsVisible = () => Outside.ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.Percentage
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "PipsZZKey",
                    Label = "Value(pips)",
                    InputType = ParamInputType.Text,
                    GetDefault = p => p.ZigZagParams.PipsZZ.ToString("0.############################", CultureInfo.InvariantCulture),
                    OnChanged = _ => UpdatePips(),
                    IsVisible = () => Outside.ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.Pips
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "PriorityZZKey",
                    Label = "Priority(HH/HL)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ZigZagParams.Priority_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Priority_Data)),
                    OnChanged = _ => UpdatePriority(),
                    IsVisible = () => Outside.ZigZagParams.ZigZagMode_Input == ZigZagMode_Data.NoLag_HighLow
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "ZZSourceKey",
                    Label = "Source(TF)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ZigZagParams.ZigZagSource_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(ZigZagSource_Data)),
                    OnChanged = _ => UpdateZigZagSource(),
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "TurningPointKey",
                    Label = "Turning Point?",
                    InputType = ParamInputType.Checkbox,
                    GetDefault = p => p.ZigZagParams.ShowTurningPoint,
                    OnChanged = _ => UpdateCheckbox("TurningPointKey", val => Outside.ZigZagParams.ShowTurningPoint = val)
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "MTFSourceKey",
                    Label = "MTF(source)",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => p.ZigZagParams.MTFSource_Panel.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(MTF_Sources)),
                    OnChanged = _ => UpdateMTFSource(),
                    IsVisible = () => isMTF()
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "MTFCandlesKey",
                    Label = "Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Standard_Sources.m30.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Standard_Sources)),
                    OnChanged = _ => UpdateCandles(),
                    IsVisible = () => isMTF() && (Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Standard || Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Heikin_Ash)
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "MTFRenkoKey",
                    Label = "Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Renko_Sources.Re1.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Renko_Sources)),
                    OnChanged = _ => UpdateRenko(),
                    IsVisible = () => isMTF() && Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Renko
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "MTFRangeKey",
                    Label = "Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Range_Sources.Ra1.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Range_Sources)),
                    OnChanged = _ => UpdateRange(),
                    IsVisible = () => isMTF() && Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Range
                },
                new()
                {
                    Region = "ZigZag",
                    RegionOrder = 3,
                    Key = "MTFTicksKey",
                    Label = "Interval",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = p => Tick_Sources.t100.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(Tick_Sources)),
                    OnChanged = _ => UpdateTick(),
                    IsVisible = () => isMTF() && Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Tick
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
                Text = "Weis & Wyckoff System",
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
            grid.AddChild(CreateModeInfo_Button(FirstParams.Template.ToString()), 0, 1, 1, 3);
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
                var groupGrid = new Grid(9, 5); // Increase total rows for independent ratio: from 6 => 9
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
            CheckboxHandler(key);
        }
        private void CheckboxHandler(string key)
        {
            switch (key) {
                default:
                    break;
            }

            RecalculateOutsideWithMsg();
        }

        // ==== Wyckoff Bars ====
        private void UpdateNumbers()
        {
            var selected = comboBoxMap["ShowNumbersKey"].SelectedItem;
            if (Enum.TryParse(selected, out Numbers_Data numbersType) && numbersType != Outside.WyckoffParams.Numbers_Input)
            {
                Outside.WyckoffParams.Numbers_Input = numbersType;
                RecalculateOutsideWithMsg(numbersType == Numbers_Data.None);
            }
        }
        private void UpdateNumbersPosition()
        {
            var selected = comboBoxMap["NumbersPositionKey"].SelectedItem;
            if (Enum.TryParse(selected, out NumbersPosition_Data positionType) && positionType != Outside.WyckoffParams.NumbersPosition_Input)
            {
                Outside.WyckoffParams.NumbersPosition_Input = positionType;
                RecalculateOutsideWithMsg(false);
            }
        }

        private void UpdateNumbersBothPosition()
        {
            var selected = comboBoxMap["BothPositionKey"].SelectedItem;
            if (Enum.TryParse(selected, out NumbersBothPosition_Data positionType) && positionType != Outside.WyckoffParams.NumbersBothPosition_Input)
            {
                Outside.WyckoffParams.NumbersBothPosition_Input = positionType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateNumbersColoring()
        {
            var selected = comboBoxMap["NumbersColorKey"].SelectedItem;
            if (Enum.TryParse(selected, out NumbersColor_Data colorType) && colorType != Outside.WyckoffParams.NumbersColor_Input)
            {
                Outside.WyckoffParams.NumbersColor_Input = colorType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateBarsColoring()
        {
            var selected = comboBoxMap["BarsColorKey"].SelectedItem;
            if (Enum.TryParse(selected, out BarsColor_Data colorType) && colorType != Outside.WyckoffParams.BarsColor_Input)
            {
                Outside.WyckoffParams.BarsColor_Input = colorType;
                RecalculateOutsideWithMsg(false);
            }
        }

        // ==== Coloring ====
        private void UpdateStrengthFilter()
        {
            var selected = comboBoxMap["StrengthFilterKey"].SelectedItem;
            if (Enum.TryParse(selected, out StrengthFilter_Data filterType) && filterType != Outside.ColoringParams.StrengthFilter_Input)
            {
                Outside.ColoringParams.StrengthFilter_Input = filterType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private void UpdateMAType()
        {
            var selected = comboBoxMap["MATypeKey"].SelectedItem;
            if (Outside.UseCustomMAs) {
                if (Enum.TryParse(selected, out MAType_Data MAType) && MAType != Outside.customMAtype)
                {
                    Outside.customMAtype = MAType;
                    RecalculateOutsideWithMsg();
                }
            } else {
                if (Enum.TryParse(selected, out MovingAverageType MAType) && MAType != Outside.ColoringParams.MAtype)
                {
                    Outside.ColoringParams.MAtype = MAType;
                    RecalculateOutsideWithMsg();
                }
            }
        }
        private void UpdateMAPeriod()
        {
            if (int.TryParse(textInputMap["MAPeriodKey"].Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                if (value != Outside.ColoringParams.MAperiod)
                {
                    Outside.ColoringParams.MAperiod = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateNormalizePeriod()
        {
            if (int.TryParse(textInputMap["NmlzPeriodKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.NormalizePeriod)
                {
                    Outside.ColoringParams.NormalizePeriod = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateNormalizeMultiplier()
        {
            if (int.TryParse(textInputMap["NmlzMultipKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.NormalizeMultiplier)
                {
                    Outside.ColoringParams.NormalizeMultiplier = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }

        private void UpdatePercentilePeriod()
        {
            if (int.TryParse(textInputMap["PctilePeriodKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Pctile_Period)
                {
                    Outside.ColoringParams.Pctile_Period = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateStrengthRatio()
        {
            var selected = comboBoxMap["StrengthRatioKey"].SelectedItem;
            if (Enum.TryParse(selected, out StrengthRatio_Data ratioType) && ratioType != Outside.ColoringParams.StrengthRatio_Input)
            {
                Outside.ColoringParams.StrengthRatio_Input = ratioType;
                RecalculateOutsideWithMsg(false);
            }
        }


        // Percentile
        private void UpdateLowest_Pctile()
        {
            if (int.TryParse(textInputMap["LowestPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Lowest_PctileValue)
                {
                    Outside.ColoringParams.Lowest_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLow_Pctile()
        {
            if (int.TryParse(textInputMap["LowPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Low_PctileValue)
                {
                    Outside.ColoringParams.Low_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverage_Pctile()
        {
            if (int.TryParse(textInputMap["AveragePctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Average_PctileValue)
                {
                    Outside.ColoringParams.Average_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHigh_Pctile()
        {
            if (int.TryParse(textInputMap["HighPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.High_PctileValue)
                {
                    Outside.ColoringParams.High_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltra_Pctile()
        {
            if (int.TryParse(textInputMap["UltraPctileKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Ultra_PctileValue)
                {
                    Outside.ColoringParams.Ultra_PctileValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }

        // Percentage => Normalized_Emphasized
        private void UpdateLowest_Pct()
        {
            if (double.TryParse(textInputMap["LowestPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Lowest_PctValue)
                {
                    Outside.ColoringParams.Lowest_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLow_Pct()
        {
            if (double.TryParse(textInputMap["LowPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Low_PctValue)
                {
                    Outside.ColoringParams.Low_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverage_Pct()
        {
            if (double.TryParse(textInputMap["AveragePctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Average_PctValue)
                {
                    Outside.ColoringParams.Average_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHigh_Pct()
        {
            if (double.TryParse(textInputMap["HighPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.High_PctValue)
                {
                    Outside.ColoringParams.High_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltra_Pct()
        {
            if (double.TryParse(textInputMap["UltraPctKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Ultra_PctValue)
                {
                    Outside.ColoringParams.Ultra_PctValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }

        // Fixed
        private void UpdateLowest_Fixed()
        {
            if (double.TryParse(textInputMap["LowestFixedKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Lowest_FixedValue)
                {
                    Outside.ColoringParams.Lowest_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateLow_Fixed()
        {
            if (double.TryParse(textInputMap["LowFixedKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Low_FixedValue)
                {
                    Outside.ColoringParams.Low_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateAverage_Fixed()
        {
            if (double.TryParse(textInputMap["AverageFixedKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Average_FixedValue)
                {
                    Outside.ColoringParams.Average_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateHigh_Fixed()
        {
            if (double.TryParse(textInputMap["HighFixedKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.High_FixedValue)
                {
                    Outside.ColoringParams.High_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateUltra_Fixed()
        {
            if (double.TryParse(textInputMap["UltraFixedKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ColoringParams.Ultra_FixedValue)
                {
                    Outside.ColoringParams.Ultra_FixedValue = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }


        // ==== Weis Waves ====
        private void UpdateWaves()
        {
            var selected = comboBoxMap["ShowWavesKey"].SelectedItem;
            if (Enum.TryParse(selected, out ShowWaves_Data wavesType) && wavesType != Outside.WavesParams.ShowWaves_Input)
            {
                Outside.WavesParams.ShowWaves_Input = wavesType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateOtherWaves()
        {
            var selected = comboBoxMap["OtherWavesKey"].SelectedItem;
            if (Enum.TryParse(selected, out ShowOtherWaves_Data wavesType) && wavesType != Outside.WavesParams.ShowOtherWaves_Input)
            {
                Outside.WavesParams.ShowOtherWaves_Input = wavesType;
                RecalculateOutsideWithMsg(wavesType == ShowOtherWaves_Data.No);
            }
        }
        private void UpdateMarks()
        {
            var selected = comboBoxMap["MarksKey"].SelectedItem;
            if (Enum.TryParse(selected, out ShowMarks_Data markType) && markType != Outside.WavesParams.ShowMarks_Input)
            {
                Outside.WavesParams.ShowMarks_Input = markType;
                RecalculateOutsideWithMsg(markType == ShowMarks_Data.No);
            }
        }
        private void UpdateVolumeRatio()
        {
            if (double.TryParse(textInputMap["RatioVolumeKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.WavesParams.WW_Ratio)
                {
                    Outside.WavesParams.WW_Ratio = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateEvsRRatio()
        {
            if (double.TryParse(textInputMap["RatioEvsRKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.WavesParams.EvsR_Ratio)
                {
                    Outside.WavesParams.EvsR_Ratio = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateWavesMode()
        {
            var selected = comboBoxMap["WavesModeKey"].SelectedItem;
            if (Enum.TryParse(selected, out WavesMode_Data wavesType) && wavesType != Outside.WavesParams.WavesMode_Input)
            {
                Outside.WavesParams.WavesMode_Input = wavesType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateYellowZZ()
        {
            var selected = comboBoxMap["YellowZZKey"].SelectedItem;
            if (Enum.TryParse(selected, out YellowZigZag_Data yellowType) && yellowType != Outside.WavesParams.YellowZigZag_Input)
            {
                Outside.WavesParams.YellowZigZag_Input = yellowType;
                RecalculateOutsideWithMsg();
            }
        }

        // ==== ZigZag ====
        private void UpdateZigZagMode()
        {
            var selected = comboBoxMap["ZigZagModeKey"].SelectedItem;
            if (Enum.TryParse(selected, out ZigZagMode_Data zigzagType) && zigzagType != Outside.ZigZagParams.ZigZagMode_Input)
            {
                Outside.ZigZagParams.ZigZagMode_Input = zigzagType;
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdatePercentage()
        {
            if (double.TryParse(textInputMap["PercentageZZKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ZigZagParams.PercentageZZ)
                {
                    Outside.ZigZagParams.PercentageZZ = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdatePips()
        {
            if (double.TryParse(textInputMap["PipsZZKey"].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (value != Outside.ZigZagParams.PipsZZ)
                {
                    Outside.ZigZagParams.PipsZZ = value;
                    ApplyBtn.IsVisible = true;
                }
            }
        }
        private void UpdateZigZagSource()
        {
            var selected = comboBoxMap["ZZSourceKey"].SelectedItem;
            if (Enum.TryParse(selected, out ZigZagSource_Data sourceType) && sourceType != Outside.ZigZagParams.ZigZagSource_Input)
            {
                Outside.ZigZagParams.ZigZagSource_Input = sourceType;
                if (sourceType == ZigZagSource_Data.MultiTF) {
                    UpdateMTFSource();
                    return;
                }
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdateMTFSource()
        {
            var selected = comboBoxMap["MTFSourceKey"].SelectedItem;
            if (Enum.TryParse(selected, out MTF_Sources sourceType))
            {
                Outside.ZigZagParams.MTFSource_Panel = sourceType;
                switch (sourceType)
                {
                    case MTF_Sources.Tick:
                        UpdateTick(); return;
                    case MTF_Sources.Renko:
                        UpdateRenko(); return;
                    case MTF_Sources.Range:
                        UpdateRange(); return;
                    default:
                        UpdateCandles(); return;
                }
            }
        }
        private void UpdateCandles() {
            var selected = comboBoxMap["MTFCandlesKey"].SelectedItem;

            TimeFrame value = StringToTimeframe(selected);
            if (Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Heikin_Ash)
                value = StringToTimeframe(selected, true);

            UpdateMTFInterval(value);
        }
        private void UpdateRenko() {
            var selected = comboBoxMap["MTFRenkoKey"].SelectedItem;
            TimeFrame value = StringToRenko(selected);
            UpdateMTFInterval(value);
        }
        private void UpdateRange() {
            var selected = comboBoxMap["MTFRangeKey"].SelectedItem;
            TimeFrame value = StringToRange(selected);
            UpdateMTFInterval(value);
        }
        private void UpdateTick() {
            var selected = comboBoxMap["MTFTicksKey"].SelectedItem;
            TimeFrame value = StringToTick(selected);
            UpdateMTFInterval(value);
        }
        private void UpdateMTFInterval(TimeFrame tf)
        {
            string[] timesBased = { "Minute", "Hour", "Daily", "Day", "Weekly", "Monthly" };
            string tfName = tf.ToString();
            bool isSelected = Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Tick && tfName.Contains("Tick") ||
                              Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Range && tfName.Contains("Range") ||
                              Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Renko && tfName.Contains("Renko") ||
                              Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Heikin_Ash && tfName.Contains("Heikin") ||
                              Outside.ZigZagParams.MTFSource_Panel == MTF_Sources.Standard && timesBased.Any(tfName.Contains);
            if (isSelected) {
                Outside.SetMTFSource_TimeFrame(tf);
                RecalculateOutsideWithMsg();
            }
        }
        private void UpdatePriority()
        {
            var selected = comboBoxMap["PriorityZZKey"].SelectedItem;
            if (Enum.TryParse(selected, out Priority_Data priorityType) && priorityType != Outside.ZigZagParams.Priority_Input)
            {
                Outside.ZigZagParams.Priority_Input = priorityType;
                RecalculateOutsideWithMsg(false);
            }
        }
        private static TimeFrame StringToTimeframe(string inputTF, bool isHeikin = false)
        {
            TimeFrame ifWrong = TimeFrame.Minute;
            switch (inputTF)
            {
                case "m1": return !isHeikin ? TimeFrame.Minute : TimeFrame.HeikinMinute;
                case "m2": return !isHeikin ? TimeFrame.Minute2 : TimeFrame.HeikinMinute2;
                case "m3": return !isHeikin ? TimeFrame.Minute3 : TimeFrame.HeikinMinute3;
                case "m4": return !isHeikin ? TimeFrame.Minute4 : TimeFrame.HeikinMinute4;
                case "m5": return !isHeikin ? TimeFrame.Minute5 : TimeFrame.HeikinMinute5;
                case "m6": return !isHeikin ? TimeFrame.Minute6 : TimeFrame.HeikinMinute6;
                case "m7": return !isHeikin ? TimeFrame.Minute7 : TimeFrame.HeikinMinute7;
                case "m8": return !isHeikin ? TimeFrame.Minute8 : TimeFrame.HeikinMinute8;
                case "m9": return !isHeikin ? TimeFrame.Minute9 : TimeFrame.HeikinMinute9;
                case "m10": return !isHeikin ? TimeFrame.Minute10 : TimeFrame.HeikinMinute10;
                case "m15": return !isHeikin ? TimeFrame.Minute15 : TimeFrame.HeikinMinute15;
                case "m30": return !isHeikin ? TimeFrame.Minute30 : TimeFrame.HeikinMinute30;
                case "m45": return !isHeikin ? TimeFrame.Minute45 : TimeFrame.HeikinMinute45;
                case "h1": return !isHeikin ? TimeFrame.Hour : TimeFrame.HeikinHour;
                case "h2": return !isHeikin ? TimeFrame.Hour2 : TimeFrame.HeikinHour2;
                case "h3": return !isHeikin ? TimeFrame.Hour3 : TimeFrame.HeikinHour3;
                case "h4": return !isHeikin ? TimeFrame.Hour4 : TimeFrame.HeikinHour4;
                case "h6": return !isHeikin ? TimeFrame.Hour6 : TimeFrame.HeikinHour6;
                case "h8": return !isHeikin ? TimeFrame.Hour8 : TimeFrame.HeikinHour8;
                case "h12": return !isHeikin ? TimeFrame.Hour12 : TimeFrame.HeikinHour12;
                case "D1": return !isHeikin ? TimeFrame.Daily : TimeFrame.HeikinDaily;
                case "D2": return !isHeikin ? TimeFrame.Day2 : TimeFrame.HeikinDay2;
                case "D3": return !isHeikin ? TimeFrame.Day3 : TimeFrame.HeikinDay3;
                case "W1": return !isHeikin ? TimeFrame.Weekly : TimeFrame.HeikinWeekly;
                case "Month1": return !isHeikin ? TimeFrame.Monthly : TimeFrame.HeikinMonthly;
                default:
                    break;
            }
            return ifWrong;
        }

        private static TimeFrame StringToRenko(string inputTF)
        {
            TimeFrame ifWrong = TimeFrame.Minute;
            switch (inputTF)
            {
                case "Re1": return TimeFrame.Renko1;
                case "Re2": return TimeFrame.Renko2;
                case "Re3": return TimeFrame.Renko3;
                case "Re4": return TimeFrame.Renko4;
                case "Re5": return TimeFrame.Renko5;
                case "Re6": return TimeFrame.Renko6;
                case "Re7": return TimeFrame.Renko7;
                case "Re8": return TimeFrame.Renko8;
                case "Re9": return TimeFrame.Renko9;
                case "Re10": return TimeFrame.Renko10;
                case "Re15": return TimeFrame.Renko15;
                case "Re20": return TimeFrame.Renko20;
                case "Re25": return TimeFrame.Renko25;
                case "Re30": return TimeFrame.Renko30;
                case "Re35": return TimeFrame.Renko35;
                case "Re40": return TimeFrame.Renko40;
                case "Re45": return TimeFrame.Renko45;
                case "Re50": return TimeFrame.Renko50;
                case "Re100": return TimeFrame.Renko100;
                case "Re150": return TimeFrame.Renko150;
                case "Re200": return TimeFrame.Renko200;
                case "Re300": return TimeFrame.Renko300;
                case "Re500": return TimeFrame.Renko500;
                case "Re800": return TimeFrame.Renko800;
                case "Re1000": return TimeFrame.Renko1000;
                case "Re2000": return TimeFrame.Renko2000;
                default:
                    break;
            }
            return ifWrong;
        }
        private static TimeFrame StringToRange(string inputTF)
        {
            TimeFrame ifWrong = TimeFrame.Minute;
            switch (inputTF)
            {
                case "Ra1": return TimeFrame.Range1;
                case "Ra2": return TimeFrame.Range2;
                case "Ra3": return TimeFrame.Range3;
                case "Ra4": return TimeFrame.Range4;
                case "Ra5": return TimeFrame.Range5;
                case "Ra8": return TimeFrame.Range8;
                case "Ra10": return TimeFrame.Range10;
                case "Ra20": return TimeFrame.Range20;
                case "Ra30": return TimeFrame.Range30;
                case "Ra50": return TimeFrame.Range50;
                case "Ra80": return TimeFrame.Range80;
                case "Ra100": return TimeFrame.Range100;
                case "Ra150": return TimeFrame.Range150;
                case "Ra200": return TimeFrame.Range200;
                case "Ra300": return TimeFrame.Range300;
                case "Ra500": return TimeFrame.Range500;
                case "Ra800": return TimeFrame.Range800;
                case "Ra1000": return TimeFrame.Range1000;
                case "Ra2000": return TimeFrame.Range2000;
                case "Ra5000": return TimeFrame.Range5000;
                case "Ra7500": return TimeFrame.Range7500;
                case "Ra10000": return TimeFrame.Range10000;
                default:
                    break;
            }
            return ifWrong;
        }
        private static TimeFrame StringToTick(string inputTF)
        {
            TimeFrame ifWrong = TimeFrame.Minute;
            switch (inputTF)
            {
                case "t1": return TimeFrame.Tick;
                case "t2": return TimeFrame.Tick2;
                case "t3": return TimeFrame.Tick3;
                case "t4": return TimeFrame.Tick4;
                case "t5": return TimeFrame.Tick5;
                case "t6": return TimeFrame.Tick6;
                case "t7": return TimeFrame.Tick7;
                case "t8": return TimeFrame.Tick8;
                case "t9": return TimeFrame.Tick9;
                case "t10": return TimeFrame.Tick10;
                case "t15": return TimeFrame.Tick15;
                case "t20": return TimeFrame.Tick20;
                case "t25": return TimeFrame.Tick25;
                case "t30": return TimeFrame.Tick30;
                case "t40": return TimeFrame.Tick40;
                case "t50": return TimeFrame.Tick50;
                case "t60": return TimeFrame.Tick60;
                case "t80": return TimeFrame.Tick80;
                case "t90": return TimeFrame.Tick90;
                case "t100": return TimeFrame.Tick100;
                case "t150": return TimeFrame.Tick150;
                case "t200": return TimeFrame.Tick200;
                case "t300": return TimeFrame.Tick300;
                case "t500": return TimeFrame.Tick500;
                case "t750": return TimeFrame.Tick750;
                case "t1000": return TimeFrame.Tick1000;
                default:
                    break;
            }
            return ifWrong;
        }

        private void RecalculateOutsideWithMsg(bool reset = true, bool isTemplate = false)
        {
            // Avoid multiples calls when loading parameters from LocalStorage
            if (isLoadingParams)
                return;

            string current = isTemplate ? ModeBtn.Text : "Custom";
            if (!isTemplate)
                Outside.Template_Input = Template_Data.Custom;

            ModeBtn.Text = $"{current}\nCalculating...";
            Outside.BeginInvokeOnMainThread(() => {
                try { _progressBar.IsIndeterminate = true; } catch { }
            });

            if (reset) {
                Outside.BeginInvokeOnMainThread(() => {
                    Outside.Chart.RemoveAllObjects();
                    Outside.Chart.ResetBarColors();
                });
            }

            Outside.BeginInvokeOnMainThread(() => {
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

            Outside.Template_Input = Outside.Template_Input switch
            {
                Template_Data.Insider => Template_Data.Volume,
                Template_Data.Volume => Template_Data.Time,
                Template_Data.Time => Template_Data.BigBrain,
                Template_Data.BigBrain => Template_Data.Insider,
                _ => Template_Data.Insider
            };
            ModeBtn.Text = Outside.Template_Input.ToString();

            ChangePanelParams_DesignTemplates();
            ChangePanelParams_SpecificChartTemplates();

            RefreshVisibility();
            RecalculateOutsideWithMsg(true, true);

            cleaningProgress.Complete(PopupNotificationState.Success);
        }
        private void PrevModeEvent(ButtonClickEventArgs e)
        {
            PopupNotification  cleaningProgress = Outside.Notifications.ShowPopup(
                Outside.NOTIFY_CAPTION,
                "Cleaning up the chart...",
                PopupNotificationState.InProgress
            );

            Outside.Template_Input = Outside.Template_Input switch
            {
                Template_Data.BigBrain => Template_Data.Time,
                Template_Data.Time => Template_Data.Volume,
                Template_Data.Volume => Template_Data.Insider,
                Template_Data.Insider => Template_Data.BigBrain,
                _ => Template_Data.Insider
            };
            ModeBtn.Text = Outside.Template_Input.ToString();

            ChangePanelParams_DesignTemplates();
            ChangePanelParams_SpecificChartTemplates();

            RefreshVisibility();
            RecalculateOutsideWithMsg(true, true);

            cleaningProgress.Complete(PopupNotificationState.Success);
        }

        private void ChangePanelParams_DesignTemplates() {
            switch (Outside.Template_Input)
            {
                case Template_Data.Insider:
                    comboBoxMap["ShowNumbersKey"].SelectedItem = $"{Numbers_Data.Both}";
                    comboBoxMap["NumbersPositionKey"].SelectedItem = $"{NumbersPosition_Data.Inside}";
                    comboBoxMap["NumbersColorKey"].SelectedItem = $"{NumbersColor_Data.Volume}";
                    comboBoxMap["BarsColorKey"].SelectedItem = $"{BarsColor_Data.Volume}";

                    comboBoxMap["ShowWavesKey"].SelectedItem = $"{ShowWaves_Data.EffortvsResult}";
                    comboBoxMap["OtherWavesKey"].SelectedItem = $"{ShowOtherWaves_Data.Both}";
                    comboBoxMap["MarksKey"].SelectedItem = $"{ShowMarks_Data.No}";

                    checkBoxMap["EnableWyckoffKey"].IsChecked = true;
                    Outside.WyckoffParams.EnableWyckoff = true;
                    checkBoxMap["CurrentWaveKey"].IsChecked = true;
                    Outside.WavesParams.ShowCurrentWave = true;
                    checkBoxMap["FillBarsKey"].IsChecked = true;
                    Outside.WyckoffParams.FillBars = true;
                    checkBoxMap["OutlineKey"].IsChecked = false;
                    Outside.WyckoffParams.KeepOutline = false;

                    Outside.Chart.ChartType = ChartType.Candlesticks;
                    break;
                case Template_Data.Volume:
                    comboBoxMap["ShowNumbersKey"].SelectedItem = $"{Numbers_Data.Volume}";
                    comboBoxMap["NumbersPositionKey"].SelectedItem = $"{NumbersPosition_Data.Inside}";
                    comboBoxMap["NumbersColorKey"].SelectedItem = $"{NumbersColor_Data.Volume}";
                    comboBoxMap["BarsColorKey"].SelectedItem = $"{BarsColor_Data.Volume}";

                    comboBoxMap["ShowWavesKey"].SelectedItem = $"{ShowWaves_Data.Volume}";
                    comboBoxMap["OtherWavesKey"].SelectedItem = $"{ShowOtherWaves_Data.Price}";
                    comboBoxMap["MarksKey"].SelectedItem = $"{ShowMarks_Data.No}";

                    checkBoxMap["EnableWyckoffKey"].IsChecked = true;
                    checkBoxMap["CurrentWaveKey"].IsChecked = true;
                    checkBoxMap["FillBarsKey"].IsChecked = true;
                    checkBoxMap["OutlineKey"].IsChecked = false;
                    break;
                case Template_Data.Time:
                    comboBoxMap["ShowNumbersKey"].SelectedItem = $"{Numbers_Data.Time}";
                    comboBoxMap["NumbersPositionKey"].SelectedItem = $"{NumbersPosition_Data.Inside}";
                    comboBoxMap["NumbersColorKey"].SelectedItem = $"{NumbersColor_Data.Time}";
                    comboBoxMap["BarsColorKey"].SelectedItem = $"{BarsColor_Data.Time}";

                    comboBoxMap["ShowWavesKey"].SelectedItem = $"{ShowWaves_Data.EffortvsResult}";
                    comboBoxMap["OtherWavesKey"].SelectedItem = $"{ShowOtherWaves_Data.Time}";
                    comboBoxMap["MarksKey"].SelectedItem = $"{ShowMarks_Data.No}";

                    checkBoxMap["EnableWyckoffKey"].IsChecked = true;
                    checkBoxMap["CurrentWaveKey"].IsChecked = true;
                    checkBoxMap["FillBarsKey"].IsChecked = true;
                    checkBoxMap["OutlineKey"].IsChecked = false;
                    break;
                case Template_Data.BigBrain:
                    comboBoxMap["ShowNumbersKey"].SelectedItem = $"{Numbers_Data.Both}";
                    comboBoxMap["NumbersPositionKey"].SelectedItem = $"{NumbersPosition_Data.Inside}";

                    // causes a 2x UI update, no idea why.
                    // comboBoxMap["NumbersColorKey"].SelectedItem = $"{NumbersColor_Data.Time}";
                    // comboBoxMap["BarsColorKey"].SelectedItem = $"{BarsColor_Data.Volume}";

                    comboBoxMap["ShowWavesKey"].SelectedItem = $"{ShowWaves_Data.Both}";
                    comboBoxMap["OtherWavesKey"].SelectedItem = $"{ShowOtherWaves_Data.Both}";
                    comboBoxMap["MarksKey"].SelectedItem = $"{ShowMarks_Data.Both}";

                    checkBoxMap["EnableWyckoffKey"].IsChecked = true;
                    Outside.WyckoffParams.EnableWyckoff = true;
                    checkBoxMap["CurrentWaveKey"].IsChecked = true;
                    Outside.WavesParams.ShowCurrentWave = true;
                    checkBoxMap["FillBarsKey"].IsChecked = true;
                    Outside.WyckoffParams.FillBars = true;
                    checkBoxMap["OutlineKey"].IsChecked = false;
                    Outside.WyckoffParams.KeepOutline = false;
                    break;
                default: break;
            }
        }
        private void ChangePanelParams_SpecificChartTemplates() {
            if (Outside.Template_Input == Template_Data.Custom)
                return;
            // Tick / Time-Based Chart (Standard Candles/Heikin-Ash)
            if (Outside.BooleanUtils.isTickChart || !Outside.BooleanUtils.isPriceBased_Chart) {
                if (Outside.BooleanUtils.isTickChart) {
                    comboBoxMap["StrengthFilterKey"].SelectedItem = $"{StrengthFilter_Data.Both}";

                    textInputMap["MAPeriodKey"].Text = "20";
                    comboBoxMap["MATypeKey"].SelectedItem = $"{MovingAverageType.Triangular}";

                    textInputMap["LowestFixedKey"].Text = "0.5";
                    textInputMap["LowFixedKey"].Text = "1.2";
                    textInputMap["AverageFixedKey"].Text = "2.5";
                    textInputMap["HighFixedKey"].Text = "3.5";
                    textInputMap["UltraFixedKey"].Text = "3.51";
                }
            }
            // Range
            if (Outside.BooleanUtils.isPriceBased_Chart && !Outside.BooleanUtils.isRenkoChart && !Outside.BooleanUtils.isTickChart) {
                comboBoxMap["ShowNumbersKey"].SelectedItem = $"{Numbers_Data.Volume}";
                comboBoxMap["StrengthFilterKey"].SelectedItem = $"{StrengthFilter_Data.MA}";

                textInputMap["MAPeriodKey"].Text = "20";
                comboBoxMap["MATypeKey"].SelectedItem = $"{MovingAverageType.Triangular}";

                textInputMap["LowestFixedKey"].Text = "0.5";
                textInputMap["LowFixedKey"].Text = "1.2";
                textInputMap["AverageFixedKey"].Text = "2.5";
                textInputMap["HighFixedKey"].Text = "3.5";
                textInputMap["UltraFixedKey"].Text = "3.51";
            }
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
                ? $"WWS {BrokerPrefix} {SymbolPrefix} {TimeframePrefix}"
                : $"WWS {SymbolPrefix} {TimeframePrefix}";
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
            storageModel.Params["PanelMode"] = Outside.Template_Input;

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
            string templateText = storageModel.Params["PanelMode"].ToString();
            _ = Enum.TryParse(templateText, out Template_Data templateMode);
            Outside.Template_Input = templateMode;
            ModeBtn.Text = templateText;

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
