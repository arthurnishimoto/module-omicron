/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2022		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2022, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
 
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable CS0618 // Type or member is obsolete

#if USING_GETREAL3D
public class CAVE2RPCManager : getReal3D.MonoBehaviourWithRpc
{
#else
public class CAVE2RPCManager : MonoBehaviour {
#endif
    delegate void CAVE2RPCDelegate();

    // Cluster Sync
    public int cave2RPCCallCount;

    // Remote Networking
    HostTopology topology;
    [SerializeField] int hostId;
    int connectionId;
    [SerializeField] int connID;

    HashSet<int> clientIDs = new HashSet<int>();
#if USING_GETREAL3D
    [SerializeField]
    bool useGetReal3DRPC = true;
#endif
    [Header("Message Server")]
    [SerializeField]
    bool useMsgServer;

    const short Msg_RemoteTerminal = 1104;
    const short Msg_ClientInfo = 1101;
    // static short Msg_CAVE2RPC2 = 202;
    // static short Msg_CAVE2RPC16 = 216;
    // static short Msg_OmicronSensorList = 1100;

    NetworkServerSimple msgServer;
    NetworkMessageDelegate serverOnClientConnect;
    NetworkMessageDelegate serverOnClientDisconnect;
    NetworkMessageDelegate serverOnData;

    Hashtable serverMessageDelegates = new Hashtable();

    [SerializeField]
    int serverListenPort = 9105;

    [SerializeField]
    int reliableChannelId;

    [SerializeField]
    int unreliableChannelId;

    [SerializeField]
    int stateUpdateChannelId;

    [SerializeField]
    int maxConnections = 100;

    public enum MsgType { Reliable, Unreliable, StateUpdate };

    float serverUpdateDataDelay = 1.0f;
    float serverUpdateTimer;

    [Header("Message Client")]
    [SerializeField]
    bool useMsgClient;

    NetworkClient msgClient;
    NetworkMessageDelegate clientOnConnect;
    NetworkMessageDelegate clientOnDisconnect;
    NetworkMessageDelegate clientOnData;
    NetworkMessageDelegate clientOnReceiveOmicronSensorList;
    NetworkMessageDelegate clientOnSendMsg2;
    NetworkMessageDelegate clientOnSendMsg16;
    Hashtable clientMessageDelegates = new Hashtable();

    [SerializeField]
    string serverIP = null;

    [SerializeField]
    bool debugMsg = false;

    bool clientConnectedToServer = false;

    // [SerializeField]
    // bool autoReconnect = true;

    // [SerializeField]
    // float autoReconnectDelay = 5;

    float autoReconnectTimer;
    int reconnectAttemptCount;

    [SerializeField]
    RemoteTerminal remoteTerminal = null;

    string defaultTargetObjectName;

    [Header("Debug")]
    [SerializeField]
    bool debugRPC = false;

    [SerializeField]
    bool hideLogWarning = true;

    [SerializeField]
    bool debugNetSpeed = false;

    [SerializeField]
    float clientSendRate;

    int nPacketsSent;
    float packetOutTimer;

    int nPacketsReceived;
    float packetInTimer;

    [SerializeField]
    float clientReceiveRate;

    public void LogUI(string msg)
    {
        if (remoteTerminal)
            remoteTerminal.PrintUI(msg);
        else
            Debug.Log(msg);
    }

    public int GetConnID()
    {
        return connID;
    }

    public void EnableMsgServer(bool value)
    {
        useMsgServer = value;
        // Debug.Log("MsgServer " + (value ? "enabled" : "disabled"));
    }

    public void EnableMsgClient(bool value)
    {
        useMsgClient = value;
        // Debug.Log("MsgClient " + (value ? "enabled" : "disabled"));
    }

