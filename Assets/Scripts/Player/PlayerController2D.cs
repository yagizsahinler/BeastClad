using System;
using UnityEngine;
using BeastClad.Input;

namespace BeastClad.Player
{
    /// <summary>
    /// Core 2D Top-Down Player Controller for BeastClad.
    /// Manages 8-directional physics-based movement and prototype Dash with local cooldown.
    /// Modularly designed to integrate with the Legs Infuse module in later phases.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Input Reference")]
        [Tooltip("Optional reference to the PlayerInputReader. If omitted, auto-fetches from this GameObject.")]
        [SerializeField] private PlayerInputReader inputReader;

        [Header("Movement Settings")]
        [Tooltip("Base movement speed in units per second.")]
        [SerializeField] private float moveSpeed = 6.5f;

        [Tooltip("Acceleration rate when speeding up or changing directions.")]
        [SerializeField] private float acceleration = 65f;

        [Tooltip("Deceleration rate when stopping.")]
        [SerializeField] private float deceleration = 75f;

        [Header("Dash Settings (Legs Infuse Prototype)")]
        [Tooltip("Velocity burst speed during dash.")]
        [SerializeField] private float dashSpeed = 20f;

        [Tooltip("Duration of the active dash in seconds.")]
        [SerializeField] private float dashDuration = 0.18f;

        [Tooltip("Local cooldown duration before dash can be used again.")]
        [SerializeField] private float dashCooldown = 2.0f;

