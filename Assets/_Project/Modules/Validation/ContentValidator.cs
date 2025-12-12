#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Modules.Validation
{
    public static class ContentValidator
    {
        public struct ValidationResult
        {
            public MathProblemSO asset;
            public string message;
            public bool isError;
        }

        public static List<ValidationResult> ValidateAll(List<MathProblemSO> problems)
        {
            var results = new List<ValidationResult>();

            foreach (var problem in problems)
            {
                if (problem == null) continue;

                // 1. Audio Accessibility (Critical)
                if (problem.instructionAudio == null)
                {
                    results.Add(new ValidationResult { 
                        asset = problem, 
                        message = "Missing 'Instruction Audio' (Required for pre-readers)", 
                        isError = true 
                    });
                }

                // 2. Logic Safety
                if (problem.distractorValues != null && problem.distractorValues.Contains(problem.correctValue))
                {
                    results.Add(new ValidationResult { 
                        asset = problem, 
                        message = $"Distractor list contains correct value ({problem.correctValue})", 
                        isError = true 
                    });
                }

                // 3. Subitising Specifics
                if (problem.flashDuration < 0.1f)
                {
                    results.Add(new ValidationResult { 
                        asset = problem, 
                        message = "Flash duration is too short (< 0.1s)", 
                        isError = false 
                    });
                }
            }
            return results;
        }
    }
}
#endif
