#Requires -Version 5.1
<#
.SYNOPSIS
    Builds and deploys MunControlProtocol.Career, regenerates kRPC stubs if
    Python is available, then builds MunControlProtocol.MCP.

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

$projectPath = Join-Path $PSScriptRoot "..\src\MunControlProtocol.Career\MunControlProtocol.Career.csproj"
$mcpProject  = Join-Path $PSScriptRoot "..\src\MunControlProtocol.MCP\MunControlProtocol.MCP.csproj"
$destDir     = Join-Path $KspInstallDir "GameData\MunControlProtocol"
$stubsOut    = Join-Path $PSScriptRoot "..\src\MunControlProtocol.MCP\Krpc\MunControlProtocolStubs.cs"

# 1. Build and deploy the Career DLL
Write-Host "Building MunControlProtocol.Career (Release)..."
dotnet build $projectPath -c Release /p:KspInstallDir="$KspInstallDir" --nologo -v minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$buildOut  = Join-Path $PSScriptRoot "..\src\MunControlProtocol.Career\bin\Release\net472"
$sharedOut = Join-Path $PSScriptRoot "..\src\MunControlProtocol.Shared\bin\Release\net472"

if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir | Out-Null
}

Copy-Item -Path (Join-Path $buildOut  "MunControlProtocol.Career.dll") -Destination $destDir -Force
Write-Host "  Deployed MunControlProtocol.Career.dll -> $destDir"
Copy-Item -Path (Join-Path $sharedOut "MunControlProtocol.Shared.dll") -Destination $destDir -Force
Write-Host "  Deployed MunControlProtocol.Shared.dll (net472) -> $destDir"

# 2. Regenerate kRPC stubs from the deployed DLL
#    Needed when a [KRPCProcedure] is added, removed, or its signature changes.
#    Safe to skip if only implementation bodies changed.
$careerDll = Join-Path $destDir "MunControlProtocol.Career.dll"
Write-Host ""

if (Get-Command python -ErrorAction SilentlyContinue) {
    Write-Host "Regenerating kRPC stubs from deployed DLL..."
    $pyScript = @"
import sys
sys.argv = ['krpc-clientgen', 'csharp', 'MunControlProtocol',
            r'$careerDll', '--ksp', r'$KspInstallDir', '-o', r'$stubsOut']
from krpctools.clientgen import main
main()
"@
    $tmpPy = [System.IO.Path]::GetTempFileName() + ".py"
    [System.IO.File]::WriteAllText($tmpPy, $pyScript, [System.Text.Encoding]::UTF8)
    try {
        python $tmpPy
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "krpc-clientgen exited with code $LASTEXITCODE. Stubs NOT updated."
            Write-Warning 'Install with: pip install "krpctools==0.5.4" "setuptools<71"'
            exit $LASTEXITCODE
        }
        Write-Host "  Stubs updated -> $stubsOut"
    } finally {
        Remove-Item $tmpPy -ErrorAction SilentlyContinue
    }
} else {
    Write-Host "  [stub regen skipped -- python not found]"
    Write-Host "  If you changed a [KRPCProcedure] signature, install Python then re-run this script:"
    Write-Host '    pip install "krpctools==0.5.4" "setuptools<71"'
}

# 3. Build the MCP server (picks up any stub changes)
Write-Host ""
Write-Host "Building MunControlProtocol.MCP..."
dotnet build $mcpProject --nologo -v minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 4. Smoke-test: verify all expected MCP tools are registered
$mcpExe    = Join-Path $PSScriptRoot "..\src\MunControlProtocol.MCP\bin\Debug\net8.0\MunControlProtocol.MCP.exe"
$testScript = Join-Path $PSScriptRoot "test-mcp-tools.py"
Write-Host ""

if (Get-Command python -ErrorAction SilentlyContinue) {
    Write-Host "Smoke-testing MCP tool registration..."
    python $testScript $mcpExe
    if ($LASTEXITCODE -ne 0) {
        Write-Error "MCP tool-registration check failed - see output above."
        exit $LASTEXITCODE
    }
} else {
    Write-Host "  [tool registration check skipped -- python not found]"
}

Write-Host ""
Write-Host "Done. Restart KSP to load the updated Career addon."
