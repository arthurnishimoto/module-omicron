using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkedVRPlayerManager : NetworkBehaviour {

    public float networkUpdateDelay = 0.1f;
    float networkWaitTimer = 0;

    public bool localPlayer;

    public Transform headObject;
    public Transform[] wandObjects;

    [SyncVar]
    public Vector3 headPosition;

    [SyncVar]
    public Quaternion headRotation;

    [SyncVar]
    public Vector3 playerPosition;

    [SyncVar]
    public Quaternion playerRotation;

    [SyncVar]
    public Vector3 wandPosition;

    [SyncVar]
    public Quaternion wandRotation;

    // Use this for initialization
    void Start () {
        localPlayer = isLocalPlayer;
        UpdatePosition();
        if (!isLocalPlayer)
        {
            // Freeze character controller
            BroadcastMessage("SetLocalControl", false);

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
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (isLocalPlayer)
        {
            if (networkWaitTimer <= 0)
            {
                // Sends my head position to server
                CmdUpdateHeadTransform(headObject.localPosition, headObject.localRotation);
                CmdUpdatePlayerTransform(transform.position, transform.rotation);
                int wandIndex = 0;
                foreach (Transform wand in wandObjects)
                {
                    CmdUpdateWandTransform(wandIndex, wand.localPosition, wand.localRotation);
                    wandIndex++;
                }

                networkWaitTimer = networkUpdateDelay;
            }
            else
            {
                networkWaitTimer -= Time.deltaTime;
            }
        }
        else
        {
            // Update head position (or other players) from server
            SendMessage("SetHeadPosition", headPosition);
            SendMessage("SetHeadRotation", headRotation);
            SendMessage("SetWandPosition", wandPosition);
            SendMessage("SetWandRotation", wandRotation);
            transform.position = playerPosition;
            transform.rotation = playerRotation;
        }
    }

    // Client command to set the player's head position
    // so that the server can SyncVar it to the other players
    [Command]
    void CmdUpdateHeadTransform(Vector3 position, Quaternion rotation)
    {
        headPosition = position;
        headRotation = rotation;
    }

    [Command]
    void CmdUpdatePlayerTransform(Vector3 position, Quaternion rotation)
    {
        playerPosition = position;
        playerRotation = rotation;
    }

    [Command]
    void CmdUpdateWandTransform(int wandID, Vector3 position, Quaternion rotation)
    {
        wandPosition = position;
        wandRotation = rotation;
    }
}
