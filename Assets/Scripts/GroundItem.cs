using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundItem : MonoBehaviour
{
    public ItemObject item;
    public bool isPickedUp = false;

    public bool isDropped = false;
    public float timeSinceDropped;
    public float timeToWaitToPickUpAfterDropping = 3f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Only allow the player to pick the item back up X seconds after dropping. Used in Player class.
        if (isDropped)
        {
            timeSinceDropped += Time.deltaTime;
            if (timeSinceDropped >= timeToWaitToPickUpAfterDropping)
            {
                isDropped = false;
                timeSinceDropped = 0f;
            }
        }
    }
}
