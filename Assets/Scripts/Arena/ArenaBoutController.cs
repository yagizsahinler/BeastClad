using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeastClad.Player;

namespace BeastClad.Arena
{
    public enum ArenaBoutState
    {
        Idle,
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

        private ArenaBoutState currentState = ArenaBoutState.Idle;
        [SerializeField] private Vector3 challengerRingPosition = new Vector3(-0.5f, 0f, 0f);
        [SerializeField] private Vector3 concourseSpawnPosition = new Vector3(-6.8f, -1.0f, 0f);
        [SerializeField] private GameObject ringGateBarrier;
        private Vector3 playerStartPos;
        private Vector3 gladiatorStartPos;

        public ArenaBoutState CurrentState => currentState;
        public string CurrentRank => divisionRanks.Count > currentRankIndex ? divisionRanks[currentRankIndex] : "Bronze League";
        public SanctionedGladiatorAI2D Gladiator => gladiator;
        public PlayerStatsComponent PlayerStats => playerStats;
        public int PrizePurse => prizePurseCredits;
        public Vector3 ConcourseSpawnPosition => concourseSpawnPosition;
        public GameObject RingGateBarrier => ringGateBarrier;

        private static ArenaBoutController activeInstance;
        public static ArenaBoutController ActiveInstance
        {
            get
            {
                if (activeInstance == null) activeInstance = FindAnyObjectByType<ArenaBoutController>();
                return activeInstance;
            }
            private set => activeInstance = value;
        }
        public bool IsBoutInProgress => currentState == ArenaBoutState.PreMatch || currentState == ArenaBoutState.ActiveBout;

        public event Action<ArenaBoutState> OnStateChanged;
        public event Action<int> OnCountdownTick;
        public event Action<bool, int, string> OnMatchConcluded; // (playerWon, prizePurse, newRank)

        private void Awake()
        {
            ActiveInstance = this;
            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStatsComponent>();
            if (playerWallet == null) playerWallet = FindAnyObjectByType<PlayerWallet>();
            if (gladiator == null) gladiator = FindAnyObjectByType<SanctionedGladiatorAI2D>();

            if (World.PlayerSessionState.HasActiveSession)
            {
                currentRankIndex = Mathf.Clamp(World.PlayerSessionState.ArenaRankIndex, 0, divisionRanks.Count - 1);
            }
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

        private void OnDestroy()
        {
            if (ActiveInstance == this) ActiveInstance = null;
        }

        private void Start()
        {
            if (playerStats != null) playerStartPos = playerStats.transform.position;
            if (gladiator != null) gladiatorStartPos = gladiator.transform.position;

            ResetToIdleState();
        }

        public void SetRingGateBarrier(GameObject barrier)
        {
            ringGateBarrier = barrier;
            UpdateGateBarrier();
        }

        private void UpdateGateBarrier()
        {
            if (ringGateBarrier != null)
            {
                bool closeGate = (currentState == ArenaBoutState.PreMatch || currentState == ArenaBoutState.ActiveBout);
                ringGateBarrier.SetActive(closeGate);
            }
        }

        public void ResetToIdleState()
        {
            StopAllCoroutines();
            CancelInvoke();
            currentState = ArenaBoutState.Idle;
            UpdateGateBarrier();
            OnStateChanged?.Invoke(currentState);

            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pCtrl != null) pCtrl.SetMovementLocked(false);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);
            if (gladiator != null)
            {
                gladiator.EnableCombat(false);
                gladiator.ResetGladiator(gladiatorStartPos);
            }
        }

        public void StartBout()
        {
            if (currentState == ArenaBoutState.PreMatch || currentState == ArenaBoutState.ActiveBout)
            {
                Debug.LogWarning("[ArenaBoutController] Bout is already active!");
                return;
            }

            StopAllCoroutines();
            CancelInvoke();

            if (ArenaAttendantNPC.ActiveInstance != null)
            {
                ArenaAttendantNPC.ActiveInstance.CloseAttendant();
            }

            if (playerStats != null)
            {
                Vector3 spawnPos = challengerRingPosition != Vector3.zero ? challengerRingPosition : playerStartPos;
                playerStats.transform.position = spawnPos;
                var rb = playerStats.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = spawnPos;
                Physics2D.SyncTransforms();

                playerStats.ResetHealth();
                var pCtrl = playerStats.GetComponent<PlayerController2D>();
                if (pCtrl != null)
                {
                    pCtrl.SetFacingDirection(Vector2.right);
                    pCtrl.SetMovementLocked(true);
                }
            }

            if (gladiator != null)
            {
                gladiator.ResetGladiator(gladiatorStartPos);
                gladiator.EnableCombat(false);
            }

            UpdateGateBarrier();
            StartCoroutine(BoutCountdownRoutine());
        }

