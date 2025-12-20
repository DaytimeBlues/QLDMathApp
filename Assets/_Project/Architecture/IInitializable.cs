using System.Collections;

namespace QLDMathApp.Architecture
{
    /// <summary>
    /// Interface for services that require asynchronous initialization.
    /// Used by AppBootstrapper to ensure all systems are ready before game start.
    /// </summary>
    public interface IInitializable
    {
        bool IsInitialized { get; }
        IEnumerator Initialize();
    }
}
