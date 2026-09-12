using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastClad.Data.Persistence
{
    /// <summary>
    /// Master root serializable contract for player progression, economy, and inventory persistence.
    /// Written atomically to Application.persistentDataPath/savegame.json.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        [Header("Save Metadata")]
        public int saveVersion = 1;
        public string lastSavedTimestamp;
        public string lastSceneName = "District_CentralHub";

        [Header("Economy & League Standings")]
        public int credits = 1000;
        public int arenaRankIndex = 0;
        public string arenaRankTitle = "Bronze League - Rank III";

        [Header("Specimen Roster & Loadout")]
        public List<SavedMonsterData> rosterMonsters = new List<SavedMonsterData>();
        public List<SavedEquippedSlot> equippedSlots = new List<SavedEquippedSlot>();
        public List<SavedMonsterData> quarantinedMonsters = new List<SavedMonsterData>();

        [Header("Narrative & Quest Progression")]
        public List<string> storyFlags = new List<string>();

        public PlayerSaveData()
        {
            lastSavedTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Creates a fresh default game state for a new player.
        /// </summary>
        public static PlayerSaveData CreateDefault()
        {
            var data = new PlayerSaveData
            {
                saveVersion = 1,
                lastSavedTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                lastSceneName = "District_CentralHub",
                credits = 1000,
                arenaRankIndex = 0,
                arenaRankTitle = "Bronze League - Rank III",
                rosterMonsters = new List<SavedMonsterData>(),
                equippedSlots = new List<SavedEquippedSlot>(),
                quarantinedMonsters = new List<SavedMonsterData>(),
                storyFlags = new List<string> { "GameStarted" }
            };

            return data;
        }

        public bool HasFlag(string flag)
        {
            return !string.IsNullOrEmpty(flag) && storyFlags.Contains(flag);
        }

        public void SetFlag(string flag, bool state = true)
        {
            if (string.IsNullOrEmpty(flag)) return;

            if (state && !storyFlags.Contains(flag))
            {
                storyFlags.Add(flag);
            }
            else if (!state && storyFlags.Contains(flag))
            {
                storyFlags.Remove(flag);
            }
        }
    }
}
