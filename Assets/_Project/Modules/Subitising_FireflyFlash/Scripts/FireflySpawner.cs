using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// Spawns fireflies in the jar using:
    /// - Standard dice patterns (Level 1 - aids subitising)
    /// - Random positions (Level 2 - tests true subitising)
    /// </summary>
    public class FireflySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject fireflyPrefab;
        [SerializeField] private Transform jarContainer;
        [SerializeField] private Transform countingFingerPrefab;
        
        [Header("Pattern Settings")]
        [SerializeField] private bool useDicePatterns = true;
        [SerializeField] private float spawnRadius = 1.5f;
        
        private List<GameObject> _spawnedFireflies = new List<GameObject>();
        
        // Standard dice patterns (normalized -1 to 1 coordinates)
        private static readonly Vector2[][] DicePatterns = new Vector2[][]
        {
            // 1 = center
            new Vector2[] { Vector2.zero },
            // 2 = diagonal
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) },
            // 3 = diagonal line
            new Vector2[] { new Vector2(-0.5f, 0.5f), Vector2.zero, new Vector2(0.5f, -0.5f) },
            // 4 = corners
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
                           new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) },
            // 5 = corners + center
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                           new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) },
            // 6 = two columns
            new Vector2[] { new Vector2(-0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                           new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                           new Vector2(-0.5f, -0.6f), new Vector2(0.5f, -0.6f) },
            // 7-10: Extended patterns
            new Vector2[] { new Vector2(-0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0f, 0.3f),
                           new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                           new Vector2(-0.5f, -0.6f), new Vector2(0.5f, -0.6f) },
        };

        public int CurrentCount => _spawnedFireflies.Count;

        public void SpawnFireflies(int count)
        {
            ClearFireflies();
            
            Vector2[] positions = GetPositions(count);
            
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = jarContainer.position + (Vector3)(positions[i] * spawnRadius);
                GameObject firefly = Instantiate(fireflyPrefab, pos, Quaternion.identity, jarContainer);
                
                // Add gentle float animation
                var animator = firefly.GetComponent<FireflyAnimator>();
                if (animator != null)
                {
                    animator.StartFloating(Random.Range(0f, 1f)); // Random phase offset
                }
                
                _spawnedFireflies.Add(firefly);
            }
        }

        private Vector2[] GetPositions(int count)
        {
            if (useDicePatterns && count <= DicePatterns.Length)
            {
                return DicePatterns[count - 1];
            }
            
            // Fallback: Random positions
            Vector2[] positions = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = Random.insideUnitCircle * 0.8f;
            }
            return positions;
        }

        public void HideFireflies()
        {
            foreach (var firefly in _spawnedFireflies)
            {
                // Fade out with glow effect
                var animator = firefly.GetComponent<FireflyAnimator>();
                if (animator != null)
                {
                    animator.FadeOut(0.2f);
                }
                else
                {
                    firefly.SetActive(false);
                }
            }
        }

        public void ShowFireflies()
        {
            foreach (var firefly in _spawnedFireflies)
            {
                firefly.SetActive(true);
                var animator = firefly.GetComponent<FireflyAnimator>();
                if (animator != null)
                {
                    animator.FadeIn(0.2f);
                    animator.StopFloating(); // Freeze for counting
                }
            }
        }

        /// <summary>
        /// EXPLANATORY FEEDBACK: Animate a finger counting each firefly.
        /// "One... Two... Three!" with audio and visual cues.
        /// </summary>
        public IEnumerator AnimateCountingSequence()
        {
            if (countingFingerPrefab == null) yield break;
            
            Transform finger = Instantiate(countingFingerPrefab, jarContainer);
            
            for (int i = 0; i < _spawnedFireflies.Count; i++)
            {
                GameObject firefly = _spawnedFireflies[i];
                
                // Move finger to firefly
                float moveTime = 0.3f;
                Vector3 startPos = finger.position;
                Vector3 endPos = firefly.transform.position;
                
                for (float t = 0; t < moveTime; t += Time.deltaTime)
                {
                    finger.position = Vector3.Lerp(startPos, endPos, t / moveTime);
                    yield return null;
                }
                
                // Highlight firefly (pulse effect)
                var animator = firefly.GetComponent<FireflyAnimator>();
                if (animator != null)
                {
                    animator.Pulse();
                }
                
                // Say the number (would trigger NumberAudioService)
                Debug.Log($"[Counting] {i + 1}");
                
                yield return new WaitForSeconds(0.5f);
            }
            
            Destroy(finger.gameObject);
        }

        private void ClearFireflies()
        {
            foreach (var firefly in _spawnedFireflies)
            {
                if (firefly != null) Destroy(firefly);
            }
            _spawnedFireflies.Clear();
        }
    }
}
