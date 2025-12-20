using System;
using UnityEngine;
using System.Collections;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Architecture.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("State")]
        public GameState CurrentState;
        public MathProblemSO CurrentProblem;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Initialize Systems
            ChangeState(GameState.MainMenu);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            EventBus.OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] State: {newState}");
        }

        // Called by UI to start a session
        public void StartSession(MathProblemSO[] problemSet)
        {
            StartCoroutine(SessionRoutine(problemSet));
        }

        private IEnumerator SessionRoutine(MathProblemSO[] problems)
        {
            foreach (var problem in problems)
            {
                CurrentProblem = problem;
                
                // 1. Instruction phase
                // check if we need to show a demo based on difficulty
                
                // 2. Play Phase
                ChangeState(GameState.Gameplay);
                EventBus.OnProblemStarted?.Invoke(problem.questionId);

                // Wait for answer (handled by InteractionController -> EventBus)
                bool answered = false;
                Action<bool, float> handler = (correct, time) => answered = true;
                EventBus.OnAnswerAttempted += handler;

                yield return new WaitUntil(() => answered);
                EventBus.OnAnswerAttempted -= handler;

                // 3. Feedback Phase
                ChangeState(GameState.Feedback); // Now matches EventBus enum
                yield return new WaitForSeconds(2.0f); // Wait for explanation audio
            }
            
            Debug.Log("Session Complete");
            ChangeState(GameState.MainMenu);
        }
    }
}
