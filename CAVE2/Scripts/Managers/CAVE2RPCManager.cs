using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

#if USING_GETREAL3D
public class CAVE2RPCManager : getReal3D.MonoBehaviourWithRpc
{
#else
public class CAVE2RPCManager : MonoBehaviour {
#endif

    // Cluster Sync
    public int cave2RPCCallCount;

    // Remote Networking
    [Header("Message Server")]
    [SerializeField]
    bool useMsgServer;

    static short MessageID = 1104;
    NetworkServerSimple msgServer;
    NetworkMessageDelegate serverOnClientConnect;
    NetworkMessageDelegate serverOnClientDisconnect;

    [SerializeField]
    int serverListenPort = 9105;

    [Header("Message Client")]
    [SerializeField]
    bool useMsgClient;

    NetworkClient msgClient;
    NetworkMessageDelegate clientOnConnect;
    NetworkMessageDelegate clientOnDisconnect;
    NetworkMessageDelegate clientOnData;

    [SerializeField]
    string serverIP;

    private void Start()
    {
        msgServer = new NetworkServerSimple();

        if(useMsgServer)
        {
            StartNetServer();
        }
        if (useMsgClient)
        {
            StartNetClient();
        }
    }

    private void StartNetServer()
    {
        msgServer.Listen(serverListenPort);

        serverOnClientConnect += ServerOnClientConnect;
        serverOnClientDisconnect += ServerOnClientDisconnect;

        msgServer.RegisterHandler(MsgType.Connect, serverOnClientConnect);
        msgServer.RegisterHandler(MsgType.Disconnect, serverOnClientDisconnect);

        Debug.Log("Starting message server on port " + serverListenPort);
    }

    private void StartNetClient()
    {
        Debug.Log("Connecting to message server " + serverIP + ":" + serverListenPort);
        msgClient.Connect(serverIP, serverListenPort);

        clientOnConnect += ClientOnConnect;
        clientOnDisconnect += ClientOnDisconnect;
        clientOnData += ClientOnRecvMsg;

        msgClient.RegisterHandler(MsgType.Connect, clientOnConnect);
        msgClient.RegisterHandler(MsgType.Disconnect, clientOnDisconnect);
        msgClient.RegisterHandler(MessageID, clientOnData);
    }

    void ServerOnClientConnect(NetworkMessage msg)
    {
        System.Collections.ObjectModel.ReadOnlyCollection<NetworkConnection> connections = msgServer.connections;

        foreach(NetworkConnection client in connections)
        {
            if(client != null)
            {
                Debug.Log("Msg Server: Client ID " + client.connectionId + " '" + client.address + "' connected");
            }
        } 
    }

    void ServerOnClientDisconnect(NetworkMessage msg)
    {
        Debug.Log("Msg Server: Client disconnected");
    }

    void ClientOnConnect(NetworkMessage msg)
    {
        Debug.Log("Msg Client: Connected to " + serverIP);
    }

    void ClientOnDisconnect(NetworkMessage msg)
    {
        Debug.Log("Msg Client: Disconnected");
    }

    void ServerSendMsgToClients(string msgStr)
    {
        System.Collections.ObjectModel.ReadOnlyCollection<NetworkConnection> connections = msgServer.connections;

        foreach (NetworkConnection client in connections)
        {
            if (client != null)
            {
                NetworkWriter writer = new NetworkWriter();
                writer.StartMessage(MessageID);
                writer.Write(msgStr);
                writer.FinishMessage();

                msgServer.SendWriterTo(client.connectionId, writer, 0);
            }
        }
    }

    void ClientOnRecvMsg(NetworkMessage msg)
    {
        // Reset reader index
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();
        string[] msgStrArray = msgString.Split(' ');

        for(int i = 0; i < msgStrArray.Length; i++)
        {
            Debug.Log("[" + i + "] = '" + msgStrArray[i] + "'");
        }
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param)
    {
        //Debug.Log ("Broadcast '" +methodName +"' on "+ targetObjectName);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC", targetObjectName, methodName, param);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2)
    {
        //Debug.Log ("Broadcast '" +methodName +"' on "+ targetObjectName);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC4", targetObjectName, methodName, param, param2);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, object param3)
    {
        //Debug.Log ("Broadcast '" +methodName +"' on "+ targetObjectName);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC5", targetObjectName, methodName, param, param2, param3);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
        //Debug.Log ("Broadcast '" +methodName +"' on "+ targetObjectName);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC6", targetObjectName, methodName, param, param2, param3, param4);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName)
    {
        BroadcastMessage(targetObjectName, methodName, 0);
    }

    public void Destroy(string targetObjectName)
    {
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("CAVE2DestroyRPC", targetObjectName);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            Destroy(targetObject);
        }
#endif
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
    public void SendCAVE2RPC(string targetObjectName, string methodName, object param)
    {
        cave2RPCCallCount++;
        //Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
    }

    [getReal3D.RPC]
    public void SendCAVE2RPC4(string targetObjectName, string methodName, object param, object param2)
    {
        cave2RPCCallCount++;
        //Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    [getReal3D.RPC]
    public void SendCAVE2RPC5(string targetObjectName, string methodName, object param, object param2, object param3)
    {
        cave2RPCCallCount++;
        //Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    [getReal3D.RPC]
    public void SendCAVE2RPC6(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
        cave2RPCCallCount++;
        //Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    [getReal3D.RPC]
    public void CAVE2DestroyRPC(string targetObjectName)
    {
        cave2RPCCallCount++;
        //Debug.Log ("SendCAVE2RPC: call 'Destroy' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            Destroy(targetObject);
        }
    }
#endif
}
