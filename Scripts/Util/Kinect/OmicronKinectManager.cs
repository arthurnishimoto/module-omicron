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

public class OmicronKinectManager : OmicronEventClient {

	public GameObject kinect2bodyPrefab;

	public Vector4 kinectSensorTransformData;
    public bool applyKinectSensorTransform;

	public bool enableBodyTracking = true;
	public bool enableSpeechRecognition = true;
	public float minimumSpeechConfidence = 0.3f;

    public int trackedBodyCount;
	Hashtable trackedBodies;

	public GameObject[] voiceCommandListeners;

	public UnityEngine.UI.Text debugText;

    public Transform floorPlane;

	// Use this for initialization
	new void Start () {
        eventOptions = EventBase.ServiceType.ServiceTypeAny;
        trackedBodies = new Hashtable ();
		InitOmicron ();
	}

	// Update is called once per frame
	void Update () {
        trackedBodyCount = trackedBodies.Count;

        Vector3 floorClipPlane = new Vector3(kinectSensorTransformData.x, kinectSensorTransformData.y, kinectSensorTransformData.z);

        if (applyKinectSensorTransform)
        {
            if( kinectSensorTransformData.w > 0 )
                transform.localPosition = new Vector3(transform.localPosition.x, kinectSensorTransformData.w, transform.localPosition.z);
            Vector3 kinectEulerAngles = transform.localEulerAngles;
            kinectEulerAngles.x = Mathf.Atan(floorClipPlane.z / floorClipPlane.y);
            transform.localEulerAngles = kinectEulerAngles;


        }
    }

    public override void OnEvent( EventData e )
	{
		if (enableBodyTracking && e.serviceType == EventBase.ServiceType.ServiceTypeMocap )
		{
			int sourceID = (int)e.sourceId;
            kinectSensorTransformData = new Vector4(e.orx, e.ory, e.orz, e.orw);
 
            if (kinectSensorTransformData.w != 0)
            {
                // transform.localPosition = new Vector3(0, kinectSensorTransformData.w, 0);
                //transform.localEulerAngles = new Vector3(kinectSensorTransformData.x * Mathf.Rad2Deg, kinectSensorTransformData.y * Mathf.Rad2Deg, kinectSensorTransformData.z * Mathf.Rad2Deg);
            }

            if ( !trackedBodies.ContainsKey( sourceID ) )
			{
                Debug.Log("Creating new tracked body " + sourceID);
				CreateBody(sourceID);
			}
		}
		else if (enableSpeechRecognition && e.serviceType == EventBase.ServiceType.ServiceTypeSpeech)
		{
			string speechString = e.getExtraDataString();
			float speechConfidence = e.posx;

			string debugText = "Received Speech: '" + speechString + "' at " +speechConfidence.ToString("F2")+ " confidence";
			debugText += "\nMin confidence: "+minimumSpeechConfidence.ToString("F2");
			//Debug.Log("Received Speech: '" + speechString + "' at " +speechConfidence+ " confidence" );
#if USING_GETREAL3D
			CallRpc("SetHUDSpeechDebugText",debugText);
#endif
			if( speechConfidence >= minimumSpeechConfidence )
			{
				foreach( GameObject voiceListeners in voiceCommandListeners )
				{
					voiceListeners.SendMessage("OnVoiceCommand", speechString);
				}
			}
		}
	}

#if USING_GETREAL3D
	[getReal3D.RPC]
	void SetHUDSpeechDebugText(string s)
	{
        if(debugText)
		    debugText.text = s;
	}
#endif
	void CreateBody( int sourceID )
	{
		GameObject body;

		body = Instantiate(kinect2bodyPrefab) as GameObject;

		body.transform.parent = transform;
		body.transform.localPosition = Vector3.zero;
		body.transform.localRotation = Quaternion.identity;
		body.GetComponent<OmicronKinectEventClient>().bodyID = sourceID;
		body.GetComponent<OmicronKinectEventClient>().kinectManager = this;
		body.layer = gameObject.layer;
		trackedBodies.Add( sourceID, body );
	}

	public void RemoveBody(int bodyID )
	{
		trackedBodies.Remove( bodyID );
	}

}
