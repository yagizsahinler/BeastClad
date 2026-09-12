using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using BeastClad.Data;
using BeastClad.Interaction;
using BeastClad.Player;

namespace BeastClad.UI
{
    /// <summary>
    /// UI Manager for Corporate Vendor Kiosks.
    /// Handles the interaction prompt, catalog browser, spec inspection,
    /// credit transactions, and automatic state-registered monster delivery.
    /// </summary>
    [DisallowMultipleComponent]
    public class CorporateVendorUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject interactionPromptPanel;
        [SerializeField] private GameObject vendorModalPanel;

        [Header("Prompt Elements")]
        [SerializeField] private Text promptText;

        [Header("Vendor Header")]
        [SerializeField] private Text kioskTitleText;
        [SerializeField] private Text kioskSubtitleText;

        [Header("Pack Details Elements")]
        [SerializeField] private Text packTitleText;
        [SerializeField] private Text registrationBadgeText;
        [SerializeField] private Text packDescText;
        [SerializeField] private Text speciesSpecsText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text playerBalanceText;
        [SerializeField] private Text transactionFeedbackText;

        [Header("Buttons")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button nextPackButton;
        [SerializeField] private Button prevPackButton;
        [SerializeField] private Button closeButton;

        private CorporateVendorKiosk activeKiosk;
        private int currentPackIndex = 0;

        private void Awake()
        {
            if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
            if (vendorModalPanel != null) vendorModalPanel.SetActive(false);

            HookButtonListeners();
        }

        private void OnEnable()
        {
            CorporateVendorKiosk.OnInteractionPrompt += HandleInteractionPrompt;
            CorporateVendorKiosk.OnKioskOpened += HandleKioskOpened;
            CorporateVendorKiosk.OnKioskClosed += HandleKioskClosed;

            HookButtonListeners();
        }

        private void OnDisable()
        {
            CorporateVendorKiosk.OnInteractionPrompt -= HandleInteractionPrompt;
            CorporateVendorKiosk.OnKioskOpened -= HandleKioskOpened;
            CorporateVendorKiosk.OnKioskClosed -= HandleKioskClosed;
        }

        private void HookButtonListeners()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
                purchaseButton.onClick.AddListener(HandlePurchaseClicked);
            }
            if (nextPackButton != null)
            {
                nextPackButton.onClick.RemoveListener(HandleNextPackClicked);
                nextPackButton.onClick.AddListener(HandleNextPackClicked);
            }
            if (prevPackButton != null)
            {
                prevPackButton.onClick.RemoveListener(HandlePrevPackClicked);
                prevPackButton.onClick.AddListener(HandlePrevPackClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
                closeButton.onClick.AddListener(HandleCloseClicked);
            }
        }

        private void Update()
        {
            if (vendorModalPanel == null || !vendorModalPanel.activeSelf) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                HandleCloseClicked();
            }
            else if (kb.qKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            {
                HandlePrevPackClicked();
            }
            else if (kb.eKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            {
                HandleNextPackClicked();
            }
            else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            {
                HandlePurchaseClicked();
            }
        }

        private void HandleInteractionPrompt(CorporateVendorKiosk kiosk, bool show)
        {
            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(show);
                if (show && promptText != null && kiosk != null)
                {
                    promptText.text = kiosk.GetPromptMessage();
                }
            }
        }

        private void HandleKioskOpened(CorporateVendorKiosk kiosk)
        {
            activeKiosk = kiosk;
            currentPackIndex = 0;

            if (vendorModalPanel != null)
            {
                vendorModalPanel.SetActive(true);
            }

            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.SetActive(false);
            }

            if (transactionFeedbackText != null)
            {
                transactionFeedbackText.text = "";
            }

