using UnityEngine;

namespace CelestiaVR
{
    /// <summary>
    /// Floor glow ring that appears when the player approaches the telescope.
    /// Pulses to draw attention when the player is nearby but hasn't grabbed it yet.
    ///
    /// Scene setup:
    ///   Add this to the Telescope GameObject.
    ///   Create a Quad flat on the floor as a child (GlowRing), assign it here.
    ///   Use an Unlit emissive material with a circular ring texture.
    /// </summary>
    public class TelescopeProximityIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject glowRing;
        [SerializeField] private float activationRadius = 2f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float minAlpha = 0.2f;
        [SerializeField] private float maxAlpha = 0.8f;

        private Transform _playerHead;
        private Renderer _ringRenderer;
        private MaterialPropertyBlock _propBlock;
        private bool _telescopeGrabbed;

        private void Start()
        {
            if (glowRing != null)
            {
                _ringRenderer = glowRing.GetComponent<Renderer>();
                _propBlock = new MaterialPropertyBlock();
                glowRing.SetActive(false);
            }

            var cam = Camera.main;
            if (cam != null) _playerHead = cam.transform;
        }

        public void OnTelescopeGrabbed()  => _telescopeGrabbed = true;
        public void OnTelescopeReleased() => _telescopeGrabbed = false;

        private void Update()
        {
            if (_playerHead == null || glowRing == null) return;

            float dist = Vector3.Distance(_playerHead.position, transform.position);
            bool inRange = dist < activationRadius && !_telescopeGrabbed;

            glowRing.SetActive(inRange);

            if (inRange && _ringRenderer != null)
            {
                float alpha = Mathf.Lerp(minAlpha, maxAlpha,
                    (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

                _ringRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat("_Alpha", alpha);
                _ringRenderer.SetPropertyBlock(_propBlock);
            }
        }
    }
}
