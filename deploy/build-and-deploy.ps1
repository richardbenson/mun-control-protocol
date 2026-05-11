#Requires -Version 5.1
<#
.SYNOPSIS
    Builds KSPMissionControl.Career and deploys the DLL to KSP GameData.

.PARAMETER KspInstallDir
    Path to the KSP installation root (contains KSP_x64.exe).
    Falls back to $env:KspInstallDir if not provided.

.EXAMPLE
    .\build-and-deploy.ps1 "C:\Games\KSP"
    .\build-and-deploy.ps1   # uses $env:KspInstallDir
#>
param(
    [string]$KspInstallDir = $env:KspInstallDir
)

$ErrorActionPreference = "Stop"

if (-not $KspInstallDir) {
    Write-Error "KspInstallDir is required. Pass it as an argument or set `$env:KspInstallDir."
}

$projectPath = Join-Path $PSScriptRoot "..\src\KSPMissionControl.Career\KSPMissionControl.Career.csproj"
$destDir     = Join-Path $KspInstallDir "GameData\KSPMissionControl"

Write-Host "Building KSPMissionControl.Career (Release)..."
dotnet build $projectPath -c Release /p:KspInstallDir="$KspInstallDir" --nologo -v minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$buildOut = Join-Path $PSScriptRoot "..\src\KSPMissionControl.Career\bin\Release\net472"

if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir | Out-Null
}

# Copy Career DLL + Shared DLL (dependency) to GameData.
foreach ($dll in @("KSPMissionControl.Career.dll", "KSPMissionControl.Shared.dll")) {
    $src = Join-Path $buildOut $dll
    Copy-Item -Path $src -Destination $destDir -Force
    Write-Host "  Deployed $dll -> $destDir"
}

Write-Host "Done. Restart KSP to load the updated addon."
