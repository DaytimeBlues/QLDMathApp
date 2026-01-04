using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.UI
{
    /// <summary>
    /// Landing page view that allows domain selection.
    /// </summary>
    public class LandingPageView : UIView
    {
        [Header("Domain Selection Buttons")]
        [SerializeField] private Button countingButton;
        [SerializeField] private Button subitisingButton;
        [SerializeField] private Button patternsButton;

        protected override void Awake()
        {
            base.Awake();

            // Setup button listeners
            if (countingButton != null)
                countingButton.onClick.AddListener(() => OnDomainSelected("Counting"));
            
            if (subitisingButton != null)
                subitisingButton.onClick.AddListener(() => OnDomainSelected("Subitising"));
            
            if (patternsButton != null)
                patternsButton.onClick.AddListener(() => OnDomainSelected("Patterns"));
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (countingButton != null)
                countingButton.onClick.RemoveAllListeners();
            
            if (subitisingButton != null)
                subitisingButton.onClick.RemoveAllListeners();
            
            if (patternsButton != null)
                patternsButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Called when a domain is selected.
        /// </summary>
        private void OnDomainSelected(string domain)
        {
            Debug.Log($"[LandingPageView] Domain selected: {domain}");
            
            // Raise event for GameManager to handle
            EventBus.OnDomainSelected?.Invoke(domain);
        }

        public override void Show()
        {
            base.Show();
            Debug.Log("[LandingPageView] Showing landing page.");
        }

        public override void Hide()
        {
            base.Hide();
            Debug.Log("[LandingPageView] Hiding landing page.");
        }
    }
}
