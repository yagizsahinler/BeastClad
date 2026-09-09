using UnityEngine;
using UnityEngine.UI;
using BeastClad.Data;
using BeastClad.Interaction;

namespace BeastClad.UI
{
    /// <summary>
    /// UI Manager for NPC interactions, Guild contract briefings,
    /// objective tracking, and dialogue presentation on the HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public class GuildDialogueUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject interactionPromptPanel;
        [SerializeField] private GameObject dialogueModalPanel;

        [Header("Prompt UI Elements")]
        [SerializeField] private Text promptText;

        [Header("Dialogue Modal Elements")]
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text speakerRoleText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private Text objectiveStatusText;
        [SerializeField] private Text rewardSummaryText;
        [SerializeField] private Text footerHintText;

        private void Awake()
        {
            if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
            if (dialogueModalPanel != null) dialogueModalPanel.SetActive(false);
        }

        private void OnEnable()
        {
            InteractableNPC.OnInteractionPrompt += HandleInteractionPrompt;
            InteractableNPC.OnDialogueStateChanged += HandleDialogueStateChanged;
            InteractableNPC.OnDialogueClosed += HandleDialogueClosed;
        }

        private void OnDisable()
        {
            InteractableNPC.OnInteractionPrompt -= HandleInteractionPrompt;
            InteractableNPC.OnDialogueStateChanged -= HandleDialogueStateChanged;
            InteractableNPC.OnDialogueClosed -= HandleDialogueClosed;
        }

        private void HandleInteractionPrompt(InteractableNPC npc, bool show)
        {
            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(show);
                if (show && promptText != null && npc != null)
                {
                    promptText.text = $"[ F ] Speak with {npc.NPCName}";
                }
            }
        }

        private void HandleDialogueStateChanged(InteractableNPC npc, GuildContractSO contract, ContractStatus status, string text, bool justCompleted)
        {
            if (dialogueModalPanel == null) return;

            dialogueModalPanel.SetActive(true);

            if (speakerNameText != null)
            {
                speakerNameText.text = npc != null ? npc.NPCName : "Unknown";
            }

            if (speakerRoleText != null)
            {
                speakerRoleText.text = npc != null ? npc.RoleTitle : "";
            }

            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = text;
            }

            if (contract != null)
            {
                if (objectiveStatusText != null)
                {
                    string target = contract.targetSpecies != null ? contract.targetSpecies.commonName : "Unknown Target";
                    if (status == ContractStatus.Completed)
                    {
                        objectiveStatusText.text = $"<color=#00FF88>✔ Contract Fulfilled: 1/1 {target} Delivered</color>";
                    }
                    else if (npc != null && npc.CheckHasRequiredSpecimen())
                    {
                        objectiveStatusText.text = $"<color=#FFD700>★ Objective: 1/1 {target} (Ready for Turn-in!)</color>";
                    }
                    else
                    {
                        objectiveStatusText.text = $"<color=#FFCC44>Objective: 0/1 {target} captured</color>";
                    }
                }

                if (rewardSummaryText != null)
                {
                    rewardSummaryText.text = $"Reward: <b>{contract.rewardSummary}</b>";
                }
            }
            else
            {
                if (objectiveStatusText != null) objectiveStatusText.text = "";
                if (rewardSummaryText != null) rewardSummaryText.text = "";
            }

            if (footerHintText != null)
            {
                if (justCompleted)
                {
                    footerHintText.text = "[ F ] or [ Esc ] Close Dialogue";
                }
                else if (status == ContractStatus.Active && npc != null && npc.CheckHasRequiredSpecimen())
                {
                    footerHintText.text = "[ F ] Deliver Specimen & Claim Bounty";
                }
                else
                {
                    footerHintText.text = "[ F ] Continue  |  [ Esc ] Close";
                }
            }
        }

        private void HandleDialogueClosed()
        {
            if (dialogueModalPanel != null)
            {
                dialogueModalPanel.SetActive(false);
            }
        }

        public void SetReferences(GameObject promptPanel, GameObject modalPanel, Text prompt, Text name, Text role, Text body, Text objective, Text reward, Text hint)
        {
            interactionPromptPanel = promptPanel;
            dialogueModalPanel = modalPanel;
            promptText = prompt;
            speakerNameText = name;
            speakerRoleText = role;
            dialogueBodyText = body;
            objectiveStatusText = objective;
            rewardSummaryText = reward;
            footerHintText = hint;
        }
    }
}
