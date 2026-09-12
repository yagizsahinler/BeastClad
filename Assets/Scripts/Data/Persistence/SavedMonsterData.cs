using System;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Data.Persistence
{
    /// <summary>
    /// Serializable Data Transfer Object (DTO) for storing a single MonsterInstance.
    /// Decouples immutable ScriptableObject references (MonsterDataSO) into a string speciesId
    /// that can be serialized to JSON and hydrated back via MonsterDatabaseSO.
    /// </summary>
    [Serializable]
    public class SavedMonsterData
    {
        [Tooltip("Unique runtime specimen identifier.")]
        public string instanceId;

        [Tooltip("Identifier linking to MonsterDataSO.speciesId.")]
        public string speciesId;

        [Tooltip("Custom specimen name or tag.")]
        public string nickname;

        [Tooltip("Municipal legal registration status.")]
        public RegistrationStatus registrationStatus;

        [Tooltip("Biological stability percentage (100% down to 0%).")]
        public float stability = 100f;

        [Tooltip("ISO timestamp of specimen acquisition.")]
        public string captureTimestamp;

        public SavedMonsterData() { }

        public SavedMonsterData(MonsterInstance instance)
        {
            if (instance == null) return;

            instanceId = instance.instanceId;
            speciesId = instance.template != null ? instance.template.speciesId : "unknown_species";
            nickname = instance.nickname;
            registrationStatus = instance.registrationStatus;
            stability = instance.stability;
            captureTimestamp = instance.captureTimestamp;
        }

        /// <summary>
        /// Reconstructs a live MonsterInstance using the provided monster database.
        /// </summary>
        public MonsterInstance ToInstance(MonsterDatabaseSO database)
        {
            MonsterDataSO template = database != null ? database.GetSpecies(speciesId) : null;
            var instance = new MonsterInstance(template, registrationStatus)
            {
                instanceId = string.IsNullOrEmpty(instanceId) ? Guid.NewGuid().ToString().Substring(0, 8) : instanceId,
                nickname = string.IsNullOrEmpty(nickname) ? (template != null ? template.commonName : "Unknown Specimen") : nickname,
                stability = stability,
                captureTimestamp = string.IsNullOrEmpty(captureTimestamp) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : captureTimestamp
            };

            return instance;
        }

        public static SavedMonsterData FromInstance(MonsterInstance instance)
        {
            return instance != null ? new SavedMonsterData(instance) : null;
        }
    }
}
