using System;
using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Pure C# runtime instance of an acquired monster.
    /// Separates immutable species data (MonsterDataSO) from live individual state
    /// (capture timestamp, nickname, health, registration, and stability).
    /// </summary>
    [Serializable]
    public class MonsterInstance
    {
        [Tooltip("Unique individual specimen identifier.")]
        public string instanceId;

        [Tooltip("Underlying species data template.")]
        public MonsterDataSO template;

        [Tooltip("Custom specimen name or tag.")]
        public string nickname;

        [Tooltip("Municipal legal registration status (Wild caught beasts are Unregistered).")]
        public RegistrationStatus registrationStatus;

        [Tooltip("Biological stability percentage (100% down to 0%).")]
        public float stability = 100f;

        [Tooltip("ISO timestamp of when this specimen was trapped.")]
        public string captureTimestamp;

        public MonsterInstance(MonsterDataSO species, RegistrationStatus status = RegistrationStatus.Unregistered)
        {
            instanceId = Guid.NewGuid().ToString().Substring(0, 8);
            template = species;
            nickname = species != null ? species.commonName : "Wild Specimen";
            registrationStatus = status;
            stability = 100f;
            captureTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
