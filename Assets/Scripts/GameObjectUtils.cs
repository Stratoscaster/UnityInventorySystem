using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameObjectUtils
{
    public static readonly string SLOT_QUANTITY_TAG = "SlotQuantity";
    public static readonly string SLOT_BACKGROUND_TAG = "SlotBackground";
    public static readonly string SLOT_SPRITE_IMAGE_TAG = "SlotSpriteImage";
    /// <summary>
    ///  Recursively search through the rootSlotDisplay GameObject, searching for any child with a tag that exactly matches childTag.
    ///  <br/><br/>
    /// The searchMode defines whether to search across all siblings of a child before continuing (horizontal search), or to go through one branch as quickly as possible (vertical search). 
    /// DepthFirst is faster for objects at the bottom of a branch, however BreadthFirst is faster for objects that have multiple siblings.
    /// If you have a complex GameObject tree with multiple branches, this method generally may be slower than using alternative methods to search.
    /// </summary>
    /// <param name="rootSlotDisplay">The GameObject to search.</param>
    /// <param name="childTag">The tag text to search for (case sensitive).</param>
    /// <param name="searchMode">The TagSearchMode to use, see docs for more info.</param>
    /// <param name="checkRootForTag">Whether to check the given rootSlotDisplay for the tag as well</param>
    /// <returns>A child GameObject matching the tag given, or null if none found.</returns>
    public static GameObject SearchForTagInChildren(GameObject rootSlotDisplay, string childTag, TagSearchMode searchMode = TagSearchMode.BreadthFirst, bool checkRootForTag = false)
    {
        if (checkRootForTag && rootSlotDisplay.CompareTag(childTag))
        {
            return rootSlotDisplay;
        }
        if (rootSlotDisplay.transform.childCount <= 0) { return null; } // If the current game object has no children, then return null (if no children, none can have tag)

        List<GameObject> childrenWithChildren = new List<GameObject>();

        // For each of the current game object's children
        for (int i = 0; i < rootSlotDisplay.transform.childCount; i++)
        {
            Transform child = rootSlotDisplay.transform.GetChild(i);
            // First, check and return if it has a tag equual to the search parameter
            if (child.CompareTag(childTag))
            {
                return child.gameObject;
            }

            // Otherwise, if it also has children, we need to search even deeper (either now for DepthFirst, or later for BreadthFirst)
            if (child.childCount > 0)
            {
                // For "BreadthFirst" mode, we wait until all children of the rootSlotDisplay have been searched before trying to go one level deeper
                if (searchMode == TagSearchMode.BreadthFirst)
                {
                    childrenWithChildren.Add(child.gameObject);

                    // For "DepthFirst" mode, we immediately go to the bottom of the GameObject node tree as fast as possible
                }
                else if (searchMode == TagSearchMode.DepthFirst)
                {
                    GameObject innerResult = SearchForTagInChildren(child.gameObject, childTag);
                    if (innerResult != null)
                    {
                        return innerResult;
                    }
                }
            }
        }

        // If we're in BreadthFirst mode, we now need to recurse one level deeper for each child that has children GameObjects
        if (searchMode == TagSearchMode.BreadthFirst)
        {
            foreach (GameObject child in childrenWithChildren)
            {
                GameObject innerResult = SearchForTagInChildren(child.gameObject, childTag);
                if (innerResult != null)
                {
                    return innerResult;
                }
            }
        }

        // Default case
        return null;
    }
}

public enum TagSearchMode
{
    BreadthFirst,
    DepthFirst
}
