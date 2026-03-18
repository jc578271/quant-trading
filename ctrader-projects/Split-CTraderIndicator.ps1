[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string]$ManifestPath,

    [string]$SourcePath,

    [string]$TargetDir,

    [switch]$SkipMainFile,

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

function Get-LineSlice {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [int]$StartLine,

        [Parameter(Mandatory = $true)]
        [int]$EndLine
    )

    if ($StartLine -lt 1) {
        throw "StartLine must be >= 1. Received: $StartLine"
    }

    if ($EndLine -lt $StartLine) {
        throw "EndLine must be >= StartLine. Received: $StartLine..$EndLine"
    }

    if ($EndLine -gt $Lines.Count) {
        throw "Line range $StartLine..$EndLine exceeds file length $($Lines.Count)."
    }

    $result = New-Object System.Collections.Generic.List[string]
    for ($index = $StartLine - 1; $index -le $EndLine - 1; $index++) {
        $result.Add($Lines[$index])
    }

    return $result.ToArray()
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

function Get-LineRangesContent {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [AllowNull()]
        [object]$Ranges
    )

    $result = New-Object System.Collections.Generic.List[string]

    foreach ($range in (Get-NormalizedList -Value $Ranges)) {
        if ($null -eq $range) {
            continue
        }

        if ($null -eq $range.startLine -or $null -eq $range.endLine) {
            throw "Each line range must define 'startLine' and 'endLine'."
        }

        $slice = Get-LineSlice -Lines $Lines -StartLine ([int]$range.startLine) -EndLine ([int]$range.endLine)
        $result.AddRange([string[]]$slice)
    }

    return $result.ToArray()
}

function Get-LiteralLines {
    param(
        [AllowNull()]
        [object]$Value
    )

    $result = New-Object System.Collections.Generic.List[string]
    foreach ($item in (Get-NormalizedList -Value $Value)) {
        $result.Add([string]$item)
    }

    return $result.ToArray()
}

function Get-SectionPrefixLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$SourceLines,

        [Parameter(Mandatory = $true)]
        [object]$Section,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$SharedHeader
    )

    $result = New-Object System.Collections.Generic.List[string]

    if ($Section.prependSharedHeader -and $SharedHeader -and $SharedHeader.Count -gt 0) {
        $result.AddRange([string[]]$SharedHeader)
    }

    $prependLineRanges = if ($Section.PSObject.Properties.Name -contains "prependLineRanges") { $Section.prependLineRanges } else { $null }
    $prependLiteralLines = if ($Section.PSObject.Properties.Name -contains "prependLiteralLines") { $Section.prependLiteralLines } else { $null }
    $rangeLines = @(Get-LineRangesContent -Lines $SourceLines -Ranges $prependLineRanges)
    $literalLines = @(Get-LiteralLines -Value $prependLiteralLines)

    if ($rangeLines.Count -gt 0) {
        $result.AddRange([string[]]$rangeLines)
    }
    if ($literalLines.Count -gt 0) {
        $result.AddRange([string[]]$literalLines)
    }

    return $result.ToArray()
}

function Get-SectionSuffixLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$SourceLines,

        [Parameter(Mandatory = $true)]
        [object]$Section,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
    [string[]]$SharedFooter
    )

    $appendLiteralLines = if ($Section.PSObject.Properties.Name -contains "appendLiteralLines") { $Section.appendLiteralLines } else { $null }
    $appendLineRanges = if ($Section.PSObject.Properties.Name -contains "appendLineRanges") { $Section.appendLineRanges } else { $null }
    $literalLines = @(Get-LiteralLines -Value $appendLiteralLines)
    $rangeLines = @(Get-LineRangesContent -Lines $SourceLines -Ranges $appendLineRanges)
    $result = New-Object System.Collections.Generic.List[string]

    if ($literalLines.Count -gt 0) {
        $result.AddRange([string[]]$literalLines)
    }
    if ($rangeLines.Count -gt 0) {
        $result.AddRange([string[]]$rangeLines)
    }

    if ($Section.appendSharedFooter -and $SharedFooter -and $SharedFooter.Count -gt 0) {
        $result.AddRange([string[]]$SharedFooter)
    }

    return $result.ToArray()
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

