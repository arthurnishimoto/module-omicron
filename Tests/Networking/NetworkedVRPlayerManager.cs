using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkedVRPlayerManager : NetworkBehaviour {

    public bool localPlayer;

    [SyncVar]
    public Vector3 headPosition;

    // Use this for initialization
    void Start () {
        localPlayer = isLocalPlayer;
        if (!isLocalPlayer)
        {
            // Freeze character controller
            SendMessage("FreezeMovement", true);

            // Disable non-local player cameras
            Camera[] playerCamera = gameObject.GetComponentsInChildren<Camera>();
            foreach (Camera c in playerCamera)
            {
                c.gameObject.SetActive(false);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	    if(isLocalPlayer)
        {
            // Sends my head position to server
            CmdSetHeadPosition(CAVE2Manager.GetHead(1).position);
        }
        else
        {
            // Update head position (or other players) from server
            SendMessage("SetHeadPosition", headPosition);
        }
	}

    // Client command to set the player's head position
    // so that the server can SyncVar it to the other players
    [Command]
    void CmdSetHeadPosition(Vector3 position)
    {
        headPosition = position;
    }
}
