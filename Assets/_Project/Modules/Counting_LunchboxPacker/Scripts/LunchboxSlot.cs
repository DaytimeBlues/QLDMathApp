using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace QLDMathApp.Modules.Counting
{
    /// <summary>
    /// The lunchbox target area with dynamic slots.
    /// Slots appear based on target count, providing visual scaffold.
    /// </summary>
    public class LunchboxSlot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab; // Dashed outline slot
        [SerializeField] private Image lunchboxImage;
        [SerializeField] private ParticleSystem acceptParticles;
        
        [Header("Styling")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.7f);
        
        private List<Transform> _slots = new List<Transform>();
        private List<DraggableItem> _packedItems = new List<DraggableItem>();

        public void ResetSlots(int count)
        {
            // Clear existing
            foreach (var slot in _slots)
            {
                Destroy(slot.gameObject);
            }
            _slots.Clear();
            _packedItems.Clear();
            
            // Create new slots (visual scaffold showing how many to pack)
            for (int i = 0; i < count; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotsContainer);
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
                
                // Hide the dashed outline
                _slots[slotIndex].GetComponent<Image>().enabled = false;
                
                // Particles
                if (acceptParticles != null)
                {
                    acceptParticles.transform.position = _slots[slotIndex].position;
                    acceptParticles.Play();
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
            if (lunchboxImage != null)
            {
                lunchboxImage.color = on ? highlightColor : normalColor;
            }
        }
    }
}
