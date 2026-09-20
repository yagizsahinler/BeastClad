using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeastClad.Combat
{
    /// <summary>
    /// Lightweight floating damage and combat status popup rendered on a World-Space Canvas.
    /// Provides vibrant elemental color grading, scale-punch pop animations, upward drift,
    /// and alpha fading. Fully recyclable via CombatFeedbackManager object pool.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DamagePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text popupText;
        [SerializeField] private Outline textOutline;
        [SerializeField] private Shadow textShadow;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float lifetime = 0.65f;
        [SerializeField] private float upwardSpeed = 1.6f;
        [SerializeField] private float punchScale = 1.35f;

        private float elapsed;
        private Vector3 startPosition;
        private float horizontalVelocity;
        private Action<DamagePopup> returnToPoolCallback;
        private bool isActive = false;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (popupText == null) popupText = GetComponentInChildren<Text>();

            // Ensure crisp outlines for readability against any background
            if (textOutline == null && popupText != null)
            {
                textOutline = popupText.gameObject.AddComponent<Outline>();
                textOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                textOutline.effectDistance = new Vector2(1.2f, -1.2f);
            }
            if (textShadow == null && popupText != null)
            {
                textShadow = popupText.gameObject.AddComponent<Shadow>();
                textShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
                textShadow.effectDistance = new Vector2(2f, -2f);
            }
        }

        public void Initialize(string text, Color textColor, Vector3 worldPosition, bool isCrit, bool isGuard, Action<DamagePopup> returnCallback)
        {
            returnToPoolCallback = returnCallback;
            startPosition = worldPosition + new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(0.2f, 0.5f), 0f);
            transform.position = startPosition;

            horizontalVelocity = UnityEngine.Random.Range(-0.4f, 0.4f);
            elapsed = 0f;
            isActive = true;
            gameObject.SetActive(true);

            if (popupText != null)
            {
                popupText.text = text;
                popupText.color = textColor;
                popupText.fontSize = isCrit ? 26 : (isGuard ? 20 : 22);
                popupText.fontStyle = isCrit ? FontStyle.Bold : FontStyle.Normal;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            float initialScale = isCrit ? (punchScale * 1.25f) : punchScale;
            transform.localScale = Vector3.one * initialScale;
        }

        private void Update()
        {
            if (!isActive) return;

            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            // Scale punch settle
            float scaleDuration = 0.12f;
            if (elapsed < scaleDuration)
            {
                float t = elapsed / scaleDuration;
                float currentScale = Mathf.Lerp(punchScale, 1.0f, t);
                transform.localScale = Vector3.one * currentScale;
            }
            else
            {
                transform.localScale = Vector3.one;
            }

            // Upward drift and slight lateral drift
            Vector3 pos = transform.position;
            pos.y += upwardSpeed * dt;
            pos.x += horizontalVelocity * dt;
            transform.position = pos;

            // Fade out in last 40% of lifetime
            float fadeStart = lifetime * 0.6f;
            if (elapsed >= fadeStart)
            {
                float fadeT = (elapsed - fadeStart) / (lifetime - fadeStart);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Clamp01(1f - fadeT);
                }
            }

            if (elapsed >= lifetime)
            {
                Recycle();
            }
        }

        private void Recycle()
        {
            isActive = false;
            gameObject.SetActive(false);
            returnToPoolCallback?.Invoke(this);
        }
    }
}
