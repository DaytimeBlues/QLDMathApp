using UnityEngine;

namespace QLDMathApp.Architecture.UI
{
    /// <summary>
    /// FOREST THEME: Enchanted Forest UI theme for diverse learners.
    /// Replaces the high-tech Forest HUD with a warm, nature-based aesthetic.
    /// </summary>
    [CreateAssetMenu(fileName = "ForestTheme", menuName = "Education/Forest UI Theme")]
    public class ForestTheme : ScriptableObject
    {
        [Header("Earthy Palette (Garden Identity)")]
        [Tooltip("Deep Forest Moss / Background")]
        public Color backgroundColor = new Color(0.05f, 0.15f, 0.05f); // Deep Forest Green
        
        [Tooltip("Firefly Glow - Growth / Correctness")]
        public Color growthGreen = new Color(0.7f, 1f, 0.4f); // #B3FF66
        
        [Tooltip("Autumn Leaf - Encouragement / Soft Alert")]
        public Color softOrange = new Color(1f, 0.6f, 0.2f); // #FF9933
        
        [Tooltip("Twilight Sky - Secondary HUD")]
        public Color twilightBlue = new Color(0.2f, 0.4f, 0.8f); // #3366CC

        [Header("Typography (Nature Font)")]
        public float baseFontSize = 32f;
        public float headingFontSize = 56f;
        public Color primaryTextColor = new Color(1f, 0.95f, 0.8f); // Warm Cream text
        public Color accentTextColor = new Color(0.7f, 1f, 0.4f);  // Glow Green accent

        [Header("Enchanted Elements")]
        [Tooltip("Vine border color")]
        public Color vineBorderColor = new Color(0.4f, 0.6f, 0.2f, 0.5f);
        [Tooltip("Magic panel opacity")]
        public float panelAlpha = 0.7f;

        [Header("Garden Growth (Progression)")]
        public Color growthSproutColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
        public Color growthBloomColor = new Color(0.7f, 1f, 0.4f, 1f);

        [Header("Atmosphere")]
        public float particleGlowIntensity = 1.2f;
        public float windSwaySpeed = 0.5f;
        public AnimationCurve organicTransition = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Apply theme to a UI Image to create a wooden/leaf-style panel.
        /// </summary>
        public void ApplyPanelTheme(UnityEngine.UI.Image image)
        {
            image.color = new Color(0.1f, 0.2f, 0.1f, panelAlpha);
        }

        public Color GetCorrectColor() => growthGreen;
        public Color GetIncorrectColor() => softOrange;
    }
}
