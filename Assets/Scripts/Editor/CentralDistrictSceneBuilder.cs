using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using BeastClad.Data;
using BeastClad.Player;
using BeastClad.Security;
using BeastClad.UI;
using BeastClad.World;

namespace BeastClad.Editor
{
    public static class CentralDistrictSceneBuilder
    {
        [MenuItem("BeastClad/Build Central District Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Resources
            Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // ==========================================
            // 1. CAMERA & SMOOTH FOLLOW
            // ==========================================
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.12f, 1f); // Dark metropolitan navy
            camObj.AddComponent<AudioListener>();

            var camFollow = camObj.AddComponent<CameraFollow2D>();
            camFollow.SetBounds(new Vector2(-12.0f, -8.0f), new Vector2(12.0f, 8.0f));

            // ==========================================
            // 2. 2D LIGHTING (URP 2D)
            // ==========================================
            var globalLightObj = new GameObject("Global Light 2D");
            var globalLight = globalLightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.92f, 0.94f, 1.0f, 1.0f);
            globalLight.intensity = 0.95f;

            // Center Concourse Cyan Glow
            CreatePointLight("Light_CenterPlaza", new Vector3(0f, 0f, 0f), new Color(0.15f, 0.85f, 0.95f, 1f), 3.0f, 10.0f, 1.8f);

            // North Gate Royal Gold (Colosseum)
            CreatePointLight("Light_NorthColosseumGate", new Vector3(0f, 7.5f, 0f), new Color(1f, 0.8f, 0.2f, 1f), 2.5f, 9.0f, 2.2f);

            // West Gate Emerald Neon (Outlands Frontier)
            CreatePointLight("Light_WestOutlandsGate", new Vector3(-12.0f, 0f, 0f), new Color(0.1f, 0.95f, 0.45f, 1f), 2.5f, 9.0f, 2.0f);

            // South-East Undercity Hazard Crimson (Duct Access)
            CreatePointLight("Light_SouthUndercityDuct", new Vector3(8.5f, -6.5f, 0f), new Color(1f, 0.2f, 0.35f, 1f), 2.5f, 8.5f, 2.2f);

            // ==========================================
            // 3. ENVIRONMENT & GEOMETRY
            // ==========================================
            var envRoot = new GameObject("Environment");

            // Foundation Plate
            CreateSpriteObject(envRoot.transform, "FoundationFloor", uisprite,
                Vector3.zero, new Vector2(36f, 24f),
                new Color(0.12f, 0.14f, 0.19f, 1f), -15);

            // Main Walkable Concourse
            CreateSpriteObject(envRoot.transform, "WalkablePavement", uisprite,
                Vector3.zero, new Vector2(32f, 20f),
                new Color(0.18f, 0.21f, 0.28f, 1f), -14);

            // Plaza Center Round Decal
            CreateSpriteObject(envRoot.transform, "CenterPlazaDecal", knob,
                Vector3.zero, new Vector2(6.5f, 6.5f),
                new Color(0.24f, 0.30f, 0.40f, 0.9f), -12);

            CreateSpriteObject(envRoot.transform, "CenterCoreRing", knob,
                Vector3.zero, new Vector2(3.5f, 3.5f),
                new Color(0.12f, 0.15f, 0.20f, 1f), -11);

            // Directional Illuminated Runways
            // 1. North Pathway (To Colosseum Gate)
            CreateSpriteObject(envRoot.transform, "Path_NorthColosseum", uisprite,
                new Vector3(0f, 4.5f, 0f), new Vector2(4.5f, 8.0f),
                new Color(0.25f, 0.27f, 0.34f, 1f), -10);
            CreateSpriteObject(envRoot.transform, "LED_North_L", uisprite,
                new Vector3(-2.25f, 4.5f, 0f), new Vector2(0.2f, 8.0f),
                new Color(1f, 0.8f, 0.2f, 0.9f), -9);
            CreateSpriteObject(envRoot.transform, "LED_North_R", uisprite,
                new Vector3(2.25f, 4.5f, 0f), new Vector2(0.2f, 8.0f),
                new Color(1f, 0.8f, 0.2f, 0.9f), -9);

