using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Data-driven definition for deployable field snares, cages, and subdual traps.
    /// </summary>
    [CreateAssetMenu(fileName = "Trap_New", menuName = "BeastClad/Data/Trap Data")]
    public class TrapDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string trapId = "metal_snare";
        public string trapName = "Heavy Wire Snare";

        [TextArea(2, 3)]
        public string description;

        [Header("Trap Mechanics")]
        [Tooltip("Trigger trigger radius on the arena floor.")]
        public float triggerRadius = 1.2f;

        [Tooltip("How long the trapped beast remains immobilized before breaking free (seconds).")]
        public float immobilizeDuration = 12.0f;

        [Tooltip("Poise damage dealt upon springing, weakening the beast for subdual.")]
        public float subdualBonus = 50f;

        [Header("Visuals")]
        public Sprite trapOpenSprite;
        public Sprite trapSprungSprite;
        public Color trapColor = new Color(0.7f, 0.7f, 0.75f, 1f);
    }
}
