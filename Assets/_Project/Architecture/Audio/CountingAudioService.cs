using UnityEngine;
using System.Collections;
using QLDMathApp.Architecture.Audio;

namespace QLDMathApp.Architecture.Audio
{
    /// <summary>
    /// COUNTING AUDIO: Orchestrates counting sequences with proper pacing.
    /// "One... Two... Three!" with natural rhythm.
    /// Used for explanatory feedback when child gets answer wrong.
    /// </summary>
    public class CountingAudioService : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float pauseBetweenNumbers = 0.6f;
        [SerializeField] private float emphasisOnLast = 0.3f; // Extra pause before final number
        
        [Header("References")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] numberClips; // Index 0 = "One"

        private Coroutine _countingCoroutine;

        /// <summary>
        /// Count from 1 to target with audio.
        /// </summary>
        public void CountTo(int target, System.Action onComplete = null)
        {
            if (_countingCoroutine != null)
            {
                StopCoroutine(_countingCoroutine);
            }
            _countingCoroutine = StartCoroutine(CountSequence(target, onComplete));
        }

        /// <summary>
        /// Count with visual callbacks for each number (e.g., to pulse fireflies).
        /// </summary>
        public void CountToWithCallbacks(int target, System.Action<int> onEachNumber, System.Action onComplete = null)
        {
            if (_countingCoroutine != null)
            {
                StopCoroutine(_countingCoroutine);
            }
            _countingCoroutine = StartCoroutine(CountSequenceWithCallbacks(target, onEachNumber, onComplete));
        }

        private IEnumerator CountSequence(int target, System.Action onComplete)
        {
            for (int i = 1; i <= target; i++)
            {
                // Extra pause before last number for emphasis
                if (i == target)
                {
                    yield return new WaitForSeconds(emphasisOnLast);
                }

                // Play number audio
                PlayNumber(i);
                
                // Wait for audio + pause
                float clipLength = GetClipLength(i);
                yield return new WaitForSeconds(clipLength + pauseBetweenNumbers);
            }

            _countingCoroutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator CountSequenceWithCallbacks(int target, System.Action<int> onEachNumber, System.Action onComplete)
        {
            for (int i = 1; i <= target; i++)
            {
                if (i == target)
                {
                    yield return new WaitForSeconds(emphasisOnLast);
                }

                // Trigger callback (e.g., pulse firefly)
                onEachNumber?.Invoke(i);
                
                // Play number audio
                PlayNumber(i);
                
                float clipLength = GetClipLength(i);
                yield return new WaitForSeconds(clipLength + pauseBetweenNumbers);
            }

            _countingCoroutine = null;
            onComplete?.Invoke();
        }

        private void PlayNumber(int number)
        {
            // Try TTS first
            if (TTSService.Instance != null && TTSService.Instance.enabled)
            {
                TTSService.Instance.SpeakNumber(number);
            }
            // Fallback to pre-recorded
            else if (number >= 1 && number <= numberClips.Length && numberClips[number - 1] != null)
            {
                audioSource.PlayOneShot(numberClips[number - 1]);
            }
            else
            {
                Debug.Log($"[CountingAudio] {number}");
            }
        }

        private float GetClipLength(int number)
        {
            if (number >= 1 && number <= numberClips.Length && numberClips[number - 1] != null)
            {
                return numberClips[number - 1].length;
            }
            return 0.5f; // Default estimate
        }

        public void StopCounting()
        {
            if (_countingCoroutine != null)
            {
                StopCoroutine(_countingCoroutine);
                _countingCoroutine = null;
            }
        }
    }
}
