# Phase 2 — EditorService — KSP Addon

## Summary

Implement the KSP-side data collection: a new `EditorService` that reads `EditorLogic.fetch.ship` on the Unity main thread, serialises it to JSON, and caches it. Wire the cache refresh into `MunControlProtocolAddon.Update()`. Expose the data via a new `GetCurrentCraft` kRPC procedure on `TechTreeService`.

## Context

Phase 1 must be merged first (provides `EditorLogic`, `ShipConstruct`, `EditorFacility` stubs and the `CraftDesign` shared model).

The existing pattern (from `BuildingsService` and `PartsService`):
- A `static StateCache<string> Cache` stores the latest JSON snapshot.
- `RefreshCache()` is called from `MunControlProtocolAddon.Update()` on the Unity main thread.
- kRPC procedures in `TechTreeService` read `Cache.Snapshot` — never touching KSP APIs directly.

`EditorService` must follow this pattern exactly. The JSON it produces does NOT need to use `CraftDesign` — it hand-builds a JSON string using `StringBuilder` (same as all other services). `CraftDesign` is only used by the MCP layer for deserialisation.

## JSON Shape

```json
{
  "name": "Mun Mission I",
  "editorType": "VAB",
  "partCount": 12,
  "totalMassT": 15.42,
  "totalCost": 52300.0,
  "crewCapacity": 1,
  "parts": [
    {
      "name": "mk1pod",
      "title": "Mk1 Command Pod",
      "massT": 0.84,
      "resourceMassT": 0.056,
      "cost": 600.0,
      "stageIndex": 0,
      "resources": [
        {"name": "ElectricCharge", "amount": 50.0, "maxAmount": 50.0},
        {"name": "Monopropellant", "amount": 10.0, "maxAmount": 10.0}
      ],
      "engine": null,
      "command": {"crewCapacity": 1, "hasSas": true, "sasLevel": 3, "hibernationCharge": 0},
      "antenna": null,
      "tank": null,
      "solarPanel": null
    }
  ]
}
```

Return the literal string `"null"` (not an empty object) if `EditorLogic.fetch == null` or `EditorLogic.fetch.ship == null`.

## Module Info

Reuse the module-reading logic already in `PartsService.AppendModuleInfo`. The method is `private static` — either duplicate the pattern in `EditorService` (preferred, keeps services independent) or extract it to a shared internal helper. Do not make `PartsService.AppendModuleInfo` public.

The module data shape must be identical to what `PartsService` produces (same JSON keys) so the MCP layer can reuse the same `PartModules` model types for deserialisation.

## Mass Calculation

- `part.mass` = dry mass (tonnes)
- `resourceMassT` = sum of `res.amount * res.info.density` over all resources on that part
- `totalMassT` = sum of `(part.mass + resourceMassT)` across all parts

## Files Expected to Change

| File | Change |
|------|--------|
| `src/MunControlProtocol.Career/Services/EditorService.cs` | **New file** — full JSON serialisation logic |
| `src/MunControlProtocol.Career/Services/TechTreeService.cs` | Add `[KRPCProcedure] GetCurrentCraft()` |
| `src/MunControlProtocol.Career/MunControlProtocolAddon.cs` | Add `_lastEditorRefresh` + `EditorService.RefreshCache()` call |

## Acceptance Criteria

- Career project (net472) builds cleanly.
- `GetCurrentCraft()` is registered as a kRPC procedure named `"GetCurrentCraft"` on the `"MunControlProtocol"` service.
- When `EditorLogic.fetch` or `.ship` is null, the cached snapshot is `"null"`.
- When a ship is loaded, the snapshot is valid JSON matching the shape above.
- Refresh rate for editor data: every 1 second (same cadence as tech tree — reuse `_lastTechTreeRefresh` gate or add a dedicated `_lastEditorRefresh` field).
