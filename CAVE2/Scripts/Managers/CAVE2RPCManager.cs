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
    NetworkMessageDelegate serverOnData;

    [SerializeField]
    int serverListenPort = 9105;

    [SerializeField]
    int reliableChannelId;

    [SerializeField]
    int unreliableChannelId;

    [SerializeField]
    int maxConnections = 100;

    [Header("Message Client")]
    [SerializeField]
    bool useMsgClient;

    NetworkClient msgClient;
    NetworkMessageDelegate clientOnConnect;
    NetworkMessageDelegate clientOnDisconnect;
    NetworkMessageDelegate clientOnData;

    [SerializeField]
    string serverIP;

    [SerializeField]
    bool debug;

    [SerializeField]
    RemoteTerminal remoteTerminal;

    private void LogUI(string msg)
    {
        if (remoteTerminal)
            remoteTerminal.PrintUI(msg);
        else
            Debug.Log(msg);
    }

    private void Start()
    {
        msgServer = new NetworkServerSimple();
        msgClient = new NetworkClient();

        ConnectionConfig myConfig = new ConnectionConfig();
        reliableChannelId = myConfig.AddChannel(QosType.Reliable);
        unreliableChannelId = myConfig.AddChannel(QosType.Unreliable);

        msgServer.Configure(myConfig, maxConnections);
        msgClient.Configure(myConfig, maxConnections);

        if (useMsgServer)
        {
            StartNetServer();
        }
        if (useMsgClient)
        {
            StartNetClient();
        }
    }

    private void Update()
    {
        msgServer.Update();
    }

    private void StartNetServer()
    {
        serverOnClientConnect += ServerOnClientConnect;
        serverOnClientDisconnect += ServerOnClientDisconnect;
        serverOnData += ServerOnRecvMsg;

        msgServer.RegisterHandler(MsgType.Connect, serverOnClientConnect);
        msgServer.RegisterHandler(MsgType.Disconnect, serverOnClientDisconnect);
        msgServer.RegisterHandler(MessageID, serverOnData);
        
        msgServer.Listen(serverListenPort);
        LogUI("Starting message server on port " + serverListenPort);
    }

    private void StartNetClient()
    {
        LogUI("Msg Client: Connecting to server " + serverIP + ":" + serverListenPort);
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
        LogUI("Msg Server: Client connected. Total " + connections.Count);

        foreach (NetworkConnection client in connections)
        {
            if(client != null)
            {
                LogUI("Msg Server: Client ID " + client.connectionId + " '" + client.address + "' connected");
            }
        } 
    }

    void ServerOnClientDisconnect(NetworkMessage msg)
    {
        LogUI("Msg Server: Client disconnected");
    }

    void ClientOnConnect(NetworkMessage msg)
    {
        LogUI("Msg Client: Connected to " + serverIP);
    }

    void ClientOnDisconnect(NetworkMessage msg)
    {
        LogUI("Msg Client: Disconnected");
    }

    void ServerSendMsgToClients(string msgStr, bool useReliable = true, NetworkConnection ignoreClient = null)
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
                if(debug)
                    LogUI("sending: " + msgStr);
                msgServer.SendWriterTo(client.connectionId, writer, useReliable ? reliableChannelId : unreliableChannelId);
            }
        }
    }

    void ClientOnRecvMsg(NetworkMessage msg)
    {
        ProcessMsg(msg);
    }

    void ServerOnRecvMsg(NetworkMessage msg)
    {
        // Reset reader index
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();
        if (debug)
            LogUI("Msg Server: ServerOnRecvMsg '" + msgString + "'");
        ProcessMsg(msg);

        // Forward message to clients (except sender client);
        ServerSendMsgToClients(msgString, true, msg.conn);
    }

    public void ProcessMsg(NetworkMessage msg)
    {
        // Reset reader index
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();
        char[] charSeparators = new char[] { '|' };
        string[] msgStrArray = msgString.Split(charSeparators, System.StringSplitOptions.RemoveEmptyEntries);

        GameObject targetObj = GameObject.Find(msgStrArray[1]);
        string functionName = msgStrArray[0];

        if (targetObj != null)
        {
            // CAVE2 Transform Sync
            if (functionName.Equals("SyncPosition"))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                targetObj.BroadcastMessage("SyncPosition", new Vector3(x, y, z));
            }
            else if (functionName.Equals("SyncRotation"))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float w = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);
                float.TryParse(msgStrArray[5], out w);

                targetObj.BroadcastMessage("SyncRotation", new Quaternion(x, y, z, w));
            }

            // Generic Transform functions
            else if (functionName.Equals("translate", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                if(msgStrArray.Length == 6)
                {
                    if(msgStrArray[5].Equals("self", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObj.transform.Translate(x, y, z, Space.Self);
                    }
                    else if (msgStrArray[5].Equals("world", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObj.transform.Translate(x, y, z, Space.World);
                    }
                }
                else
                {
                    targetObj.transform.Translate(x, y, z, Space.Self);
                }
            }
            else if (functionName.Equals("rotate", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                if (msgStrArray.Length == 6)
                {
                    if (msgStrArray[5].Equals("self", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObj.transform.Rotate(x, y, z, Space.Self);
                    }
                    else if (msgStrArray[5].Equals("world", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObj.transform.Rotate(x, y, z, Space.World);
                    }
                }
                else
                {
                    targetObj.transform.Rotate(x, y, z, Space.Self);
                }
            }
            else if (functionName.Equals("setPosition", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                targetObj.transform.position = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setEulerAngles", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                targetObj.transform.eulerAngles = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setLocalPosition", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                targetObj.transform.localPosition = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setLocalEulerAngles", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[3], out y);
                float.TryParse(msgStrArray[4], out z);

                targetObj.transform.localEulerAngles = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setLocalScale", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;

                if (msgStrArray.Length == 3)
                {
                    float.TryParse(msgStrArray[2], out x);
                    y = x;
                    z = x;
                }
                else
                {
                    float.TryParse(msgStrArray[2], out x);
                    float.TryParse(msgStrArray[3], out y);
                    float.TryParse(msgStrArray[4], out z);
                }

                targetObj.transform.localScale = new Vector3(x, y, z);
            }

            // Let the object handle the message
            else
            {
                string[] paramArray = new string[msgStrArray.Length - 2];
                for(int i = 0; i < paramArray.Length; i++)
                {
                    paramArray[i] = msgStrArray[i + 2];
                }
                targetObj.SendMessage(functionName, paramArray);
            }
        }
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, bool useReliable = true)
    {
        if (debug)
        {
            LogUI("CAVE2 BroadcastMessage (Param 1) '" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(methodName + "|" + targetObjectName + "|" + param, useReliable);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC", targetObjectName, methodName, param);
        else
            SendCAVE2RPC(targetObjectName, methodName, param);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, bool useReliable = true)
    {
        if (debug)
        {
            LogUI("CAVE2 BroadcastMessage (Param 4)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(methodName + "|" + targetObjectName + "|" + param + "|" + param2, useReliable);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC4", targetObjectName, methodName, param, param2);
        else
            SendCAVE2RPC4(targetObjectName, methodName, param, param2);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, object param3, bool useReliable = true)
    {
        if (debug)
        {
            LogUI("CAVE2 BroadcastMessage (Param 5)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(methodName + "|" + targetObjectName + "|" + param + "|" + param2 + "|" + param3, useReliable);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC5", targetObjectName, methodName, param, param2, param3);
        else
            SendCAVE2RPC5(targetObjectName, methodName, param, param2, param3);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, bool useReliable = true)
    {
        if (debug)
        {
            LogUI("CAVE2 BroadcastMessage (Param 6)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(methodName + "|" + targetObjectName + "|" + param + "|" + param2 + "|" + param3 + "|" + param4, useReliable);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC6", targetObjectName, methodName, param, param2, param3, param4);
        else
            SendCAVE2RPC6(targetObjectName, methodName, param, param2, param3, param4);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, bool useReliable = true)
    {
        ServerSendMsgToClients(methodName + "|" + targetObjectName, useReliable);
        BroadcastMessage(targetObjectName, methodName, 0, useReliable);
    }

    public void Destroy(string targetObjectName)
    {
        ServerSendMsgToClients(targetObjectName + " CAVE2DestroyRPC");
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("CAVE2DestroyRPC", targetObjectName);
        else
            CAVE2DestroyRPC(targetObjectName);
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

        if (debug)
            Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC failed to find gameObject '" + targetObjectName + "'");
        }
    }

    [getReal3D.RPC]
    public void SendCAVE2RPC4(string targetObjectName, string methodName, object param, object param2)
    {
        cave2RPCCallCount++;
        if (debug)
            Debug.Log ("SendCAVE2RPC4: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC4 failed to find gameObject '" + targetObjectName + "'");
        }
    }

    [getReal3D.RPC]
    public void SendCAVE2RPC5(string targetObjectName, string methodName, object param, object param2, object param3)
    {
        cave2RPCCallCount++;
        if (debug)
            Debug.Log ("SendCAVE2RPC5: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC5 failed to find gameObject '" + targetObjectName + "'");
        }
    }

    [getReal3D.RPC]
    public void SendCAVE2RPC6(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
        cave2RPCCallCount++;
        if (debug)
            Debug.Log ("SendCAVE2RPC6: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC6 failed to find gameObject '" + targetObjectName + "'");
        }
    }

    [getReal3D.RPC]
    public void CAVE2DestroyRPC(string targetObjectName)
    {
        cave2RPCCallCount++;
        if (debug)
            Debug.Log ("SendCAVE2RPC: call 'Destroy' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            Destroy(targetObject);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: CAVE2DestroyRPC failed to find gameObject '" + targetObjectName + "'");
        }
    }
#endif
}
