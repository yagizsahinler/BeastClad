using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using BeastClad.Underground;
using BeastClad.Player;

namespace BeastClad.UI
{
    /// <summary>
    /// UI manager for the underground pit betting system.
    /// Handles wager selection (100 / 250 / 500 CR), countdown banners,
    /// boss health bar for the pit brawler, and match outcome modals.
    /// </summary>
    [DisallowMultipleComponent]
    public class PitMatchWagerUI : MonoBehaviour
    {
        [Header("Controller Reference")]
        [SerializeField] private PitMatchWagerController wagerController;

        [Header("Prompt Panel")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private Text promptText;

        [Header("Wager Modal Panel")]
        [SerializeField] private GameObject wagerModalPanel;
        [SerializeField] private Text modalTitleText;
        [SerializeField] private Text modalSubtitleText;
        [SerializeField] private Button wager100Btn;
        [SerializeField] private Button wager250Btn;
        [SerializeField] private Button wager500Btn;
        [SerializeField] private Text selectedWagerText;
        [SerializeField] private Text payoutInfoText;
        [SerializeField] private Text walletBalanceText;
        [SerializeField] private Button enterPitBtn;
        [SerializeField] private Button closeBtn;

        [Header("Central Announcer Banner")]
        [SerializeField] private GameObject announcerBannerRoot;
        [SerializeField] private Text announcerMainText;
        [SerializeField] private Text announcerSubText;

        [Header("Brawler Boss Health Bar")]
        [SerializeField] private GameObject brawlerBarRoot;
        [SerializeField] private Text brawlerNameText;
        [SerializeField] private Text brawlerEpithetText;
        [SerializeField] private Image brawlerHealthFill;
        [SerializeField] private Text brawlerHealthNumText;

        [Header("Outcome Modal Panel")]
        [SerializeField] private GameObject outcomeModalPanel;
        [SerializeField] private Text outcomeTitleText;
        [SerializeField] private Text outcomeDetailsText;
        [SerializeField] private Button returnHubBtn;

        private int selectedWager = 250;
        private PlayerWallet cachedWallet;

        private void Awake()
        {
            if (wagerController == null)
            {
                wagerController = FindAnyObjectByType<PitMatchWagerController>();
            }

            if (promptPanel != null) promptPanel.SetActive(false);
            if (wagerModalPanel != null) wagerModalPanel.SetActive(false);
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
            if (brawlerBarRoot != null) brawlerBarRoot.SetActive(false);
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);

            HookButtons();
        }

        private void OnEnable()
        {
            PitmasterJax.OnInteractionPrompt += HandlePrompt;
            PitmasterJax.OnDialogueOpened += HandleJaxOpened;
            PitmasterJax.OnDialogueClosed += HandleJaxClosed;

            if (wagerController != null)
            {
                wagerController.OnStateChanged += HandleStateChanged;
                wagerController.OnCountdownTick += HandleCountdownTick;
                wagerController.OnBoutConcluded += HandleBoutConcluded;
                wagerController.OnReturnedToHub += HandleReturnedToHub;

                if (wagerController.Brawler != null)
                {
                    wagerController.Brawler.OnHealthChanged += HandleBrawlerHealthChanged;
                }
            }

            HookButtons();
        }

        private void OnDisable()
        {
            PitmasterJax.OnInteractionPrompt -= HandlePrompt;
            PitmasterJax.OnDialogueOpened -= HandleJaxOpened;
            PitmasterJax.OnDialogueClosed -= HandleJaxClosed;

            if (wagerController != null)
            {
                wagerController.OnStateChanged -= HandleStateChanged;
                wagerController.OnCountdownTick -= HandleCountdownTick;
                wagerController.OnBoutConcluded -= HandleBoutConcluded;
                wagerController.OnReturnedToHub -= HandleReturnedToHub;

                if (wagerController.Brawler != null)
                {
                    wagerController.Brawler.OnHealthChanged -= HandleBrawlerHealthChanged;
                }
            }
        }

        private void Start()
        {
            RefreshWagerSelectionDisplay();
        }

