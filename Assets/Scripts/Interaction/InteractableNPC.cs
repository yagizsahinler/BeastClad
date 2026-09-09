using System;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Data;
using BeastClad.Player;
using BeastClad.Trapping;

namespace BeastClad.Interaction
{
    /// <summary>
    /// Friendly NPC in the world capable of conversing with the player,
    /// offering Guild hunting contracts, verifying captured specimens,
    /// and dispensing bounty payouts.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractableNPC : MonoBehaviour
    {
        [Header("NPC Identity")]
        [SerializeField] private string npcName = "Guildmaster Vane";
        [SerializeField] private string roleTitle = "Adventurer's Guild Representative";

        [Header("Contract Setup")]
        [SerializeField] private GuildContractSO offeredContract;

        // Current runtime contract status
        private ContractStatus currentStatus = ContractStatus.Available;
        private bool isPlayerNearby;
        private PlayerTrapperComponent cachedTrapper;
        private PlayerWallet cachedWallet;

        public string NPCName => npcName;
        public string RoleTitle => roleTitle;
        public GuildContractSO Contract => offeredContract;
        public ContractStatus Status => currentStatus;

        // Decoupled UI Events
        public static event Action<InteractableNPC, bool> OnInteractionPrompt;
        public static event Action<InteractableNPC, GuildContractSO, ContractStatus, string, bool> OnDialogueStateChanged;
        public static event Action OnDialogueClosed;

        public event Action<GuildContractSO> OnContractAccepted;
        public event Action<GuildContractSO, MonsterInstance> OnContractTurnedIn;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (!isPlayerNearby) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            // Interact [F]
            if (kb.fKey.wasPressedThisFrame)
            {
                Interact();
            }

            // Dismiss [Escape]
            if (kb.escapeKey.wasPressedThisFrame)
            {
                CloseDialogue();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNearby = true;
                cachedTrapper = other.GetComponent<PlayerTrapperComponent>();
                cachedWallet = other.GetComponent<PlayerWallet>();
                OnInteractionPrompt?.Invoke(this, true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNearby = false;
                cachedTrapper = null;
                cachedWallet = null;
                OnInteractionPrompt?.Invoke(this, false);
                OnDialogueClosed?.Invoke();
            }
        }

        public void Interact()
        {
            if (offeredContract == null)
            {
                string idleDialogue = "The wild sectors are restless today. Tread carefully beyond the perimeter.";
                OnDialogueStateChanged?.Invoke(this, null, ContractStatus.Completed, idleDialogue, false);
                return;
            }

            switch (currentStatus)
            {
                case ContractStatus.Available:
                    // Briefing and acceptance
                    currentStatus = ContractStatus.Active;
                    OnContractAccepted?.Invoke(offeredContract);
                    Debug.Log($"<color=#FFCC00>[Guild Contract Accepted]</color> <b>{offeredContract.contractTitle}</b>!");
                    OnDialogueStateChanged?.Invoke(this, offeredContract, currentStatus, offeredContract.dialogueBriefing, false);
                    break;

                case ContractStatus.Active:
                    var targetSpecimen = FindMatchingSpecimenInPlayerCrate();
                    if (targetSpecimen != null)
                    {
                        // Player possesses target specimen -> Turn in!
                        CompleteContract(targetSpecimen);
                    }
                    else
                    {
                        // Contract in progress
                        OnDialogueStateChanged?.Invoke(this, offeredContract, currentStatus, offeredContract.dialogueInProgress, false);
                    }
                    break;

                case ContractStatus.Completed:
                    OnDialogueStateChanged?.Invoke(this, offeredContract, currentStatus, offeredContract.dialogueCompleted, false);
                    break;
            }
        }

        public bool CheckHasRequiredSpecimen()
        {
            return FindMatchingSpecimenInPlayerCrate() != null;
        }

        private MonsterInstance FindMatchingSpecimenInPlayerCrate()
        {
            if (cachedTrapper == null || offeredContract == null || offeredContract.targetSpecies == null)
            {
                return null;
            }

            foreach (var specimen in cachedTrapper.CapturedMonsters)
            {
                if (specimen != null && specimen.template == offeredContract.targetSpecies)
                {
                    return specimen;
                }
            }

            return null;
        }

        public void CompleteContract(MonsterInstance specimen)
        {
            if (currentStatus == ContractStatus.Completed) return;

            currentStatus = ContractStatus.Completed;

            // Remove specimen from player transport crate
            if (cachedTrapper != null && specimen != null)
            {
                cachedTrapper.CapturedMonsters.Remove(specimen);
                Debug.Log($"<color=#00FFAA>[Guild Turn-In]</color> Delivered specimen <b>{specimen.nickname}</b> (ID: {specimen.instanceId}) to {npcName}.");
            }

            // Award Credits to player wallet
            if (cachedWallet != null && offeredContract != null)
            {
                cachedWallet.AddCredits(offeredContract.rewardCredits);
            }

            OnContractTurnedIn?.Invoke(offeredContract, specimen);
            OnDialogueStateChanged?.Invoke(this, offeredContract, currentStatus, offeredContract.dialogueCompleted, true);
        }

        public void CloseDialogue()
        {
            OnDialogueClosed?.Invoke();
        }

        public void SetContract(GuildContractSO contract)
        {
            offeredContract = contract;
            currentStatus = ContractStatus.Available;
        }
    }
}
