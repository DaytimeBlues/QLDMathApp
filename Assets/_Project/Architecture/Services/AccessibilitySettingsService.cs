using System;
using UnityEngine;
using UnityEngine.Audio;
using QLDMathApp.Architecture.Services;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// ACCESSIBILITY SETTINGS SERVICE: Centralized, persistent settings management.
    /// AUDIT FIX: Consolidated version using PersistenceService (JSON).
    /// Provides inclusive design for diverse learners (Zen Mode, High Contrast, Reduced Motion).
    /// </summary>
    public sealed class AccessibilitySettingsService : MonoBehaviour
    {
        public static AccessibilitySettingsService Instance { get; private set; }

        [Header("References")]
        [SerializeField] private AudioMixer masterMixer;

        public bool ZenMode { get; private set; }
        public bool HighContrast { get; private set; }
        public bool ReducedMotion { get; private set; }

        /// <summary>
        /// Fired whenever any setting changes.
        /// </summary>
        public event Action Changed;

        // Legacy event support for backward compatibility
        public static event Action<bool> OnZenModeChanged;
        public static event Action<bool> OnHighContrastChanged;
        public static event Action<bool> OnReducedMotionChanged;

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
            ApplyAudioSettings();
            Changed?.Invoke();
            OnZenModeChanged?.Invoke(value);
        }

        public void SetHighContrast(bool value)
        {
            if (HighContrast == value) return;
            HighContrast = value;
            Save();
            Changed?.Invoke();
            OnHighContrastChanged?.Invoke(value);
        }

        public void SetReducedMotion(bool value)
        {
            if (ReducedMotion == value) return;
            ReducedMotion = value;
            Save();
            Changed?.Invoke();
            OnReducedMotionChanged?.Invoke(value);
        }

        private void Load()
        {
            if (PersistenceService.Instance == null)
            {
                Debug.LogWarning("[AccessibilityService] PersistenceService not ready, using defaults.");
                return;
            }

            var data = PersistenceService.Instance.Load<AppUserData>();
            ZenMode = data.ZenMode;
            HighContrast = data.HighContrast;
            ReducedMotion = data.ReducedMotion;
            
            ApplyAudioSettings();
        }

        private void Save()
        {
            if (PersistenceService.Instance == null) return;
            
            var data = PersistenceService.Instance.Load<AppUserData>();
            data.ZenMode = ZenMode;
            data.HighContrast = HighContrast;
            data.ReducedMotion = ReducedMotion;
            
            PersistenceService.Instance.Save(data);
        }

        private void ApplyAudioSettings()
        {
            if (masterMixer != null)
            {
                // ZEN MODE: Reduce audio stimulation (mute music, keep voices)
                masterMixer.SetFloat("MusicVolume", ZenMode ? -80f : 0f);
                masterMixer.SetFloat("VoiceVolume", 0f);
            }
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
