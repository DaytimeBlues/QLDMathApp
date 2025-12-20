using QLDMathApp.Architecture.Events;
using QLDMathApp.Architecture;
using System.Collections;

namespace QLDMathApp.Modules.Magi
{
    /// <summary>
    /// FOREST HELPER SYSTEM: Provides friendly feedback and guidance.
    /// Owl: Wise/Instructional (Rational)
    /// Bunny: Encouraging/Kind (Nurturing)
    /// Cat: Clever/Advice (Intuitive)
    /// </summary>
    public class NatureHelperSystem : MonoBehaviour, IInitializable
    {
        public bool IsInitialized { get; private set; }

        public IEnumerator Initialize()
        {
            yield return null; // Ready immediately
            IsInitialized = true;
            Debug.Log("[NatureHelperSystem] System Ready.");
        }
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
                if (responseTime < 1500f)
                {
                    EventBus.OnGuideSpoke?.Invoke(GuidePersonality.WiseOwl, "Whoo! You and the forest are working in perfect harmony.");
                }
                else
                {
                    EventBus.OnGuideSpoke?.Invoke(GuidePersonality.KindBunny, "What a wonderful job! You found the answer so carefully.");
                }
            }
            else
            {
                EventBus.OnGuideSpoke?.Invoke(GuidePersonality.CuriousCat, "Mew! Maybe try looking at the very middle of the clearing next time.");
                EventBus.OnGuideSpoke?.Invoke(GuidePersonality.WiseOwl, "Let's take a deep breath and look at the fireflies again, little sprout.");
            }
        }
    }
}
