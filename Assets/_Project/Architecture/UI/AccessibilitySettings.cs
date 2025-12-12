using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace QLDMathApp.Architecture.UI
{
    /// <summary>
    /// ACCESSIBILITY SETTINGS: Inclusive design for diverse learners.
    /// - Zen Mode: Reduces sensory stimulation (for ASD students)
    /// - High Contrast: WCAG 2.1 AA compliant colors
    /// - Audio Descriptions: Extended voice-over support
    /// </summary>
    public class AccessibilitySettings : MonoBehaviour
    {
        public static AccessibilitySettings Instance { get; private set; }

        [Header("Current Settings")]
        [SerializeField] private bool zenModeEnabled;
        [SerializeField] private bool highContrastEnabled;
        [SerializeField] private bool reducedMotionEnabled;
        [SerializeField] private float audioDescriptionDelay = 0.5f;

        [Header("References")]
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private NeoSkeuoTheme normalTheme;
        [SerializeField] private NeoSkeuoTheme highContrastTheme;

        private const string ZEN_MODE_KEY = "AccessibilityZenMode";
        private const string HIGH_CONTRAST_KEY = "AccessibilityHighContrast";
        private const string REDUCED_MOTION_KEY = "AccessibilityReducedMotion";

        public bool ZenMode => zenModeEnabled;
        public bool HighContrast => highContrastEnabled;
        public bool ReducedMotion => reducedMotionEnabled;
        public NeoSkeuoTheme CurrentTheme => highContrastEnabled ? highContrastTheme : normalTheme;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSettings()
        {
            zenModeEnabled = PlayerPrefs.GetInt(ZEN_MODE_KEY, 0) == 1;
            highContrastEnabled = PlayerPrefs.GetInt(HIGH_CONTRAST_KEY, 0) == 1;
            reducedMotionEnabled = PlayerPrefs.GetInt(REDUCED_MOTION_KEY, 0) == 1;
            
            ApplySettings();
        }

        public void SetZenMode(bool enabled)
        {
            zenModeEnabled = enabled;
            PlayerPrefs.SetInt(ZEN_MODE_KEY, enabled ? 1 : 0);
            ApplySettings();
        }

        public void SetHighContrast(bool enabled)
        {
            highContrastEnabled = enabled;
            PlayerPrefs.SetInt(HIGH_CONTRAST_KEY, enabled ? 1 : 0);
            ApplySettings();
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotionEnabled = enabled;
            PlayerPrefs.SetInt(REDUCED_MOTION_KEY, enabled ? 1 : 0);
            ApplySettings();
        }

        private void ApplySettings()
        {
            // ZEN MODE: Reduce audio stimulation
            if (masterMixer != null)
            {
                // Mute background music in zen mode
                masterMixer.SetFloat("MusicVolume", zenModeEnabled ? -80f : 0f);
                // Keep voice-overs audible
                masterMixer.SetFloat("VoiceVolume", 0f);
            }

            // Broadcast to all listeners
            ZenModeChanged?.Invoke(zenModeEnabled);
            HighContrastChanged?.Invoke(highContrastEnabled);
            ReducedMotionChanged?.Invoke(reducedMotionEnabled);
        }

        // Events for systems to listen to
        public static event System.Action<bool> ZenModeChanged;
        public static event System.Action<bool> HighContrastChanged;
        public static event System.Action<bool> ReducedMotionChanged;

        /// <summary>
        /// Should particles be disabled in current mode?
        /// </summary>
        public bool ShouldDisableParticles()
        {
            return zenModeEnabled || reducedMotionEnabled;
        }

        /// <summary>
        /// Get animation duration multiplier (0 = instant, 1 = normal).
        /// </summary>
        public float GetAnimationMultiplier()
        {
            return reducedMotionEnabled ? 0f : 1f;
        }
    }
}
