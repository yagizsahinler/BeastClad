using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using BeastClad.Arena;
using BeastClad.Combat;
using BeastClad.Data;
using BeastClad.Interaction;
using BeastClad.Player;
using BeastClad.Security;
using BeastClad.UI;
using BeastClad.World;

namespace BeastClad.Editor
{
    /// <summary>
    /// Master visual builder for the Grand Colosseum scene (Arena_Colosseum.unity).
    /// Constructs a cyber-Roman amphitheater with tiered spectator grandstands,
    /// an expansive golden sand fighting ring, corner floodlight towers,
    /// Aegis Corporate showroom pavilion, and full tournament bout wiring.
    /// </summary>
    public static class ArenaColosseumSceneBuilder
    {
        [MenuItem("BeastClad/Build Arena Colosseum Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/DefaultVolumeProfile.asset");

            // ==========================================
            // 1. CAMERA & POST-PROCESSING
            // ==========================================
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.04f, 1f); // Deep amphitheater obsidian
            camObj.AddComponent<AudioListener>();

            var uacd = camObj.AddComponent<UniversalAdditionalCameraData>();
            uacd.renderPostProcessing = true;

            var camFollow = camObj.AddComponent<CameraFollow2D>();
            camFollow.SetBounds(new Vector2(-16.0f, -11.0f), new Vector2(16.0f, 11.0f));

            if (volumeProfile != null)
            {
                var volObj = new GameObject("Global Volume");
                var vol = volObj.AddComponent<Volume>();
                vol.isGlobal = true;
                vol.profile = volumeProfile;
            }

            // ==========================================
            // 2. 2D LIGHTING (GRAND ARENA AMBIENCE)
            // ==========================================
            var globalLightObj = new GameObject("Global Light 2D");
            var globalLight = globalLightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.95f, 0.88f, 0.78f, 1f); // Warm Roman afternoon sunlight
            globalLight.intensity = 0.85f;

            // 4 Corner Arena Floodlights
            CreatePointLight("Floodlight_NW", new Vector3(-8.5f, 5.5f, 0f), new Color(1f, 0.92f, 0.75f, 1f), 3.0f, 12.0f, 1.8f);
            CreatePointLight("Floodlight_NE", new Vector3(8.5f, 5.5f, 0f), new Color(1f, 0.92f, 0.75f, 1f), 3.0f, 12.0f, 1.8f);
            CreatePointLight("Floodlight_SW", new Vector3(-8.5f, -5.5f, 0f), new Color(1f, 0.92f, 0.75f, 1f), 3.0f, 12.0f, 1.8f);
            CreatePointLight("Floodlight_SE", new Vector3(8.5f, -5.5f, 0f), new Color(1f, 0.92f, 0.75f, 1f), 3.0f, 12.0f, 1.8f);

            // Center Ring Golden Spotlight
            CreatePointLight("Light_CenterRing", new Vector3(2.0f, 0f, 0f), new Color(1f, 0.85f, 0.4f, 1f), 4.0f, 14.0f, 1.6f);

            // ==========================================
            // 3. ENVIRONMENT & GRANDSTAND ARCHITECTURE
            // ==========================================
            var envRoot = new GameObject("Environment");

            // Massive Amphitheater Foundation
            CreateSpriteObject(envRoot.transform, "AmphitheaterBase", uisprite,
                Vector3.zero, new Vector2(40f, 26f),
                new Color(0.08f, 0.07f, 0.06f, 1f), -20);

            // Tiered Spectator Grandstands (North & South)
            CreateSpriteObject(envRoot.transform, "Grandstand_North_Tier3", uisprite,
                new Vector3(0f, 9.8f, 0f), new Vector2(36f, 3.2f),
                new Color(0.14f, 0.12f, 0.10f, 1f), -18);
            CreateSpriteObject(envRoot.transform, "Grandstand_North_Tier2", uisprite,
                new Vector3(0f, 8.2f, 0f), new Vector2(34f, 2.0f),
                new Color(0.18f, 0.15f, 0.12f, 1f), -17);
            CreateSpriteObject(envRoot.transform, "Grandstand_North_Tier1", uisprite,
                new Vector3(0f, 7.0f, 0f), new Vector2(32f, 1.6f),
                new Color(0.22f, 0.18f, 0.14f, 1f), -16);

            CreateSpriteObject(envRoot.transform, "Grandstand_South_Tier1", uisprite,
                new Vector3(0f, -7.0f, 0f), new Vector2(32f, 1.6f),
                new Color(0.22f, 0.18f, 0.14f, 1f), -16);
            CreateSpriteObject(envRoot.transform, "Grandstand_South_Tier2", uisprite,
                new Vector3(0f, -8.2f, 0f), new Vector2(34f, 2.0f),
                new Color(0.18f, 0.15f, 0.12f, 1f), -17);
            CreateSpriteObject(envRoot.transform, "Grandstand_South_Tier3", uisprite,
                new Vector3(0f, -9.8f, 0f), new Vector2(36f, 3.2f),
                new Color(0.14f, 0.12f, 0.10f, 1f), -18);

