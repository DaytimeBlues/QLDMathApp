using UnityEngine;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Architecture.Managers
{
    /// <summary>
    /// GARDEN GROWTH MANAGER: Manages adaptive difficulty and performance tracking.
    /// Uses the "Magic Garden" metaphor: success makes the garden bloom.
    /// </summary>
    public class GardenGrowthManager : MonoBehaviour
    {
        [Header("Garden Calibration")]
        [SerializeField] private float baseGrowthRate = 0.4f;
        [SerializeField] private float bloomIncreaseRate = 0.05f;
        [SerializeField] private float fadeDecreaseRate = 0.1f;
        [SerializeField] private float fastResponseThreshold = 2000f; // ms
        
        private float _currentGrowthRate;
        
        private void Awake()
        {
            _currentGrowthRate = baseGrowthRate;
        }
        
        private void OnEnable()
        {
            EventBus.OnAnswerAttempted += HandleAnswerAttempted;
        }
        
        private void OnDisable()
        {
            EventBus.OnAnswerAttempted -= HandleAnswerAttempted;
        }
        
        private void Start()
        {
            // Initial garden state
            EventBus.OnGardenBloomChanged?.Invoke(_currentGrowthRate);
            Debug.Log($"[GardenGrowth] Magic Garden initialized. Current Bloom: {_currentGrowthRate * 100:F1}%");
        }
        
        private void HandleAnswerAttempted(bool isCorrect, float responseTime)
        {
            if (isCorrect)
            {
                float bonus = responseTime < fastResponseThreshold ? 0.02f : 0f;
                _currentGrowthRate = Mathf.Clamp01(_currentGrowthRate + bloomIncreaseRate + bonus);
            }
            else
            {
                _currentGrowthRate = Mathf.Clamp01(_currentGrowthRate - fadeDecreaseRate);
            }
            
            // Magic garden growth update
            EventBus.OnGardenBloomChanged?.Invoke(_currentGrowthRate);
            
            // FOREST HELP: Check for intervention thresholds
            if (_currentGrowthRate > 0.85f)
            {
                Debug.Log("[GardenGrowth] Garden is blooming! Escalating difficulty.");
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.LevelUp);
            }
            else if (_currentGrowthRate < 0.25f)
            {
                Debug.Log("[GardenGrowth] Garden needs help. Sending friendly forest assistance.");
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.ScaffoldDown);
            }
        }
    }
}
