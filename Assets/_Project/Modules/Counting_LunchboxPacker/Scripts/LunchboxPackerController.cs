using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Counting
{
    /// <summary>
    /// LUNCHBOX PACKER: Counting game (AC9M1N01).
    /// "Pack 3 strawberries for the kangaroo!"
    /// Child drags items to lunchbox, practicing one-to-one correspondence.
    /// </summary>
    public class LunchboxPackerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform itemSpawnArea;
        [SerializeField] private LunchboxSlot lunchbox;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Image characterImage; // The kangaroo
        [SerializeField] private Animator characterAnimator;
        
        [Header("Prefabs")]
        [SerializeField] private DraggableItem[] foodPrefabs; // strawberry, apple, banana, etc.
        
        [Header("Settings")]
        [SerializeField] private int maxItemsToSpawn = 8; // More than needed for distraction
        
        private MathProblemSO _currentProblem;
        private int _targetCount;
        private int _packedCount;
        private List<DraggableItem> _spawnedItems = new List<DraggableItem>();
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
            _packedCount = 0;
            
            ClearItems();
            SpawnItems();
            
            // Play instruction: "Pack 3 strawberries for Kanga!"
            if (problem.questionAudio != null)
            {
                audioSource.PlayOneShot(problem.questionAudio);
            }
            
            // Character looks expectant
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Waiting");
            }
            
            _roundStartTime = Time.time;
            lunchbox.ResetSlots(_targetCount);
        }

        private void SpawnItems()
        {
            if (foodPrefabs.Length == 0) return;
            
            // Spawn target items + distractors
            DraggableItem prefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
            
            for (int i = 0; i < maxItemsToSpawn; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                DraggableItem item = Instantiate(prefab, spawnPos, Quaternion.identity, itemSpawnArea);
                item.OnDroppedInLunchbox += HandleItemPacked;
                item.OnDroppedOutside += HandleItemDroppedOutside;
                _spawnedItems.Add(item);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // Scatter items in the spawn area
            float x = Random.Range(-2f, 2f);
            float y = Random.Range(-1f, 1f);
            return itemSpawnArea.position + new Vector3(x, y, 0);
        }

        private void HandleItemPacked(DraggableItem item)
        {
            _packedCount++;
            
            // Play count audio: "One!", "Two!", etc.
            // (Would use NumberAudioService here)
            Debug.Log($"[Lunchbox] Packed: {_packedCount}");
            
            // Visual: Item settles into lunchbox slot
            lunchbox.AcceptItem(item, _packedCount - 1);
            
            // Check if done
            if (_packedCount >= _targetCount)
            {
                StartCoroutine(CompleteRound());
            }
        }

        private void HandleItemDroppedOutside(DraggableItem item)
        {
            // Return to original position (no penalty, safe failure)
            item.ReturnToStart();
        }

        private IEnumerator CompleteRound()
        {
            yield return new WaitForSeconds(0.3f);
            
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            
            // SUCCESS!
            EventBus.OnAnswerAttempted?.Invoke(true, responseTime);
            EventBus.OnPlaySuccessFeedback?.Invoke();
            
            // Character celebrates
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Happy");
            }
            
            // Play "Thank you!" or "Yummy!"
            yield return new WaitForSeconds(2f);
            
            Debug.Log("[Lunchbox] Round complete!");
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
            Debug.Log("[Lunchbox] Playing demo...");
            
            // Highlight the lunchbox
            lunchbox.Highlight(true);
            
            // Animate one item moving to lunchbox
            if (_spawnedItems.Count > 0)
            {
                var demoItem = _spawnedItems[0];
                yield return demoItem.AnimateDemoMove(lunchbox.GetSlotPosition(0));
            }
            
            lunchbox.Highlight(false);
            
            // Reset for player to try
            ClearItems();
            SpawnItems();
            _packedCount = 0;
        }

        private void ClearItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                {
                    item.OnDroppedInLunchbox -= HandleItemPacked;
                    item.OnDroppedOutside -= HandleItemDroppedOutside;
                    Destroy(item.gameObject);
                }
            }
            _spawnedItems.Clear();
        }
    }
}
