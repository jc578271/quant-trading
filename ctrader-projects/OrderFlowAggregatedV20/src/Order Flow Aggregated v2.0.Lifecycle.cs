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
    public partial class OrderFlowTicksV20 : Indicator
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

        protected override void Initialize()
        {
            ConnectSocket();
            Timer.Start(TimeSpan.FromSeconds(1));

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
                // Should 'never' go bellow 0.2 pips.
                double rowSizePips = Math.Max(0.2, Math.Round(rowSizeInTick, 2));
                heightPips = rowSizePips;
                heightATR = rowSizePips;
            }

            // Define rowHeight by Pips
            rowHeight = Symbol.PipSize * heightPips;

            // Filters
            Dynamic_Series = CreateDataSeries();
            DeltaChange_Series = CreateDataSeries();
            DeltaBuySell_Sum_Series = CreateDataSeries();
            SubtractDelta_Series = CreateDataSeries();
            SumDelta_Series = CreateDataSeries();
            PercentageRatio_Series = CreateDataSeries();
            PercentileRatio_Series = CreateDataSeries();

            if (!UseCustomMAs)
                CreateOrReset_cTraderIndicators();

            // First Ticks Data
            TicksOHLC = MarketData.GetBars(TimeFrame.Tick);

            // Load all at once, mostly due to:
            // Loading parameters that have it
            DailyBars = MarketData.GetBars(TimeFrame.Daily);
            WeeklyBars = MarketData.GetBars(TimeFrame.Weekly);
            MonthlyBars = MarketData.GetBars(TimeFrame.Monthly);
            MiniVPs_Bars = MarketData.GetBars(ProfileParams.MiniVPs_Timeframe);

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

            DrawOnScreen("Loading Ticks Data... \n or \n Calculating...");

            string[] timesBased = { "Minute", "Hour", "Daily", "Day" };
            string ticksInfo = $"Keep in mind: \n 1) Tick data are stored in RAM \n 2) 'Lower Timeframe' with 'Small Row Size' \n   - Takes longer to calculate/draw the entirely data";
            Second_DrawOnScreen($"Taking too long? You can: \n 1) Increase the Row Size \n 2) Disable Volume Profile (High Performance) \n\n {ticksInfo}");

            // Design
            Chart.ChartType = ChartType.Hlc;

            // Performance Drawing
            Chart.ZoomChanged += PerformanceDrawing;
            Chart.ScrollChanged += PerformanceDrawing;
            Bars.BarOpened += LiveDrawing;

            // Required to recalculate the histograms in Live Market
            string currentTimeframe = Chart.TimeFrame.ToString();
            isPriceBased_Chart = currentTimeframe.Contains("Renko") || currentTimeframe.Contains("Range") || currentTimeframe.Contains("Tick");
            if (isPriceBased_Chart) {
                Bars.BarOpened += (_) => BooleanUtils.isPriceBased_NewBar = true;
                // Even with any additional recalculation here,
                // when running on Backtest, any drawing that uses avoidStretching remains the same as in live market
                // works as expected in live market though                
            }
            isRenkoChart = currentTimeframe.Contains("Renko");

            // Spike Filter + Ultra Bubbles + Spike Levels
            Bars.BarOpened += (_) =>
            {
                BooleanLocks.SetAllToFalse();
                BooleanLocks.SetAllNewBar();
                BooleanUtils.isUpdateVP = true;
                if (ProfileParams.UpdateProfile_Input != UpdateProfile_Data.EveryTick_CPU_Workout)
                    prevUpdatePrice = _.Bars.LastBar.Close;
                try { PerformanceDrawing(true); } catch { } // Draw without scroll or zoom
            };

            // Fixed Range Profiles
            RangeInitialize();

            // Params Panel
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

            IndicatorParams DefaultParams = new() {
                GeneralParams = GeneralParams,
                RowHeightInPips = heightPips,
                ProfileParams = ProfileParams,
                NodesParams = NodesParams,

                SpikeFilterParams = SpikeFilterParams,
                SpikeLevelParams = SpikeLevelParams,
                SpikeRatioParams = SpikeRatioParams,

                BubblesChartParams = BubblesChartParams,
                BubblesLevelParams = BubblesLevelParams,
                BubblesRatioParams = BubblesRatioParams,

                ResultParams = ResultParams,
                MiscParams = MiscParams,
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

            StackPanel stackPanel = new()
            {
                VerticalAlignment = vAlign,
                HorizontalAlignment = hAlign,
                Orientation = Orientation.Horizontal,
            };
            AddHiddenButton(stackPanel, Color.FromHex("#7F808080"));
            AddExportButton(stackPanel, Color.FromHex("#7F808080"));
            Chart.AddControl(stackPanel);
        }

        private void AddHiddenButton(Panel panel, Color btnColor)
        {
            Button button = new()
            {
                Text = "ODFT",
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
            try { _tcpStream?.Close(); } catch {}
            try { _tcpClient?.Close(); } catch {}

            _tcpStream = null;
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
            if (_tcpStream == null)
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
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonString);
                _tcpStream.Write(dataBytes, 0, dataBytes.Length);
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
            if (_tcpStream == null)
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
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonString);
                _tcpStream.Write(dataBytes, 0, dataBytes.Length);
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
            if (_tcpClient != null && _tcpClient.Connected && _tcpStream != null)
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
                _tcpStream = _tcpClient.GetStream();
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

                Print("Starting Order Flow Export...");
                ClearAndRecalculate();
                Print("Order Flow Export Finished.");
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

        public override void Calculate(int index)
        {
            // Tick Data Collection on chart
            bool isOnChart = LoadTickStrategy_Input != LoadTickStrategy_Data.At_Startup_Sync;
            if (isOnChart && !TickObjs.isLoadingComplete)
                LoadMoreTicksOnChart();

            bool isOnChartAsync = LoadTickStrategy_Input == LoadTickStrategy_Data.On_ChartEnd_Async;
            if (isOnChartAsync && !TickObjs.isLoadingComplete)
                return;

            // Removing Messages
            if (!IsLastBar) {
                DrawOnScreen("");
                Second_DrawOnScreen("");
            }

            // For some reason, the OrderFlow() call doesn't seem to be enough.
            LockODFTemplate();

            // ==== Chart Segmentation ====
            CreateSegments(index);

            /*
               After Initialize() or Indicator's restart, when loading settings that have Week/Month Profiles
               Calculate() will draw any period that Tick Data is available
               instead of drawing at current lookback DATE as ClearAndRecalculate() loop.
               It's expected but not desired behavior.
            */
            if (PanelSwitch_Input != PanelSwitch_Data.Order_Flow_Ticks && !IsLastBar) {
                CreateMonthlyVP(index);
                CreateWeeklyVP(index);
            }

            // LookBack
            Bars ODF_Bars = MiscParams.ODFInterval_Input == ODFInterval_Data.Daily ? DailyBars : WeeklyBars;

            // Get Index of ODF Interval to continue only in Lookback
            int iVerify = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
            if (ODF_Bars.ClosePrices.Count - iVerify > GeneralParams.Lookback)
                return;

            int TF_idx = ODF_Bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);
            int indexStart = Bars.OpenTimes.GetIndexByTime(ODF_Bars.OpenTimes[TF_idx]);

            // ODF/VP => Reset filters and main VP
            if (index == indexStart ||
                (index - 1) == indexStart && isPriceBased_Chart ||
                (index - 1) == indexStart && (index - 1) != ClearIdx.MainVP
            )
                MassiveCleanUp(indexStart, index);

            // Historical data
            if (!IsLastBar) {
                // Required for [Ultra Bubbles, Spike] Levels in Historical Data
                BooleanLocks.LevelsToFalse();

                if (PanelSwitch_Input != PanelSwitch_Data.Volume_Profile) 
                {
                    CreateOrderFlow(index);
                    /*
                    if (!isPriceBased_Chart)
                        CreateOrderFlow(index);
                    else {
                        // PriceGap condition can't handle very strong gaps
                        try { CreateOrderFlow(index); } catch { };
                    }
                    */
                }

                if (PanelSwitch_Input != PanelSwitch_Data.Order_Flow_Ticks) {
                    if (ProfileParams.EnableMainVP)
                        VolumeProfile(indexStart, index);
                    
                    CreateMiniVPs(index);
                }

                BooleanUtils.isUpdateVP = true; // chart end
            }
            else
            {
                if (PanelSwitch_Input != PanelSwitch_Data.Volume_Profile) 
                {
                    // Required for Non-Time based charts (Renko, Range, Ticks)
                    if (BooleanUtils.isPriceBased_NewBar) {
                        lockNotifyInPriceBased(true);

                        CreateOrderFlow(index - 1);
                        BooleanUtils.isPriceBased_NewBar = false;

                        lockNotifyInPriceBased(false);
                        return;
                    }
                    CreateOrderFlow(index);
                }
                
                if (PanelSwitch_Input != PanelSwitch_Data.Order_Flow_Ticks) 
                {
                    // Live VP
                    if (UpdateVPStrategy_Input == UpdateVPStrategy_Data.SameThread_MayFreeze)
                    {
                        if (ProfileParams.EnableMainVP)
                            LiveVP_Update(indexStart, index);
                        else if (!ProfileParams.EnableMainVP && ProfileParams.EnableMiniProfiles)
                            LiveVP_Update(indexStart, index, true);
                    }
                    else
                        LiveVP_Concurrent(index, indexStart);
                }
            }
        }

        void CreateOrderFlow(int idx)
        {
            VolumesRank.Clear();
            VolumesRank_Up.Clear();
            VolumesRank_Down.Clear();
            DeltaRank.Clear();
            int[] resetDelta = {0, 0};
            MinMaxDelta = resetDelta;
            OrderFlow(idx);
        }
        void lockNotifyInPriceBased(bool value) {
            BooleanLocks.spikeNotify = value;
            BooleanLocks.ultraNotify = !value;
        }

        private Dictionary<string, object> BuildExportPayload(int iStart)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                ["open"] = Bars.OpenPrices[iStart],
                ["high"] = Bars.HighPrices[iStart],
                ["low"] = Bars.LowPrices[iStart],
                ["close"] = Bars.ClosePrices[iStart],
                ["volumesRank"] = VolumesRank,
                ["volumesRankUp"] = VolumesRank_Up,
                ["volumesRankDown"] = VolumesRank_Down,
                ["deltaRank"] = DeltaRank,
                ["minMaxDelta"] = MinMaxDelta,
                ["spread"] = Symbol.Spread
            };

            Dictionary<string, object> sourceMeta = new Dictionary<string, object>
            {
                ["symbol"] = Symbol.Name,
                ["timeframe"] = Chart.TimeFrame.ShortName,
                ["legacy_type"] = ExportEventName
            };

            return BuildContractEnvelope(iStart, payload, sourceMeta);
        }

        private void AppendDirectCsv(Dictionary<string, object> exportData)
        {
            if (!_isManualCsvExportInProgress)
                return;

            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder.Trim();
            Directory.CreateDirectory(outputFolder);

            string symbol = ResolveExportSymbol(exportData, sourceMeta: null);
            string filePath = Path.Combine(outputFolder, $"history_orderflowaggregated_{symbol}.csv");
            bool writeHeader = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;

            using (StreamWriter writer = new StreamWriter(filePath, true, Utf8NoBom))
            {
                if (writeHeader)
                    writer.WriteLine(string.Join(",", ExportCsvHeaders));

                var payload = exportData["payload"] as Dictionary<string, object>;
                var sourceMeta = exportData["source_meta"] as Dictionary<string, object>;
                if (payload == null || sourceMeta == null)
                    return;

                Dictionary<double, double> volumesRank = GetDoubleDictionary(payload, "volumesRank");
                Dictionary<double, double> volumesRankUp = GetDoubleDictionary(payload, "volumesRankUp");
                Dictionary<double, double> volumesRankDown = GetDoubleDictionary(payload, "volumesRankDown");
                Dictionary<double, double> deltaRank = GetDoubleDictionary(payload, "deltaRank");
                double[] minMaxDelta = GetMinMaxDelta(payload);

                double[] orderedPriceLevels = volumesRank.Keys
                    .Concat(volumesRankUp.Keys)
                    .Concat(volumesRankDown.Keys)
                    .Concat(deltaRank.Keys)
                    .Distinct()
                    .OrderBy(price => price)
                    .ToArray();

                foreach (double priceLevel in orderedPriceLevels)
                {
                    string[] rowValues = new string[ExportCsvHeaders.Length];

                    for (int i = 0; i < ExportCsvHeaders.Length; i++)
                    {
                        string key = ExportCsvHeaders[i];
                        object value = ResolveFlattenedExportValue(
                            exportData,
                            sourceMeta,
                            payload,
                            key,
                            priceLevel,
                            volumesRank,
                            volumesRankUp,
                            volumesRankDown,
                            deltaRank,
                            minMaxDelta);
                        rowValues[i] = EscapeCsvValue(ConvertExportValue(value));
                    }

                    writer.WriteLine(string.Join(",", rowValues));
                }
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

            if (key == "type" && exportData.TryGetValue("event", out value))
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

        private Dictionary<double, double> GetDoubleDictionary(Dictionary<string, object> payload, string key)
        {
            if (!payload.TryGetValue(key, out object value) || value == null)
                return new Dictionary<double, double>();

            if (value is Dictionary<double, double> doubleDictionary)
                return new Dictionary<double, double>(doubleDictionary);

            if (value is Dictionary<double, int> intDictionary)
                return intDictionary.ToDictionary(entry => entry.Key, entry => (double)entry.Value);

            return new Dictionary<double, double>();
        }

        private double[] GetMinMaxDelta(Dictionary<string, object> payload)
        {
            if (payload.TryGetValue("minMaxDelta", out object value))
            {
                if (value is int[] intArray)
                    return intArray.Select(number => (double)number).ToArray();

                if (value is double[] doubleArray)
                    return doubleArray;
            }

            return new double[] { 0, 0 };
        }

        private object ResolveFlattenedExportValue(
            Dictionary<string, object> exportData,
            Dictionary<string, object> sourceMeta,
            Dictionary<string, object> payload,
            string key,
            double priceLevel,
            Dictionary<double, double> volumesRank,
            Dictionary<double, double> volumesRankUp,
            Dictionary<double, double> volumesRankDown,
            Dictionary<double, double> deltaRank,
            double[] minMaxDelta)
        {
            switch (key)
            {
                case "symbol":
                    return ResolveSourceMetaValue(exportData, sourceMeta, "symbol");
                case "timeframe":
                    return ResolveSourceMetaValue(exportData, sourceMeta, "timeframe");
                case "price_level":
                    return priceLevel;
                case "volume_total":
                    return volumesRank.TryGetValue(priceLevel, out double totalVolume) ? totalVolume : 0;
                case "volume_buy":
                    return volumesRankUp.TryGetValue(priceLevel, out double buyVolume) ? buyVolume : 0;
                case "volume_sell":
                    return volumesRankDown.TryGetValue(priceLevel, out double sellVolume) ? sellVolume : 0;
                case "delta":
                    return deltaRank.TryGetValue(priceLevel, out double delta) ? delta : 0;
                case "min_delta":
                    return minMaxDelta.Length > 0 ? minMaxDelta[0] : 0;
                case "max_delta":
                    return minMaxDelta.Length > 1 ? minMaxDelta[1] : 0;
                default:
                    return payload.TryGetValue(key, out object value) ? value : ResolveExportValue(exportData, key);
            }
        }

        private object ResolveSourceMetaValue(
            Dictionary<string, object> exportData,
            Dictionary<string, object> sourceMeta,
            string key)
        {
            if (sourceMeta.TryGetValue(key, out object value))
                return value;

            if (key == "symbol" && exportData.TryGetValue("instrument", out value))
                return value;

            return null;
        }

        private string ResolveExportSymbol(
            Dictionary<string, object> exportData,
            Dictionary<string, object> sourceMeta)
        {
            object symbolValue = sourceMeta != null
                ? ResolveSourceMetaValue(exportData, sourceMeta, "symbol")
                : ResolveExportValue(exportData, "symbol");
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

        private void ExportCsvData(int iStart)
        {
            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(iStart);
                AppendDirectCsv(exportData);
            }
            catch (Exception ex)
            {
                Print("CSV Export Error: " + ex.Message);
            }
        }

        public void SendSocketData(int iStart)
        {
            // Filter out empty bars: only send if volume profile data exists
            if (VolumesRank == null || VolumesRank.Count == 0) return;
            if (!EnsureSocketConnected())
            {
                _droppedEventsTotal++;
                return;
            }

            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(iStart);

                string jsonString = JsonSerializer.Serialize(exportData) + "\n";
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonString);
                try
                {
                    _tcpStream.Write(dataBytes, 0, dataBytes.Length);
                }
                catch (Exception)
                {
                    HandleSocketWriteFailure();
                }
            }
            catch (Exception) { }
        }

        private void MassiveCleanUp(int indexStart, int index) {
            // Reset VP
            // Segments are identified by TF_idx(start)
            // No need to clean up even if it's Daily Interval
            if (!IsLastBar)
                PerformanceTick.startIdx_MainVP = PerformanceTick.lastIdx_MainVP;
            VP_VolumesRank.Clear();
            VP_VolumesRank_Up.Clear();
            VP_VolumesRank_Down.Clear();
            VP_VolumesRank_Subt.Clear();
            VP_DeltaRank.Clear();
            ClearIdx.MainVP = index == indexStart ? index : (index - 1);

            // Reset Filters
            /*
            A high memory usage at the first indicator's startup or after being compiled was located here,
            where I just repeat the "// Filters" Initialize() code.
            - CreateDataSeries() first call outside Initialize() leads to massive RAM usage, even if it's suppose to be empty,
              then the GC does it job.
            - Subsequents calls doesn't affect anything, just increase 5-7mb of ram incrementally.

            Also, I did a simple test where just one rectangle with the same ID(name), is created/update every Calculate() call:
                Chart.DrawRectangle("eat-ram-now", Bars.OpenTimes[index - 1], Bars.OpenPrices[index - 1], Bars.OpenTimes[index], Bars.ClosePrices[index - 1], Color.Red);
            After the first tick update, the memory usage grows up quickly (Max 3gb) then (maybe GC does it) return to normality.

            - This happens ONLY ONCE, at the first custom indicator's / cTrader startup
            - Since everything works as expected afterwards, it's OK... I suppose.

            - In this test, no "Algo" instances or multiples charts were open, just:
                - Open Task Manager
                - Open cTrader > Trade > 1 EURUSD chart > add "eat-ram-now" test indicator > wait the first tick
                - Watch the cTrader memory usage rise and fall quickly.

            // ==================================

            Now about the Filters:

            At lower timeframes like 1m (high bars count) WITH ANY FILTER ACTIVATED:
            - AsyncTickLoading or Calculate() first call, everything is drawn at the speed of light...

            However, when ClearAndRecalculate() is called from ParamsPanel:
            - Any(even one) .Result[index] of filters(MAs, StdDev) in Loop Segments of OrderFlow(),
            causes a SEVERE SLOWDOWN in Loop performance, no matter if the recalculations are made insider a Timer.

            So, in order to reach the same performance as Calculate() with the Filters activated:
             - Custom MAs implementation is required.
            */            
            
            if (UseCustomMAs) {
                // Any
                _dynamicBuffer.Clear();
                _maDynamic.Clear();
                // Delta(only)
                _deltaBuffer.ClearAll();
            }
            else {
                for (int i = 0; i < Bars.Count; i++)
                {
                    Dynamic_Series[i] = double.NaN;
                    DeltaChange_Series[i] = double.NaN;
                    DeltaBuySell_Sum_Series[i] = double.NaN;
                    SubtractDelta_Series[i] = double.NaN;
                    SumDelta_Series[i] = double.NaN;
                    PercentageRatio_Series[i] = double.NaN;
                    PercentileRatio_Series[i] = double.NaN;
                }
                CreateOrReset_cTraderIndicators();
            }
            

            // Reset Levels
            if (BubblesLevelParams.ResetDaily && BubblesChartParams.EnableBubblesChart)
            {
                foreach (var rect in ultraRectangles.Values) {
                    if (rect.isActive) {
                        rect.Rectangle.Time2 = Bars.OpenTimes[indexStart];
                    }
                }

                ultraRectangles.Clear();
            }
            if (SpikeLevelParams.ResetDaily && SpikeFilterParams.EnableSpikeFilter)
            {
                foreach (var rect in spikeRectangles.Values) {
                    if (rect.isActive) {
                        rect.Rectangle.Time2 = Bars.OpenTimes[indexStart];
                    }
                }

                spikeRectangles.Clear();
            }
        }
        private void CreateOrReset_cTraderIndicators() {
            // Large
            MADynamic_LargeFilter = Indicators.MovingAverage(Dynamic_Series, ResultParams.MAperiod, ResultParams.MAtype);
            MASubtract_LargeFilter = Indicators.MovingAverage(SubtractDelta_Series, ResultParams.MAperiod, ResultParams.MAtype);

            // Bubbles
            MABubbles_Delta = Indicators.MovingAverage(Dynamic_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            MABubbles_DeltaBuySell_Sum = Indicators.MovingAverage(DeltaBuySell_Sum_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            MABubbles_DeltaChange = Indicators.MovingAverage(DeltaChange_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            MABubbles_SubtractDelta = Indicators.MovingAverage(SubtractDelta_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            MABubbles_SumDelta = Indicators.MovingAverage(SumDelta_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);

            StdDevBubbles_Delta = Indicators.StandardDeviation(Dynamic_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            StdDevBubbles_DeltaChange = Indicators.StandardDeviation(DeltaChange_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            StdDevBubbles_DeltaBuySell_Sum = Indicators.StandardDeviation(DeltaBuySell_Sum_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            StdDevBubbles_SubtractDelta = Indicators.StandardDeviation(SubtractDelta_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);
            StdDevBubbles_SumDelta = Indicators.StandardDeviation(SumDelta_Series, BubblesChartParams.MAperiod, BubblesChartParams.MAtype);

            // Spike
            MASpike_Delta = Indicators.MovingAverage(Dynamic_Series, SpikeFilterParams.MAperiod, SpikeFilterParams.MAtype);
            MASpike_DeltaBuySell_Sum = Indicators.MovingAverage(DeltaBuySell_Sum_Series, SpikeFilterParams.MAperiod, SpikeFilterParams.MAtype);
            MASpike_SumDelta = Indicators.MovingAverage(SumDelta_Series, SpikeFilterParams.MAperiod, SpikeFilterParams.MAtype);

            StdDevSpike_Delta = Indicators.StandardDeviation(Dynamic_Series, SpikeFilterParams.MAperiod, SpikeFilterParams.MAtype);
            StdDevSpike_DeltaBuySell_Sum = Indicators.StandardDeviation(DeltaBuySell_Sum_Series, SpikeFilterParams.MAperiod, SpikeFilterParams.MAtype);
            StdDevSpike_SumDelta = Indicators.StandardDeviation(SumDelta_Series, SpikeFilterParams.MAperiod, SpikeFilterParams.MAtype);

            // Spike => Percentage Ratio
            MARatio_Percentage =  Indicators.MovingAverage(PercentageRatio_Series, SpikeRatioParams.MAperiod_PctSpike, MovingAverageType.Simple);
        }

        // *********** ORDER FLOW TICKS ***********
        private void LockODFTemplate() {
            // Lock Bubbles Chart template
            if (BubblesChartParams.EnableBubblesChart) {
                MiscParams.ShowHist = false;
                MiscParams.ShowNumbers = false;
                ProfileParams.EnableMainVP = false;
                ProfileParams.EnableMiniProfiles = false;
                SpikeFilterParams.EnableSpikeFilter = false;
                ResultParams.ShowResults = false;
                
            }
            // Lock Spike Chart template
            if (SpikeFilterParams.EnableSpikeChart) {
                SpikeFilterParams.EnableSpikeFilter = true;
                MiscParams.ShowHist = false;
                ResultParams.ShowResults = false;
            }
        }

    }
}
