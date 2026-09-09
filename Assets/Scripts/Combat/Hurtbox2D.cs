using System;
using UnityEngine;

namespace BeastClad.Combat
{
    /// <summary>
    /// Receiving component for 2D damage collisions.
    /// Attached to characters, dummies, or breakables that can take elemental hits.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hurtbox2D : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("When true, hits are ignored.")]
        [SerializeField] private bool isInvulnerable = false;

        public bool IsInvulnerable
        {
            get => isInvulnerable;
            set => isInvulnerable = value;
        }

        public event Action<DamagePayload> OnHitReceived;

        /// <summary>
        /// Attempts to deliver a damage payload to this hurtbox.
        /// </summary>
        public bool ReceiveHit(DamagePayload payload)
        {
            if (isInvulnerable) return false;

            OnHitReceived?.Invoke(payload);
            return true;
        }
    }
}
