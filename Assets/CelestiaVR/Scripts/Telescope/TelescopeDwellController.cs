using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Raycasts from the eyepiece camera center each frame while the telescope is grabbed.
    /// Feeds hit results into DiscoveryManager to drive dwell detection.
    ///
    /// Attach to the same GameObject as TelescopeController.
    /// </summary>
    [RequireComponent(typeof(TelescopeController))]
    public class TelescopeDwellController : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 2000f;
        [SerializeField] private LayerMask celestialObjectLayer;

        [Header("Dwell Cursor (optional)")]
        [Tooltip("UI Image used as a circular dwell progress indicator in the eyepiece")]
        [SerializeField] private UnityEngine.UI.Image dwellCursorFill;

        private TelescopeController _telescope;
        private CelestialObjectTag _lastHit;

        private void Awake()
        {
            _telescope = GetComponent<TelescopeController>();

            if (DiscoveryManager.Instance != null)
                DiscoveryManager.Instance.OnDwellProgress.AddListener(OnDwellProgress);
        }

        private void Update()
        {
            if (!_telescope.IsGrabbed)
            {
                if (_lastHit != null)
                {
                    DiscoveryManager.Instance?.SetGazeTarget(null);
                    _lastHit = null;
                }
                return;
            }

            Camera cam = _telescope.EyepieceCamera;
            if (cam == null || !cam.enabled) return;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            CelestialObjectTag hit = null;

            if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, celestialObjectLayer))
                hit = hitInfo.collider.GetComponent<CelestialObjectTag>();

            if (hit != _lastHit)
            {
                _lastHit = hit;
                DiscoveryManager.Instance?.SetGazeTarget(hit);
                SetCursorProgress(0f);
            }
        }

        private void OnDwellProgress(CelestialObjectData data, float progress)
        {
            SetCursorProgress(progress);
        }

        private void SetCursorProgress(float t)
        {
            if (dwellCursorFill != null)
                dwellCursorFill.fillAmount = t;
        }

        private void OnDestroy()
        {
            if (DiscoveryManager.Instance != null)
                DiscoveryManager.Instance.OnDwellProgress.RemoveListener(OnDwellProgress);
        }
    }
}
