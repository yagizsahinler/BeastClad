using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Data-driven definition for an active combat skill tied to an infused monster limb.
    /// Can spawn 2D hitboxes, apply elemental status, or terraform the arena floor.
    /// </summary>
    [CreateAssetMenu(fileName = "Skill_New", menuName = "BeastClad/Data/Skill Definition")]
    public class SkillDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string skillName = "Basic Skill";

        [TextArea(2, 3)]
        public string description;

        [Header("Combat Profile")]
        [Tooltip("Elemental affinity of this attack/skill.")]
        public ElementalTypeSO element;

        [Tooltip("Base physical or elemental damage dealt.")]
        public float baseDamage = 10f;

        [Tooltip("Forward reach or radius in 2D world units.")]
        public float range = 1.5f;

        [Tooltip("Active hitbox lifespan in seconds.")]
        public float hitboxDuration = 0.15f;

        [Header("Combat Timing Lifecycle (Seconds)")]
        [Tooltip("Startup wind-up duration before hitbox is active. Default 0 for immediate activation.")]
        [Min(0f)]
        public float startupDuration = 0.0f;

        [Tooltip("Recovery duration / attack follow-through lockout after active hitbox expires.")]
        [Min(0f)]
        public float recoveryDuration = 0.05f;

        [Header("Animation & Visual Effects (VFX)")]
        [Tooltip("Mecanim animator trigger or state name to play on execution.")]
        public string animationTriggerName = "Attack";

        [Tooltip("Slash arc, bio-stinger trail, or projectile visual effect spawned on attack execution.")]
        public GameObject attackVfxPrefab;

        [Tooltip("Burst, spark, or splash visual effect spawned at impact location on hitting a hurtbox.")]
        public GameObject hitImpactVfxPrefab;

        [Header("Audio Signatures")]
        [Tooltip("Sound played upon swinging, firing, or initiating the skill.")]
        public AudioClip attackSound;

        [Tooltip("Sound played upon striking an opponent or hurtbox.")]
        public AudioClip hitSound;

        [Header("Terraforming & Battlefield Hazards")]
        [Tooltip("Does this attack deposit a surface hazard (e.g. Water puddle, acid slick)?")]
        public bool spawnsSurfaceHazard;

        [Tooltip("Optional 2D surface hazard prefab to spawn on the arena floor upon execution.")]
        public GameObject surfaceHazardPrefab;

        [Tooltip("Lifetime of the spawned surface hazard in seconds.")]
        public float hazardLifetime = 8.0f;
    }
}
