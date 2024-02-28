using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines an items type, and what kind of slots it can be swapped into.
/// </summary>
public enum ItemType
{
    /// <summary>
    /// Either there is no item or it does not have any type.
    /// </summary>
    Null = 0,
    /// <summary>
    /// Any general item that is usually stackable.
    /// </summary>
    Default,
    /// <summary>
    /// Any shield equipable.
    /// </summary>
    Shield,
    /// <summary>
    /// Any weapon equipable.
    /// </summary>
    Weapon,
    /// <summary>
    /// Any heltmet equipable.
    /// </summary>
    Helmet,
    /// <summary>
    /// Any chest-piece equipable.
    /// </summary>
    Chest,
    /// <summary>
    /// Anything you can put on them dogs.
    /// </summary>
    Boots,
    /// <summary>
    /// Any food (consumable) item.
    /// </summary>
    Food,
    /// <summary>
    /// This item type is used to disallow a slot from being swapped to or from <b>entirely</b>.
    /// </summary>
    NoItemTypesAllowed  
}

public enum Attributes
{
    Agility,
    Intellect,
    Stamina,
    Strength
}

public abstract class ItemObject : ScriptableObject
{
    public int Id;
    public Sprite itemSprite;
    public ItemType type;
    public string itemName;
    [TextArea(15, 20)]
    public string description;
    public ItemBuff[] buffs;

    public Item CreateItem(bool generateNewValue=true)
    {
        Item newItem = new Item(this, generateNewValue);

        return newItem;
    }

    public ItemType GetItemType()
    {
        return this.type;
    }
}

[System.Serializable]
public class Item
{
    public string itemName;
    public int Id = -1;
    public ItemBuff[] buffs;
    [Tooltip("Is Stackable will change to false if the item has buffs unless Force Stackable Even With Buffs is true.")]
    public bool forceStackableEvenWithBuffs = false;
    public bool isStackable = true;
    public bool isDroppable = true;

    public Item(ItemObject item, bool generateNewValue=true)
    {
        this.itemName = item != null ? item.name : "";
        this.Id = item != null ? item.Id : -1;

        if (item == null)
        {
            this.isStackable = false;
            return;
        }
        // Copy over buffs
        buffs = new ItemBuff[item.buffs.Length];
        
        foreach (ItemBuff buff in item.buffs)
        {
            buffs.Append(new ItemBuff(buff.attribute, buff.min, buff.max, buff.value, generateNewValue));
        }
        
        if (buffs.Length > 0 && !forceStackableEvenWithBuffs)
        {
            isStackable = false;
        }
    }

    public string GetBuffSignatures(bool verbose=false)
    {
        if (this.buffs == null || this.buffs.Length < 1) { return "[no buffs]"; }
        string result = "[";
        foreach (ItemBuff buff in this.buffs)
        {
            result += buff.ToString(false) + ", ";
        }
        result = result.Substring(0, result.Length - 1);
        result += "]";
        return result;
    }
}

[System.Serializable]
public class ItemBuff
{
    public Attributes attribute;
    public int value;
    public int min;
    public int max;

    public ItemBuff(ItemBuff buff, bool generateNewValue=false) // Copy value is default behavior
    {
        this.value = buff.value;
        this.min = buff.min;
        this.max = buff.max;
        this.attribute = buff.attribute;

        if (generateNewValue)
        {
            GenerateValue();
        }
    }

    public ItemBuff(Attributes attribute, int min, int max, int value=1, bool generateNewValue=true)
    {   
        this.attribute = attribute;
        this.min = min;
        this.max = max;

        if (generateNewValue)
        {
            GenerateValue();
        } else
        {
            this.value = value;
        }
    }

    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }

    public void SetValue(int value)
    {
        this.value = Mathf.Clamp(value, this.min, this.max);
    }

    public string ToString(bool verbose=false)
    {
        string minMax = $"<{this.min},{this.max}>";
        return $"{this.attribute}: {this.value} {(verbose ? minMax : "")}";
    }
}