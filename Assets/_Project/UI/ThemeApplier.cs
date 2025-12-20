using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.UI.Services;

namespace QLDMathApp.UI
{
    /// <summary>
    /// THEME APPLIER: Reacts to accessibility setting changes.
    /// Attach to any UI element that should respond to High Contrast/Zen/Reduced Motion.
    /// Automatically subscribes to AccessibilitySettingsService.Changed.
    /// </summary>
    public class ThemeApplier : MonoBehaviour
    {
        [Header("Target Graphics")]
        [SerializeField] private Graphic[] normalGraphics;
        [SerializeField] private Graphic[] importantGraphics;

        [Header("Normal Theme")]
        [SerializeField] private Color normalBackground = new Color(0.95f, 0.9f, 0.85f);
        [SerializeField] private Color normalForeground = new Color(0.2f, 0.15f, 0.1f);
        [SerializeField] private Color normalAccent = new Color(0.4f, 0.7f, 0.4f);

        [Header("High Contrast Theme")]
        [SerializeField] private Color highContrastBackground = Color.black;
        [SerializeField] private Color highContrastForeground = Color.white;
        [SerializeField] private Color highContrastAccent = Color.yellow;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private void OnEnable()
        {
            if (AccessibilitySettingsService.Instance != null)
            {
                AccessibilitySettingsService.Instance.Changed += ApplyTheme;
            }

            ApplyTheme();
        }

        private void OnDisable()
        {
            if (AccessibilitySettingsService.Instance != null)
            {
                AccessibilitySettingsService.Instance.Changed -= ApplyTheme;
            }
        }

        private void Start()
        {
            ApplyTheme();
        }

        /// <summary>
        /// Apply current accessibility settings to this element.
        /// </summary>
        public void ApplyTheme()
        {
            var settings = AccessibilitySettingsService.Instance;
            if (settings == null) return;

            bool highContrast = settings.HighContrast;
            bool reducedMotion = settings.ReducedMotion;

            // Apply colors
            Color bg = highContrast ? highContrastBackground : normalBackground;
            Color fg = highContrast ? highContrastForeground : normalForeground;
            Color accent = highContrast ? highContrastAccent : normalAccent;

            foreach (var graphic in normalGraphics)
            {
                if (graphic != null)
                {
                    graphic.color = bg;
                }
            }

            foreach (var graphic in importantGraphics)
            {
                if (graphic != null)
                {
                    graphic.color = accent;
                }
            }

            // Handle reduced motion
            if (animator != null)
            {
                animator.speed = reducedMotion ? 0f : 1f;
            }
        }

        /// <summary>
        /// Get whether animations should play.
        /// </summary>
        public static bool ShouldAnimate()
        {
            return AccessibilitySettingsService.Instance == null || 
                   !AccessibilitySettingsService.Instance.ReducedMotion;
        }

        /// <summary>
        /// Get animation speed multiplier (0 or 1).
        /// </summary>
        public static float AnimationSpeed()
        {
            return ShouldAnimate() ? 1f : 0f;
        }
    }
}