    private void Start()
    {
        SetupNetworking();

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
        UpdateNetwork();

        if (debugNetSpeed)
        {
            packetOutTimer += Time.deltaTime;
            packetInTimer += Time.deltaTime;

            if (packetOutTimer > 5 && nPacketsSent > 0)
            {
                clientSendRate = packetOutTimer / nPacketsSent;
                packetOutTimer = 0;
                nPacketsSent = 0;
            }
            if (packetInTimer > 5 && nPacketsReceived > 0)
            {
                clientReceiveRate = packetInTimer / nPacketsReceived;
                packetInTimer = 0;
                nPacketsReceived = 0;
            }
        }

        if(serverUpdateTimer > serverUpdateDataDelay)
        {
            // Periodically update clients with latest sensor list
            // Replacing this string[] since getReal3D RPC dosen't support parameter of type string[]?
            //CAVE2.SendMessage(gameObject.name, "UpdateOmicronSensorList", CAVE2.Input.GetSensorList(), CAVE2.Input.GetWandControllerList());
            foreach (string sensor in CAVE2.Input.GetSensorList())
            {
                CAVE2.SendMessage(gameObject.name, "UpdateOmicronSensor", sensor);
            }
            foreach (string wand in CAVE2.Input.GetWandControllerList())
            {
                CAVE2.SendMessage(gameObject.name, "UpdateOmicronController", wand);
            }
            serverUpdateTimer = 0;
        }
        else
        {
            serverUpdateTimer += Time.deltaTime;
        }
    }

    public bool IsReconnecting()
    {
        return (reconnectAttemptCount > 0);
    }

    private void SetupNetworking()
    {
        GlobalConfig gConfig = new GlobalConfig();
        //gConfig.MaxPacketSize = 500;

        NetworkTransport.Init(gConfig);

        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);
        unreliableChannelId = config.AddChannel(QosType.Unreliable);
        stateUpdateChannelId = config.AddChannel(QosType.StateUpdate);

        topology = new HostTopology(config, maxConnections);
        msgClient = new NetworkClient();
    }

    private void StartNetServer()
    {
        hostId = NetworkTransport.AddHost(topology, serverListenPort);

        LogUI("Starting message server on port " + serverListenPort);
    }

    private void StartNetClient()
    {
        LogUI("Msg Client: Connecting to server " + serverIP + ":" + serverListenPort);

        byte error;
        hostId = NetworkTransport.AddHost(topology);
        connectionId = NetworkTransport.Connect(hostId, serverIP, serverListenPort, 0, out error);
        connID = connectionId;

        if (error != 0)
        {
            LogUI("Msg Client: Connection Error: " + ((NetworkError)error).ToString());
        }
        /*
        // Setup delegates
        clientOnReceiveOmicronSensorList += ClientOnReceiveOmicronSensorList;
        clientOnSendMsg2 += SendCAVE2RPC2;

        // Register handlers
        msgClient.RegisterHandler(Msg_CAVE2RPC2, clientOnSendMsg2);
        msgClient.RegisterHandler(Msg_OmicronSensorList, clientOnReceiveOmicronSensorList);

        msgClient.Configure(topology);
        msgClient.Connect(serverIP, serverListenPort);
        */
    }

