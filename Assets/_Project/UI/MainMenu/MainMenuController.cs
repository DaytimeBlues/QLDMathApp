using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace QLDMathApp.UI.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playSubitisingButton;
        [SerializeField] private Button settingsButton;

        private void Start()
        {
            playSubitisingButton.onClick.AddListener(PlayGame);
            settingsButton.onClick.AddListener(OpenSettings);
        }

        private void PlayGame()
        {
            // Load Firefly Intercept scene
            // 'Single' mode to clear Garden memory
            Debug.Log("[Forest] PATTERN BLUE: Initiating Firefly Intercept...");
            SceneManager.LoadScene("FireflyIntercept", LoadSceneMode.Single);
        }

        private void OpenSettings()
        {
            Debug.Log("[MainMenu] Settings clicked (Stub).");
        }
    }
}
