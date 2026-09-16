using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using BeastClad.Arena;

namespace BeastClad.UI
{
    /// <summary>
    /// HUD controller for sanctioned corporate arena bouts.
    /// Manages gladiator boss health bar, central announcer banner,
    /// division ladder rank progression display, and attendant bout registration modal.
    /// </summary>
    [DisallowMultipleComponent]
    public class ArenaMatchUI : MonoBehaviour
    {
        [Header("Controller Reference")]
        [SerializeField] private ArenaBoutController boutController;

        [Header("Opponent Health Bar")]
        [SerializeField] private GameObject opponentBarRoot;
        [SerializeField] private Text opponentNameText;
        [SerializeField] private Text opponentSponsorText;
        [SerializeField] private Image opponentHealthFill;
        [SerializeField] private Text opponentHealthNumberText;

        [Header("Central Announcer Banner")]
        [SerializeField] private GameObject announcerBannerRoot;
        [SerializeField] private Text announcerMainText;
        [SerializeField] private Text announcerSubText;

        [Header("Division Ladder Badge")]
        [SerializeField] private Text divisionRankText;

        [Header("Attendant Interaction Prompt")]
        [SerializeField] private GameObject attendantPromptPanel;
        [SerializeField] private Text attendantPromptText;

        [Header("Attendant Registration Modal")]
        [SerializeField] private GameObject registrationModalPanel;
        [SerializeField] private Text modalTitleText;
        [SerializeField] private Text modalDialogueText;
        [SerializeField] private Text modalOpponentInfoText;
        [SerializeField] private Text modalStandingText;
        [SerializeField] private Text modalPurseText;
        [SerializeField] private Button enterRingButton;
        [SerializeField] private Button closeModalButton;

        [Header("Post-Match Outcome Modal")]
        [SerializeField] private GameObject outcomeModalPanel;
        [SerializeField] private Text outcomeTitleText;
        [SerializeField] private Text outcomeSubtitleText;
        [SerializeField] private Text outcomeStandingText;
        [SerializeField] private Text outcomeRewardsText;
        [SerializeField] private Button nextBoutButton;
        [SerializeField] private Text nextBoutButtonText;
        [SerializeField] private Button exitToConcourseButton;
        [SerializeField] private Button leaveArenaButton;
        [SerializeField] private Button closeOutcomeModalButton;

        public bool IsOutcomeModalOpen => outcomeModalPanel != null && outcomeModalPanel.activeSelf;
        public bool IsRegistrationModalOpen => registrationModalPanel != null && registrationModalPanel.activeSelf;
        public bool IsOpen => IsOutcomeModalOpen || IsRegistrationModalOpen;

        private void Awake()
        {
            if (boutController == null)
            {
                boutController = FindAnyObjectByType<ArenaBoutController>();
            }

            if (attendantPromptPanel != null) attendantPromptPanel.SetActive(false);
            if (registrationModalPanel != null) registrationModalPanel.SetActive(false);
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (opponentBarRoot != null) opponentBarRoot.SetActive(false);
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);

            HookButtons();
        }

        private void OnEnable()
        {
            ArenaAttendantNPC.OnInteractionPrompt += HandleAttendantPrompt;
            ArenaAttendantNPC.OnAttendantOpened += HandleAttendantOpened;
            ArenaAttendantNPC.OnAttendantClosed += HandleAttendantClosed;

            if (boutController != null)
            {
                boutController.OnStateChanged += HandleStateChanged;
                boutController.OnCountdownTick += HandleCountdownTick;
                boutController.OnMatchConcluded += HandleMatchConcluded;

                if (boutController.Gladiator != null)
                {
                    boutController.Gladiator.OnHealthChanged += HandleGladiatorHealthChanged;
                }
            }

            HookButtons();
        }

        private void OnDisable()
        {
            ArenaAttendantNPC.OnInteractionPrompt -= HandleAttendantPrompt;
            ArenaAttendantNPC.OnAttendantOpened -= HandleAttendantOpened;
            ArenaAttendantNPC.OnAttendantClosed -= HandleAttendantClosed;

            if (boutController != null)
            {
                boutController.OnStateChanged -= HandleStateChanged;
                boutController.OnCountdownTick -= HandleCountdownTick;
                boutController.OnMatchConcluded -= HandleMatchConcluded;

                if (boutController.Gladiator != null)
                {
                    boutController.Gladiator.OnHealthChanged -= HandleGladiatorHealthChanged;
                }
            }

            UnhookButtons();
        }