            // Spectator Railings & Gold Trim Lines
            CreateSpriteObject(envRoot.transform, "Railing_North", uisprite,
                new Vector3(0f, 6.2f, 0f), new Vector2(30f, 0.25f),
                new Color(0.95f, 0.75f, 0.2f, 1f), -14);
            CreateSpriteObject(envRoot.transform, "Railing_South", uisprite,
                new Vector3(0f, -6.2f, 0f), new Vector2(30f, 0.25f),
                new Color(0.95f, 0.75f, 0.2f, 1f), -14);

            // Digital Crowd Silhouettes & Sponsor Billboards
            CreateWorldSign(envRoot.transform, "Sponsor_AegisBanner", new Vector3(0f, 8.8f, 0f),
                "AEGIS-FAUNA CORPORATION // SANCTIONED ARENA TOURNAMENT", new Color(0.2f, 0.9f, 1f, 1f),
                "DIVISION BRONZE • AUTHORIZED BIOMETRICS ONLY • PURSE: 500 CR", new Color(1f, 0.85f, 0.2f, 1f));

            // Staging Concourse Floor (West Side)
            CreateSpriteObject(envRoot.transform, "ConcourseFloor", uisprite,
                new Vector3(-9.5f, 0f, 0f), new Vector2(10f, 12f),
                new Color(0.15f, 0.17f, 0.22f, 1f), -15);
            CreateSpriteObject(envRoot.transform, "ConcourseBorder", uisprite,
                new Vector3(-9.5f, 0f, 0f), new Vector2(10.2f, 12.2f),
                new Color(0.2f, 0.75f, 0.95f, 1f), -16);

            // ==========================================
            // 4. CENTRAL GRAND COMBAT RING
            // ==========================================
            var arenaRingRoot = new GameObject("GrandCombatRing");
            arenaRingRoot.transform.SetParent(envRoot.transform);
            arenaRingRoot.transform.position = new Vector3(3.0f, 0f, 0f);

            // Golden Arena Sand Bed
            CreateSpriteObject(arenaRingRoot.transform, "SandArenaFloor", uisprite,
                Vector3.zero, new Vector2(14f, 10.5f),
                new Color(0.68f, 0.48f, 0.24f, 1f), -14);

            // Inner Gladiator Circle
            CreateSpriteObject(arenaRingRoot.transform, "InnerSandCircle", knob,
                Vector3.zero, new Vector2(8.5f, 8.5f),
                new Color(0.78f, 0.56f, 0.28f, 1f), -13);

            // Core Sigil Ring
            CreateSpriteObject(arenaRingRoot.transform, "GladiatorCoreRing", knob,
                Vector3.zero, new Vector2(4.5f, 4.5f),
                new Color(0.55f, 0.38f, 0.18f, 1f), -12);

            // Red Combat Perimeter Border
            CreateSpriteObject(arenaRingRoot.transform, "CombatPerimeterHazard", uisprite,
                Vector3.zero, new Vector2(14.4f, 10.9f),
                new Color(0.85f, 0.15f, 0.15f, 1f), -15);

            // Physical Arena Collision Enclosure
            var boundsObj = new GameObject("ArenaCollisionBounds");
            boundsObj.transform.SetParent(arenaRingRoot.transform);
            boundsObj.transform.localPosition = Vector3.zero;

            CreateVisibleWall(boundsObj.transform, "Wall_North", new Vector2(0f, 5.5f), new Vector2(14.8f, 0.8f), uisprite, new Color(0.35f, 0.25f, 0.15f, 1f));
            CreateVisibleWall(boundsObj.transform, "Wall_South", new Vector2(0f, -5.5f), new Vector2(14.8f, 0.8f), uisprite, new Color(0.35f, 0.25f, 0.15f, 1f));
            CreateVisibleWall(boundsObj.transform, "Wall_East", new Vector2(7.2f, 0f), new Vector2(0.8f, 11.8f), uisprite, new Color(0.35f, 0.25f, 0.15f, 1f));
            CreateVisibleWall(boundsObj.transform, "Wall_West", new Vector2(-7.2f, 0f), new Vector2(0.8f, 11.8f), uisprite, new Color(0.35f, 0.25f, 0.15f, 1f));

            // ==========================================
            // 5. AEGIS CORPORATE COMMERCE KIOSK
            // ==========================================
            var kioskObj = new GameObject("CorporateKiosk_Aegis");
            kioskObj.transform.SetParent(envRoot.transform);
            kioskObj.transform.position = new Vector3(-10.5f, 3.5f, 0f);

