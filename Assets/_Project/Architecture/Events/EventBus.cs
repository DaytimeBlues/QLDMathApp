using System;
using UnityEngine;

namespace QLDMathApp.Architecture.Events
{
    public static class EventBus
    {
        // Game State Events
        public static Action<GameState> OnGameStateChanged;

        // Gameplay Events
        public static Action<string> OnProblemStarted;
        public static Action<bool, float> OnAnswerAttempted;
        public static Action<InterventionType> OnInterventionTriggered;

        // Enchanted Forest Theme Events
        public static Action<float> OnGrowthProgressChanged; // Replaces OngrowthProgressChanged

        // Nature Guide System Events
        public static Action<GuidePersonality, string> OnGuideSpoke; // Replaces OnAgentFeedbackRequested
    }

    // UPDATED ENUMS FOR ENCHANTED FOREST
    public enum GuidePersonality { WiseOwl, KindBunny, CuriousCat }
    public enum GameState { MainMenu, Instruction, Gameplay, Feedback, Paused } // Fixed: Feedback capitalized
    public enum InterventionType { LevelUp, ScaffoldDown, ShowDemo }
}
