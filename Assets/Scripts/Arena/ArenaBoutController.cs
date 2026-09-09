using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Player;

namespace BeastClad.Arena
{
    public enum ArenaBoutState
    {
        PreMatch,
        ActiveBout,
        Victory,
        Defeat
    }

    /// <summary>
    /// Master controller for sanctioned corporate arena bouts.
    /// Manages match countdown, gladiator activation, outcome detection,
    /// division ladder rank promotion, and prize purse payouts.
    /// </summary>
    [DisallowMultipleComponent]
    public class ArenaBoutController : MonoBehaviour
    {
        [Header("Participant References")]
        [SerializeField] private PlayerStatsComponent playerStats;
        [SerializeField] private PlayerWallet playerWallet;
        [SerializeField] private SanctionedGladiatorAI2D gladiator;

        [Header("Match Settings")]
        [SerializeField] private int countdownDuration = 3;
        [SerializeField] private int prizePurseCredits = 500;

        [Header("Division Ladder")]
        [SerializeField] private List<string> divisionRanks = new List<string>
        {
            "Bronze League - Rank III",
            "Bronze League - Rank II",
            "Bronze League - Rank I",
            "Silver League - Rank III"
        };
        private int currentRankIndex = 0;

        private ArenaBoutState currentState = ArenaBoutState.PreMatch;
        private Vector3 playerStartPos;
        private Vector3 gladiatorStartPos;

        public ArenaBoutState CurrentState => currentState;
        public string CurrentRank => divisionRanks.Count > currentRankIndex ? divisionRanks[currentRankIndex] : "Bronze League";
        public SanctionedGladiatorAI2D Gladiator => gladiator;
        public PlayerStatsComponent PlayerStats => playerStats;
        public int PrizePurse => prizePurseCredits;

        public event Action<ArenaBoutState> OnStateChanged;
        public event Action<int> OnCountdownTick;
        public event Action<bool, int, string> OnMatchConcluded; // (playerWon, prizePurse, newRank)

        private void Awake()
        {
            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStatsComponent>();
            if (playerWallet == null) playerWallet = FindAnyObjectByType<PlayerWallet>();
            if (gladiator == null) gladiator = FindAnyObjectByType<SanctionedGladiatorAI2D>();
        }

        private void OnEnable()
        {
            if (gladiator != null)
            {
                gladiator.OnDefeated += HandleGladiatorDefeated;
            }

            if (playerStats != null)
            {
                playerStats.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (gladiator != null)
            {
                gladiator.OnDefeated -= HandleGladiatorDefeated;
            }

            if (playerStats != null)
            {
                playerStats.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Start()
        {
            if (playerStats != null) playerStartPos = playerStats.transform.position;
            if (gladiator != null) gladiatorStartPos = gladiator.transform.position;

            StartCoroutine(BoutCountdownRoutine());
        }

        private IEnumerator BoutCountdownRoutine()
        {
            currentState = ArenaBoutState.PreMatch;
            OnStateChanged?.Invoke(currentState);

            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pCtrl != null) pCtrl.SetMovementLocked(true);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);
            if (gladiator != null) gladiator.EnableCombat(false);

            int remaining = countdownDuration;
            while (remaining > 0)
            {
                OnCountdownTick?.Invoke(remaining);
                yield return new WaitForSeconds(1.0f);
                remaining--;
            }

            // Fight!
            OnCountdownTick?.Invoke(0);
            currentState = ArenaBoutState.ActiveBout;
            OnStateChanged?.Invoke(currentState);

            if (pCtrl != null) pCtrl.SetMovementLocked(false);
            if (pInfuse != null) pInfuse.SetCombatEnabled(true);
            if (gladiator != null)
            {
                gladiator.EnableCombat(true);
            }

            Debug.Log("<color=#FFD700>[Arena Announcer]</color> <b>ROUND 1 — FIGHT!</b> Sanctioned bout has commenced!");
        }

        private void HandleGladiatorDefeated(SanctionedGladiatorAI2D defeatedGladiator)
        {
            if (currentState != ArenaBoutState.ActiveBout) return;

            currentState = ArenaBoutState.Victory;
            OnStateChanged?.Invoke(currentState);

            // Lock player movement and combat upon victory
            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pCtrl != null) pCtrl.SetMovementLocked(true);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);

            // Award prize purse
            if (playerWallet != null)
            {
                playerWallet.AddCredits(prizePurseCredits);
            }

            // Increment Division Ladder Rank
            if (currentRankIndex < divisionRanks.Count - 1)
            {
                currentRankIndex++;
            }
            string newRank = CurrentRank;

            Debug.Log($"<color=#FFD700>[Arena Victory!]</color> Player won the bout! Awarded <b>+{prizePurseCredits} Credits</b>. Promoted to: <b>{newRank}</b>.");
            OnMatchConcluded?.Invoke(true, prizePurseCredits, newRank);
        }

        private void HandlePlayerDeath()
        {
            if (currentState != ArenaBoutState.ActiveBout) return;

            currentState = ArenaBoutState.Defeat;
            OnStateChanged?.Invoke(currentState);

            // Lock player movement and combat upon defeat
            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pCtrl != null) pCtrl.SetMovementLocked(true);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);

            if (gladiator != null)
            {
                gladiator.EnableCombat(false);
            }

            Debug.Log("<color=#FF4444>[Arena Defeat]</color> Player was knocked out! Bout terminated.");
            OnMatchConcluded?.Invoke(false, 0, CurrentRank);
        }

        public void RestartBout()
        {
            StopAllCoroutines();

            if (playerStats != null)
            {
                playerStats.transform.position = playerStartPos;
                playerStats.ResetHealth();
            }

            if (gladiator != null)
            {
                gladiator.ResetGladiator(gladiatorStartPos);
            }

            StartCoroutine(BoutCountdownRoutine());
        }

        public void SetReferences(PlayerStatsComponent stats, PlayerWallet wallet, SanctionedGladiatorAI2D glad)
        {
            playerStats = stats;
            playerWallet = wallet;
            gladiator = glad;
        }
    }
}
