using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Data
{
    /// <summary>
    /// ScriptableObject defining an illicit, black-market beast offering sold by the Ripperdoc.
    /// Features overclocked stats, reduced biological stability, and recoil self-damage.
    /// Stamped with RegistrationStatus.Contraband.
    /// </summary>
    [CreateAssetMenu(fileName = "Ripperdoc_NewProduct", menuName = "BeastClad/Data/Ripperdoc Product")]
    public class RipperdocProductSO : ScriptableObject
    {
        [Header("Product Identity")]
        [Tooltip("Unique catalog ID.")]
        public string productId = "contraband_product_default";

        [Tooltip("Black market listing title.")]
        public string productTitle = "Overclocked Volt Mantis [NEURAL-SHUNT]";

        [TextArea(2, 4)]
        [Tooltip("Flavor lore describing the illegal biological modification.")]
        public string description = "Illegally spliced with high-output discharge coils. Delivers devastating offensive strikes, but unstable feedback shocks the bearer.";

        [Header("Pricing & Species")]
        [Tooltip("Black market purchase price in Credits.")]
        [Min(0)]
        public int costCredits = 350;

        [Tooltip("The underlying species data template.")]
        public MonsterDataSO monsterSpecies;

        [Header("Overclock & Risk Parameters")]
        [Tooltip("State legal status (Always Contraband for Ripperdoc products).")]
        public RegistrationStatus registrationStatus = RegistrationStatus.Contraband;

        [Tooltip("Biological stability percentage (e.g., 60% indicates unstable modified tissue).")]
        [Range(10f, 100f)]
        public float stabilityPercentage = 60f;

        [Tooltip("Flat bonus damage added to all melee strikes executed by this limb.")]
        public float overclockBonusDamage = 15f;

        [Tooltip("Bio-stress recoil damage inflicted onto the player upon striking with this limb.")]
        public float recoilSelfDamage = 4f;

        /// <summary>
        /// Instantiates a runtime MonsterInstance carrying the contraband tag and reduced stability.
        /// </summary>
        public MonsterInstance CreateContrabandInstance()
        {
            if (monsterSpecies == null)
            {
                Debug.LogError($"[Ripperdoc] Product '{productTitle}' has no MonsterDataSO assigned!");
                return null;
            }

            var instance = new MonsterInstance(monsterSpecies, registrationStatus)
            {
                nickname = $"{monsterSpecies.commonName} [OVERCLOCKED]",
                stability = stabilityPercentage
            };

            return instance;
        }
    }
}
