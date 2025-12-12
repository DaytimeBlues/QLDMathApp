using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using QLDMathApp.Architecture.UI;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.UI
{
    /// <summary>
    /// MAIN MENU: Entry point with Neo-Skeuomorphic styling.
    /// - Play button (launches Hub Map)
    /// - Settings button (accessibility)
    /// - Progress indicator
    /// Voice-over driven, minimal text for pre-readers.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Image progressFill;
        [SerializeField] private CanvasGroup menuCanvasGroup;
        
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip welcomeClip; // "Welcome! Let's learn math!"
        [SerializeField] private AudioClip playButtonClip; // "Tap to play!"
        [SerializeField] private AudioClip menuMusic;
        
        [Header("Scenes")]
        [SerializeField] private string hubSceneName = "HubMapScene";
        
        [Header("Animation")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private float idleWaveInterval = 5f;

        private float _lastWaveTime;

        private void Start()
        {
            // Validate required references
            if (playButton == null || settingsButton == null)
            {
                Debug.LogError("[MainMenu] Play or Settings button not assigned!");
                return;
            }
            
            // Setup buttons
            playButton.onClick.AddListener(OnPlayPressed);
            settingsButton.onClick.AddListener(OnSettingsPressed);
            
            // Disable buttons during welcome sequence
            playButton.interactable = false;
            settingsButton.interactable = false;
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            
            // Play welcome audio
            StartCoroutine(WelcomeSequence());
            
            // Update progress display
            UpdateProgressDisplay();
            
            // Start background music
            if (menuMusic != null && audioSource != null)
            {
                audioSource.clip = menuMusic;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void Update()
        {
            // Periodic character wave to keep engagement
            if (Time.time - _lastWaveTime > idleWaveInterval)
            {
                if (characterAnimator != null)
                {
                    characterAnimator.SetTrigger("Wave");
                }
                _lastWaveTime = Time.time;
            }
        }

        private System.Collections.IEnumerator WelcomeSequence()
        {
            yield return new WaitForSeconds(0.5f);
            
            // Welcome voice
            if (welcomeClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(welcomeClip);
                yield return new WaitForSeconds(welcomeClip.length);
            }
            
            // Highlight play button
            yield return PulseButton(playButton);
            
            // "Tap to play"
            if (playButtonClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(playButtonClip);
            }
            
            // Re-enable buttons after welcome sequence
            playButton.interactable = true;
            settingsButton.interactable = true;
        }

        private System.Collections.IEnumerator PulseButton(Button button)
        {
            RectTransform rt = button.GetComponent<RectTransform>();
            Vector3 originalScale = rt.localScale;
            
            for (int i = 0; i < 2; i++)
            {
                // Scale up
                for (float t = 0; t < 0.2f; t += Time.deltaTime)
                {
                    rt.localScale = Vector3.Lerp(originalScale, originalScale * 1.1f, t / 0.2f);
                    yield return null;
                }
                
                // Scale down
                for (float t = 0; t < 0.2f; t += Time.deltaTime)
                {
                    rt.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t / 0.2f);
                    yield return null;
                }
            }
            
            rt.localScale = originalScale;
        }

        private void OnPlayPressed()
        {
            StartCoroutine(TransitionToHub());
        }

        private System.Collections.IEnumerator TransitionToHub()
        {
            // Fade out
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                menuCanvasGroup.alpha = 1f - (t / 0.5f);
                yield return null;
            }
            
            // Load hub scene
            SceneManager.LoadScene(hubSceneName);
        }

        private void OnSettingsPressed()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        private void UpdateProgressDisplay()
        {
            if (progressFill == null) return;
            
            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            int totalLevels = 10; // Assume 10 levels total
            
            progressFill.fillAmount = (float)unlockedLevel / totalLevels;
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
    }
}
