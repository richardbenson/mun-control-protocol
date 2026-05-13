# Mun Control Protocol

A C# mod and companion MCP server that gives your AI assistant live read access to your Kerbal Space Program 1 career save — so you can ask Claude things like _"with my current tech and funds, what's the best Mun lander I can build?"_ and get a grounded, data-driven answer rather than a hallucinated one.

Supports **KSP 1.12.x**. KSP2 is not supported.

**v0.1 — first usable release** · Windows · Claude Desktop (and any MCP-compatible client)

---

## The 17 tools

### Live career data (requires KSP running)

| Tool                      | What it returns                                                                                                  |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `get_career_state`        | Funds, science points, and reputation                                                                            |
| `get_tech_tree`           | All tech nodes with unlock status and the parts they gate                                                        |
| `get_parts_by_category`   | Parts catalog filtered by category (engine, fuel tank, …)                                                        |
| `get_part_stats`          | Part metadata + module-specific stats (engine, antenna, tank, command pod, solar panel)                          |
| `get_science_status`      | All science subjects with completion state and diminishing-return multipliers                                    |
| `get_vessels`             | Active flights with orbital data and crew manifest                                                               |
| `get_building_levels`     | KSC facility upgrade levels (VAB, Runway, Tracking Station, …)                                                   |
| `get_kerbals`             | Full Kerbal roster — name, type, experience, assignment status                                                   |
| `get_body_info`           | Celestial body data: mass, radius, atmosphere height, SOI, parent body, orbital period, sidereal rotation period |
| `get_difficulty_settings` | Career modifiers: reward multipliers, CommNet config, reentry heat, respawn                                      |

### Formula tools (no KSP connection needed)

These tools run pure maths — useful for planning missions before launch.

| Tool                          | What it calculates                                                                                   |
| ----------------------------- | ---------------------------------------------------------------------------------------------------- |
| `calculate_delta_v`           | Tsiolkovsky rocket equation: ΔV = Isp × g₀ × ln(m_wet / m_dry)                                       |
| `calculate_orbital_velocity`  | Circular orbital speed at a given altitude around a body                                             |
| `calculate_orbital_period`    | Orbital period of a circular orbit at a given altitude                                               |
| `calculate_hohmann_transfer`  | Both burn ΔVs, total ΔV, and coast time for a Hohmann transfer between two circular orbits           |
| `calculate_escape_velocity`   | Escape velocity at a given altitude (defaults to surface)                                            |
| `calculate_synchronous_orbit` | Altitude and velocity of a stationary orbit; returns null altitude if the orbit is below the surface |
| `calculate_commnet_range`     | Maximum CommNet link range between two antennas using KSP's geometric-mean formula                   |

---

## Quick start

See [INSTALL.md](INSTALL.md).

---

## Y tho?

Whilst there is an almost infinite number of answers to questions for KSP out there, I personally found that many of them assumed more knowledge of the game than I had or different sets of parts than I have. I also didn't want any "just build this" kind of answers because I want to learn. Asking Claude directly produced hallucinations (especially in formulas and parts details) and again assumed I could use parts I haven't yet unlocked. So having Claude (or your chose AI tool) being able to read information straight from the game meant I could ask questions and get answers applicable to my exact situation, including science, funds and unlocks. Adding common formula to the tools also means these are calculated in code and not in the LLM, again reducing hallucinations.

As a noob, I also didn't want to ask stupid questions on forums or Reddit which have been answered 100 times before but also in slightly different situations and with answers I don't understand (yet).

---

## Example queries

- _"What's my current funds, science, and reputation?"_
- _"Which tech nodes should I unlock next to reach Duna?"_
- _"What science experiments haven't I done at Kerbin yet?"_
- _"Design a comms constellation for Duna coverage."_
- _"What's the best lander I can build with my current tech and funds?"_
- _"How much ΔV do I need to transfer from LKO to a 200 km orbit?"_
- _"What's the synchronous orbit altitude for Duna, and will my antenna reach it?"_

---

## Real example question and response

[E2E test conversation](docs/mun-control-protocol-e2e-test.pdf)

> I want to get a satellite into a synchronous orbit around Kerbin. Based on my current tech tree and available parts, what engines and fuel tanks should I consider, and roughly how much delta-v will I need for a launch to orbit plus the transfer?

Let me compute the exact transfer Δv first.

**Δv budget**

