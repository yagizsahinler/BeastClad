using UnityEngine;
using UnityEngine.UI;
using BeastClad.Player;

namespace BeastClad.UI
{
    /// <summary>
    /// HUD widget displaying the player's current credit balance.
    /// Dynamically listens to PlayerWallet events.
    /// </summary>
    [DisallowMultipleComponent]
    public class WalletHUDUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWallet wallet;
        [SerializeField] private Text creditsText;

        [Header("Styling")]
        [SerializeField] private string prefix = "CREDITS:";
        [SerializeField] private string suffix = "CR";

        private void Awake()
        {
            if (wallet == null)
            {
                wallet = FindAnyObjectByType<PlayerWallet>();
            }
        }

        private void OnEnable()
        {
            if (wallet != null)
            {
                wallet.OnCreditsChanged += UpdateDisplay;
                UpdateDisplay(wallet.Credits);
            }
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.OnCreditsChanged -= UpdateDisplay;
            }
        }

        private void Start()
        {
            if (wallet != null)
            {
                UpdateDisplay(wallet.Credits);
            }
        }

        public void UpdateDisplay(int currentCredits)
        {
            if (creditsText != null)
            {
                creditsText.text = $"<b>{prefix}</b> <color=#FFD700>{currentCredits:N0} {suffix}</color>";
            }
        }

        public void SetReferences(PlayerWallet playerWallet, Text text)
        {
            wallet = playerWallet;
            creditsText = text;
            if (wallet != null)
            {
                UpdateDisplay(wallet.Credits);
            }
        }
    }
}
