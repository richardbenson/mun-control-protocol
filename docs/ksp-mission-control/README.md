# KSP Mission Control

A C# mod and companion MCP server that exposes Kerbal Space Program 1 career save data to AI assistants (Claude Desktop and any other MCP-compatible client) so the AI can answer questions like *"with my current tech, what's the best Mun lander I can build?"*.

This directory contains the implementation plan. Phase prompts under `PHASE_XX.prompt.md` are intended to be handed to a coding agent one at a time.

---

## Scope

### In scope
- Read-only access to KSP1 (v1.12.x) career-mode data
- 10 MCP tools covering career currencies, tech tree, parts catalog and stats, science status, vessels, kerbals, buildings, celestial bodies, and difficulty settings
- Three-project C# solution: shared DTOs, in-game kRPC service extension, MCP server console exe
- Private GitHub repo, manual deploy (no CI)
- xUnit tests for `Shared` and `MCP` projects; manual in-game smoke testing for `Career` extension
- End-user `INSTALL.md` covering kRPC install, GameData copy, MCP exe placement, Claude Desktop config

### Out of scope (initial version)
- Writing data back to KSP (no launches, no purchases)
- KSP2
- Multiplayer
- Mod-added parts beyond what `PartLoader` exposes (should work but untested)
- VAB current-ship analysis
- GitHub Actions CI (live KSP can't run in CI; defer to a later epic if useful)
- Public release / license file

---

## Architecture (summary — full version in the requirements doc)

```
Claude Desktop  ─MCP/stdio─▶  KSPMissionControl.MCP.exe  ─kRPC/TCP─▶  KSP + kRPC mod + KSPMissionControl.Career.dll
```

Three projects:

| Project | Framework | Purpose |
|---|---|---|
| `KSPMissionControl.Shared` | netstandard2.0 | DTOs only, referenced by both other projects |
| `KSPMissionControl.Career` | net472 | kRPC service extension, ships in `GameData/` |
| `KSPMissionControl.MCP` | net8.0 | Console exe registered in Claude Desktop config |

The MCP server uses generated kRPC C# stubs (`krpc-clientgen`) to call into the Career extension. Stubs are committed to source control; regenerated only when the Career service surface changes.

See the requirements doc for the full MCP tool surface, KSP API mapping, and threading constraints.

---

## Phase plan

| # | Phase | Branch | Files | Status |
|---|---|---|---|---|
| 1 | Scaffolding & repo | `feature/ksp-mission-control-phase-01` | ~8 | not started |
| 2 | Vertical slice: `get_career_state` | `feature/ksp-mission-control-phase-02` | ~8 | not started |
| 3 | Career foundation + `get_tech_tree` | `feature/ksp-mission-control-phase-03` | ~7 | not started |
| 4 | Parts catalog (basic) | `feature/ksp-mission-control-phase-04` | ~6 | not started |
| 5 | Module-specific part stats | `feature/ksp-mission-control-phase-05` | ~4 | not started |
| 6 | Science | `feature/ksp-mission-control-phase-06` | ~5 | not started |
| 7 | Buildings, difficulty, built-in passthroughs | `feature/ksp-mission-control-phase-07` | ~9 | not started |
| 8 | README + INSTALL + release packaging | `feature/ksp-mission-control-phase-08` | ~4 | not started |

Live status in [PROGRESS.md](PROGRESS.md). Per-phase detail in `PHASE_XX.md` and `PHASE_XX.prompt.md`.

---

## Branching

- Feature branch: `feature/ksp-mission-control` (cut from `main`)
- Phase branches: `feature/ksp-mission-control-phase-01` … `phase-08`, each cut from the feature branch
- Phase PRs target the feature branch; the feature branch PR targets `main`

---

## Definition of Done (project level)

The project is complete when **all** of the following are true:

1. **Build**: `dotnet build KSPMissionControl.sln` succeeds with zero warnings on a fresh checkout (after the developer sets `KspInstallDir` per Phase 1).
2. **Tests**: `dotnet test` passes for `KSPMissionControl.Shared.Tests` and `KSPMissionControl.MCP.Tests`.
3. **In-game integration**: with KSP running, the kRPC mod loaded, and `KSPMissionControl.Career.dll` deployed, all 10 MCP tools return non-error responses against a career save and the data matches what's visible in-game (manually spot-checked).
4. **End-to-end via Claude Desktop**: each of the four worked-example queries from the requirements doc returns a coherent answer:
   - "Best Mun lander I can build?"
   - "What to unlock next for Duna?"
   - "Maximise science from what I haven't done?"
   - "Design a Duna comms constellation"
5. **Install docs**: a developer who has never seen the project can follow `INSTALL.md` end-to-end and reach a working setup.
6. **Repo state**: pushed to private GitHub, all phase PRs merged into the feature branch, feature branch merged into `main`.

---

## Risks

- **Stub-regen friction**: every change to the Career service surface costs a KSP relaunch + `krpc-clientgen` run. Phase prompts are designed to ship each service's full surface in one go.
- **Threading**: KSP API access is Unity-main-thread-only. The cache pattern established in Phase 3 must be followed by every subsequent Career service.
- **Parts payload size**: `get_part_stats` and `get_science_status` can return large blobs. Tool-layer parameter validation enforces filters; worth load-testing once Phase 5 lands.
- **Per-machine paths**: KSP/Unity DLL refs are absolute paths. `Directory.Build.props` (Phase 1) keeps these in one ignored file per checkout.
