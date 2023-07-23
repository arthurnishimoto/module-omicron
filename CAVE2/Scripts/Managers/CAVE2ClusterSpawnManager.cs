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

#if UNITY_2020_3_OR_NEWER
#else
#if USING_GETREAL3D
public class CAVE2ClusterSpawnManager : getReal3D.MonoBehaviourWithRpc {
#else
public class CAVE2ClusterSpawnManager : MonoBehaviour {
#endif
    public CAVE2VRLobbyManager lobbyManager;

    Hashtable spawnedPlayerList;
    Hashtable clientPlayerList;
    public float networkUpdateDelay = 0.1f;
    float networkWaitTimer = 0;

    // Use this for initialization
    void Start () {
        spawnedPlayerList = new Hashtable();
        clientPlayerList = new Hashtable();
        lobbyManager = GetComponent<CAVE2VRLobbyManager>();
    }
	
	// Update is called once per frame
	void Update () {

        if (CAVE2.IsMaster())
        {
            if (networkWaitTimer <= 0)
            {
#if USING_GETREAL3D
                ICollection keys = spawnedPlayerList.Keys;
                foreach (int netID in keys)
                {
                    GameObject g = spawnedPlayerList[netID] as GameObject;


                    NetworkedVRPlayerManager netPlayer = g.GetComponent<NetworkedVRPlayerManager>();
                    CallRpc("UpdateNetworkPlayerRPC", netID, netPlayer.playerPosition, netPlayer.playerRotation, netPlayer.headPosition, netPlayer.headRotation);

                }
#endif
            }
                networkWaitTimer = networkUpdateDelay;
            }
            else
            {
                networkWaitTimer -= Time.deltaTime;
            }

    }

    public void SpawnNetworkPlayerOnCAVE2(GameObject source)
    {
        NetworkedVRPlayerManager player = source.GetComponent<NetworkedVRPlayerManager>();
        int netID = (int)player.networkID;
        if (!spawnedPlayerList.ContainsKey(netID))
        {
#if USING_GETREAL3D
            CallRpc("SpawnNetworkPlayerRPC", netID, player.playerName, player.playerType);
#endif
            Debug.Log("Added " + netID + " to list");
            spawnedPlayerList.Add(netID, source);
        }
    }
#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    void SpawnNetworkPlayerRPC(int netID, string playerName, string playerType)
    {
        Debug.Log("Added " + netID + " to CAVE2 display client");
        
        GameObject newNetPlayer = Instantiate(lobbyManager.gamePlayerPrefab) as GameObject;
        newNetPlayer.name = "CAVE2 Client (remote " +netID + ")";

        NetworkedVRPlayerManager vrNetPlayer = newNetPlayer.GetComponent<NetworkedVRPlayerManager>();
        vrNetPlayer.playerName = playerName;
        vrNetPlayer.playerType = playerType;
        vrNetPlayer.SetNetID((uint)netID);

        clientPlayerList.Add(netID, vrNetPlayer);
    }
#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    void UpdateNetworkPlayerRPC(int netID, Vector3 position, Quaternion rotation, Vector3 headPos, Quaternion headRot)
    {
        if( clientPlayerList.ContainsKey(netID) )
        {
            NetworkedVRPlayerManager vrNetPlayer = clientPlayerList[netID] as NetworkedVRPlayerManager;
            vrNetPlayer.playerPosition = new Vector3(position.x, position.y, position.z);
            vrNetPlayer.playerRotation = rotation;
            vrNetPlayer.headPosition = headPos;
            vrNetPlayer.headRotation = headRot;
        }
    }
}
#endif