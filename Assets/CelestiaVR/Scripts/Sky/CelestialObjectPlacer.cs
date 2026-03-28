using System.Collections.Generic;
using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Spawns celestial object prefabs at positions derived from their RA/Dec coordinates.
    ///
    /// Coordinate conversion (RA/Dec → Unity world position):
    ///
    ///   Standard equatorial → Cartesian (right-handed, Z = vernal equinox direction):
    ///     x = cos(dec) * cos(ra)
    ///     y = sin(dec)
    ///     z = cos(dec) * sin(ra)
    ///
    ///   Remapped to Unity axes (+Y = north pole, +Z = vernal equinox, +X = east):
    ///     Unity.x =  cos(dec) * sin(ra)   [east]
    ///     Unity.y =  sin(dec)              [north pole]
    ///     Unity.z =  cos(dec) * cos(ra)   [vernal equinox / spring point]
    ///
    ///   The sky dome parent is then rotated by SkyDomeController to align the vernal
    ///   equinox with the local sidereal time, giving correct horizon-relative positions.
    ///
    /// Objects are placed at radius skyDomeRadius from the dome's origin (the player position).
    /// </summary>
    public class CelestialObjectPlacer : MonoBehaviour
    {
        [System.Serializable]
        public class CelestialEntry
        {
            public CelestialObjectData data;
            [Tooltip("Optional override prefab. If null, uses the defaultObjectPrefab.")]
            public GameObject prefabOverride;
        }

        [Header("Sky Dome")]
        [Tooltip("The SkyDome transform that rotates (parent of all placed objects)")]
        [SerializeField] private Transform skyDome;
        [Tooltip("Radius of the sky sphere in Unity units")]
        [SerializeField] private float skyDomeRadius = 500f;

        [Header("Prefabs")]
        [Tooltip("Default prefab used when an entry has no override")]
        [SerializeField] private GameObject defaultObjectPrefab;

        [Header("Celestial Objects")]
        [SerializeField] private List<CelestialEntry> celestialObjects = new();

        // Runtime lookup: objectName → instantiated GameObject
        private readonly Dictionary<string, GameObject> _instances = new();

        private void Start()
        {
            PlaceAll();
        }

        public void PlaceAll()
        {
            foreach (var entry in celestialObjects)
            {
                if (entry.data == null) continue;
                PlaceObject(entry);
            }
        }

        private void PlaceObject(CelestialEntry entry)
        {
            CelestialObjectData data = entry.data;
            Vector3 localPos = RaDecToLocalPosition(data.rightAscensionHours, data.declinationDegrees, skyDomeRadius);

            GameObject prefab = entry.prefabOverride != null ? entry.prefabOverride : defaultObjectPrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"[CelestialObjectPlacer] No prefab for {data.objectName}. Skipping.");
                return;
            }

            // Instantiate as child of this transform (CelestialObjects) so hierarchy stays clean
            GameObject instance = Instantiate(prefab, transform);
            instance.name = data.objectName;
            instance.transform.localPosition = localPos;
            instance.transform.localScale = Vector3.one * data.visualScale;

            // Face the center of the dome (important for billboard quads / disc-shaped objects)
            instance.transform.LookAt(skyDome.position, Vector3.up);
            instance.transform.Rotate(0f, 180f, 0f); // flip to face inward

            // Assign material if provided
            if (data.objectMaterial != null)
            {
                var renderer = instance.GetComponentInChildren<Renderer>();
                if (renderer != null) renderer.material = data.objectMaterial;
            }

            // Ensure a collider exists for telescope dwell raycast
            if (instance.GetComponentInChildren<Collider>() == null)
            {
                var col = instance.AddComponent<SphereCollider>();
                col.radius = 0.5f; // local space — scales with the object
            }

            // Store reference and attach data for the discovery system
            _instances[data.objectName] = instance;
            var tag = instance.GetComponent<CelestialObjectTag>();
            if (tag == null) tag = instance.AddComponent<CelestialObjectTag>();
            tag.Data = data;
        }

        /// <summary>
        /// Converts equatorial coordinates to a local position on the sky sphere.
        /// The returned vector is in the sky dome's local space.
        /// </summary>
        public static Vector3 RaDecToLocalPosition(float raHours, float decDegrees, float radius)
        {
            float ra = raHours * 15f * Mathf.Deg2Rad;  // hours → degrees → radians
            float dec = decDegrees * Mathf.Deg2Rad;

            // Unity axes: +Y = NCP, +Z = vernal equinox, +X = east
            float x = Mathf.Cos(dec) * Mathf.Sin(ra);
            float y = Mathf.Sin(dec);
            float z = Mathf.Cos(dec) * Mathf.Cos(ra);

            return new Vector3(x, y, z) * radius;
        }

        /// <summary>
        /// Returns the instantiated GameObject for a named celestial object, or null.
        /// Used by the discovery system to highlight/target objects.
        /// </summary>
        public GameObject GetInstance(string objectName)
        {
            _instances.TryGetValue(objectName, out var go);
            return go;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (skyDome == null) return;
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.1f);
            Gizmos.DrawWireSphere(skyDome.position, skyDomeRadius);

            foreach (var entry in celestialObjects)
            {
                if (entry.data == null) continue;
                Vector3 localPos = RaDecToLocalPosition(
                    entry.data.rightAscensionHours,
                    entry.data.declinationDegrees,
                    skyDomeRadius);
                Vector3 worldPos = skyDome.TransformPoint(localPos);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(worldPos, skyDomeRadius * 0.01f);
                UnityEditor.Handles.Label(worldPos, entry.data.objectName);
            }
        }
#endif
    }
}
