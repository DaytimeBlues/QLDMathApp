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
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

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

        private void Start()
        {
            StartNewSession();
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
            if (!IsSessionActive) return;
            
            IsSessionActive = false;
            float duration = SessionDurationMinutes;
            
            // Persist stats
            int totalSessions = PlayerPrefs.GetInt("TotalSessions", 0) + 1;
            float totalMinutes = PlayerPrefs.GetFloat("TotalMinutes", 0f) + duration;
            
            // Update accuracy
            if (_problemsAttempted > 0)
            {
                float sessionAccuracy = (_problemsCorrect * 100f) / _problemsAttempted;
                float prevAccuracy = PlayerPrefs.GetFloat("OverallAccuracy", 0f);
                int prevTotal = PlayerPrefs.GetInt("TotalProblems", 0);
                
                // Weighted average
                int newTotal = prevTotal + _problemsAttempted;
                float newAccuracy = ((prevAccuracy * prevTotal) + (sessionAccuracy * _problemsAttempted)) / newTotal;
                
                PlayerPrefs.SetFloat("OverallAccuracy", newAccuracy);
                PlayerPrefs.SetInt("TotalProblems", newTotal);
            }
            
            PlayerPrefs.SetInt("TotalSessions", totalSessions);
            PlayerPrefs.SetFloat("TotalMinutes", totalMinutes);
            PlayerPrefs.Save();
            
            Debug.Log($"[SessionManager] Session ended. Duration: {duration:F1}min, Problems: {_problemsAttempted}, Correct: {_problemsCorrect}");
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
