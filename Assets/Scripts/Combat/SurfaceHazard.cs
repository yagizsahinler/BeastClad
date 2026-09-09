using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Combat
{
    public enum HazardState
    {
        WaterPuddle,
        ElectrifiedShockZone
    }

    /// <summary>
    /// Represents a persistent 2D battlefield surface hazard entity on the arena floor.
    /// Demonstrates the core Terraforming & Chemistry mechanic:
    /// A Water Puddle struck by an Electric attack transmutes into an Electrified AoE Shock Zone.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SurfaceHazard : MonoBehaviour
    {
        [Header("Hazard Configuration")]
        [SerializeField] private HazardState state = HazardState.WaterPuddle;
        [SerializeField] private ElementalTypeSO hydroElement;
        [SerializeField] private ElementalTypeSO voltElement;
        [SerializeField] private float lifetime = 8.0f;
        [SerializeField] private float shockTickInterval = 0.4f;
        [SerializeField] private float shockTickDamage = 6.0f;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color waterColor = new Color(0.15f, 0.65f, 1.0f, 0.6f);
        [SerializeField] private Color electricColor = new Color(1.0f, 0.92f, 0.25f, 0.85f);

        private float timeRemaining;
        private float nextShockTick;
        private readonly List<Hurtbox2D> entitiesInPuddle = new List<Hurtbox2D>();

        public HazardState State => state;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            timeRemaining = lifetime;
            UpdateVisuals();
        }

        private void Update()
        {
            timeRemaining -= Time.deltaTime;

            // Electrified pulse effect & periodic damage ticks
            if (state == HazardState.ElectrifiedShockZone)
            {
                // Pulsate electric glow
                if (spriteRenderer != null)
                {
                    float pulse = 0.7f + 0.3f * Mathf.PingPong(Time.time * 8f, 1f);
                    spriteRenderer.color = new Color(electricColor.r, electricColor.g, electricColor.b, pulse);
                }

                if (Time.time >= nextShockTick)
                {
                    nextShockTick = Time.time + shockTickInterval;
                    ExecuteShockTick();
                }
            }

            if (timeRemaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Hurtbox2D>(out var hurtbox))
            {
                if (!entitiesInPuddle.Contains(hurtbox))
                {
                    entitiesInPuddle.Add(hurtbox);

                    // If already electrified, deliver immediate shock on entry
                    if (state == HazardState.ElectrifiedShockZone)
                    {
                        DeliverShockTo(hurtbox);
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<Hurtbox2D>(out var hurtbox))
            {
                entitiesInPuddle.Remove(hurtbox);
            }
        }

        /// <summary>
        /// Applies incoming elemental strike to the puddle, triggering chemical reactions.
        /// </summary>
        public void ApplyElementalStrike(DamagePayload strike)
        {
            if (strike.element == null) return;

            // Chemistry Reaction: Water + Volt = Electrified Shock Zone
            if (state == HazardState.WaterPuddle && strike.element.elementId == "Volt")
            {
                ElectrifyPuddle();
            }
            // Chemistry Reaction: Water + Pyro = Steam Quench
            else if (strike.element.elementId == "Pyro")
            {
                Debug.Log("<color=#FF5500>[Terraforming Chemistry]</color> Pyro strike quenched by Water Puddle! (Extinguished)");
                timeRemaining = Mathf.Min(timeRemaining, 1.0f); // Fast evaporate
            }
        }

        /// <summary>
        /// Transmutes this water puddle into a dangerous Electrified AoE Shock Zone.
        /// </summary>
        public void ElectrifyPuddle()
        {
            state = HazardState.ElectrifiedShockZone;
            timeRemaining = Mathf.Max(timeRemaining, 5.0f); // Extend duration on reaction
            UpdateVisuals();

            Debug.Log("<color=#FFD700>[Terraforming Reaction]</color> <b>ELECTRO-CONDUCTIVE DISCHARGE!</b> Water Puddle electrified into an <b>AoE Shock Zone</b>!");

            // Immediate initial shock burst to all targets inside
            ExecuteShockTick();
        }

        private void ExecuteShockTick()
        {
            // Clean nulls from list
            entitiesInPuddle.RemoveAll(h => h == null);

            foreach (var target in entitiesInPuddle)
            {
                DeliverShockTo(target);
            }
        }

        private void DeliverShockTo(Hurtbox2D target)
        {
            if (target == null) return;

            var payload = new DamagePayload(
                shockTickDamage,
                voltElement,
                Vector2.zero,
                0f,
                gameObject
            );

            target.ReceiveHit(payload);
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = state == HazardState.ElectrifiedShockZone ? electricColor : waterColor;
            }
        }

        public void Initialize(ElementalTypeSO hydro, ElementalTypeSO volt, float duration = 8f)
        {
            hydroElement = hydro;
            voltElement = volt;
            lifetime = duration;
            timeRemaining = duration;
            UpdateVisuals();
        }
    }
}
