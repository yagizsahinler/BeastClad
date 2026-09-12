using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastClad.World
{
    /// <summary>
    /// Identifies arrival destination coordinates and facing orientation for scene transitions.
    /// Self-registers to a registry so SceneTransitionManager can locate target spawns instantly.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneSpawnPoint : MonoBehaviour
    {
        [Header("Spawn Identity")]
        [Tooltip("Unique tag for this spawn point within the scene (e.g. Spawn_FromHub, Spawn_FromWilderness)")]
        [SerializeField] private string spawnTag = "Spawn_Default";

        [Tooltip("Direction player character faces immediately after teleporting")]
        [SerializeField] private Vector2 defaultFacing = Vector2.down;

        public string SpawnTag => spawnTag;
        public Vector2 DefaultFacing => defaultFacing;

        private static readonly List<SceneSpawnPoint> activeSpawnPoints = new List<SceneSpawnPoint>();
        public static IReadOnlyList<SceneSpawnPoint> ActiveSpawnPoints => activeSpawnPoints;

        private void OnEnable()
        {
            if (!activeSpawnPoints.Contains(this))
            {
                activeSpawnPoints.Add(this);
            }
        }

        private void OnDisable()
        {
            activeSpawnPoints.Remove(this);
        }

        public static SceneSpawnPoint FindByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;

            for (int i = 0; i < activeSpawnPoints.Count; i++)
            {
                if (activeSpawnPoints[i] != null && activeSpawnPoints[i].SpawnTag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    return activeSpawnPoints[i];
                }
            }

            // Fallback: search in scene if registry was empty
            var all = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Exclude);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].SpawnTag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    return all[i];
                }
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, 0.6f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, (Vector3)defaultFacing.normalized * 1.2f);
        }
    }
}
