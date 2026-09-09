using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Data-driven definition for placeable wild beast lures and pheromone baits.
    /// Drives attraction radiuses, target affinities, and deployment duration.
    /// </summary>
    [CreateAssetMenu(fileName = "Lure_New", menuName = "BeastClad/Data/Lure Data")]
    public class LureDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string lureId = "basic_lure";
        public string lureName = "Pheromone Bait";

        [TextArea(2, 3)]
        public string description;

        [Header("Attraction Dynamics")]
        [Tooltip("Target element this scent is tuned to attract. If null, attracts any wild beast.")]
        public ElementalTypeSO targetedElement;

        [Tooltip("Effective radius within which wild monsters sense the scent.")]
        public float attractionRadius = 10.0f;

        [Tooltip("How long the scent remains active before dissipating (seconds).")]
        public float duration = 20.0f;

        [Header("Visuals")]
        public Sprite lureSprite;
        public Color scentAuraColor = new Color(0.2f, 0.9f, 0.4f, 0.35f);
    }
}
