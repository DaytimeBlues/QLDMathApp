using System;
using UnityEngine;

namespace QLDMathApp.UI.Events
{
    /// <summary>
    /// ScriptableObject event channel for string-parameterized events.
    /// Used for scene names, identifiers, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "QLDMathApp/Events/String Event Channel", fileName = "StringEventChannel")]
    public class StringEventChannelSO : ScriptableObject
    {
        public event Action<string> Raised;

        public void Raise(string value) => Raised?.Invoke(value);
    }
}
