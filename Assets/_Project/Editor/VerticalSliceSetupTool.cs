#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using QLDMathApp.Architecture.Data;
using QLDMathApp.Bootstrap;
using QLDMathApp.Modules.Subitising;
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
            CreateForestThemeAsset();
            CreateContentRegistryAndSamples();
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateFireflyInterceptScene();
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
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
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

        private static void CreateForestThemeAsset()
        {
            string path = "Assets/_Project/Resources/Data/ForestTheme.asset";
            var theme = AssetDatabase.LoadAssetAtPath<QLDMathApp.Architecture.UI.ForestTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<QLDMathApp.Architecture.UI.ForestTheme>();
                // Standard Forest Identity
                AssetDatabase.CreateAsset(theme, path);
            }
        }

        private static void CreateMainMenuScene()
        {
            string path = "Assets/_Project/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Find Theme
            var theme = AssetDatabase.LoadAssetAtPath<QLDMathApp.Architecture.UI.ForestTheme>("Assets/_Project/Resources/Data/ForestTheme.asset");

            // UI Setup
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Background
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.1f); // Dark Forest background
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Scanlines (Procedural)
            var linesGO = new GameObject("ForestMist", typeof(Image));
            linesGO.transform.SetParent(canvasGO.transform, false);
            var linesImg = linesGO.GetComponent<Image>();
            linesImg.color = new Color(0.7f, 1f, 0.4f, 0.03f); // Faint forest glow
            var linesRT = linesGO.GetComponent<RectTransform>();
            linesRT.anchorMin = Vector2.zero;
            linesRT.anchorMax = Vector2.one;
            linesRT.offsetMin = Vector2.zero;
            linesRT.offsetMax = Vector2.zero;

            // Title
            var titleGO = new GameObject("Title", typeof(Text));
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleText = titleGO.GetComponent<Text>();
            titleText.text = "ENCHANTED FOREST\n<size=30>LEARNING JOURNEY</size>";
            titleText.fontSize = 80;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.7f, 1f, 0.4f); // Forest Green
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.supportRichText = true;
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, 300);
            titleRT.sizeDelta = new Vector2(1000, 300);

            // Buttons Container
            var buttonsGO = new GameObject("ButtonsContainer");
            buttonsGO.transform.SetParent(canvasGO.transform, false);
            var buttonsRT = buttonsGO.AddComponent<RectTransform>();
            buttonsRT.anchoredPosition = new Vector2(0, -100);

            // Play Button
            var playBtnGO = CreateThemedButton("Btn_Play", "START ADVENTURE", new Vector2(0, 100), buttonsGO.transform);
            var playBtn = playBtnGO.GetComponent<Button>();

            // Settings Button
            var setBtnGO = CreateThemedButton("Btn_Settings", "GARDEN SETTINGS", new Vector2(0, -50), buttonsGO.transform);
            var setBtn = setBtnGO.GetComponent<Button>();

            // Controller
            var controllerGO = new GameObject("MainMenuController");
            var controller = controllerGO.AddComponent<MainMenuController>();

            // Wiring
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("playSubitisingButton").objectReferenceValue = playBtn;
            so.FindProperty("settingsButton").objectReferenceValue = setBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static GameObject CreateThemedButton(string name, string label, Vector2 pos, Transform parent)
        {
            var btnGO = new GameObject(name, typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            
            var rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector3(400, 80);
            rt.anchoredPosition = pos;

            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.1f, 0.6f, 0.1f, 0.2f); // Transparent Green

            var outlineGO = new GameObject("Outline", typeof(Image));
            outlineGO.transform.SetParent(btnGO.transform, false);
            var outRT = outlineGO.GetComponent<RectTransform>();
            outRT.anchorMin = Vector2.zero;
            outRT.anchorMax = Vector2.one;
            outRT.sizeDelta = new Vector2(4, 4);
            var outImg = outlineGO.GetComponent<Image>();
            outImg.color = new Color(0.7f, 1f, 0.4f, 1f); // Solid Green

            var textGO = new GameObject("Label", typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);
            var txt = textGO.GetComponent<Text>();
            txt.text = label;
            txt.fontSize = 32;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.7f, 1f, 0.4f); // Green
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var txtRT = textGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;

            return btnGO;
        }

        private static void CreateFireflyInterceptScene()
        {
            string path = "Assets/_Project/Scenes/FireflyIntercept.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Load Theme
            var theme = AssetDatabase.LoadAssetAtPath<QLDMathApp.Architecture.UI.ForestTheme>("Assets/_Project/Resources/Data/ForestTheme.asset");

            // UI Setup
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Background (Enchanted Garden)
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = new Color(0.02f, 0.05f, 0.02f); // Garden Twilight
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Firefly Clearing (formerly interception field)
            var fieldGO = new GameObject("FireflyClearing", typeof(Image), typeof(CanvasGroup));
            fieldGO.transform.SetParent(canvasGO.transform, false);
            var fieldImg = fieldGO.GetComponent<Image>();
            fieldImg.color = new Color(0.7f, 1f, 0.4f, 0.05f); // Forest Glow tint
            var fieldRT = fieldGO.GetComponent<RectTransform>();
            fieldRT.sizeDelta = new Vector2(1200, 800);
            var fieldGroup = fieldGO.GetComponent<CanvasGroup>();

            // HUD Placeholder
            var hudGO = new GameObject("HUD", typeof(Text));
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hudTxt = hudGO.GetComponent<Text>();
            hudTxt.text = "GARDEN GROWTH: 0%";
            hudTxt.fontSize = 40;
            hudTxt.alignment = TextAnchor.UpperRight;
            hudTxt.color = new Color(0.7f, 1f, 0.4f);
            hudTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var hudRT = hudGO.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(1, 1);
            hudRT.anchorMax = new Vector2(1, 1);
            hudRT.pivot = new Vector2(1, 1);
            hudRT.anchoredPosition = new Vector2(-50, -50);
            hudRT.sizeDelta = new Vector2(400, 100);

            // HELPER BUBBLE
            var guideGO = new GameObject("HelperBubble", typeof(CanvasGroup));
            guideGO.transform.SetParent(canvasGO.transform, false);
            var guideRT = guideGO.GetComponent<RectTransform>();
            guideRT.anchorMin = new Vector2(0, 1);
            guideRT.anchorMax = new Vector2(0, 1);
            guideRT.pivot = new Vector2(0, 1);
            guideRT.anchoredPosition = new Vector2(50, -50);
            guideRT.sizeDelta = new Vector2(600, 200);
            
            var gTitle = new GameObject("Personality", typeof(Text));
            gTitle.transform.SetParent(guideGO.transform, false);
            var gTitleTxt = gTitle.GetComponent<Text>();
            gTitleTxt.fontSize = 30;
            gTitleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            gTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);

            var gContent = new GameObject("Message", typeof(Text));
            gContent.transform.SetParent(guideGO.transform, false);
            var gContentTxt = gContent.GetComponent<Text>();
            gContentTxt.fontSize = 24;
            gContentTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            var hBubble = guideGO.AddComponent<QLDMathApp.Modules.NatureGuides.HelperBubble>();
            SerializedObject hso = new SerializedObject(hBubble);
            hso.FindProperty("agentNameLabel").objectReferenceValue = gTitleTxt;
            hso.FindProperty("messageText").objectReferenceValue = gContentTxt;
            hso.FindProperty("displayGroup").objectReferenceValue = guideGO.GetComponent<CanvasGroup>();
            hso.ApplyModifiedProperties();

            // Logic
            var logicGO = new GameObject("FireflyGameLoop");
            var controller = logicGO.AddComponent<FireflyGameLoop>();
            
            // Wire logic
            SerializedObject lso = new SerializedObject(controller);
            lso.FindProperty("answerButtonsCanvas").objectReferenceValue = fieldGroup;
            if (theme != null) lso.FindProperty("theme").objectReferenceValue = theme;
            lso.ApplyModifiedProperties();
            
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/_Project/Scenes/_Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/FireflyIntercept.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
#endif
