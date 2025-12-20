using UnityEngine;
using System.Collections;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// FIREFLY ANIMATOR: Animates individual fireflies with floating and glowing effects.
    /// Provides gentle, natural movement for the forest garden.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FireflyAnimator : MonoBehaviour
    {
        [Header("Nature Animation")]
        [SerializeField] private float floatAmplitude = 0.1f;
        [SerializeField] private float floatSpeed = 2f;
        
        [Header("Garden Glow")]
        [SerializeField] private float glowIntensity = 2.0f;
        [SerializeField] private float glowSpeed = 2f;
        
        private SpriteRenderer _spriteRenderer;
        private Vector3 _basePosition;
        private float _phaseOffset;
        private bool _isFloating;
        private Color _baseColor;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseColor = _spriteRenderer.color;
        }

        public void StartFloating(float phaseOffset = 0f)
        {
            _basePosition = transform.localPosition;
            _phaseOffset = phaseOffset;
            _isFloating = true;
        }

        public void StopFloating()
        {
            _isFloating = false;
            transform.localPosition = _basePosition;
        }

        private void Update()
        {
            if (_isFloating)
            {
                // Gentle swaying motion
                float yOffset = Mathf.Sin((Time.time + _phaseOffset) * floatSpeed) * floatAmplitude;
                float xOffset = Mathf.Sin((Time.time + _phaseOffset) * floatSpeed * 0.7f) * floatAmplitude * 0.5f;
                transform.localPosition = _basePosition + new Vector3(xOffset, yOffset, 0);
                
                // Magical firefly glow
                float glow = 0.8f + Mathf.Sin((Time.time + _phaseOffset) * glowSpeed) * 0.2f;
                _spriteRenderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * glow);
            }
        }

        public void FadeOut(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f, 0f, duration, true));
        }

        public void FadeIn(float duration)
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            StartCoroutine(FadeRoutine(0f, 1f, duration, false));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, bool hideAtEnd)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(from, to, t / duration);
                _spriteRenderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
                yield return null;
            }
            
            _spriteRenderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, to);
            
            if (hideAtEnd)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// MAGIC PULSE: Pulse when counted during the explanation phase.
        /// </summary>
        public void Pulse()
        {
            StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 pulseScale = originalScale * 1.5f;
            
            // Expand
            for (float t = 0; t < 0.15f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(originalScale, pulseScale, t / 0.15f);
                yield return null;
            }
            
            // Shrink back
            for (float t = 0; t < 0.25f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(pulseScale, originalScale, t / 0.25f);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
    }
}
