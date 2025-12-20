using UnityEngine;
using System.Collections;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Modules.Subitising;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace QLDMathApp.Modules.Subitising
{
    public class FireflyGameLoop : MonoBehaviour 
    {
        [Header("Forest References")]
        [SerializeField] private FireflySpawner fireflySpawner;
        [SerializeField] private CanvasGroup guessCanvas;
        [SerializeField] private Button[] numberButtons;
        [SerializeField] private Button missionStartButton;
        
        [Header("Timing (Adaptive Mastery)")]
        [SerializeField] private float baseScanTime = 1.2f;
        [SerializeField] private float minScanTime = 0.6f;
        [SerializeField] private float maxScanTime = 2.0f;
        
        private float _currentScanTime;
        private int _totalRounds = 5;
        private int _currentRound = 0;

        private void Start()
        {
            _currentScanTime = baseScanTime;
            
            if (guessCanvas != null)
            {
                guessCanvas.alpha = 0;
                guessCanvas.interactable = false;
            }
            
            if (missionStartButton != null)
            {
                missionStartButton.gameObject.SetActive(true);
                missionStartButton.onClick.AddListener(StartMission);
            }
            
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int val = i + 1;
                numberButtons[i].onClick.AddListener(() => OnNumberSelected(val));
            }
        }

        private void OnEnable()
        {
            EventBus.OnGrowthProgressChanged += HandleGrowthChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGrowthProgressChanged -= HandleGrowthChanged;
        }

        private void HandleGrowthChanged(float growth)
        {
            // Adaptive difficulty: Higher progress = shorter flash time
            _currentScanTime = Mathf.Lerp(maxScanTime, minScanTime, growth);
        }

        public void StartMission()
        {
            StartCoroutine(GameRoutine());
        }

        private IEnumerator GameRoutine() 
        {
            _currentRound++;
            if (missionStartButton != null) missionStartButton.gameObject.SetActive(false);
            if (guessCanvas != null) { guessCanvas.alpha = 0; guessCanvas.interactable = false; }

            int targetFireflyCount = Random.Range(1, 6);
            Debug.Log($"[Forest] Can you spot {targetFireflyCount} fireflies?");
            
            fireflySpawner.SpawnFireflies(targetFireflyCount);
            yield return new WaitForSeconds(_currentScanTime); 
            fireflySpawner.HideFireflies();
            
            if (guessCanvas != null)
            {
                guessCanvas.alpha = 1;
                guessCanvas.interactable = true;
            }
        }

        private void OnNumberSelected(int guess)
        {
            if (guessCanvas != null) guessCanvas.interactable = false;

            if (guess == fireflySpawner.CurrentCount)
            {
                // Success
                EventBus.OnAnswerAttempted?.Invoke(true, 1000f);
                EventBus.OnGuideSpoke?.Invoke(GuidePersonality.KindBunny, "Well done! You counted them all!");
                
                // Update growth on success
                float progress = Mathf.Clamp01((_currentRound / (float)_totalRounds));
                EventBus.OnGrowthProgressChanged?.Invoke(progress);
                
                StartCoroutine(SuccessDelay());
            }
            else
            {
                // Correction
                EventBus.OnAnswerAttempted?.Invoke(false, 1000f);
                EventBus.OnGuideSpoke?.Invoke(GuidePersonality.CuriousCat, "Let's count them together!");
                
                // Show current growth progress (replacing SyncRate)
                EventBus.OnGrowthProgressChanged?.Invoke(Mathf.Max(0f, (_currentRound / (float)_totalRounds) - 0.2f));
                
                StartCoroutine(ScaffoldingSequence());
            }
        }

        private IEnumerator SuccessDelay()
        {
            yield return new WaitForSeconds(2f);
            if (_currentRound < _totalRounds) StartMission();
            else SceneManager.LoadScene("HubMap");
        }

        private IEnumerator ScaffoldingSequence()
        {
            fireflySpawner.ShowFireflies();
            yield return fireflySpawner.AnimateCountingSequence();
            yield return new WaitForSeconds(1f);
            StartMission();
        }
    }
}
