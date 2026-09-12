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
using BeastClad.Player;
using BeastClad.Interaction;
using BeastClad.Underground;
using BeastClad.UI;
using BeastClad.World;

namespace BeastClad.Editor
{
    public static class UndergroundSceneBuilder
    {
        [MenuItem("BeastClad/Build Underground Black Market Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Sprites and Fonts
            Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            VolumeProfile volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/DefaultVolumeProfile.asset");

            // ==========================================
            // 1. CAMERA & FOLLOW
            // ==========================================
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, -5f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f); // Dark industrial abyss
            camObj.AddComponent<AudioListener>();

            var uacd = camObj.AddComponent<UniversalAdditionalCameraData>();
            uacd.renderPostProcessing = true;

            var camFollow = camObj.AddComponent<CameraFollow2D>();
            camFollow.SetBounds(new Vector2(-10.0f, -6.5f), new Vector2(10.0f, 5.5f));

            if (volumeProfile != null)
            {
                var volObj = new GameObject("Global Volume");
                var vol = volObj.AddComponent<Volume>();
                vol.isGlobal = true;
                vol.profile = volumeProfile;
            }

            // ==========================================
            // 2. 2D LIGHTING (URP 2D)
            // ==========================================
            var globalLightObj = new GameObject("Global Light 2D");
            var globalLight = globalLightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.92f, 0.94f, 1.0f, 1.0f);
            globalLight.intensity = 0.95f; // Bright and clear

            // Neon light over Clinic
            var clinicLightObj = new GameObject("Clinic_NeonRedLight");
            clinicLightObj.transform.position = new Vector3(-8f, 2.5f, 0f);
            var clinicLight = clinicLightObj.AddComponent<Light2D>();
            clinicLight.lightType = Light2D.LightType.Point;
            clinicLight.color = new Color(1f, 0.2f, 0.35f, 1f);
            clinicLight.pointLightInnerRadius = 2.0f;
            clinicLight.pointLightOuterRadius = 8.5f;
            clinicLight.intensity = 2.2f;

            // Harsh light over Pit
            var pitLightObj = new GameObject("Pit_AmberFloodlight");
            pitLightObj.transform.position = new Vector3(8f, 3.5f, 0f);
            var pitLight = pitLightObj.AddComponent<Light2D>();
            pitLight.lightType = Light2D.LightType.Point;
            pitLight.color = new Color(1f, 0.7f, 0.15f, 1f);
            pitLight.pointLightInnerRadius = 3.0f;
            pitLight.pointLightOuterRadius = 10.0f;
            pitLight.intensity = 2.5f;

            // Hub cold cyan accent
            var hubLightObj = new GameObject("Hub_CyanLight");
            hubLightObj.transform.position = new Vector3(0f, -2f, 0f);
            var hubLight = hubLightObj.AddComponent<Light2D>();
            hubLight.lightType = Light2D.LightType.Point;
            hubLight.color = new Color(0.15f, 0.85f, 0.95f, 1f);
            hubLight.pointLightInnerRadius = 2.0f;
            hubLight.pointLightOuterRadius = 7.5f;
            hubLight.intensity = 1.8f;

            // ==========================================
            // 3. ENVIRONMENT ROOT
            // ==========================================
            var envRoot = new GameObject("Environment");

            // A. Foundation Floor (Dark Slate)
            CreateSpriteObject(envRoot.transform, "FoundationFloor", uisprite,
                new Vector3(0f, 0.5f, 0f), new Vector3(34f, 22f, 1f),
                new Color(0.14f, 0.16f, 0.22f, 1f), -15);

            // B. Main Walkable Concourse (Lighter Slate with Seams)
            CreateSpriteObject(envRoot.transform, "WalkablePavement", uisprite,
                new Vector3(0f, 0.5f, 0f), new Vector3(31f, 19f, 1f),
                new Color(0.20f, 0.23f, 0.30f, 1f), -14);

            // Quadrant tiles for texture and depth
            CreateSpriteObject(envRoot.transform, "Tile_NW", uisprite, new Vector3(-7.5f, 5.0f, 0f), new Vector3(14.5f, 8.5f, 1f), new Color(0.17f, 0.20f, 0.26f, 1f), -13);
            CreateSpriteObject(envRoot.transform, "Tile_NE", uisprite, new Vector3(7.5f, 5.0f, 0f), new Vector3(14.5f, 8.5f, 1f), new Color(0.17f, 0.20f, 0.26f, 1f), -13);
            CreateSpriteObject(envRoot.transform, "Tile_SW", uisprite, new Vector3(-7.5f, -4.0f, 0f), new Vector3(14.5f, 8.5f, 1f), new Color(0.17f, 0.20f, 0.26f, 1f), -13);
            CreateSpriteObject(envRoot.transform, "Tile_SE", uisprite, new Vector3(7.5f, -4.0f, 0f), new Vector3(14.5f, 8.5f, 1f), new Color(0.17f, 0.20f, 0.26f, 1f), -13);

            // C. Dedicated Navigational Pathways (Clear Walkways)
            // 1. Central Runway (Entrance -> Hub Center)
            CreateSpriteObject(envRoot.transform, "Path_EntranceRunway", uisprite,
                new Vector3(0f, -3.5f, 0f), new Vector3(3.6f, 9.5f, 1f),
                new Color(0.26f, 0.30f, 0.39f, 1f), -10);

            // Cyan LED border strips along the entrance path
            CreateSpriteObject(envRoot.transform, "LED_Strip_Left", uisprite,
                new Vector3(-1.8f, -3.5f, 0f), new Vector3(0.18f, 9.5f, 1f),
                new Color(0f, 0.9f, 1f, 0.95f), -9);
            CreateSpriteObject(envRoot.transform, "LED_Strip_Right", uisprite,
                new Vector3(1.8f, -3.5f, 0f), new Vector3(0.18f, 9.5f, 1f),
                new Color(0f, 0.9f, 1f, 0.95f), -9);

            // 2. West Concourse Path (Hub -> Dr. Silas Clinic)
            CreateSpriteObject(envRoot.transform, "Path_WestClinic", uisprite,
                new Vector3(-4.0f, 1.2f, 0f), new Vector3(6.5f, 3.2f, 1f),
                new Color(0.28f, 0.22f, 0.28f, 1f), -10);

            CreateSpriteObject(envRoot.transform, "LED_Clinic_Top", uisprite,
                new Vector3(-4.0f, 2.75f, 0f), new Vector3(6.5f, 0.18f, 1f),
                new Color(1f, 0.15f, 0.35f, 0.95f), -9);
            CreateSpriteObject(envRoot.transform, "LED_Clinic_Bottom", uisprite,
                new Vector3(-4.0f, -0.35f, 0f), new Vector3(6.5f, 0.18f, 1f),
                new Color(1f, 0.15f, 0.35f, 0.95f), -9);

            // 3. East Concourse Path (Hub -> Pitmaster Jax & The Pit)
            CreateSpriteObject(envRoot.transform, "Path_EastPit", uisprite,
                new Vector3(2.0f, 1.2f, 0f), new Vector3(3.5f, 3.2f, 1f),
                new Color(0.32f, 0.26f, 0.18f, 1f), -10);

