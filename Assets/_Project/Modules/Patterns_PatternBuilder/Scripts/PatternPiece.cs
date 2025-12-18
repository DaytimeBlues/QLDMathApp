using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace QLDMathApp.Modules.Patterns
{
    /// <summary>
    /// SYNC SIGNAL WAVEFORM: Individual signal block for Sync Ratio Sequence.
    /// Digital transitions and glitch chimes.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PatternPiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("NERV Theme")]
        [SerializeField] private Architecture.UI.NERVTheme theme;

        [Header("UI")]
        [SerializeField] private Image shapeImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image mysteryIcon; // Question mark for hidden slot
        
        [Header("Animation")]
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private float pulseDuration = 0.3f;
        
        private Button _button;
        private int _valueIndex;
        private bool _isMystery;
        private bool _isChoice;
        private Color _originalColor;
        private Vector3 _originalScale;

        public int ValueIndex => _valueIndex;
        public bool IsMystery => _isMystery;
        public event Action<PatternPiece> OnSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
            _button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// Setup as display piece (non-interactive, shows pattern).
        /// </summary>
        public void SetupDisplay(Sprite shape, Color color, int index)
        {
            _valueIndex = index;
            _isMystery = false;
            _isChoice = false;

            shapeImage.sprite = shape;
            shapeImage.color = color;
            _originalColor = color;
            
            if (mysteryIcon != null) mysteryIcon.gameObject.SetActive(false);
            
            _button.interactable = false;
        }

        /// <summary>
        /// Setup as mystery slot (shows question mark).
        /// </summary>
        public void SetupMystery()
        {
            _isMystery = true;
            _isChoice = false;

            shapeImage.gameObject.SetActive(false);
            if (mysteryIcon != null) mysteryIcon.gameObject.SetActive(true);
            
            _button.interactable = false;
        }

        /// <summary>
        /// Setup as choice button (interactive).
        /// </summary>
        public void SetupChoice(Sprite shape, Color color, int valueIndex)
        {
            _valueIndex = valueIndex;
            _isMystery = false;
            _isChoice = true;

            shapeImage.sprite = shape;
            shapeImage.color = color;
            _originalColor = color;
            
            if (mysteryIcon != null) mysteryIcon.gameObject.SetActive(false);
            
            _button.interactable = true;
        }

        /// <summary>
        /// Reveal mystery slot with actual answer.
        /// </summary>
        public void RevealAs(Sprite shape, Color color)
        {
            _isMystery = false;
            
            shapeImage.gameObject.SetActive(true);
            shapeImage.sprite = shape;
            shapeImage.color = color;
            
            if (mysteryIcon != null) mysteryIcon.gameObject.SetActive(false);
            
            // Pop animation
            StartCoroutine(PopAnimation());
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        public void Pulse()
        {
            StartCoroutine(PulseAnimation());
        }

        private System.Collections.IEnumerator PulseAnimation()
        {
            // Digital "Glitch" Pulse
            Vector3 targetScale = _originalScale * pulseScale;
            float duration = theme != null ? theme.glitchDuration : pulseDuration;
            
            // Pulse up (Instant jump)
            transform.localScale = targetScale;
            if (theme != null) shapeImage.color = theme.syncGreen;
            yield return new WaitForSeconds(duration / 2);

            // Back to normal
            transform.localScale = _originalScale;
            shapeImage.color = _originalColor;
        }

        private System.Collections.IEnumerator PopAnimation()
        {
            Vector3 startScale = Vector3.zero;
            Vector3 overshoot = _originalScale * 1.3f;
            
            // Pop in
            for (float t = 0; t < 0.15f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(startScale, overshoot, t / 0.15f);
                yield return null;
            }
            
            // Settle
            for (float t = 0; t < 0.1f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(overshoot, _originalScale, t / 0.1f);
                yield return null;
            }

            transform.localScale = _originalScale;
        }

        private void HandleClick()
        {
            if (_isChoice)
            {
                OnSelected?.Invoke(this);
            }
        }

        // Neo-Skeuomorphic press effect
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isChoice && _button.interactable)
            {
                transform.localScale = _originalScale * 0.95f;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isChoice)
            {
                transform.localScale = _originalScale;
            }
        }
    }
}
