/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2022		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2022, Electronic Visualization Laboratory, University of Illinois at Chicago
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

public class CAVE2HeadMarker : MonoBehaviour {

    [SerializeField]
    int headID = 1;

    LineRenderer headToGroundLine;
    LineRenderer forwardLine;

    [SerializeField]
    bool showLine = false;

    [SerializeField]
    Material lineMaterial = null;

    // Use this for initialization
    void Start () {
        if (!CAVE2.OnCAVE2Display() && !CAVE2.UsingHMDVR())
        {
            headToGroundLine = gameObject.AddComponent<LineRenderer>();
#if UNITY_5_5_OR_NEWER
            headToGroundLine.startWidth = 0.02f;
            headToGroundLine.endWidth = 0.02f;
            headToGroundLine.generateLightingData = true;
#else
            headToGroundLine.SetWidth(0.02f, 0.02f);
#endif
            headToGroundLine.material = lineMaterial;

        
            GameObject forwardReference = new GameObject("Head-ForwardRef");
            forwardReference.transform.parent = transform;
            forwardLine = forwardReference.AddComponent<LineRenderer>();
#if UNITY_5_5_OR_NEWER
            forwardLine.startWidth = 0.02f;
            forwardLine.endWidth = 0.02f;
            forwardLine.generateLightingData = true;
#else
            forwardLine.SetWidth(0.02f, 0.02f);
#endif
            forwardLine.material = lineMaterial;
        }

        CAVE2.RegisterHeadObject(headID, gameObject);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        // Runtime check in case of reset (i.e. update in editor)
        if (!CAVE2.IsHeadRegistered(headID, gameObject))
        {
            Debug.LogWarning("CAVE2HeadMarker: Re-registering ID " + headID);
            CAVE2.RegisterHeadObject(headID, gameObject);
        }

        transform.localPosition = CAVE2.GetHeadPosition(headID);
        transform.localRotation = CAVE2.GetHeadRotation(headID);

        if (forwardLine && headToGroundLine)
        {
            forwardLine.enabled = showLine;

            headToGroundLine.SetPosition(0, new Vector3(transform.position.x, transform.parent.position.y, transform.position.z));
            headToGroundLine.SetPosition(1, transform.position);

        
            forwardLine.SetPosition(0, transform.position);
            forwardLine.SetPosition(1, transform.position + transform.forward * 0.2f);
        }
    }
}
