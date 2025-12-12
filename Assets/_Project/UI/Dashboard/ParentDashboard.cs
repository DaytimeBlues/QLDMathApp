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
        [Header("UI Elements")]
        [SerializeField] private Text totalSessionsText;
        [SerializeField] private Text totalTimeText;
        [SerializeField] private Text overallAccuracyText;
        
        [Header("Skill Bars")]
        [SerializeField] private Image countingBar;
        [SerializeField] private Image subitisingBar;
        [SerializeField] private Image patternsBar;
        
        [Header("Skill Labels")]
        [SerializeField] private Text countingLabel;
        [SerializeField] private Text subitisingLabel;
        [SerializeField] private Text patternsLabel;
        
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
            {
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            }
            
            if (resetProgressButton != null)
            {
                resetProgressButton.onClick.AddListener(OnResetProgress);
            }
            
            RefreshData();
        }

        private void OnEnable()
        {
            RefreshData();
        }

        public void RefreshData()
        {
            // Load stats from PlayerPrefs (in real app, would use DataService)
            int sessions = PlayerPrefs.GetInt("TotalSessions", 0);
            float totalMinutes = PlayerPrefs.GetFloat("TotalMinutes", 0f);
            float accuracy = PlayerPrefs.GetFloat("OverallAccuracy", 0f);
            
            float countingAccuracy = PlayerPrefs.GetFloat("CountingAccuracy", 0f);
            float subitisingAccuracy = PlayerPrefs.GetFloat("SubitisingAccuracy", 0f);
            float patternsAccuracy = PlayerPrefs.GetFloat("PatternsAccuracy", 0f);

            // Update text
            if (totalSessionsText != null)
            {
                totalSessionsText.text = $"{sessions} Sessions";
            }
            
            if (totalTimeText != null)
            {
                int hours = Mathf.FloorToInt(totalMinutes / 60);
                int mins = Mathf.FloorToInt(totalMinutes % 60);
                totalTimeText.text = hours > 0 ? $"{hours}h {mins}m" : $"{mins} minutes";
            }
            
            if (overallAccuracyText != null)
            {
                overallAccuracyText.text = $"{accuracy:F0}% Accuracy";
            }

            // Update skill bars
            UpdateBar(countingBar, countingLabel, countingAccuracy, "Counting");
            UpdateBar(subitisingBar, subitisingLabel, subitisingAccuracy, "Subitising");
            UpdateBar(patternsBar, patternsLabel, patternsAccuracy, "Patterns");
        }

        private void UpdateBar(Image bar, Text label, float accuracy, string skillName)
        {
            if (bar != null)
            {
                bar.fillAmount = accuracy / 100f;
                bar.color = GetColorForAccuracy(accuracy);
            }
            
            if (label != null)
            {
                label.text = $"{skillName}: {accuracy:F0}%";
            }
        }

        private Color GetColorForAccuracy(float accuracy)
        {
            if (accuracy < 50) return lowColor;
            if (accuracy < 80) return medColor;
            return highColor;
        }

        private void OnResetProgress()
        {
            // Confirmation would be needed in real app
            PlayerPrefs.DeleteAll();
            RefreshData();
            Debug.Log("[ParentDashboard] Progress reset!");
        }
    }
}
