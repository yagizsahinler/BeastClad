using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using BeastClad.Combat;
using BeastClad.Data;
using BeastClad.Interaction;
using BeastClad.Player;
using BeastClad.Trapping;
using BeastClad.UI;
using BeastClad.World;

namespace BeastClad.Editor
{
    /// <summary>
    /// Master visual builder for the Outlands Wilderness scene (Adventurer_Wilderness.unity).
    /// Constructs a lush, atmospheric woodland expedition biome with multi-layered terrain,
    /// a vibrant Hydro elemental pond, clustered pine trees and mossy boulders,
    /// Guildmaster Vane's expedition camp with a glowing campfire, and roaming wild beasts.
    /// </summary>
    public static class WildernessSceneBuilder
    {
        [MenuItem("BeastClad/Build Wilderness Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/DefaultVolumeProfile.asset");

            MonsterDataSO wyrmData = AssetDatabase.LoadAssetAtPath<MonsterDataSO>("Assets/Data/Monsters/Monster_TorrentWyrm.asset");
            MonsterDataSO mantisData = AssetDatabase.LoadAssetAtPath<MonsterDataSO>("Assets/Data/Monsters/Monster_VoltMantis.asset");
            GuildContractSO contractData = AssetDatabase.LoadAssetAtPath<GuildContractSO>("Assets/Data/Contracts/Contract_TorrentWyrmBounty.asset");

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
            cam.backgroundColor = new Color(0.06f, 0.11f, 0.07f, 1f); // Deep forest canopy
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
            // 2. 2D LIGHTING (FOREST SUNLIGHT & CAMPFIRE)
            // ==========================================
            var globalLightObj = new GameObject("Global Light 2D");
            var globalLight = globalLightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.92f, 0.98f, 0.90f, 1f); // Dappled woodland daylight
            globalLight.intensity = 0.90f;

            // Campfire Warm Glow (North-East Camp)
            CreatePointLight("Light_Campfire", new Vector3(7.5f, 3.5f, 0f), new Color(1f, 0.65f, 0.15f, 1f), 2.0f, 9.0f, 2.2f);

            // Hydro Pond Cyan Bioluminescence (North-West Pond)
            CreatePointLight("Light_HydroPond", new Vector3(-6.5f, 3.5f, 0f), new Color(0.1f, 0.75f, 1f, 1f), 3.0f, 10.0f, 1.6f);

            // Southern Outlands Gate Light
            CreatePointLight("Light_SouthGate", new Vector3(0f, -5.5f, 0f), new Color(0.2f, 0.95f, 0.45f, 1f), 2.5f, 8.5f, 1.8f);

            // ==========================================
            // 3. MULTI-LAYERED WOODLAND TERRAIN
            // ==========================================
            var envRoot = new GameObject("Environment");

            // Base Woodland Earth
            CreateSpriteObject(envRoot.transform, "WoodlandBasePlate", uisprite,
                Vector3.zero, new Vector2(40f, 26f),
                new Color(0.09f, 0.16f, 0.09f, 1f), -20);

            // Lush Meadow Grass Patches
            CreateSpriteObject(envRoot.transform, "Meadow_Main", uisprite,
                new Vector3(0f, 0f, 0f), new Vector2(34f, 20f),
                new Color(0.14f, 0.26f, 0.14f, 1f), -18);
            CreateSpriteObject(envRoot.transform, "Meadow_NorthWest", knob,
                new Vector3(-6.5f, 3.5f, 0f), new Vector2(16f, 13f),
                new Color(0.12f, 0.22f, 0.12f, 1f), -17);
            CreateSpriteObject(envRoot.transform, "Meadow_SouthEast", knob,
                new Vector3(7.0f, -3.0f, 0f), new Vector2(15f, 12f),
                new Color(0.16f, 0.28f, 0.15f, 1f), -17);

