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

            if (notifyAndRecalculate)
            {
                RecalculateAllStats();
                OnInfuseChanged?.Invoke(slot, monster, module);
            }

            return true;
        }

        /// <summary>
        /// Removes any monster equipped to the specified socket.
        /// </summary>
        public void UnequipMonster(EquipmentSlot slot)
        {
            equippedMonsters.Remove(slot);
            equippedModules.Remove(slot);

            RecalculateAllStats();
            OnInfuseChanged?.Invoke(slot, null, null);
        }

        public MonsterDataSO GetEquippedMonster(EquipmentSlot slot)
        {
            return equippedMonsters.TryGetValue(slot, out var monster) ? monster : null;
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
