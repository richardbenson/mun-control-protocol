# Phase 7 — Buildings, difficulty, built-in passthroughs

## Summary
Close out the MCP tool surface:
- Add two final Career-extension services: `BuildingsService` (VAB/SPH/Launchpad/Runway/Tracking Station/R&D/Admin/Mission Control upgrade levels) and `DifficultyService` (career modifiers including CommNet range/DSN, science rewards, reentry heating, etc.).
- Add three thin MCP wrappers over kRPC built-in services: `get_vessels`, `get_kerbals`, `get_body_info`. No Career extension required for these.

After this phase, all 10 tools from the requirements doc are live.

## Context
`BuildingsService` and `DifficultyService` are intentionally trivial — the values are static-per-save and the service procedures are pure-getter. `Time.realtimeSinceStartup`-throttled refresh as before, but a longer interval (5s+) is fine since these change rarely.

The three passthroughs are MCP-side only because kRPC's `SpaceCenter` already exposes all the underlying data. The work is mapping kRPC's types into the Shared DTOs and registering the tools.

This is the largest phase by file count (~9) but each piece is small.

## Files expected to change
- `src/KSPMissionControl.Career/Services/BuildingsService.cs` — new
- `src/KSPMissionControl.Career/Services/DifficultyService.cs` — new
- `src/KSPMissionControl.Shared/Models/BuildingLevels.cs` — new
- `src/KSPMissionControl.Shared/Models/DifficultySettings.cs` — new
- `src/KSPMissionControl.Shared/Models/Vessel.cs`, `Kerbal.cs`, `BodyInfo.cs` — new (DTOs for the passthroughs)
- `src/KSPMissionControl.Career/KSPMissionControlAddon.cs` — extend
- `src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs` — regenerated
- `src/KSPMissionControl.MCP/Tools/CareerTools.cs` (or split into BuildingsTools / DifficultyTools — your call)
- `src/KSPMissionControl.MCP/Tools/VesselsTools.cs`, `KerbalsTools.cs`, `BodiesTools.cs` — new
- Tests for each of the three passthrough tools

## Acceptance criteria
1. `get_building_levels` returns integer level (0..n) for each of: VAB, SPH, Launchpad, Runway, Tracking Station, R&D, Astronaut Complex, Mission Control, Administration. Match in-game KSC view.
2. `get_difficulty_settings` returns at minimum: CommNet range modifier, DSN modifier, science reward, funds reward, reputation reward, reentry heating, crash tolerance, plasma blackout. Use the field names exposed by `HighLogic.CurrentGame.Parameters` and its `CustomParams<>` accessors.
3. `get_vessels` returns active flights with: name, type (`Probe`, `Lander`, `Ship`, `Station`, `Base`, `Plane`, `Relay`, `Rover`, `EVA`, `Flag`, `Debris`), situation (`Landed`, `Splashed`, `PreLaunch`, `Flying`, `SubOrbital`, `Orbiting`, `Escaping`, `Docked`), body (current SOI), crew names list, and basic orbital data (apoapsis, periapsis, inclination — tonnes/metres, no need for full state vector).
4. `get_kerbals` returns the full roster with: name, experience level (0..5), specialty (Pilot/Engineer/Scientist/Tourist), assigned vessel name (or null if available), location (Available / Assigned / KIA / Missing).
5. `get_body_info` with no arg returns a list of all celestial bodies with name, mass, radius, atmosphere height (0 if no atmosphere), SOI radius, and parent body name. With a `body` arg, returns just that body.
6. `dotnet test` passes.
7. PR opened against `feature/ksp-mission-control` from `feature/ksp-mission-control-phase-07`.
