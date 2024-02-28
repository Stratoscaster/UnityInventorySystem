using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public InventoryObject inventory;

    public MouseDragObject mouseDragObject = new MouseDragObject(null, null);    // Temporary

    public void OnTriggerEnter(Collider other)
    {
        GroundItem item = other.GetComponent<GroundItem>();

        bool wasPickedUp = inventory.PickUpItem(item);
        if (wasPickedUp)
        {
            Destroy(other.gameObject);
        }
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            inventory.Save();
        } else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            inventory.Load();
        }
    }

    public void OnEnable()
    {
        inventory.SetPlayer(this);
    }

    private void OnApplicationQuit()
    {
        inventory.container.Clear();    // Clear on play end
    }
}
