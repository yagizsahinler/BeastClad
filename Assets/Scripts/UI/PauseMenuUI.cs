using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using BeastClad.Persistence;
using BeastClad.Player;
using BeastClad.Interaction;
using BeastClad.Arena;
using BeastClad.Underground;

namespace BeastClad.UI
{
    /// <summary>
    /// Master in-game Pause Menu manager.
    /// Universal across all gameplay sectors. Handles unscaled input toggling via [Esc],
    /// gameplay state freezing (Time.timeScale = 0f), modal priority resolution,
    /// atomic saving, controls cheatsheet inspection, and seamless transition back to MainMenu.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        [Header("Root Panels")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject controlsModalPanel;

        [Header("Sector & Player Profile Displays")]
        [SerializeField] private Text sectorTitleText;
        [SerializeField] private Text creditsValueText;
        [SerializeField] private Text arenaRankText;
        [SerializeField] private Text statusFeedbackText;

        [Header("Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button closeControlsButton;

        private bool isPaused = false;
        private Coroutine feedbackCoroutine;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            EnsureUIHierarchy();
            HookListeners();

            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (controlsModalPanel != null) controlsModalPanel.SetActive(false);
            if (statusFeedbackText != null) statusFeedbackText.text = "";
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnhookListeners();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                // Priority Check: If any NPC, shop, or match modal is open, let that modal consume [Esc]
                if (IsAnyGameplayModalOpen())
                {
                    return;
                }

                // If controls sub-modal is open, dismiss it back to pause menu
                if (controlsModalPanel != null && controlsModalPanel.activeSelf)
                {
                    HandleCloseControlsClicked();
                    return;
                }

                // Otherwise toggle pause menu
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;

            var player = FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.SetMovementLocked(true);
            }

            RefreshProfileDisplay();

            if (controlsModalPanel != null) controlsModalPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (statusFeedbackText != null) statusFeedbackText.text = "";
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            var player = FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.SetMovementLocked(false);
            }

            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (controlsModalPanel != null) controlsModalPanel.SetActive(false);
        }

        public void HandleSaveClicked()
        {
            try
            {
                bool success = SaveManager.Instance.SaveCurrentGame();
                if (success)
                {
                    ShowStatusFeedback("<color=#00FFAA>✔ GAME STATE COMMITTED TO DISK</color>");
                    RefreshProfileDisplay();
                }
                else
                {
                    ShowStatusFeedback("<color=#FF4444>✖ SAVE OPERATION FAILED</color>");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PauseMenuUI] Save failed: {ex.Message}");
                ShowStatusFeedback("<color=#FF4444>✖ ERROR SAVING GAME</color>");
            }
        }

        public void HandleControlsClicked()
        {
            if (controlsModalPanel != null)
            {
                controlsModalPanel.SetActive(true);
            }
        }

        public void HandleCloseControlsClicked()
        {
            if (controlsModalPanel != null)
            {
                controlsModalPanel.SetActive(false);
            }
        }

        public void HandleMainMenuClicked()
        {
            try
            {
                SaveManager.Instance.SaveCurrentGame();
            }
            catch { }

            ResumeGame();
            SceneManager.LoadScene("MainMenu");
        }

        public void HandleQuitClicked()
        {
            try
            {
                SaveManager.Instance.SaveCurrentGame();
            }
            catch { }

            ResumeGame();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RefreshProfileDisplay()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string sectorFriendly = sceneName switch
            {
                "District_CentralHub" => "DISTRICT CENTRAL HUB // CIVIC QUADRANT",
                "Arena_Colosseum" => "MUNICIPAL COLOSSEUM // SANCTIONED ARENA",
                "Adventurer_Wilderness" => "OUTLAND WILDERNESS // UNREGULATED BIOME",
                "Underground_BlackMarket" => "UNDERGROUND SLUMS // BLACK MARKET SECTOR",
                _ => sceneName.ToUpper()
            };

            if (sectorTitleText != null)
            {
                sectorTitleText.text = $"<color=#00FFAA>SECTOR:</color> {sectorFriendly}";
            }

            int credits = 0;
            var player = FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                var wallet = player.GetComponent<PlayerWallet>();
                if (wallet != null) credits = wallet.Credits;
            }
            else if (SaveManager.Instance.CurrentSaveData != null)
            {
                credits = SaveManager.Instance.CurrentSaveData.credits;
            }

            if (creditsValueText != null)
            {
                creditsValueText.text = $"<color=#FFD700>CREDITS:</color> {credits:N0} CR";
            }

            string rank = "Bronze League - Rank III";
            if (SaveManager.Instance.CurrentSaveData != null && !string.IsNullOrEmpty(SaveManager.Instance.CurrentSaveData.arenaRankTitle))
            {
                rank = SaveManager.Instance.CurrentSaveData.arenaRankTitle;
            }

            if (arenaRankText != null)
            {
                arenaRankText.text = $"<color=#38BDF8>DIVISION:</color> {rank}";
            }
        }

        private void ShowStatusFeedback(string message)
        {
            if (statusFeedbackText == null) return;

            statusFeedbackText.text = message;
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ClearFeedbackRoutine());
        }

        private IEnumerator ClearFeedbackRoutine()
        {
            yield return new WaitForSecondsRealtime(3.5f);
            if (statusFeedbackText != null)
            {
                statusFeedbackText.text = "";
            }
        }

        /// <summary>
        /// Checks whether any gameplay modal (NPC dialogue, vendor, ripperdoc, pit wager, match end) is open.
        /// When true, [Esc] is consumed to dismiss that modal first instead of opening the Pause Menu.
        /// </summary>
        public static bool IsAnyGameplayModalOpen()
        {
            var vendorUI = FindAnyObjectByType<CorporateVendorUI>();
            if (vendorUI != null && vendorUI.IsOpen) return true;

            var ripperUI = FindAnyObjectByType<RipperdocUI>();
            if (ripperUI != null && ripperUI.IsOpen) return true;

            var arenaUI = FindAnyObjectByType<ArenaMatchUI>();
            if (arenaUI != null && arenaUI.IsOpen) return true;

            var pitUI = FindAnyObjectByType<PitMatchWagerUI>();
            if (pitUI != null && pitUI.IsOpen) return true;

            var guildUI = FindAnyObjectByType<GuildDialogueUI>();
            if (guildUI != null && guildUI.IsDialogueOpen) return true;

            var attendant = ArenaAttendantNPC.ActiveInstance ?? FindAnyObjectByType<ArenaAttendantNPC>();
            if (attendant != null && attendant.IsOpen) return true;

            var jax = PitmasterJax.ActiveInstance ?? FindAnyObjectByType<PitmasterJax>();
            if (jax != null && jax.IsOpen) return true;

            var doc = RipperdocVendor.ActiveInstance ?? FindAnyObjectByType<RipperdocVendor>();
            if (doc != null && doc.IsOpen) return true;

            var kiosk = FindAnyObjectByType<CorporateVendorKiosk>();
            if (kiosk != null && kiosk.IsOpen) return true;

            return false;
        }

        private void HookListeners()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(ResumeGame);
                resumeButton.onClick.AddListener(ResumeGame);
            }

            if (saveGameButton != null)
            {
                saveGameButton.onClick.RemoveListener(HandleSaveClicked);
                saveGameButton.onClick.AddListener(HandleSaveClicked);
            }

            if (controlsButton != null)
            {
                controlsButton.onClick.RemoveListener(HandleControlsClicked);
                controlsButton.onClick.AddListener(HandleControlsClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
                mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuitClicked);
                quitButton.onClick.AddListener(HandleQuitClicked);
            }

            if (closeControlsButton != null)
            {
                closeControlsButton.onClick.RemoveListener(HandleCloseControlsClicked);
                closeControlsButton.onClick.AddListener(HandleCloseControlsClicked);
            }
        }

        private void UnhookListeners()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(ResumeGame);
            if (saveGameButton != null) saveGameButton.onClick.RemoveListener(HandleSaveClicked);
            if (controlsButton != null) controlsButton.onClick.RemoveListener(HandleControlsClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(HandleQuitClicked);
            if (closeControlsButton != null) closeControlsButton.onClick.RemoveListener(HandleCloseControlsClicked);
        }

        /// <summary>
        /// Self-bootstraps pause UI hierarchy if components are missing on the Canvas.
        /// </summary>
        public void EnsureUIHierarchy()
        {
            if (pauseMenuPanel != null && controlsModalPanel != null) return;

            // Search for existing children by name
            var existingPause = transform.Find("PauseMenuPanel");
            if (existingPause != null)
            {
                pauseMenuPanel = existingPause.gameObject;
            }

            var existingControls = transform.Find("ControlsModalPanel");
            if (existingControls != null)
            {
                controlsModalPanel = existingControls.gameObject;
            }

            // If already complete, bind references
            if (pauseMenuPanel != null && controlsModalPanel != null)
            {
                BindReferencesFromHierarchy();
                return;
            }

            // Build hierarchy dynamically if not present
            BuildRuntimeHierarchy();
            BindReferencesFromHierarchy();
        }

        private void BindReferencesFromHierarchy()
        {
            if (pauseMenuPanel != null)
            {
                if (sectorTitleText == null) sectorTitleText = pauseMenuPanel.transform.Find("Dialog/Header/SectorText")?.GetComponent<Text>();
                if (creditsValueText == null) creditsValueText = pauseMenuPanel.transform.Find("Dialog/Header/ProfileStrip/CreditsText")?.GetComponent<Text>();
                if (arenaRankText == null) arenaRankText = pauseMenuPanel.transform.Find("Dialog/Header/ProfileStrip/RankText")?.GetComponent<Text>();
                if (statusFeedbackText == null) statusFeedbackText = pauseMenuPanel.transform.Find("Dialog/StatusFeedbackText")?.GetComponent<Text>();

                var btnGroup = pauseMenuPanel.transform.Find("Dialog/ButtonGroup");
                if (btnGroup != null)
                {
                    if (resumeButton == null) resumeButton = btnGroup.Find("ResumeButton")?.GetComponent<Button>();
                    if (saveGameButton == null) saveGameButton = btnGroup.Find("SaveButton")?.GetComponent<Button>();
                    if (controlsButton == null) controlsButton = btnGroup.Find("ControlsButton")?.GetComponent<Button>();
                    if (mainMenuButton == null) mainMenuButton = btnGroup.Find("MainMenuButton")?.GetComponent<Button>();
                    if (quitButton == null) quitButton = btnGroup.Find("QuitButton")?.GetComponent<Button>();
                }
            }

            if (controlsModalPanel != null)
            {
                if (closeControlsButton == null) closeControlsButton = controlsModalPanel.transform.Find("Dialog/CloseButton")?.GetComponent<Button>();
            }
        }

        public void BuildRuntimeHierarchy()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            Sprite uisprite = null;