        [Header("Visual Feedback (Optional)")]
        [Tooltip("SpriteRenderer for directional flipping. If null, auto-searches child or this object.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Internal State
        private Rigidbody2D rb;
        private Vector2 currentVelocity;
        private Vector2 dashDirection;
        private float dashTimeRemaining;
        private bool wasCooldownActive;
        private bool isMovementLocked = false;

        // Public Properties for HUD / Combat Systems
        public bool IsMovementLocked => isMovementLocked;
        public bool IsDashing { get; private set; }
        public Vector2 FacingDirection { get; private set; } = Vector2.down;
        public float DashCooldownRemaining { get; private set; }
        public float DashCooldownDuration => dashCooldown;

        /// <summary>
        /// Normalized cooldown value from 1.0 (cooling down) down to 0.0 (ready).
        /// Directly feeds Unity UI Image Radial 360 Fill.
        /// </summary>
        public float DashCooldownNormalized => dashCooldown > 0f ? Mathf.Clamp01(DashCooldownRemaining / dashCooldown) : 0f;

        // Events
        public event Action<Vector2> OnDashStarted;
        public event Action OnDashEnded;
        public event Action OnDashReady;

        private void Awake()
        {
            Application.runInBackground = true;

            rb = GetComponent<Rigidbody2D>();
            ConfigureRigidbody();

            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnDashTriggered += TryPerformDash;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnDashTriggered -= TryPerformDash;
            }

            // Halt physics on disable
            SetVelocity(Vector2.zero);
            IsDashing = false;
        }

        private void Update()
        {
            UpdateCooldowns();
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            if (isMovementLocked)
            {
                SetVelocity(Vector2.zero);
                currentVelocity = Vector2.zero;
                return;
            }

            if (IsDashing)
            {
                ExecuteDashPhysics();
            }
            else
            {
                ExecuteStandardMovementPhysics();
            }
        }

        /// <summary>
        /// Ensures Rigidbody2D is properly configured for 2D top-down sandbox physics.
        /// </summary>
        private void ConfigureRigidbody()
        {
            rb.gravityScale = 0f; // Critical: No falling in top-down RPGs
            rb.freezeRotation = true; // Constraints: Prevent spinning on collisions
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth visuals
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Prevent clipping through walls during dashes
        }

        /// <summary>
        /// Decrements cooldown timers and fires readiness events.
        /// </summary>
        private void UpdateCooldowns()
        {
            if (DashCooldownRemaining > 0f)
            {
                DashCooldownRemaining -= Time.deltaTime;
                wasCooldownActive = true;

                if (DashCooldownRemaining <= 0f)
                {
                    DashCooldownRemaining = 0f;
                    if (wasCooldownActive)
                    {
                        wasCooldownActive = false;
                        OnDashReady?.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// Tracks the last non-zero movement direction to orient dashes and attacks when standing still.
        /// </summary>
        private void UpdateFacingDirection()
        {
            if (IsDashing || inputReader == null) return;

            Vector2 input = inputReader.MoveInput;
            if (input.sqrMagnitude > 0.01f)
            {
                FacingDirection = input.normalized;

                // Simple placeholder visual flip for horizontal movement
                if (spriteRenderer != null && Mathf.Abs(FacingDirection.x) > 0.05f)
                {
                    spriteRenderer.flipX = FacingDirection.x < 0f;
                }
            }
        }

        public void SetFacingDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude > 0.01f)
            {
                FacingDirection = dir.normalized;
                if (spriteRenderer != null && Mathf.Abs(FacingDirection.x) > 0.05f)
                {
                    spriteRenderer.flipX = FacingDirection.x < 0f;
                }
            }
        }

        /// <summary>
        /// Attempts to trigger a dash if not currently dashing and cooldown is elapsed.
        /// </summary>
        public void TryPerformDash()
        {
            if (IsDashing || DashCooldownRemaining > 0f) return;

            Vector2 input = inputReader != null ? inputReader.MoveInput : Vector2.zero;
            dashDirection = input.sqrMagnitude > 0.01f ? input.normalized : FacingDirection;

            if (dashDirection.sqrMagnitude < 0.01f)
            {
                dashDirection = Vector2.down; // Safe fallback
            }

            IsDashing = true;
            dashTimeRemaining = dashDuration;
            DashCooldownRemaining = dashCooldown;

            OnDashStarted?.Invoke(dashDirection);
        }

        /// <summary>
        /// Applies fixed-velocity propulsion during active dash frames.
        /// </summary>
        private void ExecuteDashPhysics()
        {
            dashTimeRemaining -= Time.fixedDeltaTime;

            SetVelocity(dashDirection * dashSpeed);

            if (dashTimeRemaining <= 0f)
            {
                IsDashing = false;
                OnDashEnded?.Invoke();
            }
        }

        /// <summary>
        /// Applies responsive acceleration/deceleration towards target move input.
        /// </summary>
        private void ExecuteStandardMovementPhysics()
        {
            Vector2 rawInput = inputReader != null ? inputReader.MoveInput : Vector2.zero;

            // Clamp diagonal input magnitude to 1.0 to prevent faster diagonal speeds (8-way movement fix)
            Vector2 targetInput = Vector2.ClampMagnitude(rawInput, 1f);
            Vector2 targetVelocity = targetInput * moveSpeed;

            float currentSpeed = currentVelocity.magnitude;
            float rate = targetInput.sqrMagnitude > 0.01f ? acceleration : deceleration;

            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);
            SetVelocity(currentVelocity);
        }

        /// <summary>
        /// Safe cross-version velocity setter (handles Unity 6 linearVelocity vs legacy velocity).
        /// </summary>
        private void SetVelocity(Vector2 vel)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = vel;
#else
            rb.velocity = vel;
#endif
        }

        #region Public Modifiers for Future Infuse Modules

        /// <summary>
        /// Allows the Infuse system (e.g. Legs slot) to dynamically modify movement speed.
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0.5f, speed);
        }

        /// <summary>
        /// Allows the Infuse system (e.g. Legs slot) to override dash parameters.
        /// </summary>
        public void SetDashParameters(float speed, float duration, float cooldown)
        {
            dashSpeed = Mathf.Max(1f, speed);
            dashDuration = Mathf.Max(0.05f, duration);
            dashCooldown = Mathf.Max(0.1f, cooldown);
        }

        public void SetMovementLocked(bool locked)
        {
            isMovementLocked = locked;
            if (locked)
            {
                SetVelocity(Vector2.zero);
                currentVelocity = Vector2.zero;
            }
        }

        #endregion
    }
}
