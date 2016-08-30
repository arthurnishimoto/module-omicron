using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkedPlayerManager : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        if (!isLocalPlayer)
        {
            SendMessage("FreezeMovement", true);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
