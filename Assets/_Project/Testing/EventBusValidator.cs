using UnityEngine;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Testing
{
    /// <summary>
    /// Validates that the Observer Pattern is working correctly.
    /// Logs all events passing through the EventBus.
    /// </summary>
    public class EventBusValidator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool logEvents = true;
        
        private int _correctCount;
        private int _incorrectCount;
        private int _interventionCount;

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += LogGameState;
            EventBus.OnAnswerAttempted += LogAnswer;
            EventBus.OnInterventionTriggered += LogIntervention;
            EventBus.OnPlaySuccessFeedback += LogSuccessFeedback;
            EventBus.OnPlayCorrectionFeedback += LogCorrectionFeedback;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= LogGameState;
            EventBus.OnAnswerAttempted -= LogAnswer;
            EventBus.OnInterventionTriggered -= LogIntervention;
            EventBus.OnPlaySuccessFeedback -= LogSuccessFeedback;
            EventBus.OnPlayCorrectionFeedback -= LogCorrectionFeedback;
        }

        private void LogGameState(GameState state)
        {
            if (logEvents)
                Debug.Log($"<color=yellow>[EventBus] GameStateChanged -> {state}</color>");
        }

        private void LogAnswer(bool correct, float time)
        {
            if (correct) _correctCount++;
            else _incorrectCount++;
            
            if (logEvents)
            {
                string color = correct ? "green" : "red";
                Debug.Log($"<color={color}>[EventBus] AnswerAttempted -> Correct:{correct}, Time:{time:F0}ms</color>");
            }
        }

        private void LogIntervention(InterventionType type)
        {
            _interventionCount++;
            if (logEvents)
                Debug.Log($"<color=cyan>[EventBus] InterventionTriggered -> {type}</color>");
        }

        private void LogSuccessFeedback()
        {
            if (logEvents)
                Debug.Log("<color=green>[EventBus] PlaySuccessFeedback</color>");
        }

        private void LogCorrectionFeedback()
        {
            if (logEvents)
                Debug.Log("<color=orange>[EventBus] PlayCorrectionFeedback</color>");
        }

        /// <summary>
        /// Call this to get a summary of all events logged.
        /// </summary>
        public void PrintSummary()
        {
            Debug.Log("=== EVENT BUS SUMMARY ===");
            Debug.Log($"Correct Answers: {_correctCount}");
            Debug.Log($"Incorrect Answers: {_incorrectCount}");
            Debug.Log($"Interventions: {_interventionCount}");
            Debug.Log($"Accuracy: {(_correctCount * 100f / Mathf.Max(1, _correctCount + _incorrectCount)):F1}%");
        }
    }
}
