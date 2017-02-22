/**************************************************************************************************
* THE OMICRON PROJECT
*-------------------------------------------------------------------------------------------------
* Copyright 2010-2016             Electronic Visualization Laboratory, University of Illinois at Chicago
* Authors:                                                                                
* Arthur Nishimoto                anishimoto42@gmail.com
*-------------------------------------------------------------------------------------------------
* Copyright (c) 2010-2016, Electronic Visualization Laboratory, University of Illinois at Chicago
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
* DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
* USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
* ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*************************************************************************************************/

using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class OmicronMocapUpdater : MonoBehaviour {

    public enum MocapObject {None, Head1, Wand1, Wand2}
    public MocapObject presetMocapObject = MocapObject.None;

	public int mocapID = 1;

    public bool useRawGetReal3D = false;
	// Update is called once per frame
	void Update () {
        switch(presetMocapObject)
        {
            case (MocapObject.Head1):
                mocapID = CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().Head1MocapID;
                break;
            case (MocapObject.Wand1):
                mocapID = CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().Wand1MocapID;
                break;
            case (MocapObject.Wand2):
                mocapID = CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().Wand2MocapID;
                break;
            default:
                mocapID = 0;
                break;
        }

        if (useRawGetReal3D)
        {
            if (mocapID == CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().Head1MocapID)
            {
                transform.localPosition = getReal3D.Input.GetSensor("Head").position;
                transform.localRotation = getReal3D.Input.GetSensor("Head").rotation;
            }
            else if (mocapID == CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().Wand1MocapID)
            {
                transform.localPosition = getReal3D.Input.GetSensor("Wand").position;
                transform.localRotation = getReal3D.Input.GetSensor("Wand").rotation;
            }
        }
        else
        {
            transform.localPosition = CAVE2Manager.GetMocapState(mocapID).position;
            transform.localRotation = CAVE2Manager.GetMocapState(mocapID).rotation;
        }
	}

	void FixedUpdate()
	{
		//if( getReal3D.Cluster.isMaster )
		//{
		//	rigidbody.MovePosition(cave2Manager.getHead(headID).position);
		//}
	}
}
