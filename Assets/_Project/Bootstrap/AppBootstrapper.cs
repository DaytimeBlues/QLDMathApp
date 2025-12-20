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

        public static System.Action OnServicesReady;

        private IEnumerator Start()
        {
            if (_booted) yield break;
            _booted = true;

            DontDestroyOnLoad(gameObject);
            Debug.Log("[AppBootstrapper] Initializing Core Systems...");

            // 1. Persistence (Sync/Async ready)
            var persistence = Object.FindFirstObjectByType<QLDMathApp.Architecture.Services.PersistenceService>();
            if (persistence == null)
            {
                var persistObj = new GameObject("PersistenceService");
                persistence = persistObj.AddComponent<QLDMathApp.Architecture.Services.PersistenceService>();
            }
            yield return persistence.Initialize();

            // 2. Adaptive Mastery & Helper Systems
            var gardenManager = CreateService<QLDMathApp.Architecture.Managers.GardenGrowthManager>("GardenGrowthManager");
            var helperSystem = CreateService<QLDMathApp.Modules.Magi.NatureHelperSystem>("NatureHelperSystem");
            var session = CreateService<QLDMathApp.Architecture.Managers.SessionManager>("SessionManager");
            var accessibility = CreateService<QLDMathApp.Architecture.UI.AccessibilitySettings>("AccessibilitySettings");

            // Initialize in parallel
            yield return gardenManager.Initialize();
            yield return helperSystem.Initialize();
            yield return session.Initialize();
            yield return accessibility.Initialize();

            // 3. Data Warmup
            if (contentRegistry != null)
                Debug.Log($"[AppBootstrapper] Library Warmup: {contentRegistry.AllProblems.Count} activities ready.");

            // Verification of Service Readiness
            bool allReady = persistence.IsInitialized && gardenManager.IsInitialized && 
                            helperSystem.IsInitialized && session.IsInitialized && accessibility.IsInitialized;

            if (allReady)
            {
                Debug.Log("[AppBootstrapper] Services Validated. Signalling Ready...");
                OnServicesReady?.Invoke();
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("[AppBootstrapper] Critical Service Failure during initialization!");
            }
        }

        private T CreateService<T>(string name) where T : MonoBehaviour, IInitializable
        {
            var service = Object.FindFirstObjectByType<T>();
            if (service == null)
            {
                var go = new GameObject(name);
                DontDestroyOnLoad(go);
                service = go.AddComponent<T>();
            }
            return service;
        }
    }
}
