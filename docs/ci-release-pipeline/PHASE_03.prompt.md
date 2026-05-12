Read docs/ci-release-pipeline/PHASE_03.md for full context before starting.

You are implementing Phase 3 of the CI/Release Pipeline epic for the KSP Mission Control project.
Work in branch `feature/ci-release-pipeline-phase-3` (create it from `feature/ci-release-pipeline`
after Phases 01 and 02 have been merged).

## Objective

Create `.github/workflows/release.yml` so that pushing a `v*` tag produces a GitHub Release
with `KSPMissionControl-vX.Y.Z.zip` attached automatically. Also add a `-NoKsp` switch to
`deploy/package-release.ps1` for local stub-based packaging.

## File 1: `.github/workflows/release.yml`

Read the existing `.github/workflows/ci.yml` (created in Phase 02) and mirror its structure
for consistency. The release workflow is structurally similar but adds packaging and release
creation steps.

```yaml
name: Release

on:
  push:
    tags: ['v*']

permissions:
  contents: write

jobs:
  release:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Extract version
        id: version
        shell: bash
        run: |
          TAG="${GITHUB_REF_NAME}"
          VERSION="${TAG#v}"
          echo "tag=$TAG" >> "$GITHUB_OUTPUT"
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"
          # Split into components for the AVC file
          IFS='.' read -r MAJOR MINOR PATCH_FULL <<< "$VERSION"
          PATCH="${PATCH_FULL%%-*}"   # strip pre-release suffix if present
          echo "major=$MAJOR" >> "$GITHUB_OUTPUT"
          echo "minor=$MINOR" >> "$GITHUB_OUTPUT"
          echo "patch=$PATCH" >> "$GITHUB_OUTPUT"
          # Mark as pre-release if tag contains a hyphen (e.g. v0.2.0-rc1)
          if [[ "$TAG" == *-* ]]; then
            echo "prerelease=true" >> "$GITHUB_OUTPUT"
          else
            echo "prerelease=false" >> "$GITHUB_OUTPUT"
          fi

      - name: Restore
        run: dotnet restore KSPMissionControl.sln

      - name: Build stubs
        run: |
          dotnet build lib/stubs/Assembly-CSharp/Assembly-CSharp.csproj -c Release --no-restore
          dotnet build lib/stubs/UnityEngine.CoreModule/UnityEngine.CoreModule.csproj -c Release --no-restore
          dotnet build lib/stubs/UnityEngine/UnityEngine.csproj -c Release --no-restore
          dotnet build lib/stubs/KRPC.Core/KRPC.Core.csproj -c Release --no-restore
          dotnet build lib/stubs/KRPC.SpaceCenter/KRPC.SpaceCenter.csproj -c Release --no-restore

      - name: Build and test
        run: dotnet test KSPMissionControl.sln -c Release --no-restore --verbosity normal

      - name: Publish MCP server (win-x64)
        run: >
          dotnet publish src/KSPMissionControl.MCP/KSPMissionControl.MCP.csproj
          -c Release -r win-x64 --self-contained false
          -o publish/mcp --no-restore --nologo

      - name: Stage GameData
        shell: bash
        run: |
          mkdir -p publish/GameData/KSPMissionControl
          cp src/KSPMissionControl.Career/bin/Release/net472/KSPMissionControl.Career.dll \
             publish/GameData/KSPMissionControl/
          cp src/KSPMissionControl.Shared/bin/Release/net472/KSPMissionControl.Shared.dll \
             publish/GameData/KSPMissionControl/

      - name: Write AVC version file
        shell: bash
        env:
          MAJOR: ${{ steps.version.outputs.major }}
          MINOR: ${{ steps.version.outputs.minor }}
          PATCH: ${{ steps.version.outputs.patch }}
        run: |
          cat > publish/GameData/KSPMissionControl/KSPMissionControl.version << EOF
          {
            "NAME": "KSP Mission Control",
            "URL": "https://raw.githubusercontent.com/richardbenson/ksp-mission-control/main/GameData/KSPMissionControl/KSPMissionControl.version",
            "DOWNLOAD": "https://github.com/richardbenson/ksp-mission-control/releases",
            "VERSION": { "MAJOR": $MAJOR, "MINOR": $MINOR, "PATCH": $PATCH, "BUILD": 0 },
            "KSP_VERSION_MIN": { "MAJOR": 1, "MINOR": 12, "PATCH": 0 },
            "KSP_VERSION_MAX": { "MAJOR": 1, "MINOR": 12, "PATCH": 9 }
          }
          EOF

      - name: Copy install docs
        shell: bash
        run: |
          cp INSTALL.md publish/
          cp deploy/claude_desktop_config.example.json publish/

      - name: Create ZIP
        shell: bash
        env:
          VERSION: ${{ steps.version.outputs.version }}
        run: |
          cd publish
          7z a "../KSPMissionControl-v${VERSION}.zip" .
          cd ..

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v2
        with:
          name: KSP Mission Control ${{ steps.version.outputs.tag }}
          prerelease: ${{ steps.version.outputs.prerelease }}
          files: KSPMissionControl-${{ steps.version.outputs.tag }}.zip
          body: |
            See [INSTALL.md](https://github.com/richardbenson/ksp-mission-control/blob/main/INSTALL.md) for installation instructions.

            **MCP server:** requires [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).
            **KSP mod:** install to `GameData/` via CKAN or manually.
```