            CreateSpriteObject(kioskObj.transform, "KioskCounter", uisprite,
                Vector3.zero, new Vector2(4.2f, 2.2f),
                new Color(0.12f, 0.16f, 0.22f, 1f), 2);
            CreateSpriteObject(kioskObj.transform, "KioskBorder", uisprite,
                Vector3.zero, new Vector2(4.5f, 2.5f),
                new Color(0.1f, 0.9f, 1f, 1f), 1);

            CreateWorldSign(kioskObj.transform, "KioskSign", new Vector3(0f, 1.6f, 0f),
                "AEGIS BIOMETRIC COMMERCE KIOSK", new Color(0.1f, 0.95f, 1f, 1f),
                "[ F ] Access Sanctioned Starter Packs", new Color(0.85f, 0.95f, 1f, 1f));

            var kioskCol = kioskObj.AddComponent<BoxCollider2D>();
            kioskCol.isTrigger = true;
            kioskCol.size = new Vector2(4.5f, 2.5f);
            kioskObj.AddComponent<CorporateVendorKiosk>();

            // ==========================================
            // 6. MUNICIPAL CHECKPOINT ARCHWAY & ENFORCER
            // ==========================================
            var archObj = new GameObject("MunicipalScanner_Archway");
            archObj.transform.SetParent(envRoot.transform);
            archObj.transform.position = new Vector3(-4.5f, 0f, 0f);

            CreateSpriteObject(archObj.transform, "ArchFrame", uisprite,
                Vector3.zero, new Vector2(1.5f, 5.0f),
                new Color(0.25f, 0.22f, 0.15f, 1f), 1);
            CreateSpriteObject(archObj.transform, "ArchBorder", uisprite,
                Vector3.zero, new Vector2(1.7f, 5.2f),
                new Color(1f, 0.85f, 0.2f, 1f), 0);

            var beamObj = new GameObject("ScanBeam");
            beamObj.transform.SetParent(archObj.transform, false);
            var beamSr = beamObj.AddComponent<SpriteRenderer>();
            beamSr.sprite = uisprite;
            beamSr.drawMode = SpriteDrawMode.Sliced;
            beamSr.size = new Vector2(1.2f, 4.6f);
            beamSr.color = new Color(0f, 1f, 0.5f, 0.65f); // Cleared emerald inside Colosseum
            beamSr.sortingOrder = 2;

            var scanCol = archObj.AddComponent<BoxCollider2D>();
            scanCol.isTrigger = true;
            scanCol.size = new Vector2(1.5f, 5.0f);

            var scannerZone = archObj.AddComponent<MunicipalScannerZone>();

            // Stationed Officer Vance
            var enfObj = new GameObject("MunicipalEnforcer_OfficerVance");
            enfObj.transform.SetParent(envRoot.transform);
            enfObj.transform.position = new Vector3(-4.5f, 3.2f, 0f);
            var enfSr = enfObj.AddComponent<SpriteRenderer>();
            enfSr.sprite = knob;
            enfSr.color = new Color(0.2f, 0.55f, 0.95f, 1f);
            enfObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            enfSr.sortingOrder = 4;

            var enfCol = enfObj.AddComponent<CircleCollider2D>();
            enfCol.radius = 0.8f / 6.5f;

            var enfAI = enfObj.AddComponent<MunicipalEnforcerAI2D>();
            var enfSO = new SerializedObject(enfAI);
            enfSO.FindProperty("enforcerName").stringValue = "Officer Vance";
            enfSO.FindProperty("unitBadge").stringValue = "Aegis-Arena-Patrol 202";
            enfSO.FindProperty("enforcerRenderer").objectReferenceValue = enfSr;
            enfSO.ApplyModifiedProperties();

            var scanSO = new SerializedObject(scannerZone);
            scanSO.FindProperty("checkpointName").stringValue = "Colosseum Internal Security Gate";
            scanSO.FindProperty("sectorName").stringValue = "Grand Colosseum Ring";
            scanSO.FindProperty("beamRenderer").objectReferenceValue = beamSr;
            scanSO.FindProperty("stationedEnforcer").objectReferenceValue = enfAI;
            scanSO.ApplyModifiedProperties();

            // ==========================================
            // 7. EXIT PORTAL TO CENTRAL DISTRICT HUB
            // ==========================================
            var exitObj = new GameObject("Exit_ToCentralHub");
            exitObj.transform.SetParent(envRoot.transform);
            exitObj.transform.position = new Vector3(-10.5f, -3.5f, 0f);

            CreateSpriteObject(exitObj.transform, "ExitFrame", uisprite,
                Vector3.zero, new Vector2(4.5f, 2.5f),
                new Color(0.25f, 0.20f, 0.12f, 0.95f), 1);
            CreateSpriteObject(exitObj.transform, "ExitBorder", uisprite,
                Vector3.zero, new Vector2(4.8f, 2.8f),
                new Color(1f, 0.85f, 0.2f, 1f), 0);

