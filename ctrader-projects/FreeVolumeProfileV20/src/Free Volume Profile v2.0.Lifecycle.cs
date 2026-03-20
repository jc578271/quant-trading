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
            if (_tcpClient != null && _tcpClient.Connected) return;

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

                Print("Starting Volume Profile Export...");
                ClearAndRecalculate();
                Print("Volume Profile Export Finished.");

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
            if (ExportHistory)
            {
                if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                    ExportCsvData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                    ExportCsvData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
            }
            else if (IsLastBar)
            {
                if (ProfileParams.EnableMainVP && VP_VolumesRank.Count > 0)
                    SendSocketData(index, "main", VP_VolumesRank, VP_VolumesRank_Up, VP_VolumesRank_Down, VP_DeltaRank, VP_MinMaxDelta);

                if (ProfileParams.EnableMiniProfiles && MiniRank.Normal.Count > 0)
                    SendSocketData(index, "mini", MiniRank.Normal, MiniRank.Up, MiniRank.Down, MiniRank.Delta, MiniRank.MinMaxDelta);
            }
        }

        public void SendSocketData(int index, string profileType, Dictionary<double, double> volRank, Dictionary<double, double> volUp, Dictionary<double, double> volDown, Dictionary<double, double> deltaRank, double[] minMaxDelta)
        {
            try
            {
                Dictionary<string, object> exportData = BuildExportPayload(index, profileType, volRank, volUp, volDown, deltaRank, minMaxDelta);

                string jsonString = JsonSerializer.Serialize(exportData);
                byte[] data = Encoding.UTF8.GetBytes(jsonString + "\n");
                _networkStream?.Write(data, 0, data.Length);
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
            if (!DirectCsvExport || !_isManualCsvExportInProgress)
                return;

            string outputFolder = string.IsNullOrWhiteSpace(CsvOutputFolder) ? DefaultCsvOutputFolder : CsvOutputFolder.Trim();
            Directory.CreateDirectory(outputFolder);

            string filePath = Path.Combine(outputFolder, "history_volumeprofile.csv");
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
