Read `docs/ksp-mission-control/PHASE_02.md` and `docs/ksp-mission-control/README.md` first for full context. Also re-read `PROGRESS.md` Phase 1 notes for the pinned NuGet package versions.

You are implementing Phase 2 of KSP Mission Control: a vertical slice that wires the **single** MCP tool `get_career_state` end-to-end through the kRPC built-in `SpaceCenter` service. Do **not** create or modify the `KSPMissionControl.Career` project in this phase.

## Branch
- Cut `feature/ksp-mission-control-phase-02` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Shared DTO
Create `src/KSPMissionControl.Shared/Models/CareerState.cs`. Public class with three `double` properties (kRPC returns `float` for funds/sci/rep, but doubles avoid precision loss in JSON serialisation):
- `Funds`
- `Science`
- `Reputation`

Add an XML doc comment on each property describing units (currency, science points, reputation points).

### 2. kRPC connection wrapper
Create `src/KSPMissionControl.MCP/Krpc/KrpcConnection.cs`. Responsibilities:
- Wraps a single `KRPC.Client.Connection` instance.
- Lazy-connects on first access (the MCP host process may start before KSP is ready).
- Exposes a `SpaceCenter` property returning a cached `KRPC.Client.Services.SpaceCenter.Service`.
- Implements `IDisposable`; disposes the underlying connection.
- On connection failure, throws a typed `KrpcConnectionException` with the inner exception attached and a message instructing the user to launch KSP and verify the kRPC server is running on the default port.

This class is the **only** place in the MCP server that touches the kRPC NuGet directly. All tool classes go through it.

### 3. MCP tool: `get_career_state`
Create `src/KSPMissionControl.MCP/Tools/CareerTools.cs`. One async method `GetCareerStateAsync()` that:
- Calls `KrpcConnection.SpaceCenter.Funds`, `.Science`, `.ReputationValue` (verify the exact property name in the `KRPC.Client` SpaceCenter service — it may be `.Reputation`).
- Returns a `CareerState` DTO.
- Decorate with whatever attributes `ModelContextProtocol` requires to expose this method as the MCP tool `get_career_state`. Description: "Returns current career-mode currencies: funds, science, and reputation."
- The tool takes no parameters.

### 4. MCP server bootstrap
Replace `src/KSPMissionControl.MCP/Program.cs` with a real MCP server entry point:
- Use the `ModelContextProtocol` package's host builder pattern (consult the package's README on NuGet — the API is stable as of v1.x).
- Register the stdio transport (Claude Desktop uses stdio).
- Register `CareerTools` as a tool source.
- Construct a single `KrpcConnection` instance and inject it into `CareerTools`.
- On unhandled exception during startup, log to **stderr** (stdout is reserved for MCP protocol traffic — never write logs there) and exit with code 1.

### 5. xUnit test projects
Create two test projects under `tests/`:

#### `tests/KSPMissionControl.Shared.Tests/`
- `net8.0` target, references `KSPMissionControl.Shared`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`.
- One test file with one trivial test confirming `CareerState` properties round-trip through `System.Text.Json`. This validates the project wires up correctly; deeper tests come in later phases.

#### `tests/KSPMissionControl.MCP.Tests/`
- `net8.0` target, references `KSPMissionControl.MCP`, the same xUnit packages, and `Moq` (or `NSubstitute` — pick one, justify briefly in the PR description).
- `CareerToolsTests.cs` with at least:
  - One test mocking the `KrpcConnection` (or its `SpaceCenter` accessor) to return canned values, asserting `CareerTools.GetCareerStateAsync()` maps them into a `CareerState` correctly.
  - One test asserting `CareerTools.GetCareerStateAsync()` propagates `KrpcConnectionException` rather than swallowing it.
- Note: this requires `KrpcConnection` to expose its `SpaceCenter` accessor through an interface or virtual member so it can be mocked. Refactor accordingly — but keep the interface internal-with-`InternalsVisibleTo` for the test project rather than polluting the public API.

Add both test projects to `KSPMissionControl.sln`.

### 6. Claude Desktop config example
Create `deploy/claude_desktop_config.example.json` with the entry from the requirements doc. Use a placeholder `C:\\path\\to\\KSPMissionControl.MCP.exe` and add a comment-style note in the surrounding markdown (or as a sibling `README.md` in `deploy/`) explaining where to drop this on Windows (`%APPDATA%\Claude\claude_desktop_config.json`).

### 7. Manual smoke test
- Build the solution.
- Copy `KSPMissionControl.MCP.exe` (and its dependencies) to a known path.
- Edit your local Claude Desktop config to point at it.
- Launch KSP, load a career save, confirm kRPC server is running.
- Restart Claude Desktop, ask "what's my current funds, science, and reputation?".
- Verify the answer matches the in-game HUD.

### 8. Update PROGRESS
Set Phase 2 status to `complete`, fill dates, and in **Notes** record:
- The exact `ModelContextProtocol` API entry point you used (e.g. `McpServerBuilder.ForStdio()...`) so Phase 3+ can copy the pattern.
- Whether the kRPC SpaceCenter property is `.ReputationValue` or `.Reputation` (or whatever it actually is) — Phase 3+ will not need this but the consistency note is useful.

## Code patterns to establish
- All MCP tool classes live under `src/KSPMissionControl.MCP/Tools/`, one class per logical group.
- All kRPC access goes through `KrpcConnection` — tools never import `KRPC.Client.*` directly except via the wrapper.
- All DTOs returned across the MCP boundary live in `KSPMissionControl.Shared.Models`.
- Logs go to **stderr**. Stdout belongs to the MCP transport.

## Edge cases
- **MCP host starts before KSP**: must not crash. Lazy connection + first-call retry is acceptable; if connection still fails, propagate `KrpcConnectionException` so the MCP layer returns a clean tool error rather than dying.
- **kRPC port unreachable**: typed exception with actionable message ("launch KSP and confirm kRPC server is enabled in the kRPC mod's status window").
- **Career save not loaded**: `SpaceCenter.Funds` returns 0 in non-career modes. That's acceptable for this tool — don't over-engineer detection here.

## Definition of done for this phase
All acceptance criteria in `PHASE_02.md` are met. PR is open against `feature/ksp-mission-control` with a description summarising the live-test result (funds value matched HUD, etc.).
