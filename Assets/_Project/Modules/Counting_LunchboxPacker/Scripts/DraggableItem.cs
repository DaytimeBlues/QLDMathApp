using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace QLDMathApp.Modules.Counting
{
    /// <summary>
    /// NERV SUPPLY MODULE: Draggable module for Entry Plug Supply.
    /// Large touch target, smooth drag, tactical snap-back on mission failure.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("NERV Theme")]
        [SerializeField] private Architecture.UI.NERVTheme theme;

        [Header("Settings")]
        [SerializeField] private float dragScale = 1.2f;
        [SerializeField] private float snapBackSpeed = 10f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropSound;
        
        private Vector3 _startPosition;
        private Vector2 _dragOffset;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private AudioSource _audioSource;
        private bool _isDragging;
        private Vector3 _originalScale;
        
        public event Action<DraggableItem> OnConnectionEstablished; // Renamed from OnDroppedInLunchbox
        public event Action<DraggableItem> OnConnectionFailed;    // Renamed from OnDroppedOutside

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            _audioSource = GetComponentInParent<AudioSource>();
            _startPosition = _rectTransform.anchoredPosition;
            _originalScale = transform.localScale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            
            // Visual feedback: lift up
            transform.localScale = _originalScale * dragScale;
            _canvasGroup.blocksRaycasts = false; // Allow drop detection
            
            // Audio
            if (pickupSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(pickupSound);
            }
            
            // Haptic
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
            
            // Calculate drag offset
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            _dragOffset = (Vector2)_rectTransform.anchoredPosition - localPoint;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            
            _rectTransform.anchoredPosition = localPoint + (Vector2)_dragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            transform.localScale = _originalScale;
            _canvasGroup.blocksRaycasts = true;
            
            if (dropSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(dropSound);
            }
            
            // Check if dropped on Entry Plug
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            bool connectedToPlug = false;
            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<LunchboxSlot>() != null)
                {
                    connectedToPlug = true;
                    break;
                }
            }
            
            if (connectedToPlug)
            {
                OnConnectionEstablished?.Invoke(this);
            }
            else
            {
                OnConnectionFailed?.Invoke(this);
            }
        }

        public void ReturnToStart()
        {
            StartCoroutine(AnimateReturn());
        }

        private System.Collections.IEnumerator AnimateReturn()
        {
            Vector3 startPos = _rectTransform.anchoredPosition;
            float distance = Vector3.Distance(startPos, _startPosition);
            float duration = distance / snapBackSpeed;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                _rectTransform.anchoredPosition = Vector3.Lerp(startPos, _startPosition, t / duration);
                yield return null;
            }
            
            _rectTransform.anchoredPosition = _startPosition;
        }

        /// <summary>
        /// EXPLANATORY FEEDBACK: Animate item moving to target for demo.
        /// </summary>
        public System.Collections.IEnumerator AnimateDemoMove(Vector3 targetPosition)
        {
            Vector3 startPos = _rectTransform.anchoredPosition;
            float duration = 1f;
            
            // Lift
            transform.localScale = _originalScale * dragScale;
            yield return new WaitForSeconds(0.2f);
            
            // Move
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                _rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPosition, t / duration);
                yield return null;
            }
            
            // Drop
            _rectTransform.anchoredPosition = targetPosition;
            transform.localScale = _originalScale;
            
            if (dropSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(dropSound);
            }
        }
    }
}
