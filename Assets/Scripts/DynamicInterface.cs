using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DynamicInterface : UserInterface
{
    public GameObject inventoryPrefab;

    public int X_SPACE_BETWEEN_ITEMS;
    public int Y_SPACE_BETWEEN_ITEMS;
    public int NUMBER_OF_COLUMNS;

    public float SLOT_WIDTH = 50f;
    public float SLOT_HEIGHT = 50f;

    public Vector2 PADDING = new Vector2(10f, 10f);

    public override void CreateSlots()
    {
        itemsDisplayed = new Dictionary<GameObject, InventorySlot>();   // Wipe our display
        int slotNumber = 0;
        foreach (InventorySlot slot in inventory.container.slots)
        {
            InitializeDynamicSlotDisplays(slot, slotNumber);
            slotNumber++;
        }
    }

    public GameObject InitializeDynamicSlotDisplays(InventorySlot slot, int slotNumber)
    {
        // Instantiate the slot GameObject
        GameObject slotDisplay = Instantiate(inventoryPrefab,
                Vector3.zero, Quaternion.identity, transform);

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
            // Initialize slot display sprite
            ItemObject slotItemObject = inventory.database.GetItemObjectFromId(itemId);
            Sprite itemSprite = slotItemObject.itemSprite;
            if (itemSprite == null)
            {
                LogError($"Item sprite for item not specified: " + slot.GetSignature());
            }

            slot.slotSpriteImage.GetComponent<Image>().sprite = itemSprite;
        }

        // Set the UI Rect parameters
        RectTransform slotRect = slotDisplay.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0, 1);
        slotRect.anchorMax = new Vector2(0, 1);
        Vector3 itemPosition = GetItemPosition(slotNumber);
        Debug.Log("Initiating slot at postion: " + itemPosition);
        slotRect.anchoredPosition = itemPosition;

        // Initialize slot quantity text
        int slotQuantity = slot.quantity;
        string quantityText = slotQuantity > 0 ? slotQuantity.ToString("n0") : "";

        slot.slotQuantity.GetComponent<TextMeshProUGUI>().text = quantityText;

        // Initialize Event Triggers for Slot Display GameObject
        if (this.EnableDraggableSlots)
        {
            InitializeEventsForSlotDisplay(slotDisplay);
        }
        else
        {
            Log("Draggable Slots are DISABLED.");
        }

        slotDisplay.name = $"InventorySlot {slotNumber + 1}";


        itemsDisplayed.Add(slotDisplay, slot);
        return slotDisplay;
    }

    private Vector3 GetItemPosition(int itemIndex)
    {
        float xPosition = (SLOT_WIDTH + X_SPACE_BETWEEN_ITEMS) * (itemIndex % NUMBER_OF_COLUMNS) + PADDING.x;
        float yPosition = (-Y_SPACE_BETWEEN_ITEMS - SLOT_HEIGHT) * (itemIndex / NUMBER_OF_COLUMNS) - PADDING.y;
        return new Vector3(xPosition, yPosition, 0f);
    }

    new public void Log(string message)
    {
        Debug.Log("[DynamicInterface] " + message);
    }

    new public void LogWarning(string message)
    {
        Debug.LogWarning("[DynamicInterface] " + message);
    }

    new public void LogError(string message)
    {
        Debug.LogError("[DynamicInterface] " + message);
    }
}
