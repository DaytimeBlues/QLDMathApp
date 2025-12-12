using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.UI.Events;
using QLDMathApp.UI.Services;

namespace QLDMathApp.UI
{
    /// <summary>
    /// SETTINGS PANEL V2: Decoupled from MainMenuController.
    /// - Uses event channels for visibility
    /// - Syncs with AccessibilitySettingsService
    /// - Stays in sync if settings change elsewhere
    /// </summary>
    public class SettingsPanelV2 : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private BoolEventChannelSO settingsVisibilityRequested; // true=open, false=close

        [Header("Toggles")]
        [SerializeField] private Toggle zenModeToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle reducedMotionToggle;

        [Header("Icons")]
        [SerializeField] private Image zenModeIcon;
        [SerializeField] private Image highContrastIcon;
        [SerializeField] private Image reducedMotionIcon;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleOnSound;
        [SerializeField] private AudioClip toggleOffSound;
        [SerializeField] private AudioClip zenModeDescription;
        [SerializeField] private AudioClip highContrastDescription;
        [SerializeField] private AudioClip reducedMotionDescription;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        [Header("Styling")]
        [SerializeField] private Color onColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color offColor = new Color(0.8f, 0.8f, 0.8f);

        private bool _suppressToggleCallbacks;

        private void OnEnable()
        {
            // Subscribe to visibility events
            if (settingsVisibilityRequested != null)
                settingsVisibilityRequested.Raised += OnVisibilityRequested;

            // Subscribe to settings changes
            if (AccessibilitySettingsService.Instance != null)
                AccessibilitySettingsService.Instance.Changed += SyncFromService;

            // Wire up toggles
            if (zenModeToggle != null) zenModeToggle.onValueChanged.AddListener(OnZenChanged);
            if (highContrastToggle != null) highContrastToggle.onValueChanged.AddListener(OnContrastChanged);
            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged.AddListener(OnMotionChanged);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            SyncFromService();
        }

        private void OnDisable()
        {
            if (settingsVisibilityRequested != null)
                settingsVisibilityRequested.Raised -= OnVisibilityRequested;

            if (AccessibilitySettingsService.Instance != null)
                AccessibilitySettingsService.Instance.Changed -= SyncFromService;

            if (zenModeToggle != null) zenModeToggle.onValueChanged.RemoveListener(OnZenChanged);
            if (highContrastToggle != null) highContrastToggle.onValueChanged.RemoveListener(OnContrastChanged);
            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged.RemoveListener(OnMotionChanged);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        private void OnVisibilityRequested(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible) SyncFromService();
        }

        private void SyncFromService()
        {
            var svc = AccessibilitySettingsService.Instance;
            if (svc == null) return;

            _suppressToggleCallbacks = true;
            
            if (zenModeToggle != null) zenModeToggle.isOn = svc.ZenMode;
            if (highContrastToggle != null) highContrastToggle.isOn = svc.HighContrast;
            if (reducedMotionToggle != null) reducedMotionToggle.isOn = svc.ReducedMotion;
            
            _suppressToggleCallbacks = false;

            UpdateIconColors();
        }

        private void OnZenChanged(bool isOn)
        {
            if (_suppressToggleCallbacks) return;
            AccessibilitySettingsService.Instance?.SetZenMode(isOn);
            PlayToggle(isOn, zenModeDescription);
            UpdateIconColors();
        }

        private void OnContrastChanged(bool isOn)
        {
            if (_suppressToggleCallbacks) return;
            AccessibilitySettingsService.Instance?.SetHighContrast(isOn);
            PlayToggle(isOn, highContrastDescription);
            UpdateIconColors();
        }

        private void OnMotionChanged(bool isOn)
        {
            if (_suppressToggleCallbacks) return;
            AccessibilitySettingsService.Instance?.SetReducedMotion(isOn);
            PlayToggle(isOn, reducedMotionDescription);
            UpdateIconColors();
        }

        private void PlayToggle(bool isOn, AudioClip description)
        {
            if (audioSource == null) return;

            var sfx = isOn ? toggleOnSound : toggleOffSound;
            if (sfx != null) audioSource.PlayOneShot(sfx);
            if (description != null) audioSource.PlayOneShot(description);
        }

        private void UpdateIconColors()
        {
            if (zenModeIcon != null && zenModeToggle != null)
                zenModeIcon.color = zenModeToggle.isOn ? onColor : offColor;

            if (highContrastIcon != null && highContrastToggle != null)
                highContrastIcon.color = highContrastToggle.isOn ? onColor : offColor;

            if (reducedMotionIcon != null && reducedMotionToggle != null)
                reducedMotionIcon.color = reducedMotionToggle.isOn ? onColor : offColor;
        }

        private void Close()
        {
            if (settingsVisibilityRequested != null)
            {
                settingsVisibilityRequested.Raise(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
