using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Hub
{
    /// <summary>
    /// HUB MAP: Adventure-style navigation screen.
    /// Implements spatial reasoning (ACARA) through map navigation.
    /// Children track their avatar moving through the world.
    /// </summary>
    public class HubMapController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform avatarTransform;
        [SerializeField] private List<MapNode> mapNodes;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Avatar Animation")]
        [SerializeField] private float avatarMoveSpeed = 2f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio")]
        [SerializeField] private AudioClip nodeUnlockSound;
        [SerializeField] private AudioClip avatarWalkSound;
        
        private MapNode _currentNode;
        private bool _isMoving;

        private void Start()
        {
            InitializeMap();
        }

        private void InitializeMap()
        {
            // Load progress from DataService
            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            
            for (int i = 0; i < mapNodes.Count; i++)
            {
                bool isUnlocked = i < unlockedLevel;
                bool isCurrent = i == unlockedLevel - 1;
                
                mapNodes[i].Initialize(i + 1, isUnlocked, isCurrent);
                mapNodes[i].OnNodeSelected += HandleNodeSelected;
            }
            
            // Position avatar at current node
            if (unlockedLevel > 0 && unlockedLevel <= mapNodes.Count)
            {
                _currentNode = mapNodes[unlockedLevel - 1];
                avatarTransform.position = _currentNode.AvatarPosition;
            }
        }

        private void HandleNodeSelected(MapNode node)
        {
            if (_isMoving) return;
            if (!node.IsUnlocked) return;
            
            if (node == _currentNode)
            {
                // Tap on current node = start activity
                LaunchActivity(node);
            }
            else
            {
                // Move avatar to new node
                StartCoroutine(MoveAvatarToNode(node));
            }
        }

        private System.Collections.IEnumerator MoveAvatarToNode(MapNode targetNode)
        {
            _isMoving = true;
            
            if (avatarWalkSound != null)
            {
                audioSource.clip = avatarWalkSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            Vector3 startPos = avatarTransform.position;
            Vector3 endPos = targetNode.AvatarPosition;
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / avatarMoveSpeed;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = moveCurve.Evaluate(t / duration);
                avatarTransform.position = Vector3.Lerp(startPos, endPos, progress);
                yield return null;
            }
            
            avatarTransform.position = endPos;
            audioSource.Stop();
            
            _currentNode = targetNode;
            _isMoving = false;
        }

        private void LaunchActivity(MapNode node)
        {
            Debug.Log($"[Hub] Launching activity: {node.ActivitySceneName}");
            
            // Store current node for return
            PlayerPrefs.SetInt("LastNodeIndex", node.NodeIndex);
            
            // Load the activity scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(node.ActivitySceneName);
        }

        /// <summary>
        /// Called when an activity is completed to unlock next node.
        /// </summary>
        public void UnlockNextNode()
        {
            int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
            int nextLevel = currentUnlocked + 1;
            
            if (nextLevel <= mapNodes.Count)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
                
                MapNode newNode = mapNodes[nextLevel - 1];
                newNode.PlayUnlockAnimation();
                
                if (nodeUnlockSound != null)
                {
                    audioSource.PlayOneShot(nodeUnlockSound);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var node in mapNodes)
            {
                node.OnNodeSelected -= HandleNodeSelected;
            }
        }
    }
}
