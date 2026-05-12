Read docs/ci-release-pipeline/PHASE_01.md for full context before starting.

You are implementing Phase 1 of the CI/Release Pipeline epic for the KSP Mission Control project.
Work in branch `feature/ci-release-pipeline-phase-1` (create it from `feature/ci-release-pipeline`,
which itself branches from `main`).

## Objective

Make `KSPMissionControl.Career` compile via `dotnet build` without a local KSP installation.
This requires five C# stub projects — one per external assembly reference — committed to `lib/stubs/`.

## Step 0 — verify the API surface

Before writing any stubs, read every `.cs` file in `src/KSPMissionControl.Career/` and confirm
the complete list of KSP/Unity/kRPC types used. PHASE_01.md contains the expected list; treat
discrepancies as authoritative (the source files win).

## Step 1 — create the stub projects

Create the following directory layout:

```
lib/stubs/
  Assembly-CSharp/
    Assembly-CSharp.csproj
    KspTypes.cs
  UnityEngine.CoreModule/
    UnityEngine.CoreModule.csproj
    UnityTypes.cs
  UnityEngine/
    UnityEngine.csproj         ← empty assembly, no .cs files needed
  KRPC.Core/
    KRPC.Core.csproj
    KrpcTypes.cs
  KRPC.SpaceCenter/
    KRPC.SpaceCenter.csproj    ← empty assembly, no .cs files needed
```

### Stub project conventions

All stub projects:
- Target `net472`
- Set `<Nullable>disable</Nullable>` (KSP API has no nullability annotations)
- Set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>` (stubs generate CS1591 etc.)
- Set `<GenerateDocumentationFile>false</GenerateDocumentationFile>`
- Set `<AssemblyName>` to the real DLL name (e.g. `<AssemblyName>Assembly-CSharp</AssemblyName>`)
- Explicitly set `<RootNamespace>` to empty or the correct namespace

### Stub code conventions

- All stub types are `public` with the namespace matching the real assembly
- All method bodies throw `new NotImplementedException()` — stubs are compile-time only
- All properties use `{ get => throw new NotImplementedException(); set { } }` or `=> throw new NotImplementedException();`
- Static classes use `static` modifier; abstract classes use `abstract`
- Attribute classes must inherit from `System.Attribute`
- Enum values must have at least the members that appear in Career source files

### Assembly-CSharp/KspTypes.cs

Define all KSP types listed in PHASE_01.md. Key structural notes:
- `HighLogic` is a `public static class`
- `GameParameters.DifficultyParams`, `GameParameters.CareerParams`, and `GameParameters.CustomParameterNode` are nested public classes inside `GameParameters`
- `CommNet` is a namespace; `CommNetParams` is a class inside it that extends `GameParameters.CustomParameterNode`
- `KSPAddon` is an `[AttributeUsage(AttributeTargets.Class)]` class with a nested enum `Startup`
- `KerbalRoster` — `Crew` and `Tourist` are `IEnumerable<ProtoCrewMember>` properties
- `PartLoader.LoadedPartsList` is a `public static List<AvailablePart>`
- `PartModuleList` and `PartResourceList` implement `IEnumerable<PartModule>` / `IEnumerable<PartResource>` respectively
- `ModuleEngines`, `ModuleDataTransmitter`, `ModuleCommand`, `ModuleSAS`, `ModuleDeployableSolarPanel` all extend `PartModule`
- `ScienceSubject` — note the alias in ScienceService.cs: `using KspScienceSubject = global::ScienceSubject;` so the class is in the global namespace

### UnityEngine.CoreModule/UnityTypes.cs

Define all Unity types listed in PHASE_01.md. Key structural notes:
- Namespace is `UnityEngine` for all types
- `Object` is `public class Object` (not `System.Object`) — it needs `DontDestroyOnLoad(Object target)` as a static method
- `Component` extends `Object` and has a `public GameObject gameObject` property
- `Behaviour` extends `Component`
- `MonoBehaviour` extends `Behaviour`
- `Debug` is a non-static class with all methods `public static`
- `Time` is a non-static class with `public static float realtimeSinceStartup`
- `Mathf` is a non-static class with `public static int RoundToInt(float f)`

### KRPC.Core/KrpcTypes.cs

```
namespace KRPC.Service
{
    public enum GameScene { All, Flight, SpaceCenter, TrackingStation, Editor }
}