            var exitCol = exitObj.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(4.5f, 2.5f);

            var exitTrig = exitObj.AddComponent<SceneTransitionTrigger>();
            var tso = new SerializedObject(exitTrig);
            tso.FindProperty("targetSceneName").stringValue = "District_CentralHub";
            tso.FindProperty("targetSpawnTag").stringValue = "Spawn_FromArena";
            tso.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            tso.FindProperty("promptText").stringValue = "[ F ] Return to Central Metro District";
            tso.ApplyModifiedProperties();

            CreateWorldSign(exitObj.transform, "Sign_ExitHub", new Vector3(0f, 0.5f, 0f),
                "⬅ GATES TO CENTRAL METRO DISTRICT", new Color(1f, 0.85f, 0.2f, 1f),
                "[ F ] Depart Arena Concourse", new Color(0.85f, 0.95f, 1f, 1f));

            // Spawn Point from Hub
            var spawnObj = new GameObject("Spawn_FromHub");
            spawnObj.transform.SetParent(envRoot.transform);
            spawnObj.transform.position = new Vector3(-7.8f, -1.5f, 0f);
            var sp = spawnObj.AddComponent<SceneSpawnPoint>();
            var spSO = new SerializedObject(sp);
            spSO.FindProperty("spawnTag").stringValue = "Spawn_FromHub";
            spSO.FindProperty("defaultFacing").vector2Value = Vector2.right;
            spSO.ApplyModifiedProperties();

            // ==========================================
            // 8. GLADIATOR VALERIUS & BOUT CONTROLLER
            // ==========================================
            var gladObj = new GameObject("Gladiator_Valerius");
            gladObj.transform.position = new Vector3(5.5f, 0f, 0f);

            var gladSr = gladObj.AddComponent<SpriteRenderer>();
            gladSr.sprite = knob;
            gladSr.color = new Color(0.95f, 0.45f, 0.15f, 1f); // Gladiator Crimson/Orange
            gladObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            gladSr.sortingOrder = 10;

            var gladCol = gladObj.AddComponent<CircleCollider2D>();
            gladCol.radius = 0.8f / 6.5f;

            gladObj.AddComponent<Hurtbox2D>();
            var gladRb = gladObj.AddComponent<Rigidbody2D>();
            gladRb.gravityScale = 0f;
            gladRb.freezeRotation = true;

            var gladAI = gladObj.AddComponent<SanctionedGladiatorAI2D>();
            var gladSO = new SerializedObject(gladAI);
            gladSO.FindProperty("gladiatorName").stringValue = "Valerius the Shock-Lancer";
            gladSO.FindProperty("corporateSponsor").stringValue = "Aegis-Fauna Corp (Bronze League)";
            gladSO.FindProperty("maxHealth").floatValue = 180f;
            gladSO.FindProperty("defense").floatValue = 5f;
            gladSO.FindProperty("moveSpeed").floatValue = 4.2f;
            gladSO.FindProperty("spriteRenderer").objectReferenceValue = gladSr;
            gladSO.ApplyModifiedProperties();

            // Arena Bout Controller
            var boutObj = new GameObject("ArenaBoutController");
            var boutCtrl = boutObj.AddComponent<ArenaBoutController>();

            // ==========================================
            // 9. PLAYER SETUP
            // ==========================================
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerObj;
            if (playerPrefab != null)
            {
                playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerObj.name = "Player";
                playerObj.tag = "Player";
                playerObj.transform.position = new Vector3(-0.5f, 0f, 0f);
            }
            else
            {
                playerObj = new GameObject("Player");
                playerObj.name = "Player";
                playerObj.tag = "Player";
                playerObj.transform.position = new Vector3(-0.5f, 0f, 0f);
                playerObj.AddComponent<SpriteRenderer>().sprite = knob;
                playerObj.AddComponent<Rigidbody2D>().gravityScale = 0f;
                playerObj.AddComponent<CapsuleCollider2D>();
                playerObj.AddComponent<PlayerController2D>();
                playerObj.AddComponent<PlayerStatsComponent>();
                playerObj.AddComponent<PlayerWallet>();
                playerObj.AddComponent<PlayerMonsterRoster>();
                playerObj.AddComponent<PlayerInfuseManager>();
                playerObj.AddComponent<CooldownController>();
            }

            camFollow.SetTarget(playerObj.transform);

            var pStats = playerObj.GetComponent<PlayerStatsComponent>();
            var pWallet = playerObj.GetComponent<PlayerWallet>();

            var boutSO = new SerializedObject(boutCtrl);
            boutSO.FindProperty("playerStats").objectReferenceValue = pStats;
            boutSO.FindProperty("playerWallet").objectReferenceValue = pWallet;
            boutSO.FindProperty("gladiator").objectReferenceValue = gladAI;
            boutSO.ApplyModifiedProperties();

