# Phase 04 — CKAN Metadata & Submission Guide

## Goal

Produce a valid NetKAN file for the GameData mod component and a complete guide for
submitting it to the public CKAN index — everything needed to go from "ZIP exists on GitHub"
to "installable via CKAN".

## What CKAN installs

CKAN can only install files into the KSP `GameData/` directory. It will install:

```
GameData/KSPMissionControl/
  KSPMissionControl.Career.dll
  KSPMissionControl.Shared.dll
  KSPMissionControl.version
```

The `mcp/` server component is explicitly out of CKAN scope and must be installed manually
by the player. This must be clearly documented in the CKAN description and in INSTALL.md.

## NetKAN file format

The NetKAN file (`ckan/ksp-mission-control.netkan`) drives the automated CKAN metadata
generator (the "Inflator"). For a GitHub Releases source, the minimal required fields are:

```jsonc
{
  "spec_version": "v1.4",
  "identifier": "KSPMissionControl",
  "name": "KSP Mission Control",
  "abstract": "kRPC-based MCP server bridge for AI-assisted KSP career management. Exposes career data (tech tree, science, kerbals, buildings, difficulty) over the Model Context Protocol.",
  "author": [ "RichardBenson" ],
  "license": "MIT",
  "resources": {
    "homepage": "https://github.com/richardbenson/ksp-mission-control",
    "repository": "https://github.com/richardbenson/ksp-mission-control",
    "bugtracker": "https://github.com/richardbenson/ksp-mission-control/issues"
  },
  "$kref": "#/ckan/github/richardbenson/ksp-mission-control",
  "$vref": "#/ckan/ksp-avc",
  "depends": [
    { "name": "kRPC" }
  ],
  "install": [
    {
      "find": "GameData",
      "install_to": "GameData"
    }
  ],
  "ksp_version_min": "1.12.0",
  "ksp_version_max": "1.12.9",
  "tags": [ "career", "information" ]
}
```

### Key field explanations

**`$kref: #/ckan/github/user/repo`**
Tells the Inflator to pull release metadata (version, download URL, release date) from
the GitHub Releases API. The version is read from the release tag name (strips the leading `v`).

**`$vref: #/ckan/ksp-avc`**
Tells the Inflator to fetch `KSPMissionControl.version` from inside the release ZIP to
determine KSP version compatibility. The AVC file written by the Phase 03 release workflow
provides `KSP_VERSION_MIN` and `KSP_VERSION_MAX`.

**`depends: [kRPC]`**
CKAN identifier for the kRPC mod. This ensures CKAN installs kRPC automatically when
installing KSP Mission Control.

**`install: find: GameData`**
Extracts the `GameData/` folder from the ZIP and merges it into the player's KSP `GameData/`.
The `mcp/` folder in the ZIP is ignored by this directive.

## INSTALL.md update

Add a "CKAN Installation" section before the existing manual installation section:

```markdown
## CKAN Installation (recommended for the KSP mod)

The KSP mod component (Career DLL) is available via CKAN. Search for **KSP Mission Control**
in the CKAN client and click Install. CKAN will automatically install kRPC as a dependency.

**Important:** CKAN installs only the KSP mod. You must install the MCP server separately —
follow the [MCP Server Installation](#mcp-server-installation) section below.
```

## Files expected to change

| File | Change |
|---|---|
| `ckan/ksp-mission-control.netkan` | new |
| `docs/CKAN-SUBMISSION.md` | new |
| `INSTALL.md` | add CKAN section |

## Acceptance criteria

1. `ckan/ksp-mission-control.netkan` is valid JSON with all required NetKAN fields
2. Running `netkan --validate ckan/ksp-mission-control.netkan` (the CKAN validator) passes
3. `docs/CKAN-SUBMISSION.md` covers every step from "I have a tagged release" to "CKAN users can install it"
4. `INSTALL.md` clearly explains that CKAN installs only the mod component
