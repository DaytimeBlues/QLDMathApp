using UnityEngine;

namespace QLDMathApp.Architecture.UI
{
    /// <summary>
    /// NEO-SKEUOMORPHIC THEME: Centralized styling for Claymorphism look.
    /// All UI components reference this for consistent aesthetics.
    /// </summary>
    [CreateAssetMenu(fileName = "NeoSkeuoTheme", menuName = "Education/UI Theme")]
    public class NeoSkeuoTheme : ScriptableObject
    {
        [Header("Primary Colors")]
        [Tooltip("Warm cream - main background")]
        public Color backgroundColor = new Color(0.98f, 0.96f, 0.92f); // #FAF5EB
        
        [Tooltip("Soft clay - button surfaces")]
        public Color buttonColor = new Color(0.95f, 0.85f, 0.75f); // #F2D9BF
        
        [Tooltip("Deep brown - shadows")]
        public Color shadowColor = new Color(0.6f, 0.5f, 0.4f); // #998066
        
        [Tooltip("Warm highlight - top edges")]
        public Color highlightColor = new Color(1f, 0.98f, 0.95f); // #FFFAF2

        [Header("Accent Colors")]
        public Color successColor = new Color(0.6f, 0.85f, 0.6f);  // Soft green
        public Color warningColor = new Color(1f, 0.85f, 0.5f);    // Soft yellow
        public Color activeColor = new Color(0.5f, 0.75f, 0.95f);  // Soft blue

        [Header("Typography")]
        [Tooltip("Large touch targets require large text")]
        public float baseFontSize = 32f;
        public float headingFontSize = 48f;
        public Color textColor = new Color(0.25f, 0.2f, 0.15f); // Dark brown
        
        [Header("Shadows (Neumorphism)")]
        [Tooltip("Offset for drop shadow")]
        public Vector2 shadowOffset = new Vector2(4f, -4f);
        [Tooltip("Blur radius")]
        public float shadowBlur = 8f;
        [Tooltip("Offset for inner highlight")]
        public Vector2 highlightOffset = new Vector2(-2f, 2f);

        [Header("Button States")]
        public float buttonPressDepth = 4f; // Pixels to move down
        public float buttonScaleOnHover = 1.05f;
        public float buttonAnimationSpeed = 0.1f;

        [Header("Animation")]
        public float feedbackDuration = 0.3f;
        public AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Apply theme to a UI Image to create neo-skeuomorphic button look.
        /// </summary>
        public void ApplyButtonStyle(UnityEngine.UI.Image image)
        {
            image.color = buttonColor;
            // Note: Actual shadow/highlight requires shader or multiple images
        }

        /// <summary>
        /// Get pressed state color (darker).
        /// </summary>
        public Color GetPressedColor()
        {
            return Color.Lerp(buttonColor, shadowColor, 0.3f);
        }
    }
}
