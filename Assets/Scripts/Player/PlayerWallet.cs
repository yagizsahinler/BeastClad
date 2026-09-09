using System;
using UnityEngine;

namespace BeastClad.Player
{
    /// <summary>
    /// Tracks the player's currency (Credits) acquired through guild bounties,
    /// commercial trading, and arena prize purses.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Starting Balance")]
        [SerializeField] private int startingCredits = 0;

        public int Credits { get; private set; }

        public event Action<int> OnCreditsChanged;

        private void Awake()
        {
            Credits = startingCredits;
        }

        public void AddCredits(int amount)
        {
            if (amount <= 0) return;

            Credits += amount;
            Debug.Log($"<color=#FFD700>[Wallet]</color> Received <b>+{amount} Credits</b>. Current Balance: <b>{Credits} Credits</b>.");
            OnCreditsChanged?.Invoke(Credits);
        }

        public bool TrySpendCredits(int amount)
        {
            if (amount <= 0) return false;

            if (Credits >= amount)
            {
                Credits -= amount;
                Debug.Log($"<color=#FFD700>[Wallet]</color> Spent <b>-{amount} Credits</b>. Current Balance: <b>{Credits} Credits</b>.");
                OnCreditsChanged?.Invoke(Credits);
                return true;
            }

            Debug.LogWarning($"<color=#FF6666>[Wallet]</color> Insufficient credits! Need {amount}, have {Credits}.");
            return false;
        }
    }
}