function New-RangeSet {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [object[]]$Sections
    )

    $removed = New-Object bool[] ($Lines.Count)

    foreach ($section in $Sections) {
        if (-not $section.removeFromMain) {
            continue
        }

        for ($lineNumber = [int]$section.startLine; $lineNumber -le [int]$section.endLine; $lineNumber++) {
            $removed[$lineNumber - 1] = $true
        }
    }

    return $removed
}

function Test-OverlappingSections {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Sections
    )

    $sorted = $Sections | Sort-Object @{ Expression = { [int]$_.startLine } }, @{ Expression = { [int]$_.endLine } }

    for ($i = 1; $i -lt $sorted.Count; $i++) {
        $prev = $sorted[$i - 1]
        $curr = $sorted[$i]

        if ([int]$curr.startLine -le [int]$prev.endLine) {
            throw "Sections overlap: '$($prev.name)' ($($prev.startLine)-$($prev.endLine)) and '$($curr.name)' ($($curr.startLine)-$($curr.endLine))."
        }
    }
}

$manifestFullPath = [System.IO.Path]::GetFullPath($ManifestPath)
if (-not (Test-Path -LiteralPath $manifestFullPath)) {
    throw "Manifest not found: $manifestFullPath"
}

$manifestDirectory = Split-Path -Parent $manifestFullPath
$manifest = Get-Content -LiteralPath $manifestFullPath -Raw | ConvertFrom-Json

$resolvedSourcePath = if ($PSBoundParameters.ContainsKey("SourcePath")) {
    [System.IO.Path]::GetFullPath($SourcePath)
}
else {
    Resolve-PathFromManifest -BaseDirectory $manifestDirectory -PathValue $manifest.sourcePath
}

$resolvedTargetDir = if ($PSBoundParameters.ContainsKey("TargetDir")) {
    [System.IO.Path]::GetFullPath($TargetDir)
}
else {
    Resolve-PathFromManifest -BaseDirectory $manifestDirectory -PathValue $manifest.targetDir
}

if ([string]::IsNullOrWhiteSpace($resolvedSourcePath)) {
    throw "SourcePath is required either in the manifest or as a parameter."
}

if ([string]::IsNullOrWhiteSpace($resolvedTargetDir)) {
    throw "TargetDir is required either in the manifest or as a parameter."
}

if (-not (Test-Path -LiteralPath $resolvedSourcePath)) {
    throw "Source file not found: $resolvedSourcePath"
}

if (-not $manifest.sections -or $manifest.sections.Count -eq 0) {
    throw "Manifest must define at least one section."
}

$sourceLines = [System.IO.File]::ReadAllLines($resolvedSourcePath, $Utf8)
$sections = @($manifest.sections)

foreach ($section in $sections) {
    if (-not $section.name) {
        throw "Each section must define 'name'."
    }

    if (-not $section.outputFileName) {
        throw "Section '$($section.name)' must define 'outputFileName'."
    }

    if ($null -eq $section.startLine -or $null -eq $section.endLine) {
        throw "Section '$($section.name)' must define 'startLine' and 'endLine'."
    }

    [void](Get-LineSlice -Lines $sourceLines -StartLine ([int]$section.startLine) -EndLine ([int]$section.endLine))
}

Test-OverlappingSections -Sections $sections

$sharedHeader = [string[]]@()
if ($manifest.sharedHeader) {
    $sharedHeader = [string[]](Get-LineSlice -Lines $sourceLines -StartLine ([int]$manifest.sharedHeader.startLine) -EndLine ([int]$manifest.sharedHeader.endLine))
}

