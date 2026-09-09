using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Trapping
{
    /// <summary>
    /// Deployed pheromone lure beacon in the wilderness.
    /// Emits a scent aura that attracts matching wild monsters via steering.
    /// </summary>
    public class PlaceableLure : MonoBehaviour
    {
        public static readonly List<PlaceableLure> ActiveLures = new List<PlaceableLure>();

        [Header("Lure Data")]
        [SerializeField] private LureDataSO lureData;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer baitRenderer;
        [SerializeField] private SpriteRenderer scentAuraRenderer;

        private float remainingTime;

        public LureDataSO Data => lureData;
        public float AttractionRadius => lureData != null ? lureData.attractionRadius : 10f;
        public bool IsActive => remainingTime > 0f;

        private void OnEnable()
        {
            if (!ActiveLures.Contains(this))
            {
                ActiveLures.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveLures.Remove(this);
        }

        private void Awake()
        {
            if (lureData != null)
            {
                remainingTime = lureData.duration;
            }
            else
            {
                remainingTime = 15f;
            }

            UpdateVisuals();
        }

        private void Update()
        {
            remainingTime -= Time.deltaTime;

            // Pulsate scent aura
            if (scentAuraRenderer != null)
            {
                float pulse = 0.25f + 0.15f * Mathf.PingPong(Time.time * 2.5f, 1f);
                Color c = lureData != null ? lureData.scentAuraColor : new Color(0.2f, 0.9f, 0.4f, 0.35f);
                scentAuraRenderer.color = new Color(c.r, c.g, c.b, pulse);
            }

            if (remainingTime <= 0f)
            {
                Debug.Log($"<color=#77DD77>[Lure]</color> <b>{gameObject.name}</b> scent has dissipated.");
                Destroy(gameObject);
            }
        }

        public void Initialize(LureDataSO data, Sprite baitSprite, Sprite auraSprite)
        {
            lureData = data;
            remainingTime = data != null ? data.duration : 15f;

            if (baitRenderer != null && baitSprite != null)
            {
                baitRenderer.sprite = baitSprite;
            }

            if (scentAuraRenderer != null && auraSprite != null)
            {
                scentAuraRenderer.sprite = auraSprite;
                float diameter = AttractionRadius * 2f;
                scentAuraRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (lureData == null) return;

            if (scentAuraRenderer != null)
            {
                scentAuraRenderer.color = lureData.scentAuraColor;
                float diameter = AttractionRadius * 2f;
                scentAuraRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }
    }
}