    void UpdateNetwork()
    {
        NetworkEventType recData;
        do
        {
            int recHostId;
            int recConnectionId;
            int channelId;
            int bufferSize = 1024;
            byte[] recBuffer = new byte[bufferSize];
            int dataSize;
            byte error;
            recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            bool isClient = connectionId == recConnectionId;

            switch (recData)
            {
                case NetworkEventType.Nothing: break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("ConnectEvent");
                    if (isClient)
                    {
                        ClientOnConnect();
                    }
                    else
                    {
                        ServerOnClientConnect(recConnectionId);
                    }
                    break;
                case NetworkEventType.DataEvent:
                    NetworkReader networkReader = new NetworkReader(recBuffer);

                    // First two bytes is the msg size
                    byte[] readerMsgSizeData = networkReader.ReadBytes(2);
                    short readerMsgSize = (short)((readerMsgSizeData[1] << 8) + readerMsgSizeData[0]);

                    // Next two bytes is the msg type
                    byte[] readerMsgTypeData = networkReader.ReadBytes(2);
                    short readerMsgType = (short)((readerMsgTypeData[1] << 8) + readerMsgTypeData[0]);

                    // New delegate format
                    if (readerMsgType >= 1000)
                    {
                        bool knownMessage = true;
                        switch (readerMsgType)
                        {
                            case Msg_RemoteTerminal:
                                if (remoteTerminal == null)
                                {
                                    Debug.Log("Warning: Received RemoteTerminal message, but no Remote Terminal is assigned to CAVE2RPCManager!");
                                }
                                else
                                {
                                    int srcID = networkReader.ReadInt32();
                                    string msgString = networkReader.ReadString();

                                    if (debugMsg)
                                    {
                                        Debug.Log("RemoteTerminal from connID " + srcID + ": '" + msgString + "'" + connectionId);
                                        LogUI("connID " + srcID + ": " + msgString);
                                    }
                                    remoteTerminal.MsgFromCAVE2RPCManager(msgString);

                                    // If server forward message from client to other clients
                                    if (connectionId == 0)
                                    {
                                        SendTerminalMsg(msgString, true, srcID);
                                    }
                                }
                                break;
                            case Msg_ClientInfo:
                                if (isClient)
                                {
                                    OnClientReceiveClientInfo(networkReader.ReadMessage<ClientInfoMsg>());
                                }
                                else
                                {
                                    OnServerReceiveClientInfo(networkReader.ReadMessage<ClientInfoMsg>());
                                }
                                break;
                            default:
                                knownMessage = false;
                                break;
                        }

                        if (!knownMessage)
                        {
                            if (clientMessageDelegates.ContainsKey(readerMsgType))
                            {
                                NetworkMessageDelegate msgDel = (NetworkMessageDelegate)clientMessageDelegates[readerMsgType];

                                // OmicronSensorListMsg msg = networkReader.ReadMessage<OmicronSensorListMsg>();
                            }
                            else
                            {
                                Debug.Log("Warning: Unknown client message type " + readerMsgType);
                            }
                        }
                        return;
                    }
                    string targetObjectName = networkReader.ReadString();
                    string methodName = networkReader.ReadString();
                    int paramCount = networkReader.ReadInt32();

                    nPacketsReceived++;

                    switch (readerMsgType)
                    {
                        case (101): BroadcastCAVE2RPC(targetObjectName, methodName, ReaderToObject(networkReader)); break;
                        case (102): BroadcastCAVE2RPC4(targetObjectName, methodName, ReaderToObject(networkReader), ReaderToObject(networkReader)); break;
                        case (201): SendCAVE2RPC(targetObjectName, methodName, ReaderToObject(networkReader)); break;
                        case (202):
                            SendCAVE2RPC4(targetObjectName, methodName,
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader)); break;
                        case (203):
                            SendCAVE2RPC5(targetObjectName, methodName,
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader)); break;
                        case (204):
                            SendCAVE2RPC6(targetObjectName, methodName,
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader)); break;
                        case (205):
                            SendCAVE2RPC7(targetObjectName, methodName,
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader)); break;
                        case (207):
                            SendCAVE2RPC9(targetObjectName, methodName,
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader)); break;
                        case (216):
                            SendCAVE2RPC18(targetObjectName, methodName,
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader),
                            ReaderToObject(networkReader)); break;
                    }
                    break;

                case NetworkEventType.DisconnectEvent:
                    Debug.Log("DisconnectEvent");
                    if (connectionId != recConnectionId)
                    {
                        ServerOnClientDisconnect(recConnectionId);
                    }
                    break;
                case NetworkEventType.BroadcastEvent:
                    Debug.Log("BroadcastEvent");
                    break;
            }

        } while (recData != NetworkEventType.Nothing);
    }

    void ServerOnClientConnect(int clientConnectionId)
    {
        LogUI("Msg Server: Client " + clientConnectionId + " connected.");
        clientIDs.Add(clientConnectionId);

        // Assign new clientID to client
        ClientInfoMsg clientIDMsg = new ClientInfoMsg();
        clientIDMsg.connID = clientConnectionId;

        ServerSendToClient(clientConnectionId, Msg_ClientInfo, clientIDMsg);

        /*
        OmicronSensorListMsg sensorListMsg = new OmicronSensorListMsg();
        sensorListMsg.sensorList = CAVE2.Input.GetSensorList();
        sensorListMsg.controllerList = CAVE2.Input.GetWandControllerList();

        ServerSendToClient(clientConnectionId, Msg_OmicronSensorList, sensorListMsg);
        */
        CAVE2.SendMessage(gameObject.name, "UpdateOmicronSensorList", CAVE2.Input.GetSensorList(), CAVE2.Input.GetWandControllerList());
    }

    void ServerOnClientDisconnect(int clientConnectionId)
    {
        LogUI("Msg Server: Client " + clientConnectionId + " disconnected.");
        clientIDs.Remove(clientConnectionId);
    }

    void OnServerReceiveClientInfo(ClientInfoMsg msg)
    {
        LogUI("Msg Server: Client info " + msg.connID + ": " + msg.hostName + " (" + msg.deviceType + ")");
        LogUI("     Process ID: " + msg.processID);
    }

    void ClientOnConnect()
    {
        LogUI("Msg Client: Connected to " + serverIP + ":" + serverListenPort);
        
        clientConnectedToServer = true;
        reconnectAttemptCount = 0;
    }

    void ClientOnDisconnect(NetworkMessage msg)
    {
        LogUI("Msg Client: Disconnected");
        clientConnectedToServer = false;
    }

    void ClientOnReceiveOmicronSensorList(NetworkMessage msg)
    {

    }

    void OnClientReceiveClientInfo(ClientInfoMsg msg)
    {
        // Server assigned connID
        connID = msg.connID;

        // Note this can be different from connectionId!

        // Sets the window title to include this window's processID
        var currentProc = System.Diagnostics.Process.GetCurrentProcess();
        
        // Appends a unique negative connID to the window title.
        // This is a temporary connID assignment so that each window will
        // have a unique ID.
        // This will later be compared and reassigned
        CAVE2ClusterManager.SetWindowTitle(-connID);

        // Confirm connID with server and send additional client info
        msg.connID = connID;
        msg.hostName = CAVE2Manager.GetMachineName();
        msg.clientIP = "[IP ADDRESS]";
        msg.processID = currentProc.Id;
        msg.deviceType = Application.platform.ToString();
        // msg.deviceType = SystemInfo.deviceType.ToString();
        // msg.deviceModel = SystemInfo.deviceModel;

        ClientSendToServer(Msg_ClientInfo, msg);

        
        
    }

    public void SendTerminalMsg(string msgString, bool useReliable, int forwardingID = -1)
    {
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(Msg_RemoteTerminal);
        if (forwardingID == -1)
        {
            writer.Write(connectionId);
        }
        else
        {
            writer.Write(forwardingID);
        }
        writer.Write(msgString);
        writer.FinishMessage();

        int channelId = useReliable ? reliableChannelId : unreliableChannelId;

        byte[] writerData = writer.AsArray();

        byte error;

        if (connectionId == 0)
        {
            // Server to client(s)
            foreach(int clientId in clientIDs)
            {
                if (debugMsg)
                {
                    Debug.Log("RemoteTerminal out: " + connectionId + "'" + msgString + "' " + hostId + " " + clientId + " " + forwardingID);
                }

                NetworkTransport.Send(hostId, clientId, channelId, writerData, writerData.Length, out error);
            }
        }
        else
        {
            // Client to Server
            NetworkTransport.Send(hostId, connectionId, channelId, writerData, writerData.Length, out error);

            if (debugMsg)
            {
                Debug.Log("RemoteTerminal out: " + connectionId + "'" + msgString + "' " + hostId + " " + connectionId);
            }
        }

        
    }

    void ServerSendToClient(int clientId, short messageID, MessageBase msg, MsgType msgType = MsgType.Reliable)
    {
        int channelId = reliableChannelId;
        switch (msgType)
        {
            case (MsgType.Reliable): channelId = reliableChannelId; break;
            case (MsgType.Unreliable): channelId = unreliableChannelId; break;
            case (MsgType.StateUpdate): channelId = stateUpdateChannelId; break;
        }

        if(clientIDs.Contains(clientId))
        {
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.StartMessage(messageID);
            networkWriter.Write(msg);

            networkWriter.FinishMessage();

            byte[] writerData = networkWriter.AsArray();

            byte error;
            NetworkTransport.Send(hostId, clientId, channelId, writerData, writerData.Length, out error);
            nPacketsSent++;
        }
        else
        {
            Debug.LogWarning("Unknown client ID: " + clientId);
        }
    }

    void ClientSendToServer(short messageID, MessageBase msg, MsgType msgType = MsgType.Reliable)
    {
        int channelId = reliableChannelId;
        switch (msgType)
        {
            case (MsgType.Reliable): channelId = reliableChannelId; break;
            case (MsgType.Unreliable): channelId = unreliableChannelId; break;
            case (MsgType.StateUpdate): channelId = stateUpdateChannelId; break;
        }

        NetworkWriter networkWriter = new NetworkWriter();
        networkWriter.StartMessage(messageID);
        networkWriter.Write(msg);

        networkWriter.FinishMessage();

        byte[] writerData = networkWriter.AsArray();

        byte error;
        NetworkTransport.Send(hostId, connectionId, channelId, writerData, writerData.Length, out error);
        nPacketsSent++;
    }

    void ServerSendMsgToClients(byte[] writerData, MsgType msgType)
    {
        int channelId = reliableChannelId;
        switch(msgType)
        {
            case (MsgType.Reliable): channelId = reliableChannelId; break;
            case (MsgType.Unreliable): channelId = unreliableChannelId; break;
            case (MsgType.StateUpdate): channelId = stateUpdateChannelId; break;
        }

        if (useMsgServer)
        {
            foreach (int clientId in clientIDs)
            {
                byte error;
                NetworkTransport.Send(hostId, clientId, channelId, writerData, writerData.Length, out error);
                nPacketsSent++;
            }
        }
        if(useMsgClient && clientConnectedToServer)
        {
            byte error;
            NetworkTransport.Send(hostId, connectionId, channelId, writerData, writerData.Length, out error);
            nPacketsSent++;
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
        if (debugMsg)
            LogUI("Msg Server: ServerOnRecvMsg '" + msgString + "'");
        ProcessMsg(msg);

        // Forward message to clients (except sender client);
        //ServerSendMsgToClients(msgString, true, msg.conn);
    }

    public void ProcessMsg(NetworkMessage msg)
    {
        // Reset reader index
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();
        char[] charSeparators = new char[] { '|' };
        string[] msgStrArray = msgString.Split(charSeparators, System.StringSplitOptions.RemoveEmptyEntries);

        GameObject targetObj = null;
        string targetObjectName = "";
        string functionName = msgStrArray[0];

        // Starting index of data fields
        // Assumes index:
        // 0: Function Name
        // 1: Target GameObject Name
        // 2: Data parameters
        // May change if a default GameObject is set i.e.
        // 0: Function Name
        //  : (GameObject name is not provided)
        // 1: Data parameters
        int startingDataIndex = 2;

        if (msgStrArray.Length < 2)
        {
            LogUI(msg.conn.address + " sent unknown msg '" + msgString + "'");
        }
        else
        {
            targetObjectName = msgStrArray[1];
            targetObj = GameObject.Find(targetObjectName);
            if(targetObj == null)
            {
                targetObjectName = defaultTargetObjectName;
                targetObj = GameObject.Find(targetObjectName);
                startingDataIndex = 1;
            }
        }

        if (targetObj != null)
        {
            // CAVE2 Transform Sync
            if (functionName.Equals("SyncPosition"))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

                targetObj.SendMessage("SyncPosition", new Vector3(x, y, z));
            }
            else if (functionName.Equals("SyncRotation"))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float w = 0;
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);
                float.TryParse(msgStrArray[startingDataIndex + 3], out w);

                targetObj.SendMessage("SyncRotation", new Quaternion(x, y, z, w));
            }

            // Generic Transform functions
            else if (functionName.Equals("translate", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

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
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

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
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

                targetObj.transform.position = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setEulerAngles", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

                targetObj.transform.eulerAngles = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setLocalPosition", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[2], out x);
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

                targetObj.transform.localPosition = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setLocalEulerAngles", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;
                float.TryParse(msgStrArray[startingDataIndex], out x);
                float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                float.TryParse(msgStrArray[startingDataIndex + 2], out z);

                targetObj.transform.localEulerAngles = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setLocalScale", System.StringComparison.OrdinalIgnoreCase))
            {
                float x = 0;
                float y = 0;
                float z = 0;

                if (msgStrArray.Length == 3)
                {
                    float.TryParse(msgStrArray[startingDataIndex], out x);
                    y = x;
                    z = x;
                }
                else
                {
                    float.TryParse(msgStrArray[startingDataIndex], out x);
                    float.TryParse(msgStrArray[startingDataIndex + 1], out y);
                    float.TryParse(msgStrArray[startingDataIndex + 2], out z);
                }

                targetObj.transform.localScale = new Vector3(x, y, z);
            }
            else if (functionName.Equals("setTargetObject", System.StringComparison.OrdinalIgnoreCase))
            {
                defaultTargetObjectName = targetObjectName;
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
        else
        {
            LogUI("CAVE2RPCManager: Msg target object '" + msgStrArray[1] + "' not found");
        }
    }

    private string TypeToString(object obj)
    {
        if (obj is System.Enum || obj is System.Int32)
        {
            return "Int32";
        }
        else if (obj is Vector3)
        {
            return "Vector3";
        }
        else if (obj is Vector2)
        {
            return "Vector2";
        }
        else if (obj is System.Single)
        {
            return "Single";
        }
        else if (obj is System.Boolean)
        {
            return "Boolean";
        }
        else if (obj is System.String)
        {
            return "String";
        }
        else if (obj is Quaternion)
        {
            return "Quaternion";
        }
        else if (obj is Color)
        {
            return "Color";
        }

        return "OBJECT";
    }

    private void ParamToByte(NetworkWriter writer, object param, string typeOverride = null)
    {
        if (typeOverride == null)
        {
            writer.Write(TypeToString(param));
        }

        if (param is System.Enum || param is System.Int32)
        {
            writer.Write((int)param);
        }
        else if (param is Vector3)
        {
            writer.Write((Vector3)param);
        }
        else if (param is Vector2)
        {
            writer.Write((Vector2)param);
        }
        else if (param is System.Single)
        {
            writer.Write((System.Single)param);
        }
        else if (param is System.Boolean)
        {
            writer.Write((System.Boolean)param);
        }
        else if (param is System.String)
        {
            writer.Write((System.String)param);
        }
        else if (param is Quaternion)
        {
            writer.Write((Quaternion)param);
        }
        else if (param is Color)
        {
            writer.Write((Color)param);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: Unknown param " + TypeToString(param));
        }
    }

    private object ReaderToObject(NetworkReader networkReader)
    {
        string type = networkReader.ReadString();
        if(type == "Int32")
        {
            return networkReader.ReadInt32();
        }
        else if (type == "Vector3")
        {
            return networkReader.ReadVector3();
        }
        else if (type == "Vector2")
        {
            return networkReader.ReadVector2();
        }
        else if (type == "Single")
        {
            return networkReader.ReadSingle();
        }
        else if (type == "Boolean")
        {
            return networkReader.ReadBoolean();
        }
        else if (type == "String")
        {
            return networkReader.ReadString();
        }
        else if (type.Contains("String["))
        {
            int arraySize = 0;
            string indexStr = type.Substring(type.IndexOf("[") + 1, type.Length - type.IndexOf("]"));
            if (int.TryParse(indexStr, out arraySize))
            {
                string[] strArr = new string[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    strArr[i] = networkReader.ReadString();
                }
                return strArr;
            }
            else
            {
                Debug.LogWarning("CAVE2RPCManager: Invalid array specification '" + type + "'");
                return null;
            }
        }
        else if (type == "Quaternion")
        {
            return networkReader.ReadQuaternion();
        }
        else if (type == "Color")
        {
            return networkReader.ReadColor();
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: Unknown param " + type);
            return null;
        }
    }
    
    void WriteParameters(NetworkWriter writer, object[] parameters)
    {
        int paramCount = 0;
        foreach(object o in parameters)
        {
            if(o is System.Array)
            {
                paramCount += ((object[])o).Length;
            }
            else
            {
                paramCount++;
            }
        }
        writer.Write(paramCount);

        foreach (object o in parameters)
        {
            if (o is System.Array)
            {
                object[] objArr = (object[])o;

                writer.Write(TypeToString(objArr[0]) + "[" + objArr.Length + "]");

                for(int i = 0; i < objArr.Length; i++)
                {
                    ParamToByte(writer, objArr[i], TypeToString(objArr[0]));
                }
            }
            else
            {
                ParamToByte(writer, o);
            }
            
        }
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, MsgType msgType = MsgType.Reliable)
    {
        if (debugRPC)
        {
            LogUI("CAVE2 BroadcastMessage (Param 1) '" + methodName + "' on " + targetObjectName);
        }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
        {
            getReal3D.RpcManager.call("BroadcastCAVE2RPC", targetObjectName, methodName, param);
        }
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(101);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] { param });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            BroadcastCAVE2RPC(targetObjectName, methodName, param);
        }
