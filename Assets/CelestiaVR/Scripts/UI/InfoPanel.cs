using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CelestiaVR
{
    /// <summary>
    /// World-space info panel that appears when a celestial object is discovered.
    ///
    /// Scene setup:
    ///   InfoPanel (this script, Canvas in World Space, initially inactive)
    ///   ├── ObjectName       (TextMeshProUGUI)
    ///   ├── ObjectType       (TextMeshProUGUI)
    ///   ├── Description      (TextMeshProUGUI)
    ///   ├── Distance         (TextMeshProUGUI)
    ///   ├── Fact             (TextMeshProUGUI)
    ///   └── ConstellationDiagram (Image)
    ///
    /// Wire DiscoveryManager.OnObjectDiscovered → InfoPanel.Show() in Inspector.
    /// </summary>
    public class InfoPanel : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI objectNameText;
        [SerializeField] private TextMeshProUGUI objectTypeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI factText;

        [Header("Visuals")]
        [SerializeField] private Image constellationDiagram;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Behaviour")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float autoHideAfterSeconds = 0f;  // 0 = never auto-hide
        [SerializeField] private bool billboardToCamera = true;

        private Camera _mainCamera;
        private Coroutine _fadeCoroutine;
        private Coroutine _autoHideCoroutine;

        private void Awake()
        {
            _mainCamera = Camera.main;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!gameObject.activeSelf || !billboardToCamera || _mainCamera == null) return;
            transform.rotation = Quaternion.LookRotation(
                transform.position - _mainCamera.transform.position);
        }

        /// <summary>
        /// Called by DiscoveryManager.OnObjectDiscovered (wire in Inspector).
        /// </summary>
        public void Show(CelestialObjectData data)
        {
            if (data == null) return;

            if (objectNameText)    objectNameText.text    = data.objectName;
            if (objectTypeText)    objectTypeText.text    = data.objectType.ToString();
            if (descriptionText)   descriptionText.text   = data.description;
            if (distanceText)      distanceText.text      = $"Distance: {data.distanceFromEarth}";
            if (factText)          factText.text          = data.objectFact;

            if (constellationDiagram)
            {
                constellationDiagram.sprite = data.constellationDiagram;
                constellationDiagram.gameObject.SetActive(data.constellationDiagram != null);
            }

            gameObject.SetActive(true);

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(0f, 1f));

            if (autoHideAfterSeconds > 0f)
            {
                if (_autoHideCoroutine != null) StopCoroutine(_autoHideCoroutine);
                _autoHideCoroutine = StartCoroutine(AutoHide());
            }
        }

        public void Hide()
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(canvasGroup.alpha, 0f, deactivateOnDone: true));
        }

        private IEnumerator Fade(float from, float to, bool deactivateOnDone = false)
        {
            if (canvasGroup == null) yield break;
            float t = 0f;
            canvasGroup.alpha = from;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            canvasGroup.alpha = to;
            if (deactivateOnDone) gameObject.SetActive(false);
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(autoHideAfterSeconds);
            Hide();
        }
    }
}
