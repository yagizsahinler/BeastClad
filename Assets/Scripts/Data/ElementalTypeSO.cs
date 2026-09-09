using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Data-driven definition for an elemental affinity in BeastClad.
    /// Drives Chemistry reactions, HUD colors, and terraforming triggers.
    /// </summary>
    [CreateAssetMenu(fileName = "Element_New", menuName = "BeastClad/Data/Elemental Type")]
    public class ElementalTypeSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique elemental identifier (e.g., Hydro, Pyro, Volt, Geo, Bio, Cryo, Plastic).")]
        public string elementId = "Neutral";

        [Tooltip("Display name shown in UI and inspection panels.")]
        public string displayName = "Neutral";

        [Header("Visuals & Tints")]
        [Tooltip("Signature color for UI borders, damage numbers, and particle effects.")]
        public Color elementColor = Color.white;

        [Tooltip("Optional icon for UI loadout slots.")]
        public Sprite elementIcon;

        [Header("Aesthetic / Flavor")]
        [TextArea(2, 4)]
        public string description;
    }
}
