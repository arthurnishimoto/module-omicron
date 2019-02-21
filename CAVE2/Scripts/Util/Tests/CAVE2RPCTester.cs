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

public class CAVE2RPCTester : MonoBehaviour {

    Text uiText;

    int testMode = 0;

    // Use this for initialization
    void Start()
    {
        uiText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (uiText != null)
        {
            if(CAVE2.IsMaster())
            {
                if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.Button5))
                {
                    switch(testMode)
                    {
                        case (0):
                            CAVE2.SendMessage(gameObject.name, "ProcessRPCEvent");
                            break;
                        case (1):
                            CAVE2.SendMessage(gameObject.name, "ProcessRPCEvent1", 1);
                            break;
                        case (2):
                            CAVE2.SendMessage(gameObject.name, "ProcessRPCEvent2", 2, "Two");
                            break;
                        case (3):
                            CAVE2.SendMessage(gameObject.name, "ProcessRPCEvent3", 3, "Three", new Vector3(1, 2, 3));
                            break;
                        case (4):
                            CAVE2.SendMessage(gameObject.name, "ProcessRPCEvent4", 1, "Two", new Vector3(3, 3.2f, 3.3f), 4.0f);
                            break;
                        case (5):
                            CallRPCFromFunction();
                            break;
                    }
                    testMode++;
                    if (testMode >= 6)
                        testMode = 0;
                }
            }
            
        }
    }

    public void CallRPCFromFunction()
    {
        CAVE2.SendMessage(gameObject.name, "ProcessRPCEventFunc");
    }

    void ProcessRPCEvent()
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (No Param)";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (No Param)";
        }
    }

    void ProcessRPCEventFunc()
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (No Param) from Function()";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (No Param) from Function()";
        }
    }

    void ProcessRPCEvent1(int id)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (1 Param)\n";
            uiText.text += "param[0]: " + id;
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (1 Param)\n";
            uiText.text += "param[0]: " + id;
        }
    }

    void ProcessRPCEvent2(object[] param)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (2 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1];
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (2 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1];
        }
    }

    void ProcessRPCEvent3(object[] param)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (3 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (3 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
        }
    }

    void ProcessRPCEvent4(object[] param)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (4 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
            uiText.text += "param[3]: " + param[3] + "\n";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (4 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
            uiText.text += "param[3]: " + param[3] + "\n";
        }
    }
}
