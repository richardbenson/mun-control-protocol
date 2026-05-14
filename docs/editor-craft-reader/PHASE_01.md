# Phase 1 — KSP Stubs + Shared Models

## Summary

Add the KSP compile-time stubs needed for the editor API and create the shared model types that the MCP layer will deserialise into. No application logic in this phase — just types.

## Context

The Career project (net472) compiles against stub assemblies under `lib/stubs/` that mimic the KSP public API at compile time. The Shared project (netstandard2.0) holds DTOs that flow from the kRPC JSON string → MCP deserialiser → tool return value.

The `Part` class stub already exists in `lib/stubs/Assembly-CSharp/KspTypes.cs` but is missing `partInfo` and `inverseStage`. `EditorLogic`, `ShipConstruct`, and `EditorFacility` do not exist yet.

## Files Expected to Change

| File | Change |
|------|--------|
| `lib/stubs/Assembly-CSharp/KspTypes.cs` | Add `EditorLogic`, `ShipConstruct`, `EditorFacility`; add `partInfo` and `inverseStage` to `Part` |
| `src/MunControlProtocol.Shared/Models/CraftDesign.cs` | **New file** — `CraftDesign`, `CraftPart`, `CraftResource` |

## Acceptance Criteria

- Both the Career stub project (net472) and the Shared project (netstandard2.0) build without errors or warnings.
- `CraftPart` references the existing `PartModules` types (`EngineInfo`, `AntennaInfo`, `TankInfo`, `CommandInfo`, `SolarPanelInfo`) by namespace — no duplicated module models.
- No application code, no kRPC attributes, no MCP attributes.
