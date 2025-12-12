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
            // Load Firefly Flash scene
            // 'Single' mode to clear Main Menu memory
            Debug.Log("[MainMenu] Loading Firefly Flash...");
            SceneManager.LoadScene("FireflyFlash", LoadSceneMode.Single);
        }

        private void OpenSettings()
        {
            Debug.Log("[MainMenu] Settings clicked (Stub).");
        }
    }
}
