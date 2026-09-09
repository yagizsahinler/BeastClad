using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Single source of truth for an individual monster species in BeastClad.
    /// Encapsulates elemental typings, state registration status, and anatomical infuse modules.
    /// </summary>
    [CreateAssetMenu(fileName = "Monster_New", menuName = "BeastClad/Data/Monster Data")]
    public class MonsterDataSO : ScriptableObject
    {
        [Header("Identity & Lore")]
        [Tooltip("Unique species identifier (e.g. 'torrent_wyrm').")]
        public string speciesId = "unknown_species";

        [Tooltip("Common name displayed in menus, lore entries, and inventory.")]
        public string commonName = "Wild Monster";

        [Tooltip("State legal registration category.")]
        public RegistrationStatus defaultRegistration = RegistrationStatus.Legal;

        [Header("Elemental Affinities")]
        [Tooltip("Primary element dictating chemical reactions and visual aura.")]
        public ElementalTypeSO primaryElement;

        [Tooltip("Optional secondary element for advanced hybrid monsters.")]
        public ElementalTypeSO secondaryElement;

        [Header("UI Visuals")]
        [Tooltip("Portrait or catalog icon for this creature.")]
        public Sprite monsterIcon;

        [TextArea(2, 4)]
        public string loreDescription;

        [Header("Anatomical Infuse Modules")]
        [Tooltip("Module bio-socketed when equipped to the Head (HP / Sensory Utility).")]
        public InfusePartSO headModule;

        [Tooltip("Module bio-socketed when equipped to the Chest (Defense / Shielding).")]
        public InfusePartSO chestModule;

        [Tooltip("Module bio-socketed when equipped to the Left Arm (Attack / Primary Combo).")]
        public InfusePartSO leftArmModule;

        [Tooltip("Module bio-socketed when equipped to the Right Arm (Attack / Finisher Combo).")]
        public InfusePartSO rightArmModule;

        [Tooltip("Module bio-socketed when equipped to the Legs (Speed / Dash Evasion).")]
        public InfusePartSO legsModule;

        /// <summary>
        /// Retrieves the corresponding InfusePartSO module for the given equipment slot.
        /// </summary>
        public InfusePartSO GetModuleForSlot(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Head => headModule,
                EquipmentSlot.Chest => chestModule,
                EquipmentSlot.LeftArm => leftArmModule,
                EquipmentSlot.RightArm => rightArmModule,
                EquipmentSlot.Legs => legsModule,
                _ => null
            };
        }
    }
}
