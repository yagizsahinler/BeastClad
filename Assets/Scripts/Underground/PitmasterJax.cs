using System;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Player;

namespace BeastClad.Underground
{
    /// <summary>
    /// NPC host for the underground unsanctioned pit matches.
    /// Interacting with Jax opens the wager placement terminal.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PitmasterJax : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string npcName = "Pitmaster Jax";
        [SerializeField] private string title = "Unsanctioned Pit Operator";
        [SerializeField] private string promptText = "Talk to Pitmaster Jax (Place Wager)";

        private bool isPlayerNearby;
        private bool isOpen;

        public string NPCName => npcName;
        public string Title => title;
        public bool IsOpen => isOpen;
        public static PitmasterJax ActiveInstance { get; private set; }

        public static event Action<PitmasterJax, bool> OnInteractionPrompt;
        public static event Action<PitmasterJax> OnDialogueOpened;
        public static event Action OnDialogueClosed;

        private void Awake()
        {
            ActiveInstance = this;
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnDestroy()
        {
            if (ActiveInstance == this) ActiveInstance = null;
        }

        private void Update()
        {
            if (!isPlayerNearby) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.fKey.wasPressedThisFrame)
            {
                if (!isOpen)
                {
                    if (BeastClad.Interaction.RipperdocVendor.ActiveInstance != null && BeastClad.Interaction.RipperdocVendor.ActiveInstance.IsOpen) return;
                    OpenTerminal();
                }
                else
                {
                    CloseTerminal();
                }
            }

            if (kb.escapeKey.wasPressedThisFrame && isOpen)
            {
                CloseTerminal();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNearby = true;
                OnInteractionPrompt?.Invoke(this, true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNearby = false;
                if (isOpen)
                {
                    CloseTerminal();
                }
                OnInteractionPrompt?.Invoke(this, false);
            }
        }

        public void OpenTerminal()
        {
            isOpen = true;
            OnDialogueOpened?.Invoke(this);
            Debug.Log($"<color=#FF5500>[Underground Pit]</color> Speaking with <b>{npcName}</b>.");
        }

        public void CloseTerminal()
        {
            isOpen = false;
            OnDialogueClosed?.Invoke();
        }

        public string GetPromptMessage()
        {
            return $"[ F ] {promptText}";
        }
    }
}
