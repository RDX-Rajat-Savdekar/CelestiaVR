using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// ScriptableObject holding data for a single celestial object.
    /// Positions are given as J2000 equatorial coordinates (RA/Dec).
    /// RA is in decimal hours (0–24), Dec is in decimal degrees (-90 to +90).
    /// </summary>
    [CreateAssetMenu(fileName = "NewCelestialObject", menuName = "CelestiaVR/Celestial Object Data")]
    public class CelestialObjectData : ScriptableObject
    {
        [Header("Identity")]
        public string objectName = "Unknown";
        public CelestialObjectType objectType;

        [Header("Position (J2000 Equatorial)")]
        [Tooltip("Right Ascension in decimal hours (0–24)")]
        public float rightAscensionHours = 0f;
        [Tooltip("Declination in decimal degrees (-90 to +90)")]
        public float declinationDegrees = 0f;

        [Header("Info Panel Content")]
        [TextArea(2, 4)]
        public string description = "";
        public string distanceFromEarth = "";
        public string objectFact = "";
        public Sprite constellationDiagram;

        [Header("Visual")]
        [Tooltip("Scale multiplier applied to the prefab when placed in the sky dome")]
        public float visualScale = 1f;
        public Material objectMaterial;
    }

    public enum CelestialObjectType
    {
        Planet,
        Star,
        Nebula,
        Galaxy,
        StarCluster,
        Moon
    }
}
