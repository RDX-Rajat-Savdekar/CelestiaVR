using UnityEngine;
using UnityEngine.InputSystem;

namespace CelestiaVR
{
    /// <summary>
    /// Handles telescope button inputs:
    ///   X button (left controller primary)  → toggle Discovery Mode
    ///   B button (right controller secondary) → open/close settings panel
    ///
    /// Uses hardcoded Quest controller binding paths — no InputActionAsset needed.
    /// </summary>
    public class TelescopeInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject discoveryModeIndicator;
        [SerializeField] private GameObject settingsPanel;

        private InputAction _xButton;
        private InputAction _bButton;
        private bool _discoveryModeOn = false;

        private void Awake()
        {
            // X button = left controller primaryButton
            _xButton = new InputAction("XButton", binding: "<XRController>{LeftHand}/primaryButton");
            // B button = right controller secondaryButton
            _bButton = new InputAction("BButton", binding: "<XRController>{RightHand}/secondaryButton");
        }

        private void OnEnable()
        {
            _xButton.performed += OnDiscoveryToggle;
            _bButton.performed += OnSettingsToggle;
            _xButton.Enable();
            _bButton.Enable();
        }

        private void OnDisable()
        {
            _xButton.performed -= OnDiscoveryToggle;
            _bButton.performed -= OnSettingsToggle;
            _xButton.Disable();
            _bButton.Disable();
        }

        private void OnDiscoveryToggle(InputAction.CallbackContext ctx)
        {
            _discoveryModeOn = !_discoveryModeOn;
            ConstellationManager.Instance?.ToggleLines();

            if (discoveryModeIndicator != null)
                discoveryModeIndicator.SetActive(_discoveryModeOn);

            Debug.Log($"[Telescope] Discovery Mode: {(_discoveryModeOn ? "ON" : "OFF")}");
        }

        private void OnSettingsToggle(InputAction.CallbackContext ctx)
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}
