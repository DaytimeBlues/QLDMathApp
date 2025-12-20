using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.UI;

namespace QLDMathApp.Modules.Hub
{
    /// <summary>
    /// NERV TACTICAL DISPLAY: GeoFront map navigation screen.
    /// Implements spatial reasoning (ACARA) through tactical map navigation.
    /// Children track their Eva Unit moving through Tokyo-3.
    /// </summary>
    public class HubMapController : MonoBehaviour
    {
        [Header("NERV Theme")]
        [SerializeField] private NERVTheme theme;

        [Header("Tactical Display References")]
        [SerializeField, FormerlySerializedAs("avatarTransform")] private Transform evaUnitTransform; 
        [SerializeField, FormerlySerializedAs("mapNodes")] private List<MapNode> tacticalSectors; 
        [SerializeField] private AudioSource audioSource;
        
        [Header("Eva Unit Animation")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio (NERV Alerts)")]
        [SerializeField] private AudioClip sectorUnlockSound;
        [SerializeField] private AudioClip evaMovementSound;
        
        private MapNode _currentSector;
        private bool _isMoving;

        private void Start()
        {
            InitializeTacticalMap();
        }

        private void InitializeTacticalMap()
        {
            // Load progress from DataService (MAGI)
            int unlockedSector = PlayerPrefs.GetInt("UnlockedLevel", 1);
            
            for (int i = 0; i < tacticalSectors.Count; i++)
            {
                bool isUnlocked = i < unlockedSector;
                bool isCurrent = i == unlockedSector - 1;
                
                tacticalSectors[i].Initialize(i + 1, isUnlocked, isCurrent);
                tacticalSectors[i].OnNodeSelected += HandleSectorSelected;
            }
            
            // Position Eva Unit at current sector
            if (unlockedSector > 0 && unlockedSector <= tacticalSectors.Count)
            {
                _currentSector = tacticalSectors[unlockedSector - 1];
                evaUnitTransform.position = _currentSector.AvatarPosition;
            }
        }

        private void HandleSectorSelected(MapNode node)
        {
            if (_isMoving) return;
            if (!node.IsUnlocked) return;
            
            if (node == _currentSector)
            {
                // Tap on current node = start interception (activity)
                LaunchInterception(node);
            }
            else
            {
                // Move Eva Unit to new sector
                StartCoroutine(MoveEvaToSector(node));
            }
        }

        private System.Collections.IEnumerator MoveEvaToSector(MapNode targetSector)
        {
            _isMoving = true;
            
            if (evaMovementSound != null)
            {
                audioSource.clip = evaMovementSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            Vector3 startPos = evaUnitTransform.position;
            Vector3 endPos = targetSector.AvatarPosition;
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = moveCurve.Evaluate(t / duration);
                evaUnitTransform.position = Vector3.Lerp(startPos, endPos, progress);
                yield return null;
            }
            
            evaUnitTransform.position = endPos;
            audioSource.Stop();
            
            _currentSector = targetSector;
            _isMoving = false;
        }

        private void LaunchInterception(MapNode node)
        {
            Debug.Log($"[NERV] Initiating interception at {node.ActivitySceneName}");
            
            // Store current sector for return
            PlayerPrefs.SetInt("LastNodeIndex", node.NodeIndex);
            
            // Load the interception scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(node.ActivitySceneName);
        }

        /// <summary>
        /// Called when an interception is successful to unlock next sector.
        /// </summary>
        public void UnlockNextSector()
        {
            int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
            int nextSector = currentUnlocked + 1;
            
            if (nextSector <= tacticalSectors.Count)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextSector);
                
                MapNode newNode = tacticalSectors[nextSector - 1];
                newNode.PlayUnlockAnimation();
                
                if (sectorUnlockSound != null)
                {
                    audioSource.PlayOneShot(sectorUnlockSound);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var node in tacticalSectors)
            {
                node.OnNodeSelected -= HandleSectorSelected;
            }
        }
    }
}
