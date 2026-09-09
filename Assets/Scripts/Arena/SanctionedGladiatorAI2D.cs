using System;
using UnityEngine;
using BeastClad.Combat;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Arena
{
    public enum GladiatorAIState
    {
        Inactive,
        Approaching,
        Circling,
        Telegraphing,
        Attacking,
        Defeated
    }

    /// <summary>
    /// Autonomous sanctioned tournament gladiator AI infused with corporate monster gear.
    /// Operates with real-time tactical spacing, attack telegraphs, and cooldown-driven abilities.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Hurtbox2D))]
    public class SanctionedGladiatorAI2D : MonoBehaviour
    {
        [Header("Gladiator Identity")]
        [SerializeField] private string gladiatorName = "Valerius the Shock-Lancer";
        [SerializeField] private string corporateSponsor = "Aegis-Fauna Corp (Bronze League)";

        [Header("Attributes")]
        [SerializeField] private float maxHealth = 180f;
        [SerializeField] private float defense = 5f;
        [SerializeField] private float moveSpeed = 4.2f;

        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 2.4f;
        [SerializeField] private float attackCooldown = 2.2f;
        [SerializeField] private float attackDamage = 18f;
        [SerializeField] private float telegraphDuration = 0.45f;
        [SerializeField] private ElementalTypeSO attackElement;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private Hurtbox2D hurtbox;
        private Transform playerTarget;
        private GladiatorAIState currentState = GladiatorAIState.Inactive;

        private float currentHealth;
        private float attackTimer;
        private float stateTimer;
        private int circleDirection = 1;

        public string GladiatorName => gladiatorName;
        public string CorporateSponsor => corporateSponsor;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public GladiatorAIState CurrentState => currentState;
        public bool IsDefeated => currentState == GladiatorAIState.Defeated;

        public event Action<float, float> OnHealthChanged;
        public event Action<SanctionedGladiatorAI2D> OnDefeated;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            hurtbox = GetComponent<Hurtbox2D>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            currentHealth = maxHealth;
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

        private void Start()
        {
            FindPlayerTarget();
        }

        private void Update()
        {
            if (currentState == GladiatorAIState.Inactive || currentState == GladiatorAIState.Defeated)
            {
                return;
            }

            if (playerTarget == null)
            {
                FindPlayerTarget();
                if (playerTarget == null) return;
            }

            attackTimer -= Time.deltaTime;

            switch (currentState)
            {
                case GladiatorAIState.Approaching:
                    UpdateApproaching();
                    break;

                case GladiatorAIState.Circling:
                    UpdateCircling();
                    break;

                case GladiatorAIState.Telegraphing:
                    UpdateTelegraphing();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (currentState == GladiatorAIState.Inactive || currentState == GladiatorAIState.Defeated || currentState == GladiatorAIState.Telegraphing)
            {
                SetVelocity(Vector2.zero);
                return;
            }

            if (playerTarget == null)
            {
                SetVelocity(Vector2.zero);
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - rb.position;
            float dist = toPlayer.magnitude;
            Vector2 dir = dist > 0.01f ? toPlayer / dist : Vector2.zero;

            UpdateFacing(dir.x);

            if (currentState == GladiatorAIState.Approaching)
            {
                SetVelocity(dir * moveSpeed);
            }
            else if (currentState == GladiatorAIState.Circling)
            {
                // Tangent vector for lateral circling
                Vector2 tangent = new Vector2(-dir.y, dir.x) * circleDirection;
                // Maintain standoff distance around attackRange
                float distanceCorrection = dist - attackRange;
                Vector2 moveDir = (tangent + dir * distanceCorrection).normalized;
                SetVelocity(moveDir * (moveSpeed * 0.75f));
            }
            else
            {
                SetVelocity(Vector2.zero);
            }
        }

        private void FindPlayerTarget()
        {
            var pc = FindAnyObjectByType<PlayerController2D>();
            if (pc != null)
            {
                playerTarget = pc.transform;
            }
        }

        private void UpdateApproaching()
        {
            float dist = Vector2.Distance(rb.position, playerTarget.position);
            if (dist <= attackRange)
            {
                if (attackTimer <= 0f)
                {
                    StartTelegraph();
                }
                else
                {
                    currentState = GladiatorAIState.Circling;
                    stateTimer = UnityEngine.Random.Range(1.2f, 2.0f);
                    circleDirection = UnityEngine.Random.value > 0.5f ? 1 : -1;
                }
            }
        }

        private void UpdateCircling()
        {
            stateTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                float dist = Vector2.Distance(rb.position, playerTarget.position);
                if (dist <= attackRange * 1.25f)
                {
                    StartTelegraph();
                }
                else
                {
                    currentState = GladiatorAIState.Approaching;
                }
            }
            else if (stateTimer <= 0f)
            {
                circleDirection = -circleDirection;
                stateTimer = UnityEngine.Random.Range(1.2f, 2.5f);
            }
        }

        private void StartTelegraph()
        {
            currentState = GladiatorAIState.Telegraphing;
            stateTimer = telegraphDuration;
            SetVelocity(Vector2.zero);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.9f, 0.2f, 1f); // Electric telegraph flash
            }
        }

        private void UpdateTelegraphing()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ExecuteAttack();
            }
        }

        private void ExecuteAttack()
        {
            currentState = GladiatorAIState.Attacking;
            attackTimer = attackCooldown;

            if (playerTarget == null)
            {
                ReturnToNormal();
                return;
            }

            Vector2 attackDir = ((Vector2)playerTarget.position - rb.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(attackDir * 1.2f);

            // Spawn Melee Hitbox
            var hitboxObj = new GameObject("GladiatorHitbox");
            hitboxObj.transform.position = spawnPos;

            var col = hitboxObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.9f;

            var payload = new DamagePayload(
                attackDamage,
                attackElement,
                attackDir,
                6.0f,
                gameObject
            );

            var hitbox = hitboxObj.AddComponent<Hitbox2D>();
            hitbox.Initialize(payload, 0.18f);

            // Short lunge impulse
            SetVelocity(attackDir * (moveSpeed * 1.6f));

            ReturnToNormal();
        }

        private void ReturnToNormal()
        {
            currentState = GladiatorAIState.Circling;
            stateTimer = UnityEngine.Random.Range(1.0f, 1.8f);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.9f, 0.35f, 0.25f, 1f); // Gladiator armor crimson
            }
        }

        private void HandleHitReceived(DamagePayload payload)
        {
            if (currentState == GladiatorAIState.Defeated) return;

            float effectiveDamage = Mathf.Max(1f, payload.rawDamage - defense);
            currentHealth = Mathf.Clamp(currentHealth - effectiveDamage, 0f, maxHealth);

            Debug.Log($"<color=#FF5533>[Gladiator Hit]</color> {gladiatorName} received {effectiveDamage:F1} damage ({currentHealth:F1}/{maxHealth} HP).");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Quick hit reaction flash
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                Invoke(nameof(ResetVisualColor), 0.1f);
            }

            if (currentHealth <= 0f)
            {
                Defeat();
            }
        }

        private void ResetVisualColor()
        {
            if (spriteRenderer != null && currentState != GladiatorAIState.Defeated)
            {
                spriteRenderer.color = new Color(0.9f, 0.35f, 0.25f, 1f);
            }
        }

        private void Defeat()
        {
            currentState = GladiatorAIState.Defeated;
            SetVelocity(Vector2.zero);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.35f, 0.35f, 0.4f, 0.7f); // Defeated desaturated tint
            }

            Debug.Log($"<color=#FFD700>[Gladiator Down!]</color> <b>{gladiatorName}</b> has been defeated in sanctioned combat!");
            OnDefeated?.Invoke(this);
        }

        private void UpdateFacing(float xDir)
        {
            if (spriteRenderer != null && Mathf.Abs(xDir) > 0.05f)
            {
                spriteRenderer.flipX = xDir < 0f;
            }
        }

        private void SetVelocity(Vector2 vel)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = vel;
#else
            rb.velocity = vel;
#endif
        }

        public void EnableCombat(bool enable)
        {
            if (currentState == GladiatorAIState.Defeated) return;

            if (enable)
            {
                currentState = GladiatorAIState.Approaching;
                attackTimer = 1.0f; // Initial grace period before first attack
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(0.9f, 0.35f, 0.25f, 1f);
                }
            }
            else
            {
                currentState = GladiatorAIState.Inactive;
                SetVelocity(Vector2.zero);
            }
        }

        public void ResetGladiator(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            rb.position = spawnPosition;
            currentHealth = maxHealth;
            currentState = GladiatorAIState.Inactive;
            SetVelocity(Vector2.zero);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.9f, 0.35f, 0.25f, 1f);
            }
        }

        public void SetReferences(ElementalTypeSO element, Sprite sprite)
        {
            attackElement = element;
            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
    }
}
