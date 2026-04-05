from __future__ import annotations

from types import SimpleNamespace

import mt5_client
from mt5_client import MT5Client


class FakeMetaTrader5:
    ORDER_TYPE_BUY = 0
    ORDER_TYPE_SELL = 1
    TRADE_ACTION_DEAL = 1
    ORDER_TIME_GTC = 0
    ORDER_FILLING_IOC = 1
    TRADE_RETCODE_DONE = 10009

    def __init__(self) -> None:
        self.initialize_calls: list[tuple[tuple, dict]] = []
        self.visible_symbols = {"XAUUSD.demo": False}

    def initialize(self, *args, **kwargs):
        self.initialize_calls.append((args, kwargs))
        return True

    def last_error(self):
        return (0, "ok")

    def account_info(self):
        return SimpleNamespace(balance=10000.0)

    def symbol_info(self, symbol):
        if symbol not in self.visible_symbols:
            return None
        return SimpleNamespace(
            volume_step=0.01,
            volume_min=0.01,
            volume_max=100.0,
            point=0.01,
            visible=self.visible_symbols[symbol],
        )

    def symbol_select(self, symbol, enabled):
        if symbol not in self.visible_symbols:
            return False
        self.visible_symbols[symbol] = bool(enabled)
        return True

    def positions_get(self, symbol=None):
        return []


def test_mt5_client_connect_uses_demo_env_settings(monkeypatch):
    fake_mt5 = FakeMetaTrader5()
    monkeypatch.setattr(mt5_client, "mt5", fake_mt5)
    monkeypatch.setenv("QT_MT5_LOGIN", "12345678")
    monkeypatch.setenv("QT_MT5_PASSWORD", "demo-pass")
    monkeypatch.setenv("QT_MT5_SERVER", "Demo-Server")
    monkeypatch.setenv("QT_MT5_TERMINAL_PATH", r"C:\MT5\terminal64.exe")
    monkeypatch.setenv("QT_MT5_TIMEOUT_MS", "45000")
    monkeypatch.setenv("QT_MT5_PORTABLE", "1")

    client = MT5Client()

    assert client.connect() is True
    args, kwargs = fake_mt5.initialize_calls[-1]
    assert args == (r"C:\MT5\terminal64.exe",)
    assert kwargs["login"] == 12345678
    assert kwargs["password"] == "demo-pass"
    assert kwargs["server"] == "Demo-Server"
    assert kwargs["timeout"] == 45000
    assert kwargs["portable"] is True


def test_mt5_client_resolves_symbol_override_and_selects_hidden_symbol(monkeypatch):
    fake_mt5 = FakeMetaTrader5()
    monkeypatch.setattr(mt5_client, "mt5", fake_mt5)
    monkeypatch.setenv("QT_MT5_SYMBOL_XAUUSD", "XAUUSD.demo")

    client = MT5Client()
    client.connected = True

    assert client.resolve_symbol("XAUUSD") == "XAUUSD.demo"
    assert fake_mt5.visible_symbols["XAUUSD.demo"] is True
