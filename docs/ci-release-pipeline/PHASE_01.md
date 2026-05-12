# Phase 01 — KSP Assembly Stubs

## Goal

`KSPMissionControl.Career` must compile with `dotnet build` on a machine that has no KSP
installation — no `KspInstallDir` environment variable, no `Directory.Build.props.user`.
This unblocks every CI step that follows.

## Background

`Career.csproj` references five external assemblies via `$(KspInstallDir)` HintPaths:

| Reference name | Source in KSP install |
|---|---|
| `Assembly-CSharp` | `KSP_x64_Data/Managed/Assembly-CSharp.dll` |
| `UnityEngine` | `KSP_x64_Data/Managed/UnityEngine.dll` |
| `UnityEngine.CoreModule` | `KSP_x64_Data/Managed/UnityEngine.CoreModule.dll` |
| `KRPC.Core` | `GameData/kRPC/KRPC.Core.dll` |
| `KRPC.SpaceCenter` | `GameData/kRPC/KRPC.SpaceCenter.dll` |

When `KspInstallDir` is empty these HintPaths resolve to nothing and the build fails.

## Solution

Five C# stub projects, one per assembly, committed to `lib/stubs/`. Each project sets
`<AssemblyName>` to the real DLL name (e.g. `Assembly-CSharp`) so that the resulting
stub DLL has the correct assembly identity. `Career.csproj` gains a conditional reference
block: real DLLs when `$(KspInstallDir)` is set, stubs otherwise.

At KSP runtime the real installed assemblies (same names) take precedence — the stub
references in `Career.dll` resolve against the real game assemblies transparently.

## KSP API surface used by Career

Derived from reading all six Career source files. The stub must cover:

### Assembly-CSharp stubs (namespace as-is in KSP)

**HighLogic** (static class)
- `static Game CurrentGame`
- `static GameScenes LoadedScene`

**Game** (class)
- `GameParameters Parameters`
- `FlightState flightState`
- `KerbalRoster CrewRoster`

**GameScenes** (enum) — `SPACECENTER, EDITOR, FLIGHT, TRACKSTATION`

**GameParameters** (class)
- `T CustomParams<T>() where T : GameParameters.CustomParameterNode`
- `GameParameters.DifficultyParams Difficulty`
- `GameParameters.CareerParams Career`

**GameParameters.CustomParameterNode** (class) — base for CommNetParams

**GameParameters.DifficultyParams** (class)
- `bool EnableCommNet`
- `double ReentryHeatScale`
- `bool MissingCrewsRespawn`

**GameParameters.CareerParams** (class)
- `double ScienceGainMultiplier`
- `double FundsGainMultiplier`
- `double RepGainMultiplier`
- `double FundsLossMultiplier`
- `double RepLossMultiplier`

**CommNet.CommNetParams** (class, extends GameParameters.CustomParameterNode)
- `double rangeModifier`
- `double DSNModifier`
- `bool requireSignalForControl`
- `float occlusionMultiplierVac`
- `bool plasmaBlackout`

**FlightState** (class) — `List<ProtoVessel> protoVessels`

**ProtoVessel** (class)
- `string vesselName`
- `List<ProtoCrewMember> GetVesselCrew()`

**ProtoCrewMember** (class)
- `string name`
- `int experienceLevel`
- `string trait`
- `ProtoCrewMember.RosterStatus rosterStatus`
- Nested enum **RosterStatus**: `Available, Assigned, Dead, Missing`

**KerbalRoster** (class)
- `IEnumerable<ProtoCrewMember> Crew`
- `IEnumerable<ProtoCrewMember> Tourist`

**KSPAddon** (Attribute class)
- `KSPAddon(KSPAddon.Startup startup, bool once)`
- Nested enum **Startup**: `Instantly` (and others as `= 0`)

**ScenarioUpgradeableFacilities** (static class)
- `static float GetFacilityLevel(string facilityName)`
- `static int GetFacilityLevelCount(string facilityName)`

**ResearchAndDevelopment** (class)
- `static ResearchAndDevelopment Instance`
- `static string GetTechnologyTitle(string techId)`
- `static RDTech.State GetTechnologyState(string techId)`
- `ProtoTechNode GetTechState(string techId)`
- `static bool PartModelPurchased(AvailablePart ap)`

**ProtoTechNode** (class) — `int scienceCost`

**RDTech** (class) — nested enum **State**: `Available`

**GameDatabase** (class)
- `static GameDatabase Instance`
- `ConfigNode[] GetConfigNodes(string typeName)`

**ConfigNode** (class)
- `string GetValue(string name)`
- `ConfigNode[] GetNodes(string name)`

**PartLoader** (static class)
- `static List<AvailablePart> LoadedPartsList`

