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
            // Load Angel Intercept scene
            // 'Single' mode to clear Tactical Display memory
            Debug.Log("[NERV] PATTERN BLUE: Initiating Angel Intercept...");
            SceneManager.LoadScene("AngelIntercept", LoadSceneMode.Single);
        }

        private void OpenSettings()
        {
            Debug.Log("[MainMenu] Settings clicked (Stub).");
        }
    }
}
