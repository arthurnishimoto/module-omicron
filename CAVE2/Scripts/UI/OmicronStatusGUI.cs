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
using UnityEngine.UI;

public class OmicronStatusGUI : MonoBehaviour {

    [SerializeField]
    OmicronManager omicronManager;

    [SerializeField]
    Text uiText;

    OmicronManager.ConnectionState connectionState;

    // Use this for initialization
    void Start () {
        omicronManager = CAVE2.GetCAVE2Manager().GetComponent<OmicronManager>();
        uiText = gameObject.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
	    if(uiText != null)
        {
            if(omicronManager == null)
            {
                uiText.text = "Omicron Not Found";
                uiText.color = Color.yellow;
            }
            else
            {
                if (CAVE2.IsMaster())
                {
                    CAVE2.SendMessage(gameObject.name, "UpdateOmicronConnectionState", omicronManager.GetConnectionState());

                    if (connectionState == OmicronManager.ConnectionState.Connected)
                    {
                        uiText.text = "Connected to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.green;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.NotConnected)
                    {
                        uiText.text = "Not Connected";
                        uiText.color = Color.white;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.FailedToConnect)
                    {
                        uiText.text = "Failed to connect to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.red;
                    }
                }
                else
                {
                    if (connectionState == OmicronManager.ConnectionState.Connected)
                    {
                        uiText.text = "Master connected to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.green;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.NotConnected)
                    {
                        uiText.text = "Master Not Connected";
                        uiText.color = Color.white;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.FailedToConnect)
                    {
                        uiText.text = "Master failed to connect to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.red;
                    }
                }
            }
        }
	}

    void UpdateOmicronConnectionState(OmicronManager.ConnectionState state)
    {
        connectionState = state;
    }

    void UpdateOmicronConnectionState(int state)
    {
        // Placeholder to prevent error if CAVE2RPC Message Server is calling this function
        // In practice this shouldn't happen, since the CAVE2RPC via getReal3D will call the
        // above function

        connectionState = (OmicronManager.ConnectionState)state;
    }
}
