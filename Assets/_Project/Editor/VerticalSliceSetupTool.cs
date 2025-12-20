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
            CreateNERVThemeAsset();
            CreateContentRegistryAndSamples();
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateAngelInterceptScene();
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

        private static void CreateNERVThemeAsset()
        {
            string path = "Assets/_Project/Resources/Data/NERVTheme.asset";
            var theme = AssetDatabase.LoadAssetAtPath<QLDMathApp.Architecture.UI.NERVTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<QLDMathApp.Architecture.UI.NERVTheme>();
                // Standard NERV Identity
                AssetDatabase.CreateAsset(theme, path);
            }
        }

        private static void CreateMainMenuScene()
        {
            string path = "Assets/_Project/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Find Theme
            var theme = AssetDatabase.LoadAssetAtPath<QLDMathApp.Architecture.UI.NERVTheme>("Assets/_Project/Resources/Data/NERVTheme.asset");

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
            bgImg.color = new Color(0.05f, 0.05f, 0.1f); // Dark NERV background
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Scanlines (Procedural)
            var linesGO = new GameObject("TacticalScanlines", typeof(Image));
            linesGO.transform.SetParent(canvasGO.transform, false);
            var linesImg = linesGO.GetComponent<Image>();
            linesImg.color = new Color(0f, 1f, 0f, 0.03f); // Faint green overlay
            var linesRT = linesGO.GetComponent<RectTransform>();
            linesRT.anchorMin = Vector2.zero;
            linesRT.anchorMax = Vector2.one;
            linesRT.offsetMin = Vector2.zero;
            linesRT.offsetMax = Vector2.zero;

            // Title
            var titleGO = new GameObject("Title", typeof(Text));
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleText = titleGO.GetComponent<Text>();
            titleText.text = "NERV TACTICAL HUB\n<size=30>QLD MATHEMATICS INTERFACE</size>";
            titleText.fontSize = 80;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.5f, 0f); // NERV Orange
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
            var playBtnGO = CreateThemedButton("Btn_Play", "INITIATE INTERCEPT", new Vector2(0, 100), buttonsGO.transform);
            var playBtn = playBtnGO.GetComponent<Button>();

            // Settings Button
            var setBtnGO = CreateThemedButton("Btn_Settings", "TACTICAL CONFIG", new Vector2(0, -50), buttonsGO.transform);
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
            img.color = new Color(0.1f, 1f, 0.4f, 0.2f); // Transparent Sync Green

            var outlineGO = new GameObject("Outline", typeof(Image));
            outlineGO.transform.SetParent(btnGO.transform, false);
            var outRT = outlineGO.GetComponent<RectTransform>();
            outRT.anchorMin = Vector2.zero;
            outRT.anchorMax = Vector2.one;
            outRT.sizeDelta = new Vector2(4, 4);
            var outImg = outlineGO.GetComponent<Image>();
            outImg.color = new Color(0.1f, 1f, 0.4f, 1f); // Solid Sync Green

            var textGO = new GameObject("Label", typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);
            var txt = textGO.GetComponent<Text>();
            txt.text = label;
            txt.fontSize = 32;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.1f, 1f, 0.4f); // Sync Green
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var txtRT = textGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;

            return btnGO;
        }

        private static void CreateAngelInterceptScene()
        {
            string path = "Assets/_Project/Scenes/AngelIntercept.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Load Theme
            var theme = AssetDatabase.LoadAssetAtPath<QLDMathApp.Architecture.UI.NERVTheme>("Assets/_Project/Resources/Data/NERVTheme.asset");

            // UI Setup
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Background (Tokyo-3 Radar)
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.05f); // Deep tactical black
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // Interception Field (formerly jarContainer)
            var fieldGO = new GameObject("InterceptionField", typeof(Image), typeof(CanvasGroup));
            fieldGO.transform.SetParent(canvasGO.transform, false);
            var fieldImg = fieldGO.GetComponent<Image>();
            fieldImg.color = new Color(0.1f, 1f, 0.4f, 0.05f); // Sync Green tint
            var fieldRT = fieldGO.GetComponent<RectTransform>();
            fieldRT.sizeDelta = new Vector2(1200, 800);
            var fieldGroup = fieldGO.GetComponent<CanvasGroup>();

            // HUD Placeholder
            var hudGO = new GameObject("HUD", typeof(Text));
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hudTxt = hudGO.GetComponent<Text>();
            hudTxt.text = "SYNC RATIO: 40.0%";
            hudTxt.fontSize = 40;
            hudTxt.alignment = TextAnchor.UpperRight;
            hudTxt.color = new Color(0.1f, 1f, 0.4f);
            hudTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var hudRT = hudGO.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(1, 1);
            hudRT.anchorMax = new Vector2(1, 1);
            hudRT.pivot = new Vector2(1, 1);
            hudRT.anchoredPosition = new Vector2(-50, -50);
            hudRT.sizeDelta = new Vector2(400, 100);

            // MAGI DISPLAY
            var magiGO = new GameObject("MagiDisplay", typeof(CanvasGroup));
            magiGO.transform.SetParent(canvasGO.transform, false);
            var magiRT = magiGO.GetComponent<RectTransform>();
            magiRT.anchorMin = new Vector2(0, 1);
            magiRT.anchorMax = new Vector2(0, 1);
            magiRT.pivot = new Vector2(0, 1);
            magiRT.anchoredPosition = new Vector2(50, -50);
            magiRT.sizeDelta = new Vector2(600, 200);
            
            var mTitle = new GameObject("Personality", typeof(Text));
            mTitle.transform.SetParent(magiGO.transform, false);
            var mTitleTxt = mTitle.GetComponent<Text>();
            mTitleTxt.fontSize = 30;
            mTitleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            mTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);

            var mContent = new GameObject("Message", typeof(Text));
            mContent.transform.SetParent(magiGO.transform, false);
            var mContentTxt = mContent.GetComponent<Text>();
            mContentTxt.fontSize = 24;
            mContentTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            var mDisplay = magiGO.AddComponent<QLDMathApp.Modules.Magi.MagiDisplay>();
            SerializedObject mso = new SerializedObject(mDisplay);
            mso.FindProperty("personalityLabel").objectReferenceValue = mTitleTxt;
            mso.FindProperty("messageText").objectReferenceValue = mContentTxt;
            mso.FindProperty("displayGroup").objectReferenceValue = magiGO.GetComponent<CanvasGroup>();
            mso.ApplyModifiedProperties();

            // Logic
            var logicGO = new GameObject("AngelInterceptController");
            var controller = logicGO.AddComponent<AngelInterceptController>();
            
            // Wire logic
            SerializedObject lso = new SerializedObject(controller);
            lso.FindProperty("interceptionFieldGroup").objectReferenceValue = fieldGroup;
            if (theme != null) lso.FindProperty("theme").objectReferenceValue = theme;
            lso.ApplyModifiedProperties();
            
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/_Project/Scenes/_Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/AngelIntercept.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
#endif
