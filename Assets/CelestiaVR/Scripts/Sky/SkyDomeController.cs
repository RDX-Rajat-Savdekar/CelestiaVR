using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Rotates the sky dome (and all child celestial objects) at Earth's sidereal rate.
    ///
    /// The rotation axis points toward the north celestial pole. For Griffith Observatory
    /// (latitude 34.1184°N), Polaris sits 34.1° above the northern horizon, so the pole
    /// axis in world space is tilted: it points North on the horizon and up by the latitude.
    ///
    /// In Unity's coordinate system (Y-up, Z-forward = South):
    ///   North horizon direction = -Z
    ///   Pole axis = rotate the Y axis toward -Z by (90 - latitude) degrees
    ///             = Vector3(-sin(lat), cos(lat), 0) ... no, let's derive it properly:
    ///
    ///   If +Y = zenith and -Z = North:
    ///   Polaris direction = sin(lat)*Y + cos(lat)*(-Z)
    ///                     = (0, sin(lat), -cos(lat))  [normalized]
    ///
    ///   For LA lat=34.1184°: (0, sin(34.1184°), -cos(34.1184°)) ≈ (0, 0.561, -0.828)
    /// </summary>
    public class SkyDomeController : MonoBehaviour
    {
        [Header("Observer Location")]
        [Tooltip("Griffith Observatory latitude in degrees")]
        [SerializeField] private float observerLatitudeDegrees = 34.1184f;

        [Header("Rotation")]
        [Tooltip("Earth's sidereal rotation rate in degrees per second (0.0042°/s ≈ 360°/23h56m)")]
        [SerializeField] private float siderealRateDegPerSec = 0.0042f;
        [Tooltip("Enable/disable sky dome rotation (useful for debugging)")]
        [SerializeField] public bool enableRotation = true;

        [Header("Debug")]
        [SerializeField] private bool drawPoleAxis;

        // Celestial pole direction in local world space
        private Vector3 _poleAxis;

        private void Awake()
        {
            _poleAxis = ComputePoleAxis(observerLatitudeDegrees);
        }

        private void Update()
        {
            if (!enableRotation) return;
            transform.Rotate(_poleAxis, siderealRateDegPerSec * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Snaps the sky dome to a specific sidereal hour angle offset (degrees).
        /// Call this to set the initial sky orientation for the experience's reference time.
        /// </summary>
        public void SetInitialRotation(float localSiderealAngleDegrees)
        {
            transform.rotation = Quaternion.AngleAxis(localSiderealAngleDegrees, _poleAxis);
        }

        /// <summary>
        /// Computes the north celestial pole direction in Unity world space.
        /// Unity convention: +Y = up (zenith), -Z = north on horizon, +X = east.
        /// </summary>
        private static Vector3 ComputePoleAxis(float latitudeDeg)
        {
            float lat = latitudeDeg * Mathf.Deg2Rad;
            // Polaris direction: sin(lat) upward, cos(lat) toward north horizon (-Z)
            return new Vector3(0f, Mathf.Sin(lat), -Mathf.Cos(lat)).normalized;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawPoleAxis) return;
            _poleAxis = ComputePoleAxis(observerLatitudeDegrees);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, _poleAxis * 10f);
            UnityEditor.Handles.Label(transform.position + _poleAxis * 10.5f, "Celestial Pole");
        }
#endif
    }
}
