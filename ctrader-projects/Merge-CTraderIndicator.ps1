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

function Get-NormalizedList {
    param(
        [AllowNull()]
        [object]$Value
    )

    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [System.Array]) {
        return @($Value)
    }

    return @($Value)
}

function Get-LineRangeCount {
    param(
        [AllowNull()]
        [object]$Ranges
    )

    $count = 0
    foreach ($range in (Get-NormalizedList -Value $Ranges)) {
        if ($null -eq $range) {
            continue
        }

        if ($null -eq $range.startLine -or $null -eq $range.endLine) {
            throw "Each line range must define 'startLine' and 'endLine'."
        }

        $count += ([int]$range.endLine - [int]$range.startLine + 1)
    }

    return $count
}

function Get-LiteralLineCount {
    param(
        [AllowNull()]
        [object]$Value
    )

    return @(Get-NormalizedList -Value $Value).Count
}

function Apply-LineReplacements {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [AllowNull()]
        [object]$Replacements
    )

    $replacementList = @(Get-NormalizedList -Value $Replacements)
    if ($replacementList.Count -eq 0) {
        return $Lines
    }

    $result = New-Object System.Collections.Generic.List[string]
    foreach ($line in $Lines) {
        $newLine = $line
        foreach ($replacement in $replacementList) {
            if ($null -eq $replacement) {
                continue
            }

            $oldText = [string]$replacement.oldText
            $newText = [string]$replacement.newText

            if ($newLine -eq $oldText) {
                $newLine = $newText
            }
        }
        $result.Add($newLine)
    }

    return $result.ToArray()
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

    $patternList = @(Get-NormalizedList -Value $Patterns)

    if ($patternList.Count -eq 0) {
        return $Lines.Count - 1
    }

    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    foreach ($pattern in $patternList) {
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

$mainLines = @([System.IO.File]::ReadAllLines($mainInputPath, $Utf8))
if ($footerLineCount -gt 0) {
    if ($mainLines.Count -lt $footerLineCount) {
        throw "Main split file is shorter than sharedFooter length."
    }

    $footerLines = @($mainLines[($mainLines.Count - $footerLineCount)..($mainLines.Count - 1)])
    $mainLines = if ($mainLines.Count -eq $footerLineCount) {
        [string[]]@()
    }
    else {
        @($mainLines[0..($mainLines.Count - $footerLineCount - 1)])
    }
}
else {
    $footerLines = [string[]]@()
}

$trimPatterns = @()
if ($manifest.PSObject.Properties.Name -contains "merge" -and $manifest.merge) {
    if ($manifest.merge.PSObject.Properties.Name -contains "trimTrailingPatterns") {
        $trimPatterns = @($manifest.merge.trimTrailingPatterns)
    }
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

$mainMergeReplacements = @()
if ($manifest.PSObject.Properties.Name -contains "merge" -and $manifest.merge) {
    if ($manifest.merge.PSObject.Properties.Name -contains "mainInputReplacements") {
        $mainMergeReplacements = @($manifest.merge.mainInputReplacements)
    }
}
$mainLines = Apply-LineReplacements -Lines $mainLines -Replacements $mainMergeReplacements

$mainTrailingLinesCount = 0
$insertMainTrailingLinesBeforeSection = $null
if ($manifest.PSObject.Properties.Name -contains "merge" -and $manifest.merge) {
    if ($manifest.merge.PSObject.Properties.Name -contains "mainTrailingLinesCount") {
        $mainTrailingLinesCount = [int]$manifest.merge.mainTrailingLinesCount
    }
    if ($manifest.merge.PSObject.Properties.Name -contains "insertMainTrailingLinesBeforeSection") {
        $insertMainTrailingLinesBeforeSection = [string]$manifest.merge.insertMainTrailingLinesBeforeSection
    }
}

$mainTrailingLines = [string[]]@()
if ($mainTrailingLinesCount -gt 0) {
    if ($mainLines.Count -lt $mainTrailingLinesCount) {
        throw "Main split file is shorter than mainTrailingLinesCount."
    }

    $mainTrailingLines = @($mainLines[($mainLines.Count - $mainTrailingLinesCount)..($mainLines.Count - 1)])
    $mainLines = if ($mainLines.Count -eq $mainTrailingLinesCount) {
        [string[]]@()
    }
    else {
        @($mainLines[0..($mainLines.Count - $mainTrailingLinesCount - 1)])
    }
}

if ($mainTrailingLines.Count -gt 0) {
    $mainTrailingBlankCount = 0
}

$mergedLines = New-Object System.Collections.Generic.List[string]
$mergedLines.AddRange([string[]]$mainLines)

for ($sectionIndex = 0; $sectionIndex -lt $sections.Count; $sectionIndex++) {
    $section = $sections[$sectionIndex]
    $sectionPath = Join-Path $resolvedSourceDir ([string]$section.outputFileName)
    if (-not (Test-Path -LiteralPath $sectionPath)) {
        throw "Section file not found: $sectionPath"
    }

    $sectionHeaderCount = 0
    if ($section.prependSharedHeader) {
        $sectionHeaderCount += $headerLineCount
    }
    $prependLineRanges = if ($section.PSObject.Properties.Name -contains "prependLineRanges") { $section.prependLineRanges } else { $null }
    $prependLiteralLines = if ($section.PSObject.Properties.Name -contains "prependLiteralLines") { $section.prependLiteralLines } else { $null }
    $appendLiteralLines = if ($section.PSObject.Properties.Name -contains "appendLiteralLines") { $section.appendLiteralLines } else { $null }
    $appendLineRanges = if ($section.PSObject.Properties.Name -contains "appendLineRanges") { $section.appendLineRanges } else { $null }

    $sectionHeaderCount += Get-LineRangeCount -Ranges $prependLineRanges
    $sectionHeaderCount += Get-LiteralLineCount -Value $prependLiteralLines

    $sectionFooterCount = 0
    $sectionFooterCount += Get-LiteralLineCount -Value $appendLiteralLines
    $sectionFooterCount += Get-LineRangeCount -Ranges $appendLineRanges
    if ($section.appendSharedFooter) {
        $sectionFooterCount += $footerLineCount
    }

    $sectionLines = Get-TrimmedSectionLines -Path $sectionPath -HeaderLineCount $sectionHeaderCount -FooterLineCount $sectionFooterCount

    $insertTrailingHere = $mainTrailingLines.Count -gt 0 -and $insertMainTrailingLinesBeforeSection -and $section.name -eq $insertMainTrailingLinesBeforeSection

    if ($insertTrailingHere) {
        $mergedLines.AddRange([string[]]$mainTrailingLines)
    }

    if ($sectionIndex -eq 0 -and $mainTrailingBlankCount -gt 0) {
        $mergedLines.Add("")
    }
    elseif ($sectionIndex -gt 0) {
        $previousSection = $sections[$sectionIndex - 1]
        $gapLineCount = [int]$section.startLine - [int]$previousSection.endLine - 1
        if ($insertTrailingHere) {
            $gapLineCount -= $mainTrailingLines.Count
        }
        if ($gapLineCount -lt 0) {
            $gapLineCount = 0
        }
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
