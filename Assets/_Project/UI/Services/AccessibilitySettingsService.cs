using System;
using UnityEngine;

namespace QLDMathApp.UI.Services
{
    /// <summary>
    /// ACCESSIBILITY SETTINGS SERVICE: Centralized, persistent settings management.
    /// Replaces fragile singleton assumptions with a proper service pattern.
    /// Persists to PlayerPrefs and broadcasts changes via event.
    /// </summary>
    public sealed class AccessibilitySettingsService : MonoBehaviour
    {
        public static AccessibilitySettingsService Instance { get; private set; }

        public bool ZenMode { get; private set; }
        public bool HighContrast { get; private set; }
        public bool ReducedMotion { get; private set; }

        /// <summary>
        /// Fired whenever any setting changes.
        /// </summary>
        public event Action Changed;

        private const string KeyZen = "acc_zen";
        private const string KeyContrast = "acc_contrast";
        private const string KeyMotion = "acc_motion";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        public void SetZenMode(bool value)
        {
            if (ZenMode == value) return;
            ZenMode = value;
            Save();
            Changed?.Invoke();
        }

        public void SetHighContrast(bool value)
        {
            if (HighContrast == value) return;
            HighContrast = value;
            Save();
            Changed?.Invoke();
        }

        public void SetReducedMotion(bool value)
        {
            if (ReducedMotion == value) return;
            ReducedMotion = value;
            Save();
            Changed?.Invoke();
        }

        private void Load()
        {
            ZenMode = PlayerPrefs.GetInt(KeyZen, 0) == 1;
            HighContrast = PlayerPrefs.GetInt(KeyContrast, 0) == 1;
            ReducedMotion = PlayerPrefs.GetInt(KeyMotion, 0) == 1;
        }

        private void Save()
        {
            PlayerPrefs.SetInt(KeyZen, ZenMode ? 1 : 0);
            PlayerPrefs.SetInt(KeyContrast, HighContrast ? 1 : 0);
            PlayerPrefs.SetInt(KeyMotion, ReducedMotion ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Should particles be disabled in current mode?
        /// </summary>
        public bool ShouldDisableParticles()
        {
            return ZenMode || ReducedMotion;
        }

        /// <summary>
        /// Get animation duration multiplier (0 = instant, 1 = normal).
        /// </summary>
        public float GetAnimationMultiplier()
        {
            return ReducedMotion ? 0f : 1f;
        }
    }
}