            // 2. West Pathway (To Outlands Frontier Gate)
            CreateSpriteObject(envRoot.transform, "Path_WestOutlands", uisprite,
                new Vector3(-6.5f, 0f, 0f), new Vector2(11.0f, 4.2f),
                new Color(0.22f, 0.26f, 0.24f, 1f), -10);
            CreateSpriteObject(envRoot.transform, "LED_West_T", uisprite,
                new Vector3(-6.5f, 2.1f, 0f), new Vector2(11.0f, 0.2f),
                new Color(0.1f, 0.95f, 0.45f, 0.9f), -9);
            CreateSpriteObject(envRoot.transform, "LED_West_B", uisprite,
                new Vector3(-6.5f, -2.1f, 0f), new Vector2(11.0f, 0.2f),
                new Color(0.1f, 0.95f, 0.45f, 0.9f), -9);

            // 3. South-East Pathway (To Undercity Duct)
            CreateSpriteObject(envRoot.transform, "Path_SouthUndercity", uisprite,
                new Vector3(5.5f, -4.0f, 0f), new Vector2(8.5f, 3.8f),
                new Color(0.26f, 0.18f, 0.22f, 1f), -10);
            CreateSpriteObject(envRoot.transform, "LED_SE_T", uisprite,
                new Vector3(5.5f, -2.1f, 0f), new Vector2(8.5f, 0.2f),
                new Color(1f, 0.2f, 0.35f, 0.9f), -9);
            CreateSpriteObject(envRoot.transform, "LED_SE_B", uisprite,
                new Vector3(5.5f, -5.9f, 0f), new Vector2(8.5f, 0.2f),
                new Color(1f, 0.2f, 0.35f, 0.9f), -9);

            // Center Directory Signboard
            CreateWorldSign(envRoot.transform, "Sign_MetroDirectory", new Vector3(0f, -0.6f, 0f),
                "AETHELGARD METRO TRANSIT // CENTRAL CONCOURSE", new Color(0.25f, 0.9f, 1f, 1f),
                "⬅ THE OUTLANDS (WILDERNESS)   |   ⬆ GRAND COLOSSEUM (CIVIL)   |   ⬇ SECTOR 0: UNDERCITY ➡", new Color(1f, 0.85f, 0.2f, 1f));

            // Perimeter Solid Boundary Walls
            var boundsObj = new GameObject("DistrictBounds");
            boundsObj.transform.SetParent(envRoot.transform);

            // Solid steel walls with hazard trims
            CreateVisibleWall(boundsObj.transform, "Wall_North_L", new Vector2(-8.5f, 9.5f), new Vector2(14f, 1.2f), uisprite);
            CreateVisibleWall(boundsObj.transform, "Wall_North_R", new Vector2(8.5f, 9.5f), new Vector2(14f, 1.2f), uisprite);

            CreateVisibleWall(boundsObj.transform, "Wall_South", new Vector2(0f, -9.5f), new Vector2(32f, 1.2f), uisprite);

            CreateVisibleWall(boundsObj.transform, "Wall_West_T", new Vector2(-15.5f, 6.0f), new Vector2(1.2f, 8.0f), uisprite);
            CreateVisibleWall(boundsObj.transform, "Wall_West_B", new Vector2(-15.5f, -6.0f), new Vector2(1.2f, 8.0f), uisprite);

            CreateVisibleWall(boundsObj.transform, "Wall_East", new Vector2(15.5f, 0f), new Vector2(1.2f, 20f), uisprite);

            // ==========================================
            // 4. NORTH WING: GRAND COLOSSEUM GATE & SCANNER
            // ==========================================
            var northGateRoot = new GameObject("Gate_GrandColosseum");
            northGateRoot.transform.SetParent(envRoot.transform);
            northGateRoot.transform.position = new Vector3(0f, 7.5f, 0f);

            // Gate Frame & Neon Sign
            CreateSpriteObject(northGateRoot.transform, "ArchwayFrame", uisprite,
                Vector3.zero, new Vector2(6.5f, 3.2f),
                new Color(0.35f, 0.28f, 0.15f, 1f), 1);
            CreateSpriteObject(northGateRoot.transform, "ArchwayBorder", uisprite,
                Vector3.zero, new Vector2(6.8f, 3.5f),
                new Color(1f, 0.8f, 0.2f, 1f), 0);

