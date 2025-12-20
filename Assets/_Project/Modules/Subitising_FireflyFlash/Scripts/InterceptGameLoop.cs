using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QLDMathApp.Architecture.Audio;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Modules.Subitising; // Correct Namespace
using UnityEngine.SceneManagement;

using UnityEngine.Serialization;
using QLDMathApp.Architecture.UI;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// INTERCEPT GAME LOOP: Standard pilot mission loop for Angel subitising.
    /// handles phase transitions from visualization to countermeasure.
    /// </summary>
    public class InterceptGameLoop : MonoBehaviour // Renamed from FireflyGameLoop
    {
        [Header("NERV Theme")]
        [SerializeField] private NERVTheme theme;

        [Header("Tactical References")]
        [SerializeField, FormerlySerializedAs("spawner")] private AngelSpawner angelSpawner;
        [SerializeField, FormerlySerializedAs("inputButtonsCanvas")] private CanvasGroup countermeasureCanvas; 
        [SerializeField, FormerlySerializedAs("numberButtons")] private Button[] terminalButtons; 
        [SerializeField, FormerlySerializedAs("repeatAudioButton")] private Button replayMagiButton;
        [SerializeField, FormerlySerializedAs("readinessButton")] private Button missionStartButton; 
        
        [Header("Mission Assets")]
        [SerializeField, FormerlySerializedAs("correctParticles")] private ParticleSystem syncSuccessEffect;
        [SerializeField, FormerlySerializedAs("visualFeedbackContainer")] private GameObject magiAnalysisContainer; 

        // State
        private MathProblemSO currentProblem;
        private int currentRound = 0;
        private const int TOTAL_ROUNDS = 5;

        private void Start()
        {
            // Initial setup
            if (countermeasureCanvas != null)
            {
                countermeasureCanvas.alpha = 0;
                countermeasureCanvas.interactable = false;
            }
            else
            {
                Debug.LogWarning("[NERV] countermeasureCanvas (formerly inputButtonsCanvas) is unassigned in the Inspector!");
            }
            
            if (missionStartButton != null)
            {
                missionStartButton.gameObject.SetActive(true);
                missionStartButton.onClick.AddListener(StartMission);
            }
            
            // Wire up terminal buttons
            for (int i = 0; i < terminalButtons.Length; i++)
            {
                int val = i + 1;
                terminalButtons[i].onClick.AddListener(() => OnTerminalSelection(val));
            }
            
            // Replay button
            if (replayMagiButton != null)
                replayMagiButton.onClick.AddListener(PlayCurrentMissionObjective);
        }

        public void StartMission()
        {
            StartCoroutine(MissionRoutine());
        }

        private IEnumerator MissionRoutine()
        {
            currentRound++;
            if (missionStartButton != null) missionStartButton.gameObject.SetActive(false); 

            // 1. Setup Phase
            float scanTime = 1.5f;
            int angelCount = Random.Range(1, 6); 
            
            Debug.Log($"[NERV] PATTERN BLUE: {angelCount} Angels visualized.");
            angelSpawner.SpawnFireflies(angelCount); 
            
            // 2. Alert Phase
            yield return new WaitForSeconds(1.0f);

            // 3. Visualization Phase
            yield return new WaitForSeconds(scanTime);

            // 4. Interference Phase
            angelSpawner.HideFireflies(); 
            
            // 5. Countermeasure Phase (Show Terminal)
            if (countermeasureCanvas != null)
            {
                countermeasureCanvas.alpha = 1;
                countermeasureCanvas.interactable = true;
            }
            
            Debug.Log("[NERV] Terminal active. Pilot, input Angel count.");
        }

        private void OnTerminalSelection(int value)
        {
            if (countermeasureCanvas != null) countermeasureCanvas.interactable = false;

            if (value == angelSpawner.CurrentCount) // Correct
            {
                StartCoroutine(SyncSuccessRoutine());
            }
            else // Incorrect
            {
                StartCoroutine(MagiAnalysisRoutine(value));
            }
        }

        private IEnumerator SyncSuccessRoutine()
        {
            if (syncSuccessEffect != null) syncSuccessEffect.Play();
            
            Debug.Log("[NERV] Sync successful. Target neutralized.");
            EventBus.OnAnswerAttempted?.Invoke(true, 1000f); // Default time for now
            
            yield return new WaitForSeconds(2.0f);
            
            if (currentRound < TOTAL_ROUNDS)
            {
                PrepareNextMission();
            }
            else
            {
                ConcludeOperation();
            }
        }

        private IEnumerator MagiAnalysisRoutine(int guess)
        {
            // 1. Reveal (No buzzer)
            angelSpawner.ShowFireflies();
            
            Debug.Log("[NERV] Countermeasure failed. Initiating MAGI analysis.");
            EventBus.OnSyncRateChanged?.Invoke(Mathf.Max(0f, (currentRound / (float)TOTAL_ROUNDS) - 0.2f));
            
            yield return new WaitForSeconds(3.0f); 
            
            PrepareNextMission();
        }

        private void PrepareNextMission()
        {
            if (countermeasureCanvas != null)
            {
                countermeasureCanvas.alpha = 0;
                countermeasureCanvas.interactable = false;
            }
            if (missionStartButton != null) missionStartButton.gameObject.SetActive(true);
        }

        private void ConcludeOperation()
        {
            SceneManager.LoadScene("HubMap", LoadSceneMode.Single);
        }

        private void PlayCurrentMissionObjective()
        {
            // Audio replay logic
        }
    }
}
