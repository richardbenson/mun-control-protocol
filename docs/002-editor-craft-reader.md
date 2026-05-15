# Editor Craft Reader

**Completed:** 2026-05-15

---

## Original Requirements

Add a `get_current_craft` MCP tool that lets the LLM inspect the ship currently open in the VAB or SPH, assess its design, and evaluate mission viability. The tool returns `null` when not in the EDITOR scene or when no craft is loaded.

Craft-level fields: `name`, `editorType` ("VAB"/"SPH"), `partCount`, `totalMassT`, `totalCost`, `crewCapacity`, `parts`. Per-part fields: `name`, `title`, `massT` (dry), `resourceMassT` (wet minus dry), `cost`, `stageIndex` (`Part.inverseStage`), `resources`, and nullable module sub-objects (`engine`, `tank`, `command`, `antenna`, `solarPanel`) reusing the existing `PartModules` types from `MunControlProtocol.Shared`.

No delta-v computation in the addon — the LLM groups parts by `stageIndex` and uses the existing `formula_*` tools (Tsiolkovsky) per stage. Data is cached on the Unity main thread via `StateCache<string>` with a 1 s refresh, so kRPC threads never touch KSP APIs directly.

---

## Work Done

### Phase 1 — KSP Stubs + Shared Models

Added compile-time stubs for `EditorLogic`, `ShipConstruct`, and `EditorFacility` to `lib/stubs/Assembly-CSharp/KspTypes.cs`, and added the missing `partInfo` and `inverseStage` fields to the existing `Part` stub. Created the new shared model file `src/MunControlProtocol.Shared/Models/CraftDesign.cs` containing `CraftDesign`, `CraftPart`, and `CraftResource`, with `CraftPart` referencing the existing `PartModules` types by namespace — no duplication of module models.

### Phase 2 — EditorService — KSP Addon

Implemented `EditorService` in the Career project: reads `EditorLogic.fetch.ship` on the Unity main thread, hand-builds a JSON string with `StringBuilder` (matching the established pattern from `BuildingsService` and `PartsService`), and stores it in a `StateCache<string>`. Module-reading logic was duplicated from `PartsService.AppendModuleInfo` rather than extracted to keep services independent. The cache is refreshed from `MunControlProtocolAddon.Update()` via a dedicated `_lastEditorRefresh` gate at 1 s cadence. Added the `[KRPCProcedure] GetCurrentCraft()` method to `TechTreeService`; when `EditorLogic.fetch` or `.ship` is null it returns the literal string `"null"`.

### Phase 3 — MCP Plumbing + EditorTools

Wired `GetCurrentCraft` through the four-file MCP plumbing pattern: `IKrpcConnection` interface declaration, `MunControlProtocolStubs.cs` protobuf client stub, `KrpcConnection.cs` implementation, and the new `EditorTools.cs` with the `get_current_craft` MCP tool. The tool method returns `CraftDesign?` — deserialises the kRPC JSON into `CraftDesign`, or returns `null` to the LLM when the JSON is `"null"`.

### Phase 4 — Tests

Created `tests/MunControlProtocol.MCP.Tests/EditorToolsTests.cs` with seven unit tests using `Mock<IKrpcConnection>` via Moq. Tests cover: null JSON, empty JSON, top-level field mapping, multi-part lists, engine module mapping, resource mapping, and null module fields. All existing tests continue to pass.

---

## Lessons Learned

- **Services hand-build JSON with `StringBuilder`** — they do not use `CraftDesign` or any shared model for serialisation. `CraftDesign` is only used by the MCP deserialiser. Future services should follow this asymmetry.
- **Module logic is intentionally duplicated per service** — `PartsService.AppendModuleInfo` is `private static` and is not shared. Copy the pattern rather than extracting a helper; this keeps services independently modifiable.
- **`EditorLogic.fetch` can be null outside the editor** — the cached snapshot must be the literal string `"null"` (not an empty JSON object) so the MCP layer can return a proper JSON null to the LLM.
- **`Part.inverseStage` was missing from the stubs** — the `Part` stub in `KspTypes.cs` lacked both `partInfo` and `inverseStage`; these must be added before any editor-side field access compiles.
- **The four-file MCP plumbing pattern is strict** — every new kRPC call requires changes to all four files (`IKrpcConnection`, stubs, `KrpcConnection`, and a `*Tools.cs`). Skipping any one breaks the build or leaves the tool unregistered.
- **Tests use hand-written JSON constants** — not generated output from `EditorService`. This decouples test correctness from serialisation implementation and catches mapping bugs independently.
