using UnityEngine;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Data;
using System.Collections.Generic;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// DIRECTOR SYSTEM: Controls Programmatic Leveling.
    /// Analyzes the last N interactions to decide the next difficulty.
    /// PERFORMANCE: Uses incremental tracking instead of LINQ to avoid GC allocations.
    /// </summary>
    public class ProgressionService : MonoBehaviour
    {
        [Header("Config")]
        public int analysisWindowSize = 5;
        public float fluencyTimeThreshold = 2000f; // 2 seconds

        private Queue<bool> _accuracyHistory = new Queue<bool>();
        private Queue<float> _timeHistory = new Queue<float>();
        
        // PERFORMANCE: Running totals for incremental calculation
        private int _correctCount = 0;
        private float _totalTime = 0f;

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
            // PERFORMANCE: Update running totals when dequeuing
            if (_accuracyHistory.Count >= analysisWindowSize)
            {
                bool oldAccuracy = _accuracyHistory.Dequeue();
                float oldTime = _timeHistory.Dequeue();
                
                if (oldAccuracy) _correctCount--;
                _totalTime -= oldTime;
            }

            // Add new data and update running totals
            _accuracyHistory.Enqueue(isCorrect);
            _timeHistory.Enqueue(timeMs);
            
            if (isCorrect) _correctCount++;
            _totalTime += timeMs;

            AnalyzeAndDirect();
        }

        private void AnalyzeAndDirect()
        {
            if (_accuracyHistory.Count < 3) return; // Need more data

            // PERFORMANCE: Use running totals instead of LINQ
            float accuracy = (float)_correctCount / _accuracyHistory.Count;
            float avgTime = _totalTime / _timeHistory.Count;

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
            _correctCount = 0;
            _totalTime = 0f;
        }
    }
}
