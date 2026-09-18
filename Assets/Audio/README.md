# BeastClad Audio Directory

This directory houses all sound design and musical score assets for BeastClad.

## Subdirectories:
- **`BGM/`**: Loopable district themes (High Spire corporate concourse, Primordial Outlands, Colosseum battle fanfare, Underground synth-grime). Format: `.ogg` or `.mp3`.
- **`SFX/`**: Low-latency one-shot sound effects. Format: uncompressed `.wav` (44.1 kHz, 16-bit).
  - `Combat/`: Melee swings, hurt grunts, elemental reactions, shield impacts, dash slipstreams.
  - `Infuse/`: Mechanical latch clicks, biological bone bonding, local cooldown ready chimes.
  - `Security/`: Municipal gate scans, contraband alarms, quick-disarm locker clicks.
  - `UI/`: Button hovers, inventory tab swaps, kiosk transactions.
- **`Ambience/`**: Environmental background noise (subterranean pipe dripping, arena crowd buzz, woodland wind).

The active audio bus is managed via [`AudioManager.cs`](file:///c:/Users/shnlr/Deneysel/Assets/Scripts/Audio/AudioManager.cs).