        private void HookButtons()
        {
            if (enterRingButton != null)
            {
                enterRingButton.onClick.RemoveListener(HandleEnterRingClicked);
                enterRingButton.onClick.AddListener(HandleEnterRingClicked);
            }
            if (closeModalButton != null)
            {
                closeModalButton.onClick.RemoveListener(HandleCloseModalClicked);
                closeModalButton.onClick.AddListener(HandleCloseModalClicked);
            }
            if (nextBoutButton != null)
            {
                nextBoutButton.onClick.RemoveListener(HandleNextBoutClicked);
                nextBoutButton.onClick.AddListener(HandleNextBoutClicked);
            }
            if (exitToConcourseButton != null)
            {
                exitToConcourseButton.onClick.RemoveListener(HandleExitToConcourseClicked);
                exitToConcourseButton.onClick.AddListener(HandleExitToConcourseClicked);
            }
            if (leaveArenaButton != null)
            {
                leaveArenaButton.onClick.RemoveListener(HandleLeaveArenaClicked);
                leaveArenaButton.onClick.AddListener(HandleLeaveArenaClicked);
            }
            if (closeOutcomeModalButton != null)
            {
                closeOutcomeModalButton.onClick.RemoveListener(HandleCloseOutcomeModalClicked);
                closeOutcomeModalButton.onClick.AddListener(HandleCloseOutcomeModalClicked);
            }
        }

        private void UnhookButtons()
        {
            if (enterRingButton != null) enterRingButton.onClick.RemoveListener(HandleEnterRingClicked);
            if (closeModalButton != null) closeModalButton.onClick.RemoveListener(HandleCloseModalClicked);
            if (nextBoutButton != null) nextBoutButton.onClick.RemoveListener(HandleNextBoutClicked);
            if (exitToConcourseButton != null) exitToConcourseButton.onClick.RemoveListener(HandleExitToConcourseClicked);
            if (leaveArenaButton != null) leaveArenaButton.onClick.RemoveListener(HandleLeaveArenaClicked);
            if (closeOutcomeModalButton != null) closeOutcomeModalButton.onClick.RemoveListener(HandleCloseOutcomeModalClicked);
        }

        private void Start()
        {
            InitializeDisplay();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // If outcome modal is open, allow [Enter] for next bout and [Escape] for concourse
            if (outcomeModalPanel != null && outcomeModalPanel.activeSelf)
            {
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                {
                    HandleNextBoutClicked();
                }
                else if (kb.escapeKey.wasPressedThisFrame)
                {
                    HandleExitToConcourseClicked();
                }
                return;
            }

            // If registration modal is open, allow [Enter] to confirm and [Escape] to cancel
            if (registrationModalPanel != null && registrationModalPanel.activeSelf)
            {
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                {
                    HandleEnterRingClicked();
                }
                else if (kb.escapeKey.wasPressedThisFrame)
                {
                    HandleCloseModalClicked();
                }
            }
        }

        private void InitializeDisplay()
        {
            if (boutController == null) return;

            // Opponent bar remains hidden until bout begins
            if (opponentBarRoot != null)
            {
                opponentBarRoot.SetActive(false);
            }

            var glad = boutController.Gladiator;
            if (glad != null)
            {
                if (opponentNameText != null) opponentNameText.text = glad.GladiatorName;
                if (opponentSponsorText != null) opponentSponsorText.text = glad.CorporateSponsor;
                UpdateHealthBar(glad.CurrentHealth, glad.MaxHealth);
            }

            if (divisionRankText != null)
            {
                divisionRankText.text = $"<b>DIVISION LADDER</b>\n{boutController.CurrentRank}";
            }
        }

        #region Attendant Modal Handlers

        private void HandleAttendantPrompt(ArenaAttendantNPC attendant, bool isNearby)
        {
            if (attendantPromptPanel != null)
            {
                bool isBoutActive = boutController != null && boutController.IsBoutInProgress;
                bool show = isNearby && !isBoutActive && (registrationModalPanel == null || !registrationModalPanel.activeSelf);
                attendantPromptPanel.SetActive(show);
                if (show && attendantPromptText != null && attendant != null)
                {
                    attendantPromptText.text = attendant.GetPromptMessage();
                }
            }
        }

