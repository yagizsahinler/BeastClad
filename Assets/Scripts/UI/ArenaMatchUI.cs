using UnityEngine;
using UnityEngine.UI;
using BeastClad.Arena;

namespace BeastClad.UI
{
    /// <summary>
    /// HUD controller for sanctioned corporate arena bouts.
    /// Manages gladiator boss health bar, central announcer banner,
    /// and division ladder rank progression display.
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

        private void Awake()
        {
            if (boutController == null)
            {
                boutController = FindAnyObjectByType<ArenaBoutController>();
            }
        }

        private void OnEnable()
        {
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
        }

        private void OnDisable()
        {
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
        }

        private void Start()
        {
            InitializeDisplay();
        }

        private void InitializeDisplay()
        {
            if (boutController == null) return;

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
            if (state == ArenaBoutState.ActiveBout)
            {
                if (opponentBarRoot != null) opponentBarRoot.SetActive(true);
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
    }
}
