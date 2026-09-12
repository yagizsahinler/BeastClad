using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeastClad.Editor
{
    public static class BuildSettingsConfigurator
    {
        private static readonly string[] ScenePaths = new string[]
        {
            "Assets/Scenes/District_CentralHub.unity",
            "Assets/Scenes/Adventurer_Wilderness.unity",
            "Assets/Scenes/Arena_Colosseum.unity",
            "Assets/Scenes/Underground_BlackMarket.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        [MenuItem("BeastClad/Configure Build Scenes")]
        public static void ConfigureBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();

            foreach (var path in ScenePaths)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset != null)
                {
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                }
                else
                {
                    Debug.LogWarning($"[BuildSettingsConfigurator] Scene asset not found at '{path}'. It will be added once built.");
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"<color=#00FFAA>[BuildSettingsConfigurator]</color> Successfully configured {scenes.Count} scenes in EditorBuildSettings!");
        }
    }
}
