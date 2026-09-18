# BeastClad: Technical Asset Creation & Import Guidelines

**Project:** BeastClad  
**Version:** 1.0.0  
**Target Engine:** Unity 6 (Universal Render Pipeline 2D)  
**Lead Architectural Directive:** Strictly adhere to these metric and formatting rules to ensure all visual, animation, and audio assets integrate seamlessly with BeastClad's modular "Infuse" paperdoll and dynamic 2D Y-sorting systems.

---

## 1. Pixel Grid & Unit Scale Standard

| Asset Category | Pixel Dimensions | Pixels Per Unit (PPU) | Native World Size |
| :--- | :--- | :--- | :--- |
| **Ground / Floor Tiles** | $32 \times 32$ px | **32 PPU** | $1.0 \times 1.0$ meters |
| **Player Chassis** | $48 \times 48$ px | **32 PPU** | $1.5 \times 1.5$ meters |
| **Infuse Armor Overlays** | $48 \times 48$ px (Matching Canvas) | **32 PPU** | $1.5 \times 1.5$ meters |
| **Small / Swift Beasts** (e.g. Gale Hare) | $32 \times 32$ px | **32 PPU** | $1.0 \times 1.0$ meters |
| **Standard Monsters** (e.g. Torrent Wyrm) | $48 \times 48$ px | **32 PPU** | $1.5 \times 1.5$ meters |
| **Heavy / Apex Beasts** (e.g. Ironhide Boar) | $64 \times 64$ to $96 \times 96$ px | **32 PPU** | $2.0$ to $3.0$ meters |
| **Melee VFX / Slash Trails** | $64 \times 64$ px | **32 PPU** | $2.0 \times 2.0$ meters |
| **UI Inventory / Catalog Icons** | $64 \times 64$ px (or $128 \times 128$) | N/A (Canvas Scaler) | UI World / Screen Space |

> [!IMPORTANT]
> **Strict Metric Rule:** All world and character sprites **MUST** be authored at **32 PPU**. Never author world sprites at arbitrary resolutions. Mixing PPUs causes colliders to detach from visual graphics and results in pixel shimmering.

---

## 2. Pivot Point & Anchoring Standards

Consistent pivot placement is essential for the modular paperdoll system and URP 2D vertical Y-axis depth sorting (`TransparencySortMode: CustomAxis (0, 1, 0)`).

* **Player Base Chassis:**
  * **Pivot:** `Bottom Center [0.5, 0.0]` (located directly at the ground contact point between feet).
* **Infuse Armor Overlays (Head, Chest, Left Arm, Right Arm, Legs):**
  * **Pivot:** `Bottom Center [0.5, 0.0]`.
  * **Canvas Requirement:** Overlays **must be drawn on the identical $48 \times 48$ px canvas** positioned relative to the base chassis. This allows overlays to simply drop onto child sockets without needing manual pixel coordinate offsets in code.
* **Environment Obstacles & Props (Pillars, Trees, Crates, Railings):**
  * **Pivot:** `Bottom Center [0.5, 0.0]` at the base of the object where it touches the ground. This guarantees that moving entities correctly sort behind the prop when north of it and in front when south.
* **Surface Hazards (Puddles, Slicks, Shock Zones):**
  * **Pivot:** `Center [0.5, 0.5]`.
* **UI Icons & Projectiles:**
  * **Pivot:** `Center [0.5, 0.5]`.

---

## 3. Unity Texture Import Preset Specifications

When importing sprite textures into Unity, apply these exact settings (or apply the project preset):

* **Texture Type:** `Sprite (2D and UI)`
* **Sprite Mode:** `Multiple` (for spritesheets) or `Single`
* **Pixels Per Unit:** `32`
* **Mesh Type:** `Full Rect` (prevents tight polygon clipping on thin pixel details)
* **Extrude Edges:** `1`
* **Filter Mode:** `Point (no filter)` (Strictly mandatory for crisp pixel art)
* **Compression:** `None` (for PC Standalone; prevents color banding on elemental glows)
* **Read/Write:** Disabled (unless pixel analysis is explicitly required)

---

## 4. Sorting Layers Reference Table

Every visual renderer in the game must belong to one of these designated sorting layers. **Do NOT leave assets on `Default` with arbitrary sorting order numbers.**

