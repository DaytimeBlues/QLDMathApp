using UnityEngine;
using System;
using System.Collections;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Architecture.Managers
{
    /// <summary>
    /// SESSION MANAGER: Tracks session-level data.
    /// - Session start/end
    /// - Total time played
    /// - Problems attempted this session
    /// Persists to PlayerPrefs on session end.
    /// </summary>
    using QLDMathApp.Architecture.Services;

    public class SessionManager : MonoBehaviour, IInitializable
    {
        public static SessionManager Instance { get; private set; }
        public bool IsInitialized { get; private set; }

        [Header("Session Data")]
        [SerializeField] private float sessionTimeoutMinutes = 15f;
        
        private DateTime _sessionStart;
        private int _problemsAttempted;
        private int _problemsCorrect;
        private float _lastInteractionTime;

        public bool IsSessionActive { get; private set; }
        public float SessionDurationMinutes => (float)(DateTime.Now - _sessionStart).TotalMinutes;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventBus.OnAnswerAttempted += OnAnswerAttempted;
            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnAnswerAttempted -= OnAnswerAttempted;
            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        public IEnumerator Initialize()
        {
            // Session manager is ready immediately as it waits for persistence service
            IsInitialized = true;
            StartNewSession();
            yield return null;
        }

        private void Start()
        {
            // Logic moved to Initialize/StartNewSession
        }

        private void Update()
        {
            // Check for session timeout
            if (IsSessionActive && Time.time - _lastInteractionTime > sessionTimeoutMinutes * 60f)
            {
                Debug.Log("[SessionManager] Session timed out due to inactivity.");
                EndSession();
            }
        }

        public void StartNewSession()
        {
            _sessionStart = DateTime.Now;
            _problemsAttempted = 0;
            _problemsCorrect = 0;
            _lastInteractionTime = Time.time;
            IsSessionActive = true;
            
            Debug.Log("[SessionManager] New session started.");
        }

        private void OnAnswerAttempted(bool correct, float time)
        {
            _lastInteractionTime = Time.time;
            _problemsAttempted++;
            if (correct) _problemsCorrect++;
        }

        private void OnGameStateChanged(GameState state)
        {
            _lastInteractionTime = Time.time;
            
            if (state == GameState.MainMenu && _problemsAttempted > 0)
            {
                // Returned to menu after playing = session complete
                EndSession();
                StartNewSession(); // Ready for next
            }
        }

        public void EndSession()
        {
            if (!IsSessionActive || PersistenceService.Instance == null) return;
            
            IsSessionActive = false;
            float duration = SessionDurationMinutes;
            
            // AUDIT FIX: Using PersistenceService instead of insecure PlayerPrefs
            var data = PersistenceService.Instance.Load<AppUserData>();
            
            data.TotalSessions++;
            data.TotalMinutes += duration;
            
            // Update accuracy
            if (_problemsAttempted > 0)
            {
                float sessionAccuracy = (_problemsCorrect * 100f) / _problemsAttempted;
                
                // Weighted average
                int newTotal = data.TotalProblems + _problemsAttempted;
                data.OverallAccuracy = ((data.OverallAccuracy * data.TotalProblems) + (sessionAccuracy * _problemsAttempted)) / newTotal;
                data.TotalProblems = newTotal;
            }
            
            PersistenceService.Instance.Save(data);
            
            Debug.Log($"[SessionManager] Session persisted to JSON. Duration: {duration:F1}min, Problems: {_problemsAttempted}, Correct: {_problemsCorrect}");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App backgrounded - save progress
                EndSession();
            }
            else
            {
                // App resumed
                StartNewSession();
            }
        }

        private void OnApplicationQuit()
        {
            EndSession();
        }
    }
}
