import logging
import os
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
import joblib

class AIAnalyzer:
    def __init__(self, mt5_client, model_path="model.pkl"):
        self.mt5_client = mt5_client
        self.raw_data_buffer = {}  # Map: symbol -> list of raw data dicts
        self.latest_state = {}     # Map: symbol -> current combined state
        self.last_buffer_process_time = 0
        self.model_path = model_path
        self.csv_wyckoff = "history_wyckoff.csv"
        self.csv_orderflow = "history_orderflow.csv"
        self.csv_vp = "history_volumeprofile.csv"
        self.model = self._load_or_train_initial_model()

    def _load_or_train_initial_model(self):
        """Tải mô hình AI thật, nếu chưa có thì train một mô hình giả lập ban đầu khù khờ."""
        if os.path.exists(self.model_path):
            logging.info(f"Loading existing AI model from {self.model_path}...")
            return joblib.load(self.model_path)
            
        logging.info("No AI model found. Training initial dummy model...")
        # Dummy data: Features = [Delta, Wyckoff_Wave, VP_POC_Distance, Tick_Volumes]
        # Target = 1 (BUY), -1 (SELL), 0 (HOLD)
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
        
        # Save model for future uses
        joblib.dump(model, self.model_path)
        logging.info("Saved initial AI model to disk.")
        return model

    def export_individual_csv(self, symbol, data):
        import csv
        import json
        
        # 1. Wyckoff Export
        if "wyckoffVolume" in data:
            file_exists = os.path.isfile(self.csv_wyckoff)
            with open(self.csv_wyckoff, 'a', newline='', encoding='utf-8') as f:
                headers = ["symbol", "timestamp", "open", "high", "low", "close", "waveVolume", "wavePrice", "waveDirection", "wyckoffVolume", "zigZag"]
                writer = csv.DictWriter(f, fieldnames=headers, extrasaction='ignore')
                if not file_exists: writer.writeheader()
                data["symbol"] = symbol
                writer.writerow(data)
                
        # 2. Order Flow Export
        if "deltaRank" in data or data.get("type") == "order_flow_aggregated":
            file_exists = os.path.isfile(self.csv_orderflow)
            with open(self.csv_orderflow, 'a', newline='', encoding='utf-8') as f:
                headers = ["symbol", "timestamp", "open", "high", "low", "close", "volumesRank", "volumesRankUp", "volumesRankDown", "deltaRank", "minMaxDelta"]
                writer = csv.DictWriter(f, fieldnames=headers, extrasaction='ignore')
                if not file_exists: writer.writeheader()
                row = data.copy()
                row["symbol"] = symbol
                for k in ["volumesRank", "volumesRankUp", "volumesRankDown", "deltaRank"]:
                    if k in row: row[k] = json.dumps(row[k])
                if "minMaxDelta" in row: row["minMaxDelta"] = json.dumps(row["minMaxDelta"])
                writer.writerow(row)
                
        # 3. Volume Profile Export
        if "vpPOC" in data:
            file_exists = os.path.isfile(self.csv_vp)
            with open(self.csv_vp, 'a', newline='', encoding='utf-8') as f:
                headers = ["symbol", "timestamp", "open", "high", "low", "close", "vpPOC", "vpVAH", "vpVAL", "vpTotalVolume", "vpProfileCount"]
                writer = csv.DictWriter(f, fieldnames=headers, extrasaction='ignore')
                if not file_exists: writer.writeheader()
                data["symbol"] = symbol
                writer.writerow(data)

    def process_data(self, data):
        """
        Buffer incoming data from cTrader to handle asynchronous network delivery
        and mismatching timeframes perfectly via a chronological forward-fill mechanism.
        """
        import time
        if not data: return

        symbol = data.get("symbol", "EURUSD")
        
        # EXPORT raw unmerged ticked data to 3 individual CSV files directly
        self.export_individual_csv(symbol, data)
        
        if symbol not in self.raw_data_buffer:
            self.raw_data_buffer[symbol] = []
            self.latest_state[symbol] = {}
            
        self.raw_data_buffer[symbol].append(data)
        
        # Every 100 ticks OR every 2 seconds, trigger the merge process
        now = time.time()
        if len(self.raw_data_buffer[symbol]) > 100 or (now - self.last_buffer_process_time > 2.0):
            self._process_buffer(symbol)
            self.last_buffer_process_time = now

    def _process_buffer(self, symbol):
        buffer = self.raw_data_buffer[symbol]
        if not buffer: return
        
        # Sort chronologically by timestamp
        # Timestamps are ISO strings like 2026-03-06T15:14:23.8640000Z
        buffer.sort(key=lambda x: x.get("timestamp", ""))
        
        state = self.latest_state[symbol]
        
        for data in buffer:
            state.update(data) # Forward fill missing keys intelligently
            
            # Check if we have received at least 1 data point from all 3 indicators
            if "deltaRank" not in state or "wyckoffVolume" not in state or "vpPOC" not in state:
                continue
                
            
            # Extract features for AI Prediction
            delta = 0
            if "deltaRank" in state and state["deltaRank"]:
                delta = sum(state["deltaRank"].values())
                
            tick_vol = 0
            if "volumesRank" in state and state["volumesRank"]:
                tick_vol = sum(state["volumesRank"].values())
                
            wyckoff_wave = state.get("waveVolume", 0) * (1 if state.get("waveDirection") == "Up" else -1)
            
            # Current price tracking
            current_price = state.get("close", 0)
            if data.get("close") is not None:
                current_price = data.get("close")
                
            poc = state.get("vpPOC", 0)
            poc_distance = current_price - poc
            
            # Perform inference if a new tick just arrived affecting price/delta
            features = np.array([[delta, wyckoff_wave, poc_distance, tick_vol]])
            
            try:
                prediction = self.model.predict(features)[0]
                probability = np.max(self.model.predict_proba(features))
                
                # Only trade if model is confident
                if probability > 0.65:
                    # Risk parameters
                    RISK_PERCENTAGE = 1.0
                    STOP_LOSS_PIPS = 20
                    REWARD_RATIO = 2.0
                    
                    if prediction == 1:
                        logging.info(f"==> AI SIGNAL (Prob: {probability:.2f}): BUY {symbol} at {current_price}")
                        
                    elif prediction == -1:
                        logging.info(f"==> AI SIGNAL (Prob: {probability:.2f}): SELL {symbol} at {current_price}")
                        
            except Exception as e:
                logging.error(f"AI Prediction Error: {e}")
                
        # Clear buffer after processing
        self.raw_data_buffer[symbol] = []
        # logging.info(f"[Buffer] Successfully merged and wrote states for {symbol}.")
