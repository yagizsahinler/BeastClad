using System;
using UnityEngine;
using BeastClad.Data;
using BeastClad.Combat;

namespace BeastClad.Player
{
    /// <summary>
    /// Manages the player's core runtime health and combined statistics.
    /// Recalculates stats dynamically whenever the Infuse loadout changes.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStatsComponent : MonoBehaviour
    {
        [Header("Base Character Attributes")]
        [SerializeField] private float baseMaxHP = 100f;
        [SerializeField] private float baseDefense = 10f;
        [SerializeField] private float baseAttack = 15f;
        [SerializeField] private float baseMoveSpeed = 6.5f;

        // Cumulative stat modifiers from all 5 infused limb sockets
        private StatModifierGroup activeModifiers;

        public float CurrentHP { get; private set; }
        public float MaxHP => Mathf.Max(1f, baseMaxHP + activeModifiers.bonusMaxHP);
        public float Defense => Mathf.Max(0f, baseDefense + activeModifiers.bonusDefense);
        public float Attack => Mathf.Max(1f, baseAttack + activeModifiers.bonusAttack);
        public float MoveSpeed => Mathf.Max(1f, baseMoveSpeed + activeModifiers.bonusMoveSpeed);

        public event Action<float, float> OnHealthChanged;
        public event Action OnStatsChanged;
        public event Action OnDeath;

        private Hurtbox2D cachedHurtbox;

        private void Awake()
        {
            CurrentHP = MaxHP;
            cachedHurtbox = GetComponent<Hurtbox2D>();
        }

        private void Start()
        {
            if (CurrentHP <= 0f) CurrentHP = MaxHP;
        }

        private void OnEnable()
        {
            if (cachedHurtbox == null) cachedHurtbox = GetComponent<Hurtbox2D>();
            if (cachedHurtbox != null)
            {
                cachedHurtbox.OnHitReceived += HandleHitReceived;
            }
        }

        private void OnDisable()
        {
            if (cachedHurtbox != null)
            {
                cachedHurtbox.OnHitReceived -= HandleHitReceived;
            }
        }

        private void HandleHitReceived(DamagePayload payload)
        {
            if (CurrentHP <= 0f) CurrentHP = MaxHP;

            float effectiveDamage = Mathf.Max(1f, payload.rawDamage - Defense);
            bool isShielded = Defense >= payload.rawDamage;

            // Apply pushback impulse to player
            var pc = GetComponent<PlayerController2D>();
            if (pc != null && payload.knockbackForce > 0.05f)
            {
                pc.ApplyKnockbackImpulse(payload.knockbackDirection, payload.knockbackForce * 0.75f);
            }

            // Flash paperdoll visually
            var visual = GetComponent<PlayerInfuseVisualController>();
            if (visual != null)
            {
                visual.FlashAllOverlays(new Color(1f, 0.35f, 0.35f, 1f), 0.12f);
            }

            TakeDamage(payload.rawDamage);

            // Trigger hit feedback (camera shake, hitstop, damage popup)
            Combat.CombatFeedbackManager.Instance?.TriggerHitFeedback(
                transform.position,
                payload,
                effectiveDamage,
                false,
                isShielded
            );
        }

        /// <summary>
        /// Applies aggregated stat modifiers from all equipped monster parts.
        /// </summary>
        public void ApplyModifiers(StatModifierGroup modifiers)
        {
            float previousMaxHP = MaxHP;
            activeModifiers = modifiers;

            // Preserve health percentage or clamp
            if (MaxHP != previousMaxHP)
            {
                float ratio = previousMaxHP > 0 ? CurrentHP / previousMaxHP : 1f;
                CurrentHP = Mathf.Clamp(MaxHP * ratio, 1f, MaxHP);
                OnHealthChanged?.Invoke(CurrentHP, MaxHP);
            }

            OnStatsChanged?.Invoke();
        }

        public void TakeDamage(float rawDamage)
        {
            if (CurrentHP <= 0f) return;

            float effectiveDamage = Mathf.Max(1f, rawDamage - Defense);
            CurrentHP = Mathf.Clamp(CurrentHP - effectiveDamage, 0f, MaxHP);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);

            if (CurrentHP <= 0f)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// Applies unmitigated true damage directly to health, bypassing defense armor.
        /// Used for biological stress, recoil feedback, and internal toxicity.
        /// </summary>
        public void TakeTrueDamage(float damage)
        {
            if (damage <= 0f) return;
            if (CurrentHP <= 0f) CurrentHP = MaxHP;

            CurrentHP = Mathf.Clamp(CurrentHP - damage, 0f, MaxHP);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);

            var visual = GetComponent<PlayerInfuseVisualController>();
            if (visual != null)
            {
                visual.FlashAllOverlays(new Color(1f, 0.15f, 0.25f, 1f), 0.08f);
            }

            Combat.CombatFeedbackManager.Instance?.SpawnDamagePopup(
                transform.position,
                $"-{damage:F0} BIO-STRESS",
                new Color(1f, 0.2f, 0.35f, 1f),
                true,
                false
            );
            Combat.CombatFeedbackManager.Instance?.TriggerScreenShake(0.20f);

            if (CurrentHP <= 0f)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            CurrentHP = Mathf.Clamp(CurrentHP + amount, 0f, MaxHP);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        public void ResetHealth()
        {
            CurrentHP = MaxHP;
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }
    }
}
