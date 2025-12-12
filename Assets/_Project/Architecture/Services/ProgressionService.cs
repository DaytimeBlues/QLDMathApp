using UnityEngine;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Data;
using System.Collections.Generic;
using System.Linq;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// DIRECTOR SYSTEM: Controls Programmatic Leveling.
    /// Analyzes the last N interactions to decide the next difficulty.
    /// </summary>
    public class ProgressionService : MonoBehaviour
    {
        [Header("Config")]
        public int analysisWindowSize = 5;
        public float fluencyTimeThreshold = 2000f; // 2 seconds

        private Queue<bool> _accuracyHistory = new Queue<bool>();
        private Queue<float> _timeHistory = new Queue<float>();

        private void OnEnable()
        {
            EventBus.OnAnswerAttempted += RecordPerformance;
        }

        private void OnDisable()
        {
            EventBus.OnAnswerAttempted -= RecordPerformance;
        }

        private void RecordPerformance(bool isCorrect, float timeMs)
        {
            if (_accuracyHistory.Count >= analysisWindowSize)
            {
                _accuracyHistory.Dequeue();
                _timeHistory.Dequeue();
            }

            _accuracyHistory.Enqueue(isCorrect);
            _timeHistory.Enqueue(timeMs);

            AnalyzeAndDirect();
        }

        private void AnalyzeAndDirect()
        {
            if (_accuracyHistory.Count < 3) return; // Need more data

            float accuracy = (float)_accuracyHistory.Count(a => a) / _accuracyHistory.Count;
            float avgTime = _timeHistory.Average();

            // LOGIC: Structural Hybridization Driver
            if (accuracy > 0.8f && avgTime < fluencyTimeThreshold)
            {
                // High Accuracy + Fast Speed = Mastery
                Debug.Log("[Director] Mastery Detected. Leveling Up.");
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.LevelUp);
                ClearHistory(); // Reset for new level
            }
            else if (accuracy < 0.5f)
            {
                // Low Accuracy = Needs Scaffolding
                Debug.Log("[Director] Struggle Detected. Triggering Instruction.");
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.ShowDemo);
                ClearHistory();
            }
            // Else: Maintain Flow (Do nothing, keep practicing)
        }

        private void ClearHistory()
        {
            _accuracyHistory.Clear();
            _timeHistory.Clear();
        }
    }
}
