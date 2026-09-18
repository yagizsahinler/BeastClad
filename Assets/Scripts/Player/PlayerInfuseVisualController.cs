using System.Collections.Generic;
using UnityEngine;
using BeastClad.Data;

namespace BeastClad.Player
{
    /// <summary>
    /// Controls the modular 2D paperdoll visual rendering for the player character.
    /// Manages overlay SpriteRenderers for the 5 anatomical infuse slots (Head, Chest,
    /// Left Arm, Right Arm, Legs) on the 'EquippedArmor' sorting layer, dynamically updating
    /// directional sprites (Front, Back, Side with FlipX) in response to movement and equip changes.
    /// </summary>
    [RequireComponent(typeof(PlayerInfuseManager))]
    [DisallowMultipleComponent]
    public class PlayerInfuseVisualController : MonoBehaviour
    {
        [Header("Chassis Reference")]
        [Tooltip("Root or base body SpriteRenderer on the 'Entities' sorting layer.")]
        [SerializeField] private SpriteRenderer baseChassisRenderer;

        [Header("Paperdoll Overlay Renderers ('EquippedArmor' Sorting Layer)")]
        [SerializeField] private SpriteRenderer headOverlayRenderer;
        [SerializeField] private SpriteRenderer chestOverlayRenderer;
        [SerializeField] private SpriteRenderer leftArmOverlayRenderer;
        [SerializeField] private SpriteRenderer rightArmOverlayRenderer;
        [SerializeField] private SpriteRenderer legsOverlayRenderer;

        [Header("Combat & VFX Socket Anchors")]
        [SerializeField] private Transform meleeSocket;
        [SerializeField] private Transform centerMassSocket;
        [SerializeField] private Transform groundSocket;

        // Active equipped ambient VFX instances
        private readonly Dictionary<EquipmentSlot, GameObject> activeEquippedVfx = new Dictionary<EquipmentSlot, GameObject>();

        // Component references
        private PlayerInfuseManager infuseManager;
        private PlayerController2D playerController;

        // Direction tracking
        private Vector2 lastFacingDirection = Vector2.down;

        public Transform MeleeSocket => meleeSocket != null ? meleeSocket : transform;
        public Transform CenterMassSocket => centerMassSocket != null ? centerMassSocket : transform;
        public Transform GroundSocket => groundSocket != null ? groundSocket : transform;

