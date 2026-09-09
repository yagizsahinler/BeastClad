using System.Collections.Generic;
using UnityEngine;

namespace BeastClad.Combat
{
    /// <summary>
    /// Ephemeral 2D melee trigger hitbox spawned during limb attacks.
    /// Delivers DamagePayload to any Hurtbox2D or SurfaceHazard it overlaps.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Hitbox2D : MonoBehaviour
    {
        private DamagePayload payload;
        private float lifetime = 0.15f;
        private readonly HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

        public void Initialize(DamagePayload damagePayload, float duration)
        {
            payload = damagePayload;
            lifetime = Mathf.Max(0.05f, duration);
            hitTargets.Clear();

            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore attacker
            if (payload.source != null && (other.gameObject == payload.source || other.transform.IsChildOf(payload.source.transform)))
            {
                return;
            }

            if (hitTargets.Contains(other)) return;

            // Deliver to Hurtbox
            if (other.TryGetComponent<Hurtbox2D>(out var hurtbox))
            {
                hitTargets.Add(other);
                hurtbox.ReceiveHit(payload);
            }

            // Interact with Surface Hazard (Terraforming Chemistry)
            if (other.TryGetComponent<SurfaceHazard>(out var hazard))
            {
                hitTargets.Add(other);
                hazard.ApplyElementalStrike(payload);
            }
        }
    }
}
