using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace QLDMathApp.Architecture.Data
{
    /// <summary>
    /// CONTENT REGISTRY: Central catalog of all math problems.
    /// Replaces hardcoded problem lists in modules.
    /// Use this to query problems by skill, difficulty, or curriculum code.
    /// </summary>
    [CreateAssetMenu(fileName = "ContentRegistry", menuName = "Education/Content Registry")]
    public class ContentRegistrySO : ScriptableObject
    {
        [Header("All Problems")]
        [Tooltip("Complete list of all MathProblemSO assets")]
        [SerializeField] private List<MathProblemSO> allProblems = new List<MathProblemSO>();

        public IReadOnlyList<MathProblemSO> AllProblems => allProblems;
        
#if UNITY_EDITOR 
        public List<MathProblemSO> Editor_AllProblems => allProblems; 
#endif

        /// <summary>
        /// Get all problems for a specific skill.
        /// </summary>
        public List<MathProblemSO> GetBySkill(SkillId skill)
        {
            return allProblems.Where(p => p.skillId == skill).ToList();
        }

        /// <summary>
        /// Get all problems for a curriculum code.
        /// </summary>
        public List<MathProblemSO> GetByCurriculumCode(CurriculumCode code)
        {
            return allProblems.Where(p => p.curriculumCode == code).ToList();
        }

        /// <summary>
        /// Get problems within a difficulty range.
        /// </summary>
        public List<MathProblemSO> GetByDifficultyRange(float min, float max)
        {
            return allProblems.Where(p => p.difficultyRating >= min && p.difficultyRating <= max).ToList();
        }

        /// <summary>
        /// Get Foundation-level problems only (appropriate for 5-year-olds).
        /// </summary>
        public List<MathProblemSO> GetFoundationProblems()
        {
            return allProblems.Where(p => p.IsFoundationLevel).ToList();
        }

        /// <summary>
        /// Get Year 1 problems (graduation targets).
        /// </summary>
        public List<MathProblemSO> GetYear1Problems()
        {
            return allProblems.Where(p => !p.IsFoundationLevel).ToList();
        }

        /// <summary>
        /// Get the next appropriate problem based on current skill and performance.
        /// PERFORMANCE: Uses simple loop instead of LINQ to avoid GC allocations.
        /// </summary>
        public MathProblemSO GetNextProblem(SkillId currentSkill, float targetDifficulty)
        {
            MathProblemSO closest = null;
            float minDistance = float.MaxValue;

            foreach (var problem in allProblems)
            {
                if (problem.skillId != currentSkill) continue;
                
                float distance = Mathf.Abs(problem.difficultyRating - targetDifficulty);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = problem;
                }
            }

            return closest;
        }

        /// <summary>
        /// Total problem count.
        /// </summary>
        public int TotalProblems => allProblems.Count;

        /// <summary>
        /// Get all unique skills in the registry.
        /// </summary>
        public List<SkillId> GetAllSkills()
        {
            return allProblems.Select(p => p.skillId).Distinct().ToList();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor utility: Auto-populate from project.
        /// </summary>
        [ContextMenu("Find All Problems in Project")]
        private void FindAllProblems()
        {
            allProblems.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MathProblemSO");
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var problem = UnityEditor.AssetDatabase.LoadAssetAtPath<MathProblemSO>(path);
                if (problem != null)
                {
                    allProblems.Add(problem);
                }
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[ContentRegistry] Found {allProblems.Count} problems.");
        }
#endif
    }
}
