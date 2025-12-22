# QLDMathApp Audit Pack [Part 4: Pedagogy & Spatial Logic]

This document covers the educational data models and the spatial logic for pattern recognition ("subitising").

## 1. Math Data Model (MathProblemSO.cs)
**Decision**: Use ScriptableObjects aligned with the ACARA curriculum (Foundation/Year 1).
**Rationale**: Allows for externalized content management. Included `flashDuration` and `visualPattern` specifically to support "Active Ingredients" found in subitising research.

```csharp
// Path: Assets/_Project/Architecture/Data/MathProblemSO.cs
using UnityEngine;
using System.Collections.Generic;

namespace QLDMathApp.Architecture.Data
{
    public enum CurriculumCode { AC9MFN01, AC9MFN02, AC9MFA01, AC9M1N01 } // AC9 = Australian Curriculum v9
    public enum PatternType { Dice, TenFrame, Fingers, Irregular, Line }

    [CreateAssetMenu(fileName = "NewMathProblem", menuName = "Education/MathProblem")]
    public class MathProblemSO : ScriptableObject
    {
        [Header("Pedagogy")]
        public CurriculumCode curriculumCode;
        public float difficultyRating; // 0.0 to 1.0

        [Header("Content")]
        public int correctValue;
        public List<int> distractorValues;
        public float flashDuration = 2.0f; // How long patterns are visible

        [Header("Explanatory Feedback")]
        public AudioClip explanationAudio; // The "WHY" (Active Ingredient)
        public Sprite visualHint;
    }
}
```

## 2. Spatial Pattern Logic (AngelSpawner.cs)
**Decision**: Use predefined Dice Patterns for Level 1, fallback to random for Level 2.
**Rationale**: Dice patterns are easier for child brains to subitise. Moving to "Irregular" patterns is the true test of subitising vs. pattern recall. The "AnimateCountingSequence" provides the essential scaffolding (one-to-one correspondence) during correction.

```csharp
// Path: Assets/_Project/Modules/Subitising_FireflyFlash/Scripts/AngelSpawner.cs
using UnityEngine;
using System.Collections.Generic;

namespace QLDMathApp.Modules.Subitising
{
    public class AngelSpawner : MonoBehaviour
    {
        // Standard normalized dice patterns for 1-6
        private static readonly Vector2[][] DicePatterns = new Vector2[][]
        {
            new Vector2[] { Vector2.zero },                                  // 1
            new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) }, // 2
            // ... (3, 4, 5, 6 patterns) ...
        };

        public void SpawnFireflies(int count)
        {
            Vector2[] positions = GetPositions(count);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = jarContainer.position + (Vector3)(positions[i] * spawnRadius);
                Instantiate(fireflyPrefab, pos, Quaternion.identity);
            }
        }

        public IEnumerator AnimateCountingSequence()
        {
            // Scaffolding: Visually point to and count each item
            for (int i = 0; i < _spawnedAngels.Count; i++)
            {
                // Move finger, pulse angel, say number
                Debug.Log($"[Counting] {i + 1}");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
```

## Audit Summary for Other LLM
1. **System Consistency**: The project uses a unified NERV theme, but the underlying logic remains strictly educational (ACARA aligned).
2. **Adaptive Loop**: `SyncRatioManager` -> `EventBus` -> `AngelInterceptController` (Adjusts timing).
3. **Feedback Loop**: `AngelInterceptController` -> `EventBus` -> `MagiSystem` -> `EventBus` -> `MagiDisplay`.
4. **Assembly Safety**: ASMDEF files are used to enforce clear boundaries and build performance.
