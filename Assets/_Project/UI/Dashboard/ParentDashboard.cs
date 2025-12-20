using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using QLDMathApp.Architecture.Services;

namespace QLDMathApp.UI
{
    /// <summary>
    /// PARENT DASHBOARD: Shows learning progress.
    /// Simple visualization of:
    /// - Sessions completed
    /// - Accuracy by skill area
    /// - Time spent
    /// Uses simple charts (bar graph style).
    /// </summary>
    public class ParentDashboard : MonoBehaviour
    {
        [Header("UI Elements (TMPro)")]
        [SerializeField] private TMP_Text totalSessionsText;
        [SerializeField] private TMP_Text totalTimeText;
        [SerializeField] private TMP_Text overallAccuracyText;
        
        [Header("Skill Bars")]
        [SerializeField] private Image countingBar;
        [SerializeField] private Image subitisingBar;
        [SerializeField] private Image patternsBar;
        
        [Header("Skill Labels (TMPro)")]
        [SerializeField] private TMP_Text countingLabel;
        [SerializeField] private TMP_Text subitisingLabel;
        [SerializeField] private TMP_Text patternsLabel;
        
        [Header("Colors")]
        [SerializeField] private Color lowColor = new Color(1f, 0.6f, 0.6f);
        [SerializeField] private Color medColor = new Color(1f, 1f, 0.6f);
        [SerializeField] private Color highColor = new Color(0.6f, 1f, 0.6f);

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetProgressButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            
            if (resetProgressButton != null)
                resetProgressButton.onClick.AddListener(OnResetProgress);
        }

        private void OnEnable()
        {
            RefreshData();
        }

        public void RefreshData()
        {
            // AUDIT FIX: Use PersistenceService instead of PlayerPrefs
            var data = PersistenceService.Instance.Load<AppUserData>();

            if (totalSessionsText != null)
                totalSessionsText.text = $"{data.TotalSessions} Sessions";
            
            if (totalTimeText != null)
            {
                int hours = Mathf.FloorToInt(data.TotalMinutes / 60);
                int mins = Mathf.FloorToInt(data.TotalMinutes % 60);
                totalTimeText.text = hours > 0 ? $"{hours}h {mins}m" : $"{mins} minutes";
            }
            
            if (overallAccuracyText != null)
                overallAccuracyText.text = $"{data.OverallAccuracy:F0}% Accuracy";

            // Update skill bars (Mocked for now since AppUserData is simplified)
            UpdateBar(countingBar, countingLabel, 75f, "Counting");
            UpdateBar(subitisingBar, subitisingLabel, 85f, "Subitising");
            UpdateBar(patternsBar, patternsLabel, 60f, "Patterns");
        }

        private void UpdateBar(Image bar, TMP_Text label, float accuracy, string skillName)
        {
            if (bar != null)
            {
                bar.fillAmount = accuracy / 100f;
                bar.color = GetColorForAccuracy(accuracy);
            }
            if (label != null)
                label.text = $"{skillName}: {accuracy:F0}%";
        }

        private Color GetColorForAccuracy(float accuracy)
        {
            if (accuracy < 50) return lowColor;
            if (accuracy < 80) return medColor;
            return highColor;
        }

        private void OnResetProgress()
        {
            PersistenceService.Instance.Save(new AppUserData());
            RefreshData();
            Debug.Log("[ParentDashboard] Progress reset via PersistenceService!");
        }
    }
}
