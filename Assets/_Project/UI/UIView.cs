using UnityEngine;

namespace QLDMathApp.UI
{
    /// <summary>
    /// Base class for UI views managed by the navigation stack.
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Called when the view is shown.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Called when the view is hidden.
        /// </summary>
        public virtual void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the view receives data.
        /// </summary>
        public virtual void SetData(object data)
        {
            // Override in derived classes to handle specific data types
        }
    }
}
