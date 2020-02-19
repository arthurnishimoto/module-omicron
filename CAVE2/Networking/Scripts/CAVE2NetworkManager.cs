using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CAVE2NetworkManager : MonoBehaviour
{
    private short ClientConnect = 1000;

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
    int connID = -1;

    public void Start()
    {
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
                        case (ConnectState.Connected): status = "Connected"; break;
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

        netClient.Connect(serverIP, serverListenPort);
        connectState = ConnectState.Connecting;
    }

    // Server function handlers
    void OnServerClientConnected(NetworkMessage netmsg)
    {
        ClientInfoMsg clientInfo = netmsg.ReadMessage<ClientInfoMsg>();

        Debug.Log("Client " + clientInfo.clientIP + " (" + clientInfo.hostName + " " + clientInfo.deviceType + ": " + clientInfo.deviceModel + ") connected to server as connID " + clientInfo.connID);
    }

    void OnClientDisconnected(NetworkMessage netmsg)
    {
        Debug.Log("Client disconnected");
    }

    // Client function handlers
    void OnClientConnected(NetworkMessage netmsg)
    {
        Debug.Log("Client connected to server");
        connID = netClient.connection.connectionId;

        ClientInfoMsg clientInfo = new ClientInfoMsg();
        clientInfo.clientIP = localIP;
        clientInfo.hostName = hostName;
        clientInfo.connID = connID;
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
}

public class ClientInfoMsg : MessageBase
{
    public string clientIP;
    public string hostName;
    public int connID;
    public string deviceModel;
    public string deviceType;
}
