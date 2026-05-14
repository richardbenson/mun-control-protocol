# Phase 3 — MCP Plumbing + EditorTools

## Summary

Wire the new `GetCurrentCraft` kRPC procedure through the MCP server and expose it as the `get_current_craft` MCP tool.

## Context

Phase 2 must be merged first (provides the `GetCurrentCraft` kRPC procedure in `TechTreeService`).

The MCP plumbing follows a strict four-file pattern — look at how any existing method flows through:
1. `IKrpcConnection` — interface declaration
2. `MunControlProtocolStubs.cs` — kRPC protobuf client stub
3. `KrpcConnection.cs` — implementation (calls the stub)
4. A `*Tools.cs` file — the MCP tool using `IKrpcConnection`

For `get_current_craft` the tool method returns `CraftDesign?` (nullable). If the JSON from kRPC is `"null"`, the tool returns `null` to the LLM (no object — MCP serialises this as JSON null).

## Files Expected to Change

| File | Change |
|------|--------|
| `src/MunControlProtocol.MCP/Krpc/IKrpcConnection.cs` | Add `string GetCurrentCraft()` |
| `src/MunControlProtocol.MCP/Krpc/MunControlProtocolStubs.cs` | Add `GetCurrentCraft()` stub method |
| `src/MunControlProtocol.MCP/Krpc/KrpcConnection.cs` | Implement `IKrpcConnection.GetCurrentCraft()` |
| `src/MunControlProtocol.MCP/Tools/EditorTools.cs` | **New file** — `get_current_craft` MCP tool |

## Tool Description (for the LLM)

The XML doc on `GetCurrentCraftAsync` should say:

> Returns the craft currently open in the Vehicle Assembly Building (VAB) or Space Plane Hangar (SPH), or null if the editor is not open or no craft is loaded.
> Each part includes: name, title, dry mass (massT), resource mass (resourceMassT), cost, stageIndex (0 = last stage to fire), resources with current fill levels, and any installed modules (engine, tank, command pod, antenna, solar panel).
> Use stageIndex to group parts by stage, then apply the Tsiolkovsky formula (formula_delta_v) with stage wet/dry mass and mass-weighted ISP to estimate delta-v per stage.

## Acceptance Criteria

- MCP project (net8) builds cleanly with no errors.
- `get_current_craft` appears in the registered tool list.
- When kRPC returns `"null"`, the tool returns `null`.
- When kRPC returns valid JSON, the tool returns a deserialised `CraftDesign`.
- Existing tests still pass (`dotnet test`).
