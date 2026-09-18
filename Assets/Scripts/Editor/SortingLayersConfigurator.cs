using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeastClad.Editor
{
    /// <summary>
    /// Configures 2D sorting layers and URP 2D transparency sorting (Custom Y-Axis)
    /// to establish the proper depth-sorting foundation for BeastClad.
    /// </summary>
    public static class SortingLayersConfigurator
    {
        private static readonly string[] RequiredSortingLayers = new string[]
        {
            "Default",
            "Background",
            "Floor",
            "FloorHazards",
            "Props_Low",
            "Entities",
            "EquippedArmor",
            "VFX_Under",
            "Props_High",
            "VFX_Over",
            "UI_World"
        };

        [MenuItem("BeastClad/Configure 2D Sorting & Layers")]
        public static void ConfigureAll()
        {
            ConfigureSortingLayers();
            ConfigureRenderer2DSorting();
        }

        public static void ConfigureSortingLayers()
        {
            var tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAsset == null || tagManagerAsset.Length == 0)
            {
                Debug.LogError("[SortingLayersConfigurator] Failed to load TagManager.asset!");
                return;
            }

            var so = new SerializedObject(tagManagerAsset[0]);
            var sortingLayersProp = so.FindProperty("m_SortingLayers");
            if (sortingLayersProp == null)
            {
                Debug.LogError("[SortingLayersConfigurator] Could not locate 'm_SortingLayers' property in TagManager!");
                return;
            }

            // Collect existing layer names
            var existingLayers = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < sortingLayersProp.arraySize; i++)
            {
                var elem = sortingLayersProp.GetArrayElementAtIndex(i);
                var nameProp = elem.FindPropertyRelative("name");
                if (nameProp != null)
                {
                    existingLayers.Add(nameProp.stringValue);
                }
            }

            bool addedAny = false;
            foreach (var layerName in RequiredSortingLayers)
            {
                if (!existingLayers.Contains(layerName))
                {
                    int newIndex = sortingLayersProp.arraySize;
                    sortingLayersProp.InsertArrayElementAtIndex(newIndex);
                    var newElem = sortingLayersProp.GetArrayElementAtIndex(newIndex);
                    var nameProp = newElem.FindPropertyRelative("name");
                    var uniqueIdProp = newElem.FindPropertyRelative("uniqueID");

                    if (nameProp != null) nameProp.stringValue = layerName;
                    if (uniqueIdProp != null)
                    {
                        uniqueIdProp.intValue = Mathf.Abs((layerName + newIndex).GetHashCode());
                    }
                    addedAny = true;
                    Debug.Log($"[SortingLayersConfigurator] Added sorting layer: <b>{layerName}</b>");
                }
            }

            if (addedAny)
            {
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log("<color=#00FFAA>[SortingLayersConfigurator]</color> Sorting layers successfully updated!");
            }
            else
            {
                Debug.Log("<color=#00FFAA>[SortingLayersConfigurator]</color> All required sorting layers already present.");
            }
        }

        public static void ConfigureRenderer2DSorting()
        {
            var renderer2D = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Settings/Renderer2D.asset");
            if (renderer2D == null)
            {
                Debug.LogWarning("[SortingLayersConfigurator] Could not find Assets/Settings/Renderer2D.asset");
                return;
            }

            var so = new SerializedObject(renderer2D);
            var sortModeProp = so.FindProperty("m_TransparencySortMode");
            var sortAxisProp = so.FindProperty("m_TransparencySortAxis");

            if (sortModeProp != null)
            {
                // 3 = TransparencySortMode.CustomAxis
                sortModeProp.intValue = 3;
            }

            if (sortAxisProp != null)
            {
                // Sort along positive Y axis for 2D top-down
                sortAxisProp.vector3Value = new Vector3(0f, 1f, 0f);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(renderer2D);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#00FFAA>[SortingLayersConfigurator]</color> Renderer2D configured for 2D Top-Down Y-Sorting (CustomAxis: 0, 1, 0)!");
        }
    }
}
