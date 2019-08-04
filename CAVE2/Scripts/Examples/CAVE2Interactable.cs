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

public class CAVE2Interactable : MonoBehaviour {

    [SerializeField]
    protected bool wandPointing = false;

    [SerializeField]
    protected bool wandTouching = false;

    protected float lastWandTouchingTime;
    protected float lastWandPointingTime;

    float wandOverTimeout = 0.05f;

    void Update()
    {
        UpdateWandOverTimer();
    }

    protected void UpdateWandOverTimer()
    {
        if (Time.time - lastWandTouchingTime > wandOverTimeout)
        {
            wandTouching = false;
        }
        if (Time.time - lastWandPointingTime > wandOverTimeout)
        {
            wandPointing = false;
        }
    }

    public void OnWandButtonDown(CAVE2.WandEvent evt)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt[0];
        //int wandID = (int)evt[1];
        //CAVE2.Button button = (CAVE2.Button)evt[2];


        //Debug.Log("OnWandButtonDown: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandButton(CAVE2.WandEvent evt)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt[0];
        //int wandID = (int)evt[1];
        //CAVE2.Button button = (CAVE2.Button)evt[2];


        //Debug.Log("OnWandButton: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandButtonUp(CAVE2.WandEvent evt)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt[0];
        //int wandID = (int)evt[1];
        //CAVE2.Button button = (CAVE2.Button)evt[2];


        //Debug.Log("OnWandButtonUp: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandTouching(CAVE2.WandEvent eventInfo)
    {
        OnWandTouchEvent();
    }

    public void OnWandTouching()
    {
        OnWandTouchEvent();
    }

    protected void OnWandTouchEvent()
    {
        lastWandTouchingTime = Time.time;
        wandTouching = true;
    }

    public void OnWandPointing(CAVE2.WandEvent eventInfo)
    {
        OnWandPointingEvent();
    }

    protected void OnWandPointingEvent()
    {
        lastWandPointingTime = Time.time;
        wandPointing = true;
    }

    /*
    public void OnWandButtonDown(CAVE2.Button button)
    {
        // Deprecated - Legacy Support Only
    }

    public void OnWandButton(CAVE2.Button button)
    {
        // Deprecated - Legacy Support Only
    }

    public void OnWandButtonUp(CAVE2.Button button)
    {
        // Deprecated - Legacy Support Only
    }*/
}
