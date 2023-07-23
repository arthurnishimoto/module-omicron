using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_2020_3_OR_NEWER
#else
#pragma warning disable CS0618 // Type or member is obsolete
public class CAVE2NetworkManager : MonoBehaviour
{
    [SerializeField]
    bool dontDestroyOnLoad = true;

    // Custom NetMsg IDs
    private short ClientConnect = 1000;
    private short ServerAssign = 1001;
    private short PosUpdate = 1002;
    // private short CAVE2BroadcastMsg1 = 101;
    // private short CAVE2BroadcastMsg2 = 102;
    // private short CAVE2SendMsg = 200;
    // private short CAVE2SendMsg1 = 201;
    // private short CAVE2SendMsg2 = 201;

    public enum NetMode { None, Server, Client, Display };

    [SerializeField]
    NetMode networkMode = NetMode.Server;

    [SerializeField]
    string serverIP;

    [SerializeField]
    int serverListenPort = 5555;

    [SerializeField]
    string localIP;

    string hostName;

    NetworkServerSimple netServer;
    NetworkClient netClient;

    [SerializeField]
    Text debugText = null;

    public enum ConnectState { None, Connecting, Connected, Disconnected, Reconnecting };

    ConnectState connectState = ConnectState.None;
    int connID = 0;

    Dictionary<string, ClientInfo> clientList;
    Dictionary<string, GameObject> gameObjectList;

    public void Start()
    {
        gameObjectList = new Dictionary<string, GameObject>();

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (serverIP.Length == 0)
            serverIP = "127.0.0.1";

        switch (networkMode)
        {
            case (NetMode.Server):
                StartServer();
                break;
            case (NetMode.Client):
                StartClient();
                break;
            default:
                break;
        }

        hostName = System.Net.Dns.GetHostName();
        System.Net.IPAddress[] iPAddresses = System.Net.Dns.GetHostEntry(hostName).AddressList;
        foreach(System.Net.IPAddress addr in iPAddresses)
        {
            localIP = addr.ToString();
        }
        
    }

    public void Update()
    {
        switch (networkMode)
        {
            case (NetMode.Server):
                UpdateServer();
                break;
            case (NetMode.Client):
                if (debugText)
                {
                    debugText.text = "Local: " + hostName + " (" + localIP + ")\n";
                    debugText.text += "Server: " + serverIP + ":" + serverListenPort + "\n";
                    string status = "UNKNOWN";
                    switch(connectState)
                    {
                        case (ConnectState.None): status = "Not Connected"; break;
                        case (ConnectState.Connecting): status = "Connecting"; break;
                        case (ConnectState.Connected): status = "Connected (connID: " + connID + ")"; break;
                        case (ConnectState.Disconnected): status = "Disconnected"; break;

                    }
                    debugText.text += "Status: " + status + "\n";
                }
                break;
            default:
                break;
        }

        
    }

    public void UpdateServer()
    {
        netServer.Update();
    }

    public void StartServer()
    {
        clientList = new Dictionary<string, ClientInfo>();

        netServer = new NetworkServerSimple();

        netServer.RegisterHandler(ClientConnect, OnServerClientConnected);
        netServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

        if (netServer.Listen(serverIP, serverListenPort))
        {
            Debug.Log("Started server on " + serverIP + ":" + serverListenPort);
        }
    }

    public void StartClient()
    {
        Debug.Log("Client has started - connecting to " + serverIP + ":" + serverListenPort);
        netClient = new NetworkClient();

        netClient.RegisterHandler(MsgType.Connect, OnClientConnected);
        netClient.RegisterHandler(MsgType.Disconnect, OnServerDisconnected);
        netClient.RegisterHandler(ServerAssign, OnServerAssignment);
        netClient.RegisterHandler(PosUpdate, OnServerPositionUpdate);

        netClient.Connect(serverIP, serverListenPort);
        connectState = ConnectState.Connecting;
    }

    private void SendTo(int connID, short msgType, MessageBase msg, int channel = 0)
    {
        NetworkWriter netWriter = new NetworkWriter();
        netWriter.StartMessage(msgType);
        msg.Serialize(netWriter);
        netWriter.FinishMessage();
        netServer.SendWriterTo(connID, netWriter, channel);
    }

    // Derived from NetworkServer
    // https://github.com/jamesjlinden/unity-decompiled/blob/master/UnityEngine.Networking/NetworkServer.cs
    private void SendToClient(int connectionId, short msgType, MessageBase msg, int channel = 0)
    {
        if (connectionId < netServer.connections.Count)
        {
            NetworkConnection connection = netServer.connections[connectionId];
            if (connection != null)
            {
                connection.Send(msgType, msg);
                return;
            }
        }
        if (!LogFilter.logError)
            return;
        Debug.LogError((object)("Failed to send message to connection ID '" + (object)connectionId + ", not found in connection list"));
    }

    // Derived from NetworkServer
    // https://github.com/jamesjlinden/unity-decompiled/blob/master/UnityEngine.Networking/NetworkServer.cs
    private bool SendToAll(short msgType, MessageBase msg, int channel = 0)
    {
        bool flag = true;
        for (int index = 0; index < netServer.connections.Count; ++index)
        {
            NetworkConnection connection = netServer.connections[index];
            if (connection != null)
                flag &= connection.Send(msgType, msg);
        }
        return flag;
    }

