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

public class CAVE2WandMocapUpdater : MonoBehaviour
{
    public int wandID = 1;

    // The wand's center position is likely not the center of the physical wand,
    // but the center of the tracking markers.
    // The WandUpdater will handle the offset between the wand center and the tracking
    // center so that the virtual and physical wand align
    public Transform virtualWand;

    Joint wandJoint;

    public Vector3 positionOffset;
    public Vector3 RotationOffset;

    void Start()
    {
        if (virtualWand && wandID == 1 && !CAVE2.UsingHMDVR())
        {
            virtualWand.localPosition = CAVE2.Input.wandTrackingOffset[wandID - 1];
        }

        // Register this gameobject as wand
        CAVE2.RegisterWandObject(wandID, gameObject);
    }

    void FixedUpdate()
    {
        // Runtime check in case of reset (i.e. update in editor)
        if (!CAVE2.IsWandRegistered(wandID, gameObject))
        {
            Debug.LogWarning("CAVE2WandMocapUpdater: Re-registering ID " + wandID);
            CAVE2.RegisterWandObject(wandID, gameObject);
        }

        if (virtualWand && virtualWand.GetComponent<Rigidbody>())
        {
            transform.localPosition = CAVE2Manager.GetWandPosition(wandID) + positionOffset;
            transform.localRotation = CAVE2Manager.GetWandRotation(wandID);
            float timeSinceLastUpdate = CAVE2Manager.GetWandTimeSinceUpdate(wandID);

            // If position and rotation are zero, wand is not tracking, disable drawing and physics
            if ( timeSinceLastUpdate > 0.5f && virtualWand.gameObject.activeSelf)
            {
                virtualWand.gameObject.SetActive(false);
            }
            else if ( timeSinceLastUpdate < 0.5f && !virtualWand.gameObject.activeSelf)
            {
                virtualWand.gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualWand == null || virtualWand.GetComponent<Rigidbody>() == null)
        {
            transform.localPosition = CAVE2.Input.GetWandPosition(wandID) + positionOffset;
            transform.localRotation = CAVE2.Input.GetWandRotation(wandID);
        }
    }

    public Vector3 GetMocapPosition()
    {
        return CAVE2.Input.GetWandPosition(wandID);
    }
}
