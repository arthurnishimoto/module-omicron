using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CAVE2NetworkManager : MonoBehaviour
{
    [SerializeField]
    bool dontDestroyOnLoad = true;

    // Custom NetMsg IDs
    private short ClientConnect = 1000;
    private short ServerAssign = 1001;
    private short PosUpdate = 1002;
    private short DestroyObj = 1003;
    private short CAVE2Msg1 = 101;
    private short CAVE2Msg2 = 102;
    private short CAVE2Msg3 = 103;
    private short CAVE2Msg4 = 104;
    private short CAVE2Msg5 = 105;
    private short CAVE2Msg6 = 106;
    private short CAVE2Msg7 = 107;
    private short CAVE2Msg16 = 116;

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
    Text debugText;

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
        netClient.RegisterHandler(DestroyObj, OnServerDestroyGameObject);

        netClient.RegisterHandler(CAVE2Msg1, ReceiveMessage);
        netClient.RegisterHandler(CAVE2Msg2, ReceiveMessage2);
        netClient.RegisterHandler(CAVE2Msg3, ReceiveMessage3);
        netClient.RegisterHandler(CAVE2Msg4, ReceiveMessage4);
        netClient.RegisterHandler(CAVE2Msg5, ReceiveMessage5);
        netClient.RegisterHandler(CAVE2Msg6, ReceiveMessage6);
        netClient.RegisterHandler(CAVE2Msg7, ReceiveMessage7);
        netClient.RegisterHandler(CAVE2Msg16, ReceiveMessage16);

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
                connection.SendByChannel(msgType, msg, channel);
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
                flag &= connection.SendByChannel(msgType, msg, channel);
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

    private GameObject VerifyGameObject(string gameObjectName)
    {
        GameObject targetGameObject;
        if (!gameObjectList.ContainsKey(gameObjectName))
        {
            targetGameObject = GameObject.Find(gameObjectName);
        }
        else
        {
            targetGameObject = gameObjectList[gameObjectName];
        }
        gameObjectList[gameObjectName] = targetGameObject;

        return targetGameObject;
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].BroadcastMessage(methodName, param);

            // Send to others
            CAVE2Msg1 msg = new CAVE2Msg1();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.broadcast = true;
            msg.param = param;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg1, msg, (int)channel);
            }
            else
            {
                netClient.SendByChannel(CAVE2Msg1, msg, (int)channel);
            }
        }
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].BroadcastMessage(methodName, new object[] { param, param2 });

            // Send to others
            CAVE2Msg2 msg = new CAVE2Msg2();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.broadcast = true;
            msg.param = param;
            msg.param2 = param2;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg2, msg, (int)channel);
            }
            else
            {
                netClient.SendByChannel(CAVE2Msg2, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, param);

            // Send to others
            CAVE2Msg1 msg = new CAVE2Msg1();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg1, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg1, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2 });

            // Send to others
            CAVE2Msg2 msg = new CAVE2Msg2();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg2, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg2, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2, param3 });

            // Send to others
            CAVE2Msg3 msg = new CAVE2Msg3();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;
            msg.param3 = param3;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg3, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg3, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2, param3, param4 });

            // Send to others
            CAVE2Msg4 msg = new CAVE2Msg4();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;
            msg.param3 = param3;
            msg.param4 = param4;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg4, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg4, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2, param3, param4, param5 });

            // Send to others
            CAVE2Msg5 msg = new CAVE2Msg5();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;
            msg.param3 = param3;
            msg.param4 = param4;
            msg.param5 = param5;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg5, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg5, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6 });

            // Send to others
            CAVE2Msg6 msg = new CAVE2Msg6();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;
            msg.param3 = param3;
            msg.param4 = param4;
            msg.param5 = param5;
            msg.param6 = param6;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg6, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg6, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7 });

            // Send to others
            CAVE2Msg7 msg = new CAVE2Msg7();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;
            msg.param3 = param3;
            msg.param4 = param4;
            msg.param5 = param5;
            msg.param6 = param6;
            msg.param7 = param7;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg7, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg7, msg, (int)channel);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16, CAVE2RPCManager.MsgType channel = CAVE2RPCManager.MsgType.Reliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            gameObjectList[targetObjectName].SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 });

            // Send to others
            CAVE2Msg16 msg = new CAVE2Msg16();
            msg.gameObjectName = targetObjectName;
            msg.methodName = methodName;
            msg.param = param;
            msg.param2 = param2;
            msg.param3 = param3;
            msg.param4 = param4;
            msg.param5 = param5;
            msg.param6 = param6;
            msg.param7 = param7;
            msg.param8 = param8;
            msg.param9 = param9;
            msg.param10 = param10;
            msg.param11 = param11;
            msg.param12 = param12;
            msg.param13 = param13;
            msg.param14 = param14;
            msg.param15 = param15;
            msg.param16 = param16;

            if (networkMode == NetMode.Server)
            {
                SendToAll(CAVE2Msg16, msg, (int)channel);
            }
            else if (networkMode != NetMode.None)
            {
                netClient.SendByChannel(CAVE2Msg16, msg, (int)channel);
            }
        }
    }

    public void ReceiveMessage(NetworkMessage netmsg)
    {
        CAVE2Msg1 msg = netmsg.ReadMessage<CAVE2Msg1>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, msg.param);
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, msg.param);
            }
        }
    }

    public void ReceiveMessage2(NetworkMessage netmsg)
    {
        CAVE2Msg2 msg = netmsg.ReadMessage<CAVE2Msg2>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2 });
            }
        }
    }

    public void ReceiveMessage3(NetworkMessage netmsg)
    {
        CAVE2Msg3 msg = netmsg.ReadMessage<CAVE2Msg3>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3 });
            }
        }
    }

    public void ReceiveMessage4(NetworkMessage netmsg)
    {
        CAVE2Msg4 msg = netmsg.ReadMessage<CAVE2Msg4>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4 });
            }
        }
    }

    public void ReceiveMessage5(NetworkMessage netmsg)
    {
        CAVE2Msg5 msg = netmsg.ReadMessage<CAVE2Msg5>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5 });
            }
        }
    }

    public void ReceiveMessage6(NetworkMessage netmsg)
    {
        CAVE2Msg6 msg = netmsg.ReadMessage<CAVE2Msg6>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5, msg.param6 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5, msg.param6 });
            }
        }
    }

    public void ReceiveMessage7(NetworkMessage netmsg)
    {
        CAVE2Msg7 msg = netmsg.ReadMessage<CAVE2Msg7>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5, msg.param6, msg.param7 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5, msg.param6, msg.param7 });
            }
        }
    }

    public void ReceiveMessage16(NetworkMessage netmsg)
    {
        CAVE2Msg16 msg = netmsg.ReadMessage<CAVE2Msg16>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            if (msg.broadcast)
            {
                gameObjectList[targetObjectName].BroadcastMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5, msg.param6, msg.param7, msg.param8, msg.param9, msg.param10, msg.param11, msg.param12, msg.param13, msg.param14, msg.param15, msg.param16 });
            }
            else
            {
                gameObjectList[targetObjectName].SendMessage(msg.methodName, new object[] { msg.param, msg.param2, msg.param3, msg.param4, msg.param5, msg.param6, msg.param7, msg.param8, msg.param9, msg.param10, msg.param11, msg.param12, msg.param13, msg.param14, msg.param15, msg.param16 });
            }
        }
    }

    public void UpdatePosition(string targetObjectName, Vector3 position, bool isLocal = false, int channel = Channels.DefaultUnreliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            ObjPosMsg msg = new ObjPosMsg();
            msg.gameObjectName = targetObjectName;
            msg.position = position;
            msg.isLocal = isLocal;

            SendToAll(PosUpdate, msg, channel);
        }
    }

    public void Destroy(string targetObjectName, int channel = Channels.DefaultUnreliable)
    {
        if (VerifyGameObject(targetObjectName) != null)
        {
            // Call locally
            Destroy(gameObjectList[targetObjectName]);

            ObjPosMsg msg = new ObjPosMsg();
            msg.gameObjectName = targetObjectName;

            SendToAll(DestroyObj, msg, channel);
        }
    }

    public void OnServerPositionUpdate(NetworkMessage netmsg)
    {
        ObjPosMsg msg = netmsg.ReadMessage<ObjPosMsg>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            if (msg.isLocal)
            {
                gameObjectList[targetObjectName].transform.localPosition = msg.position;
            }
            else
            {
                gameObjectList[targetObjectName].transform.position = msg.position;
            }
        }
    }

    public void OnServerDestroyGameObject(NetworkMessage netmsg)
    {
        ObjPosMsg msg = netmsg.ReadMessage<ObjPosMsg>();
        string targetObjectName = msg.gameObjectName;

        if (VerifyGameObject(targetObjectName) != null)
        {
            Destroy(gameObjectList[targetObjectName]);
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
    public bool broadcast;
}

public class CAVE2Msg1 : CAVE2Msg
{
    public object param;
}

public class CAVE2Msg2 : CAVE2Msg
{
    public object param;
    public object param2;
}

public class CAVE2Msg3 : CAVE2Msg
{
    public object param;
    public object param2;
    public object param3;
}

public class CAVE2Msg4 : CAVE2Msg
{
    public object param;
    public object param2;
    public object param3;
    public object param4;
}

public class CAVE2Msg5 : CAVE2Msg
{
    public object param;
    public object param2;
    public object param3;
    public object param4;
    public object param5;
}

public class CAVE2Msg6 : CAVE2Msg
{
    public object param;
    public object param2;
    public object param3;
    public object param4;
    public object param5;
    public object param6;
}

public class CAVE2Msg7 : CAVE2Msg
{
    public object param;
    public object param2;
    public object param3;
    public object param4;
    public object param5;
    public object param6;
    public object param7;
}

public class CAVE2Msg16 : CAVE2Msg
{
    public object param;
    public object param2;
    public object param3;
    public object param4;
    public object param5;
    public object param6;
    public object param7;
    public object param8;
    public object param9;
    public object param10;
    public object param11;
    public object param12;
    public object param13;
    public object param14;
    public object param15;
    public object param16;
}