            // Winding Packed-Soil Expedition Trails
            // South-to-Center Main Path
            CreateSpriteObject(envRoot.transform, "Trail_SouthToCenter", uisprite,
                new Vector3(0f, -2.5f, 0f), new Vector2(4.5f, 8.0f),
                new Color(0.42f, 0.28f, 0.16f, 1f), -15);
            // East-West Transverse Trail
            CreateSpriteObject(envRoot.transform, "Trail_EastWest", uisprite,
                new Vector3(0f, 0.5f, 0f), new Vector2(28f, 3.8f),
                new Color(0.40f, 0.27f, 0.15f, 1f), -15);
            // North-East Path to Guild Camp
            CreateSpriteObject(envRoot.transform, "Trail_ToCamp", uisprite,
                new Vector3(6.5f, 2.5f, 0f), new Vector2(4.2f, 6.0f),
                new Color(0.38f, 0.25f, 0.14f, 1f), -15);
            // North-West Path to Hydro Basin
            CreateSpriteObject(envRoot.transform, "Trail_ToPond", uisprite,
                new Vector3(-6.0f, 2.5f, 0f), new Vector2(4.2f, 6.0f),
                new Color(0.38f, 0.25f, 0.14f, 1f), -15);

            // ==========================================
            // 4. ELEMENTAL HYDRO WATER BASIN (POND)
            // ==========================================
            var pondRoot = new GameObject("HydroElementalBasin");
            pondRoot.transform.SetParent(envRoot.transform);
            pondRoot.transform.position = new Vector3(-7.5f, 3.5f, 0f);

            // Shoreline Sand Trim
            CreateSpriteObject(pondRoot.transform, "PondShoreline", knob,
                Vector3.zero, new Vector2(10.5f, 7.5f),
                new Color(0.65f, 0.52f, 0.28f, 1f), -14);

            // Water Surface
            CreateSpriteObject(pondRoot.transform, "PondWater", knob,
                Vector3.zero, new Vector2(9.5f, 6.5f),
                new Color(0.08f, 0.45f, 0.72f, 0.92f), -13);

            // Water Ripple Core
            CreateSpriteObject(pondRoot.transform, "PondCoreRipple", knob,
                new Vector3(0.5f, 0f, 0f), new Vector2(5.5f, 3.5f),
                new Color(0.18f, 0.68f, 0.95f, 0.85f), -12);

            CreateWorldSign(pondRoot.transform, "Sign_HydroPond", new Vector3(0f, 4.2f, 0f),
                "TORRENT BASIN // HYDRO SPECIMEN HABITAT", new Color(0.2f, 0.85f, 1f, 1f),
                "[ TRAPPING PERMITTED • DEPLOY HYDRO PHEROMONE LURE ]", new Color(0.85f, 0.95f, 1f, 1f));

            // ==========================================
            // 5. TRAIL FLORA & MOSSY BOULDERS
            // ==========================================
            var floraRoot = new GameObject("ForestFloraAndBoulders");
            floraRoot.transform.SetParent(envRoot.transform);

            // Clustered Pine Trees with Colliders
            CreatePineTree(floraRoot.transform, "Tree_NW_1", new Vector3(-13.5f, 6.5f, 0f), uisprite, knob);
            CreatePineTree(floraRoot.transform, "Tree_NW_2", new Vector3(-11.5f, 8.0f, 0f), uisprite, knob);
            CreatePineTree(floraRoot.transform, "Tree_NW_3", new Vector3(-14.0f, 3.0f, 0f), uisprite, knob);

            CreatePineTree(floraRoot.transform, "Tree_NE_1", new Vector3(13.5f, 7.5f, 0f), uisprite, knob);
            CreatePineTree(floraRoot.transform, "Tree_NE_2", new Vector3(11.5f, 8.5f, 0f), uisprite, knob);

            CreatePineTree(floraRoot.transform, "Tree_SW_1", new Vector3(-13.0f, -5.5f, 0f), uisprite, knob);
            CreatePineTree(floraRoot.transform, "Tree_SW_2", new Vector3(-10.5f, -7.0f, 0f), uisprite, knob);

