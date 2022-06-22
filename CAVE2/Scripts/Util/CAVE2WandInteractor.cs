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

public class CAVE2WandInteractor : MonoBehaviour {

    [SerializeField]
    int wandID = 1;

    [SerializeField]
    LayerMask wandLayerMask = -1;

    CAVE2PlayerIdentity playerID;

    [SerializeField]
    bool wandPointing;

    [SerializeField]
    bool wandTouching;

    GameObject touchingObject;

    [SerializeField]
    GameObject grabbedObject;

    // Use this for initialization
    void Start () {
        playerID = GetComponentInParent<CAVE2PlayerIdentity>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 raySource = transform.position;

        // Helper override to set wand pointer to mouse cursor
        if( CAVE2.Input.GetComponent<CAVE2AdvancedTrackingSimulator>().wandUsesHeadPosition )
        {
            raySource = transform.parent.position;
        }

        // Shoot a ray from the wand
        Ray ray = new Ray(raySource, transform.TransformDirection(Vector3.forward));
        RaycastHit hit;

        // Get the first collider that was hit by the ray
        wandPointing = Physics.Raycast(ray, out hit, 100, wandLayerMask);
        Debug.DrawLine(ray.origin, hit.point); // Draws a line in the editor

        if (grabbedObject)
        {
            CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(playerID, wandID, CAVE2.Button.None, CAVE2.InteractionType.Touching);
            ProcessButtons(grabbedObject, playerInfo);
        }

        if (wandPointing) // The wand is pointed at a collider
        {
            CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(playerID, wandID, CAVE2.Button.None, CAVE2.InteractionType.Pointing);

            // Send a message to the hit object telling it that the wand is hovering over it
            hit.collider.gameObject.SendMessage("OnWandPointing", playerInfo, SendMessageOptions.DontRequireReceiver);

            ProcessButtons(hit.collider.gameObject, playerInfo);
        }

        // Button interaction for touching is here since it needs to be in Update to correctly trigger
        if(wandTouching && touchingObject)
        {
            CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(playerID, wandID, CAVE2.Button.None, CAVE2.InteractionType.Touching);
            ProcessButtons(touchingObject, playerInfo);
        } 
    }

    void OnTriggerStay(Collider collider)
    {
        CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(playerID, wandID, CAVE2.Button.None, CAVE2.InteractionType.Touching);

        // Send a message to the hit object telling it that the wand is hovering over it
        collider.gameObject.SendMessage("OnWandTouching", playerInfo, SendMessageOptions.DontRequireReceiver);

        if (collider.GetComponent<CAVE2Interactable>())
        {
            touchingObject = collider.gameObject;
            wandTouching = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.GetComponent<CAVE2Interactable>())
        {
            touchingObject = null;
            wandTouching = false;
        }
    }

    void ProcessButtons(GameObject interactedObject, CAVE2.WandEvent playerInfo)
    {
        foreach (CAVE2.Button currentButton in CAVE2.Button.GetValues(typeof(CAVE2.Button)))
        {
            playerInfo.button = currentButton;
            
            // OnWandButtonDown
            if (CAVE2Manager.GetButtonDown(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButtonDown", playerInfo, SendMessageOptions.DontRequireReceiver);
            }

            // OnWandButton
            else if (CAVE2Manager.GetButton(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButton", playerInfo, SendMessageOptions.DontRequireReceiver);
            }

            // OnWandButtonUp
            if (CAVE2Manager.GetButtonUp(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButtonUp", playerInfo, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void ProcessButtons(int wandID, GameObject interactedObject, CAVE2.WandEvent playerInfo)
    {
        foreach (CAVE2.Button currentButton in CAVE2.Button.GetValues(typeof(CAVE2.Button)))
        {
            playerInfo.button = currentButton;

            // OnWandButtonDown
            if (CAVE2Manager.GetButtonDown(wandID, currentButton))
            {
                Debug.Log("OnWandButtonDown " + interactedObject.name + " " + currentButton.ToString());
                interactedObject.SendMessage("OnWandButtonDown", playerInfo, SendMessageOptions.DontRequireReceiver);
            }

            // OnWandButton
            else if (CAVE2Manager.GetButton(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButton", playerInfo, SendMessageOptions.DontRequireReceiver);
            }

            // OnWandButtonUp
            if (CAVE2Manager.GetButtonUp(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButtonUp", playerInfo, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public bool GrabbedObject(GameObject g)
    {
        if (grabbedObject == null)
        {
            grabbedObject = g;
            return true;
        }
        return false;
    }

    public void ReleaseObject(GameObject g)
    {
        if(grabbedObject == g)
            grabbedObject = null;
    }

    public CAVE2PlayerIdentity GetPlayerID()
    {
        return playerID;
    }

    public int GetWandID()
    {
        return wandID;
    }

    public int GetLayerMask()
    {
        return wandLayerMask;
    }
}
