using UnityEngine;
using UnityEngine.UI;
using System;

namespace QLDMathApp.Modules.Hub
{
    /// <summary>
    /// Individual node on the adventure map.
    /// Neo-Skeuomorphic styling with locked/unlocked states.
    /// </summary>
    public class MapNode : MonoBehaviour
    {
        [Header("Node Info")]
        [SerializeField] private int nodeIndex;
        [SerializeField] private string activitySceneName;
        [SerializeField] private Sprite nodeIcon;
        
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private Image glowRing; // For current node
        [SerializeField] private Transform avatarAnchor;
        [SerializeField] private ParticleSystem unlockParticles;
        
        [Header("Styling")]
        [SerializeField] private Color unlockedColor = new Color(1f, 0.9f, 0.7f);
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color currentGlowColor = new Color(1f, 0.8f, 0.3f);
        
        private bool _isUnlocked;
        private bool _isCurrent;
        private Animator _animator;
        
        public event Action<MapNode> OnNodeSelected;
        
        public int NodeIndex => nodeIndex;
        public string ActivitySceneName => activitySceneName;
        public bool IsUnlocked => _isUnlocked;
        public Vector3 AvatarPosition => avatarAnchor != null ? avatarAnchor.position : transform.position;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            button.onClick.AddListener(HandleClick);
        }

        public void Initialize(int index, bool unlocked, bool current)
        {
            nodeIndex = index;
            _isUnlocked = unlocked;
            _isCurrent = current;
            
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Icon
            if (iconImage != null && nodeIcon != null)
            {
                iconImage.sprite = nodeIcon;
            }
            
            // Lock state
            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(!_isUnlocked);
            }
            
            // Color
            if (iconImage != null)
            {
                iconImage.color = _isUnlocked ? unlockedColor : lockedColor;
            }
            
            // Glow for current
            if (glowRing != null)
            {
                glowRing.gameObject.SetActive(_isCurrent);
                if (_isCurrent)
                {
                    glowRing.color = currentGlowColor;
                    // Could add pulsing animation here
                }
            }
            
            // Button interactivity
            button.interactable = _isUnlocked;
        }

        private void HandleClick()
        {
            if (!_isUnlocked)
            {
                // Play "locked" wobble animation
                if (_animator != null)
                {
                    _animator.SetTrigger("Wobble");
                }
                return;
            }
            
            OnNodeSelected?.Invoke(this);
        }

        public void PlayUnlockAnimation()
        {
            _isUnlocked = true;
            
            if (_animator != null)
            {
                _animator.SetTrigger("Unlock");
            }
            
            if (unlockParticles != null)
            {
                unlockParticles.Play();
            }
            
            UpdateVisuals();
        }

        public void SetAsCurrent(bool isCurrent)
        {
            _isCurrent = isCurrent;
            
            if (glowRing != null)
            {
                glowRing.gameObject.SetActive(isCurrent);
            }
        }
    }
}
