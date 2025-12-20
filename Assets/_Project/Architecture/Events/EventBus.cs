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

        #region Enchanted Forest Events
        // Param: Bloom Rate (0.0 to 1.0)
        public static Action<float> OnGardenBloomChanged;
        #endregion

        #region Forest Helper Events
        // Param: Helper (Owl, Bunny, Cat), Message
        public static Action<ForestHelper, string> OnHelperConsulted;
        #endregion
    }

    public enum ForestHelper
    {
        Owl,   // Wise/Rational
        Bunny, // Kind/Nurturing
        Cat    // Clever/Intuitive
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
