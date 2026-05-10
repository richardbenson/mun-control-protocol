Read `docs/ksp-mission-control/PHASE_08.md` and `docs/ksp-mission-control/README.md` first. Glance at all earlier `PHASE_XX.md` files for the feature list to advertise.

You are implementing Phase 8: documentation and release packaging. No application code changes.

## Branch
- Cut `feature/ksp-mission-control-phase-08` from `feature/ksp-mission-control`.
- PR target: `feature/ksp-mission-control` (then a final PR will go from there into `main`).

## Tasks

### 1. Root `README.md`
Replace the Phase 1 stub. Sections:

#### Overview (one paragraph)
What it does, who it's for, what KSP version. The "give your AI access to your career save" elevator pitch.

#### Status badge / version
Placeholder line for "v0.1 — first usable release" or similar.

#### The 10 tools
Bullet list, one line each:
- `get_career_state` — funds, science, reputation
- `get_tech_tree` — all nodes with unlock status and parts
- `get_parts_by_category` — parts filtered by category
- `get_part_stats` — part metadata + module-specific stats (engine, antenna, tank, command, solar)
- `get_science_status` — science subjects with diminishing-return state
- `get_vessels` — active flights with orbital data and crew
- `get_building_levels` — KSC facility upgrade levels
- `get_kerbals` — full Kerbal roster
- `get_body_info` — celestial body data
- `get_difficulty_settings` — career modifiers including CommNet

#### Quick start
"See [INSTALL.md](INSTALL.md)."

#### Example queries
4-5 lines of natural-language example queries (pull from the requirements doc's worked examples).

#### Architecture
One paragraph + the ASCII diagram from the requirements doc.

#### Contributing
Link to `docs/ksp-mission-control/` for the implementation plan and per-phase docs. One sentence on the dev loop: "build → `deploy/build-and-deploy.ps1` → relaunch KSP → test."

#### License
"Currently a private project. License TBD."

### 2. `INSTALL.md`
End-user install guide. Audience: KSP modder, comfortable with GameData, never built a C# project, never seen MCP before.

#### Prerequisites
- KSP 1.12.x (note: KSP2 not supported)
- .NET 8 runtime (link to Microsoft download — point to the runtime, not the SDK)
- Claude Desktop installed
- A career-mode save (Sandbox works for some tools; Science works for most; Career covers everything)

#### Step 1 — Install kRPC mod
- Preferred: install via CKAN. Search for "kRPC", install. CKAN handles GameData placement.
- Manual: download the latest kRPC release from its SpaceDock or GitHub releases, drop the `kRPC` folder into KSP's `GameData/`. Link to the kRPC docs.
- After install, launch KSP once and confirm the kRPC server window appears (it autoshows on first run; it lives at the bottom-right of the Space Center scene).

#### Step 2 — Install the Career extension DLL
- Download the latest `KSPMissionControl-vX.Y.Z.zip` from the GitHub releases page (link with placeholder URL).
- Extract.
- Copy the `KSPMissionControl/` folder from the extracted `GameData/` into KSP's `GameData/` so the path becomes `<KSP>/GameData/KSPMissionControl/KSPMissionControl.Career.dll`.

#### Step 3 — Place the MCP server
- From the same extracted zip, find the `mcp/` folder. It contains `KSPMissionControl.MCP.exe` and its dependencies.
- Move the entire folder to a stable location (e.g. `C:\Tools\KSPMissionControl\mcp\` on Windows, `~/Tools/KSPMissionControl/mcp/` on macOS).
- Note the **full path** to `KSPMissionControl.MCP.exe`.

#### Step 4 — Edit Claude Desktop config
- Open `claude_desktop_config.json`. Location:
  - **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
  - **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Add (or merge) the following — using your actual path from Step 3:

```json
{
  "mcpServers": {
    "ksp": {
      "command": "C:\\Tools\\KSPMissionControl\\mcp\\KSPMissionControl.MCP.exe"
    }
  }
}
```

If `mcpServers` already exists, add the `"ksp"` entry alongside whatever's already there.

#### Step 5 — Restart Claude Desktop and verify
- Fully quit Claude Desktop (not just close the window — quit from system tray / menu bar).
- Relaunch.
- Launch KSP, load a career save, confirm the kRPC server is running (its window in the Space Center scene should say "Server Online").
- In Claude, ask: "Using the KSP tools, what's my current funds, science, and reputation?"
- Expected: Claude calls `get_career_state` and reports values matching your in-game HUD.

#### Troubleshooting
- **Claude says no KSP tools available**: Claude Desktop didn't pick up the config. Confirm the JSON is valid (use a linter), confirm you fully quit-and-relaunched, check Claude Desktop's developer logs.
- **Claude returns an error mentioning kRPC connection**: KSP isn't running, kRPC server isn't started, or kRPC's port (default 50000) is firewalled.
- **Tools work but values look wrong**: confirm you're on the expected save (career vs sandbox) and that the kRPC mod is current (1.12-compatible).

### 3. Release packaging script
Create `deploy/package-release.ps1`:

```
param([string]$Version = "0.1.0")
```

Steps:
1. `dotnet publish src/KSPMissionControl.MCP/KSPMissionControl.MCP.csproj -c Release -r win-x64 --self-contained false -o ./publish/mcp` (this gets a runnable exe + deps without bundling the .NET runtime — the user installs it once via the prerequisites). Optionally also produce a `--self-contained true` build for users who can't install .NET 8 — your call, document the choice.
2. `dotnet build src/KSPMissionControl.Career/KSPMissionControl.Career.csproj -c Release` → copy the produced `KSPMissionControl.Career.dll` and `KSPMissionControl.Shared.dll` into `./publish/GameData/KSPMissionControl/`.
3. Copy `INSTALL.md` and `deploy/claude_desktop_config.example.json` into `./publish/`.
4. `Compress-Archive -Path ./publish/* -DestinationPath ./KSPMissionControl-v$Version.zip -Force`.
5. Echo the path of the produced zip.

Run the script once and verify the zip extracts to a structure matching the INSTALL.md instructions.

### 4. Update PROGRESS
Mark Phase 8 complete. Mark all eight phases done. In a final notes block at the bottom of PROGRESS.md, add:
- The zip filename produced.
- A "definition of done" check confirming each criterion in `README.md`'s DoD section was met (or noting any deferred items).
- A link to the open feature-branch PR into `main`.

### 5. Final PR
- Merge the Phase 8 PR into `feature/ksp-mission-control`.
- Open a PR from `feature/ksp-mission-control` into `main`. PR description: project summary, link to `README.md`, link to `INSTALL.md`, the 10-tool list, and a one-paragraph "what this enables" statement.

## Edge cases
- **macOS/Linux MCP exe**: the `dotnet publish` command produces a Windows-targeted exe with `-r win-x64`. If cross-platform support is desired, also produce `osx-x64` and `linux-x64` builds. Decision: ship Windows-only in v0.1; document this in `INSTALL.md` and add a `# Future` note in `README.md` about cross-platform.
- **kRPC port conflict**: rare but happens. INSTALL.md troubleshooting mentions it; don't try to autodetect.
- **User has multiple KSP installs** (modded + stock): they need to copy the DLL into the correct one. INSTALL.md should mention this.

## Definition of done
All acceptance criteria in `PHASE_08.md` are met. Project DoD in `README.md` is fully checked. PR is open into `main`.
