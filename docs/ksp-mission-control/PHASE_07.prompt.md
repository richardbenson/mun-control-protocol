Read `docs/ksp-mission-control/PHASE_07.md`, `PHASE_03.md`, `PHASE_03.prompt.md`, and `PHASE_02.prompt.md` first. This phase reuses the Career-side service pattern (Phase 3) for two new services and the kRPC-passthrough pattern (Phase 2) for three new tools.

You are implementing Phase 7: buildings, difficulty, and three passthrough tools. After this phase, all 10 MCP tools from the requirements doc are live.

## Branch
- Cut `feature/ksp-mission-control-phase-07` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Buildings (Career extension)

#### `src/KSPMissionControl.Shared/Models/BuildingLevels.cs`
Public sealed class with int properties (0-indexed level, where 0 = unupgraded):
- `Vab`, `Sph`, `Launchpad`, `Runway`, `TrackingStation`, `ResearchAndDevelopment`, `AstronautComplex`, `MissionControl`, `Administration`.

#### `src/KSPMissionControl.Career/Services/BuildingsService.cs`
Cache type: `BuildingLevels`. Cache-population (Unity main thread):
- Use `ScenarioUpgradeableFacilities.GetFacilityLevel("SpaceCenter/VehicleAssemblyBuilding")` etc. The facility ID strings are:
  - `SpaceCenter/VehicleAssemblyBuilding`
  - `SpaceCenter/SpaceplaneHangar`
  - `SpaceCenter/LaunchPad`
  - `SpaceCenter/Runway`
  - `SpaceCenter/TrackingStation`
  - `SpaceCenter/ResearchAndDevelopment`
  - `SpaceCenter/AstronautComplex`
  - `SpaceCenter/MissionControl`
  - `SpaceCenter/Administration`
- The returned `float` is 0..1 for some facilities; multiply by `getLevelCount` to get the integer level. Verify by reading `ScenarioUpgradeableFacilities` in `Assembly-CSharp.dll`.
- Refresh cadence: 5 seconds is plenty (building upgrades require leaving the scene anyway).

Service procedure: `GetBuildingLevels()` returns `BuildingLevels`.

### 2. Difficulty (Career extension)

#### `src/KSPMissionControl.Shared/Models/DifficultySettings.cs`
Public sealed class with double / bool properties for at least:
- `CommNetRangeModifier` (double — 1.0 = stock)
- `DsnModifier` (double — DSN strength multiplier)
- `RequireSignalForControl` (bool)
- `RequireSignalForScience` (bool)
- `EnableCommNet` (bool)
- `OccludeBodies` (bool — does CommNet check line-of-sight)
- `RangeModifier` (double — overall range multiplier; some saves use this instead of DSN)
- `ScienceRewardsMultiplier` (double)
- `FundsRewardsMultiplier` (double)
- `ReputationRewardsMultiplier` (double)
- `FundsPenaltiesMultiplier` (double)
- `ReputationPenaltiesMultiplier` (double)
- `ReentryHeatingMultiplier` (double)
- `CrashToleranceMultiplier` (double)
- `PlasmaBlackout` (bool)
- `KerbalGToleranceMultiplier` (double)
- `MissingCrewsRespawn` (bool)

#### `src/KSPMissionControl.Career/Services/DifficultyService.cs`
Cache type: `DifficultySettings`. Cache-population:
- `var pp = HighLogic.CurrentGame.Parameters;`
- `var commNet = pp.CustomParams<CommNet.CommNetParams>();`
- `var difficulty = pp.Difficulty;`
- `var career = pp.Career;`
- Map fields explicitly. Refresh cadence: every scene change is enough; an `Update()`-based 30-second timer is also fine.

Service procedure: `GetDifficultySettings()`.

### 3. Update addon
Extend `KSPMissionControlAddon.cs` to construct both new services and pump their caches. Re-deploy DLL.

### 4. Regenerate stubs
Per Phase 3's recorded command. Replace stubs file.

### 5. MCP wrappers for the two new services
In `CareerTools.cs` (or a new `BuildingsTools.cs` / `DifficultyTools.cs`):
- `get_building_levels` — no params. Description: "Returns the upgrade level of each KSC facility (0-indexed; 0 = unupgraded, max varies per building)."
- `get_difficulty_settings` — no params. Description: "Returns all relevant career difficulty modifiers including CommNet, science/funds rewards, reentry heating, crash tolerance."

### 6. Passthrough DTOs

