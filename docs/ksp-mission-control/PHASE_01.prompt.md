Read `docs/ksp-mission-control/PHASE_01.md` and `docs/ksp-mission-control/README.md` first for full context.

You are implementing Phase 1 of the KSP Mission Control project: scaffolding the C# solution and setting up the repo. No application logic is to be written in this phase — only project files, build configuration, and git/GitHub setup.

## Branch
- Cut `feature/ksp-mission-control` from `main` (if it does not already exist).
- Cut `feature/ksp-mission-control-phase-01` from `feature/ksp-mission-control`.
- All work in this phase lands on `feature/ksp-mission-control-phase-01`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Repo init
- `git init` if no repo exists.
- Create `.gitignore` covering: Visual Studio (`bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`), Rider (`.idea/`), and project-specific entries: `Directory.Build.props.user`, `deploy/GameData/KSPMissionControl/*.dll` (we don't ship build outputs in source).
- Create a one-paragraph root `README.md` that points to `docs/ksp-mission-control/README.md` for the implementation plan.

### 2. Solution + projects
Create `KSPMissionControl.sln` at the repo root with three projects under `src/`:

#### `src/KSPMissionControl.Shared/KSPMissionControl.Shared.csproj`
- `<TargetFramework>netstandard2.0</TargetFramework>`
- No package or project references.
- No source files yet (an empty project is fine).

#### `src/KSPMissionControl.Career/KSPMissionControl.Career.csproj`
- `<TargetFramework>net472</TargetFramework>`
- ProjectReference → `KSPMissionControl.Shared`
- Three local DLL references using the `$(KspInstallDir)` MSBuild property (defined in `Directory.Build.props.user`):
  - `$(KspInstallDir)\KSP_x64_Data\Managed\Assembly-CSharp.dll`
  - `$(KspInstallDir)\KSP_x64_Data\Managed\UnityEngine.dll`
  - `$(KspInstallDir)\GameData\kRPC\KRPC.SpaceCenter.dll`
- All three local references must have `<Private>false</Private>` (do not copy to output — KSP supplies them at runtime).
- No source files yet.

#### `src/KSPMissionControl.MCP/KSPMissionControl.MCP.csproj`
- `<TargetFramework>net8.0</TargetFramework>`
- `<OutputType>Exe</OutputType>`
- ProjectReference → `KSPMissionControl.Shared`
- PackageReference → `ModelContextProtocol` (use the latest 1.x stable version available on NuGet; pin the exact version)
- PackageReference → `KRPC.Client` (latest stable from NuGet; pin the exact version)
- One source file: `Program.cs` containing only `Console.WriteLine("KSPMissionControl.MCP — Phase 1 placeholder");` inside `Main`.

### 3. Per-machine config
Create `Directory.Build.props` at the repo root with:
- `<LangVersion>latest</LangVersion>`
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- `<Nullable>enable</Nullable>` (only for projects that support it — gate with a condition so it doesn't break `net472`)
- An `<Import>` of `Directory.Build.props.user` with `Condition="Exists('$(MSBuildThisFileDirectory)Directory.Build.props.user')"`.

Create `Directory.Build.props.template` (committed) with a single `<KspInstallDir>` property and a comment instructing the developer to copy this file to `Directory.Build.props.user` and edit the path. Default value should be a clearly-fake path like `C:\Path\To\Kerbal Space Program`.

The real `Directory.Build.props.user` is in `.gitignore` (see step 1).

### 4. Verify build
- Create your own `Directory.Build.props.user` pointing at the local KSP install. Do **not** commit it.
- Run `dotnet build KSPMissionControl.sln` and confirm zero errors, zero warnings.
- Confirm `bin/Debug/net8.0/KSPMissionControl.MCP.dll` and `bin/Debug/net472/KSPMissionControl.Career.dll` exist.

### 5. GitHub
- Create a **private** GitHub repo (the user has already confirmed private).
- Push `main` and `feature/ksp-mission-control` to the remote.
- Push `feature/ksp-mission-control-phase-01` and open the PR into `feature/ksp-mission-control`.

### 6. Update PROGRESS
Edit `docs/ksp-mission-control/PROGRESS.md`:
- Phase 1 status → `complete`
- Fill in `Started` and `Completed` dates (today's date in YYYY-MM-DD).
- Notes: record the GitHub repo URL and the exact NuGet package versions you pinned for `ModelContextProtocol` and `KRPC.Client` — Phase 2 will need them.

## Edge cases
- If `dotnet` reports it can't find `Assembly-CSharp.dll`, the developer hasn't created `Directory.Build.props.user` from the template — the error message in the template comment must call this out explicitly.
- If `Nullable=enable` blows up on `net472`, gate it with `Condition="'$(TargetFramework)' != 'net472'"`.
- The `KRPC.Client` NuGet package may require additional transitive dependencies on `net8.0`. Let `dotnet restore` resolve them; do not pin transitives manually.

## Definition of done for this phase
All five acceptance criteria in `PHASE_01.md` are met. PR is open against `feature/ksp-mission-control` with a description summarising what's wired up and noting that no application code was written.
