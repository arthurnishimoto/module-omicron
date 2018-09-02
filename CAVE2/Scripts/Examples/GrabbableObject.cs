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

public class GrabbableObject : CAVE2Interactable {

    [SerializeField]
    CAVE2.Button grabButton = CAVE2.Button.Button3;

    [SerializeField]
    CAVE2.InteractionType grabStyle = CAVE2.InteractionType.Any;

    bool usedGravity;

    [SerializeField]
    RigidbodyConstraints constraints;

    FixedJoint joint;

    [SerializeField]
    bool grabbed;

    bool wasGrabbed;

    Queue previousPositions = new Queue();

    Collider[] grabberColliders;

    int grabbingWandID;

    void Update()
    {
        UpdateWandOverTimer();

        if( CAVE2.Input.GetButtonUp(grabbingWandID, grabButton) && grabbed )
        {
            OnWandGrabRelease();
        }
    }
    void FixedUpdate()
    {
        if( grabbed )
        {
            previousPositions.Enqueue(GetComponent<Rigidbody>().position);

            // Save only the last 5 frames of positions
            if(previousPositions.Count > 5)
            {
                previousPositions.Dequeue();
            }
        }
        else if(wasGrabbed)
        {
            Vector3 throwForce = Vector3.zero;
            for (int i = 0; i < previousPositions.Count; i++ )
            {
                Vector3 s1 = (Vector3)previousPositions.Dequeue();
                Vector3 s2 = GetComponent<Rigidbody>().position;
                
                if ( previousPositions.Count > 0 )
                    s2 = (Vector3)previousPositions.Dequeue();
                throwForce += (s2 - s1);
            }
            GetComponent<Rigidbody>().AddForce(throwForce * 10, ForceMode.Impulse);
            wasGrabbed = false;
        }
    }

    new void OnWandButtonDown(CAVE2.WandEvent evt)
    {
        if( evt.button == grabButton && (evt.interactionType == grabStyle || grabStyle == CAVE2.InteractionType.Any))
        {
            OnWandGrab(CAVE2.GetWandObject(evt.wandID).transform);
            grabbingWandID = evt.wandID;
        }
    }

    void OnWandGrab(Transform grabber)
    {
        if (GetComponent<Rigidbody>() && transform.parent != grabber )
        {
            usedGravity = GetComponent<Rigidbody>().useGravity;
            GetComponent<Rigidbody>().useGravity = false;
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = grabber.GetComponentInChildren<Rigidbody>();
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;

            // Disable collisions between grabber and collider while held
            grabberColliders = grabber.root.GetComponentsInChildren<Collider>();
            foreach (Collider c in grabberColliders)
            {
                Physics.IgnoreCollision(c, GetComponent<Collider>(), true);
            }
        }
        grabbed = true;
    }

    void OnWandGrabRelease()
    {
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().useGravity = usedGravity;
            Destroy(joint);
        }

        // Re-enable collisions between grabber and collider after released
        foreach (Collider c in grabberColliders)
        {
            Physics.IgnoreCollision(c, GetComponent<Collider>(), false);
        }

        grabbed = false;
        wasGrabbed = true;
    }
}
