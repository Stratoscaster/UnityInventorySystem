using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] items;
    private Dictionary<ItemObject, int> itemObjectToId = new Dictionary<ItemObject, int>();

    public int GetId(ItemObject item)
    {
        if (!itemObjectToId.ContainsKey(item)) { return -1; }
        return itemObjectToId[item];
    }

    public ItemObject GetItemObjectFromId(int id)
    {
        if (id < 0) { return null; }
        if (id >= items.Length) {
            Log("GetItemObjectFromId() - id exceeded items length: " + items.ToString());
            return null; 
        }
        return items[id];
    }

    public bool IsItemObjectInDatabase(ItemObject query)
    {
        return items == null || items.Contains(query);
    }

    public bool IsItemIdInDatabase(int id)
    {
        return itemObjectToId.Values.Contains(id);
    }

    public void OnAfterDeserialize()
    {
        int currentId = 0;
        foreach (var item in items)
        {
            if (item == null) continue;
            item.Id = currentId;
            itemObjectToId.Add(item, currentId);
            currentId++;
        }
    }

    public void OnBeforeSerialize()
    {
        itemObjectToId = new Dictionary<ItemObject, int>(); // Initialize new dict
    }

    public void Log(string message)
    {
        Debug.Log("[ItemDatabaseObject] " + message);
    }
}
