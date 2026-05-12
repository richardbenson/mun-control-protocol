Read docs/ci-release-pipeline/PHASE_04.md for full context before starting.

You are implementing Phase 4 of the CI/Release Pipeline epic for the KSP Mission Control project.
Work in branch `feature/ci-release-pipeline-phase-4` (create it from `feature/ci-release-pipeline`
after Phase 03 has been merged).

Phase 03 must be complete: a GitHub Release must exist with a ZIP that contains
`GameData/KSPMissionControl/KSPMissionControl.version`.

## Objective

Produce three deliverables:
1. `ckan/ksp-mission-control.netkan` — valid NetKAN metadata file
2. `docs/CKAN-SUBMISSION.md` — complete submission guide (field-by-field + PR walkthrough)
3. Update to `INSTALL.md` — add CKAN as the recommended mod install method

## Step 1 — create `ckan/ksp-mission-control.netkan`

Create the `ckan/` directory and write the NetKAN file. Read PHASE_04.md for the complete
JSON content. Verify:
- The JSON is valid (no trailing commas, correct bracket nesting)
- `identifier` is `KSPMissionControl` (no spaces; this becomes the CKAN package ID)
- `$kref` uses the actual GitHub username/repo from the git remote (verify with `git remote -v`)
- `license` matches the LICENSE file at the repo root
- `depends` includes `kRPC` (the CKAN identifier for the kRPC mod — verify this is the correct CKAN ID by checking https://github.com/KSP-CKAN/CKAN-meta or searching the CKAN client)
- `ksp_version_min` and `ksp_version_max` match what the mod actually targets

## Step 2 — create `docs/CKAN-SUBMISSION.md`

Write a complete, step-by-step guide. Audience: the mod author (you, future-you). Content:

### Section: Prerequisites

- A tagged GitHub Release exists with the ZIP on GitHub Releases
- The ZIP contains `GameData/KSPMissionControl/KSPMissionControl.version` with correct version numbers
- The `ckan/ksp-mission-control.netkan` file is committed to this repo

### Section: First-time submission (submitting a new mod)

Explain the NetKAN submission process step by step:

1. **Fork** https://github.com/KSP-CKAN/NetKAN on GitHub
2. **Clone** the fork locally
3. **Copy** `ckan/ksp-mission-control.netkan` into the fork's `NetKAN/` directory
4. **Validate** the file locally:
   - Install the CKAN CLI: `pip install ckan` or download from https://github.com/nicowillis/ckan-cli
   - Or use Docker: `docker run --rm -it ghcr.io/KSP-CKAN/ckan-validate ...`
   - Command: `netkan --validate NetKAN/ksp-mission-control.netkan`
5. **Test inflation** (optional but recommended):
   - The Inflator turns the `.netkan` into a `.ckan` (the actual install metadata)
   - Run: `netkan --inflate NetKAN/ksp-mission-control.netkan` to see what CKAN will generate
6. **Commit** with message: `Add KSP Mission Control`
7. **Open a PR** to `KSP-CKAN/NetKAN` with title `Add KSP Mission Control`
   - PR body: brief description of the mod, link to GitHub repo, link to the release
8. **Wait** — CKAN maintainers review PRs; expect 1–7 days for a first submission

### Section: Releasing a new version

Explain what happens automatically vs. what's manual:

**Automatic (via NetKAN bot):**
- When you push a new `v*` tag, the GitHub release workflow creates a Release with a new ZIP
- The CKAN NetKAN bot monitors GitHub Releases via the `$kref` directive
- The bot detects the new release, inflates the updated `.netkan`, and opens a PR to
  `KSP-CKAN/CKAN-meta` automatically
- Within 1–2 hours of the GitHub Release being published, the new version appears in CKAN

**Nothing manual is required** for subsequent releases once the initial NetKAN PR is merged.

### Section: Testing the install locally

Explain how to test the `.ckan`/`.netkan` without going through the public CKAN bot:

1. Download the latest CKAN client from https://github.com/KSP-CKAN/CKAN/releases
2. In the CKAN GUI, go to Settings → Compatible KSP Versions, confirm 1.12.x is listed
3. Use File → Install from .ckan or the CLI: `ckan install ksp-mission-control.ckan`
4. Verify `GameData/KSPMissionControl/` appears in the KSP install
5. Verify the `mcp/` folder does NOT appear (CKAN should not install it)

### Section: Updating the netkan file

If the mod's KSP compatibility range changes or fields need updating:
1. Edit `ckan/ksp-mission-control.netkan` in this repo
2. Open a PR to `KSP-CKAN/NetKAN` with just the updated file
3. Title: `Update KSP Mission Control (ksp_version_max bump)` or similar

### Section: NetKAN field reference

Brief explanation of each field in `ksp-mission-control.netkan`:

| Field | What it does |
|---|---|
| `spec_version` | NetKAN schema version — keep at `v1.4` |
| `identifier` | CKAN package ID — never change after first submission |
| `name` | Display name in CKAN client |
| `abstract` | Short description (1–2 sentences) |
| `author` | List of GitHub usernames |
| `license` | SPDX identifier matching LICENSE file |
| `$kref` | Tells the Inflator where to fetch release info |
| `$vref` | Tells the Inflator where to fetch KSP version compatibility |
| `depends` | CKAN mods that must be installed first |
| `install` | What to extract from the ZIP and where |
| `ksp_version_min/max` | Override if `$vref` AVC parsing fails |
| `tags` | Discovery tags in the CKAN client |

## Step 3 — update `INSTALL.md`

Read the existing `INSTALL.md` first. Find the appropriate location (before or after the
existing "Prerequisites" section, depending on structure) and add:

```markdown
## Installing via CKAN (recommended for the KSP mod)

The KSP mod component of KSP Mission Control is available in CKAN. Search for
**KSP Mission Control** and click Install. CKAN will automatically install kRPC as a
dependency.

> **Note:** CKAN installs only the KSP mod (the `GameData/KSPMissionControl/` files).
> The MCP server must be installed separately — see the [MCP Server](#mcp-server) section below.
```

Adjust the anchor link (`#mcp-server`) to match the actual heading ID in the existing file.

## Validation

After writing the netkan file:
1. Confirm it is valid JSON (no syntax errors)
2. Confirm the `$kref` GitHub path matches the actual remote URL (check with `git remote get-url origin`)
3. Confirm `"license": "MIT"` matches the LICENSE file
4. Confirm the `depends` CKAN identifier for kRPC — the correct CKAN ID is `kRPC` (capital R, capital PC).
   Verify at https://github.com/KSP-CKAN/CKAN-meta/blob/master/kRPC.ckan if needed.

## Completion

When the phase is done:
1. Open `docs/ci-release-pipeline/PROGRESS.md`
2. Set Phase 04 Status to `complete`, fill in the completed date
3. Commit all changes to `feature/ci-release-pipeline-phase-4`
4. Open a PR targeting `feature/ci-release-pipeline`
5. After this PR merges, open a final PR from `feature/ci-release-pipeline` → `main`
