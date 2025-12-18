using UnityEngine;

namespace QLDMathApp.Architecture.UI
{
    /// <summary>
    /// NERV THEME: Evangelion-inspired high-tech UI theme.
    /// Replaces the friendly Neo-Skeuomorphic look with a technical HUD aesthetic.
    /// </summary>
    [CreateAssetMenu(fileName = "NERVTheme", menuName = "Education/NERV UI Theme")]
    public class NERVTheme : ScriptableObject
    {
        [Header("Primary Colors (NERV Identity)")]
        [Tooltip("NERV Dark Purple / Black")]
        public Color backgroundColor = new Color(0.1f, 0.05f, 0.15f); // Dark Purple/Black
        
        [Tooltip("Neon Green - Sync / Correctness")]
        public Color syncGreen = new Color(0.63f, 1f, 0f); // #A1FF00
        
        [Tooltip("Alert Red - Danger / Incorrect")]
        public Color alertRed = new Color(0.7f, 0f, 0f); // #B20000
        
        [Tooltip("Emergency Orange - Warning / HUD")]
        public Color emergencyOrange = new Color(1f, 0.55f, 0f); // #FF8C00

        [Header("Typography (High-Tech)")]
        public float baseFontSize = 32f;
        public float headingFontSize = 56f;
        public Color terminalTextColor = new Color(0.63f, 1f, 0f); // Neon Green text
        public Color headerTextColor = new Color(1f, 0.55f, 0f);    // Orange header

        [Header("HUD Elements")]
        [Tooltip("Hexagonal border color")]
        public Color hexBorderColor = new Color(0.63f, 1f, 0f, 0.5f);
        [Tooltip("Panel opacity")]
        public float panelAlpha = 0.8f;

        [Header("Sync Ratio (Progression)")]
        public Color syncLowColor = new Color(1f, 1f, 1f, 0.3f);
        public Color syncHighColor = new Color(0.63f, 1f, 0f, 1f);

        [Header("Animation (Digital)")]
        public float glitchDuration = 0.15f;
        public float scanlineSpeed = 1.0f;
        public AnimationCurve digitalTransition = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// Apply theme to a UI Image to create NERV-style hexagonal button look.
        /// </summary>
        public void ApplyButtonTheme(UnityEngine.UI.Image image)
        {
            image.color = new Color(0.1f, 0.1f, 0.1f, panelAlpha);
            // Hexagonal masking or sprite assignment would happen here in a real Unity project
        }

        public Color GetCorrectColor() => syncGreen;
        public Color GetIncorrectColor() => alertRed;
    }
}