            CreateSpriteObject(envRoot.transform, "LED_Pit_Top", uisprite,
                new Vector3(2.0f, 2.75f, 0f), new Vector3(3.5f, 0.18f, 1f),
                new Color(1f, 0.7f, 0.1f, 0.95f), -9);
            CreateSpriteObject(envRoot.transform, "LED_Pit_Bottom", uisprite,
                new Vector3(2.0f, -0.35f, 0f), new Vector3(3.5f, 0.18f, 1f),
                new Color(1f, 0.7f, 0.1f, 0.95f), -9);

            // Central Junction Decal
            CreateSpriteObject(envRoot.transform, "HubCenterPad", knob,
                new Vector3(0f, 1.2f, 0f), new Vector3(3.2f, 3.2f, 1f),
                new Color(0.30f, 0.36f, 0.48f, 1f), -8);

            // Wayfinding World Signs
            CreateWorldSign(envRoot.transform, "Sign_Entrance", new Vector3(0f, -4.2f, 0f),
                "SECTOR 0: UNDERCITY", new Color(0.2f, 0.9f, 1f, 1f),
                "⬅ DR. SILAS (CLINIC)   |   THE GUTTER PIT (ARENA) ➡", new Color(1f, 0.85f, 0.2f, 1f));

            // South Exit Gate back to Central District
            var exitRoot = new GameObject("Exit_ToCentralHub");
            exitRoot.transform.SetParent(envRoot.transform);
            exitRoot.transform.position = new Vector3(0f, -7.2f, 0f);

            CreateSpriteObject(exitRoot.transform, "ExitStairwellPad", uisprite,
                Vector3.zero, new Vector2(4.2f, 2.4f),
                new Color(0.20f, 0.24f, 0.32f, 1f), -9);

            CreateWorldSign(exitRoot.transform, "Sign_ExitHub", new Vector3(0f, 0.6f, 0f),
                "⬆ STAIRS TO CENTRAL METRO DISTRICT", new Color(0.25f, 0.9f, 1f, 1f),
                "[ F ] Ascend to Central Concourse", new Color(0.9f, 0.9f, 0.95f, 1f));

            var exitCol = exitRoot.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(3.6f, 2.0f);

            var exitTrig = exitRoot.AddComponent<SceneTransitionTrigger>();
            var etSO = new SerializedObject(exitTrig);
            etSO.FindProperty("targetSceneName").stringValue = "District_CentralHub";
            etSO.FindProperty("targetSpawnTag").stringValue = "Spawn_FromUnderground";
            etSO.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            etSO.FindProperty("promptText").stringValue = "[ F ] Climb Stairs to Central Metro District";
            etSO.ApplyModifiedProperties();

            var spawnFromHub = new GameObject("Spawn_FromHub");
            spawnFromHub.transform.SetParent(envRoot.transform);
            spawnFromHub.transform.position = new Vector3(0f, -5.5f, 0f);
            var spHub = spawnFromHub.AddComponent<SceneSpawnPoint>();
            var spHubSO = new SerializedObject(spHub);
            spHubSO.FindProperty("spawnTag").stringValue = "Spawn_FromHub";
            spHubSO.FindProperty("defaultFacing").vector2Value = Vector2.up;
            spHubSO.ApplyModifiedProperties();

            // ==========================================
            // 4. BOUNDS & SOLID VISIBLE WALLS
            // ==========================================
            var boundsObj = new GameObject("UndergroundBounds");
            boundsObj.transform.SetParent(envRoot.transform);

            // Visible walls with matching BoxCollider2D & high-contrast hazard trim footings
            CreateVisibleWall(boundsObj.transform, "Wall_North", new Vector2(0f, 10.5f), new Vector2(32f, 1.2f), uisprite);
            CreateVisibleWall(boundsObj.transform, "Wall_South", new Vector2(0f, -9.5f), new Vector2(32f, 1.2f), uisprite);
            CreateVisibleWall(boundsObj.transform, "Wall_West", new Vector2(-16f, 0.5f), new Vector2(1.2f, 20f), uisprite);
            CreateVisibleWall(boundsObj.transform, "Wall_East", new Vector2(16f, 0.5f), new Vector2(1.2f, 20f), uisprite);

            // Corner Bulkhead Pillars
            CreatePillar(boundsObj.transform, "Pillar_NW", new Vector2(-15.5f, 10.0f), uisprite);
            CreatePillar(boundsObj.transform, "Pillar_NE", new Vector2(15.5f, 10.0f), uisprite);
            CreatePillar(boundsObj.transform, "Pillar_SW", new Vector2(-15.5f, -9.0f), uisprite);
            CreatePillar(boundsObj.transform, "Pillar_SE", new Vector2(15.5f, -9.0f), uisprite);

            // ==========================================
            // 5. ZONE 1: DR. SILAS'S CLINIC (WEST WING)
            // ==========================================
            var clinicRoot = new GameObject("DrSilas_ClinicZone");
            clinicRoot.transform.SetParent(envRoot.transform);
            clinicRoot.transform.position = new Vector3(-8f, 2.5f, 0f);

            // Platform Outer Border (Crimson Neon)
            CreateSpriteObject(clinicRoot.transform, "ClinicBorderFrame", uisprite,
                Vector3.zero, new Vector3(9.5f, 8.0f, 1f),
                new Color(1f, 0.15f, 0.35f, 0.95f), -7);

            // Platform Floor Plate (Sterile Slate-Teal)
            CreateSpriteObject(clinicRoot.transform, "ClinicFloorPlate", uisprite,
                Vector3.zero, new Vector3(9.1f, 7.6f, 1f),
                new Color(0.12f, 0.16f, 0.22f, 1f), -6);

            // Floor Medical Cross Painted Emblem
            CreateSpriteObject(clinicRoot.transform, "FloorCross_H", uisprite,
                new Vector3(0f, 0f, 0f), new Vector3(4.2f, 1.3f, 1f),
                new Color(0.9f, 0.15f, 0.3f, 0.45f), -5);
            CreateSpriteObject(clinicRoot.transform, "FloorCross_V", uisprite,
                new Vector3(0f, 0f, 0f), new Vector3(1.3f, 4.2f, 1f),
                new Color(0.9f, 0.15f, 0.3f, 0.45f), -5);

            // Back Wall Medical Console & Surgical Slab (Visual landmarks)
            CreateSpriteObject(clinicRoot.transform, "SurgicalConsoleBackdrop", uisprite,
                new Vector3(-2.8f, 2.4f, 0f), new Vector3(2.5f, 1.8f, 1f),
                new Color(0.22f, 0.26f, 0.34f, 1f), 1);
            CreateSpriteObject(clinicRoot.transform, "MonitorScreen_Left", uisprite,
                new Vector3(-2.8f, 2.6f, 0f), new Vector3(2.0f, 1.0f, 1f),
                new Color(0f, 0.95f, 0.7f, 1f), 2);

