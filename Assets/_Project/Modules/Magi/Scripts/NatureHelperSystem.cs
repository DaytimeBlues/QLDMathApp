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
                // Successful blooming mastery
                if (responseTime < 1500f)
                {
                    // Rational Agent (Owl) triggers on quick mastery
                    EventBus.OnAgentFeedbackRequested?.Invoke(PedagogicalAgent.Rational, "Whoo! You and the forest are working in perfect harmony.");
                }
                else
                {
                    // Nurturing Agent (Bunny) triggered on successful but slower answer
                    EventBus.OnAgentFeedbackRequested?.Invoke(PedagogicalAgent.Nurturing, "What a wonderful job! You found the answer so carefully.");
                }
            }
            else
            {
                // Soft support - Intuitive Agent (Cat) provides clever advice
                EventBus.OnAgentFeedbackRequested?.Invoke(PedagogicalAgent.Intuitive, "Mew! Maybe try looking at the very middle of the clearing next time.");
                
                // Rational Agent (Owl) provides a gentle reminder
                EventBus.OnAgentFeedbackRequested?.Invoke(PedagogicalAgent.Rational, "Let's take a deep breath and look at the fireflies again, little sprout.");
            }
        }
    }
}
