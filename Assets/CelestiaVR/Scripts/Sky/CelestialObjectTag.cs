using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Attached to every instantiated celestial object prefab.
    /// Provides a runtime reference to the object's ScriptableObject data.
    /// The telescope discovery system queries this component via raycast.
    /// </summary>
    public class CelestialObjectTag : MonoBehaviour
    {
        public CelestialObjectData Data { get; set; }
    }
}
