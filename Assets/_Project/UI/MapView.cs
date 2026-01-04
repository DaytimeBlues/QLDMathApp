using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QLDMathApp.Architecture.Data;

namespace QLDMathApp.UI
{
    /// <summary>
    /// Map view that displays levels filtered by domain.
    /// </summary>
    public class MapView : UIView
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI domainLabel;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private Button levelButtonPrefab;
        [SerializeField] private Button backButton;

        private string _currentDomain;

        protected override void Awake()
        {
            base.Awake();

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackPressed);
            }
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackPressed);
            }
        }

        public override void SetData(object data)
        {
            if (data is string domain)
            {
                _currentDomain = domain;
                UpdateMapForDomain(domain);
            }
            else
            {
                Debug.LogWarning($"[MapView] SetData received unexpected type: {data?.GetType().Name ?? "null"}");
            }
        }

        /// <summary>
        /// Updates the map display for the specified domain.
        /// </summary>
        private void UpdateMapForDomain(string domain)
        {
            Debug.Log($"[MapView] Updating map for domain: {domain}");

            // Update domain label
            if (domainLabel != null)
            {
                domainLabel.text = $"{domain} Levels";
            }

            // Clear existing level buttons
            ClearLevelButtons();

            // Generate level buttons filtered by domain
            GenerateLevelButtons(domain);
        }

        /// <summary>
        /// Clear all level buttons from the container.
        /// </summary>
        private void ClearLevelButtons()
        {
            if (levelContainer == null) return;

            foreach (Transform child in levelContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Generate level buttons for the specified domain.
        /// </summary>
        private void GenerateLevelButtons(string domain)
        {
            if (levelContainer == null || levelButtonPrefab == null)
            {
                Debug.LogWarning("[MapView] levelContainer or levelButtonPrefab not assigned.");
                return;
            }

            // For now, generate placeholder levels
            // In a real implementation, this would fetch levels from ContentRegistrySO
            int levelCount = GetLevelCountForDomain(domain);

            for (int i = 1; i <= levelCount; i++)
            {
                int levelIndex = i; // Capture for closure
                var button = Instantiate(levelButtonPrefab, levelContainer);
                
                // Configure button
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"Level {levelIndex}";
                }

                // Add click listener
                button.onClick.AddListener(() => OnLevelSelected(levelIndex));
            }

            Debug.Log($"[MapView] Generated {levelCount} level buttons for {domain}.");
        }

        /// <summary>
        /// Get the number of levels for a domain.
        /// This is a placeholder - should query ContentRegistrySO in production.
        /// </summary>
        private int GetLevelCountForDomain(string domain)
        {
            // Placeholder: return different counts based on domain
            switch (domain)
            {
                case "Counting":
                    return 5;
                case "Subitising":
                    return 4;
                case "Patterns":
                    return 3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Called when a level is selected.
        /// </summary>
        private void OnLevelSelected(int levelIndex)
        {
            Debug.Log($"[MapView] Level {levelIndex} selected for domain: {_currentDomain}");
            
            // TODO: Load the selected level
            // This would typically trigger scene loading or level initialization
        }

        /// <summary>
        /// Called when the back button is pressed.
        /// </summary>
        private void OnBackPressed()
        {
            Debug.Log("[MapView] Back button pressed.");
            
            // Get the GameManager and call NavigateBack
            if (QLDMathApp.Architecture.Managers.GameManager.Instance != null)
            {
                QLDMathApp.Architecture.Managers.GameManager.Instance.NavigateBack();
            }
        }

        public override void Show()
        {
            base.Show();
            Debug.Log($"[MapView] Showing map view for domain: {_currentDomain}");
        }

        public override void Hide()
        {
            base.Hide();
            Debug.Log("[MapView] Hiding map view.");
        }
    }
}
