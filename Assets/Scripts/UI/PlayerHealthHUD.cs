using System;
using UnityEngine;
using UnityEngine.UI;
using BeastClad.Player;
using BeastClad.Arena;
using BeastClad.Underground;

namespace BeastClad.UI
{
    /// <summary>
    /// Dynamic character health bar HUD.
    /// Remains hidden during peaceful exploration to maintain a clean screen,
    /// and smoothly fades in whenever the player enters combat, takes damage,
    /// executes combat abilities, or engages hostile opponents.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHealthHUD : MonoBehaviour
    {
        [Header("Target References")]
        [SerializeField] private PlayerStatsComponent playerStats;
        [SerializeField] private PlayerInfuseManager infuseManager;

        [Header("UI Visual Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Image healthLagImage;
        [SerializeField] private Text healthNumberText;
        [SerializeField] private Text titleLabelText;

        [Header("Combat Dynamics")]
        [Tooltip("Seconds the health bar remains fully visible after the last combat event.")]
        [SerializeField] private float combatLingerDuration = 5.0f;
        [SerializeField] private float fadeInSpeed = 7.0f;
        [SerializeField] private float fadeOutSpeed = 2.0f;
        [SerializeField] private float lagCatchupSpeed = 0.8f;

        [Header("Health Bar Color Tiers")]
        [SerializeField] private Color healthyColor = new Color(0.06f, 0.72f, 0.50f, 1f);   // Emerald (#10B981)
        [SerializeField] private Color cautionColor = new Color(0.96f, 0.62f, 0.04f, 1f);   // Amber (#F59E0B)
        [SerializeField] private Color criticalColor = new Color(0.94f, 0.27f, 0.27f, 1f);  // Crimson (#EF4444)
        [SerializeField] private Color lagBarColor = new Color(1f, 0.85f, 0.4f, 0.85f);     // Trailing Amber

        private float combatTimer = 0f;
        private float previousHealth = -1f;
        private float targetFill = 1f;
        private float lagFill = 1f;
        private bool forceVisibleInArena = false;

        public bool IsInCombat => combatTimer > 0f || forceVisibleInArena;
        public float Alpha => canvasGroup != null ? canvasGroup.alpha : 0f;
        public float TargetFill => targetFill;

        private void Awake()
        {
            EnsureDependencies();
            EnsureUIHierarchy();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void OnEnable()
        {
            BindEvents();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void Start()
        {
            EnsureDependencies();
            if (playerStats != null)
            {
                previousHealth = playerStats.CurrentHP;
                UpdateHealthValues(playerStats.CurrentHP, playerStats.MaxHP);
            }
        }

        public void InitializeHUD()
        {
            EnsureDependencies();
            EnsureUIHierarchy();
            BindEvents();
            if (playerStats != null)
            {
                previousHealth = playerStats.CurrentHP;
                UpdateHealthValues(playerStats.CurrentHP, playerStats.MaxHP);
            }
        }

        private void Update()
        {
            ManualUpdate(Time.deltaTime);
        }

        public void ManualUpdate(float dt)
        {
            // Fallback acquisition if player loaded late
            if (playerStats == null)
            {
                EnsureDependencies();
            }

            // Check if active arena bout or pit match forces combat state
            CheckExternalCombatStates();

            // Check for nearby aggressive combatants
            CheckNearbyHostiles();

            // Decay combat timer
            if (combatTimer > 0f)
            {
                combatTimer -= dt;
            }

            // Determine target alpha
            float targetAlpha = 0f;
            if (IsInCombat)
            {
                targetAlpha = 1f;
            }
            else if (playerStats != null && playerStats.CurrentHP < playerStats.MaxHP * 0.99f)
            {
                // If slightly damaged out of combat, linger faintly or fade
                targetAlpha = combatTimer > -1.5f ? 0.4f : 0f;
            }

            // Smoothly lerp alpha
            if (canvasGroup != null)
            {
                float speed = targetAlpha > canvasGroup.alpha ? fadeInSpeed : fadeOutSpeed;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * dt);
            }

            // Smoothly lerp lag bar
            if (healthLagImage != null && lagFill > targetFill)
            {
                lagFill = Mathf.MoveTowards(lagFill, targetFill, lagCatchupSpeed * dt);
                healthLagImage.fillAmount = lagFill;
            }
        }

        public void TriggerCombat(float duration = -1f)
        {
            combatTimer = duration > 0f ? duration : combatLingerDuration;
        }

        public void EnsureDependencies()
        {
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStatsComponent>();
            }

            if (infuseManager == null)
            {
                infuseManager = FindAnyObjectByType<PlayerInfuseManager>();
            }
        }

        public void BindEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += HandleHealthChanged;
            }

            if (infuseManager != null)
            {
                infuseManager.OnSkillExecuted += HandleSkillExecuted;
            }

            // Arena controller hook
            var arena = FindAnyObjectByType<ArenaBoutController>();
            if (arena != null)
            {
                arena.OnStateChanged += HandleArenaStateChanged;
            }

            // Pit match controller hook
            var pit = FindAnyObjectByType<PitMatchWagerController>();
            if (pit != null)
            {
                pit.OnStateChanged += HandlePitStateChanged;
            }
        }

        public void UnbindEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= HandleHealthChanged;
            }

