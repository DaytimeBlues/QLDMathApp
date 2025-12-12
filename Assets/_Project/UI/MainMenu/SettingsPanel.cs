using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.Architecture.UI;

namespace QLDMathApp.UI
{
    /// <summary>
    /// SETTINGS PANEL: Toggle accessibility options.
    /// Uses icons + voice-over, no text for pre-readers.
    /// All toggles apply immediately.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Toggles")]
        [SerializeField] private Toggle zenModeToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle reducedMotionToggle;
        
        [Header("Toggle Icons")]
        [SerializeField] private Image zenModeIcon;
        [SerializeField] private Image highContrastIcon;
        [SerializeField] private Image reducedMotionIcon;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleOnSound;
        [SerializeField] private AudioClip toggleOffSound;
        [SerializeField] private AudioClip zenModeDescription; // "Quiet mode - less sounds"
        [SerializeField] private AudioClip highContrastDescription; // "Bright colors"
        [SerializeField] private AudioClip reducedMotionDescription; // "Less movement"
        
        [Header("Close Button")]
        [SerializeField] private Button closeButton;
        [SerializeField] private MainMenuController mainMenu;

        [Header("Styling")]
        [SerializeField] private Color onColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color offColor = new Color(0.8f, 0.8f, 0.8f);

        private void Start()
        {
            // Load current settings
            if (AccessibilitySettings.Instance != null)
            {
                zenModeToggle.isOn = AccessibilitySettings.Instance.ZenMode;
                highContrastToggle.isOn = AccessibilitySettings.Instance.HighContrast;
                reducedMotionToggle.isOn = AccessibilitySettings.Instance.ReducedMotion;
            }

            // Setup listeners
            zenModeToggle.onValueChanged.AddListener(OnZenModeChanged);
            highContrastToggle.onValueChanged.AddListener(OnHighContrastChanged);
            reducedMotionToggle.onValueChanged.AddListener(OnReducedMotionChanged);
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnClosePressed);
            }

            UpdateToggleVisuals();
        }

        private void OnZenModeChanged(bool isOn)
        {
            if (AccessibilitySettings.Instance != null)
            {
                AccessibilitySettings.Instance.SetZenMode(isOn);
            }
            
            PlayToggleSound(isOn);
            UpdateToggleVisuals();
            
            // Play description
            if (zenModeDescription != null && audioSource != null)
            {
                audioSource.PlayOneShot(zenModeDescription);
            }
        }

        private void OnHighContrastChanged(bool isOn)
        {
            if (AccessibilitySettings.Instance != null)
            {
                AccessibilitySettings.Instance.SetHighContrast(isOn);
            }
            
            PlayToggleSound(isOn);
            UpdateToggleVisuals();
            
            if (highContrastDescription != null && audioSource != null)
            {
                audioSource.PlayOneShot(highContrastDescription);
            }
        }

        private void OnReducedMotionChanged(bool isOn)
        {
            if (AccessibilitySettings.Instance != null)
            {
                AccessibilitySettings.Instance.SetReducedMotion(isOn);
            }
            
            PlayToggleSound(isOn);
            UpdateToggleVisuals();
            
            if (reducedMotionDescription != null && audioSource != null)
            {
                audioSource.PlayOneShot(reducedMotionDescription);
            }
        }

        private void PlayToggleSound(bool isOn)
        {
            if (audioSource == null) return;
            
            AudioClip clip = isOn ? toggleOnSound : toggleOffSound;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void UpdateToggleVisuals()
        {
            if (zenModeIcon != null)
            {
                zenModeIcon.color = zenModeToggle.isOn ? onColor : offColor;
            }
            
            if (highContrastIcon != null)
            {
                highContrastIcon.color = highContrastToggle.isOn ? onColor : offColor;
            }
            
            if (reducedMotionIcon != null)
            {
                reducedMotionIcon.color = reducedMotionToggle.isOn ? onColor : offColor;
            }
        }

        private void OnClosePressed()
        {
            if (mainMenu != null)
            {
                mainMenu.CloseSettings();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
