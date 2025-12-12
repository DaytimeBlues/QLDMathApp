#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using QLDMathApp.Architecture.Data;
using System.Collections.Generic;

namespace QLDMathApp.Editor
{
    /// <summary>
    /// EDITOR UTILITY: Creates MathProblemSO assets for testing.
    /// Menu: Tools > QLD Math App > Create Sample Problems
    /// </summary>
    public static class SampleProblemGenerator
    {
        private const string PROBLEMS_PATH = "Assets/_Project/Content/Problems/";



        [MenuItem("Tools/QLD Math App/Create Sample Problems/Subitising (1-5)")]
        public static void CreateSubitisingProblems()
        {
            EnsureFolderExists();
            
            for (int i = 1; i <= 5; i++)
            {
                MathProblemSO problem = ScriptableObject.CreateInstance<MathProblemSO>();
                problem.questionId = $"SUB_001_{i}";
                problem.correctValue = i;
                problem.distractorValues = GenerateDistractors(i, 1, 5, 2);
                problem.curriculumCode = CurriculumCode.AC9MFN02;
                problem.skillId = SkillId.SUBITISE_1_TO_5;
                problem.difficultyRating = (i - 1) * 0.2f;
                
                string path = PROBLEMS_PATH + $"Subitising/SUB_001_{i}.asset";
                EnsureSubfolder("Subitising");
                AssetDatabase.CreateAsset(problem, path);
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("[ProblemGen] Created 5 Subitising problems in " + PROBLEMS_PATH);
        }

        [MenuItem("Tools/QLD Math App/Create Sample Problems/Counting (1-10)")]
        public static void CreateCountingProblems()
        {
            EnsureFolderExists();
            EnsureSubfolder("Counting");
            
            for (int i = 1; i <= 10; i++)
            {
                MathProblemSO problem = ScriptableObject.CreateInstance<MathProblemSO>();
                problem.questionId = $"COUNT_001_{i}";
                problem.correctValue = i;
                problem.distractorValues = new List<int>(); // No distractors for counting
                problem.curriculumCode = CurriculumCode.AC9MFN01;
                problem.skillId = i <= 5 ? SkillId.COUNT_1_TO_5 : SkillId.COUNT_1_TO_10;
                problem.difficultyRating = (i - 1) * 0.1f;
                
                string path = PROBLEMS_PATH + $"Counting/COUNT_001_{i}.asset";
                AssetDatabase.CreateAsset(problem, path);
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("[ProblemGen] Created 10 Counting problems in " + PROBLEMS_PATH);
        }

        [MenuItem("Tools/QLD Math App/Create Sample Problems/All")]
        public static void CreateAllProblems()
        {
            CreateSubitisingProblems();
            CreateCountingProblems();
        }

        private static List<int> GenerateDistractors(int correct, int min, int max, int count)
        {
            List<int> distractors = new List<int>();
            
            // Add adjacent numbers as distractors
            if (correct > min) distractors.Add(correct - 1);
            if (correct < max) distractors.Add(correct + 1);
            
            // Fill remaining with random if needed
            while (distractors.Count < count && distractors.Count < (max - min))
            {
                int rand = Random.Range(min, max + 1);
                if (rand != correct && !distractors.Contains(rand))
                {
                    distractors.Add(rand);
                }
            }
            
            return distractors;
        }

        private static void EnsureFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Content"))
                AssetDatabase.CreateFolder("Assets/_Project", "Content");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Content/Problems"))
                AssetDatabase.CreateFolder("Assets/_Project/Content", "Problems");
        }

        private static void EnsureSubfolder(string name)
        {
            string path = "Assets/_Project/Content/Problems/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Content/Problems", name);
            }
        }
    }
}
#endif
