using System;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.World
{
    /// <summary>
    /// Lightweight static service that captures and holds runtime player state across scene transitions.
    /// Preserves currency, roster collections, equipped limb modules, and target spawn positioning.
    /// Non-destructive: if no transition occurred (e.g. running an isolated test scene directly in Editor),
    /// HasActiveSession is false and local scene defaults are used.
    /// </summary>
    public static class PlayerSessionState
    {
        public static bool HasActiveSession { get; private set; }
        public static int Credits { get; private set; }
        public static string PendingSpawnTag { get; set; } = string.Empty;

        private static readonly List<MonsterInstance> savedRoster = new List<MonsterInstance>();
        private static readonly Dictionary<EquipmentSlot, MonsterInstance> savedEquippedInstances = new Dictionary<EquipmentSlot, MonsterInstance>();
        private static readonly Dictionary<EquipmentSlot, MonsterDataSO> savedEquippedMonsters = new Dictionary<EquipmentSlot, MonsterDataSO>();

        public static IReadOnlyList<MonsterInstance> SavedRoster => savedRoster;

        /// <summary>
        /// Extracts and caches runtime state from the active player GameObject.
        /// </summary>
        public static void CaptureFromPlayer(GameObject player)
        {
            if (player == null) return;

            // 1. Wallet
            var wallet = player.GetComponent<PlayerWallet>();
            if (wallet != null)
            {
                Credits = wallet.Credits;
            }

            // 2. Monster Roster
            var roster = player.GetComponent<PlayerMonsterRoster>();
            savedRoster.Clear();
            if (roster != null && roster.Roster != null)
            {
                foreach (var m in roster.Roster)
                {
                    if (m != null) savedRoster.Add(m);
                }
            }

            // 3. Equipped Infuse Loadout
            var infuse = player.GetComponent<PlayerInfuseManager>();
            savedEquippedInstances.Clear();
            savedEquippedMonsters.Clear();
            if (infuse != null)
            {
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    var inst = infuse.GetEquippedInstance(slot);
                    var data = infuse.GetEquippedMonster(slot);

                    if (inst != null) savedEquippedInstances[slot] = inst;
                    else if (data != null) savedEquippedMonsters[slot] = data;
                }
            }

            HasActiveSession = true;
            Debug.Log($"<color=#38BDF8>[PlayerSessionState]</color> Captured state: <b>{Credits} Credits</b>, <b>{savedRoster.Count} Roster Specimens</b>, <b>{savedEquippedInstances.Count + savedEquippedMonsters.Count} Equipped Slots</b>.");
        }

        /// <summary>
        /// Hydrates a freshly loaded player GameObject with cached session data.
        /// </summary>
        public static void ApplyToPlayer(GameObject player)
        {
            if (player == null || !HasActiveSession) return;

            // 1. Wallet
            var wallet = player.GetComponent<PlayerWallet>();
            if (wallet != null)
            {
                wallet.SetCredits(Credits);
            }

            // 2. Monster Roster
            var roster = player.GetComponent<PlayerMonsterRoster>();
            if (roster != null)
            {
                roster.ClearRoster();
                foreach (var m in savedRoster)
                {
                    roster.AddMonster(m);
                }
            }

            // 3. Equipped Infuse Loadout
            var infuse = player.GetComponent<PlayerInfuseManager>();
            if (infuse != null)
            {
                // Clear any default slots first
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    infuse.UnequipMonster(slot);
                }

                // Restore instances first (with contraband/stability)
                foreach (var kvp in savedEquippedInstances)
                {
                    infuse.EquipMonster(kvp.Key, kvp.Value, false);
                }

                // Restore data templates if any
                foreach (var kvp in savedEquippedMonsters)
                {
                    if (!savedEquippedInstances.ContainsKey(kvp.Key))
                    {
                        infuse.EquipMonster(kvp.Key, kvp.Value, false);
                    }
                }

                infuse.RecalculateAllStats();
            }

            Debug.Log($"<color=#38BDF8>[PlayerSessionState]</color> Restored state onto player: <b>{Credits} Credits</b>, <b>{savedRoster.Count} Roster Specimens</b>.");
        }

        public static void ClearSession()
        {
            HasActiveSession = false;
            Credits = 0;
            PendingSpawnTag = string.Empty;
            savedRoster.Clear();
            savedEquippedInstances.Clear();
            savedEquippedMonsters.Clear();
        }
    }
}
