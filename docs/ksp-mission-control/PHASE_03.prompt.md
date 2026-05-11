Read `docs/ksp-mission-control/PHASE_03.md`, `docs/ksp-mission-control/README.md`, and `docs/ksp-mission-control/PROGRESS.md` (Phase 2 notes) first. Also read the requirements doc at `.project-cache/.../docs/KSPMissionControl-Requirements.md` (or wherever the user's requirements doc lives in this repo) — sections "Build Notes / Threading Constraint" and "kRPC Client Stub Generation" are critical.

You are implementing Phase 3: standing up the Career kRPC service extension and shipping the first real service (`TechTreeService`). This phase establishes patterns that all subsequent Career-side phases (4, 5, 6, 7) will follow. Code carefully.

## Branch
- Cut `feature/ksp-mission-control-phase-03` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Reusable threading-safe cache
Create `src/KSPMissionControl.Career/Internal/StateCache.cs`. A small generic class:

```
StateCache<T> where T : class
- T? Snapshot { get; }              // serve from here on the kRPC thread
- void Update(T newSnapshot)        // call from the Unity main thread
- thread-safe via volatile reference swap or lock — pick one and document why
```

This is the **only** concurrency primitive Career services use. Every later service holds one or more `StateCache<T>` fields, populates them in `MonoBehaviour.Update()` or scene-change callbacks, and reads from them in kRPC procedure bodies.

Do not over-engineer this. A volatile reference swap is sufficient because the snapshot is immutable once published.

### 2. Addon entry point
Create `src/KSPMissionControl.Career/KSPMissionControlAddon.cs`:
- `[KSPAddon(KSPAddon.Startup.Instantly, true)]` — register on every scene; we only do work in scenes where R&D state exists.
- Inherits `MonoBehaviour`.
- On `Awake()`: register the kRPC service. The kRPC API for service registration in C# extensions is documented in kRPC's source — find it (search `KRPC.Service.Scanner` and `KRPCService` attribute usage in any kRPC C# extension example). The `[KRPCService]` attribute on a class plus `[KRPCProcedure]` on its public methods is the typical shape.
- On `Update()`: only when `HighLogic.LoadedScene` is one where the R&D state is initialised (Space Center, R&D, Tracking Station, Editor, Flight). Refresh the `TechTreeService`'s cache no more than once per second (track `Time.realtimeSinceStartup`). Tech tree state doesn't change every frame — once per second is fine and keeps overhead negligible.
- On `OnDestroy()`: clean up.

The addon is the **only** place that touches the Unity lifecycle. Services have no Unity dependency; they take a `StateCache<T>` from the addon.

### 3. The service
Create `src/KSPMissionControl.Career/Services/TechTreeService.cs`:
- Decorated with `[KRPCService(GameScene = GameScene.All)]` (or whichever scope kRPC's API expects).
- Holds a `StateCache<List<TechNode>>` populated by the addon.
- One `[KRPCProcedure]` method `GetTechTree()` returning `IList<TechNode>` (or `string` JSON if kRPC's serialiser doesn't handle the DTO well — investigate first).
- The procedure body must **only** read from the cache. Zero KSP API calls inside the procedure.

The cache-population logic (called from the addon, on the Unity thread) builds a `List<TechNode>` from:
- `ResearchAndDevelopment.GetTechnologyState(nodeId)` for status
- `ResearchAndDevelopment.GetTechTreeNodes()` (or the equivalent — verify the exact API; the `ResearchAndDevelopment` class is in `Assembly-CSharp.dll`) for the list of nodes and their `parts` collections
- For each node, populate: id, title, science cost, status (`Unlocked` / `Available` / `Locked` enum), part names (just names — full part data comes in Phase 4)

### 4. Shared DTO
Create `src/KSPMissionControl.Shared/Models/TechNode.cs`:
- Public sealed class.
- Properties: `Id` (string), `Title` (string), `ScienceCost` (int), `Status` (enum: `Locked`, `Available`, `Unlocked`), `PartNames` (`IReadOnlyList<string>`).
- The `Status` enum lives in the same file or alongside in `Models/`.
- Make all properties get/set (kRPC serialiser may need setters).

### 5. Deploy script
Create `deploy/build-and-deploy.ps1`:
- Takes `$KspInstallDir` from env or first argument.
- Runs `dotnet build src/KSPMissionControl.Career/KSPMissionControl.Career.csproj -c Release`.
- Creates `$KspInstallDir/GameData/KSPMissionControl/` if missing.
- Copies the built `.dll` (and any of its non-KSP/non-Unity dependencies — the `Shared` DLL must come along) into that folder.
- Echoes the destination path.

This script is the developer's loop for every Career-side change going forward.

### 6. Stub generation
With `KSPMissionControl.Career.dll` deployed to `GameData/KSPMissionControl/`, run `krpc-clientgen` offline (KSP does **not** need to be running):
```
python -c "
import sys
sys.argv = [
    'krpc-clientgen', 'csharp', 'KSPMissionControl',
    r'<KspInstallDir>/GameData/KSPMissionControl/KSPMissionControl.Career.dll',
    '--ksp', r'<KspInstallDir>',
    '-o', r'src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs'
]
from krpctools.clientgen import main; main()
" | python
```
Requires: `pip install krpctools==0.5.4 setuptools` (one-time setup; `setuptools<71` needed for `pkg_resources`).
- Place output at `src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs`.
- Commit it. Do not edit by hand. If the file needs to change, regenerate.

### 7. MCP tool
Extend `src/KSPMissionControl.MCP/Tools/CareerTools.cs` (or create a new `TechTreeTools.cs` if `CareerTools` is getting cluttered — your call) with `GetTechTreeAsync()`:
- Calls into the generated stub for `KSPMissionControl.TechTreeService.GetTechTree()`.
- Maps the kRPC-returned shape into the `TechNode` DTO from `Shared` (the generated stubs may produce their own type for the kRPC service's return; bridge it to the `Shared` DTO so the MCP boundary stays consistent).
- Decorate as MCP tool `get_tech_tree` with description "Returns all tech tree nodes with their unlock status and the parts contained in each node."

### 8. Tests
- `tests/KSPMissionControl.Shared.Tests/`: add a `TechNodeTests.cs` with a JSON round-trip test.
- `tests/KSPMissionControl.MCP.Tests/`: add a `TechTreeToolsTests.cs` mocking the generated stub (interface-extract or wrap as needed) to confirm the mapping logic.

### 9. Manual smoke test
- Run `deploy/build-and-deploy.ps1`.
- Launch KSP, load a career save.
- Open the R&D building, note which nodes are unlocked vs available vs locked.
- In Claude Desktop, ask "list my tech tree nodes and which ones I've unlocked".
- Spot-check a handful of nodes against the in-game R&D screen.

### 10. Update PROGRESS
Set Phase 3 status to `complete`, dates filled. **Notes** must record:
- Exact `krpc-clientgen` command used (full args).
- Any quirks in the generated stub shape worth knowing for Phase 4+ (e.g. "kRPC wraps `IList<TechNode>` as a custom collection type — wrap with `.ToList()` at the MCP boundary").
- The chosen StateCache concurrency primitive (volatile vs lock) and one-line justification.

## Code patterns established here (every later Career phase MUST follow)
- One service class per `Services/` file, decorated with `[KRPCService]`.
- Each service holds `StateCache<T>` field(s) populated externally.
- Services contain **zero** KSP API calls. All KSP access lives in the addon's Unity-thread refresh logic.
- Generated stubs are committed under `src/KSPMissionControl.MCP/Krpc/`. Regenerate, do not edit.
- Each new MCP tool gets one mocked unit test for mapping logic.
- Every Career-side phase ends with a manual smoke test run.

## Edge cases
- **Loading scene before R&D state exists**: `ResearchAndDevelopment.Instance` is null in the main menu. Guard the cache-refresh logic with a null check; serve a stale snapshot (or empty list) if R&D isn't loaded yet.
- **Sandbox / Science save**: tech tree exists in Science mode but funds don't; in Sandbox there's no progression. Don't error — just return what's there. The MCP tool description doesn't promise career-only.
- **kRPC service registration failure**: log loudly to KSP's `Player.log` via `Debug.LogError(...)` so the user sees it in `~/AppData/LocalLow/Squad/Kerbal Space Program/Player.log`.
- **Mod-added parts in nodes**: should appear automatically via the same `node.parts` enumeration. Don't filter them out.

## Definition of done for this phase
All acceptance criteria in `PHASE_03.md` are met. PR description includes a screenshot or quoted output of the smoke test (Claude's response next to a description of the in-game R&D state).
