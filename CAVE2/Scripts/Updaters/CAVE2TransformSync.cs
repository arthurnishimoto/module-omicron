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

public class CAVE2TransformSync : MonoBehaviour {

    enum UpdateMode {Manual, Update, Fixed, Late, Adaptive};

    [SerializeField]
    UpdateMode updateMode = UpdateMode.Fixed;

    public float updateSpeed = 3;
    float updateTimer;

    public bool syncPosition = true;
    public bool syncRotation;
    public bool syncScale;

    public Transform testSyncObject;

    Vector3 nextPosition;
    Quaternion nextRotation;
    Vector3 nextScale;

    bool gotFirstUpdateFromMaster;
    bool hasScaleFromMaster;

    [SerializeField]
    UnityEngine.UI.Text adaptiveDebugText;

    public void Update()
    {
        if (updateMode == UpdateMode.Update)
            UpdateSync();

        if(adaptiveDebugText && updateMode == UpdateMode.Adaptive)
        {
            adaptiveDebugText.text = "Master:\n(" + nextPosition.x.ToString("F2") + ", " + nextPosition.y.ToString("F2") + ", " + nextPosition.z.ToString("F2") + ")\n";
            adaptiveDebugText.text += "   (" + nextRotation.eulerAngles.x.ToString("F2") + ", " + nextRotation.eulerAngles.y.ToString("F2") + ", " + nextRotation.eulerAngles.z.ToString("F2") + ")\n";
            adaptiveDebugText.text += "Client:\n(" + transform.position.x.ToString("F2") + ", " + transform.position.y.ToString("F2") + ", " + transform.position.z.ToString("F2") + ")\n";
            adaptiveDebugText.text += "   (" + transform.rotation.eulerAngles.x.ToString("F2") + ", " + transform.rotation.eulerAngles.y.ToString("F2") + ", " + transform.rotation.eulerAngles.z.ToString("F2") + ")\n";

            Vector3 posDiff = nextPosition - transform.position;
            Vector3 rotDiff = nextRotation.eulerAngles - transform.rotation.eulerAngles;

            adaptiveDebugText.text += "Drift:\n(" + posDiff.x.ToString("F2") + ", " + posDiff.y.ToString("F2") + ", " + posDiff.z.ToString("F2") + ")\n";
            adaptiveDebugText.text += "   (" + rotDiff.x.ToString("F2") + ", " + rotDiff.y.ToString("F2") + ", " + rotDiff.z.ToString("F2") + ")\n";

        }
        else if(adaptiveDebugText)
        {
            adaptiveDebugText.text = "";
        }
    }
    public void FixedUpdate()
    {
        if (updateMode == UpdateMode.Fixed || updateMode == UpdateMode.Adaptive)
            UpdateSync();
    }

    public void LateUpdate()
    {
        if (updateMode == UpdateMode.Late)
            UpdateSync();
    }

    void UpdateSync()
    {
        if (CAVE2.IsMaster())
        {
            if (updateTimer < 0)
            {
                if(syncPosition && syncRotation)
                {
                    CAVE2.SendMessage(gameObject.name, "SyncPosRot", transform.position.x, transform.position.y, transform.position.z, transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w, false);
                }
                else if (syncPosition)
                {
                    CAVE2.SendMessage(gameObject.name, "SyncPosition", transform.position.x, transform.position.y, transform.position.z, false);
                }
                else if(syncRotation)
                {
                    CAVE2.SendMessage(gameObject.name, "SyncRotation", transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w, false);
                }
                else if (syncScale)
                {
                    CAVE2.SendMessage(gameObject.name, "SyncScale", transform.localScale.x, transform.localScale.y, transform.localScale.z, false);
                }

                updateTimer = updateSpeed;
            }

            updateTimer -= Time.fixedDeltaTime;
        }
        else if(gotFirstUpdateFromMaster)
        {
            if (updateMode != UpdateMode.Adaptive)
            {
                transform.position = nextPosition;
                transform.rotation = nextRotation;
            }
        }

        if (hasScaleFromMaster)
        {
            transform.localScale = nextScale;
        }

        if (testSyncObject)
        {
            transform.localPosition = testSyncObject.localPosition;
            transform.localRotation = testSyncObject.localRotation;
        }
    }

    public void SyncPosition(Vector3 position)
    {
        nextPosition = position;
        gotFirstUpdateFromMaster = true;
    }

    public void SyncPosition(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));
    }

    public void SyncRotation(Quaternion rotation)
    {
        nextRotation = rotation;
        gotFirstUpdateFromMaster = true;
    }

    public void SyncRotation(object[] data)
    {
        SyncRotation(new Quaternion((float)data[0], (float)data[1], (float)data[2], (float)data[3]));
    }

    public void SyncPosRot(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));
        SyncRotation(new Quaternion((float)data[3], (float)data[4], (float)data[5], (float)data[6]));
    }

    public void SyncScale(Vector3 value)
    {
        nextScale = value;
        hasScaleFromMaster = true;
    }

    public void SyncScale(object[] data)
    {
        SyncScale(new Vector3((float)data[0], (float)data[1], (float)data[2]));
    }
}
