using System;
using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Stat modifiers granted to the player character when a monster part is infused.
    /// Follows the core rule: Head = HP, Chest = Defense, Arms = Attack, Legs = Speed.
    /// </summary>
    [Serializable]
    public struct StatModifierGroup
    {
        [Tooltip("Bonus to maximum Hit Points (Head focus).")]
        public float bonusMaxHP;

        [Tooltip("Bonus to flat Defense / damage reduction (Chest focus).")]
        public float bonusDefense;

        [Tooltip("Bonus to Attack Power (Arms focus).")]
        public float bonusAttack;

        [Tooltip("Bonus to movement speed in units/sec (Legs focus).")]
        public float bonusMoveSpeed;

        public static StatModifierGroup Zero => new StatModifierGroup();

        public static StatModifierGroup operator +(StatModifierGroup a, StatModifierGroup b)
        {
            return new StatModifierGroup
            {
                bonusMaxHP = a.bonusMaxHP + b.bonusMaxHP,
                bonusDefense = a.bonusDefense + b.bonusDefense,
                bonusAttack = a.bonusAttack + b.bonusAttack,
                bonusMoveSpeed = a.bonusMoveSpeed + b.bonusMoveSpeed
            };
        }
    }
}
