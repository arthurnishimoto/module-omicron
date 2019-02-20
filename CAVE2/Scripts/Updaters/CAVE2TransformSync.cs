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

    enum UpdateMode { Manual, Update, Fixed, Late};

    [SerializeField]
    UpdateMode updateMode = UpdateMode.Fixed;

    public float updateSpeed = 3;
    float updateTimer;

    public bool syncPosition = true;
    public bool syncRotation;

    public Transform testSyncObject;

    Vector3 nextPosition;
    Quaternion nextRotation;

    public void Update()
    {
        if (updateMode == UpdateMode.Update)
            UpdateSync();
    }
    public void FixedUpdate()
    {
        if (updateMode == UpdateMode.Fixed)
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
                if (syncPosition)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncPosition", transform.position.x, transform.position.y, transform.position.z, false);
                }
                if (syncRotation)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncRotation", transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w, false);
                }

                updateTimer = updateSpeed;
            }

            updateTimer -= Time.fixedDeltaTime;
        }
        else
        {
            transform.position = nextPosition;
            transform.rotation = nextRotation;
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
    }

    public void SyncPosition(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));
    }

    public void SyncRotation(Quaternion rotation)
    {
        nextRotation = rotation;
    }

    public void SyncRotation(object[] data)
    {
        SyncRotation(new Quaternion((float)data[0], (float)data[1], (float)data[2], (float)data[3]));
    }
}