            CreateSpriteObject(clinicRoot.transform, "OperatingTable", uisprite,
                new Vector3(1.2f, 2.4f, 0f), new Vector3(2.4f, 1.3f, 1f),
                new Color(0.35f, 0.40f, 0.48f, 1f), 1);
            CreateSpriteObject(clinicRoot.transform, "OperatingTablePad", uisprite,
                new Vector3(1.2f, 2.4f, 0f), new Vector3(2.1f, 1.0f, 1f),
                new Color(0.15f, 0.65f, 0.55f, 1f), 2);

            // Clinic Overhead Backlit Neon Sign
            CreateSpriteObject(clinicRoot.transform, "ClinicSign_Board", uisprite,
                new Vector3(0f, 4.3f, 0f), new Vector3(8.0f, 1.1f, 1f),
                new Color(0.10f, 0.04f, 0.07f, 0.95f), 5);
            CreateSpriteObject(clinicRoot.transform, "ClinicSign_Border", uisprite,
                new Vector3(0f, 4.3f, 0f), new Vector3(8.2f, 1.25f, 1f),
                new Color(1f, 0.15f, 0.35f, 1f), 4);

            CreateWorldSign(clinicRoot.transform, "Sign_SilasClinic", new Vector3(0f, 4.3f, 0f),
                "DR. SILAS // THE STITCHER", new Color(1f, 0.25f, 0.45f, 1f),
                "Unlicensed Neuro-Chirurgeon • [ F ] Contraband Augments", new Color(0.85f, 0.88f, 0.95f, 1f));

            // Glowing Interaction Ring on Floor Under Doctor
            CreateSpriteObject(clinicRoot.transform, "Silas_InteractionRing", knob,
                new Vector3(0f, -0.5f, 0f), new Vector3(2.8f, 2.8f, 1f),
                new Color(1f, 0.15f, 0.35f, 0.4f), -4);
            CreateSpriteObject(clinicRoot.transform, "Silas_InteractionCore", knob,
                new Vector3(0f, -0.5f, 0f), new Vector3(2.2f, 2.2f, 1f),
                new Color(0.15f, 0.08f, 0.12f, 0.85f), -3);

            // Dr. Silas NPC
            var silasObj = new GameObject("Ripperdoc_DrSilas");
            silasObj.transform.SetParent(clinicRoot.transform);
            silasObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            var silasSr = silasObj.AddComponent<SpriteRenderer>();
            silasSr.sprite = knob;
            silasSr.color = new Color(1f, 0.25f, 0.45f, 1f); // Distinct crimson surgeon
            silasObj.transform.localScale = new Vector3(6.5f, 6.5f, 1f);
            silasSr.sortingOrder = 3;

            var silasCol = silasObj.AddComponent<CircleCollider2D>();
            silasCol.isTrigger = true;
            silasCol.radius = 2.0f / 6.5f; // Effective world radius = 2.0m (eliminates cross-hub overlap)

            var silasVendor = silasObj.AddComponent<RipperdocVendor>();
            var overMantisSO = AssetDatabase.LoadAssetAtPath<RipperdocProductSO>("Assets/Data/BlackMarket/Ripperdoc_OverclockedMantis.asset");
            var wyrmSO = AssetDatabase.LoadAssetAtPath<RipperdocProductSO>("Assets/Data/BlackMarket/Ripperdoc_HyperVolatileWyrm.asset");
            var catalogList = new List<RipperdocProductSO>();
            if (overMantisSO != null) catalogList.Add(overMantisSO);
            if (wyrmSO != null) catalogList.Add(wyrmSO);
            silasVendor.SetCatalog(catalogList);

            // ==========================================
            // 6. ZONE 2: THE GUTTER PIT ARENA (EAST WING)
            // ==========================================
            var pitRoot = new GameObject("UnsanctionedPit_Arena");
            pitRoot.transform.SetParent(envRoot.transform);
            pitRoot.transform.position = new Vector3(8.5f, 3.5f, 0f);

            // Outer Pit Platform Frame
            CreateSpriteObject(pitRoot.transform, "PitOuterFrame", uisprite,
                Vector3.zero, new Vector3(11.2f, 11.2f, 1f),
                new Color(0.45f, 0.32f, 0.20f, 1f), -7);

            // Pit Sawdust Floor (Blood-Stained Gritty Arena)
            CreateSpriteObject(pitRoot.transform, "PitSawdustFloor", uisprite,
                Vector3.zero, new Vector3(10.8f, 10.8f, 1f),
                new Color(0.28f, 0.16f, 0.13f, 1f), -6);

            // Inner Ring Octagon / Circle
            CreateSpriteObject(pitRoot.transform, "PitInnerBloodRing", knob,
                Vector3.zero, new Vector3(8.5f, 8.5f, 1f),
                new Color(0.42f, 0.18f, 0.14f, 0.9f), -5);
            CreateSpriteObject(pitRoot.transform, "PitBloodCenterDecal", knob,
                Vector3.zero, new Vector3(4.5f, 4.5f, 1f),
                new Color(0.55f, 0.12f, 0.10f, 0.7f), -4);

            CreateWorldSign(pitRoot.transform, "Sign_PitArena", new Vector3(0f, 5.9f, 0f),
                "THE GUTTER PIT", new Color(1f, 0.65f, 0.15f, 1f),
                "Unsanctioned Arena • No Rules", new Color(0.95f, 0.4f, 0.35f, 1f));

            // Solid Rusted Cage Barriers & Colliders
            CreatePitCageFence(pitRoot.transform, "PitFence_North", new Vector2(0f, 5.2f), new Vector2(10.8f, 0.7f), uisprite);
            CreatePitCageFence(pitRoot.transform, "PitFence_South", new Vector2(0f, -5.2f), new Vector2(10.8f, 0.7f), uisprite);
            CreatePitCageFence(pitRoot.transform, "PitFence_East", new Vector2(5.2f, 0f), new Vector2(0.7f, 10.8f), uisprite);
            // West fence has open gate gap at y = -1 (local y for gate)
            CreatePitCageFence(pitRoot.transform, "PitFence_West_Top", new Vector2(-5.2f, 2.7f), new Vector2(0.7f, 5.2f), uisprite);
            CreatePitCageFence(pitRoot.transform, "PitFence_West_Bottom", new Vector2(-5.2f, -3.7f), new Vector2(0.7f, 3.2f), uisprite);

            // Gate Stanchion Towers with Amber Beacons
            CreateGateStanchion(pitRoot.transform, "GateTower_Top", new Vector2(-5.2f, 0.2f), uisprite, knob);
            CreateGateStanchion(pitRoot.transform, "GateTower_Bottom", new Vector2(-5.2f, -2.0f), uisprite, knob);

            // Gate Threshold Walkway (Visual entrance ramp)
            CreateSpriteObject(pitRoot.transform, "GateEntranceRamp", uisprite,
                new Vector3(-5.2f, -0.9f, 0f), new Vector3(1.8f, 2.0f, 1f),
                new Color(0.38f, 0.25f, 0.18f, 1f), -4);

            // Pitmaster Jax's Booking Station
            var jaxRoot = new GameObject("PitmasterJax_Station");
            jaxRoot.transform.SetParent(envRoot.transform);
            jaxRoot.transform.position = new Vector3(1.8f, 2.6f, 0f);

