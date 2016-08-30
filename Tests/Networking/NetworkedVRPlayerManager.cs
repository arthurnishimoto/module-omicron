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
            headPosition = CAVE2Manager.GetHead(1).position;
        }
        else
        {
            SendMessage("SetHeadPosition", headPosition);
        }
	}
}
