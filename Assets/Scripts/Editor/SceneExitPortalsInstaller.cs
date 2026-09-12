using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using BeastClad.World;
using BeastClad.UI;

namespace BeastClad.Editor
{
    public static class SceneExitPortalsInstaller
    {
        [MenuItem("BeastClad/Install Scene Exit Portals")]
        public static void InstallAllExitPortals()
        {
            InstallArenaColosseumPortals();
            InstallWildernessPortals();
        }

        public static void InstallArenaColosseumPortals()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Arena_Colosseum.unity");
            Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Check if already installed
            var existingExit = GameObject.Find("Exit_ToCentralHub");
            if (existingExit != null) Object.DestroyImmediate(existingExit);

            var existingSpawn = GameObject.Find("Spawn_FromHub");
            if (existingSpawn != null) Object.DestroyImmediate(existingSpawn);

            // Exit Portal at (-8.0, -3.5, 0)
            var exitObj = new GameObject("Exit_ToCentralHub");
            exitObj.transform.position = new Vector3(-8.0f, -3.5f, 0f);

            var sr = exitObj.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(4.5f, 2.5f);
            sr.color = new Color(0.25f, 0.22f, 0.15f, 0.95f);
            sr.sortingOrder = 1;

            var col = exitObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(4.5f, 2.5f);

            var trig = exitObj.AddComponent<SceneTransitionTrigger>();
            var tso = new SerializedObject(trig);
            tso.FindProperty("targetSceneName").stringValue = "District_CentralHub";
            tso.FindProperty("targetSpawnTag").stringValue = "Spawn_FromArena";
            tso.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            tso.FindProperty("promptText").stringValue = "[ F ] Return to Central Metro District";
            tso.ApplyModifiedProperties();

            // Sign
            var signObj = new GameObject("Sign_ExitHub");
            signObj.transform.SetParent(exitObj.transform, false);
            signObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var tm = signObj.AddComponent<TextMesh>();
            tm.text = "⬅ GATES TO CENTRAL DISTRICT\n[ F ] Depart to Central Concourse";
            tm.fontSize = 20;
            tm.characterSize = 0.05f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 0.85f, 0.2f, 1f);
            var mr = signObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 15;

            // Spawn point for arrival from Hub
            var spawnObj = new GameObject("Spawn_FromHub");
            spawnObj.transform.position = new Vector3(-5.0f, 0f, 0f);
            var sp = spawnObj.AddComponent<SceneSpawnPoint>();
            var spso = new SerializedObject(sp);
            spso.FindProperty("spawnTag").stringValue = "Spawn_FromHub";
            spso.FindProperty("defaultFacing").vector2Value = Vector2.right;
            spso.ApplyModifiedProperties();

            // Ensure prompt UI on HUD Canvas
            EnsurePromptUI(defaultFont, uisprite);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=#00FFAA>[ExitPortals]</color> Installed exit portal in <b>Arena_Colosseum.unity</b>.");
        }

        public static void InstallWildernessPortals()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Adventurer_Wilderness.unity");
            Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Check if already installed
            var existingExit = GameObject.Find("Exit_ToCentralHub");
            if (existingExit != null) Object.DestroyImmediate(existingExit);

            var existingSpawn = GameObject.Find("Spawn_FromHub");
            if (existingSpawn != null) Object.DestroyImmediate(existingSpawn);

            // Exit Portal at (0, -5.5, 0)
            var exitObj = new GameObject("Exit_ToCentralHub");
            exitObj.transform.position = new Vector3(0f, -5.5f, 0f);

            var sr = exitObj.AddComponent<SpriteRenderer>();
            sr.sprite = uisprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(4.5f, 2.2f);
            sr.color = new Color(0.18f, 0.26f, 0.20f, 0.95f);
            sr.sortingOrder = 1;

            var col = exitObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(4.5f, 2.2f);

            var trig = exitObj.AddComponent<SceneTransitionTrigger>();
            var tso = new SerializedObject(trig);
            tso.FindProperty("targetSceneName").stringValue = "District_CentralHub";
            tso.FindProperty("targetSpawnTag").stringValue = "Spawn_FromWilderness";
            tso.FindProperty("mode").enumValueIndex = (int)TransitionTriggerMode.InteractPrompt;
            tso.FindProperty("promptText").stringValue = "[ F ] Return to Central Metro District";
            tso.ApplyModifiedProperties();

            // Sign
            var signObj = new GameObject("Sign_ExitHub");
            signObj.transform.SetParent(exitObj.transform, false);
            signObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            var tm = signObj.AddComponent<TextMesh>();
            tm.text = "⬇ TRAIL TO CENTRAL METRO DISTRICT\n[ F ] Return to Central Concourse";
            tm.fontSize = 20;
            tm.characterSize = 0.05f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.2f, 0.95f, 0.45f, 1f);
            var mr = signObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 15;

            // Spawn point for arrival from Hub
            var spawnObj = new GameObject("Spawn_FromHub");
            spawnObj.transform.position = new Vector3(0f, -4.0f, 0f);
            var sp = spawnObj.AddComponent<SceneSpawnPoint>();
            var spso = new SerializedObject(sp);
            spso.FindProperty("spawnTag").stringValue = "Spawn_FromHub";
            spso.FindProperty("defaultFacing").vector2Value = Vector2.up;
            spso.ApplyModifiedProperties();

            // Ensure prompt UI on HUD Canvas
            EnsurePromptUI(defaultFont, uisprite);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=#00FFAA>[ExitPortals]</color> Installed exit portal in <b>Adventurer_Wilderness.unity</b>.");
        }

        private static void EnsurePromptUI(Font font, Sprite uisprite)
        {
            var hud = GameObject.Find("HUD_Canvas");
            if (hud == null) return;

            var existing = hud.transform.Find("SceneTransitionPromptHUD");
            if (existing != null) return;

            var promptRoot = new GameObject("SceneTransitionPromptHUD");
            promptRoot.transform.SetParent(hud.transform, false);
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
            txt.text = "[ F ] Return to Central Metro District";
            txt.raycastTarget = false;

            var promptUI = promptRoot.AddComponent<SceneTransitionPromptUI>();
            var pso = new SerializedObject(promptUI);
            pso.FindProperty("promptPanel").objectReferenceValue = promptRoot;
            pso.FindProperty("promptText").objectReferenceValue = txt;
            pso.ApplyModifiedProperties();
        }
    }
}
