#!/usr/bin/env python3
"""
MCP tool-discovery smoke test for Mun Control Protocol.

Launches the MCP server exe, performs the JSON-RPC initialize handshake,
calls tools/list, and verifies every expected tool is registered.

Usage:
    python test-mcp-tools.py <path-to-MunControlProtocol.MCP.exe>

Exit codes:
    0  all expected tools present
    1  one or more tools missing (or server failed to respond)
"""

import json
import subprocess
import sys
import threading

# Keep this list in sync with the WithTools<> registrations in Program.cs.
EXPECTED_TOOLS = [
    # Live career data tools
    "get_current_craft",
    "get_career_state",
    "get_tech_tree",
    "get_parts_by_category",
    "get_part_stats",
    "get_science_status",
    "get_vessels",
    "get_building_levels",
    "get_kerbals",
    "get_body_info",
    "get_difficulty_settings",
    # Formula tools (no KSP connection required)
    "calculate_delta_v",
    "calculate_orbital_velocity",
    "calculate_orbital_period",
    "calculate_hohmann_transfer",
    "calculate_escape_velocity",
    "calculate_synchronous_orbit",
    "calculate_commnet_range",
]

TIMEOUT_S = 10


def _mcp_msg(obj: dict) -> str:
    return json.dumps(obj, separators=(",", ":")) + "\n"


def discover_tools(exe_path: str) -> list[str] | None:
    """Launch the MCP server and return the list of registered tool names."""
    messages = [
        _mcp_msg({
            "jsonrpc": "2.0", "id": 1, "method": "initialize",
            "params": {
                "protocolVersion": "2024-11-05",
                "capabilities": {},
                "clientInfo": {"name": "smoke-test", "version": "1.0"},
            },
        }),
        _mcp_msg({"jsonrpc": "2.0", "method": "notifications/initialized", "params": {}}),
        _mcp_msg({"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}),
    ]

    proc = subprocess.Popen(
        [exe_path],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
    )

    result: list[str] | None = None
    found = threading.Event()

    def _read_stdout():
        nonlocal result
        for line in proc.stdout:
            line = line.strip()
            if not line:
                continue
            try:
                msg = json.loads(line)
                if msg.get("id") == 2 and "result" in msg:
                    result = [t["name"] for t in msg["result"].get("tools", [])]
                    found.set()
                    return
            except json.JSONDecodeError:
                pass

    reader = threading.Thread(target=_read_stdout, daemon=True)
    reader.start()

    for msg in messages:
        proc.stdin.write(msg)
        proc.stdin.flush()

    found.wait(timeout=TIMEOUT_S)
    proc.terminate()

    stderr_output = proc.stderr.read().strip()
    if stderr_output:
        for line in stderr_output.splitlines():
            print(f"  [server] {line}", file=sys.stderr)

    return result


def main() -> int:
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <path-to-MunControlProtocol.MCP.exe>", file=sys.stderr)
        return 1

    exe = sys.argv[1]
    print(f"Launching: {exe}")

    tools = discover_tools(exe)

    if tools is None:
        print("ERROR: MCP server did not respond to tools/list within "
              f"{TIMEOUT_S}s.", file=sys.stderr)
        return 1

    tool_set = set(tools)
    expected_set = set(EXPECTED_TOOLS)

    missing    = sorted(expected_set - tool_set)
    unexpected = sorted(tool_set - expected_set)

    print(f"\nDiscovered {len(tools)} tool(s):")
    for name in sorted(tools):
        tag = " [UNEXPECTED]" if name in unexpected else ""
        print(f"  {name}{tag}")

    if missing:
        print(f"\nERROR: {len(missing)} expected tool(s) not registered:")
        for name in missing:
            print(f"  {name}")
        return 1

    if unexpected:
        print(f"\nWARNING: {len(unexpected)} unexpected tool(s) found "
              "(update EXPECTED_TOOLS in this script if intentional):")
        for name in unexpected:
            print(f"  {name}")

    print(f"\nAll {len(EXPECTED_TOOLS)} expected tools present.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