            CreateWorldSign(northGateRoot.transform, "Sign_ColosseumGate", new Vector3(0f, 1.5f, 0f),
                "GRAND COLOSSEUM // CIVIL ARENA DISTRICT", new Color(1f, 0.85f, 0.2f, 1f),
                "[ MUNICIPAL SECURITY CHECKPOINT 01 • LICENSED BIOMETRICS ONLY ]", new Color(0.85f, 0.88f, 0.95f, 1f));

            // Stationed Enforcer AI
            var enforcerObj = new GameObject("MunicipalEnforcer_OfficerThorne");
            enforcerObj.transform.SetParent(northGateRoot.transform);
            enforcerObj.transform.localPosition = new Vector3(2.4f, -0.6f, 0f);
            var enfSr = enforcerObj.AddComponent<SpriteRenderer>();
            enfSr.sprite = knob;
            enfSr.color = new Color(0.2f, 0.55f, 0.95f, 1f); // Corporate Law Blue
            enforcerObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            enfSr.sortingOrder = 4;

            var enfCol = enforcerObj.AddComponent<CircleCollider2D>();
            enfCol.radius = 0.8f / 6.5f;

            var enfAI = enforcerObj.AddComponent<MunicipalEnforcerAI2D>();
            var enfSO = new SerializedObject(enfAI);
            enfSO.FindProperty("enforcerName").stringValue = "Officer Thorne";
            enfSO.FindProperty("unitBadge").stringValue = "Aegis-Civil-Patrol 101";
            enfSO.FindProperty("enforcerRenderer").objectReferenceValue = enfSr;
            enfSO.ApplyModifiedProperties();

            // Municipal Scanner Checkpoint Zone
            var scannerObj = new GameObject("MunicipalScanner_Gate01");
            scannerObj.transform.SetParent(northGateRoot.transform);
            scannerObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            var scanBeamObj = new GameObject("ScanBeam");
            scanBeamObj.transform.SetParent(scannerObj.transform, false);
            var scanBeamSr = scanBeamObj.AddComponent<SpriteRenderer>();
            scanBeamSr.sprite = uisprite;
            scanBeamSr.drawMode = SpriteDrawMode.Sliced;
            scanBeamSr.size = new Vector2(4.5f, 1.2f);
            scanBeamSr.color = new Color(0.95f, 0.75f, 0.1f, 0.45f);
            scanBeamSr.sortingOrder = 2;

            var scanCol = scannerObj.AddComponent<BoxCollider2D>();
            scanCol.isTrigger = true;
            scanCol.size = new Vector2(4.5f, 1.5f);

            var scannerZone = scannerObj.AddComponent<MunicipalScannerZone>();
            var scanSO = new SerializedObject(scannerZone);
            scanSO.FindProperty("checkpointName").stringValue = "Colosseum Gate 01 Checkpoint";
            scanSO.FindProperty("sectorName").stringValue = "Civil Arena District";
            scanSO.FindProperty("beamRenderer").objectReferenceValue = scanBeamSr;
            scanSO.FindProperty("stationedEnforcer").objectReferenceValue = enfAI;
            scanSO.ApplyModifiedProperties();

            // Municipal Biometric Quarantine Locker (Contraband Stash & Disarmament Terminal)
            var lockerObj = new GameObject("MunicipalQuarantineLocker_Gate01");
            lockerObj.transform.SetParent(northGateRoot.transform);
            lockerObj.transform.localPosition = new Vector3(-4.2f, -0.6f, 0f);

            CreateSpriteObject(lockerObj.transform, "LockerCabinet", uisprite,
                Vector3.zero, new Vector2(2.2f, 2.6f),
                new Color(0.10f, 0.14f, 0.20f, 1f), 3);
            CreateSpriteObject(lockerObj.transform, "LockerBorder", uisprite,
                Vector3.zero, new Vector2(2.4f, 2.8f),
                new Color(0.1f, 0.85f, 1f, 1f), 2);

            var statusLightObj = new GameObject("StatusLight");
            statusLightObj.transform.SetParent(lockerObj.transform, false);
            statusLightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var lockerLightSr = statusLightObj.AddComponent<SpriteRenderer>();
            lockerLightSr.sprite = knob;
            lockerLightSr.color = new Color(0.1f, 0.85f, 1f, 1f);
            statusLightObj.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
            lockerLightSr.sortingOrder = 5;

