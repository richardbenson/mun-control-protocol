# PROGRESS

Live status of the KSP Mission Control implementation. Each phase entry is updated by the agent that completes the phase.

---

## Phase 1 — Scaffolding & repo
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-01`
- **Dependencies**: none
- **Started**: 2026-05-10
- **Completed**: 2026-05-10
- **Notes**: NuGet packages pinned — ModelContextProtocol 1.3.0, KRPC.Client 0.5.4 (NU1701 suppressed on MCP project — package only ships .NET Framework targets but works over TCP on net8.0). GitHub push + PR deferred: gh CLI not yet installed on Windows; install it before Phase 2 and push main, feature/ksp-mission-control, and feature/ksp-mission-control-phase-01, then open PR into feature/ksp-mission-control.

## Phase 2 — Vertical slice: `get_career_state`
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-02`
- **Dependencies**: Phase 1
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: MCP server bootstrap uses `Host.CreateApplicationBuilder(args)` + `services.AddMcpServer().WithStdioServerTransport().WithTools<CareerTools>()`. Tool methods decorated with `[McpServerTool(Name = "...")]`; description comes from XML doc `<summary>` (the attribute has no `Description` property). kRPC SpaceCenter service is `connection.SpaceCenter()` extension method (not `new Service(conn)`). Career property is `.Reputation` (not `.ReputationValue`). Moq requires `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` alongside the test project's `InternalsVisibleTo` to proxy internal interfaces. Smoke test passed 2026-05-11: Claude Desktop returned Funds √1,800,773 / Science 1.8 / Reputation 361.8, matching the in-game HUD.

## Phase 3 — Career foundation + `get_tech_tree`
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-03`
- **Dependencies**: Phase 2
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: StateCache uses volatile reference swap (immutable snapshot, 64-bit reference write is atomic — no lock needed). TechTreeService returns JSON string from kRPC (not IList<TechNode>) because kRPC's serialiser only handles primitives and remote-object types, not plain data records. Career side builds JSON manually via StringBuilder — no extra NuGet dependency in the GameData DLL. Stub regen does **not** require KSP to be running — `krpctools` reads the deployed DLL directly. One-time setup: `pip install "krpctools==0.5.4" "setuptools<71"`. Regen command: `python -c "import sys; sys.argv=['krpc-clientgen','csharp','KSPMissionControl',r'<KspInstallDir>/GameData/KSPMissionControl/KSPMissionControl.Career.dll','--ksp',r'<KspInstallDir>','-o','src/KSPMissionControl.MCP/Krpc/KSPMissionControlStubs.cs']; from krpctools.clientgen import main; main()"`. KrpcConnection uses type aliases (`SpaceCenterService`, `KspMcService`) to resolve ambiguity between the two `Service` classes. MCP deserialization uses `JsonStringEnumConverter` + `PropertyNameCaseInsensitive` since Career produces camelCase JSON keys. Smoke test passed 2026-05-11: Claude Desktop returned correct tech tree data matching the in-game R&D screen.

## Phase 4 — Parts catalog (basic)
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-04`
- **Dependencies**: Phase 3
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: kRPC does not support two static classes with the same `[KRPCService(Name = "KSPMissionControl")]` — the scanner rejects duplicate service ids. Fix: `PartsService.cs` is a caching helper only (no `[KRPCService]` attribute); the two new `[KRPCProcedure]` methods (`GetPartsByCategory`, `GetPartByName`) live in `TechTreeService.cs`, the single `[KRPCService]` class, delegating to `PartsService`. Wet-mass calculation: `PartResource.amount × PartResourceDefinition.density` (tonnes/unit) — some parts define resources but launch full (e.g. monoprop pods), so wet > dry as expected; parts with no resources have massWet == massDry. Parts with `TechRequired = ""` (null/empty) are excluded — they have no tech node. "start" parts are always included (they are always `RDTech.State.Available`). Stubs regenerated from deployed DLL via krpctools 0.5.4 (KSP not running). Smoke test pending — requires loading career save.

