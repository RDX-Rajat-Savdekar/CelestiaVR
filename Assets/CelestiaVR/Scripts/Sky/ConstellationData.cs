using UnityEngine;

namespace CelestiaVR
{
    [CreateAssetMenu(fileName = "NewConstellation", menuName = "CelestiaVR/Constellation Data")]
    public class ConstellationData : ScriptableObject
    {
        [System.Serializable]
        public struct ConstellationStar
        {
            public string starName;
            [Tooltip("Right Ascension in decimal hours (0-24)")]
            public float raHours;
            [Tooltip("Declination in decimal degrees (-90 to +90)")]
            public float decDegrees;
        }

        [Header("Identity")]
        public string constellationName;
        [TextArea(2, 3)]
        public string mythology;

        [Header("Stars")]
        public ConstellationStar[] stars;

        [Header("Connections")]
        [Tooltip("Pairs of star indices defining line segments. Must be even length.")]
        public int[] connectionIndices;

        [Header("Visuals")]
        public Color lineColor = new Color(0.4f, 0.6f, 1f, 0.4f);
        public Color highlightColor = new Color(0.6f, 0.85f, 1f, 1f);
        public float lineWidth = 0.3f;
        public float highlightWidth = 0.5f;

        [Header("Linked Objects")]
        [Tooltip("CelestialObjectData assets that belong to this constellation — discovering any one highlights this constellation")]
        public CelestialObjectData[] linkedObjects;
    }
}
