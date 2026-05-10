# Phase 6 — Science

## Summary
Add `ScienceService` to the Career extension and expose `get_science_status` on the MCP server. The tool answers "what science have I done, what's left, and what's the diminishing return state?" with optional filters for body and situation to keep response sizes manageable.

## Context
The full experiment × biome × situation × body matrix is enormous (a stock save can have 10,000+ unique science subjects when fully iterated). Returning the whole thing on every call would dominate context budgets. The tool **requires** filtering by body in practice, with optional situation narrowing on top.

KSP exposes science subjects via `ResearchAndDevelopment.GetExperimentSubjects()`. Each `ScienceSubject` has an `id` of the form `experimentId@bodyName{Situation}{Biome}` (e.g. `crewReport@KerbinFlyingHighlands`), plus per-subject science earned, science cap, multipliers, and `subjectValue` (the diminishing-return scaling factor).

## Files expected to change
- `src/KSPMissionControl.Career/Services/ScienceService.cs` — new
- `src/KSPMissionControl.Shared/Models/ScienceSubject.cs` — new
- `src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs` — regenerated
- `src/KSPMissionControl.MCP/Tools/ScienceTools.cs` — new
- `src/KSPMissionControl.Career/KSPMissionControlAddon.cs` — extend to refresh science cache
- `tests/KSPMissionControl.MCP.Tests/ScienceToolsTests.cs` — new

## Acceptance criteria
1. `get_science_status body=Kerbin` returns all science subjects pertaining to Kerbin, with `Earned`, `Cap`, `Remaining` (`Cap - Earned`), `SubjectValue` (diminishing-return scaling 0..1).
2. `get_science_status body=Kerbin situation=FlyingLow` further narrows to flying-low subjects only.
3. `get_science_status` with no body returns a warning message and refuses to dump the entire matrix (or returns a small summary instead — see prompt).
4. Filter values match what's visible in-game in the Mission Archives / R&D archives screen.
5. `dotnet test` passes.
6. PR opened against `feature/ksp-mission-control` from `feature/ksp-mission-control-phase-06`.
