Read `docs/ksp-mission-control/PHASE_05.md`, `PHASE_04.md`, and `PHASE_04.prompt.md` first. Phase 5 extends the work from Phase 4; it does not introduce new patterns.

You are implementing Phase 5: per-module part stats.

## Branch
- Cut `feature/ksp-mission-control-phase-05` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Sub-DTOs in Shared
Under `src/KSPMissionControl.Shared/Models/PartModules/`, create one file per module type:

#### `EngineInfo.cs`
- `ThrustVacuum` (kN), `ThrustAsl` (kN), `IspVacuum` (s), `IspAsl` (s), `FuelFlowVacuum` (units/s), `Propellants` (`IList<string>` — resource names like `LiquidFuel`, `Oxidizer`, `MonoPropellant`, `XenonGas`).

#### `AntennaInfo.cs`
- `Range` (metres), `Type` (string: `Internal` / `Direct` / `Relay`), `Combinable` (bool), `PacketSize` (Mits), `PacketInterval` (seconds).

#### `ResourceCapacity.cs`
- `ResourceName` (string), `MaxAmount` (double, units).

#### `TankInfo.cs`
- `Resources` (`IList<ResourceCapacity>`).

#### `CommandInfo.cs`
- `CrewCapacity` (int), `HasSas` (bool), `SasLevel` (int — 0 if no SAS), `HibernationCharge` (double — EC consumed per second when hibernating; 0 if no hibernation).

#### `SolarPanelInfo.cs`
- `ChargeRate` (kW at 1 AU equivalent — KSP exposes this as `chargeRate` in EC/s, which is effectively kW), `Retractable` (bool).

All public sealed classes with get/set properties.

### 2. Extend `PartInfo`
Add nullable properties to `PartInfo`:
- `Engine` (`EngineInfo?`)
- `Antenna` (`AntennaInfo?`)
- `Tank` (`TankInfo?`)
- `Command` (`CommandInfo?`)
- `SolarPanel` (`SolarPanelInfo?`)

A part can populate multiple of these (a command pod may have a battery + integrated antenna + crew + SAS — that's `Command` + `Tank` + `Antenna`). `Tank` covers any storage part: batteries, fuel tanks, monopropellant tanks, xenon tanks.

### 3. Extend `PartsService` cache population
In the per-`AvailablePart` loop (Phase 4), inspect `part.partPrefab.Modules`:

```
foreach (PartModule mod in part.partPrefab.Modules):
  - if mod is ModuleEngines or ModuleEnginesFX: populate Engine
  - if mod is ModuleDataTransmitter (or ModuleDeployableAntenna with associated transmitter): populate Antenna
  - if mod is ModuleCommand: populate Command (crew capacity is on part.partPrefab.CrewCapacity, not the module)
  - if mod is ModuleSAS: extend Command.HasSas / SasLevel
  - if mod is ModuleDeployableSolarPanel: populate SolarPanel
```

Tank info is built from `part.partPrefab.Resources` — every part that defines `RESOURCE` entries gets a `TankInfo`. This catches batteries (`ElectricCharge`), fuel tanks (`LiquidFuel`/`Oxidizer`), monoprop tanks, xenon tanks, and any modded resource container.

For engines:
- `Thrust`: use `ModuleEngines.maxThrust` (vacuum) and recompute ASL via `atmosphereCurve.Evaluate(1.0f)` if needed (Isp curve is keyed on atmospheric pressure 0..1).
- `Isp`: `atmosphereCurve.Evaluate(0)` for vacuum, `atmosphereCurve.Evaluate(1)` for sea level.
- `FuelFlow`: `maxFuelFlow` (units/s).
- `Propellants`: `propellants.Select(p => p.name).ToList()`.

For antennas:
- `Range`: `ModuleDataTransmitter.antennaPower` (this is the antenna's power rating in metres, not raw range — but it's what KSP calls "range" in tooltips).
- `Type`: `antennaType.ToString()` (enum: `Internal`, `Direct`, `Relay`).
- `Combinable`: the `antennaCombinable` field.
- `PacketSize` / `PacketInterval`: direct fields.

Verify all field/property names by reading Squad's source (or the relevant `.cs` files in `Assembly-CSharp.dll` decompiled — most KSP modders have ILSpy handy). The names above are correct for KSP 1.12.x.

### 4. Regenerate stubs
Per Phase 3's recorded command. Replace the stubs file.

### 5. MCP layer
No new MCP tools. The existing `get_part_stats` and `get_parts_by_category` automatically return the richer `PartInfo` because the DTO is a single source of truth. Verify the `PartsTools` mapping logic still compiles cleanly (the mapping from kRPC stub-types to Shared DTOs may need extending if the stubs split into nested types).

### 6. Tests
- One unit test per sub-DTO confirming JSON round-trip in `KSPMissionControl.Shared.Tests`.
- One additional `PartsToolsTests` case confirming that a part with multiple modules returns all sub-DTOs populated, and a structural part (e.g. a strut) returns them all null.

### 7. Manual smoke test
- Deploy + regenerate stubs + rebuild MCP.
- In Claude: "what's the vacuum Isp of the LV-909 Terrier?" — match against in-game tooltip (345s in stock 1.12).
- In Claude: "what's the range of the Communotron 16?" — match against in-game tooltip (500 km direct).
- In Claude: "how much LiquidFuel does an FL-T400 hold?" — match (180 units).
- In Claude: "how much electric charge in a Z-100 battery?" — match (100 EC).

### 8. Update PROGRESS
Mark Phase 5 complete. Notes: any KSP API name surprises (e.g. if `antennaPower` was renamed in 1.12) so future maintainers don't waste time on the same mistakes.

## Edge cases
- **Engines with multi-mode** (`MultiModeEngine`): a part with two engine modules (one for each mode). Decision: capture the **first** `ModuleEngines`/`ModuleEnginesFX` only; document this in the engine sub-DTO summary as "single mode shown for multi-mode engines". Solving multi-mode properly is out of scope for v1.
- **Boil-off / cryo tanks** (modded — Stockalike Station Parts, Near Future): not in stock. Just expose the `Resources` list as-is; they'll appear with their LH2 / LCH4 / etc. resources. Mods that define custom `PartModule` types for boil-off won't be inspected — that's documented as out-of-scope in the requirements doc.
- **Fairings** with internal cargo: the `cargoBays` / `fairings` modules are not modeled. Out of scope for v1.
- **Procedural parts** (modded): same.

## Definition of done
All acceptance criteria in `PHASE_05.md` are met. PR description includes 3-4 in-game tooltip values vs. Claude's responses to demonstrate the spot-checks.