            CreatePineTree(floraRoot.transform, "Tree_SE_1", new Vector3(13.0f, -5.5f, 0f), uisprite, knob);
            CreatePineTree(floraRoot.transform, "Tree_SE_2", new Vector3(10.5f, -7.0f, 0f), uisprite, knob);

            // Mossy Granite Boulders
            CreateBoulder(floraRoot.transform, "Boulder_1", new Vector3(-2.5f, 3.8f, 0f), new Vector2(2.2f, 1.8f), knob);
            CreateBoulder(floraRoot.transform, "Boulder_2", new Vector3(2.5f, -2.5f, 0f), new Vector2(2.0f, 1.6f), knob);
            CreateBoulder(floraRoot.transform, "Boulder_3", new Vector3(-7.5f, -3.5f, 0f), new Vector2(2.4f, 1.9f), knob);
            CreateBoulder(floraRoot.transform, "Boulder_4", new Vector3(8.5f, -2.5f, 0f), new Vector2(2.1f, 1.7f), knob);

            // ==========================================
            // 6. TRAILBOUND PERIMETER ROCK WALLS
            // ==========================================
            var boundaryRoot = new GameObject("OutlandsPerimeterBounds");
            boundaryRoot.transform.SetParent(envRoot.transform);

            CreatePerimeterWall(boundaryRoot.transform, "Boundary_North", new Vector2(0f, 10.5f), new Vector2(36f, 1.5f), uisprite);
            CreatePerimeterWall(boundaryRoot.transform, "Boundary_West", new Vector2(-16.5f, 0f), new Vector2(1.5f, 22f), uisprite);
            CreatePerimeterWall(boundaryRoot.transform, "Boundary_East", new Vector2(16.5f, 0f), new Vector2(1.5f, 22f), uisprite);
            CreatePerimeterWall(boundaryRoot.transform, "Boundary_South_Left", new Vector2(-9.5f, -9.5f), new Vector2(15f, 1.5f), uisprite);
            CreatePerimeterWall(boundaryRoot.transform, "Boundary_South_Right", new Vector2(9.5f, -9.5f), new Vector2(15f, 1.5f), uisprite);

            // ==========================================
            // 7. TRAVELLERS' GUILD EXPEDITION CAMP
            // ==========================================
            var campRoot = new GameObject("TrappersGuildExpeditionCamp");
            campRoot.transform.SetParent(envRoot.transform);
            campRoot.transform.position = new Vector3(8.5f, 3.8f, 0f);

            // Camp Ground Tarp
            CreateSpriteObject(campRoot.transform, "CampTarp", uisprite,
                Vector3.zero, new Vector2(7.5f, 5.5f),
                new Color(0.35f, 0.28f, 0.18f, 1f), -14);
            CreateSpriteObject(campRoot.transform, "CampTarpBorder", uisprite,
                Vector3.zero, new Vector2(7.8f, 5.8f),
                new Color(0.85f, 0.65f, 0.25f, 1f), -15);

            // Canvas Expedition Tent
            CreateSpriteObject(campRoot.transform, "ExpeditionTent", uisprite,
                new Vector3(1.8f, 1.0f, 0f), new Vector2(3.2f, 2.6f),
                new Color(0.55f, 0.38f, 0.18f, 1f), 2);
            CreateSpriteObject(campRoot.transform, "TentFlap", uisprite,
                new Vector3(1.8f, 0.3f, 0f), new Vector2(1.2f, 1.2f),
                new Color(0.25f, 0.18f, 0.10f, 1f), 3);

            // Stone Campfire
            var fireObj = CreateSpriteObject(campRoot.transform, "CampfireRing", knob,
                new Vector3(-1.2f, -0.6f, 0f), new Vector2(1.5f, 1.5f),
                new Color(0.35f, 0.35f, 0.35f, 1f), 1);
            CreateSpriteObject(fireObj.transform, "FireEmbers", knob,
                Vector3.zero, new Vector2(0.9f, 0.9f),
                new Color(1f, 0.45f, 0.1f, 1f), 2);

