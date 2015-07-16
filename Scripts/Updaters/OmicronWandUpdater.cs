/**************************************************************************************************
* THE OMICRON PROJECT
*-------------------------------------------------------------------------------------------------
* Copyright 2010-2015             Electronic Visualization Laboratory, University of Illinois at Chicago
* Authors:                                                                                
* Arthur Nishimoto                anishimoto42@gmail.com
*-------------------------------------------------------------------------------------------------
* Copyright (c) 2010-2015, Electronic Visualization Laboratory, University of Illinois at Chicago
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

public class OmicronWandUpdater : MonoBehaviour {
	public int wandID = 1;

	CAVE2Manager cave2Manager;

	public void InitOmicron()
	{
		cave2Manager = GameObject.Find ("CAVE2-InputManager").GetComponent<CAVE2Manager> ();
	}

	// Use this for initialization
	public void Start () {
		InitOmicron ();
	}
	
	// Update is called once per frame
	void Update() {

			if( !cave2Manager.wandMousePointerEmulation )
			{
				if( GetComponent<Rigidbody>() )
				{
					GetComponent<Rigidbody>().MovePosition( CAVE2Manager.GetWandPosition(wandID) );
					GetComponent<Rigidbody>().MoveRotation( CAVE2Manager.GetWandRotation(wandID) );
				}
				else
				{
					transform.localPosition = CAVE2Manager.GetWandPosition(wandID);
					transform.localRotation = CAVE2Manager.GetWandRotation(wandID);
				}
			}
			else // Mouse pointer mode
			{
				if( GetComponent<Rigidbody>() )
				{
					GetComponent<Rigidbody>().MovePosition( CAVE2Manager.GetWandPosition(wandID) );
				}
				else
				{
					transform.localPosition = CAVE2Manager.GetWandPosition(wandID);
				}

				// Mouse pointer ray controls rotation
				Vector2 position = new Vector3( Input.mousePosition.x, Input.mousePosition.y );
				
				// Ray extending from main camera into screen from touch point
				Ray ray = Camera.main.ScreenPointToRay(position);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 100))
				{
					//transform.LookAt( hit.point );
				}
				else
				{
					//transform.LookAt( ray.GetPoint(1000) );
				}
				transform.LookAt( ray.GetPoint(1000) );
				// Update the wandSate rotation (opposite of normal since this object is determining the rotation)
				cave2Manager.wandEmulatedRotation = transform.eulerAngles;
			}
	}
}
