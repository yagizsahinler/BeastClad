using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Interaction
{
    /// <summary>
    /// Interactive world-space kiosk terminal where players can browse
    /// and purchase state-sanctioned corporate starter packs.
    /// Uses New Input System for interaction [F] and dismissal [Escape].
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CorporateVendorKiosk : MonoBehaviour
    {
        [Header("Kiosk Branding")]
        [SerializeField] private string kioskName = "Aegis-Fauna Automated Kiosk";
        [SerializeField] private string corporationTitle = "Aegis-Fauna Biometrics Corp";
        [SerializeField] private string promptActionText = "Access Aegis-Fauna Sales Kiosk";

        [Header("Catalog")]
        [SerializeField] private List<CorporateStarterPackSO> availablePacks = new List<CorporateStarterPackSO>();

        private bool isPlayerNearby;
        private bool isKioskOpen;
        private PlayerWallet cachedWallet;
        private PlayerMonsterRoster cachedRoster;

        public string KioskName => kioskName;
        public string CorporationTitle => corporationTitle;
        public IReadOnlyList<CorporateStarterPackSO> AvailablePacks => availablePacks;
        public PlayerWallet CachedWallet => cachedWallet;
        public PlayerMonsterRoster CachedRoster => cachedRoster;
        public bool IsOpen => isKioskOpen;

        // Static Events for decoupled UI
        public static event Action<CorporateVendorKiosk, bool> OnInteractionPrompt;
        public static event Action<CorporateVendorKiosk> OnKioskOpened;
        public static event Action OnKioskClosed;

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

            if (kb.fKey.wasPressedThisFrame)
            {
                if (!isKioskOpen)
                {
                    OpenKiosk();
                }
                else
                {
                    CloseKiosk();
                }
            }

            if (kb.escapeKey.wasPressedThisFrame && isKioskOpen)
            {
                CloseKiosk();
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
                if (isKioskOpen)
                {
                    CloseKiosk();
                }
                OnInteractionPrompt?.Invoke(this, false);
                cachedWallet = null;
                cachedRoster = null;
            }
        }

        public void OpenKiosk()
        {
            isKioskOpen = true;
            OnKioskOpened?.Invoke(this);
            Debug.Log($"<color=#00E5FF>[Kiosk Terminal]</color> Connected to <b>{kioskName}</b>.");
        }

        public void CloseKiosk()
        {
            isKioskOpen = false;
            OnKioskClosed?.Invoke();
            Debug.Log($"<color=#00E5FF>[Kiosk Terminal]</color> Disconnected from <b>{kioskName}</b>.");
        }

        public string GetPromptMessage()
        {
            return $"[ F ] {promptActionText}";
        }

        public void SetCatalog(List<CorporateStarterPackSO> packs)
        {
            availablePacks = packs != null ? new List<CorporateStarterPackSO>(packs) : new List<CorporateStarterPackSO>();
        }
    }
}