        private void HookButtons()
        {
            if (wager100Btn != null)
            {
                wager100Btn.onClick.RemoveListener(Select100Wager);
                wager100Btn.onClick.AddListener(Select100Wager);
            }
            if (wager250Btn != null)
            {
                wager250Btn.onClick.RemoveListener(Select250Wager);
                wager250Btn.onClick.AddListener(Select250Wager);
            }
            if (wager500Btn != null)
            {
                wager500Btn.onClick.RemoveListener(Select500Wager);
                wager500Btn.onClick.AddListener(Select500Wager);
            }
            if (enterPitBtn != null)
            {
                enterPitBtn.onClick.RemoveListener(HandleEnterPitClicked);
                enterPitBtn.onClick.AddListener(HandleEnterPitClicked);
            }
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveListener(CloseWagerModal);
                closeBtn.onClick.AddListener(CloseWagerModal);
            }
            if (returnHubBtn != null)
            {
                returnHubBtn.onClick.RemoveListener(HandleReturnHubClicked);
                returnHubBtn.onClick.AddListener(HandleReturnHubClicked);
            }
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (wagerModalPanel != null && wagerModalPanel.activeSelf)
            {
                if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) Select100Wager();
                if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) Select250Wager();
                if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) Select500Wager();

                if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                {
                    HandleEnterPitClicked();
                }

                if (kb.escapeKey.wasPressedThisFrame)
                {
                    CloseWagerModal();
                }
            }
            else if (outcomeModalPanel != null && outcomeModalPanel.activeSelf)
            {
                if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
                {
                    HandleReturnHubClicked();
                }
            }
        }

        private void HandlePrompt(PitmasterJax jax, bool isNear)
        {
            if (promptPanel == null) return;

            if (isNear && (wagerModalPanel == null || !wagerModalPanel.activeSelf))
            {
                promptPanel.SetActive(true);
                if (promptText != null) promptText.text = jax.GetPromptMessage();
            }
            else
            {
                promptPanel.SetActive(false);
            }
        }

        private void HandleJaxOpened(PitmasterJax jax)
        {
            if (promptPanel != null) promptPanel.SetActive(false);
            OpenWagerModal();
        }

        private void HandleJaxClosed()
        {
            CloseWagerModal();
        }

        public void OpenWagerModal()
        {
            if (wagerModalPanel != null) wagerModalPanel.SetActive(true);
            RefreshWagerSelectionDisplay();
        }

        public void CloseWagerModal()
        {
            if (wagerModalPanel != null) wagerModalPanel.SetActive(false);
        }

        public void Select100Wager()
        {
            selectedWager = 100;
            RefreshWagerSelectionDisplay();
        }

        public void Select250Wager()
        {
            selectedWager = 250;
            RefreshWagerSelectionDisplay();
        }

        public void Select500Wager()
        {
            selectedWager = 500;
            RefreshWagerSelectionDisplay();
        }

        private void RefreshWagerSelectionDisplay()
        {
            if (cachedWallet == null) cachedWallet = FindAnyObjectByType<PlayerWallet>();

            float multiplier = wagerController != null ? wagerController.PayoutMultiplier : 2.5f;
            int potentialPayout = Mathf.RoundToInt(selectedWager * multiplier);
            int netProfit = potentialPayout - selectedWager;

            if (selectedWagerText != null)
            {
                selectedWagerText.text = $"Selected Wager: <color=#FFCC00>{selectedWager} Credits</color>";
            }

            if (payoutInfoText != null)
            {
                payoutInfoText.text = $"Potential Payout: <color=#00FFAA>{potentialPayout} Credits</color> (<color=#38BDF8>+{netProfit} Net</color> @ {multiplier:F1}x)";
            }

            int currentBalance = cachedWallet != null ? cachedWallet.Credits : 0;
            bool canAfford = currentBalance >= selectedWager;

            if (walletBalanceText != null)
            {
                string balanceColor = canAfford ? "#00FFAA" : "#EF4444";
                walletBalanceText.text = $"Purse Balance: <color={balanceColor}>{currentBalance} Credits</color>";
            }

            if (enterPitBtn != null)
            {
                enterPitBtn.interactable = canAfford;
            }
        }

        private void HandleEnterPitClicked()
        {
            if (wagerController == null) wagerController = FindAnyObjectByType<PitMatchWagerController>();
            if (wagerController == null) return;

            if (wagerController.CanAffordWager(selectedWager))
            {
                CloseWagerModal();
                wagerController.StartBoutWithWager(selectedWager);
            }
            else
            {
                RefreshWagerSelectionDisplay();
            }
        }

        private void HandleStateChanged(PitBoutState state)
        {
            if (state == PitBoutState.ActiveBout)
            {
                if (brawlerBarRoot != null) brawlerBarRoot.SetActive(true);
                if (wagerController != null && wagerController.Brawler != null)
                {
                    var brawler = wagerController.Brawler;
                    if (brawlerNameText != null) brawlerNameText.text = brawler.BrawlerName;
                    if (brawlerEpithetText != null) brawlerEpithetText.text = brawler.Epithet;
                    UpdateBrawlerHealthBar(brawler.CurrentHealth, brawler.MaxHealth);
                }
            }
            else if (state == PitBoutState.Idle)
            {
                if (brawlerBarRoot != null) brawlerBarRoot.SetActive(false);
                if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
                if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            }
        }

        private void HandleCountdownTick(int remainingSeconds)
        {
            if (announcerBannerRoot == null || announcerMainText == null) return;

            announcerBannerRoot.SetActive(true);

            if (remainingSeconds > 0)
            {
                announcerMainText.text = $"<color=#FFCC00>{remainingSeconds}</color>";
                if (announcerSubText != null) announcerSubText.text = "UNSANCTIONED DEATHMATCH COMMENCING";
            }
            else
            {
                announcerMainText.text = "<color=#FF0055>FIGHT!</color>";
                if (announcerSubText != null) announcerSubText.text = "NO RULES • NO REFEREE";
                Invoke(nameof(HideAnnouncerBanner), 1.5f);
            }
        }

        private void HideAnnouncerBanner()
        {
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
        }

        private void HandleBrawlerHealthChanged(float current, float max)
        {
            UpdateBrawlerHealthBar(current, max);
        }

        private void UpdateBrawlerHealthBar(float current, float max)
        {
            if (brawlerHealthFill != null)
            {
                brawlerHealthFill.fillAmount = Mathf.Clamp01(current / max);
            }

            if (brawlerHealthNumText != null)
            {
                brawlerHealthNumText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)} HP";
            }
        }

        private void HandleBoutConcluded(bool playerWon, int payoutCredits, int wager)
        {
            if (brawlerBarRoot != null) brawlerBarRoot.SetActive(false);
            if (outcomeModalPanel == null) return;

            outcomeModalPanel.SetActive(true);

            if (playerWon)
            {
                if (outcomeTitleText != null)
                {
                    outcomeTitleText.text = "<color=#00FFAA>PIT CHAMPION!</color>";
                }
                if (outcomeDetailsText != null)
                {
                    outcomeDetailsText.text = $"You butchered the brawler and seized the purse!\n\n<color=#FFCC00>Payout: +{payoutCredits} Credits</color>\n(Wager: {wager} CR • 2.5x Multiplier)";
                }
            }
            else
            {
                if (outcomeTitleText != null)
                {
                    outcomeTitleText.text = "<color=#EF4444>CARRIED OUT IN A SACK!</color>";
                }
                if (outcomeDetailsText != null)
                {
                    outcomeDetailsText.text = $"You collapsed in the pit dirt.\n\n<color=#EF4444>Lost Wager: -{wager} Credits</color>\nDr. Silas will be pleased to patch you up...";
                }
            }
        }

        private void HandleReturnHubClicked()
        {
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (wagerController != null)
            {
                wagerController.ReturnToHub();
            }
        }

        private void HandleReturnedToHub()
        {
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (brawlerBarRoot != null) brawlerBarRoot.SetActive(false);
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
        }
    }
}