#else
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(101);
        writer.Write(targetObjectName);
        writer.Write(methodName);

        WriteParameters(writer, new object[] { param });

        writer.FinishMessage();

        ServerSendMsgToClients(writer.ToArray(), msgType);

        BroadcastCAVE2RPC(targetObjectName, methodName, param);
#endif
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, MsgType msgType = MsgType.Reliable)
    {
        if (debugRPC)
        {
            LogUI("CAVE2 BroadcastMessage (Param 4) '" + methodName + "' on " + targetObjectName);
        }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("BroadcastCAVE2RPC4", targetObjectName, methodName, param, param2);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(102);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] { param, param2 });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            BroadcastCAVE2RPC4(targetObjectName, methodName, param, param2);
        }
#else
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(102);
        writer.Write(targetObjectName);
        writer.Write(methodName);

        WriteParameters(writer, new object[] { param, param2 });

        writer.FinishMessage();

        ServerSendMsgToClients(writer.ToArray(), msgType);

        BroadcastCAVE2RPC4(targetObjectName, methodName, param, param2);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, MsgType msgType = MsgType.Reliable)
    {
        if (debugRPC)
        {
            LogUI("CAVE2 SendMessage (Param 1) '" + methodName + "' on " + targetObjectName);
        }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC", targetObjectName, methodName, param);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(201);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] { param });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            SendCAVE2RPC(targetObjectName, methodName, param);
        }
