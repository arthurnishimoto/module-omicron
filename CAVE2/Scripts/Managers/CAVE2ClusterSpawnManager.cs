using UnityEngine;
using System.Collections;

#if USING_GETREAL3D
public class CAVE2ClusterSpawnManager : getReal3D.MonoBehaviourWithRpc {
#else
public class CAVE2ClusterSpawnManager : MonoBehaviour {
#endif
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
        if (CAVE2.IsMaster())
        {
            if (networkWaitTimer <= 0)
            {
                ICollection keys = spawnedPlayerList.Keys;
                foreach (int netID in keys)
                {
                    GameObject g = spawnedPlayerList[netID] as GameObject;
                    NetworkedVRPlayerManager netPlayer = g.GetComponent<NetworkedVRPlayerManager>();
#if USING_GETREAL3D
                    getReal3D.RpcManager.call("UpdateNetworkPlayerRPC", netID, netPlayer.playerPosition, netPlayer.playerRotation, netPlayer.headPosition, netPlayer.headRotation);
#endif
                }
                networkWaitTimer = networkUpdateDelay;
            }
            else
            {
                networkWaitTimer -= Time.deltaTime;
            }
        }
    }

    public void SpawnNetworkPlayerOnCAVE2(GameObject source)
    {
        NetworkedVRPlayerManager player = source.GetComponent<NetworkedVRPlayerManager>();
        int netID = (int)player.networkID;
        if (!spawnedPlayerList.ContainsKey(netID))
        {
#if USING_GETREAL3D
            getReal3D.RpcManager.call("SpawnNetworkPlayerRPC", netID, player.playerName, player.playerType);
#endif
            Debug.Log("Added " + netID + " to list");
            spawnedPlayerList.Add(netID, source);
        }
    }
#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    void SpawnNetworkPlayerRPC(int netID, string playerName, string playerType)
    {
        Debug.Log("Added " + netID + " to CAVE2 display client");
        
        GameObject newNetPlayer = Instantiate(lobbyManager.gamePlayerPrefab) as GameObject;
        newNetPlayer.name = "CAVE2 Client (remote " +netID + ")";

        NetworkedVRPlayerManager vrNetPlayer = newNetPlayer.GetComponent<NetworkedVRPlayerManager>();
        vrNetPlayer.playerName = playerName;
        vrNetPlayer.playerType = playerType;
        vrNetPlayer.SetNetID((uint)netID);

        clientPlayerList.Add(netID, vrNetPlayer);
    }
#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    void UpdateNetworkPlayerRPC(int netID, Vector3 position, Quaternion rotation, Vector3 headPos, Quaternion headRot)
    {
        if( clientPlayerList.ContainsKey(netID) )
        {
            NetworkedVRPlayerManager vrNetPlayer = clientPlayerList[netID] as NetworkedVRPlayerManager;
            vrNetPlayer.playerPosition = new Vector3(position.x, position.y, position.z);
            vrNetPlayer.playerRotation = rotation;
            vrNetPlayer.headPosition = headPos;
            vrNetPlayer.headRotation = headRot;
        }
    }
}
