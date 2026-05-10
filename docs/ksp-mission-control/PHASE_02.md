# Phase 2 — Vertical slice: `get_career_state`

## Summary
Wire one MCP tool end-to-end using only kRPC's built-in `SpaceCenter` service. No Career extension yet. By the end of this phase, Claude Desktop can call `get_career_state` and receive live funds/science/reputation from a running KSP career save.

This phase establishes the patterns every subsequent MCP tool will follow:
- The `KrpcConnection` wrapper (lazy connect, single shared connection, disposal)
- The MCP tool registration shape (using `ModelContextProtocol` package conventions)
- The shared-DTO pattern (Career writes them, MCP reads them — even though only MCP touches them this phase)
- The xUnit test layout (one test project per testable assembly)

## Context
Picking `get_career_state` deliberately: it's the smallest tool, depends on **no** Career extension code, and exercises the full Claude Desktop ↔ MCP exe ↔ kRPC ↔ KSP path. If anything in that path is broken, this phase exposes it before we've built anything on top.

The Career extension is intentionally untouched in this phase. It's introduced in Phase 3.

## Files expected to change
- `src/KSPMissionControl.Shared/Models/CareerState.cs` — new
- `src/KSPMissionControl.MCP/Krpc/KrpcConnection.cs` — new
- `src/KSPMissionControl.MCP/Tools/CareerTools.cs` — new
- `src/KSPMissionControl.MCP/Program.cs` — replace placeholder with real MCP server bootstrap
- `tests/KSPMissionControl.Shared.Tests/KSPMissionControl.Shared.Tests.csproj` — new
- `tests/KSPMissionControl.MCP.Tests/KSPMissionControl.MCP.Tests.csproj` — new
- `tests/KSPMissionControl.MCP.Tests/CareerToolsTests.cs` — new
- `deploy/claude_desktop_config.example.json` — new
- `KSPMissionControl.sln` — add the two test projects

## Acceptance criteria
1. `dotnet test` passes.
2. With KSP running, kRPC mod loaded, and a career save loaded, manually invoking the MCP server via `claude_desktop_config` and asking Claude "what's my current funds?" returns the actual funds value visible on the in-game HUD.
3. The MCP server logs a clear error and exits non-zero if it cannot connect to kRPC on startup (e.g. KSP isn't running).
4. PR opened against `feature/ksp-mission-control` from `feature/ksp-mission-control-phase-02`.
