using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using UnityEngine.Rendering.Universal.Internal;
using System.Linq;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject //, ISerializationCallbackReceiver
{
    public string savePath;
    public ItemDatabaseObject database;
    public bool debugInventoryOnAdd = true;
    public Inventory container;
    public ItemObjectDictionary itemObjectDictionary;

    private Player player;

    public void SetPlayer(Player player)
    {
        this.player = player;
    }

    public void OnEnable()
    {
        itemObjectDictionary = ItemObjectDictionary.GetInstance();
    }

    public bool PickUpItem(GroundItem groundItem)
    {
        //If the groundItem has not already been picked up (prevent double pickups), and was not just dropped recently, pick it up

        if (groundItem != null && groundItem.item != null  && database.IsItemObjectInDatabase(groundItem.item) && !groundItem.isPickedUp && !groundItem.isDropped)
        {
            groundItem.isPickedUp = true;
            return AddItem(groundItem.item, 1);
        }
        return false;
    }

    public bool AddItem(ItemObject item, int quantity)
    {
        return AddItem(new Item(item), quantity);
    } 

    public bool AddItem(Item item, int quantity)
    { 
        if (!item.isStackable)
        {
            InventorySlot filledSlot = SetNextEmptySlot(item, quantity);
            if (filledSlot == null)
            {
                return false;   // If no slot available, return false picked-up condition
            }
            return true;
        } else
        {
            InventorySlot slot = container.GetSlotWithItem(item.Id);
            if (slot == null)
            {
                InventorySlot filledSlot = SetNextEmptySlot(item, quantity);
                if (filledSlot == null)
                {
                    return false;
                }
                return true;
            } else
            {
                slot.AddAmount(quantity);
                return true;
            }
        }

        /*
        if (debugInventoryOnAdd)
        {
            string debugString = $"INV ({container.slots.Length} slot{(container.slots.Length > 1 ? "s" : "")}):\n";
            foreach (InventorySlot slt in container.slots)
            {
                debugString += $"\t{slt.GetSignature()}";
            }
            Debug.Log(debugString);
        }
        */
    }

    public bool SwapSlots(InventorySlot first, InventorySlot second, bool bypassSlotAllowedTypes=false)
    {
        ItemType firstType = !first.IsEmpty() ? database.GetItemObjectFromId(first.item.Id).GetItemType() : ItemType.Null;
        ItemType secondType = !second.IsEmpty() ? database.GetItemObjectFromId(second.item.Id).GetItemType() : ItemType.Null;

        // If the slot does not have any specified allowances, assume it allows all groundItem types
        bool firstAllowsAllItemTypes = first.allowedItemTypes.Count == 0;
        bool secondAllowsAllItemTypes = second.allowedItemTypes.Count == 0;

        // If the other slot is empty, or this slot accept the other slots items, and this slot doesn't disallow all groundItem types, then yes
        bool firstSlotAllowsSecondItem = (second.IsEmpty() || first.allowedItemTypes.Contains(secondType)) && !second.allowedItemTypes.Contains(ItemType.NoItemTypesAllowed);
        bool secondSlotAllowsFirstItem = (first.IsEmpty() || second.allowedItemTypes.Contains(firstType)) && !second.allowedItemTypes.Contains(ItemType.NoItemTypesAllowed);

        
        // If all groundItem types allowed, or if slot allows specified items, then yes
        bool firstToSecondSwapAllowed = firstAllowsAllItemTypes || firstSlotAllowsSecondItem;
        bool secondToFirstSwapAllowed = secondAllowsAllItemTypes || secondSlotAllowsFirstItem;

        if (!(firstToSecondSwapAllowed && secondToFirstSwapAllowed) && !bypassSlotAllowedTypes) { return false; }

        // Create a swap space to temporarily hold the groundItem while they are swapped
        InventorySlot swapSpace = new InventorySlot(first.item, first.quantity);

        // Swap time
        first.UpdateSlot(second.item, second.quantity);
        second.UpdateSlot(swapSpace.item, swapSpace.quantity);

        return true;
    }

    public void DropItem(InventorySlot slot)
    {
        Item itemToDrop = slot.item;
        int quantityToDrop = slot.quantity;

        slot.ClearSlot();

        if (!itemToDrop.isDroppable) { return; }

        ItemObjectDictionaryEntry entry = itemObjectDictionary.GetEntry(itemToDrop.itemName);
        if (entry == null)
        {
            entry = itemObjectDictionary.GetEntry("", true);    // Get the default prefab as a fallback
        } 
        if (entry == null) { 
            itemObjectDictionary.LogError("Could not find default prefab or prefab for item with name: " + itemToDrop.itemName);
            return;
        }

        GameObject itemPrefab = entry.prefab;

        ItemObject itemObject = database.GetItemObjectFromId(itemToDrop.Id);

        for (int i = 0; i < quantityToDrop; i++)
        {
            GameObject droppedItem = Instantiate(itemPrefab, player.transform.position, Quaternion.identity);
            GroundItem groundItem = droppedItem.GetComponent<GroundItem>();
            groundItem.item = itemObject;
            groundItem.isDropped = true;
            groundItem.isPickedUp = false;
        }
    }

    public InventorySlot SetNextEmptySlot(Item item, int quantity)
    {
        InventorySlot emptySlot = GetNextEmptySlot();
        if (emptySlot == null) { return null; }
        emptySlot.UpdateSlot(item, quantity);

        return emptySlot;
    }

    private InventorySlot GetNextEmptySlot()
    {
        foreach (InventorySlot slot in container.slots)
        {
            if (slot.IsEmpty())
            {
                return slot;
            }
        }

        return null;
    }

    public InventorySlot GetItemSlot(Item item)
    {
        foreach (InventorySlot slot in container.slots)
        {
            if (slot.item.Id == item.Id)
            {
                return slot;
            }
        }
        return null;
    }

    // Saving and loading data from binary-written save files (safe tamper-proof method)

    [ContextMenu("Save")]
    public void Save()
    {
        ValidateSavePath();
        // Non-tampering-safe method
        string filePath = string.Concat(Application.persistentDataPath, savePath);
        string saveData = JsonUtility.ToJson(container, true);
        Debug.Log("Saving the following InventoryObject data:\n" + saveData);
        ////using System.Runtime.Serialization.Formatters.Binary;
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(filePath);
        //bf.Serialize(file, saveData);
        //file.Close();

        //Tamper-proof method
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, container);
        stream.Close();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        container = new Inventory();
    }

    private void ValidateSavePath()
    {
        if (savePath.StartsWith('/')) { return; }
        savePath = '/' + savePath;
    }

    [ContextMenu("Load")]
    public void Load()
    {
        ValidateSavePath();
        string filePath = string.Concat(Application.persistentDataPath, savePath);
        if (!File.Exists(filePath))
        {
            Clear();
            Debug.Log("No save file was found.");
            return;
        }
        //// For Using non-tamper-proof method
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Open(filePath, FileMode.Open);
        //string fileJson = bf.Deserialize(file).ToString();
        //Debug.Log("Loading the following InventoryObject data:\n" + fileJson);
        //JsonUtility.FromJsonOverwrite(fileJson, this);
        //file.Close();

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        Inventory newContainer = (Inventory)formatter.Deserialize(stream);

        for (int i = 0; i < newContainer.slots.Length; i++)
        {
            if (i >= container.slots.Length)
            {
                Debug.LogWarning($"InventoryObject could not load {newContainer.slots.Length - container.slots.Length} slots in from save file due to slot capacity limit. DATA WILL BE LOST IF YOU SAVE.");
                return;
            }
            InventorySlot loadedSlot = newContainer.slots[i];
            container.slots[i].UpdateSlot(loadedSlot.item, loadedSlot.quantity);
        }

        stream.Close();
    }

    public bool HasItem(Item item)
    {
        return container.ContainsItem(item.Id);
    }

    // Not needed due to using IFormatter
    //public void OnAfterDeserialize()
    //{
    //    for (int i = 0; i < container.items.Count; i++)
    //    {
    //        container.items[i].groundItem = database.GetItemObjectFromId(container.items[i].ID);
    //    }
    //}

    //public void OnBeforeSerialize()
    //{
    //    // Unused
    //}

//    private void OnEnable()
//    {
//        // Check if in UnityEditor
//#if UNITY_EDITOR
//        // Load the database manually in editor context
//        database = AssetDatabase.LoadAssetAtPath<ItemDatabaseObject>("Assets/Resources/ItemDatabase.asset");
//#else
//        database = Resources.Load<ItemDatabaseObject>("ItemDatabase");
//#endif

//    }
}
[System.Serializable]
public class Inventory
{
    public InventorySlot[] slots;

    public Inventory()
    {
        this.slots = new InventorySlot[24];
    }

    public void Clear()
    {
        this.slots = new InventorySlot[24];
    }

    public bool ContainsItem(int itemId)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.GetItemId() == itemId)
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsItem(string itemName)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.GetItemName() == itemName)
            {
                return true;
            }
        }
        return false;
    }



    public InventorySlot GetSlotWithItem(string itemName)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.GetItemName() == itemName)
            {
                return slot;
            }
        }

        return null;
    }

    public InventorySlot GetSlotWithItem(int id)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.GetItemId() == id)
            {
                return slot;
            }
        }

        return null;
    }
}