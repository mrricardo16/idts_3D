[CmdletBinding()]
param(
    [string]$BlenderPath,
    [switch]$Analyze
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-ChildPath {
    param(
        [Parameter(Mandatory)] [string]$Parent,
        [Parameter(Mandatory)] [string]$Child
    )

    $resolvedParent = [System.IO.Path]::GetFullPath($Parent).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    $resolvedChild = [System.IO.Path]::GetFullPath($Child)
    if (-not $resolvedChild.StartsWith("$resolvedParent$([System.IO.Path]::DirectorySeparatorChar)", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to operate outside controlled directory '$resolvedParent': '$resolvedChild'."
    }
}

function Get-TileUris {
    param([Parameter(Mandatory)] $Tile)

    $uris = @($Tile.content.uri)
    $childrenProperty = $Tile.PSObject.Properties["children"]
    if ($null -ne $childrenProperty) {
        foreach ($child in @($childrenProperty.Value)) {
            $uris += Get-TileUris -Tile $child
        }
    }
    return $uris
}

function Assert-TilesetUris {
    param(
        [Parameter(Mandatory)] [string]$TilesetPath,
        [Parameter(Mandatory)] [string]$ContentDirectory
    )

    $tileset = Get-Content -Raw -Encoding utf8 -LiteralPath $TilesetPath | ConvertFrom-Json
    foreach ($uri in Get-TileUris -Tile $tileset.root) {
        if ([string]::IsNullOrWhiteSpace($uri) -or $uri -notmatch '^[^\\/:]+\.glb$') {
            throw "Tileset content URI is not a simple relative GLB URI: '$uri'."
        }

        $contentPath = Join-Path $ContentDirectory $uri
        if (-not (Test-Path -LiteralPath $contentPath -PathType Leaf)) {
            throw "Tileset content URI does not resolve to a file: '$uri'."
        }
    }
}

$toolRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$outputRoot = Join-Path $toolRoot "output"
$stagingRoot = Join-Path $outputRoot "staging"
$publishRoot = Join-Path $outputRoot "tileset"
$reportsRoot = Join-Path $outputRoot "reports"
Assert-ChildPath -Parent $toolRoot -Child $outputRoot

if ([string]::IsNullOrWhiteSpace($BlenderPath)) {
    $BlenderPath = $env:BLENDER_PATH
}

if ([string]::IsNullOrWhiteSpace($BlenderPath)) {
    throw "Pass -BlenderPath or set BLENDER_PATH to blender.exe."
}

$resolvedBlenderPath = [System.IO.Path]::GetFullPath($BlenderPath)
if (-not (Test-Path -LiteralPath $resolvedBlenderPath -PathType Leaf)) {
    throw "Blender executable was not found: $resolvedBlenderPath"
}

$gltfTransform = Get-Command gltf-transform -ErrorAction Stop
$start = [DateTime]::UtcNow

if (Test-Path -LiteralPath $outputRoot) {
    Remove-Item -LiteralPath $outputRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingRoot, $publishRoot, $reportsRoot -Force | Out-Null

$generatorScript = Join-Path $toolRoot "scripts\generate_micro_warehouse.py"
& $resolvedBlenderPath --background --factory-startup --python $generatorScript -- --output $stagingRoot
if ($LASTEXITCODE -ne 0) {
    throw "Blender export failed with exit code $LASTEXITCODE."
}

$metadataPath = Join-Path $stagingRoot "tile-metadata.json"
$glbFiles = @(Get-ChildItem -LiteralPath $stagingRoot -Filter *.glb -File | Sort-Object Name)
if ($glbFiles.Count -ne 5 -or -not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
    throw "Blender export must produce five GLBs and tile-metadata.json."
}

foreach ($glbFile in $glbFiles) {
    & $gltfTransform.Source validate $glbFile.FullName --format md | Tee-Object -FilePath (Join-Path $reportsRoot "$($glbFile.BaseName).validate.md")
    if ($LASTEXITCODE -ne 0) {
        throw "glTF validation failed for $($glbFile.Name)."
    }

    & $gltfTransform.Source inspect $glbFile.FullName --format csv | Tee-Object -FilePath (Join-Path $reportsRoot "$($glbFile.BaseName).inspect.csv")
    if ($LASTEXITCODE -ne 0) {
        throw "glTF inspection failed for $($glbFile.Name)."
    }
}

$tilesetPath = Join-Path $stagingRoot "tileset.json"
node (Join-Path $toolRoot "scripts\generate-tileset.mjs") $metadataPath $tilesetPath
if ($LASTEXITCODE -ne 0) {
    throw "tileset.json generation failed with exit code $LASTEXITCODE."
}
Assert-TilesetUris -TilesetPath $tilesetPath -ContentDirectory $stagingRoot

Copy-Item -Path (Join-Path $stagingRoot "*") -Destination $publishRoot -Recurse -Force
Remove-Item -LiteralPath $stagingRoot -Recurse -Force

$analysisStatus = "not-requested"
if ($Analyze) {
    $tilesTools = Get-Command 3d-tiles-tools -ErrorAction SilentlyContinue
    if ($null -eq $tilesTools) {
        $analysisStatus = "skipped-command-not-found"
    }
    else {
        $analysisStatus = "completed"
        & $tilesTools.Source analyze -i $publishRoot -o (Join-Path $reportsRoot "3d-tiles-tools-analyze") | Tee-Object -FilePath (Join-Path $reportsRoot "3d-tiles-tools-analyze.log")
        if ($LASTEXITCODE -ne 0) {
            throw "3d-tiles-tools analyze failed with exit code $LASTEXITCODE."
        }
    }
}

$metadata = Get-Content -Raw -Encoding utf8 -LiteralPath (Join-Path $publishRoot "tile-metadata.json") | ConvertFrom-Json
$reportTiles = foreach ($tile in $metadata.tiles) {
    $file = Get-Item -LiteralPath (Join-Path $publishRoot $tile.contentUri)
    [ordered]@{
        id = $tile.id
        file = $file.Name
        bytes = $file.Length
        nodes = $tile.stats.nodeCount
        meshes = $tile.stats.meshCount
        triangles = $tile.stats.triangleCount
        blenderWorldBounds = $tile.blenderWorldBounds
        gltfBounds = $tile.bounds
    }
}

$report = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString("O")
    durationMilliseconds = [math]::Round(([DateTime]::UtcNow - $start).TotalMilliseconds, 0)
    blender = & $resolvedBlenderPath --version | Select-Object -First 1
    gltfTransform = (& $gltfTransform.Source --version | Select-Object -First 1)
    compression = [ordered]@{ draco = $false; meshopt = $false; ktx2 = $false; webp = $false }
    analysis = $analysisStatus
    tiles = $reportTiles
}
$report | ConvertTo-Json -Depth 8 | Set-Content -Encoding utf8 -LiteralPath (Join-Path $reportsRoot "build-report.json")

Write-Host "Generated tileset: $publishRoot"
Write-Host "Build report: $(Join-Path $reportsRoot 'build-report.json')"
