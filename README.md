# KSP Mission Control

A C# mod and companion MCP server that gives your AI assistant live read access to your Kerbal Space Program 1 career save — so you can ask Claude things like *"with my current tech and funds, what's the best Mun lander I can build?"* and get a grounded, data-driven answer rather than a hallucinated one.

Supports **KSP 1.12.x**. KSP2 is not supported.

**v0.1 — first usable release** · Windows · Claude Desktop (and any MCP-compatible client)

---

## The 17 tools

### Live career data (requires KSP running)

| Tool | What it returns |
|---|---|
| `get_career_state` | Funds, science points, and reputation |
| `get_tech_tree` | All tech nodes with unlock status and the parts they gate |
| `get_parts_by_category` | Parts catalog filtered by category (engine, fuel tank, …) |
| `get_part_stats` | Part metadata + module-specific stats (engine, antenna, tank, command pod, solar panel) |
| `get_science_status` | All science subjects with completion state and diminishing-return multipliers |
| `get_vessels` | Active flights with orbital data and crew manifest |
| `get_building_levels` | KSC facility upgrade levels (VAB, Runway, Tracking Station, …) |
| `get_kerbals` | Full Kerbal roster — name, type, experience, assignment status |
| `get_body_info` | Celestial body data: mass, radius, atmosphere height, SOI, parent body, orbital period, sidereal rotation period |
| `get_difficulty_settings` | Career modifiers: reward multipliers, CommNet config, reentry heat, respawn |

### Formula tools (no KSP connection needed)

These tools run pure maths — useful for planning missions before launch.

| Tool | What it calculates |
|---|---|
| `calculate_delta_v` | Tsiolkovsky rocket equation: ΔV = Isp × g₀ × ln(m_wet / m_dry) |
| `calculate_orbital_velocity` | Circular orbital speed at a given altitude around a body |
| `calculate_orbital_period` | Orbital period of a circular orbit at a given altitude |
| `calculate_hohmann_transfer` | Both burn ΔVs, total ΔV, and coast time for a Hohmann transfer between two circular orbits |
| `calculate_escape_velocity` | Escape velocity at a given altitude (defaults to surface) |
| `calculate_synchronous_orbit` | Altitude and velocity of a stationary orbit; returns null altitude if the orbit is below the surface |
| `calculate_commnet_range` | Maximum CommNet link range between two antennas using KSP's geometric-mean formula |

---

## Quick start

See [INSTALL.md](INSTALL.md).

---

## Example queries

- *"What's my current funds, science, and reputation?"*
- *"Which tech nodes should I unlock next to reach Duna?"*
- *"What science experiments haven't I done at Kerbin yet?"*
- *"Design a comms constellation for Duna coverage."*
- *"What's the best lander I can build with my current tech and funds?"*
- *"How much ΔV do I need to transfer from LKO to a 200 km orbit?"*
- *"What's the synchronous orbit altitude for Duna, and will my antenna reach it?"*

---

## Architecture

The MCP server runs as a local console exe. Claude Desktop talks to it over stdio (MCP protocol). The exe talks to KSP over TCP using the kRPC mod.

```
Claude Desktop  ─MCP/stdio─▶  KSPMissionControl.MCP.exe  ─kRPC/TCP─▶  KSP + kRPC mod + KSPMissionControl.Career.dll
```

Three C# projects:

| Project | Target | Purpose |
|---|---|---|
| `KSPMissionControl.Shared` | netstandard2.0 | DTOs shared by both sides |
| `KSPMissionControl.Career` | net472 | kRPC service extension, deployed into `GameData/` |
| `KSPMissionControl.MCP` | net8.0 | Console exe registered in Claude Desktop config |

The Career extension runs inside KSP and exposes career data over kRPC. The MCP server calls it using generated C# stubs (committed to source; regenerated when the Career service surface changes).

---

## Contributing

The implementation plan and per-phase design docs live in [docs/ksp-mission-control/](docs/ksp-mission-control/). Dev loop: edit → `deploy/build-and-deploy.ps1` → relaunch KSP → test.

---

## Future

- macOS and Linux builds of the MCP server
- GitHub Actions packaging (pending a way to mock kRPC in CI)
- VAB current-ship analysis
- Write-back tools (unlock tech, purchase parts) if kRPC ever supports it

---

## License

MIT — see [LICENSE](LICENSE).

Third-party notices (kRPC/LGPL-3.0, Google.Protobuf/BSD-3-Clause) — see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
