using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// FIREFLY SPAWNER: Manages the collection of magical fireflies.
    /// RENAMED from AngelSpawner as per Audit.
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
        
        private List<FireflyAnimator> _activeFireflies = new List<FireflyAnimator>();
        private List<FireflyAnimator> _fireflyPool = new List<FireflyAnimator>();

        // Standard dice patterns
        private static readonly Vector2[][] DicePatterns = new Vector2[][]
        {
            new Vector2[] { Vector2.zero },
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) },
            new Vector2[] { new Vector2(-0.5f, 0.5f), Vector2.zero, new Vector2(0.5f, -0.5f) },
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
                           new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) },
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                           new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) },
            new Vector2[] { new Vector2(-0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                           new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                           new Vector2(-0.5f, -0.6f), new Vector2(0.5f, -0.6f) },
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
                firefly.StartFloating(Random.Range(0f, 1f));
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
                return DicePatterns[count - 1];
            
            Vector2[] positions = new Vector2[count];
            for (int i = 0; i < count; i++)
                positions[i] = Random.insideUnitCircle * 0.8f;
            return positions;
        }

        public void HideFireflies()
        {
            foreach (var firefly in _activeFireflies)
                if (firefly != null) firefly.FadeOut(0.2f);
        }

        public void ShowFireflies()
        {
            foreach (var firefly in _activeFireflies)
            {
                if (firefly != null)
                {
                    firefly.gameObject.SetActive(true);
                    firefly.FadeIn(0.2f);
                    firefly.StopFloating();
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
                
                firefly.Pulse();
                yield return new WaitForSeconds(0.5f);
            }
            Destroy(finger);
        }
    }
}
