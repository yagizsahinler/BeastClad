# Lessons Learned & Architecture Patterns

## 1. Unity C# `UnityEngine.Object` and Null-Coalescing Operator (`??`)
- **Issue:** C# `??` checks for pure C# reference null. In Unity, destroyed or fake-null `UnityEngine.Object` instances (like `MarshalledUnityObject`) evaluate to `false` in Unity's overloaded `operator ==(Object, Object)` / `implicit bool`, but are NOT pure C# `null`. Using `go.GetComponent<T>() ?? go.AddComponent<T>()` can fail or throw `MissingComponentException`.
- **Pattern:** Always use explicit `if (comp == null) comp = go.AddComponent<T>();` for all `UnityEngine.Object` types.

## 2. Unity 6 API Deprecations
- **Velocity:** In Unity 6, `rb2d.velocity` is deprecated. Always use `rb2d.linearVelocity`.
- **Object Finding:** `FindObjectsByType<T>(FindObjectsSortMode)` is deprecated in Unity 6. Always use `FindObjectsByType<T>(FindObjectsInactive.Exclude)` or `FindObjectsByType<T>()`.
- **Singleton Finding:** `FindFirstObjectByType<T>` is deprecated; use `FindAnyObjectByType<T>()`.

## 3. Unity 2D Physics Triggers & Sync
- **Trigger Collision Requirement:** In Unity 2D, at least one of the colliding objects must have a `Rigidbody2D` (either Dynamic or Kinematic) for `OnTriggerEnter2D` to fire.
- **Position Sync:** When moving a `Rigidbody2D` directly, use `rb.position = ...` or call `Physics2D.SyncTransforms()` before simulating physics steps.

## 4. Input Action References & Resilient Fallbacks
- **Issue:** If `PlayerInputReader.moveAction` is unassigned, `MoveInput` became zero unless fallback polling was implemented.
- **Pattern:** Always implement direct polling in `PollFallbacks()` (polling `Keyboard.current` for WASD/arrows and `Gamepad.current` for sticks) so player movement can never be paralyzed. Wire `Player/Move` and `Player/Sprint` sub-assets from `InputSystem_Actions.inputactions` on prefabs and scene instances.

## 5. PlayerInfuseManager Loadout Serialization
- **Issue:** Calling `pInfuse.EquipMonster()` in an editor script populates runtime dictionaries, not serialized fields (`initialHeadMonster`, etc.). On `Awake()`, `InitializeLoadout()` wiped the loadout back to empty.
- **Pattern:** Always serialize the default monsters onto `PlayerInfuseManager` (`initialHeadMonster`, `initialChestMonster`, etc.) via `SerializedObject` or in the prefab, and set `notifyAndRecalculate = true` on startup.

## 6. Interaction Trigger Reference Caching & Global Fallbacks
- **Issue:** UI managers that rely solely on cached references from `OnTriggerEnter2D` (e.g. `cachedWallet`, `cachedRoster`) can encounter null references if interactions are triggered in unexpected sequences or during automated testing.
- **Pattern:** Always include a fallback to `FindAnyObjectByType<T>()` in UI handlers (`if (wallet == null) wallet = FindAnyObjectByType<PlayerWallet>();`) so interactions remain resilient under all conditions.

## 7. Unity UI & New Input System EventSystem Requirement
- **Issue:** Without an `EventSystem` with `InputSystemUIInputModule` in the scene, Unity UI `Button` components will never receive pointer hover, press, or click events, causing buttons to appear completely unresponsive.
- **Pattern:** Every scene with interactive UI canvases must include an `EventSystem` configured with `InputSystemUIInputModule` and bound to `UI/Point`, `UI/Click`, `UI/Navigate`, etc. Non-interactive child graphics (backgrounds, decorative borders, text labels) should have `raycastTarget = false` to guarantee unambiguous hit testing on interactive button graphics.