#if UNITY_EDITOR
            uisprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif

            // 1. PAUSE MENU PANEL
            if (pauseMenuPanel == null)
            {
                var pauseObj = new GameObject("PauseMenuPanel");
                pauseObj.transform.SetParent(transform, false);
                var prt = pauseObj.AddComponent<RectTransform>();
                prt.anchorMin = Vector2.zero;
                prt.anchorMax = Vector2.one;
                prt.sizeDelta = Vector2.zero;

                // Dark semi-transparent fullscreen backdrop
                var bg = pauseObj.AddComponent<Image>();
                bg.color = new Color(0.03f, 0.05f, 0.08f, 0.88f);

                // Dialog Box
                var dialog = new GameObject("Dialog");
                dialog.transform.SetParent(pauseObj.transform, false);
                var drt = dialog.AddComponent<RectTransform>();
                drt.anchorMin = new Vector2(0.5f, 0.5f);
                drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.pivot = new Vector2(0.5f, 0.5f);
                drt.sizeDelta = new Vector2(620f, 660f);

                var dimg = dialog.AddComponent<Image>();
                dimg.sprite = uisprite;
                dimg.type = Image.Type.Sliced;
                dimg.color = new Color(0.08f, 0.11f, 0.16f, 0.98f);

                // Header
                var headerObj = new GameObject("Header");
                headerObj.transform.SetParent(dialog.transform, false);
                var hrt = headerObj.AddComponent<RectTransform>();
                hrt.anchorMin = new Vector2(0f, 1f);
                hrt.anchorMax = new Vector2(1f, 1f);
                hrt.pivot = new Vector2(0.5f, 1f);
                hrt.anchoredPosition = new Vector2(0f, -24f);
                hrt.sizeDelta = new Vector2(-40f, 130f);

                var titleObj = new GameObject("TitleText");
                titleObj.transform.SetParent(headerObj.transform, false);
                var trt = titleObj.AddComponent<RectTransform>();
                trt.anchorMin = new Vector2(0f, 0.5f);
                trt.anchorMax = new Vector2(1f, 1f);
                trt.offsetMin = Vector2.zero;
                trt.offsetMax = Vector2.zero;
                var ttxt = titleObj.AddComponent<Text>();
                ttxt.font = font;
                ttxt.fontSize = 36;
                ttxt.fontStyle = FontStyle.Bold;
                ttxt.alignment = TextAnchor.MiddleCenter;
                ttxt.color = new Color(0.2f, 0.85f, 1f, 1f);
                ttxt.text = "SYSTEM PAUSED";
                var tout = titleObj.AddComponent<Outline>();
                tout.effectColor = new Color(0f, 0f, 0f, 0.9f);
                tout.effectDistance = new Vector2(2f, -2f);

                var secObj = new GameObject("SectorText");
                secObj.transform.SetParent(headerObj.transform, false);
                var srt = secObj.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0f, 0.2f);
                srt.anchorMax = new Vector2(1f, 0.55f);
                srt.offsetMin = Vector2.zero;
                srt.offsetMax = Vector2.zero;
                var stxt = secObj.AddComponent<Text>();
                stxt.font = font;
                stxt.fontSize = 18;
                stxt.fontStyle = FontStyle.Bold;
                stxt.alignment = TextAnchor.MiddleCenter;
                stxt.color = new Color(0.8f, 0.9f, 0.95f, 0.9f);
                stxt.text = "SECTOR: DISTRICT CENTRAL HUB";
                sectorTitleText = stxt;

                // Profile Strip (Credits & Rank)
                var profObj = new GameObject("ProfileStrip");
                profObj.transform.SetParent(headerObj.transform, false);
                var prrt = profObj.AddComponent<RectTransform>();
                prrt.anchorMin = new Vector2(0f, 0f);
                prrt.anchorMax = new Vector2(1f, 0.25f);
                prrt.offsetMin = Vector2.zero;
                prrt.offsetMax = Vector2.zero;

                var crObj = new GameObject("CreditsText");
                crObj.transform.SetParent(profObj.transform, false);
                var crt = crObj.AddComponent<RectTransform>();
                crt.anchorMin = new Vector2(0f, 0f);
                crt.anchorMax = new Vector2(0.5f, 1f);
                crt.offsetMin = Vector2.zero;
                crt.offsetMax = Vector2.zero;
                var crtxt = crObj.AddComponent<Text>();
                crtxt.font = font;
                crtxt.fontSize = 17;
                crtxt.alignment = TextAnchor.MiddleCenter;
                crtxt.color = new Color(1f, 0.85f, 0.2f, 1f);
                crtxt.text = "CREDITS: 1,000 CR";
                creditsValueText = crtxt;

                var rkObj = new GameObject("RankText");
                rkObj.transform.SetParent(profObj.transform, false);
                var rkrt = rkObj.AddComponent<RectTransform>();
                rkrt.anchorMin = new Vector2(0.5f, 0f);
                rkrt.anchorMax = new Vector2(1f, 1f);
                rkrt.offsetMin = Vector2.zero;
                rkrt.offsetMax = Vector2.zero;
                var rktxt = rkObj.AddComponent<Text>();
                rktxt.font = font;
                rktxt.fontSize = 17;
                rktxt.alignment = TextAnchor.MiddleCenter;
                rktxt.color = new Color(0.3f, 0.8f, 1f, 1f);
                rktxt.text = "DIVISION: Bronze III";
                arenaRankText = rktxt;

                // Button Group
                var btnGroup = new GameObject("ButtonGroup");
                btnGroup.transform.SetParent(dialog.transform, false);
                var bgrt = btnGroup.AddComponent<RectTransform>();
                bgrt.anchorMin = new Vector2(0.5f, 0.5f);
                bgrt.anchorMax = new Vector2(0.5f, 0.5f);
                bgrt.pivot = new Vector2(0.5f, 0.5f);
                bgrt.anchoredPosition = new Vector2(0f, -40f);
                bgrt.sizeDelta = new Vector2(480f, 320f);

                var vlg = btnGroup.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 14f;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                resumeButton = CreateMenuButton(btnGroup.transform, "ResumeButton", "RESUME", new Color(0.12f, 0.55f, 0.65f, 1f), font, uisprite);
                saveGameButton = CreateMenuButton(btnGroup.transform, "SaveButton", "SAVE GAME", new Color(0.15f, 0.45f, 0.35f, 1f), font, uisprite);
                controlsButton = CreateMenuButton(btnGroup.transform, "ControlsButton", "CONTROLS & MANUAL", new Color(0.25f, 0.35f, 0.5f, 1f), font, uisprite);
                mainMenuButton = CreateMenuButton(btnGroup.transform, "MainMenuButton", "RETURN TO MAIN MENU", new Color(0.45f, 0.35f, 0.15f, 1f), font, uisprite);
                quitButton = CreateMenuButton(btnGroup.transform, "QuitButton", "QUIT TO DESKTOP", new Color(0.5f, 0.18f, 0.18f, 1f), font, uisprite);

                // Status Toast
                var statusObj = new GameObject("StatusFeedbackText");
                statusObj.transform.SetParent(dialog.transform, false);
                var sbrt = statusObj.AddComponent<RectTransform>();
                sbrt.anchorMin = new Vector2(0f, 0f);
                sbrt.anchorMax = new Vector2(1f, 0f);
                sbrt.pivot = new Vector2(0.5f, 0f);
                sbrt.anchoredPosition = new Vector2(0f, 25f);
                sbrt.sizeDelta = new Vector2(-40f, 40f);
                var sbTxt = statusObj.AddComponent<Text>();
                sbTxt.font = font;
                sbTxt.fontSize = 19;
                sbTxt.fontStyle = FontStyle.Bold;
                sbTxt.alignment = TextAnchor.MiddleCenter;
                sbTxt.color = new Color(0f, 1f, 0.65f, 1f);
                sbTxt.text = "";
                statusFeedbackText = sbTxt;

                pauseMenuPanel = pauseObj;
            }

            // 2. CONTROLS MODAL PANEL
            if (controlsModalPanel == null)
            {
                var ctrlObj = new GameObject("ControlsModalPanel");
                ctrlObj.transform.SetParent(transform, false);
                var crt = ctrlObj.AddComponent<RectTransform>();
                crt.anchorMin = Vector2.zero;
                crt.anchorMax = Vector2.one;
                crt.sizeDelta = Vector2.zero;

                var bg = ctrlObj.AddComponent<Image>();
                bg.color = new Color(0.02f, 0.03f, 0.05f, 0.94f);

                var dialog = new GameObject("Dialog");
                dialog.transform.SetParent(ctrlObj.transform, false);
                var drt = dialog.AddComponent<RectTransform>();
                drt.anchorMin = new Vector2(0.5f, 0.5f);
                drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.pivot = new Vector2(0.5f, 0.5f);
                drt.sizeDelta = new Vector2(740f, 660f);

                var dimg = dialog.AddComponent<Image>();
                dimg.sprite = uisprite;
                dimg.type = Image.Type.Sliced;
                dimg.color = new Color(0.08f, 0.11f, 0.16f, 0.98f);

                // Title
                var ctitle = new GameObject("TitleText");
                ctitle.transform.SetParent(dialog.transform, false);
                var ctr = ctitle.AddComponent<RectTransform>();
                ctr.anchorMin = new Vector2(0f, 1f);
                ctr.anchorMax = new Vector2(1f, 1f);
                ctr.pivot = new Vector2(0.5f, 1f);
                ctr.anchoredPosition = new Vector2(0f, -25f);
                ctr.sizeDelta = new Vector2(-40f, 50f);
                var ctxt = ctitle.AddComponent<Text>();
                ctxt.font = font;
                ctxt.fontSize = 32;
                ctxt.fontStyle = FontStyle.Bold;
                ctxt.alignment = TextAnchor.MiddleCenter;
                ctxt.color = new Color(0.2f, 0.85f, 1f, 1f);
                ctxt.text = "OPERATIONAL CONTROLS & BINDINGS";

                // Content Text
                var contentObj = new GameObject("ControlsContent");
                contentObj.transform.SetParent(dialog.transform, false);
                var conrt = contentObj.AddComponent<RectTransform>();
                conrt.anchorMin = new Vector2(0.5f, 0.5f);
                conrt.anchorMax = new Vector2(0.5f, 0.5f);
                conrt.pivot = new Vector2(0.5f, 0.5f);
                conrt.anchoredPosition = new Vector2(0f, 5f);
                conrt.sizeDelta = new Vector2(640f, 440f);

                var conTxt = contentObj.AddComponent<Text>();
                conTxt.font = font;
                conTxt.fontSize = 19;
                conTxt.alignment = TextAnchor.UpperLeft;
                conTxt.lineSpacing = 1.3f;
                conTxt.color = new Color(0.85f, 0.92f, 0.98f, 1f);
                conTxt.text =
                    "<b><color=#00FFAA>MOVEMENT & EXPLORATION</color></b>\n" +
                    " • <b>[ W ][ A ][ S ][ D ] / Arrows</b> : Locomotion in 2D sectors\n" +
                    " • <b>[ Left Shift ]</b> : Biometric Sprint (Speed Burst)\n" +
                    " • <b>[ F ]</b> : Interact / Speak / Access Terminals & Kiosks\n\n" +
                    "<b><color=#FFD700>INFUSE COMBAT & MODULES</color></b>\n" +
                    " • <b>[ Left Click ] / [ J ]</b> : Left Arm Offensive Strike\n" +
                    " • <b>[ Right Click ] / [ K ]</b> : Right Arm Heavy / Special Strike\n" +
                    " • <b>[ Space ]</b> : Legs Evasive Dash / Thruster Dodge\n" +
                    " • <b>[ Q ]</b> : Head Utility Module (Bio-Scanner / EMP)\n" +
                    " • <b>[ E ]</b> : Chest Core Module (Armor Overcharge / Barrier)\n\n" +
                    "<b><color=#38BDF8>SYSTEM SHORTCUTS</color></b>\n" +
                    " • <b>[ Esc ]</b> : Pause Menu / Close Active Dialogs\n" +
                    " • <b>[ F5 ] / [ F6 ]</b> : QuickSave / QuickLoad Live Game State";

                // Close Button
                closeControlsButton = CreateMenuButton(dialog.transform, "CloseButton", "BACK TO PAUSE MENU", new Color(0.2f, 0.45f, 0.6f, 1f), font, uisprite);
                var cbrt = closeControlsButton.GetComponent<RectTransform>();
                cbrt.anchorMin = new Vector2(0.5f, 0f);
                cbrt.anchorMax = new Vector2(0.5f, 0f);
                cbrt.pivot = new Vector2(0.5f, 0f);
                cbrt.anchoredPosition = new Vector2(0f, 30f);
                cbrt.sizeDelta = new Vector2(360f, 52f);

                controlsModalPanel = ctrlObj;
            }
        }

        private static Button CreateMenuButton(Transform parent, string name, string text, Color color, Font font, Sprite uisprite)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440f, 52f);

            var img = btnObj.AddComponent<Image>();
            img.sprite = uisprite;
            img.type = Image.Type.Sliced;
            img.color = color;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.3f;
            colors.pressedColor = color * 0.8f;
            colors.selectedColor = color * 1.2f;
            btn.colors = colors;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = text;
            txt.raycastTarget = false;

            var outl = textObj.AddComponent<Outline>();
            outl.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            return btn;
        }
    }
}
