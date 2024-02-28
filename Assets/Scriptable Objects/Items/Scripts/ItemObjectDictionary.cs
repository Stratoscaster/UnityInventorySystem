using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemObjectDictionary : ScriptableObject
{
    private Dictionary<string, ItemObjectDictionaryEntry> dictionary = new Dictionary<string, ItemObjectDictionaryEntry>();

    private static ItemObjectDictionary instance;

    private static readonly string DEFAULT_PREFAB_NAME = "DefaultGroundItem";
    private ItemObjectDictionary()
    {
    }

    private void OnEnable()
    {
        InitEntries();
    }

    private void InitEntries()
    {
        // Create instantiation of prefabs with the PrefabManager in your
        // main project & attach them to dictionary entries
        GameObject defaultPrefab = Resources.Load(DEFAULT_PREFAB_NAME) as GameObject;
        if (defaultPrefab == null)
        {
            LogWarning("Could not locate 'DefaultGroundItem' prefab in Resources folder. " +
                "This prefab is recommended as a fallback for when a prefab cannot be found.");
        } else
        {
            UpsertEntry(DEFAULT_PREFAB_NAME, ItemType.Default, "The default prefab.", defaultPrefab);
        }
    }

    public ItemObjectDictionaryEntry UpsertEntry(string name, ItemType type = ItemType.Null, 
                                                 string description = null, GameObject prefab = null)
    {
        if (name == null) { return null; }
        ItemObjectDictionaryEntry entry;
        if (dictionary.ContainsKey(name) && dictionary[name] != null)
        {
            // Get existing entry

            entry = dictionary[name];
            // Check if each component is specified (not null), and if it is, then set it to the new value
            entry.type = type != ItemType.Null ? type : entry.type;
            entry.description = description != null ? description : entry.description;
            entry.prefab = prefab !=null ? prefab : entry.prefab;
        } else
        {
            // Create new entry

            entry = new ItemObjectDictionaryEntry(name, type, description, prefab);
        }


        dictionary.Add(name, entry);
        return entry;
    }

    public ItemObjectDictionaryEntry GetEntry(string name, bool getDefaultPrefab=false)
    {
        if (getDefaultPrefab)
        {
            return dictionary.ContainsKey(DEFAULT_PREFAB_NAME) ? dictionary[DEFAULT_PREFAB_NAME] : null;
        }
        if (!dictionary.ContainsKey(name)) { return null; }
        return dictionary[name];
    }

    public static ItemObjectDictionary GetInstance()
    {
        if (instance == null)
        {
            instance = ScriptableObject.CreateInstance<ItemObjectDictionary>();
        }

        return instance;
    }

    public void Log(string message) { Debug.Log("[ItemObjectDictionary] " + message); }
    public void LogWarning(string message) { Debug.LogWarning("[ItemObjectDictionary] " + message); } 
    public void LogError(string message) { Debug.LogError("[ItemObjectDictionary] " + message); }
}

public class ItemObjectDictionaryEntry
{
    public string name;
    public ItemType type;
    public string description;
    public GameObject prefab;  
    public ItemObjectDictionaryEntry(string name, ItemType type, string description, GameObject prefab)
    {
        this.name = name;
        this.type = type;
        this.description = description;
        this.prefab = prefab;
    }
}
