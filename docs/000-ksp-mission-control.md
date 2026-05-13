# Mun Control Protocol

**Completed:** 2026-05-11

---

## Original Requirements

A C# mod and companion MCP server that exposes Kerbal Space Program 1 career-save data to AI assistants (Claude Desktop and any other MCP-compatible client), answering questions like "with my current tech, what's the best Mun lander I can build?"

The system is read-only (no game writes), targets KSP 1.12.x career mode, and ships 10 MCP tools covering: career currencies, tech tree, parts catalog (basic and module-specific), science status, vessels, kerbals, buildings, celestial bodies, and difficulty settings.

Three projects in one solution:

| Project | Framework | Purpose |
|---|---|---|
| `MunControlProtocol.Shared` | netstandard2.0 | DTOs only, referenced by both other projects |
| `MunControlProtocol.Career` | net472 | kRPC service extension, ships in `GameData/` |
| `MunControlProtocol.MCP` | net8.0 | Console MCP server registered in Claude Desktop config |

Out of scope for v0.1: write-back to KSP, KSP2, multiplayer, CI/CD, VAB current-ship analysis, public release.

---

## Work Done

### Phase 1 — Scaffolding & repo

Established the three-project solution skeleton with correct target frameworks, a `Directory.Build.props` system externalising per-machine KSP install paths behind a git-ignored file, and `.gitignore`. NuGet packages pinned (ModelContextProtocol 1.3.0, KRPC.Client 0.5.4). The `NU1701` warning on the MCP project (KRPC.Client only ships .NET Framework targets but works over TCP on net8.0) was suppressed. GitHub push was deferred because `gh` CLI was not yet installed on Windows.

### Phase 2 — Vertical slice: `get_career_state`

Wired the first MCP tool end-to-end using only kRPC's built-in `SpaceCenter` service. Established the key patterns followed by every later phase: `KrpcConnection` wrapper (lazy connect, single shared connection, disposal), MCP tool registration via `[McpServerTool]` + XML doc `<summary>`, shared-DTO layout, and xUnit test projects. Smoke test passed against a live career save — Claude Desktop returned correct Funds/Science/Reputation matching the in-game HUD.

Notable corrections from expected API: tool description uses XML `<summary>` (not an attribute property); kRPC SpaceCenter is accessed via `connection.SpaceCenter()` extension method (not `new Service(conn)`); career reputation is `.Reputation` not `.ReputationValue`; Moq requires `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` alongside the test project's `InternalsVisibleTo` to proxy internal interfaces.

### Phase 3 — Career foundation + `get_tech_tree`

Introduced the `MunControlProtocol.Career` kRPC service extension — the highest-risk phase. Established the threading-safe `StateCache` pattern: a volatile reference swap of an immutable snapshot, making the 64-bit reference write atomic with no lock needed. kRPC's serialiser only handles primitives and remote-object types, not plain data records, so `TechTreeService` returns a JSON string built manually via `StringBuilder` (no extra NuGet dependency in the GameData DLL). MCP-side deserialisation uses `JsonStringEnumConverter` + `PropertyNameCaseInsensitive`.

Stub generation via `krpctools` 0.5.4 does **not** require KSP to be running — it reads the deployed DLL directly. `KrpcConnection` uses type aliases (`SpaceCenterService`, `KspMcService`) to resolve ambiguity between two `Service` classes. Smoke test passed: correct tech tree data matching the in-game R&D screen.

### Phase 4 — Parts catalog (basic)

Added `PartsService` and exposed `get_parts_by_category` / `get_part_stats` with basic part metadata (name, title, category, mass dry/wet, cost, tech node required). kRPC rejects two static classes sharing the same `[KRPCService(Name = "...")]` attribute, so `PartsService` is a non-decorated caching helper; its procedures live in `TechTreeService` and delegate to it. Wet-mass calculation uses `PartResource.amount × PartResourceDefinition.density`. Parts with an empty `TechRequired` are excluded; "start" node parts are always included.

### Phase 5 — Module-specific part stats

Extended `PartInfo` with optional typed sub-DTOs populated only when the relevant module is present: engines (thrust, Isp, propellants), antennas (range, type, combinability), tanks/batteries (resource capacities), command pods (crew count, SAS level), solar panels (charge rate, retractable). `ModuleDataTransmitter.antennaType` is an `AntennaType` enum serialised to title-case via a custom helper. No stubs regeneration needed — the kRPC interface was unchanged. `<LangVersion>latest</LangVersion>` and `<Nullable>enable</Nullable>` were added to the Shared csproj to compile nullable sub-DTO properties against the `net472` target.

### Phase 6 — Science

