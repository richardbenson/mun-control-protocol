# CI / Release Pipeline

**Completed:** 2026-05-12

---

## Original Requirements

Automate building, testing, and releasing Mun Control Protocol via GitHub Actions, removing
the requirement for a local KSP installation during packaging. The goal was to make every
versioned release a single `git tag` command with no manual steps.

The five external assemblies required by `MunControlProtocol.Career` (`Assembly-CSharp`,
`UnityEngine`, `UnityEngine.CoreModule`, `KRPC.Core`, `KRPC.SpaceCenter`) are normally
sourced from a local KSP installation via `$(KspInstallDir)` HintPaths. CI has no KSP
install, so the project could not build at all without a solution.

Version management was deliberately simplified: the git tag is the single source of truth
for version numbers — no version file to keep in sync. CKAN support was in scope for the
GameData mod component only; the MCP server is out of CKAN scope and must be installed
manually.

Out of scope: additional platforms (Linux/macOS), code signing, and publishing the MCP
server to a package registry.

---

## Work Done

### Phase 1 — KSP Assembly Stubs

Five C# stub projects were added to `lib/stubs/`, one per external assembly. Each project
sets `<AssemblyName>` to the real DLL name (e.g. `Assembly-CSharp`) so that at KSP runtime
the real installed assemblies resolve transparently over the stubs. `Career.csproj` gained
a conditional reference block selecting real DLLs when `$(KspInstallDir)` is set and stubs
otherwise. All stub references use `<Private>false</Private>` so no stub DLL is copied to
the Career bin output. The KSP API surface was fully enumerated from all six Career source
files to ensure coverage without excess.

### Phase 2 — CI Build & Test Workflow

`.github/workflows/ci.yml` was added, triggering on all pushes and on PRs targeting `main`.
The job runs on `windows-latest` (chosen because `net472` builds are unreliable on Linux
without `mono-devel`) and uses NuGet caching keyed on `.csproj` file hashes. Stubs are
built explicitly before the solution build, since the stub DLLs must exist on disk before
`Career.csproj` can reference them at restore/build time.

### Phase 3 — Release Workflow + Packaging

`.github/workflows/release.yml` triggers on `v*` tag pushes. It strips the leading `v`
from the tag, builds and tests the full solution, publishes the MCP server (win-x64,
framework-dependent), assembles a `publish/` staging tree, writes the AVC
`MunControlProtocol.version` file with MAJOR/MINOR/PATCH split from the tag, zips to
`MunControlProtocol-v$VERSION.zip`, and creates a GitHub Release via
`softprops/action-gh-release@v2`. Pre-release tags (containing `-`) automatically produce
a GitHub pre-release. `deploy/package-release.ps1` was updated with a `-NoKsp` switch for
local testing without a KSP install.

### Phase 4 — CKAN Metadata & Submission Guide

`ckan/mun-control-protocol.netkan` was added using `$kref: #/ckan/github/...` (version
from GitHub Releases tag) and `$vref: #/ckan/ksp-avc` (KSP compatibility from the AVC
file inside the ZIP). The `install` directive uses `find: GameData` which silently ignores
the `mcp/` folder. A `depends: [kRPC]` entry ensures CKAN installs kRPC automatically.
`docs/CKAN-SUBMISSION.md` documents every step from tagged release to CKAN availability.
`INSTALL.md` was updated with a prominent CKAN section that warns users CKAN installs only
the mod component, not the MCP server.

---

## Lessons Learned

- **`net472` builds require `windows-latest` in GitHub Actions.** The `.NET SDK` can cross-compile `net472` on Linux via reference assemblies, but the KSP stub projects produce output DLLs that must exist as real build artifacts — not just reference assemblies — before dependent projects can resolve them. Linux runners hit edge cases; `windows-latest` is the safe choice for KSP mod CI.

- **Stub DLLs must be built in a separate step before the solution build.** `dotnet build MunControlProtocol.sln` does not guarantee build order across independent projects. Explicitly building each stub project first avoids sporadic "reference not found" failures on clean runs.

- **The git tag is the only version source; split it in the workflow.** Extracting `MAJOR/MINOR/PATCH` with `cut -d. -fN` in bash keeps everything consistent and eliminates version-file drift. The AVC file written at release time reflects whatever tag was pushed.

- **`<Private>false</Private>` on stub references is non-negotiable.** Without it, stub DLLs are copied into the Career bin output and would be deployed inside the ZIP, shadowing the real KSP assemblies at runtime and likely causing type-identity conflicts.

- **CKAN's `find: GameData` install directive silently ignores everything else in the ZIP.** There is no need for an exclusion list; the `mcp/` folder is simply not installed. This is the canonical approach for mods that bundle non-GameData content.

- **The AVC `$vref` mechanism reads the version file from inside the release ZIP, not from the repo.** The file must be written by the release workflow (not committed to source) so that its VERSION field matches the tag. A committed static version file would cause every release to report the same KSP compatibility range until manually updated.

- **KerbalRoster iterators are `Crew` and `Tourist` (not `Kerbals`).** The real KSP 1.12 API uses these property names; the stubs were corrected after initial implementation when the Career project was tested against the actual game assemblies.