| Sorting Layer Name | Usage Description | Typical Entities |
| :--- | :--- | :--- |
| `Background` | Distant backdrops, skyboxes, underground chasms | Void backgrounds |
| `Floor` | Base terrain tilemaps | Dirt, asphalt, grass, arena ring sand |
| `FloorHazards` | Persistent 2D terraforming hazards | Water puddles, electric shock zones, fire slicks |
| `Props_Low` | Flat ground objects beneath characters | Low carpets, sewer grates, chalk lane markings |
| `Entities` | Dynamic vertical Y-sorted characters & objects | Player base chassis, NPCs, wild beasts, gladiators |
| `EquippedArmor` | Modular infuse paperdoll overlays | Head, Chest, Left Arm, Right Arm, Legs overlays |
| `VFX_Under` | Ground visual effects beneath feet | Dash dust clouds, target reticles, summon circles |
| `Props_High` | Overhanging geometry above characters | Stadium archways, tree canopies, cage roofs |
| `VFX_Over` | High-priority combat visual effects | Melee slash trails, lightning bolts, sparks, hit bursts |
| `UI_World` | In-world user interface elements | Dynamic health bars, scanner reticles, interaction prompts |

---

## 5. Modular "Infuse" Paperdoll Directional Standard

Because BeastClad uses top-down 4-directional gameplay, Infuse limb overlays support directional variants:

* **`overlayFront`:** Character facing Down (facing camera).
* **`overlayBack`:** Character facing Up (facing away from camera).
* **`overlaySide`:** Character facing Horizontal. The engine automatically applies `SpriteRenderer.flipX` when the player turns Left vs Right.
* *Fallback:* If an asset only has a single neutral overlay, assign it to `visualOverlaySprite` on [`InfusePartSO`](file:///c:/Users/shnlr/Deneysel/Assets/Scripts/Data/InfusePartSO.cs), and the system will gracefully use it across all angles.

---

## 6. Elemental Palette Conventions

To maintain visual clarity during intense elemental combat and chemical reactions, use these reference hexadecimal color signatures:

* **Hydro (Water):** `#00BFFF` (Deep Electric Cyan), Highlight: `#E0FFFF`
* **Volt (Electric):** `#FFD700` (Amber Gold), Electric Core: `#FFFF80`
* **Pyro (Fire):** `#FF4500` (Molten Crimson), Inner Flame: `#FFA500`
* **Geo (Earth / Mineral):** `#8B4513` (Saddle Ochre), Armor Plate: `#A9A9A9`
* **Bio / Acid (Toxic):** `#32CD32` (Caustic Lime), Chemical Slime: `#8A2BE2`

---

## 7. Audio Asset Conventions

* **Format:** Uncompressed `.wav` for low-latency SFX and combat impacts; streaming `.ogg` or `.mp3` for BGM and district ambient loops.
* **Master Sample Rate:** 44,100 Hz, 16-bit.
* **Dynamic Range & Normalization:** SFX normalized to `-3 dBFS` peak.
* **Visceral Sound Palette:**
  * *Infuse Equipping:* Heavy mechanical latch engages followed by visceral biological pulse/snap.
  * *Cooldown Ready:* Subtle high-tech audio chime (prevents HUD gazing).
  * *Melee Impacts:* Satisfying fleshy thud backed by armor clatter.
  * *Illegal Bio-Overclock:* High-frequency bio-resonance whine or distorted metallic crunch.

---

## 8. Directory Structure Hierarchy

All incoming assets must be placed according to this directory blueprint:

```
Assets/
├── Art/
│   ├── ASSET_GUIDELINES.md
│   ├── Characters/
│   │   ├── Player/               # Base chassis spritesheets & animations
│   │   ├── InfuseOverlays/       # Head/, Chest/, LeftArm/, RightArm/, Legs/
│   │   ├── Monsters/             # Folders per species (e.g. TorrentWyrm/)
│   │   ├── Enemies/              # Gladiators, Enforcers, Brawlers
│   │   └── NPCs/                 # Cassian, Silas, Jax, Concourse Citizens
│   ├── Environment/
│   │   ├── CentralHub/           # Concourse tiles, kiosks, security gates
│   │   ├── Colosseum/            # Sand tiles, arena barriers, corporate banners
│   │   ├── Wilderness/           # Woodland grass, rocks, water pools, trees
│   │   └── Underground/          # Slum concrete, cage walls, neon signage
│   ├── VFX/                      # Slashes, water splashes, shock arcs, dust
│   └── UI/                       # Monster icons, contract parchment, HUD frames
└── Audio/
    ├── README.md
    ├── BGM/                      # District & battle music tracks
    ├── SFX/
    │   ├── Combat/               # Attacks, impacts, dashes, shields
    │   ├── Infuse/               # Socket snaps, unequip, ready chimes
    │   ├── Security/             # Scanners, alarm sirens, contraband alerts
    │   └── UI/                   # Clicks, menu whooshes, purchase registers
    └── Ambience/                 # Rain, crowd cheers, subway hums
```
