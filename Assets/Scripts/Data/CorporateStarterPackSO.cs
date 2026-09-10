using System;
using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// ScriptableObject defining commercial, state-sanctioned monster starter packs.
    /// Acquired through licensed corporate kiosks and vendors.
    /// Sets official state legal registration status on purchase.
    /// </summary>
    [CreateAssetMenu(fileName = "Pack_NewCorporatePack", menuName = "BeastClad/Data/Corporate Starter Pack")]
    public class CorporateStarterPackSO : ScriptableObject
    {
        [Header("Corporate Branding")]
        [Tooltip("Unique catalog identifier (e.g., 'aegis_bulwark_kit').")]
        public string packId = "corporate_pack_default";

        [Tooltip("Commercial product title.")]
        public string packName = "Aegis-Fauna Mk-IV Bulwark Kit";

        [Tooltip("Manufacturing corporation or sanctioning authority.")]
        public string corporateBrand = "Aegis-Fauna Biometrics Corp";

        [Tooltip("Official municipal license and registration serial tag.")]
        public string certificationSerial = "AFC-REG-9021";

        [TextArea(2, 4)]
        [Tooltip("Commercial advertising and specifications summary.")]
        public string description = "Sanctioned Tier-1 combat specimen with integrated neural damping and certified kinetic plating.";

        [Header("Pricing & Contents")]
        [Tooltip("Retail price in Credits.")]
        [Min(0)]
        public int costCredits = 500;

        [Tooltip("The underlying species included in this starter pack.")]
        public MonsterDataSO monsterSpecies;

        [Tooltip("State legal registration category awarded on purchase.")]
        public RegistrationStatus registrationStatus = RegistrationStatus.Legal;

        [Header("Visuals")]
        [Tooltip("Kiosk catalog thumbnail or corporate emblem.")]
        public Sprite packIcon;

        /// <summary>
        /// Creates a freshly minted, legally certified MonsterInstance for the player's roster.
        /// </summary>
        public MonsterInstance CreateMonsterInstance()
        {
            if (monsterSpecies == null)
            {
                Debug.LogError($"[CorporatePack] Pack '{packName}' has no MonsterDataSO assigned!");
                return null;
            }

            var instance = new MonsterInstance(monsterSpecies, registrationStatus)
            {
                nickname = $"{monsterSpecies.commonName} [AFC-CERT]"
            };

            return instance;
        }
    }
}
