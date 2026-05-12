# Phase 03 — Release Workflow + Packaging

## Goal

Pushing a `v*` tag to GitHub automatically builds the release ZIP and creates a GitHub Release
with the ZIP attached as an asset. No manual packaging step is needed.

## Trigger

```yaml
on:
  push:
    tags: ['v*']
```

## Version extraction

The tag name (e.g. `v0.2.0`) is the sole version source. The workflow strips the leading `v`:

```bash
VERSION="${GITHUB_REF_NAME#v}"   # v0.2.0 → 0.2.0
```

This version string is passed to all build steps that embed a version (AVC file, zip filename).

## Release job steps

1. Checkout (with full history: `fetch-depth: 0`)
2. Setup .NET 8 SDK
3. Cache NuGet packages
4. Restore solution
5. Build stubs (same five commands as CI workflow)
6. Build + test solution (`-c Release`)
7. Publish MCP server (`dotnet publish`, win-x64, framework-dependent, output to `publish/mcp/`)
8. Build Career DLL (stubs, Release, output to `src/KSPMissionControl.Career/bin/Release/net472/`)
9. Build Shared DLL (already built in step 6, but ensure Release build)
10. Assemble `publish/` directory layout:
    ```
    publish/
      mcp/                          ← from dotnet publish
      GameData/KSPMissionControl/
        KSPMissionControl.Career.dll
        KSPMissionControl.Shared.dll
        KSPMissionControl.version   ← written in step 11
      INSTALL.md
      claude_desktop_config.example.json
    ```
11. Write `publish/GameData/KSPMissionControl/KSPMissionControl.version`:
    ```json
    {
      "NAME": "KSP Mission Control",
      "URL": "https://raw.githubusercontent.com/richardbenson/ksp-mission-control/main/GameData/KSPMissionControl/KSPMissionControl.version",
      "DOWNLOAD": "https://github.com/richardbenson/ksp-mission-control/releases",
      "VERSION": { "MAJOR": X, "MINOR": Y, "PATCH": Z, "BUILD": 0 },
      "KSP_VERSION_MIN": { "MAJOR": 1, "MINOR": 12, "PATCH": 0 },
      "KSP_VERSION_MAX": { "MAJOR": 1, "MINOR": 12, "PATCH": 9 }
    }
    ```
    The workflow parses the `VERSION` string and splits it into MAJOR/MINOR/PATCH integers.
12. Zip `publish/*` → `KSPMissionControl-v$VERSION.zip`
13. Create GitHub Release using `softprops/action-gh-release@v2`:
    - `name: KSP Mission Control v$VERSION`
    - `draft: false`
    - `prerelease: false` (set to `true` if the tag contains `-` e.g. `v0.2.0-rc1`)
    - `body`: brief changelog placeholder or leave blank (user edits after)
    - `files: KSPMissionControl-v$VERSION.zip`

## GitHub token

The release creation step needs `permissions: contents: write` at the job level.

## `package-release.ps1` update

Add a note at the top of `deploy/package-release.ps1` that for official releases CI now
handles packaging — the script remains useful for local testing only. Add a `[switch]$NoKsp`
parameter that, when set, skips the `KspInstallDir` requirement check so the script can be
run in the same stub-based mode locally:

```powershell
if (-not $NoKsp -and -not $KspInstallDir) {
    Write-Error "KspInstallDir is required ..."
}
```

If `$NoKsp` is set, the Career build step omits `/p:KspInstallDir=...`.

## Files expected to change

| File | Change |
|---|---|
| `.github/workflows/release.yml` | new |
| `deploy/package-release.ps1` | add `-NoKsp` switch |

## Acceptance criteria

1. Pushing `git tag v0.2.0 && git push --tags` triggers the release workflow
2. A GitHub Release named "KSP Mission Control v0.2.0" is created with the ZIP attached
3. The ZIP contains exactly: `mcp/`, `GameData/KSPMissionControl/`, `INSTALL.md`,
   `claude_desktop_config.example.json`, `KSPMissionControl.version` inside GameData
4. `KSPMissionControl.version` contains the correct MAJOR/MINOR/PATCH from the tag
5. A pre-release tag (e.g. `v0.2.0-rc1`) produces a GitHub pre-release
6. `deploy/package-release.ps1 -NoKsp -Version 0.2.0` runs locally without requiring `KspInstallDir`
