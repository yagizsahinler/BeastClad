using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using BeastClad.UI;

namespace BeastClad.Editor
{
    /// <summary>
    /// Automated scene builder for Assets/Scenes/MainMenu.unity.
    /// Sets up the 2D orthographic presentation camera, EventSystem,
    /// Cyber-Roman typographic title banner, interactive menu action buttons,
    /// audio settings slider, controls guide modal, and New Game confirmation dialog.
    /// Also registers MainMenu.unity as Scene 0 in EditorBuildSettings.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        [MenuItem("BeastClad/Build Main Menu Scene", priority = 1)]
        public static void BuildScene()
        {
            // 1. Create New Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Resources
            var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            // 3. Camera Setup
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.045f, 0.07f, 1f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            camObj.AddComponent<AudioListener>();

            // 4. EventSystem Setup
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();

            // 5. Canvas Root
            var canvasObj = new GameObject("MainMenu_Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            var mainMenuUI = canvasObj.AddComponent<MainMenuUI>();
            var so = new SerializedObject(mainMenuUI);

            // 6. Background Panels & Cyber Deco
            BuildBackgroundDecorations(canvasObj.transform, uisprite, bgSprite);

            // 7. Title Header
            BuildTitleSection(canvasObj.transform, defaultFont);

            // 8. Main Buttons Menu
            BuildMainMenuButtons(canvasObj.transform, defaultFont, uisprite, so);

            // 9. Modals (Settings, Lore, Confirm New Game)
            BuildModals(canvasObj.transform, defaultFont, uisprite, so);

            so.ApplyModifiedProperties();

            // 10. Save Scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"<color=#00FFAA>[MainMenuSceneBuilder]</color> Successfully built and saved <b>{scenePath}</b>!");

            // 11. Register in EditorBuildSettings as Index 0
            RegisterSceneInBuildSettings(scenePath);
        }

        private static void BuildBackgroundDecorations(Transform parent, Sprite uisprite, Sprite bgSprite)
        {
            // Dark vignette backdrop
            var bgObj = new GameObject("BackgroundBackdrop");
            bgObj.transform.SetParent(parent, false);
            var bgrt = bgObj.AddComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.sizeDelta = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color = new Color(0.02f, 0.03f, 0.05f, 0.95f);
            bgImg.raycastTarget = false;

            // Cyber accent horizontal bar top
            var topBar = new GameObject("TopCyberBar");
            topBar.transform.SetParent(parent, false);
            var trt = topBar.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = Vector2.zero;
            trt.sizeDelta = new Vector2(0f, 4f);
            var timg = topBar.AddComponent<Image>();
            timg.sprite = uisprite;
            timg.color = new Color(0.2f, 0.85f, 1f, 0.8f);
            timg.raycastTarget = false;

            // Cyber accent horizontal bar bottom
            var btmBar = new GameObject("BottomCyberBar");
            btmBar.transform.SetParent(parent, false);
            var brt = btmBar.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(0f, 4f);
            var bimg = btmBar.AddComponent<Image>();
            bimg.sprite = uisprite;
            bimg.color = new Color(1f, 0.75f, 0.1f, 0.8f);
            bimg.raycastTarget = false;
        }

        private static void BuildTitleSection(Transform parent, Font font)
        {
            var headerRoot = new GameObject("TitleHeader");
            headerRoot.transform.SetParent(parent, false);
            var hrt = headerRoot.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0f, 1f);
            hrt.anchoredPosition = new Vector2(120f, -90f);
            hrt.sizeDelta = new Vector2(800f, 220f);

            // Title Text
            var titleObj = new GameObject("MainTitleText");
            titleObj.transform.SetParent(headerRoot.transform, false);
            var trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.4f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var ttxt = titleObj.AddComponent<Text>();
            ttxt.font = font;
            ttxt.fontSize = 76;
            ttxt.fontStyle = FontStyle.Bold;
            ttxt.alignment = TextAnchor.MiddleLeft;
            ttxt.color = new Color(0.2f, 0.88f, 1f, 1f);
            ttxt.text = "BEASTCLAD";
            ttxt.raycastTarget = false;

            var tout = titleObj.AddComponent<Outline>();
            tout.effectColor = new Color(0f, 0f, 0f, 0.95f);
            tout.effectDistance = new Vector2(3f, -3f);

            var tshad = titleObj.AddComponent<Shadow>();
            tshad.effectColor = new Color(0f, 0.5f, 0.8f, 0.5f);
            tshad.effectDistance = new Vector2(0f, -4f);

            // Tagline
            var tagObj = new GameObject("TaglineText");
            tagObj.transform.SetParent(headerRoot.transform, false);
            var tagrt = tagObj.AddComponent<RectTransform>();
            tagrt.anchorMin = new Vector2(0f, 0.1f);
            tagrt.anchorMax = new Vector2(1f, 0.4f);
            tagrt.offsetMin = Vector2.zero;
            tagrt.offsetMax = Vector2.zero;

            var tagTxt = tagObj.AddComponent<Text>();
            tagTxt.font = font;
            tagTxt.fontSize = 21;
            tagTxt.fontStyle = FontStyle.Bold;
            tagTxt.alignment = TextAnchor.MiddleLeft;
            tagTxt.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            tagTxt.text = "SYNTHETIC BIO-ARMOR RPG  //  NEO-VERIDIA";
            tagTxt.raycastTarget = false;

            var tagOut = tagObj.AddComponent<Outline>();
            tagOut.effectColor = new Color(0f, 0f, 0f, 0.9f);
            tagOut.effectDistance = new Vector2(1.5f, -1.5f);

            // Subtitle Description
            var subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(headerRoot.transform, false);
            var subrt = subObj.AddComponent<RectTransform>();
            subrt.anchorMin = new Vector2(0f, -0.2f);
            subrt.anchorMax = new Vector2(1f, 0.1f);
            subrt.offsetMin = Vector2.zero;
            subrt.offsetMax = Vector2.zero;

            var subTxt = subObj.AddComponent<Text>();
            subTxt.font = font;
            subTxt.fontSize = 16;
            subTxt.alignment = TextAnchor.MiddleLeft;
            subTxt.color = new Color(0.7f, 0.82f, 0.92f, 0.75f);
            subTxt.text = "Infuse captive specimens into cybernetic body modules. Conquer the colosseum, tame the outlands, or profit in shadows.";
            subTxt.raycastTarget = false;

            // Version label (Bottom Left)
            var verObj = new GameObject("VersionText");
            verObj.transform.SetParent(parent, false);
            var vert = verObj.AddComponent<RectTransform>();
            vert.anchorMin = new Vector2(0f, 0f);
            vert.anchorMax = new Vector2(0f, 0f);
            vert.pivot = new Vector2(0f, 0f);
            vert.anchoredPosition = new Vector2(40f, 25f);
            vert.sizeDelta = new Vector2(400f, 30f);

            var vtxt = verObj.AddComponent<Text>();
            vtxt.font = font;
            vtxt.fontSize = 15;
            vtxt.alignment = TextAnchor.MiddleLeft;
            vtxt.color = new Color(0.5f, 0.65f, 0.75f, 0.6f);
            vtxt.text = "Build 0.5.2 • Pre-Alpha Sandbox Phase 5.2";
            vtxt.raycastTarget = false;
        }

