using System;
using UnityEngine;
using BeastClad.Data;
using BeastClad.Combat;

namespace BeastClad.Trapping
{
    public enum WildMonsterState
    {
        Wandering,
        AttractedToLure,
        SnaredInTrap,
        Captured
    }

    /// <summary>
    /// Autonomous 2D wild monster behavior in outdoor sandbox zones.
    /// Handles organic roaming, steering toward matching pheromone lures,
    /// and struggling when caught in field snares.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Hurtbox2D))]
    public class WildMonsterAI2D : MonoBehaviour
    {
        [Header("Monster Identity")]
        [SerializeField] private MonsterDataSO monsterData;

        [Header("Movement & AI")]
        [SerializeField] private float wanderSpeed = 2.5f;
        [SerializeField] private float lureAttractionSpeed = 3.5f;
        [SerializeField] private float wanderRadius = 6.0f;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Internal State
        private Rigidbody2D rb;
        private Hurtbox2D hurtbox;
        private WildMonsterState currentState = WildMonsterState.Wandering;
        private Vector2 spawnOrigin;
        private Vector2 currentWanderTarget;
        private float wanderTimer;
        private PlaceableLure activeTargetLure;
        private PlaceableTrap currentTrap;
        private Vector3 initialPositionInTrap;

        public string SpeciesName => monsterData != null ? monsterData.commonName : "Wild Beast";
        public MonsterDataSO Data => monsterData;
        public WildMonsterState CurrentState => currentState;
        public bool IsTrapped => currentState == WildMonsterState.SnaredInTrap;

        public event Action<MonsterInstance> OnCaptured;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ConfigureRigidbody();

            hurtbox = GetComponent<Hurtbox2D>();
            spawnOrigin = transform.position;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            UpdateMonsterVisuals();
            PickNewWanderTarget();
        }

        private void OnEnable()
        {
            if (hurtbox != null)
            {
                hurtbox.OnHitReceived += HandleHitReceived;
            }
        }

        private void OnDisable()
        {
            if (hurtbox != null)
            {
                hurtbox.OnHitReceived -= HandleHitReceived;
            }
        }

        private void ConfigureRigidbody()
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            switch (currentState)
            {
                case WildMonsterState.Wandering:
                    CheckForNearbyLures();
                    break;

                case WildMonsterState.AttractedToLure:
                    ValidateActiveLure();
                    break;

                case WildMonsterState.SnaredInTrap:
                    ExecuteStruggleVisual();
                    break;
            }
        }

        private void FixedUpdate()
        {
            switch (currentState)
            {
                case WildMonsterState.Wandering:
                    ExecuteWanderPhysics();
                    break;

                case WildMonsterState.AttractedToLure:
                    ExecuteLureSteeringPhysics();
                    break;

                case WildMonsterState.SnaredInTrap:
                    SetVelocity(Vector2.zero);
                    break;
            }
        }

        #region AI Movement & Lure Steering

        private void CheckForNearbyLures()
        {
            foreach (var lure in PlaceableLure.ActiveLures)
            {
                if (lure == null || !lure.IsActive) continue;

                float dist = Vector2.Distance(transform.position, lure.transform.position);
                if (dist <= lure.AttractionRadius)
                {
                    // Check element affinity: matches target element or general lure
                    if (lure.Data == null || lure.Data.targetedElement == null ||
                        (monsterData != null && monsterData.primaryElement == lure.Data.targetedElement))
                    {
                        activeTargetLure = lure;
                        currentState = WildMonsterState.AttractedToLure;
                        Debug.Log($"<color=#77FFAA>[Wild Beast]</color> <b>{SpeciesName}</b> caught scent of {lure.Data?.lureName}! Lured toward trap zone.");
                        return;
                    }
                }
            }
        }

        private void ValidateActiveLure()
        {
            if (activeTargetLure == null || !activeTargetLure.IsActive)
            {
                activeTargetLure = null;
                currentState = WildMonsterState.Wandering;
                PickNewWanderTarget();
            }
        }

        private void ExecuteWanderPhysics()
        {
            wanderTimer -= Time.fixedDeltaTime;
            if (wanderTimer <= 0f)
            {
                PickNewWanderTarget();
            }

            Vector2 toTarget = currentWanderTarget - (Vector2)transform.position;
            if (toTarget.sqrMagnitude > 0.25f)
            {
                SetVelocity(toTarget.normalized * wanderSpeed);
                UpdateFacing(toTarget.x);
            }
            else
            {
                SetVelocity(Vector2.zero);
            }
        }

        private void ExecuteLureSteeringPhysics()
        {
            if (activeTargetLure == null) return;

            Vector2 toLure = (Vector2)activeTargetLure.transform.position - (Vector2)transform.position;
            if (toLure.sqrMagnitude > 0.1f)
            {
                SetVelocity(toLure.normalized * lureAttractionSpeed);
                UpdateFacing(toLure.x);
            }
            else
            {
                SetVelocity(Vector2.zero);
            }
        }

        private void PickNewWanderTarget()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            currentWanderTarget = spawnOrigin + randomOffset;
            wanderTimer = UnityEngine.Random.Range(3f, 6f);
        }

        private void UpdateFacing(float xDir)
        {
            if (spriteRenderer != null && Mathf.Abs(xDir) > 0.05f)
            {
                spriteRenderer.flipX = xDir < 0f;
            }
        }

        #endregion

        #region Trapping & Capture

        public void TrapInSnare(PlaceableTrap trap, float duration)
        {
            currentState = WildMonsterState.SnaredInTrap;
            currentTrap = trap;
            initialPositionInTrap = transform.position;
            SetVelocity(Vector2.zero);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f); // Strained struggle tint
            }
        }

        public void BreakFree()
        {
            currentState = WildMonsterState.Wandering;
            currentTrap = null;
            UpdateMonsterVisuals();
            PickNewWanderTarget();
        }

        /// <summary>
        /// Subdues the immobilized monster and creates an acquired MonsterInstance.
        /// </summary>
        public MonsterInstance SubdueAndCapture()
        {
            currentState = WildMonsterState.Captured;
            SetVelocity(Vector2.zero);

            var instance = new MonsterInstance(monsterData, RegistrationStatus.Unregistered);

            Debug.Log($"<color=#00FF88>[Capture Success!]</color> Trapped and secured: <b>{SpeciesName}</b> (Tag: {instance.instanceId}, Unregistered)!");

            OnCaptured?.Invoke(instance);

            Destroy(gameObject, 0.1f);
            return instance;
        }

        private void ExecuteStruggleVisual()
        {
            // Rapid subtle struggle shake
            float shake = Mathf.Sin(Time.time * 25f) * 0.08f;
            transform.position = initialPositionInTrap + new Vector3(shake, 0f, 0f);
        }

        private void HandleHitReceived(DamagePayload payload)
        {
            Debug.Log($"<color=#FFAA00>[Wild Beast]</color> {SpeciesName} weakened by {payload.rawDamage:F1} {payload.element?.displayName} strike!");
        }

        #endregion

        private void SetVelocity(Vector2 vel)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = vel;
#else
            rb.velocity = vel;
#endif
        }

        private void UpdateMonsterVisuals()
        {
            if (spriteRenderer != null && monsterData != null && monsterData.primaryElement != null)
            {
                spriteRenderer.color = monsterData.primaryElement.elementColor;
            }
        }

        public void Initialize(MonsterDataSO data, Sprite sprite)
        {
            monsterData = data;
            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
            UpdateMonsterVisuals();
        }
    }
}
