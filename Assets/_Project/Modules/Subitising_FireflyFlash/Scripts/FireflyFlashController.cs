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
    /// FIREFLY FLASH: Core subitising game controller.
    /// Implements:
    /// - Timed stimulus display (0.8-1.5s adaptive)
    /// - Dice pattern arrangements (Level 1) vs Random (Level 2)
    /// - Explanatory feedback on incorrect answers
    /// </summary>
    public class FireflyFlashController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FireflySpawner fireflySpawner;
        [SerializeField] private AnswerButtonGroup answerButtons;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private CanvasGroup jarCanvasGroup;
        
        [Header("Timing (Adaptive)")]
        [SerializeField] private float baseDisplayTime = 1.2f;
        [SerializeField] private float minDisplayTime = 0.6f;
        [SerializeField] private float maxDisplayTime = 2.0f;
        
        private float _currentDisplayTime;
        private MathProblemSO _currentProblem;
        private bool _isWaitingForAnswer;
        private float _roundStartTime;

        private void Start()
        {
            _currentDisplayTime = baseDisplayTime;
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
        /// Called by GameManager to start a round with a specific problem.
        /// </summary>
        public void StartRound(MathProblemSO problem)
        {
            _currentProblem = problem;
            StartCoroutine(RoundSequence());
        }

        private IEnumerator RoundSequence()
        {
            // 1. INSTRUCTION PHASE: Play voice-over
            if (_currentProblem.questionAudio != null)
            {
                audioSource.PlayOneShot(_currentProblem.questionAudio);
                yield return new WaitForSeconds(_currentProblem.questionAudio.length + 0.3f);
            }

            // 2. STIMULUS PHASE: Show fireflies
            fireflySpawner.SpawnFireflies(_currentProblem.correctValue);
            jarCanvasGroup.alpha = 1f;
            
            // Wait for adaptive display time
            yield return new WaitForSeconds(_currentDisplayTime);
            
            // 3. HIDE: Fireflies disappear (the "flash")
            fireflySpawner.HideFireflies();
            jarCanvasGroup.alpha = 0.3f; // Dim the jar
            
            // 4. RESPONSE PHASE: Enable answer buttons
            _roundStartTime = Time.time;
            _isWaitingForAnswer = true;
            
            // Generate distractors and correct answer for buttons
            answerButtons.SetupButtons(
                _currentProblem.correctValue,
                _currentProblem.distractorValues.ToArray()
            );
            answerButtons.EnableButtons(true);
            
            // Wait for answer (handled via OnAnswerSelected callback)
        }

        /// <summary>
        /// Called by AnswerButton when child taps a number.
        /// </summary>
        public void OnAnswerSelected(int selectedValue)
        {
            if (!_isWaitingForAnswer) return;
            _isWaitingForAnswer = false;
            
            float responseTime = (Time.time - _roundStartTime) * 1000f; // ms
            bool isCorrect = (selectedValue == _currentProblem.correctValue);
            
            answerButtons.EnableButtons(false);
            
            // Fire event for DataService and ProgressionService
            EventBus.OnAnswerAttempted?.Invoke(isCorrect, responseTime);
            
            if (isCorrect)
            {
                StartCoroutine(CorrectSequence());
            }
            else
            {
                StartCoroutine(IncorrectSequence());
            }
        }

        private IEnumerator CorrectSequence()
        {
            // MULTI-SENSORY FEEDBACK
            EventBus.OnPlaySuccessFeedback?.Invoke();
            
            // Adaptive: Speed up next round (approach fluency)
            _currentDisplayTime = Mathf.Max(minDisplayTime, _currentDisplayTime - 0.1f);
            
            yield return new WaitForSeconds(1.5f);
            
            // Signal ready for next problem
            Debug.Log("[FireflyFlash] Round Complete - Correct!");
        }

        private IEnumerator IncorrectSequence()
        {
            // EXPLANATORY FEEDBACK (Not just "Wrong!")
            EventBus.OnPlayCorrectionFeedback?.Invoke();
            
            // 1. Re-show the fireflies (frozen)
            fireflySpawner.ShowFireflies();
            jarCanvasGroup.alpha = 1f;
            
            // 2. Play explanation audio
            if (_currentProblem.explanationAudio != null)
            {
                audioSource.PlayOneShot(_currentProblem.explanationAudio);
            }
            
            // 3. Animate counting (finger touches each firefly)
            yield return fireflySpawner.AnimateCountingSequence();
            
            // Adaptive: Slow down next round (more scaffold)
            _currentDisplayTime = Mathf.Min(maxDisplayTime, _currentDisplayTime + 0.2f);
            
            yield return new WaitForSeconds(1.0f);
            
            Debug.Log("[FireflyFlash] Round Complete - Scaffolded.");
        }

        private void HandleIntervention(InterventionType type)
        {
            switch (type)
            {
                case InterventionType.LevelUp:
                    // Reduce display time significantly (mastery mode)
                    _currentDisplayTime = Mathf.Max(minDisplayTime, _currentDisplayTime - 0.3f);
                    Debug.Log($"[FireflyFlash] Level Up! Display time now: {_currentDisplayTime}s");
                    break;
                    
                case InterventionType.ScaffoldDown:
                case InterventionType.ShowDemo:
                    // Increase display time (struggle mode)
                    _currentDisplayTime = maxDisplayTime;
                    Debug.Log("[FireflyFlash] Scaffolding activated. Max display time.");
                    break;
            }
        }
    }
}
