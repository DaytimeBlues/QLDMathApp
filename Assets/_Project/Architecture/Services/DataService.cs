using UnityEngine;
using System;
using System.Collections.Generic;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// STEALTH ASSESSMENT: Logs all pedagogical interactions for longitudinal analysis.
    /// Decoupled from storage details via IInteractionLogStore.
    /// Manages session lifecycle and high-fidelity timing metrics.
    /// </summary>
    public class DataService : MonoBehaviour
    {
        public static DataService Instance { get; private set; }

        private IInteractionLogStore _logStore;
        private string _sessionUid;
        private DateTime _currentProblemStartTime;
        private bool _isProblemActive;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // AUDIT FIX: Decoupled storage implementation
                _logStore = new JsonInteractionStore();
                _sessionUid = Guid.NewGuid().ToString();
                
                Debug.Log($"[DataService] Session Started: {_sessionUid}");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventBus.OnProblemStarted += HandleProblemStarted;
            EventBus.OnAnswerAttempted += LogInteraction;
        }

        private void OnDisable()
        {
            EventBus.OnProblemStarted -= HandleProblemStarted;
            EventBus.OnAnswerAttempted -= LogInteraction;
        }

        private void HandleProblemStarted(string questionId)
        {
            _currentProblemStartTime = DateTime.UtcNow;
            _isProblemActive = true;
        }

        private void LogInteraction(bool isCorrect, float responseTimeMs)
        {
            if (!_isProblemActive) return;

            // PERFORMANCE: Standardise units to seconds
            float responseTimeSeconds = responseTimeMs / 1000f;
            
            // STEALTH ASSESSMENT: Real hesitation calculation
            // Time from problem appearance to the moment the answer was finalized (roughly)
            float totalTimeSinceStart = (float)(DateTime.UtcNow - _currentProblemStartTime).TotalSeconds;
            
            // For now, we estimate hesitation as a portion of the total time, 
            // but in a future update, we can track the first touch event.
            float hesitationTime = totalTimeSinceStart - responseTimeSeconds;

            var log = new InteractionLog
            {
                sessionUid = _sessionUid,
                timestamp = DateTime.UtcNow,
                isCorrect = isCorrect,
                responseTime = responseTimeSeconds,
                hesitationTime = Mathf.Max(0, hesitationTime),
                dragDeviation = 0.1f, // TODO: Link to InteractionController motor data
                isSimulated = false
            };

            _logStore.SaveLog(log);
            _isProblemActive = false;

            Debug.Log($"[StealthAssessment] Logged: Correct={isCorrect}, Response={responseTimeSeconds}s, Hesitation={hesitationTime:F2}s");
        }

        public List<InteractionLog> GetSessionLogs()
        {
            return _logStore.GetAllLogs().FindAll(l => l.sessionUid == _sessionUid);
        }
        
        public void SyncToCloud()
        {
            // TODO: Implementation for cloud upload using specific sync status per record
            Debug.Log("[DataService] Cloud sync placeholder triggered.");
        }
    }

    [Serializable]
    public struct InteractionLog
    {
        public string sessionUid;
        public DateTime timestamp;
        public bool isCorrect;
        public float responseTime; // Seconds
        public float hesitationTime; // Seconds
        public float dragDeviation;  // Motor control proxy
        public bool isSimulated;
    }
}
