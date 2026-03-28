using System.Collections.Generic;
using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Simple pool of LineRenderer components used by ConstellationRenderer.
    /// Avoids repeated GetComponent calls and makes bulk color/width updates fast.
    /// </summary>
    public class ConstellationLinePool : MonoBehaviour
    {
        private readonly List<LineRenderer> _lines = new();

        public LineRenderer Get(Material mat)
        {
            var go = new GameObject("Line");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = mat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            _lines.Add(lr);
            return lr;
        }

        public void Clear()
        {
            foreach (var lr in _lines)
                if (lr != null) Destroy(lr.gameObject);
            _lines.Clear();
        }

        public void SetAllActive(bool active)
        {
            foreach (var lr in _lines)
                if (lr != null) lr.gameObject.SetActive(active);
        }

        public void SetAllColor(Color c)
        {
            foreach (var lr in _lines)
            {
                if (lr == null) continue;
                lr.startColor = c;
                lr.endColor = c;
            }
        }

        public void SetAllWidth(float w)
        {
            foreach (var lr in _lines)
            {
                if (lr == null) continue;
                lr.startWidth = w;
                lr.endWidth = w;
            }
        }
    }
}