        private static void BuildMainMenuButtons(Transform parent, Font font, Sprite uisprite, SerializedObject so)
        {
            var menuRoot = new GameObject("MenuButtonGroup");
            menuRoot.transform.SetParent(parent, false);
            var mrt = menuRoot.AddComponent<RectTransform>();
            mrt.anchorMin = new Vector2(0f, 0.5f);
            mrt.anchorMax = new Vector2(0f, 0.5f);
            mrt.pivot = new Vector2(0f, 0.5f);
            mrt.anchoredPosition = new Vector2(120f, -60f);
            mrt.sizeDelta = new Vector2(500f, 380f);

            var vlg = menuRoot.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 1. CONTINUE BUTTON (Contains title & subtext)
            var contBtnObj = CreateStyledButton(menuRoot.transform, "ContinueButton", new Color(0.12f, 0.5f, 0.65f, 1f), 68f, uisprite);
            var contBtn = contBtnObj.GetComponent<Button>();
            so.FindProperty("continueButton").objectReferenceValue = contBtn;

            var contTextObj = new GameObject("MainText");
            contTextObj.transform.SetParent(contBtnObj.transform, false);
            var ctrt = contTextObj.AddComponent<RectTransform>();
            ctrt.anchorMin = new Vector2(0f, 0.4f);
            ctrt.anchorMax = new Vector2(1f, 1f);
            ctrt.offsetMin = new Vector2(24f, 0f);
            ctrt.offsetMax = new Vector2(-24f, 0f);
            var ctxt = contTextObj.AddComponent<Text>();
            ctxt.font = font;
            ctxt.fontSize = 24;
            ctxt.fontStyle = FontStyle.Bold;
            ctxt.alignment = TextAnchor.MiddleLeft;
            ctxt.color = Color.white;
            ctxt.text = "CONTINUE CAMPAIGN";
            ctxt.raycastTarget = false;
            so.FindProperty("continueButtonText").objectReferenceValue = ctxt;

            var contSubObj = new GameObject("SubText");
            contSubObj.transform.SetParent(contBtnObj.transform, false);
            var csrt = contSubObj.AddComponent<RectTransform>();
            csrt.anchorMin = new Vector2(0f, 0f);
            csrt.anchorMax = new Vector2(1f, 0.45f);
            csrt.offsetMin = new Vector2(24f, 0f);
            csrt.offsetMax = new Vector2(-24f, 0f);
            var cstxt = contSubObj.AddComponent<Text>();
            cstxt.font = font;
            cstxt.fontSize = 15;
            cstxt.alignment = TextAnchor.MiddleLeft;
            cstxt.color = new Color(0.8f, 0.92f, 1f, 0.8f);
            cstxt.text = "No saved campaign found";
            cstxt.raycastTarget = false;
            so.FindProperty("continueSubText").objectReferenceValue = cstxt;

            // 2. NEW GAME BUTTON
            var newGameBtnObj = CreateStyledButtonWithText(menuRoot.transform, "NewGameButton", "NEW CAMPAIGN", new Color(0.15f, 0.55f, 0.38f, 1f), 56f, font, uisprite);
            so.FindProperty("newGameButton").objectReferenceValue = newGameBtnObj.GetComponent<Button>();

            // 3. SETTINGS & CONTROLS BUTTON
            var settingsBtnObj = CreateStyledButtonWithText(menuRoot.transform, "SettingsButton", "SETTINGS & CONTROLS", new Color(0.24f, 0.34f, 0.48f, 1f), 56f, font, uisprite);
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtnObj.GetComponent<Button>();

            // 4. LORE & ARCHIVES BUTTON
            var loreBtnObj = CreateStyledButtonWithText(menuRoot.transform, "LoreButton", "CODEX & BIO-INFUSE LORE", new Color(0.42f, 0.32f, 0.16f, 1f), 56f, font, uisprite);
            so.FindProperty("loreButton").objectReferenceValue = loreBtnObj.GetComponent<Button>();

            // 5. QUIT BUTTON
            var quitBtnObj = CreateStyledButtonWithText(menuRoot.transform, "QuitButton", "QUIT GAME", new Color(0.52f, 0.18f, 0.18f, 1f), 56f, font, uisprite);
            so.FindProperty("quitButton").objectReferenceValue = quitBtnObj.GetComponent<Button>();
        }

