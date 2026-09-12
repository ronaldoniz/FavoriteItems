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
$readmePath = Join-Path $projectRoot "README.md"
$changelogPath = Join-Path $projectRoot "CHANGELOG.md"
$licensePath = Join-Path $projectRoot "LICENSE"
$iconPath = Join-Path $projectRoot "icon.png"
$dllPath = Join-Path $projectRoot ("bin\{0}\FavoriteItems.dll" -f $Configuration)

# English is the project's required language for public and user-facing text. This check prevents
# Portuguese runtime wording from earlier builds from returning unnoticed in source or binaries.
$languageAuditFiles = @(
    (Join-Path $projectRoot "README.md"),
    (Join-Path $projectRoot "CHANGELOG.md"),
    (Join-Path $projectRoot "manifest.json"),
    (Join-Path $projectRoot "CONTRIBUTING.md"),
    (Join-Path $projectRoot "DEVELOPMENT.md")
) + @(Get-ChildItem -LiteralPath (Join-Path $projectRoot "src") -Filter "*.cs" -File -Recurse | Select-Object -ExpandProperty FullName) +
    @(Get-ChildItem -LiteralPath (Join-Path $projectRoot "docs") -Filter "*.md" -File -Recurse | Select-Object -ExpandProperty FullName) +
    @(Get-ChildItem -LiteralPath (Join-Path $projectRoot "tests") -Filter "*.cs" -File -Recurse | Select-Object -ExpandProperty FullName)

$legacyNonEnglishPhraseData = @(
    "RmF2b3JpdG86",
    "UmVtb3ZpZG8gZG9zIGZhdm9yaXRvcw==",
    "QXRpdmEgbyBnZXN0bw==",
    "TW9zdHJhIHVtYSBlc3RyZWxh",
    "TW9zdHJhIHVtYSBtZW5zYWdlbQ==",
    "SW1wZWRlIHF1ZQ==",
    "bmFvIGRldGVjdGFkbw==",
    "ZGV0ZWN0YWRvLCBtYXM=",
    "UHJvdGVjYW8gZG8gcXVpY2sgc3RhY2s=",
    "cHJvdGVjYW8gcG9yIGZhdm9yaXRvcw==",
    "RmFsaGEgYW8gY29uc3VsdGFy",
    "TmFvIGZvaSBwb3NzaXZlbA==",
    "Y2FycmVnYWRvLiBVc2UgQWx0K2NsaXF1ZQ=="
)
$legacyNonEnglishPhrases = @($legacyNonEnglishPhraseData | ForEach-Object {
    [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($_))
})

foreach ($phrase in $legacyNonEnglishPhrases) {
    $sourceMatch = Select-String -LiteralPath $languageAuditFiles -SimpleMatch -CaseSensitive:$false -Pattern $phrase
    if ($sourceMatch) {
        throw "English-language policy check failed. Found a legacy Portuguese phrase in $($sourceMatch[0].Path): $phrase"
    }
}

$requiredSourceFiles = @(
    $manifestPath,
    $readmePath,
    $changelogPath,
    $licensePath,
    $iconPath,
    $dllPath
)

foreach ($path in $requiredSourceFiles) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required package source file was not found: $path"
    }
}