            // Elevated Platform
            CreateSpriteObject(jaxRoot.transform, "Jax_Platform", uisprite,
                Vector3.zero, new Vector3(3.2f, 3.0f, 1f),
                new Color(0.35f, 0.25f, 0.15f, 1f), -5);
            CreateSpriteObject(jaxRoot.transform, "Jax_PlatformBorder", uisprite,
                Vector3.zero, new Vector3(3.4f, 3.2f, 1f),
                new Color(0.85f, 0.55f, 0.1f, 1f), -6);

            // Interaction Ring Under Jax
            CreateSpriteObject(jaxRoot.transform, "Jax_InteractionRing", knob,
                Vector3.zero, new Vector3(2.6f, 2.6f, 1f),
                new Color(1f, 0.7f, 0.1f, 0.45f), -4);

            // Overhead Banner
            CreateSpriteObject(jaxRoot.transform, "Jax_BannerBoard", uisprite,
                new Vector3(0f, 2.0f, 0f), new Vector3(4.6f, 0.9f, 1f),
                new Color(0.12f, 0.08f, 0.03f, 0.95f), 5);
            CreateSpriteObject(jaxRoot.transform, "Jax_BannerBorder", uisprite,
                new Vector3(0f, 2.0f, 0f), new Vector3(4.8f, 1.05f, 1f),
                new Color(1f, 0.7f, 0.1f, 1f), 4);

            CreateWorldSign(jaxRoot.transform, "Sign_JaxBanner", new Vector3(0f, 2.0f, 0f),
                "PITMASTER JAX", new Color(1f, 0.8f, 0.2f, 1f),
                "[ F ] Place Wager (2.5x Payout)", new Color(0.9f, 0.9f, 0.85f, 1f));

            // Pitmaster Jax NPC
            var jaxObj = new GameObject("Pitmaster_Jax");
            jaxObj.transform.SetParent(jaxRoot.transform);
            jaxObj.transform.localPosition = Vector3.zero;
            var jaxSr = jaxObj.AddComponent<SpriteRenderer>();
            jaxSr.sprite = knob;
            jaxSr.color = new Color(1f, 0.72f, 0.2f, 1f); // Brass/gold
            jaxObj.transform.localScale = new Vector3(7.0f, 7.0f, 1f);
            jaxSr.sortingOrder = 3;

            var jaxCol = jaxObj.AddComponent<CircleCollider2D>();
            jaxCol.isTrigger = true;
            jaxCol.radius = 2.0f / 7.0f; // Effective world radius = 2.0m (eliminates cross-hub overlap)
            var jax = jaxObj.AddComponent<PitmasterJax>();

            // Pit Brawler: Grimlock the Flesh-Render
            var brawlerObj = new GameObject("PitBrawler_Grimlock");
            brawlerObj.transform.SetParent(pitRoot.transform);
            brawlerObj.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            var brawlerSr = brawlerObj.AddComponent<SpriteRenderer>();
            brawlerSr.sprite = knob;
            brawlerSr.color = new Color(0.95f, 0.22f, 0.22f, 1f); // Menacing bright crimson
            brawlerObj.transform.localScale = new Vector3(8.0f, 8.0f, 1f);
            brawlerSr.sortingOrder = 3;

            var brawlerRb = brawlerObj.AddComponent<Rigidbody2D>();
            brawlerRb.gravityScale = 0f;
            brawlerRb.freezeRotation = true;
            brawlerRb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var brawlerCol = brawlerObj.AddComponent<CircleCollider2D>();
            brawlerCol.radius = 0.8f / 8.0f; // 0.1f -> Effective world radius = 0.8m (1.6m diameter, leaves arena open)

            var brawlerHurtbox = brawlerObj.AddComponent<Hurtbox2D>();

            var brawlerAI = brawlerObj.AddComponent<UnsanctionedPitBrawlerAI2D>();
            var brawlerSO = new SerializedObject(brawlerAI);
            brawlerSO.FindProperty("brawlerName").stringValue = "Grimlock the Flesh-Render";
            brawlerSO.FindProperty("epithet").stringValue = "Contraband-Infused Pit Champion";
            brawlerSO.FindProperty("maxHealth").floatValue = 220f;
            brawlerSO.FindProperty("defense").floatValue = 4f;
            brawlerSO.FindProperty("moveSpeed").floatValue = 4.6f;
            brawlerSO.FindProperty("attackRange").floatValue = 2.4f;
            brawlerSO.FindProperty("attackCooldown").floatValue = 1.8f;
            brawlerSO.FindProperty("attackDamage").floatValue = 22f;
            brawlerSO.FindProperty("telegraphDuration").floatValue = 0.35f;
            var pyroElement = AssetDatabase.LoadAssetAtPath<ElementalTypeSO>("Assets/Data/Elements/Element_Pyro.asset");
            if (pyroElement != null) brawlerSO.FindProperty("attackElement").objectReferenceValue = pyroElement;
            brawlerSO.FindProperty("spriteRenderer").objectReferenceValue = brawlerSr;
            brawlerSO.ApplyModifiedProperties();

            // ==========================================
            // 7. SPAWN POINTS & WAGER CONTROLLER
            // ==========================================
            var pitSpawnPlayer = new GameObject("PitSpawn_Player");
            pitSpawnPlayer.transform.SetParent(pitRoot.transform);
            pitSpawnPlayer.transform.localPosition = new Vector3(0f, -2.0f, 0f); // 3.2m north of south fence

            var pitSpawnBrawler = new GameObject("PitSpawn_Brawler");
            pitSpawnBrawler.transform.SetParent(pitRoot.transform);
            pitSpawnBrawler.transform.localPosition = new Vector3(0f, 2.0f, 0f);

            var hubExitPoint = new GameObject("HubExit_Point");
            hubExitPoint.transform.SetParent(jaxRoot.transform);
            hubExitPoint.transform.localPosition = new Vector3(0f, -1.8f, 0f);

            var wagerCtrlObj = new GameObject("PitMatchWagerController");
            var wagerCtrl = wagerCtrlObj.AddComponent<PitMatchWagerController>();

            // ==========================================
            // 8. PLAYER CHARACTER SETUP
            // ==========================================
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerObj;
            if (playerPrefab != null)
            {
                playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerObj.name = "Player";
                playerObj.transform.position = new Vector3(0f, -6.0f, 0f); // Centered on entrance runway
            }
            else
            {
                playerObj = new GameObject("Player");
                playerObj.transform.position = new Vector3(0f, -6.0f, 0f);
                playerObj.AddComponent<SpriteRenderer>().sprite = knob;
                playerObj.AddComponent<Rigidbody2D>().gravityScale = 0f;
                playerObj.AddComponent<CapsuleCollider2D>();
                playerObj.AddComponent<Hurtbox2D>();
                playerObj.AddComponent<PlayerController2D>();
                playerObj.AddComponent<PlayerStatsComponent>();
                playerObj.AddComponent<PlayerWallet>();
                playerObj.AddComponent<PlayerMonsterRoster>();
                playerObj.AddComponent<PlayerInfuseManager>();
                playerObj.AddComponent<PlayerCombatExecutor>();
                playerObj.AddComponent<CooldownController>();
            }

