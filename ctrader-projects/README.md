# cTrader Projects

This folder is the shared build environment for multi-file cTrader indicators.

## Shared Environment

- [Directory.Build.props](D:\projects\quant-trading\ctrader-projects\Directory.Build.props)
  Common .NET/cTrader project settings for every indicator project under `ctrader-projects`
- [Directory.Packages.props](D:\projects\quant-trading\ctrader-projects\Directory.Packages.props)
  Central package version management for `cTrader.Automate`
- [NuGet.Config](D:\projects\quant-trading\ctrader-projects\NuGet.Config)
  Shared NuGet source configuration
- [Build-CTraderProjects.ps1](D:\projects\quant-trading\ctrader-projects\Build-CTraderProjects.ps1)
  Shared build script that sets the repo-local `.dotnet-cli` and `.nuget` caches
- [Split-CTraderIndicator.ps1](D:\projects\quant-trading\ctrader-projects\Split-CTraderIndicator.ps1)
  Reusable UTF-8-safe splitter for turning a single raw indicator file into a multi-file project layout
- [Merge-CTraderIndicator.ps1](D:\projects\quant-trading\ctrader-projects\Merge-CTraderIndicator.ps1)
  Reusable UTF-8-safe merger for writing split project files back into one raw indicator file

## Conventions

- Each indicator gets its own folder under `ctrader-projects`
- The buildable `.csproj` stays in that indicator's `src` folder
- The raw single-file source in `..\\ctrader-indicators\\Raw Source Code` remains the cTrader import/export target
- The multi-file project under `ctrader-projects` is the maintainable source for IDE work, GitNexus exploration, and builds
- After build, each generated `.algo` is copied automatically to `Documents\cAlgo\Sources\Indicators`

## Build All cTrader Projects

```powershell
powershell -ExecutionPolicy Bypass -File D:\projects\quant-trading\ctrader-projects\Build-CTraderProjects.ps1
```

## Build One Indicator

```powershell
powershell -ExecutionPolicy Bypass -File D:\projects\quant-trading\ctrader-projects\Build-CTraderProjects.ps1 `
  -Project D:\projects\quant-trading\ctrader-projects\OrderFlowAggregatedV20\src\OrderFlowAggregatedV20.csproj
```

## Add Another Indicator

1. Create `ctrader-projects\<IndicatorName>\src`
2. Split the raw source with [Split-CTraderIndicator.ps1](D:\projects\quant-trading\ctrader-projects\Split-CTraderIndicator.ps1)
3. Add a minimal `.csproj` in `src`
4. Add the project to [quant-trading.sln](D:\projects\quant-trading\quant-trading.sln)
5. Build with [Build-CTraderProjects.ps1](D:\projects\quant-trading\ctrader-projects\Build-CTraderProjects.ps1)

## Merge Back To Raw Source

Write the split project files back into `..\\ctrader-indicators\\Raw Source Code`:

```powershell
powershell -ExecutionPolicy Bypass -File D:\projects\quant-trading\ctrader-projects\Merge-CTraderIndicator.ps1 `
  -ManifestPath D:\projects\quant-trading\ctrader-projects\OrderFlowAggregatedV20\split-manifest.json
```

Preview first:

```powershell
powershell -ExecutionPolicy Bypass -File D:\projects\quant-trading\ctrader-projects\Merge-CTraderIndicator.ps1 `
  -ManifestPath D:\projects\quant-trading\ctrader-projects\OrderFlowAggregatedV20\split-manifest.json `
  -WhatIf
```