Added `ScienceService` and `get_science_status`. `ResearchAndDevelopment.GetExperimentSubjects()` does not exist in KSP's Assembly-CSharp — the subjects dictionary is internal. Fix: scan all instance fields of `ResearchAndDevelopment` for `Dictionary<string, ScienceSubject>` by reflection. Subject ID parsing (`<expId>@<body><situation><biome>`) resolves body via `FlightGlobals.Bodies` and situation via a longest-first static set to avoid prefix collisions. Two caches: full subjects JSON (filtered on request) and per-body summary JSON. Science refresh cadence is 5 seconds.

### Phase 7 — Buildings, difficulty, built-in passthroughs

Closed out all 10 MCP tools. `BuildingsService` and `DifficultyService` are pure-getter Career services. `get_kerbals` was also implemented as a Career-side kRPC procedure (using `HighLogic.CurrentGame.CrewRoster` and `flightState.protoVessels`) because kRPC 0.5.4's C# client does not expose `SpaceCenter.AstronautComplex` or `CrewMember.ExperienceLevel`/`ExperienceTrait`/`Vessel` — contrary to the original phase prompt. `get_vessels` and `get_body_info` remain kRPC SpaceCenter passthroughs. 47 tests passing (11 Shared + 36 MCP).

### Phase 8 — README + INSTALL + release packaging

Corrected Phase 7 compile errors discovered during packaging: several `CommNetParams`/`DifficultyParams`/`CareerParams` field names were wrong in KSP 1.12 (see Lessons Learned below). Root `README.md` rewritten; `INSTALL.md` written for end users familiar with GameData but not C#/MCP; `deploy/package-release.ps1` produces `MunControlProtocol-vX.Y.Z.zip`. Zip verified. All 47 tests pass. Cross-platform MCP build deferred to v0.2 (Windows-only in v0.1).

---

## Lessons Learned

- **KSP 1.12 Assembly-CSharp field names differ from documentation.** Field names discovered during Phase 8 packaging: `CommNetParams` has `rangeModifier`, `DSNModifier`, `requireSignalForControl`, `occlusionMultiplierVac`, `plasmaBlackout` — it does NOT have `requireSignalForScience` or `enableCommNet`. `EnableCommNet` lives in `GameParameters.DifficultyParams`. `CareerParams` uses `ScienceGainMultiplier` (not `SciGainMultiplier`), `FundsGainMultiplier`, `FundsLossMultiplier`, `RepGainMultiplier`, `RepLossMultiplier`. `DifficultyParams` does NOT expose `CrashTolerance` or `KerbalGToleranceMultiplier`. Always verify field names against the actual Assembly-CSharp before writing Career service code.

- **kRPC rejects duplicate `[KRPCService]` names.** Only one static class per service name is allowed. Pattern established: one `[KRPCService]` entry-point class (e.g. `TechTreeService`) plus plain C# helper classes (e.g. `PartsService`) that the service class delegates to.

- **kRPC serialiser handles primitives and remote objects only — not plain CLR records.** Returning structured data from Career procedures requires serialising to JSON string on the Career side and deserialising on the MCP side. Use `StringBuilder` in the Career DLL (no extra NuGet); use `JsonSerializer` with `PropertyNameCaseInsensitive` and `JsonStringEnumConverter` on the MCP side.

- **Stub regeneration does not require a running KSP instance.** `krpctools` reads the deployed DLL directly. Exact command (after `pip install "krpctools==0.5.4" "setuptools<71"`): call `krpc-clientgen csharp MunControlProtocol <path-to-Career.dll> --ksp <KspInstallDir> -o <stubs-output-path>`. Regenerate any time the Career service surface changes; stubs are committed to source control.

- **`ResearchAndDevelopment.GetExperimentSubjects()` doesn't exist.** The subjects dictionary is internal. Retrieve it by scanning all instance fields of `ResearchAndDevelopment` for `Dictionary<string, ScienceSubject>` by type — the field name varies across KSP patches.

- **`KerbalRoster` indexer takes `int`, not `KerbalType`.** Correct API to enumerate crew by type: `roster.Crew` (IEnumerable) and `roster.Tourist` (IEnumerable). The indexer form used in the original prompt does not compile.

- **kRPC 0.5.4 C# client doesn't expose astronaut or crew APIs.** `SpaceCenter.AstronautComplex`, `CrewMember.ExperienceLevel`, `CrewMember.ExperienceTrait`, and `CrewMember.Vessel` are absent from the client. Implement crew roster tools as Career-side kRPC procedures using `HighLogic.CurrentGame.CrewRoster` and `flightState.protoVessels` — not as SpaceCenter passthroughs.

- **Moq proxying internal interfaces requires two `InternalsVisibleTo` entries.** The test project's `InternalsVisibleTo` alone is insufficient; add `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` to the project under test so Moq's runtime code-gen can proxy internal types.

- **Science subject ID parsing needs longest-first matching for both body names and situation strings** to avoid prefix shadowing (e.g., `InSpaceHigh` must be tried before `InSpaceLow`; body matching via `FlightGlobals.Bodies` sorted longest-first prevents `Kerbin` from shadowing `KerbinSuperMun` if such bodies exist).
