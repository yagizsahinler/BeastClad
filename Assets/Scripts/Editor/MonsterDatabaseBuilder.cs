using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Editor
{
    /// <summary>
    /// Editor utility that gathers all MonsterDataSO assets in the project
    /// and populates the master MonsterDatabase.asset in Assets/Resources.
    /// </summary>
    public static class MonsterDatabaseBuilder
    {
        private const string ResourcesFolderPath = "Assets/Resources";
        private const string DatabaseAssetPath = "Assets/Resources/MonsterDatabase.asset";

        [MenuItem("BeastClad/Build Monster Database")]
        public static MonsterDatabaseSO BuildDatabase()
        {
            // Ensure Resources directory exists
            if (!AssetDatabase.IsValidFolder(ResourcesFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                Debug.Log($"<color=#00FFAA>[MonsterDatabaseBuilder]</color> Created directory: {ResourcesFolderPath}");
            }

            // Find all MonsterDataSO assets
            string[] guids = AssetDatabase.FindAssets("t:MonsterDataSO");
            var monsterList = new List<MonsterDataSO>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var monster = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(path);
                if (monster != null)
                {
                    monsterList.Add(monster);
                    Debug.Log($"<color=#38BDF8>[MonsterDatabaseBuilder]</color> Indexed monster: <b>{monster.commonName}</b> (speciesId: '{monster.speciesId}') at {path}");
                }
            }

            // Load existing or create new MonsterDatabaseSO
            var database = AssetDatabase.LoadAssetAtPath<MonsterDatabaseSO>(DatabaseAssetPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<MonsterDatabaseSO>();
                AssetDatabase.CreateAsset(database, DatabaseAssetPath);
                Debug.Log($"<color=#00FFAA>[MonsterDatabaseBuilder]</color> Created new MonsterDatabaseSO at {DatabaseAssetPath}");
            }

            database.SetMonsters(monsterList);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FFAA>[MonsterDatabaseBuilder]</color> <b>Successfully built MonsterDatabase!</b> Indexed {monsterList.Count} species.");
            return database;
        }
    }
}
