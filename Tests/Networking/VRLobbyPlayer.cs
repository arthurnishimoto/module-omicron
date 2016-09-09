using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VRLobbyPlayer : NetworkLobbyPlayer {

    [SyncVar]
    public string playerName;

    public void SetPlayerName(string text)
    {
        playerName = text;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("OnStartLocalPlayer");
        CAVE2VRLobbyManager.LobbyManager.SetLocalLobbyPlayer(gameObject);
    }
}
