using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using UnityEngine.Serialization;

namespace QLDMathApp.Modules.Counting
{
    /// <summary>
    /// ENTRY PLUG SLOTS: Dynamic module slots for pilot interface.
    /// Slots provide visual stabilization for supply missions.
    /// </summary>
    public class EntryPlugSlot : MonoBehaviour // Renamed from LunchboxSlot
    {
        [Header("Tactical References")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField, FormerlySerializedAs("slotPrefab")] private GameObject signalSlotPrefab; 
        [SerializeField, FormerlySerializedAs("lunchboxImage")] private Image plugInterfaceImage;
        [SerializeField, FormerlySerializedAs("acceptParticles")] private ParticleSystem initializationParticles;
        
        [Header("NERV Interface Styling")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField, FormerlySerializedAs("highlightColor")] private Color activeHighlightColor = new Color(1f, 1f, 0.7f);
        
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
                GameObject slot = Instantiate(signalSlotPrefab, slotsContainer);
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
                
                // Hide the standby outline
                _slots[slotIndex].GetComponent<Image>().enabled = false;
                
                // Initialization Feedback
                if (initializationParticles != null)
                {
                    initializationParticles.transform.position = _slots[slotIndex].position;
                    initializationParticles.Play();
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
            if (plugInterfaceImage != null)
            {
                plugInterfaceImage.color = on ? activeHighlightColor : normalColor;
            }
        }
    }
}
