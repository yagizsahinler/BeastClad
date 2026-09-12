using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BeastClad.Player;

namespace BeastClad.World
{
    /// <summary>
    /// Persistent singleton controller managing seamless scene transitions.
    /// Handles cinematic fade overlays, asynchronous scene loading, spawn point positioning,
    /// and player session data restoration.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var found = FindAnyObjectByType<SceneTransitionManager>();
                    if (found != null)
                    {
                        instance = found;
                    }
                    else
                    {
                        var go = new GameObject("SceneTransitionManager");
                        instance = go.AddComponent<SceneTransitionManager>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Transition Settings")]
        [SerializeField] private float fadeDuration = 0.35f;

        private CanvasGroup fadeCanvasGroup;
        private bool isTransitioning = false;

        public bool IsTransitioning => isTransitioning;
        public event Action<string> OnTransitionStarted;
        public event Action<string> OnTransitionCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (instance == null)
            {
                var go = new GameObject("SceneTransitionManager");
                instance = go.AddComponent<SceneTransitionManager>();
                if (Application.isPlaying) DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            EnsureFadeCanvas();
        }

        private void EnsureFadeCanvas()
        {
            if (fadeCanvasGroup != null) return;

            var canvasObj = new GameObject("TransitionFadeCanvas");
            canvasObj.transform.SetParent(transform, false);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Above all game and HUD elements

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;

            var imgObj = new GameObject("FadeOverlay");
            imgObj.transform.SetParent(canvasObj.transform, false);
            var rect = imgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var img = imgObj.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;
        }

        /// <summary>
        /// Initiates a smooth scene transition to the target destination.
        /// </summary>
        public void TransitionToScene(string sceneName, string targetSpawnTag = "Spawn_Default")
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[SceneTransition] Transition to '{sceneName}' requested while already transitioning!");
                return;
            }

            StartCoroutine(TransitionRoutine(sceneName, targetSpawnTag));
        }

        private IEnumerator TransitionRoutine(string sceneName, string targetSpawnTag)
        {
            isTransitioning = true;
            EnsureFadeCanvas();
            OnTransitionStarted?.Invoke(sceneName);

            // 1. Lock player & capture state
            var player = FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.SetMovementLocked(true);
                PlayerSessionState.CaptureFromPlayer(player.gameObject);
            }
            PlayerSessionState.PendingSpawnTag = targetSpawnTag;

            // 2. Fade to black
            fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;

            // 3. Load destination scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneTransition] Failed to load scene '{sceneName}'! Is it added to Build Settings?");
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
                isTransitioning = false;
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Allow Awake() and Start() of loaded scene to execute
            yield return null;
            yield return new WaitForEndOfFrame();

            // 4. Locate player in new scene & reposition
            var newPlayer = FindAnyObjectByType<PlayerController2D>();
            if (newPlayer != null)
            {
                newPlayer.SetMovementLocked(true);

                // Reposition at designated spawn point
                var spawn = SceneSpawnPoint.FindByTag(targetSpawnTag);
                if (spawn != null)
                {
                    newPlayer.transform.position = spawn.transform.position;
                    var rb = newPlayer.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.position = spawn.transform.position;
                    }
                    Physics2D.SyncTransforms();
                    newPlayer.SetFacingDirection(spawn.DefaultFacing);
                    Debug.Log($"<color=#00FFAA>[SceneTransition]</color> Teleported player to <b>{spawn.SpawnTag}</b> at {spawn.transform.position}.");
                }

                // Snap camera immediately
                var camFollow = FindAnyObjectByType<CameraFollow2D>();
                if (camFollow != null)
                {
                    camFollow.SetTarget(newPlayer.transform);
                }

                // Hydrate persisted player session data
                PlayerSessionState.ApplyToPlayer(newPlayer.gameObject);
                newPlayer.SetMovementLocked(false);
            }

            // 5. Fade in from black
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            isTransitioning = false;

            OnTransitionCompleted?.Invoke(sceneName);
            Persistence.SaveManager.Instance.SaveCurrentGame(sceneName);
            Debug.Log($"<color=#00FFAA>[SceneTransition]</color> Successfully entered <b>{sceneName}</b> at '{targetSpawnTag}'.");
        }
    }
}
