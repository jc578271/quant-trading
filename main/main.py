import asyncio
import logging
from mt5_client import MT5Client
from socket_server import SocketServer
from ai_analyzer import AIAnalyzer

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

async def main():
    logging.info("Starting Quantum Trade AI System...")
    
    # Initialize MT5 Client
    mt5_client = MT5Client()
    if not mt5_client.connect():
        logging.error("Failed to connect to MT5. Ensure MT5 is running and AutoTrading is enabled.")
        # Proceeding without MT5 strictly for testing socket only (optional)
        # return
        
    # Initialize AI Analyzer
    analyzer = AIAnalyzer(mt5_client)
    
    # Initialize and start Socket Server
    server = SocketServer(host='127.0.0.1', port=5555, callback=analyzer.process_data)
    
    try:
        await server.start()
    except KeyboardInterrupt:
        logging.info("Shutting down Application...")
    finally:
        analyzer.shutdown()
        mt5_client.disconnect()

if __name__ == '__main__':
    # Run the asyncio event loop
    asyncio.run(main())
