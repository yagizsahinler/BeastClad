using System;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Player;
using BeastClad.Security;

namespace BeastClad.World
{
    public enum TransitionTriggerMode
    {
        WalkThrough,
        InteractPrompt
    }

    /// <summary>
    /// Interactive or walk-through portal trigger placed at district thresholds.
    /// Integrates optional MunicipalScannerZone security gating and triggers
    /// smooth transitions via SceneTransitionManager.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("Target Destination")]
        [Tooltip("Exact name of the scene file to load (must be in Build Settings)")]
        [SerializeField] private string targetSceneName = "District_CentralHub";

        [Tooltip("Matching spawnTag of SceneSpawnPoint in target scene")]
        [SerializeField] private string targetSpawnTag = "Spawn_Default";

        [Header("Trigger Settings")]
        [SerializeField] private TransitionTriggerMode mode = TransitionTriggerMode.InteractPrompt;
        [SerializeField] private string promptText = "[ F ] Enter District";

        [Header("Security Gating (Optional)")]
        [Tooltip("If assigned, checks scanner clearance before permitting passage")]
        [SerializeField] private MunicipalScannerZone requiredScanner;
        [SerializeField] private bool denyOnViolation = true;
        [SerializeField] private string denialWarning = "ACCESS DENIED: Unlicensed Biometrics Detected!";

        private bool isPlayerNear = false;
        private GameObject cachedPlayer;

        public string TargetSceneName => targetSceneName;
        public string TargetSpawnTag => targetSpawnTag;
        public TransitionTriggerMode Mode => mode;
        public string PromptText => promptText;

        public static event Action<SceneTransitionTrigger, bool, string> OnPromptChanged;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Update()
        {
            if (!isPlayerNear || mode != TransitionTriggerMode.InteractPrompt) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.fKey.wasPressedThisFrame)
            {
                AttemptTransition();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNear = true;
                cachedPlayer = other.gameObject;

                if (mode == TransitionTriggerMode.WalkThrough)
                {
                    AttemptTransition();
                }
                else
                {
                    OnPromptChanged?.Invoke(this, true, promptText);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                isPlayerNear = false;
                cachedPlayer = null;
                OnPromptChanged?.Invoke(this, false, string.Empty);
            }
        }

        private void AttemptTransition()
        {
            // Security Scanner Check
            if (requiredScanner != null && denyOnViolation)
            {
                var infuse = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerInfuseManager>() : null;
                var roster = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerMonsterRoster>() : null;
                var report = requiredScanner.PerformScan(infuse, roster);

                if (!report.IsClear)
                {
                    Debug.LogWarning($"<color=#FF0044>[Security Gate]</color> Clearance rejected at <b>{gameObject.name}</b>: {report.GetSummaryText()}");
                    OnPromptChanged?.Invoke(this, true, $"<color=#FF3344>{denialWarning}</color>");
                    return;
                }
            }

            OnPromptChanged?.Invoke(this, false, string.Empty);
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetSpawnTag);
        }
    }
}
