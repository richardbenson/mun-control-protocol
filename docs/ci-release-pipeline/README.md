# CI / Release Pipeline

## Overview

Automates building, testing, and releasing KSP Mission Control via GitHub Actions. Removes
the requirement for a local KSP installation during packaging and makes every versioned
release a single `git tag` command.

## Scope

- KSP assembly stubs so `KSPMissionControl.Career` compiles in CI without a KSP install
- GitHub Actions CI workflow: build + test on every PR and push
- GitHub Actions release workflow: triggered by `v*` tag push, produces a GitHub Release with the distributable ZIP attached
- Version derived from the git tag only — no version file to keep in sync
- CKAN metadata (`.netkan`) for the GameData mod component, plus a full NetKAN submission guide

## Out of Scope

- Additional MCP platform targets (Linux, macOS) — win-x64 only
- Notarisation / code signing
- Publishing the MCP server to any package registry

## Architecture

```
git tag v0.2.0
  └─> .github/workflows/release.yml
        ├─ dotnet test (all projects, Career via stubs)
        ├─ dotnet publish MCP (win-x64, framework-dependent)
        ├─ dotnet build Career (via stubs)
        ├─ assemble publish/ layout
        ├─ write GameData/KSPMissionControl/KSPMissionControl.version
        ├─ zip → KSPMissionControl-v0.2.0.zip
        └─> GitHub Release (draft=false, attach zip)
```

## Stub strategy

Five C# stub projects live in `lib/stubs/`. Each has `<AssemblyName>` matching the real KSP/Unity/kRPC DLL name so that `KSPMissionControl.Career.dll`, compiled against stubs in CI, resolves at KSP runtime against the real installed assemblies — identical assembly names, same public API surface, no implementation bodies. `Career.csproj` selects real DLLs when `$(KspInstallDir)` is set, stubs otherwise.

## CKAN

CKAN indexes only the `GameData/KSPMissionControl/` portion. Users install the MCP server
separately from the GitHub Release page (covered in INSTALL.md and the CKAN description).

## Definition of Done

- [ ] `dotnet build src/KSPMissionControl.Career/` succeeds on a machine with no KSP installed
- [ ] `dotnet test` passes in GitHub Actions on every PR
- [ ] Pushing `git tag v0.2.0 && git push --tags` creates a GitHub Release with `KSPMissionControl-v0.2.0.zip` attached, no manual steps
- [ ] The ZIP contains `mcp/`, `GameData/KSPMissionControl/`, `INSTALL.md`, `claude_desktop_config.example.json`, and `KSPMissionControl.version`
- [ ] `ckan/ksp-mission-control.netkan` is valid NetKAN JSON, passes `netkan` lint
- [ ] `docs/CKAN-SUBMISSION.md` documents every step to submit to the public NetKAN repo

## Documents

| File | Purpose |
|------|---------|
| [PROGRESS.md](PROGRESS.md) | Phase status tracker |
| [PHASE_01.md](PHASE_01.md) | KSP Assembly Stubs — detail |
| [PHASE_01.prompt.md](PHASE_01.prompt.md) | KSP Assembly Stubs — executor prompt |
| [PHASE_02.md](PHASE_02.md) | CI Build & Test Workflow — detail |
| [PHASE_02.prompt.md](PHASE_02.prompt.md) | CI Build & Test Workflow — executor prompt |
| [PHASE_03.md](PHASE_03.md) | Release Workflow + Packaging — detail |
| [PHASE_03.prompt.md](PHASE_03.prompt.md) | Release Workflow + Packaging — executor prompt |
| [PHASE_04.md](PHASE_04.md) | CKAN Metadata & Submission Guide — detail |
| [PHASE_04.prompt.md](PHASE_04.prompt.md) | CKAN Metadata & Submission Guide — executor prompt |
