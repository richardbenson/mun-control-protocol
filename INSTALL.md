# Installing KSP Mission Control

This guide walks you through getting KSP Mission Control running with Claude Desktop. It assumes you are comfortable dropping files into `GameData/` but have never built a C# project or configured an MCP server.

---

## Prerequisites

Before you start, make sure you have:

- **KSP 1.12.x** — KSP2 is not supported. If you have multiple KSP installs (modded and stock), decide which one you want to use and keep it in mind throughout this guide.
- **Claude Desktop** — installed and signed in.
- **A career-mode save** — a Sandbox save works for `get_vessels` and `get_body_info`; a Science save works for most tools; a Career save is needed for funds, reputation, buildings, and difficulty settings.

---

## Step 1 — Install the kRPC mod

kRPC is the bridge that lets the MCP server talk to KSP. Install it before doing anything else.

**Preferred — via CKAN:**

1. Open CKAN and select your KSP install.
2. Search for **kRPC** and install it. CKAN places all files in the right `GameData/` locations.

**Manual fallback:**

1. Download the latest kRPC release from [SpaceDock](https://spacedock.info/mod/69/kRPC) or [GitHub Releases](https://github.com/krpc/krpc/releases).
2. Extract the archive and copy the `kRPC/` folder into your KSP `GameData/` folder so the path becomes `<KSP>/GameData/kRPC/`.
3. See the [kRPC documentation](https://krpc.github.io/krpc/) if you run into trouble.

**Verify:**

Launch KSP and load any save. You should see a small kRPC server window in the bottom-right corner of the Space Center scene. It is fine to leave the default settings (localhost, port 50000).

---

## CKAN Installation (recommended for the KSP mod)

The KSP mod component (Career DLL) is available via CKAN. Search for **KSP Mission Control**
in the CKAN client and click Install. CKAN will automatically install kRPC as a dependency,
so you can skip Step 1 below if you use this method.

**Important:** CKAN installs only the KSP mod. You must install the MCP server separately —
follow the [Step 3 — Place the MCP server](#step-3--place-the-mcp-server) section below.

---

## Step 2 — Install the Career extension DLL

> **Skip this step if you installed via CKAN above.**

1. Download the latest **KSPMissionControl-vX.Y.Z.zip** from the [GitHub releases page](https://github.com/richardbenson/ksp-mission-control/releases).
2. Extract the zip.
3. Inside the extracted folder, find `GameData/KSPMissionControl/`. Copy the entire `KSPMissionControl/` folder into your KSP `GameData/` folder.

The result should be:

```
<KSP>/GameData/KSPMissionControl/KSPMissionControl.Career.dll
<KSP>/GameData/KSPMissionControl/KSPMissionControl.Shared.dll
```

If you have multiple KSP installs, copy into the one you chose in the Prerequisites step.

---

## Step 3 — Place the MCP server

1. In the same extracted zip, find the `mcp/` folder. It contains a single file: `KSPMissionControl.MCP.exe`.
2. Copy `KSPMissionControl.MCP.exe` to a stable location where it won't get accidentally deleted:
   - **Windows:** `C:\Tools\KSPMissionControl\`
   - **macOS:** `~/Tools/KSPMissionControl/`
3. Note the **full path** to the exe — you will need it in the next step.

---

## Step 4 — Edit Claude Desktop config

1. Open `claude_desktop_config.json` in a text editor. It lives at:
   - **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
   - **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

2. Add the `"ksp"` entry to the `"mcpServers"` section. Use the actual path from Step 3:

```json
{
  "mcpServers": {
    "ksp": {
      "command": "C:\\Tools\\KSPMissionControl\\KSPMissionControl.MCP.exe"
    }
  }
}
```

If `"mcpServers"` already exists with other entries, add `"ksp"` alongside them — do not replace the existing entries.

A ready-made `claude_desktop_config.example.json` is included in the zip — use it as a reference template.

---

## Step 5 — Restart Claude Desktop and verify

1. **Fully quit** Claude Desktop — not just close the window. On Windows use the system tray icon; on macOS use the menu bar icon.
2. Relaunch Claude Desktop.
3. Launch KSP and load a career save. Confirm the kRPC server window in the Space Center scene says **Server Online** (click the server window if needed to start the server).
4. In Claude Desktop, ask:

   > Using the KSP tools, what's my current funds, science, and reputation?

   Expected: Claude calls `get_career_state` and reports numbers matching your in-game HUD.

---

## Troubleshooting

**Claude says no KSP tools are available**

Claude Desktop did not pick up the config change. Check:
- The JSON is valid — paste it into [jsonlint.com](https://jsonlint.com) to confirm.
- You fully quit and relaunched (not just closed the window).
- The path in `"command"` is correct and uses `\\` on Windows.
- Check Claude Desktop's developer logs: `Help → Open Logs Folder`.

**Claude returns a connection error or kRPC error**

The MCP server could not reach KSP:
- KSP is not running, or you haven't loaded a save yet.
- The kRPC server is not started — open the kRPC window in the Space Center scene and click **Start Server**.
- The default kRPC port (50000) is blocked by a firewall or another process. Check the kRPC window for the port it's actually using.

**Tools work but values look wrong**

- Confirm you loaded the expected save (career vs. sandbox affects what career data is present).
- Confirm the kRPC mod version is compatible with your KSP 1.12.x build.
- If you have multiple KSP installs, confirm the Career DLL is in the one you launched.
