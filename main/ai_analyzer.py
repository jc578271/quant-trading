import logging
import os
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
import joblib
from order_simulator import OrderSimulator

class AIAnalyzer:
    def __init__(self, mt5_client, model_path="model.pkl", sim_config=None):
        self.mt5_client = mt5_client
        self.raw_data_buffer = {}  # Map: symbol -> list of raw data dicts
        self.latest_state = {}     # Map: symbol -> current combined state
        self.order_book = {}
        self.last_buffer_process_time = 0
        self.model_path = model_path
        
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

    def _load_or_train_initial_model(self):
        """Tải mô hình AI thật, nếu chưa có thì train một mô hình giả lập ban đầu."""
        if os.path.exists(self.model_path):
            logging.info(f"Loading existing AI model from {self.model_path}...")
            return joblib.load(self.model_path)
            
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
        return model

    def export_individual_csv(self, symbol, data):
        """Dynamic CSV export based on received data keys, replacing hard-coded logic."""
        import csv
        import json
        import os
        
        msg_type = data.get("event", data.get("type", "unknown"))
        if msg_type == "unknown":
            if "wyckoffVolume" in data: msg_type = "wyckoff"
            elif "vpPOC" in data: msg_type = "volumeprofile"
            elif "deltaRank" in data: msg_type = "orderflow"
            
        filename = f"history_{msg_type.lower().replace('_', '')}.csv"
        
        row = data.copy()
        row["symbol"] = symbol
        
        # Serialize dicts/lists to JSON strings
        for k, v in row.items():
            if isinstance(v, (dict, list)):
                row[k] = json.dumps(v)
        
        # Dynamic header management: Read existing headers or create new ones
        file_exists = os.path.isfile(filename) and os.path.getsize(filename) > 0
        fieldnames = []
        if file_exists:
            try:
                with open(filename, 'r', encoding='utf-8') as f:
                    fieldnames = next(csv.reader(f), [])
            except Exception:
                fieldnames = []
        
        # Append any new keys discovered in this row to the headers
        current_keys = sorted(row.keys())
        for k in current_keys:
            if k not in fieldnames:
                fieldnames.append(k)
        
        if not fieldnames:
            fieldnames = current_keys

        try:
            with open(filename, 'a', newline='', encoding='utf-8') as f:
                writer = csv.DictWriter(f, fieldnames=fieldnames, extrasaction='ignore')
                if not file_exists:
                    writer.writeheader()
                writer.writerow(row)
        except Exception as e:
            logging.error(f"CSV Export Error for {filename}: {e}")

    def export_alert_csv(self, symbol, data):
        """Export Bookmap alerts to symbol-specific CSV files."""
        import csv
        import os
        
        # Sanitize symbol for filename
        clean_symbol = "".join(c if c.isalnum() or c in ".-" else "_" for c in symbol)
        filename = f"history_alert_{clean_symbol}.csv"
        
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
        
        file_exists = os.path.isfile(filename) and os.path.getsize(filename) > 0
        fieldnames = ["Timestamp", "AlertNumber", "Symbol", "AlertName", "Value", "Price", "Popup", "RawText"]
        
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
            
        # Dynamic export for indicators
        merged_data = dict(data)
        merged_data.update(payload)
        merged_data["instrument"] = symbol
        if "symbol" not in merged_data and "symbol" in source_meta:
            merged_data["symbol"] = source_meta["symbol"]
        self.export_individual_csv(symbol, merged_data)
        
        if symbol not in self.raw_data_buffer:
            self.raw_data_buffer[symbol] = []
            self.latest_state[symbol] = {}
            
        self.raw_data_buffer[symbol].append(merged_data)
        
        now = time.time()
        if len(self.raw_data_buffer[symbol]) > 100 or (now - self.last_buffer_process_time > 2.0):
            self._process_buffer(symbol)
            self.last_buffer_process_time = now

    def _process_buffer(self, symbol):
        buffer = self.raw_data_buffer[symbol]
        if not buffer: return
        
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
                
                # Only trade if model is confident
                if probability > 0.65:
                    if prediction == 1:
                        self.simulator.open_trade(1, symbol, current_price, spread)
                    elif prediction == -1:
                        self.simulator.open_trade(-1, symbol, current_price, spread)
                        
            except Exception as e:
                logging.error(f"AI Prediction Error: {e}")
                
        # Clear buffer after processing
        self.raw_data_buffer[symbol] = []

    def shutdown(self):
        """Print final session summary."""
        self.simulator.print_summary()
