using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CelestiaVR
{
    /// <summary>
    /// Futuristic holographic inspection panel that appears 1.5m in front of the user.
    ///
    /// Layout:
    ///   Left  half → 3D model viewport (planet sphere or constellation star map)
    ///   Right half → info text (name, type, distance, fact)
    ///
    /// Triggered by right index trigger when an object is highlighted.
    /// </summary>
    public class InspectionPanel : MonoBehaviour
    {
        public static InspectionPanel Instance { get; private set; }

        [Header("Panel Root")]
        [SerializeField] private Transform panelRoot;           // the whole panel
        [SerializeField] private CanvasGroup canvasGroup;       // for fade
        [SerializeField] private float distanceFromUser = 1.5f;

        [Header("3D Model Stage")]
        [SerializeField] private Transform modelStage;          // where 3D model spawns
        [SerializeField] private float modelStageRadius = 0.12f;

        [Header("Info Fields")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI factText;

        [Header("Animation")]
        [SerializeField] private float animDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip panelAppearSound;

        [Header("Sub-views")]
        [SerializeField] private PlanetInspectionView planetView;
        [SerializeField] private ConstellationInspectionView constellationView;

        private Camera _mainCamera;
        private bool _isOpen;
        private Coroutine _animCoroutine;
        private GameObject _pulledObject;     // the sky object being inspected

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _mainCamera = Camera.main;
            panelRoot.gameObject.SetActive(false);
        }

        /// <summary>
        /// Opens the inspection panel for a discovered celestial object.
        /// skyObjectTransform = the GameObject in the sky (for pull animation origin).
        /// </summary>
        public void Open(CelestialObjectData data, Transform skyObjectTransform = null)
        {
            if (data == null) return;
            if (_isOpen) Close();

            _isOpen = true;
            _pulledObject = skyObjectTransform?.gameObject;

            // Position panel in front of user
            Vector3 forward = _mainCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 targetPos = _mainCamera.transform.position + forward * distanceFromUser;
            targetPos.y = _mainCamera.transform.position.y;

            panelRoot.position = targetPos;
            panelRoot.rotation = Quaternion.LookRotation(forward);

            // Fill info
            if (nameText)        nameText.text        = data.objectName;
            if (typeText)        typeText.text        = data.objectType.ToString().ToUpper();
            if (descriptionText) descriptionText.text = data.description;
            if (distanceText)    distanceText.text    = data.distanceFromEarth;
            if (factText)        factText.text        = data.objectFact;

            // Show correct 3D view
            planetView?.gameObject.SetActive(false);
            constellationView?.gameObject.SetActive(false);

            if (data.objectType == CelestialObjectType.Planet ||
                data.objectType == CelestialObjectType.Star   ||
                data.objectType == CelestialObjectType.Moon)
            {
                planetView?.gameObject.SetActive(true);
                planetView?.Show(data, modelStageRadius);
            }
            else
            {
                constellationView?.gameObject.SetActive(true);
                constellationView?.Show(data, modelStageRadius);
            }

            panelRoot.gameObject.SetActive(true);

            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimateOpen(skyObjectTransform));
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimateClose());
        }

        private IEnumerator AnimateOpen(Transform origin)
        {
            audioSource?.PlayOneShot(panelAppearSound);
            panelRoot.localScale = Vector3.one * 0.01f;
            if (canvasGroup) canvasGroup.alpha = 0f;

            // If we have an origin in the sky, start from there
            Vector3 startPos = origin != null ? origin.position : panelRoot.position;
            Vector3 endPos   = panelRoot.position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / animDuration;
                float ease = 1f - Mathf.Pow(1f - t, 3f); // ease out cubic
                panelRoot.position   = Vector3.Lerp(startPos, endPos, ease);
                panelRoot.localScale = Vector3.Lerp(Vector3.one * 0.01f, Vector3.one, ease);
                if (canvasGroup) canvasGroup.alpha = ease;
                yield return null;
            }

            panelRoot.localScale = Vector3.one;
            if (canvasGroup) canvasGroup.alpha = 1f;
        }

        private IEnumerator AnimateClose()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / (animDuration * 0.6f);
                float ease = Mathf.Pow(t, 2f);
                panelRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.01f, ease);
                if (canvasGroup) canvasGroup.alpha = 1f - ease;
                yield return null;
            }
            panelRoot.gameObject.SetActive(false);
            panelRoot.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            // Always face the user
            if (_isOpen && _mainCamera != null)
            {
                Vector3 dir = panelRoot.position - _mainCamera.transform.position;
                if (dir.sqrMagnitude > 0.001f)
                    panelRoot.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}
