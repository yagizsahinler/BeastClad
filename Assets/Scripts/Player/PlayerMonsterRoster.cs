using System;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Player
{
    /// <summary>
    /// Manages the player's collection of acquired MonsterInstance specimens
    /// obtained from wild trapping, corporate purchases, or black market contracts.
    /// Acts as the central vault and inventory for monsters ready to be infused into gear.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerMonsterRoster : MonoBehaviour
    {
        [Header("Runtime Specimen Roster")]
        [SerializeField] private List<MonsterInstance> roster = new List<MonsterInstance>();

        public IReadOnlyList<MonsterInstance> Roster => roster;
        public int Count => roster.Count;

        public event Action<MonsterInstance> OnMonsterAdded;
        public event Action<MonsterInstance> OnMonsterRemoved;

        /// <summary>
        /// Registers an acquired monster instance to the player's personal roster.
        /// </summary>
        public void AddMonster(MonsterInstance monster)
        {
            if (monster == null) return;

            roster.Add(monster);
            Debug.Log($"<color=#00FFCC>[Monster Roster]</color> Added <b>{monster.nickname}</b> " +
                      $"({(monster.template != null ? monster.template.commonName : "Unknown")}) " +
                      $"| Status: <b>{monster.registrationStatus}</b> | Total in Roster: {roster.Count}");

            OnMonsterAdded?.Invoke(monster);
        }

        /// <summary>
        /// Removes a monster instance from the player's roster (e.g., when turning in a bounty or selling).
        /// </summary>
        public bool RemoveMonster(MonsterInstance monster)
        {
            if (monster == null) return false;

            bool removed = roster.Remove(monster);
            if (removed)
            {
                Debug.Log($"<color=#FF9999>[Monster Roster]</color> Removed <b>{monster.nickname}</b> from roster. Remaining: {roster.Count}");
                OnMonsterRemoved?.Invoke(monster);
            }
            return removed;
        }

        /// <summary>
        /// Checks if the roster contains any specimen matching the given species template.
        /// </summary>
        public bool HasMonsterSpecies(MonsterDataSO species)
        {
            if (species == null) return false;
            for (int i = 0; i < roster.Count; i++)
            {
                if (roster[i] != null && roster[i].template == species)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves all specimens with a specific legal registration status.
        /// </summary>
        public List<MonsterInstance> GetMonstersByRegistration(RegistrationStatus status)
        {
            var results = new List<MonsterInstance>();
            for (int i = 0; i < roster.Count; i++)
            {
                if (roster[i] != null && roster[i].registrationStatus == status)
                {
                    results.Add(roster[i]);
                }
            }
            return results;
        }
    }
}
