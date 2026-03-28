using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CelestiaVR
{
    /// <summary>
    /// Handles the experience's opening sequence:
    ///   1. Starts in dark void (fade overlay fully black)
    ///   2. Onboarding panel appears (controller diagram)
    ///   3. After onboarding dismissed → fade in to Griffith balcony
    ///
    /// Scene setup:
    ///   SceneTransitionController (this script)
    ///   Attach a full-screen black Image on a Screen Space Overlay Canvas as the fade overlay.
    ///   Or use a black quad parented to the camera for VR (recommended).
    ///
    /// The balcony environment should start disabled and gets enabled after fade.
    /// </summary>
    public class SceneTransitionController : MonoBehaviour
    {
        [Header("Fade")]
        [Tooltip("A black quad/Image covering the view — alpha 1=black, 0=clear")]
        [SerializeField] private Graphic fadeOverlay;
        [SerializeField] private float fadeDuration = 2f;

        [Header("Scene Objects")]
        [SerializeField] private GameObject balconyEnvironment;   // enable after fade
        [SerializeField] private OnboardingPanel onboardingPanel;

        [Header("Audio")]
        [SerializeField] private AmbientAudioController ambientAudio;

        private void Start()
        {
            // Start fully black
            if (fadeOverlay != null)
            {
                var c = fadeOverlay.color;
                c.a = 1f;
                fadeOverlay.color = c;
            }

            if (balconyEnvironment != null)
                balconyEnvironment.SetActive(false);

            StartCoroutine(OpeningSequence());
        }

        private IEnumerator OpeningSequence()
        {
            // Brief hold in black void
            yield return new WaitForSeconds(0.5f);

            // Show onboarding panel (it auto-dismisses after 20s)
            if (onboardingPanel != null)
                onboardingPanel.gameObject.SetActive(true);

            // Wait for onboarding to finish (20s or skip)
            // Poll until onboarding panel is inactive
            if (onboardingPanel != null)
                yield return new WaitUntil(() => !onboardingPanel.gameObject.activeSelf);

            // Enable environment
            if (balconyEnvironment != null)
                balconyEnvironment.SetActive(true);

            // Fade in
            yield return StartCoroutine(Fade(1f, 0f));

            // Fade overlay no longer needed
            if (fadeOverlay != null)
                fadeOverlay.gameObject.SetActive(false);
        }

        /// <summary>Call this to fade to black and exit.</summary>
        public IEnumerator FadeToBlackAndExit(float holdDuration = 0.5f)
        {
            if (fadeOverlay != null)
                fadeOverlay.gameObject.SetActive(true);

            yield return StartCoroutine(Fade(0f, 1f));
            yield return new WaitForSeconds(holdDuration);
        }

        private IEnumerator Fade(float fromAlpha, float toAlpha)
        {
            if (fadeOverlay == null) yield break;

            float t = 0f;
            Color c = fadeOverlay.color;

            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                fadeOverlay.color = c;
                yield return null;
            }

            c.a = toAlpha;
            fadeOverlay.color = c;
        }
    }
}
