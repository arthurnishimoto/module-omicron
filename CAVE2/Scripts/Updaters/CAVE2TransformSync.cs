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
using System.Collections.Generic;

public class CAVE2TransformSync : MonoBehaviour {

    public enum UpdateMode {Manual, Update, Fixed, Late, Adaptive, ClientAdaptive, ClientUpdate};

    [SerializeField]
    UpdateMode updateMode = UpdateMode.Fixed;

    public float updateSpeed = 3;
    float updateTimer;

    float delayTimer = 0;

    public bool syncPosition = true;
    public bool syncRotation;
    public bool syncScale;

    [SerializeField]
    bool useLocal = false;

    public Transform testSyncObject;

    Vector3 nextPosition;
    Quaternion nextRotation;
    Vector3 nextScale;

    bool gotFirstUpdateFromMaster;
    bool hasScaleFromMaster;

    [SerializeField]
    UnityEngine.UI.Text adaptiveDebugText = null;

    Vector3 posDiff;
    Vector3 rotDiff;

    float timeSinceLastChange;

    [SerializeField]
    float adaptiveThreshold = 0.05f; // Drift allowed before correction (meters)

    [SerializeField]
    float adaptiveDelay = 2f; // Time in seconds before drift correction

    float driftValue;

    [Header("Advanced Cluster Sync")]
    [SerializeField]
    bool useAdvancedClusterSync;

    [SerializeField]
    UnityEngine.UI.Text advSyncDebugText;

    Dictionary<int, Vector3> clusterPositions = new Dictionary<int, Vector3>();
    Dictionary<int, Vector3> clusterRotations = new Dictionary<int, Vector3>();

    float advUpdateTimer;
    float advUpdateTime = 0.1f;

    public void Update()
    {
        if (updateMode == UpdateMode.Update)
        {
            timeSinceLastChange += Time.deltaTime;
            UpdateSync();
        }

        if (CAVE2.IsMaster())
        {
            if (updateTimer < 0)
            {
                SyncTransform();
                updateTimer = updateSpeed;
            }

            updateTimer -= Time.deltaTime;
        }

        posDiff = nextPosition - (useLocal ? transform.localPosition : transform.position);
        rotDiff = nextRotation.eulerAngles - (useLocal ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles);

        if (adaptiveDebugText)
        {
            adaptiveDebugText.text = GetAdaptiveDebugText();
        }

        if (CAVE2.IsMaster() && useAdvancedClusterSync)
        {
            if (advSyncDebugText)
            {
                advSyncDebugText.text = gameObject.name + "\n";
                advSyncDebugText.text += "Master Pos: " + transform.position + " nClientPos: " + clusterPositions.Count + "\n";

                foreach (KeyValuePair<int, Vector3> kvp in clusterPositions)
                {
                    float diff = (kvp.Value - transform.position).magnitude;
                    advSyncDebugText.text += kvp.Key + " " + kvp.Value + " Diff " + diff + "\n";
                }
            }

            if(delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
            }
            else
            {
                int syncingDisplayNode = 1;
                if (updateMode == UpdateMode.ClientUpdate && clusterPositions.ContainsKey(syncingDisplayNode))
                {
                    Vector3 clientPos = clusterPositions[syncingDisplayNode];
                    Vector3 clientRot = clusterRotations[syncingDisplayNode];

                    if (useLocal)
                    {
                        if (syncPosition)
                        {
                            transform.localPosition = clientPos;
                        }
                        if (syncRotation)
                        {
                            transform.localEulerAngles = clientRot;
                        }
                    }
                    else
                    {
                        if (syncPosition)
                        {
                            transform.position = clientPos;
                        }
                        if (syncRotation)
                        {
                            transform.eulerAngles = clientRot;
                        }
                    }
                }
            }
        }
        else if (useAdvancedClusterSync)
        {
            if (delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
            }
            else
            {
                delayTimer = 0;

                if (advUpdateTimer > updateSpeed)
                {
                    Vector3 position = (useLocal ? transform.localPosition : transform.position);
                    Vector3 rotation = (useLocal ? transform.localEulerAngles : transform.eulerAngles);

                    int connID = CAVE2.GetCAVE2Manager().GetComponent<CAVE2RPCManager>().GetConnID();
                    CAVE2.SendMessage(gameObject.name, "ClientPosRos", position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, connID, CAVE2RPCManager.MsgType.StateUpdate);

                    advUpdateTimer = 0;
                }
                else
                {
                    advUpdateTimer += Time.deltaTime;
                }
            }            
        }
    }

