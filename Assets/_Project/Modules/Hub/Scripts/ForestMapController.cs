using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.UI;

namespace QLDMathApp.Modules.Hub
{
    /// <summary>
    /// FOREST MAP CONTROLLER: Enchanted Forest navigation screen.
    /// Implements spatial reasoning (ACARA) through clearing navigation.
    /// Children help their animal guide move through the magical clearing.
    /// </summary>
    public class ForestMapController : MonoBehaviour
    {
        [Header("Garden Theme")]
        [SerializeField] private Architecture.UI.ForestTheme theme; 

        [Header("Clearing References")]
        [SerializeField] private Transform guideTransform; 
        [SerializeField] private List<MapNode> forestClearings; 
        [SerializeField] private AudioSource audioSource;
        
        [Header("Guide Animation")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio (Forest Sounds)")]
        [SerializeField] private AudioClip clearingUnlockSound;
        [SerializeField] private AudioClip guideMovementSound;
        
        private MapNode _currentClearing;
        private bool _isMoving;

        private void OnEnable()
        {
            foreach (var node in forestClearings)
            {
                if (node != null)
                    node.OnNodeSelected += HandleClearingSelected;
            }
        }

        private void OnDisable()
        {
            foreach (var node in forestClearings)
            {
                if (node != null)
                    node.OnNodeSelected -= HandleClearingSelected;
            }
        }

        private void Start()
        {
            InitializeForestMap();
        }

        private void InitializeForestMap()
        {
            // Load progress from PersistenceService
            var userData = Architecture.Services.PersistenceService.Instance.Load<Architecture.Services.AppUserData>();
            int unlockedClearing = 1; // Default to first clearing
            
            for (int i = 0; i < forestClearings.Count; i++)
            {
                bool isUnlocked = i < unlockedClearing;
                bool isCurrent = i == unlockedClearing - 1;
                
                forestClearings[i].Initialize(i + 1, isUnlocked, isCurrent);
            }
            
            if (unlockedClearing > 0 && unlockedClearing <= forestClearings.Count)
            {
                _currentClearing = forestClearings[unlockedClearing - 1];
                guideTransform.position = _currentClearing.AvatarPosition;
            }
        }

        private void HandleClearingSelected(MapNode node)
        {
            if (_isMoving) return;
            if (!node.IsUnlocked) return;
            
            if (node == _currentClearing)
            {
                LaunchActivity(node);
            }
            else
            {
                StartCoroutine(MoveGuideToClearing(node));
            }
        }

        private System.Collections.IEnumerator MoveGuideToClearing(MapNode targetClearing)
        {
            _isMoving = true;
            
            if (guideMovementSound != null)
            {
                audioSource.clip = guideMovementSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            Vector3 startPos = guideTransform.position;
            Vector3 endPos = targetClearing.AvatarPosition;
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = moveCurve.Evaluate(t / duration);
                guideTransform.position = Vector3.Lerp(startPos, endPos, progress);
                yield return null;
            }
            
            guideTransform.position = endPos;
            audioSource.Stop();
            
            _currentClearing = targetClearing;
            _isMoving = false;
        }

        private void LaunchActivity(MapNode node)
        {
            Debug.Log($"[ForestMap] Heading to {node.ActivitySceneName}...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(node.ActivitySceneName);
        }

        public void UnlockNextClearing()
        {
            // Logic to update persistence and animate unlock
            Debug.Log("[ForestMap] A new part of the forest is waking up!");
        }
    }
}
