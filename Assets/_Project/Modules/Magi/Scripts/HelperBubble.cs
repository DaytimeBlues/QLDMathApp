using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QLDMathApp.Architecture.Events;
using System.Collections;

namespace QLDMathApp.Modules.Magi
{
    /// <summary>
    /// HELPER BUBBLE: Visualizes feedback from the Forest Guides (Owl, Bunny, Cat).
    /// Uses TextMeshPro and maxVisibleCharacters for efficient, GC-friendly typing.
    /// </summary>
    public class HelperBubble : MonoBehaviour
    {
        [Header("UI References (TMPro)")]
        [SerializeField] private TMP_Text agentNameLabel;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private CanvasGroup displayGroup;

        [Header("Helper Styling")]
        [SerializeField] private Color colorOwl = new Color(0.1f, 0.8f, 1f);     // Sky Blue
        [SerializeField] private Color colorBunny = new Color(1f, 0.6f, 0.8f);  // Pink
        [SerializeField] private Color colorCat = new Color(1f, 0.8f, 0.2f);    // Gold

        private Coroutine _fadeRoutine;

        private void Start()
        {
            if (displayGroup != null) displayGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            EventBus.OnGuideSpoke += HandleGuideSpoke;
        }

        private void OnDisable()
        {
            EventBus.OnGuideSpoke -= HandleGuideSpoke;
        }

        private void HandleGuideSpoke(GuidePersonality personality, string message)
        {
            if (agentNameLabel == null || messageText == null) return;

            // Set Agent Name and Color
            agentNameLabel.text = personality.ToString().ToUpper();
            switch (personality)
            {
                case GuidePersonality.WiseOwl: agentNameLabel.color = colorOwl; break;
                case GuidePersonality.KindBunny: agentNameLabel.color = colorBunny; break;
                case GuidePersonality.CuriousCat: agentNameLabel.color = colorCat; break;
            }

            // Simple Fade In/Out with Efficient Typing
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(ShowMessageRoutine(message));
        }

        private IEnumerator ShowMessageRoutine(string fullMessage)
        {
            displayGroup.alpha = 1f;
            
            // AUDIT FIX (Performance): Use maxVisibleCharacters instead of string concatenation
            // This avoids thousands of string allocations per second.
            messageText.text = fullMessage;
            messageText.maxVisibleCharacters = 0;

            int totalVisibleCharacters = fullMessage.Length;
            int counter = 0;

            while (counter <= totalVisibleCharacters)
            {
                messageText.maxVisibleCharacters = counter;
                counter++;
                yield return new WaitForSeconds(0.02f); // Gentle forest typing
            }

            yield return new WaitForSeconds(3.0f); // Stay visible
            
            // Fade out
            float duration = 0.5f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                displayGroup.alpha = 1f - (t / duration);
                yield return null;
            }
            displayGroup.alpha = 0f;
        }
    }
}
