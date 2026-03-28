using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace CelestiaVR
{
    /// <summary>
    /// Core telescope controller.
    ///
    /// Scene setup:
    ///   Telescope (this script + XRGrabInteractable + ConfigurableJoint)
    ///   └── EyepieceAnchor          (empty, where the player's eye goes)
    ///   └── EyepieceCamera          (Camera, renders to RenderTexture, disabled by default)
    ///   └── TelescopeBarrel         (visual mesh, rotates for pan)
    ///
    /// Controls (while grabbed):
    ///   Left joystick  → pan (azimuth / elevation)
    ///   Right joystick → zoom (FOV 2–60°)
    ///   X button       → toggle Discovery Mode (handled by TelescopeInputHandler)
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class TelescopeController : MonoBehaviour
    {
        [Header("Eyepiece Camera")]
        [SerializeField] private Camera eyepieceCamera;
        [SerializeField] private RenderTexture eyepieceRenderTexture;

        [Header("Pan Settings")]
        [SerializeField] private float panSpeed = 30f;           // degrees per second
        [SerializeField] private float elevationMin = -30f;
        [SerializeField] private float elevationMax = 85f;

        [Header("Zoom Settings")]
        [SerializeField] private float fovMin = 2f;
        [SerializeField] private float fovMax = 60f;
        [SerializeField] private float zoomSpeed = 20f;          // degrees FOV per second

        [Header("Input Actions")]
        [SerializeField] private InputActionReference leftJoystick;   // pan
        [SerializeField] private InputActionReference rightJoystick;  // zoom

        [Header("Pivot")]
        [Tooltip("The transform that rotates when panning. Usually the telescope barrel root.")]
        [SerializeField] private Transform barrelPivot;

        // State
        private XRGrabInteractable _grab;
        private bool _isGrabbed;
        private float _currentFov;
        private float _azimuth;    // horizontal angle
        private float _elevation;  // vertical angle

        public bool IsGrabbed => _isGrabbed;
        public Camera EyepieceCamera => eyepieceCamera;

        private void Awake()
        {
            _grab = GetComponent<XRGrabInteractable>();
            _grab.selectEntered.AddListener(OnGrabbed);
            _grab.selectExited.AddListener(OnReleased);

            _currentFov = fovMax;

            if (eyepieceCamera != null)
            {
                eyepieceCamera.enabled = false;
                if (eyepieceRenderTexture != null)
                    eyepieceCamera.targetTexture = eyepieceRenderTexture;
            }
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            _isGrabbed = true;
            if (eyepieceCamera != null)
                eyepieceCamera.enabled = true;
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            _isGrabbed = false;
            if (eyepieceCamera != null)
                eyepieceCamera.enabled = false;

            // Clear any active dwell when telescope is put down
            DiscoveryManager.Instance?.SetGazeTarget(null);
        }

        private void Update()
        {
            if (!_isGrabbed) return;

            HandlePan();
            HandleZoom();
            UpdateEyepieceCameraFov();
        }

        private void HandlePan()
        {
            if (leftJoystick == null) return;
            Vector2 input = leftJoystick.action.ReadValue<Vector2>();
            if (input.sqrMagnitude < 0.01f) return;

            _azimuth   += input.x * panSpeed * Time.deltaTime;
            _elevation -= input.y * panSpeed * Time.deltaTime;  // invert Y for natural feel
            _elevation  = Mathf.Clamp(_elevation, elevationMin, elevationMax);

            if (barrelPivot != null)
                barrelPivot.localRotation = Quaternion.Euler(_elevation, _azimuth, 0f);
        }

        private void HandleZoom()
        {
            if (rightJoystick == null) return;
            float input = rightJoystick.action.ReadValue<Vector2>().y;
            if (Mathf.Abs(input) < 0.01f) return;

            _currentFov += input * zoomSpeed * Time.deltaTime;
            _currentFov  = Mathf.Clamp(_currentFov, fovMin, fovMax);
        }

        private void UpdateEyepieceCameraFov()
        {
            if (eyepieceCamera != null)
                eyepieceCamera.fieldOfView = _currentFov;
        }

        private void OnDestroy()
        {
            if (_grab != null)
            {
                _grab.selectEntered.RemoveListener(OnGrabbed);
                _grab.selectExited.RemoveListener(OnReleased);
            }
        }
    }
}