        private static void BuildModals(Transform parent, Font font, Sprite uisprite, SerializedObject so)
        {
            // ----------------------------------------------------
            // A. SETTINGS MODAL
            // ----------------------------------------------------
            var setObj = new GameObject("SettingsModalPanel");
            setObj.transform.SetParent(parent, false);
            var srt = setObj.AddComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.sizeDelta = Vector2.zero;
            var sbg = setObj.AddComponent<Image>();
            sbg.color = new Color(0.02f, 0.03f, 0.05f, 0.94f);
            so.FindProperty("settingsModalPanel").objectReferenceValue = setObj;

            var sdiag = new GameObject("Dialog");
            sdiag.transform.SetParent(setObj.transform, false);
            var sdrt = sdiag.AddComponent<RectTransform>();
            sdrt.anchorMin = new Vector2(0.5f, 0.5f);
            sdrt.anchorMax = new Vector2(0.5f, 0.5f);
            sdrt.pivot = new Vector2(0.5f, 0.5f);
            sdrt.sizeDelta = new Vector2(800f, 680f);
            var sdImg = sdiag.AddComponent<Image>();
            sdImg.sprite = uisprite;
            sdImg.type = Image.Type.Sliced;
            sdImg.color = new Color(0.08f, 0.11f, 0.16f, 0.98f);

            var stitle = CreateHeaderText(sdiag.transform, "TitleText", "SYSTEM SETTINGS & CONTROLS", font, new Vector2(0f, -25f), 32);

            // Volume Section
            var volSection = new GameObject("VolumeSection");
            volSection.transform.SetParent(sdiag.transform, false);
            var vsrt = volSection.AddComponent<RectTransform>();
            vsrt.anchorMin = new Vector2(0.5f, 1f);
            vsrt.anchorMax = new Vector2(0.5f, 1f);
            vsrt.pivot = new Vector2(0.5f, 1f);
            vsrt.anchoredPosition = new Vector2(0f, -80f);
            vsrt.sizeDelta = new Vector2(700f, 50f);

            var volLabel = new GameObject("Label");
            volLabel.transform.SetParent(volSection.transform, false);
            var vlrt = volLabel.AddComponent<RectTransform>();
            vlrt.anchorMin = new Vector2(0f, 0f);
            vlrt.anchorMax = new Vector2(0.3f, 1f);
            vlrt.offsetMin = Vector2.zero;
            vlrt.offsetMax = Vector2.zero;
            var vltxt = volLabel.AddComponent<Text>();
            vltxt.font = font;
            vltxt.fontSize = 20;
            vltxt.fontStyle = FontStyle.Bold;
            vltxt.alignment = TextAnchor.MiddleLeft;
            vltxt.color = new Color(0.2f, 0.85f, 1f, 1f);
            vltxt.text = "MASTER AUDIO:";
            vltxt.raycastTarget = false;

            var sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(volSection.transform, false);
            var slrt = sliderObj.AddComponent<RectTransform>();
            slrt.anchorMin = new Vector2(0.32f, 0.2f);
            slrt.anchorMax = new Vector2(0.82f, 0.8f);
            slrt.offsetMin = Vector2.zero;
            slrt.offsetMax = Vector2.zero;
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            var slBg = sliderObj.AddComponent<Image>();
            slBg.sprite = uisprite;
            slBg.type = Image.Type.Sliced;
            slBg.color = new Color(0.15f, 0.2f, 0.28f, 1f);
            slider.targetGraphic = slBg;

            var slValObj = new GameObject("VolumeValue");
            slValObj.transform.SetParent(volSection.transform, false);
            var svrt = slValObj.AddComponent<RectTransform>();
            svrt.anchorMin = new Vector2(0.85f, 0f);
            svrt.anchorMax = new Vector2(1f, 1f);
            svrt.offsetMin = Vector2.zero;
            svrt.offsetMax = Vector2.zero;
            var svtxt = slValObj.AddComponent<Text>();
            svtxt.font = font;
            svtxt.fontSize = 20;
            svtxt.fontStyle = FontStyle.Bold;
            svtxt.alignment = TextAnchor.MiddleCenter;
            svtxt.color = Color.white;
            svtxt.text = "100%";
            svtxt.raycastTarget = false;

            so.FindProperty("volumeSlider").objectReferenceValue = slider;
            so.FindProperty("volumeValueText").objectReferenceValue = svtxt;

            // Controls Keybindings List
            var ctrlList = new GameObject("ControlsContent");
            ctrlList.transform.SetParent(sdiag.transform, false);
            var clrt = ctrlList.AddComponent<RectTransform>();
            clrt.anchorMin = new Vector2(0.5f, 0.5f);
            clrt.anchorMax = new Vector2(0.5f, 0.5f);
            clrt.pivot = new Vector2(0.5f, 0.5f);
            clrt.anchoredPosition = new Vector2(0f, -30f);
            clrt.sizeDelta = new Vector2(700f, 380f);

            var cltxt = ctrlList.AddComponent<Text>();
            cltxt.font = font;
            cltxt.fontSize = 18;
            cltxt.alignment = TextAnchor.UpperLeft;
            cltxt.lineSpacing = 1.3f;
            cltxt.color = new Color(0.88f, 0.94f, 1f, 1f);
            cltxt.raycastTarget = false;
            cltxt.text =
                "<b><color=#00FFAA>PRIMARY COMMANDS</color></b>\n" +
                " • <b>[ W ][ A ][ S ][ D ] / Arrows</b> : Movement through 2D quadrants\n" +
                " • <b>[ Left Shift ]</b> : Biometric Sprint (Speed Boost)\n" +
                " • <b>[ F ]</b> : Interact / Speak / Kiosks / Quarantine Lockers\n\n" +
                "<b><color=#FFD700>INFUSED BIO-ARMOR COMBAT</color></b>\n" +
                " • <b>[ Left Click ] / [ J ]</b> : Left Arm Offensive Strike\n" +
                " • <b>[ Right Click ] / [ K ]</b> : Right Arm Heavy / Special Ability\n" +
                " • <b>[ Space ]</b> : Legs Evasive Dash / Thruster Dodge\n" +
                " • <b>[ Q ]</b> : Head Utility Module (Bio-Scanner / EMP Pulse)\n" +
                " • <b>[ E ]</b> : Chest Core Module (Armor Overcharge / Kinetic Shield)\n\n" +
                "<b><color=#38BDF8>PERSISTENCE & NAVIGATION</color></b>\n" +
                " • <b>[ Esc ]</b> : In-Game Pause Menu / Dismiss Active Modals\n" +
                " • <b>[ F5 ] / [ F6 ]</b> : QuickSave / QuickLoad";

            // Close Settings Button
            var closeSetBtnObj = CreateStyledButtonWithText(sdiag.transform, "CloseButton", "RETURN TO MAIN MENU", new Color(0.2f, 0.45f, 0.6f, 1f), 50f, font, uisprite);
            var csbrt = closeSetBtnObj.GetComponent<RectTransform>();
            csbrt.anchorMin = new Vector2(0.5f, 0f);
            csbrt.anchorMax = new Vector2(0.5f, 0f);
            csbrt.pivot = new Vector2(0.5f, 0f);
            csbrt.anchoredPosition = new Vector2(0f, 25f);
            csbrt.sizeDelta = new Vector2(340f, 50f);
            so.FindProperty("closeSettingsButton").objectReferenceValue = closeSetBtnObj.GetComponent<Button>();

            // ----------------------------------------------------
            // B. LORE MODAL
            // ----------------------------------------------------
            var loreObj = new GameObject("LoreModalPanel");
            loreObj.transform.SetParent(parent, false);
            var lrt = loreObj.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.sizeDelta = Vector2.zero;
            var lbg = loreObj.AddComponent<Image>();
            lbg.color = new Color(0.02f, 0.03f, 0.05f, 0.94f);
            so.FindProperty("loreModalPanel").objectReferenceValue = loreObj;

            var ldiag = new GameObject("Dialog");
            ldiag.transform.SetParent(loreObj.transform, false);
            var ldrt = ldiag.AddComponent<RectTransform>();
            ldrt.anchorMin = new Vector2(0.5f, 0.5f);
            ldrt.anchorMax = new Vector2(0.5f, 0.5f);
            ldrt.pivot = new Vector2(0.5f, 0.5f);
            ldrt.sizeDelta = new Vector2(820f, 680f);
            var ldImg = ldiag.AddComponent<Image>();
            ldImg.sprite = uisprite;
            ldImg.type = Image.Type.Sliced;
            ldImg.color = new Color(0.08f, 0.11f, 0.16f, 0.98f);

            CreateHeaderText(ldiag.transform, "TitleText", "BEASTCLAD // CODEX & SYSTEMS LORE", font, new Vector2(0f, -25f), 30);

            var loreBody = new GameObject("LoreContent");
            loreBody.transform.SetParent(ldiag.transform, false);
            var lbrt = loreBody.AddComponent<RectTransform>();
            lbrt.anchorMin = new Vector2(0.5f, 0.5f);
            lbrt.anchorMax = new Vector2(0.5f, 0.5f);
            lbrt.pivot = new Vector2(0.5f, 0.5f);
            lbrt.anchoredPosition = new Vector2(0f, 10f);
            lbrt.sizeDelta = new Vector2(720f, 480f);

            var lbtxt = loreBody.AddComponent<Text>();
            lbtxt.font = font;
            lbtxt.fontSize = 17;
            lbtxt.alignment = TextAnchor.UpperLeft;
            lbtxt.lineSpacing = 1.35f;
            lbtxt.color = new Color(0.88f, 0.92f, 0.98f, 1f);
            lbtxt.raycastTarget = false;
            lbtxt.text =
                "<b><color=#FFD700>THE WORLD OF NEO-VERIDIA (YEAR 2184)</color></b>\n" +
                "Following the Collapse, apex synthetic wildlife colonized the Outland Biomes. Humanity survived through the breakthrough science of <i>Bio-Infusion</i>.\n\n" +
                "<b><color=#00FFAA>THE INFUSE SYSTEM</color></b>\n" +
                "Monsters in BeastClad do not fight as independent companions. Instead, their genetic codes and biophysical organs are converted into wearable exosuits:\n" +
                " • <b>Head Slot</b>: Sensory optics, targeting, and raw biometric HP capacity.\n" +
                " • <b>Chest Slot</b>: Hardened exoskeleton, elemental armor defense, and kinetic core.\n" +
                " • <b>Left & Right Arms</b>: Offensive attack modules and heavy weaponry.\n" +
                " • <b>Legs Slot</b>: Locomotion thrusters, dash cooldowns, and sprint velocity.\n\n" +
                "<b><color=#38BDF8>THREE PATHS TO SUPREMACY</color></b>\n" +
                " 1. <b>The Sanctioned Colosseum</b>: Earn corporate glory, prize purses, and ladder division rank.\n" +
                " 2. <b>The Adventurer's Guild</b>: Trap wild beasts in the lethal Outland biomes for bounty contracts.\n" +
                " 3. <b>The Black Market & Ripperdocs</b>: Overclock neural ports, trade contraband, and fight in unsanctioned death pits.";

            var closeLoreBtnObj = CreateStyledButtonWithText(ldiag.transform, "CloseButton", "RETURN TO MAIN MENU", new Color(0.42f, 0.32f, 0.16f, 1f), 50f, font, uisprite);
            var clbrt = closeLoreBtnObj.GetComponent<RectTransform>();
            clbrt.anchorMin = new Vector2(0.5f, 0f);
            clbrt.anchorMax = new Vector2(0.5f, 0f);
            clbrt.pivot = new Vector2(0.5f, 0f);
            clbrt.anchoredPosition = new Vector2(0f, 25f);
            clbrt.sizeDelta = new Vector2(340f, 50f);
            so.FindProperty("closeLoreButton").objectReferenceValue = closeLoreBtnObj.GetComponent<Button>();

            // ----------------------------------------------------
            // C. CONFIRM NEW GAME MODAL
            // ----------------------------------------------------
            var confObj = new GameObject("NewGameConfirmModalPanel");
            confObj.transform.SetParent(parent, false);
            var cfrt = confObj.AddComponent<RectTransform>();
            cfrt.anchorMin = Vector2.zero;
            cfrt.anchorMax = Vector2.one;
            cfrt.sizeDelta = Vector2.zero;
            var cfbg = confObj.AddComponent<Image>();
            cfbg.color = new Color(0.02f, 0.03f, 0.05f, 0.94f);
            so.FindProperty("newGameConfirmModalPanel").objectReferenceValue = confObj;

            var cfdiag = new GameObject("Dialog");
            cfdiag.transform.SetParent(confObj.transform, false);
            var cfdrt = cfdiag.AddComponent<RectTransform>();
            cfdrt.anchorMin = new Vector2(0.5f, 0.5f);
            cfdrt.anchorMax = new Vector2(0.5f, 0.5f);
            cfdrt.pivot = new Vector2(0.5f, 0.5f);
            cfdrt.sizeDelta = new Vector2(620f, 360f);
            var cfImg = cfdiag.AddComponent<Image>();
            cfImg.sprite = uisprite;
            cfImg.type = Image.Type.Sliced;
            cfImg.color = new Color(0.12f, 0.08f, 0.08f, 0.98f);

            CreateHeaderText(cfdiag.transform, "TitleText", "OVERWRITE EXISTING CAMPAIGN?", font, new Vector2(0f, -25f), 28);

            var warnTextObj = new GameObject("WarningText");
            warnTextObj.transform.SetParent(cfdiag.transform, false);
            var wtrt = warnTextObj.AddComponent<RectTransform>();
            wtrt.anchorMin = new Vector2(0.5f, 0.5f);
            wtrt.anchorMax = new Vector2(0.5f, 0.5f);
            wtrt.pivot = new Vector2(0.5f, 0.5f);
            wtrt.anchoredPosition = new Vector2(0f, 15f);
            wtrt.sizeDelta = new Vector2(520f, 140f);

            var wtxt = warnTextObj.AddComponent<Text>();
            wtxt.font = font;
            wtxt.fontSize = 18;
            wtxt.alignment = TextAnchor.MiddleCenter;
            wtxt.lineSpacing = 1.3f;
            wtxt.color = new Color(1f, 0.85f, 0.85f, 1f);
            wtxt.raycastTarget = false;
            wtxt.text =
                "<color=#FF4444><b>WARNING:</b></color> An active save file was detected.\n\n" +
                "Starting a New Campaign will permanently reset your roster, equipped bio-modules, credits, and arena standing.\n\n" +
                "Are you sure you want to proceed?";

            // Confirm Button
            var confBtnObj = CreateStyledButtonWithText(cfdiag.transform, "ConfirmButton", "START NEW GAME (OVERWRITE)", new Color(0.65f, 0.18f, 0.18f, 1f), 48f, font, uisprite);
            var cfbrt = confBtnObj.GetComponent<RectTransform>();
            cfbrt.anchorMin = new Vector2(0.5f, 0f);
            cfbrt.anchorMax = new Vector2(0.5f, 0f);
            cfbrt.pivot = new Vector2(0.5f, 0f);
            cfbrt.anchoredPosition = new Vector2(-150f, 30f);
            cfbrt.sizeDelta = new Vector2(280f, 48f);
            so.FindProperty("confirmNewGameButton").objectReferenceValue = confBtnObj.GetComponent<Button>();

            // Cancel Button
            var cancelBtnObj = CreateStyledButtonWithText(cfdiag.transform, "CancelButton", "CANCEL", new Color(0.25f, 0.35f, 0.45f, 1f), 48f, font, uisprite);
            var canbrt = cancelBtnObj.GetComponent<RectTransform>();
            canbrt.anchorMin = new Vector2(0.5f, 0f);
            canbrt.anchorMax = new Vector2(0.5f, 0f);
            canbrt.pivot = new Vector2(0.5f, 0f);
            canbrt.anchoredPosition = new Vector2(150f, 30f);
            canbrt.sizeDelta = new Vector2(240f, 48f);
            so.FindProperty("cancelNewGameButton").objectReferenceValue = cancelBtnObj.GetComponent<Button>();
        }