        private void HandleAttendantOpened(ArenaAttendantNPC attendant)
        {
            if (attendantPromptPanel != null) attendantPromptPanel.SetActive(false);
            if (registrationModalPanel == null) return;

            bool isBoutActive = boutController != null && boutController.IsBoutInProgress;
            if (isBoutActive)
            {
                registrationModalPanel.SetActive(false);
                return;
            }

            registrationModalPanel.SetActive(true);

            if (modalTitleText != null)
            {
                modalTitleText.text = $"{attendant.AttendantName.ToUpper()} // {attendant.RoleTitle.ToUpper()}";
            }

            if (modalDialogueText != null)
            {
                modalDialogueText.text = $"\"{attendant.DialogueMessage}\"";
            }

            if (modalOpponentInfoText != null && boutController != null && boutController.Gladiator != null)
            {
                modalOpponentInfoText.text = $"<b>Opponent:</b> {boutController.Gladiator.GladiatorName}\n<b>Sponsor:</b> {boutController.Gladiator.CorporateSponsor}";
            }

            if (modalStandingText != null && boutController != null)
            {
                modalStandingText.text = $"<b>Division Standing:</b> {boutController.CurrentRank}";
            }

            if (modalPurseText != null && boutController != null)
            {
                modalPurseText.text = $"<b>Sanctioned Prize Purse:</b> <color=#FFD700>+{boutController.PrizePurse} Credits</color>";
            }
        }

        private void HandleAttendantClosed()
        {
            if (registrationModalPanel != null)
            {
                registrationModalPanel.SetActive(false);
            }
        }

        private void HandleEnterRingClicked()
        {
            if (registrationModalPanel != null)
            {
                registrationModalPanel.SetActive(false);
            }

            if (ArenaAttendantNPC.ActiveInstance != null)
            {
                ArenaAttendantNPC.ActiveInstance.CloseAttendant();
            }

            if (boutController != null)
            {
                boutController.StartBout();
            }
        }

        private void HandleCloseModalClicked()
        {
            if (ArenaAttendantNPC.ActiveInstance != null)
            {
                ArenaAttendantNPC.ActiveInstance.CloseAttendant();
            }
            else if (registrationModalPanel != null)
            {
                registrationModalPanel.SetActive(false);
            }
        }

        #endregion

        #region Bout State & Countdown Handlers

        private void HandleCountdownTick(int secondsRemaining)
        {
            if (announcerBannerRoot == null || announcerMainText == null) return;

            announcerBannerRoot.SetActive(true);

            if (secondsRemaining > 0)
            {
                announcerMainText.text = $"<color=#FFCC00>{secondsRemaining}</color>";
                if (announcerSubText != null)
                {
                    announcerSubText.text = "SANCTIONED BOUT COMMENCING";
                }
            }
            else
            {
                announcerMainText.text = "<color=#00FFAA>FIGHT!</color>";
                if (announcerSubText != null)
                {
                    announcerSubText.text = "ROUND 1";
                }

                // Hide banner shortly after fight starts
                Invoke(nameof(HideAnnouncerBanner), 1.4f);
            }
        }

        private void HandleStateChanged(ArenaBoutState state)
        {
            if (state == ArenaBoutState.Idle)
            {
                if (opponentBarRoot != null) opponentBarRoot.SetActive(false);
                if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
                if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            }
            else if (state == ArenaBoutState.PreMatch)
            {
                if (opponentBarRoot != null) opponentBarRoot.SetActive(true);
                if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
                if (attendantPromptPanel != null) attendantPromptPanel.SetActive(false);
                if (registrationModalPanel != null) registrationModalPanel.SetActive(false);
            }
            else if (state == ArenaBoutState.ActiveBout)
            {
                if (opponentBarRoot != null) opponentBarRoot.SetActive(true);
                if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
                if (attendantPromptPanel != null) attendantPromptPanel.SetActive(false);
                if (registrationModalPanel != null) registrationModalPanel.SetActive(false);
            }
        }

        private void HandleMatchConcluded(bool playerWon, int prizeCredits, string newRank)
        {
            CancelInvoke(nameof(HideAnnouncerBanner));

            if (announcerBannerRoot != null)
            {
                announcerBannerRoot.SetActive(true);
            }

            if (playerWon)
            {
                if (announcerMainText != null)
                {
                    announcerMainText.text = "<color=#FFD700>VICTORY!</color>";
                }
                if (announcerSubText != null)
                {
                    announcerSubText.text = $"Purse: <b>+{prizeCredits} Credits</b>  |  Promoted to: <b>{newRank}</b>";
                }
            }
            else
            {
                if (announcerMainText != null)
                {
                    announcerMainText.text = "<color=#FF4444>DEFEAT</color>";
                }
                if (announcerSubText != null)
                {
                    announcerSubText.text = "Knocked Out in Sanctioned Combat";
                }
            }

            if (divisionRankText != null)
            {
                divisionRankText.text = $"<b>DIVISION LADDER</b>\n{newRank}";
            }

            // Launch Post-Match Outcome Modal with action choices
            StartCoroutine(ShowOutcomeModalRoutine(playerWon, prizeCredits, newRank));
        }

