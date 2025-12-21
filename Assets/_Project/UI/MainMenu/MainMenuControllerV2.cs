using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.UI.Events;
using QLDMathApp.UI.Services;
using QLDMathApp.Architecture.Services;

namespace QLDMathApp.UI
{
    /// <summary>
    /// MAIN MENU V2: Decoupled, skip-safe, uses event channels.
    /// - No direct reference to SettingsPanel
    /// - Uses SceneTransitioner for async loads
    /// - Respects ReducedMotion setting
    /// </summary>
    public class MainMenuControllerV2 : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Image progressFill;

        [Header("Settings Events")]
        [SerializeField] private BoolEventChannelSO settingsVisibilityRequested; // true=open, false=close

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip welcomeClip;
        [SerializeField] private AudioClip playButtonClip;
        [SerializeField] private AudioClip menuMusic;

        [Header("Scenes")]
        [SerializeField] private string hubSceneName = "HubMapScene";

        [Header("Animation")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private float idleWaveInterval = 5f;

        private float _lastWaveTime;
        private Coroutine _welcomeRoutine;

        private void Awake()
        {
            // Fail-fast: makes setup errors obvious in Editor.
            if (playButton == null) Debug.LogError("[MainMenuV2] playButton not assigned.", this);
            if (settingsButton == null) Debug.LogError("[MainMenuV2] settingsButton not assigned.", this);
        }

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsPressed);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayPressed);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsPressed);
        }

        private void Start()
        {
            UpdateProgressDisplay();
            StartMusic();
            _welcomeRoutine = StartCoroutine(WelcomeSequence());
        }

        private void Update()
        {
            if (Time.time - _lastWaveTime > idleWaveInterval)
            {
                if (characterAnimator != null &&
                    AccessibilitySettingsService.Instance != null &&
                    !AccessibilitySettingsService.Instance.ReducedMotion)
                {
                    characterAnimator.SetTrigger("Wave");
                }

                _lastWaveTime = Time.time;
            }
        }

        private void StartMusic()
        {
            if (audioSource == null || menuMusic == null) return;
            
            // Check Zen Mode - no music if enabled
            if (AccessibilitySettingsService.Instance != null && 
                AccessibilitySettingsService.Instance.ZenMode)
            {
                return;
            }
            
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        private IEnumerator WelcomeSequence()
        {
            yield return new WaitForSeconds(0.5f);

            if (audioSource != null && welcomeClip != null)
            {
                audioSource.PlayOneShot(welcomeClip);
                yield return new WaitForSeconds(welcomeClip.length);
            }

            // Don't pulse if reduced motion is on.
            bool shouldAnimate = AccessibilitySettingsService.Instance == null || 
                                 !AccessibilitySettingsService.Instance.ReducedMotion;
            
            if (shouldAnimate && playButton != null)
            {
                yield return PulseButton(playButton);
            }

            if (audioSource != null && playButtonClip != null)
            {
                audioSource.PlayOneShot(playButtonClip);
            }
        }

        private IEnumerator PulseButton(Button button)
        {
            if (button == null) yield break;

            var rt = button.GetComponent<RectTransform>();
            if (rt == null) yield break;

            Vector3 original = rt.localScale;
            Vector3 target = original * 1.1f;

            for (int i = 0; i < 2; i++)
            {
                yield return Scale(rt, original, target, 0.15f);
                yield return Scale(rt, target, original, 0.15f);
            }

            rt.localScale = original;
        }

        private IEnumerator Scale(RectTransform rt, Vector3 a, Vector3 b, float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                rt.localScale = Vector3.Lerp(a, b, t / seconds);
                yield return null;
            }
            rt.localScale = b;
        }

        private void OnPlayPressed()
        {
            // Skip any intro sequence and stop VO so transitions feel crisp.
            if (_welcomeRoutine != null) StopCoroutine(_welcomeRoutine);
            if (audioSource != null) audioSource.Stop();

            // Use async scene transitioner if available, fallback to sync
            if (SceneTransitioner.Instance != null)
            {
                SceneTransitioner.Instance.LoadScene(hubSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(hubSceneName);
            }
        }

        private void OnSettingsPressed()
        {
            if (settingsVisibilityRequested != null)
            {
                settingsVisibilityRequested.Raise(true);
            }
        }

        private void UpdateProgressDisplay()
        {
            if (progressFill == null) return;

            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            int totalLevels = 10; // TODO: Replace with ScriptableObject config
            progressFill.fillAmount = Mathf.Clamp01((float)unlockedLevel / totalLevels);
        }
    }
}
