using UnityEngine;
using UnityEngine.InputSystem;

namespace CelestiaVR
{
    /// <summary>
    /// Listens for right index trigger press.
    /// If an object is currently highlighted by DiscoveryManager → opens InspectionPanel.
    /// If panel is already open → closes it.
    /// </summary>
    public class InspectionTrigger : MonoBehaviour
    {
        // Right index trigger
        private InputAction _trigger;
        private CelestialObjectTag _lastHighlighted;

        private void Awake()
        {
            _trigger = new InputAction("InspectTrigger",
                binding: "<XRController>{RightHand}/triggerButton");
        }

        private void OnEnable()
        {
            _trigger.performed += OnTriggerPressed;
            _trigger.Enable();
        }

        private void OnDisable()
        {
            _trigger.performed -= OnTriggerPressed;
            _trigger.Disable();
        }

        private void OnTriggerPressed(InputAction.CallbackContext ctx)
        {
            if (InspectionPanel.Instance == null) return;

            // Close if already open
            if (InspectionPanel.Instance.IsOpen)
            {
                InspectionPanel.Instance.Close();
                return;
            }

            // Find the current highlighted/discovered object via DiscoveryManager
            // We track the last dwell-completed object through DiscoveryManager's event
            if (_lastHighlighted != null && _lastHighlighted.Data != null)
            {
                InspectionPanel.Instance.Open(_lastHighlighted.Data, _lastHighlighted.transform);
            }
        }

        private void OnEnable_DiscoveryHook()
        {
            if (DiscoveryManager.Instance != null)
                DiscoveryManager.Instance.OnObjectDiscovered.AddListener(OnObjectDiscovered);
        }

        private void Start()
        {
            // Hook into discovery events after all Awake() calls
            if (DiscoveryManager.Instance != null)
                DiscoveryManager.Instance.OnObjectDiscovered.AddListener(OnObjectDiscovered);
        }

        private void OnDestroy()
        {
            if (DiscoveryManager.Instance != null)
                DiscoveryManager.Instance.OnObjectDiscovered.RemoveListener(OnObjectDiscovered);
        }

        private void OnObjectDiscovered(CelestialObjectData data)
        {
            // Find the tag in the scene that matches this data
            var tags = FindObjectsByType<CelestialObjectTag>(FindObjectsSortMode.None);
            foreach (var tag in tags)
                if (tag.Data == data) { _lastHighlighted = tag; break; }
        }
    }
}
