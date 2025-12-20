using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using QLDMathApp.Architecture.Audio;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Bootstrap
{
    [DefaultExecutionOrder(-100)]
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

            DontDestroyOnLoad(gameObject);

            Debug.Log("[AppBootstrapper] Initializing Forest Services...");

            // Audio system
            if (AudioQueueService.Instance == null)
            {
                var audioObj = new GameObject("AudioQueueService");
                DontDestroyOnLoad(audioObj);
                audioObj.AddComponent<AudioQueueService>();
            }

            // Garden Growth Manager (adaptive difficulty)
            if (Object.FindFirstObjectByType<QLDMathApp.Architecture.Managers.GardenGrowthManager>() == null)
            {
                var growObj = new GameObject("GardenGrowthManager");
                DontDestroyOnLoad(growObj);
                growObj.AddComponent<QLDMathApp.Architecture.Managers.GardenGrowthManager>();
                Debug.Log("[Bootstrap] Garden Growth Manager planted.");
            }

            // Nature Helper System (feedback guides)
            if (Object.FindFirstObjectByType<QLDMathApp.Modules\.NatureGuides.NatureHelperSystem>() == null)
            {
                var guideObj = new GameObject("NatureHelperSystem");
                DontDestroyOnLoad(guideObj);
                guideObj.AddComponent<QLDMathApp.Modules\.NatureGuides.NatureHelperSystem>();
                Debug.Log("[Bootstrap] Forest Guides awakened.");
            }

            // Data warmup
            if (contentRegistry != null)
                Debug.Log($"[AppBootstrapper] Library Warmup: {contentRegistry.AllProblems.Count} activities ready.");

            // Wait one frame for systems to initialize (replaces brittle WaitForSeconds)
            yield return null;
            
            Debug.Log("[AppBootstrapper] Enchanted Forest Initialized. Entering Main Menu...");
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single); 
        }
    }
}
