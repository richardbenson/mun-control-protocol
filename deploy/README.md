# Deployment

## Claude Desktop configuration

Copy `claude_desktop_config.example.json` into your Claude Desktop config file and update the path.

**Windows config location:**
```
%APPDATA%\Claude\claude_desktop_config.json
```

Replace `C:\path\to\MunControlProtocol.MCP.exe` with the actual path to the built executable, e.g.:
```
C:\Users\you\MunControlProtocol\MunControlProtocol.MCP.exe
```

If a `claude_desktop_config.json` already exists, merge the `mcpServers` block into it rather than replacing the file.

## Pre-requisites

1. KSP 1.12.x installed.
2. kRPC mod installed and enabled (see `INSTALL.md` in the repo root, coming in Phase 8).
3. A career save loaded in KSP before asking Claude any career-data questions.
