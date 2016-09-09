using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CAVE2VRLobbyManager : NetworkLobbyManager {

    public UnityEngine.UI.InputField serverAddressField;
    public UnityEngine.UI.InputField localPlayerNameField;

    NetworkConnection localPlayerConnection;

    public string playerName;

    public GameObject localPlayer;

    public static CAVE2VRLobbyManager LobbyManager;

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        //CAVE2Manager.CAVE2LoadSceneOnDisplays(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void StartServerAsHost()
    {
        StartHost();
    }

    public void ConnectToServer()
    {
        StartClient();
    }

    void Start()
    {
        LobbyManager = this;
        serverAddressField.text = networkAddress;
    }

    public void SetServerAddress(string serverIP)
    {
        networkAddress = serverIP;
    }

    public void SetLocalLobbyPlayer(GameObject player)
    {
        localPlayer = player;
        playerName = "Player " + localPlayer.GetComponent<NetworkIdentity>().netId;
    }

    public void SetLocalLobbyPlayerName(string name)
    {
        playerName = localPlayerNameField.text;
        if(localPlayer != null)
            localPlayer.SendMessage("SetPlayerName", localPlayerNameField.text);
    }
}
