using UnityEngine;
using System.Collections;
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
        //Debug.Log(conn.);
    }
}