            // Wooden Supply Crates & Sacks
            CreateSpriteObject(campRoot.transform, "SupplyCrate_1", uisprite,
                new Vector3(-1.8f, 1.2f, 0f), new Vector2(1.2f, 1.2f),
                new Color(0.48f, 0.32f, 0.16f, 1f), 2);
            CreateSpriteObject(campRoot.transform, "SupplyCrate_2", uisprite,
                new Vector3(-0.8f, 1.4f, 0f), new Vector2(1.0f, 1.0f),
                new Color(0.42f, 0.28f, 0.14f, 1f), 2);

            CreateWorldSign(campRoot.transform, "Sign_GuildCamp", new Vector3(0f, 2.8f, 0f),
                "TRAVELLERS' GUILD EXPEDITION CAMP", new Color(1f, 0.85f, 0.25f, 1f),
                "[ TALK TO GUILDMASTER VANE • CONTRACTS & BOUNTIES ]", new Color(0.9f, 0.95f, 1f, 1f));

            // Guildmaster Vane NPC
            var npcObj = new GameObject("NPC_GuildmasterVane");
            npcObj.transform.SetParent(campRoot.transform);
            npcObj.transform.localPosition = new Vector3(-1.0f, 0.6f, 0f);

            var npcSr = npcObj.AddComponent<SpriteRenderer>();
            npcSr.sprite = knob;
            npcSr.color = new Color(0.2f, 0.85f, 0.45f, 1f); // Guild Hunter Emerald
            npcObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            npcSr.sortingOrder = 10;

            var npcCol = npcObj.AddComponent<CircleCollider2D>();
            npcCol.radius = 1.2f / 6.5f;
            npcCol.isTrigger = true;

            var npcComp = npcObj.AddComponent<InteractableNPC>();
            var npcSO = new SerializedObject(npcComp);
            npcSO.FindProperty("npcName").stringValue = "Guildmaster Vane";
            npcSO.FindProperty("roleTitle").stringValue = "Adventurer's Guild Representative";
            if (contractData != null)
            {
                npcSO.FindProperty("offeredContract").objectReferenceValue = contractData;
            }
            npcSO.ApplyModifiedProperties();

            // ==========================================
            // 8. SOUTHERN PORTAL TO CENTRAL DISTRICT HUB
            // ==========================================
            var exitObj = new GameObject("Exit_ToCentralHub");
            exitObj.transform.SetParent(envRoot.transform);
            exitObj.transform.position = new Vector3(0f, -7.5f, 0f);

            CreateSpriteObject(exitObj.transform, "GateFrame", uisprite,
                Vector3.zero, new Vector2(5.5f, 2.8f),
                new Color(0.18f, 0.26f, 0.20f, 0.95f), 1);
            CreateSpriteObject(exitObj.transform, "GateBorder", uisprite,
                Vector3.zero, new Vector2(5.8f, 3.1f),
                new Color(0.1f, 0.95f, 0.45f, 1f), 0);

            var exitCol = exitObj.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(5.5f, 2.8f);

            var exitTrig = exitObj.AddComponent<SceneTransitionTrigger>();
            var tso = new SerializedObject(exitTrig);
            tso.FindProperty("targetSceneName").stringValue = "District_CentralHub";
            tso.FindProperty("targetSpawnTag").stringValue = "Spawn_FromWilderness";
            tso.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            tso.FindProperty("promptText").stringValue = "[ F ] Return to Central Metro District";
            tso.ApplyModifiedProperties();

            CreateWorldSign(exitObj.transform, "Sign_ExitHub", new Vector3(0f, 0.6f, 0f),
                "⬇ GATES TO CENTRAL METRO DISTRICT", new Color(0.2f, 0.95f, 0.45f, 1f),
                "[ F ] Depart to Central Concourse", new Color(0.85f, 0.95f, 0.88f, 1f));

