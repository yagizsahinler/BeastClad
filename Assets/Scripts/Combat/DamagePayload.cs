using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Combat
{
    /// <summary>
    /// Data payload delivered by a Hitbox2D when striking a Hurtbox2D or SurfaceHazard.
    /// Encapsulates raw damage, elemental affinity, knockback, and origin.
    /// </summary>
    public struct DamagePayload
    {
        public float rawDamage;
        public ElementalTypeSO element;
        public Vector2 knockbackDirection;
        public float knockbackForce;
        public GameObject source;
        public GameObject hitImpactVfxPrefab;
        public AudioClip hitSound;

        public DamagePayload(float damage, ElementalTypeSO elem, Vector2 knockbackDir, float knockbackStrength, GameObject attacker)
        {
            rawDamage = damage;
            element = elem;
            knockbackDirection = knockbackDir;
            knockbackForce = knockbackStrength;
            source = attacker;
            hitImpactVfxPrefab = null;
            hitSound = null;
        }

        public DamagePayload(float damage, ElementalTypeSO elem, Vector2 knockbackDir, float knockbackStrength, GameObject attacker, GameObject impactVfx, AudioClip impactSound)
        {
            rawDamage = damage;
            element = elem;
            knockbackDirection = knockbackDir;
            knockbackForce = knockbackStrength;
            source = attacker;
            hitImpactVfxPrefab = impactVfx;
            hitSound = impactSound;
        }
    }
}
