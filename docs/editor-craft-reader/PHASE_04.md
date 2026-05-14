# Phase 4 — Tests

## Summary

Write unit tests for `EditorTools.GetCurrentCraftAsync`. Coverage should match the depth of `VesselsToolsTests` — happy path, null/empty JSON, and field-mapping assertions.

## Context

Phase 3 must be merged first (provides `EditorTools` and the updated `IKrpcConnection`).

The existing test pattern (see `tests/MunControlProtocol.MCP.Tests/VesselsToolsTests.cs`):
- `Mock<IKrpcConnection>` via Moq
- Arrange: `mock.Setup(c => c.GetCurrentCraft()).Returns(json)`
- Act: `await tool.GetCurrentCraftAsync()`
- Assert on the returned object

The JSON used in tests should be hand-written constants (not generated), matching the shape produced by `EditorService.BuildJson` (documented in `PHASE_02.md`).

## Test Cases Required

| Test | Scenario |
|------|----------|
| `GetCurrentCraftAsync_ReturnsNull_WhenJsonIsNull` | kRPC returns `"null"` → tool returns `null` |
| `GetCurrentCraftAsync_ReturnsNull_WhenJsonIsEmpty` | kRPC returns `""` → tool returns `null` |
| `GetCurrentCraftAsync_MapsTopLevelFields` | Valid JSON → `Name`, `EditorType`, `PartCount`, `TotalMassT`, `TotalCost`, `CrewCapacity` all mapped correctly |
| `GetCurrentCraftAsync_MapsParts` | JSON with two parts → `Parts.Count == 2`, first part fields correct |
| `GetCurrentCraftAsync_MapsEngineModule` | Part with `engine` sub-object → `Parts[0].Engine` not null, thrust/ISP values correct |
| `GetCurrentCraftAsync_MapsResources` | Part with resources → `Parts[0].Resources[0].Name`, `.Amount`, `.MaxAmount` correct |
| `GetCurrentCraftAsync_NullModulesWhenAbsent` | Part JSON with `"engine": null` → `Parts[0].Engine == null` |

## Files Expected to Change

| File | Change |
|------|--------|
| `tests/MunControlProtocol.MCP.Tests/EditorToolsTests.cs` | **New file** |

## Acceptance Criteria

- `dotnet test` passes with all new tests green.
- No existing tests are broken.
- Every test uses `Mock<IKrpcConnection>` — no real kRPC connection.
