using UnityEngine;
using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture.Managers;

namespace QLDMathApp.Testing
{
    /// <summary>
    /// Manual test helper for UI navigation stack.
    /// Attach to a GameObject in a test scene to validate navigation flow.
    /// </summary>
    public class NavigationTester : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool testDomainSelection = false;
        [SerializeField] private string testDomain = "Counting";

        private void Update()
        {
            // Trigger domain selection test with key press
            if (Input.GetKeyDown(KeyCode.T) || testDomainSelection)
            {
                testDomainSelection = false;
                TestDomainSelection();
            }

            // Navigate back with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TestNavigateBack();
            }

            // Show landing page with Home key
            if (Input.GetKeyDown(KeyCode.Home))
            {
                TestShowLandingPage();
            }
        }

        private void TestDomainSelection()
        {
            Debug.Log($"<color=cyan>[NavigationTester] Testing domain selection: {testDomain}</color>");
            EventBus.OnDomainSelected?.Invoke(testDomain);
        }

        private void TestNavigateBack()
        {
            Debug.Log("<color=cyan>[NavigationTester] Testing navigate back</color>");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NavigateBack();
            }
            else
            {
                Debug.LogWarning("[NavigationTester] GameManager.Instance is null");
            }
        }

        private void TestShowLandingPage()
        {
            Debug.Log("<color=cyan>[NavigationTester] Testing show landing page</color>");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowLandingPage();
            }
            else
            {
                Debug.LogWarning("[NavigationTester] GameManager.Instance is null");
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Navigation Test Controls:");
            GUILayout.Label("T - Test Domain Selection");
            GUILayout.Label("Escape - Navigate Back");
            GUILayout.Label("Home - Show Landing Page");
            
            if (GameManager.Instance != null)
            {
                GUILayout.Label($"Current State: {GameManager.Instance.CurrentState}");
            }
            
            GUILayout.EndArea();
        }
    }
}
