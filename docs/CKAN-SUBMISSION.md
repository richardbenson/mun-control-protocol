# CKAN Submission Guide — Mun Control Protocol

This guide covers everything from "I have a tagged GitHub Release" to "CKAN users can install
the mod" — including the first-time NetKAN PR, how subsequent releases flow automatically,
local testing, and updating the metadata when needed.

---

## Prerequisites

Before submitting to CKAN, confirm:

- [ ] A tagged GitHub Release exists (e.g. `v1.0.0`) with the ZIP attached to it
- [ ] The ZIP contains `GameData/MunControlProtocol/MunControlProtocol.version` with correct
      `VERSION` and `KSP_VERSION_MIN` / `KSP_VERSION_MAX` fields
- [ ] `ckan/mun-control-protocol.netkan` is committed to this repo on the `main` branch
- [ ] The `license` field in the netkan file matches the LICENSE file at the repo root (`MIT`)

---

## First-time submission (adding the mod to CKAN)

This is a one-time process. After the NetKAN PR merges, all future releases are handled
automatically by the CKAN bot.

### 1. Fork the NetKAN repository

Go to https://github.com/KSP-CKAN/NetKAN and click **Fork**.

### 2. Clone your fork

```bash
git clone https://github.com/<your-github-username>/NetKAN.git
cd NetKAN
```

### 3. Copy the netkan file into the fork

```bash
cp /path/to/mun-control-protocol/ckan/mun-control-protocol.netkan NetKAN/
```

### 4. Validate the file locally

**Option A — Python CLI:**

```bash
pip install ckan
netkan --validate NetKAN/mun-control-protocol.netkan
```

**Option B — Docker:**

```bash
docker run --rm -v "$(pwd):/work" ghcr.io/KSP-CKAN/ckan-validate \
  /work/NetKAN/mun-control-protocol.netkan
```

Fix any validation errors before continuing.

### 5. Test inflation (optional but recommended)

Inflation converts the `.netkan` into the `.ckan` file that the CKAN client installs from.
Running it locally lets you check the generated metadata before going public.

```bash
netkan --inflate NetKAN/mun-control-protocol.netkan
```

Review the output: confirm the version, download URL, and install directives look correct.

### 6. Commit and push

```bash
git add NetKAN/mun-control-protocol.netkan
git commit -m "Add Mun Control Protocol"
git push origin main
```

### 7. Open a pull request

Go to https://github.com/KSP-CKAN/NetKAN and open a PR from your fork's `main` branch.

**Title:** `Add Mun Control Protocol`

**Body:**

```
Adds Mun Control Protocol, a kRPC-based MCP server bridge for AI-assisted KSP career
management.

- GitHub repository: https://github.com/richardbenson/mun-control-protocol
- Latest release: https://github.com/richardbenson/mun-control-protocol/releases/latest
- License: MIT
- KSP version: 1.12.x
- Depends on: kRPC
```

### 8. Wait for review

CKAN maintainers review first-submission PRs. Expect **1–7 days**. They may request
field corrections — watch for PR comments and address them promptly.

---

## Releasing a new version

Once the first-time NetKAN PR is merged, **no manual CKAN steps are needed** for subsequent
releases.

**What happens automatically:**

1. You push a `v*` tag (e.g. `v1.1.0`) — the GitHub Release workflow creates a Release with
   the ZIP attached.
2. The CKAN NetKAN bot polls GitHub Releases via the `$kref` directive and detects the new
   release.
3. The bot inflates the updated `.netkan`, generating a new `.ckan` file, and opens a PR to
   `KSP-CKAN/CKAN-meta` automatically.
4. Within **1–2 hours** of the GitHub Release being published, the new version appears in
   the CKAN client for all users.

You only need to publish the GitHub Release — CKAN takes care of the rest.

---

## Testing the install locally

To verify the mod installs correctly before the NetKAN PR goes live:

1. Download the latest CKAN client from https://github.com/KSP-CKAN/CKAN/releases
2. In the CKAN GUI, go to **Settings → Compatible KSP Versions** and confirm `1.12.x` is listed
3. Inflate the netkan file locally to produce a `.ckan` file (see Step 5 above)
4. Install via CLI: `ckan install mun-control-protocol.ckan`
5. Verify `GameData/MunControlProtocol/` appears in the KSP install
6. Verify the `mcp/` folder does **not** appear — CKAN should not install it (the `find: GameData`
   install directive scopes the install to `GameData/` only)

---

## Updating the netkan file

If the mod's KSP compatibility range changes, or any other field needs updating:

1. Edit `ckan/mun-control-protocol.netkan` in this repository
2. Fork `KSP-CKAN/NetKAN` (or use your existing fork) and update the file there too
3. Open a PR to `KSP-CKAN/NetKAN` with just the updated file
4. Use a descriptive title, e.g.:
   - `Update Mun Control Protocol (ksp_version_max bump to 1.12.9)`
   - `Update Mun Control Protocol (add resources.spacedock field)`

---

## NetKAN field reference

| Field | What it does |
|---|---|
| `spec_version` | NetKAN schema version — keep at `v1.4` |
| `identifier` | CKAN package ID — **never change** after the first submission; changing it creates a duplicate entry |
| `name` | Display name shown in the CKAN client |
| `abstract` | Short description (1–2 sentences) shown in the CKAN client |
| `author` | List of GitHub usernames for the mod authors |
| `license` | SPDX identifier — must match the LICENSE file in the repository |
| `resources` | URLs for homepage, repository, and bug tracker |
| `$kref` | Tells the Inflator to fetch release info (version, download URL) from GitHub Releases |
| `$vref` | Tells the Inflator to read KSP version compatibility from the AVC `.version` file inside the ZIP |
| `depends` | CKAN mods that must be installed first — `kRPC` is automatically installed as a dependency |
| `install` | What to extract from the ZIP and where — `find: GameData` extracts only `GameData/`, ignoring `mcp/` |
| `ksp_version_min/max` | Fallback compatibility bounds if `$vref` AVC parsing fails |
| `tags` | Discovery tags in the CKAN client — `career` and `information` are appropriate |
