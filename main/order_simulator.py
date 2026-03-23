import logging
import json
import time
from datetime import datetime
from pathlib import Path

from runtime_paths import TRADE_HISTORY_KEYS, trade_history_file

class OrderSimulator:
    """
    Paper-trading order simulator.
    Tracks simulated positions, P&L, and logs trade history to JSONL.
    """
    def __init__(self, config=None):
        cfg = config or {}
        # Account
        self.initial_balance = cfg.get("balance", 10000.0)
        self.balance = self.initial_balance

        # Risk:Reward
        self.min_rr = cfg.get("min_rr", 2.0)

        # Stop Loss
        self.sl_mode = cfg.get("sl_mode", "fixed")      # "fixed" (pips) or "percent" (% balance)
        self.sl_value = cfg.get("sl_value", 20.0)        # pips or %

        # Take Profit
        self.tp_mode = cfg.get("tp_mode", "rr")          # "rr" (R:R * SL) or "fixed" (pips)
        self.tp_value = cfg.get("tp_value", 2.0)         # multiplier or pips

        # Lot sizing
        self.lot_mode = cfg.get("lot_mode", "auto")      # "auto" (% risk) or "fixed"
        self.lot_value = cfg.get("lot_value", 1.0)       # % risk or fixed lot

        # Instrument info
        self.pip_size = cfg.get("pip_size", 0.01)        # XAUUSD = 0.01
        self.pip_value_per_lot = cfg.get("pip_value_per_lot", 10.0)  # $ per pip per lot

        # Max concurrent trades
        self.max_open_trades = cfg.get("max_open_trades", 1)

        # State
        self.open_trades = []       # list of active trade dicts
        self.closed_trades = []     # list of closed trade dicts
        self.trade_counter = 0
        # Default runtime/trade_history.jsonl path resolves through runtime_paths.trade_history_file().
        self.csv_path = cfg.get("trade_log", str(trade_history_file()))

        # Cooldown: prevent rapid re-entry after close
        self.last_close_time = 0
        self.cooldown_seconds = cfg.get("cooldown_seconds", 5)

        self._print_config()

    def _print_config(self):
        logging.info("=" * 60)
        logging.info("  ORDER SIMULATOR CONFIG")
        logging.info(f"  Balance:     ${self.balance:,.2f}")
        logging.info(f"  Min R:R:     {self.min_rr}")
        logging.info(f"  SL Mode:     {self.sl_mode} ({self.sl_value} {'pips' if self.sl_mode == 'fixed' else '%'})")
        logging.info(f"  TP Mode:     {self.tp_mode} ({self.tp_value} {'x R:R' if self.tp_mode == 'rr' else 'pips'})")
        logging.info(f"  Lot Mode:    {self.lot_mode} ({self.lot_value} {'% risk' if self.lot_mode == 'auto' else 'lot'})")
        logging.info(f"  Pip Size:    {self.pip_size}")
        logging.info(f"  Pip Value:   ${self.pip_value_per_lot}/pip/lot")
        logging.info("=" * 60)

    # ────────── Lot Sizing ──────────
    def _calculate_lot_size(self, sl_pips):
        if self.lot_mode == "fixed":
            return self.lot_value

        # Auto: risk X% of balance
        risk_amount = self.balance * (self.lot_value / 100.0)
        if sl_pips <= 0:
            return 0.01  # minimum
        lot = risk_amount / (sl_pips * self.pip_value_per_lot)
        return round(max(0.01, lot), 2)

    # ────────── SL / TP Calculation ──────────
    def _calc_sl_pips(self):
        if self.sl_mode == "fixed":
            return self.sl_value

        # Percent of balance converted to pips
        risk_amount = self.balance * (self.sl_value / 100.0)
        lot_size = self.lot_value if self.lot_mode == "fixed" else 0.01
        sl_pips = risk_amount / (lot_size * self.pip_value_per_lot)
        return round(sl_pips, 1)

    def _calc_tp_pips(self, sl_pips):
        if self.tp_mode == "fixed":
            return self.tp_value
        # R:R mode
        return sl_pips * self.tp_value

    # ────────── Open Trade ──────────
    def open_trade(self, signal, symbol, price, spread=0.0):
        """
        signal: 1 (BUY) or -1 (SELL)
        Returns True if trade opened, False if skipped.
        """
        # Cooldown check
        if time.time() - self.last_close_time < self.cooldown_seconds:
            return False

        # Max trades check
        if len(self.open_trades) >= self.max_open_trades:
            return False

        # Don't open same direction if already have one
        for t in self.open_trades:
            if t["symbol"] == symbol and t["direction"] == signal:
                return False

        direction = "BUY" if signal == 1 else "SELL"
        spread_pips = spread / self.pip_size if self.pip_size > 0 else 0

        # Entry price adjusted for spread
        if signal == 1:  # BUY at Ask
            entry = price + (spread / 2)
        else:            # SELL at Bid
            entry = price - (spread / 2)

        # Calculate SL/TP
        sl_pips = self._calc_sl_pips()
        tp_pips = self._calc_tp_pips(sl_pips)

        # R:R check
        actual_rr = tp_pips / sl_pips if sl_pips > 0 else 0
        if actual_rr < self.min_rr:
            return False

        # Lot size
        lot_size = self._calculate_lot_size(sl_pips)

        # Price levels
        if signal == 1:  # BUY
            sl_price = entry - (sl_pips * self.pip_size)
            tp_price = entry + (tp_pips * self.pip_size)
        else:            # SELL
            sl_price = entry + (sl_pips * self.pip_size)
            tp_price = entry - (tp_pips * self.pip_size)

        self.trade_counter += 1
        trade = {
            "id": self.trade_counter,
            "symbol": symbol,
            "direction": signal,
            "direction_str": direction,
            "entry_price": round(entry, 2),
            "sl_price": round(sl_price, 2),
            "tp_price": round(tp_price, 2),
            "sl_pips": sl_pips,
            "tp_pips": tp_pips,
            "lot_size": lot_size,
            "spread_pips": round(spread_pips, 2),
            "open_time": datetime.utcnow().isoformat(),
            "balance_at_open": self.balance,
        }
        self.open_trades.append(trade)

        logging.info(
            f"\033[1;33m"
            f"  ╔══ TRADE #{trade['id']} OPENED ══╗\n"
            f"  ║  {direction} {symbol} @ {entry:.2f}\n"
            f"  ║  Lot: {lot_size}  |  Spread: {spread_pips:.1f} pips\n"
            f"  ║  SL: {sl_price:.2f} ({sl_pips} pips)\n"
            f"  ║  TP: {tp_price:.2f} ({tp_pips} pips)  R:R = {actual_rr:.1f}\n"
            f"  ╚════════════════════════╝"
            f"\033[0m"
        )
        return True

    # ────────── Check Open Trades ──────────
    def check_open_trades(self, symbol, current_price):
        """Check if any open trades hit SL or TP. Call on every tick."""
        trades_to_close = []

        for trade in self.open_trades:
            if trade["symbol"] != symbol:
                continue

            if trade["direction"] == 1:  # BUY
                if current_price <= trade["sl_price"]:
                    trades_to_close.append((trade, "SL", current_price))
                elif current_price >= trade["tp_price"]:
                    trades_to_close.append((trade, "TP", current_price))
            else:  # SELL
                if current_price >= trade["sl_price"]:
                    trades_to_close.append((trade, "SL", current_price))
                elif current_price <= trade["tp_price"]:
                    trades_to_close.append((trade, "TP", current_price))

        for trade, reason, exit_price in trades_to_close:
            self._close_trade(trade, reason, exit_price)

    # ────────── Close Trade ──────────
    def _close_trade(self, trade, reason, exit_price):
        self.open_trades.remove(trade)
        self.last_close_time = time.time()

        # P&L calculation
        if trade["direction"] == 1:  # BUY
            pnl_pips = (exit_price - trade["entry_price"]) / self.pip_size
        else:  # SELL
            pnl_pips = (trade["entry_price"] - exit_price) / self.pip_size

        pnl_dollar = pnl_pips * trade["lot_size"] * self.pip_value_per_lot
        balance_before = self.balance
        self.balance += pnl_dollar
        balance_change_pct = (pnl_dollar / balance_before) * 100 if balance_before > 0 else 0

        # Color: green for profit, red for loss
        if pnl_dollar >= 0:
            color = "\033[1;32m"  # green
            icon = "✅"
        else:
            color = "\033[1;31m"  # red
            icon = "❌"

        logging.info(
            f"{color}"
            f"\n  ╔══ TRADE #{trade['id']} CLOSED ({reason}) {icon} ══╗\n"
            f"  ║  {trade['direction_str']} {trade['symbol']}\n"
            f"  ║  Entry: {trade['entry_price']:.2f} → Exit: {exit_price:.2f}\n"
            f"  ║  P&L: {pnl_pips:+.1f} pips = ${pnl_dollar:+,.2f}\n"
            f"  ║  Balance: ${balance_before:,.2f} → ${self.balance:,.2f} ({balance_change_pct:+.2f}%)\n"
            f"  ╚══════════════════════════════════╝"
            f"\033[0m"
        )

        # Log to JSONL
        closed_record = {
            "timestamp": datetime.utcnow().isoformat(),
            "symbol": trade["symbol"],
            "direction": trade["direction_str"],
            "entry_price": trade["entry_price"],
            "sl_price": trade["sl_price"],
            "tp_price": trade["tp_price"],
            "lot_size": trade["lot_size"],
            "exit_price": round(exit_price, 2),
            "exit_reason": reason,
            "pnl_pips": round(pnl_pips, 1),
            "pnl_dollar": round(pnl_dollar, 2),
            "pnl_percent": round(balance_change_pct, 2),
            "balance_before": round(balance_before, 2),
            "balance_after": round(self.balance, 2),
        }
        self.closed_trades.append(closed_record)
        self._log_trade_jsonl(closed_record)

    # ────────── CSV Logging ──────────
    def _log_trade_jsonl(self, record):
        ordered_record = {key: record.get(key) for key in TRADE_HISTORY_KEYS}
        Path(self.csv_path).parent.mkdir(parents=True, exist_ok=True)
        with open(self.csv_path, "a", encoding="utf-8") as f:
            f.write(json.dumps(ordered_record, ensure_ascii=False, separators=(",", ":")))
            f.write("\n")

    def _log_trade_csv(self, record):
        self._log_trade_jsonl(record)

    # ────────── Summary ──────────
    def get_summary(self):
        total_trades = len(self.closed_trades)
        wins = sum(1 for t in self.closed_trades if t["pnl_dollar"] > 0)
        losses = sum(1 for t in self.closed_trades if t["pnl_dollar"] <= 0)
        total_pnl = sum(t["pnl_dollar"] for t in self.closed_trades)
        win_rate = (wins / total_trades * 100) if total_trades > 0 else 0
        net_change = ((self.balance - self.initial_balance) / self.initial_balance) * 100

        return {
            "balance": self.balance,
            "initial_balance": self.initial_balance,
            "net_pnl": total_pnl,
            "net_change_pct": net_change,
            "total_trades": total_trades,
            "wins": wins,
            "losses": losses,
            "win_rate": win_rate,
            "open_trades": len(self.open_trades),
        }

    def print_summary(self):
        s = self.get_summary()
        color = "\033[1;32m" if s["net_pnl"] >= 0 else "\033[1;31m"
        logging.info(
            f"{color}"
            f"\n  ╔══════ SESSION SUMMARY ══════╗\n"
            f"  ║  Balance:    ${s['balance']:,.2f}\n"
            f"  ║  Net P&L:    ${s['net_pnl']:+,.2f} ({s['net_change_pct']:+.2f}%)\n"
            f"  ║  Trades:     {s['total_trades']} (W:{s['wins']} / L:{s['losses']})\n"
            f"  ║  Win Rate:   {s['win_rate']:.1f}%\n"
            f"  ║  Open:       {s['open_trades']}\n"
            f"  ╚═════════════════════════════╝"
            f"\033[0m"
        )
