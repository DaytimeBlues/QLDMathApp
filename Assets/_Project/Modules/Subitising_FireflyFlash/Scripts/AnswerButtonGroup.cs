using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace QLDMathApp.Modules.Subitising
{
    /// <summary>
    /// NERV TERMINAL GROUP: Manages high-tech pilot response terminals.
    /// Large touch targets (min 60x60 units) for steady pilot input.
    /// </summary>
    public class AnswerButtonGroup : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private List<AnswerButton> buttons;
        
        [Header("Settings")]
        [SerializeField] private bool shufflePositions = true;
        
        private AngelInterceptController _controller;

        private void Awake()
        {
            _controller = GetComponentInParent<AngelInterceptController>();
        }

        /// <summary>
        /// Setup buttons with correct answer and distractors.
        /// </summary>
        public void SetupButtons(int correctValue, int[] distractors)
        {
            List<int> allValues = new List<int> { correctValue };
            allValues.AddRange(distractors);
            
            // Limit to available buttons
            while (allValues.Count > buttons.Count)
            {
                allValues.RemoveAt(allValues.Count - 1);
            }
            
            if (shufflePositions)
            {
                ShuffleList(allValues);
            }
            
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i < allValues.Count)
                {
                    buttons[i].gameObject.SetActive(true);
                    buttons[i].SetValue(allValues[i]);
                    buttons[i].OnPressed = OnButtonPressed;
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }

        public void EnableButtons(bool enabled)
        {
            foreach (var button in buttons)
            {
                button.SetInteractable(enabled);
            }
        }

        private void OnButtonPressed(int value)
        {
            _controller?.OnAnswerSelected(value);
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