            // Target camera to player
            camFollow.SetTarget(playerObj.transform);

            var playerWallet = playerObj.GetComponent<PlayerWallet>();
            if (playerWallet != null)
            {
                var walletSO = new SerializedObject(playerWallet);
                walletSO.FindProperty("startingCredits").intValue = 1000;
                walletSO.ApplyModifiedProperties();
            }

            var playerInfuse = playerObj.GetComponent<PlayerInfuseManager>();
            var playerStats = playerObj.GetComponent<PlayerStatsComponent>();

            // Configure initial test loadout
            var torrentWyrm = AssetDatabase.LoadAssetAtPath<MonsterDataSO>("Assets/Data/Monsters/Monster_TorrentWyrm.asset");
            var ironhideBoar = AssetDatabase.LoadAssetAtPath<MonsterDataSO>("Assets/Data/Monsters/Monster_IronhideBoar.asset");
            var voltMantis = AssetDatabase.LoadAssetAtPath<MonsterDataSO>("Assets/Data/Monsters/Monster_VoltMantis.asset");

            if (playerInfuse != null)
            {
                var infuseSO = new SerializedObject(playerInfuse);
                if (torrentWyrm != null) infuseSO.FindProperty("initialHeadMonster").objectReferenceValue = torrentWyrm;
                if (ironhideBoar != null) infuseSO.FindProperty("initialChestMonster").objectReferenceValue = ironhideBoar;
                if (voltMantis != null) infuseSO.FindProperty("initialRightArmMonster").objectReferenceValue = voltMantis;
                if (torrentWyrm != null) infuseSO.FindProperty("initialLeftArmMonster").objectReferenceValue = torrentWyrm;
                if (voltMantis != null) infuseSO.FindProperty("initialLegsMonster").objectReferenceValue = voltMantis;
                infuseSO.ApplyModifiedProperties();
            }

            // Wire WagerController
            var wagerSO = new SerializedObject(wagerCtrl);
            wagerSO.FindProperty("playerStats").objectReferenceValue = playerStats;
            wagerSO.FindProperty("playerWallet").objectReferenceValue = playerWallet;
            wagerSO.FindProperty("brawler").objectReferenceValue = brawlerAI;
            wagerSO.FindProperty("playerSpawnPoint").objectReferenceValue = pitSpawnPlayer.transform;
            wagerSO.FindProperty("brawlerSpawnPoint").objectReferenceValue = pitSpawnBrawler.transform;
            wagerSO.FindProperty("hubExitPoint").objectReferenceValue = hubExitPoint.transform;
            wagerSO.FindProperty("payoutMultiplier").floatValue = 2.5f;
            wagerSO.ApplyModifiedProperties();

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

            // Ripperdoc UI
            BuildRipperdocUI(canvasObj.transform, defaultFont, uisprite, bgSprite);

            // Pit Match Wager UI
            BuildPitMatchWagerUI(canvasObj.transform, wagerCtrl, defaultFont, uisprite, bgSprite);

            // Scene Transition Prompt UI (Bottom-Center Banner)
            BuildSceneTransitionPromptHUD(canvasObj.transform, defaultFont, uisprite);

            // ==========================================
            // 10. SAVE SCENE
            // ==========================================
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Underground_BlackMarket.unity");
            Debug.Log("<color=#00FFAA>[Scene Builder]</color> Successfully overhauled <b>Assets/Scenes/Underground_BlackMarket.unity</b> with high-clarity geometry!");
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

            // Main wall body
            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = new Color(0.24f, 0.28f, 0.38f, 1f); // Solid industrial steel
            sr.sortingOrder = 2;
            wall.transform.localScale = Vector3.one;

            // Wall Bevel Rim (Lighter trim)
            var rim = new GameObject(name + "_Rim");
            rim.transform.SetParent(wall.transform, false);
            var rsr = rim.AddComponent<SpriteRenderer>();
            rsr.sprite = uisprite;
            rsr.drawMode = SpriteDrawMode.Sliced;
            float rimThickness = Mathf.Min(0.35f, size.y * 0.4f);
            rsr.size = new Vector2(size.x * 0.98f, rimThickness);
            rsr.color = new Color(0.42f, 0.48f, 0.62f, 1f);
            rsr.sortingOrder = 3;
            rim.transform.localPosition = new Vector3(0f, (size.y - rimThickness) * 0.5f, 0f);

            // Bottom Hazard Stripe Trim (Marks exact collision threshold)
            var hazard = new GameObject(name + "_HazardStripe");
            hazard.transform.SetParent(wall.transform, false);
            var hsr = hazard.AddComponent<SpriteRenderer>();
            hsr.sprite = uisprite;
            hsr.drawMode = SpriteDrawMode.Sliced;
            float hazardThickness = Mathf.Min(0.25f, size.y * 0.25f);
            hsr.size = new Vector2(size.x * 0.98f, hazardThickness);
            hsr.color = new Color(0.95f, 0.75f, 0.05f, 1f); // Hazard yellow
            hsr.sortingOrder = 3;
            hazard.transform.localPosition = new Vector3(0f, -(size.y - hazardThickness) * 0.5f, 0f);
        }

        private static void CreatePillar(Transform parent, string name, Vector2 pos, Sprite uisprite)
        {
            var pillar = new GameObject(name);
            pillar.transform.SetParent(parent);
            pillar.transform.position = pos;

            var col = pillar.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.2f, 2.2f);

            var sr = pillar.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(2.2f, 2.2f);
            sr.color = new Color(0.20f, 0.24f, 0.32f, 1f);
            sr.sortingOrder = 3;
            pillar.transform.localScale = Vector3.one;