| Phase                                   | Δv             |
| --------------------------------------- | -------------- |
| Launchpad → LKO (~80 km)                | ~3,400 m/s     |
| Hohmann burn 1 (LKO → transfer ellipse) | 668 m/s        |
| Hohmann burn 2 (circularize at keo)     | 431 m/s        |
| Inclination cleanup + margin            | ~200 m/s       |
| **Total**                               | **~4,700 m/s** |

Transfer coast time: ~5,000 s (~83 min).

**Engine recommendations from your unlocked parts**

**Booster stage (atmospheric ascent)** — want high thrust, decent sea-level Isp:

- **LV-T45 "Swivel"** (215 kN, 320/250 s, _gimbaled_) — your steering core. The gimbal matters because none of your pods have built-in SAS.
- Pair with **2× RT-10 "Hammers"** or **2× BACC "Thumpers"** as radial SRBs for TWR off the pad. Hammers are cheap and respect the 18 t launchpad limit.
- If you want a 2.5 m core instead: **RE-I5 "Skipper"** (650 kN, 320/280 s) — but that pushes you near the 18 t launchpad cap quickly.

**Upper stage / transfer stage (vacuum)** — want high Isp, low mass:

- **LV-909 "Terrier"** (60 kN, **345 s vac**, only 0.5 t) — ideal for a light comsat. 60 kN is plenty to push a ~2 t upper stage.
- **RE-L10 "Poodle"** (250 kN, **350 s vac**) — best Isp you have, but at 1.75 t it's overkill for a tiny satellite.

**Fuel tank recommendations**

- Upper/transfer stage: **FL-T200** (1.13 t wet, 200 units) with a Terrier gives ~2,000 m/s Δv on a ~0.5 t payload — comfortable margin for the 1,099 m/s transfer.
- Booster stage: **FL-T800** (4.5 t wet) under a Swivel works for the lift portion, or use **X200-8** on a 2.5 m core if you go Skipper.

**Sample two-stage design** (rough mass budget, well under 18 t):

```
Payload     : Stayputnik + battery + HG-5 antenna + advSAS module    ~0.4 t
Transfer    : LV-909 Terrier + FL-T200                                ~1.7 t
Booster     : LV-T45 Swivel + FL-T800 + 2× Hammer SRBs                ~12 t
Total                                                                 ~14 t
```

**Practical considerations from your career state**

- **Antenna**: Use the **HG-5 (5 Mm range)** from Basic Science — combined with DSN level 1 (Tracking Station t1, 2 Gm), gives you ~100,000 km combined range. Plenty for a 3,463 km keo sat. The internal 5 km probe antenna **won't work** at that altitude.
- **No solar panels yet** — Electrics (90 sci) is locked. A pure battery-powered comsat will drain over time. Two workarounds:
  1. Add several battery packs (cheap fix, finite lifetime)
  2. Send a Kerbal pilot in a Mk1 pod instead of Stayputnik (50 EC + crew SAS, no ongoing draw beyond probe core baseline)
- **No maneuver nodes** (Tracking Station t1) — you'll have to time burns by eye against orbital periods. Keostationary is particularly unforgiving on circularization timing.
- **Launchpad 18 t limit** — keeps you in 1.25 m stack territory; the design above fits comfortably.

**Recommendation**

Before launching, consider upgrading **Tracking Station to t2 (38k funds)** — maneuver nodes will save you 100+ m/s in sloppy circularization. You have ~504k funds, so the upgrade is affordable. Without it, plan an extra ~200 m/s of margin (already included above).

---

## Architecture

The MCP server runs as a local console exe. Claude Desktop talks to it over stdio (MCP protocol). The exe talks to KSP over TCP using the kRPC mod.

```
Claude Desktop  ─MCP/stdio─▶  MunControlProtocol.MCP.exe  ─kRPC/TCP─▶  KSP + kRPC mod + MunControlProtocol.Career.dll
```

Three C# projects:

| Project                    | Target         | Purpose                                           |
| -------------------------- | -------------- | ------------------------------------------------- |
| `MunControlProtocol.Shared` | netstandard2.0 | DTOs shared by both sides                         |
| `MunControlProtocol.Career` | net472         | kRPC service extension, deployed into `GameData/` |
| `MunControlProtocol.MCP`    | net8.0         | Console exe registered in Claude Desktop config   |

The Career extension runs inside KSP and exposes career data over kRPC. The MCP server calls it using generated C# stubs (committed to source; regenerated when the Career service surface changes).

---

## Contributing

The implementation plan and per-phase design docs live in [docs/mun-control-protocol/](docs/mun-control-protocol/). Dev loop: edit → `deploy/build-and-deploy.ps1` → relaunch KSP → test.

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
