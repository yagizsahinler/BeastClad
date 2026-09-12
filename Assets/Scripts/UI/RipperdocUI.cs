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
    /// UI Manager for the back-alley Ripperdoc clinic.
    /// Manages the illegal specimen browser, displaying overclock bonuses,
    /// bio-recoil penalties, and purchasing contraband beasts into the player roster.
    /// </summary>
    [DisallowMultipleComponent]
    public class RipperdocUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private GameObject modalPanel;

        [Header("Prompt Elements")]
        [SerializeField] private Text promptText;

        [Header("Clinic Header")]
        [SerializeField] private Text doctorNameText;
        [SerializeField] private Text clinicSubtitleText;

        [Header("Product Details")]
        [SerializeField] private Text productTitleText;
        [SerializeField] private Text warningBadgeText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text overclockStatsText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text playerBalanceText;
        [SerializeField] private Text feedbackText;

        [Header("Buttons")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;

        private RipperdocVendor activeVendor;
        private int currentIndex = 0;

        private void Awake()
        {
            if (promptPanel != null) promptPanel.SetActive(false);
            if (modalPanel != null) modalPanel.SetActive(false);

            HookListeners();
        }

        private void OnEnable()
        {
            RipperdocVendor.OnInteractionPrompt += HandlePrompt;
            RipperdocVendor.OnVendorOpened += HandleOpened;
            RipperdocVendor.OnVendorClosed += HandleClosed;

            HookListeners();
        }

        private void OnDisable()
        {
            RipperdocVendor.OnInteractionPrompt -= HandlePrompt;
            RipperdocVendor.OnVendorOpened -= HandleOpened;
            RipperdocVendor.OnVendorClosed -= HandleClosed;
        }

        private void HookListeners()
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
                purchaseButton.onClick.AddListener(HandlePurchaseClicked);
            }
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(HandleNextClicked);
                nextButton.onClick.AddListener(HandleNextClicked);
            }
            if (prevButton != null)
            {
                prevButton.onClick.RemoveListener(HandlePrevClicked);
                prevButton.onClick.AddListener(HandlePrevClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
                closeButton.onClick.AddListener(HandleCloseClicked);
            }
        }

        private void Update()
        {
            if (modalPanel == null || !modalPanel.activeSelf) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                HandleCloseClicked();
            }
            else if (kb.qKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            {
                HandlePrevClicked();
            }
            else if (kb.eKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            {
                HandleNextClicked();
            }
            else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            {
                HandlePurchaseClicked();
            }
        }

        private void HandlePrompt(RipperdocVendor vendor, bool show)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(show);
                if (show && promptText != null && vendor != null)
                {
                    promptText.text = vendor.GetPromptMessage();
                }
            }
        }

        private void HandleOpened(RipperdocVendor vendor)
        {
            activeVendor = vendor;
            currentIndex = 0;

            if (modalPanel != null) modalPanel.SetActive(true);
            if (promptPanel != null) promptPanel.SetActive(false);
            if (feedbackText != null) feedbackText.text = "";

            UpdateDisplay();
        }

        private void HandleClosed()
        {
            activeVendor = null;
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        public void UpdateDisplay()
        {
            if (activeVendor == null) return;

            if (doctorNameText != null) doctorNameText.text = activeVendor.DoctorName.ToUpper();
            if (clinicSubtitleText != null) clinicSubtitleText.text = $"BLACK MARKET CLINIC: {activeVendor.Title.ToUpper()}";

            var catalog = activeVendor.Catalog;
            if (catalog == null || catalog.Count == 0)
            {
                if (productTitleText != null) productTitleText.text = "OUT OF CONTRABAND";
                if (descriptionText != null) descriptionText.text = "The Enforcers raided the underground lab. Check back later.";
                if (purchaseButton != null) purchaseButton.interactable = false;
                return;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, catalog.Count - 1);
            var item = catalog[currentIndex];
            if (item == null) return;

            if (productTitleText != null) productTitleText.text = $"<b>{item.productTitle}</b>";

            if (warningBadgeText != null)
            {
                warningBadgeText.text = $"<color=#FF0044>⚠ PROHIBITED CONTRABAND // CODE 14-B</color>  |  Stability: <b>{item.stabilityPercentage:F0}%</b>";
            }

            if (descriptionText != null)
            {
                descriptionText.text = item.description;
            }

            if (overclockStatsText != null)
            {
                overclockStatsText.text = $"<color=#FF4444>★ Overclock Attack: <b>+{item.overclockBonusDamage:F0} DMG</b></color>  |  " +
                                         $"<color=#F59E0B>⚠ Bio-Recoil: <b>-{item.recoilSelfDamage:F0} HP per strike</b></color>";
            }

            var wallet = activeVendor.CachedWallet;
            if (wallet == null) wallet = FindAnyObjectByType<PlayerWallet>();
            int credits = wallet != null ? wallet.Credits : 0;

            if (priceText != null)
            {
                priceText.text = $"Street Price: <b>{item.costCredits:N0} Credits</b>";
            }

            if (playerBalanceText != null)
            {
                playerBalanceText.text = $"Your Credits: <color=#FFD700><b>{credits:N0} CR</b></color>";
            }

            if (purchaseButton != null)
            {
                purchaseButton.interactable = credits >= item.costCredits;
            }

            if (nextButton != null) nextButton.interactable = catalog.Count > 1;
            if (prevButton != null) prevButton.interactable = catalog.Count > 1;
        }

        private void HandlePurchaseClicked()
        {
            if (activeVendor == null) return;

            var catalog = activeVendor.Catalog;
            if (catalog == null || currentIndex >= catalog.Count) return;

            var item = catalog[currentIndex];
            if (item == null) return;

            var wallet = activeVendor.CachedWallet;
            if (wallet == null) wallet = FindAnyObjectByType<PlayerWallet>();

            var roster = activeVendor.CachedRoster;
            if (roster == null) roster = FindAnyObjectByType<PlayerMonsterRoster>();

            if (wallet == null)
            {
                if (feedbackText != null) feedbackText.text = "<color=#FF4444>ERROR: No payment account found.</color>";
                return;
            }

            if (wallet.Credits < item.costCredits)
            {
                if (feedbackText != null)
                {
                    feedbackText.text = $"<color=#FF4444>DR. SILAS: \"No credits, no surgery. Come back with {item.costCredits:N0} CR.\"</color>";
                }
                return;
            }

            if (wallet.TrySpendCredits(item.costCredits))
            {
                var newBeast = item.CreateContrabandInstance();
                if (roster != null && newBeast != null)
                {
                    roster.AddMonster(newBeast);
                }

                if (feedbackText != null)
                {
                    feedbackText.text = $"<color=#00FFAA>✔ SURGERY COMPLETE: <b>{newBeast?.nickname}</b> delivered to your roster! Hide it from municipal scanners.</color>";
                }

                Debug.Log($"<color=#DC2626>[Ripperdoc Purchase]</color> Acquired contraband <b>{item.productTitle}</b> for <b>{item.costCredits} Credits</b>. Stability: {item.stabilityPercentage}%.");
                UpdateDisplay();
                Persistence.SaveManager.Instance.SaveCurrentGame();
            }
        }

        private void HandleNextClicked()
        {
            if (activeVendor == null || activeVendor.Catalog.Count == 0) return;
            currentIndex = (currentIndex + 1) % activeVendor.Catalog.Count;
            if (feedbackText != null) feedbackText.text = "";
            UpdateDisplay();
        }

        private void HandlePrevClicked()
        {
            if (activeVendor == null || activeVendor.Catalog.Count == 0) return;
            currentIndex = (currentIndex - 1 + activeVendor.Catalog.Count) % activeVendor.Catalog.Count;
            if (feedbackText != null) feedbackText.text = "";
            UpdateDisplay();
        }

        private void HandleCloseClicked()
        {
            if (activeVendor != null)
            {
                activeVendor.CloseVendor();
            }
            else
            {
                HandleClosed();
            }
        }

        public void SetReferences(
            GameObject pPanel, Text pTxt,
            GameObject mPanel, Text dName, Text cSub,
            Text pTitle, Text warn, Text desc, Text oStats,
            Text pPrice, Text pBal, Text feed,
            Button buyBtn, Button nBtn, Button prBtn, Button cBtn)
        {
            promptPanel = pPanel;
            promptText = pTxt;
            modalPanel = mPanel;
            doctorNameText = dName;
            clinicSubtitleText = cSub;
            productTitleText = pTitle;
            warningBadgeText = warn;
            descriptionText = desc;
            overclockStatsText = oStats;
            priceText = pPrice;
            playerBalanceText = pBal;
            feedbackText = feed;
            purchaseButton = buyBtn;
            nextButton = nBtn;
            prevButton = prBtn;
            closeButton = cBtn;
        }
    }
}
