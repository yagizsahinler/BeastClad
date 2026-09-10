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
            transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
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
