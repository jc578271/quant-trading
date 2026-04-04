using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    public partial class OhlcTrainingExporterV10 : Indicator
    {
        private readonly Dictionary<string, ControlBase> _paramControlMap = new();
        private readonly Dictionary<string, ControlBase> _paramRowMap = new();
        private List<ParamDefinition> _paramDefinitions;

        private enum ParamInputType
        {
            Text,
            Checkbox,
            ComboBox
        }

        private class ParamDefinition
        {
            public string Region { get; set; }
            public int RegionOrder { get; set; }
            public string Key { get; set; }
            public string Label { get; set; }
            public ParamInputType InputType { get; set; }
            public Func<string> GetDefault { get; set; }
            public Func<string[]> EnumOptions { get; set; }
            public Action<string> OnChanged { get; set; }
            public Func<bool> IsVisible { get; set; }
        }

        private void CreateParamsPanel()
        {
            _paramDefinitions = BuildParamDefinitions();

            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "12 12 12 12"
            };

            foreach (var region in _paramDefinitions.OrderBy(x => x.RegionOrder).GroupBy(x => x.Region))
            {
                panel.AddChild(new TextBlock
                {
                    Text = region.Key,
                    Style = Styles.CreateHeaderStyle(),
                    Margin = "0 10 0 4",
                    TextAlignment = TextAlignment.Center
                });

                foreach (var definition in region)
                {
                    ControlBase row = definition.InputType switch
                    {
                        ParamInputType.Text => CreateTextInputRow(definition),
                        ParamInputType.ComboBox => CreateComboBoxRow(definition),
                        ParamInputType.Checkbox => CreateCheckBoxRow(definition),
                        _ => throw new InvalidOperationException("Unsupported input type.")
                    };

                    _paramRowMap[definition.Key] = row;
                    panel.AddChild(row);
                }
            }

            ResolvePanelAlignment(PanelAlignment_Input, out var verticalAlignment, out var horizontalAlignment);

            _paramsPanel = panel;
            _paramsBorder = new Border
            {
                VerticalAlignment = verticalAlignment,
                HorizontalAlignment = horizontalAlignment,
                Style = Styles.CreatePanelBackgroundStyle(),
                Margin = "20 40 20 20",
                Width = 290,
                Child = panel
            };

            Chart.AddControl(_paramsBorder);
            UpdateDynamicVisibility();
        }

        private List<ParamDefinition> BuildParamDefinitions()
        {
            return new List<ParamDefinition>
            {
                new ParamDefinition
                {
                    Region = "Export Settings",
                    RegionOrder = 0,
                    Key = "LoadFrom",
                    Label = "Load From",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = () => LoadTickFrom_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(LoadTickFrom_Data)),
                    OnChanged = _ => UpdateLoadFromSelection(),
                    IsVisible = () => true
                },
                new ParamDefinition
                {
                    Region = "Export Settings",
                    RegionOrder = 0,
                    Key = "CustomDate",
                    Label = "Custom (dd/MM/yyyy)",
                    InputType = ParamInputType.Text,
                    GetDefault = () => StringDate,
                    OnChanged = _ => UpdateCustomDate(),
                    IsVisible = () => LoadTickFrom_Input == LoadTickFrom_Data.Custom
                },
                new ParamDefinition
                {
                    Region = "Export Settings",
                    RegionOrder = 0,
                    Key = "OutputFolder",
                    Label = "History Output Folder",
                    InputType = ParamInputType.Text,
                    GetDefault = () => CsvOutputFolder,
                    OnChanged = _ => UpdateOutputFolder(),
                    IsVisible = () => true
                },
                new ParamDefinition
                {
                    Region = "Panel",
                    RegionOrder = 1,
                    Key = "PanelAlignment",
                    Label = "Panel Alignment",
                    InputType = ParamInputType.ComboBox,
                    GetDefault = () => PanelAlignment_Input.ToString(),
                    EnumOptions = () => Enum.GetNames(typeof(PanelAlignment)),
                    OnChanged = _ => UpdatePanelAlignmentSelection(),
                    IsVisible = () => true
                }
            };
        }

        private ControlBase CreateTextInputRow(ParamDefinition definition)
        {
            var textBox = new TextBox
            {
                Text = definition.GetDefault?.Invoke() ?? string.Empty,
                Style = Styles.CreateInputStyle(),
                TextAlignment = TextAlignment.Center,
                Margin = "0 5 0 0"
            };
            textBox.TextChanged += _ => definition.OnChanged?.Invoke(definition.Key);
            _paramControlMap[definition.Key] = textBox;

            var row = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 4 0 0"
            };
            row.AddChild(new TextBlock
            {
                Text = definition.Label,
                TextAlignment = TextAlignment.Center
            });
            row.AddChild(textBox);
            return row;
        }

        private ControlBase CreateComboBoxRow(ParamDefinition definition)
        {
            var comboBox = new ComboBox
            {
                Style = Styles.CreateComboBoxStyle(),
                Margin = "0 5 0 0"
            };

            foreach (string option in definition.EnumOptions?.Invoke() ?? Array.Empty<string>())
                comboBox.AddItem(option);

            comboBox.SelectedItem = definition.GetDefault?.Invoke();
            comboBox.SelectedItemChanged += _ => definition.OnChanged?.Invoke(definition.Key);
            _paramControlMap[definition.Key] = comboBox;

            var row = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 4 0 0"
            };
            row.AddChild(new TextBlock
            {
                Text = definition.Label,
                TextAlignment = TextAlignment.Center
            });
            row.AddChild(comboBox);
            return row;
        }

        private ControlBase CreateCheckBoxRow(ParamDefinition definition)
        {
            var checkBox = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            checkBox.Click += _ => definition.OnChanged?.Invoke(definition.Key);
            _paramControlMap[definition.Key] = checkBox;

            var row = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 4 0 0"
            };
            row.AddChild(new TextBlock
            {
                Text = definition.Label,
                TextAlignment = TextAlignment.Center
            });
            row.AddChild(checkBox);
            return row;
        }

        private void AddExportButton()
        {
            ResolvePanelAlignment(PanelAlignment_Input, out var verticalAlignment, out var horizontalAlignment);

            var stackPanel = new StackPanel
            {
                VerticalAlignment = verticalAlignment,
                HorizontalAlignment = horizontalAlignment,
                Orientation = Orientation.Horizontal
            };

            _panelToggleButton = new Button
            {
                Text = "Panel",
                Padding = 0,
                Height = 22,
                Width = 50,
                Margin = 2,
                Style = Styles.CreateButtonStyle()
            };
            _panelToggleButton.Click += _ => UpdatePanelVisibility();
            stackPanel.AddChild(_panelToggleButton);

            _exportButton = new Button
            {
                Text = "Export",
                Padding = 0,
                Height = 22,
                Width = 50,
                Margin = 2,
                Style = Styles.CreateButtonStyle()
            };
            _exportButton.Click += ExportEvent;
            stackPanel.AddChild(_exportButton);

            _socketStatusText = new TextBlock
            {
                Text = _socketState,
                Margin = "6 4 6 0",
                Style = Styles.CreateHeaderStyle(),
                TextAlignment = TextAlignment.Center
            };
            stackPanel.AddChild(_socketStatusText);

            var reconnectButton = new Button
            {
                Text = "Reconnect",
                Padding = 0,
                Height = 22,
                Width = 75,
                Margin = 2,
                Style = Styles.CreateButtonStyle()
            };
            reconnectButton.Click += ReconnectEvent;
            stackPanel.AddChild(reconnectButton);

            _controlsBorder = new Border
            {
                VerticalAlignment = verticalAlignment,
                HorizontalAlignment = horizontalAlignment,
                Margin = "20 8 20 20",
                Style = Styles.CreatePanelBackgroundStyle(),
                Child = stackPanel
            };

            Chart.AddControl(_controlsBorder);
            UpdateSocketStatusDisplay();
        }

        private void UpdatePanelVisibility()
        {
            if (_paramsBorder == null)
                return;

            _paramsBorder.IsVisible = !_paramsBorder.IsVisible;
            if (_panelToggleButton != null)
                _panelToggleButton.Text = _paramsBorder.IsVisible ? "Hide" : "Panel";
        }

        private void UpdateSocketStatusDisplay()
        {
            if (_socketStatusText == null)
                return;

            _socketStatusText.Text = _socketState + (_hasDateSelectionError ? " | date error" : string.Empty);
        }

        private void UpdateLoadFromSelection()
        {
            if (_panelAlignmentComboBox == null)
            {
                _loadFromComboBox = _paramControlMap["LoadFrom"] as ComboBox;
                _customDateTextBox = _paramControlMap["CustomDate"] as TextBox;
                _outputFolderTextBox = _paramControlMap["OutputFolder"] as TextBox;
                _panelAlignmentComboBox = _paramControlMap["PanelAlignment"] as ComboBox;
            }

            string selected = (_paramControlMap["LoadFrom"] as ComboBox)?.SelectedItem;
            if (Enum.TryParse(selected, out LoadTickFrom_Data mode))
                LoadTickFrom_Input = mode;

            TryResolveFromDateTime(showErrors: false, out _selectedFromDateTimeUtc);
            UpdateDynamicVisibility();
            UpdateSocketStatusDisplay();
        }

        private void UpdateCustomDate()
        {
            StringDate = (_paramControlMap["CustomDate"] as TextBox)?.Text ?? StringDate;
            TryResolveFromDateTime(showErrors: false, out _selectedFromDateTimeUtc);
            UpdateSocketStatusDisplay();
        }

        private void UpdateOutputFolder()
        {
            CsvOutputFolder = (_paramControlMap["OutputFolder"] as TextBox)?.Text ?? CsvOutputFolder;
        }

        private void UpdatePanelAlignmentSelection()
        {
            string selected = (_paramControlMap["PanelAlignment"] as ComboBox)?.SelectedItem;
            if (Enum.TryParse(selected, out PanelAlignment alignment))
                PanelAlignment_Input = alignment;

            ApplyPanelAlignment();
        }

        private void ApplyPanelAlignment()
        {
            ResolvePanelAlignment(PanelAlignment_Input, out var verticalAlignment, out var horizontalAlignment);

            if (_paramsBorder != null)
            {
                _paramsBorder.VerticalAlignment = verticalAlignment;
                _paramsBorder.HorizontalAlignment = horizontalAlignment;
            }

            if (_controlsBorder != null)
            {
                _controlsBorder.VerticalAlignment = verticalAlignment;
                _controlsBorder.HorizontalAlignment = horizontalAlignment;
            }
        }

        private void UpdateDynamicVisibility()
        {
            foreach (var definition in _paramDefinitions)
            {
                if (_paramRowMap.TryGetValue(definition.Key, out var row))
                    row.IsVisible = definition.IsVisible?.Invoke() ?? true;
            }
        }

        private static void ResolvePanelAlignment(PanelAlignment alignment, out VerticalAlignment verticalAlignment, out HorizontalAlignment horizontalAlignment)
        {
            verticalAlignment = VerticalAlignment.Bottom;
            horizontalAlignment = HorizontalAlignment.Right;

            switch (alignment)
            {
                case PanelAlignment.Bottom_Left:
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
                case PanelAlignment.Top_Left:
                    verticalAlignment = VerticalAlignment.Top;
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
                case PanelAlignment.Top_Center:
                    verticalAlignment = VerticalAlignment.Top;
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;
                case PanelAlignment.Top_Right:
                    verticalAlignment = VerticalAlignment.Top;
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;
                case PanelAlignment.Center_Left:
                    verticalAlignment = VerticalAlignment.Center;
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
                case PanelAlignment.Center_Right:
                    verticalAlignment = VerticalAlignment.Center;
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;
                case PanelAlignment.Bottom_Center:
                    verticalAlignment = VerticalAlignment.Bottom;
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;
                case PanelAlignment.Bottom_Right:
                    verticalAlignment = VerticalAlignment.Bottom;
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;
            }
        }
    }
}
