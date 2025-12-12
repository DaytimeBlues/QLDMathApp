using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using QLDMathApp.Architecture.Audio;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.Architecture.Bootstrapper
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

            // Data warmup (optional)
            if (contentRegistry != null)
                Debug.Log($"[AppBootstrapper] Registry Loaded: {contentRegistry.allProblems.Count} problems.");

            yield return null;

            Debug.Log("[AppBootstrapper] Initialization Complete. Loading Main Menu...");
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single); 
        }
    }
}
