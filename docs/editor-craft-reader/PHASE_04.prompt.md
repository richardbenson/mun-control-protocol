Read `docs/editor-craft-reader/PHASE_04.md` for full context before starting.

## Task

You are implementing Phase 4 of the Editor Craft Reader feature: unit tests for `EditorTools`. Phase 3 must already be merged into `feature/editor-craft-reader`.

Work in branch `feature/editor-craft-reader-phase-4` (branch off `feature/editor-craft-reader`).

---

## Style Reference

Read `tests/MunControlProtocol.MCP.Tests/VesselsToolsTests.cs` before writing any code. Follow its conventions exactly:
- `public sealed class EditorToolsTests`
- `private const string` JSON constants at the top
- `Mock<IKrpcConnection>` via Moq
- `[Fact]` per test, one assertion concept per test
- Async tests: `public async Task TestName()`

---

## File to Create: `tests/MunControlProtocol.MCP.Tests/EditorToolsTests.cs`

Define the following JSON constants at the top of the class:

```csharp
private const string EnginePartJson = """
    {
      "name": "liquidEngine",
      "title": "LV-T45 Swivel",
      "massT": 1.5,
      "resourceMassT": 0.0,
      "cost": 1200.0,
      "stageIndex": 1,
      "resources": [],
      "engine": {
        "thrustVacuum": 215.0,
        "thrustAsl": 168.0,
        "ispVacuum": 320.0,
        "ispAsl": 270.0,
        "fuelFlowVacuum": 0.068,
        "propellants": ["LiquidFuel", "Oxidizer"]
      },
      "antenna": null,
      "tank": null,
      "command": null,
      "solarPanel": null
    }
    """;

private const string PodPartJson = """
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
      "antenna": null,
      "tank": null,
      "command": {"crewCapacity": 1, "hasSas": true, "sasLevel": 3, "hibernationCharge": 0},
      "solarPanel": null
    }
    """;

private const string CraftJson = $$"""
    {
      "name": "Mun Mission I",
      "editorType": "VAB",
      "partCount": 2,
      "totalMassT": 2.396,
      "totalCost": 1800.0,
      "crewCapacity": 1,
      "parts": [{{EnginePartJson}}, {{PodPartJson}}]
    }
    """;
```

Implement the seven tests listed in `PHASE_04.md`. Key assertions:

- `GetCurrentCraftAsync_MapsTopLevelFields`: assert `Name == "Mun Mission I"`, `EditorType == "VAB"`, `PartCount == 2`, `TotalMassT == 2.396`, `TotalCost == 1800.0`, `CrewCapacity == 1`.
- `GetCurrentCraftAsync_MapsParts`: assert `result!.Parts.Count == 2`, `result.Parts[0].Name == "liquidEngine"`, `result.Parts[1].Name == "mk1pod"`.
- `GetCurrentCraftAsync_MapsEngineModule`: use a JSON with just `EnginePartJson` in parts, assert `Parts[0].Engine != null`, `Parts[0].Engine!.ThrustVacuum == 215.0`, `Parts[0].Engine.IspVacuum == 320.0`.
- `GetCurrentCraftAsync_MapsResources`: use a JSON wrapping just `PodPartJson`, assert `Parts[0].Resources.Count == 2`, `Parts[0].Resources[0].Name == "ElectricCharge"`, `Parts[0].Resources[0].Amount == 50.0`, `Parts[0].Resources[0].MaxAmount == 50.0`.
- `GetCurrentCraftAsync_NullModulesWhenAbsent`: from `PodPartJson` assert `Parts[0].Engine == null` and `Parts[0].Tank == null`.

For tests that need a single-part craft, wrap the individual part constant in a minimal craft JSON wrapper inline (no need for additional constants).

---

## Build + Test Verification

```
dotnet test tests/MunControlProtocol.MCP.Tests
```

All tests (new and existing) must pass.

---

## Completion

Update `docs/editor-craft-reader/PROGRESS.md`: set Phase 4 status to `complete`, fill in the completed date.

Open a PR from `feature/editor-craft-reader-phase-4` targeting `feature/editor-craft-reader`.
