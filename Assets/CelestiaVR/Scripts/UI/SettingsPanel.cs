using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CelestiaVR
{
    /// <summary>
    /// Settings panel on the observatory wall (B button opens it).
    ///
    /// Scene setup:
    ///   SettingsPanel (this script, World Space Canvas, starts inactive)
    ///   ├── AudioSlider          (Slider — wire to SetAudioVolume)
    ///   ├── ConstellationToggle  (Toggle — wire to ToggleConstellations)
    ///   ├── LocomotionToggle     (Toggle — wire to ToggleLocomotion)
    ///   └── ExitButton           (Button — wire to ExitExperience)
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";

        [Header("Locomotion")]
        [SerializeField] private GameObject continuousMovementProvider;
        [SerializeField] private GameObject teleportProvider;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        // ── Wired from Slider.onValueChanged ──────────────────────────────────
        public void SetAudioVolume(float sliderValue)
        {
            // Slider 0-1 → log scale for AudioMixer (-80 to 0 dB)
            float db = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
            audioMixer?.SetFloat(masterVolumeParam, db);
        }

        // ── Wired from Toggle.onValueChanged ──────────────────────────────────
        public void ToggleConstellations(bool on)
        {
            // Force constellation lines to the desired state
            if (ConstellationManager.Instance == null) return;
            bool current = ConstellationManager.Instance.AreLinesVisible;
            if (current != on) ConstellationManager.Instance.ToggleLines();
        }

        public void ToggleLocomotion(bool useTeleport)
        {
            if (continuousMovementProvider != null)
                continuousMovementProvider.SetActive(!useTeleport);
            if (teleportProvider != null)
                teleportProvider.SetActive(useTeleport);
        }

        // ── Wired from ExitButton.onClick ──────────────────────────────────────
        public void ExitExperience()
        {
            StartCoroutine(FadeAndExit());
        }

        private System.Collections.IEnumerator FadeAndExit()
        {
            // Simple fade to black — replace with your scene fade system if available
            yield return new WaitForSeconds(0.5f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
