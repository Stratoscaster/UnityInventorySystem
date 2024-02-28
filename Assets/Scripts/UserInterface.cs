using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;

public abstract class UserInterface : MonoBehaviour
{
    public InventoryObject inventory;

    protected Dictionary<GameObject, InventorySlot> itemsDisplayed = new Dictionary<GameObject, InventorySlot>();

    public bool EnableDraggableSlots = true;

    public Player player;

    public void OnEnable()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            LogError("No GameObject tagged as 'Player' found.");
        }
        else
        {
            player = playerObject.GetComponent<Player>();
            if (player == null)
            {
                LogError("Player GameObject did not contain a Player script component.");
            }
        }
    }

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            LogError($"AddEvent() failed: object ({obj.name}) you are attempting to add an Event & Event Callback to has no EventTrigger component. Add one and this object ({obj.name}) will work.");
            return;
        }
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        trigger.triggers.Add(eventTrigger);


        // For debugging:
        //obj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { DebugOnClickListener(); });
        //Log("# of Triggers after adding new event trigger: " + trigger.triggers.Count);
    }

    protected void InitializeEventsForSlotDisplay(GameObject slotDisplay)
    {
        AddEvent(slotDisplay, EventTriggerType.PointerClick, evnt => { OnClickSlot(slotDisplay); });
        AddEvent(slotDisplay, EventTriggerType.PointerEnter, evnt => { OnEnterSlot(slotDisplay); });
        AddEvent(slotDisplay, EventTriggerType.PointerExit, evnt => { OnExitSlot(slotDisplay); });
        AddEvent(slotDisplay, EventTriggerType.BeginDrag, evnt => { OnStartDragSlot(slotDisplay); });
        AddEvent(slotDisplay, EventTriggerType.EndDrag, evnt => { OnEndDragSlot(slotDisplay); });
        AddEvent(slotDisplay, EventTriggerType.Drag, evnt => { OnDragSlot(slotDisplay); });
    }
    
    public void DebugOnClickListener()
    {
        Log("DebugOnClickListener()");
    }

    public abstract void CreateSlots();

    //public void HandleUIDragDropEvent(PointerEventData eventData, GameObject slotDisplay)
    //{
    //    Debug.Log("HandleUIDragDropEvent called;");
    //    if (!this.itemsDisplayed.ContainsKey(slotDisplay)) {
    //        Debug.Log("Could not find InventorySlot with GameObject of name: " + slotDisplay.name);
    //        return;
    //    }
    //    InventorySlot slot = this.itemsDisplayed[slotDisplay];
    //    Debug.Log("You clicked on this slot: " + slot.GetSignature());
    //}

    public void UpdateSlots()
    {
        foreach (KeyValuePair<GameObject, InventorySlot> slotDisplay in itemsDisplayed)
        {
            //GameObject display = slotDisplay.Key;
            InventorySlot slot = slotDisplay.Value;
            Image spriteImage = slot.slotSpriteImage.GetComponent<Image>();
            if (!slot.IsEmpty())
            {
                spriteImage.sprite = inventory.database.GetItemObjectFromId(slot.GetItemId()).itemSprite;
                spriteImage.color = new Color(1,1,1,1);   // Full color & alpha
                slot.slotQuantity.GetComponent<TextMeshProUGUI>().text = slot.item.isStackable? slot.quantity.ToString("n0") : ""; // Only show quantities for stackable objects
            } else
            {
                slot.slotQuantity.GetComponent<TextMeshProUGUI>().text = "";
                spriteImage.sprite = null;
                spriteImage.color = new Color(0, 0, 0, 0);
            }
        }
    }


    public void OnClickSlot(GameObject slotDisplay)
    {
        Log("OnClickSlot() called.");
    }

    public void OnEnterSlot(GameObject slotDisplay)
    {
        if (slotDisplay == null || player.mouseDragObject == null) { return; }
        player.mouseDragObject.hoverObject = slotDisplay;
        // TODO: Add script to each slot GameObject that contains a reference to the InventorySlot it represents
        // Get all inventories in the game (not super efficient...)
        UserInterface[] inventories = Object.FindObjectsOfType<UserInterface>();
        // Find which inventory owns that gameobject (and therefore slot)
        foreach (UserInterface inventory in inventories)
        {
            if (!inventory.itemsDisplayed.ContainsKey(slotDisplay)) { continue; }
            //Debug.Log("Set hovered slot.");
            player.mouseDragObject.hoverSlot = inventory.itemsDisplayed[slotDisplay];
            break;
        }
    }

    public void OnExitSlot(GameObject slotDisplay)
    {
        if (player.mouseDragObject == null) { return; }
        player.mouseDragObject.hoverObject = null;
        player.mouseDragObject.hoverSlot = null;
    }

    public void OnStartDragSlot(GameObject slotDisplay) 
    {
        //Log("OnStartDragSlot()");
        GameObject mouseObject = new GameObject();  // Initializing a new GameObject var actually does Instantiate() automatically
        RectTransform rect = mouseObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50f,50f);
        mouseObject.transform.SetParent(transform.parent); // The UserInterface script is a component of the Canvas so we can set it as the parent
        InventorySlot slot = itemsDisplayed[slotDisplay];
        if (!slot.IsEmpty()) // If we're dragging a non-empty slot
        {
            Image slotImage = mouseObject.AddComponent<Image>();
            slotImage.sprite = inventory.database.GetItemObjectFromId(slot.GetItemId()).itemSprite;
            slotImage.raycastTarget = false; // Make the sprite image object non-interactable (it should only follow mouse)
        }

        player.mouseDragObject = new MouseDragObject(mouseObject, slot);
    }

    public void OnEndDragSlot(GameObject slotDisplay)
    {
        //Log("OnEndDragSlot()");
        //Destroy(playerObject.mouseDragObject.mouseObject);
        //playerObject.mouseDragObject = null;

        if (player.mouseDragObject.hoverSlot != null)
        {
            //inventory.SwapSlots(itemsDisplayed[slotDisplay], itemsDisplayed[player.mouseDragObject.hoverObject]); // Before equipment inventory change (also supports storage)
            // Get the parent inventory of the mouseDragObject
            //UserInterface otherInventory = player.mouseDragObject.hoverSlot.parent;
            inventory.SwapSlots(player.mouseDragObject.slot, player.mouseDragObject.hoverSlot);

        }
        else
        {
            inventory.DropItem(player.mouseDragObject.slot);
        }

        Destroy(player.mouseDragObject.mouseObject);
        player.mouseDragObject = null;
    }
    
    public void OnDragSlot(GameObject slotDisplay)
    {
        if (player.mouseDragObject == null) { return; }
        //Log("OnDragSlot()");
        player.mouseDragObject.mouseObject.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        LinkSlotsToSelf();
        CreateSlots();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSlots();
    }

    // Update parent reference on all slots of this inventory
    // so that we know which UserInterface a slot is coming from during drag
    void LinkSlotsToSelf()
    {
        foreach(InventorySlot slot in inventory.container.slots)
        {
            slot.parent = this;
        }
    }

    public virtual void Log(string message)
    {
        Debug.Log("[UserInterface] " + message);
    }

    public virtual void LogWarning(string message)
    {
        Debug.LogWarning("[UserInterface] " + message);
    }

    public virtual void LogError(string message)
    {
        Debug.LogError("[UserInterface] " + message);
    }
}

public class MouseDragObject
{
    public GameObject mouseObject;
    public InventorySlot slot;
    public InventorySlot hoverSlot;
    public GameObject hoverObject;

    public MouseDragObject(GameObject mouseObject, InventorySlot slot, InventorySlot hoverSlot = null, GameObject hoverObject = null)
    {
        this.mouseObject = mouseObject;
        this.slot = slot;
        this.hoverSlot = hoverSlot; 
        this.hoverObject = hoverObject;
    }
}