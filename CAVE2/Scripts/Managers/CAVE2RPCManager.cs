/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2023		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2023, Electronic Visualization Laboratory, University of Illinois at Chicago
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
using System;
using Unity.Collections;
using UnityEngine.XR;
using System.IO;

#if UNITY_2020_3_OR_NEWER && USING_CAVE2
// Requires 'Unity Transport' package to be installed
// from Package Manager
using Unity.Networking.Transport;
using System.Net;

public class CAVE2RPCManager : MonoBehaviour
{
    internal int cave2RPCCallCount;

    // CAVE2 Internal RPC Message IDs
    const uint RPC_SetConnID = 1000;

    [Header("Message Server")]
    [SerializeField]
    bool useMsgServer;

    [SerializeField]
    int serverListenPort = 9105;

    private NetworkDriver m_ServerDriver;
    private NativeList<NetworkConnection> m_Connections;

    [Header("Message Client")]
    [SerializeField]
    bool useMsgClient;

    [SerializeField]
    int connID = -1;

    [SerializeField]
    string serverIP = null;

    private NetworkDriver m_ClientDriver;
    private NetworkConnection m_ClientConnection;

    [SerializeField]
    bool connectedToServer;

    [SerializeField]
    bool autoReconnectClient;

    [SerializeField]
    float autoReconnectDelay = 1.0f;

    [SerializeField]
    float autoReconnectTimer;

    [SerializeField]
    int reconnectCount = -1;

    [Header("Debug")]
    [SerializeField]
    bool selfTestRoutine;

    bool serverRunning;
    bool clientRunning;

    public void EnableMsgServer(bool value)
    {
        if (selfTestRoutine)
            return;

        useMsgServer = value;
        Debug.Log("MsgServer " + (value ? "enabled" : "disabled"));
    }

    public void EnableMsgClient(bool value)
    {
        if (selfTestRoutine)
            return;

        useMsgClient = value;
        Debug.Log("MsgClient " + (value ? "enabled" : "disabled"));
    }

    private void Start()
    {
        reconnectCount = -1;

        if (selfTestRoutine)
        {
            RunSelfTest();
        }

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

    private void SetupNetworking()
    {
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    void RunSelfTest()
    {
        serverIP = "127.0.0.1";
        serverListenPort = 9004;

        useMsgServer = true;
        useMsgClient = true;
    }

    private void StartNetServer()
    {
        m_ServerDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = (ushort)serverListenPort;
        if (m_ServerDriver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind to port " + serverListenPort);
        }
        else
        {
            m_ServerDriver.Listen();
            Debug.Log("Starting message server on port " + serverListenPort);
            serverRunning = true;
        }
    }

    private void StartNetClient()
    {
        m_ClientDriver = NetworkDriver.Create();
        m_ClientConnection = default(NetworkConnection);

        ConnectNetClient();
    }

    private void ConnectNetClient()
    {
        NetworkEndPoint.TryParse(serverIP, (ushort)serverListenPort, out var endpoint);
        m_ClientConnection = m_ClientDriver.Connect(endpoint);

        if (m_ClientConnection.IsCreated)
        {
            string reconnectStr = " Reconnect count: " + reconnectCount;
            if (reconnectCount > 0)
            {
                Debug.Log("Msg Client: Connecting to server " + serverIP + ":" + serverListenPort + reconnectStr);
            }
            else
            {
                Debug.Log("Msg Client: Connecting to server " + serverIP + ":" + serverListenPort);
            }
            clientRunning = true;

            reconnectCount++;
        }
    }

    public void OnDestroy()
    {
        if (m_ServerDriver.IsCreated)
        {
            m_ServerDriver.Dispose();
        }
        m_Connections.Dispose();
    }

    private void Update()
    {
        if (useMsgServer)
        {
            UpdateServer();
        }
        if (useMsgClient)
        {
            UpdateClient();
        }
    }

    private void UpdateServer()
    {
        if (m_ServerDriver.IsCreated)
        {
            m_ServerDriver.ScheduleUpdate().Complete();

            // Clean up connections
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                {
                    m_Connections.RemoveAtSwapBack(i);
                    --i;
                }
            }

            // Accept new connections
            NetworkConnection c;
            while ((c = m_ServerDriver.Accept()) != default(NetworkConnection))
            {
                m_Connections.Add(c);
                Debug.Log("Msg Server: Client " + c.InternalId + " connected.");
                SetClientID(c);
            }

            DataStreamReader stream;
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    continue;

                NetworkEvent.Type cmd;
                while ((cmd = m_ServerDriver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Msg Server: Client " + c.InternalId + " disconnected.");
                        m_Connections[i] = default(NetworkConnection);
                    }
                }
            }
        }
        else
        {
            serverRunning = false;
        }
    }