#else
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(201);
        writer.Write(targetObjectName);
        writer.Write(methodName);

        WriteParameters(writer, new object[] { param });

        writer.FinishMessage();

        ServerSendMsgToClients(writer.ToArray(), msgType);

        SendCAVE2RPC(targetObjectName, methodName, param);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, MsgType msgType = MsgType.Reliable)
    {
        if (debugRPC)
        {
            LogUI("CAVE2 SendMessage (Param 4)'" + methodName + "' on " + targetObjectName);
        }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC4", targetObjectName, methodName, param, param2);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(202);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] { param, param2 });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            SendCAVE2RPC4(targetObjectName, methodName, param, param2);
        }
#else
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(202);
        writer.Write(targetObjectName);
        writer.Write(methodName);

        WriteParameters(writer, new object[] { param, param2 });

        writer.FinishMessage();

        ServerSendMsgToClients(writer.ToArray(), msgType);

       SendCAVE2RPC4(targetObjectName, methodName, param, param2);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, MsgType msgType = MsgType.Reliable)
{
    if (debugRPC)
    {
        LogUI("CAVE2 SendMessage (Param 5)'" + methodName + "' on " + targetObjectName);
    }

#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC5", targetObjectName, methodName, param, param2, param3);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(203);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] { param, param2, param3 });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            SendCAVE2RPC5(targetObjectName, methodName, param, param2, param3);
        }
