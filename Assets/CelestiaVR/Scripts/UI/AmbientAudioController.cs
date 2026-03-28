using System.Collections;
using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Manages layered ambient audio for the Griffith Observatory balcony.
    ///
    /// Scene setup — attach to an empty GameObject "AmbientAudio":
    ///   Assign AudioClips in Inspector. All loops play automatically on Start.
    ///   One-shot sounds (crickets, coyote) fire randomly on timers.
    ///
    /// Clips needed (all free from freesound.org / BBC Sound Effects):
    ///   windLoop       — gentle wind loop
    ///   windGust       — single wind gust burst
    ///   cityHum        — distant LA traffic hum loop
    ///   cricketOneShot — single cricket chirp
    ///   coyoteRare     — distant coyote howl (rare)
    /// </summary>
    public class AmbientAudioController : MonoBehaviour
    {
        [Header("Loop Sources")]
        [SerializeField] private AudioSource windLoopSource;
        [SerializeField] private AudioSource cityHumSource;

        [Header("One-Shot Sources")]
        [SerializeField] private AudioSource oneShotSource;

        [Header("Clips")]
        [SerializeField] private AudioClip windLoop;
        [SerializeField] private AudioClip windGust;
        [SerializeField] private AudioClip cityHum;
        [SerializeField] private AudioClip cricketOneShot;
        [SerializeField] private AudioClip coyoteRare;

        [Header("Timing")]
        [SerializeField] private float windGustMinInterval  = 8f;
        [SerializeField] private float windGustMaxInterval  = 20f;
        [SerializeField] private float cricketMinInterval   = 5f;
        [SerializeField] private float cricketMaxInterval   = 15f;
        [SerializeField] private float coyoteChance         = 0.08f;  // 8% chance per cricket event

        [Header("Volumes")]
        [SerializeField] private float windVolume   = 0.4f;
        [SerializeField] private float cityVolume   = 0.25f;
        [SerializeField] private float gustVolume   = 0.6f;
        [SerializeField] private float cricketVolume = 0.5f;
        [SerializeField] private float coyoteVolume  = 0.35f;

        private void Start()
        {
            PlayLoop(windLoopSource,  windLoop,  windVolume);
            PlayLoop(cityHumSource,   cityHum,   cityVolume);

            StartCoroutine(WindGustRoutine());
            StartCoroutine(CricketRoutine());
        }

        private void PlayLoop(AudioSource source, AudioClip clip, float volume)
        {
            if (source == null || clip == null) return;
            source.clip   = clip;
            source.loop   = true;
            source.volume = volume;
            source.spatialBlend = 0f; // 2D — ambient fills the whole space
            source.Play();
        }

        private IEnumerator WindGustRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(windGustMinInterval, windGustMaxInterval));
                PlayOneShot(windGust, gustVolume);
            }
        }

        private IEnumerator CricketRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(cricketMinInterval, cricketMaxInterval));
                PlayOneShot(cricketOneShot, cricketVolume);

                if (coyoteRare != null && Random.value < coyoteChance)
                {
                    yield return new WaitForSeconds(2f);
                    PlayOneShot(coyoteRare, coyoteVolume);
                }
            }
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (oneShotSource == null || clip == null) return;
            oneShotSource.PlayOneShot(clip, volume);
        }

        /// <summary>Called by SettingsPanel audio slider.</summary>
        public void SetMasterVolume(float volume)
        {
            if (windLoopSource)  windLoopSource.volume  = windVolume  * volume;
            if (cityHumSource)   cityHumSource.volume   = cityVolume  * volume;
        }
    }
}
