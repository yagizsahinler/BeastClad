using System.Collections;
using UnityEngine;

namespace BeastClad.Combat
{
    /// <summary>
    /// Interactive target dummy placed in the test arena to receive elemental attacks,
    /// display damage feedback, and verify terraforming shock reactions.
    /// </summary>
    [RequireComponent(typeof(Hurtbox2D))]
    public class TargetDummy2D : MonoBehaviour
    {
        [Header("Feedback Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = new Color(0.85f, 0.65f, 0.45f, 1f); // Wood/leather tone
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private Color shockColor = new Color(1f, 1f, 0.2f, 1f);

        private Hurtbox2D hurtbox;
        private Rigidbody2D rb;
        private Coroutine flashRoutine;

        private void Awake()
        {
            hurtbox = GetComponent<Hurtbox2D>();
            rb = GetComponent<Rigidbody2D>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        private void OnEnable()
        {
            if (hurtbox != null)
            {
                hurtbox.OnHitReceived += HandleHitReceived;
            }
        }

        private void OnDisable()
        {
            if (hurtbox != null)
            {
                hurtbox.OnHitReceived -= HandleHitReceived;
            }
        }

        private void HandleHitReceived(DamagePayload payload)
        {
            string elemName = payload.element != null ? payload.element.displayName : "Physical";
            string colorHex = payload.element != null ? ColorUtility.ToHtmlStringRGB(payload.element.elementColor) : "FFFFFF";

            Debug.Log($"<color=#{colorHex}>[Combat Dummy]</color> Took <b>{payload.rawDamage:F1}</b> {elemName} damage! (Source: {payload.source?.name})");

            // Flash effect
            Color flashCol = payload.element != null && payload.element.elementId == "Volt" ? shockColor : hitFlashColor;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashColorRoutine(flashCol));

            // Knockback
            if (rb != null && payload.knockbackForce > 0.05f)
            {
                rb.AddForce(payload.knockbackDirection * payload.knockbackForce, ForceMode2D.Impulse);
            }
        }

        private IEnumerator FlashColorRoutine(Color flashCol)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flashCol;
                yield return new WaitForSeconds(0.12f);
                spriteRenderer.color = normalColor;
            }
        }
    }
}
