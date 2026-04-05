import logging
import os

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

try:
    import MetaTrader5 as mt5
except ModuleNotFoundError:
    mt5 = None

class MT5Client:
    def __init__(self):
        self.connected = False
        self.last_error = None
        self.login = _env_int("QT_MT5_LOGIN")
        self.password = os.environ.get("QT_MT5_PASSWORD")
        self.server = os.environ.get("QT_MT5_SERVER")
        self.terminal_path = os.environ.get("QT_MT5_TERMINAL_PATH")
        self.timeout_ms = _env_int("QT_MT5_TIMEOUT_MS", default=60000) or 60000
        self.portable = os.environ.get("QT_MT5_PORTABLE") == "1"
        self.symbol_overrides = _load_symbol_overrides()

    def connect(self):
        if mt5 is None:
            self.last_error = "MetaTrader5 module not available"
            logging.error(self.last_error)
            return False
        initialize_args = []
        initialize_kwargs = {
            "timeout": self.timeout_ms,
            "portable": self.portable,
        }
        if self.terminal_path:
            initialize_args.append(self.terminal_path)
        if self.login is not None:
            initialize_kwargs["login"] = self.login
        if self.password:
            initialize_kwargs["password"] = self.password
        if self.server:
            initialize_kwargs["server"] = self.server

        if not mt5.initialize(*initialize_args, **initialize_kwargs):
            self.last_error = str(mt5.last_error())
            logging.error(f"initialize() failed, error code = {self.last_error}")
            return False
        self.connected = True
        self.last_error = None
        logging.info("Connected to MetaTrader 5")
        return True

    def calculate_lot_size(self, symbol, risk_percentage, sl_pips):
        """Tính toán Khối lượng (Lot Size) dựa trên % rủi ro của Balance"""
        account_info = mt5.account_info()
        if not account_info:
            logging.error("Failed to get MT5 account info")
            return None
            
        balance = account_info.balance
        risk_money = balance * (risk_percentage / 100.0)
        
        symbol_info = mt5.symbol_info(symbol)
        if not symbol_info:
            logging.error("Symbol not found or MT5 not connected")
            return None
            
        # Example calculation for ordinary forex pairs
        # Pip value = (1 pip / exchange rate) * lot_size
        pip_value = 10.0 # Assuming 1 standard lot = $10 / pip typically for EURUSD and standard pairs.
        
        # We simplify the calculation: Lot = Risk$ / (SL_Pips * PipValue_Per_Lot)
        # In a generic robust model you'd use mt5.symbol_info_tick and calculate exact tick value
        lot_size = risk_money / (sl_pips * pip_value) if sl_pips > 0 else 0
        
        # Round the lot size properly according to broker specs
        lot_step = symbol_info.volume_step
        min_lot = symbol_info.volume_min
        max_lot = symbol_info.volume_max
        lot_size = round(lot_size / lot_step) * lot_step
        
        if lot_size < min_lot: lot_size = min_lot
        if lot_size > max_lot: lot_size = max_lot
            
        return lot_size

    def place_order_with_risk(self, symbol, order_type, risk_percentage, sl_pips, rr_ratio):
        """Vào lệnh với Stop Loss tính theo % số dư, Take Profit theo tỷ lệ R:R"""
        if not self.connected:
            logging.error("Not connected to MT5")
            return None

        broker_symbol = self.resolve_symbol(symbol)
        if not broker_symbol:
            return None
            
        action = mt5.ORDER_TYPE_BUY if order_type.upper() == 'BUY' else mt5.ORDER_TYPE_SELL
        
        tick = mt5.symbol_info_tick(broker_symbol)
        if not tick:
            self.last_error = f"Cannot get tick for {broker_symbol}"
            logging.error(self.last_error)
            return None
            
        price = tick.ask if action == mt5.ORDER_TYPE_BUY else tick.bid
        
        # Calculate volume based on Risk %
        volume = self.calculate_lot_size(broker_symbol, risk_percentage, sl_pips)
        if not volume: return None

        # Calculate exact SL/TP prices
        # pip_size for Forex is usually 0.0001
        sym_info = mt5.symbol_info(broker_symbol)
        pip_size = sym_info.point * 10 
        if "JPY" in symbol: pip_size = sym_info.point * 10
        
        sl_distance = sl_pips * pip_size
        tp_distance = sl_distance * rr_ratio
        
        if action == mt5.ORDER_TYPE_BUY:
            sl_price = price - sl_distance
            tp_price = price + tp_distance
        else:
            sl_price = price + sl_distance
            tp_price = price - tp_distance
            
        request = {
            "action": mt5.TRADE_ACTION_DEAL,
            "symbol": broker_symbol,
            "volume": float(volume),
            "type": action,
            "price": price,
            "sl": float(sl_price),
            "tp": float(tp_price),
            "deviation": 20,
            "magic": 234000,
            "comment": "AI Risk Managed",
            "type_time": mt5.ORDER_TIME_GTC,
            "type_filling": mt5.ORDER_FILLING_IOC,
        }
            
        result = mt5.order_send(request)
        if result.retcode != mt5.TRADE_RETCODE_DONE:
            self.last_error = str(result.retcode)
            logging.error(f"Order failed, retcode={result.retcode}")
            return None
            
        self.last_error = None
        logging.info(
            f"Order Success: {order_type} {volume} Lots {broker_symbol} | Price: {result.price} | SL: {sl_price} | TP: {tp_price}"
        )
        return result

    def resolve_symbol(self, symbol):
        broker_symbol = self.symbol_overrides.get(symbol.upper(), symbol)
        symbol_info = mt5.symbol_info(broker_symbol)
        if symbol_info is None:
            self.last_error = f"Symbol not found in MT5: {broker_symbol}"
            logging.error(self.last_error)
            return None
        if not getattr(symbol_info, "visible", True):
            if not mt5.symbol_select(broker_symbol, True):
                self.last_error = f"Failed to select symbol in MT5: {broker_symbol}"
                logging.error(self.last_error)
                return None
        return broker_symbol

    def has_open_position(self, symbol):
        if not self.connected:
            return False
        broker_symbol = self.resolve_symbol(symbol)
        if not broker_symbol:
            return False
        positions = mt5.positions_get(symbol=broker_symbol)
        return bool(positions)

    def disconnect(self):
        if self.connected:
            mt5.shutdown()
            self.connected = False
            logging.info("Disconnected from MetaTrader 5")


def _env_int(name, default=None):
    value = os.environ.get(name)
    if value in (None, ""):
        return default
    try:
        return int(value)
    except ValueError:
        return default


def _load_symbol_overrides():
    overrides = {}
    for key, value in os.environ.items():
        if not key.startswith("QT_MT5_SYMBOL_") or not value:
            continue
        symbol = key.removeprefix("QT_MT5_SYMBOL_").upper()
        overrides[symbol] = value
    return overrides
