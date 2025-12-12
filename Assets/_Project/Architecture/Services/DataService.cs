using UnityEngine;
using QLDMathApp.Architecture.Events;
using System.Collections.Generic;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// OFFLINE-FIRST: Logs all interactions to local storage (mocked here).
    /// Implements Stealth Assessment.
    /// </summary>
    public class DataService : MonoBehaviour
    {
        // In a real implementation, this would be an SQLite Connection
        private List<InteractionLog> _localSessionLog = new List<InteractionLog>();

        private void OnEnable()
        {
            EventBus.OnAnswerAttempted += LogInteraction;
        }

        private void OnDisable()
        {
            EventBus.OnAnswerAttempted -= LogInteraction;
        }

        private void LogInteraction(bool isCorrect, float responseTimeMs)
        {
            // Create the log entry
            var log = new InteractionLog
            {
                timestamp = System.DateTime.UtcNow,
                isCorrect = isCorrect,
                responseTimeMs = responseTimeMs,
                // Stealth Assessment Metrics
                hesitationTime = UnityEngine.Input.touchCount > 0 ? 0.5f : 0f, // Mock logic
                dragDeviation = 0.1f // Mock logic
            };

            _localSessionLog.Add(log);
            
            // Persist to Local DB immediately
            SaveToLocalDatabase(log);

            Debug.Log($"[StealthAssessment] Logged: Correct={isCorrect}, Time={responseTimeMs}ms");
        }

        private void SaveToLocalDatabase(InteractionLog log)
        {
            // TODO: SQLite.Insert(log);
            // This ensures data is safe even if the battery dies 1 second later.
        }
        
        public void SyncToCloud()
        {
            // TODO: Check internet -> Upload _localSessionLog -> Clear local if success
        }
    }

    [System.Serializable]
    public struct InteractionLog
    {
        public System.DateTime timestamp;
        public bool isCorrect;
        public float responseTimeMs;
        public float hesitationTime; // Time before first touch
        public float dragDeviation;  // Motor control proxy
    }
}