    private void UpdateClient()
    {
        if (clientRunning == false && autoReconnectClient)
        {
            autoReconnectTimer += Time.deltaTime;

            if (autoReconnectTimer > autoReconnectDelay)
            {
                ConnectNetClient();
                autoReconnectTimer = 0;
            }
        }

        if (m_ClientDriver.IsCreated)
        {
            m_ClientDriver.ScheduleUpdate().Complete();

            if (!m_ClientConnection.IsCreated)
            {
                if (!connectedToServer)
                    Debug.Log("Msg Client: Failed to create connection.");
                clientRunning = false;
                return;
            }
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_ClientConnection.PopEvent(m_ClientDriver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("Msg Client: Connected to " + serverIP + ":" + serverListenPort);
                    connectedToServer = true;
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    uint msgType = stream.ReadUInt();

                    if (msgType < 1000) // Generic CAVE2 RPC Calls (Send/BroadcastMessage)
                    {
                        FixedString32Bytes targetGameObject = stream.ReadFixedString32();
                        FixedString32Bytes targetFunction = stream.ReadFixedString32();
                        uint paramCount = stream.ReadUInt();

                        switch (msgType)
                        {
                            case (201):
                                SendCAVE2RPC(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream));
                                break;
                            case (202):
                                SendCAVE2RPC2(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream), ReadObject(ref stream));
                                break;
                            case (203):
                                SendCAVE2RPC3(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream));
                                break;
                            case (204):
                                SendCAVE2RPC4(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream));
                                break;
                            case (205):
                                SendCAVE2RPC5(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream));
                                break;
                            case (207):
                                SendCAVE2RPC7(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream));
                                break;
                            case (216):
                                SendCAVE2RPC16(targetGameObject.ToString(), targetFunction.ToString(), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream)
                                    , ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream), ReadObject(ref stream)
                                    );
                                break;
                            default:
                                Debug.Log("Unhandled msgType: " + msgType);
                                break;
                        }
                    }
                    else // CAVE2 Internal Calls
                    {
                        switch (msgType)
                        {
                            case (RPC_SetConnID):
                                connID = (int)stream.ReadUInt();
                                Debug.Log("Msg Client: Assigned connID " + connID + " from server.");
                                break;
                        }
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Msg Client: Disconnected from server.");
                    m_ClientConnection = default(NetworkConnection);
                    clientRunning = false;
                    autoReconnectTimer = 0;
                }
            }
        }
    }

    internal int GetConnID()
    {
        return connID;
    }

    internal bool IsReconnecting()
    {
        return autoReconnectTimer != 0;
    }

    public enum MsgType { Reliable, Unreliable, StateUpdate };

    /*
    void ServerSendToClient(int clientId, short messageID, MessageBase msg, MsgType msgType = MsgType.Reliable)
    {
        int channelId = reliableChannelId;
        switch (msgType)
        {
            case (MsgType.Reliable): channelId = reliableChannelId; break;
            case (MsgType.Unreliable): channelId = unreliableChannelId; break;
            case (MsgType.StateUpdate): channelId = stateUpdateChannelId; break;
        }

        if (clientIDs.Contains(clientId))
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
    */

    enum ObjectTypeID { Object, EnumInt32, Vector3, Vector2, Single, Boolean, String, Quaternion };

    private ObjectTypeID ObjectToTypeID(object obj)
    {
        if (obj is System.Enum || obj is System.Int32)
        {
            return ObjectTypeID.EnumInt32;
        }
        else if (obj is Vector3)
        {
            return ObjectTypeID.Vector3;
        }
        else if (obj is Vector2)
        {
            return ObjectTypeID.Vector2;
        }
        else if (obj is System.Single)
        {
            return ObjectTypeID.Single;
        }
        else if (obj is System.Boolean)
        {
            return ObjectTypeID.Boolean;
        }
        else if (obj is System.String)
        {
            return ObjectTypeID.String;
        }
        else if (obj is Quaternion)
        {
            return ObjectTypeID.Quaternion;
        }

        return ObjectTypeID.Object;
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

        return "OBJECT";
    }
    private void ParamToByte(NetworkDriver writer, object param, string typeOverride = null)
    {
        /*
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
        else
        {
            Debug.LogWarning("CAVE2RPCManager: Unknown param " + TypeToString(param));
        }
        */
    }

    public void Destroy(string targetObjectName)
    {

    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, MsgType msgType = MsgType.Reliable)
    {
        throw new NotImplementedException();
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, MsgType msgType = MsgType.Reliable)
    {
        throw new NotImplementedException();
    }

    private DataStreamWriter WriteObject(object param, DataStreamWriter writer)
    {
        writer.WriteUInt((uint)ObjectToTypeID(param));

        if (ObjectToTypeID(param) == ObjectTypeID.EnumInt32)
        {
            writer.WriteInt((int)param);
        }
        else if (ObjectToTypeID(param) == ObjectTypeID.Vector3)
        {
            Vector3 vector3 = (Vector3)param;
            writer.WriteFloat(vector3.x);
            writer.WriteFloat(vector3.y);
            writer.WriteFloat(vector3.z);
        }
        else if (ObjectToTypeID(param) == ObjectTypeID.Vector2)
        {
            Vector2 vector2 = (Vector2)param;
            writer.WriteFloat(vector2.x);
            writer.WriteFloat(vector2.y);
        }
        else if (ObjectToTypeID(param) == ObjectTypeID.Single)
        {
            writer.WriteFloat((float)param);
        }
        else if (ObjectToTypeID(param) == ObjectTypeID.Boolean)
        {
            writer.WriteInt((int)param);
        }
        else if (ObjectToTypeID(param) == ObjectTypeID.String)
        {
            writer.WriteFixedString32((string)param);
        }
        else if (ObjectToTypeID(param) == ObjectTypeID.Quaternion)
        {
            Quaternion quaternion = (Quaternion)param;
            writer.WriteFloat(quaternion.x);
            writer.WriteFloat(quaternion.y);
            writer.WriteFloat(quaternion.z);
            writer.WriteFloat(quaternion.w);
        }
        return writer;
    }

    private object ReadObject(ref DataStreamReader reader)
    {
        uint objectType = reader.ReadUInt();
        ObjectTypeID objectTypeID = (ObjectTypeID)objectType;

        switch (objectTypeID)
        {
            case (ObjectTypeID.EnumInt32):
                return reader.ReadInt();
            case (ObjectTypeID.Vector3):
                Vector3 vector3;
                vector3.x = reader.ReadFloat();
                vector3.y = reader.ReadFloat();
                vector3.z = reader.ReadFloat();
                return vector3;
            case (ObjectTypeID.Vector2):
                Vector2 vector2;
                vector2.x = reader.ReadFloat();
                vector2.y = reader.ReadFloat();
                return vector2;
            case (ObjectTypeID.Single):
                return reader.ReadFloat();
            case (ObjectTypeID.Boolean):
                return reader.ReadInt();
            case (ObjectTypeID.String):
                return reader.ReadFixedString32();
            case (ObjectTypeID.Quaternion):
                Quaternion quaternion;
                quaternion.x = reader.ReadFloat();
                quaternion.y = reader.ReadFloat();
                quaternion.z = reader.ReadFloat();
                quaternion.w = reader.ReadFloat();
                return quaternion;
            default:
                return null;
        }
    }
    internal void SetClientID(NetworkConnection connection)
    {
        m_ServerDriver.BeginSend(NetworkPipeline.Null, connection, out DataStreamWriter writer);
        if (writer.IsCreated)
        {
            writer.WriteUInt(RPC_SetConnID); // MessageType
            writer.WriteInt(connection.InternalId);
            m_ServerDriver.EndSend(writer);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, MsgType msgType = MsgType.Reliable, int connID = -1)
    {
        uint paramCount = 1;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            NetworkConnection client = m_Connections[i];
            if (connID == -1 || client.InternalId == connID)
            {
                m_ServerDriver.BeginSend(NetworkPipeline.Null, client, out DataStreamWriter writer);
                if (writer.IsCreated)
                {
                    writer.WriteUInt(201); // MessageType
                    writer.WriteFixedString32(targetObjectName);
                    writer.WriteFixedString32(methodName);
                    writer.WriteUInt(paramCount); // ParamCount

                    writer = WriteObject(param, writer);

                    m_ServerDriver.EndSend(writer);
                }
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, MsgType msgType = MsgType.Reliable)
    {
        uint paramCount = 2;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_ServerDriver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            if (writer.IsCreated)
            {
                writer.WriteUInt(202); // MessageType
                writer.WriteFixedString32(targetObjectName);
                writer.WriteFixedString32(methodName);
                writer.WriteUInt(paramCount); // ParamCount

                writer = WriteObject(param, writer);
                writer = WriteObject(param2, writer);

                m_ServerDriver.EndSend(writer);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, MsgType msgType = MsgType.Reliable)
    {
        uint paramCount = 3;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_ServerDriver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            if (writer.IsCreated)
            {
                writer.WriteUInt(203); // MessageType
                writer.WriteFixedString32(targetObjectName);
                writer.WriteFixedString32(methodName);
                writer.WriteUInt(paramCount); // ParamCount

                writer = WriteObject(param, writer);
                writer = WriteObject(param2, writer);
                writer = WriteObject(param3, writer);

                m_ServerDriver.EndSend(writer);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, MsgType msgType = MsgType.Reliable)
    {
        uint paramCount = 4;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_ServerDriver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            if (writer.IsCreated)
            {
                writer.WriteUInt(204); // MessageType
                writer.WriteFixedString32(targetObjectName);
                writer.WriteFixedString32(methodName);
                writer.WriteUInt(paramCount); // ParamCount

                writer = WriteObject(param, writer);
                writer = WriteObject(param2, writer);
                writer = WriteObject(param3, writer);
                writer = WriteObject(param4, writer);

                m_ServerDriver.EndSend(writer);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, MsgType msgType = MsgType.Reliable)
    {
        uint paramCount = 5;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_ServerDriver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            if (writer.IsCreated)
            {
                writer.WriteUInt(205); // MessageType
                writer.WriteFixedString32(targetObjectName);
                writer.WriteFixedString32(methodName);
                writer.WriteUInt(paramCount); // ParamCount

                writer = WriteObject(param, writer);
                writer = WriteObject(param2, writer);
                writer = WriteObject(param3, writer);
                writer = WriteObject(param4, writer);
                writer = WriteObject(param5, writer);

                m_ServerDriver.EndSend(writer);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, MsgType msgType = MsgType.Reliable)
    {
        uint paramCount = 7;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_ServerDriver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            if (writer.IsCreated)
            {
                writer.WriteUInt(207); // MessageType
                writer.WriteFixedString32(targetObjectName);
                writer.WriteFixedString32(methodName);
                writer.WriteUInt(paramCount); // ParamCount

                writer = WriteObject(param, writer);
                writer = WriteObject(param2, writer);
                writer = WriteObject(param3, writer);
                writer = WriteObject(param4, writer);
                writer = WriteObject(param5, writer);
                writer = WriteObject(param6, writer);
                writer = WriteObject(param7, writer);

                m_ServerDriver.EndSend(writer);
            }
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        uint paramCount = 16;

        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_ServerDriver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            if (writer.IsCreated)
            {
                writer.WriteUInt(216); // MessageType
                writer.WriteFixedString32(targetObjectName);
                writer.WriteFixedString32(methodName);
                writer.WriteUInt(paramCount); // ParamCount

                writer = WriteObject(param, writer);
                writer = WriteObject(param2, writer);
                writer = WriteObject(param3, writer);
                writer = WriteObject(param4, writer);
                writer = WriteObject(param5, writer);
                writer = WriteObject(param6, writer);
                writer = WriteObject(param7, writer);
                writer = WriteObject(param8, writer);
                writer = WriteObject(param9, writer);
                writer = WriteObject(param10, writer);
                writer = WriteObject(param11, writer);
                writer = WriteObject(param12, writer);
                writer = WriteObject(param13, writer);
                writer = WriteObject(param14, writer);
                writer = WriteObject(param15, writer);
                writer = WriteObject(param16, writer);
                m_ServerDriver.EndSend(writer);
            }
        }
    }

    public void SendCAVE2RPC(string targetObjectName, string methodName, object param)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if(targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, param);
        }
    }

    public void SendCAVE2RPC2(string targetObjectName, string methodName, object param, object param2)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if (targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, new object[] { param, param2 });
        }
    }

    public void SendCAVE2RPC3(string targetObjectName, string methodName, object param, object param2, object param3)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if (targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, new object[] { param, param2, param3 });
        }
    }

    public void SendCAVE2RPC4(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if (targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, new object[] { param, param2, param3, param4 });
        }
    }

    public void SendCAVE2RPC5(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if (targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5});
        }
    }

    public void SendCAVE2RPC7(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if (targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7 });
        }
    }

    public void SendCAVE2RPC16(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16)
    {
        GameObject targetGameObject = GameObject.Find(targetObjectName);
        if (targetGameObject != null)
        {
            targetGameObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 });
        }
    }
}
#elif USING_CAVE2
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
    bool autoUseHeadNodeAsServerIP = true;

    [SerializeField]
    bool debugMsg = false;

    bool clientConnectedToServer = false;

    [SerializeField]
    bool autoReconnect = true;

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

    void ConfigurationLoaded(DefaultConfig config)
    {
        ClusterConfig cConfig = ConfigurationManager.loadedConfig.clusterConfig;
        if (cConfig.headNodeIPAddesss.Length > 0 && autoUseHeadNodeAsServerIP)
        {
            serverIP = cConfig.headNodeIPAddesss;
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
                    else
                    {
                        ClientOnDisconnect();
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
        GetComponent<CAVE2ClusterManager>().RemoveClient(clientConnectionId);
    }

    void OnServerReceiveClientInfo(ClientInfoMsg msg)
    {
        LogUI("Msg Server: Client info " + msg.connID + ": " + msg.hostName + " (" + msg.deviceType + ")");
        LogUI("     Process ID: " + msg.processID);
        GetComponent<CAVE2ClusterManager>().AddClient(msg.connID, msg.hostName, msg.deviceType);
    }

    void ClientOnConnect()
    {
        LogUI("Msg Client: Connected to " + serverIP + ":" + serverListenPort);
        
        clientConnectedToServer = true;
        reconnectAttemptCount = 0;
    }

    void ClientOnDisconnect()
    {
        LogUI("Msg Client: Disconnected");
        clientConnectedToServer = false;

        if (autoReconnect)
        {
            StartNetClient();
        }
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
#else
public class CAVE2RPCManager : MonoBehaviour
{
    public enum MsgType { Reliable, Unreliable, StateUpdate };

    public int cave2RPCCallCount;

    public void EnableMsgServer(bool value)
    {

    }

    public void EnableMsgClient(bool value)
    {

    }

    internal int GetConnID()
    {
        return -1;
    }

    internal bool IsReconnecting()
    {
        return false;
    }

    public void Destroy(string targetObjectName)
    {
        GameObject target = GameObject.Find(targetObjectName);
        if (target != null)
        {
            Destroy(target);
        }
    }

    public void SendTerminalMsg(string msgString, bool useReliable, int forwardingID = -1)
    {

    }

    // Helper function to search for a gameobject by name.
    // Original implementation: Simple/expensive GameObject.Find() every time
    // TODO Optimization: GameObject.Find() first time, lookup table for repeated calls
    GameObject GetGameObject(string targetObjectName)
    {
        return GameObject.Find(targetObjectName);
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, MsgType msgType = MsgType.Reliable, int connID = -1)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, MsgType msgType = MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        GameObject targetObject = GetGameObject(targetObjectName);
        if (targetObject != null)
        {
            targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 }, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SendCAVE2RPC(string targetObjectName, string methodName, object param)
    {
    }

    public void SendCAVE2RPC2(string targetObjectName, string methodName, object param, object param2)
    {
    }

    public void SendCAVE2RPC3(string targetObjectName, string methodName, object param, object param2, object param3)
    {
    }

    public void SendCAVE2RPC4(string targetObjectName, string methodName, object param, object param2, object param3, object param4)
    {
    }

    public void SendCAVE2RPC5(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5)
    { 
    }

    public void SendCAVE2RPC7(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7)
    {
    }

    public void SendCAVE2RPC16(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16)
    {
    }
}
#endif

