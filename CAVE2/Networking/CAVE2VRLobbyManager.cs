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
using System.Collections.Generic;
using UnityEngine.Networking;

public class CAVE2VRLobbyManager : NetworkLobbyManager {

    public UnityEngine.UI.InputField serverAddressField;
    public UnityEngine.UI.InputField localPlayerNameField;
    public UnityEngine.UI.Text localPlayerTypeText;

    NetworkConnection localPlayerConnection;

    public string playerName;

    public GameObject localPlayer;

    public static CAVE2VRLobbyManager LobbyManager;

    public Camera lobbyCamera;

    public GameObject lobbyCanvas;

    CAVE2ClusterSpawnManager cave2SpawnManager;

    public bool cave2Client = false;

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        //CAVE2Manager.CAVE2LoadSceneOnDisplays(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void StartServerAsHost()
    {
        StartHost();
    }

    public void StartDedicatedServer()
    {
        StartServer();
        showLobbyGUI = true;
    }

    public void ConnectToServer()
    {
        networkAddress = serverAddressField.text;
        StartClient();
        PlayerPrefs.SetString("ServerIPAddress", networkAddress);
    }

    void Start()
    {
        LobbyManager = this;
        serverAddressField.text = networkAddress;

        NetworkedVRPlayerManager playerInfo = lobbyPlayerPrefab.GetComponent<NetworkedVRPlayerManager>();
        string playerType = playerInfo.localPlayerControllerPrefab.GetComponent<VRPlayerWrapper>().GetVRTypeLabel();
        localPlayerTypeText.text = "Player type '" + playerType + "' detected";

        if(PlayerPrefs.GetString("ServerIPAddress").Length > 0 )
        {
            serverAddressField.text = PlayerPrefs.GetString("ServerIPAddress");
        }
        if (PlayerPrefs.GetString("LocalPlayerName").Length > 0)
        {
            localPlayerNameField.text = PlayerPrefs.GetString("LocalPlayerName");
            playerName = localPlayerNameField.text;
        }

        if( !CAVE2.IsMaster() )
        {
            StartHost();
            cave2Client = true;
        }
    }

    public void SetLocalLobbyPlayer(GameObject player)
    {
        localPlayer = player;
        if(playerName == "")
            playerName = "Player " + localPlayer.GetComponent<NetworkIdentity>().netId;

        SetupLobbyAsPlayScene();
    }

    public void SetLocalLobbyPlayerName(string name)
    {
        playerName = localPlayerNameField.text;
        if(localPlayer != null)
            localPlayer.SendMessage("SetPlayerName", localPlayerNameField.text);

        PlayerPrefs.SetString("LocalPlayerName", playerName);
    }

    void SetupLobbyAsPlayScene()
    {
        lobbyCamera.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(false);
    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnLobbyClientConnect: " + conn.address);

    }

    public override void OnLobbyClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnLobbyClientDisconnect: " + conn.address);

    }

    public void SpawnPlayerOnCAVE2(GameObject source)
    {
        GetComponent<CAVE2ClusterSpawnManager>().SpawnNetworkPlayerOnCAVE2(source);
    }
}
