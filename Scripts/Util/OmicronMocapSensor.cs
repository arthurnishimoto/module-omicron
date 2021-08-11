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

public class OmicronMocapSensor : OmicronEventClient
{
    public int sourceID = 1; // -1 for any

    [SerializeField] Vector3 position;
    [SerializeField] Quaternion orientation;
    Vector3 positionMod = Vector3.one;

    [SerializeField] float timeSinceLastUpdate;
    float lastPosDeltaMagnitude;
    float lastRotDeltaMagnitude;

    Vector3 lastPosition;
    // Quaternion lastRotation;

    int updateEvents;
    float updateTimer;

    [SerializeField]
    float updateLatency;

    [SerializeField]
    CAVE2RPCManager.MsgType sendDataMode = CAVE2RPCManager.MsgType.Unreliable;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeMocap;
        InitOmicron();
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
 
        lastPosDeltaMagnitude = (lastPosition - position).magnitude;
        lastPosition = position;
        // lastRotation = orientation;

        if (lastPosDeltaMagnitude != 0)
            timeSinceLastUpdate = 0;

        updateTimer += Time.deltaTime;
        if(updateTimer > 5 && updateEvents > 0 )
        {
            updateLatency = updateTimer / updateEvents;
            updateTimer = 0;
            updateEvents = 0;
        }
    }

    public override void OnEvent(EventData e)
    {
        if (e.sourceId == sourceID || sourceID == -1)
        {
            UpdateTransform( new Vector3(e.posx * positionMod.x, e.posy * positionMod.y, e.posz * positionMod.z), new Quaternion(e.orx, e.ory, e.orz, e.orw));
        }
    }

    public void UpdateTransform(Vector3 pos, Quaternion rot)
    {
        if (CAVE2.GetCAVE2Manager().sendTrackingData)
        {
            CAVE2.SendMessage(gameObject.name, "SendTransformInfo", pos, rot, sendDataMode);
        }
        else
        {
            position = pos;
            orientation = rot;
            timeSinceLastUpdate = 0;
        }
    }

    public void SendTransformInfo(object[] param)
    {
        position = (Vector3)param[0];
        orientation = (Quaternion)param[1];
        timeSinceLastUpdate = 0;
        updateEvents++;
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public Quaternion GetOrientation()
    {
        return orientation;
    }

    public float GetTimeSinceLastUpdate()
    {
        return timeSinceLastUpdate;
    }


    public void SetPositionMod(Vector3 value)
    {
        positionMod = value;
    }

    public float GetUpdateLatency()
    {
        return updateLatency;
    }
}
