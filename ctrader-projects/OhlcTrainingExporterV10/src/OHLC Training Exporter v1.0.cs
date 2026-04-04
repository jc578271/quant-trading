using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public partial class OhlcTrainingExporterV10 : Indicator
    {
        private const string DefaultCsvOutputFolder = @"D:\projects\quant-trading\logs";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private const string HistoryFileSchema = "ohlc-history/v1";
        private const string EventContractSchema = "event-contract/v1";
        private const string EventSource = "ctrader";
        private const string SourceInstanceName = "OhlcTrainingExporterV10";
        private const string ExportEventName = "ohlc_bar";
        private const int SocketPort = 5555;
        private const string SocketHost = "127.0.0.1";
        private const int HeartbeatIntervalMs = 5000;
        private const int ReconnectDelayMs = 5000;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Button _exportButton;
        private bool _isManualCsvExportInProgress;
        private int _reconnectCount;
        private long _droppedEventsTotal;
        private string _socketState = "socket disconnected";

        private Border _paramsBorder;
        private StackPanel _paramsPanel;
        private TextBlock _socketStatusText;
        private ComboBox _loadFromComboBox;
        private TextBox _customDateTextBox;
        private TextBox _outputFolderTextBox;
        private ComboBox _panelAlignmentComboBox;
        private DateTime _selectedFromDateTimeUtc = DateTime.UtcNow.Date;
        private bool _hasDateSelectionError;
        private DateTime _nextHeartbeatAtUtc = DateTime.MinValue;
        private DateTime _nextReconnectAtUtc = DateTime.MinValue;
        private bool _hasConnectedOnce;
        private Border _controlsBorder;
        private Button _panelToggleButton;

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

        public enum PanelAlignment
        {
            Top_Left,
            Top_Center,
            Top_Right,
            Center_Left,
            Center_Right,
            Bottom_Left,
            Bottom_Center,
            Bottom_Right
        }

        [Parameter("Load From:", DefaultValue = LoadTickFrom_Data.Today, Group = "==== Export Settings ====")]
        public LoadTickFrom_Data LoadTickFrom_Input { get; set; }

        [Parameter("Custom (dd/mm/yyyy):", DefaultValue = "00/00/0000", Group = "==== Export Settings ====")]
        public string StringDate { get; set; }

        [Parameter("History Output Folder", DefaultValue = DefaultCsvOutputFolder, Group = "==== Python AI Export ====")]
        public string CsvOutputFolder { get; set; }

        [Parameter("Panel Alignment:", DefaultValue = PanelAlignment.Bottom_Right, Group = "==== Panel ====")]
        public PanelAlignment PanelAlignment_Input { get; set; }

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

        private string BuildEventId(string symbol, DateTime ts)
        {
            return $"ctrader-{ExportEventName}-{symbol}-{ts:o}";
        }
    }
}
