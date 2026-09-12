using System;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Data.Persistence
{
    /// <summary>
    /// Serializable mapping of an anatomical infuse socket to a stored monster specimen.
    /// </summary>
    [Serializable]
    public class SavedEquippedSlot
    {
        [Tooltip("The anatomical socket (Head, Chest, LeftArm, RightArm, Legs).")]
        public EquipmentSlot slot;

        [Tooltip("The monster specimen data assigned to this slot.")]
        public SavedMonsterData monster;

        public SavedEquippedSlot() { }

        public SavedEquippedSlot(EquipmentSlot slot, SavedMonsterData monster)
        {
            this.slot = slot;
            this.monster = monster;
        }
    }
}
