/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2018		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2018, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
 
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

    public void OnEvent(TouchPoint touchPoint)
    {
        Vector3 screenPosition = RawTouchPosToCanvasCoords(touchPoint.GetPosition());
        int touchID = touchPoint.GetID();
        //Debug.Log(touchPoint.GetPosition() + " " + touchPoint.GetID() + " " + touchPoint.GetGesture());

        if (!touchList.ContainsKey(touchID))
        {
            if (touchPoint.GetGesture() == EventBase.Type.Down)
            {
                GameObject visualMarker = Instantiate(touchPointPrefab);
                visualMarker.name = "TouchPoint " + touchID;
                visualMarker.transform.SetParent(transform);

                // Update position with new touch data
                visualMarker.transform.position = screenPosition;
                visualMarker.transform.localScale = Vector3.one * 10;

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
                visualMarker.transform.localScale = Vector3.one * 10;

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

    public override void OnEvent(EventData e)
    {
        //Debug.Log("OmicronEventClient: '" + name + "' received " + e.serviceType);
        TouchPoint touchPoint = new TouchPoint(e);
        OnEvent(touchPoint);
    }
}
