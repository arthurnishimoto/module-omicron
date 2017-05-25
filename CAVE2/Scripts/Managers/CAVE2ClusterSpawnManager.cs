using UnityEngine;
using System.Collections;

public class CAVE2ClusterSpawnManager : getReal3D.MonoBehaviourWithRpc {

    public CAVE2VRLobbyManager lobbyManager;

    Hashtable spawnedPlayerList;
    Hashtable clientPlayerList;
    public float networkUpdateDelay = 0.1f;
    float networkWaitTimer = 0;

    // Use this for initialization
    void Start () {
        spawnedPlayerList = new Hashtable();
        clientPlayerList = new Hashtable();
        lobbyManager = GetComponent<CAVE2VRLobbyManager>();
    }
	
	// Update is called once per frame
	void Update () {
        if (networkWaitTimer <= 0)
        {
            ICollection keys = spawnedPlayerList.Keys;
            foreach( int netID in keys )
            {
                GameObject g = spawnedPlayerList[netID] as GameObject;
                NetworkedVRPlayerManager netPlayer = g.GetComponent<NetworkedVRPlayerManager>();

                UpdateNetworkPlayerRPC(netID, netPlayer.playerPosition, netPlayer.playerRotation, netPlayer.headPosition, netPlayer.headRotation);
            }
            /*
            // Sends my head position to server
            CmdUpdateHeadTransform(headPosition, headRotation);
            CmdUpdatePlayerTransform(playerPosition, playerRotation);
            int wandIndex = 0;
            foreach (Transform wand in wandObjects)
            {
                CmdUpdateWandTransform(wandIndex, wand.localPosition, wand.localRotation);
                wandIndex++;
            }
            */
            networkWaitTimer = networkUpdateDelay;
        }
        else
        {
            networkWaitTimer -= Time.deltaTime;
        }
    }

    public void SpawnNetworkPlayerOnCAVE2(GameObject source)
    {
        int netID = (int)source.GetComponent<NetworkedVRPlayerManager>().networkID;
        if (!spawnedPlayerList.ContainsKey(netID))
        {
            getReal3D.RpcManager.call("SpawnNetworkPlayerRPC", netID, source.transform.position, source.transform.rotation);
            Debug.Log("Added " + netID + " to list");
            spawnedPlayerList.Add(netID, source);
        }
    }

    [getReal3D.RPC]
    void SpawnNetworkPlayerRPC(int netID, Vector3 position, Quaternion rotation)
    {
        Debug.Log("Added " + netID + " to CAVE2 display client");
        
        GameObject newNetPlayer = Instantiate(lobbyManager.gamePlayerPrefab) as GameObject;
        newNetPlayer.name = "CAVE2 Client (remote " +netID + ")";

        NetworkedVRPlayerManager vrNetPlayer = newNetPlayer.GetComponent<NetworkedVRPlayerManager>();

        vrNetPlayer.playerPosition = new Vector3(position.x, position.y - 50, position.z);
        vrNetPlayer.playerRotation = rotation;

        Debug.Log(position);
        Debug.Log(rotation);
        Debug.Log(newNetPlayer.name);

        clientPlayerList.Add(netID, vrNetPlayer);
    }

    [getReal3D.RPC]
    void UpdateNetworkPlayerRPC(int netID, Vector3 position, Quaternion rotation, Vector3 headPos, Quaternion headRot)
    {
        if( clientPlayerList.ContainsKey(netID) )
        {
            NetworkedVRPlayerManager vrNetPlayer = clientPlayerList[netID] as NetworkedVRPlayerManager;
            vrNetPlayer.playerPosition = new Vector3(position.x, position.y - 50, position.z);
            vrNetPlayer.playerRotation = rotation;
            vrNetPlayer.headPosition = headPos;
            vrNetPlayer.headRotation = headRot;
        }
    }
}
