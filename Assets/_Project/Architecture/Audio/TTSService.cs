using UnityEngine;
using System;
using System.Collections;

namespace QLDMathApp.Architecture.Audio
{
    /// <summary>
    /// TTS SERVICE: Text-to-Speech wrapper for voice-over generation.
    /// Uses platform-native TTS or cloud API.
    /// Falls back to pre-recorded clips if TTS unavailable.
    /// 
    /// INTEGRATION OPTIONS:
    /// 1. Android: TextToSpeech API (native)
    /// 2. iOS: AVSpeechSynthesizer (native)
    /// 3. Cloud: Google Cloud TTS, Amazon Polly, Azure Speech
    /// </summary>
    public class TTSService : MonoBehaviour
    {
        public static TTSService Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool usePlatformTTS = true;
        [SerializeField] private float speechRate = 0.8f; // Slower for children
        [SerializeField] private float pitch = 1.1f; // Slightly higher for friendly tone
        [SerializeField] private string voiceLocale = "en-AU"; // Australian English

        [Header("Fallback Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] numberClips; // Index 0 = "One", Index 9 = "Ten"
        [SerializeField] private AudioClip[] phraseClips; // Common phrases

        private bool _isSpeaking;
        private Action _onComplete;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _ttsObject;
#endif

        public bool IsSpeaking => _isSpeaking;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTTS();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeTTS()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _ttsObject = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TTSInitListener(this));
                Debug.Log("[TTS] Android TTS initialized");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[TTS] Failed to initialize Android TTS: " + e.Message);
                usePlatformTTS = false;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS uses native plugin - would need Objective-C bridge
            Debug.Log("[TTS] iOS TTS - requires native plugin");
            usePlatformTTS = false;
#else
            Debug.Log("[TTS] Platform TTS not available - using fallback audio");
            usePlatformTTS = false;
#endif
        }

        /// <summary>
        /// Speak a number (1-10).
        /// </summary>
        public void SpeakNumber(int number, Action onComplete = null)
        {
            if (number < 1 || number > 10)
            {
                Debug.LogWarning($"[TTS] Number out of range: {number}");
                onComplete?.Invoke();
                return;
            }

            string text = GetNumberWord(number);
            Speak(text, onComplete, numberClips.Length >= number ? numberClips[number - 1] : null);
        }

        /// <summary>
        /// Speak a phrase.
        /// </summary>
        public void SpeakPhrase(string phrase, Action onComplete = null)
        {
            Speak(phrase, onComplete, null);
        }

        /// <summary>
        /// Speak text with optional fallback clip.
        /// </summary>
        public void Speak(string text, Action onComplete = null, AudioClip fallbackClip = null)
        {
            _onComplete = onComplete;
            _isSpeaking = true;

            if (usePlatformTTS && TryPlatformTTS(text))
            {
                // Platform TTS handling
                StartCoroutine(WaitForTTSComplete(text.Length * 0.1f)); // Estimate duration
            }
            else if (fallbackClip != null)
            {
                // Use pre-recorded audio
                audioSource.PlayOneShot(fallbackClip);
                StartCoroutine(WaitForAudioComplete(fallbackClip.length));
            }
            else
            {
                // No audio available
                Debug.Log($"[TTS] Would speak: \"{text}\"");
                _isSpeaking = false;
                _onComplete?.Invoke();
            }
        }

        private bool TryPlatformTTS(string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_ttsObject != null)
            {
                _ttsObject.Call<int>("speak", text, 0, null, null);
                return true;
            }
#endif
            return false;
        }

        private IEnumerator WaitForTTSComplete(float estimatedDuration)
        {
            yield return new WaitForSeconds(estimatedDuration);
            _isSpeaking = false;
            _onComplete?.Invoke();
        }

        private IEnumerator WaitForAudioComplete(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isSpeaking = false;
            _onComplete?.Invoke();
        }

        private string GetNumberWord(int number)
        {
            string[] words = { "One", "Two", "Three", "Four", "Five", 
                              "Six", "Seven", "Eight", "Nine", "Ten" };
            return words[number - 1];
        }

        /// <summary>
        /// Common phrases for the app.
        /// </summary>
        public void SpeakWelcome() => SpeakPhrase("Welcome! Let's learn math together!");
        public void SpeakCorrect() => SpeakPhrase("Great job! That's right!");
        public void SpeakTryAgain() => SpeakPhrase("Let's try again. You can do it!");
        public void SpeakCountWithMe() => SpeakPhrase("Let's count together.");
        public void SpeakHowMany() => SpeakPhrase("How many can you see?");
        public void SpeakWhatComesNext() => SpeakPhrase("What comes next in the pattern?");

        private void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_ttsObject != null)
            {
                _ttsObject.Call("shutdown");
                _ttsObject.Dispose();
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android TTS Init Listener (nested class)
        private class TTSInitListener : AndroidJavaProxy
        {
            private TTSService _service;
            
            public TTSInitListener(TTSService service) 
                : base("android.speech.tts.TextToSpeech$OnInitListener")
            {
                _service = service;
            }
            
            public void onInit(int status)
            {
                if (status == 0) // SUCCESS
                {
                    // Set locale to Australian English
                    AndroidJavaObject locale = new AndroidJavaObject("java.util.Locale", "en", "AU");
                    _service._ttsObject.Call<int>("setLanguage", locale);
                    _service._ttsObject.Call<int>("setSpeechRate", _service.speechRate);
                    _service._ttsObject.Call<int>("setPitch", _service.pitch);
                }
            }
        }
#endif
    }
}