#### `src/KSPMissionControl.Shared/Models/Vessel.cs`
- `Name`, `Type` (string), `Situation` (string), `Body` (string), `CrewNames` (`IList<string>`), `Apoapsis` (m), `Periapsis` (m), `Inclination` (degrees).

#### `src/KSPMissionControl.Shared/Models/Kerbal.cs`
- `Name`, `ExperienceLevel` (int), `Specialty` (string), `AssignedVessel` (string?, null if available), `Location` (string — `Available` / `Assigned` / `KIA` / `Missing`).

#### `src/KSPMissionControl.Shared/Models/BodyInfo.cs`
- `Name`, `Mass` (kg), `Radius` (m), `AtmosphereHeight` (m, 0 if vacuum), `SoiRadius` (m), `Parent` (string?, null for the Sun), `OrbitalPeriod` (s, null for the Sun), `SemiMajorAxis` (m, null for the Sun).

### 7. MCP passthrough tools (no Career-side work)
Three new files in `src/KSPMissionControl.MCP/Tools/`:

#### `VesselsTools.cs`
- `get_vessels` — no params. Description: "Returns all active vessels (active flights only — does not include debris unless filtered manually). Includes orbital data and crew."
- Iterates `KrpcConnection.SpaceCenter.Vessels` (kRPC built-in property), maps each `KRPC.Client.Services.SpaceCenter.Vessel` into the Shared `Vessel` DTO. Use `vessel.Orbit.Apoapsis - vessel.Orbit.Body.EquatorialRadius` for AGL apoapsis (kRPC returns radii from body center; users want altitudes). Crew names from `vessel.Crew`.
- Filter out `Debris` vessel type by default (mention this in the tool description so the AI knows). Provide an optional `include_debris` bool param if you want to be thorough — mark optional, default false.

#### `KerbalsTools.cs`
- `get_kerbals` — no params. Description: "Returns the full Kerbal roster with experience levels and current assignment."
- Iterates `KrpcConnection.SpaceCenter.AstronautComplex.AvailableCrew`, `AssignedCrew`, etc. (verify exact accessor names on the kRPC `SpaceCenter` C# stub). Map into the Shared `Kerbal` DTO.

#### `BodiesTools.cs`
- `get_body_info` — optional `body` parameter (string). Description: "Returns celestial body data: orbital, atmospheric, and physical properties. Omit `body` for all bodies; provide a name (e.g. 'Mun', 'Duna') for one."
- Iterates `KrpcConnection.SpaceCenter.Bodies` (a dictionary keyed by name). Maps each into `BodyInfo`. If `body` arg given, return single-element list (or empty if not found).

### 8. Tests
For each new tool, one mocked-stub test in `tests/KSPMissionControl.MCP.Tests/`. Don't try to mock the entire kRPC `SpaceCenter` graph — extract a minimal interface for what each tool actually touches and mock that.

### 9. Manual smoke test
- Deploy + regenerate + rebuild.
- In Claude:
  - "what level is my VAB?" — match KSC view.
  - "is CommNet enabled in my save?" — match difficulty options.
  - "list all my active vessels" — match Tracking Station.
  - "show my kerbal roster" — match Astronaut Complex.
  - "what's the SOI of the Mun?" — should return ~2.4 million metres.

### 10. Update PROGRESS
Mark Phase 7 complete. Notes: any kRPC stub property names that differed from this document (the kRPC C# API is largely stable but minor naming surprises happen).

## Edge cases
- **Debris filter**: by default `get_vessels` excludes debris. The AI may ask "list everything including debris" — the optional `include_debris` param handles this without making the default response huge.
- **Kerbal not currently assigned but in EVA**: their `Location` is `Assigned` and `AssignedVessel` is the EVA Kerbal "vessel". Don't filter; expose as-is.
- **Orbiting body with no atmosphere**: `AtmosphereHeight` = 0. Don't return null.
- **Sun**: has no parent and no orbit. Make `Parent`, `OrbitalPeriod`, `SemiMajorAxis` nullable in the DTO. Document the Sun being parent-less in the tool description.
- **Custom planet packs (OPM, JNSQ)**: handled automatically — `Bodies` enumerates everything kRPC sees.
- **Building levels in Sandbox**: all buildings are max level. Don't error; return the max values.
- **DSN modifier vs Range modifier**: depending on save version these may live on different parameter classes. Defensively read both via `CustomParams<>` and document which is null in the response.

## Definition of done
All acceptance criteria in `PHASE_07.md` are met. PR description includes a checklist of all 10 MCP tools confirming each works against the smoke-test save. With this phase merged, the project is functionally complete — Phase 8 is documentation and packaging only.