            var cap = new GameObject(name + "_Cap");
            cap.transform.SetParent(pillar.transform, false);
            var csr = cap.AddComponent<SpriteRenderer>();
            csr.sprite = uisprite;
            csr.drawMode = SpriteDrawMode.Sliced;
            csr.size = new Vector2(1.5f, 1.5f);
            csr.color = new Color(0.42f, 0.50f, 0.65f, 1f);
            csr.sortingOrder = 4;
        }

        private static void CreatePitCageFence(Transform parent, string name, Vector2 localPos, Vector2 size, Sprite uisprite)
        {
            var fence = new GameObject(name);
            fence.transform.SetParent(parent);
            fence.transform.localPosition = localPos;

            var col = fence.AddComponent<BoxCollider2D>();
            col.size = size;

            // Visible barrier body
            var sr = fence.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = new Color(0.48f, 0.35f, 0.24f, 1f); // Rusted heavy iron
            sr.sortingOrder = 2;
            fence.transform.localScale = Vector3.one;

            // Caution rail trim
            var rail = new GameObject(name + "_TopRail");
            rail.transform.SetParent(fence.transform, false);
            var rsr = rail.AddComponent<SpriteRenderer>();
            rsr.sprite = uisprite;
            rsr.drawMode = SpriteDrawMode.Sliced;
            float railThickness = Mathf.Min(0.25f, size.y * 0.35f);
            rsr.size = new Vector2(size.x * 0.95f, railThickness);
            rsr.color = new Color(0.95f, 0.60f, 0.15f, 1f); // Warning orange
            rsr.sortingOrder = 3;
            rail.transform.localPosition = new Vector3(0f, (size.y - railThickness) * 0.5f, 0f);
        }

        private static void CreateGateStanchion(Transform parent, string name, Vector2 localPos, Sprite uisprite, Sprite knob)
        {
            var tower = new GameObject(name);
            tower.transform.SetParent(parent, false);
            tower.transform.localPosition = localPos;

            var sr = tower.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.9f, 1.6f);
            sr.color = new Color(0.30f, 0.24f, 0.18f, 1f);
            sr.sortingOrder = 3;
            tower.transform.localScale = Vector3.one;

            // Flashing amber beacon
            var beacon = new GameObject("AmberBeacon");
            beacon.transform.SetParent(tower.transform, false);
            beacon.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var bsr = beacon.AddComponent<SpriteRenderer>();
            bsr.sprite = knob;
            bsr.color = new Color(1f, 0.75f, 0.1f, 1f);
            bsr.sortingOrder = 4;
            float knobUnit = knob.rect.width / knob.pixelsPerUnit;
            beacon.transform.localScale = new Vector3(0.55f / knobUnit, 0.35f / knobUnit, 1f);
        }

        private static void CreateWorldSign(Transform parent, string name, Vector3 localPos, string mainText, Color mainColor, string subText = null, Color? subColor = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;

            var tm = obj.AddComponent<TextMesh>();
            tm.text = mainText;
            tm.fontSize = 48;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = mainColor;
            tm.fontStyle = FontStyle.Bold;

            var mr = obj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 15;

            if (!string.IsNullOrEmpty(subText))
            {
                var subObj = new GameObject(name + "_Sub");
                subObj.transform.SetParent(obj.transform, false);
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
            txt.text = "[ F ] Climb Stairs to Central Metro District";
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

        private static void BuildRipperdocUI(Transform canvasTransform, Font font, Sprite uisprite, Sprite bgSprite)
        {
            var managerObj = new GameObject("RipperdocManager");
            managerObj.transform.SetParent(canvasTransform, false);
            var mrt = managerObj.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.sizeDelta = Vector2.zero;

            var ripperUI = managerObj.AddComponent<RipperdocUI>();

            // 1. Prompt Panel (Bottom Center)
            var promptObj = new GameObject("RipperdocPromptPanel");
            promptObj.transform.SetParent(managerObj.transform, false);
            var prt = promptObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.16f);
            prt.anchorMax = new Vector2(0.5f, 0.16f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(580f, 60f);

            var pbg = promptObj.AddComponent<Image>();
            pbg.sprite = uisprite;
            pbg.color = new Color(0.1f, 0.04f, 0.06f, 0.94f);
            pbg.raycastTarget = false;

            var pTextObj = new GameObject("PromptText");
            pTextObj.transform.SetParent(promptObj.transform, false);
            var ptrt = pTextObj.AddComponent<RectTransform>();
            ptrt.anchorMin = Vector2.zero;
            ptrt.anchorMax = Vector2.one;
            ptrt.sizeDelta = Vector2.zero;
            var ptxt = pTextObj.AddComponent<Text>();
            ptxt.font = font;
            ptxt.fontSize = 24;
            ptxt.fontStyle = FontStyle.Bold;
            ptxt.alignment = TextAnchor.MiddleCenter;
            ptxt.color = new Color(1f, 0.25f, 0.4f, 1f);
            ptxt.text = "[ F ] Consult Dr. Silas (Ripperdoc)";
            ptxt.raycastTarget = false;
            ptxt.verticalOverflow = VerticalWrapMode.Overflow;

            var pOutline = pTextObj.AddComponent<Outline>();
            pOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            pOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // 2. Modal Panel
            var modalObj = new GameObject("RipperdocModalPanel");
            modalObj.transform.SetParent(managerObj.transform, false);
            var mort = modalObj.AddComponent<RectTransform>();
            mort.anchorMin = new Vector2(0.5f, 0.5f);
            mort.anchorMax = new Vector2(0.5f, 0.5f);
            mort.pivot = new Vector2(0.5f, 0.5f);
            mort.sizeDelta = new Vector2(920f, 640f);

            var mbg = modalObj.AddComponent<Image>();
            mbg.sprite = bgSprite;
            mbg.color = new Color(0.08f, 0.04f, 0.06f, 0.96f);

            // Title & Subtitle
            var titleTxt = CreateText(modalObj.transform, "Title", font, 32, FontStyle.Bold, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(800f, 45f),
                new Color(1f, 0.2f, 0.35f, 1f), "DR. SILAS'S NEURO-CLINIC");

            var subTxt = CreateText(modalObj.transform, "Subtitle", font, 20, FontStyle.Italic, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -75f), new Vector2(800f, 30f),
                new Color(0.75f, 0.75f, 0.8f, 1f), "Unlicensed Neuro-Chirurgeon • Contraband Bio-Augments");

            // Product Title & Warning Badge
            var prodTitleTxt = CreateText(modalObj.transform, "ProdTitle", font, 28, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-100f, -130f), new Vector2(550f, 40f),
                new Color(1f, 0.85f, 0.2f, 1f), "OVERCLOCKED SPECIMEN");

            var warnBadgeTxt = CreateText(modalObj.transform, "WarningBadge", font, 22, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, -130f), new Vector2(260f, 40f),
                new Color(1f, 0.2f, 0.3f, 1f), "[ CONTRABAND ]");

            // Description
            var descTxt = CreateText(modalObj.transform, "Description", font, 20, FontStyle.Normal, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(800f, 80f),
                Color.white, "Description text goes here.");

            // Overclock Stats
            var statsTxt = CreateText(modalObj.transform, "Stats", font, 20, FontStyle.Normal, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -285f), new Vector2(800f, 90f),
                new Color(0.2f, 0.9f, 1f, 1f), "+Overclock Attack / -Recoil HP");

            // Price & Balance
            var priceTxt = CreateText(modalObj.transform, "Price", font, 26, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-180f, 140f), new Vector2(350f, 40f),
                new Color(1f, 0.85f, 0.2f, 1f), "PRICE: 350 CR");

            var balTxt = CreateText(modalObj.transform, "Balance", font, 24, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(180f, 140f), new Vector2(350f, 40f),
                new Color(0f, 1f, 0.7f, 1f), "PURSE: 1,000 CR");

            var feedTxt = CreateText(modalObj.transform, "Feedback", font, 20, FontStyle.Italic, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(700f, 35f),
                Color.yellow, "");

            // Buttons
            var prevBtn = CreateButton(modalObj.transform, "Btn_Prev", font, "< PREV [Q]", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-260f, 50f), new Vector2(160f, 55f),
                new Color(0.2f, 0.2f, 0.25f, 1f));

            var buyBtn = CreateButton(modalObj.transform, "Btn_Buy", font, "PURCHASE & IMPLANT [ENTER]", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(340f, 55f),
                new Color(0.8f, 0.15f, 0.25f, 1f));

            var nextBtn = CreateButton(modalObj.transform, "Btn_Next", font, "NEXT [E] >", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260f, 50f), new Vector2(160f, 55f),
                new Color(0.2f, 0.2f, 0.25f, 1f));

            var closeBtn = CreateButton(modalObj.transform, "Btn_Close", font, "X", uisprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-35f, -35f), new Vector2(48f, 48f),
                new Color(0.4f, 0.1f, 0.1f, 1f), 24);

            modalObj.SetActive(false);
            promptObj.SetActive(false);

            // Serialize properties onto RipperdocUI
            var rso = new SerializedObject(ripperUI);
            rso.FindProperty("promptPanel").objectReferenceValue = promptObj;
            rso.FindProperty("modalPanel").objectReferenceValue = modalObj;
            rso.FindProperty("promptText").objectReferenceValue = ptxt;
            rso.FindProperty("doctorNameText").objectReferenceValue = titleTxt;
            rso.FindProperty("clinicSubtitleText").objectReferenceValue = subTxt;
            rso.FindProperty("productTitleText").objectReferenceValue = prodTitleTxt;
            rso.FindProperty("warningBadgeText").objectReferenceValue = warnBadgeTxt;
            rso.FindProperty("descriptionText").objectReferenceValue = descTxt;
            rso.FindProperty("overclockStatsText").objectReferenceValue = statsTxt;
            rso.FindProperty("priceText").objectReferenceValue = priceTxt;
            rso.FindProperty("playerBalanceText").objectReferenceValue = balTxt;
            rso.FindProperty("feedbackText").objectReferenceValue = feedTxt;
            rso.FindProperty("purchaseButton").objectReferenceValue = buyBtn;
            rso.FindProperty("nextButton").objectReferenceValue = nextBtn;
            rso.FindProperty("prevButton").objectReferenceValue = prevBtn;
            rso.FindProperty("closeButton").objectReferenceValue = closeBtn;
            rso.ApplyModifiedProperties();
        }

        private static void BuildPitMatchWagerUI(Transform canvasTransform, PitMatchWagerController wagerCtrl, Font font, Sprite uisprite, Sprite bgSprite)
        {
            var managerObj = new GameObject("PitMatchWagerManager");
            managerObj.transform.SetParent(canvasTransform, false);
            var mrt = managerObj.AddComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.sizeDelta = Vector2.zero;

            var wagerUI = managerObj.AddComponent<PitMatchWagerUI>();

            // 1. Prompt Panel
            var promptObj = new GameObject("PitPromptPanel");
            promptObj.transform.SetParent(managerObj.transform, false);
            var prt = promptObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.16f);
            prt.anchorMax = new Vector2(0.5f, 0.16f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(580f, 60f);

            var pbg = promptObj.AddComponent<Image>();
            pbg.sprite = uisprite;
            pbg.color = new Color(0.12f, 0.08f, 0.02f, 0.94f);
            pbg.raycastTarget = false;

            var ptxt = CreateText(promptObj.transform, "PromptText", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(1f, 0.75f, 0.2f, 1f), "[ F ] Talk to Pitmaster Jax (Place Wager)");

            // 2. Wager Selection Modal
            var modalObj = new GameObject("WagerModalPanel");
            modalObj.transform.SetParent(managerObj.transform, false);
            var mort = modalObj.AddComponent<RectTransform>();
            mort.anchorMin = new Vector2(0.5f, 0.5f);
            mort.anchorMax = new Vector2(0.5f, 0.5f);
            mort.pivot = new Vector2(0.5f, 0.5f);
            mort.sizeDelta = new Vector2(920f, 640f);

            var mbg = modalObj.AddComponent<Image>();
            mbg.sprite = bgSprite;
            mbg.color = new Color(0.10f, 0.07f, 0.04f, 0.96f);

            var titleTxt = CreateText(modalObj.transform, "ModalTitle", font, 32, FontStyle.Bold, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(800f, 45f),
                new Color(1f, 0.65f, 0.15f, 1f), "UNSANCTIONED PIT DEATHMATCH");

            var subTxt = CreateText(modalObj.transform, "ModalSubtitle", font, 20, FontStyle.Italic, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -75f), new Vector2(800f, 30f),
                new Color(0.85f, 0.8f, 0.7f, 1f), "Pitmaster Jax: 'Place yer bet, fleshbag. Winner takes all—2.5x payout!'");

            // Wager Tier Buttons
            var btn100 = CreateButton(modalObj.transform, "Btn_Wager100", font, "100 CR\n(Payout: 250 CR)", uisprite,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-220f, -165f), new Vector2(190f, 75f),
                new Color(0.25f, 0.2f, 0.15f, 1f));

            var btn250 = CreateButton(modalObj.transform, "Btn_Wager250", font, "250 CR\n(Payout: 625 CR)", uisprite,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(190f, 75f),
                new Color(0.45f, 0.3f, 0.1f, 1f));

            var btn500 = CreateButton(modalObj.transform, "Btn_Wager500", font, "500 CR\n(Payout: 1,250 CR)", uisprite,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(220f, -165f), new Vector2(190f, 75f),
                new Color(0.25f, 0.2f, 0.15f, 1f));

            var wagerTxt = CreateText(modalObj.transform, "SelectedWagerText", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -255f), new Vector2(700f, 40f),
                new Color(1f, 0.85f, 0.2f, 1f), "Selected Wager: 250 Credits");

            var payoutTxt = CreateText(modalObj.transform, "PayoutInfoText", font, 22, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(700f, 40f),
                new Color(0f, 1f, 0.7f, 1f), "Potential Payout: 625 Credits (+375 Net @ 2.5x)");

            var balanceTxt = CreateText(modalObj.transform, "WalletBalanceText", font, 24, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(600f, 35f),
                new Color(0.3f, 0.8f, 1f, 1f), "Purse Balance: 1,000 Credits");

            var enterBtn = CreateButton(modalObj.transform, "Btn_EnterPit", font, "BET & ENTER THE PIT [SPACE]", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(380f, 55f),
                new Color(0.85f, 0.35f, 0.1f, 1f), 24);

            var closeWagerBtn = CreateButton(modalObj.transform, "Btn_CloseWager", font, "X", uisprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-35f, -35f), new Vector2(48f, 48f),
                new Color(0.4f, 0.15f, 0.1f, 1f), 24);

            // 3. Central Announcer Banner
            var bannerRoot = new GameObject("PitAnnouncerBanner");
            bannerRoot.transform.SetParent(managerObj.transform, false);
            var brt = bannerRoot.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.6f);
            brt.anchorMax = new Vector2(0.5f, 0.6f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(800f, 130f);

            var bnbg = bannerRoot.AddComponent<Image>();
            bnbg.sprite = uisprite;
            bnbg.color = new Color(0.08f, 0.04f, 0.04f, 0.95f);
            bnbg.raycastTarget = false;

            var annMain = CreateText(bannerRoot.transform, "MainText", font, 54, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(750f, 65f),
                new Color(1f, 0.8f, 0.1f, 1f), "3");

            var annSub = CreateText(bannerRoot.transform, "SubText", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(750f, 35f),
                new Color(0.9f, 0.9f, 0.9f, 1f), "UNSANCTIONED DEATHMATCH COMMENCING");

            // 4. Brawler Boss Bar (Top Center)
            var bossBarRoot = new GameObject("BrawlerBossBar");
            bossBarRoot.transform.SetParent(managerObj.transform, false);
            var bbrt = bossBarRoot.AddComponent<RectTransform>();
            bbrt.anchorMin = new Vector2(0.5f, 1f);
            bbrt.anchorMax = new Vector2(0.5f, 1f);
            bbrt.pivot = new Vector2(0.5f, 1f);
            bbrt.anchoredPosition = new Vector2(0f, -35f);
            bbrt.sizeDelta = new Vector2(720f, 65f);

            var bbBg = bossBarRoot.AddComponent<Image>();
            bbBg.sprite = uisprite;
            bbBg.color = new Color(0.12f, 0.06f, 0.06f, 0.95f);
            bbBg.raycastTarget = false;

            var bNameTxt = CreateText(bossBarRoot.transform, "BrawlerName", font, 24, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0.03f, 0.5f), new Vector2(0.65f, 0.95f), Vector2.zero, Vector2.zero,
                new Color(1f, 0.35f, 0.35f, 1f), "Grimlock the Flesh-Render");

            var bEpiTxt = CreateText(bossBarRoot.transform, "BrawlerEpithet", font, 18, FontStyle.Italic, TextAnchor.MiddleRight,
                new Vector2(0.65f, 0.5f), new Vector2(0.97f, 0.95f), Vector2.zero, Vector2.zero,
                new Color(0.9f, 0.7f, 0.4f, 1f), "Pit Champion");

            // Health Bar Background & Fill
            var hbBgObj = new GameObject("HealthBar_BG");
            hbBgObj.transform.SetParent(bossBarRoot.transform, false);
            var hbrt = hbBgObj.AddComponent<RectTransform>();
            hbrt.anchorMin = new Vector2(0.5f, 0f);
            hbrt.anchorMax = new Vector2(0.5f, 0f);
            hbrt.pivot = new Vector2(0.5f, 0.5f);
            hbrt.anchoredPosition = new Vector2(0f, 18f);
            hbrt.sizeDelta = new Vector2(680f, 24f);
            var hbBgImg = hbBgObj.AddComponent<Image>();
            hbBgImg.sprite = uisprite;
            hbBgImg.color = new Color(0.25f, 0.1f, 0.1f, 1f);
            hbBgImg.raycastTarget = false;

            var hbFillObj = new GameObject("HealthBar_Fill");
            hbFillObj.transform.SetParent(hbBgObj.transform, false);
            var hfrt = hbFillObj.AddComponent<RectTransform>();
            hfrt.anchorMin = Vector2.zero;
            hfrt.anchorMax = Vector2.one;
            hfrt.sizeDelta = Vector2.zero;
            var hbFillImg = hbFillObj.AddComponent<Image>();
            hbFillImg.sprite = uisprite;
            hbFillImg.type = Image.Type.Filled;
            hbFillImg.fillMethod = Image.FillMethod.Horizontal;
            hbFillImg.fillOrigin = 0;
            hbFillImg.fillAmount = 1.0f;
            hbFillImg.color = new Color(0.95f, 0.2f, 0.2f, 1f);
            hbFillImg.raycastTarget = false;

            var hbNumTxt = CreateText(hbBgObj.transform, "HealthNum", font, 20, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                Color.white, "220 / 220 HP");

            // 5. Outcome Modal
            var outcomeObj = new GameObject("OutcomeModalPanel");
            outcomeObj.transform.SetParent(managerObj.transform, false);
            var ort = outcomeObj.AddComponent<RectTransform>();
            ort.anchorMin = new Vector2(0.5f, 0.5f);
            ort.anchorMax = new Vector2(0.5f, 0.5f);
            ort.pivot = new Vector2(0.5f, 0.5f);
            ort.sizeDelta = new Vector2(720f, 440f);

            var obg = outcomeObj.AddComponent<Image>();
            obg.sprite = bgSprite;
            obg.color = new Color(0.08f, 0.05f, 0.05f, 0.98f);

            var outTitle = CreateText(outcomeObj.transform, "OutcomeTitle", font, 36, FontStyle.Bold, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(650f, 50f),
                new Color(0f, 1f, 0.7f, 1f), "PIT CHAMPION!");

            var outDetails = CreateText(outcomeObj.transform, "OutcomeDetails", font, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(650f, 140f),
                Color.white, "You butchered the brawler and seized the purse!\n\n+625 Credits");

            var retHubBtn = CreateButton(outcomeObj.transform, "Btn_ReturnHub", font, "COLLECT & RETURN TO HUB [ENTER]", uisprite,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(400f, 55f),
                new Color(0.2f, 0.6f, 0.4f, 1f), 22);

            promptObj.SetActive(false);
            modalObj.SetActive(false);
            bannerRoot.SetActive(false);
            bossBarRoot.SetActive(false);
            outcomeObj.SetActive(false);

            // Serialize properties onto PitMatchWagerUI
            var wso = new SerializedObject(wagerUI);
            wso.FindProperty("wagerController").objectReferenceValue = wagerCtrl;
            wso.FindProperty("promptPanel").objectReferenceValue = promptObj;
            wso.FindProperty("promptText").objectReferenceValue = ptxt;
            wso.FindProperty("wagerModalPanel").objectReferenceValue = modalObj;
            wso.FindProperty("modalTitleText").objectReferenceValue = titleTxt;
            wso.FindProperty("modalSubtitleText").objectReferenceValue = subTxt;
            wso.FindProperty("wager100Btn").objectReferenceValue = btn100;
            wso.FindProperty("wager250Btn").objectReferenceValue = btn250;
            wso.FindProperty("wager500Btn").objectReferenceValue = btn500;
            wso.FindProperty("selectedWagerText").objectReferenceValue = wagerTxt;
            wso.FindProperty("payoutInfoText").objectReferenceValue = payoutTxt;
            wso.FindProperty("walletBalanceText").objectReferenceValue = balanceTxt;
            wso.FindProperty("enterPitBtn").objectReferenceValue = enterBtn;
            wso.FindProperty("closeBtn").objectReferenceValue = closeWagerBtn;

            wso.FindProperty("announcerBannerRoot").objectReferenceValue = bannerRoot;
            wso.FindProperty("announcerMainText").objectReferenceValue = annMain;
            wso.FindProperty("announcerSubText").objectReferenceValue = annSub;

            wso.FindProperty("brawlerBarRoot").objectReferenceValue = bossBarRoot;
            wso.FindProperty("brawlerNameText").objectReferenceValue = bNameTxt;
            wso.FindProperty("brawlerEpithetText").objectReferenceValue = bEpiTxt;
            wso.FindProperty("brawlerHealthFill").objectReferenceValue = hbFillImg;
            wso.FindProperty("brawlerHealthNumText").objectReferenceValue = hbNumTxt;

            wso.FindProperty("outcomeModalPanel").objectReferenceValue = outcomeObj;
            wso.FindProperty("outcomeTitleText").objectReferenceValue = outTitle;
            wso.FindProperty("outcomeDetailsText").objectReferenceValue = outDetails;
            wso.FindProperty("returnHubBtn").objectReferenceValue = retHubBtn;
            wso.ApplyModifiedProperties();
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