        private void Awake()
        {
            infuseManager = GetComponent<PlayerInfuseManager>();
            playerController = GetComponent<PlayerController2D>();

            if (baseChassisRenderer == null)
            {
                baseChassisRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureSortingLayers();
        }

        private void OnEnable()
        {
            if (infuseManager != null)
            {
                infuseManager.OnInfuseChanged += HandleInfuseChanged;
            }
        }

        private void OnDisable()
        {
            if (infuseManager != null)
            {
                infuseManager.OnInfuseChanged -= HandleInfuseChanged;
            }
        }

        private void Start()
        {
            RefreshAllOverlays();
        }

        private void LateUpdate()
        {
            if (playerController == null) return;

            Vector2 currentFacing = playerController.FacingDirection;
            if (currentFacing.sqrMagnitude < 0.01f) currentFacing = Vector2.down;

            // Check if facing changed noticeably
            if (Vector2.Angle(currentFacing, lastFacingDirection) > 10f)
            {
                lastFacingDirection = currentFacing;
                UpdateDirectionalOverlays(currentFacing);
            }
        }

        /// <summary>
        /// Ensures all paperdoll renderers are assigned to the 'EquippedArmor' sorting layer.
        /// </summary>
        public void EnsureSortingLayers()
        {
            const string armorLayer = "EquippedArmor";
            const string entityLayer = "Entities";

            if (baseChassisRenderer != null)
            {
                baseChassisRenderer.sortingLayerName = entityLayer;
                baseChassisRenderer.sortingOrder = 0;
            }

            ConfigureOverlayRenderer(headOverlayRenderer, armorLayer, 5);
            ConfigureOverlayRenderer(chestOverlayRenderer, armorLayer, 2);
            ConfigureOverlayRenderer(leftArmOverlayRenderer, armorLayer, 3);
            ConfigureOverlayRenderer(rightArmOverlayRenderer, armorLayer, 4);
            ConfigureOverlayRenderer(legsOverlayRenderer, armorLayer, 1);
        }

        private void ConfigureOverlayRenderer(SpriteRenderer sr, string layerName, int orderInLayer)
        {
            if (sr == null) return;
            sr.sortingLayerName = layerName;
            sr.sortingOrder = orderInLayer;
        }

        /// <summary>
        /// Handles monster module equip / unequip changes across the 5 slots.
        /// </summary>
        private void HandleInfuseChanged(EquipmentSlot slot, MonsterDataSO monster, InfusePartSO module)
        {
            var renderer = GetRendererForSlot(slot);
            if (renderer == null) return;

            // Clean up previous equipped ambient VFX
            if (activeEquippedVfx.TryGetValue(slot, out var existingVfx) && existingVfx != null)
            {
                Destroy(existingVfx);
                activeEquippedVfx.Remove(slot);
            }

            if (module == null || monster == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                return;
            }

            renderer.enabled = true;
            Vector2 facing = playerController != null ? playerController.FacingDirection : Vector2.down;
            ApplyDirectionalSpriteToSlot(slot, module, facing);

            // Optional ambient VFX hook
            if (module.equippedVfxPrefab != null)
            {
                var vfx = Instantiate(module.equippedVfxPrefab, renderer.transform);
                vfx.transform.localPosition = Vector3.zero;
                activeEquippedVfx[slot] = vfx;
            }

            // Provide visual feedback cue (subtle flash in element color)
            Color flashColor = monster.primaryElement != null ? monster.primaryElement.elementColor : Color.white;
            StartCoroutine(FlashSlotRoutine(renderer, flashColor));
        }

        private System.Collections.IEnumerator FlashSlotRoutine(SpriteRenderer renderer, Color flashColor)
        {
            if (renderer == null) yield break;
            Color originalColor = Color.white;
            renderer.color = flashColor;
            yield return new WaitForSeconds(0.12f);
            if (renderer != null)
            {
                renderer.color = originalColor;
            }
        }

        /// <summary>
        /// Updates the directional sprite representations and flips across all 5 slots.
        /// </summary>
        public void UpdateDirectionalOverlays(Vector2 facingDir)
        {
            if (infuseManager == null) return;

            UpdateSlotDirection(EquipmentSlot.Head, facingDir);
            UpdateSlotDirection(EquipmentSlot.Chest, facingDir);
            UpdateSlotDirection(EquipmentSlot.LeftArm, facingDir);
            UpdateSlotDirection(EquipmentSlot.RightArm, facingDir);
            UpdateSlotDirection(EquipmentSlot.Legs, facingDir);
        }

        private void UpdateSlotDirection(EquipmentSlot slot, Vector2 facingDir)
        {
            var module = infuseManager.GetEquippedModule(slot);
            if (module == null) return;

            ApplyDirectionalSpriteToSlot(slot, module, facingDir);
        }

        private void ApplyDirectionalSpriteToSlot(EquipmentSlot slot, InfusePartSO module, Vector2 facingDir)
        {
            var renderer = GetRendererForSlot(slot);
            if (renderer == null || module == null) return;

            Sprite targetSprite = module.GetOverlayForDirection(facingDir);
            renderer.sprite = targetSprite;

            // Flip horizontally if facing Left
            bool isFacingLeft = facingDir.x < -0.3f;
            renderer.flipX = isFacingLeft;
        }

        /// <summary>
        /// Fully refreshes all 5 paperdoll overlay renderers.
        /// </summary>
        public void RefreshAllOverlays()
        {
            if (infuseManager == null) return;

            Vector2 facing = playerController != null ? playerController.FacingDirection : Vector2.down;

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                var monster = infuseManager.GetEquippedMonster(slot);
                var module = infuseManager.GetEquippedModule(slot);
                HandleInfuseChanged(slot, monster, module);
            }

            UpdateDirectionalOverlays(facing);
        }

        public SpriteRenderer GetRendererForSlot(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Head => headOverlayRenderer,
                EquipmentSlot.Chest => chestOverlayRenderer,
                EquipmentSlot.LeftArm => leftArmOverlayRenderer,
                EquipmentSlot.RightArm => rightArmOverlayRenderer,
                EquipmentSlot.Legs => legsOverlayRenderer,
                _ => null
            };
        }

        public void AssignRenderers(
            SpriteRenderer chassis,
            SpriteRenderer head,
            SpriteRenderer chest,
            SpriteRenderer leftArm,
            SpriteRenderer rightArm,
            SpriteRenderer legs,
            Transform melee,
            Transform center,
            Transform ground)
        {
            baseChassisRenderer = chassis;
            headOverlayRenderer = head;
            chestOverlayRenderer = chest;
            leftArmOverlayRenderer = leftArm;
            rightArmOverlayRenderer = rightArm;
            legsOverlayRenderer = legs;
            meleeSocket = melee;
            centerMassSocket = center;
            groundSocket = ground;
            EnsureSortingLayers();
        }
    }
}
