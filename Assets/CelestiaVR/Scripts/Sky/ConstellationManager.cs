using System.Collections.Generic;
using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Manages all ConstellationRenderers in the scene.
    /// - Handles the X-button toggle for showing/hiding all constellation lines.
    /// - Exposes HighlightForObject() so the discovery system can highlight the
    ///   correct constellation when a celestial object is discovered.
    /// </summary>
    public class ConstellationManager : MonoBehaviour
    {
        public static ConstellationManager Instance { get; private set; }

        [SerializeField] private Transform skyDome;
        [SerializeField] private float skyDomeRadius = 50f;
        [SerializeField] private Material constellationLineMaterial;
        [SerializeField] private ConstellationData[] constellations;

        private readonly List<ConstellationRenderer> _renderers = new();
        private readonly Dictionary<CelestialObjectData, ConstellationRenderer> _objectMap = new();
        private bool _linesVisible = false;
        private ConstellationRenderer _currentHighlight;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            BuildConstellations();
        }

        private void BuildConstellations()
        {
            if (constellations == null) return;

            foreach (var data in constellations)
            {
                if (data == null) continue;

                var go = new GameObject(data.constellationName);
                go.transform.SetParent(skyDome, false);

                go.AddComponent<ConstellationLinePool>();
                var cr = go.AddComponent<ConstellationRenderer>();
                cr.data             = data;
                cr.skyDomeRadius    = skyDomeRadius;
                cr.lineMaterial     = constellationLineMaterial;

                _renderers.Add(cr);

                // Map each linked object to this renderer for fast lookup
                if (data.linkedObjects != null)
                    foreach (var obj in data.linkedObjects)
                        if (obj != null) _objectMap[obj] = cr;
            }
        }

        /// <summary>
        /// Toggle constellation lines on/off. Called by X button input.
        /// </summary>
        public void ToggleLines()
        {
            _linesVisible = !_linesVisible;
            foreach (var cr in _renderers)
                cr.SetVisibility(_linesVisible);
        }

        public bool AreLinesVisible => _linesVisible;

        /// <summary>
        /// Highlight the constellation that contains the discovered object.
        /// Automatically unhighlights the previous one.
        /// </summary>
        public void HighlightForObject(CelestialObjectData discoveredObject)
        {
            if (_currentHighlight != null)
                _currentHighlight.Highlight(false);

            if (discoveredObject == null) return;

            if (_objectMap.TryGetValue(discoveredObject, out var cr))
            {
                _currentHighlight = cr;
                cr.Highlight(true);
            }
        }

        public void ClearHighlight()
        {
            if (_currentHighlight != null)
            {
                _currentHighlight.Highlight(false);
                _currentHighlight = null;
            }
        }
    }
}