            UpdateDisplay();
        }

        private void HandleKioskClosed()
        {
            activeKiosk = null;
            if (vendorModalPanel != null)
            {
                vendorModalPanel.SetActive(false);
            }
        }

        public void UpdateDisplay()
        {
            if (activeKiosk == null) return;

            if (kioskTitleText != null) kioskTitleText.text = activeKiosk.KioskName.ToUpper();
            if (kioskSubtitleText != null) kioskSubtitleText.text = $"OFFICIAL CONTRACTOR: {activeKiosk.CorporationTitle}";

            var packs = activeKiosk.AvailablePacks;
            if (packs == null || packs.Count == 0)
            {
                if (packTitleText != null) packTitleText.text = "OUT OF STOCK";
                if (packDescText != null) packDescText.text = "No commercial specimens currently available in this terminal.";
                if (purchaseButton != null) purchaseButton.interactable = false;
                return;
            }

            currentPackIndex = Mathf.Clamp(currentPackIndex, 0, packs.Count - 1);
            var pack = packs[currentPackIndex];
            if (pack == null) return;

            if (packTitleText != null)
            {
                packTitleText.text = $"<b>{pack.packName}</b>";
            }

            if (registrationBadgeText != null)
            {
                string statusColor = pack.registrationStatus == RegistrationStatus.Legal ? "#00FF88" : "#FFAA00";
                registrationBadgeText.text = $"<color={statusColor}>★ MUNICIPAL REGISTRATION: {pack.registrationStatus.ToString().ToUpper()}</color>  |  Serial: <b>{pack.certificationSerial}</b>";
            }

            if (packDescText != null)
            {
                packDescText.text = $"{pack.description}\n<color=#AAAAAA>Brand: {pack.corporateBrand}</color>";
            }

            if (speciesSpecsText != null && pack.monsterSpecies != null)
            {
                var species = pack.monsterSpecies;
                string elemStr = species.primaryElement != null ? species.primaryElement.displayName : "Neutral";
                speciesSpecsText.text = $"<b>Specimen:</b> {species.commonName}  |  <b>Element:</b> {elemStr}\n" +
                                        $"<b>Certified Modules:</b> Head [HP], Chest [Defense], Left Arm [Atk], Right Arm [Atk], Legs [Speed]";
            }

            var wallet = activeKiosk.CachedWallet;
            if (wallet == null) wallet = FindAnyObjectByType<PlayerWallet>();
            int playerCredits = wallet != null ? wallet.Credits : 0;

            if (priceText != null)
            {
                priceText.text = $"Retail Price: <b>{pack.costCredits:N0} Credits</b>";
            }

            if (playerBalanceText != null)
            {
                playerBalanceText.text = $"Your Account Balance: <color=#FFD700><b>{playerCredits:N0} Credits</b></color>";
            }

            if (purchaseButton != null)
            {
                bool canAfford = playerCredits >= pack.costCredits;
                purchaseButton.interactable = canAfford;
            }

            if (nextPackButton != null) nextPackButton.interactable = packs.Count > 1;
            if (prevPackButton != null) prevPackButton.interactable = packs.Count > 1;
        }

        private void HandlePurchaseClicked()
        {
            if (activeKiosk == null) return;

            var packs = activeKiosk.AvailablePacks;
            if (packs == null || currentPackIndex >= packs.Count) return;

            var pack = packs[currentPackIndex];
            if (pack == null) return;

            var wallet = activeKiosk.CachedWallet;
            if (wallet == null) wallet = FindAnyObjectByType<PlayerWallet>();
            var roster = activeKiosk.CachedRoster;
            if (roster == null) roster = FindAnyObjectByType<PlayerMonsterRoster>();

            if (wallet == null)
            {
                if (transactionFeedbackText != null)
                {
                    transactionFeedbackText.text = "<color=#FF4444>ERROR: No biometric payment account connected.</color>";
                }
                return;
            }

            if (wallet.Credits < pack.costCredits)
            {
                if (transactionFeedbackText != null)
                {
                    transactionFeedbackText.text = $"<color=#FF4444>TRANSACTION REJECTED: Insufficient Credits ({wallet.Credits} / {pack.costCredits} CR required).</color>";
                }
                return;
            }

            if (wallet.TrySpendCredits(pack.costCredits))
            {
                var newBeast = pack.CreateMonsterInstance();
                if (roster != null && newBeast != null)
                {
                    roster.AddMonster(newBeast);
                }

                if (transactionFeedbackText != null)
                {
                    transactionFeedbackText.text = $"<color=#00FFAA>✔ TRANSACTION APPROVED: <b>{newBeast?.nickname}</b> registered under License {pack.certificationSerial} and added to your roster!</color>";
                }

                Debug.Log($"<color=#00FFAA>[Corporate Purchase]</color> Acquired <b>{pack.packName}</b>! Deducted <b>{pack.costCredits} Credits</b>. New Balance: <b>{wallet.Credits} Credits</b>.");
                UpdateDisplay();
                Persistence.SaveManager.Instance.SaveCurrentGame();
            }
        }

        private void HandleNextPackClicked()
        {
            if (activeKiosk == null || activeKiosk.AvailablePacks.Count == 0) return;
            currentPackIndex = (currentPackIndex + 1) % activeKiosk.AvailablePacks.Count;
            if (transactionFeedbackText != null) transactionFeedbackText.text = "";
            var pack = activeKiosk.AvailablePacks[currentPackIndex];
            Debug.Log($"<color=#00E5FF>[Kiosk UI]</color> Switched to pack: <b>{pack?.packName}</b> ({currentPackIndex + 1}/{activeKiosk.AvailablePacks.Count})");
            UpdateDisplay();
        }

        private void HandlePrevPackClicked()
        {
            if (activeKiosk == null || activeKiosk.AvailablePacks.Count == 0) return;
            currentPackIndex = (currentPackIndex - 1 + activeKiosk.AvailablePacks.Count) % activeKiosk.AvailablePacks.Count;
            if (transactionFeedbackText != null) transactionFeedbackText.text = "";
            var pack = activeKiosk.AvailablePacks[currentPackIndex];
            Debug.Log($"<color=#00E5FF>[Kiosk UI]</color> Switched to pack: <b>{pack?.packName}</b> ({currentPackIndex + 1}/{activeKiosk.AvailablePacks.Count})");
            UpdateDisplay();
        }

        private void HandleCloseClicked()
        {
            Debug.Log("<color=#00E5FF>[Kiosk UI]</color> Closing Kiosk modal window.");
            if (activeKiosk != null)
            {
                activeKiosk.CloseKiosk();
            }
            else
            {
                HandleKioskClosed();
            }
        }

        public void SetReferences(
            GameObject promptPanel, Text promptTxt,
            GameObject modalPanel, Text titleTxt, Text subTxt,
            Text pTitleTxt, Text badgeTxt, Text pDescTxt, Text specsTxt,
            Text pPriceTxt, Text balTxt, Text feedbackTxt,
            Button buyBtn, Button nextBtn, Button prevBtn, Button exitBtn)
        {
            interactionPromptPanel = promptPanel;
            promptText = promptTxt;
            vendorModalPanel = modalPanel;
            kioskTitleText = titleTxt;
            kioskSubtitleText = subTxt;
            packTitleText = pTitleTxt;
            registrationBadgeText = badgeTxt;
            packDescText = pDescTxt;
            speciesSpecsText = specsTxt;
            priceText = pPriceTxt;
            playerBalanceText = balTxt;
            transactionFeedbackText = feedbackTxt;
            purchaseButton = buyBtn;
            nextPackButton = nextBtn;
            prevPackButton = prevBtn;
            closeButton = exitBtn;
        }
    }
}