            // Spawn Point from Hub
            var spawnObj = new GameObject("Spawn_FromHub");
            spawnObj.transform.SetParent(envRoot.transform);
            spawnObj.transform.position = new Vector3(0f, -5.2f, 0f);
            var sp = spawnObj.AddComponent<SceneSpawnPoint>();
            var spSO = new SerializedObject(sp);
            spSO.FindProperty("spawnTag").stringValue = "Spawn_FromHub";
            spSO.FindProperty("defaultFacing").vector2Value = Vector2.up;
            spSO.ApplyModifiedProperties();

            // ==========================================
            // 9. WILD ROAMING BEASTS
            // ==========================================
            // Torrent Wyrm near the Hydro Pond
            var wyrmObj = new GameObject("Wild_TorrentWyrm");
            wyrmObj.transform.position = new Vector3(-5.5f, 2.8f, 0f);
            var wyrmSr = wyrmObj.AddComponent<SpriteRenderer>();
            wyrmSr.sprite = knob;
            wyrmSr.color = new Color(0.1f, 0.65f, 1f, 1f); // Hydro Cyan
            wyrmObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            wyrmSr.sortingOrder = 10;

            var wyrmCol = wyrmObj.AddComponent<CircleCollider2D>();
            wyrmCol.radius = 0.8f / 6.5f;

            wyrmObj.AddComponent<Hurtbox2D>();
            var wyrmRb = wyrmObj.AddComponent<Rigidbody2D>();
            wyrmRb.gravityScale = 0f;
            wyrmRb.freezeRotation = true;

            var wyrmAI = wyrmObj.AddComponent<WildMonsterAI2D>();
            var wyrmSO = new SerializedObject(wyrmAI);
            if (wyrmData != null) wyrmSO.FindProperty("monsterData").objectReferenceValue = wyrmData;
            wyrmSO.FindProperty("spriteRenderer").objectReferenceValue = wyrmSr;
            wyrmSO.FindProperty("wanderRadius").floatValue = 5.0f;
            wyrmSO.ApplyModifiedProperties();

            // Volt Mantis in South-East meadow clearing
            var mantisObj = new GameObject("Wild_VoltMantis");
            mantisObj.transform.position = new Vector3(6.0f, -2.5f, 0f);
            var mantisSr = mantisObj.AddComponent<SpriteRenderer>();
            mantisSr.sprite = knob;
            mantisSr.color = new Color(1f, 0.85f, 0.1f, 1f); // Volt Amber
            mantisObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            mantisSr.sortingOrder = 10;

            var mantisCol = mantisObj.AddComponent<CircleCollider2D>();
            mantisCol.radius = 0.8f / 6.5f;

            mantisObj.AddComponent<Hurtbox2D>();
            var mantisRb = mantisObj.AddComponent<Rigidbody2D>();
            mantisRb.gravityScale = 0f;
            mantisRb.freezeRotation = true;

            var mantisAI = mantisObj.AddComponent<WildMonsterAI2D>();
            var mantisSO = new SerializedObject(mantisAI);
            if (mantisData != null) mantisSO.FindProperty("monsterData").objectReferenceValue = mantisData;
            mantisSO.FindProperty("spriteRenderer").objectReferenceValue = mantisSr;
            mantisSO.FindProperty("wanderRadius").floatValue = 6.0f;
            mantisSO.ApplyModifiedProperties();