            var lockerLabelObj = new GameObject("OverheadLabel");
            lockerLabelObj.transform.SetParent(lockerObj.transform, false);
            lockerLabelObj.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            var lockerTm = lockerLabelObj.AddComponent<TextMesh>();
            lockerTm.text = "QUARANTINE LOCKER\n[SECURE • READY]";
            lockerTm.fontSize = 20;
            lockerTm.characterSize = 0.045f;
            lockerTm.alignment = TextAlignment.Center;
            lockerTm.anchor = TextAnchor.MiddleCenter;
            lockerTm.color = new Color(0.1f, 0.85f, 1f, 1f);
            var lockerMr = lockerLabelObj.GetComponent<MeshRenderer>();
            if (lockerMr != null) lockerMr.sortingOrder = 15;

            var lockerCol = lockerObj.AddComponent<CircleCollider2D>();
            lockerCol.isTrigger = true;
            lockerCol.radius = 2.2f;

            var lockerComp = lockerObj.AddComponent<MunicipalQuarantineLocker>();
            var lockerSO = new SerializedObject(lockerComp);
            var pLockerId = lockerSO.FindProperty("lockerId");
            if (pLockerId != null) pLockerId.stringValue = "Municipal Biometric Locker 01";
            var pCheckpointSector = lockerSO.FindProperty("checkpointSector");
            if (pCheckpointSector != null) pCheckpointSector.stringValue = "Grand Colosseum North Gate";
            var pLinkedScanner = lockerSO.FindProperty("linkedScanner");
            if (pLinkedScanner != null) pLinkedScanner.objectReferenceValue = scannerZone;
            var pStatusLight = lockerSO.FindProperty("statusLight");
            if (pStatusLight != null) pStatusLight.objectReferenceValue = lockerLightSr;
            var pOverheadLabel = lockerSO.FindProperty("overheadLabel");
            if (pOverheadLabel != null) pOverheadLabel.objectReferenceValue = lockerTm;
            lockerSO.ApplyModifiedProperties();

            // Link locker back to scanner
            scanSO.Update();
            var pQuarantineLocker = scanSO.FindProperty("quarantineLocker");
            if (pQuarantineLocker != null) pQuarantineLocker.objectReferenceValue = lockerComp;
            scanSO.ApplyModifiedProperties();

            // Transition Trigger to Arena Colosseum
            var colosseumTriggerObj = new GameObject("Transition_ToColosseum");
            colosseumTriggerObj.transform.SetParent(northGateRoot.transform);
            colosseumTriggerObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            var cTrigCol = colosseumTriggerObj.AddComponent<BoxCollider2D>();
            cTrigCol.isTrigger = true;
            cTrigCol.size = new Vector2(4.5f, 1.2f);

            var cTrig = colosseumTriggerObj.AddComponent<SceneTransitionTrigger>();
            var cTrigSO = new SerializedObject(cTrig);
            cTrigSO.FindProperty("targetSceneName").stringValue = "Arena_Colosseum";
            cTrigSO.FindProperty("targetSpawnTag").stringValue = "Spawn_FromHub";
            cTrigSO.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            cTrigSO.FindProperty("promptText").stringValue = "[ F ] Enter Grand Colosseum (Tournament Arena)";
            cTrigSO.FindProperty("requiredScanner").objectReferenceValue = scannerZone;
            cTrigSO.FindProperty("denyOnViolation").boolValue = true;
            cTrigSO.FindProperty("denialWarning").stringValue = "SECURITY LOCK: Unlicensed Biometrics Detected! Colosseum Gates Sealed.";
            cTrigSO.ApplyModifiedProperties();

            // Spawn Point when returning from Colosseum
            var spawnFromArena = new GameObject("Spawn_FromArena");
            spawnFromArena.transform.SetParent(northGateRoot.transform);
            spawnFromArena.transform.localPosition = new Vector3(0f, -1.8f, 0f);
            var spArena = spawnFromArena.AddComponent<SceneSpawnPoint>();
            var spArenaSO = new SerializedObject(spArena);
            spArenaSO.FindProperty("spawnTag").stringValue = "Spawn_FromArena";
            spArenaSO.FindProperty("defaultFacing").vector2Value = Vector2.down;
            spArenaSO.ApplyModifiedProperties();

