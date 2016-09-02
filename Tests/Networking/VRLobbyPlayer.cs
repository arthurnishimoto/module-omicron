using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VRLobbyPlayer : NetworkLobbyPlayer {

    public string playerName;

    public void SetPlayerName(string text)
    {
        playerName = text;
    }
}
