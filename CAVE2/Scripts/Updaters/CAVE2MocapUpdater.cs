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

public class CAVE2MocapUpdater : MonoBehaviour {

    enum PredefinedMocapSensors {None = -1, Head_Tracker = 0, Wand_Batman = 1, Wand_Robin = 2, HoloLens = 5, Mirage = 6, HoloLens2 = 7};
    [SerializeField]
    PredefinedMocapSensors usePredefined = PredefinedMocapSensors.None;

    public int sourceID = 1;

    // Offset to tracking data (ex. object pivot vs tracking marker center)
    [SerializeField] Vector3 posOffset = Vector3.zero;
    [SerializeField] Vector3 rotOffset = Vector3.zero;

    [SerializeField]
    bool useLateUpdate = false;

    [Header("Debug")]
    [SerializeField]
    bool trackUpdateLatency = false;

    [SerializeField]
    float updateLatency = 0;

    float updateTimer;
    float updateEvents;

    [SerializeField]
    UnityEngine.UI.Text debugText = null;

    // Use this for initialization
    void Start () {
	    if(usePredefined != PredefinedMocapSensors.None)
        {
            sourceID = (int)usePredefined;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (!useLateUpdate)
        {
            transform.localPosition = CAVE2.GetMocapPosition(sourceID) + posOffset;
            transform.localRotation = CAVE2.GetMocapRotation(sourceID);
            transform.Rotate(rotOffset);

            if(trackUpdateLatency)
            {
                updateEvents++;
                updateTimer += Time.deltaTime;

                if(updateTimer > 5 && updateEvents > 0)
                {
                    updateLatency = updateTimer / updateEvents;
                    updateTimer = 0;
                    updateEvents = 0;

                    if(debugText)
                    {
                        debugText.text = updateLatency.ToString();
                    }
                }

            }
        }
    }

    void LateUpdate()
    {
        if (useLateUpdate)
        {
            transform.localPosition = CAVE2.GetMocapPosition(sourceID) + posOffset;
            transform.localRotation = CAVE2.GetMocapRotation(sourceID);
            transform.Rotate(rotOffset);
        }
    }

    public void SetPosOffset(Vector3 offset)
    {
        posOffset = offset;
    }

    public void SetSourceID(int id)
    {
        sourceID = id;
    }

    public Vector3 GetMocapPosition()
    {
        return CAVE2.GetMocapPosition(sourceID);
    }
}
