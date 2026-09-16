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
        [SerializeField] private ArenaBoutController boutController;
        private bool isSubscribed = false;

        public string AttendantName => attendantName;
        public string RoleTitle => roleTitle;
        public string DialogueMessage => dialogueMessage;
        public string PromptText => promptText;

        public bool IsOpen
        {
            get
            {
                if (!IsInteractionPermitted && isOpen)
                {
                    isOpen = false;
                }
                return isOpen;
            }
        }

        public bool IsInteractionPermitted
        {
            get
            {
                SubscribeToBoutController();
                if (boutController != null && boutController.IsBoutInProgress)
                {
                    return false;
                }
                return true;
            }
        }

        private static ArenaAttendantNPC activeInstance;
        public static ArenaAttendantNPC ActiveInstance
        {
            get
            {
                if (activeInstance == null) activeInstance = FindAnyObjectByType<ArenaAttendantNPC>();
                return activeInstance;
            }
            private set => activeInstance = value;
        }

        public static event Action<ArenaAttendantNPC, bool> OnInteractionPrompt;
        public static event Action<ArenaAttendantNPC> OnAttendantOpened;
        public static event Action OnAttendantClosed;

        private void Awake()
        {
            ActiveInstance = this;
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
            SubscribeToBoutController();
        }

        private void OnEnable()
        {
            SubscribeToBoutController();
        }

        private void OnDisable()
        {
            UnsubscribeFromBoutController();
        }

        private void OnDestroy()
        {
            UnsubscribeFromBoutController();
            if (activeInstance == this) activeInstance = null;
        }

        private void SubscribeToBoutController()
        {
            if (isSubscribed) return;
            if (boutController == null) boutController = ArenaBoutController.ActiveInstance;
            if (boutController != null)
            {
                boutController.OnStateChanged -= HandleBoutStateChanged;
                boutController.OnStateChanged += HandleBoutStateChanged;
                isSubscribed = true;
            }
        }

        private void UnsubscribeFromBoutController()
        {
            if (boutController != null)
            {
                boutController.OnStateChanged -= HandleBoutStateChanged;
            }
            isSubscribed = false;
        }

        private void HandleBoutStateChanged(ArenaBoutState newState)
        {
            if (newState == ArenaBoutState.PreMatch || newState == ArenaBoutState.ActiveBout)
            {
                if (isOpen)
                {
                    CloseAttendant();
                }
                if (isPlayerNearby)
                {
                    isPlayerNearby = false;
                    OnInteractionPrompt?.Invoke(this, false);
                }
            }
        }

        private void Update()
        {
            if (!IsInteractionPermitted)
            {
                if (isOpen)
                {
                    CloseAttendant();
                }
                if (isPlayerNearby)
                {
                    isPlayerNearby = false;
                    OnInteractionPrompt?.Invoke(this, false);
                }
                return;
            }

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
                if (!IsInteractionPermitted)
                {
                    isPlayerNearby = false;
                    return;
                }

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
            if (!IsInteractionPermitted)
            {
                Debug.LogWarning($"<color=#FF8800>[Arena Registrar]</color> Cannot speak with {attendantName} while a bout is in progress!");
                return;
            }

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
            return IsInteractionPermitted ? $"[ F ] {promptText}" : "";
        }
    }
}
