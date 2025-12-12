using System;
using UnityEngine;

namespace QLDMathApp.UI.Events
{
    /// <summary>
    /// ScriptableObject event channel for bool-parameterized events.
    /// Used for toggle states like settings visibility.
    /// </summary>
    [CreateAssetMenu(menuName = "QLDMathApp/Events/Bool Event Channel", fileName = "BoolEventChannel")]
    public class BoolEventChannelSO : ScriptableObject
    {
        public event Action<bool> Raised;

        public void Raise(bool value) => Raised?.Invoke(value);
    }
}
