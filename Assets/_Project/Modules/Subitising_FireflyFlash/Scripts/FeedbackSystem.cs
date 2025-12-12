using UnityEngine;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// MULTI-SENSORY FEEDBACK SYSTEM (Observer Pattern)
    /// Listens to EventBus and triggers:
    /// - Audio (chimes, voice)
    /// - Visual (particles, screen effects)
    /// - Haptic (vibration)
    /// 
    /// Completely decoupled from game logic.
    /// </summary>
    public class FeedbackSystem : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successChime;
        [SerializeField] private AudioClip encouragementChime; // Gentle, not punishing
        
        [Header("Particles")]
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private ParticleSystem sparkleParticles;
        
        [Header("Screen Effects")]
        [SerializeField] private CanvasGroup screenFlashGroup;
        [SerializeField] private Animator characterAnimator; // The "guide" character
        
        [Header("Settings")]
        [SerializeField] private bool enableHaptics = true;
        
        private void OnEnable()
        {
            EventBus.OnPlaySuccessFeedback += PlaySuccessFeedback;
            EventBus.OnPlayCorrectionFeedback += PlayCorrectionFeedback;
        }

        private void OnDisable()
        {
            EventBus.OnPlaySuccessFeedback -= PlaySuccessFeedback;
            EventBus.OnPlayCorrectionFeedback -= PlayCorrectionFeedback;
        }

        /// <summary>
        /// CORRECT ANSWER: Celebration!
        /// </summary>
        private void PlaySuccessFeedback()
        {
            // AUDIO
            if (successChime != null)
            {
                audioSource.PlayOneShot(successChime);
            }
            
            // VISUAL: Confetti burst
            if (confettiParticles != null)
            {
                confettiParticles.Play();
            }
            
            // CHARACTER: Happy animation
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Celebrate");
            }
            
            // HAPTIC: Double pulse
            if (enableHaptics)
            {
                TriggerHaptic(HapticType.Success);
            }
            
            // SCREEN: Brief green tint
            StartCoroutine(FlashScreen(new Color(0.3f, 1f, 0.3f, 0.2f), 0.2f));
        }

        /// <summary>
        /// INCORRECT ANSWER: Gentle encouragement (NOT punishment).
        /// This is the "learning moment" - never shame the child.
        /// </summary>
        private void PlayCorrectionFeedback()
        {
            // AUDIO: Gentle "hmm" sound, not a buzzer
            if (encouragementChime != null)
            {
                audioSource.PlayOneShot(encouragementChime);
            }
            
            // VISUAL: Soft sparkles (still positive)
            if (sparkleParticles != null)
            {
                sparkleParticles.Play();
            }
            
            // CHARACTER: "Thinking" pose (curious, not disappointed)
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Think");
            }
            
            // HAPTIC: Single gentle tap
            if (enableHaptics)
            {
                TriggerHaptic(HapticType.Light);
            }
            
            // No harsh screen flash - keep it calm
        }

        private System.Collections.IEnumerator FlashScreen(Color color, float duration)
        {
            if (screenFlashGroup == null) yield break;
            
            // Flash in
            screenFlashGroup.GetComponent<UnityEngine.UI.Image>().color = color;
            screenFlashGroup.alpha = 1f;
            
            // Fade out
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                screenFlashGroup.alpha = 1f - (t / duration);
                yield return null;
            }
            
            screenFlashGroup.alpha = 0f;
        }

        private void TriggerHaptic(HapticType type)
        {
            #if UNITY_ANDROID || UNITY_IOS
            switch (type)
            {
                case HapticType.Success:
                    // Double vibration would need a native plugin
                    Handheld.Vibrate();
                    break;
                case HapticType.Light:
                    // Light tap - needs native plugin for fine control
                    break;
            }
            #endif
        }

        private enum HapticType
        {
            Light,
            Success
        }
    }
}
