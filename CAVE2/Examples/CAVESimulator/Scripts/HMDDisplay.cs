/**************************************************************************************************
 * HMDDisplay.cs
 *
 * Turns the attached object into a virtual reality display. Assumes parent object has a
 * VRDisplayManager to get the tracked head position and the virtual world head position.
 * 
 * Specialized version of CAVE2Display, allows display to move (as if attached to head)
 *-------------------------------------------------------------------------------------------------
 * Copyright 2018   		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2018, Electronic Visualization Laboratory, University of Illinois at Chicago
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

public class HMDDisplay : CAVE2Display
{
    [SerializeField]
    bool applyHeadOffsetToScreenPosition = false;

    [SerializeField]
    Transform head2 = null;

    [SerializeField]
    bool otherTrackedPerspective = false;

    [SerializeField]
    Vector3 screenOffset = Vector3.zero;

    [SerializeField]
    Vector3 headOffsetModifer = Vector3.zero;

    [SerializeField]
    Vector3 screenOffsetModifier = Vector3.zero;

    [Header("Debug Analysis")]
    [SerializeField]
    Vector3 headLocation = Vector3.zero;

    [SerializeField]
    Vector3 head2Location = Vector3.zero;

    // Update is called once per frame
    void Update () {
        headLocation = head.localPosition;
        if(head2)
        {
            head2Location = head2.localPosition;
        }

        if (applyHeadOffsetToScreenPosition)
        {
            screenOffset = head.localPosition;
            // Remove initial offset between head and display height
            screenOffset.y -= (displayInfo.Px_UpperLeft.y + displayInfo.Px_LowerLeft.y) / 2.0f;
        }

        Vector3 screenOffset2 = Vector3.zero;
        if (head2 && otherTrackedPerspective)
        {
            screenOffset2 = head2.localPosition;
            // Remove initial offset between head and display height
            screenOffset2.y -= (displayInfo.Px_UpperLeft.y + displayInfo.Px_LowerLeft.y) / 2.0f;

            //headOffset = screenOffset2 - screenOffset + headOffsetModifer;
            //screenOffset += headOffset;
            headOffset = screenOffset + headOffsetModifer;
            screenOffset += headOffset;
        }

        screenUL = displayInfo.Px_UpperLeft + screenOffset + screenOffsetModifier;
        screenLL = displayInfo.Px_LowerLeft + screenOffset + screenOffsetModifier;
        screenLR = displayInfo.Px_LowerRight + screenOffset + screenOffsetModifier;

        if (vrCamera)
        {
            vrCamera.transform.localEulerAngles = transform.parent.localEulerAngles;
        }
    }

    // Ignore this for AR HMDs
    public new void SetVRDisplayMask(LayerMask newMask)
    {

    }
}