            if (infuseManager != null)
            {
                infuseManager.OnSkillExecuted -= HandleSkillExecuted;
            }

            var arena = FindAnyObjectByType<ArenaBoutController>();
            if (arena != null)
            {
                arena.OnStateChanged -= HandleArenaStateChanged;
            }

            var pit = FindAnyObjectByType<PitMatchWagerController>();
            if (pit != null)
            {
                pit.OnStateChanged -= HandlePitStateChanged;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            // If taking damage, immediately trigger combat and record lag bar
            if (previousHealth >= 0f && current < previousHealth)
            {
                TriggerCombat();
            }

            previousHealth = current;
            UpdateHealthValues(current, max);
        }

        private void HandleSkillExecuted(Data.EquipmentSlot slot, Data.InfusePartSO module)
        {
            // Executing attack skills enters combat mode
            TriggerCombat();
        }

        private void HandleArenaStateChanged(ArenaBoutState state)
        {
            forceVisibleInArena = (state == ArenaBoutState.PreMatch || state == ArenaBoutState.ActiveBout);
            if (forceVisibleInArena)
            {
                TriggerCombat(10f);
            }
        }

        private void HandlePitStateChanged(PitBoutState state)
        {
            bool inPitFight = (state == PitBoutState.Countdown || state == PitBoutState.ActiveBout);
            if (inPitFight)
            {
                TriggerCombat(10f);
            }
        }

        private void CheckExternalCombatStates()
        {
            var arena = FindAnyObjectByType<ArenaBoutController>();
            if (arena != null)
            {
                forceVisibleInArena = (arena.CurrentState == ArenaBoutState.PreMatch || arena.CurrentState == ArenaBoutState.ActiveBout);
            }
        }

        private void CheckNearbyHostiles()
        {
            if (playerStats == null) return;

            Vector2 playerPos = playerStats.transform.position;

            // Check Colosseum Gladiator
            var glad = FindAnyObjectByType<SanctionedGladiatorAI2D>();
            if (glad != null && !glad.IsDefeated && glad.CurrentState != GladiatorAIState.Inactive && Vector2.Distance(playerPos, glad.transform.position) < 14f)
            {
                TriggerCombat(2.0f);
                return;
            }

            // Check Pit Brawler
            var brawler = FindAnyObjectByType<UnsanctionedPitBrawlerAI2D>();
            if (brawler != null && !brawler.IsDefeated && brawler.CurrentState != PitBrawlerState.Inactive && Vector2.Distance(playerPos, brawler.transform.position) < 14f)
            {
                TriggerCombat(2.0f);
                return;
            }

            // Check Municipal Enforcers
            var enforcers = FindObjectsByType<Security.MunicipalEnforcerAI2D>(FindObjectsInactive.Exclude);
            foreach (var enf in enforcers)
            {
                if (enf != null && enf.CurrentState == Security.EnforcerState.Alerted && Vector2.Distance(playerPos, enf.transform.position) < 12f)
                {
                    TriggerCombat(2.0f);
                    return;
                }
            }
        }

        private void UpdateHealthValues(float current, float max)
        {
            targetFill = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = targetFill;

                // Color tier transition
                if (targetFill > 0.5f)
                {
                    healthFillImage.color = healthyColor;
                }
                else if (targetFill > 0.25f)
                {
                    healthFillImage.color = cautionColor;
                }
                else
                {
                    healthFillImage.color = criticalColor;
                }
            }

