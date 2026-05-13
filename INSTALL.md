# Installing Mun Control Protocol

This guide walks you through getting Mun Control Protocol running with an MCP-compatible AI client. Claude Desktop is the path of least resistance, but the same MCP server works with any tool that supports local (stdio) MCP servers — see [Step 4](#step-4--configure-your-mcp-client) for setup against the top five clients. It assumes you are comfortable dropping files into `GameData/` but have never built a C# project or configured an MCP server.

---

## Prerequisites

Before you start, make sure you have:

- **KSP 1.12.x** — KSP2 is not supported. If you have multiple KSP installs (modded and stock), decide which one you want to use and keep it in mind throughout this guide.
- **An MCP-compatible AI client** — installed and signed in. See [Step 4](#step-4--configure-your-mcp-client) for the supported clients and their free-tier status.
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

The KSP mod component (Career DLL) is available via CKAN. Search for **Mun Control Protocol**
in the CKAN client and click Install. CKAN will automatically install kRPC as a dependency,
so you can skip Step 1 below if you use this method.

**Important:** CKAN installs only the KSP mod. You must install the MCP server separately —
follow the [Step 3 — Place the MCP server](#step-3--place-the-mcp-server) section below.

---

## Step 2 — Install the Career extension DLL

> **Skip this step if you installed via CKAN above.**

1. Download the latest **MunControlProtocol-vX.Y.Z.zip** from the [GitHub releases page](https://github.com/richardbenson/mun-control-protocol/releases).
2. Extract the zip.
3. Inside the extracted folder, find `GameData/MunControlProtocol/`. Copy the entire `MunControlProtocol/` folder into your KSP `GameData/` folder.

The result should be:

```
<KSP>/GameData/MunControlProtocol/MunControlProtocol.Career.dll
<KSP>/GameData/MunControlProtocol/MunControlProtocol.Shared.dll
```

If you have multiple KSP installs, copy into the one you chose in the Prerequisites step.

---

## Step 3 — Place the MCP server

1. In the same extracted zip, find the `mcp/` folder. It contains a single file: `MunControlProtocol.MCP.exe`.
2. Copy `MunControlProtocol.MCP.exe` to a stable location where it won't get accidentally deleted:
   - **Windows:** `C:\Tools\MunControlProtocol\`
   - **macOS:** `~/Tools/MunControlProtocol/`
3. Note the **full path** to the exe — you will need it in the next step.

---

## Step 4 — Configure your MCP client

Pick the AI client you want to use. Mun Control Protocol ships as a **local stdio MCP server** — the same `MunControlProtocol.MCP.exe` from Step 3 works with every client below. Only the config file location and JSON shape differ.

> **Free tier note:** The MCP server runs locally on your machine and is free. What costs money is the AI model that calls it. Of the five clients below, **Claude Desktop, Cursor, VS Code (GitHub Copilot), and Windsurf** all allow local MCP servers on their free tiers. **Claude Code** requires a paid Claude Pro/Max subscription or API credits. ChatGPT (Plus and above) and Claude.ai are deliberately omitted: at time of writing they only support **remote** (HTTP/SSE) MCP servers via Developer Mode, so this local exe will not connect to them out of the box.

Throughout the snippets below, replace the example path with the full path to `MunControlProtocol.MCP.exe` from Step 3. On Windows you must escape backslashes in JSON (`\\`).

### Option A — Claude Desktop (free tier supported)

The original MCP client and the simplest setup.

1. Open `claude_desktop_config.json` in a text editor. It lives at:
   - **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
   - **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`
2. Add the `"ksp"` entry to the `"mcpServers"` section:

   ```json
   {
     "mcpServers": {
       "ksp": {
         "command": "C:\\Tools\\MunControlProtocol\\MunControlProtocol.MCP.exe"
       }
     }
   }
   ```

   If `"mcpServers"` already exists with other entries, add `"ksp"` alongside them — do not replace the existing entries. A ready-made `claude_desktop_config.example.json` is included in the zip as a reference.

3. **Fully quit** Claude Desktop (system tray on Windows, menu bar on macOS — closing the window is not enough) and relaunch.

### Option B — Claude Code CLI (paid Claude subscription or API credits)

Anthropic's terminal client. MCP works on any plan that includes Claude Code (Pro, Max, or pay-as-you-go API).

Add the server with a single command:

```bash
claude mcp add ksp -- "C:\Tools\MunControlProtocol\MunControlProtocol.MCP.exe"
```

The `--` separator is required — it tells Claude Code that everything after it is the server command, not a CLI flag. Use `-s user` to make the server available across all projects (default is project-only). Verify with `claude mcp list`.

### Option C — Cursor (free tier supported)

Cursor is a VS Code fork with native MCP support on all plans, including Free.

1. Create or edit `mcp.json`:
   - **Global (all projects):** `~/.cursor/mcp.json` on macOS/Linux, `%USERPROFILE%\.cursor\mcp.json` on Windows
   - **Project-only:** `.cursor/mcp.json` in the project root
2. Use the same `"mcpServers"` shape as Claude Desktop:

   ```json
   {
     "mcpServers": {
       "ksp": {
         "command": "C:\\Tools\\MunControlProtocol\\MunControlProtocol.MCP.exe"
       }
     }
   }
   ```

3. Fully quit and reopen Cursor — MCP servers are only loaded at startup. Confirm the server appears under **Settings → Features → MCP**.

### Option D — VS Code with GitHub Copilot (free Copilot tier supported)

GitHub Copilot's agent mode in VS Code reads MCP servers from an `mcp.json` file. The Copilot free tier includes agent mode with monthly request limits.

1. Open the command palette and run **MCP: Open User Configuration** (global) or **MCP: Open Workspace Folder Configuration** (per-project, written to `.vscode/mcp.json`).
2. Add the `"ksp"` entry. **Note:** VS Code uses the root key `"servers"`, **not** `"mcpServers"` — this is the most common config mistake when copy-pasting from a Claude or Cursor setup.

   ```json
   {
     "servers": {
       "ksp": {
         "type": "stdio",
         "command": "C:\\Tools\\MunControlProtocol\\MunControlProtocol.MCP.exe"
       }
     }
   }
   ```

3. Save the file. VS Code will prompt you to trust the server the first time it starts. Open Copilot Chat, switch to **Agent** mode, and the KSP tools will be available.

### Option E — Windsurf (free tier supported)

Windsurf (the Codeium/Cognition editor) supports local MCP servers on all plans, including Free.

1. Open **Windsurf Settings → Cascade → Plugins → Manage plugins → View raw config** (or edit `~/.codeium/windsurf/mcp_config.json` directly).
2. Add a `"ksp"` entry under `"mcpServers"`:

   ```json
   {
     "mcpServers": {
       "ksp": {
         "command": "C:\\Tools\\MunControlProtocol\\MunControlProtocol.MCP.exe"
       }
     }
   }
   ```

3. Click **Refresh** in the plugins panel (or restart Windsurf) and the KSP tools will show up in Cascade.

---

## Step 5 — Verify

1. Make sure your MCP client has been fully restarted after the config change (see the per-client notes in Step 4).
2. Launch KSP and load a career save. Confirm the kRPC server window in the Space Center scene says **Server Online** (click the server window if needed to start the server).
3. In your AI client, ask:

   > Using the KSP tools, what's my current funds, science, and reputation?

   Expected: the assistant calls `get_career_state` and reports numbers matching your in-game HUD.

---

## Troubleshooting

**The assistant says no KSP tools are available**

The client did not pick up the config change. Check:
- The JSON is valid — paste it into [jsonlint.com](https://jsonlint.com) to confirm.
- You fully quit and relaunched the client (not just closed the window).
- The path in `"command"` is correct and uses `\\` on Windows.
- For VS Code, the root key is `"servers"`, not `"mcpServers"`.
- Look in the client's logs: Claude Desktop → `Help → Open Logs Folder`; VS Code → **MCP: Show Output**; Cursor/Windsurf → settings panel for the MCP/plugins section.

**The assistant returns a connection error or kRPC error**

The MCP server could not reach KSP:
- KSP is not running, or you haven't loaded a save yet.
- The kRPC server is not started — open the kRPC window in the Space Center scene and click **Start Server**.
- The default kRPC port (50000) is blocked by a firewall or another process. Check the kRPC window for the port it's actually using.

**Tools work but values look wrong**

- Confirm you loaded the expected save (career vs. sandbox affects what career data is present).
- Confirm the kRPC mod version is compatible with your KSP 1.12.x build.
- If you have multiple KSP installs, confirm the Career DLL is in the one you launched.
