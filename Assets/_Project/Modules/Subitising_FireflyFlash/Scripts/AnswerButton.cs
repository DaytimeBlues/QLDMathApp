using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// NEO-SKEUOMORPHIC BUTTON: Tactile, 3D-style button for young children.
    /// - Large touch target (60x60 minimum)
    /// - Visual "press down" effect
    /// - Audio feedback on touch
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AnswerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("UI")]
        [SerializeField] private Text numberText;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image shadowImage;
        
        [Header("Neo-Skeuomorphic Settings")]
        [SerializeField] private float pressOffset = 4f; // Pixels to move down on press
        [SerializeField] private Color normalColor = new Color(0.95f, 0.85f, 0.7f); // Clay/cream
        [SerializeField] private Color pressedColor = new Color(0.85f, 0.75f, 0.6f);
        
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
            
            // Reset visual state
            buttonImage.color = normalColor;
            _rectTransform.anchoredPosition = _originalPosition;
            if (shadowImage != null) shadowImage.enabled = true;
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        // IPointerDownHandler - Immediate tactile feedback
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
            // Visual: Push down effect (like a real button)
            _rectTransform.anchoredPosition = _originalPosition - new Vector2(0, pressOffset);
            buttonImage.color = pressedColor;
            
            // Hide shadow when "pressed in"
            if (shadowImage != null) shadowImage.enabled = false;
            
            // Audio feedback
            if (tapSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(tapSound, 0.5f);
            }
            
            // Haptic feedback (requires mobile plugin)
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate(); // Simple vibration
            #endif
        }

        // IPointerUpHandler - Return to normal
        public void OnPointerUp(PointerEventData eventData)
        {
            // Visual: Pop back up
            _rectTransform.anchoredPosition = _originalPosition;
            buttonImage.color = normalColor;
            if (shadowImage != null) shadowImage.enabled = true;
        }

        private void HandleClick()
        {
            OnPressed?.Invoke(_value);
        }
    }
}
