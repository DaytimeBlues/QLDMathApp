using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using QLDMathApp.Architecture.UI;

namespace QLDMathApp.Modules.Patterns
{
    /// <summary>
    /// PATTERN FLOWER: Individual flower for the pattern-building garden activity.
    /// Children complete the sequence of colored flowers to make the garden bloom.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PatternFlower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Garden Theme")]
        [SerializeField] private Architecture.UI.ForestTheme theme; // TODO: Replace with GardenTheme

        [Header("UI References")]
        [SerializeField] private Image flowerImage;
        [SerializeField] private Image leafImage;
        [SerializeField] private Image questionMarkIcon; 
        
        [Header("Animation")]
        [SerializeField] private float pulseScale = 1.25f;
        [SerializeField] private float pulseDuration = 0.4f;
        
        private Button _button;
        private int _valueIndex;
        private bool _isMystery;
        private bool _isChoice;
        private Color _originalColor;
        private Vector3 _originalScale;

        public int ValueIndex => _valueIndex;
        public bool IsMystery => _isMystery;
        public event Action<PatternFlower> OnSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
            _button.onClick.AddListener(HandleClick);
        }

        public void SetupDisplay(Sprite flowerSprite, Color color, int index)
        {
            _valueIndex = index;
            _isMystery = false;
            _isChoice = false;

            flowerImage.sprite = flowerSprite;
            flowerImage.color = color;
            _originalColor = color;
            
            if (questionMarkIcon != null) questionMarkIcon.gameObject.SetActive(false);
            _button.interactable = false;
        }

        public void SetupMystery()
        {
            _isMystery = true;
            _isChoice = false;

            flowerImage.gameObject.SetActive(false);
            if (questionMarkIcon != null) questionMarkIcon.gameObject.SetActive(true);
            
            _button.interactable = false;
        }

        public void SetupChoice(Sprite flowerSprite, Color color, int valueIndex)
        {
            _valueIndex = valueIndex;
            _isMystery = false;
            _isChoice = true;

            flowerImage.sprite = flowerSprite;
            flowerImage.color = color;
            _originalColor = color;
            
            if (questionMarkIcon != null) questionMarkIcon.gameObject.SetActive(false);
            _button.interactable = true;
        }

        public void RevealAs(Sprite flowerSprite, Color color)
        {
            _isMystery = false;
            
            flowerImage.gameObject.SetActive(true);
            flowerImage.sprite = flowerSprite;
            flowerImage.color = color;
            
            if (questionMarkIcon != null) questionMarkIcon.gameObject.SetActive(false);
            
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
            Vector3 targetScale = _originalScale * pulseScale;
            
            // Gentle garden pulse
            for (float t = 0; t < pulseDuration / 2; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(_originalScale, targetScale, t / (pulseDuration / 2));
                yield return null;
            }
            
            for (float t = 0; t < pulseDuration / 2; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(targetScale, _originalScale, t / (pulseDuration / 2));
                yield return null;
            }

            transform.localScale = _originalScale;
        }

        private System.Collections.IEnumerator PopAnimation()
        {
            Vector3 startScale = Vector3.zero;
            Vector3 overshoot = _originalScale * 1.4f;
            
            for (float t = 0; t < 0.2f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(startScale, overshoot, t / 0.2f);
                yield return null;
            }
            
            for (float t = 0; t < 0.15f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(overshoot, _originalScale, t / 0.15f);
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

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isChoice && _button.interactable)
            {
                transform.localScale = _originalScale * 0.92f;
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
