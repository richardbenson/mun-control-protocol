Read `docs/ksp-mission-control/PHASE_06.md`, `PHASE_03.md`, and `PHASE_03.prompt.md` first. Phase 6 is a third application of the Phase 3 service pattern.

You are implementing Phase 6: science status.

## Branch
- Cut `feature/ksp-mission-control-phase-06` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Shared DTO
Create `src/KSPMissionControl.Shared/Models/ScienceSubject.cs`:
- `Id` (string — the `crewReport@KerbinFlyingHighlands` style id)
- `ExperimentId` (string — extracted from the id, e.g. `crewReport`)
- `Body` (string — e.g. `Kerbin`)
- `Situation` (string — e.g. `FlyingHigh`, `InSpaceLow`, `Landed`, `Splashed`, `FlyingLow`)
- `Biome` (string — e.g. `Highlands`, or empty if global)
- `Title` (string — kRPC's `ScienceSubject.Title` may already provide a human-readable form)
- `Earned` (double, science points)
- `Cap` (double, science points)
- `Remaining` (double, computed: `Cap - Earned`)
- `SubjectValue` (double, the diminishing-return scaling currently applied: 1.0 = full value, lower = degraded)
- `ScienceMultiplier` (double, the body+situation scalar)

All sealed class, get/set.

### 2. ScienceService
Create `src/KSPMissionControl.Career/Services/ScienceService.cs`. Cache type: `List<ScienceSubject>`.

Cache population (Unity main thread):
- Iterate `ResearchAndDevelopment.GetExperimentSubjects()` (returns `IList<ScienceSubject>` in the Squad namespace — collides with our DTO name, so qualify as `global::ScienceSubject` or alias the using).
- For each Squad subject, parse the `id` into `ExperimentId`, `Body`, `Situation`, `Biome`. The id format is `<expId>@<body><Situation><Biome>`. Body names are unambiguous (Kerbin, Mun, Minmus, Duna, ...); situations are a closed enum (`Landed`, `Splashed`, `FlyingLow`, `FlyingHigh`, `InSpaceLow`, `InSpaceHigh`); biome is the remainder. **Edge case**: not every subject has a biome (e.g. atmospheric experiments often don't). If the parsed remainder is empty, set `Biome = ""`.
- Populate the DTO from the Squad subject's properties: `science`, `scienceCap`, `subjectValue`, `scientificValue` (verify exact KSP property names — the requirements doc lists `science`, `scienceCap`, `subjectValue`, `scientificValue`).
- Cache the list.

Refresh cadence: science state changes when experiments are completed/transmitted. Refreshing once every few seconds is fine. Hook the same tick as the tech tree refresh, or use the kRPC built-in `OnScienceChanged` event if available — your call.

Service procedures (kRPC thread, cache-only):
- `GetScienceSubjects(string body, string situation)` — returns `IList<ScienceSubject>` filtered by the (possibly empty) arguments. Empty string = no filter on that axis.

### 3. Update addon
Extend `KSPMissionControlAddon.cs` to construct `ScienceService` and pump its cache.

### 4. Regenerate stubs
Per Phase 3's recorded command.

### 5. MCP tool
Create `src/KSPMissionControl.MCP/Tools/ScienceTools.cs` with `get_science_status`:
- Optional parameters: `body` (string), `situation` (string).
- Description: "Returns the player's science subject status. Subject id format: `<experimentId>@<body><situation><biome>`. Provide `body` to scope the response (strongly recommended). Optionally provide `situation` (Landed, Splashed, FlyingLow, FlyingHigh, InSpaceLow, InSpaceHigh) to narrow further."
- Validation: if `body` is null/empty:
  - Do NOT call kRPC for the full matrix.
  - Instead, return a small object: `{ warning: "No body filter; returning per-body summary only", summary: [{ body: "Kerbin", subjectsTotal: N, subjectsCompleted: M, scienceRemaining: X }, ...] }`.
  - This requires a second cache surface in `ScienceService`: a per-body summary. Build it during cache population (it's cheap: group by body, count, sum).
  - Add a second service procedure `GetSciencePerBodySummary()` returning that summary list.
- If `body` is provided: pass through (with optional `situation`) to `GetScienceSubjects`.

### 6. Tests
- `ScienceToolsTests.cs`: mocked-stub tests covering (a) body+situation filter, (b) body-only filter, (c) no-body returns summary not the full matrix.
- Optional: a parsing test for the id-decomposition logic if it lives anywhere in the MCP project (it shouldn't — parsing is on the Career side — but if you extract a helper, test it).

### 7. Manual smoke test
- Deploy + regenerate + rebuild.
- In-game: open R&D archives, note Kerbin science status (should be heavily progressed in any non-fresh save).
- In Claude: "what science do I still have left to gather on Kerbin?" — should list subjects with `Remaining > 0` and reasonable `SubjectValue` (close to 1 for untouched, lower for repeated).
- In Claude: "show science summary across all bodies" (no body filter) — should return the per-body summary, not the full matrix.

### 8. Update PROGRESS
Mark Phase 6 complete. Notes: the exact id-parsing logic chosen (the body-name-then-situation-then-biome string split is fiddly — record any quirks).

## Edge cases
- **Subjects with `Cap == 0`**: experiments that aren't valid for that body+situation. Filter them out during cache population — they're not actionable.
- **Asteroid science / comet science** (KSP 1.10+ comet update): subjects exist for `Asteroid` and `Comet` "bodies". Pass them through; the AI can decide what to do with them.
- **Modded experiments** (DMagic Orbital Science, etc.): handled automatically via `GetExperimentSubjects()`. Their body/situation/biome parsing should still work because the id format is enforced by KSP.
- **Recovery and recovery-from-orbit**: subjects with id like `recovery@KerbinSrfLandedKSC`. Don't break parsing on these.
- **Body names with multiple words** (modded planets like `Eve OPM`): unlikely in stock, but if parsing chokes, fall back to leaving Body as the longest known-body prefix match. Stock-only: don't over-engineer.

## Definition of done
All acceptance criteria in `PHASE_06.md` are met. PR description includes a comparison between Claude's response and the in-game R&D archives view for one body.
