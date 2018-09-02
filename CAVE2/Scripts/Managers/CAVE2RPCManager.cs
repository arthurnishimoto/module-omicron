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

    [SerializeField]
    bool debug;

    private void Start()
    {
        msgServer = new NetworkServerSimple();
        msgClient = new NetworkClient();

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

        msgServer.RegisterHandler(MsgType.Connect, serverOnClientConnect);
        msgServer.RegisterHandler(MsgType.Disconnect, serverOnClientDisconnect);

        msgServer.Listen(serverListenPort);
        Debug.Log("Starting message server on port " + serverListenPort);
    }

    private void StartNetClient()
    {
        Debug.Log("Msg Client: Connecting to server " + serverIP + ":" + serverListenPort);
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
        Debug.Log("Msg Server: Client connected. Total " + connections.Count);

        foreach (NetworkConnection client in connections)
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
                Debug.Log("sending: " + msgStr);
                msgServer.SendWriterTo(client.connectionId, writer, 0);
            }
        }
    }

    void ClientOnRecvMsg(NetworkMessage msg)
    {
        // Reset reader index
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();
        string[] msgStrArray = msgString.Split('|');

        for(int i = 0; i < msgStrArray.Length; i++)
        {
            Debug.Log("[" + i + "] = '" + msgStrArray[i] + "'");
        }
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param)
    {
        if (debug)
        {
            Debug.Log("CAVE2 BroadcastMessage (Param 1) '" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(targetObjectName + "|" + methodName + "|" + param);
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

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2)
    {
        if (debug)
        {
            Debug.Log("CAVE2 BroadcastMessage (Param 4)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(targetObjectName + "|" + methodName + "|" + param + "|" + param2);
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

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, object param3)
    {
        if (debug)
        {
            Debug.Log("CAVE2 BroadcastMessage (Param 5)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(targetObjectName + "|" + methodName + "|" + param + "|" + param2 + "|" + param3);
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

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
        if (debug)
        {
            Debug.Log("CAVE2 BroadcastMessage (Param 6)'" + methodName + "' on " + targetObjectName);
        }

        ServerSendMsgToClients(targetObjectName + "|" + methodName + "|" + param + "|" + param2 + "|" + param3 + "|" + param4);
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

    public void BroadcastMessage(string targetObjectName, string methodName)
    {
        ServerSendMsgToClients(targetObjectName + "|" + methodName);
        BroadcastMessage(targetObjectName, methodName, 0);
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