    public string GetAdaptiveDebugText()
    {
        string text = gameObject.name + "\n";
        text += "Master:\t (" + nextPosition.x.ToString("F2") + ", " + nextPosition.y.ToString("F2") + ", " + nextPosition.z.ToString("F2") + ")\n";
        text += "\t\t\t (" + nextRotation.eulerAngles.x.ToString("F2") + ", " + nextRotation.eulerAngles.y.ToString("F2") + ", " + nextRotation.eulerAngles.z.ToString("F2") + ")\n";
        text += "Client:\t (" + transform.position.x.ToString("F2") + ", " + transform.position.y.ToString("F2") + ", " + transform.position.z.ToString("F2") + ")\n";
        text += "\t\t\t (" + transform.rotation.eulerAngles.x.ToString("F2") + ", " + transform.rotation.eulerAngles.y.ToString("F2") + ", " + transform.rotation.eulerAngles.z.ToString("F2") + ")\n";

        text += "Drift:\t\t (" + posDiff.x.ToString("F2") + ", " + posDiff.y.ToString("F2") + ", " + posDiff.z.ToString("F2") + ")\n";
        text += "\t\t\t (" + rotDiff.x.ToString("F2") + ", " + rotDiff.y.ToString("F2") + ", " + rotDiff.z.ToString("F2") + ")\n";
        text += "Drift Value: " + driftValue + " > " + adaptiveThreshold + " = " + (driftValue > adaptiveThreshold) + "\n";
        text += "Time since last change: " + timeSinceLastChange.ToString("F3") + " (Sync on " + adaptiveDelay + ")";

        return text;
    }

    public void FixedUpdate()
    {
        if (updateMode == UpdateMode.Fixed || updateMode == UpdateMode.Adaptive || updateMode == UpdateMode.ClientAdaptive)
        {
            timeSinceLastChange += Time.fixedDeltaTime;
            UpdateSync();
        }
    }

    public void LateUpdate()
    {
        if (updateMode == UpdateMode.Late)
        {
            timeSinceLastChange += Time.deltaTime;
            UpdateSync();   
        }
    }

    public void SyncTransform()
    {
        Vector3 position = (useLocal ? transform.localPosition : transform.position);
        Quaternion rotation = (useLocal ? transform.localRotation : transform.rotation);

        if (syncPosition && syncRotation)
        {
            CAVE2.SendMessage(gameObject.name, "SyncPosRot", position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, CAVE2RPCManager.MsgType.StateUpdate);
        }
        else if (syncPosition)
        {
            CAVE2.SendMessage(gameObject.name, "SyncPosition", position.x, position.y, position.z, CAVE2RPCManager.MsgType.StateUpdate);
        }
        else if (syncRotation)
        {
            CAVE2.SendMessage(gameObject.name, "SyncRotation", rotation.x, rotation.y, rotation.z, rotation.w, CAVE2RPCManager.MsgType.StateUpdate);
        }
        else if (syncScale)
        {
            CAVE2.SendMessage(gameObject.name, "SyncScale", transform.localScale.x, transform.localScale.y, transform.localScale.z, CAVE2RPCManager.MsgType.StateUpdate);
        }
    }