            // ==========================================
            // 5. WEST WING: OUTLANDS FRONTIER GATE (WILDERNESS)
            // ==========================================
            var westGateRoot = new GameObject("Gate_WildOutlands");
            westGateRoot.transform.SetParent(envRoot.transform);
            westGateRoot.transform.position = new Vector3(-13.5f, 0f, 0f);

            CreateSpriteObject(westGateRoot.transform, "BlastGateFrame", uisprite,
                Vector3.zero, new Vector2(3.2f, 6.5f),
                new Color(0.24f, 0.32f, 0.26f, 1f), 1);
            CreateSpriteObject(westGateRoot.transform, "BlastGateBorder", uisprite,
                Vector3.zero, new Vector2(3.5f, 6.8f),
                new Color(0.1f, 0.95f, 0.45f, 1f), 0);

            CreateWorldSign(westGateRoot.transform, "Sign_WildGate", new Vector3(0f, 3.2f, 0f),
                "OUTLANDS EXPEDITION GATE // THE WILDERNESS", new Color(0.1f, 0.95f, 0.45f, 1f),
                "[ WILD BEAST BIOMES • TRAPPING PERMITTED ]", new Color(0.85f, 0.95f, 0.88f, 1f));

            // Transition Trigger to Wilderness
            var wildTriggerObj = new GameObject("Transition_ToWilderness");
            wildTriggerObj.transform.SetParent(westGateRoot.transform);
            wildTriggerObj.transform.localPosition = new Vector3(-0.6f, 0f, 0f);

            var wTrigCol = wildTriggerObj.AddComponent<BoxCollider2D>();
            wTrigCol.isTrigger = true;
            wTrigCol.size = new Vector2(1.5f, 4.5f);

            var wTrig = wildTriggerObj.AddComponent<SceneTransitionTrigger>();
            var wTrigSO = new SerializedObject(wTrig);
            wTrigSO.FindProperty("targetSceneName").stringValue = "Adventurer_Wilderness";
            wTrigSO.FindProperty("targetSpawnTag").stringValue = "Spawn_FromHub";
            wTrigSO.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            wTrigSO.FindProperty("promptText").stringValue = "[ F ] Depart for The Wild Outlands (Trapping Zone)";
            wTrigSO.ApplyModifiedProperties();

            // Spawn Point when returning from Wilderness
            var spawnFromWild = new GameObject("Spawn_FromWilderness");
            spawnFromWild.transform.SetParent(westGateRoot.transform);
            spawnFromWild.transform.localPosition = new Vector3(1.8f, 0f, 0f);
            var spWild = spawnFromWild.AddComponent<SceneSpawnPoint>();
            var spWildSO = new SerializedObject(spWild);
            spWildSO.FindProperty("spawnTag").stringValue = "Spawn_FromWilderness";
            spWildSO.FindProperty("defaultFacing").vector2Value = Vector2.right;
            spWildSO.ApplyModifiedProperties();

            // ==========================================
            // 6. SOUTH-EAST WING: UNDERCITY MAINTENANCE DUCT (BLACK MARKET)
            // ==========================================
            var seGateRoot = new GameObject("Gate_Sector0Undercity");
            seGateRoot.transform.SetParent(envRoot.transform);
            seGateRoot.transform.position = new Vector3(9.5f, -6.5f, 0f);

            CreateSpriteObject(seGateRoot.transform, "DuctHatchFrame", uisprite,
                Vector3.zero, new Vector2(4.5f, 3.8f),
                new Color(0.35f, 0.18f, 0.22f, 1f), 1);
            CreateSpriteObject(seGateRoot.transform, "DuctHatchBorder", uisprite,
                Vector3.zero, new Vector2(4.8f, 4.1f),
                new Color(1f, 0.2f, 0.35f, 1f), 0);

            CreateWorldSign(seGateRoot.transform, "Sign_UndercityDuct", new Vector3(0f, 1.8f, 0f),
                "MAINTENANCE SHAFT 14-B // SECTOR 0", new Color(1f, 0.25f, 0.4f, 1f),
                "⚠ UNAUTHORIZED ENTRY STRICTLY FORBIDDEN • PIT ACCESS ⚠", new Color(1f, 0.6f, 0.2f, 1f));

            // Transition Trigger to Underground Black Market
            var underTriggerObj = new GameObject("Transition_ToUnderground");
            underTriggerObj.transform.SetParent(seGateRoot.transform);
            underTriggerObj.transform.localPosition = Vector3.zero;

