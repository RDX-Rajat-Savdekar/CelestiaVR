using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Shows a miniature 3D holographic star map of a constellation in the inspection panel.
    ///
    /// Stars are placed at their correct relative 3D positions (from RA/Dec → sphere),
    /// then scaled to fit the model stage. Constellation lines are drawn in bright cyan.
    /// The whole map slowly rotates so the user can view it from all angles.
    /// </summary>
    public class ConstellationInspectionView : MonoBehaviour
    {
        [SerializeField] private Material starMaterial;            // Unlit white/yellow
        [SerializeField] private Material lineMaterial;            // Unlit cyan
        [SerializeField] private float rotateSpeed = 8f;

        private GameObject _mapRoot;
        private GameObject _imageQuad;
        private bool _active;

        public void Show(CelestialObjectData objectData, float stageRadius)
        {
            Clear();
            _active = true;

            // Show deep sky object photo as a static backdrop when available
            if (objectData.constellationDiagram != null)
            {
                _imageQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(_imageQuad.GetComponent<Collider>());
                _imageQuad.transform.SetParent(transform, false);
                _imageQuad.transform.localPosition = Vector3.zero;
                _imageQuad.transform.localScale    = Vector3.one * (stageRadius * 2f);
                _imageQuad.name = objectData.objectName + "_Image";
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.mainTexture = objectData.constellationDiagram;
                _imageQuad.GetComponent<Renderer>().material = mat;
            }

            // Build rotating constellation star map overlay (if constellation is linked)
            ConstellationData constellation = FindConstellation(objectData);
            if (constellation != null)
            {
                _mapRoot = new GameObject("ConstellationMap");
                _mapRoot.transform.SetParent(transform, false);
                _mapRoot.transform.localPosition = Vector3.zero;
                BuildStarMap(constellation, stageRadius);
            }
            else if (objectData.constellationDiagram == null)
            {
                Debug.LogWarning($"[ConstellationInspectionView] No constellation or image found for {objectData.objectName}");
            }
        }

        private void BuildStarMap(ConstellationData data, float stageRadius)
        {
            if (data.stars == null || data.stars.Length == 0) return;

            // Convert all stars to unit sphere positions
            Vector3[] positions = new Vector3[data.stars.Length];
            for (int i = 0; i < data.stars.Length; i++)
            {
                positions[i] = RaDecToUnit(data.stars[i].raHours, data.stars[i].decDegrees);
            }

            // Center the constellation: subtract centroid so it's centered in the stage
            Vector3 centroid = Vector3.zero;
            foreach (var p in positions) centroid += p;
            centroid /= positions.Length;
            for (int i = 0; i < positions.Length; i++)
                positions[i] -= centroid;

            // Scale to fit stage radius
            float maxDist = 0f;
            foreach (var p in positions) maxDist = Mathf.Max(maxDist, p.magnitude);
            float scale = maxDist > 0.001f ? stageRadius / maxDist : 1f;
            for (int i = 0; i < positions.Length; i++)
                positions[i] *= scale;

            // Draw stars
            var starMat = starMaterial != null ? starMaterial : CreateUnlitMat(Color.white);
            foreach (var pos in positions)
            {
                var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(star.GetComponent<Collider>());
                star.transform.SetParent(_mapRoot.transform, false);
                star.transform.localPosition = pos;
                star.transform.localScale    = Vector3.one * (stageRadius * 0.06f);
                star.GetComponent<Renderer>().material = starMat;
                star.name = "Star";
            }

            // Draw constellation lines
            if (data.connectionIndices == null || data.connectionIndices.Length < 2) return;
            var lineMat = lineMaterial != null ? lineMaterial : CreateUnlitMat(new Color(0.3f, 0.9f, 1f));

            int segCount = data.connectionIndices.Length / 2;
            for (int i = 0; i < segCount; i++)
            {
                int aIdx = data.connectionIndices[i * 2];
                int bIdx = data.connectionIndices[i * 2 + 1];
                if (aIdx >= positions.Length || bIdx >= positions.Length) continue;

                var lineGo = new GameObject($"Line_{i}");
                lineGo.transform.SetParent(_mapRoot.transform, false);

                var lr = lineGo.AddComponent<LineRenderer>();
                lr.material          = lineMat;
                lr.positionCount     = 2;
                lr.SetPosition(0, positions[aIdx]);
                lr.SetPosition(1, positions[bIdx]);
                lr.startWidth        = stageRadius * 0.015f;
                lr.endWidth          = stageRadius * 0.015f;
                lr.startColor        = new Color(0.3f, 0.9f, 1f, 0.9f);
                lr.endColor          = new Color(0.3f, 0.9f, 1f, 0.9f);
                lr.useWorldSpace     = false;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        public void Clear()
        {
            _active = false;
            if (_mapRoot != null)  { Destroy(_mapRoot);    _mapRoot    = null; }
            if (_imageQuad != null) { Destroy(_imageQuad); _imageQuad  = null; }
        }

        private void Update()
        {
            if (!_active || _mapRoot == null) return;
            _mapRoot.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
        }

        private static Vector3 RaDecToUnit(float raHours, float decDeg)
        {
            float ra  = raHours * 15f * Mathf.Deg2Rad;
            float dec = decDeg * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Cos(dec) * Mathf.Sin(ra),
                Mathf.Sin(dec),
                Mathf.Cos(dec) * Mathf.Cos(ra));
        }

        private static ConstellationData FindConstellation(CelestialObjectData objectData)
        {
            // Search all ConstellationData assets at runtime via Resources or cached list
            var all = Resources.FindObjectsOfTypeAll<ConstellationData>();
            foreach (var c in all)
                if (c.linkedObjects != null)
                    foreach (var linked in c.linkedObjects)
                        if (linked == objectData) return c;
            return null;
        }

        private static Material CreateUnlitMat(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = color;
            return mat;
        }
    }
}
