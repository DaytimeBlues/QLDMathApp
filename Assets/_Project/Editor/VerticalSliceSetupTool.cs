#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Architecture.Bootstrapper;
using QLDMathApp.Modules.FireflyFlash;
using QLDMathApp.UI.MainMenu;

namespace QLDMathApp.Editor
{
    public class VerticalSliceSetupTool
    {
        [MenuItem("Tools/Setup Vertical Slice")]
        public static void RunSetup()
        {
            Debug.Log("--- Starting Vertical Slice Setup ---");

            EnsureDirectories();
            CreateContentRegistryAndSamples();
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateFireflyScene();
            SetupBuildSettings();

            Debug.Log("--- Setup Complete! ---");
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/_Project/Scenes",
                "Assets/_Project/Resources/Content",
                "Assets/_Project/Resources/Data"
            };

            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            }
        }

        private static void CreateContentRegistryAndSamples()
        {
            // 1. Registry
            string registryPath = "Assets/_Project/Resources/Data/ContentRegistry.asset";
            var registry = AssetDatabase.LoadAssetAtPath<ContentRegistrySO>(registryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<ContentRegistrySO>();
                AssetDatabase.CreateAsset(registry, registryPath);
            }

            // 2. Samples
            for (int i = 1; i <= 5; i++)
            {
                string problemPath = $"Assets/_Project/Resources/Content/Prob_Subitise_{i}.asset";
                var problem = AssetDatabase.LoadAssetAtPath<MathProblemSO>(problemPath);
                if (problem == null)
                {
                    problem = ScriptableObject.CreateInstance<MathProblemSO>();
                    problem.skillId = SkillId.SUBITISE_1_TO_5;
                    problem.correctValue = i;
                    problem.questionId = $"SUB_1to5_00{i}";
                    problem.flashDuration = 2.0f - (i * 0.2f); // Harder as we go
                    problem.distractorValues = new List<int>();
                    // Simple distractors
                    if (i > 1) problem.distractorValues.Add(i - 1);
                    if (i < 5) problem.distractorValues.Add(i + 1);
                    
                    AssetDatabase.CreateAsset(problem, problemPath);
                }

                if (!registry.Editor_AllProblems.Contains(problem))
                {
                    registry.Editor_AllProblems.Add(problem);
                }
            }
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }

        private static void CreateBootstrapScene()
        {
            string path = "Assets/_Project/Scenes/_Bootstrap.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create AppBootstrapper
            var go = new GameObject("AppBootstrapper");
            var bootstrapper = go.AddComponent<AppBootstrapper>();
            
            // Link Registry (Find asset)
            var registry = AssetDatabase.LoadAssetAtPath<ContentRegistrySO>("Assets/_Project/Resources/Data/ContentRegistry.asset");
            // Reflection or SerializedObject to set private field if needed, but assuming [SerializeField] public-ish access or direct assignment if public.
            // Since it's [SerializeField] private, we use SerializedObject.
            SerializedObject so = new SerializedObject(bootstrapper);
            so.FindProperty("contentRegistry").objectReferenceValue = registry;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateMainMenuScene()
        {
            string path = "Assets/_Project/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // UI Setup
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Controller
            var controllerGO = new GameObject("MainMenuController");
            var controller = controllerGO.AddComponent<MainMenuController>();

            // Buttons
            var playBtnGO = new GameObject("Btn_Play", typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            playBtnGO.transform.SetParent(canvasGO.transform, false);
            var playBtn = playBtnGO.GetComponent<UnityEngine.UI.Button>();

            var setBtnGO = new GameObject("Btn_Settings", typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            setBtnGO.transform.SetParent(canvasGO.transform, false);
            var setBtn = setBtnGO.GetComponent<UnityEngine.UI.Button>();

            // Wiring
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("playSubitisingButton").objectReferenceValue = playBtn;
            so.FindProperty("settingsButton").objectReferenceValue = setBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateFireflyScene()
        {
            string path = "Assets/_Project/Scenes/FireflyFlash.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var logicGO = new GameObject("FireflyGameLoop");
            logicGO.AddComponent<FireflyGameLoop>();
            
            // Note: Creating the full UI structure here is complex code-side. 
            // We create a minimal placeholder for wiring.
            var canvasGO = new GameObject("Canvas");
            canvasGO.AddComponent<Canvas>();
            
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/_Project/Scenes/_Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/FireflyFlash.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
#endif
