using UnityEngine;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// Plays number audio for counting and explanations.
    /// Uses Australian-accented voice recordings.
    /// </summary>
    public class NumberAudioService : MonoBehaviour
    {
        [Header("Number Clips (1-10)")]
        [SerializeField] private AudioClip[] numberClips; // Index 0 = "One", Index 9 = "Ten"
        
        [Header("Phrase Clips")]
        [SerializeField] private AudioClip letsMeCountClip;
        [SerializeField] private AudioClip wellDoneClip;
        [SerializeField] private AudioClip tryAgainClip;
        
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void PlayNumber(int number)
        {
            if (number >= 1 && number <= numberClips.Length)
            {
                _audioSource.PlayOneShot(numberClips[number - 1]);
            }
        }

        public float GetNumberClipLength(int number)
        {
            if (number >= 1 && number <= numberClips.Length && numberClips[number - 1] != null)
            {
                return numberClips[number - 1].length;
            }
            return 0.5f; // Default fallback
        }

        public void PlayLetMeCount()
        {
            if (letsMeCountClip != null)
            {
                _audioSource.PlayOneShot(letsMeCountClip);
            }
        }

        public void PlayWellDone()
        {
            if (wellDoneClip != null)
            {
                _audioSource.PlayOneShot(wellDoneClip);
            }
        }
    }
}