    // Server function handlers --------------------------------------------------------------
    void OnServerClientConnected(NetworkMessage netmsg)
    {
        ClientInfoMsg clientInfo = netmsg.ReadMessage<ClientInfoMsg>();

        string clientID = clientInfo.clientIP + ":" + netmsg.conn.connectionId;

        if (clientList.ContainsKey(clientID))
        {
        }
        else
        {
            ClientInfo newClient = new ClientInfo();
            newClient.connID = netmsg.conn.connectionId;

            newClient.clientIP = clientInfo.clientIP;
            newClient.hostName = clientInfo.hostName;
            newClient.deviceModel = clientInfo.deviceModel;
            newClient.deviceType = clientInfo.deviceType;
            newClient.instanceID = 0;
            newClient.active = true;

            Debug.Log("Client " + clientID + " connected to server");

            // Informs client of its server assigned connID
            ServerAssignmentMsg assignMsg = new ServerAssignmentMsg();
            assignMsg.connID = newClient.connID;
            SendTo(newClient.connID, ServerAssign, assignMsg);
        }
    }

    void OnClientDisconnected(NetworkMessage netmsg)
    {
        Debug.Log("Client " + netmsg.conn.connectionId + " disconnected");
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        GameObject targetGameObject;
        if (!gameObjectList.ContainsKey(targetObjectName))
        {
            targetGameObject = GameObject.Find(targetObjectName);
        }
        else
        {
            targetGameObject = gameObjectList[targetObjectName];
        }

        if (targetGameObject != null)
        {
            gameObjectList[targetObjectName] = targetGameObject;

            // Call locally
            gameObjectList[targetObjectName].BroadcastMessage(methodName, param);

            // Send to others
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        GameObject targetGameObject;
        if (!gameObjectList.ContainsKey(targetObjectName))
        {
            targetGameObject = GameObject.Find(targetObjectName);
        }
        else
        {
            targetGameObject = gameObjectList[targetObjectName];
        }

        if (targetGameObject != null)
        {
            gameObjectList[targetObjectName] = targetGameObject;

            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, param);

            // Send to others

        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        GameObject targetGameObject;
        if (!gameObjectList.ContainsKey(targetObjectName))
        {
            targetGameObject = GameObject.Find(targetObjectName);
        }
        else
        {
            targetGameObject = gameObjectList[targetObjectName];
        }

        if (targetGameObject != null)
        {
            gameObjectList[targetObjectName] = targetGameObject;

            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2 });

            // Send to others

        }
    }

    public void SendMessage2(NetworkMessage netmsg)
    {

    }

    public void UpdatePosition(string targetObjectName, Vector3 position, bool isLocal = false, int channel = Channels.DefaultUnreliable)
    {
        GameObject targetGameObject;
        if (!gameObjectList.ContainsKey(targetObjectName))
        {
            targetGameObject = GameObject.Find(targetObjectName);
        }
        else
        {
            targetGameObject = gameObjectList[targetObjectName];
        }

        if (targetGameObject != null)
        {
            gameObjectList[targetObjectName] = targetGameObject;

            ObjPosMsg msg = new ObjPosMsg();
            msg.gameObjectName = targetObjectName;
            msg.position = position;
            msg.isLocal = isLocal;

            SendToAll(PosUpdate, msg, channel);
        }
    }

    public void OnServerPositionUpdate(NetworkMessage netmsg)
    {
        ObjPosMsg msg = netmsg.ReadMessage<ObjPosMsg>();
        string targetObjectName = msg.gameObjectName;

        GameObject targetGameObject = gameObjectList[targetObjectName];

        if (targetGameObject == null)
        {
            targetGameObject = GameObject.Find(targetObjectName);
        }

        if (targetGameObject != null)
        {
            gameObjectList[targetObjectName] = targetGameObject;

            if (msg.isLocal)
            {
                targetGameObject.transform.localPosition = msg.position;
            }
            else
            {
                targetGameObject.transform.position = msg.position;
            }
        }
    }

    // Client function handlers --------------------------------------------------------------
    void OnClientConnected(NetworkMessage netmsg)
    {
        Debug.Log("Client connected to server");

        ClientInfoMsg clientInfo = new ClientInfoMsg();
        clientInfo.clientIP = localIP;
        clientInfo.hostName = hostName;
        clientInfo.deviceType = SystemInfo.deviceType.ToString();
        clientInfo.deviceModel = SystemInfo.deviceModel;
        netClient.Send(ClientConnect, clientInfo);

        connectState = ConnectState.Connected;
    }

    void OnServerDisconnected(NetworkMessage netmsg)
    {
        Debug.Log("Server disconnected");
        connectState = ConnectState.Disconnected;
    }

    // Any initial client information after connecting with server
    void OnServerAssignment(NetworkMessage netmsg)
    {
        ServerAssignmentMsg serverAssign = netmsg.ReadMessage<ServerAssignmentMsg>();
        connID = serverAssign.connID; // Server assigned connection ID
    }
}

public class ClientInfo
{
    public string clientIP;
    public string hostName;
    public int connID;
    public string deviceModel;
    public string deviceType;
    public int instanceID = 0;
    public bool active;
}

public class ClientInfoMsg : MessageBase
{
    public string clientIP;
    public string hostName;
    public string deviceModel;
    public string deviceType;
    public int connID;
    public int processID;
    public Vector2 windowPosition;
    public Vector2 windowSize;
}

public class ServerAssignmentMsg : MessageBase
{
    public int connID;
}

public class ObjPosMsg : MessageBase
{
    public string gameObjectName;
    public Vector3 position;
    public bool isLocal;
}

public class CAVE2Msg : MessageBase
{
    public string gameObjectName;
    public string methodName;
}

#pragma warning restore CS0618 // Type or member is obsolete
#endif