        private IEnumerator ShowOutcomeModalRoutine(bool playerWon, int prizeCredits, string newRank)
        {
            yield return new WaitForSeconds(1.2f);

            if (outcomeModalPanel != null)
            {
                outcomeModalPanel.SetActive(true);

                if (outcomeTitleText != null)
                {
                    outcomeTitleText.text = playerWon ? "<color=#FFD700>VICTORY!</color>" : "<color=#FF4444>DEFEAT</color>";
                }

                if (outcomeSubtitleText != null)
                {
                    outcomeSubtitleText.text = playerWon
                        ? "SANCTIONED TOURNAMENT BOUT CONCLUDED"
                        : "KNOCKED OUT IN SANCTIONED COMBAT";
                }

                if (outcomeStandingText != null)
                {
                    outcomeStandingText.text = $"<b>Division Standing:</b> <color=#38BDF8>{newRank}</color>";
                }

                if (outcomeRewardsText != null)
                {
                    outcomeRewardsText.text = playerWon
                        ? $"<b>Prize Purse Awarded:</b> <color=#FFD700>+{prizeCredits} Credits</color>"
                        : "<b>Medical Recovery:</b> Stabilized by Aegis emergency attendants.";
                }

                if (nextBoutButtonText != null)
                {
                    nextBoutButtonText.text = playerWon ? "PROCEED TO NEXT BOUT [ENTER]" : "REMATCH BOUT [ENTER]";
                }
            }
        }

        public void HandleNextBoutClicked()
        {
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
            if (boutController != null)
            {
                boutController.StartNextBout();
            }
        }

        public void HandleExitToConcourseClicked()
        {
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
            if (boutController != null)
            {
                boutController.ReturnToConcourse();
            }
        }

        public void HandleLeaveArenaClicked()
        {
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (announcerBannerRoot != null) announcerBannerRoot.SetActive(false);
            if (boutController != null)
            {
                boutController.ExitToCentralHub();
            }
        }

        public void HandleCloseOutcomeModalClicked()
        {
            if (outcomeModalPanel != null) outcomeModalPanel.SetActive(false);
            if (boutController != null && boutController.CurrentState != ArenaBoutState.ActiveBout)
            {
                boutController.ResetToIdleState();
            }
        }

        private void HandleGladiatorHealthChanged(float current, float max)
        {
            UpdateHealthBar(current, max);
        }

        private void UpdateHealthBar(float current, float max)
        {
            float fill = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (opponentHealthFill != null)
            {
                opponentHealthFill.fillAmount = fill;
            }

            if (opponentHealthNumberText != null)
            {
                opponentHealthNumberText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)} HP";
            }
        }

        private void HideAnnouncerBanner()
        {
            if (announcerBannerRoot != null && boutController != null && boutController.CurrentState == ArenaBoutState.ActiveBout)
            {
                announcerBannerRoot.SetActive(false);
            }
        }

        #endregion

        public void SetReferences(
            ArenaBoutController controller,
            GameObject barRoot, Text oppName, Text oppSponsor, Image oppFill, Text oppHpNum,
            GameObject bannerRoot, Text mainText, Text subText,
            Text rankText)
        {
            boutController = controller;
            opponentBarRoot = barRoot;
            opponentNameText = oppName;
            opponentSponsorText = oppSponsor;
            opponentHealthFill = oppFill;
            opponentHealthNumberText = oppHpNum;
            announcerBannerRoot = bannerRoot;
            announcerMainText = mainText;
            announcerSubText = subText;
            divisionRankText = rankText;
        }

        public void SetAttendantUIReferences(
            GameObject promptPnl, Text pText,
            GameObject modalPnl, Text mTitle, Text mDialogue, Text mOpponent, Text mStanding, Text mPurse,
            Button enterBtn, Button closeBtn)
        {
            attendantPromptPanel = promptPnl;
            attendantPromptText = pText;
            registrationModalPanel = modalPnl;
            modalTitleText = mTitle;
            modalDialogueText = mDialogue;
            modalOpponentInfoText = mOpponent;
            modalStandingText = mStanding;
            modalPurseText = mPurse;
            enterRingButton = enterBtn;
            closeModalButton = closeBtn;
            HookButtons();
        }

        public void SetOutcomeUIReferences(
            GameObject modalPnl, Text title, Text subtitle, Text standing, Text rewards,
            Button nextBtn, Text nextBtnTxt, Button concourseBtn, Button exitBtn, Button closeBtn)
        {
            outcomeModalPanel = modalPnl;
            outcomeTitleText = title;
            outcomeSubtitleText = subtitle;
            outcomeStandingText = standing;
            outcomeRewardsText = rewards;
            nextBoutButton = nextBtn;
            nextBoutButtonText = nextBtnTxt;
            exitToConcourseButton = concourseBtn;
            leaveArenaButton = exitBtn;
            closeOutcomeModalButton = closeBtn;
            HookButtons();
        }
    }
}
