using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public Image[] slotImages; // Assign in Inspector
    public Sprite[] itemIcons; // Assign item icons in Inspector

    private int selectedSlot = 0;

    void Update()
    {
        // Select slot with number keys
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SelectSlot(i);
            }
        }
    }

    public void SetSlotIcon(int slot, Sprite icon)
    {
        if (slot >= 0 && slot < slotImages.Length)
            slotImages[slot].sprite = icon;
    }

    void SelectSlot(int slot)
    {
        selectedSlot = slot;
        // Highlight selected slot (e.g., change color)
        for (int i = 0; i < slotImages.Length; i++)
            slotImages[i].color = (i == slot) ? Color.white : Color.grey;
    }
}
