using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.Serialization;
using QLDMathApp.Architecture.UI;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Managers;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// ANGEL INTERCEPT CONTROLLER: Core logic for NERV subitising missions.
    /// Manages Angel visualizations, MAGI analysis, and pilot sync.
    /// </summary>
    public class AngelInterceptController : MonoBehaviour
    {
        [Header("NERV Mission Configuration")]
        [SerializeField] private NERVTheme theme;
        
        [Header("References")]
        [SerializeField, FormerlySerializedAs("spawner")] private AngelSpawner angelSpawner;
        [SerializeField, FormerlySerializedAs("answerButtonGroup")] private AnswerButtonGroup terminalGroup;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, FormerlySerializedAs("jarCanvas")] private CanvasGroup interceptionFieldGroup; 
        [SerializeField, FormerlySerializedAs("readinessCanvas")] private CanvasGroup missionReadyCanvas; // New field added
        
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
            EventBus.OnSyncRateChanged += HandleSyncRateChanged;
            EventBus.OnProblemStarted += HandleProblemStarted;
        }

        private void OnDisable()
        {
            EventBus.OnInterventionTriggered -= HandleIntervention;
            EventBus.OnSyncRateChanged -= HandleSyncRateChanged;
            EventBus.OnProblemStarted -= HandleProblemStarted;
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
            
            terminalGroup.SetupButtons(
                _currentProblem.correctValue,
                _currentProblem.distractorValues.ToArray()
            );
            terminalGroup.EnableButtons(true);
        }

        public void OnAnswerSelected(int selectedValue)
        {
            if (!_isWaitingForAnswer) return;
            _isWaitingForAnswer = false;
            
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            bool isCorrect = (selectedValue == _currentProblem.correctValue);
            
            terminalGroup.EnableButtons(false);
            
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
            EventBus.OnPlaySuccessFeedback?.Invoke();
            yield return new WaitForSeconds(1.5f);
            Debug.Log("[AngelIntercept] Target Neutralized.");
        }

        private IEnumerator FailureSequence()
        {
            EventBus.OnPlayCorrectionFeedback?.Invoke();
            
            // MAGI ANALYSIS (Re-show frozen)
            angelSpawner.ShowFireflies();
            interceptionFieldGroup.alpha = 1f;
            
            if (_currentProblem.explanationAudio != null)
            {
                audioSource.PlayOneShot(_currentProblem.explanationAudio);
            }
            
            yield return angelSpawner.AnimateCountingSequence();
            
            yield return new WaitForSeconds(1.0f);
            Debug.Log("[AngelIntercept] MAGI Analysis complete. Scaffolding deployed.");
        }

        private void HandleSyncRateChanged(float rate)
        {
            _currentSyncRatio = rate * 100f;
            // Linear scaling: higher sync = shorter scan time
            _currentScanTime = Mathf.Lerp(maxScanTime, minScanTime, rate);
        }

        private void HandleProblemStarted(string questionId)
        {
            // Bridge: When Game Manager starts a problem, we start our round logic
            // We assume GameManager.Instance.CurrentProblem is set, or we could fetch via ID
            var problem = GameManager.Instance != null ? GameManager.Instance.CurrentProblem : null;
            if (problem != null && problem.questionId == questionId)
            {
                StartRound(problem);
            }
            else
            {
                Debug.LogWarning($"[AngelIntercept] Problem mismatch or GM missing. ID: {questionId}");
            }
        }

        private void HandleIntervention(InterventionType type)
        {
            // Optional: Add unique state changes for LevelUp/ScaffoldDown
            Debug.Log($"[NERV] Intervention Received: {type}");
        }
    }
}
