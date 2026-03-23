# Quant Trading Repository

This repository contains various custom tools, indicators, and addons for different quantitative trading platforms. 

## Project Structure

The workspace is organized into several platform-specific directories:

- **[`bookmap-addons/`](bookmap-addons/)**: Contains custom Addons and API integrations for Bookmap (e.g., BrAPI event listeners, footprint extensions).
- **[`../ctrader-indicators/`](https://github.com/jc578271/ctrader-indicators?tab=readme-ov-file)**: Contains indicators created for cTrader. This includes Order Flow Ticks, Volume Profile, TPO Profile, and other advanced trading tools.
- **[`tradingview-indicators/`](tradingview-indicators/)**: Contains custom PineScript indicators for TradingView.
- **[`main/`](main/)**: General application code, utility scripts, and core trading logic.

## Setup & External References

Some components of this repository rely on external sub-projects. For instance, to set up the latest cTrader indicators:

1. `git clone git@github.com:jc578271/ctrader-indicators.git ../ctrader-indicators`

*(More setup instructions and documentation can be found in the respective subdirectories.)*
