using UnityEngine;
using BeastClad.Player;

namespace BeastClad.World
{
    /// <summary>
    /// Smooth top-down 2D camera follow with customizable bounding limits.
    /// Provides cinematic centering and navigation clarity across hubs and arenas.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 6.0f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Bounding Box (Optional)")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-6f, -3f);
        [SerializeField] private Vector2 maxBounds = new Vector2(6f, 3f);

        [Header("Camera Shake / Trauma")]
        [SerializeField] private float maxShakeOffset = 0.45f;
        [SerializeField] private float traumaDecay = 1.75f;
        [SerializeField] private float shakeFrequency = 28f;

        public static CameraFollow2D Instance { get; private set; }

        private float trauma = 0f;
        private float noiseSeedX;
        private float noiseSeedY;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            noiseSeedX = Random.Range(0f, 1000f);
            noiseSeedY = Random.Range(1000f, 2000f);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (target == null)
            {
                var pc = FindAnyObjectByType<PlayerController2D>();
                if (pc != null) target = pc.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                var pc = FindAnyObjectByType<PlayerController2D>();
                if (pc != null) target = pc.transform;
                if (target == null) return;
            }

            Vector3 desiredPos = target.position + offset;

            if (useBounds)
            {
                desiredPos.x = Mathf.Clamp(desiredPos.x, minBounds.x, maxBounds.x);
                desiredPos.y = Mathf.Clamp(desiredPos.y, minBounds.y, maxBounds.y);
            }

            desiredPos.z = offset.z;
            Vector3 basePos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

            // Compute Trauma Shake
            if (trauma > 0f)
            {
                float shake = trauma * trauma; // Non-linear response
                float time = Time.unscaledTime * shakeFrequency;
                float offsetX = (Mathf.PerlinNoise(noiseSeedX + time, 0f) * 2f - 1f) * maxShakeOffset * shake;
                float offsetY = (Mathf.PerlinNoise(0f, noiseSeedY + time) * 2f - 1f) * maxShakeOffset * shake;

                basePos.x += offsetX;
                basePos.y += offsetY;

                trauma = Mathf.Max(0f, trauma - traumaDecay * Time.unscaledDeltaTime);
            }

            transform.position = basePos;
        }

        /// <summary>
        /// Adds camera trauma (0.0 to 1.0). Trauma decays over time and causes screen shake.
        /// </summary>
        public void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }
    }
}
