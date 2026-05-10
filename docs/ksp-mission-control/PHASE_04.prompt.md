Read `docs/ksp-mission-control/PHASE_04.md`, then re-read `PHASE_03.md` and `PHASE_03.prompt.md` — Phase 4 strictly follows the patterns Phase 3 established (StateCache, addon-driven refresh, zero KSP API in service procedures, regenerate stubs, etc.).

You are implementing Phase 4: parts catalog (basic fields only). Module-specific stats are deliberately deferred to Phase 5; do not pull them in here.

## Branch
- Cut `feature/ksp-mission-control-phase-04` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control`.

## Tasks

### 1. Shared DTO
Create `src/KSPMissionControl.Shared/Models/PartInfo.cs`:
- Properties: `Name`, `Title`, `Category` (string — use the KSP `PartCategories` enum's `ToString()`; expressing as string keeps the DTO independent of KSP DLLs), `MassDry` (double, tonnes), `MassWet` (double, tonnes), `Cost` (double, funds), `TechRequired` (string, tech node id).
- Public sealed class, get/set properties (kRPC serialiser).

### 2. PartsService
Create `src/KSPMissionControl.Career/Services/PartsService.cs`. Mirrors the Phase 3 pattern.

The cached snapshot type: `Dictionary<string, PartInfo>` keyed by part name (allows O(1) lookup for `GetPartStats(partName)`).

Cache population (called from the addon, on Unity main thread):
- Iterate `PartLoader.LoadedPartsList`.
- For each `AvailablePart`:
  - Skip if its `TechRequired` tech node is **not** in the unlocked set (`ResearchAndDevelopment.GetTechnologyState(part.TechRequired) != RDTech.State.Available`).
  - Build a `PartInfo`:
    - `Name = part.name`
    - `Title = part.title`
    - `Category = part.category.ToString()`
    - `Cost = part.cost`
    - `MassDry = part.partPrefab.mass`
    - `MassWet = part.partPrefab.mass + part.partPrefab.Resources.Sum(r => r.amount * r.info.density)` (verify property names by reading the relevant Squad classes; `PartResource.amount` is in units, `PartResourceDefinition.density` is in tonnes per unit)
    - `TechRequired = part.TechRequired`
- Cache it.

Service procedures (kRPC thread, cache-only):
- `GetPartsByCategory(string category)` — returns `IList<PartInfo>` of cached entries whose `Category` equals the argument (case-insensitive).
- `GetPartByName(string name)` — returns `PartInfo` or null.

Refresh cadence: tech-tree-driven (when the tech tree changes, the parts list changes). Simplest: refresh once per second alongside the tech tree, or hook the same refresh trigger.

### 3. Update addon
Extend `KSPMissionControlAddon.cs` to:
- Construct `PartsService` and inject its `StateCache<Dictionary<string, PartInfo>>`.
- Refresh the parts cache from the same tick that refreshes the tech tree (they're correlated).

### 4. Regenerate stubs
With the new service deployed and KSP running, re-run `krpc-clientgen` per Phase 3's recorded command. Replace `KSPMissionControlStubs.cs`.

### 5. MCP tool layer
Create `src/KSPMissionControl.MCP/Tools/PartsTools.cs` with two MCP tools:

#### `get_parts_by_category`
- Required parameter: `category` (string).
- Description: "Returns parts unlocked in the player's current tech tree, filtered to the given category (engine, fuel tank, command, science, communication, structural, ...)."
- Validates `category` is non-empty before calling kRPC; returns a clean MCP tool error if missing.

#### `get_part_stats`
- Optional parameters: `part_name` (string), `category` (string).
- Description: "Returns part stats. Provide either `part_name` for a single part, or `category` for all parts in that category. At least one is required."
- Validation: if both null/empty, return MCP tool error "Provide either part_name or category."
- If `part_name` provided: call `GetPartByName`, return single-element list (or empty if not found).
- If only `category`: call `GetPartsByCategory`.
- If both: prefer `part_name` (it's more specific), ignore `category`.

### 6. Tests
- `PartsToolsTests.cs`: mocked-stub tests for both tools, including the "neither parameter provided" validation case.

### 7. Manual smoke test
- Deploy + regenerate stubs (per Phase 3 script).
- In-game: open VAB, filter to Command Pods, note the list of unlocked pods.
- In Claude: "show me my available command pods" — list should match.
- In Claude: "what's the dry mass of the Mk1 command pod?" — should match VAB.

### 8. Update PROGRESS
Mark Phase 4 complete. Notes: any quirks discovered with the wet-mass calculation (some parts have resources defined but are usually launched empty; document the choice).

## Edge cases
- **Parts with `TechRequired = "start"`**: these are the always-available starter parts. They should be included.
- **Parts with no `TechRequired`**: shouldn't happen in stock, but defend against it (treat as locked, or as start — your call, document the choice).
- **EVA construction parts, kerbal-only items**: include them; the AI can decide if they're relevant.
- **Mod-added parts**: `LoadedPartsList` enumerates them automatically. Don't filter.
- **Case sensitivity on category**: KSP's `PartCategories` enum is PascalCase (`Engine`, `FuelTank`, `Communication`). Accept any case in the MCP parameter; normalize before comparing.

## Definition of done
All acceptance criteria in `PHASE_04.md` are met. PR description includes one or two example Claude responses demonstrating the new tools working.
