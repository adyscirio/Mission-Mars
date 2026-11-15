# Mission-Mars — 8-Sol Martian Habitat Survival Simulator

**Mission-Mars** is an interactive 3D planetary survival and resource-management simulator built in **Unity 6 (C#)**. Designed for the **NASA Space Apps Challenge**, the simulation places the player in command of a Martian surface outpost over an **8-Sol** mission window, combining true Martian gravity ($g = -3.72\text{ m/s}^2$), dynamic environmental hazards, and real planetary science insights.

---

## Key Features

* **Authentic Martian Physics:** Character locomotion and jump trajectories calibrated to Martian surface gravity ($3.72\text{ m/s}^2$, $\sim 38\%$ of Earth's gravity).
* **8-Sol Dynamic Event Campaign:** Eight sequential Sol-specific operational crises driven by a modular `ScriptableObject` architecture (`MarsEventData`), covering solar radiation storms, dust storms, hydroponic crop stress, water ice extraction, and structural dome integrity.
* **Physical-Spatial 3D Gameplay:** Players must physically navigate the Martian outpost (`Astronauta_Caminando`) from the exterior airlock (`PuntoSpawnAfuera`) to inspect and interact (`[E]`) with physical base modules:
  * **Geodesic Habitat Dome (`Cupula`)**
  * **Dual Photovoltaic Solar Arrays (`Panel solar`)**
  * **Hydroponic Plantation (`PLANTACION`)**
  * **Water Extraction Units (`agua`)**
  * **Field Operations Terminal (`TerminalDeCampo`)**
* **Real-Time Environmental & Shield VFX:** Dynamic directional sunlight transitions and a procedural cyan electromagnetic dome shield (`SolarStormEffect.cs`) during solar energetic particle events, paired with volumetric Martian dust storm effects (`TormentaArena`).
* **Closed-Loop Resource Telemetry:** Live HUD tracking **Sol Progress (`Sol X / 8`)**, **Energy**, **Life Support**, and **Shield Integrity**, delivering verified NASA planetary **Science Facts** after every resolved event.

---

## NASA Space Apps Evaluation Alignment

| Criterion | Implementation in Mission-Mars |
| :--- | :--- |
| **Impact** | Transforms abstract planetary engineering constraints into hands-on STEM decision-making, illustrating cascading dependencies between power, life support, and radiation shielding. |
| **Creativity** | Replaces static menu management with 3D spatial exploration, real-time atmospheric lighting shifts, geodesic energy shield VFX, and daily EVA exterior resets (`PlayerDailyReset.cs`). |
| **Validity** | Implements accurate Martian gravity ($-3.72\text{ m/s}^2$), Martian solar days (Sols), baseline ECLSS resource decay, and documented NASA Mars Exploration Program science facts. |
| **Relevance** | Directly addresses NASA's Moon-to-Mars habitation priorities: surface power generation, radiation protection, closed-loop life support, and crew autonomy. |
| **Presentation** | Clean English telemetry HUD, non-overlapping mission outcome states (`MISSION COMPLETE` / `MISSION FAILED`), and single-click `Restart Mission` loop. |

---

## Project Architecture

```text
Assets/
└── _Project/
    ├── Events/                  # ScriptableObject assets for Sol 1–8 scenarios
    └── Scripts/
        ├── ResourceManager.cs   # 8-Sol loop, daily resource decay, win/loss evaluation
        ├── MarsEventManager.cs  # Event triggers, 3D target navigation & [E] interactions
        ├── SolarStormEffect.cs  # Solar storm lighting & geodesic shield VFX controller
        ├── PlayerDailyReset.cs  # Repositions astronaut outside dome at each new Sol
        └── GameUIController.cs  # Telemetry HUD, event panels & mission restart logic