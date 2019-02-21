/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2018		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2018, Electronic Visualization Laboratory, University of Illinois at Chicago
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

#if USING_GETREAL3D
public class CAVE2RPCManager : getReal3D.MonoBehaviourWithRpc
{
#else
public class CAVE2RPCManager : MonoBehaviour {
#endif

    // Cluster Sync
    public int cave2RPCCallCount;

    [SerializeField]
    bool debugRPC = false;

    // Remote Networking
    [Header("Message Server")]
    [SerializeField]
    public bool useMsgServer;

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
    public bool useMsgClient;

    NetworkClient msgClient;
    NetworkMessageDelegate clientOnConnect;
    NetworkMessageDelegate clientOnDisconnect;
    NetworkMessageDelegate clientOnData;

    [SerializeField]
    string serverIP;

    [SerializeField]
    bool debugMsg;

    bool connected = false;

    [SerializeField]
    bool autoReconnect = true;

    [SerializeField]
    float autoReconnectDelay = 5;

    float autoReconnectTimer;
    int reconnectAttemptCount;

    [SerializeField]
    RemoteTerminal remoteTerminal;

    string defaultTargetObjectName;

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

        if(useMsgClient && autoReconnect && !connected)
        {
            if (autoReconnectTimer < autoReconnectDelay)
            {
                autoReconnectTimer += Time.deltaTime;
            }
            else
            {
                reconnectAttemptCount++;
                LogUI("Msg Client: Reconnecting to server " + serverIP + ":" + serverListenPort + " (Attempt: " + reconnectAttemptCount + ")");
                msgClient.Disconnect();
                msgClient.Connect(serverIP, serverListenPort);

                autoReconnectTimer = 0;
            }
        }

    }

    public bool IsReconnecting()
    {
        return (reconnectAttemptCount > 0);
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
        connected = true;
        reconnectAttemptCount = 0;
    }

    void ClientOnDisconnect(NetworkMessage msg)
    {
        LogUI("Msg Client: Disconnected");
        connected = false;
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
                if(debugMsg)
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
        if (debugMsg)
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

    public void SendMessage(string targetObjectName, string methodName, object param, bool useReliable = true)
    {
        if (debugRPC)
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
            targetObject.SendMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, bool useReliable = true)
    {
        if (debugRPC)
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
            targetObject.SendMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, bool useReliable = true)
    {
        if (debugRPC)
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
            targetObject.SendMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, bool useReliable = true)
    {
        if (debugRPC)
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
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, bool useReliable = true)
    {
        if (debugRPC)
        {
            LogUI("CAVE2 BroadcastMessage (Param 9)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(methodName + "|" + targetObjectName + "|" + param + "|" + param2 + "|" + param3 + "|" + param4 + "|" + param5 + "|" + param6 + "|" + param7, useReliable);
#if USING_GETREAL3D
        if (getReal3D.Cluster.isMaster)
            getReal3D.RpcManager.call("SendCAVE2RPC9", targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7);
        else
            SendCAVE2RPC9(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7 }, SendMessageOptions.DontRequireReceiver);
        }
#endif
    }

    public void SendMessage(string targetObjectName, string methodName, bool useReliable = true)
    {
        ServerSendMsgToClients(methodName + "|" + targetObjectName, useReliable);
        SendMessage(targetObjectName, methodName, 0, useReliable);
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

        if (debugRPC)
            Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
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
        if (debugRPC)
            Debug.Log ("SendCAVE2RPC4: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
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
        if (debugRPC)
            Debug.Log ("SendCAVE2RPC5: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
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
        if (debugRPC)
            Debug.Log ("SendCAVE2RPC6: call '" +methodName +"' on "+targetObjectName);

        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC6 failed to find gameObject '" + targetObjectName + "'");
        }
    }

    [getReal3D.RPC]
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
        else
        {
            Debug.LogWarning("CAVE2RPCManager: SendCAVE2RPC9 failed to find gameObject '" + targetObjectName + "'");
        }
    }

    [getReal3D.RPC]
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
#endif
}
