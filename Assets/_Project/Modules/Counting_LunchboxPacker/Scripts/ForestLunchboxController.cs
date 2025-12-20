using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using QLDMathApp.Architecture.UI;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Counting
{
    /// <summary>
    /// FOREST LUNCHBOX: Counting game for Foundation Year.
    /// "Help the animals pack 3 apples for the picnic!"
    /// Child drags items to the basket, practicing one-to-one correspondence.
    /// </summary>
    public class ForestLunchboxController : MonoBehaviour
    {
        [Header("Garden Theme")]
        [SerializeField] private ForestTheme theme; // TODO: Replace with GardenTheme

        [Header("Basket References")]
        [SerializeField] private Transform supplySpawnArea; 
        [SerializeField] private LunchboxSlot picnicBasket; 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Image guideImage; 
        [SerializeField] private Animator guideAnimator;
        
        [Header("Prefabs")]
        [SerializeField] private DraggableItem[] supplyPrefabs; 
        
        [Header("Activity Settings")]
        [SerializeField] private int maxSuppliesAvailable = 8;
        
        private MathProblemSO _currentProblem;
        private int _targetCount;
        private int _suppliedCount;
        private List<DraggableItem> _spawnedModules = new List<DraggableItem>();
        private float _roundStartTime;

        private void OnEnable()
        {
            EventBus.OnInterventionTriggered += HandleIntervention;
        }

        private void OnDisable()
        {
            EventBus.OnInterventionTriggered -= HandleIntervention;
        }

        public void StartRound(MathProblemSO problem)
        {
            _currentProblem = problem;
            _targetCount = problem.correctValue;
            _suppliedCount = 0;
            
            ClearSupplies();
            SpawnSupplies();
            
            // Play instruction: "Initialize 3 items for the picnic!"
            if (problem.questionAudio != null)
            {
                audioSource.PlayOneShot(problem.questionAudio);
            }
            
            // Guide monitoring
            if (guideAnimator != null)
            {
                guideAnimator.SetTrigger("Monitoring");
            }
            
            _roundStartTime = Time.time;
            picnicBasket.ResetSlots(_targetCount);
        }

        private void SpawnSupplies()
        {
            if (supplyPrefabs.Length == 0) return;
            
            // Spawn target items + distractors
            DraggableItem prefab = supplyPrefabs[Random.Range(0, supplyPrefabs.Length)];
            
            for (int i = 0; i < maxSuppliesAvailable; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                DraggableItem item = Instantiate(prefab, spawnPos, Quaternion.identity, supplySpawnArea);
                item.OnConnectionEstablished += HandleModuleSupplied; 
                item.OnConnectionFailed += HandleModuleDroppedOutside;
                _spawnedModules.Add(item);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            float x = Random.Range(-2f, 2f);
            float y = Random.Range(-1f, 1f);
            return supplySpawnArea.position + new Vector3(x, y, 0);
        }

        private void HandleModuleSupplied(DraggableItem item)
        {
            _suppliedCount++;
            
            Debug.Log($"[ForestLunchbox] Item {_suppliedCount} packed.");
            
            // Visual: Item locks into basket slot
            picnicBasket.AcceptItem(item, _suppliedCount - 1);
            
            // Check if activity objective met
            if (_suppliedCount >= _targetCount)
            {
                StartCoroutine(CompleteSupplyForestRound());
            }
        }

        private void HandleModuleDroppedOutside(DraggableItem item)
        {
            item.ReturnToStart();
        }

        private IEnumerator CompleteSupplyForestRound()
        {
            yield return new WaitForSeconds(0.3f);
            
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            
            // ACTIVITY SUCCESS
            EventBus.OnAnswerAttempted?.Invoke(true, responseTime);
            EventBus.OnGuideSpoke?.Invoke(GuidePersonality.KindBunny, "What a lovely picnic basket! You've packed it perfectly.");
            
            // Magic garden growth update (Audit Fix: Replaces OnMasteryLevelChanged)
            EventBus.OnGrowthProgressChanged?.Invoke(0.85f); 

            if (guideAnimator != null)
            {
                guideAnimator.SetTrigger("AllClear");
            }
            
            yield return new WaitForSeconds(2f);
            Debug.Log("[ForestLunchbox] Lunchbox packed. Ready for the picnic!");
        }

        private void HandleIntervention(InterventionType type)
        {
            if (type == InterventionType.ShowDemo)
            {
                StartCoroutine(PlayDemoSequence());
            }
        }

        private IEnumerator PlayDemoSequence()
        {
            // EXPLANATORY FEEDBACK: Show how to pack items
            Debug.Log("[ForestLunchbox] Playing demo...");
            
            // Highlight the Basket
            picnicBasket.Highlight(true);
            
            // Animate one item moving to basket
            if (_spawnedModules.Count > 0)
            {
                var demoItem = _spawnedModules[0];
                yield return demoItem.AnimateDemoMove(picnicBasket.GetSlotPosition(0));
            }
            
            picnicBasket.Highlight(false);
            
            // Reset for player to try
            ClearSupplies();
            SpawnSupplies();
            _suppliedCount = 0;
        }

        private void ClearSupplies()
        {
            foreach (var item in _spawnedModules)
            {
                if (item != null)
                {
                    item.OnConnectionEstablished -= HandleModuleSupplied;
                    item.OnConnectionFailed -= HandleModuleDroppedOutside;
                    Destroy(item.gameObject);
                }
            }
            _spawnedModules.Clear();
        }
    }
}
}
