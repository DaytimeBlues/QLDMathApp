# QLDMathApp Audit Pack [Part 3: Developer Tools & Metadata]

This document covers the custom Editor tools developed to automate project setup and ensure codebase integrity via "Agentic" verification protocols.

## 1. Agentic Probe (AgenticQualityAssurance.cs)
**Decision**: Use C# Reflection to scan for null references and curriculum alignment.
**Rationale**: In complex Unity projects, manual checking of SerializedFields and ScriptableObject data is error-prone. This tool provides an "X-Ray Vision" into the project state.

```csharp
// Path: Assets/_Project/Editor/AgenticQualityAssurance.cs
using UnityEngine;
using UnityEditor;
using System.Reflection;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Editor.Tools
{
    public class AgenticQualityAssurance : EditorWindow
    {
        private void ScanForNulls()
        {
            // Use Reflection to find [SerializeField] fields that are null
            var monobehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in monobehaviours)
            {
                var type = mb.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    bool isSerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                    if (!isSerialized || field.FieldType.IsValueType) continue;

                    var value = field.GetValue(mb);
                    if (value == null || value.ToString() == "null")
                    {
                        Debug.LogWarning($"[ALERT] {mb.name}: Field '{field.Name}' is NULL.");
                    }
                }
            }
        }

        private void VerifyCurriculum()
        {
            // Audit MathProblemSO assets for Year 1 range (1-20)
            string[] guids = AssetDatabase.FindAssets("t:MathProblemSO");
            foreach (var guid in guids)
            {
                var problem = AssetDatabase.LoadAssetAtPath<MathProblemSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (problem.correctValue < 1 || problem.correctValue > 20)
                    Debug.LogWarning($"[WARNING] {problem.name}: Outside Year 1 range.");
            }
        }
    }
}
```

## 2. Automated Setup (VerticalSliceSetupTool.cs)
**Decision**: Provide a one-click scene generator for the NERV theme.
**Rationale**: Ensures consistency across scenes and simplifies onboarding for the developer. Uses `SerializedObject` to wire private fields during procedural generation.

```csharp
// Path: Assets/_Project/Editor/VerticalSliceSetupTool.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace QLDMathApp.Editor
{
    public class VerticalSliceSetupTool
    {
        [MenuItem("Tools/Setup Vertical Slice")]
        public static void RunSetup()
        {
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateAngelInterceptScene();
            SetupBuildSettings();
            AssetDatabase.Refresh();
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var go = new GameObject("AppBootstrapper");
            var bootstrapper = go.AddComponent<AppBootstrapper>();
            
            // Wiring [SerializeField] private fields via SerializedObject
            SerializedObject so = new SerializedObject(bootstrapper);
            so.FindProperty("contentRegistry").objectReferenceValue = registry;
            so.ApplyModifiedProperties();
            
            EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/_Bootstrap.unity");
        }
    }
}
#endif
```

## 3. Assembly Dependency Map
**Decision**: Segment the project into distinct assemblies to prevent spaghetti code and circular dependencies.
**Rationale**: Essential for large-scale Unity development.

| Assembly | Responsibility | References |
| :--- | :--- | :--- |
| `QLDMathApp.Architecture` | Core Events, Data models, Base Managers | Unity Engine |
| `QLDMathApp.Modules` | Gameplay Logic (Magi, Subitising) | Architecture |
| `QLDMathApp.Bootstrap` | Entry point, System Composition | Architecture, Modules, UI |
| `QLDMathApp.Editor` | QA Tools, Setup Wizards | Architecture, Modules, Bootstrap |

**Note on ERR_001**: A circular dependency initially occurred between Architecture and Modules. It was resolved by extracting the `AppBootstrapper` into the `Bootstrap` assembly.
