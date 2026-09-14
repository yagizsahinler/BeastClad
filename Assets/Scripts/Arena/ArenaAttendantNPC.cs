using System;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Player;

namespace BeastClad.Arena
{
    /// <summary>
    /// Interactive attendant NPC stationed by the Colosseum ring entrance.
    /// Interacting with the attendant opens the Sanctioned Bout Registration modal,
    /// gating tournament combat and gladiator activation behind explicit player consent.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class ArenaAttendantNPC : MonoBehaviour
    {
        [Header("Attendant Identity")]
        [SerializeField] private string attendantName = "Proctor Cassian";
        [SerializeField] private string roleTitle = "Colosseum Match Registrar";
        [SerializeField] private string promptText = "Talk to Arena Attendant (Register for Bout)";
        [TextArea(2, 4)]
        [SerializeField] private string dialogueMessage = "Greetings, Challenger. Your biometric profile and armor permits are in order. Are you ready to enter the arena sands for your sanctioned bout against Valerius?";

        private bool isPlayerNearby;
        private bool isOpen;

        public string AttendantName => attendantName;
        public string RoleTitle => roleTitle;
        public string DialogueMessage => dialogueMessage;
        public string PromptText => promptText;
        public bool IsOpen => isOpen;

        public static ArenaAttendantNPC ActiveInstance { get; private set; }

        public static event Action<ArenaAttendantNPC, bool> OnInteractionPrompt;
        public static event Action<ArenaAttendantNPC> OnAttendantOpened;
        public static event Action OnAttendantClosed;

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
                    OpenAttendant();
                }
                else
                {
                    CloseAttendant();
                }
            }

            if (kb.escapeKey.wasPressedThisFrame && isOpen)
            {
                CloseAttendant();
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
                    CloseAttendant();
                }
                OnInteractionPrompt?.Invoke(this, false);
            }
        }

        public void OpenAttendant()
        {
            isOpen = true;
            OnAttendantOpened?.Invoke(this);
            Debug.Log($"<color=#FFD700>[Arena Registrar]</color> Speaking with <b>{attendantName}</b>.");
        }

        public void CloseAttendant()
        {
            isOpen = false;
            OnAttendantClosed?.Invoke();
            Debug.Log($"<color=#FFD700>[Arena Registrar]</color> Closed registration terminal.");
        }

        public string GetPromptMessage()
        {
            return $"[ F ] {promptText}";
        }
    }
}
