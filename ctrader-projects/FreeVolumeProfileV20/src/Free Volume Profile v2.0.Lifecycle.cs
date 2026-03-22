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
    public partial class FreeVolumeProfileV20 : Indicator
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

                Print("Starting Volume Profile Export...");
                ClearAndRecalculate();
                Print("Volume Profile Export Finished.");
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
                Orientation = Orientation.Horizontal,
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
            if (_isManualCsvExportInProgress)
            {
                if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                    ExportCsvData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                    ExportCsvData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
            }

            if (IsLastBar)
            {
                if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                    SendSocketData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                    SendSocketData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
            }
        }

        public void SendSocketData(int index, string profileType, Dictionary<double, double> volRank, Dictionary<double, double> volUp, Dictionary<double, double> volDown, Dictionary<double, double> deltaRank, double[] minMaxDelta)
        {
            if (!EnsureSocketConnected())
            {
                _droppedEventsTotal++;
                return;
            }

            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, profileType, volRank, volUp, volDown, deltaRank, minMaxDelta);

                string jsonString = JsonSerializer.Serialize(exportData);
                byte[] data = Encoding.UTF8.GetBytes(jsonString + "\n");
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

            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                ["profile_type"] = profileType,
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

            Dictionary<string, object> sourceMeta = new Dictionary<string, object>
            {
                ["symbol"] = Symbol.Name,
                ["timeframe"] = Chart.TimeFrame.ShortName,
                ["legacy_type"] = ExportEventName
            };

            return BuildContractEnvelope(index, payload, sourceMeta);
        }

        private void AppendDirectCsv(Dictionary<string, object> exportData)
        {
            if (!_isManualCsvExportInProgress)
                return;

            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder.Trim();
            Directory.CreateDirectory(outputFolder);

            string symbol = ResolveExportSymbol(exportData, sourceMeta: null);
            string filePath = Path.Combine(outputFolder, $"history_volumeprofile_{symbol}.csv");
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
            if (payload.TryGetValue(key, out object value) && value is Dictionary<double, double> dictionary)
                return dictionary;

            return new Dictionary<double, double>();
        }

        private double[] GetMinMaxDelta(Dictionary<string, object> payload)
        {
            if (payload.TryGetValue("minMaxDelta", out object value))
            {
                if (value is double[] doubleArray)
                    return doubleArray;

                if (value is int[] intArray)
                    return intArray.Select(number => (double)number).ToArray();
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
                case "profile_type":
                    return payload.TryGetValue("profile_type", out object profileType) ? profileType : string.Empty;
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

    }
}
