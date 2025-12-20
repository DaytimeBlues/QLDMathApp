using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using UnityEngine.Serialization;

namespace QLDMathApp.Modules.Counting
{
    /// <summary>
    /// LUNCHBOX SLOTS: Dynamic slots for the forest picnic basket.
    /// Provides visual scaffolding to help children count items into the basket.
    /// </summary>
    public class LunchboxSlot : MonoBehaviour
    {
        [Header("Basket References")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField, FormerlySerializedAs("signalSlotPrefab")] private GameObject itemSlotPrefab; 
        [SerializeField, FormerlySerializedAs("plugInterfaceImage")] private Image basketImage;
        [SerializeField, FormerlySerializedAs("initializationParticles")] private ParticleSystem charmParticles;
        
        [Header("Garden Interface Styling")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField, FormerlySerializedAs("activeHighlightColor")] private Color activeHighlightColor = new Color(1f, 1f, 0.7f);
        
        private List<Transform> _slots = new List<Transform>();
        private List<DraggableItem> _packedItems = new List<DraggableItem>();

        public void ResetSlots(int count)
        {
            // Clear existing
            foreach (var slot in _slots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _slots.Clear();
            _packedItems.Clear();
            
            // Create new slots (visual scaffold showing how many to pack)
            for (int i = 0; i < count; i++)
            {
                GameObject slot = Instantiate(itemSlotPrefab, slotsContainer);
                _slots.Add(slot.transform);
                
                // Position slots in a row
                float spacing = 80f;
                float startX = -(count - 1) * spacing / 2f;
                slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * spacing, 0);
            }
        }

        public void AcceptItem(DraggableItem item, int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _slots.Count)
            {
                // Move item to slot position
                item.transform.SetParent(slotsContainer);
                item.GetComponent<RectTransform>().anchoredPosition = 
                    _slots[slotIndex].GetComponent<RectTransform>().anchoredPosition;
                
                // Hide the placeholder
                var img = _slots[slotIndex].GetComponent<Image>();
                if (img != null) img.enabled = false;
                
                // Magic Feedback
                if (charmParticles != null)
                {
                    charmParticles.transform.position = _slots[slotIndex].position;
                    charmParticles.Play();
                }
                
                _packedItems.Add(item);
                
                // Disable further dragging
                item.enabled = false;
            }
        }

        public Vector3 GetSlotPosition(int index)
        {
            if (index >= 0 && index < _slots.Count)
            {
                return _slots[index].GetComponent<RectTransform>().anchoredPosition;
            }
            return Vector3.zero;
        }

        public void Highlight(bool on)
        {
            if (basketImage != null)
            {
                basketImage.color = on ? activeHighlightColor : normalColor;
            }
        }
    }
}
