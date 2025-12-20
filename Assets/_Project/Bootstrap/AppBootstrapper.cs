using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using QLDMathApp.Architecture.Audio;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Bootstrap
{
    public class AppBootstrapper : MonoBehaviour
    {
        private static bool _booted;

        [Header("Configuration")]
        [SerializeField] private ContentRegistrySO contentRegistry;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private IEnumerator Start()
        {
            if (_booted) yield break;
            _booted = true;

            DontDestroyOnLoad(gameObject); // Keep the bootstrap root alive if desired.

            Debug.Log("[AppBootstrapper] Initializing Services...");

            // Audio system
            if (AudioQueueService.Instance == null)
            {
                var audioObj = new GameObject("AudioQueueService");
                DontDestroyOnLoad(audioObj); // Critical if you LoadScene(Single).
                audioObj.AddComponent<AudioQueueService>();
            }

            // NERV Adaptive Systems
            if (Object.FindFirstObjectByType<QLDMathApp.Architecture.Managers.SyncRatioManager>() == null)
            {
                var syncManagerObj = new GameObject("NERV_SyncRatioManager");
                DontDestroyOnLoad(syncManagerObj);
                syncManagerObj.AddComponent<QLDMathApp.Architecture.Managers.SyncRatioManager>();
            }

            if (Object.FindFirstObjectByType<QLDMathApp.Modules.Magi.MagiSystem>() == null)
            {
                var magiObj = new GameObject("MAGI_System");
                DontDestroyOnLoad(magiObj);
                magiObj.AddComponent<QLDMathApp.Modules.Magi.MagiSystem>();
            }

            // Data warmup (optional)
            if (contentRegistry != null)
                Debug.Log($"[AppBootstrapper] Registry Loaded: {contentRegistry.AllProblems.Count} problems.");

            yield return null;

            Debug.Log("[AppBootstrapper] Initialization Complete. Loading Main Menu...");
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single); 
        }
    }
}
