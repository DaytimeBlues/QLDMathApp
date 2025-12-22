# QLDMathApp Audit Pack [Part 1: Core Architecture & Logic]

This document contains the foundational logic and architectural decisions for the **QLDMathApp** project, an educational Unity application re-themed with an Evangelion (NERV) aesthetic.

## 1. Event-Driven Architecture (EventBus.cs)
**Decision**: Use a static `EventBus` to decouple gameplay modules from core systems.
**Rationale**: In an educational app, we need various systems (Feedback, Analytics, Progress) to listen to the same gameplay events without tight coupling.

```csharp
// Path: Assets/_Project/Architecture/Events/EventBus.cs
using System;
using UnityEngine;

namespace QLDMathApp.Architecture.Events
{
    public static class EventBus
    {
        #region Game State Events
        public static Action<GameState> OnGameStateChanged;
        #endregion

        #region Gameplay Events
        public static Action<string> OnProblemStarted;
        public static Action<bool, float> OnAnswerAttempted;
        public static Action<InterventionType> OnInterventionTriggered;
        #endregion

        #region Evangelion Thermal Events
        public static Action<float> OnSyncRateChanged; // 0.0 to 1.0 (Sync Ratio)
        #endregion

        #region Magi System Events
        public static Action<MagiPersonality, string> OnMagiConsulted;
        #endregion
    }

    public enum MagiPersonality { Melchior, Balthasar, Casper }
    public enum GameState { MainMenu, Instruction, Gameplay, Feedback, Paused }
    public enum InterventionType { LevelUp, ScaffoldDown, ShowDemo }
}
```

## 2. Adaptive Difficulty (SyncRatioManager.cs)
**Decision**: Calculate a "Sync Ratio" (Mastery metric) instead of a simple score.
**Rationale**: To provide an adaptive experience for Year 1 students, we track both correctness and response time.

```csharp
// Path: Assets/_Project/Architecture/Managers/SyncRatioManager.cs
using UnityEngine;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Architecture.Managers
{
    public class SyncRatioManager : MonoBehaviour
    {
        [SerializeField] private float baseSyncRate = 0.4f;
        [SerializeField] private float increaseRate = 0.05f;
        [SerializeField] private float decreaseRate = 0.1f;
        [SerializeField] private float speedBonusThreshold = 2000f;
        
        private float _currentSyncRate;
        
        private void Awake() { _currentSyncRate = baseSyncRate; }
        private void OnEnable() { EventBus.OnAnswerAttempted += HandleAnswerAttempted; }
        private void OnDisable() { EventBus.OnAnswerAttempted -= HandleAnswerAttempted; }
        
        private void HandleAnswerAttempted(bool isCorrect, float responseTime)
        {
            if (isCorrect)
            {
                float bonus = responseTime < speedBonusThreshold ? 0.02f : 0f;
                _currentSyncRate = Mathf.Clamp01(_currentSyncRate + increaseRate + bonus);
            }
            else
            {
                _currentSyncRate = Mathf.Clamp01(_currentSyncRate - decreaseRate);
            }
            
            EventBus.OnSyncRateChanged?.Invoke(_currentSyncRate);
            
            // Trigger Intervention Thresholds
            if (_currentSyncRate > 0.85f)
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.LevelUp);
            else if (_currentSyncRate < 0.25f)
                EventBus.OnInterventionTriggered?.Invoke(InterventionType.ScaffoldDown);
        }
    }
}
```

## 3. Tripartite Feedback (MagiSystem.cs)
**Decision**: Implement the "MAGI" logic for pedagogical feedback.
**Rationale**: Provides multiple "voices" (Rational, Nurturing, Intuitive) to guide the student, moving beyond binary success/failure.

```csharp
// Path: Assets/_Project/Modules/Magi/Scripts/MagiSystem.cs
using UnityEngine;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Magi
{
    public class MagiSystem : MonoBehaviour
    {
        private void OnEnable() { EventBus.OnAnswerAttempted += ConsultMagi; }
        private void OnDisable() { EventBus.OnAnswerAttempted -= ConsultMagi; }
        
        private void ConsultMagi(bool isCorrect, float responseTime)
        {
            if (isCorrect)
            {
                if (responseTime < 1500f)
                    EventBus.OnMagiConsulted?.Invoke(MagiPersonality.Melchior, "Target neutralized. Reaction time within optimal parameters.");
                else
                    EventBus.OnMagiConsulted?.Invoke(MagiPersonality.Balthasar, "Good job, pilot. You identified the threat safely.");
            }
            else
            {
                EventBus.OnMagiConsulted?.Invoke(MagiPersonality.Casper, "Pattern unrecognized. Casper suggests re-scanning the central cluster.");
                EventBus.OnMagiConsulted?.Invoke(MagiPersonality.Melchior, "Error in subitising logic. Recalibrating visual sensors.");
            }
        }
    }
}
```

## 4. Initialization (AppBootstrapper.cs)
**Decision**: Use a "Compose Root" in a dedicated assembly (`Bootstrap`) to link systems.
**Rationale**: Prevents circular dependencies between `Architecture` (Events) and `Modules` (Gameplay Logic).

```csharp
// Path: Assets/_Project/Bootstrap/AppBootstrapper.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using QLDMathApp.Architecture.Audio; // Circular Dep avoidance via new ASMDEF

namespace QLDMathApp.Bootstrap
{
    public class AppBootstrapper : MonoBehaviour
    {
        private static bool _booted;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private IEnumerator Start()
        {
            if (_booted) yield break;
            _booted = true;
            DontDestroyOnLoad(gameObject);

            // Init Dynamic Systems
            if (Object.FindFirstObjectByType<QLDMathApp.Architecture.Managers.SyncRatioManager>() == null)
            {
                var go = new GameObject("NERV_SyncRatioManager");
                DontDestroyOnLoad(go);
                go.AddComponent<QLDMathApp.Architecture.Managers.SyncRatioManager>();
            }

            if (Object.FindFirstObjectByType<QLDMathApp.Modules.Magi.MagiSystem>() == null)
            {
                var go = new GameObject("MAGI_System");
                DontDestroyOnLoad(go);
                go.AddComponent<QLDMathApp.Modules.Magi.MagiSystem>();
            }

            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single); 
        }
    }
}
```