$dllBytes = [IO.File]::ReadAllBytes($dllPath)
foreach ($phrase in $legacyNonEnglishPhrases) {
    $needle = [Text.Encoding]::Unicode.GetBytes($phrase)
    $found = $false
    for ($i = 0; $i -le $dllBytes.Length - $needle.Length -and -not $found; $i++) {
        $found = $true
        for ($j = 0; $j -lt $needle.Length; $j++) {
            if ($dllBytes[$i + $j] -ne $needle[$j]) {
                $found = $false
                break
            }
        }
    }
    if ($found) {
        throw "English-language policy check failed. The compiled DLL contains a legacy Portuguese phrase: $phrase"
    }
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
if ($manifest.name -ne "FavoriteItems") {
    throw "manifest.json name must be FavoriteItems."
}
if ($manifest.version_number -notmatch '^\d+\.\d+\.\d+$') {
    throw "manifest.json version_number must use Major.Minor.Patch format."
}
if ([string]::IsNullOrWhiteSpace($manifest.description) -or $manifest.description.Length -gt 250) {
    throw "manifest.json description must contain between 1 and 250 characters."
}
if ($manifest.website_url -ne "https://github.com/ronaldoniz/FavoriteItems") {
    throw "manifest.json website_url must point to the official GitHub repository."
}

$expectedDependency = "denikson-BepInExPack_Valheim-5.4.2350"
$dependencies = @($manifest.dependencies)
if ($dependencies.Count -ne 1 -or $dependencies[0] -ne $expectedDependency) {
    throw "manifest.json must contain only the required dependency $expectedDependency."
}

$projectXml = [xml](Get-Content -Raw -LiteralPath (Join-Path $projectRoot "FavoriteItems.csproj"))
$projectVersion = [string]$projectXml.Project.PropertyGroup.Version
if ($projectVersion -ne $manifest.version_number) {
    throw "Project version $projectVersion does not match manifest version $($manifest.version_number)."
}

$pluginSource = Get-Content -Raw -LiteralPath (Join-Path $projectRoot "src\FavoriteItemsPlugin.cs")
$pluginVersionMatch = [regex]::Match($pluginSource, 'PluginVersion\s*=\s*"(?<version>\d+\.\d+\.\d+)"')
if (-not $pluginVersionMatch.Success -or $pluginVersionMatch.Groups['version'].Value -ne $manifest.version_number) {
    throw "PluginVersion does not match manifest version $($manifest.version_number)."
}

$dll = Get-Item -LiteralPath $dllPath
$expectedFileVersion = $manifest.version_number + ".0"
if ($dll.VersionInfo.FileVersion -ne $expectedFileVersion) {
    throw "DLL file version $($dll.VersionInfo.FileVersion) does not match $expectedFileVersion."
}

Add-Type -AssemblyName System.Drawing
$icon = [System.Drawing.Image]::FromFile($iconPath)
try {
    if ($icon.Width -ne 256 -or $icon.Height -ne 256) {
        throw "icon.png must be exactly 256x256 pixels."
    }
    if ($icon.RawFormat.Guid -ne [System.Drawing.Imaging.ImageFormat]::Png.Guid) {
        throw "icon.png must use PNG format."
    }
}
finally {
    $icon.Dispose()
}

New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$packageName = "ronaldoniz-FavoriteItems-{0}-thunderstore.zip" -f $manifest.version_number
$zipPath = [IO.Path]::GetFullPath((Join-Path $outputRoot $packageName))
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
$stageRoot = [IO.Path]::GetFullPath((Join-Path $systemTempRoot ("FavoriteItems-Thunderstore-" + [Guid]::NewGuid().ToString("N"))))
if (-not $stageRoot.StartsWith($systemTempRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Resolved staging path escaped the system temporary directory."
}

try {
    $pluginsPath = Join-Path $stageRoot "plugins"
    New-Item -ItemType Directory -Path $pluginsPath -Force | Out-Null

    Copy-Item -LiteralPath $iconPath -Destination (Join-Path $stageRoot "icon.png")
    Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $stageRoot "manifest.json")
    Copy-Item -LiteralPath $readmePath -Destination (Join-Path $stageRoot "README.md")
    Copy-Item -LiteralPath $changelogPath -Destination (Join-Path $stageRoot "CHANGELOG.md")
    Copy-Item -LiteralPath $licensePath -Destination (Join-Path $stageRoot "LICENSE")
    Copy-Item -LiteralPath $dllPath -Destination (Join-Path $pluginsPath "FavoriteItems.dll")

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
            [IO.Path]::GetFileName($resolvedStage).StartsWith("FavoriteItems-Thunderstore-", [StringComparison]::Ordinal)) {
            Remove-Item -LiteralPath $resolvedStage -Recurse -Force
        }
    }
}

$expectedEntries = @(
    "CHANGELOG.md",
    "icon.png",
    "LICENSE",
    "manifest.json",
    "plugins/FavoriteItems.dll",
    "README.md"
) | Sort-Object

$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $actualEntries = @($archive.Entries | ForEach-Object { $_.FullName.Replace("\", "/") }) | Sort-Object
}
finally {
    $archive.Dispose()
}

$entryComparison = Compare-Object -ReferenceObject $expectedEntries -DifferenceObject $actualEntries
if ($entryComparison) {
    throw "Package contents did not match the expected Thunderstore structure: $($entryComparison | Out-String)"
}

$dllHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $dllPath).Hash
$zipHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath).Hash

[pscustomobject]@{
    Package = $zipPath
    PackageVersion = $manifest.version_number
    DllVersion = $dll.VersionInfo.FileVersion
    DllSha256 = $dllHash
    PackageSha256 = $zipHash
    Entries = $actualEntries -join ", "
}