**AvailablePart** (class)
- `string name`
- `string title`
- `string TechRequired`
- `PartCategories category`
- `float cost`
- `Part partPrefab`

**PartCategories** (enum) — members don't matter, just needs `.ToString()`

**Part** (class)
- `float mass`
- `int CrewCapacity`
- `PartResourceList Resources`
- `PartModuleList Modules`

**PartResourceList** (class, IEnumerable<PartResource>) — `int Count`

**PartModuleList** (class, IEnumerable<PartModule>)

**PartModule** (class) — base only

**PartResource** (class)
- `double amount`
- `double maxAmount`
- `string resourceName`
- `PartResourceDefinition info`

**PartResourceDefinition** (class) — `double density`

**ModuleEngines** (class extends PartModule)
- `FloatCurve atmosphereCurve`
- `double maxThrust`
- `float maxFuelFlow`
- `List<Propellant> propellants`

**FloatCurve** (class) — `float Evaluate(float t)`

**Propellant** (class) — `string name`

**ModuleDataTransmitter** (class extends PartModule)
- `double antennaPower`
- `AntennaType antennaType`
- `bool antennaCombinable`
- `float packetSize`
- `float packetInterval`

**AntennaType** (enum) — members don't matter, needs `.ToString()`

**ModuleCommand** (class extends PartModule) — type identity only

**ModuleSAS** (class extends PartModule) — `int SASServiceLevel`

**ModuleDeployableSolarPanel** (class extends PartModule)
- `float chargeRate`
- `bool retractable`

**ScienceSubject** (class)
- `double scienceCap`
- `double science`
- `double subjectValue`
- `double scientificValue`
- `string title`
- `string id`

**FlightGlobals** (static class) — `static List<CelestialBody> Bodies`

**CelestialBody** (class) — `string name`

### UnityEngine.CoreModule stubs

**Object** (class, namespace UnityEngine) — `static void DontDestroyOnLoad(Object target)`

**Component** (class, extends Object) — `GameObject gameObject`

**Behaviour** (class, extends Component) — empty

**MonoBehaviour** (class, extends Behaviour) — empty

**GameObject** (class, extends Object) — empty

**Debug** (static class, namespace UnityEngine)
- `static void Log(object message)`
- `static void LogWarning(object message)`
- `static void LogError(object message)`

**Time** (static class, namespace UnityEngine) — `static float realtimeSinceStartup`

**Mathf** (static class, namespace UnityEngine) — `static int RoundToInt(float f)`

### UnityEngine stubs

Empty assembly — just needs the correct assembly identity so the reference resolves. All
used Unity types live in `UnityEngine.CoreModule`.

### KRPC.Core stubs (namespaces KRPC.Service and KRPC.Service.Attributes)

**GameScene** (enum, namespace KRPC.Service) — `All`

**KRPCServiceAttribute** (class, namespace KRPC.Service.Attributes, alias `[KRPCService]`)
- `string Name`
- `GameScene GameScene`

**KRPCProcedureAttribute** (class, namespace KRPC.Service.Attributes, alias `[KRPCProcedure]`)

### KRPC.SpaceCenter stubs

Empty assembly — referenced by Career.csproj but no types from it appear in Career
source code. Needed so the reference doesn't cause a build warning/error.

## Files expected to change

| File | Change |
|---|---|
| `lib/stubs/Assembly-CSharp/Assembly-CSharp.csproj` | new |
| `lib/stubs/Assembly-CSharp/KspTypes.cs` | new |
| `lib/stubs/UnityEngine.CoreModule/UnityEngine.CoreModule.csproj` | new |
| `lib/stubs/UnityEngine.CoreModule/UnityTypes.cs` | new |
| `lib/stubs/UnityEngine/UnityEngine.csproj` | new |
| `lib/stubs/KRPC.Core/KRPC.Core.csproj` | new |
| `lib/stubs/KRPC.Core/KrpcTypes.cs` | new |
| `lib/stubs/KRPC.SpaceCenter/KRPC.SpaceCenter.csproj` | new |
| `src/KSPMissionControl.Career/KSPMissionControl.Career.csproj` | add conditional stub references |
| `KSPMissionControl.sln` | add 5 stub projects (BuildInSolution=false guard or separate `Stubs` folder) |

## Acceptance criteria

1. `dotnet build src/KSPMissionControl.Career/KSPMissionControl.Career.csproj -c Release` exits 0 on a machine with no `KspInstallDir` set
2. On a machine with `KspInstallDir` set (local dev), the same command still exits 0 using the real DLLs
3. `dotnet build` at solution root exits 0 (stubs compile)
4. No stub DLL is copied to the Career bin output (`<Private>false</Private>` on all stub references)
