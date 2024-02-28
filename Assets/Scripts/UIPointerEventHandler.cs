using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPointerEventHandler : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerMoveHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private UIController controller;

    public void SetUIController(UIController controller) {
        this.controller = controller;
    }

    private UIController GetUIController()
    {
        return this.controller;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ProcessPointerEvent(eventData, PointerEventType.PointerClick);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ProcessPointerEvent(eventData, PointerEventType.PointerDown);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        ProcessPointerEvent(eventData, PointerEventType.PointerMove);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ProcessPointerEvent(eventData, PointerEventType.PointerUp);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ProcessPointerEvent(eventData, PointerEventType.PointerEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ProcessPointerEvent(eventData, PointerEventType.PointerExit);
    }

    public void ProcessPointerEvent(PointerEventData eventData, PointerEventType eventType)
    {
        if (controller == null)
        {
            Debug.Log("DisplayInventory not set on UIPointerEventHandler.");
            return;
        }
        if (!controller.GetSubscribedPointerEventTypes().Contains(eventType)) { return; }

        controller.HandlePointerEvent(eventType, eventData, gameObject);
    }

    public void DebugTrigger()
    {
        Debug.Log("Fired!");
    }


}

public enum PointerEventType
{
    PointerClick,
    PointerDown,
    PointerUp,
    PointerMove,
    PointerEnter,
    PointerExit,
    
}
