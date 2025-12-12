#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace QLDMathApp.Editor
{
    /// <summary>
    /// AUTO-SETUP: Creates a test scene on first run.
    /// Runs automatically when Unity compiles.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoSetupScene
    {
        static AutoSetupScene()
        {
            // Only run once
            if (EditorPrefs.GetBool("QLDMathApp_SetupComplete", false))
                return;
            
            EditorApplication.delayCall += SetupProject;
        }

        [MenuItem("Tools/QLD Math App/Setup Test Scene NOW")]
        public static void SetupProject()
        {
            Debug.Log("[AutoSetup] Setting up QLD Math App...");
            
            // Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create GameSystem object
            GameObject gameSystem = new GameObject("GameSystem");
            gameSystem.AddComponent<QLDMathApp.Architecture.Managers.GameManager>();
            gameSystem.AddComponent<QLDMathApp.Architecture.Input.InteractionController>();
            
            // Create Tester object
            GameObject tester = new GameObject("Tester");
            tester.AddComponent<QLDMathApp.Testing.GameplayTester>();
            tester.AddComponent<QLDMathApp.Testing.EventBusValidator>();
            
            // Save the scene
            string scenePath = "Assets/_Project/Scenes/TestScene.unity";
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            // Mark setup as complete
            EditorPrefs.SetBool("QLDMathApp_SetupComplete", true);
            
            Debug.Log("[AutoSetup] Test scene created at: " + scenePath);
            Debug.Log("[AutoSetup] Press PLAY to test! Use S/Y/N/U/D keys.");
            
            // Show message
            EditorUtility.DisplayDialog(
                "QLD Math App Ready!",
                "Test scene created!\n\nPress PLAY to test.\n\nKeyboard controls:\nS = Start session\nY = Correct answer\nN = Wrong answer\nU = Level up\nD = Show demo",
                "OK"
            );
        }
    }
}
#endif
