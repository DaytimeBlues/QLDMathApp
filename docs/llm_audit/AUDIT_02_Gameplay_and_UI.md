# QLDMathApp Audit Pack [Part 2: Gameplay & UI System]

This document covers the implementation of the core gameplay loop ("Angel Intercept") and the visual feedback interface.

## 1. Mission Controller (AngelInterceptController.cs)
**Decision**: Centralize the round-based flow in a single controller that reacts to the `EventBus`.
**Rationale**: By listening to `OnProblemStarted`, the controller stays decoupled from the `GameManager` (which handles curriculum sequencing). It also dynamically scales difficulty by adjusting `_currentScanTime` based on the player's Sync Ratio.

```csharp
// Path: Assets/_Project/Modules/Subitising_FireflyFlash/Scripts/AngelInterceptController.cs
using UnityEngine;
using QLDMathApp.Architecture.Events;
using System.Collections;

namespace QLDMathApp.Modules.Subitising
{
    public class AngelInterceptController : MonoBehaviour
    {
        [Header("Timing (Sync Adaptation)")]
        [SerializeField] private float baseScanTime = 1.2f;
        [SerializeField] private float minScanTime = 0.6f;
        [SerializeField] private float maxScanTime = 2.0f;
        
        private float _currentScanTime;
        private MathProblemSO _currentProblem;
        
        private void OnEnable()
        {
            EventBus.OnSyncRateChanged += HandleSyncRateChanged;
            EventBus.OnProblemStarted += HandleProblemStarted;
        }

        private IEnumerator InterceptSequence()
        {
            // 1. SCAN PHASE: Display "Angels" (Numbers)
            angelSpawner.SpawnFireflies(_currentProblem.correctValue);
            interceptionFieldGroup.alpha = 1f;
            
            yield return new WaitForSeconds(_currentScanTime); // ADAPTIVE DELAY
            
            // 2. CLOAK: Hide Angels
            angelSpawner.HideFireflies();
            interceptionFieldGroup.alpha = 0.2f;
            
            // 3. INPUT PHASE: Enable terminal buttons
            terminalGroup.SetupButtons(_currentProblem.correctValue, _currentProblem.distractorValues.ToArray());
        }

        private void HandleSyncRateChanged(float rate)
        {
            // Linear difficulty scaling: higher sync = faster scan
            _currentScanTime = Mathf.Lerp(maxScanTime, minScanTime, rate);
        }

        private void HandleProblemStarted(string questionId)
        {
            var problem = GameManager.Instance?.CurrentProblem;
            if (problem != null && problem.questionId == questionId)
            {
                StartRound(problem);
            }
        }
    }
}
```

## 2. NERV Tactical HUD (MagiDisplay.cs)
**Decision**: Use a "Typing" animation and color-coded personalities for feedback.
**Rationale**: To create a "visually striking" and premium feel, we avoid static text. The `MagiDisplay` provides a dynamic, high-tech interface that reinforces the NERV theme.

```csharp
// Path: Assets/_Project/Modules/Magi/Scripts/MagiDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.Architecture.Events;
using System.Collections;

namespace QLDMathApp.Modules.Magi
{
    public class MagiDisplay : MonoBehaviour
    {
        [SerializeField] private Text personalityLabel;
        [SerializeField] private Text messageText;
        [SerializeField] private CanvasGroup displayGroup;

        private void OnEnable() { EventBus.OnMagiConsulted += HandleMagiConsultation; }

        private void HandleMagiConsultation(MagiPersonality personality, string message)
        {
            personalityLabel.text = personality.ToString().ToUpper();
            // Color selection based on personality (Melchior: Green, Balthasar: Orange, Casper: Red)
            SetPersonalityColors(personality);
            
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(ShowMessageRoutine(message));
        }

        private IEnumerator ShowMessageRoutine(string fullMessage)
        {
            displayGroup.alpha = 1f;
            messageText.text = "";

            // NERV-style typing effect
            foreach (char c in fullMessage)
            {
                messageText.text += c;
                yield return new WaitForSeconds(0.03f); 
            }

            yield return new WaitForSeconds(3.0f); // Exposure time
            // ... Fade Out logic ...
        }
    }
}
```
