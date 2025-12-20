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

            // Enchanted Forest Mastery systems
            if (Object.FindFirstObjectByType<QLDMathApp.Architecture.Managers.GardenGrowthManager>() == null)
            {
                var growObj = new GameObject("GardenGrowthManager");
                DontDestroyOnLoad(growObj);
                growObj.AddComponent<QLDMathApp.Architecture.Managers.GardenGrowthManager>();
            }

            if (Object.FindFirstObjectByType<QLDMathApp.Modules.Magi.NatureHelperSystem>() == null)
            {
                var guideObj = new GameObject("NatureHelperSystem");
                DontDestroyOnLoad(guideObj);
                guideObj.AddComponent<QLDMathApp.Modules.Magi.NatureHelperSystem>();
            }

            // Data warmup
            if (contentRegistry != null)
                Debug.Log($"[AppBootstrapper] Library Warmup: {contentRegistry.AllProblems.Count} activities ready.");

            // HARDENED STARTUP (Audit Fix): Wait for systems to settle
            yield return new WaitForSeconds(0.1f); 
            
            Debug.Log("[AppBootstrapper] Enchanted Forest Initialized. Entering Main Menu...");
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single); 
        }
    }
}
