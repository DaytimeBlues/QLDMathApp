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
    public class SyncSequenceController : MonoBehaviour // Renamed from PatternBuilderController
    {
        [Header("NERV Theme")]
        [SerializeField] private NERVTheme theme;

        [Header("Sync References")]
        [SerializeField, FormerlySerializedAs("patternDisplayArea")] private Transform sequenceArea; 
        [SerializeField, FormerlySerializedAs("choiceArea")] private Transform waveformOptions; 
        [SerializeField] private AudioSource audioSource;
        [SerializeField, FormerlySerializedAs("characterAnimator")] private Animator technicianAnimator; 

        [Header("Prefabs")]
        [SerializeField, FormerlySerializedAs("piecePrefab")] private PatternPiece waveformPrefab; 

        [Header("Waveform Assets")]
        [SerializeField, FormerlySerializedAs("shapeSprites")] private Sprite[] syncSignals; 
        [SerializeField, FormerlySerializedAs("patternColors")] private Color[] signalColors; 

        private List<PatternPiece> _displayedWaveform = new List<PatternPiece>();
        private List<PatternPiece> _choiceWaveforms = new List<PatternPiece>();
        private int _correctSignalIndex;
        private PatternType _currentPatternType;
        private float _roundStartTime;
        private float _currentSyncIntegrity = 50.0f;

        public enum PatternType
        {
            AB,     // Sync Signal A, B, A, B, ?
            ABB,    // Signal A, B, B, A, B, B, ?
            ABC,    // Signal A, B, C, A, B, C, ?
            AABB    // Signal A, A, B, B, A, A, ?
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
            ClearWaveform();
            
            // Generate pattern based on type
            int[] pattern = GeneratePattern(patternType, problem.correctValue);
            
            // Display pattern with last element hidden
            DisplayWaveform(pattern);
            
            // Create choice buttons
            CreateWaveformChoices(pattern[pattern.Length - 1]);
            
            // Play instruction: "Stabilize the synchronization waveform!"
            if (problem.questionAudio != null)
            {
                audioSource.PlayOneShot(problem.questionAudio);
            }
            
            _roundStartTime = Time.time;
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncIntegrity / 100f);
        }

        private int[] GeneratePattern(PatternType type, int seed)
        {
            switch (type)
            {
                case PatternType.AB:
                    return new int[] { 0, 1, 0, 1, 0, 1, 0 }; 
                case PatternType.ABB:
                    return new int[] { 0, 1, 1, 0, 1, 1, 0 }; 
                case PatternType.ABC:
                    return new int[] { 0, 1, 2, 0, 1, 2, 0 }; 
                case PatternType.AABB:
                    return new int[] { 0, 0, 1, 1, 0, 0, 1 }; 
                default:
                    return new int[] { 0, 1, 0, 1, 0 };
            }
        }

        private void DisplayWaveform(int[] pattern)
        {
            float spacing = 80f;
            float startX = -(pattern.Length - 2) * spacing / 2f; 

            for (int i = 0; i < pattern.Length - 1; i++) 
            {
                PatternPiece piece = Instantiate(waveformPrefab, sequenceArea);
                piece.SetupDisplay(
                    syncSignals[0], 
                    signalColors[pattern[i]],
                    i
                );
                piece.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(startX + i * spacing, 0);
                _displayedWaveform.Add(piece);
            }

            // Add mystery slot at the end
            PatternPiece mysterySlot = Instantiate(waveformPrefab, sequenceArea);
            mysterySlot.SetupMystery();
            mysterySlot.GetComponent<RectTransform>().anchoredPosition = 
                new Vector2(startX + (pattern.Length - 1) * spacing, 0);
            _displayedWaveform.Add(mysterySlot);

            _correctSignalIndex = pattern[pattern.Length - 1];
        }

        private void CreateWaveformChoices(int correctIndex)
        {
            List<int> choices = new List<int> { correctIndex };
            
            for (int i = 0; i < signalColors.Length && choices.Count < 3; i++)
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
                PatternPiece piece = Instantiate(waveformPrefab, waveformOptions);
                piece.SetupChoice(
                    syncSignals[0],
                    signalColors[choices[i]],
                    choices[i]
                );
                piece.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(startX + i * spacing, 0);
                piece.OnSelected += HandleChoiceSelected;
                _choiceWaveforms.Add(piece);
            }
        }

        private void HandleChoiceSelected(PatternPiece piece)
        {
            float responseTime = (Time.time - _roundStartTime) * 1000f;
            bool isCorrect = piece.ValueIndex == _correctSignalIndex;

            foreach (var p in _choiceWaveforms)
            {
                p.SetInteractable(false);
            }

            EventBus.OnAnswerAttempted?.Invoke(isCorrect, responseTime);

            if (isCorrect)
            {
                StartCoroutine(SuccessSequence(piece));
            }
            else
            {
                StartCoroutine(FailureSequence());
            }
        }

        private IEnumerator SuccessSequence(PatternPiece selectedPiece)
        {
            _currentSyncIntegrity = Mathf.Min(100f, _currentSyncIntegrity + 10.0f);
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncIntegrity / 100f);
            EventBus.OnPlaySuccessFeedback?.Invoke();

            PatternPiece mysterySlot = _displayedWaveform[_displayedWaveform.Count - 1];
            Vector3 targetPos = mysterySlot.transform.position;

            float duration = 0.5f;
            Vector3 startPos = selectedPiece.transform.position;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                selectedPiece.transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
                yield return null;
            }

            mysterySlot.RevealAs(syncSignals[0], signalColors[_correctSignalIndex]);

            if (technicianAnimator != null)
            {
                technicianAnimator.SetTrigger("SyncConfirmed");
            }

            yield return new WaitForSeconds(1.5f);
            Debug.Log("[SyncSequence] Waveform Stabilized.");
        }

        private IEnumerator FailureSequence()
        {
            _currentSyncIntegrity = Mathf.Max(0f, _currentSyncIntegrity - 15.0f);
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncIntegrity / 100f);
            EventBus.OnPlayCorrectionFeedback?.Invoke();

            yield return HighlightWaveformSequence();

            if (technicianAnimator != null)
            {
                technicianAnimator.SetTrigger("SyncError");
            }

            yield return new WaitForSeconds(1f);
            Debug.Log("[SyncSequence] MAGI Intervention required.");
        }

        private IEnumerator HighlightWaveformSequence()
        {
            foreach (var piece in _displayedWaveform)
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
                StartCoroutine(HighlightWaveformSequence());
            }
        }

        private void ClearWaveform()
        {
            foreach (var piece in _displayedWaveform)
            {
                if (piece != null) Destroy(piece.gameObject);
            }
            _displayedWaveform.Clear();

            foreach (var piece in _choiceWaveforms)
            {
                if (piece != null)
                {
                    piece.OnSelected -= HandleChoiceSelected;
                    Destroy(piece.gameObject);
                }
            }
            _choiceWaveforms.Clear();
        }
    }
}
