using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CAVE2VRLobbyManager : NetworkLobbyManager {

    public UnityEngine.UI.InputField serverAddressField;
    public UnityEngine.UI.InputField localPlayerNameField;

    NetworkConnection localPlayerConnection;

    public string playerName;

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        CAVE2Manager.CAVE2LoadSceneOnDisplays(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
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
        serverAddressField.text = networkAddress;
    }

    public void SetServerAddress(string serverIP)
    {
        networkAddress = serverIP;
    }
}
