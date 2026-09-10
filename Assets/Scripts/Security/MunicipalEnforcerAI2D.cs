using System;
using UnityEngine;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.Security
{
    public enum EnforcerState
    {
        Guarding,
        Alerted,
        Pacified
    }

    /// <summary>
    /// AI Controller for municipal security officers stationed at checkpoints.
    /// Monitors scanner reports, displays alert feedback, enforces civil codes,
    /// and escalates player Heat Level upon detecting unregistered or contraband specimens.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class MunicipalEnforcerAI2D : MonoBehaviour
    {
        [Header("Enforcer Identity")]
        [SerializeField] private string enforcerName = "Officer Vance";
        [SerializeField] private string unitBadge = "Aegis-Civil-Unit 409";

        [Header("Movement & Positioning")]
        [SerializeField] private float patrolSpeed = 2.5f;
        [SerializeField] private float alertRunSpeed = 4.2f;
        [SerializeField] private float confrontDistance = 2.0f;
        [SerializeField] private Vector3 postAnchorPosition;

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer enforcerRenderer;
        [SerializeField] private GameObject alertBadgeObject;
        [SerializeField] private SpriteRenderer alertBadgeRenderer;

        private Rigidbody2D rb;
        private Transform playerTarget;
        private EnforcerState currentState = EnforcerState.Guarding;
        private int currentHeatLevel = 0;
        private string activeSpeechText = "";

        public string EnforcerName => enforcerName;
        public string UnitBadge => unitBadge;
        public EnforcerState CurrentState => currentState;
        public int CurrentHeatLevel => currentHeatLevel;
        public string ActiveSpeechText => activeSpeechText;

        public event Action<EnforcerState> OnStateChanged;
        public event Action<int> OnHeatChanged;
        public event Action<string> OnDialogueSpoken;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            postAnchorPosition = transform.position;

            if (alertBadgeObject != null)
            {
                alertBadgeObject.SetActive(false);
            }
        }

        private void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                var p = FindAnyObjectByType<PlayerController2D>();
                if (p != null) playerTarget = p.transform;
            }

            switch (currentState)
            {
                case EnforcerState.Guarding:
                    ReturnToPost();
                    break;

                case EnforcerState.Alerted:
                    ConfrontPlayer();
                    break;

                case EnforcerState.Pacified:
                    ReturnToPost();
                    break;
            }
        }

        private void ConfrontPlayer()
        {
            if (playerTarget == null) return;

            Vector2 toPlayer = (playerTarget.position - transform.position);
            float dist = toPlayer.magnitude;

            // Flip facing
            if (enforcerRenderer != null)
            {
                enforcerRenderer.flipX = toPlayer.x < 0f;
            }

            if (dist > confrontDistance)
            {
                Vector2 dir = toPlayer.normalized;
                rb.linearVelocity = dir * alertRunSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void ReturnToPost()
        {
            Vector2 toPost = (postAnchorPosition - transform.position);
            if (toPost.magnitude > 0.3f)
            {
                rb.linearVelocity = toPost.normalized * patrolSpeed;
                if (enforcerRenderer != null)
                {
                    enforcerRenderer.flipX = toPost.x < 0f;
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                if (currentState == EnforcerState.Pacified)
                {
                    SetState(EnforcerState.Guarding);
                }
            }
        }

        public void HandleScannerCleared(MunicipalScannerZone scanner, ScanReport report)
        {
            if (currentState == EnforcerState.Alerted)
            {
                Speak("Biometrics updated to legal. Standing down, citizen.");
                SetState(EnforcerState.Pacified);
                return;
            }

            Speak("Clear biometric scan. Pass through, citizen.");
        }

        public void HandleScannerViolation(MunicipalScannerZone scanner, ScanReport report)
        {
            SetState(EnforcerState.Alerted);

            if (report.contrabandCount > 0)
            {
                currentHeatLevel = 2;
                Speak($"CODE 14-B CONTRABAND VIOLATION! Drop your illegal gear, citizen!");
                if (alertBadgeRenderer != null) alertBadgeRenderer.color = Color.red;
            }
            else
            {
                currentHeatLevel = 1;
                Speak($"Halt, citizen! Unregistered wild biological gear detected in a civil sector!");
                if (alertBadgeRenderer != null) alertBadgeRenderer.color = new Color(1f, 0.6f, 0f, 1f);
            }

            OnHeatChanged?.Invoke(currentHeatLevel);
        }

        private void SetState(EnforcerState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);

            if (alertBadgeObject != null)
            {
                alertBadgeObject.SetActive(currentState == EnforcerState.Alerted);
            }
        }

        public void Speak(string message)
        {
            activeSpeechText = message;
            Debug.Log($"<color=#38BDF8>[Enforcer {enforcerName}]</color> \"{message}\" (Heat: <b>{currentHeatLevel}</b>)");
            OnDialogueSpoken?.Invoke(message);
        }

        public void SetReferences(SpriteRenderer bodyRenderer, GameObject alertBadge, SpriteRenderer badgeRenderer, Vector3 anchorPos)
        {
            enforcerRenderer = bodyRenderer;
            alertBadgeObject = alertBadge;
            alertBadgeRenderer = badgeRenderer;
            postAnchorPosition = anchorPos;
        }
    }
}
