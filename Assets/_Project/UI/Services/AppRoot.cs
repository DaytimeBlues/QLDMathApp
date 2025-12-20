using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.UI.Services;
using QLDMathApp.UI.Events;
using QLDMathApp.Architecture.Services;

namespace QLDMathApp.UI
{
    /// <summary>
    /// APP ROOT: Persistent services container.
    /// Attach this to a "AppRoot" GameObject in your first scene.
    /// Contains services that persist across all scenes.
    /// </summary>
    public class AppRoot : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private AccessibilitySettingsService accessibilityService;
        [SerializeField] private SceneTransitioner sceneTransitioner;

        [Header("Transition Overlay (Optional)")]
        [SerializeField] private Canvas overlayCanvas;
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private Image progressBar;

        private static AppRoot _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Setup overlay canvas to render on top
            if (overlayCanvas != null)
            {
                overlayCanvas.sortingOrder = 999;
            }

            Debug.Log("[AppRoot] Initialized - services persisting across scenes.");
        }

        /// <summary>
        /// Static access to check if AppRoot is initialized.
        /// </summary>
        public static bool IsInitialized => _instance != null;
    }
}
