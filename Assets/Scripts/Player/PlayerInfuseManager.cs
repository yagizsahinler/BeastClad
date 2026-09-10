using System;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;
using BeastClad.Input;

namespace BeastClad.Player
{
    /// <summary>
    /// Core Infuse Manager for the player character.
    /// Manages equipping/unequipping monster modules across all 5 anatomical slots,
    /// recalculates composite stats, handles input dispatch, and triggers local cooldowns.
    /// </summary>
    [RequireComponent(typeof(PlayerStatsComponent))]
    [RequireComponent(typeof(CooldownController))]
    [DisallowMultipleComponent]
    public class PlayerInfuseManager : MonoBehaviour
    {
        [Header("Initial / Default Loadout (Optional)")]
        [SerializeField] private MonsterDataSO initialHeadMonster;
        [SerializeField] private MonsterDataSO initialChestMonster;
        [SerializeField] private MonsterDataSO initialLeftArmMonster;
        [SerializeField] private MonsterDataSO initialRightArmMonster;
        [SerializeField] private MonsterDataSO initialLegsMonster;

        // Internal slot state
        private readonly Dictionary<EquipmentSlot, MonsterDataSO> equippedMonsters = new Dictionary<EquipmentSlot, MonsterDataSO>();
        private readonly Dictionary<EquipmentSlot, InfusePartSO> equippedModules = new Dictionary<EquipmentSlot, InfusePartSO>();
        private readonly Dictionary<EquipmentSlot, MonsterInstance> equippedInstances = new Dictionary<EquipmentSlot, MonsterInstance>();

        // Components
        private PlayerStatsComponent playerStats;
        private CooldownController cooldownController;
        private PlayerController2D playerController;
        private PlayerInputReader inputReader;

        // Events
        public event Action<EquipmentSlot, MonsterDataSO, InfusePartSO> OnInfuseChanged;
        public event Action<EquipmentSlot, InfusePartSO> OnSkillExecuted;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStatsComponent>();
            cooldownController = GetComponent<CooldownController>();
            playerController = GetComponent<PlayerController2D>();
            inputReader = GetComponent<PlayerInputReader>();

