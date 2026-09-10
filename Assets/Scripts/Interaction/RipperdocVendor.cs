using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Interaction
{
    /// <summary>
    /// Interactive black-market merchant and illicit neuro-surgeon.
    /// Sells volatile Contraband specimens with overclocked offensive power
    /// and reduced biological stability.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RipperdocVendor : MonoBehaviour
    {
        [Header("Vendor Identity")]
        [SerializeField] private string doctorName = "Dr. Silas \"The Stitcher\"";
        [SerializeField] private string title = "Unlicensed Neuro-Chirurgeon";
        [SerializeField] private string promptText = "Consult Dr. Silas (Ripperdoc)";

        [Header("Black Market Catalog")]
        [SerializeField] private List<RipperdocProductSO> catalog = new List<RipperdocProductSO>();

        private bool isPlayerNearby;
        private bool isOpen;
        private PlayerWallet cachedWallet;
        private PlayerMonsterRoster cachedRoster;

        public string DoctorName => doctorName;
        public string Title => title;
        public IReadOnlyList<RipperdocProductSO> Catalog => catalog;
        public PlayerWallet CachedWallet => cachedWallet;
        public PlayerMonsterRoster CachedRoster => cachedRoster;
        public bool IsOpen => isOpen;
        public static RipperdocVendor ActiveInstance { get; private set; }

        // Static Events for decoupled UI
        public static event Action<RipperdocVendor, bool> OnInteractionPrompt;
        public static event Action<RipperdocVendor> OnVendorOpened;
        public static event Action OnVendorClosed;

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
                    if (BeastClad.Underground.PitmasterJax.ActiveInstance != null && BeastClad.Underground.PitmasterJax.ActiveInstance.IsOpen) return;
                    OpenVendor();
                }
                else
                {
                    CloseVendor();
                }
            }

            if (kb.escapeKey.wasPressedThisFrame && isOpen)
            {
                CloseVendor();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNearby = true;
                cachedWallet = other.GetComponent<PlayerWallet>();
                cachedRoster = other.GetComponent<PlayerMonsterRoster>();
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
                    CloseVendor();
                }
                OnInteractionPrompt?.Invoke(this, false);
                cachedWallet = null;
                cachedRoster = null;
            }
        }

        public void OpenVendor()
        {
            isOpen = true;
            OnVendorOpened?.Invoke(this);
            Debug.Log($"<color=#DC2626>[Ripperdoc Clinic]</color> Consulting with <b>{doctorName}</b>.");
        }

        public void CloseVendor()
        {
            isOpen = false;
            OnVendorClosed?.Invoke();
            Debug.Log($"<color=#DC2626>[Ripperdoc Clinic]</color> Left <b>{doctorName}'s</b> operating room.");
        }

        public string GetPromptMessage()
        {
            return $"[ F ] {promptText}";
        }

        public void SetCatalog(List<RipperdocProductSO> products)
        {
            catalog = products != null ? new List<RipperdocProductSO>(products) : new List<RipperdocProductSO>();
        }
    }
}
