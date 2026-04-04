using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using cAlgo.API;

namespace cAlgo
{
    public partial class OhlcTrainingExporterV10 : Indicator
    {
        private static readonly TimeSpan SocketReconnectDelay = TimeSpan.FromMilliseconds(ReconnectDelayMs);
        private static readonly TimeSpan SocketHeartbeatInterval = TimeSpan.FromMilliseconds(HeartbeatIntervalMs);

        protected override void Initialize()
        {
            TryResolveFromDateTime(showErrors: false, out _selectedFromDateTimeUtc);
            ConnectSocket();
            CreateParamsPanel();
            AddExportButton();
            Timer.Start(TimeSpan.FromSeconds(1));
        }

        public override void Calculate(int index)
        {
            if (_isManualCsvExportInProgress)
                ExportCsvData(index);

            if (IsLastBar)
                SendSocketData(index);
        }

        protected override void OnTimer()
        {
            RunSocketHeartbeat();
            UpdateSocketStatusDisplay();
        }

        protected override void OnDestroy()
        {
            DisconnectSocket();
            base.OnDestroy();
        }

        public void ClearAndRecalculate()
        {
            Thread.Sleep(100);

            for (int index = 0; index < Bars.Count; index++)
                ExportCsvData(index);
        }

        private void ConnectSocket()
        {
            EnsureSocketConnected(force: true);
        }

        private void DisconnectSocket()
        {
            try
            {
                _networkStream?.Close();
            }
            catch
            {
            }

            try
            {
                _tcpClient?.Close();
            }
            catch
            {
            }

            _networkStream = null;
            _tcpClient = null;
            SetSocketState("socket disconnected");
        }

        private void ReconnectEvent(ButtonClickEventArgs args)
        {
            DisconnectSocket();
            _nextReconnectAtUtc = DateTime.MinValue;
            ConnectSocket();
        }

        private void ExportEvent(ButtonClickEventArgs args)
        {
            if (_exportButton != null)
                _exportButton.IsEnabled = false;

            try
            {
                if (!TryResolveFromDateTime(showErrors: true, out _selectedFromDateTimeUtc))
                    return;

                ConnectSocket();
                _isManualCsvExportInProgress = true;
                ClearAndRecalculate();
            }
            catch (Exception ex)
            {
                Print("OHLC export failed: " + ex.Message);
                Notifications.ShowPopup("OHLC Export", ex.Message, PopupNotificationState.Error);
            }
            finally
            {
                _isManualCsvExportInProgress = false;
                if (_exportButton != null)
                    _exportButton.IsEnabled = true;
            }
        }

        private void ExportCsvData(int index)
        {
            if (!TryResolveFromDateTime(showErrors: false, out var fromDateTimeUtc))
                return;

            DateTime barTime = Bars.OpenTimes[index];
            if (barTime < fromDateTimeUtc)
                return;

            AppendDirectHistoryJsonl(BuildHistoryRecord(index));
        }

        private Dictionary<string, object> BuildHistoryRecord(int index)
        {
            string symbol = Symbol.Name;
            DateTime barTime = Bars.OpenTimes[index];
            double tickSize = Symbol.TickSize == 0 ? 1.0 : Symbol.TickSize;

            return new Dictionary<string, object>
            {
                ["schema"] = HistoryFileSchema,
                ["source"] = EventSource,
                ["source_instance"] = SourceInstanceName,
                ["event"] = ExportEventName,
                ["event_id"] = BuildEventId(symbol, barTime),
                ["instrument"] = symbol,
                ["timestamp"] = barTime.ToString("o"),
                ["bar_closed"] = index < Bars.Count - 1,
                ["bar"] = new Dictionary<string, object>
                {
                    ["open"] = Bars.OpenPrices[index],
                    ["high"] = Bars.HighPrices[index],
                    ["low"] = Bars.LowPrices[index],
                    ["close"] = Bars.ClosePrices[index],
                    ["spread"] = Symbol.Spread,
                    ["tick_size"] = tickSize,
                    ["range_ticks"] = (int)Math.Round((Bars.HighPrices[index] - Bars.LowPrices[index]) / tickSize, MidpointRounding.AwayFromZero)
                },
                ["source_meta"] = new Dictionary<string, object>
                {
                    ["symbol"] = symbol,
                    ["timeframe"] = Chart.TimeFrame.ToString(),
                    ["history_mode"] = true
                }
            };
        }

