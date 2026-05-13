#Requires -Version 5.1
<#
.SYNOPSIS
    Builds and packages Mun Control Protocol into a distributable zip.

.DESCRIPTION
    Produces MunControlProtocol-vX.Y.Z.zip containing:
      - mcp/MunControlProtocol.MCP.exe : single self-contained exe (win-x64, no .NET runtime required)
      - GameData/MunControlProtocol/    : MunControlProtocol.Career.dll + MunControlProtocol.Shared.dll
      - INSTALL.md
      - claude_desktop_config.example.json

.PARAMETER Version
    Version string used in the zip filename. Defaults to "0.1.0".

.PARAMETER KspInstallDir
    Path to the KSP installation root (contains KSP_x64.exe).
    Falls back to $env:KspInstallDir if not provided.
    Required only for the Career DLL build — not needed for the MCP exe.

.PARAMETER NoKsp
    Build the Career DLL against the pre-built stubs instead of a real KSP install.
    Stubs must already be built (dotnet build lib/stubs/...). Use this in CI or on
    machines without KSP installed.

.EXAMPLE
    .\package-release.ps1 -Version "0.1.0" -KspInstallDir "C:\Games\KSP"
    .\package-release.ps1   # uses $env:KspInstallDir, version 0.1.0
    .\package-release.ps1 -NoKsp -Version "0.2.0"
#>
param(
    [string]$Version       = "0.1.0",
    [string]$KspInstallDir = $env:KspInstallDir,
    [switch]$NoKsp
)

$ErrorActionPreference = "Stop"

if (-not $NoKsp -and -not $KspInstallDir) {
    Write-Error "KspInstallDir is required. Pass -KspInstallDir, set `$env:KspInstallDir, or use -NoKsp to build with stubs."
}

$repoRoot   = Resolve-Path (Join-Path $PSScriptRoot "..")
$publishDir = Join-Path $repoRoot "publish"
$mcpOut     = Join-Path $publishDir "mcp"
$gameDataOut = Join-Path $publishDir "GameData\MunControlProtocol"
$zipPath    = Join-Path $repoRoot "MunControlProtocol-v$Version.zip"

# ── Clean previous publish ────────────────────────────────────────────────────
if (Test-Path $publishDir) {
    Write-Host "Cleaning previous publish output..."
    Remove-Item -Recurse -Force $publishDir
}
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

# ── 1. Publish the MCP server (win-x64, self-contained single file) ──────────
Write-Host ""
Write-Host "Publishing MunControlProtocol.MCP (win-x64, self-contained single file)..."
$mcpProject = Join-Path $repoRoot "src\MunControlProtocol.MCP\MunControlProtocol.MCP.csproj"
dotnet publish $mcpProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false -o $mcpOut --nologo -v minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "  MCP server published -> $mcpOut"

# ── 2. Build stubs (when no KSP install) + Career DLL ────────────────────────
if (-not $KspInstallDir) {
    Write-Host ""
    Write-Host "Building stubs (no KSP install)..."
    $stubProjects = @(
        "Assembly-CSharp", "UnityEngine.CoreModule", "UnityEngine", "KRPC.Core", "KRPC.SpaceCenter"
    )
    foreach ($stub in $stubProjects) {
        dotnet build (Join-Path $repoRoot "lib\stubs\$stub\$stub.csproj") -c Release --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}

Write-Host ""
Write-Host "Building MunControlProtocol.Career (Release)..."
$careerProject = Join-Path $repoRoot "src\MunControlProtocol.Career\MunControlProtocol.Career.csproj"
if ($KspInstallDir) {
    dotnet build $careerProject -c Release /p:KspInstallDir="$KspInstallDir" --nologo -v minimal
} else {
    dotnet build $careerProject -c Release --nologo -v minimal
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

New-Item -ItemType Directory -Path $gameDataOut -Force | Out-Null

$careerBin = Join-Path $repoRoot "src\MunControlProtocol.Career\bin\Release\net472"
$sharedBin = Join-Path $repoRoot "src\MunControlProtocol.Shared\bin\Release\net472"

Copy-Item -Path (Join-Path $careerBin "MunControlProtocol.Career.dll") -Destination $gameDataOut -Force
Copy-Item -Path (Join-Path $sharedBin "MunControlProtocol.Shared.dll") -Destination $gameDataOut -Force
Write-Host "  Career + Shared DLLs staged -> $gameDataOut"

# ── 3. Copy install docs and example config ───────────────────────────────────
Write-Host ""
Write-Host "Copying install docs..."
Copy-Item -Path (Join-Path $repoRoot "INSTALL.md")                                          -Destination $publishDir -Force
Copy-Item -Path (Join-Path $PSScriptRoot "claude_desktop_config.example.json")              -Destination $publishDir -Force
Write-Host "  INSTALL.md and claude_desktop_config.example.json staged"

# ── 4. Zip everything ─────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Creating zip: $zipPath"
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force
Write-Host ""
Write-Host "Release package ready: $zipPath"
Write-Host ""

# ── 5. Show extracted structure ───────────────────────────────────────────────
Write-Host "Contents:"
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
$zip.Entries | Sort-Object FullName | ForEach-Object { Write-Host "  $($_.FullName)" }
$zip.Dispose()
