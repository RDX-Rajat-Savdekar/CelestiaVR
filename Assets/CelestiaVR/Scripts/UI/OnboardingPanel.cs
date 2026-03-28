using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CelestiaVR
{
    /// <summary>
    /// Onboarding panel shown at experience start.
    /// Auto-dismisses after 20 seconds or immediately on Skip button press.
    ///
    /// Scene setup:
    ///   OnboardingPanel (this script, World Space Canvas, starts active)
    ///   ├── ControllerDiagram  (Image — assign controller diagram sprite)
    ///   └── SkipButton         (Button — wire to Skip() in Inspector)
    /// </summary>
    public class OnboardingPanel : MonoBehaviour
    {
        [SerializeField] private float autoDismissSeconds = 20f;
        [SerializeField] private InputActionReference skipAction;  // optional: any face button
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (skipAction != null)
                skipAction.action.performed += _ => Skip();

            StartCoroutine(AutoDismiss());
        }

        private void OnDisable()
        {
            if (skipAction != null)
                skipAction.action.performed -= _ => Skip();
        }

        public void Skip() => StartCoroutine(FadeOut());

        private IEnumerator AutoDismiss()
        {
            yield return new WaitForSeconds(autoDismissSeconds);
            yield return FadeOut();
        }

        private IEnumerator FadeOut()
        {
            float t = 0f;
            float start = canvasGroup != null ? canvasGroup.alpha : 1f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.5f;
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(start, 0f, t);
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
