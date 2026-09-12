using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastClad.Data
{
    /// <summary>
    /// Central catalog and registry of all MonsterDataSO species assets in BeastClad.
    /// Provides high-speed O(1) dictionary lookup by speciesId for save/load hydration.
    /// Typically loaded from Assets/Resources/MonsterDatabase.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterDatabase", menuName = "BeastClad/Data/Monster Database")]
    public class MonsterDatabaseSO : ScriptableObject
    {
        private const string DefaultResourcePath = "MonsterDatabase";
        private static MonsterDatabaseSO cachedInstance;

        [Header("Registered Monster Species")]
        [SerializeField] private List<MonsterDataSO> allMonsters = new List<MonsterDataSO>();

        private readonly Dictionary<string, MonsterDataSO> speciesLookup = new Dictionary<string, MonsterDataSO>(StringComparer.OrdinalIgnoreCase);
        private bool isInitialized = false;

        public IReadOnlyList<MonsterDataSO> AllMonsters => allMonsters;

        private void OnEnable()
        {
            InitializeLookup();
        }

        public void InitializeLookup()
        {
            speciesLookup.Clear();
            if (allMonsters != null)
            {
                foreach (var monster in allMonsters)
                {
                    if (monster != null && !string.IsNullOrEmpty(monster.speciesId))
                    {
                        if (!speciesLookup.ContainsKey(monster.speciesId))
                        {
                            speciesLookup.Add(monster.speciesId, monster);
                        }
                    }
                }
            }
            isInitialized = true;
        }

        /// <summary>
        /// Retrieves a MonsterDataSO species by its unique speciesId string.
        /// Returns null if not found.
        /// </summary>
        public MonsterDataSO GetSpecies(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId)) return null;

            if (!isInitialized || speciesLookup.Count == 0)
            {
                InitializeLookup();
            }

            if (speciesLookup.TryGetValue(speciesId, out var monster))
            {
                return monster;
            }

            Debug.LogWarning($"<color=#FF9900>[MonsterDatabase]</color> Species '{speciesId}' not found in registry!");
            return null;
        }

        public void SetMonsters(List<MonsterDataSO> monsters)
        {
            allMonsters = monsters != null ? new List<MonsterDataSO>(monsters) : new List<MonsterDataSO>();
            InitializeLookup();
        }

        /// <summary>
        /// Global accessor to load the default MonsterDatabase from Resources.
        /// </summary>
        public static MonsterDatabaseSO LoadDefault()
        {
            if (cachedInstance != null) return cachedInstance;

            cachedInstance = Resources.Load<MonsterDatabaseSO>(DefaultResourcePath);
            if (cachedInstance == null)
            {
                Debug.LogWarning($"<color=#FF9900>[MonsterDatabase]</color> No '{DefaultResourcePath}' found in Resources! Attempting dynamic fallback.");
                cachedInstance = CreateInstance<MonsterDatabaseSO>();
                var loaded = Resources.LoadAll<MonsterDataSO>("");
                cachedInstance.SetMonsters(new List<MonsterDataSO>(loaded));
            }
            else
            {
                cachedInstance.InitializeLookup();
            }

            return cachedInstance;
        }
    }
}
