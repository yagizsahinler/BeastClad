using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Security
{
    /// <summary>
    /// Interactive municipal quarantine locker terminal stationed at security checkpoints.
    /// Safely unequips and vaults unregistered wild specimens and black-market contraband
    /// from player loadout and carried roster, enabling biometric scanner clearance and
    /// allowing full gear recovery after tournament bouts.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class MunicipalQuarantineLocker : MonoBehaviour
    {
        [Header("Locker Identity")]
        [SerializeField] private string lockerId = "Municipal Quarantine Locker 01";
        [SerializeField] private string checkpointSector = "Grand Colosseum North Gate";

        [Header("Linked Scanner")]
        [SerializeField] private MunicipalScannerZone linkedScanner;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer statusLight;
        [SerializeField] private TextMesh overheadLabel;
        [SerializeField] private Color emptyColor = new Color(0.1f, 0.85f, 1f, 1f);      // Cyan
        [SerializeField] private Color stashedColor = new Color(1f, 0.65f, 0.1f, 1f);   // Amber Warning

        [Header("Quarantine Vault Storage")]
        [SerializeField] private List<MonsterInstance> quarantineVault = new List<MonsterInstance>();

        private bool isPlayerNear = false;
        private GameObject cachedPlayer;

        public string LockerId => lockerId;
        public string CheckpointSector => checkpointSector;
        public IReadOnlyList<MonsterInstance> StoredInstances => quarantineVault;
        public int StoredCount => quarantineVault.Count;
        public MunicipalScannerZone LinkedScanner => linkedScanner;

        public static event Action<MunicipalQuarantineLocker, bool, string> OnPromptChanged;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
            UpdateVisuals();
        }

        private void Update()
        {
            if (!isPlayerNear) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            var infuse = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerInfuseManager>() : FindAnyObjectByType<PlayerInfuseManager>();
            var roster = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerMonsterRoster>() : FindAnyObjectByType<PlayerMonsterRoster>();

            int illegalCount = CountIllegalItems(infuse, roster);

            if (kb.fKey.wasPressedThisFrame)
            {
                if (illegalCount > 0)
                {
                    int deposited = DepositAllIllegalItems(infuse, roster);
                    string msg = $"<color=#00FFAA>✔ QUARANTINED: {deposited} illegal specimen(s) vaulted. Scanners cleared!</color>";
                    Debug.Log($"<color=#00FFAA>[Quarantine Locker]</color> {msg}");
                    OnPromptChanged?.Invoke(this, true, msg);
                }
                else if (quarantineVault.Count > 0)
                {
                    int retrieved = RetrieveAllQuarantinedItems(roster);
                    string msg = $"<color=#FFAA00>✔ RECLAIMED: {retrieved} specimen(s) returned to roster. Scanners will flag these!</color>";
                    Debug.Log($"<color=#FFAA00>[Quarantine Locker]</color> {msg}");
                    OnPromptChanged?.Invoke(this, true, msg);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNear = true;
                cachedPlayer = other.gameObject;
                RefreshPrompt();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNear = false;
                cachedPlayer = null;
                OnPromptChanged?.Invoke(this, false, string.Empty);
            }
        }

        public void RefreshPrompt()
        {
            if (!isPlayerNear) return;

            var infuse = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerInfuseManager>() : FindAnyObjectByType<PlayerInfuseManager>();
            var roster = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerMonsterRoster>() : FindAnyObjectByType<PlayerMonsterRoster>();

            int illegalCount = CountIllegalItems(infuse, roster);

            if (illegalCount > 0)
            {
                OnPromptChanged?.Invoke(this, true, $"[ F ] Quarantine Locker: Unequip & Vault Contraband ({illegalCount} Detected)");
            }
            else if (quarantineVault.Count > 0)
            {
                OnPromptChanged?.Invoke(this, true, $"[ F ] Quarantine Locker: Reclaim Stashed Gear ({quarantineVault.Count} Vaulted)");
            }
            else
            {
                OnPromptChanged?.Invoke(this, true, "Municipal Quarantine Locker: Compliant (Vault Empty)");
            }
        }

        /// <summary>
        /// Counts all equipped and rostered specimens violating legal municipal biometrics.
        /// </summary>
        public int CountIllegalItems(PlayerInfuseManager infuse, PlayerMonsterRoster roster)
        {
            int count = 0;

            if (infuse != null)
            {
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    var monster = infuse.GetEquippedMonster(slot);
                    if (monster != null && infuse.GetSlotRegistration(slot) != RegistrationStatus.Legal)
                    {
                        count++;
                    }
                }
            }

            if (roster != null && roster.Roster != null)
            {
                foreach (var item in roster.Roster)
                {
                    if (item != null && item.registrationStatus != RegistrationStatus.Legal)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Unequips all illegal equipped modules, moves carried roster contraband into the locker vault,
        /// recalculates stats, and re-triggers linked scanner evaluation.
        /// </summary>
        public int DepositAllIllegalItems(PlayerInfuseManager infuse, PlayerMonsterRoster roster)
        {
            if (infuse == null) infuse = FindAnyObjectByType<PlayerInfuseManager>();
            if (roster == null) roster = FindAnyObjectByType<PlayerMonsterRoster>();

            int count = 0;

            // 1. Unequip and vault equipped illegal modules
            if (infuse != null)
            {
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    var monsterData = infuse.GetEquippedMonster(slot);
                    if (monsterData != null)
                    {
                        var status = infuse.GetSlotRegistration(slot);
                        if (status != RegistrationStatus.Legal)
                        {
                            var instance = infuse.GetEquippedInstance(slot);
                            if (instance == null)
                            {
                                instance = new MonsterInstance(monsterData, status)
                                {
                                    nickname = $"{monsterData.commonName} [{status}]"
                                };
                            }

                            quarantineVault.Add(instance);
                            infuse.UnequipMonster(slot);
                            count++;
                            Debug.Log($"<color=#00FFAA>[Quarantine Locker]</color> Unequipped & vaulted {slot} module: <b>{instance.nickname}</b>");
                        }
                    }
                }

                infuse.RecalculateAllStats();
            }

            // 2. Vault carried contraband from player roster
            if (roster != null && roster.Roster != null)
            {
                var illegalRosterItems = new List<MonsterInstance>();
                foreach (var item in roster.Roster)
                {
                    if (item != null && item.registrationStatus != RegistrationStatus.Legal)
                    {
                        illegalRosterItems.Add(item);
                    }
                }

                foreach (var illegal in illegalRosterItems)
                {
                    quarantineVault.Add(illegal);
                    roster.RemoveMonster(illegal);
                    count++;
                    Debug.Log($"<color=#00FFAA>[Quarantine Locker]</color> Vaulted carried roster specimen: <b>{illegal.nickname}</b>");
                }
            }

            UpdateVisuals();

            // 3. Immediately re-evaluate linked scanner
            if (linkedScanner != null)
            {
                linkedScanner.PerformScan(infuse, roster);
            }

            RefreshPrompt();
            BeastClad.Persistence.SaveManager.Instance.SaveCurrentGame();
            return count;
        }

        /// <summary>
        /// Returns all stashed specimens from the vault back to the player's monster roster.
        /// </summary>
        public int RetrieveAllQuarantinedItems(PlayerMonsterRoster roster)
        {
            if (roster == null) roster = FindAnyObjectByType<PlayerMonsterRoster>();

            int count = quarantineVault.Count;
            if (roster != null)
            {
                foreach (var item in quarantineVault)
                {
                    if (item != null)
                    {
                        roster.AddMonster(item);
                    }
                }
            }

            quarantineVault.Clear();
            UpdateVisuals();

            var infuse = FindAnyObjectByType<PlayerInfuseManager>();
            if (linkedScanner != null && (isPlayerNear || linkedScanner.StationedEnforcer != null))
            {
                linkedScanner.PerformScan(infuse, roster);
            }

            RefreshPrompt();
            BeastClad.Persistence.SaveManager.Instance.SaveCurrentGame();
            return count;
        }

        private void UpdateVisuals()
        {
            bool hasItems = quarantineVault.Count > 0;
            if (statusLight != null)
            {
                statusLight.color = hasItems ? stashedColor : emptyColor;
            }

            if (overheadLabel != null)
            {
                overheadLabel.text = hasItems
                    ? $"QUARANTINE LOCKER\n[{quarantineVault.Count} CONTRABAND VAULTED]"
                    : "QUARANTINE LOCKER\n[SECURE • READY]";
                overheadLabel.color = hasItems ? stashedColor : emptyColor;
            }
        }

        public void SetStoredInstances(List<MonsterInstance> items)
        {
            quarantineVault.Clear();
            if (items != null) quarantineVault.AddRange(items);
            UpdateVisuals();
        }

        public void SetReferences(MunicipalScannerZone scanner, SpriteRenderer light, TextMesh label)
        {
            linkedScanner = scanner;
            statusLight = light;
            overheadLabel = label;
            UpdateVisuals();
        }
    }
}
