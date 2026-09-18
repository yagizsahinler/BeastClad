using UnityEditor;
using UnityEngine;
using BeastClad.Player;

namespace BeastClad.Editor
{
    public static class PlayerPrefabRigConfigurator
    {
        private const string PrefabPath = "Assets/Prefabs/Player.prefab";

        [MenuItem("BeastClad/Configure Player Paperdoll Rig")]
        public static void ConfigureRig()
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
            {
                var root = scope.prefabContentsRoot;
                if (root == null)
                {
                    Debug.LogError("[PlayerPrefabRigConfigurator] Failed to open Player.prefab!");
                    return;
                }

                // 1. Root SpriteRenderer becomes BaseChassis
                var baseRenderer = root.GetComponent<SpriteRenderer>();
                if (baseRenderer != null)
                {
                    baseRenderer.sortingLayerName = "Entities";
                    baseRenderer.sortingOrder = 0;
                }

                // 2. Infuse Sockets Parent
                var socketsRoot = root.transform.Find("Sockets");
                if (socketsRoot == null)
                {
                    var go = new GameObject("Sockets");
                    go.transform.SetParent(root.transform, false);
                    socketsRoot = go.transform;
                }

                // 3. Create or find the 5 overlay child objects
                var headSr = GetOrCreateOverlayRenderer(socketsRoot, "HeadOverlay", 5);
                var chestSr = GetOrCreateOverlayRenderer(socketsRoot, "ChestOverlay", 2);
                var leftArmSr = GetOrCreateOverlayRenderer(socketsRoot, "LeftArmOverlay", 3);
                var rightArmSr = GetOrCreateOverlayRenderer(socketsRoot, "RightArmOverlay", 4);
                var legsSr = GetOrCreateOverlayRenderer(socketsRoot, "LegsOverlay", 1);

                // 4. Combat Sockets Parent
                var combatSocketsRoot = root.transform.Find("CombatSockets");
                if (combatSocketsRoot == null)
                {
                    var go = new GameObject("CombatSockets");
                    go.transform.SetParent(root.transform, false);
                    combatSocketsRoot = go.transform;
                }

                var meleeSocket = GetOrCreateSocket(combatSocketsRoot, "MeleeSocket", new Vector3(0f, 0.4f, 0f));
                var centerMassSocket = GetOrCreateSocket(combatSocketsRoot, "CenterMassSocket", new Vector3(0f, 0.8f, 0f));
                var groundSocket = GetOrCreateSocket(combatSocketsRoot, "GroundSocket", Vector3.zero);

                // 5. Add or configure PlayerInfuseVisualController
                var visualCtrl = root.GetComponent<PlayerInfuseVisualController>();
                if (visualCtrl == null)
                {
                    visualCtrl = root.AddComponent<PlayerInfuseVisualController>();
                }

                visualCtrl.AssignRenderers(
                    baseRenderer,
                    headSr,
                    chestSr,
                    leftArmSr,
                    rightArmSr,
                    legsSr,
                    meleeSocket,
                    centerMassSocket,
                    groundSocket
                );

                Debug.Log("<color=#00FFAA>[PlayerPrefabRigConfigurator]</color> Player.prefab Paperdoll Rig successfully configured!");
            }
        }

        private static SpriteRenderer GetOrCreateOverlayRenderer(Transform parent, string name, int orderInLayer)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                child = go.transform;
            }

            var sr = child.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = child.gameObject.AddComponent<SpriteRenderer>();
            }

            sr.sortingLayerName = "EquippedArmor";
            sr.sortingOrder = orderInLayer;
            return sr;
        }

        private static Transform GetOrCreateSocket(Transform parent, string name, Vector3 localPos)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                child = go.transform;
            }

            child.localPosition = localPos;
            return child;
        }
    }
}
