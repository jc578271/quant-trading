[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Project,

    [switch]$NoRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $workspaceRoot "quant-trading.sln"

$env:DOTNET_CLI_HOME = Join-Path $workspaceRoot ".dotnet-cli"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:NUGET_PACKAGES = Join-Path $workspaceRoot ".nuget\packages"

$targetPath = if ([string]::IsNullOrWhiteSpace($Project)) {
    $solutionPath
}
elseif ([System.IO.Path]::IsPathRooted($Project)) {
    [System.IO.Path]::GetFullPath($Project)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $workspaceRoot $Project))
}

if (-not (Test-Path -LiteralPath $targetPath)) {
    throw "Build target not found: $targetPath"
}

$arguments = @(
    "build"
    $targetPath
    "-c"
    $Configuration
    "--nologo"
)

if ($NoRestore) {
    $arguments += "--no-restore"
}

& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}
