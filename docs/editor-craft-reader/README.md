# Editor Craft Reader

Adds VAB/SPH craft reading to Mun Control Protocol so the LLM can inspect the current ship design, assess it, and evaluate mission viability.

## Links

- [PROGRESS.md](PROGRESS.md)
- [Phase 1: KSP Stubs + Shared Models](PHASE_01.md)
- [Phase 2: EditorService — KSP Addon](PHASE_02.md)
- [Phase 3: MCP Plumbing + EditorTools](PHASE_03.md)
- [Phase 4: Tests](PHASE_04.md)

## Requirements

1. New `get_current_craft` MCP tool — returns the current craft from the VAB or SPH.
2. Returns `null` when not in the EDITOR scene or when no craft is loaded.
3. Craft-level fields: `name`, `editorType` ("VAB" or "SPH"), `partCount`, `totalMassT`, `totalCost`, `crewCapacity`, `parts`.
4. Per-part fields: `name`, `title`, `massT` (dry), `resourceMassT` (wet minus dry), `cost`, `stageIndex` (`Part.inverseStage`), `resources` (list of `{name, amount, maxAmount}`), and nullable module sub-objects — `engine`, `tank`, `command`, `antenna`, `solarPanel` — using the same `PartModules` types already in `MunControlProtocol.Shared`.
5. No delta-v computation in the addon. The LLM groups parts by `stageIndex` and uses the existing `formula_*` tools to compute Tsiolkovsky delta-v per stage.
6. Data is cached on the Unity main thread using the existing `StateCache<string>` pattern (1 s refresh when scene is EDITOR), so kRPC threads never call KSP APIs directly.
7. Follows exactly the same conventions as existing services (`BuildingsService`, `PartsService`) and tools (`VesselsTools`, `PartsTools`).

## Definition of Done

- `get_current_craft` is registered as an MCP tool and appears in the tool list.
- Calling it while in the VAB/SPH returns a JSON object with the fields listed above, parseable into `CraftDesign`.
- Calling it outside the EDITOR scene returns `null`.
- Unit tests for `EditorTools` pass (`dotnet test`).
- Both Career (net472) and MCP (net8) projects build cleanly.
- Existing test suite continues to pass.

## Architecture Notes

- The Career project (net472, runs inside KSP) serialises data to a JSON string and caches it.
- The kRPC procedure `GetCurrentCraft()` is added to `TechTreeService` (the single `[KRPCService]` class).
- The MCP project (net8) deserialises the JSON into `CraftDesign` and returns it directly from the tool method — matching the `VesselsTools` pattern.
- `CraftPart` reuses `EngineInfo`, `AntennaInfo`, `TankInfo`, `CommandInfo`, `SolarPanelInfo` from `MunControlProtocol.Shared.Models.PartModules`.
