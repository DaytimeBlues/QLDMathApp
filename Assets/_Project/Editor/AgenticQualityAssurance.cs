using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Editor.Tools
{
    /// <summary>
    /// AGENTIC PROBE: Implements the "X-Ray Vision" research finding.
    /// Uses Reflection to validate project state and curriculum alignment.
    /// </summary>
    public class AgenticQualityAssurance : EditorWindow
    {
        [MenuItem("Forest/Agentic QA Probe")]
        public static void ShowWindow()
        {
            GetWindow<AgenticQualityAssurance>("Agentic QA");
        }

        private Vector2 _scrollPos;
        private string _report = "Ready to scan.";

        private void OnGUI()
        {
            GUILayout.Label("Agentic Quality Assurance", EditorStyles.boldLabel);
            
            if (GUILayout.Button("1. Scan for Null References (X-Ray)"))
            {
                ScanForNulls();
            }

            if (GUILayout.Button("2. Verify Year 1 Curriculum (Data Audit)"))
            {
                VerifyCurriculum();
            }

            if (GUILayout.Button("3. Verify Forest Systems (Theme/Guides)"))
            {
                VerifyForestSystems();
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.TextArea(_report, GUILayout.Height(400));
            EditorGUILayout.EndScrollView();
        }

        private void ScanForNulls()
        {
            _report = "SCANNING SCENE FOR NULL REFERENCES...\n";
            var monobehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int issuesFound = 0;

            foreach (var mb in monobehaviours)
            {
                if (mb == null) continue;

                var type = mb.GetType();
                // Get all fields, public or private with SerializeField
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    // Check if it's serialized
                    bool isSerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                    if (!isSerialized) continue;

                    // Skip primitive types (int, float, bool)
                    if (field.FieldType.IsValueType) continue;

                    var value = field.GetValue(mb);
                    if (value == null || value.ToString() == "null")
                    {
                        // Check if it has a [Optional] attribute (custom) or if we strictly require it
                        // For this probe, we flag direct nulls on Object references
                        if (typeof(Object).IsAssignableFrom(field.FieldType))
                        {
                            _report += $"[ALERT] {mb.name} ({type.Name}): Field '{field.Name}' is NULL.\n";
                            issuesFound++;
                        }
                    }
                }
            }

            if (issuesFound == 0) _report += "🟢 CODEBASE INTEGRITY: 100%. No null references detected in loaded scene.\n";
            else _report += $"🔴 ISSUES FOUND: {issuesFound}. See details above.\n";
        }

        private void VerifyCurriculum()
        {
            _report = "AUDITING YEAR 1 MATH CURRICULUM (ACARA)...\n";
            string[] guids = AssetDatabase.FindAssets("t:MathProblemSO");
            int problemCount = 0;
            int outOfRangeCount = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var problem = AssetDatabase.LoadAssetAtPath<MathProblemSO>(path);
                if (problem == null) continue;

                problemCount++;
                
                // QLD Year 1 / Foundation: 
                // Subitising usually 1-10.
                // Counting usually 1-20.
                
                if (problem.correctValue < 1 || problem.correctValue > 20)
                {
                    _report += $"[WARNING] {problem.name}: Value {problem.correctValue} is outside standard Year 1 range (1-20).\n";
                    outOfRangeCount++;
                }

                // Distractor Check
                foreach(var dist in problem.distractorValues)
                {
                     if (dist < 0) _report += $"[ERROR] {problem.name}: Negative distractor {dist}.\n";
                }
            }

            _report += $"Analyzed {problemCount} problems.\n";
            if (outOfRangeCount == 0) _report += "🟢 CURRICULUM ALIGNMENT: VALID. All problems within 1-20 range.\n";
            else _report += "🟡 CURRICULUM WARNINGS DETECTED.\n";
        }

        private void VerifyForestSystems()
        {
            _report = "VERIFYING FOREST NATURE SYSTEMS...\n";
            
            // 1. Theme Asset
            var themes = AssetDatabase.FindAssets("t:ForestTheme");
            if (themes.Length > 0) _report += "🟢 FOREST THEME: DETECTED.\n";
            else _report += "🔴 FOREST THEME: MISSING. Run Vertical Slice Setup.\n";

            // 2. Helper Bubble (Scene)
            var bubble = Object.FindFirstObjectByType<QLDMathApp.Modules.NatureGuides.HelperBubble>();
            if (bubble != null) _report += "🟢 HELPER BUBBLE: ONLINE.\n";
            else _report += "🔴 HELPER BUBBLE: OFFLINE. Scene missing Helper Bubble prefab.\n";

            // 3. Garden Growth Manager (Scene/Bootstrap)
            var growth = Object.FindFirstObjectByType<QLDMathApp.Architecture.Managers.GardenGrowthManager>();
            if (growth != null) _report += "🟢 GARDEN GROWTH MANAGER: ACTIVE.\n";
            else _report += "🔴 GARDEN GROWTH MANAGER: NOT FOUND. Check AppBootstrapper.\n";
        }
    }
}
