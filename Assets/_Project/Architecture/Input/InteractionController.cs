using UnityEngine;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Managers;

namespace QLDMathApp.Architecture.Input
{
    /// <summary>
    /// Handles Raycasting/Touches and reporting answers.
    /// Simplified for the architecture demo.
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        private float _problemStartTime;
        private bool _inputLocked;

        private void OnEnable()
        {
            EventBus.OnProblemStarted += OnProblemStarted;
            EventBus.OnGameStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnProblemStarted -= OnProblemStarted;
            EventBus.OnGameStateChanged -= OnStateChanged;
        }

        private void OnProblemStarted(string problemId)
        {
            _problemStartTime = Time.time;
            _inputLocked = false;
        }

        private void OnStateChanged(GameState newState)
        {
            _inputLocked = (newState != GameState.Gameplay);
        }

        // Called by UI Button or DragDrop script
        public void SubmitAnswer(int value)
        {
            if (_inputLocked) return;

            float responseTime = (Time.time - _problemStartTime) * 1000f; // ms
            bool isCorrect = (value == GameManager.Instance.CurrentProblem.correctValue);

            _inputLocked = true; // Prevent double submission

            // Fire the event - Observers will handle Feedback and Logging
            EventBus.OnAnswerAttempted?.Invoke(isCorrect, responseTime);
        }
    }
}
