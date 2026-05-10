# Phase 1 — Scaffolding & repo

## Summary
Establish the empty solution skeleton, three project files with correct target frameworks, project references, a per-machine `Directory.Build.props` for KSP install paths, .gitignore, and a private GitHub repo. The solution must build cleanly with `dotnet build` after the developer fills in `KspInstallDir`.

## Context
This phase introduces no application logic. Its job is to make every subsequent phase a pure code-writing exercise — never a "now I have to fight csproj for an hour" exercise. The KSP/Unity DLL paths vary per developer machine, so they're externalised behind one MSBuild property in a git-ignored file.

Three target frameworks are in play because:
- KSP runs on Unity 2019.4 → forces `KSPMissionControl.Career` to `net472`
- Modern MCP NuGet → `KSPMissionControl.MCP` on `net8.0`
- Shared code is consumed by both → `netstandard2.0` for `KSPMissionControl.Shared`

## Files expected to change (created)
- `KSPMissionControl.sln`
- `Directory.Build.props` (root, repo-wide defaults)
- `Directory.Build.props.template` (per-machine paths template, committed; real one is git-ignored)
- `src/KSPMissionControl.Shared/KSPMissionControl.Shared.csproj`
- `src/KSPMissionControl.Career/KSPMissionControl.Career.csproj`
- `src/KSPMissionControl.MCP/KSPMissionControl.MCP.csproj`
- `src/KSPMissionControl.MCP/Program.cs` (single `Console.WriteLine` placeholder)
- `.gitignore`
- `README.md` (one-paragraph stub linking to `docs/ksp-mission-control/`)

## Acceptance criteria
1. `dotnet build KSPMissionControl.sln` completes with zero errors and zero warnings on a fresh checkout (after `Directory.Build.props.user` is created from the template).
2. `KSPMissionControl.MCP.dll` and `KSPMissionControl.Career.dll` are produced under `bin/`.
3. The repo is pushed to a **private** GitHub repo named `ksp-mission-control` (or owner-chosen equivalent) under the user's account; `feature/ksp-mission-control` exists as the working feature branch.
4. The work for this phase lands on `feature/ksp-mission-control-phase-01`, which is then PR'd into `feature/ksp-mission-control`.