$sharedFooter = [string[]]@()
if ($manifest.sharedFooter) {
    $sharedFooter = [string[]](Get-LineSlice -Lines $sourceLines -StartLine ([int]$manifest.sharedFooter.startLine) -EndLine ([int]$manifest.sharedFooter.endLine))
}

$writeResults = New-Object System.Collections.Generic.List[object]

foreach ($section in $sections) {
    $outputPath = Join-Path $resolvedTargetDir $section.outputFileName
    $sectionLines = New-Object System.Collections.Generic.List[string]

    $prefixLines = @(Get-SectionPrefixLines -SourceLines $sourceLines -Section $section -SharedHeader $sharedHeader)
    if ($prefixLines.Count -gt 0) {
        $sectionLines.AddRange([string[]]$prefixLines)
    }

    $bodyLines = @(Get-LineSlice -Lines $sourceLines -StartLine ([int]$section.startLine) -EndLine ([int]$section.endLine))
    if ($bodyLines.Count -gt 0) {
        $sectionLines.AddRange([string[]]$bodyLines)
    }

    $suffixLines = @(Get-SectionSuffixLines -SourceLines $sourceLines -Section $section -SharedFooter $sharedFooter)
    if ($suffixLines.Count -gt 0) {
        $sectionLines.AddRange([string[]]$suffixLines)
    }

    $sectionOutputReplacements = if ($section.PSObject.Properties.Name -contains "outputReplacements") { $section.outputReplacements } else { $null }
    $sectionOutputLines = Apply-LineReplacements -Lines $sectionLines.ToArray() -Replacements $sectionOutputReplacements

    if ($PSCmdlet.ShouldProcess($outputPath, "Write split section '$($section.name)'")) {
        Write-Utf8File -Path $outputPath -Lines $sectionOutputLines
    }

    $writeResults.Add([pscustomobject]@{
        Type = "Section"
        Name = [string]$section.name
        Path = $outputPath
        StartLine = [int]$section.startLine
        EndLine = [int]$section.endLine
    }) | Out-Null
}

if (-not $SkipMainFile) {
    $mainOutputFileName = if ($manifest.mainOutputFileName) {
        [string]$manifest.mainOutputFileName
    }
    else {
        [System.IO.Path]::GetFileName($resolvedSourcePath)
    }

    $mainOutputPath = Join-Path $resolvedTargetDir $mainOutputFileName
    $removedLines = New-RangeSet -Lines $sourceLines -Sections $sections
    $mainLines = New-Object System.Collections.Generic.List[string]

    for ($i = 0; $i -lt $sourceLines.Count; $i++) {
        if (-not $removedLines[$i]) {
            $mainLines.Add($sourceLines[$i])
        }
    }

    $mainOutputReplacements = if ($manifest.PSObject.Properties.Name -contains "mainOutputReplacements") { $manifest.mainOutputReplacements } else { $null }
    $mainOutputLines = Apply-LineReplacements -Lines $mainLines.ToArray() -Replacements $mainOutputReplacements

    if ($PSCmdlet.ShouldProcess($mainOutputPath, "Write main split file")) {
        Write-Utf8File -Path $mainOutputPath -Lines $mainOutputLines
    }

    $writeResults.Add([pscustomobject]@{
        Type = "Main"
        Name = $mainOutputFileName
        Path = $mainOutputPath
        StartLine = $null
        EndLine = $null
    }) | Out-Null
}

if ($PassThru) {
    $writeResults
}
else {
    Write-Host "Split complete:"
    foreach ($result in $writeResults) {
        if ($result.Type -eq "Section") {
            Write-Host " - [$($result.Type)] $($result.Name): $($result.Path) ($($result.StartLine)-$($result.EndLine))"
        }
        else {
            Write-Host " - [$($result.Type)] $($result.Name): $($result.Path)"
        }
    }
}
