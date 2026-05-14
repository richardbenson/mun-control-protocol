Read `docs/editor-craft-reader/PHASE_01.md` for full context before starting.

## Task

You are implementing Phase 1 of the Editor Craft Reader feature. This phase adds compile-time KSP stubs and shared model types only — no logic.

Work in branch `feature/editor-craft-reader-phase-1` (branch off `feature/editor-craft-reader`, which itself branches off `feature/ksp-mission-control-phase-01`).

---

## 1. KSP Stubs — `lib/stubs/Assembly-CSharp/KspTypes.cs`

Read the existing file first. Then make the following additions:

**Add to the existing `Part` class** (after `public PartModuleList Modules`):
```
public AvailablePart partInfo;
public int inverseStage;
```

**Add after the `Part` class**:
```csharp
public enum EditorFacility { None = -1, VAB = 0, SPH = 1 }

public class ShipConstruct
{
    public string shipName;
    public EditorFacility shipFacility;
    public List<Part> parts => throw new NotImplementedException();
}

public static class EditorLogic
{
    public static EditorLogic fetch => throw new NotImplementedException();
    public ShipConstruct ship => throw new NotImplementedException();
}
```

`EditorLogic.fetch` returning itself (static returning instance) matches the KSP 1.12 API — `fetch` is a static property on the class.

---

## 2. New Shared Model — `src/MunControlProtocol.Shared/Models/CraftDesign.cs`

Create a new file. Follow the style of `src/MunControlProtocol.Shared/Models/PartInfo.cs` (no comments, `sealed` classes, nullable module sub-objects).

The file should contain three public sealed classes: `CraftDesign`, `CraftPart`, and `CraftResource`.

**`CraftDesign`**:
- `string Name` — craft name
- `string EditorType` — "VAB" or "SPH"
- `int PartCount`
- `double TotalMassT` — total wet mass in tonnes
- `double TotalCost`
- `int CrewCapacity`
- `IList<CraftPart> Parts` — one entry per placed part

**`CraftPart`**:
- `string Name` — internal part name (e.g. `"mk1pod"`)
- `string Title` — display name (e.g. `"Mk1 Command Pod"`)
- `double MassT` — dry mass in tonnes
- `double ResourceMassT` — mass of resources currently loaded (wet − dry)
- `double Cost`
- `int StageIndex` — `Part.inverseStage` from KSP; 0 = last stage
- `IList<CraftResource> Resources`
- Nullable module sub-objects (use the existing types from the `PartModules` namespace):
  - `EngineInfo? Engine`
  - `AntennaInfo? Antenna`
  - `TankInfo? Tank`
  - `CommandInfo? Command`
  - `SolarPanelInfo? SolarPanel`

**`CraftResource`**:
- `string Name`
- `double Amount` — current fill level
- `double MaxAmount`

Add the `using MunControlProtocol.Shared.Models.PartModules;` import at the top. Use `IList<T>` initialised to `new List<T>()` for collection properties, matching `PartInfo.cs` conventions.

---

## Build Verification

Run:
```
dotnet build lib/stubs/Assembly-CSharp
dotnet build src/MunControlProtocol.Shared
```

Both must succeed with zero errors.

---

## Completion

Update `docs/editor-craft-reader/PROGRESS.md`: set Phase 1 status to `complete`, fill in the completed date.

Open a PR from `feature/editor-craft-reader-phase-1` targeting `feature/editor-craft-reader`.