#else
    NetworkWriter writer = new NetworkWriter();
    writer.StartMessage(203);
    writer.Write(targetObjectName);
    writer.Write(methodName);

    WriteParameters(writer, new object[] { param, param2, param3 });

    writer.FinishMessage();

    ServerSendMsgToClients(writer.ToArray(), msgType);

    SendCAVE2RPC5(targetObjectName, methodName, param, param2, param3);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, MsgType msgType = MsgType.Reliable)
{
    if (debugRPC)
    {
        LogUI("CAVE2 SendMessage (Param 6)'" + methodName + "' on " + targetObjectName);
    }

#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC6", targetObjectName, methodName, param, param2, param3, param4);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(204);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] { param, param2, param3, param4 });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            SendCAVE2RPC6(targetObjectName, methodName, param, param2, param3, param4);
        }
#else
    NetworkWriter writer = new NetworkWriter();
    writer.StartMessage(204);
    writer.Write(targetObjectName);
    writer.Write(methodName);

    WriteParameters(writer, new object[] { param, param2, param3, param4 });

    writer.FinishMessage();

    ServerSendMsgToClients(writer.ToArray(), msgType);

    SendCAVE2RPC6(targetObjectName, methodName, param, param2, param3, param4);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, MsgType msgType = MsgType.Reliable)
{
    if (debugRPC)
    {
        LogUI("CAVE2 SendMessage (Param 6)'" + methodName + "' on " + targetObjectName);
    }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC7", targetObjectName, methodName, param, param2, param3, param4, param5);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(205);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] {
        param,
        param2,
        param3,
        param4,
        param5
    });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            SendCAVE2RPC7(targetObjectName, methodName, param, param2, param3, param4, param5);
        }