            // ==========================================
            // 10. PLAYER SETUP
            // ==========================================
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerObj;
            if (playerPrefab != null)
            {
                playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerObj.name = "Player";
                playerObj.tag = "Player";
                playerObj.transform.position = new Vector3(0f, -3.5f, 0f);
            }
            else
            {
                playerObj = new GameObject("Player");
                playerObj.name = "Player";
                playerObj.tag = "Player";
                playerObj.transform.position = new Vector3(0f, -3.5f, 0f);
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

            // Ensure PlayerTrapperComponent exists for Wilderness
            if (playerObj.GetComponent<PlayerTrapperComponent>() == null)
            {
                playerObj.AddComponent<PlayerTrapperComponent>();
            }

            camFollow.SetTarget(playerObj.transform);

            // ==========================================
            // 11. UI CANVAS & EVENTSYSTEM
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

            // Trapper Field HUD UI (Bottom-Center above Cooldown bar)
            BuildTrapperFieldHUD(canvasObj.transform, defaultFont, uisprite);

            // Guild Dialogue UI
            Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            BuildGuildDialogueHUD(canvasObj.transform, defaultFont, uisprite, bgSprite);

            // Scene Transition Prompt UI
            BuildSceneTransitionPromptHUD(canvasObj.transform, defaultFont, uisprite);

            // ==========================================
            // 12. SAVE SCENE
            // ==========================================
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Adventurer_Wilderness.unity");
            Debug.Log("<color=#00FFAA>[WildernessSceneBuilder]</color> Successfully built atmospheric <b>Adventurer_Wilderness.unity</b>!");
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

        private static void CreatePineTree(Transform parent, string name, Vector3 pos, Sprite uisprite, Sprite knob)
        {
            var tree = new GameObject(name);
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            // Trunk (order 1)
            var trunk = CreateSpriteObject(tree.transform, "Trunk", uisprite,
                new Vector3(0f, -0.4f, 0f), new Vector2(0.6f, 1.2f),
                new Color(0.35f, 0.22f, 0.12f, 1f), 1);

            // Trunk Collider
            var col = tree.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            // Foliage Tiers (order 2, 3, 4)
            CreateSpriteObject(tree.transform, "Crown_Bottom", knob,
                new Vector3(0f, 0.4f, 0f), new Vector2(2.6f, 2.2f),
                new Color(0.12f, 0.28f, 0.15f, 1f), 2);
            CreateSpriteObject(tree.transform, "Crown_Mid", knob,
                new Vector3(0f, 1.2f, 0f), new Vector2(2.1f, 1.8f),
                new Color(0.16f, 0.36f, 0.18f, 1f), 3);
            CreateSpriteObject(tree.transform, "Crown_Top", knob,
                new Vector3(0f, 1.9f, 0f), new Vector2(1.5f, 1.4f),
                new Color(0.20f, 0.44f, 0.22f, 1f), 4);
        }

        private static void CreateBoulder(Transform parent, string name, Vector3 pos, Vector2 size, Sprite knob)
        {
            var boulder = new GameObject(name);
            boulder.transform.SetParent(parent, false);
            boulder.transform.localPosition = pos;

            var sr = boulder.AddComponent<SpriteRenderer>();
            sr.sprite = knob;
            sr.color = new Color(0.30f, 0.34f, 0.38f, 1f); // Granite slate
            sr.sortingOrder = 2;

            float unitW = knob.rect.width / knob.pixelsPerUnit;
            float unitH = knob.rect.height / knob.pixelsPerUnit;
            boulder.transform.localScale = new Vector3(size.x / unitW, size.y / unitH, 1f);

            var col = boulder.AddComponent<CircleCollider2D>();
            col.radius = 0.8f / (size.x / unitW);

            // Moss cap
            var moss = new GameObject("MossCap");
            moss.transform.SetParent(boulder.transform, false);
            moss.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            var msr = moss.AddComponent<SpriteRenderer>();
            msr.sprite = knob;
            msr.color = new Color(0.15f, 0.45f, 0.20f, 0.85f);
            msr.sortingOrder = 3;
            moss.transform.localScale = new Vector3(0.85f, 0.65f, 1f);
        }

        private static void CreatePerimeterWall(Transform parent, string name, Vector2 pos, Vector2 size, Sprite uisprite)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = pos;

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = new Color(0.15f, 0.22f, 0.16f, 1f);
            sr.sortingOrder = 2;
            wall.transform.localScale = Vector3.one;

            var mossTrim = new GameObject(name + "_MossTrim");
            mossTrim.transform.SetParent(wall.transform, false);
            var msr = mossTrim.AddComponent<SpriteRenderer>();
            msr.sprite = uisprite;
            msr.drawMode = SpriteDrawMode.Sliced;
            float trimThickness = Mathf.Min(0.3f, size.y * 0.3f);
            msr.size = new Vector2(size.x * 0.98f, trimThickness);
            msr.color = new Color(0.18f, 0.45f, 0.22f, 1f);
            msr.sortingOrder = 3;
            mossTrim.transform.localPosition = new Vector3(0f, -(size.y - trimThickness) * 0.5f, 0f);
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
            bgImg.color = new Color(0.06f, 0.10f, 0.08f, 0.94f);
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

        private static void BuildTrapperFieldHUD(Transform canvasTransform, Font font, Sprite uisprite)
        {
            var trapperRoot = new GameObject("TrapperFieldHUD");
            trapperRoot.transform.SetParent(canvasTransform, false);
            var rt = trapperRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 195f);
            rt.sizeDelta = new Vector2(780f, 90f);

            var bgImg = trapperRoot.AddComponent<Image>();
            bgImg.sprite = uisprite;
            bgImg.color = new Color(0.05f, 0.10f, 0.06f, 0.94f);
            bgImg.raycastTarget = false;

            // Prompt text (top half)
            var pObj = new GameObject("PromptText");
            pObj.transform.SetParent(trapperRoot.transform, false);
            var prt = pObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.02f, 0.46f);
            prt.anchorMax = new Vector2(0.98f, 0.96f);
            prt.sizeDelta = Vector2.zero;

