using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BeastClad.Data;
using BeastClad.Player;

namespace BeastClad.UI
{
    /// <summary>
    /// UI Widget representing a single limb slot on the HUD.
    /// Uses Unity UI Image Radial 360 Fill to visually drain cooldowns.
    /// </summary>
    [Serializable]
    public class CooldownSlotWidget
    {
        public EquipmentSlot slot;
        public Image slotFrame;
        public Image iconImage;
        public Image radialCooldownImage; // Image Type = Filled, Fill Method = Radial 360
        public Image gcdOverlayImage;
        public Text slotLabelText;
        public Text timerText;
        public Text keybindText;
    }

    /// <summary>
    /// Manages the real-time HUD display for all 5 limb sockets.
    /// Enforces the mandatory Radial 360 Fill visual feedback for local cooldowns and GCD.
    /// </summary>
    [DisallowMultipleComponent]
    public class LimbCooldownUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CooldownController cooldownController;
        [SerializeField] private PlayerInfuseManager infuseManager;

        [Header("Slot Widgets")]
        [SerializeField] private List<CooldownSlotWidget> widgets = new List<CooldownSlotWidget>();

        [Header("Colors")]
        [SerializeField] private Color readyColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color cooldownColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
        [SerializeField] private Color gcdColor = new Color(1f, 1f, 1f, 0.25f);

        private readonly Dictionary<EquipmentSlot, CooldownSlotWidget> widgetMap = new Dictionary<EquipmentSlot, CooldownSlotWidget>();

        private void Awake()
        {
            if (cooldownController == null)
            {
                cooldownController = FindAnyObjectByType<CooldownController>();
            }

            if (infuseManager == null)
            {
                infuseManager = FindAnyObjectByType<PlayerInfuseManager>();
            }

            BuildWidgetMap();
            ConfigureRadialFillSettings();
        }

        private void OnEnable()
        {
            if (cooldownController != null)
            {
                cooldownController.OnCooldownUpdated += HandleCooldownUpdated;
                cooldownController.OnGCDUpdated += HandleGCDUpdated;
                cooldownController.OnSlotReady += HandleSlotReady;
            }

            if (infuseManager != null)
            {
                infuseManager.OnInfuseChanged += HandleInfuseChanged;
            }
        }

        private void OnDisable()
        {
            if (cooldownController != null)
            {
                cooldownController.OnCooldownUpdated -= HandleCooldownUpdated;
                cooldownController.OnGCDUpdated -= HandleGCDUpdated;
                cooldownController.OnSlotReady -= HandleSlotReady;
            }

            if (infuseManager != null)
            {
                infuseManager.OnInfuseChanged -= HandleInfuseChanged;
            }
        }

        private void Start()
        {
            RefreshAllSlots();
        }

        private void BuildWidgetMap()
        {
            widgetMap.Clear();
            foreach (var w in widgets)
            {
                if (w != null)
                {
                    widgetMap[w.slot] = w;
                }
            }
        }

        /// <summary>
        /// Ensures all radial cooldown images are configured with Image.Type.Filled and Radial 360.
        /// </summary>
        private void ConfigureRadialFillSettings()
        {
            foreach (var w in widgets)
            {
                if (w.radialCooldownImage != null)
                {
                    w.radialCooldownImage.type = Image.Type.Filled;
                    w.radialCooldownImage.fillMethod = Image.FillMethod.Radial360;
                    w.radialCooldownImage.fillOrigin = (int)Image.Origin360.Top;
                    w.radialCooldownImage.fillClockwise = false;
                    w.radialCooldownImage.fillAmount = 0f;
                }

                if (w.gcdOverlayImage != null)
                {
                    w.gcdOverlayImage.type = Image.Type.Filled;
                    w.gcdOverlayImage.fillMethod = Image.FillMethod.Radial360;
                    w.gcdOverlayImage.fillOrigin = (int)Image.Origin360.Top;
                    w.gcdOverlayImage.fillClockwise = true;
                    w.gcdOverlayImage.fillAmount = 0f;
                }
            }
        }

        private void HandleCooldownUpdated(EquipmentSlot slot, float normalizedRemaining)
        {
            if (widgetMap.TryGetValue(slot, out var widget))
            {
                if (widget.radialCooldownImage != null)
                {
                    widget.radialCooldownImage.fillAmount = normalizedRemaining;
                }

                if (widget.timerText != null && cooldownController != null)
                {
                    float remaining = cooldownController.GetRemainingTime(slot);
                    widget.timerText.text = remaining > 0.05f ? $"{remaining:F1}s" : "";
                }
            }
        }

        private void HandleGCDUpdated(float normalizedGCD)
        {
            foreach (var w in widgets)
            {
                if (w.gcdOverlayImage != null)
                {
                    w.gcdOverlayImage.fillAmount = normalizedGCD;
                }
            }
        }

        private void HandleSlotReady(EquipmentSlot slot)
        {
            if (widgetMap.TryGetValue(slot, out var widget))
            {
                if (widget.radialCooldownImage != null)
                {
                    widget.radialCooldownImage.fillAmount = 0f;
                }
                if (widget.timerText != null)
                {
                    widget.timerText.text = "";
                }
            }
        }

        private void HandleInfuseChanged(EquipmentSlot slot, MonsterDataSO monster, InfusePartSO part)
        {
            UpdateSlotDisplay(slot, monster, part);
        }

        public void RefreshAllSlots()
        {
            if (infuseManager == null) return;

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                var monster = infuseManager.GetEquippedMonster(slot);
                var module = infuseManager.GetEquippedModule(slot);
                UpdateSlotDisplay(slot, monster, module);
            }
        }

        private void UpdateSlotDisplay(EquipmentSlot slot, MonsterDataSO monster, InfusePartSO part)
        {
            if (!widgetMap.TryGetValue(slot, out var widget)) return;

            if (part != null)
            {
                if (widget.slotLabelText != null)
                {
                    string elem = part.activeSkill?.element != null ? $"[{part.activeSkill.element.elementId}]" : "";
                    widget.slotLabelText.text = $"{slot}\n{part.partName} {elem}";
                }

                if (widget.slotFrame != null && part.activeSkill?.element != null)
                {
                    widget.slotFrame.color = part.activeSkill.element.elementColor;
                }
            }
            else
            {
                if (widget.slotLabelText != null)
                {
                    widget.slotLabelText.text = $"{slot}\n(Empty)";
                }

                if (widget.slotFrame != null)
                {
                    widget.slotFrame.color = Color.gray;
                }
            }
        }
    }
}