#else
    NetworkWriter writer = new NetworkWriter();
    writer.StartMessage(205);
    writer.Write(targetObjectName);
    writer.Write(methodName);

    WriteParameters(writer, new object[] {
        param,
        param2,
        param3,
        param4,
        param5
    });

    writer.FinishMessage();

    ServerSendMsgToClients(writer.ToArray(), msgType);

    SendCAVE2RPC7(targetObjectName, methodName, param, param2, param3, param4, param5);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, MsgType msgType = MsgType.Reliable)
{
    if (debugRPC)
    {
        LogUI("CAVE2 SendMessage (Param 9)'" + methodName + "' on " + targetObjectName);
    }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC9", targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(207);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] {
                param,
                param2,
                param3,
                param4,
                param5,
                param6,
                param7
            });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);
            SendCAVE2RPC9(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7);
        }
#else
    NetworkWriter writer = new NetworkWriter();
    writer.StartMessage(207);
    writer.Write(targetObjectName);
    writer.Write(methodName);

    WriteParameters(writer, new object[] {
        param,
        param2,
        param3,
        param4,
        param5,
        param6,
        param7
    });

    writer.FinishMessage();

    ServerSendMsgToClients(writer.ToArray(), msgType);
    SendCAVE2RPC9(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16, MsgType msgType = MsgType.Reliable)
{
    if (debugRPC)
    {
        LogUI("CAVE2 SendMessage (Param 18)'" + methodName + "' on " + targetObjectName);
    }
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("SendCAVE2RPC18", targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16);
        else
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage(216);
            writer.Write(targetObjectName);
            writer.Write(methodName);

            WriteParameters(writer, new object[] {
                param,
                param2,
                param3,
                param4,
                param5,
                param6,
                param7,
                param8,
                param9,
                param10,
                param11,
                param12,
                param13,
                param14,
                param15,
                param16
            });

            writer.FinishMessage();

            ServerSendMsgToClients(writer.ToArray(), msgType);

            SendCAVE2RPC18(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16);
        }
