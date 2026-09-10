using System;
using System.Collections;
using UnityEngine;
using BeastClad.Combat;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Underground
{
    public enum PitBrawlerState
    {
        Inactive,
        Stalking,
        Telegraphing,
        Lunging,
        Recovery,
        Defeated
    }

    /// <summary>
    /// Autonomous feral pit brawler AI fighting in the unsanctioned underground pit.
    /// Uses aggressive gap-closing, telegraph indicators, melee lunges, and takes elemental hits.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Hurtbox2D))]
    public class UnsanctionedPitBrawlerAI2D : MonoBehaviour
    {
        [Header("Brawler Identity")]
        [SerializeField] private string brawlerName = "Grimlock the Flesh-Render";
        [SerializeField] private string epithet = "Contraband-Infused Pit Champion";

        [Header("Attributes")]
        [SerializeField] private float maxHealth = 220f;
        [SerializeField] private float defense = 4f;
        [SerializeField] private float moveSpeed = 4.6f;

        [Header("Combat Tuning")]
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float attackCooldown = 1.8f;
        [SerializeField] private float attackDamage = 22f;
        [SerializeField] private float telegraphDuration = 0.35f;
        [SerializeField] private float lungeMultiplier = 2.0f;
        [SerializeField] private ElementalTypeSO attackElement;

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private Hurtbox2D hurtbox;
        private Collider2D bodyCollider;
        private Transform playerTarget;

        private PitBrawlerState currentState = PitBrawlerState.Inactive;
        private float currentHealth;
        private float attackTimer;
        private float stateTimer;
        private bool combatEnabled = false;

        private readonly Color defaultColor = new Color(0.85f, 0.25f, 0.25f, 1f); // Menacing rust crimson
        private readonly Color telegraphColor = new Color(1f, 0.85f, 0.1f, 1f); // Electric warning flash
        private readonly Color defeatedColor = new Color(0.35f, 0.35f, 0.35f, 0.6f); // Greyed out

        public string BrawlerName => brawlerName;
        public string Epithet => epithet;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public PitBrawlerState CurrentState => currentState;
        public bool IsDefeated => currentState == PitBrawlerState.Defeated;

        public event Action<float, float> OnHealthChanged;
        public event Action<UnsanctionedPitBrawlerAI2D> OnDefeated;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            hurtbox = GetComponent<Hurtbox2D>();
            bodyCollider = GetComponent<Collider2D>();

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
            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
        }

        private void Update()
        {
            if (!combatEnabled || currentState == PitBrawlerState.Inactive || currentState == PitBrawlerState.Defeated)
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
                case PitBrawlerState.Stalking:
                    UpdateStalking();
                    break;

                case PitBrawlerState.Telegraphing:
                    UpdateTelegraphing();
                    break;

                case PitBrawlerState.Recovery:
                    UpdateRecovery();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!combatEnabled || currentState == PitBrawlerState.Inactive || currentState == PitBrawlerState.Defeated || currentState == PitBrawlerState.Telegraphing || currentState == PitBrawlerState.Recovery)
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

            if (currentState == PitBrawlerState.Stalking)
            {
                SetVelocity(dir * moveSpeed);
            }
        }

        public void EnableCombat(bool enable)
        {
            combatEnabled = enable;
            if (enable)
            {
                if (currentState != PitBrawlerState.Defeated)
                {
                    currentState = PitBrawlerState.Stalking;
                    attackTimer = 0.5f; // Short initial delay before first strike
                    if (spriteRenderer != null) spriteRenderer.color = defaultColor;
                }
            }
            else
            {
                if (currentState != PitBrawlerState.Defeated)
                {
                    currentState = PitBrawlerState.Inactive;
                }
                SetVelocity(Vector2.zero);
            }
        }

        public void ResetBrawler(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            if (rb != null)
            {
                rb.position = spawnPosition;
                SetVelocity(Vector2.zero);
            }

            currentHealth = maxHealth;
            currentState = PitBrawlerState.Inactive;
            combatEnabled = false;
            attackTimer = 0.5f;

            if (bodyCollider != null) bodyCollider.enabled = true;
            if (hurtbox != null) hurtbox.IsInvulnerable = false;
            if (spriteRenderer != null) spriteRenderer.color = defaultColor;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void FindPlayerTarget()
        {
            var pc = FindAnyObjectByType<PlayerController2D>();
            if (pc != null)
            {
                playerTarget = pc.transform;
            }
        }

        private void UpdateStalking()
        {
            if (playerTarget == null) return;

            float dist = Vector2.Distance(rb.position, playerTarget.position);
            if (dist <= attackRange && attackTimer <= 0f)
            {
                StartTelegraph();
            }
        }

        private void StartTelegraph()
        {
            currentState = PitBrawlerState.Telegraphing;
            stateTimer = telegraphDuration;
            SetVelocity(Vector2.zero);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = telegraphColor;
            }
        }

        private void UpdateTelegraphing()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                ExecuteLunge();
            }
        }

        private void ExecuteLunge()
        {
            currentState = PitBrawlerState.Lunging;
            attackTimer = attackCooldown;

            if (playerTarget == null)
            {
                FinishAttack();
                return;
            }

            Vector2 attackDir = ((Vector2)playerTarget.position - rb.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(attackDir * 1.1f);

            // Spawn Melee Hitbox
            var hitboxObj = new GameObject("PitBrawlerHitbox");
            hitboxObj.transform.position = spawnPos;

            var col = hitboxObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.0f;

            var payload = new DamagePayload(
                attackDamage,
                attackElement,
                attackDir,
                7.5f,
                gameObject
            );

            var hitbox = hitboxObj.AddComponent<Hitbox2D>();
            hitbox.Initialize(payload, 0.20f);

            // Forward lunge impulse
            SetVelocity(attackDir * (moveSpeed * lungeMultiplier));

            FinishAttack();
        }

        private void FinishAttack()
        {
            currentState = PitBrawlerState.Recovery;
            stateTimer = 0.55f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
        }

        private void UpdateRecovery()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                currentState = PitBrawlerState.Stalking;
            }
        }

        private void HandleHitReceived(DamagePayload payload)
        {
            if (currentState == PitBrawlerState.Defeated) return;

            float effectiveDamage = Mathf.Max(1f, payload.rawDamage - defense);
            currentHealth = Mathf.Clamp(currentHealth - effectiveDamage, 0f, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            StartCoroutine(FlashDamageRoutine());

            if (currentHealth <= 0f)
            {
                Defeat();
            }
        }

        private IEnumerator FlashDamageRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                if (currentState != PitBrawlerState.Defeated)
                {
                    spriteRenderer.color = currentState == PitBrawlerState.Telegraphing ? telegraphColor : defaultColor;
                }
            }
        }

        private void Defeat()
        {
            currentState = PitBrawlerState.Defeated;
            combatEnabled = false;
            SetVelocity(Vector2.zero);

            if (bodyCollider != null) bodyCollider.enabled = false;
            if (hurtbox != null) hurtbox.IsInvulnerable = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = defeatedColor;
            }

            Debug.Log($"<color=#FF3333>[Underground Pit]</color> <b>{brawlerName}</b> has been brutally defeated!");
            OnDefeated?.Invoke(this);
        }

        private void UpdateFacing(float moveX)
        {
            if (Mathf.Abs(moveX) > 0.05f && spriteRenderer != null)
            {
                spriteRenderer.flipX = moveX < 0f;
            }
        }

        private void SetVelocity(Vector2 velocity)
        {
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
        }
    }
}
