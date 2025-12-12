using System;
using UnityEngine;

namespace QLDMathApp.UI.Events
{
    /// <summary>
    /// ScriptableObject event channel for parameterless events.
    /// Unity's recommended pattern for decoupling UI from systems.
    /// </summary>
    [CreateAssetMenu(menuName = "QLDMathApp/Events/Void Event Channel", fileName = "VoidEventChannel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event Action Raised;

        public void Raise() => Raised?.Invoke();
    }
}