## Phase 5 — Module-specific part stats
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-05`
- **Dependencies**: Phase 4
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: Shared project needed `<LangVersion>latest</LangVersion>` and `<Nullable>enable</Nullable>` to compile `PartInfo`'s nullable sub-DTO properties against the `net472` target — added to csproj. Stubs not regenerated: no new kRPC procedures added; `GetPartsByCategory` and `GetPartByName` return richer JSON but the kRPC interface is unchanged. `ModuleDataTransmitter.antennaType` is an `AntennaType` enum (values `INTERNAL`/`DIRECT`/`RELAY`) — serialised to title-case via a `ToTitleCase` helper. `HibernationCharge` is always 0: the EC consumption rate lives in `ModuleCommand.resHandler.inputResources` which requires non-trivial KSP reflection and was deferred as out-of-scope for v1. `ModuleSAS.SASServiceLevel` (int 0–3) confirmed correct field name in KSP 1.12. `FindObjectEnd` in `PartsService` correctly handles nested sub-DTO objects since it tracks `{`/`}` depth. Smoke tests pending — require KSP with career save loaded.

## Phase 6 — Science
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-06`
- **Dependencies**: Phase 3 (Career foundation pattern)
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: `ResearchAndDevelopment.GetExperimentSubjects()` does not exist in KSP's Assembly-CSharp — the subjects dictionary is internal. Fix: scan all instance fields of `ResearchAndDevelopment` for `Dictionary<string, ScienceSubject>` by type (field name varies across KSP patches). ID parsing: `<expId>@<body><situation><biome>` — body matched via `FlightGlobals.Bodies` (longest-first to avoid prefix shadowing); situations from a static closed set ordered longest-first (`InSpaceHigh`, `InSpaceLow`, `FlyingHigh`, `FlyingLow`, `Splashed`, `Landed`). Example edge: `crewReport@KerbinFlyingHighlands` → expId=`crewReport`, body=`Kerbin`, situation=`FlyingHigh`, biome=`lands` (note: biome remainder after consuming situation prefix; "Highlands" becomes "lands" after "FlyingHigh" is consumed — this is the expected raw form from KSP subject IDs and does not affect filtering since filters are applied to the stored body/situation fields). Two caches: subjects JSON (full, filtered on request) and per-body summary JSON. Science refresh cadence: 5 seconds (heavier than tech tree). Smoke test pending — requires KSP with career save loaded.

## Phase 7 — Buildings, difficulty, built-in passthroughs
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-07`
- **Dependencies**: Phase 3 (Career foundation pattern), Phase 2 (MCP tool registration pattern)
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: kRPC 0.5.4 C# client does NOT expose `SpaceCenter.AstronautComplex` or `CrewMember.ExperienceLevel`/`ExperienceTrait`/`Vessel` — contrary to the prompt's guidance. `get_kerbals` was implemented as a Career-side kRPC procedure (`KerbalsService`) using `HighLogic.CurrentGame.CrewRoster` and `flightState.protoVessels`, like the other Career services. `CommNet.CommNetParams` field names: `rangeModifier`, `DSNModifier`, `requireSignalForControl`, `requireSignalForScience`, `enableCommNet`, `occlusion_multiplier_vac`, `plasmaBlackout` — verify against Assembly-CSharp before deploying. `GameParameters.CareerParams` field names: `SciGainMultiplier`, `FundsGainMultiplier`, `FundsLossMultiplier`, `RepGainMultiplier`, `RepLossMultiplier`. `GameParameters.DifficultyParams` field names: `ReentryHeatScale`, `CrashTolerance`, `KerbalGToleranceMultiplier`, `MissingCrewsRespawn`. Stubs manually extended (not re-generated from DLL) — regenerate after Career DLL deploy. `get_vessels` and `get_body_info` remain kRPC SpaceCenter passthroughs (no Career-side work needed). `dotnet test` passes: 47 tests (11 Shared + 36 MCP).

## Phase 8 — README + INSTALL + release packaging
- **Status**: complete
- **Branch**: `feature/ksp-mission-control-phase-08`
- **Dependencies**: Phase 7 (all features complete)
- **Started**: 2026-05-11
- **Completed**: 2026-05-11
- **Notes**: Phase 7 compile errors corrected before packaging: `CommNetParams` does not have `requireSignalForScience` or `enableCommNet` — those fields don't exist in KSP 1.12; `EnableCommNet` lives in `GameParameters.DifficultyParams`. `occlusion_multiplier_vac` → `occlusionMultiplierVac`. `SciGainMultiplier` → `ScienceGainMultiplier`. `CrashTolerance` and `KerbalGToleranceMultiplier` don't exist in `DifficultyParams` — removed. `KerbalRoster` indexer takes `int` not `KerbalType`; correct API is `roster.Crew` and `roster.Tourist` (IEnumerable). `DifficultySettings` DTO and its unit test updated to match. Zip `KSPMissionControl-v0.1.0.zip` produced and verified. All 47 tests pass (11 Shared + 36 MCP). Definition-of-done check: build ✓ / tests ✓ / packaging ✓ / README ✓ / INSTALL ✓ / repo state: PRs pending (see Phase 8 task). Cross-platform MCP build deferred to v0.2 (Windows-only in v0.1; noted in README Future section).
