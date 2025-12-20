using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QLDMathApp.Architecture.Audio;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Modules.Subitising;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// FIREFLY GAME LOOP: Standard forest game loop for firefly counting.
    /// Handles phase transitions from visualization to answer selection.
    /// </summary>
    public class FireflyGameLoop : MonoBehaviour
    {
        [Header("Forest Theme")]
        [SerializeField] private ForestTheme theme; // TODO: Replace with ForestTheme

        [Header("Game References")]
        [SerializeField] private FireflySpawner fireflySpawner;
        [SerializeField] private CanvasGroup answerButtonsCanvas; 
        [SerializeField] private Button[] numberButtons; 
        [SerializeField] private Button replayAudioButton;
        [SerializeField] private Button gameStartButton; 
        
        [Header("Game Assets")]
        [SerializeField] private ParticleSystem successEffect;
        [SerializeField] private GameObject guideFeedbackContainer; 

        // State
        private MathProblemSO currentProblem;
        private int currentRound = 0;
        private const int TOTAL_ROUNDS = 5;
        private int _targetFireflyCount;

        private void Start()
        {
            // Initial setup
            if (answerButtonsCanvas != null)
            {
                answerButtonsCanvas.alpha = 0;
                answerButtonsCanvas.interactable = false;
            }
            
            if (gameStartButton != null)
            {
                gameStartButton.gameObject.SetActive(true);
                gameStartButton.onClick.AddListener(StartGame);
            }
            
            // Wire up number buttons
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int val = i + 1;
                numberButtons[i].onClick.AddListener(() => OnNumberSelected(val));
            }
            
            // Replay button
            if (replayAudioButton != null)
                replayAudioButton.onClick.AddListener(PlayCurrentGameInstruction);
        }

        public void StartGame()
        {
            StartCoroutine(GameRoutine());
        }

        private IEnumerator GameRoutine()
        {
            currentRound++;
            if (gameStartButton != null) gameStartButton.gameObject.SetActive(false); 

            // 1. Setup Phase
            float lookTime = 1.5f;
            _targetFireflyCount = Random.Range(1, 6); 
            
            Debug.Log($"[Forest] Game Round {currentRound}: {_targetFireflyCount} fireflies!");
            fireflySpawner.SpawnFireflies(_targetFireflyCount); 
            
            // 2. Alert Phase
            yield return new WaitForSeconds(1.0f);

            // 3. Visualization Phase
            yield return new WaitForSeconds(lookTime);

            // 4. Hide Phase
            fireflySpawner.HideFireflies(); 
            
            // 5. Answer Phase (Show Buttons)
            if (answerButtonsCanvas != null)
            {
                answerButtonsCanvas.alpha = 1;
                answerButtonsCanvas.interactable = true;
            }
            
            Debug.Log("[Forest] How many fireflies did you see?");
        }

        private void OnNumberSelected(int value)
        {
            if (answerButtonsCanvas != null) answerButtonsCanvas.interactable = false;

            if (value == fireflySpawner.CurrentCount) // Correct
            {
                StartCoroutine(SuccessRoutine());
            }
            else // Incorrect
            {
                StartCoroutine(GuideFeedbackRoutine(value));
            }
        }

        private IEnumerator SuccessRoutine()
        {
            if (successEffect != null) successEffect.Play();
            
            Debug.Log("[Forest] Correct! Well done!");
            EventBus.OnAnswerAttempted?.Invoke(true, 1000f);
            
            yield return new WaitForSeconds(2.0f);
            
            if (currentRound < TOTAL_ROUNDS)
            {
                PrepareNextRound();
            }
            else
            {
                EndGame();
            }
        }

        private IEnumerator GuideFeedbackRoutine(int guess)
        {
            // 1. Reveal (No buzzer)
            fireflySpawner.ShowFireflies();
            
            Debug.Log($"[Forest] Let's count together! There are {fireflySpawner.CurrentCount} fireflies.");
            EventBus.OnGrowthProgressChanged?.Invoke(Mathf.Max(0f, (currentRound / (float)TOTAL_ROUNDS) - 0.2f));
            
            // Show counting sequence
            yield return StartCoroutine(fireflySpawner.AnimateCountingSequence());
            
            yield return new WaitForSeconds(1.0f);
            
            PrepareNextRound();
        }

        private void PrepareNextRound()
        {
            if (answerButtonsCanvas != null)
            {
                answerButtonsCanvas.alpha = 0;
                answerButtonsCanvas.interactable = false;
            }
            if (gameStartButton != null) gameStartButton.gameObject.SetActive(true);
        }

        private void EndGame()
        {
            SceneManager.LoadScene("HubMap", LoadSceneMode.Single);
        }

        private void PlayCurrentGameInstruction()
        {
            // Audio replay logic
        }
    }
}
