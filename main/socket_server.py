import asyncio
import json
import logging

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

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
                data = await reader.read(4096)
                if not data:
                    break
                    
                message = data.decode('utf-8').strip()
                # logging.debug(f"Received message: {message}")
                
                # Assume JSON streaming, each message split by newline
                for m in message.split('\n'):
                    if not m.strip(): continue
                    try:
                        record = json.loads(m.strip())
                        if self.callback:
                            self.callback(record)
                    except json.JSONDecodeError:
                        logging.error(f"Malformed JSON from cTrader: {m}")
                        
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
