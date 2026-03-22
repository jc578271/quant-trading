from __future__ import annotations

from ai_analyzer import AIAnalyzer
from runtime_paths import ORDER_FLOW_HISTORY_FILE_NAME, alert_history_file, logs_root


def test_indicator_events_do_not_create_raw_history_csv(runtime_root, fake_mt5_client, monkeypatch):
    monkeypatch.setattr(AIAnalyzer, "_load_or_train_initial_model", lambda self: None)
    analyzer = AIAnalyzer(fake_mt5_client)

    analyzer.process_data(
        {
            "event": "order_flow_aggregated",
            "instrument": "XAUUSD",
            "timestamp": "2026-03-22T00:00:00Z",
            "payload": {
                "open": 3000.0,
                "high": 3001.0,
                "low": 2999.0,
                "close": 3000.5,
                "deltaRank": {3000.0: 10.0},
                "volumesRank": {3000.0: 20.0},
                "spread": 0.2,
            },
            "source_meta": {
                "symbol": "XAUUSD",
                "timeframe": "M1",
            },
        }
    )

    assert not (logs_root() / ORDER_FLOW_HISTORY_FILE_NAME).exists()


def test_alert_events_still_create_alert_csv(runtime_root, fake_mt5_client, monkeypatch):
    monkeypatch.setattr(AIAnalyzer, "_load_or_train_initial_model", lambda self: None)
    analyzer = AIAnalyzer(fake_mt5_client)

    analyzer.process_data(
        {
            "event": "alert",
            "instrument": "XAUUSD",
            "timestamp": "2026-03-22T00:00:00Z",
            "payload": {
                "symbol": "XAUUSD",
                "text": "XAUUSD: HIDDEN ASK DEVELOPMENT, V: 15 at 3000.5",
                "count": 1,
                "popup": True,
            },
        }
    )

    assert alert_history_file("XAUUSD").exists()
