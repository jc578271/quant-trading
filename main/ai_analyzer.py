import logging
import os
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
import joblib

class AIAnalyzer:
    def __init__(self, mt5_client, model_path="model.pkl"):
        self.mt5_client = mt5_client
        self.symbol_state = {}
        self.model_path = model_path
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

    def process_data(self, data):
        """
        Process incoming data from cTrader directly into the ML Models.
        """
        if not data: return

        symbol = data.get("Symbol", "EURUSD")
        current_price = data.get("Price", 0)
        
        # ODF Aggregated Features
        delta = data.get("Delta", 0)
        max_delta = data.get("Max_Delta", 0)
        min_delta = data.get("Min_Delta", 0)
        cum_delta = data.get("Cumulative_Delta", 0)
        
        # Weis & Wyckoff Features
        wyckoff_wave = data.get("Wyckoff_Wave", 0)
        wyckoff_vol = data.get("Wyckoff_Volume", 0)
        
        # Free Volume Profile Features
        poc = data.get("POC_Price", 0)
        vah = data.get("VAH_Price", 0)
        val = data.get("VAL_Price", 0)
        
        tick_vol = data.get("Tick_Volumes", 0)
        
        poc_distance = current_price - poc
        
        logging.info(f"[AI] {symbol} | Price: {current_price} | Delta: {delta} | WV: {wyckoff_wave} | POC: {poc}")
        
        # Construct feature vector for prediction:
        # Expected by model: [Delta, Wyckoff_Wave, VP_POC_Distance, Tick_Volumes]
        features = np.array([[delta, wyckoff_wave, poc_distance, tick_vol]])
        
        try:
            prediction = self.model.predict(features)[0]
            probability = np.max(self.model.predict_proba(features))
        except Exception as e:
            logging.error(f"AI Prediction Error: {e}")
            return
            
        # Only trade if model is confident
        if probability > 0.65:
            # Risk parameters
            RISK_PERCENTAGE = 1.0  # Risk 1% of account balance
            STOP_LOSS_PIPS = 20    # VD: 20 PIP Stop Loss
            REWARD_RATIO = 2.0     # Take Profit là 2 R:R (Tức là 40 Pips)
            
            if prediction == 1:
                logging.info(f"==> AI SIGNAL (Prob: {probability:.2f}): BUY {symbol}")
                # self.mt5_client.place_order_with_risk(symbol, "BUY", RISK_PERCENTAGE, STOP_LOSS_PIPS, REWARD_RATIO)
                
            elif prediction == -1:
                logging.info(f"==> AI SIGNAL (Prob: {probability:.2f}): SELL {symbol}")
                # self.mt5_client.place_order_with_risk(symbol, "SELL", RISK_PERCENTAGE, STOP_LOSS_PIPS, REWARD_RATIO)
