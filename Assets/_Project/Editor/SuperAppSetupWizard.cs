#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using QLDMathApp.UI.Services;
using QLDMathApp.UI.Events;

namespace QLDMathApp.Editor
{
    /// <summary>
    /// EDITOR TOOL: Creates AppRoot prefab and Event Channel assets.
    /// Menu: Tools > QLD Math App > Super App Setup
    /// </summary>
    public static class SuperAppSetupWizard
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/";
        private const string EVENTS_PATH = "Assets/_Project/UI/Events/";

        [MenuItem("Tools/QLD Math App/Super App Setup/Create All")]
        public static void CreateAll()
        {
            CreateEventChannelAssets();
            CreateAppRootPrefab();
            Debug.Log("[SuperApp] Setup complete!");
        }

        [MenuItem("Tools/QLD Math App/Super App Setup/Create Event Channels")]
        public static void CreateEventChannelAssets()
        {
            EnsureFolderExists(EVENTS_PATH);

            // Settings visibility channel
            var settingsChannel = ScriptableObject.CreateInstance<BoolEventChannelSO>();
            AssetDatabase.CreateAsset(settingsChannel, EVENTS_PATH + "SettingsVisibilityChannel.asset");

            // Scene load channel (optional)
            var sceneChannel = ScriptableObject.CreateInstance<StringEventChannelSO>();
            AssetDatabase.CreateAsset(sceneChannel, EVENTS_PATH + "SceneLoadChannel.asset");

            AssetDatabase.SaveAssets();
            Debug.Log("[SuperApp] Event channels created in " + EVENTS_PATH);
        }

        [MenuItem("Tools/QLD Math App/Super App Setup/Create AppRoot Prefab")]
        public static void CreateAppRootPrefab()
        {
            EnsureFolderExists(PREFAB_PATH);

            // Create AppRoot GameObject
            GameObject appRoot = new GameObject("AppRoot");

            // Add services
            appRoot.AddComponent<AccessibilitySettingsService>();

            // Create overlay canvas for scene transitions
            GameObject overlayGO = new GameObject("TransitionOverlay");
            overlayGO.transform.SetParent(appRoot.transform);

            Canvas canvas = overlayGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            CanvasScaler scaler = overlayGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            overlayGO.AddComponent<GraphicRaycaster>();

            // Overlay panel (black)
            GameObject panelGO = new GameObject("OverlayPanel");
            panelGO.transform.SetParent(overlayGO.transform);
            
            RectTransform panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = Color.black;

            CanvasGroup canvasGroup = panelGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Progress bar (optional)
            GameObject progressGO = new GameObject("ProgressBar");
            progressGO.transform.SetParent(panelGO.transform);
            
            RectTransform progressRT = progressGO.AddComponent<RectTransform>();
            progressRT.anchorMin = new Vector2(0.25f, 0.48f);
            progressRT.anchorMax = new Vector2(0.75f, 0.52f);
            progressRT.offsetMin = Vector2.zero;
            progressRT.offsetMax = Vector2.zero;

            Image progressImage = progressGO.AddComponent<Image>();
            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Horizontal;
            progressImage.fillAmount = 0f;
            progressImage.color = new Color(1f, 0.8f, 0.3f);

            // Add SceneTransitioner and wire up references
            SceneTransitioner transitioner = appRoot.AddComponent<SceneTransitioner>();
            
            // Use SerializedObject to set private serialized fields
            SerializedObject so = new SerializedObject(transitioner);
            so.FindProperty("overlay").objectReferenceValue = canvasGroup;
            so.FindProperty("progressBar").objectReferenceValue = progressImage;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Add AppRoot component
            appRoot.AddComponent<QLDMathApp.UI.AppRoot>();

            // Save as prefab
            string path = PREFAB_PATH + "AppRoot.prefab";
            PrefabUtility.SaveAsPrefabAsset(appRoot, path);
            Object.DestroyImmediate(appRoot);

            Debug.Log("[SuperApp] AppRoot prefab created at " + path);
        }

        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = "";

            foreach (string folder in folders)
            {
                if (string.IsNullOrEmpty(folder)) continue;

                string parentPath = currentPath;
                currentPath = string.IsNullOrEmpty(currentPath) ? folder : currentPath + "/" + folder;

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    AssetDatabase.CreateFolder(
                        string.IsNullOrEmpty(parentPath) ? "" : parentPath,
                        folder
                    );
                }
            }
        }
    }
}
#endif
