Read `docs/editor-craft-reader/PHASE_02.md` for full context before starting.

## Task

You are implementing Phase 2 of the Editor Craft Reader feature: the KSP addon side. Phase 1 (stubs + shared models) must already be merged into `feature/editor-craft-reader`.

Work in branch `feature/editor-craft-reader-phase-2` (branch off `feature/editor-craft-reader`).

---

## 1. New file: `src/MunControlProtocol.Career/Services/EditorService.cs`

Read `src/MunControlProtocol.Career/Services/BuildingsService.cs` and `src/MunControlProtocol.Career/Services/PartsService.cs` as your style references.

Implement:

```csharp
using MunControlProtocol.Career.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MunControlProtocol.Career.Services;

public static class EditorService
{
    internal static readonly StateCache<string> Cache = new();

    internal static string GetCurrentCraft() => Cache.Snapshot ?? "null";

    internal static void RefreshCache() { ... }

    private static string BuildJson() { ... }
    private static void AppendModuleInfo(StringBuilder sb, Part part) { ... }
    private static string JsonString(string s) { ... }
}
```

**`RefreshCache()`**: wrap `BuildJson()` in a try/catch like the other services. If `EditorLogic.fetch == null` or `EditorLogic.fetch.ship == null`, call `Cache.Update("null")` and return.

**`BuildJson()`**: read from `EditorLogic.fetch.ship`. The output shape is documented in `PHASE_02.md`. Key points:
- `editorType` = `ship.shipFacility == EditorFacility.VAB ? "VAB" : "SPH"`
- For each part in `ship.parts`, emit a JSON object. Use `part.partInfo.name`, `part.partInfo.title`, `part.partInfo.cost`, `part.mass`, `part.inverseStage`, `part.CrewCapacity`.
- `resourceMassT` = sum of `res.amount * res.info.density` across `part.Resources`.
- `totalMassT` = sum of `part.mass + resourceMassT` across all parts.
- `totalCost` = sum of `part.partInfo.cost` across all parts.
- `crewCapacity` = sum of `part.CrewCapacity` across all parts.

**`AppendModuleInfo(StringBuilder sb, Part part)`**: copy the logic from `PartsService.AppendModuleInfo` exactly — same JSON keys, same fields. This keeps the output shape identical so the MCP layer can reuse the same `PartModules` deserialisers. The method is `private static`.

**Resources JSON**: inside each part object, emit:
```json
"resources": [{"name": "LiquidFuel", "amount": 360.0, "maxAmount": 360.0}, ...]
```
Emit an empty array `[]` if the part has no resources.

**`JsonString(string s)`**: copy verbatim from `PartsService.JsonString` (escape `\`, `"`, `\n`, `\r`, `\t`).

---

## 2. Modify `src/MunControlProtocol.Career/Services/TechTreeService.cs`

Add one procedure at the end of the procedures block (after `GetKerbals`, before `RefreshCache`):

```csharp
/// <summary>Returns the current craft in the VAB or SPH as a JSON object, or "null" if no craft is loaded or the editor is not open.</summary>
[KRPCProcedure]
public static string GetCurrentCraft() => EditorService.GetCurrentCraft();
```

---

## 3. Modify `src/MunControlProtocol.Career/MunControlProtocolAddon.cs`

Read the file first. Add a `_lastEditorRefresh` field (initialised to `-999f`) alongside the existing timer fields.

In `Update()`, within the EDITOR scene block, add an editor-specific refresh after the existing 1s tech tree refresh:

```csharp
// Editor craft data changes as the player places/removes parts — 1s is sufficient.
if (scene == GameScenes.EDITOR)
{
    if (Time.realtimeSinceStartup - _lastEditorRefresh >= 1f)
    {
        _lastEditorRefresh = Time.realtimeSinceStartup;
        EditorService.RefreshCache();
    }
}
```

Place this block after the `_lastTechTreeRefresh` gate but before the 5s science/buildings gate, so it runs every time the tech tree also refreshes.

---

## Build Verification

```
dotnet build src/MunControlProtocol.Career
```

Must succeed with zero errors.

---

## Completion

Update `docs/editor-craft-reader/PROGRESS.md`: set Phase 2 status to `complete`, fill in the completed date.

Open a PR from `feature/editor-craft-reader-phase-2` targeting `feature/editor-craft-reader`.