            var pTxt = pObj.AddComponent<Text>();
            pTxt.font = font;
            pTxt.fontSize = 24;
            pTxt.fontStyle = FontStyle.Bold;
            pTxt.alignment = TextAnchor.MiddleCenter;
            pTxt.color = new Color(0.29f, 0.87f, 0.50f, 1f); // #4ADE80
            pTxt.text = "[ 1 ] Deploy Lure   •   [ 2 ] Deploy Snare";
            pTxt.raycastTarget = false;
            pTxt.verticalOverflow = VerticalWrapMode.Overflow;

            var pOutline = pObj.AddComponent<Outline>();
            pOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            pOutline.effectDistance = new Vector2(2f, -2f);

            // Crate count text (bottom half)
            var cObj = new GameObject("CrateCountText");
            cObj.transform.SetParent(trapperRoot.transform, false);
            var crt = cObj.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.02f, 0.04f);
            crt.anchorMax = new Vector2(0.98f, 0.48f);
            crt.sizeDelta = Vector2.zero;

            var cTxt = cObj.AddComponent<Text>();
            cTxt.font = font;
            cTxt.fontSize = 20;
            cTxt.fontStyle = FontStyle.Bold;
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.color = new Color(0.99f, 0.83f, 0.30f, 1f); // #FCD34D
            cTxt.text = "Specimen Transport Crate: 0 captured";
            cTxt.raycastTarget = false;
            cTxt.verticalOverflow = VerticalWrapMode.Overflow;

