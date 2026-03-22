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
    public partial class WeisWyckoffSystemV20 : Indicator
    {
        private static readonly TimeSpan SocketReconnectDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan SocketHeartbeatInterval = TimeSpan.FromSeconds(5);
        private DateTime _nextReconnectAtUtc = DateTime.MinValue;
        private DateTime _nextHeartbeatAtUtc = DateTime.MinValue;
        private int _reconnectCount;
        private long _droppedEventsTotal;
        private string _connectionState = "socket disconnected";
        private bool _hasConnectedOnce;
        private Button _connectionStatusButton;

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

            _connectionStatusButton = new Button()
            {
                Text = "",
                Padding = 0,
                Height = 12,
                Width = 12,
                Margin = 2,
                BackgroundColor = btnColor,
                Style = Styles.CreateButtonStyle()
            };
            panel.AddChild(_connectionStatusButton);
            UpdateConnectionStatusIndicator();

            Button reconnectButton = new()
            {
                Text = "Reconnect",
                Padding = 0,
                Height = 22,
                Width = 75,
                Margin = 2,
                BackgroundColor = btnColor
            };
            reconnectButton.Click += ReconnectEvent;
            panel.AddChild(reconnectButton);
        }

        private Color GetConnectionStatusColor()
        {
            return _connectionState == "socket connected"
                ? Color.FromHex("#2ECC71")
                : Color.FromHex("#E74C3C");
        }

        private void UpdateConnectionStatusIndicator()
        {
            if (_connectionStatusButton == null)
                return;

            _connectionStatusButton.Text = "";
            _connectionStatusButton.BackgroundColor = GetConnectionStatusColor();
        }

        private void HiddenEvent(ButtonClickEventArgs obj)
        {
            if (ParamBorder.IsVisible)
                ParamBorder.IsVisible = false;
            else
                ParamBorder.IsVisible = true;
        }

        private void CloseSocketConnection()
        {
            try { _networkStream?.Close(); } catch {}
            try { _tcpClient?.Close(); } catch {}

            _networkStream = null;
            _tcpClient = null;
        }

        private void SetConnectionState(string nextState)
        {
            if (_connectionState == nextState)
                return;

            _connectionState = nextState;
            Print(nextState);
            UpdateConnectionStatusIndicator();
        }

        private void ResetHeartbeatDeadline()
        {
            _nextHeartbeatAtUtc = DateTime.UtcNow.Add(SocketHeartbeatInterval);
        }

        private void HandleSocketDisconnect()
        {
            CloseSocketConnection();
            SetConnectionState("socket disconnected");
        }

        private bool SendConnectionHello(int reconnectCount)
        {
            if (_networkStream == null)
                return false;

            try
            {
                var hello = new Dictionary<string, object>
                {
                    ["kind"] = "connection_hello",
                    ["source"] = EventSource,
                    ["source_instance"] = SourceInstanceName,
                    ["instrument"] = Symbol.Name,
                    ["timestamp"] = DateTime.UtcNow.ToString("o"),
                    ["reconnect_count"] = reconnectCount,
                    ["dropped_events_total"] = _droppedEventsTotal
                };

                string jsonString = JsonSerializer.Serialize(hello) + "\n";
                byte[] data = Encoding.UTF8.GetBytes(jsonString);
                _networkStream.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception)
            {
                HandleSocketDisconnect();
                return false;
            }
        }

        private bool SendConnectionHeartbeat()
        {
            if (_networkStream == null)
                return false;

            try
            {
                var heartbeat = new Dictionary<string, object>
                {
                    ["kind"] = "connection_heartbeat",
                    ["source"] = EventSource,
                    ["source_instance"] = SourceInstanceName,
                    ["instrument"] = Symbol.Name,
                    ["timestamp"] = DateTime.UtcNow.ToString("o"),
                    ["reconnect_count"] = _reconnectCount,
                    ["dropped_events_total"] = _droppedEventsTotal
                };

                string jsonString = JsonSerializer.Serialize(heartbeat) + "\n";
                byte[] data = Encoding.UTF8.GetBytes(jsonString);
                _networkStream.Write(data, 0, data.Length);
                ResetHeartbeatDeadline();
                return true;
            }
            catch (Exception)
            {
                HandleSocketDisconnect();
                return false;
            }
        }

        private bool EnsureSocketConnected(bool force = false)
        {
            if (_tcpClient != null && _tcpClient.Connected && _networkStream != null)
                return true;

            DateTime now = DateTime.UtcNow;
            if (!force && now < _nextReconnectAtUtc)
                return false;

            _nextReconnectAtUtc = now.Add(SocketReconnectDelay);
            SetConnectionState("socket reconnecting");

            try
            {
                CloseSocketConnection();
                _tcpClient = new TcpClient("127.0.0.1", 5555);
                _networkStream = _tcpClient.GetStream();
                int helloReconnectCount = _hasConnectedOnce ? _reconnectCount + 1 : _reconnectCount;
                if (!SendConnectionHello(helloReconnectCount))
                    return false;

                _reconnectCount = helloReconnectCount;
                _hasConnectedOnce = true;
                ResetHeartbeatDeadline();
                SetConnectionState("socket connected");
                return true;
            }
            catch (Exception)
            {
                HandleSocketDisconnect();
                return false;
            }
        }

        private void HandleSocketWriteFailure()
        {
            _droppedEventsTotal++;
            HandleSocketDisconnect();
        }

        private void RunSocketHeartbeat()
        {
            if (!EnsureSocketConnected())
                return;

            if (DateTime.UtcNow < _nextHeartbeatAtUtc)
                return;

            SendConnectionHeartbeat();
        }

        private void ConnectSocket()
        {
            EnsureSocketConnected(force: true);
        }

        private void ReconnectEvent(ButtonClickEventArgs obj)
        {
            try
            {
                CloseSocketConnection();
                _nextReconnectAtUtc = DateTime.MinValue;
                EnsureSocketConnected(force: true);
            }
            catch (Exception ex)
            {
                Print("Reconnect Error: " + ex.Message);
            }
        }

        private void ExportEvent(ButtonClickEventArgs obj)
        {
            _exportButton.IsEnabled = false;
            try
            {
                ConnectSocket();
                _isManualCsvExportInProgress = true;

                Print("Starting Weis Wave & Wyckoff Export...");
                ClearAndRecalculate();
                Print("Weis Wave & Wyckoff Export Finished.");
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
            Timer.Start(TimeSpan.FromSeconds(1));

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
                Orientation = Orientation.Horizontal,
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

            if (_isManualCsvExportInProgress)
            {
                ExportCsvData(index);
            }

            if (IsLastBar)
            {
                SendSocketData(index);
            }
        }

        public void SendSocketData(int index)
        {
            double vol = double.IsNaN(VolumeSeries[index]) ? 0 : VolumeSeries[index];
            if (!EnsureSocketConnected())
            {
                _droppedEventsTotal++;
                return;
            }

            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, vol);

                string jsonString = JsonSerializer.Serialize(exportData) + "\n";
                byte[] data = Encoding.UTF8.GetBytes(jsonString);
                try
                {
                    _networkStream.Write(data, 0, data.Length);
                }
                catch (Exception)
                {
                    HandleSocketWriteFailure();
                }
            }
            catch { }
        }

        private Dictionary<string, object> BuildExportPayload(int index, double vol)
        {
            double time = double.IsNaN(TimeSeries[index]) ? 0 : TimeSeries[index];
            double zigzag = double.IsNaN(ZigZagBuffer[index]) ? 0 : ZigZagBuffer[index];

            Dictionary<string, object> payload = new Dictionary<string, object>
            {
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

            Dictionary<string, object> sourceMeta = new Dictionary<string, object>
            {
                ["symbol"] = Symbol.Name,
                ["timeframe"] = Chart.TimeFrame.ShortName
            };

            return BuildContractEnvelope(index, payload, sourceMeta);
        }

        private void AppendDirectCsv(Dictionary<string, object> exportData)
        {
            if (!_isManualCsvExportInProgress)
                return;

            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder.Trim();
            Directory.CreateDirectory(outputFolder);

            string symbol = ResolveExportSymbol(exportData);
            string filePath = Path.Combine(outputFolder, $"history_wyckoff_{symbol}.csv");
            bool writeHeader = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;

            using (StreamWriter writer = new StreamWriter(filePath, true, Utf8NoBom))
            {
                if (writeHeader)
                    writer.WriteLine(string.Join(",", ExportCsvHeaders));

                string[] rowValues = new string[ExportCsvHeaders.Length];

                for (int i = 0; i < ExportCsvHeaders.Length; i++)
                {
                    string key = ExportCsvHeaders[i];
                    object value = ResolveExportValue(exportData, key);
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

        private object ResolveExportValue(Dictionary<string, object> exportData, string key)
        {
            object value;
            if (exportData.TryGetValue(key, out value))
                return value;

            if (key == "symbol" || key == "timeframe")
            {
                if (exportData.TryGetValue("source_meta", out object sourceMetaObj) &&
                    sourceMetaObj is Dictionary<string, object> sourceMeta &&
                    sourceMeta.TryGetValue(key, out value))
                {
                    return value;
                }

                if (key == "symbol" && exportData.TryGetValue("instrument", out value))
                    return value;
            }

            if (exportData.TryGetValue("payload", out object payloadObj) &&
                payloadObj is Dictionary<string, object> payload &&
                payload.TryGetValue(key, out value))
            {
                return value;
            }

            return null;
        }

        private string ResolveExportSymbol(Dictionary<string, object> exportData)
        {
            object symbolValue = ResolveExportValue(exportData, "symbol");
            string rawSymbol = Convert.ToString(symbolValue, CultureInfo.InvariantCulture) ?? Symbol.Name;
            return SanitizeFileToken(rawSymbol);
        }

        private string SanitizeFileToken(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return "unknown";

            StringBuilder builder = new StringBuilder(rawValue.Length);
            foreach (char character in rawValue)
            {
                builder.Append(char.IsLetterOrDigit(character) || character == '.' || character == '-'
                    ? character
                    : '_');
            }

            return builder.ToString();
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
        
    }
}
