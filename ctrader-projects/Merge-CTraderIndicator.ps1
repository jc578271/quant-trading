[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string]$ManifestPath,

    [string]$SourceDir,

    [string]$OutputPath,

    [switch]$PassThru
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Utf8 = [System.Text.Encoding]::UTF8
$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Resolve-PathFromManifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaseDirectory,

        [AllowNull()]
        [AllowEmptyString()]
        [string]$PathValue
    )

    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return $null
    }

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BaseDirectory $PathValue))
}

function Write-Utf8File {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    }

    [System.IO.File]::WriteAllLines($Path, $Lines, $Utf8NoBom)
}

function Get-TrimmedSectionLines {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [int]$HeaderLineCount,

        [int]$FooterLineCount
    )

    $lines = [System.IO.File]::ReadAllLines($Path, $Utf8)

    $startIndex = if ($HeaderLineCount -gt 0) { $HeaderLineCount } else { 0 }
    $endIndexExclusive = if ($FooterLineCount -gt 0) { $lines.Count - $FooterLineCount } else { $lines.Count }

    if ($startIndex -gt $endIndexExclusive) {
        throw "Section trimming removed all content for $Path"
    }

    $result = New-Object System.Collections.Generic.List[string]
    for ($index = $startIndex; $index -lt $endIndexExclusive; $index++) {
        $result.Add($lines[$index])
    }

    return $result.ToArray()
}

function Get-LastIndexNotMatchingPattern {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [string[]]$Patterns
    )

    if (-not $Patterns -or $Patterns.Count -eq 0) {
        return $Lines.Count - 1
    }

    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($pattern in $Patterns) {
        [void]$set.Add($pattern)
    }

    $index = $Lines.Count - 1
    while ($index -ge 0) {
        $candidate = $Lines[$index]
        if ($set.Contains($candidate)) {
            $index--
            continue
        }
        break
    }

    return $index
}

function Get-LastNonBlankIndex {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines
    )

    $index = $Lines.Count - 1
    while ($index -ge 0) {
        if ([string]::IsNullOrWhiteSpace($Lines[$index])) {
            $index--
            continue
        }
        break
    }

    return $index
}

function Get-TrailingBlankLineCount {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines
    )

    $count = 0
    for ($index = $Lines.Count - 1; $index -ge 0; $index--) {
        if ([string]::IsNullOrWhiteSpace($Lines[$index])) {
            $count++
            continue
        }
        break
    }

    return $count
}

$manifestFullPath = [System.IO.Path]::GetFullPath($ManifestPath)
if (-not (Test-Path -LiteralPath $manifestFullPath)) {
    throw "Manifest not found: $manifestFullPath"
}

$manifestDirectory = Split-Path -Parent $manifestFullPath
$manifest = Get-Content -LiteralPath $manifestFullPath -Raw | ConvertFrom-Json

$resolvedSourceDir = if ($PSBoundParameters.ContainsKey("SourceDir")) {
    [System.IO.Path]::GetFullPath($SourceDir)
}
else {
    Resolve-PathFromManifest -BaseDirectory $manifestDirectory -PathValue $manifest.targetDir
}

$resolvedOutputPath = if ($PSBoundParameters.ContainsKey("OutputPath")) {
    [System.IO.Path]::GetFullPath($OutputPath)
}
else {
    Resolve-PathFromManifest -BaseDirectory $manifestDirectory -PathValue $manifest.sourcePath
}

if ([string]::IsNullOrWhiteSpace($resolvedSourceDir)) {
    throw "SourceDir is required either in the manifest or as a parameter."
}

if ([string]::IsNullOrWhiteSpace($resolvedOutputPath)) {
    throw "OutputPath is required either in the manifest or as a parameter."
}

if (-not (Test-Path -LiteralPath $resolvedSourceDir)) {
    throw "Source directory not found: $resolvedSourceDir"
}

if (-not $manifest.mainOutputFileName) {
    throw "Manifest must define mainOutputFileName."
}

if (-not $manifest.sections -or $manifest.sections.Count -eq 0) {
    throw "Manifest must define at least one section."
}

$mainInputPath = Join-Path $resolvedSourceDir ([string]$manifest.mainOutputFileName)
if (-not (Test-Path -LiteralPath $mainInputPath)) {
    throw "Main split file not found: $mainInputPath"
}