            if (healthNumberText != null)
            {
                healthNumberText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)} HP";
            }
        }

        /// <summary>
        /// Self-bootstraps the UI hierarchy under this RectTransform if not already configured in editor.
        /// </summary>
        public void EnsureUIHierarchy()
        {
            var rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();

            // Anchor Top-Left
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -30f);
            rt.sizeDelta = new Vector2(330f, 54f);

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;

            // Background container frame
            var bgImg = GetComponent<Image>();
            if (bgImg == null)
            {
                bgImg = gameObject.AddComponent<Image>();
                bgImg.color = new Color(0.06f, 0.08f, 0.12f, 0.94f);
                bgImg.raycastTarget = false;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Title Label (Header)
            if (titleLabelText == null)
            {
                var titleObj = transform.Find("TitleText")?.gameObject;
                if (titleObj == null)
                {
                    titleObj = new GameObject("TitleText", typeof(RectTransform));
                    titleObj.transform.SetParent(transform, false);
                }
                var trt = titleObj.GetComponent<RectTransform>();
                if (trt == null) trt = titleObj.AddComponent<RectTransform>();
                trt.anchorMin = new Vector2(0.04f, 0.55f);
                trt.anchorMax = new Vector2(0.55f, 0.95f);
                trt.sizeDelta = Vector2.zero;

                titleLabelText = titleObj.GetComponent<Text>();
                if (titleLabelText == null) titleLabelText = titleObj.AddComponent<Text>();
                titleLabelText.font = font;
                titleLabelText.fontSize = 14;
                titleLabelText.fontStyle = FontStyle.Bold;
                titleLabelText.alignment = TextAnchor.MiddleLeft;
                titleLabelText.color = new Color(0.22f, 0.74f, 0.97f, 1f); // #38BDF8 Sky Cyan
                titleLabelText.text = "CHASSIS INTEGRITY";
                titleLabelText.raycastTarget = false;
            }

            // Health Number Readout (Top-Right)
            if (healthNumberText == null)
            {
                var numObj = transform.Find("HealthNumber")?.gameObject;
                if (numObj == null)
                {
                    numObj = new GameObject("HealthNumber", typeof(RectTransform));
                    numObj.transform.SetParent(transform, false);
                }
                var nrt = numObj.GetComponent<RectTransform>();
                if (nrt == null) nrt = numObj.AddComponent<RectTransform>();
                nrt.anchorMin = new Vector2(0.55f, 0.55f);
                nrt.anchorMax = new Vector2(0.96f, 0.95f);
                nrt.sizeDelta = Vector2.zero;

                healthNumberText = numObj.GetComponent<Text>();
                if (healthNumberText == null) healthNumberText = numObj.AddComponent<Text>();
                healthNumberText.font = font;
                healthNumberText.fontSize = 14;
                healthNumberText.fontStyle = FontStyle.Bold;
                healthNumberText.alignment = TextAnchor.MiddleRight;
                healthNumberText.color = Color.white;
                healthNumberText.text = "100 / 100 HP";
                healthNumberText.raycastTarget = false;
            }

            // Tray Background
            Transform trayObj = transform.Find("BarTray");
            if (trayObj == null)
            {
                var newTray = new GameObject("BarTray", typeof(RectTransform));
                newTray.transform.SetParent(transform, false);
                trayObj = newTray.transform;
            }
            var trayRt = trayObj.GetComponent<RectTransform>();
            if (trayRt == null) trayRt = trayObj.gameObject.AddComponent<RectTransform>();
            trayRt.anchorMin = new Vector2(0.04f, 0.12f);
            trayRt.anchorMax = new Vector2(0.96f, 0.48f);
            trayRt.sizeDelta = Vector2.zero;

            var trayImg = trayObj.GetComponent<Image>();
            if (trayImg == null) trayImg = trayObj.gameObject.AddComponent<Image>();
            trayImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            trayImg.raycastTarget = false;

            // Lag Bar Fill
            if (healthLagImage == null)
            {
                var lagObj = trayObj.Find("LagFill")?.gameObject;
                if (lagObj == null)
                {
                    lagObj = new GameObject("LagFill", typeof(RectTransform));
                    lagObj.transform.SetParent(trayObj, false);
                }
                var lrt = lagObj.GetComponent<RectTransform>();
                if (lrt == null) lrt = lagObj.AddComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.sizeDelta = Vector2.zero;

                healthLagImage = lagObj.GetComponent<Image>();
                if (healthLagImage == null) healthLagImage = lagObj.AddComponent<Image>();
                healthLagImage.type = Image.Type.Filled;
                healthLagImage.fillMethod = Image.FillMethod.Horizontal;
                healthLagImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                healthLagImage.fillAmount = 1f;
                healthLagImage.color = lagBarColor;
                healthLagImage.raycastTarget = false;
            }

            // Main Health Fill
            if (healthFillImage == null)
            {
                var fillObj = trayObj.Find("MainFill")?.gameObject;
                if (fillObj == null)
                {
                    fillObj = new GameObject("MainFill", typeof(RectTransform));
                    fillObj.transform.SetParent(trayObj, false);
                }
                var frt = fillObj.GetComponent<RectTransform>();
                if (frt == null) frt = fillObj.AddComponent<RectTransform>();
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = Vector2.one;
                frt.sizeDelta = Vector2.zero;

                healthFillImage = fillObj.GetComponent<Image>();
                if (healthFillImage == null) healthFillImage = fillObj.AddComponent<Image>();
                healthFillImage.type = Image.Type.Filled;
                healthFillImage.fillMethod = Image.FillMethod.Horizontal;
                healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                healthFillImage.fillAmount = 1f;
                healthFillImage.color = healthyColor;
                healthFillImage.raycastTarget = false;
            }
        }
    }
}
