#Requires -Version 5.1
<#
.SYNOPSIS
    Builds and packages KSP Mission Control into a distributable zip.

.DESCRIPTION
    Produces KSPMissionControl-vX.Y.Z.zip containing:
      - mcp/         : KSPMissionControl.MCP.exe and its runtime dependencies (win-x64, framework-dependent)
      - GameData/KSPMissionControl/ : KSPMissionControl.Career.dll + KSPMissionControl.Shared.dll
      - INSTALL.md
      - claude_desktop_config.example.json

    The .NET 8 runtime is NOT bundled (framework-dependent publish). End users must
    install the .NET 8 Runtime separately — see INSTALL.md Prerequisites.

.PARAMETER Version
    Version string used in the zip filename. Defaults to "0.1.0".

.PARAMETER KspInstallDir
    Path to the KSP installation root (contains KSP_x64.exe).
    Falls back to $env:KspInstallDir if not provided.
    Required only for the Career DLL build — not needed for the MCP exe.

.EXAMPLE
    .\package-release.ps1 -Version "0.1.0" -KspInstallDir "C:\Games\KSP"
    .\package-release.ps1   # uses $env:KspInstallDir, version 0.1.0
#>
param(
    [string]$Version      = "0.1.0",
    [string]$KspInstallDir = $env:KspInstallDir
)

$ErrorActionPreference = "Stop"

if (-not $KspInstallDir) {
    Write-Error "KspInstallDir is required. Pass it as -KspInstallDir or set `$env:KspInstallDir."
}

$repoRoot   = Resolve-Path (Join-Path $PSScriptRoot "..")
$publishDir = Join-Path $repoRoot "publish"
$mcpOut     = Join-Path $publishDir "mcp"
$gameDataOut = Join-Path $publishDir "GameData\KSPMissionControl"
$zipPath    = Join-Path $repoRoot "KSPMissionControl-v$Version.zip"

# ── Clean previous publish ────────────────────────────────────────────────────
if (Test-Path $publishDir) {
    Write-Host "Cleaning previous publish output..."
    Remove-Item -Recurse -Force $publishDir
}
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

# ── 1. Publish the MCP server (win-x64, framework-dependent) ─────────────────
Write-Host ""
Write-Host "Publishing KSPMissionControl.MCP (win-x64, framework-dependent)..."
$mcpProject = Join-Path $repoRoot "src\KSPMissionControl.MCP\KSPMissionControl.MCP.csproj"
dotnet publish $mcpProject -c Release -r win-x64 --self-contained false -o $mcpOut --nologo -v minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "  MCP server published -> $mcpOut"

# ── 2. Build the Career DLL and copy into GameData layout ────────────────────
Write-Host ""
Write-Host "Building KSPMissionControl.Career (Release)..."
$careerProject = Join-Path $repoRoot "src\KSPMissionControl.Career\KSPMissionControl.Career.csproj"
dotnet build $careerProject -c Release /p:KspInstallDir="$KspInstallDir" --nologo -v minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

New-Item -ItemType Directory -Path $gameDataOut -Force | Out-Null

$careerBin = Join-Path $repoRoot "src\KSPMissionControl.Career\bin\Release\net472"
$sharedBin = Join-Path $repoRoot "src\KSPMissionControl.Shared\bin\Release\net472"

Copy-Item -Path (Join-Path $careerBin "KSPMissionControl.Career.dll") -Destination $gameDataOut -Force
Copy-Item -Path (Join-Path $sharedBin "KSPMissionControl.Shared.dll") -Destination $gameDataOut -Force
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