            var uTrigCol = underTriggerObj.AddComponent<BoxCollider2D>();
            uTrigCol.isTrigger = true;
            uTrigCol.size = new Vector2(3.5f, 2.5f);

            var uTrig = underTriggerObj.AddComponent<SceneTransitionTrigger>();
            var uTrigSO = new SerializedObject(uTrig);
            uTrigSO.FindProperty("targetSceneName").stringValue = "Underground_BlackMarket";
            uTrigSO.FindProperty("targetSpawnTag").stringValue = "Spawn_FromHub";
            uTrigSO.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            uTrigSO.FindProperty("promptText").stringValue = "[ F ] Descend into Sector 0: Undercity (Black Market & Pit)";
            uTrigSO.ApplyModifiedProperties();

            // Spawn Point when returning from Underground
            var spawnFromUnder = new GameObject("Spawn_FromUnderground");
            spawnFromUnder.transform.SetParent(seGateRoot.transform);
            spawnFromUnder.transform.localPosition = new Vector3(-1.8f, 1.2f, 0f);
            var spUnder = spawnFromUnder.AddComponent<SceneSpawnPoint>();
            var spUnderSO = new SerializedObject(spUnder);
            spUnderSO.FindProperty("spawnTag").stringValue = "Spawn_FromUnderground";
            spUnderSO.FindProperty("defaultFacing").vector2Value = new Vector2(-1f, 1f).normalized;
            spUnderSO.ApplyModifiedProperties();

            // ==========================================
            // 7. CENTER DEFAULT SPAWN POINT
            // ==========================================
            var spawnDefault = new GameObject("Spawn_Default");
            spawnDefault.transform.SetParent(envRoot.transform);
            spawnDefault.transform.position = new Vector3(0f, 0f, 0f);
            var spDef = spawnDefault.AddComponent<SceneSpawnPoint>();
            var spDefSO = new SerializedObject(spDef);
            spDefSO.FindProperty("spawnTag").stringValue = "Spawn_Default";
            spDefSO.FindProperty("defaultFacing").vector2Value = Vector2.down;
            spDefSO.ApplyModifiedProperties();

            // ==========================================
            // 8. PLAYER CHARACTER SETUP
            // ==========================================
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerObj;
            if (playerPrefab != null)
            {
                playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerObj.name = "Player";
                playerObj.tag = "Player";
                playerObj.transform.position = Vector3.zero;
            }
            else
            {
                playerObj = new GameObject("Player");
                playerObj.tag = "Player";
                playerObj.transform.position = Vector3.zero;
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

            var playerWallet = playerObj.GetComponent<PlayerWallet>();
            if (playerWallet != null)
            {
                var walletSO = new SerializedObject(playerWallet);
                walletSO.FindProperty("startingCredits").intValue = 1000;
                walletSO.ApplyModifiedProperties();
            }

            // ==========================================
            // 9. EVENTSYSTEM & CANVAS
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
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasObj.AddComponent<LimbCooldownUI>();
            }

            // Wallet HUD UI (Top-Right)
            BuildWalletHUD(canvasObj.transform, defaultFont, uisprite);

            // Scene Transition Prompt UI (Bottom-Center Banner)
            BuildSceneTransitionPromptHUD(canvasObj.transform, defaultFont, uisprite);

            // Municipal Scanner Warning HUD (Top-Center)
            BuildScannerWarningHUD(canvasObj.transform, defaultFont, uisprite);

            // ==========================================
            // 10. SAVE SCENE
            // ==========================================
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/District_CentralHub.unity");
            Debug.Log("<color=#00FFAA>[Scene Builder]</color> Successfully built <b>Assets/Scenes/District_CentralHub.unity</b> with 3 transit sectors!");
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

        private static void CreateVisibleWall(Transform parent, string name, Vector2 pos, Vector2 size, Sprite uisprite)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = pos;

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = new Color(0.24f, 0.28f, 0.38f, 1f);
            sr.sortingOrder = 2;
            wall.transform.localScale = Vector3.one;

