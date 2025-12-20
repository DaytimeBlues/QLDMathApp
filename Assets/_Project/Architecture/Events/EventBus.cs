using System;
using UnityEngine;

namespace QLDMathApp.Architecture.Events
{
    public static class EventBus
    {
        // usage: EventBus.OnGameStateChanged?.Invoke(newState);

        #region Game State Events
        public static Action<GameState> OnGameStateChanged;
        #endregion

        #region Gameplay Events
        // Param: Question ID
        public static Action<string> OnProblemStarted;
        
        // Param: IsCorrect, ResponseTime(ms)
        public static Action<bool, float> OnAnswerAttempted;
        
        // Triggered when mastery is detected or help is needed
        public static Action<InterventionType> OnInterventionTriggered;
        #endregion

        #region Feedback Events
        public static Action OnPlaySuccessFeedback;
        public static Action OnPlayCorrectionFeedback;
        #endregion

        #region Core Gameplay Framework (Generic)
        // Param: Mastery Grade (0.0 to 1.0)
        public static Action<float> OnMasteryLevelChanged;
        #endregion

        #region Pedagogical Agent Events
        // Param: Agent Identifier, Message
        public static Action<PedagogicalAgent, string> OnAgentFeedbackRequested;
        #endregion
    }

    public enum PedagogicalAgent
    {
        Rational,  // Wise (e.g. Owl)
        Nurturing, // Kind (e.g. Bunny)
        Intuitive  // Clever (e.g. Cat)
    }

    public enum GameState
    {
        MainMenu,
        Instruction, // "Passive" watching
        Gameplay,    // "Active" playing
        feedback,    // "Explanatory" pause
        Paused
    }

    public enum InterventionType
    {
        LevelUp,       // Increase difficulty
        ScaffoldDown,  // Decrease difficulty
        ShowDemo       // Play instructional video
    }
}
