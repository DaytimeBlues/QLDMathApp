using System;
using UnityEngine;
using System.Collections;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Events;
using QLDMathApp.UI;

namespace QLDMathApp.Architecture.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("State")]
        public GameState CurrentState;
        public MathProblemSO CurrentProblem;

        [Header("UI Navigation")]
        [SerializeField] private UIViewStack viewStack;
        [SerializeField] private LandingPageView landingPageView;
        [SerializeField] private MapView mapView;

        private string _selectedDomain;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            // Subscribe to domain selection event
            EventBus.OnDomainSelected += HandleDomainSelected;
        }

        private void OnDisable()
        {
            // Unsubscribe from domain selection event
            EventBus.OnDomainSelected -= HandleDomainSelected;
        }

        private void Start()
        {
            // Initialize Systems
            ChangeState(GameState.MainMenu);
            
            // Show landing page on start
            ShowLandingPage();
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            EventBus.OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] State: {newState}");
        }

        /// <summary>
        /// Show the landing page view.
        /// </summary>
        public void ShowLandingPage()
        {
            if (viewStack == null)
            {
                Debug.LogError("[GameManager] UIViewStack not assigned.");
                return;
            }

            if (landingPageView == null)
            {
                Debug.LogError("[GameManager] LandingPageView not assigned.");
                return;
            }

            // Clear the stack and show landing page
            viewStack.Clear();
            viewStack.Push(landingPageView);
            
            Debug.Log("[GameManager] Showing landing page.");
        }

        /// <summary>
        /// Handle domain selection event from LandingPage.
        /// </summary>
        private void HandleDomainSelected(string domain)
        {
            _selectedDomain = domain;
            Debug.Log($"[GameManager] Domain selected: {domain}");

            // Navigate to map view with domain filter
            ShowMapView(domain);
        }

        /// <summary>
        /// Show the map view with the specified domain filter.
        /// </summary>
        private void ShowMapView(string domain)
        {
            if (viewStack == null)
            {
                Debug.LogError("[GameManager] UIViewStack not assigned.");
                return;
            }

            if (mapView == null)
            {
                Debug.LogError("[GameManager] MapView not assigned.");
                return;
            }

            // Push map view onto stack with domain as data
            viewStack.Push(mapView, domain);
            
            Debug.Log($"[GameManager] Showing map view for domain: {domain}");
        }

        /// <summary>
        /// Navigate back in the view stack.
        /// </summary>
        public void NavigateBack()
        {
            if (viewStack != null && viewStack.Count > 1)
            {
                viewStack.Pop();
            }
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
