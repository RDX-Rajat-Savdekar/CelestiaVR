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

        [Header("Audio")]
        [SerializeField] private AudioSource oneShotSource;    // grab, end-stop, zoom
        [SerializeField] private AudioSource frictionSource;   // looping friction during pan
        [SerializeField] private AudioClip grabSound;
        [SerializeField] private AudioClip frictionSound;
        [SerializeField] private AudioClip zoomSound;
        [SerializeField] private AudioClip endStopSound;

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
            oneShotSource?.PlayOneShot(grabSound);
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            _isGrabbed = false;
            if (eyepieceCamera != null)
                eyepieceCamera.enabled = false;
            if (frictionSource != null && frictionSource.isPlaying)
                frictionSource.Stop();
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

            // Friction audio — loop while panning, pitch by speed
            if (frictionSource != null && frictionSound != null)
            {
                if (input.sqrMagnitude >= 0.01f)
                {
                    frictionSource.pitch = Mathf.Lerp(0.8f, 1.4f, input.magnitude);
                    if (!frictionSource.isPlaying)
                    {
                        frictionSource.clip = frictionSound;
                        frictionSource.loop = true;
                        frictionSource.Play();
                    }
                }
                else if (frictionSource.isPlaying)
                {
                    frictionSource.Stop();
                }
            }

            if (input.sqrMagnitude < 0.01f) return;

            _azimuth   += input.x * panSpeed * Time.deltaTime;
            _elevation -= input.y * panSpeed * Time.deltaTime;
            _elevation  = Mathf.Clamp(_elevation, elevationMin, elevationMax);

            if (barrelPivot != null)
                barrelPivot.localRotation = Quaternion.Euler(_elevation, _azimuth, 0f);
        }

        private bool _wasAtFovLimit = false;

        private void HandleZoom()
        {
            if (rightJoystick == null) return;
            float input = rightJoystick.action.ReadValue<Vector2>().y;
            if (Mathf.Abs(input) < 0.01f) return;

            float prevFov = _currentFov;
            _currentFov += input * zoomSpeed * Time.deltaTime;
            float clampedFov = Mathf.Clamp(_currentFov, fovMin, fovMax);

            // End-stop clunk when hitting FOV limits
            bool atLimit = Mathf.Approximately(clampedFov, _currentFov == clampedFov ? clampedFov : prevFov)
                           && (clampedFov <= fovMin || clampedFov >= fovMax);
            bool hitLimit = clampedFov != _currentFov;
            _currentFov = clampedFov;

            if (hitLimit && !_wasAtFovLimit)
                oneShotSource?.PlayOneShot(endStopSound);
            _wasAtFovLimit = hitLimit || (clampedFov <= fovMin || clampedFov >= fovMax);

            // Zoom movement sound (one-shot, don't spam — only on first input frame)
            if (Mathf.Abs(prevFov - _currentFov) > 0.05f && oneShotSource != null && zoomSound != null
                && !oneShotSource.isPlaying)
                oneShotSource.PlayOneShot(zoomSound, 0.4f);
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
