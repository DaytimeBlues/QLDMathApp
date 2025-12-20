using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using QLDMathApp.Architecture.UI;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Patterns
{
    /// <summary>
    /// SYNC SEQUENCE: Pattern building game (re-themed).
    /// "Complete the synchronization waveform!"
    /// Child copies and continues patterns to stabilize pilot sync.
    /// </summary>
    /// <summary>
    /// PATTERN FLOWER CONTROLLER: Pattern building game (Enchanted Forest theme).
    /// "Complete the flower sequence to help the garden bloom!"
    /// Child copies and continues patterns to grow the magic garden.
    /// </summary>
    public class PatternFlowerController : MonoBehaviour
    {
        [Header("Garden Theme")]
        [SerializeField] private Architecture.UI.ForestTheme theme; // TODO: Replace with GardenTheme

        [Header("Garden References")]
        [SerializeField] private Transform flowerSequenceArea; 
        [SerializeField] private Transform choiceArea; 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Animator gardenAnimator; 

        [Header("Prefabs")]
        [SerializeField] private PatternFlower flowerPrefab; 

        [Header("Flower Assets")]
        [SerializeField] private Sprite[] flowerSprites; 
        [SerializeField] private Color[] flowerColors; 

        private List<PatternFlower> _displayedFlowers = new List<PatternFlower>();
        private List<PatternFlower> _choiceFlowers = new List<PatternFlower>();
        private int _correctFlowerIndex;
        private PatternType _currentPatternType;
        private float _roundStartTime;
        private float _currentMasteryLevel = 0.5f;

        public enum PatternType
        {
            AB,     // Flower A, B, A, B, ?
            ABB,    // Flower A, B, B, A, B, B, ?
            ABC,    // Flower A, B, C, A, B, C, ?
            AABB    // Flower A, A, B, B, A, A, ?
        }

        private void OnEnable()
        {
            EventBus.OnInterventionTriggered += HandleIntervention;
        }

        private void OnDisable()
        {
            EventBus.OnInterventionTriggered -= HandleIntervention;
        }

        public void StartRound(MathProblemSO problem, PatternType patternType)
        {
            _currentPatternType = patternType;
            ClearGarden();
            
            int[] pattern = GeneratePattern(patternType, problem.correctValue);
            DisplayPattern(pattern);
            CreateChoices(pattern[pattern.Length - 1]);
            
            if (problem.questionAudio != null)
            {
                audioSource.PlayOneShot(problem.questionAudio);
            }
            
            _roundStartTime = Time.time;
            EventBus.OnGrowthProgressChanged?.Invoke(_currentMasteryLevel);
        }

        private int[] GeneratePattern(PatternType type, int seed)
        {
            switch (type)
            {
                case PatternType.AB: return new int[] { 0, 1, 0, 1, 0, 1, 0 }; 
                case PatternType.ABB: return new int[] { 0, 1, 1, 0, 1, 1, 0 }; 
                case PatternType.ABC: return new int[] { 0, 1, 2, 0, 1, 2, 0 }; 
                case PatternType.AABB: return new int[] { 0, 0, 1, 1, 0, 0, 1 }; 
                default: return new int[] { 0, 1, 0, 1, 0 };
            }
        }

        private void DisplayPattern(int[] pattern)
        {
            float spacing = 120f;
            float startX = -(pattern.Length - 1) * spacing / 2f; 

            for (int i = 0; i < pattern.Length - 1; i++) 
            {
                PatternFlower flower = Instantiate(flowerPrefab, flowerSequenceArea);
                flower.SetupDisplay(flowerSprites[0], flowerColors[pattern[i]], i);
                flower.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * spacing, 0);
                _displayedFlowers.Add(flower);
            }

            // Hidden flower at the end
            PatternFlower mysteryFlower = Instantiate(flowerPrefab, flowerSequenceArea);
            mysteryFlower.SetupMystery();
            mysteryFlower.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + (pattern.Length - 1) * spacing, 0);
            _displayedFlowers.Add(mysteryFlower);

            _correctFlowerIndex = pattern[pattern.Length - 1];
        }

        private void CreateChoices(int correctIndex)
        {
            List<int> choices = new List<int> { correctIndex };
            for (int i = 0; i < flowerColors.Length && choices.Count < 3; i++)
            {
                if (i != correctIndex) choices.Add(i);
            }

            // Shuffle
            for (int i = choices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = choices[i]; choices[i] = choices[j]; choices[j] = temp;
            }

            float spacing = 150f;
            float startX = -(choices.Count - 1) * spacing / 2f;

            for (int i = 0; i < choices.Count; i++)
            {
                PatternFlower flower = Instantiate(flowerPrefab, choiceArea);
                flower.SetupChoice(flowerSprites[0], flowerColors[choices[i]], choices[i]);
                flower.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * spacing, 0);
                flower.OnSelected += HandleFlowerSelected;
                _choiceFlowers.Add(flower);
            }
        }

        private void HandleFlowerSelected(PatternFlower flower)
        {
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            bool isCorrect = flower.ValueIndex == _correctFlowerIndex;

            foreach (var f in _choiceFlowers) f.SetInteractable(false);

            EventBus.OnAnswerAttempted?.Invoke(isCorrect, responseTime);

            if (isCorrect) StartCoroutine(SuccessSequence(flower));
            else StartCoroutine(FailureSequence());
        }

        private IEnumerator SuccessSequence(PatternFlower selectedFlower)
        {
            _currentMasteryLevel = Mathf.Min(1.0f, _currentMasteryLevel + 0.1f);
            EventBus.OnGrowthProgressChanged?.Invoke(_currentMasteryLevel);

            PatternFlower mysteryFlower = _displayedFlowers[_displayedFlowers.Count - 1];
            Vector3 targetPos = mysteryFlower.transform.position;

            float duration = 0.5f;
            Vector3 startPos = selectedFlower.transform.position;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                selectedFlower.transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
                yield return null;
            }

            mysteryFlower.RevealAs(flowerSprites[0], flowerColors[_correctFlowerIndex]);

            if (gardenAnimator != null) gardenAnimator.SetTrigger("Bloom");

            yield return new WaitForSeconds(1.5f);
            Debug.Log("[PatternFlower] Garden is blooming!");
        }

        private IEnumerator FailureSequence()
        {
            _currentMasteryLevel = Mathf.Max(0f, _currentMasteryLevel - 0.15f);
            EventBus.OnGrowthProgressChanged?.Invoke(_currentMasteryLevel);

            yield return HighlightSequence();

            if (gardenAnimator != null) gardenAnimator.SetTrigger("Mist");

            yield return new WaitForSeconds(1f);
            Debug.Log("[PatternFlower] Magical rain helps the child learn.");
        }

        private IEnumerator HighlightSequence()
        {
            foreach (var flower in _displayedFlowers)
            {
                if (!flower.IsMystery)
                {
                    flower.Pulse();
                    yield return new WaitForSeconds(0.4f);
                }
            }
        }

        private void HandleIntervention(InterventionType type)
        {
            if (type == InterventionType.ShowDemo) StartCoroutine(HighlightSequence());
        }

        private void ClearGarden()
        {
            foreach (var f in _displayedFlowers) if (f != null) Destroy(f.gameObject);
            _displayedFlowers.Clear();

            foreach (var f in _choiceFlowers)
            {
                if (f != null)
                {
                    f.OnSelected -= HandleFlowerSelected;
                    Destroy(f.gameObject);
                }
            }
            _choiceFlowers.Clear();
        }
    }
}
