using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System;
using QLDMathApp.Architecture.UI;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// Forest TERMINAL BUTTON: Tactile, high-tech button for pilot input.
    /// - Neon green borders
    /// - Digital glitch feedback
    /// - Audio feedback with Forest chimes
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AnswerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Forest Theme")]
        [SerializeField] private Architecture.UI.ForestTheme theme;

        [Header("UI")]
        [SerializeField] private Text numberText;
        [SerializeField] private Image buttonImage;
        [SerializeField, FormerlySerializedAs("shadowImage")] private Image outlineImage; 
        
        [Header("High-Tech Settings")]
        [SerializeField] private float pressOffset = 2f; 
        
        [Header("Audio")]
        [SerializeField] private AudioClip tapSound;
        
        private Button _button;
        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private int _value;
        private AudioSource _audioSource;
        
        public Action<int> OnPressed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
            _audioSource = GetComponentInParent<AudioSource>();
            
            _button.onClick.AddListener(HandleClick);
        }

        public void SetValue(int value)
        {
            _value = value;
            numberText.text = value.ToString();
            
            // Apply Forest Theme
            if (theme != null)
            {
                buttonImage.color = new Color(0.1f, 0.1f, 0.1f, theme.panelAlpha);
                if (outlineImage != null) outlineImage.color = theme.vineBorderColor;
                numberText.color = theme.primaryTextColor;
            }
            
            _rectTransform.anchoredPosition = _originalPosition;
            if (outlineImage != null) outlineImage.enabled = true;
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
            _rectTransform.anchoredPosition = _originalPosition - new Vector2(0, pressOffset);
            
            if (theme != null)
                buttonImage.color = theme.growthGreen;
            
            if (outlineImage != null) outlineImage.enabled = false;
            
            if (tapSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(tapSound, 0.5f);
            }
            
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition = _originalPosition;
            
            if (theme != null)
                buttonImage.color = new Color(0.1f, 0.1f, 0.1f, theme.panelAlpha);
                
            if (outlineImage != null) outlineImage.enabled = true;
        }

        private void HandleClick()
        {
            OnPressed?.Invoke(_value);
        }
    }
}
