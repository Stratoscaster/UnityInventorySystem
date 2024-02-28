using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class StaticInterface : UserInterface
{
    public GameObject[] slots;  // Head, Body, Left, Right, and Feet (in that order)

    public List<ItemType> allowedItemTypes = new List<ItemType>();
    public bool isForEquipment = true;
    public List<ItemType> equipmentOrder = new List<ItemType> { 
        ItemType.Helmet,
        ItemType.Chest,
        ItemType.Weapon,
        ItemType.Shield,
        ItemType.Boots
    };
    public override void CreateSlots()
    {
        itemsDisplayed = new Dictionary<GameObject, InventorySlot>();
        int slotNumber = 0;

        List<ItemType>.Enumerator equipmentEnumerator = equipmentOrder.GetEnumerator();
        foreach (InventorySlot slot in inventory.container.slots)
        {
            if (slotNumber >= slots.Length) { break; }
            slot.ClearSlot();
            GameObject slotDisplay = slots[slotNumber];

            if (isForEquipment && equipmentEnumerator.MoveNext())
            {
                slot.SetAllowedItemTypes(new List<ItemType> { equipmentEnumerator.Current });
            } else if (isForEquipment && !equipmentEnumerator.MoveNext()) {
                LogError("Equipment-based static inventory has run out of equipment types for equipment order." +
                    "\nIf this StaticInventory is not supposed to be used for equipment, set 'Is For Equipment' parameter to false in inspector.");
            } else
            {
                slot.SetAllowedItemTypes(allowedItemTypes);
            }
            

            InitializeStaticSlotDisplays(slotDisplay, slot, slotNumber);

            slotNumber++;
        }
    }

    private void InitializeStaticSlotDisplays(GameObject slotDisplay, InventorySlot slot, int slotNumber)
    {
        // Populate references to slotBackground, slotSpriteImage, and slotQuantity
        GameObject slotBackground = GameObjectUtils.SearchForTagInChildren(slotDisplay, GameObjectUtils.SLOT_BACKGROUND_TAG, checkRootForTag: true);

        if (slotBackground == null)
        {
            LogError($"Slot #{slotNumber} did not contain a GameObject with the tag '{GameObjectUtils.SLOT_BACKGROUND_TAG}'.");
        }
        else
        {
            slot.slotBackground = slotBackground;
        }

        GameObject slotSpriteImage = GameObjectUtils.SearchForTagInChildren(slotDisplay, GameObjectUtils.SLOT_SPRITE_IMAGE_TAG, checkRootForTag: true);
        if (slotSpriteImage == null)
        {
            LogError($"Slot #{slotNumber} did not contain a GameObject with the tag '{GameObjectUtils.SLOT_SPRITE_IMAGE_TAG}'.");
        }
        else
        {
            slot.slotSpriteImage = slotSpriteImage;
        }

        GameObject slotQuantityObject = GameObjectUtils.SearchForTagInChildren(slotDisplay, GameObjectUtils.SLOT_QUANTITY_TAG, checkRootForTag: true);
        if (slotQuantityObject == null)
        {
            LogError($"Slot #{slotNumber} did not contain a GameObject with the tag '{GameObjectUtils.SLOT_QUANTITY_TAG}'.");
        }
        else
        {
            slot.slotQuantity = slotQuantityObject;
        }


        int itemId = slot.item.Id;
        if (!slot.IsEmpty())
        {
            // Initialize slot display sprite if it isn't empty
            ItemObject slotItemObject = inventory.database.GetItemObjectFromId(itemId);
            Sprite itemSprite = slotItemObject.itemSprite;
            if (itemSprite == null)
            {
                LogError($"Item sprite for item not specified: " + slot.GetSignature());
            }

            slot.slotSpriteImage.GetComponent<Image>().sprite = itemSprite;
        }

        // Initialize slot quantity text
        int slotQuantity = slot.quantity;
        string quantityText = slotQuantity > 0 ? slotQuantity.ToString("n0") : "";

        slot.slotQuantity.GetComponent<TextMeshProUGUI>().text = quantityText;


        if (this.EnableDraggableSlots)
        {
            InitializeEventsForSlotDisplay(slotDisplay);
        } else
        {
            Log("Draggable Slots are DISABLED");
        }


        itemsDisplayed.Add(slotDisplay, slot);
    }

    new public void Log(string message)
    {
        Debug.Log("[StaticInterface] " + message);
    }

    new public void LogWarning(string message)
    {
        Debug.LogWarning("[StaticInterface] " + message);
    }

    new public void LogError(string message)
    {
        Debug.LogError("[StaticInterface] " + message);
    }
}
