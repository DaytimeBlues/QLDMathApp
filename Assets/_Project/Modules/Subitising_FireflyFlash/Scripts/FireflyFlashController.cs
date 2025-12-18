using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Managers;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// ANGEL INTERCEPT: Core subitising game controller (re-themed).
    /// Implements:
    /// - Timed stimulus display (Angel silhouettes)
    /// - Sync Ratio adaptation (based on response time/correctness)
    /// - Explanatory feedback via MAGI analysis
    /// </summary>
    public class FireflyFlashController : MonoBehaviour
    {
        [Header("NERV Theme")]
        [SerializeField] private NERVTheme theme;
        
        [Header("References")]
        [SerializeField] private FireflySpawner angelSpawner;
        [SerializeField] private AnswerButtonGroup answerButtons;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private CanvasGroup interceptionFieldGroup; 
        
        [Header("Timing (Sync Adaptation)")]
        [SerializeField] private float baseScanTime = 1.2f;
        [SerializeField] private float minScanTime = 0.6f;
        [SerializeField] private float maxScanTime = 2.0f;
        
        private float _currentScanTime;
        private MathProblemSO _currentProblem;
        private bool _isWaitingForAnswer;
        private float _roundStartTime;
        private float _currentSyncRatio = 40.0f; // Start at 40%

        private void Start()
        {
            _currentScanTime = baseScanTime;
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncRatio / 100f);
        }

        private void OnEnable()
        {
            EventBus.OnInterventionTriggered += HandleIntervention;
        }

        private void OnDisable()
        {
            EventBus.OnInterventionTriggered -= HandleIntervention;
        }

        /// <summary>
        /// Called by GameManager to start an Intercept round.
        /// </summary>
        public void StartRound(MathProblemSO problem)
        {
            _currentProblem = problem;
            StartCoroutine(InterceptSequence());
        }

        private IEnumerator InterceptSequence()
        {
            // 1. SCAN PHASE: MAGI Warning
            Debug.Log("[MAGI] PATTERN BLUE DETECTED.");
            if (_currentProblem.questionAudio != null)
            {
                audioSource.PlayOneShot(_currentProblem.questionAudio);
                yield return new WaitForSeconds(_currentProblem.questionAudio.length + 0.3f);
            }

            // 2. VISUALIZATION: Show Angels
            angelSpawner.SpawnFireflies(_currentProblem.correctValue);
            interceptionFieldGroup.alpha = 1f;
            
            // Wait for scan time
            yield return new WaitForSeconds(_currentScanTime);
            
            // 3. CLOAK: Angels disappear
            angelSpawner.HideFireflies();
            interceptionFieldGroup.alpha = 0.2f; // Field interference
            
            // 4. COUNTERMEASURE PHASE: Enable answer buttons
            _roundStartTime = Time.time;
            _isWaitingForAnswer = true;
            
            answerButtons.SetupButtons(
                _currentProblem.correctValue,
                _currentProblem.distractorValues.ToArray()
            );
            answerButtons.EnableButtons(true);
        }

        public void OnAnswerSelected(int selectedValue)
        {
            if (!_isWaitingForAnswer) return;
            _isWaitingForAnswer = false;
            
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            bool isCorrect = (selectedValue == _currentProblem.correctValue);
            
            answerButtons.EnableButtons(false);
            
            EventBus.OnAnswerAttempted?.Invoke(isCorrect, responseTime);
            
            if (isCorrect)
            {
                StartCoroutine(SuccessSequence());
            }
            else
            {
                StartCoroutine(FailureSequence());
            }
        }

        private IEnumerator SuccessSequence()
        {
            // SYNC RATIO INCREASE
            _currentSyncRatio = Mathf.Min(100f, _currentSyncRatio + 15.0f);
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncRatio / 100f);
            
            EventBus.OnPlaySuccessFeedback?.Invoke();
            
            // Adaptive: Speed up scan
            _currentScanTime = Mathf.Max(minScanTime, _currentScanTime - 0.1f);
            
            yield return new WaitForSeconds(1.5f);
            Debug.Log("[AngelIntercept] Target Neutralized.");
        }

        private IEnumerator FailureSequence()
        {
            // SYNC RATIO DROP
            _currentSyncRatio = Mathf.Max(0f, _currentSyncRatio - 20.0f);
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncRatio / 100f);

            EventBus.OnPlayCorrectionFeedback?.Invoke();
            
            // MAGI ANALYSIS (Re-show frozen)
            angelSpawner.ShowFireflies();
            interceptionFieldGroup.alpha = 1f;
            
            if (_currentProblem.explanationAudio != null)
            {
                audioSource.PlayOneShot(_currentProblem.explanationAudio);
            }
            
            yield return angelSpawner.AnimateCountingSequence();
            
            // Adaptive: Slow down scan
            _currentScanTime = Mathf.Min(maxScanTime, _currentScanTime + 0.2f);
            
            yield return new WaitForSeconds(1.0f);
            Debug.Log("[AngelIntercept] MAGI Analysis complete. Scaffolding deployed.");
        }

        private void HandleIntervention(InterventionType type)
        {
            switch (type)
            {
                case InterventionType.LevelUp:
                    _currentScanTime = Mathf.Max(minScanTime, _currentScanTime - 0.3f);
                    break;
                    
                case InterventionType.ScaffoldDown:
                case InterventionType.ShowDemo:
                    _currentScanTime = maxScanTime;
                    break;
            }
        }
    }
}
