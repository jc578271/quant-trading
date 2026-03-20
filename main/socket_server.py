import asyncio
from datetime import datetime, timezone
import json
import logging

from event_contract import normalize_record

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

QUARANTINE_FILE = "quarantine_events.jsonl"


def quarantine_record(received_at, reason, raw):
    quarantine_event = {
        "received_at": received_at,
        "reason": reason,
        "raw": raw,
    }
    try:
        with open(QUARANTINE_FILE, "a", encoding="utf-8") as quarantine_file:
            quarantine_file.write(json.dumps(quarantine_event, ensure_ascii=True) + "\n")
    except OSError as error:
        logging.error(f"Failed to quarantine event: {error}")

class SocketServer:
    def __init__(self, host='127.0.0.1', port=5555, callback=None):
        self.host = host
        self.port = port
        self.callback = callback
        self.server = None

    async def handle_client(self, reader, writer):
        addr = writer.get_extra_info('peername')
        logging.info(f"Connected from cTrader at {addr}")
        
        while True:
            try:
                # Use readline() because JSONs can be very large (e.g. Order Flow Footprint)
                line = await reader.readline()
                if not line:
                    break
                    
                message = line.decode('utf-8').strip()
                if not message:
                    continue
                    
                try:
                    record = json.loads(message)
                    received_at = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")
                    normalized_record, rejection_reason = normalize_record(record, received_at)
                    if rejection_reason:
                        quarantine_record(received_at, rejection_reason, record)
                        logging.error(f"Rejected event: {rejection_reason}")
                        continue
                    if self.callback:
                        self.callback(normalized_record)
                except json.JSONDecodeError:
                    logging.error(f"Malformed JSON from cTrader: {message[:100]}...")
                        
            except ConnectionResetError:
                break
            except Exception as e:
                logging.error(f"Socket error: {e}")
                break

        logging.info(f"cTrader client {addr} disconnected.")
        writer.close()
        await writer.wait_closed()

    async def start(self):
        self.server = await asyncio.start_server(self.handle_client, self.host, self.port)
        addrs = ', '.join(str(sock.getsockname()) for sock in self.server.sockets)
        logging.info(f"Socket Server listening on {addrs} for cTrader data...")

        async with self.server:
            await self.server.serve_forever()
