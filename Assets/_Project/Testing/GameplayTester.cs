using UnityEngine;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Managers;

namespace QLDMathApp.Testing
{
    /// <summary>
    /// TEST UTILITY: Simulates gameplay for testing without full UI.
    /// Attach to any GameObject and press T to simulate correct/incorrect answers.
    /// </summary>
    public class GameplayTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private MathProblemSO[] testProblems;
        
        [Header("Simulation")]
        [SerializeField] private KeyCode correctAnswerKey = KeyCode.Y;
        [SerializeField] private KeyCode incorrectAnswerKey = KeyCode.N;
        [SerializeField] private KeyCode startSessionKey = KeyCode.S;
        [SerializeField] private KeyCode levelUpKey = KeyCode.U;
        [SerializeField] private KeyCode scaffoldKey = KeyCode.D;

        private void Update()
        {
            if (!enableTesting) return;
            
            // Start a test session
            if (Input.GetKeyDown(startSessionKey) && testProblems.Length > 0)
            {
                Debug.Log("[TESTER] Starting test session...");
                GameManager.Instance?.StartSession(testProblems);
            }
            
            // Simulate correct answer
            if (Input.GetKeyDown(correctAnswerKey))
            {
                float mockTime = Random.Range(500f, 1500f);
                Debug.Log($"[TESTER] Simulating CORRECT answer (time: {mockTime}ms)");
                EventBus.OnAnswerAttempted?.Invoke(true, mockTime);
            }
            
            // Simulate incorrect answer
            if (Input.GetKeyDown(incorrectAnswerKey))
            {
                float mockTime = Random.Range(2000f, 4000f);
                Debug.Log($"[TESTER] Simulating INCORRECT answer (time: {mockTime}ms)");
                EventBus.OnAnswerAttempted?.Invoke(false, mockTime);
            }
            
            // Force level up
            if (Input.GetKeyDown(levelUpKey))
            {
                Debug.Log("[TESTER] Forcing LEVEL UP intervention");
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.LevelUp);
            }
            
            // Force scaffolding
            if (Input.GetKeyDown(scaffoldKey))
            {
                Debug.Log("[TESTER] Forcing SCAFFOLD intervention");
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.ShowDemo);
            }
        }

        private void OnGUI()
        {
            if (!enableTesting) return;
            
            int y = 10;
            int lineHeight = 25;
            
            GUI.color = Color.white;
            GUI.Label(new Rect(10, y, 400, 20), "=== QLD MATH APP TESTER ===");
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, 20), $"[S] Start Session with {testProblems.Length} problems");
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, 20), "[Y] Simulate Correct Answer");
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, 20), "[N] Simulate Incorrect Answer");
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, 20), "[U] Force Level Up");
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 400, 20), "[D] Force Scaffold Down");
            y += lineHeight * 2;
            
            // Show current state
            if (GameManager.Instance != null)
            {
                GUI.color = Color.cyan;
                GUI.Label(new Rect(10, y, 400, 20), $"State: {GameManager.Instance.CurrentState}");
                y += lineHeight;
                
                if (GameManager.Instance.CurrentProblem != null)
                {
                    GUI.Label(new Rect(10, y, 400, 20), 
                        $"Problem: {GameManager.Instance.CurrentProblem.questionId} (Answer: {GameManager.Instance.CurrentProblem.correctValue})");
                }
            }
        }
    }
}
