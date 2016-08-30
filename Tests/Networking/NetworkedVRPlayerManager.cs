using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkedVRPlayerManager : NetworkBehaviour {

    // Use this for initialization
    void Start () {
        if (!isLocalPlayer)
        {
            // Freeze character controller
            SendMessage("FreezeMovement", true);

            // Disable non-local player cameras
            Camera[] playerCamera = gameObject.GetComponentsInChildren<Camera>();
            foreach (Camera c in playerCamera)
            {
                c.enabled = false;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