            var hazard = new GameObject(name + "_HazardStripe");
            hazard.transform.SetParent(wall.transform, false);
            var hsr = hazard.AddComponent<SpriteRenderer>();
            hsr.sprite = uisprite;
            hsr.drawMode = SpriteDrawMode.Sliced;
            float hazardThickness = Mathf.Min(0.25f, size.y * 0.25f);
            hsr.size = new Vector2(size.x * 0.98f, hazardThickness);
            hsr.color = new Color(0.95f, 0.75f, 0.05f, 1f);
            hsr.sortingOrder = 3;
            hazard.transform.localPosition = new Vector3(0f, -(size.y - hazardThickness) * 0.5f, 0f);
        }

        private static void CreateWorldSign(Transform parent, string name, Vector3 localPos, string mainText, Color mainColor, string subText = null, Color? subColor = null)
        {
            var signObj = new GameObject(name);
            signObj.transform.SetParent(parent, false);
            signObj.transform.localPosition = localPos;

            var tm = signObj.AddComponent<TextMesh>();
            tm.text = mainText;
            tm.fontSize = 28;
            tm.characterSize = 0.055f;
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
                subObj.transform.localPosition = new Vector3(0f, -0.35f, 0f);

                var stm = subObj.AddComponent<TextMesh>();
                stm.text = subText;
                stm.fontSize = 20;
                stm.characterSize = 0.045f;
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
            rt.anchoredPosition = new Vector2(-25f, -25f);
            rt.sizeDelta = new Vector2(240f, 50f);

            var bgImg = walletRoot.AddComponent<Image>();
            bgImg.sprite = uisprite;
            bgImg.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);
            bgImg.raycastTarget = false;

            var textObj = new GameObject("WalletText");
            textObj.transform.SetParent(walletRoot.transform, false);
            var trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.85f, 0.2f, 1f);
            txt.text = "1,000 CR";
            txt.raycastTarget = false;

            var walletUI = walletRoot.AddComponent<WalletHUDUI>();
            var wso = new SerializedObject(walletUI);
            wso.FindProperty("creditsText").objectReferenceValue = txt;
            wso.ApplyModifiedProperties();
        }

        private static void BuildSceneTransitionPromptHUD(Transform canvasTransform, Font font, Sprite uisprite)
        {
            var promptRoot = new GameObject("SceneTransitionPromptHUD");
            promptRoot.transform.SetParent(canvasTransform, false);
            var rt = promptRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 160f);
            rt.sizeDelta = new Vector2(620f, 50f);

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
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.2f, 0.95f, 1f, 1f);
            txt.text = "[ F ] Enter District";
            txt.raycastTarget = false;

            var promptUI = promptRoot.AddComponent<SceneTransitionPromptUI>();
            var pso = new SerializedObject(promptUI);
            pso.FindProperty("promptPanel").objectReferenceValue = promptRoot;
            pso.FindProperty("promptText").objectReferenceValue = txt;
            pso.ApplyModifiedProperties();
        }

        private static void BuildScannerWarningHUD(Transform canvasTransform, Font font, Sprite uisprite)
        {
            var hudObj = new GameObject("MunicipalScannerHUD");
            hudObj.transform.SetParent(canvasTransform, false);
            var rt = hudObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -25f);
            rt.sizeDelta = new Vector2(600f, 65f);

            var bgImg = hudObj.AddComponent<Image>();
            bgImg.sprite = uisprite;
            bgImg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);
            bgImg.raycastTarget = false;

            var bannerTextObj = new GameObject("ScannerBannerText");
            bannerTextObj.transform.SetParent(hudObj.transform, false);
            var brt = bannerTextObj.AddComponent<RectTransform>();
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.sizeDelta = Vector2.zero;

            var bTxt = bannerTextObj.AddComponent<Text>();
            bTxt.font = font;
            bTxt.fontSize = 18;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
            bTxt.text = "";
            bTxt.raycastTarget = false;

            var scanUI = hudObj.AddComponent<MunicipalScannerHUD>();
            var sso = new SerializedObject(scanUI);
            var pRoot = sso.FindProperty("bannerRoot");
            if (pRoot != null) pRoot.objectReferenceValue = hudObj;
            var pBg = sso.FindProperty("bannerBg");
            if (pBg != null) pBg.objectReferenceValue = bgImg;
            var pStatus = sso.FindProperty("statusText");
            if (pStatus != null) pStatus.objectReferenceValue = bTxt;
            var pTitle = sso.FindProperty("titleText");
            if (pTitle != null) pTitle.objectReferenceValue = bTxt;
            sso.ApplyModifiedProperties();
        }
    }
}
