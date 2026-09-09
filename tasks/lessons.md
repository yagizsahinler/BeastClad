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
