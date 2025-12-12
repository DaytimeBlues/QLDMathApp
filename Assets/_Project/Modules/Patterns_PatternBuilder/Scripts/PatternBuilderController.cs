using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Patterns
{
    /// <summary>
    /// PATTERN BUILDER: Pattern recognition game (AC9M1A01 - Algebra).
    /// "What comes next in the pattern?"
    /// Child identifies and extends repeating patterns (ABAB, AABB, ABC).
    /// </summary>
    public class PatternBuilderController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform patternDisplayArea;
        [SerializeField] private Transform choiceArea;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Animator characterAnimator;

        [Header("Prefabs")]
        [SerializeField] private PatternPiece piecePrefab;

        [Header("Pattern Types")]
        [SerializeField] private Sprite[] shapeSprites; // Circle, Square, Triangle, Star
        [SerializeField] private Color[] patternColors; // Red, Blue, Yellow, Green

        private List<PatternPiece> _displayedPattern = new List<PatternPiece>();
        private List<PatternPiece> _choicePieces = new List<PatternPiece>();
        private int _correctAnswerIndex;
        private PatternType _currentPatternType;
        private float _roundStartTime;

        public enum PatternType
        {
            AB,     // Red, Blue, Red, Blue, ?
            ABB,    // Red, Blue, Blue, Red, Blue, Blue, ?
            ABC,    // Red, Blue, Yellow, Red, Blue, Yellow, ?
            AABB    // Red, Red, Blue, Blue, Red, Red, ?
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
            ClearPattern();
            
            // Generate pattern based on type
            int[] pattern = GeneratePattern(patternType, problem.correctValue);
            
            // Display pattern with last element hidden
            DisplayPattern(pattern);
            
            // Create choice buttons
            CreateChoices(pattern[pattern.Length - 1]);
            
            // Play instruction
            if (problem.questionAudio != null)
            {
                audioSource.PlayOneShot(problem.questionAudio);
            }
            
            _roundStartTime = Time.time;
        }

        private int[] GeneratePattern(PatternType type, int seed)
        {
            switch (type)
            {
                case PatternType.AB:
                    return new int[] { 0, 1, 0, 1, 0, 1, 0 }; // Last is 1
                case PatternType.ABB:
                    return new int[] { 0, 1, 1, 0, 1, 1, 0 }; // Last is 1
                case PatternType.ABC:
                    return new int[] { 0, 1, 2, 0, 1, 2, 0 }; // Last is 1
                case PatternType.AABB:
                    return new int[] { 0, 0, 1, 1, 0, 0, 1 }; // Last is 1
                default:
                    return new int[] { 0, 1, 0, 1, 0 };
            }
        }

        private void DisplayPattern(int[] pattern)
        {
            float spacing = 80f;
            float startX = -(pattern.Length - 2) * spacing / 2f; // -1 for hidden piece

            for (int i = 0; i < pattern.Length - 1; i++) // Don't show last
            {
                PatternPiece piece = Instantiate(piecePrefab, patternDisplayArea);
                piece.SetupDisplay(
                    shapeSprites[0], // Use same shape, different colors
                    patternColors[pattern[i]],
                    i
                );
                piece.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(startX + i * spacing, 0);
                _displayedPattern.Add(piece);
            }

            // Add mystery slot at the end
            PatternPiece mysterySlot = Instantiate(piecePrefab, patternDisplayArea);
            mysterySlot.SetupMystery();
            mysterySlot.GetComponent<RectTransform>().anchoredPosition = 
                new Vector2(startX + (pattern.Length - 1) * spacing, 0);
            _displayedPattern.Add(mysterySlot);

            _correctAnswerIndex = pattern[pattern.Length - 1];
        }

        private void CreateChoices(int correctIndex)
        {
            // Create 3 choices (correct + 2 distractors)
            List<int> choices = new List<int> { correctIndex };
            
            // Add distractors
            for (int i = 0; i < patternColors.Length && choices.Count < 3; i++)
            {
                if (i != correctIndex)
                {
                    choices.Add(i);
                }
            }

            // Shuffle
            for (int i = choices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = choices[i];
                choices[i] = choices[j];
                choices[j] = temp;
            }

            float spacing = 100f;
            float startX = -(choices.Count - 1) * spacing / 2f;

            for (int i = 0; i < choices.Count; i++)
            {
                PatternPiece piece = Instantiate(piecePrefab, choiceArea);
                piece.SetupChoice(
                    shapeSprites[0],
                    patternColors[choices[i]],
                    choices[i]
                );
                piece.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(startX + i * spacing, 0);
                piece.OnSelected += HandleChoiceSelected;
                _choicePieces.Add(piece);
            }
        }

        private void HandleChoiceSelected(PatternPiece piece)
        {
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            bool isCorrect = piece.ValueIndex == _correctAnswerIndex;

            // Disable all choices
            foreach (var p in _choicePieces)
            {
                p.SetInteractable(false);
            }

            EventBus.OnAnswerAttempted?.Invoke(isCorrect, responseTime);

            if (isCorrect)
            {
                StartCoroutine(CorrectSequence(piece));
            }
            else
            {
                StartCoroutine(IncorrectSequence());
            }
        }

        private IEnumerator CorrectSequence(PatternPiece selectedPiece)
        {
            EventBus.OnPlaySuccessFeedback?.Invoke();

            // Move selected piece to mystery slot
            PatternPiece mysterySlot = _displayedPattern[_displayedPattern.Count - 1];
            Vector3 targetPos = mysterySlot.transform.position;

            float duration = 0.5f;
            Vector3 startPos = selectedPiece.transform.position;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                selectedPiece.transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
                yield return null;
            }

            // Replace mystery with actual piece
            mysterySlot.RevealAs(shapeSprites[0], patternColors[_correctAnswerIndex]);

            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Celebrate");
            }

            yield return new WaitForSeconds(1.5f);
            Debug.Log("[PatternBuilder] Round Complete - Correct!");
        }

        private IEnumerator IncorrectSequence()
        {
            EventBus.OnPlayCorrectionFeedback?.Invoke();

            // EXPLANATORY FEEDBACK: Highlight the pattern rhythm
            yield return HighlightPatternSequence();

            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Think");
            }

            yield return new WaitForSeconds(1f);
            Debug.Log("[PatternBuilder] Round Complete - Scaffolded.");
        }

        private IEnumerator HighlightPatternSequence()
        {
            // Pulse through the pattern to show the rhythm
            foreach (var piece in _displayedPattern)
            {
                if (!piece.IsMystery)
                {
                    piece.Pulse();
                    yield return new WaitForSeconds(0.4f);
                }
            }
        }

        private void HandleIntervention(InterventionType type)
        {
            if (type == InterventionType.ShowDemo)
            {
                StartCoroutine(HighlightPatternSequence());
            }
        }

        private void ClearPattern()
        {
            foreach (var piece in _displayedPattern)
            {
                if (piece != null) Destroy(piece.gameObject);
            }
            _displayedPattern.Clear();

            foreach (var piece in _choicePieces)
            {
                if (piece != null)
                {
                    piece.OnSelected -= HandleChoiceSelected;
                    Destroy(piece.gameObject);
                }
            }
            _choicePieces.Clear();
        }
    }
}