namespace KRPC.Service.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class KRPCServiceAttribute : System.Attribute
    {
        public string Name { get; set; }
        public KRPC.Service.GameScene GameScene { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class KRPCProcedureAttribute : System.Attribute { }
}
```

Note: the attributes are used as `[KRPCService]` and `[KRPCProcedure]` (without "Attribute" suffix).
C# attribute resolution strips the suffix automatically, so the class names `KRPCServiceAttribute`
and `KRPCProcedureAttribute` are correct.

## Step 2 — update Career.csproj

Replace the current `<ItemGroup>` containing the five `<Reference>` elements with a conditional
pair of `<ItemGroup>` blocks:

```xml
<!-- Real KSP assemblies (local dev) -->
<ItemGroup Condition="'$(KspInstallDir)' != ''">
  <Reference Include="Assembly-CSharp">
    <HintPath>$(KspInstallDir)\KSP_x64_Data\Managed\Assembly-CSharp.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="UnityEngine">
    <HintPath>$(KspInstallDir)\KSP_x64_Data\Managed\UnityEngine.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="UnityEngine.CoreModule">
    <HintPath>$(KspInstallDir)\KSP_x64_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="KRPC.Core">
    <HintPath>$(KspInstallDir)\GameData\kRPC\KRPC.Core.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="KRPC.SpaceCenter">
    <HintPath>$(KspInstallDir)\GameData\kRPC\KRPC.SpaceCenter.dll</HintPath>
    <Private>false</Private>
  </Reference>
</ItemGroup>

<!-- Stub assemblies (CI / no KSP install) -->
<ItemGroup Condition="'$(KspInstallDir)' == ''">
  <Reference Include="Assembly-CSharp">
    <HintPath>$(MSBuildThisFileDirectory)..\..\lib\stubs\Assembly-CSharp\bin\Release\net472\Assembly-CSharp.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="UnityEngine">
    <HintPath>$(MSBuildThisFileDirectory)..\..\lib\stubs\UnityEngine\bin\Release\net472\UnityEngine.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="UnityEngine.CoreModule">
    <HintPath>$(MSBuildThisFileDirectory)..\..\lib\stubs\UnityEngine.CoreModule\bin\Release\net472\UnityEngine.CoreModule.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="KRPC.Core">
    <HintPath>$(MSBuildThisFileDirectory)..\..\lib\stubs\KRPC.Core\bin\Release\net472\KRPC.Core.dll</HintPath>
    <Private>false</Private>
  </Reference>
  <Reference Include="KRPC.SpaceCenter">
    <HintPath>$(MSBuildThisFileDirectory)..\..\lib\stubs\KRPC.SpaceCenter\bin\Release\net472\KRPC.SpaceCenter.dll</HintPath>
    <Private>false</Private>
  </Reference>
</ItemGroup>
```

The HintPaths point to the stub build output. The stubs must be built before Career can be
built in no-KSP mode — this ordering is handled by the CI workflow (Phase 2).

## Step 3 — add stub projects to solution

Add all five stub projects to `KSPMissionControl.sln`. Place them in a `Stubs` solution folder
so they don't clutter the main project list. Keep `dotnet sln` commands for this.

## Step 4 — build stub outputs locally

After creating all stub projects, build them in Release mode so the HintPath targets exist:

```
dotnet build lib/stubs/Assembly-CSharp/Assembly-CSharp.csproj -c Release
dotnet build lib/stubs/UnityEngine.CoreModule/UnityEngine.CoreModule.csproj -c Release
dotnet build lib/stubs/UnityEngine/UnityEngine.csproj -c Release
dotnet build lib/stubs/KRPC.Core/KRPC.Core.csproj -c Release
dotnet build lib/stubs/KRPC.SpaceCenter/KRPC.SpaceCenter.csproj -c Release
```

## Step 5 — verify

Run the following, with no `KspInstallDir` in the environment:

```
dotnet build src/KSPMissionControl.Career/KSPMissionControl.Career.csproj -c Release
```

The build must exit 0 with no errors. Warnings about unused types in the stubs are acceptable.

If you see CS errors about missing types, add the missing stub types — do not skip them.

## Step 6 — .gitignore

The stub projects produce `bin/` and `obj/` output under `lib/stubs/`. Add entries to `.gitignore`
(if one exists) or create it, covering:

```
lib/stubs/**/bin/
lib/stubs/**/obj/
```

The stub source files (`.csproj`, `.cs`) ARE committed. The build output is not.

## Existing conventions to follow

- `Directory.Build.props` applies `LangVersion=latest` and `TreatWarningsAsErrors=true` globally.
  The stub projects must override `TreatWarningsAsErrors` to `false` locally (inside their own `.csproj`)
  to avoid CS1591/CS0649 noise from stub bodies.
- `<Nullable>` is disabled for `net472` targets globally (see `Directory.Build.props`) — stubs are fine.

## Completion

When the phase is done:
1. Open `docs/ci-release-pipeline/PROGRESS.md`
2. Set Phase 01 Status to `complete`, fill in the completed date
3. Commit all changes to `feature/ci-release-pipeline-phase-1`
4. Open a PR targeting `feature/ci-release-pipeline`
