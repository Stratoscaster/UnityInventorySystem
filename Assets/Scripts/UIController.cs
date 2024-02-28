using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIController : MonoBehaviour
{
    public abstract void HandlePointerEvent(PointerEventType eventType, PointerEventData eventDat, GameObject eventTarget);
    public abstract List<PointerEventType> GetSubscribedPointerEventTypes(); 

}
