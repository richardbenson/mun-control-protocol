# Phase 4 — Parts catalog (basic)

## Summary
Add `PartsService` to the Career extension. Expose `get_parts_by_category` and `get_part_stats` on the MCP server, returning basic part metadata only: name, title, category, mass (dry/wet), cost, and the tech node required to unlock. Module-specific stats (engine thrust, antenna range, tank capacities) come in Phase 5.

## Context
This is the largest single data set the MCP layer exposes. Stock KSP has ~430 parts; modded saves can have thousands. The MCP tool **must** require a filter (`part_name` or `category`) — returning the entire catalog unprompted would blow the AI client's context budget.

We split parts work into two phases (this one + Phase 5) so the basic-catalog plumbing lands first and the per-module reflection complexity can iterate independently.

## Files expected to change
- `src/KSPMissionControl.Career/Services/PartsService.cs` — new
- `src/KSPMissionControl.Shared/Models/PartInfo.cs` — new (basic fields only this phase)
- `src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs` — regenerated
- `src/KSPMissionControl.MCP/Tools/PartsTools.cs` — new
- `src/KSPMissionControl.Career/KSPMissionControlAddon.cs` — extend to refresh PartsService cache
- `tests/KSPMissionControl.MCP.Tests/PartsToolsTests.cs` — new

## Acceptance criteria
1. `get_parts_by_category` returns only parts whose tech node is unlocked, filtered by the requested category (engine, fuel tank, command, structural, science, communication, etc. — match KSP's `PartCategories` enum).
2. `get_part_stats` returns a `PartInfo` for the requested `part_name`, OR a list filtered by `category`. The MCP layer rejects calls with neither parameter (return a clean validation error, do not call kRPC).
3. `PartInfo` includes: `Name` (KSP's internal id), `Title` (display name), `Category`, `MassDry` (tonnes), `MassWet` (tonnes — same as dry for parts with no resources), `Cost` (funds), `TechRequired` (tech node id).
4. Spot-check: ask Claude "what command pods are available to me?" — answer matches in-game VAB filtered to "Command" category.
5. `dotnet test` passes.
6. PR opened against `feature/ksp-mission-control` from `feature/ksp-mission-control-phase-04`.
