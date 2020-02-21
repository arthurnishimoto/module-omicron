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

public class OmicronSpeechManager : OmicronEventClient {
	public float minimumSpeechConfidence = 0.3f;

	public GameObject[] voiceCommandListeners;

	public UnityEngine.UI.Text debugText;

    // [SerializeField]
    // float audioEnergy;

    [SerializeField]
    float audioAngle;

    [SerializeField]
    float angleConfidence;

    Vector3[] recentAngles = new Vector3[10];
    int currentIndex;

	// Use this for initialization
	new void Start () {
        eventOptions = EventBase.ServiceType.ServiceTypeAny;
		InitOmicron ();
	}

	// Update is called once per frame
	void Update () {
        for(int i = 0; i < recentAngles.Length; i++)
        {
            Debug.DrawLine(transform.position,  transform.position + transform.localRotation * recentAngles[i], Color.magenta, 5);
        }
    }

    public override void OnEvent( EventData e )
	{
		if (e.serviceType == EventBase.ServiceType.ServiceTypeSpeech)
		{
			string speechString = e.getExtraDataString();
			float speechConfidence = e.posx;
            float speechAngle = e.posy;
            float angleConfidence = e.posz;

            string debugText = "Received: '" + speechString + "' at " +speechConfidence.ToString("F2")+ " confidence";
            debugText += "\nAngle: " + speechAngle.ToString("F2") + " at " + angleConfidence.ToString("F2") + "confidence";
            debugText += "\nMin confidence: "+minimumSpeechConfidence.ToString("F2");
            //Debug.Log("Received Speech: '" + speechString + "' at " +speechConfidence+ " confidence" );

            CAVE2.SendMessage(gameObject.name, "SetHUDSpeechDebugText", debugText);

			if( speechConfidence >= minimumSpeechConfidence )
			{
				foreach( GameObject voiceListeners in voiceCommandListeners )
				{
					voiceListeners.SendMessage("OnVoiceCommand", speechString);
				}
			}
		}
        else if (e.serviceType == EventBase.ServiceType.ServiceTypeAudio)
        {
            // audioEnergy = e.posx;
            audioAngle = e.posy;
            angleConfidence = e.posz;

            // string debugText = "Received: " + audioEnergy + " db at angle " + audioAngle.ToString("F2") + " at " + angleConfidence.ToString("F2") + " confidence";
            // debugText += "\nMin confidence: " + minimumSpeechConfidence.ToString("F2");
            //Debug.Log("Received Speech: '" + speechString + "' at " +speechConfidence+ " confidence" );

            //CAVE2.SendMessage(gameObject.name, "SetHUDSpeechDebugText", debugText);

            Vector3 angleVector = Vector3.zero;
            angleVector.x = angleConfidence * Mathf.Sin(-audioAngle * Mathf.Deg2Rad);
            angleVector.z = angleConfidence * Mathf.Cos(-audioAngle * Mathf.Deg2Rad);

            recentAngles[currentIndex] = angleVector;
            currentIndex++;
            if(currentIndex > recentAngles.Length - 1)
            {
                currentIndex = 0;
            }
        }
    }

	void SetHUDSpeechDebugText(string s)
	{
        if(debugText)
		    debugText.text = s;
	}
}
