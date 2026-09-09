using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastClad.Data;

namespace BeastClad.Trapping
{
    /// <summary>
    /// Attached to the player to handle field trapping mechanics:
    /// Deploying pheromone lures [1], setting wire snares [2],
    /// and executing the Subdual/Capture interaction [E] on snared beasts.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerTrapperComponent : MonoBehaviour
    {
        [Header("Trap & Lure Data")]
        [SerializeField] private LureDataSO defaultLure;
        [SerializeField] private TrapDataSO defaultTrap;

        [Header("Visual Placeholders")]
        [SerializeField] private Sprite baitSprite;
        [SerializeField] private Sprite circleAuraSprite;
        [SerializeField] private Sprite trapOpenSprite;
        [SerializeField] private Sprite trapSprungSprite;

        [Header("Interaction Settings")]
        [SerializeField] private float captureInteractionRadius = 2.5f;

        // Captured inventory (pure C# runtime instances)
        public readonly List<MonsterInstance> CapturedMonsters = new List<MonsterInstance>();

        public event Action<MonsterInstance> OnMonsterCaptured;
        public event Action<string> OnFieldPromptChanged;

        private void Update()
        {
            HandleInput();
            CheckForNearbySnaredMonsters();
        }

        private void HandleInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // Deploy Lure [1]
            if (kb.digit1Key.wasPressedThisFrame)
            {
                DeployLure();
            }

            // Deploy Trap [2]
            if (kb.digit2Key.wasPressedThisFrame)
            {
                DeployTrap();
            }

            // Subdue / Capture [E]
            if (kb.eKey.wasPressedThisFrame)
            {
                TryCaptureNearbyMonster();
            }
        }

        public void DeployLure()
        {
            if (defaultLure == null)
            {
                Debug.LogWarning("[Trapper] No LureDataSO assigned to player!");
                return;
            }

            var lureObj = new GameObject("DeployedLure_" + defaultLure.lureName);
            lureObj.transform.position = transform.position;

            // Bait visual (center)
            var baitObj = new GameObject("BaitVisual");
            baitObj.transform.SetParent(lureObj.transform, false);
            var baitSr = baitObj.AddComponent<SpriteRenderer>();
            baitSr.sprite = baitSprite;
            baitSr.color = new Color(0.9f, 0.75f, 0.2f, 1f); // Golden bait
            baitSr.sortingOrder = 1;
            baitObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            // Scent aura visual (expanding circle)
            var auraObj = new GameObject("ScentAuraVisual");
            auraObj.transform.SetParent(lureObj.transform, false);
            var auraSr = auraObj.AddComponent<SpriteRenderer>();
            auraSr.sprite = circleAuraSprite;
            auraSr.color = defaultLure.scentAuraColor;
            auraSr.sortingOrder = 0;

            var lure = lureObj.AddComponent<PlaceableLure>();
            lure.Initialize(defaultLure, baitSprite, circleAuraSprite);

            Debug.Log($"<color=#77DD77>[Trapper]</color> Deployed <b>{defaultLure.lureName}</b> at {transform.position:F1}! Emitting scent radius: {defaultLure.attractionRadius}m.");
        }

        public void DeployTrap()
        {
            if (defaultTrap == null)
            {
                Debug.LogWarning("[Trapper] No TrapDataSO assigned to player!");
                return;
            }

            var trapObj = new GameObject("DeployedTrap_" + defaultTrap.trapName);
            trapObj.transform.position = transform.position;
            trapObj.layer = LayerMask.NameToLayer("Traps");

            var sr = trapObj.AddComponent<SpriteRenderer>();
            sr.sprite = trapOpenSprite;
            sr.color = defaultTrap.trapColor;
            sr.sortingOrder = 1;
            trapObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            var col = trapObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = defaultTrap.triggerRadius * 0.5f;

            var trap = trapObj.AddComponent<PlaceableTrap>();
            trap.Initialize(defaultTrap, trapOpenSprite, trapSprungSprite);

            Debug.Log($"<color=#FF9933>[Trapper]</color> Deployed <b>{defaultTrap.trapName}</b> at {transform.position:F1}. Snare is armed!");
        }

        public bool TryCaptureNearbyMonster()
        {
            var traps = FindObjectsByType<PlaceableTrap>(FindObjectsInactive.Exclude);
            foreach (var trap in traps)
            {
                if (trap != null && trap.IsSprung && trap.SnaredMonster != null)
                {
                    float dist = Vector2.Distance(transform.position, trap.transform.position);
                    if (dist <= captureInteractionRadius)
                    {
                        var captured = trap.Capture();
                        if (captured != null)
                        {
                            CapturedMonsters.Add(captured);
                            OnMonsterCaptured?.Invoke(captured);
                            Debug.Log($"<color=#00FFCC>[Trapper Vault]</color> Total specimens in transport crate: <b>{CapturedMonsters.Count}</b>.");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CheckForNearbySnaredMonsters()
        {
            bool snaredNearby = false;
            var traps = FindObjectsByType<PlaceableTrap>(FindObjectsInactive.Exclude);
            foreach (var trap in traps)
            {
                if (trap != null && trap.IsSprung && trap.SnaredMonster != null)
                {
                    float dist = Vector2.Distance(transform.position, trap.transform.position);
                    if (dist <= captureInteractionRadius)
                    {
                        snaredNearby = true;
                        OnFieldPromptChanged?.Invoke($"[ E ] Subdue & Capture {trap.SnaredMonster.SpeciesName}!");
                        return;
                    }
                }
            }

            if (!snaredNearby)
            {
                OnFieldPromptChanged?.Invoke("[1] Deploy Lure  |  [2] Deploy Snare");
            }
        }

        public void SetReferences(LureDataSO lure, TrapDataSO trap, Sprite bait, Sprite aura, Sprite trapOpen, Sprite trapSprung)
        {
            defaultLure = lure;
            defaultTrap = trap;
            baitSprite = bait;
            circleAuraSprite = aura;
            trapOpenSprite = trapOpen;
            trapSprungSprite = trapSprung;
        }
    }
}
