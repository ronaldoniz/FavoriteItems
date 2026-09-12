[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $projectRoot "dist"
}
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
$manifestPath = Join-Path $projectRoot "manifest.json"
$dllPath = Join-Path $projectRoot ("bin\{0}\FavoriteItems.dll" -f $Configuration)

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "manifest.json was not found."
}
if (-not (Test-Path -LiteralPath $dllPath -PathType Leaf)) {
    throw "FavoriteItems.dll was not found. Build the project before packaging."
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$expectedFileVersion = $manifest.version_number + ".0"
$dll = Get-Item -LiteralPath $dllPath
if ($dll.VersionInfo.FileVersion -ne $expectedFileVersion) {
    throw "DLL file version $($dll.VersionInfo.FileVersion) does not match $expectedFileVersion."
}

New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$zipPath = [IO.Path]::GetFullPath((Join-Path $outputRoot ("FavoriteItems-{0}-install.zip" -f $manifest.version_number)))
if (-not $zipPath.StartsWith($outputRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Resolved package path escaped the output directory."
}
if (Test-Path -LiteralPath $zipPath) {
    if (-not $Force) {
        throw "Package already exists: $zipPath. Re-run with -Force to replace it."
    }
    Remove-Item -LiteralPath $zipPath -Force
}

$systemTempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$stageRoot = [IO.Path]::GetFullPath((Join-Path $systemTempRoot ("FavoriteItems-Install-" + [Guid]::NewGuid().ToString("N"))))
$pluginDirectory = Join-Path $stageRoot "BepInEx\plugins\ronaldoniz-FavoriteItems"

try {
    New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null
    Copy-Item -LiteralPath $dllPath -Destination (Join-Path $pluginDirectory "FavoriteItems.dll")

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        $stageRoot,
        $zipPath,
        [System.IO.Compression.CompressionLevel]::Optimal,
        $false
    )
}
finally {
    if (Test-Path -LiteralPath $stageRoot) {
        $resolvedStage = [IO.Path]::GetFullPath($stageRoot)
        if ($resolvedStage.StartsWith($systemTempRoot, [StringComparison]::OrdinalIgnoreCase) -and
            [IO.Path]::GetFileName($resolvedStage).StartsWith("FavoriteItems-Install-", [StringComparison]::Ordinal)) {
            Remove-Item -LiteralPath $resolvedStage -Recurse -Force
        }
    }
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = @($archive.Entries | ForEach-Object { $_.FullName.Replace("\", "/") })
}
finally {
    $archive.Dispose()
}

$expectedEntry = "BepInEx/plugins/ronaldoniz-FavoriteItems/FavoriteItems.dll"
if ($entries.Count -ne 1 -or $entries[0] -ne $expectedEntry) {
    throw "Install ZIP contents did not match the expected BepInEx structure."
}

[pscustomobject]@{
    Package = $zipPath
    PackageVersion = $manifest.version_number
    DllVersion = $dll.VersionInfo.FileVersion
    DllSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $dllPath).Hash
    PackageSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath).Hash
    Entry = $entries[0]
}
