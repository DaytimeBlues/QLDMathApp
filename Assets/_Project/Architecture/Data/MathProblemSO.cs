using UnityEngine;
using System.Collections.Generic;

namespace QLDMathApp.Architecture.Data
{
    /// <summary>
    /// ACARA Curriculum codes - Foundation Year (AC9MFN) and Year 1 (AC9M1N).
    /// Foundation Year is the correct target for 5-year-olds.
    /// </summary>
    public enum CurriculumCode
    {
        // Foundation Year (5-year-olds)
        AC9MFN01,  // Count objects (small collections)
        AC9MFN02,  // Subitise small collections (1-5)
        AC9MFN03,  // Compare quantities
        AC9MFA01,  // Simple patterns (copy, continue)
        
        // Year 1 (graduation targets)
        AC9M1N01,  // Count to 120
        AC9M1N02,  // Partition into tens/ones
        AC9M1A01   // Identify pattern rules
    }

    /// <summary>
    /// Visual pattern type for subitising (Deep Research 1).
    /// </summary>
    public enum PatternType
    {
        Dice,
        TenFrame,
        Fingers,
        Irregular,
        Line
    }

    /// <summary>
    /// Internal skill identifiers - independent of curriculum codes.
    /// Use these for progression logic; map to curriculum codes for reporting.
    /// </summary>
    public enum SkillId
    {
        // Subitising (instant recognition without counting)
        SUBITISE_1_TO_3,
        SUBITISE_1_TO_5,
        SUBITISE_DICE_PATTERNS,
        
        // Counting
        COUNT_1_TO_5,
        COUNT_1_TO_10,
        COUNT_1_TO_20,
        COUNT_OBJECTS,
        
        // Patterns
        PATTERN_AB,
        PATTERN_ABB,
        PATTERN_ABC,
        PATTERN_AABB,
        
        // Comparison
        COMPARE_MORE_LESS,
        COMPARE_SAME_DIFFERENT
    }

    [CreateAssetMenu(fileName = "NewMathProblem", menuName = "Education/MathProblem")]
    public class MathProblemSO : ScriptableObject
    {
        [Header("Pedagogy")]
        [Tooltip("ACARA curriculum code for reporting")]
        public CurriculumCode curriculumCode = CurriculumCode.AC9MFN02;
        
        [Tooltip("Internal skill for progression logic")]
        public SkillId skillId = SkillId.SUBITISE_1_TO_5;
        
        [Tooltip("0.0 = Intro, 1.0 = Mastery")]
        [Range(0f, 1f)] public float difficultyRating = 0.5f;

        [Header("Content")]
        public string questionId; // e.g., "SUB_LVL1_001"
        [Range(0.5f, 5.0f)] public float flashDuration = 2.0f; // New: Flash mechanic
        public PatternType visualPattern = PatternType.Dice;   // New: Pattern type
        public int correctValue;
        public List<int> distractorValues;

        [Header("Explanatory Feedback (Active Ingredient)")]
        [Tooltip("Audio explaining WHY the answer is correct/incorrect, not just 'Good job'")]
        public AudioClip explanationAudio; 
        [Tooltip("Visual hint to display during explanation (e.g., grouped objects)")]
        public Sprite visualHint;

        [Header("NERV Mission Parameters")]
        public float requiredSyncRate = 0.6f;
        public string interceptionType = "Angel-Class C";

        [Header("Assets")]
        public AudioClip instructionAudio; 
        public AudioClip questionAudio; // e.g., "How many fireflies?"
        public Sprite targetVisual;     // e.g., Firefly sprite

        /// <summary>
        /// Returns true if this is a Foundation-level problem (for 5-year-olds).
        /// </summary>
        public bool IsFoundationLevel => 
            curriculumCode == CurriculumCode.AC9MFN01 ||
            curriculumCode == CurriculumCode.AC9MFN02 ||
            curriculumCode == CurriculumCode.AC9MFN03 ||
            curriculumCode == CurriculumCode.AC9MFA01;
    }
}

