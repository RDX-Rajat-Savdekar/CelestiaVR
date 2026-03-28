using System.Collections;
using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Renders a single constellation as a set of LineRenderer segments.
    /// Placed as a child of SkyDome so it rotates with the sky.
    /// </summary>
    [RequireComponent(typeof(ConstellationLinePool))]
    public class ConstellationRenderer : MonoBehaviour
    {
        [SerializeField] public ConstellationData data;
        [SerializeField] public float skyDomeRadius = 50f;
        [SerializeField] public Material lineMaterial;

        private ConstellationLinePool _pool;
        private bool _isHighlighted;
        private Coroutine _highlightCoroutine;

        public ConstellationData Data => data;

        private void Awake()
        {
            _pool = GetComponent<ConstellationLinePool>();
        }

        private void Start()
        {
            BuildLines();
            SetVisibility(false);
        }

        public void BuildLines()
        {
            _pool.Clear();

            if (data == null || data.stars == null || data.connectionIndices == null) return;
            if (data.connectionIndices.Length % 2 != 0) return;

            int segmentCount = data.connectionIndices.Length / 2;
            for (int i = 0; i < segmentCount; i++)
            {
                int aIdx = data.connectionIndices[i * 2];
                int bIdx = data.connectionIndices[i * 2 + 1];
                if (aIdx >= data.stars.Length || bIdx >= data.stars.Length) continue;

                Vector3 posA = RaDecToLocal(data.stars[aIdx].raHours, data.stars[aIdx].decDegrees);
                Vector3 posB = RaDecToLocal(data.stars[bIdx].raHours, data.stars[bIdx].decDegrees);

                var lr = _pool.Get(lineMaterial);
                lr.positionCount = 2;
                lr.SetPosition(0, posA);
                lr.SetPosition(1, posB);
                lr.startWidth = data.lineWidth;
                lr.endWidth = data.lineWidth;
                lr.startColor = data.lineColor;
                lr.endColor = data.lineColor;
                lr.useWorldSpace = false; // local to SkyDome
            }
        }

        public void SetVisibility(bool visible)
        {
            _pool.SetAllActive(visible);
        }

        /// <summary>
        /// Highlights the constellation lines with a smooth fade-in glow.
        /// </summary>
        public void Highlight(bool highlight)
        {
            if (_isHighlighted == highlight) return;
            _isHighlighted = highlight;

            if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
            _highlightCoroutine = StartCoroutine(AnimateHighlight(highlight));
        }

        private IEnumerator AnimateHighlight(bool toHighlight)
        {
            Color fromColor = toHighlight ? data.lineColor : data.highlightColor;
            Color toColor   = toHighlight ? data.highlightColor : data.lineColor;
            float fromWidth = toHighlight ? data.lineWidth : data.highlightWidth;
            float toWidth   = toHighlight ? data.highlightWidth : data.lineWidth;

            float t = 0f;
            float duration = 0.4f;

            SetVisibility(true);

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                Color c = Color.Lerp(fromColor, toColor, t);
                float w = Mathf.Lerp(fromWidth, toWidth, t);
                _pool.SetAllColor(c);
                _pool.SetAllWidth(w);
                yield return null;
            }

            if (!toHighlight)
                SetVisibility(false);
        }

        private Vector3 RaDecToLocal(float raHours, float decDeg)
        {
            return CelestialObjectPlacer.RaDecToLocalPosition(raHours, decDeg, skyDomeRadius);
        }
    }
}
