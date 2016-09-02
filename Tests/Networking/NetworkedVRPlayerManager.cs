using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkedVRPlayerManager : NetworkBehaviour {

    [SyncVar]
    public string playerName = "VRPlayer";

    public CharacterLabelUI characterLabel;

    public float networkUpdateDelay = 0.1f;
    float networkWaitTimer = 0;

    public bool localPlayer;
    public GameObject localPlayerControllerPrefab;
    GameObject localPlayerController;

    public GameObject headMarkerPrefab;
    public GameObject wandMarkerPrefab;

    public GameObject headMarker;
    public GameObject[] wandMarkers;

    Transform headObject;
    Transform[] wandObjects;
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
    public override void OnStartClient() {
        base.OnStartClient();

        localPlayer = isLocalPlayer;
        characterLabel.SetName(playerName);

        if (!localPlayer)
        {
            // Freeze character controller
            localPlayerController.BroadcastMessage("SetLocalControl", false);

            // Disable non-local player cameras
            Camera[] playerCamera = gameObject.GetComponentsInChildren<Camera>();
            foreach (Camera c in playerCamera)
            {
                c.gameObject.SetActive(false);
            }

            headMarker = Instantiate(headMarkerPrefab);
            headMarker.transform.parent = transform;

            wandMarkers[0] = Instantiate(wandMarkerPrefab);
            wandMarkers[0].transform.parent = transform;
        }
        else
        {
            localPlayerController = Instantiate(localPlayerControllerPrefab, transform.position, transform.rotation) as GameObject;
            VRPlayerWrapper vrPlayer = localPlayerController.GetComponent<VRPlayerWrapper>();
            headObject = vrPlayer.GetHead();
            wandObjects = vrPlayer.GetWands();
        }

        UpdatePosition();
    }
	
	// Update is called once per frame
	void Update () {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (localPlayer)
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
            //SendMessage("SetHeadPosition", headPosition);
            //SendMessage("SetHeadRotation", headRotation);
            //SendMessage("SetWandPosition", wandPosition);
            //SendMessage("SetWandRotation", wandRotation);
            transform.position = playerPosition;
            transform.rotation = playerRotation;

            headMarker.transform.localPosition = headPosition;
            headMarker.transform.localRotation = headRotation;

            wandMarkers[0].transform.localPosition = wandPosition;
            wandMarkers[0].transform.localRotation = wandRotation;
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