            // ==========================================
            // 10. UI CANVAS & EVENTSYSTEM
            // ==========================================
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();

            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HUD_Canvas.prefab");
            GameObject canvasObj;
            if (hudPrefab != null)
            {
                canvasObj = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
                canvasObj.name = "HUD_Canvas";
            }
            else
            {
                canvasObj = new GameObject("HUD_Canvas");
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var cs = canvasObj.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasObj.AddComponent<LimbCooldownUI>();
            }

            // Wallet HUD UI (Top-Right)
            BuildWalletHUD(canvasObj.transform, defaultFont, uisprite);

            // Arena Match HUD UI (Boss bar, announcer banner, ladder rank)
            BuildArenaMatchUI(canvasObj.transform, boutCtrl, defaultFont, uisprite, bgSprite);

            // Corporate Vendor UI (Aegis Kiosk modal)
            BuildCorporateVendorUI(canvasObj.transform, defaultFont, uisprite, bgSprite);

            // Transition Prompt UI
            BuildSceneTransitionPromptHUD(canvasObj.transform, defaultFont, uisprite);

            // ==========================================
            // 11. SAVE SCENE
            // ==========================================
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Arena_Colosseum.unity");
            Debug.Log("<color=#00FFAA>[ArenaColosseumSceneBuilder]</color> Successfully built high-fidelity <b>Arena_Colosseum.unity</b>!");
        }

        private static GameObject CreatePointLight(string name, Vector3 pos, Color color, float innerR, float outerR, float intensity)
        {
            var lightObj = new GameObject(name);
            lightObj.transform.position = pos;
            var l = lightObj.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = color;
            l.pointLightInnerRadius = innerR;
            l.pointLightOuterRadius = outerR;
            l.intensity = intensity;
            return lightObj;
        }

        private static GameObject CreateSpriteObject(Transform parent, string name, Sprite sprite, Vector3 localPos, Vector2 worldSize, Color color, int sortingOrder)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            if (sprite != null && sprite.border.sqrMagnitude > 0.01f)
            {
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = worldSize;
                obj.transform.localScale = Vector3.one;
            }
            else if (sprite != null)
            {
                float unitW = sprite.rect.width / sprite.pixelsPerUnit;
                float unitH = sprite.rect.height / sprite.pixelsPerUnit;
                obj.transform.localScale = new Vector3(worldSize.x / unitW, worldSize.y / unitH, 1f);
            }
            else
            {
                obj.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            }
            return obj;
        }

        private static void CreateVisibleWall(Transform parent, string name, Vector2 localPos, Vector2 size, Sprite uisprite, Color wallColor)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = localPos;

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = wallColor;
            sr.sortingOrder = 3;
            wall.transform.localScale = Vector3.one;

            var hazard = new GameObject(name + "_HazardStripe");
            hazard.transform.SetParent(wall.transform, false);
            var hsr = hazard.AddComponent<SpriteRenderer>();
            hsr.sprite = uisprite;
            hsr.drawMode = SpriteDrawMode.Sliced;
            float hazardThickness = Mathf.Min(0.25f, size.y * 0.25f);
            hsr.size = new Vector2(size.x * 0.98f, hazardThickness);
            hsr.color = new Color(0.95f, 0.75f, 0.05f, 1f);
            hsr.sortingOrder = 4;
            hazard.transform.localPosition = new Vector3(0f, -(size.y - hazardThickness) * 0.5f, 0f);
        }

        private static void CreateWorldSign(Transform parent, string name, Vector3 localPos, string mainText, Color mainColor, string subText = null, Color? subColor = null)
        {
            var signObj = new GameObject(name);
            signObj.transform.SetParent(parent, false);
            signObj.transform.localPosition = localPos;

            var tm = signObj.AddComponent<TextMesh>();
            tm.text = mainText;
            tm.fontSize = 48;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = mainColor;
            tm.fontStyle = FontStyle.Bold;

            var mr = signObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 15;

            if (!string.IsNullOrEmpty(subText))
            {
                var subObj = new GameObject(name + "_Sub");
                subObj.transform.SetParent(signObj.transform, false);
                subObj.transform.localPosition = new Vector3(0f, -0.48f, 0f);

                var stm = subObj.AddComponent<TextMesh>();
                stm.text = subText;
                stm.fontSize = 36;
                stm.characterSize = 0.06f;
                stm.anchor = TextAnchor.MiddleCenter;
                stm.alignment = TextAlignment.Center;
                stm.color = subColor ?? Color.white;
                stm.fontStyle = FontStyle.Italic;

                var smr = subObj.GetComponent<MeshRenderer>();
                if (smr != null) smr.sortingOrder = 15;
            }
        }

        private static void BuildWalletHUD(Transform canvasTransform, Font font, Sprite uisprite)
        {
            var walletRoot = new GameObject("WalletHUD");
            walletRoot.transform.SetParent(canvasTransform, false);
            var rt = walletRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -30f);
            rt.sizeDelta = new Vector2(280f, 60f);

            var bgImg = walletRoot.AddComponent<Image>();
            bgImg.sprite = uisprite;
            bgImg.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            bgImg.raycastTarget = false;

            var textObj = new GameObject("WalletText");
            textObj.transform.SetParent(walletRoot.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.85f, 0.2f, 1f);
            txt.text = "<b>CREDITS:</b> <color=#FFD700>1,000 CR</color>";
            txt.raycastTarget = false;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            var walletUI = walletRoot.AddComponent<WalletHUDUI>();
            var wso = new SerializedObject(walletUI);
            wso.FindProperty("creditsText").objectReferenceValue = txt;
            wso.ApplyModifiedProperties();
        }

        private static void BuildArenaMatchUI(Transform canvasTransform, ArenaBoutController boutCtrl, Font font, Sprite uisprite, Sprite bgSprite)
        {
            var matchRoot = new GameObject("ArenaMatchUI");
            matchRoot.transform.SetParent(canvasTransform, false);
            var mrt = matchRoot.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.sizeDelta = Vector2.zero;

            var matchUI = matchRoot.AddComponent<ArenaMatchUI>();

            // 1. Opponent Boss Health Bar (Top Center)
            var bossBarObj = new GameObject("OpponentBossBar");
            bossBarObj.transform.SetParent(matchRoot.transform, false);
            var brt = bossBarObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 1f);
            brt.anchorMax = new Vector2(0.5f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0f, -35f);
            brt.sizeDelta = new Vector2(720f, 55f);

            var bbg = bossBarObj.AddComponent<Image>();
            bbg.sprite = uisprite;
            bbg.color = new Color(0.12f, 0.04f, 0.04f, 0.95f);

            // Health Fill
            var fillObj = new GameObject("HealthFill");
            fillObj.transform.SetParent(bossBarObj.transform, false);
            var frt = fillObj.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.sizeDelta = Vector2.zero;

            var fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = uisprite;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;
            fillImg.color = new Color(0.92f, 0.22f, 0.22f, 1f); // Crimson Health Fill

            // Opponent Name Text
            var nameTxt = CreateText(bossBarObj.transform, "OpponentName", font, 24, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.03f, 0.45f), new Vector2(0.65f, 0.95f), Vector2.zero, Vector2.zero,
                Color.white, "Valerius the Shock-Lancer");

            // Opponent Sponsor Text
            var sponsorTxt = CreateText(bossBarObj.transform, "OpponentSponsor", font, 18, FontStyle.Italic, TextAnchor.MiddleLeft,
                new Vector2(0.03f, 0.05f), new Vector2(0.65f, 0.50f), Vector2.zero, Vector2.zero,
                new Color(0.85f, 0.85f, 0.9f, 1f), "Aegis-Fauna Corp (Bronze League)");

            // Opponent Health Number Text
            var numTxt = CreateText(bossBarObj.transform, "HealthNumber", font, 22, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.65f, 0f), new Vector2(0.97f, 1f), Vector2.zero, Vector2.zero,
                new Color(1f, 0.9f, 0.3f, 1f), "180 / 180 HP");

            // 2. Central Announcer Banner
            var bannerObj = new GameObject("AnnouncerBanner");
            bannerObj.transform.SetParent(matchRoot.transform, false);
            var art = bannerObj.AddComponent<RectTransform>();
            art.anchorMin = new Vector2(0.5f, 0.5f);
            art.anchorMax = new Vector2(0.5f, 0.5f);
            art.pivot = new Vector2(0.5f, 0.5f);
            art.anchoredPosition = new Vector2(0f, 60f);
            art.sizeDelta = new Vector2(800f, 120f);

            var abg = bannerObj.AddComponent<Image>();
            abg.sprite = uisprite;
            abg.color = new Color(0.08f, 0.05f, 0.02f, 0.95f);

            var mainTxt = CreateText(bannerObj.transform, "AnnouncerMain", font, 38, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0f, 0.35f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero,
                new Color(1f, 0.85f, 0.2f, 1f), "SANCTIONED BOUT");

            var subAnnounceTxt = CreateText(bannerObj.transform, "AnnouncerSub", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0f, 0f), new Vector2(1f, 0.45f), Vector2.zero, Vector2.zero,
                Color.white, "DEFEAT YOUR OPPONENT");

            // 3. Division Ladder Badge (Top Left)
            var ladderObj = new GameObject("DivisionLadderBadge");
            ladderObj.transform.SetParent(matchRoot.transform, false);
            var lrt = ladderObj.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 1f);
            lrt.anchorMax = new Vector2(0f, 1f);
            lrt.pivot = new Vector2(0f, 1f);
            lrt.anchoredPosition = new Vector2(30f, -30f);
            lrt.sizeDelta = new Vector2(300f, 60f);

            var lbg = ladderObj.AddComponent<Image>();
            lbg.sprite = uisprite;
            lbg.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);

            var ladderTxt = CreateText(ladderObj.transform, "DivisionRankText", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(1f, 0.85f, 0.2f, 1f), "<b>DIVISION:</b> Bronze III");

            bannerObj.SetActive(false);

            var mso = new SerializedObject(matchUI);
            if (boutCtrl != null) mso.FindProperty("boutController").objectReferenceValue = boutCtrl;
            mso.FindProperty("opponentBarRoot").objectReferenceValue = bossBarObj;
            mso.FindProperty("opponentNameText").objectReferenceValue = nameTxt;
            mso.FindProperty("opponentSponsorText").objectReferenceValue = sponsorTxt;
            mso.FindProperty("opponentHealthFill").objectReferenceValue = fillImg;
            mso.FindProperty("opponentHealthNumberText").objectReferenceValue = numTxt;
            mso.FindProperty("announcerBannerRoot").objectReferenceValue = bannerObj;
            mso.FindProperty("announcerMainText").objectReferenceValue = mainTxt;
            mso.FindProperty("announcerSubText").objectReferenceValue = subAnnounceTxt;
            mso.FindProperty("divisionRankText").objectReferenceValue = ladderTxt;
            mso.ApplyModifiedProperties();
        }

        private static void BuildCorporateVendorUI(Transform canvasTransform, Font font, Sprite uisprite, Sprite bgSprite)
        {
            var managerObj = new GameObject("CorporateVendorManager");
            managerObj.transform.SetParent(canvasTransform, false);
            var mrt = managerObj.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.sizeDelta = Vector2.zero;

            var vendorUI = managerObj.AddComponent<CorporateVendorUI>();

            // 1. Interaction Prompt Panel
            var promptObj = new GameObject("KioskPromptPanel");
            promptObj.transform.SetParent(managerObj.transform, false);
            var prt = promptObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.16f);
            prt.anchorMax = new Vector2(0.5f, 0.16f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(580f, 60f);

            var pbg = promptObj.AddComponent<Image>();
            pbg.sprite = uisprite;
            pbg.color = new Color(0.06f, 0.10f, 0.18f, 0.94f);
            pbg.raycastTarget = false;

            var ptxt = CreateText(promptObj.transform, "PromptText", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.2f, 0.85f, 1f, 1f), "[ F ] Access Aegis Corporate Kiosk");

            // 2. Vendor Modal Panel
            var modalObj = new GameObject("VendorModalPanel");
            modalObj.transform.SetParent(managerObj.transform, false);
            var mort = modalObj.AddComponent<RectTransform>();
            mort.anchorMin = new Vector2(0.5f, 0.5f);
            mort.anchorMax = new Vector2(0.5f, 0.5f);
            mort.pivot = new Vector2(0.5f, 0.5f);
            mort.sizeDelta = new Vector2(920f, 640f);

            var mbg = modalObj.AddComponent<Image>();
            mbg.sprite = bgSprite != null ? bgSprite : uisprite;
            mbg.color = new Color(0.05f, 0.08f, 0.14f, 0.96f);

            // Header Title & Subtitle
            var titleTxt = CreateText(modalObj.transform, "Title", font, 32, FontStyle.Bold, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(800f, 45f),
                new Color(0.22f, 0.74f, 0.97f, 1f), "AEGIS BIO-INDUSTRIAL CORP");

            var subTxt = CreateText(modalObj.transform, "Subtitle", font, 20, FontStyle.Italic, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -75f), new Vector2(800f, 30f),
                new Color(0.65f, 0.85f, 1f, 1f), "Official Municipal League Vendor • Sanctioned Combat Specimens");

            // Product Title & Registration Badge
            var prodTitleTxt = CreateText(modalObj.transform, "PackTitle", font, 28, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-100f, -130f), new Vector2(550f, 40f),
                new Color(1f, 0.85f, 0.2f, 1f), "AEGIS STARTER PACK: TORRENT WYRM");

            var regBadgeTxt = CreateText(modalObj.transform, "RegistrationBadge", font, 22, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, -130f), new Vector2(260f, 40f),
                new Color(0.2f, 0.95f, 0.5f, 1f), "[ LEGAL SPECIMEN ]");

            // Description
            var descTxt = CreateText(modalObj.transform, "PackDescription", font, 20, FontStyle.Normal, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(800f, 80f),
                Color.white, "Fully registered municipal gladiator package with certified Hydro infusion matrices.");

            // Specs
            var specsTxt = CreateText(modalObj.transform, "SpeciesSpecs", font, 20, FontStyle.Normal, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -285f), new Vector2(800f, 90f),
                new Color(0.4f, 0.9f, 1f, 1f), "Hydro Element • Slot: Left Arm (Water Cleave) • Certified Safe");

            // Price & Balance
            var priceTxt = CreateText(modalObj.transform, "Price", font, 26, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-180f, 140f), new Vector2(350f, 40f),
                new Color(1f, 0.85f, 0.2f, 1f), "PRICE: 250 CR");

            var balTxt = CreateText(modalObj.transform, "Balance", font, 24, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(180f, 140f), new Vector2(350f, 40f),
                new Color(0f, 1f, 0.7f, 1f), "PURSE: 1,000 CR");

            var feedTxt = CreateText(modalObj.transform, "Feedback", font, 20, FontStyle.Italic, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(700f, 35f),
                Color.yellow, "");

            // Buttons
            var prevBtn = CreateButton(modalObj.transform, "Btn_Prev", font, "< PREV [Q]", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-260f, 50f), new Vector2(160f, 55f),
                new Color(0.18f, 0.22f, 0.3f, 1f));

            var buyBtn = CreateButton(modalObj.transform, "Btn_Buy", font, "PURCHASE & REGISTER [ENTER]", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(340f, 55f),
                new Color(0.1f, 0.55f, 0.85f, 1f));

            var nextBtn = CreateButton(modalObj.transform, "Btn_Next", font, "NEXT [E] >", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260f, 50f), new Vector2(160f, 55f),
                new Color(0.18f, 0.22f, 0.3f, 1f));

            var closeBtn = CreateButton(modalObj.transform, "Btn_Close", font, "X", uisprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-35f, -35f), new Vector2(48f, 48f),
                new Color(0.4f, 0.15f, 0.15f, 1f), 24);

            modalObj.SetActive(false);
            promptObj.SetActive(false);

            var vso = new SerializedObject(vendorUI);
            vso.FindProperty("interactionPromptPanel").objectReferenceValue = promptObj;
            vso.FindProperty("vendorModalPanel").objectReferenceValue = modalObj;
            vso.FindProperty("promptText").objectReferenceValue = ptxt;
            vso.FindProperty("kioskTitleText").objectReferenceValue = titleTxt;
            vso.FindProperty("kioskSubtitleText").objectReferenceValue = subTxt;
            vso.FindProperty("packTitleText").objectReferenceValue = prodTitleTxt;
            vso.FindProperty("registrationBadgeText").objectReferenceValue = regBadgeTxt;
            vso.FindProperty("packDescText").objectReferenceValue = descTxt;
            vso.FindProperty("speciesSpecsText").objectReferenceValue = specsTxt;
            vso.FindProperty("priceText").objectReferenceValue = priceTxt;
            vso.FindProperty("playerBalanceText").objectReferenceValue = balTxt;
            vso.FindProperty("transactionFeedbackText").objectReferenceValue = feedTxt;
            vso.FindProperty("purchaseButton").objectReferenceValue = buyBtn;
            vso.FindProperty("nextPackButton").objectReferenceValue = nextBtn;
            vso.FindProperty("prevPackButton").objectReferenceValue = prevBtn;
            vso.FindProperty("closeButton").objectReferenceValue = closeBtn;
            vso.ApplyModifiedProperties();
        }

        private static void BuildSceneTransitionPromptHUD(Transform canvasTransform, Font font, Sprite uisprite)
        {
            var promptRoot = new GameObject("SceneTransitionPromptHUD");
            promptRoot.transform.SetParent(canvasTransform, false);
            var rt = promptRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 300f);
            rt.sizeDelta = new Vector2(850f, 70f);

            var bgImg = promptRoot.AddComponent<Image>();
            bgImg.sprite = uisprite;
            bgImg.color = new Color(0.05f, 0.08f, 0.14f, 0.95f);
            bgImg.raycastTarget = false;

            var textObj = new GameObject("PromptText");
            textObj.transform.SetParent(promptRoot.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.22f, 0.74f, 0.97f, 1f);
            txt.text = "[ F ] Return to Central Metro District";
            txt.raycastTarget = false;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            var promptUI = promptRoot.AddComponent<SceneTransitionPromptUI>();
            var pso = new SerializedObject(promptUI);
            pso.FindProperty("promptPanel").objectReferenceValue = promptRoot;
            pso.FindProperty("promptText").objectReferenceValue = txt;
            pso.ApplyModifiedProperties();
        }

        private static Text CreateText(Transform parent, string name, Font font, int size, FontStyle style, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color, string initialText)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var txt = obj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.alignment = align;
            txt.color = color;
            txt.text = initialText;
            txt.raycastTarget = false;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        private static Button CreateButton(Transform parent, string name, Font font, string label, Sprite sprite,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, int fontSize = 22)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;
            img.raycastTarget = true;

            var btn = obj.AddComponent<Button>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;
            txt.raycastTarget = false;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return btn;
        }
    }
}
