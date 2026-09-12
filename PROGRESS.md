# BeastClad: Project Progress & Roadmap Tracker

**Project:** BeastClad  
**Lead Technical Producer Status Dashboard**  
**Engineering Philosophy:** KISS (Keep It Simple, Stupid) & YAGNI (You Aren't Gonna Need It)  
**Last Updated:** 2026-09-09  

---

## Current Status

| Metric | Current State |
| :--- | :--- |
| **Active Phase** | **Phase 5: World Integration, Persistence & Roster Management** |
| **Current Focus** | Completed Phase 5.1 (Ready for Phase 5.2: Save & Load System) |
| **Active Blockers** | None |
| **Target Build Target** | PC Standalone / Unity Editor Test Arena |

> [!NOTE]
> **Production Rule:** We do not move to the next phase until the current phase's verification criteria are fully satisfied and demonstrated in the test scene. Avoid premature abstraction, deep inheritance trees, and out-of-scope systems (no full inventory, no persistence, no procedural maps in Phase 1).

---

## Development Phases

### Phase 1: Core Loop Prototype (The Sandbox Test)
*Goal: Prove the core mechanical hook ("Infuse" equipping + local cooldowns + terraforming reaction) in an isolated, minimal test arena.*

- [x] **1.1 2D Top-Down Player Controller (New Input System)**
  - [x] Configure `Input System Package (New)` action map (`Move`, `LeftArmAttack`, `RightArmAttack`, `ChestSkill`, `HeadSkill`, `LegsDash`).
  - [x] Implement `PlayerMovement` (Rigidbody2D velocity, crisp directional top-down movement).
  - [x] Implement basic directional facing and placeholder sprite rendering.
- [x] **1.2 Foundational ScriptableObject Architecture**
  - [x] Create `ElementalTypeSO` (Hydro, Pyro, Volt, etc.) with chemistry metadata.
  - [x] Create `EquipmentSlot` enum (`Head`, `Chest`, `LeftArm`, `RightArm`, `Legs`).
  - [x] Create `MonsterDataSO` (Name, Elements, slot module references).
  - [x] Create `InfusePartSO` (Target slot, local cooldown duration, stat delta, active skill SO).
- [x] **1.3 Real-Time Cooldown & Infuse Manager Logic**
  - [x] Implement `PlayerInfuseManager` (equips/unequips monsters across 5 sockets: Head, Chest, Left Arm, Right Arm, Legs).
  - [x] Implement `CooldownController` (tracks independent local limb cooldowns and a universal 0.5s GCD; strictly NO stamina, NO PP).
  - [x] Implement `LimbCooldownUI` using Unity UI `Image` with **Radial 360 Fill** method for real-time visual cooldown feedback.
  - [x] Create 3 test monsters:
    - **Torrent Wyrm** (Hydro - Left Arm: Water Cleave that leaves a water puddle).
    - **Volt Mantis** (Volt - Right Arm: Electric Stinger that electrifies puddles).
    - **Ironhide Boar** (Geo/Earth - Chest: Defense barrier shield, Legs: Kinetic Dash).
- [x] **1.4 Elemental Chemistry & Terraforming Resolver**
  - [x] Implement 2D Melee Hitbox and Hurtbox components.
  - [x] Implement `SurfaceHazard` entity (spawns persistent 2D Water Puddle on ground from Hydro attack).
  - [x] Implement Chemistry/Terraforming trigger: Striking a Water Puddle with a Volt attack turns it into an Electrified AoE Shock Zone (continuous damage/stun).
- [x] **1.5 Single Test Arena Room (`TestScene_Sandbox` / `SampleScene`)**
  - [x] Build minimalist enclosed boundary arena with test dummies.
  - [x] Add simple debug trigger pads / keybinds to equip the test monsters into sockets.
  - [x] Verify: Rotate through Left Arm (Water), Right Arm (Electric), Chest (Barrier), and Legs (Dash); observe Radial 360 Fill UI timers and 0.5s GCD; verify puddle terraforming and electric shock reaction.
- [x] *(Strict Constraint Check)*: Confirmed NO inventory grid, NO file save/load system, and NO large outdoor world code introduced.

---

### Phase 2: The Adventurer & Trapping Module
*Goal: Implement non-combat acquisition of wild monsters and basic contract flow.*

- [x] **2.1 Basic Grid/Tilemap Outdoor Environment**
  - [x] Set up Unity 2D Tilemap grid (Grass, Dirt, Obstacles) for a compact wilderness zone.
  - [x] Implement simple 2D collision layer setup (Player, WildMonsters, Traps, Obstacles).
- [x] **2.2 Trapping & Lure System (Non-Combat Acquisition)**
  - [x] Create `LureDataSO` and placeable lure prefab (attracts specific monster types via simple path/steering).
  - [x] Create `TrapDataSO` and deployable snare/cage prefab (detects beast overlap, applies immobilize).
  - [x] Implement Subdual/Capture interaction (weakening or baiting beast into trap yields a `MonsterInstance`).
- [x] **2.3 Basic Quest & Guild Dialogue Flow**
  - [x] Create simple decoupled dialogue/interaction prompt trigger (`InteractableNPC`).
  - [x] Create basic Guild Contract data structure (`CaptureTargetSpecimen`, `TurnInReward`).
  - [x] Implement contract completion check (holding the requested captured monster fulfills contract).

---

### Phase 3: The Arena & Legal Progression
*Goal: Implement sanctioned tournament bouts and legal commercial acquisition.*

- [x] **3.1 Arena Tournament Loop**
  - [x] Create structured Arena Room with wave/bout state controller (`PreMatch`, `ActiveBout`, `Victory`, `Defeat`).
  - [x] Implement sanctioned opponent AI (human gladiator infused with corporate monster gear).
  - [x] Add basic match outcome triggers and division ladder rank increment.
- [x] **3.2 Economy & Corporate "Starter Pack" Purchasing**
  - [x] Implement simple currency counter (`Credits`).
  - [x] Build minimal kiosk/vendor UI to purchase pre-packaged corporate "Starter Pack" beasts.
  - [x] Implement automatic State Registration tag flag (`IsRegistered = true`) upon purchase.

---

### Phase 4: The Underground & Black Market
*Goal: Implement state enforcement, unregistered contraband monsters, and high-risk underground systems.*

- [x] **4.1 State Registration & Scanner Logic**
  - [x] Attach `RegistrationStatus` (`Legal`, `Unregistered`, `Contraband`) to monster data/instances.
  - [x] Create Municipal Scanner trigger zone (detects unregistered monsters in player loadout).
  - [x] Implement Enforcer alert/heat reaction when illegal monsters are detected in civil zones.
- [x] **4.2 Underground Black Market & Ripperdoc**
  - [x] Build subterranean hub scene/room with illegal merchants (`Underground_BlackMarket.unity`).
  - [x] Create Ripperdoc vendor logic: Sell volatile unregistered beasts with overclocked stats (+Attack, but self-damage or instability).
  - [x] Implement basic underground wager/betting interaction before unsanctioned pit matches (100/250/500 CR, 2.5x payout).

---

### Phase 5: World Integration, Persistence & Roster Management
*Goal: Connect the three independent gameplay paths into a cohesive sandbox world loop, implement session persistence across scenes, and build a full in-game roster and modular infuse UI.*

- [x] **5.1 Unified Overworld Hub & Scene Transition System**
  - [x] Build a central District Transit Hub connecting the 3 gameplay sectors (`Adventurer_Wilderness`, `Arena_Colosseum`, `Underground_BlackMarket`).
  - [x] Implement robust 2D Scene Transition triggers with target spawn point positioning and transition fade/loading overlay.
  - [x] Preserve player character data, equipped modules, and wallet credits across scene changes without state desync.
  - [x] Integrate municipal security checkpoint scanner gates at entrances between illegal/wild zones and civil/arena districts.
  - [x] Implement Municipal Security Quarantine Locker and Quick-Disarm (`[E]` / `[F]`) at checkpoint to safely unequip and vault contraband before entering tournament arena.

- [ ] **5.2 Persistent Save & Load System (JSON)**
  - [ ] Design robust, serializable Save Data contracts (`PlayerSaveData`: Credits, Roster Monsters, Equipped Slots, Arena Rank, Story Flags).
  - [ ] Implement atomic file I/O `SaveManager` service writing to `Application.persistentDataPath/savegame.json`.
  - [ ] Implement auto-save triggers on scene transitions, kiosk purchases, capture events, and match conclusions.
  - [ ] Provide load-game initialization on startup with graceful fallback to fresh defaults if no save file is detected.

- [ ] **5.3 Dedicated Roster & Infuse Management UI**
  - [ ] Build full-screen / modal Roster & Infuse management window toggled via `[Tab]` or `[I]`.
  - [ ] Display captured/purchased monster inventory cards with registration status tags (`Legal`, `Unregistered`, `Contraband`), stability meters, and stat deltas.
  - [ ] Implement drag-and-drop or click-to-equip assignment into the 5 anatomical sockets (Head, Chest, Left Arm, Right Arm, Legs).
  - [ ] Connect equip actions to `PlayerInfuseManager` with real-time stat recalculations and HUD radial cooldown refresh.

---

## Known Bugs & Edge Cases

| ID | Phase | Description | Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| *None logged yet* | — | All reported underground issues resolved and verified. | Resolved | Fixed collider scales, brawler radius, HUD canvas, and hub combat gating. |

---

## Backlog (Deferred for Scope Management)

*These features are strictly parked to prevent scope creep until Phase 1–5 are fully functional and verified.*

- [ ] Genetic splicing / Cross-breed monster fusion crafting.
- [ ] Multi-district sprawling open sandbox world (procedural generation or additional biomes).
- [ ] Complex dialogue trees and branching narrative cutscenes.
- [ ] Online leaderboards or multiplayer arena duels.
- [ ] Advanced dynamic lighting normal maps and post-processing polish passes.

---

## Protocol for Updating This Document

1. **Marking Progress:** As soon as an item is implemented and verified in the editor, change `- [ ]` to `- [x]`.
2. **Current Status Update:** Update the **Current Status** table at the top of the file to reflect the active sprint, current task, and any blockers.
3. **Bug Tracking:** If an edge case or regression occurs during development, add it to the **Known Bugs** table immediately before writing new features.
4. **Scope Discipline:** If a feature idea arises during development that is not essential to the current phase, place it under **Backlog**. Do not build it prematurely.
