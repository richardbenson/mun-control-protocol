# Phase 8 — README + INSTALL + release packaging

## Summary
Wrap the project: write the repo's main README, write an end-user `INSTALL.md` covering kRPC mod install, GameData copy, MCP exe placement, and Claude Desktop config, and add a PowerShell release-packaging script that produces a single distributable zip.

No application code changes.

## Context
The repo's stub README from Phase 1 just points here. This phase replaces it with the real thing. `INSTALL.md` is for end users — assume they're a KSP modder familiar with GameData but unfamiliar with C#/MCP.

The release packaging script combines the `KSPMissionControl.MCP` exe + dependencies (from `dotnet publish`), the `KSPMissionControl.Career.dll` (built `Release`), and the example Claude Desktop config into one zip ready to upload as a GitHub release asset.

## Files expected to change
- `README.md` (root) — full rewrite from Phase 1 stub
- `INSTALL.md` — new
- `deploy/package-release.ps1` — new
- `docs/ksp-mission-control/PROGRESS.md` — final update marking project complete

## Acceptance criteria
1. `README.md` includes: project overview, supported KSP version, screenshots/animated demo (or a placeholder note), quick-start link to `INSTALL.md`, contributor link to `docs/ksp-mission-control/`, list of the 10 MCP tools with one-line descriptions.
2. `INSTALL.md` is followable by a fresh user: covers prerequisites (KSP 1.12.x, .NET 8 runtime, kRPC mod), kRPC mod install via CKAN (preferred path) and manual fallback, dropping `KSPMissionControl.Career.dll` into `GameData/KSPMissionControl/`, placing the MCP exe somewhere sane, editing `claude_desktop_config.json` (with the exact JSON snippet and the path on Windows/macOS), restarting Claude Desktop, and a "first query to confirm it works" example.
3. `deploy/package-release.ps1` produces `KSPMissionControl-vX.Y.Z.zip` containing the MCP publish output, the Career DLL, the example config, and INSTALL.md.
4. The zip extracts cleanly and a fresh user can follow INSTALL.md from the extracted contents only.
5. PROGRESS.md shows all 8 phases complete with completion dates.
6. PR opened against `feature/ksp-mission-control` from `feature/ksp-mission-control-phase-08`. After merge, the feature branch PR is opened against `main`.