    void UpdateSync()
    {
        if(!CAVE2.IsMaster() && gotFirstUpdateFromMaster)
        {
            if (delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
                return;
            }
            else
            {
                delayTimer = 0;
            }

            if (updateMode != UpdateMode.Adaptive && updateMode != UpdateMode.ClientAdaptive)
            {
                if (useLocal)
                {
                    transform.localPosition = nextPosition;
                    transform.localRotation = nextRotation;
                }
                else
                {
                    transform.position = nextPosition;
                    transform.rotation = nextRotation;
                }
            }
            else
            {
                driftValue = Vector3.Magnitude(useLocal ? transform.localPosition : transform.position - nextPosition);
                if (timeSinceLastChange > adaptiveDelay && driftValue > adaptiveThreshold)
                {
                    if (useLocal)
                    {
                        transform.localPosition = nextPosition;
                        transform.localRotation = nextRotation;
                    }
                    else
                    {
                        transform.position = nextPosition;
                        transform.rotation = nextRotation;
                    }
                    timeSinceLastChange = 0;
                }
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
        if(Vector3.Magnitude(nextPosition - position) > 0.01f )
        {
            timeSinceLastChange = 0;
        }
        nextPosition = position;
        gotFirstUpdateFromMaster = true;
    }

    public void SyncPosition(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));

        if (updateMode == UpdateMode.Manual && !useAdvancedClusterSync)
        {
            UpdateSync();
        }
    }

    public void SyncRotation(Quaternion rotation)
    {
        nextRotation = rotation;
        gotFirstUpdateFromMaster = true;
    }

    public void SyncRotation(object[] data)
    {
        SyncRotation(new Quaternion((float)data[0], (float)data[1], (float)data[2], (float)data[3]));

        if (updateMode == UpdateMode.Manual && !useAdvancedClusterSync)
        {
            UpdateSync();
        }
    }

    public void SyncPosRot(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));
        SyncRotation(new Quaternion((float)data[3], (float)data[4], (float)data[5], (float)data[6]));

        if (updateMode == UpdateMode.Manual && !useAdvancedClusterSync)
        {
            UpdateSync();
        }
    }

    public void SyncScale(Vector3 value)
    {
        nextScale = value;
        hasScaleFromMaster = true;
    }

    public void SyncScale(object[] data)
    {
        SyncScale(new Vector3((float)data[0], (float)data[1], (float)data[2]));

        if (updateMode == UpdateMode.Manual && !useAdvancedClusterSync)
        {
            UpdateSync();
        }
    }

    public void SetAdaptiveSync()
    {
        CAVE2.SendMessage(gameObject.name, "SetAdaptiveSyncRPC");
    }

    void SetAdaptiveSyncRPC()
    {
        updateMode = UpdateMode.Adaptive;
    }

    public void SetClientUpdateSync()
    {
        CAVE2.SendMessage(gameObject.name, "SetClientUpdateSyncRPC");
    }

    void SetClientUpdateSyncRPC()
    {
        updateMode = UpdateMode.ClientUpdate;
    }

    public void SetUpdateSync()
    {
        CAVE2.SendMessage(gameObject.name, "SetUpdateSyncRPC");
    }

    void SetUpdateSyncRPC()
    {
        updateMode = UpdateMode.Update;
    }

    public void SetManualSync()
    {
        CAVE2.SendMessage(gameObject.name, "SetManualSyncRPC");
    }

    void SetManualSyncRPC()
    {
        updateMode = UpdateMode.Manual;
    }

    public bool IsAdaptiveSyncEnabled()
    {
        return updateMode == UpdateMode.Adaptive;
    }

    public bool IsManualSyncEnabled()
    {
        return updateMode == UpdateMode.Manual;
    }

    public void ClientPosRos(object[] data)
    {
        // CAVE2.GetCAVE2Manager().GetComponent<CAVE2RPCManager>().LogUI("Rec pos from connID " + (int)data[3]);
        int connID = (int)data[6];
        Vector3 pos = new Vector3((float)data[0], (float)data[1], (float)data[2]);
        Vector3 rot = new Vector3((float)data[3], (float)data[4], (float)data[5]);

        if (clusterPositions.ContainsKey(connID))
        {
            clusterPositions[connID] = pos;
        }
        else
        {
            clusterPositions.Add(connID, pos);
        }

        if (clusterRotations.ContainsKey(connID))
        {
            clusterRotations[connID] = rot;
        }
        else
        {
            clusterRotations.Add(connID, rot);
        }
    }

    public void DelaySync(float time = 3)
    {
        CAVE2.SendMessage(gameObject.name, "DelaySyncRPC", time);
    }

    public void DelaySyncRPC(float time = 3)
    {
        delayTimer = time;
    }
}