$sections = @($manifest.sections | Sort-Object @{ Expression = { [int]$_.startLine } }, @{ Expression = { [int]$_.endLine } })

$headerLineCount = 0
if ($manifest.sharedHeader) {
    $headerLineCount = [int]$manifest.sharedHeader.endLine - [int]$manifest.sharedHeader.startLine + 1
}

$footerLineCount = 0
if ($manifest.sharedFooter) {
    $footerLineCount = [int]$manifest.sharedFooter.endLine - [int]$manifest.sharedFooter.startLine + 1
}

$mainLines = [System.IO.File]::ReadAllLines($mainInputPath, $Utf8)
if ($footerLineCount -gt 0) {
    if ($mainLines.Count -lt $footerLineCount) {
        throw "Main split file is shorter than sharedFooter length."
    }

    $footerLines = $mainLines[($mainLines.Count - $footerLineCount)..($mainLines.Count - 1)]
    $mainLines = if ($mainLines.Count -eq $footerLineCount) {
        [string[]]@()
    }
    else {
        $mainLines[0..($mainLines.Count - $footerLineCount - 1)]
    }
}
else {
    $footerLines = [string[]]@()
}

$trimPatterns = @()
if ($manifest.merge -and $manifest.merge.trimTrailingPatterns) {
    $trimPatterns = @($manifest.merge.trimTrailingPatterns)
}

$lastMainIndex = Get-LastIndexNotMatchingPattern -Lines $mainLines -Patterns $trimPatterns
$mainLines = if ($lastMainIndex -ge 0) {
    $mainLines[0..$lastMainIndex]
}
else {
    [string[]]@()
}

$mainTrailingBlankCount = Get-TrailingBlankLineCount -Lines $mainLines
$lastMainNonBlankIndex = Get-LastNonBlankIndex -Lines $mainLines
$mainLines = if ($lastMainNonBlankIndex -ge 0) {
    $mainLines[0..$lastMainNonBlankIndex]
}
else {
    [string[]]@()
}

$mergedLines = New-Object System.Collections.Generic.List[string]
$mergedLines.AddRange([string[]]$mainLines)

for ($sectionIndex = 0; $sectionIndex -lt $sections.Count; $sectionIndex++) {
    $section = $sections[$sectionIndex]
    $sectionPath = Join-Path $resolvedSourceDir ([string]$section.outputFileName)
    if (-not (Test-Path -LiteralPath $sectionPath)) {
        throw "Section file not found: $sectionPath"
    }

    $sectionHeaderCount = if ($section.prependSharedHeader) { $headerLineCount } else { 0 }
    $sectionFooterCount = if ($section.appendSharedFooter) { $footerLineCount } else { 0 }
    $sectionLines = Get-TrimmedSectionLines -Path $sectionPath -HeaderLineCount $sectionHeaderCount -FooterLineCount $sectionFooterCount

    if ($sectionIndex -eq 0 -and $mainTrailingBlankCount -gt 0) {
        $mergedLines.Add("")
    }
    elseif ($sectionIndex -gt 0) {
        $previousSection = $sections[$sectionIndex - 1]
        $gapLineCount = [int]$section.startLine - [int]$previousSection.endLine - 1
        for ($gapIndex = 0; $gapIndex -lt $gapLineCount; $gapIndex++) {
            $mergedLines.Add("")
        }
    }

    $mergedLines.AddRange([string[]]$sectionLines)
}

if ($footerLines.Count -gt 0) {
    $mergedLines.AddRange([string[]]$footerLines)
}

if ($PSCmdlet.ShouldProcess($resolvedOutputPath, "Write merged raw indicator file")) {
    Write-Utf8File -Path $resolvedOutputPath -Lines $mergedLines.ToArray()
}

$result = [pscustomobject]@{
    MainInputPath = $mainInputPath
    OutputPath = $resolvedOutputPath
    SourceDir = $resolvedSourceDir
    SectionCount = $sections.Count
}

if ($PassThru) {
    $result
}
else {
    Write-Host "Merge complete:"
    Write-Host " - Main: $mainInputPath"
    Write-Host " - Sections: $($sections.Count)"
    Write-Host " - Output: $resolvedOutputPath"
}
