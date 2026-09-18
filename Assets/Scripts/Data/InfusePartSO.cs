using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Defines behavior, stats, and skills granted when a monster module is infused into a specific player slot.
    /// Drives local cooldowns and active skills without hardcoded values.
    /// </summary>
    [CreateAssetMenu(fileName = "InfusePart_New", menuName = "BeastClad/Data/Infuse Part")]
    public class InfusePartSO : ScriptableObject
    {
        [Header("Part Identity")]
        public string partName = "Infuse Module";

        [Header("Socket Configuration")]
        [Tooltip("Target body slot this module attaches to.")]
        public EquipmentSlot targetSlot = EquipmentSlot.Head;

        [Header("Cooldown (Mandatory Data-Driven Value)")]
        [Tooltip("Independent local cooldown for this limb skill (seconds).")]
        [Min(0.1f)]
        public float localCooldownDuration = 1.0f;

        [Header("Stat Modifiers")]
        [Tooltip("Buffs applied to the player chassis while this module is equipped.")]
        public StatModifierGroup statModifiers;

        [Header("Active Skill")]
        [Tooltip("Skill triggered when this limb's input is activated.")]
        public SkillDefinitionSO activeSkill;

        [Header("Visual Representation (Paperdoll)")]
        [Tooltip("Legacy / universal single sprite overlay.")]
        public Sprite visualOverlaySprite;

        [Tooltip("Sprite rendered when player faces Down (toward camera). Falls back to visualOverlaySprite if null.")]
        public Sprite overlayFront;

        [Tooltip("Sprite rendered when player faces Up (away from camera). Falls back to visualOverlaySprite if null.")]
        public Sprite overlayBack;

        [Tooltip("Sprite rendered when player faces Left or Right (horizontally flipped). Falls back to visualOverlaySprite if null.")]
        public Sprite overlaySide;

        [Header("Equipped VFX & Audio")]
        [Tooltip("Optional ambient loop VFX prefab attached to the socket while equipped (e.g. electric sparks, frost mist).")]
        public GameObject equippedVfxPrefab;

        [Tooltip("Sound played when this monster module is bio-socketed onto the player chassis.")]
        public AudioClip equipSound;

        /// <summary>
        /// Retrieves the most appropriate directional overlay sprite based on the player's 2D facing vector.
        /// Gracefully falls back to visualOverlaySprite if specific directional sprites are unassigned.
        /// </summary>
        public Sprite GetOverlayForDirection(Vector2 facingDir)
        {
            // Upward facing
            if (facingDir.y > 0.5f && overlayBack != null)
            {
                return overlayBack;
            }

            // Downward facing
            if (facingDir.y < -0.5f && overlayFront != null)
            {
                return overlayFront;
            }

            // Horizontal facing
            if (Mathf.Abs(facingDir.x) > 0.5f && overlaySide != null)
            {
                return overlaySide;
            }

            // Fallback hierarchy
            return visualOverlaySprite ?? overlayFront ?? overlaySide ?? overlayBack;
        }

        /// <summary>
        /// Validates whether this part can be slotted into the specified equipment socket.
        /// Allows arm parts to be equipped to either Left or Right arm sockets for dual-wielding combos.
        /// </summary>
        public bool CanEquipTo(EquipmentSlot slot)
        {
            if (targetSlot == slot) return true;

            bool isSourceArm = targetSlot == EquipmentSlot.LeftArm || targetSlot == EquipmentSlot.RightArm;
            bool isTargetArm = slot == EquipmentSlot.LeftArm || slot == EquipmentSlot.RightArm;

            return isSourceArm && isTargetArm;
        }
    }
}
