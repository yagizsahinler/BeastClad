using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BeastClad.Editor
{
    /// <summary>
    /// Master automation pipeline that sequentially executes all scene builders,
    /// applies high-fidelity 2D lighting, post-processing volume profiles,
    /// and ensures all 4 district scenes are registered in EditorBuildSettings.
    /// </summary>
    public static class MasterWorldVisualOverhaul
    {
        [MenuItem("BeastClad/Overhaul All Scene Visuals")]
        public static void OverhaulAllScenes()
        {
            Debug.Log("<color=#38BDF8>[MasterVisualOverhaul]</color> Starting comprehensive visual overhaul across all scenes...");

            // 1. Central Metro District Hub
            Debug.Log("<color=#38BDF8>[1/4]</color> Building District_CentralHub.unity...");
            CentralDistrictSceneBuilder.BuildScene();

            // 2. Grand Colosseum (Arena)
            Debug.Log("<color=#38BDF8>[2/4]</color> Building Arena_Colosseum.unity...");
            ArenaColosseumSceneBuilder.BuildScene();

            // 3. The Outlands Wilderness
            Debug.Log("<color=#38BDF8>[3/4]</color> Building Adventurer_Wilderness.unity...");
            WildernessSceneBuilder.BuildScene();

            // 4. Sector 0: Underground Black Market
            Debug.Log("<color=#38BDF8>[4/4]</color> Building Underground_BlackMarket.unity...");
            UndergroundSceneBuilder.BuildScene();

            // Ensure all 4 scenes are in EditorBuildSettings
            EnsureScenesInBuildSettings();

            Debug.Log("<color=#00FFAA>[MasterVisualOverhaul]</color> <b>✔ ALL 4 SCENES SUCCESSFULLY OVERHAULED & REBUILT!</b>");
        }

        public static void EnsureScenesInBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                "Assets/Scenes/District_CentralHub.unity",
                "Assets/Scenes/Arena_Colosseum.unity",
                "Assets/Scenes/Adventurer_Wilderness.unity",
                "Assets/Scenes/Underground_BlackMarket.unity"
            };

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var path in scenePaths)
            {
                list.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log($"<color=#00FFAA>[BuildSettings]</color> Configured {list.Count} scenes in EditorBuildSettings.");
        }
    }
}