        private IEnumerator BoutCountdownRoutine()
        {
            currentState = ArenaBoutState.PreMatch;
            UpdateGateBarrier();
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
            UpdateGateBarrier();
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
            if (currentState != ArenaBoutState.ActiveBout && currentState != ArenaBoutState.PreMatch) return;

            currentState = ArenaBoutState.Victory;
            UpdateGateBarrier();
            OnStateChanged?.Invoke(currentState);

            // Lock player movement and combat briefly upon victory celebration
            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            var pInfuse = playerStats != null ? playerStats.GetComponent<PlayerInfuseManager>() : null;
            if (pCtrl != null) pCtrl.SetMovementLocked(true);
            if (pInfuse != null) pInfuse.SetCombatEnabled(false);

            if (playerStats != null)
            {
                playerStats.ResetHealth();
            }

            // Re-enable movement after short celebration
            Invoke(nameof(UnlockPostMatchMovement), 1.2f);

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
            World.PlayerSessionState.SetArenaRankDirect(currentRankIndex, newRank);
            OnMatchConcluded?.Invoke(true, prizePurseCredits, newRank);
            Persistence.SaveManager.Instance.SaveCurrentGame();
        }

        private void UnlockPostMatchMovement()
        {
            var pCtrl = playerStats != null ? playerStats.GetComponent<PlayerController2D>() : null;
            if (pCtrl != null)
            {
                pCtrl.SetMovementLocked(false);
                Debug.Log("<color=#00FFAA>[Arena Concourse]</color> Movement unlocked. Player can freely explore and access kiosks.");
            }
        }

        private void HandlePlayerDeath()
        {
            if (currentState != ArenaBoutState.ActiveBout) return;

            currentState = ArenaBoutState.Defeat;
            UpdateGateBarrier();
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
            Persistence.SaveManager.Instance.SaveCurrentGame();

            StartCoroutine(PostDefeatRecoveryRoutine());
        }

        private IEnumerator PostDefeatRecoveryRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (playerStats != null)
            {
                playerStats.ResetHealth();
            }
            UpdateGateBarrier();
            Debug.Log("<color=#00FFAA>[Arena Medic]</color> Player stabilized.");
        }

        /// <summary>
        /// Moves player back to the concourse staging area, restores health, resets bout state to Idle,
        /// and unlocks movement so they can access the vendor, attendant, or exit portal.
        /// </summary>
        public void ReturnToConcourse()
        {
            StopAllCoroutines();
            CancelInvoke();

            Vector3 targetPos = concourseSpawnPosition != Vector3.zero ? concourseSpawnPosition : playerStartPos;
            if (playerStats != null)
            {
                playerStats.transform.position = targetPos;
                var rb = playerStats.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = targetPos;
                Physics2D.SyncTransforms();

                playerStats.ResetHealth();
                var pCtrl = playerStats.GetComponent<PlayerController2D>();
                if (pCtrl != null)
                {
                    pCtrl.SetMovementLocked(false);
                    pCtrl.SetFacingDirection(Vector2.right);
                }
                var pInfuse = playerStats.GetComponent<PlayerInfuseManager>();
                if (pInfuse != null) pInfuse.SetCombatEnabled(false);
            }

            ResetToIdleState();
            Debug.Log("<color=#00FFAA>[Arena Concourse]</color> Player returned to staging concourse.");
        }

        /// <summary>
        /// Starts the next tournament battle, healing the player and restarting the ring countdown.
        /// </summary>
        public void StartNextBout()
        {
            StartBout();
        }

        /// <summary>
        /// Transitions directly from the arena back to the Central District Hub.
        /// </summary>
        public void ExitToCentralHub()
        {
            if (World.SceneTransitionManager.Instance != null)
            {
                World.SceneTransitionManager.Instance.TransitionToScene("District_CentralHub", "Spawn_FromArena");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("District_CentralHub");
            }
        }

        public void SetRankIndex(int index)
        {
            currentRankIndex = Mathf.Clamp(index, 0, divisionRanks.Count - 1);
        }

        public void RestartBout()
        {
            StartBout();
        }

        public void SetReferences(PlayerStatsComponent stats, PlayerWallet wallet, SanctionedGladiatorAI2D glad)
        {
            playerStats = stats;
            playerWallet = wallet;
            gladiator = glad;
        }
    }
}
