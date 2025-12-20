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
    /// ENTRY PLUG SUPPLY: Counting game (re-themed).
    /// "Supply 3 modules for the Entry Plug!"
    /// Child drags items to Entry Plug, practicing one-to-one correspondence.
    /// </summary>
    public class EntryPlugSupplyController : MonoBehaviour // Renamed from LunchboxPackerController
    {
        [Header("NERV Theme")]
        [SerializeField] private NERVTheme theme;

        [Header("Supply References")]
        [SerializeField, FormerlySerializedAs("itemSpawnArea")] private Transform supplySpawnArea; 
        [SerializeField, FormerlySerializedAs("lunchbox")] private EntryPlugSlot entryPlug; 
        [SerializeField] private AudioSource audioSource;
        [SerializeField, FormerlySerializedAs("characterImage")] private Image operatorImage; 
        [SerializeField, FormerlySerializedAs("characterAnimator")] private Animator operatorAnimator;
        
        [Header("Prefabs")]
        [SerializeField, FormerlySerializedAs("foodPrefabs")] private DraggableItem[] supplyPrefabs; 
        
        [Header("Mission Settings")]
        [SerializeField, FormerlySerializedAs("maxItemsToSpawn")] private int maxSuppliesAvailable = 8;
        
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
            
            // Play instruction: "Initialize 3 power modules for EVA-01!"
            if (problem.questionAudio != null)
            {
                audioSource.PlayOneShot(problem.questionAudio);
            }
            
            // Operator monitoring
            if (operatorAnimator != null)
            {
                operatorAnimator.SetTrigger("Monitoring");
            }
            
            _roundStartTime = Time.time;
            entryPlug.ResetSlots(_targetCount);
        }

        private void SpawnSupplies()
        {
            if (supplyPrefabs.Length == 0) return;
            
            // Spawn target modules + distractors
            DraggableItem prefab = supplyPrefabs[Random.Range(0, supplyPrefabs.Length)];
            
            for (int i = 0; i < maxSuppliesAvailable; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                DraggableItem item = Instantiate(prefab, spawnPos, Quaternion.identity, supplySpawnArea);
                item.OnConnectionEstablished += HandleModuleSupplied; // Renamed from OnDroppedInLunchbox
                item.OnConnectionFailed += HandleModuleDroppedOutside; // Renamed from OnDroppedOutside
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
            
            Debug.Log($"[NERV] Module {_suppliedCount} initialized.");
            
            // Visual: Module locks into Entry Plug slot
            entryPlug.AcceptItem(item, _suppliedCount - 1);
            
            // Check if mission objective met
            if (_suppliedCount >= _targetCount)
            {
                StartCoroutine(CompleteSupplyMission());
            }
        }

        private void HandleModuleDroppedOutside(DraggableItem item)
        {
            item.ReturnToStart();
        }

        private IEnumerator CompleteSupplyMission()
        {
            yield return new WaitForSeconds(0.3f);
            
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            
            // MISSION SUCCESS
            EventBus.OnAnswerAttempted?.Invoke(true, responseTime);
            EventBus.OnPlaySuccessFeedback?.Invoke();
            
            // Sync Rate Feedback
            EventBus.OnSyncRateChanged?.Invoke(0.85f); // Example sync rate

            if (operatorAnimator != null)
            {
                operatorAnimator.SetTrigger("AllClear");
            }
            
            yield return new WaitForSeconds(2f);
            Debug.Log("[NERV] Supply mission complete. EVA-01 active.");
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
            Debug.Log("[NERV] Playing demo...");
            
            // Highlight the Entry Plug
            entryPlug.Highlight(true);
            
            // Animate one item moving to plug
            if (_spawnedModules.Count > 0)
            {
                var demoItem = _spawnedModules[0];
                yield return demoItem.AnimateDemoMove(entryPlug.GetSlotPosition(0));
            }
            
            entryPlug.Highlight(false);
            
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
