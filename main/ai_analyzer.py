import logging
import os
from order_simulator import OrderSimulator
from runtime_paths import (
    ALERT_HISTORY_HEADERS,
    event_history_file,
    event_history_headers,
    alert_history_file,
    model_file,
)

class AIAnalyzer:
    def __init__(self, mt5_client, model_path=None, sim_config=None, status=None):
        self.mt5_client = mt5_client
        self.status = status
        self.raw_data_buffer = {}  # Map: symbol -> list of raw data dicts
        self.latest_state = {}     # Map: symbol -> current combined state
        self.order_book = {}
        self.last_buffer_process_time = 0
        self.model_path = model_path or str(model_file())
        self.inference_errors_total = 0
        
        # Feature definition for the model
        self.feature_keys = ["delta", "wyckoff_wave", "poc_distance", "tick_vol"]
        self.model = self._load_or_train_initial_model()
        
        # Order Simulator
        self.simulator = OrderSimulator(sim_config or {
            "balance": 10000,
            "min_rr": 2.0,
            "sl_mode": "fixed",
            "sl_value": 20,
            "tp_mode": "rr",
            "tp_value": 2.0,
            "lot_mode": "auto",
            "lot_value": 1.0,
            "pip_size": 0.01,
            "pip_value_per_lot": 10.0,
        })

    def _publish_buffering_status(self, reason, state="degraded"):
        if self.status is None:
            return
        buffered_symbols = sum(1 for records in self.raw_data_buffer.values() if records)
        queued_records = sum(len(records) for records in self.raw_data_buffer.values())
        self.status.update_stage(
            "buffering",
            state=state,
            reason=reason,
            buffered_symbols=buffered_symbols,
            queued_records=queued_records,
        )
        self.status.publish()

    def _publish_inference_status(self, reason, state="up", last_inference_at=None):
        if self.status is None:
            return
        self.status.update_stage(
            "inference",
            state=state,
            reason=reason,
            model_loaded=self.model is not None,
            last_inference_at=last_inference_at,
            inference_errors_total=self.inference_errors_total,
        )
        self.status.publish()

    def _load_or_train_initial_model(self):
        """Tải mô hình AI thật, nếu chưa có thì train một mô hình giả lập ban đầu."""
        try:
            import joblib
            import numpy as np
            from sklearn.ensemble import RandomForestClassifier

            if os.path.exists(self.model_path):
                logging.info(f"Loading existing AI model from {self.model_path}...")
                model = joblib.load(self.model_path)
            else:
                logging.info("No AI model found. Training initial dummy model...")
                # Dummy data matches self.feature_keys: [delta, wyckoff_wave, poc_distance, tick_vol]
                X_train = np.array([
                    [ 1000,   50,   0.0010,  500],
                    [-1000,  -50,  -0.0010,  500],
                    [  100,    0,   0.0000,  100],
                    [  800,   30,   0.0005,  400],
                    [ -800,  -30,  -0.0005,  400],
                ])
                y_train = np.array([1, -1, 0, 1, -1])
                
                model = RandomForestClassifier(n_estimators=50, random_state=42)
                model.fit(X_train, y_train)
                
                joblib.dump(model, self.model_path)
                logging.info("Saved initial AI model to disk.")

            if self.status is not None:
                self.model = model
                self._publish_inference_status("model loaded", state="up")
            return model
        except ModuleNotFoundError as e:
            self.inference_errors_total += 1
            self.model = None
            logging.error(f"Model dependencies unavailable: {e}")
            self._publish_inference_status("model dependencies unavailable", state="degraded")
            return None
        except Exception as e:
            self.inference_errors_total += 1
            self.model = None
            self._publish_inference_status("model load failed", state="degraded")
            raise

    def export_individual_csv(self, symbol, data):
        """Export indicator events to stable runtime CSV files with fixed headers."""
        import csv
        import json
        
        msg_type = data.get("event", data.get("type", "unknown"))
        if msg_type == "unknown":
            if "wyckoffVolume" in data:
                msg_type = "wyckoff_state"
            elif "vpPOC" in data:
                msg_type = "volume_profile"
            elif "deltaRank" in data:
                msg_type = "order_flow_aggregated"

        headers = event_history_headers(msg_type)
        if not headers:
            logging.warning(f"Skipping runtime CSV export for unsupported event type: {msg_type}")
            return

        filename = event_history_file(msg_type)
        
        row = data.copy()
        row["symbol"] = symbol
        
        # Serialize dicts/lists to JSON strings
        for k, v in row.items():
            if isinstance(v, (dict, list)):
                row[k] = json.dumps(v)

        ordered_row = {header: row.get(header, "") for header in headers}
        file_exists = filename.is_file() and filename.stat().st_size > 0
        filename.parent.mkdir(parents=True, exist_ok=True)

        try:
            with open(filename, 'a', newline='', encoding='utf-8') as f:
                writer = csv.DictWriter(f, fieldnames=list(headers), extrasaction='ignore')
                if not file_exists:
                    writer.writeheader()
                writer.writerow(ordered_row)
        except Exception as e:
            logging.error(f"CSV Export Error for {filename}: {e}")

    def export_alert_csv(self, symbol, data):
        """Export Bookmap alerts to symbol-specific CSV files."""
        import csv
        import os
        
        # Sanitize symbol for filename
        clean_symbol = "".join(c if c.isalnum() or c in ".-" else "_" for c in symbol)
        filename = alert_history_file(clean_symbol)
        
        # Parse specific fields from the text
        import re
        text = data.get("text", "")
        
        # 1. AlertName: Extract from start of message, after symbol if present
        # Example: "HIDDEN ASK DEVELOPMENT" or "Stop buy"
        alert_name = "Unknown"
        # Look for the part after the symbol and colon, or the start of a stop/market message
        name_match = re.search(r'(?::\s+)?(HIDDEN\s+[A-Z]+\s+DEVELOPMENT|Stop\s+[a-z]+|Market\s+[a-z]+)', text, re.I)
        if name_match:
            alert_name = name_match.group(1).strip()
        elif ":" in text:
            alert_name = text.split(":", 1)[1].split(",")[0].strip()

        # 2. Value: Extract numeric values from "V: 15" or "Volume: 15"
        value = ""
        val_match = re.search(r'(?:V|Volume):\s*([\d.]+)', text, re.I)
        if val_match:
            value = val_match.group(1)

        # 3. Price: Extract numeric price after "at"
        price = ""
        price_match = re.search(r'at\s+([\d.]+)', text, re.I)
        if price_match:
            price = price_match.group(1)

        # Prepare row matching the user's template
        # Row fields: Timestamp,AlertNumber,Symbol,AlertName,Value,Price,Popup,RawText
        row = {
            "Timestamp": data.get("timestamp"),
            "AlertNumber": data.get("count"),
            "Symbol": symbol,
            "AlertName": alert_name,
            "Value": value,
            "Price": price,
            "Popup": data.get("popup"),
            "RawText": text
        }
        
        file_exists = filename.is_file() and filename.stat().st_size > 0
        fieldnames = list(ALERT_HISTORY_HEADERS)
        filename.parent.mkdir(parents=True, exist_ok=True)
        
        try:
            with open(filename, 'a', newline='', encoding='utf-8') as f:
                writer = csv.DictWriter(f, fieldnames=fieldnames, extrasaction='ignore')
                if not file_exists:
                    writer.writeheader()
                writer.writerow(row)
            # logging.info(f"Alert exported to {filename}")
        except Exception as e:
            logging.error(f"Alert CSV Export Error for {filename}: {e}")

    def process_data(self, data):
        """
        Buffer incoming data from cTrader to handle asynchronous network delivery
        and mismatching timeframes perfectly via a chronological forward-fill mechanism.
        """
        import time
        if not data: return

        event = data.get("event", data.get("type", "indicator"))
        payload = data.get("payload", data)
        if not isinstance(payload, dict):
            payload = data
        source_meta = data.get("source_meta") or {}
        symbol = data.get("instrument", data.get("symbol", source_meta.get("symbol", "EURUSD")))
        
        # Determine if this is a standard indicator or an alert
        msg_type = event
        
        if msg_type == "alert":
            alert_data = dict(data)
            alert_data.update(payload)
            self.export_alert_csv(symbol, alert_data)
            return
        elif msg_type == "dom":
            action = payload.get("action", data.get("action"))
            alias = payload.get("alias", data.get("alias", source_meta.get("alias", symbol)))
            is_bid = payload.get("isBid", data.get("isBid"))
            price = payload.get("price", data.get("price"))
            size = payload.get("size", data.get("size"))
            
            # Update local order book state
            if alias not in self.order_book:
                self.order_book[alias] = {True: {}, False: {}}
            
            side_book = self.order_book[alias][is_bid]
            if size == 0:
                side_book.pop(price, None)
            else:
                side_book[price] = size
            
            # For now, we only log if min volume reached, or just keep it quiet
            # In the future, we can add OBI calculations here.
            return
            
        elif msg_type == "dot":
            return
            
        elif msg_type == "wall":
            return

        merged_data = dict(data)
        merged_data.update(payload)
        merged_data["instrument"] = symbol
        if "symbol" not in merged_data and "symbol" in source_meta:
            merged_data["symbol"] = source_meta["symbol"]

        if symbol not in self.raw_data_buffer:
            self.raw_data_buffer[symbol] = []
            self.latest_state[symbol] = {}
            
        self.raw_data_buffer[symbol].append(merged_data)
        self._publish_buffering_status("awaiting buffered data", state="up")
        
        now = time.time()
        if len(self.raw_data_buffer[symbol]) > 100 or (now - self.last_buffer_process_time > 2.0):
            self._process_buffer(symbol)
            self.last_buffer_process_time = now

    def _process_buffer(self, symbol):
        buffer = self.raw_data_buffer[symbol]
        if not buffer: return
        self._publish_buffering_status("processing buffered data", state="up")
        if self.model is None:
            self._publish_inference_status("model dependencies unavailable", state="degraded")
            self.raw_data_buffer[symbol] = []
            self._publish_buffering_status("awaiting normalized records", state="degraded")
            return

        import numpy as np
        
        # Sort chronologically by timestamp
        buffer.sort(key=lambda x: x.get("timestamp", ""))
        
        state = self.latest_state[symbol]
        
        for data in buffer:
            state.update(data) # Forward fill missing keys intelligently
            
            # Require at least some indicators to be present
            if "wyckoffVolume" not in state and "deltaRank" not in state and "vpPOC" not in state:
                continue
                
            current_price = data.get("close") or state.get("close", 0)
            spread = state.get("spread", 0)
            
            # Check open trades on EVERY tick
            self.simulator.check_open_trades(symbol, current_price)
            
            # Extract features robustly
            def get_sum(val):
                if isinstance(val, dict): return sum(val.values())
                if isinstance(val, list): return sum(val)
                try: return float(val)
                except: return 0

            delta = get_sum(state.get("deltaRank", 0))
            tick_vol = get_sum(state.get("volumesRank", 0))
            wyckoff_wave = state.get("waveVolume", 0) * (1 if state.get("waveDirection") == "Up" else -1)
            poc_distance = current_price - state.get("vpPOC", current_price)
            
            # Perform inference if features are meaningful
            features = np.array([[delta, wyckoff_wave, poc_distance, tick_vol]])
            
            if np.all(features == 0):
                continue

            try:
                prediction = self.model.predict(features)[0]
                probability = np.max(self.model.predict_proba(features))
                self._publish_inference_status(
                    "model loaded",
                    state="up",
                    last_inference_at=data.get("timestamp"),
                )
                
                # Only trade if model is confident
                if probability > 0.65:
                    if prediction == 1:
                        self.simulator.open_trade(1, symbol, current_price, spread)
                    elif prediction == -1:
                        self.simulator.open_trade(-1, symbol, current_price, spread)
                        
            except Exception as e:
                logging.error(f"AI Prediction Error: {e}")
                self.inference_errors_total += 1
                self._publish_inference_status("prediction failed", state="degraded")
                
        # Clear buffer after processing
        self.raw_data_buffer[symbol] = []
        self._publish_buffering_status("awaiting normalized records", state="degraded")

    def shutdown(self):
        """Print final session summary."""
        self.simulator.print_summary()