            var cOutline = cObj.AddComponent<Outline>();
            cOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            cOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var trapperUI = trapperRoot.AddComponent<TrapperFieldUI>();
            var tso = new SerializedObject(trapperUI);
            tso.FindProperty("promptText").objectReferenceValue = pTxt;
            tso.FindProperty("crateCountText").objectReferenceValue = cTxt;
            tso.ApplyModifiedProperties();
        }

        private static void BuildGuildDialogueHUD(Transform canvasTransform, Font font, Sprite uisprite, Sprite bgSprite)
        {
            var managerObj = new GameObject("GuildDialogueManager");
            managerObj.transform.SetParent(canvasTransform, false);
            var mrt = managerObj.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.sizeDelta = Vector2.zero;

            var dialogueUI = managerObj.AddComponent<GuildDialogueUI>();

            // 1. Interaction Prompt Panel
            var promptObj = new GameObject("InteractionPromptPanel");
            promptObj.transform.SetParent(managerObj.transform, false);
            var prt = promptObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.16f);
            prt.anchorMax = new Vector2(0.5f, 0.16f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(580f, 60f);

            var pbg = promptObj.AddComponent<Image>();
            pbg.sprite = uisprite;
            pbg.color = new Color(0.05f, 0.12f, 0.08f, 0.94f);
            pbg.raycastTarget = false;

            var pText = CreateText(promptObj.transform, "PromptText", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.29f, 0.87f, 0.50f, 1f), "[ F ] Speak with Guildmaster Vane");

            // 2. Dialogue Modal Panel
            var modalObj = new GameObject("DialogueModalPanel");
            modalObj.transform.SetParent(managerObj.transform, false);
            var mort = modalObj.AddComponent<RectTransform>();
            mort.anchorMin = new Vector2(0.5f, 0.5f);
            mort.anchorMax = new Vector2(0.5f, 0.5f);
            mort.pivot = new Vector2(0.5f, 0.5f);
            mort.sizeDelta = new Vector2(920f, 640f);

            var mbg = modalObj.AddComponent<Image>();
            mbg.sprite = bgSprite != null ? bgSprite : uisprite;
            mbg.color = new Color(0.06f, 0.10f, 0.07f, 0.96f);

            // Speaker Name & Role
            var nameTxt = CreateText(modalObj.transform, "SpeakerNameText", font, 32, FontStyle.Bold, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(800f, 45f),
                new Color(0.29f, 0.87f, 0.50f, 1f), "Guildmaster Vane");

            var roleTxt = CreateText(modalObj.transform, "SpeakerRoleText", font, 20, FontStyle.Italic, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(800f, 30f),
                new Color(0.65f, 0.95f, 0.82f, 1f), "Adventurer's Guild Representative • Outlands Expedition");

            // Dialogue Body
            var bodyTxt = CreateText(modalObj.transform, "DialogueBodyText", font, 22, FontStyle.Normal, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(800f, 130f),
                Color.white, "Welcome to the Outlands, trapper. Hydro beasts frequent the torrent basin to the northwest.");

            // Objective Status
            var objTxt = CreateText(modalObj.transform, "ObjectiveStatusText", font, 24, FontStyle.Bold, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -320f), new Vector2(800f, 40f),
                new Color(1f, 0.85f, 0.25f, 1f), "Objective: 0/1 Torrent Wyrm captured");

            // Reward Summary
            var rewTxt = CreateText(modalObj.transform, "RewardSummaryText", font, 22, FontStyle.Bold, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -370f), new Vector2(800f, 35f),
                new Color(0.22f, 0.74f, 0.97f, 1f), "Reward: 750 Credits & Trapper Guild Commendation");

            // Footer Hint
            var footTxt = CreateText(modalObj.transform, "FooterHintText", font, 20, FontStyle.Italic, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(800f, 35f),
                new Color(0.6f, 0.7f, 0.75f, 1f), "[ F ] or [ Esc ] Close Dialogue");

            // Close button
            var closeBtn = CreateButton(modalObj.transform, "Btn_CloseDialogue", font, "X", uisprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-35f, -35f), new Vector2(48f, 48f),
                new Color(0.4f, 0.15f, 0.15f, 1f), 24);

            modalObj.SetActive(false);
            promptObj.SetActive(false);

            var dso = new SerializedObject(dialogueUI);
            dso.FindProperty("interactionPromptPanel").objectReferenceValue = promptObj;
            dso.FindProperty("dialogueModalPanel").objectReferenceValue = modalObj;
            dso.FindProperty("promptText").objectReferenceValue = pText;
            dso.FindProperty("speakerNameText").objectReferenceValue = nameTxt;
            dso.FindProperty("speakerRoleText").objectReferenceValue = roleTxt;
            dso.FindProperty("dialogueBodyText").objectReferenceValue = bodyTxt;
            dso.FindProperty("objectiveStatusText").objectReferenceValue = objTxt;
            dso.FindProperty("rewardSummaryText").objectReferenceValue = rewTxt;
            dso.FindProperty("footerHintText").objectReferenceValue = footTxt;
            dso.ApplyModifiedProperties();
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
            bgImg.color = new Color(0.04f, 0.08f, 0.06f, 0.95f);
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
