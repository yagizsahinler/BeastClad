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

        private void OnEnable()
        {
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
            TakeDamage(payload.rawDamage);
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