## 8. Anatomical Module Slot Compatibility Checks
- **Issue:** Attempting to equip a monster species into an anatomical slot where it lacks an active module (`monster.GetModuleForSlot(slot) == null`) will be rejected by `PlayerInfuseManager` and leave previous slot state unchanged.
- **Pattern:** Always check species slot capabilities before programmatic or UI equip calls, and support runtime `MonsterInstance` wrappers with custom `RegistrationStatus` to decouple legal licensing from underlying species anatomy.

## 9. Internal Bio-Stress & Recoil Damage vs. Armor Defense
- **Issue:** Self-inflicted bio-recoil damage or internal bio-stress caused by illegal neural overclocking should not be mitigated or absorbed by external armor plating (`Defense`). Using standard `TakeDamage` caused 10 DEF to reduce 4 recoil damage down to the 1 damage clamp.
- **Pattern:** Provide `TakeTrueDamage(float damage)` on `PlayerStatsComponent` for internal physiological backlash, self-harm, and toxicity, ensuring that recoil penalties have the intended balancing consequences regardless of how much armor the player stacks.

## 10. Unity Built-in Sprite PPU and 9-Slice Slicing vs. Transform Scale
- **Issue:** Unity's built-in UI sprites (`UISprite.psd`, `Background.psd`, `Knob.psd`) have `PPU = 200` (e.g., 32px / 200 PPU = 0.16 units native size). Setting `transform.localScale = (34, 22, 1)` results in an actual rendered size of only 5.44 x 3.52 units instead of 34x22 meters, leaving the scene swimming in pitch-black void. Furthermore, colliders generated with full world dimensions leave walls invisible or severely mismatching their visual sprites.
- **Pattern:** For bordered 9-slice sprites like `UISprite`, always set `sr.drawMode = SpriteDrawMode.Sliced; sr.size = targetWorldDimensions; transform.localScale = Vector3.one;`. For non-sliced sprites (e.g., `Knob`), calculate unit size (`width / PPU`) and divide target world dimensions by native unit size. Every collision wall must have matching visible geometry and high-contrast hazard trims to clearly delineate traversable space from physical boundaries.

## 11. 2D Collider Radius & Transform LossyScale Multiplications
- **Issue:** In Unity 2D, `CircleCollider2D` and `BoxCollider2D` scale with the GameObject's `transform.lossyScale`. Setting a sprite object's scale to `(6.5, 6.5, 1)` or `(8, 8, 1)` and then adding a collider with radius `2.4f` or `0.7f` produces massive world-space triggers/colliders ($2.4 \times 6.5 = 15.6\text{m}$, $0.7 \times 8 = 5.6\text{m}$). This caused Dr. Silas's trigger to encompass Pitmaster Jax 9.8m away (firing both modals simultaneously on `[F]`), and caused the Pit Brawler's solid body collider (11.2m diameter) to fill the entire 10.8m fighting cage, pinning player movement.
- **Pattern:** Always normalize collider dimensions by dividing the desired world radius by `transform.localScale.x` (`col.radius = desiredWorldMeters / transform.localScale.x`), or attach colliders to unscaled child GameObjects (`InteractionZone` with `localScale = Vector3.one`). Always calculate distances between interactable stations to ensure non-overlapping zones ($\text{dist} > r_1 + r_2$).

## 12. Hub vs. Combat Zone Gating
- **Issue:** `PlayerInfuseManager` defaults to `isCombatEnabled = true`, allowing combat inputs to fire inside non-combat civilian hubs, markets, and staging areas. Additionally, returning from a match improperly re-enabled combat before the player left the hub.
- **Pattern:** Scene controllers managing peaceful hubs must explicitly disable combat on `Start()` (`pInfuse.SetCombatEnabled(false)`). Bout/match controllers must gate combat strictly to `ActiveBout`, keeping it disabled during match countdowns and immediately upon bout conclusion (`Defeated`, `Victory`, and `ReturnToHub`).

