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
using UnityEngine.Networking;

public class NetworkedVRPlayerManager : NetworkLobbyPlayer
{

    [SyncVar]
    public string playerName = "VRPlayer";

    [SyncVar]
    public string playerType = "VR";

    public string networkAddress;
    public uint networkID;

    public GameObject characterLabelPrefab;

    public float networkUpdateDelay = 0.1f;
    float networkWaitTimer = 0;

    public bool localPlayer;
    public GameObject localPlayerControllerPrefab;
    GameObject localPlayerController;

    public GameObject headMarkerPrefab;
    public GameObject wandMarkerPrefab;

    public GameObject headMarker;
    public GameObject[] wandMarkers;

    Transform headObject;
    Transform[] wandObjects;
    [SyncVar]
    public Vector3 headPosition;

    [SyncVar]
    public Quaternion headRotation;

    [SyncVar]
    public Vector3 playerPosition;

    [SyncVar]
    public Quaternion playerRotation;

    [SyncVar]
    public Vector3 wandPosition;

    [SyncVar]
    public Quaternion wandRotation;

    CharacterLabelUI playerLabel;

    public bool cave2Client = false;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CAVE2VRLobbyManager.LobbyManager.SetLocalLobbyPlayer(gameObject);
    }

    public void SetNetID(uint netID)
    {
        networkID = netID;
        cave2Client = true;
    }

    // Use this for initialization
    public void Start() {
        //base.OnStartClient(); // don't use OnClientStart - this will cause isLocalPlayer to always return false
        NetworkIdentity netID = GetComponent<NetworkIdentity>();
        
        if (netID.connectionToClient != null)
        {
            networkAddress = "local";
            networkID = netID.netId.Value;
        }
        else if (netID.connectionToServer != null)
        {
            networkAddress = "local";
            networkID = netID.netId.Value;
        }
        else
        {
            networkAddress = "remote";
            networkID = netID.netId.Value;
        }

        // Cleanup address if client (prepended with :fff::)
        string[] splitAddr = networkAddress.Split(':');
        networkAddress = splitAddr[splitAddr.Length - 1];

        // Set gameobject name 
        if(networkID > 0)
            gameObject.name = "VRNetworkPlayer(" + networkAddress + " " + networkID + ")";

        localPlayer = isLocalPlayer;

        // Create the UI label
        GameObject playerLabelObj = Instantiate(characterLabelPrefab, transform.position, transform.rotation) as GameObject;
        playerLabelObj.transform.parent = transform;
        playerLabelObj.transform.localPosition = Vector3.up * 2f;
        playerLabel = playerLabelObj.GetComponent<CharacterLabelUI>();

        if (!localPlayer)
        {
            // Disable non-local player cameras
            Camera[] playerCamera = gameObject.GetComponentsInChildren<Camera>();
            foreach (Camera c in playerCamera)
            {
                c.gameObject.SetActive(false);
            }

            headMarker = Instantiate(headMarkerPrefab);
            headMarker.transform.parent = transform;

            wandMarkers[0] = Instantiate(wandMarkerPrefab);
            wandMarkers[0].transform.parent = transform;

            // CAVE2 - Tell Lobby Manager player needs to be spawned on display nodes
            if (CAVE2.IsMaster())
            {
                GameObject lobbyManagerObj = GameObject.Find("NetworkManager");
                if (lobbyManagerObj && networkID > 0)
                {
                    CAVE2VRLobbyManager lobbyManager = lobbyManagerObj.GetComponent<CAVE2VRLobbyManager>();
                    lobbyManager.SpawnPlayerOnCAVE2(gameObject);
                }
            }
        }
        else
        {
            if(CAVE2VRLobbyManager.LobbyManager.cave2Client)
            {
                headMarker = Instantiate(headMarkerPrefab);
                headMarker.transform.parent = transform;

                wandMarkers[0] = Instantiate(wandMarkerPrefab);
                wandMarkers[0].transform.parent = transform;
            }

            playerName = CAVE2VRLobbyManager.LobbyManager.playerName;
            localPlayerController = Instantiate(localPlayerControllerPrefab, transform.position, transform.rotation) as GameObject;
            VRPlayerWrapper vrPlayer = localPlayerController.GetComponent<VRPlayerWrapper>();
            playerType = vrPlayer.GetVRTypeLabel();
            headObject = vrPlayer.GetHead();
            wandObjects = vrPlayer.GetWands();

            CmdUpdatePlayerLabel(playerName, playerType);
        }
        UpdatePosition();
    }

    // Update is called once per frame
    void Update () {
        UpdatePosition();

        playerLabel.SetName(playerName);
        if (playerType != "")
        {
            playerLabel.SetTitle("<" + playerType + ">");
        }
        else
        {
            playerLabel.SetTitle("");
        }
    }

    void UpdatePosition()
    {
        if (localPlayer && !CAVE2VRLobbyManager.LobbyManager.cave2Client)
        {
            headPosition = headObject.localPosition;
            headRotation = headObject.localRotation;

            playerLabel.transform.localPosition = new Vector3(headPosition.x, headPosition.y + 0.3f, headPosition.z);

            playerPosition = localPlayerController.transform.position;
            playerRotation = localPlayerController.transform.rotation;

            transform.position = playerPosition;

            if (networkWaitTimer <= 0)
            {

                // Sends my head position to server
                CmdUpdateHeadTransform(headPosition, headRotation);
                CmdUpdatePlayerTransform(playerPosition, playerRotation);
                int wandIndex = 0;
                foreach (Transform wand in wandObjects)
                {
                    CmdUpdateWandTransform(wandIndex, wand.localPosition, wand.localRotation);
                    wandIndex++;
                }

                networkWaitTimer = networkUpdateDelay;
            }
            else
            {
                networkWaitTimer -= Time.deltaTime;
            }
        }
        else
        {
            // Update transforms of other players from server
            transform.position = playerPosition;
            transform.rotation = playerRotation;

            headMarker.transform.localPosition = headPosition;
            headMarker.transform.localRotation = headRotation;

            playerLabel.transform.localPosition = new Vector3(headPosition.x, headPosition.y + 0.3f, headPosition.z);

            wandMarkers[0].transform.localPosition = wandPosition;
            wandMarkers[0].transform.localRotation = wandRotation;
        }
    }

    // Client command to set the player's head position
    // so that the server can SyncVar it to the other players
    [Command]
    void CmdUpdateHeadTransform(Vector3 position, Quaternion rotation)
    {
        headPosition = position;
        headRotation = rotation;
    }

    [Command]
    void CmdUpdatePlayerTransform(Vector3 position, Quaternion rotation)
    {
        playerPosition = position;
        playerRotation = rotation;
    }

    [Command]
    void CmdUpdateWandTransform(int wandID, Vector3 position, Quaternion rotation)
    {
        wandPosition = position;
        wandRotation = rotation;
    }

    [Command]
    void CmdUpdatePlayerLabel(string name, string type)
    {
        playerName = name;
        playerType = type;
    }
}
