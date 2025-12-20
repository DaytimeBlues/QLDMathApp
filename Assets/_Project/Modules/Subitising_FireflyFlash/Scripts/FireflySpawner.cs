using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// FIREFLY SPAWNER: Creates firefly patterns in the magical jar.
    /// - Standard dice patterns (Level 1 - aids subitising)
    /// - Random positions (Level 2 - tests true subitising)
    /// </summary>
    public class FireflySpawner : MonoBehaviour
    {
        [Header("Forest References")]
        [SerializeField] private GameObject fireflyPrefab;
        [SerializeField] private Transform jarContainer;
        [SerializeField] private Transform countingFingerPrefab;
        
        [Header("Pattern Settings")]
        [SerializeField] private bool useDicePatterns = true;
        [SerializeField] private float spawnRadius = 1.5f;
        [SerializeField] private int poolSize = 10;
        
        private List<FireflyAnimator> _activeFireflies = new List<FireflyAnimator>();
        private List<FireflyAnimator> _fireflyPool = new List<FireflyAnimator>();

        private void Start()
        {
            PrewarmPool();
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                FireflyAnimator firefly = Instantiate(fireflyPrefab, jarContainer).GetComponent<FireflyAnimator>();
                firefly.gameObject.SetActive(false);
                _fireflyPool.Add(firefly);
            }
        }
        
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

        public int CurrentCount => _activeFireflies.Count;

        public void SpawnFireflies(int count)
        {
            ReturnToPool();
            
            Vector2[] positions = GetPositions(count);
            
            for (int i = 0; i < count; i++)
            {
                FireflyAnimator firefly = GetFireflyFromPool();
                firefly.transform.position = jarContainer.position + (Vector3)(positions[i] * spawnRadius);
                firefly.gameObject.SetActive(true);
                
                firefly.StartFloating(Random.Range(0f, 1f)); // Random phase offset
                firefly.FadeIn(0.2f);
                _activeFireflies.Add(firefly);
            }
        }

        private FireflyAnimator GetFireflyFromPool()
        {
            if (_fireflyPool.Count > 0)
            {
                FireflyAnimator firefly = _fireflyPool[0];
                _fireflyPool.RemoveAt(0);
                return firefly;
            }
            return Instantiate(fireflyPrefab, jarContainer).GetComponent<FireflyAnimator>();
        }

        private void ReturnToPool()
        {
            foreach (var firefly in _activeFireflies)
            {
                if (firefly != null)
                {
                    firefly.gameObject.SetActive(false);
                    _fireflyPool.Add(firefly);
                }
            }
            _activeFireflies.Clear();
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
            foreach (var firefly in _activeFireflies)
            {
                if (firefly != null)
                {
                    firefly.FadeOut(0.2f);
                }
            }
        }

        public void ShowFireflies()
        {
            foreach (var animator in _activeFireflies)
            {
                if (animator != null)
                {
                    animator.gameObject.SetActive(true);
                    animator.FadeIn(0.2f);
                    animator.StopFloating(); // Freeze for counting
                }
            }
        }

        public IEnumerator AnimateCountingSequence()
        {
            if (countingFingerPrefab == null) yield break;
            
            GameObject finger = Instantiate(countingFingerPrefab.gameObject, jarContainer);
            
            for (int i = 0; i < _activeFireflies.Count; i++)
            {
                FireflyAnimator firefly = _activeFireflies[i];
                
                float moveTime = 0.3f;
                Vector3 startPos = finger.transform.position;
                Vector3 endPos = firefly.transform.position;
                
                for (float t = 0; t < moveTime; t += Time.deltaTime)
                {
                    finger.transform.position = Vector3.Lerp(startPos, endPos, t / moveTime);
                    yield return null;
                }
                
                if (firefly != null)
                {
                    firefly.Pulse();
                }
                
                Debug.Log($"[Counting] {i + 1}");
                yield return new WaitForSeconds(0.5f);
            }
            
            Destroy(finger);
        }
    }
}
