using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CelestiaVR
{
    /// <summary>
    /// Central hub for the discovery system.
    ///
    /// Flow:
    ///   1. Telescope raycast (Gurpreet) hits a CelestialObjectTag
    ///   2. Telescope calls DiscoveryManager.Instance.SetGazeTarget(tag)
    ///   3. DiscoveryManager drives the DwellDetector on that object
    ///   4. On dwell complete → fires OnObjectDiscovered
    ///      → ConstellationManager highlights the constellation
    ///      → Info panel (Pavan) receives the CelestialObjectData
    ///
    /// Naked-eye gaze (no telescope) uses the same flow but is driven by
    /// GazeDiscoveryController which also calls SetGazeTarget().
    /// </summary>
    public class DiscoveryManager : MonoBehaviour
    {
        public static DiscoveryManager Instance { get; private set; }

        [Header("Events — wire Pavan's info panel here")]
        public UnityEvent<CelestialObjectData> OnObjectDiscovered;
        public UnityEvent<CelestialObjectData, float> OnDwellProgress; // data + 0-1 progress
        public UnityEvent OnGazeLost;

        private CelestialObjectTag _currentTarget;
        private DwellDetector _currentDwell;
        private readonly HashSet<string> _discoveredObjects = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Called every frame by the telescope or gaze controller.
        /// Pass null when nothing is being looked at.
        /// </summary>
        public void SetGazeTarget(CelestialObjectTag target)
        {
            if (target == _currentTarget) return;

            // Stop dwelling on previous target
            if (_currentDwell != null)
            {
                _currentDwell.NotifyGaze(false);
                _currentDwell.OnDwellCompleted -= HandleDwellComplete;
                _currentDwell.OnDwellProgress -= HandleDwellProgress;
            }

            _currentTarget = target;

            if (target == null)
            {
                _currentDwell = null;
                OnGazeLost?.Invoke();
                return;
            }

            // Get or add DwellDetector on the target
            _currentDwell = target.GetComponent<DwellDetector>();
            if (_currentDwell == null)
                _currentDwell = target.gameObject.AddComponent<DwellDetector>();

            _currentDwell.OnDwellCompleted += HandleDwellComplete;
            _currentDwell.OnDwellProgress += HandleDwellProgress;
            _currentDwell.NotifyGaze(true);
        }

        private void HandleDwellProgress(float progress)
        {
            if (_currentTarget != null)
                OnDwellProgress?.Invoke(_currentTarget.Data, progress);
        }

        private void HandleDwellComplete()
        {
            if (_currentTarget == null || _currentTarget.Data == null) return;

            var data = _currentTarget.Data;
            _discoveredObjects.Add(data.objectName);

            // Highlight the constellation this object belongs to
            ConstellationManager.Instance?.HighlightForObject(data);

            // Notify the info panel
            OnObjectDiscovered?.Invoke(data);

            Debug.Log($"[DiscoveryManager] Discovered: {data.objectName}");
        }

        public bool IsDiscovered(string objectName) => _discoveredObjects.Contains(objectName);
        public int DiscoveredCount => _discoveredObjects.Count;
    }
}
