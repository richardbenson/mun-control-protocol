# Phase 5 — Module-specific part stats

## Summary
Extend `PartInfo` and `PartsService` to capture the per-module stats that actually matter for mission planning: engine performance, antenna range and combinability, tank resource capacities, command pod crew, battery capacity, solar panel output. The `get_part_stats` MCP tool's response shape grows; `get_parts_by_category` is unchanged in surface area but its returned objects gain the new fields.

## Context
This is the most reflection-heavy code in the project. KSP's parts are composed of `PartModule` subclasses (`ModuleEngines`, `ModuleEnginesFX`, `ModuleDeployableAntenna`, `ModuleDataTransmitter`, `ModuleCommand`, `ModuleDeployableSolarPanel`, etc.) and a part can carry multiple modules. We don't want a giant base DTO with every possible field; instead, expose a small set of optional, typed sub-DTOs that are populated only when the relevant module is present.

This is split from Phase 4 specifically so the per-module mapping work can iterate without dragging the whole Phase 4 catalog plumbing through revisions.

## Files expected to change
- `src/KSPMissionControl.Shared/Models/PartInfo.cs` — extend with optional sub-DTO properties
- `src/KSPMissionControl.Shared/Models/PartModules/` — new directory, one file per module sub-DTO
- `src/KSPMissionControl.Career/Services/PartsService.cs` — extend cache-population with module reflection
- `src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs` — regenerated

## Acceptance criteria
1. `get_part_stats part_name=<engine>` returns engine sub-DTO with thrust (vacuum and ASL, kN), Isp (vacuum and ASL), fuel-flow rate, propellant resource names.
2. `get_part_stats part_name=<antenna>` returns antenna sub-DTO with range (metres), antenna type (`Internal` / `Direct` / `Relay`), combinable flag, packet size and interval.
3. `get_part_stats part_name=<fuel tank>` returns tank sub-DTO listing each resource (name, capacity).
4. `get_part_stats part_name=<command pod>` returns command sub-DTO with crew capacity, has-SAS flag, SAS level (0-3 if applicable), generates-power flag (some pods generate hibernation power).
5. `get_part_stats part_name=<battery>` returns the same tank sub-DTO with `ElectricCharge` capacity (batteries are storage parts; reuse the tank model).
6. `get_part_stats part_name=<solar panel>` returns solar sub-DTO with charge rate at 1 AU (kW), retractable flag.
7. Parts with none of the above modules return a `PartInfo` with all sub-DTO properties null. The base fields (mass, cost, category, tech) still populate.
8. `dotnet test` passes.
9. PR opened against `feature/ksp-mission-control` from `feature/ksp-mission-control-phase-05`.
