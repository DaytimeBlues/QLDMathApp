using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QLDMathApp.Architecture.Audio;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using UnityEngine.SceneManagement;

namespace QLDMathApp.Modules.FireflyFlash
{
    public class FireflyGameLoop : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private FireflySpawner spawner;
        [SerializeField] private CanvasGroup inputButtonsCanvas; // For hiding/locking
        [SerializeField] private Button[] numberButtons; // 1-5
        [SerializeField] private Button repeatAudioButton;
        [SerializeField] private Button readinessButton; // "Ready?"
        
        [Header("Feedback Config")]
        [SerializeField] private ParticleSystem correctParticles;
        [SerializeField] private GameObject visualFeedbackContainer; // For "Show & Tell"

        // State
        private MathProblemSO currentProblem;
        private int currentRound = 0;
        private const int TOTAL_ROUNDS = 5;

        private void Start()
        {
            // Initial setup
            inputButtonsCanvas.alpha = 0;
            inputButtonsCanvas.interactable = false;
            readinessButton.gameObject.SetActive(true);
            readinessButton.onClick.AddListener(StartRound);
            
            // Wire up number buttons
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int val = i + 1;
                numberButtons[i].onClick.AddListener(() => OnNumberSelected(val));
            }
            
            // Repeat button
            repeatAudioButton.onClick.AddListener(PlayCurrentInstruction);

            // Load first problem (Mock for Slice)
            // In real app, get from Registry
        }

        public void StartRound()
        {
            StartCoroutine(RoundRoutine());
        }

        private IEnumerator RoundRoutine()
        {
            currentRound++;
            readinessButton.gameObject.SetActive(false); // Hide ready button

            // 1. Setup Phase
            // TODO: Load from Registry
            // currentProblem = ContentRegistry.Instance.GetProblem(...)
            
            // For Vertical Slice: Hardcoded mock
            float flashTime = 2.0f;
            int answer = Random.Range(1, 4); // 1-3 for subitising
            
            spawner.SpawnFireflies(answer); // Show dots
            
            // 2. Pre-Flash Instruction
            // VO: "Get ready to look!"
            // AudioQueueService.Instance.Enqueue(preFlashClip);
            yield return new WaitForSeconds(1.0f);

            // 3. Flash Phase (Stimulus Visible)
            // Input locked.
            yield return new WaitForSeconds(flashTime);

            // 4. mask Phase (Hide Stimulus)
            spawner.HideFireflies(); 
            
            // 5. Input Phase (Show Buttons)
            inputButtonsCanvas.alpha = 1;
            inputButtonsCanvas.interactable = true;
            
            // VO: "How many?"
            // AudioQueueService.Instance.Enqueue(questionClip);
        }

        private void OnNumberSelected(int value)
        {
            // Lock input to prevent double tapping
            inputButtonsCanvas.interactable = false;

            if (value == spawner.CurrentCount) // Correct
            {
                StartCoroutine(CorrectRoutine());
            }
            else // Incorrect
            {
                StartCoroutine(IncorrectRoutine(value));
            }
        }

        private IEnumerator CorrectRoutine()
        {
            // Visuals
            correctParticles.Play();
            
            // Audio
            // VO: "That's right! Five!"
            // AudioQueueService.Instance.Enqueue(correctClip);
            
            yield return new WaitForSeconds(2.0f);
            
            if (currentRound < TOTAL_ROUNDS)
            {
                ResetRound();
            }
            else
            {
                ReturnToMenu();
            }
        }

        private IEnumerator IncorrectRoutine(int guess)
        {
            // 1. Reveal (Nobuzzer)
            spawner.ShowFireflies();
            
            // 2. Count active ingredient
            // VO: "Let's count together... One... Two..."
            
            yield return new WaitForSeconds(3.0f); // Fake duration
            
            // Retry same round? Or advance?
            // Deep Research says: "Correction through demonstration", then reset
            ResetRound();
        }

        private void ResetRound()
        {
            inputButtonsCanvas.alpha = 0;
            inputButtonsCanvas.interactable = false;
            readinessButton.gameObject.SetActive(true);
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private void PlayCurrentInstruction()
        {
            // AudioQueueService.Instance.Enqueuepriority(currentProblem.instructionAudio);
        }
    }
}
