using System;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Player
{
    /// <summary>
    /// Manages independent local cooldown timers for each of the 5 body slots
    /// and enforces a brief Global Cooldown (GCD) across all actions.
    /// Strictly NO Stamina bar, NO PP points.
    /// </summary>
    [DisallowMultipleComponent]
    public class CooldownController : MonoBehaviour
    {
        [Header("Global Cooldown (GCD)")]
        [Tooltip("Brief global delay triggered across all actions to pace combat and prevent button mashing.")]
        [SerializeField] private float gcdDuration = 0.5f;

        // Slot tracking
        private readonly Dictionary<EquipmentSlot, float> remainingTimers = new Dictionary<EquipmentSlot, float>();
        private readonly Dictionary<EquipmentSlot, float> maxDurations = new Dictionary<EquipmentSlot, float>();

        private float currentGCD;

        public float CurrentGCD => currentGCD;
        public float GCDDuration => gcdDuration;
        public bool IsGCDActive => currentGCD > 0f;

        public event Action<EquipmentSlot, float> OnCooldownTriggered;
        public event Action<EquipmentSlot> OnSlotReady;
        public event Action<EquipmentSlot, float> OnCooldownUpdated; // slot, normalizedRemaining (0.0 to 1.0)
        public event Action<float> OnGCDUpdated; // normalized GCD

        private void Awake()
        {
            // Initialize all 5 slots
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                remainingTimers[slot] = 0f;
                maxDurations[slot] = 1f;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Update GCD
            if (currentGCD > 0f)
            {
                currentGCD = Mathf.Max(0f, currentGCD - dt);
                OnGCDUpdated?.Invoke(currentGCD / gcdDuration);
            }

            // Update Local Cooldowns
            var slots = new List<EquipmentSlot>(remainingTimers.Keys);
            foreach (var slot in slots)
            {
                float current = remainingTimers[slot];
                if (current > 0f)
                {
                    float next = Mathf.Max(0f, current - dt);
                    remainingTimers[slot] = next;

                    float max = Mathf.Max(0.01f, maxDurations[slot]);
                    float normalized = next / max;
                    OnCooldownUpdated?.Invoke(slot, normalized);

                    if (next <= 0f)
                    {
                        OnSlotReady?.Invoke(slot);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if an action in the specified slot can be executed right now.
        /// </summary>
        public bool CanExecute(EquipmentSlot slot)
        {
            if (currentGCD > 0f) return false;
            return remainingTimers.TryGetValue(slot, out float time) && time <= 0f;
        }

        /// <summary>
        /// Puts a slot on its local cooldown and triggers the universal Global Cooldown (GCD).
        /// </summary>
        public void TriggerCooldown(EquipmentSlot slot, float localDuration)
        {
            float duration = Mathf.Max(0.1f, localDuration);
            remainingTimers[slot] = duration;
            maxDurations[slot] = duration;

            // Trigger GCD
            currentGCD = gcdDuration;

            OnCooldownTriggered?.Invoke(slot, duration);
            OnCooldownUpdated?.Invoke(slot, 1.0f);
        }

        /// <summary>
        /// Returns normalized local cooldown value: 1.0 (just triggered) down to 0.0 (ready).
        /// Ideal for Unity UI Image Radial 360 Fill.
        /// </summary>
        public float GetCooldownNormalized(EquipmentSlot slot)
        {
            if (remainingTimers.TryGetValue(slot, out float current) && current > 0f)
            {
                float max = maxDurations[slot];
                return Mathf.Clamp01(current / Mathf.Max(0.01f, max));
            }
            return 0f;
        }

        /// <summary>
        /// Returns raw seconds remaining on a slot's local cooldown.
        /// </summary>
        public float GetRemainingTime(EquipmentSlot slot)
        {
            return remainingTimers.TryGetValue(slot, out float time) ? time : 0f;
        }
    }
}