#else

    NetworkWriter writer = new NetworkWriter();
    writer.StartMessage(216);
    writer.Write(targetObjectName);
    writer.Write(methodName);

    WriteParameters(writer, new object[] {
        param,
        param2,
        param3,
        param4,
        param5,
        param6,
        param7,
        param8,
        param9,
        param10,
        param11,
        param12,
        param13,
        param14,
        param15,
        param16
    });

    writer.FinishMessage();

    ServerSendMsgToClients(writer.ToArray(), msgType);

    SendCAVE2RPC18(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16);
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, MsgType msgType = MsgType.Reliable)
    {
        SendMessage(targetObjectName, methodName, 0, msgType);
    }

    public void Destroy(string targetObjectName)
    {
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster && useGetReal3DRPC)
            getReal3D.RpcManager.call("CAVE2DestroyRPC", targetObjectName);
        else
            CAVE2DestroyRPC(targetObjectName);
#else
        CAVE2DestroyRPC(targetObjectName);
#endif
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void BroadcastCAVE2RPC(string targetObjectName, string methodName, object param)
    {
        cave2RPCCallCount++;

        if (debugRPC)
            Debug.Log("BroadcastCAVE2RPC: call '" + methodName + "' on " + targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: BroadcastCAVE2RPC failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void BroadcastCAVE2RPC4(string targetObjectName, string methodName, object param, object param2)
    {
        cave2RPCCallCount++;

        if (debugRPC)
            Debug.Log("BroadcastCAVE2RPC4: call '" + methodName + "' on " + targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: BroadcastCAVE2RPC failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC(string targetObjectName, string methodName, object param)
    {
        cave2RPCCallCount++;

        if (debugRPC)
            Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC4(string targetObjectName, string methodName, object param, object param2)
    {
        cave2RPCCallCount++;
        if (debugRPC)
            Debug.Log ("SendCAVE2RPC4: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC4 failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC5(string targetObjectName, string methodName, object param, object param2, object param3)
    {
        cave2RPCCallCount++;
        if (debugRPC)
            Debug.Log ("SendCAVE2RPC5: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC5 failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC6(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
        cave2RPCCallCount++;
        if (debugRPC)
            Debug.Log ("SendCAVE2RPC6: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC6 failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC7(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5)
    {
        cave2RPCCallCount++;
        if (debugRPC)
            Debug.Log("SendCAVE2RPC7: call '" + methodName + "' on " + targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC7 failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC9(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7)
    {
        cave2RPCCallCount++;
        if (debugRPC)
            Debug.Log("SendCAVE2RPC9: call '" + methodName + "' on " + targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC9 failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void SendCAVE2RPC18(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16)
    {
        cave2RPCCallCount++;
        if (debugRPC)
            Debug.Log("SendCAVE2RPC18: call '" + methodName + "' on " + targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 }, SendMessageOptions.DontRequireReceiver);
        }
        else if (!hideLogWarning)
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC18 failed to find gameObject '" + targetObjectName + "'");
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
#endif
    public void CAVE2DestroyRPC(string targetObjectName)
    {
        cave2RPCCallCount++;
        if (debugRPC)
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

}

#pragma warning restore CS0618 // Type or member is obsolete