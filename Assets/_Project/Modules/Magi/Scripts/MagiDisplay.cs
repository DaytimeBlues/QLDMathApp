using UnityEngine;
using UnityEngine.UI;
using QLDMathApp.Architecture.Events;
using System.Collections;

namespace QLDMathApp.Modules.Magi
{
    /// <summary>
    /// MAGI INTERFACE: Visualizes the tripartite feedback in the HUD.
    /// Animates messages with NERV-style typing effects.
    /// </summary>
    public class MagiDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text personalityLabel;
        [SerializeField] private Text messageText;
        [SerializeField] private CanvasGroup displayGroup;

        [Header("NERV Calibration")]
        [SerializeField] private Color colorMelchior = new Color(0.1f, 1f, 0.4f); // Green
        [SerializeField] private Color colorBalthasar = new Color(1f, 0.5f, 0f);  // Orange
        [SerializeField] private Color colorCasper = new Color(1f, 0.2f, 0.2f);     // Red

        private Coroutine _fadeRoutine;

        private void Start()
        {
            if (displayGroup != null) displayGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            EventBus.OnMagiConsulted += HandleMagiConsultation;
        }

        private void OnDisable()
        {
            EventBus.OnMagiConsulted -= HandleMagiConsultation;
        }

        private void HandleMagiConsultation(MagiPersonality personality, string message)
        {
            if (personalityLabel == null || messageText == null) return;

            // Set Personality
            personalityLabel.text = personality.ToString().ToUpper();
            switch (personality)
            {
                case MagiPersonality.Melchior: personalityLabel.color = colorMelchior; break;
                case MagiPersonality.Balthasar: personalityLabel.color = colorBalthasar; break;
                case MagiPersonality.Casper: personalityLabel.color = colorCasper; break;
            }

            // Set Color for message too for cohesive look
            messageText.color = personalityLabel.color;
            messageText.text = message;

            // Simple Fade In/Out with Typing
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(ShowMessageRoutine(message));
        }

        private IEnumerator ShowMessageRoutine(string fullMessage)
        {
            displayGroup.alpha = 1f;
            messageText.text = "";

            // Typing effect
            foreach (char c in fullMessage)
            {
                messageText.text += c;
                yield return new WaitForSeconds(0.03f); // Fast NERV typing
            }

            yield return new WaitForSeconds(3.0f); // Stay visible
            
            // Fade out
            float duration = 1.0f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                displayGroup.alpha = 1f - (t / duration);
                yield return null;
            }
            displayGroup.alpha = 0f;
        }
    }
}
