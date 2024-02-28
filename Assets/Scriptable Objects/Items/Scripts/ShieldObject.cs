using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Shield Object", menuName = "Inventory System/Items/Shield")]
public class ShieldObject : ItemObject
{
    private void Awake()
    {
        type = ItemType.Shield;
    }
}
