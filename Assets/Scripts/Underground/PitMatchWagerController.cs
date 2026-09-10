using System;
using System.Collections;
using UnityEngine;
using BeastClad.Player;

namespace BeastClad.Underground
{
    public enum PitBoutState
    {
        Idle,
        WagerPlaced,
        Countdown,
        ActiveBout,
        Victory,
        Defeat
    }

    /// <summary>
    /// Master controller for the unsanctioned underground pit bouts.
    /// Handles wager deduction, match countdown, brawler activation,
    /// victory detection with 2.5x payout, and defeat handling.
    /// </summary>
    [DisallowMultipleComponent]
    public class PitMatchWagerController : MonoBehaviour
    {
        [Header("Participant References")]
        [SerializeField] private PlayerStatsComponent playerStats;
        [SerializeField] private PlayerWallet playerWallet;
        [SerializeField] private UnsanctionedPitBrawlerAI2D brawler;

        [Header("Arena Positions")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform brawlerSpawnPoint;
        [SerializeField] private Transform hubExitPoint;

        [Header("Wager Economics")]
        [SerializeField] private float payoutMultiplier = 2.5f;
        [SerializeField] private int countdownDuration = 3;

        private PitBoutState currentState = PitBoutState.Idle;
        private int currentWager = 250;

        public PitBoutState CurrentState => currentState;
        public int CurrentWager => currentWager;
        public float PayoutMultiplier => payoutMultiplier;
        public UnsanctionedPitBrawlerAI2D Brawler => brawler;
        public PlayerWallet Wallet => playerWallet;

        public event Action<PitBoutState> OnStateChanged;
        public event Action<int> OnCountdownTick;
        public event Action<bool, int, int> OnBoutConcluded; // (playerWon, payoutCredits, wager)
        public event Action OnReturnedToHub;

        private void Awake()
        {
            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStatsComponent>();
            if (playerWallet == null) playerWallet = FindAnyObjectByType<PlayerWallet>();
            if (brawler == null) brawler = FindAnyObjectByType<UnsanctionedPitBrawlerAI2D>();
        }

        private void Start()
        {
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : FindAnyObjectByType<PlayerInfuseManager>();
            if (pInfuse != null)
            {
                pInfuse.SetCombatEnabled(false);
            }
        }

        private void OnEnable()
        {
            if (brawler != null)
            {
                brawler.OnDefeated += HandleBrawlerDefeated;
            }

            if (playerStats != null)
            {
                playerStats.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (brawler != null)
            {
                brawler.OnDefeated -= HandleBrawlerDefeated;
            }

            if (playerStats != null)
            {
                playerStats.OnDeath -= HandlePlayerDeath;
            }
        }

        public bool CanAffordWager(int amount)
        {
            if (playerWallet == null) playerWallet = FindAnyObjectByType<PlayerWallet>();
            return playerWallet != null && playerWallet.Credits >= amount;
        }

        public bool StartBoutWithWager(int wagerAmount)
        {
            if (currentState == PitBoutState.Countdown || currentState == PitBoutState.ActiveBout)
            {
                Debug.LogWarning("[Underground Pit] Bout is already in progress!");
                return false;
            }

            if (playerWallet == null) playerWallet = FindAnyObjectByType<PlayerWallet>();
            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStatsComponent>();
            if (brawler == null) brawler = FindAnyObjectByType<UnsanctionedPitBrawlerAI2D>();

            if (playerWallet == null || !playerWallet.TrySpendCredits(wagerAmount))
            {
                Debug.LogWarning($"[Underground Pit] Insufficient credits to wager {wagerAmount} CR!");
                return false;
            }

            currentWager = wagerAmount;
            Debug.Log($"<color=#FFCC00>[Pit Wager Placed]</color> Wagered <b>{currentWager} CR</b> on unsanctioned pit bout!");

            // Teleport fighters to pit ring
            if (playerStats != null && playerSpawnPoint != null)
            {
                playerStats.transform.position = playerSpawnPoint.position;
                var rb = playerStats.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = playerSpawnPoint.position;
            }

            if (brawler != null)
            {
                Vector3 brawlerPos = brawlerSpawnPoint != null ? brawlerSpawnPoint.position : brawler.transform.position;
                brawler.ResetBrawler(brawlerPos);
            }

            StartCoroutine(CountdownRoutine());
            return true;
        }

        private IEnumerator CountdownRoutine()
        {
            currentState = PitBoutState.Countdown;
            OnStateChanged?.Invoke(currentState);

            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;

            if (pCtrl != null) pCtrl.SetMovementLocked(true);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);
            if (brawler != null) brawler.EnableCombat(false);

            int remaining = countdownDuration;
            while (remaining > 0)
            {
                OnCountdownTick?.Invoke(remaining);
                yield return new WaitForSeconds(1.0f);
                remaining--;
            }

            // Bout Begins
            OnCountdownTick?.Invoke(0);
            currentState = PitBoutState.ActiveBout;
            OnStateChanged?.Invoke(currentState);

            if (pCtrl != null) pCtrl.SetMovementLocked(false);
            if (pInfuse != null) pInfuse.SetCombatEnabled(true);
            if (brawler != null) brawler.EnableCombat(true);

            Debug.Log("<color=#FF0055>[Underground Pit]</color> <b>FIGHT!</b> Unsanctioned bout in progress!");
        }

        private void HandleBrawlerDefeated(UnsanctionedPitBrawlerAI2D defeatedBrawler)
        {
            if (currentState != PitBoutState.ActiveBout) return;

            currentState = PitBoutState.Victory;
            int payout = Mathf.RoundToInt(currentWager * payoutMultiplier);

            if (playerWallet != null)
            {
                playerWallet.AddCredits(payout);
            }

            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);

            Debug.Log($"<color=#00FFAA>[Pit Victory!]</color> Player won unsanctioned bout! Payout: <b>+{payout} CR</b> (Wager: {currentWager} CR, 2.5x Multiplier).");

            OnStateChanged?.Invoke(currentState);
            OnBoutConcluded?.Invoke(true, payout, currentWager);
        }

        private void HandlePlayerDeath()
        {
            if (currentState != PitBoutState.ActiveBout) return;

            currentState = PitBoutState.Defeat;

            if (brawler != null)
            {
                brawler.EnableCombat(false);
            }

            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pCtrl != null) pCtrl.SetMovementLocked(true);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);

            Debug.Log($"<color=#FF0000>[Pit Defeat]</color> Player collapsed in the pit! Forfeited <b>{currentWager} CR</b>.");

            OnStateChanged?.Invoke(currentState);
            OnBoutConcluded?.Invoke(false, 0, currentWager);
        }

        public void ReturnToHub()
        {
            if (playerStats != null)
            {
                playerStats.ResetHealth();

                if (hubExitPoint != null)
                {
                    playerStats.transform.position = hubExitPoint.position;
                    var rb = playerStats.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.position = hubExitPoint.position;
                }

                var pCtrl = playerStats.GetComponent<PlayerController2D>();
                var pInfuse = playerStats.GetComponent<PlayerInfuseManager>();
                if (pCtrl != null) pCtrl.SetMovementLocked(false);
                if (pInfuse != null) pInfuse.SetCombatEnabled(false);
            }

            if (brawler != null && brawlerSpawnPoint != null)
            {
                brawler.ResetBrawler(brawlerSpawnPoint.position);
            }

            currentState = PitBoutState.Idle;
            OnStateChanged?.Invoke(currentState);
            OnReturnedToHub?.Invoke();
        }
    }
}