        private Dictionary<string, object> BuildExportPayload(int index)
        {
            string symbol = Symbol.Name;
            DateTime barTime = Bars.OpenTimes[index];

            return new Dictionary<string, object>
            {
                ["schema"] = EventContractSchema,
                ["source"] = EventSource,
                ["source_instance"] = SourceInstanceName,
                ["event"] = ExportEventName,
                ["event_id"] = BuildEventId(symbol, barTime),
                ["instrument"] = symbol,
                ["timestamp"] = barTime.ToString("o"),
                ["payload"] = new Dictionary<string, object>
                {
                    ["open"] = Bars.OpenPrices[index],
                    ["high"] = Bars.HighPrices[index],
                    ["low"] = Bars.LowPrices[index],
                    ["close"] = Bars.ClosePrices[index],
                    ["spread"] = Symbol.Spread,
                    ["tick_size"] = Symbol.TickSize
                },
                ["source_meta"] = new Dictionary<string, object>
                {
                    ["symbol"] = symbol,
                    ["timeframe"] = Chart.TimeFrame.ToString()
                }
            };
        }

        private void AppendDirectHistoryJsonl(Dictionary<string, object> record)
        {
            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder;
            Directory.CreateDirectory(outputFolder);

            string filePath = Path.Combine(outputFolder, $"history_ohlc_{SanitizeFileToken(Symbol.Name)}.jsonl");
            string jsonLine = JsonSerializer.Serialize(record) + Environment.NewLine;
            File.AppendAllText(filePath, jsonLine, Utf8NoBom);
        }

        private void SendSocketData(int index)
        {
            if (!IsLastBar)
                return;

            if (!EnsureSocketConnected())
                return;

            string json = JsonSerializer.Serialize(BuildExportPayload(index));
            SendSocketLine(json);
        }

        private void RunSocketHeartbeat()
        {
            if (!EnsureSocketConnected())
                return;

            if (DateTime.UtcNow < _nextHeartbeatAtUtc)
                return;

            SendHeartbeat();
        }

        private bool EnsureSocketConnected(bool force = false)
        {
            if (_tcpClient != null && _tcpClient.Connected && _networkStream != null)
                return true;

            DateTime now = DateTime.UtcNow;
            if (!force && now < _nextReconnectAtUtc)
                return false;

            _nextReconnectAtUtc = now.Add(SocketReconnectDelay);
            SetSocketState("socket reconnecting");

            try
            {
                DisconnectSocket();
                _tcpClient = new TcpClient(SocketHost, SocketPort);
                _networkStream = _tcpClient.GetStream();

                int helloReconnectCount = _hasConnectedOnce ? _reconnectCount + 1 : _reconnectCount;
                if (!SendConnectionHello(helloReconnectCount))
                    return false;

                _reconnectCount = helloReconnectCount;
                _hasConnectedOnce = true;
                _nextHeartbeatAtUtc = DateTime.UtcNow.Add(SocketHeartbeatInterval);
                SetSocketState("socket connected");
                return true;
            }
            catch
            {
                DisconnectSocket();
                return false;
            }
        }

        private bool SendConnectionHello(int reconnectCount)
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

            return SendSocketLine(JsonSerializer.Serialize(hello));
        }

        private void SendHeartbeat()
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

            if (SendSocketLine(JsonSerializer.Serialize(heartbeat)))
                _nextHeartbeatAtUtc = DateTime.UtcNow.Add(SocketHeartbeatInterval);
        }

        private bool SendSocketLine(string json)
        {
            if (_networkStream == null)
                return false;

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
                _networkStream.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch
            {
                _droppedEventsTotal++;
                DisconnectSocket();
                return false;
            }
        }

        private void SetSocketState(string state)
        {
            _socketState = state;
            UpdateSocketStatusDisplay();
        }

        private bool TryResolveFromDateTime(bool showErrors, out DateTime fromDateTimeUtc)
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime customDate;

            switch (LoadTickFrom_Input)
            {
                case LoadTickFrom_Data.Today:
                    fromDateTimeUtc = today;
                    break;
                case LoadTickFrom_Data.Yesterday:
                    fromDateTimeUtc = today.AddDays(-1);
                    break;
                case LoadTickFrom_Data.Before_Yesterday:
                    fromDateTimeUtc = today.AddDays(-2);
                    break;
                case LoadTickFrom_Data.One_Week:
                    fromDateTimeUtc = today.AddDays(-7);
                    break;
                case LoadTickFrom_Data.Two_Week:
                    fromDateTimeUtc = today.AddDays(-14);
                    break;
                case LoadTickFrom_Data.Monthly:
                    fromDateTimeUtc = today.AddMonths(-1);
                    break;
                case LoadTickFrom_Data.Custom:
                    if (!DateTime.TryParseExact(StringDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out customDate))
                    {
                        fromDateTimeUtc = today;
                        _hasDateSelectionError = true;
                        if (showErrors)
                        {
                            Notifications.ShowPopup(
                                "OHLC Export",
                                "Invalid date format. Use dd/MM/yyyy.",
                                PopupNotificationState.Error);
                        }

                        return false;
                    }

                    fromDateTimeUtc = DateTime.SpecifyKind(customDate.Date, DateTimeKind.Utc);
                    break;
                default:
                    fromDateTimeUtc = today;
                    break;
            }

            _hasDateSelectionError = false;
            return true;
        }
    }
}
