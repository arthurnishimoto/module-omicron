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

public class KinectHandGrabber : MonoBehaviour {

	public int handState;

	//int lastKnownHandState;

	public bool grabbing;
	public bool holdingObject;
	
	public bool hasGrabableObject;
	public GameObject grabableObject;
	public Transform originalParent;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if( handState == (int)OmicronKinectEventClient.KinectHandState.Open )
		{
			//lastKnownHandState = (int)OmicronKinectEventClient.KinectHandState.Open;
			grabbing = false;

			if( holdingObject )
			{
				ReleaseObject();
			}
		}
		else if( handState == (int)OmicronKinectEventClient.KinectHandState.Closed )
		{
			//lastKnownHandState = (int)OmicronKinectEventClient.KinectHandState.Closed;
			grabbing = true;
		}

	}

	void OnTriggerStay( Collider other )
	{

	}

	void SelectGrabbableObject( GameObject otherGameObject )
	{
		grabableObject = otherGameObject;
	}

	void GrabObject()
	{
		originalParent = grabableObject.transform.parent;
		grabableObject.GetComponent<Rigidbody>().isKinematic = true;
		
		grabableObject.transform.parent = transform;
		
		//grabJoint = grabableObject.AddComponent<FixedJoint>();
		//grabJoint.connectedBody = rigidbody;
		//grabJoint.breakForce = Mathf.Infinity;
		//grabJoint.breakTorque = Mathf.Infinity;
		
		holdingObject = true;
	}

	void ReleaseObject()
	{
		grabableObject.GetComponent<Rigidbody>().isKinematic = false;
		if( originalParent != transform )
			grabableObject.transform.parent = originalParent;
		else
			grabableObject.transform.parent = null;
		//grabJoint = null;
		
		holdingObject = false;
		hasGrabableObject = false;
		grabableObject = null;
	}
}
