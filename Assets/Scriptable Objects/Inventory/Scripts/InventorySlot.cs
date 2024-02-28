using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class InventorySlot
{
    //public int ID;
    public Item item;
    public int quantity;
    public List<ItemType> allowedItemTypes = new List<ItemType>();
    public UserInterface parent;

    // Holds references to GameObject children of the slot with tags of equal name. (See UserInterface.InitializeSlots() for example)
    [DoNotSerialize] public GameObject slotBackground;
    [DoNotSerialize] public GameObject slotSpriteImage;
    [DoNotSerialize] public GameObject slotQuantity;

    public void SetAllowedItemTypes(List<ItemType> allowedItemTypes)
    { 
        this.allowedItemTypes = allowedItemTypes != null ? allowedItemTypes : new List<ItemType>();
    }

    public InventorySlot()
    {
        this.item = new Item(null, false);
        this.quantity = 0;
    }

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public void UpdateSlot(Item item, int quantity)
    {
        //if (item == null || quantity <= 0)
        //{
        //    LogWarning("You are clearing an inventory slot by setting its object to null or quantity <0. It is recommended to use the InventorySlot().ClearSlot() function to do this.");
        //}
        this.item = item;
        this.quantity = quantity;
    }


    public void ClearSlot()
    {
        this.item.Id = -1;
        this.quantity = 0;
    }

    public bool IsEmpty()
    {
        return this.item == null || this.item.Id <= -1 || this.quantity <= 0;
    }

    public void AddAmount(int value)
    {
        quantity += value;
    }

    public int GetItemId()
    {
        return this.item != null ? this.item.Id : -1;
    }

    public string GetItemName()
    {
        return this.item != null ? this.item.itemName : null;
    }

    public void Log(string message)
    {
        Debug.Log("[InventoryObject] " + message);
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning("[InventoryObject] " + message);
    }

    public string GetSignature(bool verbose = false, bool verboseBuffs = false)
    {
        string name = !this.IsEmpty() ? this.item.itemName : "EMPTY";
        string id = !this.IsEmpty() ? this.item.Id.ToString() : "-1";
        string amt = !this.IsEmpty() ? this.quantity.ToString() : "0";
        return $"{name}#{id} ({amt}) {(verbose ? this.item.GetBuffSignatures(verboseBuffs) : "")}";
    }
}
