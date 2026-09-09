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

        [Header("Visual Representation")]
        [Tooltip("Optional sprite overlay rendered over the player chassis.")]
        public Sprite visualOverlaySprite;

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
