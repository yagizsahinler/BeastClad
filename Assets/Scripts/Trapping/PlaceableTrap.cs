using System;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Trapping
{
    /// <summary>
    /// Field snare deployed on the ground.
    /// Detects wild monsters, snaps shut to immobilize them, and enables the Subdual/Capture interaction.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlaceableTrap : MonoBehaviour
    {
        [Header("Trap Configuration")]
        [SerializeField] private TrapDataSO trapData;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer trapRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite sprungSprite;

        private bool isSprung;
        private float immobilizeTimeRemaining;
        private WildMonsterAI2D snaredMonster;

        public bool IsSprung => isSprung;
        public WildMonsterAI2D SnaredMonster => snaredMonster;

        public event Action<WildMonsterAI2D> OnMonsterSnared;
        public event Action OnMonsterEscaped;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (trapRenderer == null)
            {
                trapRenderer = GetComponent<SpriteRenderer>();
            }

            SetVisualState(false);
        }

        private void Update()
        {
            if (isSprung && snaredMonster != null)
            {
                immobilizeTimeRemaining -= Time.deltaTime;
                if (immobilizeTimeRemaining <= 0f)
                {
                    // Monster broke free!
                    ReleaseMonster();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isSprung) return;

            if (other.TryGetComponent<WildMonsterAI2D>(out var monster))
            {
                SpringTrap(monster);
            }
        }

        public void SpringTrap(WildMonsterAI2D monster)
        {
            if (isSprung || monster == null) return;

            isSprung = true;
            snaredMonster = monster;
            immobilizeTimeRemaining = trapData != null ? trapData.immobilizeDuration : 10f;

            SetVisualState(true);
            monster.TrapInSnare(this, immobilizeTimeRemaining);

            Debug.Log($"<color=#FF7700>[Field Trap]</color> <b>SNAP!</b> {monster.SpeciesName} stepped into snare! Immobilized for {immobilizeTimeRemaining:F1}s.");

            OnMonsterSnared?.Invoke(monster);
        }

        public void ReleaseMonster()
        {
            if (!isSprung) return;

            Debug.Log($"<color=#FF4444>[Field Trap]</color> {snaredMonster?.SpeciesName} broke free from the snare!");

            if (snaredMonster != null)
            {
                snaredMonster.BreakFree();
                snaredMonster = null;
            }

            OnMonsterEscaped?.Invoke();
            Destroy(gameObject, 0.5f);
        }

        /// <summary>
        /// Successfully captures the snared monster into the player's collection.
        /// </summary>
        public MonsterInstance Capture()
        {
            if (!isSprung || snaredMonster == null) return null;

            var instance = snaredMonster.SubdueAndCapture();
            snaredMonster = null;

            Destroy(gameObject, 0.2f);
            return instance;
        }

        private void SetVisualState(bool sprung)
        {
            if (trapRenderer != null)
            {
                if (sprung)
                {
                    if (sprungSprite != null) trapRenderer.sprite = sprungSprite;
                    trapRenderer.color = new Color(0.9f, 0.35f, 0.2f, 1f); // Alert red-orange
                    transform.localScale = new Vector3(1.1f, 1.1f, 1f);
                }
                else
                {
                    if (openSprite != null) trapRenderer.sprite = openSprite;
                    trapRenderer.color = trapData != null ? trapData.trapColor : new Color(0.7f, 0.7f, 0.75f, 1f);
                    transform.localScale = Vector3.one;
                }
            }
        }

        public void Initialize(TrapDataSO data, Sprite open, Sprite sprung)
        {
            trapData = data;
            openSprite = open;
            sprungSprite = sprung;
            SetVisualState(false);
        }
    }
}
