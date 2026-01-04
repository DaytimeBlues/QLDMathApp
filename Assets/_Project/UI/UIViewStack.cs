using System.Collections.Generic;
using UnityEngine;

namespace QLDMathApp.UI
{
    /// <summary>
    /// Manages a stack of UI views for navigation.
    /// </summary>
    public class UIViewStack : MonoBehaviour
    {
        private Stack<UIView> _viewStack = new Stack<UIView>();

        /// <summary>
        /// Push a new view onto the stack and show it.
        /// </summary>
        public void Push(UIView view, object data = null)
        {
            if (view == null)
            {
                Debug.LogError("[UIViewStack] Cannot push null view.");
                return;
            }

            // Hide current view if any
            if (_viewStack.Count > 0)
            {
                var currentView = _viewStack.Peek();
                currentView.Hide();
            }

            // Show new view
            _viewStack.Push(view);
            view.SetData(data);
            view.Show();

            Debug.Log($"[UIViewStack] Pushed {view.GetType().Name}, stack depth: {_viewStack.Count}");
        }

        /// <summary>
        /// Pop the current view and return to the previous one.
        /// </summary>
        public void Pop()
        {
            if (_viewStack.Count == 0)
            {
                Debug.LogWarning("[UIViewStack] Cannot pop from empty stack.");
                return;
            }

            var view = _viewStack.Pop();
            view.Hide();

            // Show previous view if any
            if (_viewStack.Count > 0)
            {
                var previousView = _viewStack.Peek();
                previousView.Show();
            }

            Debug.Log($"[UIViewStack] Popped {view.GetType().Name}, stack depth: {_viewStack.Count}");
        }

        /// <summary>
        /// Clear all views from the stack.
        /// </summary>
        public void Clear()
        {
            while (_viewStack.Count > 0)
            {
                var view = _viewStack.Pop();
                view.Hide();
            }

            Debug.Log("[UIViewStack] Cleared all views.");
        }

        /// <summary>
        /// Get the current view on top of the stack.
        /// </summary>
        public UIView CurrentView => _viewStack.Count > 0 ? _viewStack.Peek() : null;

        /// <summary>
        /// Get the current stack depth.
        /// </summary>
        public int Count => _viewStack.Count;
    }
}
