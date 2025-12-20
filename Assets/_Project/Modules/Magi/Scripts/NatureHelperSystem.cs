using UnityEngine;
using QLDMathApp.Architecture.Events;

namespace QLDMathApp.Modules.Magi
{
    /// <summary>
    /// FOREST HELPER SYSTEM: Provides friendly feedback and guidance.
    /// Owl: Wise/Instructional (Rational)
    /// Bunny: Encouraging/Kind (Nurturing)
    /// Cat: Clever/Advice (Intuitive)
    /// </summary>
    public class NatureHelperSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.OnAnswerAttempted += ConsultHelpers;
        }
        
        private void OnDisable()
        {
            EventBus.OnAnswerAttempted -= ConsultHelpers;
        }
        
        private void ConsultHelpers(bool isCorrect, float responseTime)
        {
            if (isCorrect)
            {
                // Successful blooming
                if (responseTime < 1500f)
                {
                    // Owl (Wise) triggers on quick mastery
                    EventBus.OnHelperConsulted?.Invoke(ForestHelper.Owl, "Whoo! You and the forest are working in perfect harmony.");
                }
                else
                {
                    // Bunny (Kind) triggered on successful but slower answer
                    EventBus.OnHelperConsulted?.Invoke(ForestHelper.Bunny, "What a wonderful job! You found the answer so carefully.");
                }
            }
            else
            {
                // Soft support - Cat provides clever advice
                EventBus.OnHelperConsulted?.Invoke(ForestHelper.Cat, "Mew! Maybe try looking at the very middle of the clearing next time.");
                
                // Owl provides a gentle reminder
                EventBus.OnHelperConsulted?.Invoke(ForestHelper.Owl, "Let's take a deep breath and look at the fireflies again, little sprout.");
            }
        }
    }
}
