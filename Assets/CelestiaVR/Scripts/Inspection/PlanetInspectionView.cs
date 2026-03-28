using UnityEngine;
using UnityEngine.InputSystem;

namespace CelestiaVR
{
    /// <summary>
    /// Shows a slowly rotating 3D planet/star sphere in the inspection panel's model stage.
    /// User can grab and rotate it with the right thumbstick while the panel is open.
    /// </summary>
    public class PlanetInspectionView : MonoBehaviour
    {
        [SerializeField] private float autoRotateSpeed = 15f;  // degrees/sec
        [SerializeField] private InputActionReference rightThumbstick;

        private GameObject _modelInstance;
        private bool _active;

        public void Show(CelestialObjectData data, float radius)
        {
            Clear();
            _active = true;

            // Create sphere
            _modelInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(_modelInstance.GetComponent<Collider>());
            _modelInstance.transform.SetParent(transform, false);
            _modelInstance.transform.localPosition = Vector3.zero;
            _modelInstance.transform.localScale    = Vector3.one * (radius * 1.8f);
            _modelInstance.name = data.objectName + "_Model";

            // Apply planet material if available
            if (data.objectMaterial != null)
                _modelInstance.GetComponent<Renderer>().material = data.objectMaterial;
            else
            {
                // Tint by object type as fallback
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = ColorForType(data.objectType);
                _modelInstance.GetComponent<Renderer>().material = mat;
            }
        }

        public void Clear()
        {
            _active = false;
            if (_modelInstance != null)
            {
                Destroy(_modelInstance);
                _modelInstance = null;
            }
        }

        private void Update()
        {
            if (!_active || _modelInstance == null) return;

            // Auto-rotate
            _modelInstance.transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.Self);

            // Manual rotate with right thumbstick X axis
            if (rightThumbstick != null)
            {
                Vector2 stick = rightThumbstick.action.ReadValue<Vector2>();
                _modelInstance.transform.Rotate(Vector3.up,   -stick.x * 60f * Time.deltaTime, Space.World);
                _modelInstance.transform.Rotate(Vector3.right,  stick.y * 60f * Time.deltaTime, Space.World);
            }
        }

        private static Color ColorForType(CelestialObjectType type) => type switch
        {
            CelestialObjectType.Planet => new Color(0.8f, 0.6f, 0.3f),
            CelestialObjectType.Star   => new Color(1f,   0.95f, 0.7f),
            CelestialObjectType.Moon   => new Color(0.7f, 0.7f, 0.7f),
            _                          => Color.white
        };
    }
}
