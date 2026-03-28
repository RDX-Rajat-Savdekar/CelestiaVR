using System;
using UnityEngine;
using UnityEngine.Events;

namespace CelestiaVR
{
    /// <summary>
    /// Generic dwell detector. Place on any GameObject that should be discoverable.
    /// External systems (telescope raycast, gaze controller) call NotifyGaze(bool)
    /// each frame to drive the timer.
    ///
    /// The telescope system (Gurpreet) calls NotifyGaze(true) while its raycast
    /// hits this object, and NotifyGaze(false) when it misses.
    /// </summary>
    public class DwellDetector : MonoBehaviour
    {
        [SerializeField] private float dwellDuration = 2.5f;

        public event Action OnDwellCompleted;
        public event Action<float> OnDwellProgress; // 0–1
        public event Action OnDwellCancelled;

        // UnityEvents for wiring in Inspector (Pavan's info panel)
        public UnityEvent onDwellCompleted;
        public UnityEvent onDwellCancelled;

        private float _timer;
        private bool _isGazing;
        private bool _hasCompleted;

        public float Progress => Mathf.Clamp01(_timer / dwellDuration);
        public bool IsGazing => _isGazing;

        /// <summary>
        /// Called every frame by the telescope/gaze controller.
        /// </summary>
        public void NotifyGaze(bool gazing)
        {
            if (_hasCompleted) return;

            if (gazing && !_isGazing)
            {
                _isGazing = true;
            }
            else if (!gazing && _isGazing)
            {
                _isGazing = false;
                _timer = 0f;
                OnDwellProgress?.Invoke(0f);
                OnDwellCancelled?.Invoke();
                onDwellCancelled?.Invoke();
            }
        }

        private void Update()
        {
            if (!_isGazing || _hasCompleted) return;

            _timer += Time.deltaTime;
            OnDwellProgress?.Invoke(Progress);

            if (_timer >= dwellDuration)
            {
                _hasCompleted = true;
                _isGazing = false;
                OnDwellCompleted?.Invoke();
                onDwellCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Resets the dwell state so this object can be discovered again.
        /// </summary>
        public void Reset()
        {
            _timer = 0f;
            _isGazing = false;
            _hasCompleted = false;
        }
    }
}
