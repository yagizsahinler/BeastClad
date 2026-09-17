# BeastClad

> *"Monsters are not your companions or pets — they are your armor, your weapons, and your leverage in a broken world."*

**BeastClad** is an experimental, passion-driven **2D Top-Down Sandbox Action-RPG** built with **Unity 6**. 

In a corporatized dystopia where magical beasts are treated as elite-monopolized bio-resources, you do not form sentimental bonds with creatures to fight turn-based stadium bouts. Instead, you **infuse** captured monsters directly into your anatomy across 5 modular equipment sockets—channeling their physical forms, elemental affinities, and combat skills into a customized, real-time combat build.

---

## 🌟 The Vision & Project Nature

> [!NOTE]
> **An Amateur Indie Project with Heart:**  
> *BeastClad* is an independent, amateur solo development project. It was born out of a desire to rethink the traditional monster-tamer formula by replacing autonomous pet AI and vertical stat grinding with **real-time skill expression, modular build synergy, and non-linear sandbox roleplay**. 
> 
> Because this is a lean solo endeavor, development is guided by strict **KISS (Keep It Simple, Stupid)** and **YAGNI (You Aren't Gonna Need It)** engineering principles—focusing on tight, tactile gameplay feel, clean modular code, and verifiable milestones rather than bloated feature scope.

---

## ⚔️ Core Gameplay Mechanics

### 1. The "Infuse" System (Biological Weaponization)
Monsters never fight as autonomous pawns on the battlefield. Captured beasts are infused into 5 distinct anatomical sockets:
- **Head Socket:** Boosts Max Hit Points (HP) and Vitality scaling; provides utility pulses and sensory cleanses (e.g. *Torrent Wyrm Sensory Crest*).
- **Chest Socket:** Fortifies Defense and Poise; deploys reactive armor barriers and energy shields (e.g. *Ironhide Boar Barrier*).
- **Left Arm & Right Arm (Dual Modular Attacks):** Boosts Attack Power; equips distinct weapons and elemental strikes (e.g. Left Arm = *Water Cleave*; Right Arm = *Electric Stinger*).
- **Legs Socket:** Boosts Movement Speed and Acceleration; dictates evasive dash maneuvers (e.g. *Volt Mantis Kinetic Dash*).

### 2. Real-Time Action with Cooldowns (No Stamina, No PP)
- **Zero Stamina Punishment:** No stamina bars, sluggish exhaustion, or movement lockouts.
- **Independent Limb Cooldowns:** Every equipped monster part recharges on its own local timer, encouraging continuous rotation between arms, defensive barriers, and mobility skills.
- **Universal 0.5s Global Cooldown (GCD):** Enforces a deliberate, rhythmic combat tempo and prevents mindless button-mashing.
- **Radial 360 Fill HUD:** Clean visual cooldown widgets providing instant, readable feedback.

### 3. Elemental Chemistry & Battlefield Terraforming
Instead of flat numerical type-matchup multipliers, combat features organic physical reactions:
- **Terrain Alteration:** Water strikes deposit persistent 2D water puddles across the floor.
- **Chain Reactions:** Striking standing water with an Electric arm-infuse electrifies the entire puddle surface into an **Electrified AoE Shock Zone**, continuously staggering and shocking all targets caught inside.

### 4. Non-Linear Sandbox Paths
The world offers multiple divergent ways to survive:
- **The Legal Arena:** Sponsored district circuits, corporate scrutiny, official bout registration (Proctor Cassian), division ladder rank promotions (Bronze $\rightarrow$ Silver $\rightarrow$ Apex), and prize purses.
- **The Adventurer's Guild:** Quarantined wilderness tracking, tactical pheromone lures, deployable wire snares, live subdual captures, and research specimen bounties (Guildmaster Vane).
- **The Illegal Underground:** Back-alley ripperdocs (Dr. Silas), volatile unregistered beasts with overclocked traits and true-damage recoil, unsanctioned pit match wagers (Pitmaster Jax), and municipal enforcer heat evasion.

### 5. World Transit, Persistence & Dynamic HUD
- **Central District Transit Hub:** Connects all sectors with seamless transitions, target spawn anchoring, and municipal security checkpoints with Quarantine Contraband Lockers.
- **Full JSON Persistence:** Automatic save/load pipeline (`SaveManager`) preserving credits, equipped limb loadouts, captured roster, and arena standing across scene transitions and sessions.
- **Dynamic Reactive HUD:** Context-aware combat health bar with auto-fading, 3-tier health thresholds, and trailing damage lag visualization.
- **Complete Menu Suite:** Dedicated startup `MainMenu` with campaign resume, settings, and bio-infuse lore codex, alongside a universal in-game `PauseMenuUI` accessible via `[Esc]`.

---

## 🎮 Controls (New Input System)

| Action | Keyboard / Mouse | Gamepad |
| :--- | :--- | :--- |
| **8-Way Movement** | `W` `A` `S` `D` / Arrow Keys | Left Analog Stick |
| **Left Arm Strike** | `Left Mouse Button` | `West Button` (X / □) |
| **Right Arm Strike** | `Right Mouse Button` | `North Button` (Y / △) |
| **Chest Skill / Shield** | `Spacebar` | `South Button` (A / ✕) or `LB` |
| **Head Utility Pulse** | `Q` | `East Button` (B / ○) |
| **Legs Dash** | `Left Shift` | `Right Trigger` (RT / R2) |
| **Deploy Pheromone Lure** | `[ 1 ]` | — |
| **Deploy Wire Snare Trap** | `[ 2 ]` | — |
| **Subdue Prey / Disarm Contraband** | `[ E ]` | — |
| **Interact / Talk to NPC** | `[ F ]` | — |
| **Pause Menu / Dismiss Active Modals** | `[ Escape ]` | `Start / Options` |

---

## 🛠️ Tech Stack & Architecture

- **Engine:** Unity 6 (`6000.6.0f1`)
- **Rendering:** Universal Render Pipeline (URP 2D) with 2D Lighting
- **Input:** Unity Input System Package (New) (`com.unity.inputsystem`)
- **Architecture Highlights:**
  - **Data-Driven Design:** Monster stats, elements, cooldowns, and skills are strictly driven by `ScriptableObject` assets (`MonsterDataSO`, `InfusePartSO`, `ElementalTypeSO`, `TrapDataSO`, `LureDataSO`, `GuildContractSO`).
  - **Decoupled Event Architecture:** UI, physics, combat pipelines, and AI interact through C# events without tight coupling.
  - **Clean Separation of Definition & State:** Immutable ScriptableObject templates are separated from runtime `MonsterInstance` capture records.

---

## 📂 Playable Scenes

| Scene Path | Description |
| :--- | :--- |
| `Assets/Scenes/MainMenu.unity` | **Startup Main Menu:** Campaign management (Continue / New Game with overwrite prompt), master audio & keybinding controls cheatsheet, and Bio-Infuse world lore codex. Registered as build index 0. |
| `Assets/Scenes/District_CentralHub.unity` | **Central District Transit Hub:** Overworld nexus connecting all sectors; features municipal security scanner gates, Quarantine Contraband Lockers, and multi-directional district transit gates. |
| `Assets/Scenes/Arena_Colosseum.unity` | **Colosseum Tournament Arena:** Sanctioned tournament ring with Proctor Cassian bout registration, match countdown, gladiator opponent AI (*Valerius the Shock-Lancer*), dynamic ring barrier, corporate kiosk, and post-match victory/exit flows. |
| `Assets/Scenes/Adventurer_Wilderness.unity` | **Wilderness Clearing & Trapping:** $32 \times 24$ woodland tilemap with roaming wild beasts, pheromone lures, wire snares, subdual captures, and Guildmaster Vane contract turn-in. |
| `Assets/Scenes/Underground_BlackMarket.unity` | **The Underground Black Market:** Contraband district featuring Dr. Silas's illegal Ripperdoc augmentations (overclocked beasts), Pitmaster Jax's unsanctioned pit match wagers, brawler combat, and municipal scanner checkpoints. |
| `Assets/Scenes/SampleScene.unity` | **Combat & Chemistry Sandbox:** Enclosed test arena with training dummies, pedestal equipment stations, and water/electric terraforming reactions. |

---

## 🗺️ Roadmap & Current Status

Current milestone progress is tracked in detail in [`PROGRESS.md`](PROGRESS.md):

- [x] **Phase 1: Core Loop Prototype** (Infuse Equipping, Cooldowns, GCD, Terraforming Reaction)
- [x] **Phase 2: The Adventurer & Trapping Module** (Tilemap Wilderness, Scent Lures, Wire Snares, Live Capture, Guild Contracts)
- [x] **Phase 3: The Arena & Legal Progression** (Gladiator Tournament Loop, Division Ladder, Corporate Starter Pack Kiosks)
- [x] **Phase 4: The Underground & Black Market** (State Registration Scanners, Ripperdoc Overclocks, Unsanctioned Pit Wagers)
- [x] **Phase 5.1 & 5.2: World Integration, Persistence & Polish** (Central Transit Hub, Scene Transitions, Quarantine Locker, JSON Save/Load, Dynamic Health HUD, Main Menu & Pause Menu, Arena Flow Gating)
- [ ] **Phase 5.3: Dedicated Roster & Infuse Management UI** (Full-screen roster modal, socket drag-and-drop, real-time stat delta breakdown)

For the complete design philosophy, mechanical specifications, and world lore, see [`CORE_GDD.md`](CORE_GDD.md).

---

## 📄 License

This project is an amateur personal development prototype. All rights to original game design concepts, lore, and custom source code are reserved. Built with Unity.