        private static GameObject CreateHeaderText(Transform parent, string name, string text, Font font, Vector2 pos, int fontSize)
        {
            var titleObj = new GameObject(name);
            titleObj.transform.SetParent(parent, false);
            var trt = titleObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = pos;
            trt.sizeDelta = new Vector2(-40f, 50f);

            var txt = titleObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.2f, 0.85f, 1f, 1f);
            txt.text = text;
            txt.raycastTarget = false;

            var outl = titleObj.AddComponent<Outline>();
            outl.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outl.effectDistance = new Vector2(2f, -2f);

            return titleObj;
        }

        private static GameObject CreateStyledButton(Transform parent, string name, Color color, float height, Sprite uisprite)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(480f, height);

            var img = btnObj.AddComponent<Image>();
            img.sprite = uisprite;
            img.type = Image.Type.Sliced;
            img.color = color;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.3f;
            colors.pressedColor = color * 0.75f;
            colors.disabledColor = new Color(0.2f, 0.22f, 0.26f, 0.6f);
            btn.colors = colors;

            return btnObj;
        }

        private static GameObject CreateStyledButtonWithText(Transform parent, string name, string label, Color color, float height, Font font, Sprite uisprite)
        {
            var btnObj = CreateStyledButton(parent, name, color, height, uisprite);

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(20f, 0f);
            trt.offsetMax = new Vector2(-20f, 0f);

            var txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 21;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;
            txt.raycastTarget = false;

            var outl = textObj.AddComponent<Outline>();
            outl.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            return btnObj;
        }

        public static void RegisterSceneInBuildSettings(string scenePath)
        {
            var currentScenes = EditorBuildSettings.scenes;
            var sceneList = new List<EditorBuildSettingsScene>();

            // Ensure MainMenu is at index 0
            sceneList.Add(new EditorBuildSettingsScene(scenePath, true));

            foreach (var s in currentScenes)
            {
                if (s.path != scenePath)
                {
                    sceneList.Add(s);
                }
            }

            EditorBuildSettings.scenes = sceneList.ToArray();
            Debug.Log($"<color=#00FFAA>[MainMenuSceneBuilder]</color> Registered {scenePath} at Build Settings index 0! Total scenes in build: {sceneList.Count}");
        }
    }
}
