using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))] // Assumes on Canvas root
public class SimpleCanvasTouch : OmicronEventClient {
    public GameObject touchPointPrefab;

    Hashtable touchList;

    RectTransform canvasRect;

    public bool debug;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypePointer;
        InitOmicron();

        canvasRect = GetComponent<RectTransform>();
        touchList = new Hashtable();
    }

    Vector3 RawTouchPosToCanvasCoords(Vector3 touchPos)
    {
        Vector2 posInCanvasCoords = touchPos;

        posInCanvasCoords.x *= canvasRect.rect.width;
        posInCanvasCoords.y = canvasRect.rect.height - (posInCanvasCoords.y * canvasRect.rect.height);

        return posInCanvasCoords;
    }

    Vector3 RawTouchSizeToCanvasCoords(Vector3 touchPos)
    {
        Vector2 posInCanvasCoords = touchPos;

        posInCanvasCoords.x *= canvasRect.rect.width * 0.1f;
        posInCanvasCoords.y *= canvasRect.rect.height * 0.1f;

        return posInCanvasCoords;
    }

    void SendTouchToEventSystem(PointerEventData pointerEvent)
    {
        // Raycast into the Event system
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEvent, raycastResults);

        // Feed event to UI objects in raycast
        foreach (RaycastResult result in raycastResults)
        {
            ExecuteEvents.ExecuteHierarchy(result.gameObject, pointerEvent, ExecuteEvents.submitHandler);
        }
    }

    void OnEvent(EventData e)
    {
        
        TouchPoint touchPoint = new TouchPoint(e);
        Vector3 screenPosition = RawTouchPosToCanvasCoords(touchPoint.GetPosition());
        int touchID = touchPoint.GetID();
        Vector3 size = RawTouchSizeToCanvasCoords(new Vector3(e.getExtraDataFloat(0), e.getExtraDataFloat(1), 1));
        if( size.magnitude == 0 )
        {
            size = Vector3.one;
        }

        if( debug )
        {
            if (touchPoint.GetGesture() != EventBase.Type.Move)
            {
                Debug.Log("OmicronEventClient: '" + name + "' received " + e.serviceType + " id: " + e.sourceId);
                Debug.Log(touchPoint.GetGesture());
            }
        }


        if (!touchList.ContainsKey(touchID))
        {
            if (touchPoint.GetGesture() == EventBase.Type.Down)
            {
                GameObject visualMarker = Instantiate(touchPointPrefab);
                visualMarker.name = "TouchPoint " + e.sourceId;
                visualMarker.transform.SetParent(transform);

                // Update position with new touch data
                visualMarker.transform.position = screenPosition;
                visualMarker.transform.localScale = size * 10;

                touchPoint.SetObjectTouched(visualMarker);
                touchPoint.Update(screenPosition, EventBase.Type.Down);
                touchList.Add(touchID, touchPoint);
            }
        }
        else
        {
            if (touchPoint.GetGesture() == EventBase.Type.Move)
            {
                // Get the existing touch data
                TouchPoint existingTouchPoint = (TouchPoint)touchList[touchID];
                GameObject visualMarker = existingTouchPoint.GetObjectTouched();

                // Update position with new touch data
                visualMarker.transform.position = RawTouchPosToCanvasCoords(touchPoint.GetPosition());
                visualMarker.transform.localScale = size * 10;

                existingTouchPoint.Update(screenPosition, EventBase.Type.Move);

                touchList[touchID] = existingTouchPoint;
                
            }
            else if (touchPoint.GetGesture() == EventBase.Type.Up)
            {
                
                // Get the existing touch data
                TouchPoint existingTouchPoint = (TouchPoint)touchList[touchID];
                GameObject visualMarker = existingTouchPoint.GetObjectTouched();
                existingTouchPoint.Update(screenPosition, EventBase.Type.Up);

                // Remove the TouchPoint
                Destroy(visualMarker);
                touchList.Remove(touchID);
            }
        }
    }
}