## Implementation notes

- `7z` is available on `windows-latest` GitHub runners via the pre-installed 7-Zip.
  Use `7z a` to create the ZIP from inside the `publish/` directory so paths inside
  the archive are relative (no leading `publish/` prefix in the ZIP entries).

- The `dotnet test` step in this workflow also runs the build (no `--no-build`), because
  we want to ensure a fresh Release build happens. The Career stub build must precede it.

- The `Stage GameData` step copies DLLs from their `bin/Release/net472/` paths. The
  Shared DLL is built as part of the solution build (Career depends on Shared). If the
  Shared DLL isn't present, the stage step will fail visibly.

- The `Write AVC version file` step uses a here-document. On Windows runners, the `bash`
  shell from Git for Windows supports `<<EOF` syntax. Test this; if there are line-ending
  issues on Windows, use PowerShell (`Set-Content`) instead.

- `softprops/action-gh-release@v2` is a well-maintained action for creating GitHub Releases.
  The `permissions: contents: write` at the job level (already in the YAML above) is required.

- Do NOT use `GITHUB_TOKEN` explicitly in the step — the action picks it up automatically
  from the environment when `permissions: contents: write` is set.

## File 2: `deploy/package-release.ps1`

Read the existing file first. Add a `-NoKsp` switch parameter:

```powershell
param(
    [string]$Version       = "0.1.0",
    [string]$KspInstallDir = $env:KspInstallDir,
    [switch]$NoKsp
)

$ErrorActionPreference = "Stop"

if (-not $NoKsp -and -not $KspInstallDir) {
    Write-Error "KspInstallDir is required. Pass -KspInstallDir, set `$env:KspInstallDir, or use -NoKsp to build with stubs."
}
```

In the Career build step, pass `KspInstallDir` only when it's set:

```powershell
if ($KspInstallDir) {
    dotnet build $careerProject -c Release /p:KspInstallDir="$KspInstallDir" --nologo -v minimal
} else {
    dotnet build $careerProject -c Release --nologo -v minimal
}
```

Also update the `Build stubs` comment — add a note that stubs must be pre-built if using `-NoKsp`:

```powershell
if (-not $KspInstallDir) {
    Write-Host "Building stubs (no KSP install)..."
    $stubProjects = @(
        "Assembly-CSharp", "UnityEngine.CoreModule", "UnityEngine", "KRPC.Core", "KRPC.SpaceCenter"
    )
    foreach ($stub in $stubProjects) {
        dotnet build (Join-Path $repoRoot "lib\stubs\$stub\$stub.csproj") -c Release --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}
```

## Verification

You cannot trigger the release workflow locally, but you can verify:
1. The YAML is syntactically valid (consistent 2-space indentation, correct step structure)
2. The version extraction logic handles both `v0.2.0` and `v0.2.0-rc1`
3. The ZIP stage step references paths that will exist after the build steps complete
4. `deploy/package-release.ps1 -NoKsp -Version 0.2.0` runs without error locally

## Completion

When the phase is done:
1. Open `docs/ci-release-pipeline/PROGRESS.md`
2. Set Phase 03 Status to `complete`, fill in the completed date
3. Commit all changes to `feature/ci-release-pipeline-phase-3`
4. Open a PR targeting `feature/ci-release-pipeline`
