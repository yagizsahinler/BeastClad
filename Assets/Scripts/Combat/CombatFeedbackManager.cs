using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BeastClad.World;
using BeastClad.Audio;
using BeastClad.UI;

namespace BeastClad.Combat
{
    /// <summary>
    /// Central manager coordinating combat impact feedback:
    /// Dynamic micro-hitstop (frame freezes), camera trauma shake, floating damage popups,
    /// and audio impact reinforcement. Fully decoupled and resilient.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatFeedbackManager : MonoBehaviour
    {
        private static CombatFeedbackManager instance;
        public static CombatFeedbackManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<CombatFeedbackManager>();
                    if (instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("[CombatFeedbackManager]");
                        instance = go.AddComponent<CombatFeedbackManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Hitstop Settings")]
        [SerializeField] private float lightHitstopDuration = 0.04f;
        [SerializeField] private float heavyHitstopDuration = 0.08f;
        [SerializeField] private float hitstopTimeScale = 0.05f;

        [Header("Camera Trauma Settings")]
        [SerializeField] private float lightTrauma = 0.20f;
        [SerializeField] private float heavyTrauma = 0.42f;

        [Header("Damage Colors")]
        [SerializeField] private Color hydroColor = new Color(0f, 0.9f, 1f, 1f);      // Cyan
        [SerializeField] private Color voltColor = new Color(1f, 0.9f, 0.1f, 1f);      // Gold / Yellow
        [SerializeField] private Color geoColor = new Color(0.95f, 0.6f, 0.2f, 1f);    // Warm Geo
        [SerializeField] private Color physicalColor = new Color(1f, 1f, 1f, 1f);       // Crisp White
        [SerializeField] private Color recoilColor = new Color(1f, 0.2f, 0.35f, 1f);   // Neon Red / Recoil
        [SerializeField] private Color guardColor = new Color(0.7f, 0.75f, 0.8f, 1f);   // Steel Gray

        // Internal pooling
        private Canvas worldCanvas;
        private readonly Queue<DamagePopup> popupPool = new Queue<DamagePopup>();
        private readonly List<DamagePopup> activePopups = new List<DamagePopup>();
        private const int PrewarmCount = 16;

        private Coroutine activeHitstopRoutine;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureWorldCanvas();
            PrewarmPopupPool();
        }

        private void EnsureWorldCanvas()
        {
            if (worldCanvas != null) return;

            var canvasObj = new GameObject("WorldSpace_CombatTextCanvas");
            canvasObj.transform.SetParent(transform, false);

            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingLayerName = "UI";
            worldCanvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20f;

            // Scale down canvas transform so UI pixels map nicely to 2D world units
            canvasObj.transform.localScale = Vector3.one * 0.02f;
        }

        private void PrewarmPopupPool()
        {
            if (worldCanvas == null) EnsureWorldCanvas();

            for (int i = 0; i < PrewarmCount; i++)
            {
                var popup = CreateNewPopupInstance();
                popup.gameObject.SetActive(false);
                popupPool.Enqueue(popup);
            }
        }

        private DamagePopup CreateNewPopupInstance()
        {
            var itemObj = new GameObject("DamagePopupItem");
            itemObj.transform.SetParent(worldCanvas.transform, false);

            var rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250f, 60f);

            var canvasGroup = itemObj.AddComponent<CanvasGroup>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var textComp = textObj.AddComponent<Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComp.verticalOverflow = VerticalWrapMode.Overflow;

            var popupComp = itemObj.AddComponent<DamagePopup>();
            return popupComp;
        }

        private void ReturnPopupToPool(DamagePopup popup)
        {
            if (popup == null) return;
            activePopups.Remove(popup);
            popupPool.Enqueue(popup);
        }

        #region Public Feedback API

        /// <summary>
        /// Main entry point for delivering complete combat feedback upon any successful hit.
        /// </summary>
        public void TriggerHitFeedback(Vector3 worldPos, DamagePayload payload, float actualDamageDealt, bool isCritical = false, bool isShielded = false)
        {
            // Determine elemental color
            Color textColor = physicalColor;
            if (payload.element != null)
            {
                switch (payload.element.elementId)
                {
                    case "Hydro":
                        textColor = hydroColor;
                        break;
                    case "Volt":
                        textColor = voltColor;
                        break;
                    case "Geo":
                        textColor = geoColor;
                        break;
                    default:
                        textColor = payload.element.elementColor;
                        break;
                }
            }

            // High damage / overclock recoil threshold
            bool isHeavy = isCritical || actualDamageDealt >= 25f;

            // Spawn floating damage popup
            string displayText;
            if (isShielded)
            {
                displayText = "GUARD";
                textColor = guardColor;
            }
            else if (isHeavy)
            {
                displayText = $"CRIT! {actualDamageDealt:F0}";
            }
            else
            {
                displayText = $"{actualDamageDealt:F0}";
            }

            SpawnDamagePopup(worldPos, displayText, textColor, isHeavy, isShielded);

            // Camera Shake
            float traumaToAdd = isHeavy ? heavyTrauma : lightTrauma;
            TriggerScreenShake(traumaToAdd);

            // Hitstop
            float hitstopDuration = isHeavy ? heavyHitstopDuration : lightHitstopDuration;
            TriggerHitstop(hitstopDuration);

            // Sound reinforcement
            if (isShielded)
            {
                AudioManager.Instance?.PlaySyntheticGuardCue();
            }
            else if (isHeavy)
            {
                AudioManager.Instance?.PlaySyntheticCritCue();
            }
        }

        /// <summary>
        /// Spawns a floating combat text popup at the specified world position.
        /// </summary>
        public void SpawnDamagePopup(Vector3 worldPos, string text, Color color, bool isCrit = false, bool isGuard = false)
        {
            if (worldCanvas == null) EnsureWorldCanvas();

            DamagePopup popup;
            if (popupPool.Count > 0)
            {
                popup = popupPool.Dequeue();
            }
            else
            {
                popup = CreateNewPopupInstance();
            }

            activePopups.Add(popup);
            popup.Initialize(text, color, worldPos, isCrit, isGuard, ReturnPopupToPool);
        }

        /// <summary>
        /// Adds trauma to CameraFollow2D for dynamic camera shake.
        /// </summary>
        public void TriggerScreenShake(float trauma)
        {
            if (CameraFollow2D.Instance != null)
            {
                CameraFollow2D.Instance.AddTrauma(trauma);
            }
        }

        /// <summary>
        /// Initiates a brief unscaled hitstop (frame freeze). Respects PauseMenu state.
        /// </summary>
        public void TriggerHitstop(float duration)
        {
            if (!Application.isPlaying) return;

            if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.IsPaused)
            {
                return; // Do not touch timescale if paused
            }

            if (activeHitstopRoutine != null)
            {
                StopCoroutine(activeHitstopRoutine);
            }

            activeHitstopRoutine = StartCoroutine(HitstopRoutine(duration));
        }

        private IEnumerator HitstopRoutine(float duration)
        {
            Time.timeScale = hitstopTimeScale;
            yield return new WaitForSecondsRealtime(duration);

            if (PauseMenuUI.Instance == null || !PauseMenuUI.Instance.IsPaused)
            {
                Time.timeScale = 1.0f;
            }

            activeHitstopRoutine = null;
        }

        #endregion
    }
}
