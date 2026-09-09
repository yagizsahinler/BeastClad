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

        [Header("Terraforming & Battlefield Hazards")]
        [Tooltip("Does this attack deposit a surface hazard (e.g. Water puddle, acid slick)?")]
        public bool spawnsSurfaceHazard;

        [Tooltip("Optional 2D surface hazard prefab to spawn on the arena floor upon execution.")]
        public GameObject surfaceHazardPrefab;

        [Tooltip("Lifetime of the spawned surface hazard in seconds.")]
        public float hazardLifetime = 8.0f;
    }
}