            InitializeLoadout();
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnLeftArmTriggered += HandleLeftArmInput;
                inputReader.OnRightArmTriggered += HandleRightArmInput;
                inputReader.OnChestTriggered += HandleChestInput;
                inputReader.OnHeadTriggered += HandleHeadInput;
                inputReader.OnDashTriggered += HandleLegsInput;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnLeftArmTriggered -= HandleLeftArmInput;
                inputReader.OnRightArmTriggered -= HandleRightArmInput;
                inputReader.OnChestTriggered -= HandleChestInput;
                inputReader.OnHeadTriggered -= HandleHeadInput;
                inputReader.OnDashTriggered -= HandleLegsInput;
            }
        }

        private void Start()
        {
            RecalculateAllStats();
        }

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            // Debug hotkeys for testing registration tiers
            if (kb.digit8Key.wasPressedThisFrame)
            {
                EquipLegalTestLoadout();
            }
            else if (kb.digit9Key.wasPressedThisFrame)
            {
                EquipUnregisteredTestLoadout();
            }
            else if (kb.digit0Key.wasPressedThisFrame)
            {
                EquipContrabandTestLoadout();
            }
        }

        public void EquipLegalTestLoadout()
        {
            if (initialHeadMonster != null) EquipMonster(EquipmentSlot.Head, initialHeadMonster, false);
            if (initialChestMonster != null) EquipMonster(EquipmentSlot.Chest, initialChestMonster, false);
            if (initialRightArmMonster != null) EquipMonster(EquipmentSlot.RightArm, initialRightArmMonster, false);
            if (initialLegsMonster != null) EquipMonster(EquipmentSlot.Legs, initialLegsMonster, false);

            if (initialLeftArmMonster != null)
            {
                var legalWyrm = new MonsterInstance(initialLeftArmMonster, RegistrationStatus.Legal)
                {
                    nickname = $"{initialLeftArmMonster.commonName} [AFC-CERT]"
                };
                EquipMonster(EquipmentSlot.LeftArm, legalWyrm, true);
            }
            Debug.Log("<color=#00FFAA>[Infuse Debug]</color> Switched to <b>ALL LEGAL</b> Corporate Loadout! (Keys: [8] Legal, [9] Wild Unregistered, [0] Contraband)");
        }

        public void EquipUnregisteredTestLoadout()
        {
            if (initialLeftArmMonster != null)
            {
                var wildWyrm = new MonsterInstance(initialLeftArmMonster, RegistrationStatus.Unregistered)
                {
                    nickname = $"Wild {initialLeftArmMonster.commonName}"
                };
                EquipMonster(EquipmentSlot.LeftArm, wildWyrm, true);
            }
            Debug.Log("<color=#FFAA00>[Infuse Debug]</color> Equipped <b>UNREGISTERED</b> Wild Beast into Left Arm! (Keys: [8] Legal, [9] Wild Unregistered, [0] Contraband)");
        }

        public void EquipContrabandTestLoadout()
        {
            if (initialRightArmMonster != null)
            {
                var contraband = new MonsterInstance(initialRightArmMonster, RegistrationStatus.Contraband)
                {
                    nickname = $"{initialRightArmMonster.commonName} [BLACK-MARKET OVERCLOCKED]"
                };
                EquipMonster(EquipmentSlot.RightArm, contraband, true);
            }
            Debug.Log("<color=#FF0044>[Infuse Debug]</color> Equipped <b>CONTRABAND</b> Overclocked Beast into Right Arm! (Keys: [8] Legal, [9] Wild Unregistered, [0] Contraband)");
        }

        private void InitializeLoadout()
        {
            if (initialHeadMonster != null) EquipMonster(EquipmentSlot.Head, initialHeadMonster, true);
            if (initialChestMonster != null) EquipMonster(EquipmentSlot.Chest, initialChestMonster, true);
            if (initialLeftArmMonster != null) EquipMonster(EquipmentSlot.LeftArm, initialLeftArmMonster, true);
            if (initialRightArmMonster != null) EquipMonster(EquipmentSlot.RightArm, initialRightArmMonster, true);
            if (initialLegsMonster != null) EquipMonster(EquipmentSlot.Legs, initialLegsMonster, true);
        }

        public void SetDefaultMonsters(MonsterDataSO head, MonsterDataSO chest, MonsterDataSO leftArm, MonsterDataSO rightArm, MonsterDataSO legs)
        {
            initialHeadMonster = head;
            initialChestMonster = chest;
            initialLeftArmMonster = leftArm;
            initialRightArmMonster = rightArm;
            initialLegsMonster = legs;
            InitializeLoadout();
        }

        /// <summary>
        /// Equips a monster to the specified anatomical socket.
        /// </summary>
        public bool EquipMonster(EquipmentSlot slot, MonsterDataSO monster, bool notifyAndRecalculate = true)
        {
            if (monster == null)
            {
                UnequipMonster(slot);
                return true;
            }

            InfusePartSO module = monster.GetModuleForSlot(slot);
            if (module == null)
            {
                Debug.LogWarning($"[InfuseManager] {monster.commonName} has no valid module for {slot}!");
                return false;
            }

            equippedMonsters[slot] = monster;
            equippedModules[slot] = module;
            equippedInstances.Remove(slot);

            if (notifyAndRecalculate)
            {
                RecalculateAllStats();
                OnInfuseChanged?.Invoke(slot, monster, module);
            }

            return true;
        }

        /// <summary>
        /// Equips a specific runtime MonsterInstance (with its unique registration status) to a socket.
        /// </summary>
        public bool EquipMonster(EquipmentSlot slot, MonsterInstance instance, bool notifyAndRecalculate = true)
        {
            if (instance == null || instance.template == null)
            {
                UnequipMonster(slot);
                return true;
            }

            bool success = EquipMonster(slot, instance.template, notifyAndRecalculate);
            if (success)
            {
                equippedInstances[slot] = instance;
            }
            return success;
        }

        /// <summary>
        /// Removes any monster equipped to the specified socket.
        /// </summary>
        public void UnequipMonster(EquipmentSlot slot)
        {
            equippedMonsters.Remove(slot);
            equippedModules.Remove(slot);
            equippedInstances.Remove(slot);

            RecalculateAllStats();
            OnInfuseChanged?.Invoke(slot, null, null);
        }

        public MonsterDataSO GetEquippedMonster(EquipmentSlot slot)
        {
            return equippedMonsters.TryGetValue(slot, out var monster) ? monster : null;
        }

        public MonsterInstance GetEquippedInstance(EquipmentSlot slot)
        {
            return equippedInstances.TryGetValue(slot, out var instance) ? instance : null;
        }

        /// <summary>
        /// Returns the municipal legal registration status of the monster in the specified slot.
        /// </summary>
        public RegistrationStatus GetSlotRegistration(EquipmentSlot slot)
        {
            if (equippedInstances.TryGetValue(slot, out var instance) && instance != null)
            {
                return instance.registrationStatus;
            }

            if (equippedMonsters.TryGetValue(slot, out var monster) && monster != null)
            {
                return monster.defaultRegistration;
            }

            return RegistrationStatus.Legal;
        }

        public InfusePartSO GetEquippedModule(EquipmentSlot slot)
        {
            return equippedModules.TryGetValue(slot, out var module) ? module : null;
        }

        /// <summary>
        /// Aggregates all stat modifiers across the 5 slots and propagates to player stats & controller.
        /// </summary>
        public void RecalculateAllStats()
        {
            var totalModifiers = StatModifierGroup.Zero;

            foreach (var kvp in equippedModules)
            {
                if (kvp.Value != null)
                {
                    totalModifiers += kvp.Value.statModifiers;
                }
            }

            if (playerStats != null)
            {
                playerStats.ApplyModifiers(totalModifiers);
            }

            if (playerController != null)
            {
                playerController.SetMoveSpeed(playerStats != null ? playerStats.MoveSpeed : 6.5f);

                // If legs slot provides specific dash parameters, apply them
                if (equippedModules.TryGetValue(EquipmentSlot.Legs, out var legsModule) && legsModule != null)
                {
                    playerController.SetDashParameters(
                        speed: 20f + legsModule.statModifiers.bonusMoveSpeed * 2f,
                        duration: 0.18f,
                        cooldown: legsModule.localCooldownDuration
                    );
                }
            }
        }

        private bool isCombatEnabled = true;
        public bool IsCombatEnabled => isCombatEnabled;

        public void SetCombatEnabled(bool enabled)
        {
            isCombatEnabled = enabled;
        }

        #region Input Handlers & Skill Dispatch

        private void HandleLeftArmInput() => TryExecuteSlot(EquipmentSlot.LeftArm);
        private void HandleRightArmInput() => TryExecuteSlot(EquipmentSlot.RightArm);
        private void HandleChestInput() => TryExecuteSlot(EquipmentSlot.Chest);
        private void HandleHeadInput() => TryExecuteSlot(EquipmentSlot.Head);
        private void HandleLegsInput() => TryExecuteSlot(EquipmentSlot.Legs);

        /// <summary>
        /// Attempts to execute an action for the specified limb slot, respecting local cooldowns and GCD.
        /// </summary>
        public bool TryExecuteSlot(EquipmentSlot slot)
        {
            if (!isCombatEnabled || cooldownController == null || !cooldownController.CanExecute(slot))
            {
                return false;
            }

            if (!equippedModules.TryGetValue(slot, out var module) || module == null)
            {
                // Uninfused fallback
                if (slot == EquipmentSlot.Legs && playerController != null)
                {
                    playerController.TryPerformDash();
                    cooldownController.TriggerCooldown(slot, 2.0f);
                    return true;
                }
                return false;
            }

            // Put slot on cooldown and trigger GCD
            cooldownController.TriggerCooldown(slot, module.localCooldownDuration);

            // Execute specific slot behavior
            if (slot == EquipmentSlot.Legs && playerController != null)
            {
                playerController.TryPerformDash();
            }

            string skillName = module.activeSkill != null ? module.activeSkill.skillName : module.partName;
            string elemName = module.activeSkill?.element != null ? module.activeSkill.element.displayName : "Neutral";
            Debug.Log($"<color=#00FFCC>[Infuse Combat]</color> Executed <b>{slot}</b>: <i>{skillName}</i> ({elemName}) | Cooldown: {module.localCooldownDuration:F1}s | GCD: {cooldownController.GCDDuration:F1}s");

            OnSkillExecuted?.Invoke(slot, module);
            return true;
        }

        #endregion
    }
}
