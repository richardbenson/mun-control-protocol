# Changelog

## v0.1 — 2026-05-13

First public release of Mun Control Protocol.

### Live career data tools

These tools require KSP to be running with the kRPC server online.

- **`get_career_state`** — current funds, science points, and reputation
- **`get_tech_tree`** — all tech nodes with unlock status and the parts they gate
- **`get_parts_by_category`** — parts catalog filtered by type (engine, fuel tank, command pod, antenna, battery, solar panel, etc.)
- **`get_part_stats`** — detailed stats for a named part including module-specific data: engine thrust and ISP, antenna range, tank and battery capacity, command pod crew count and SAS level, solar panel charge rate
- **`get_science_status`** — all science subjects with completion state and diminishing-return multipliers
- **`get_vessels`** — active flights with orbital elements (AP, PE, inclination, period) and crew manifest
- **`get_building_levels`** — KSC facility upgrade levels (VAB, SPH, Runway, Launchpad, Tracking Station, R&D, Mission Control, Astronaut Complex)
- **`get_kerbals`** — full Kerbal roster with name, type (Pilot/Engineer/Scientist), experience level, and assignment status
- **`get_body_info`** — celestial body properties: mass, radius, atmosphere height, SOI, gravitational parameter, parent body, orbital period, and sidereal rotation period
- **`get_difficulty_settings`** — career reward/penalty multipliers, CommNet configuration, re-entry heat, and respawn settings

### Formula tools

These tools perform orbital mechanics calculations and work without a KSP connection.

- **`calculate_delta_v`** — Tsiolkovsky rocket equation: ΔV = Isp × g₀ × ln(m_wet / m_dry)
- **`calculate_orbital_velocity`** — circular orbital speed at a given altitude
- **`calculate_orbital_period`** — period of a circular orbit at a given altitude
- **`calculate_hohmann_transfer`** — both burn ΔVs, total ΔV, and coast time for a two-burn transfer
- **`calculate_escape_velocity`** — escape velocity from a body at a given altitude
- **`calculate_synchronous_orbit`** — altitude and velocity of a stationary orbit
- **`calculate_commnet_range`** — maximum CommNet link range between two antennas using KSP's geometric-mean formula

### Installation

See [INSTALL.md](INSTALL.md) for full instructions.

**Requirements:** KSP 1.12.x, kRPC mod, Claude Desktop. No additional runtimes required — the MCP server is a self-contained Windows executable.
