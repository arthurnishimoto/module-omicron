using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RemoteTerminal : MonoBehaviour {

    [SerializeField]
    bool startServer;

    [SerializeField]
    bool connectToServer;

    bool isServer;
    bool connecting;

    // Server
    NetworkServerSimple server;
    NetworkMessageDelegate serverOnClientConnect;
    NetworkMessageDelegate serverOnClientDisconnect;
    NetworkMessageDelegate serverOnData;

    [SerializeField]
    int serverListenPort = 9104;
    static short TerminalMsgID = 1104;

    [SerializeField]
    int maxConnections = 100;

    int reliableChannelId;
    int unreliableChannelId;

    [SerializeField]
    Text terminalTextLog;

    [SerializeField]
    Text secondaryTerminalText;

    // Client
    [SerializeField]
    string serverIP = "localhost";
    NetworkClient client;
    NetworkMessageDelegate clientOnConnect;
    NetworkMessageDelegate clientOnDisconnect;
    NetworkMessageDelegate clientOnData;
    bool localConnection = false; // Flag to disable cyclic events if connecting to a local msg server

    // Command Terminal
    [SerializeField]
    InputField commandLine;
    ArrayList cmdHistory = new ArrayList();
    int currentCmdHistoryLine;

    ArrayList terminalHistory = new ArrayList();

    [SerializeField]
    GameObject selectedObject;

    public void Start()
    {
        server = new NetworkServerSimple();
        serverOnClientConnect += ServerOnClientConnect;
        serverOnClientDisconnect += ServerOnClientDisconnect;
        serverOnData += ServerOnData;

        server.RegisterHandler(MsgType.Connect, serverOnClientConnect);
        server.RegisterHandler(MsgType.Disconnect, serverOnClientDisconnect);
        server.RegisterHandler(TerminalMsgID, serverOnData);

        client = new NetworkClient();
        clientOnConnect += ClientOnConnect;
        clientOnDisconnect += ClientOnDisconnect;
        clientOnData += ClientOnData;

        client.RegisterHandler(MsgType.Connect, clientOnConnect);
        client.RegisterHandler(MsgType.Disconnect, clientOnDisconnect);
        client.RegisterHandler(TerminalMsgID, clientOnData);

        ConnectionConfig myConfig = new ConnectionConfig();
        reliableChannelId = myConfig.AddChannel(QosType.Reliable);
        unreliableChannelId = myConfig.AddChannel(QosType.Unreliable);

        server.Configure(myConfig, maxConnections);
        client.Configure(myConfig, maxConnections);
    }

    public void Update()
    {
        if(startServer)
        {
            StartServer();
            startServer = false;
            isServer = true;
        }
        if (connectToServer)
        {
            ConnectToServer();
            connectToServer = false;
        }

        server.Update();
        UpdateTerminal();
    }
    public void StartServer()
    {
        server.Listen(serverListenPort);
        PrintUI("Server: Starting on port " + serverListenPort);
    }

    public void StartClient(bool local = false)
    {
        if (!connecting)
        {
            ConnectToServer(local);
        }
    }

    public void ShowInputField(bool value)
    {
        commandLine.gameObject.SetActive(value);
    }

    public void PrintUI(object log)
    {
        terminalTextLog.text = "";
        terminalHistory.Add(log.ToString());
        if(terminalHistory.Count > 25)
        {
            terminalHistory.RemoveRange(0, 1);
        }
        if (terminalTextLog)
        {
            int i = 0;
            foreach(string s in terminalHistory)
            {
                terminalTextLog.text += "\n" + s;
                i++;
            }
            if (secondaryTerminalText)
                secondaryTerminalText.text = terminalTextLog.text;
        }
        Debug.Log(log.ToString());
    }

    void ServerOnClientConnect(NetworkMessage msg)
    {
        System.Collections.ObjectModel.ReadOnlyCollection<NetworkConnection> connections = server.connections;
        PrintUI("Server: Client Connected. Total: " + connections.Count);

        //for(int i = 0; i < connections.Count; i++)
        //{
        //    if(connections[i] != null)
        //        PrintUI("Server: Client [" + connections[i].connectionId + "] = " + connections[i].address);
        //    else
        //        PrintUI("Server: Client [" + i + "] = null");
        //}
    }

    void ServerOnClientDisconnect(NetworkMessage msg)
    {
        System.Collections.ObjectModel.ReadOnlyCollection<NetworkConnection> connections = server.connections;
        PrintUI("Server: Client Disconnected. Total: " + connections.Count);

        //for (int i = 0; i < connections.Count; i++)
        //{
        //    if (connections[i] != null)
        //        PrintUI("Server: Client [" + connections[i].connectionId + "] = " + connections[i].address);
        //    else
        //        PrintUI("Server: Client [" + i + "] = null");
        //}
    }

    void ServerOnData(NetworkMessage msg)
    {
        // Sets reader to beginning
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();

        //string[] msgArray = msgString.Split(' ');
        //ParseMessage(msgArray);

        if (CAVE2.IsMaster())
        {
            CAVE2.SendMessage(gameObject.name, "CAVE2ClusterMsg", msgString);
        }
    }

    // Client Functions ---------------------------------------------------------------------------
    void ConnectToServer(bool local = false)
    {
        PrintUI("Client: Connecting to " + serverIP + ":" + serverListenPort);
        client.Connect(serverIP, serverListenPort);
        connecting = true;

        localConnection = local;
    }

    void ClientOnConnect(NetworkMessage msg)
    {
        PrintUI("Client: Connected to " + serverIP);
    }

    void ClientOnDisconnect(NetworkMessage msg)
    {
        PrintUI("Client: Disconnected");
        connecting = false;
    }

    void ClientOnData(NetworkMessage msg)
    {
        // Sets reader to beginning
        msg.reader.SeekZero();

        string msgString = msg.reader.ReadString();

        //string[] msgArray = msgString.Split(' ');
        //ParseMessage(msgArray);

        if (CAVE2.IsMaster() && !localConnection)
        {
            CAVE2.SendMessage(gameObject.name, "CAVE2ClusterMsg", msgString);
        }
    }

    void CAVE2ClusterMsg(string msgString)
    {
        // CAVE2 RPC Message Format
        // "functionName|targetGameObjectName|param1|param2|etc"
        char[] charSeparators = new char[] { '|' };
        string[] msgStrArray = msgString.Split(charSeparators, System.StringSplitOptions.RemoveEmptyEntries);
        ParseMessage(msgStrArray);
    }

    void ParseMessage(string[] msgArray)
    {
        string rootCommand = msgArray[0];

        if(rootCommand.Equals("translate", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if(targetObject != null)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float.TryParse(msgArray[2], out x);
                float.TryParse(msgArray[3], out y);
                float.TryParse(msgArray[4], out z);

                if(msgArray.Length == 6)
                {
                    if(msgArray[5].Equals("self", System.StringComparison.OrdinalIgnoreCase) )
                    {
                        targetObject.transform.Translate(x, y, z, Space.Self);
                    }
                    else if (msgArray[5].Equals("world", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObject.transform.Translate(x, y, z, Space.World);
                    }
                }
                else
                {
                    targetObject.transform.Translate(x, y, z, Space.Self);
                } 
            }
        }
        else if (rootCommand.Equals("setPosition", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float.TryParse(msgArray[2], out x);
                float.TryParse(msgArray[3], out y);
                float.TryParse(msgArray[4], out z);

                targetObject.transform.position = new Vector3(x, y, z);
            }
        }
        else if (rootCommand.Equals("setLocalPosition", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float.TryParse(msgArray[2], out x);
                float.TryParse(msgArray[3], out y);
                float.TryParse(msgArray[4], out z);

                targetObject.transform.localPosition = new Vector3(x, y, z);
            }
        }
        else if (rootCommand.Equals("setPositionRotation", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float rx = 0;
                float ry = 0;
                float rz = 0;

                float.TryParse(msgArray[2], out x);
                float.TryParse(msgArray[3], out y);
                float.TryParse(msgArray[4], out z);

                float.TryParse(msgArray[5], out rx);
                float.TryParse(msgArray[6], out ry);
                float.TryParse(msgArray[7], out rz);

                targetObject.transform.position = new Vector3(x, y, z);
                targetObject.transform.eulerAngles = new Vector3(rx, ry, rz);
            }
        }
        else if (rootCommand.Equals("setLocalPositionRotation", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float rx = 0;
                float ry = 0;
                float rz = 0;

                float.TryParse(msgArray[2], out x);
                float.TryParse(msgArray[3], out y);
                float.TryParse(msgArray[4], out z);

                float.TryParse(msgArray[5], out rx);
                float.TryParse(msgArray[6], out ry);
                float.TryParse(msgArray[7], out rz);

                targetObject.transform.localPosition = new Vector3(x, y, z);
                targetObject.transform.localEulerAngles = new Vector3(rx, ry, rz);
            }
        }
        else if (rootCommand.Equals("rotate", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float.TryParse(msgArray[2], out x);
                float.TryParse(msgArray[3], out y);
                float.TryParse(msgArray[4], out z);

                if (msgArray.Length == 6)
                {
                    if (msgArray[5].Equals("self", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObject.transform.Rotate(x, y, z, Space.Self);
                    }
                    else if (msgArray[5].Equals("world", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetObject.transform.Rotate(x, y, z, Space.World);
                    }
                }
                else
                {
                    targetObject.transform.Rotate(x, y, z, Space.Self);
                }
            }
        }
        else if (rootCommand.Equals("getPosition", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                SendCommand("message_to_remote " + targetObject.transform.position.x + " " + targetObject.transform.position.y + " " + targetObject.transform.position.z);
            }
        }
        else if (rootCommand.Equals("getLocalPosition", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                SendCommand("message_to_remote " + targetObject.transform.localPosition);
            }
        }
        else if (rootCommand.Equals("getRotation", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                SendCommand("message_to_remote " + targetObject.transform.rotation);
            }
        }
        else if (rootCommand.Equals("getLocalRotation", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                SendCommand("message_to_remote " + targetObject.transform.localRotation);
            }
        }
        else if (rootCommand.Equals("getEulerAngles", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                SendCommand("message_to_remote " + targetObject.transform.eulerAngles.x + " " + targetObject.transform.eulerAngles.y + " " + targetObject.transform.eulerAngles.z);
            }
        }
        else if (rootCommand.Equals("getLocalEulerAngles", System.StringComparison.OrdinalIgnoreCase))
        {
            string objectName = msgArray[1];
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject != null)
            {
                SendCommand("message_to_remote " + targetObject.transform.localEulerAngles.x + " " + targetObject.transform.localEulerAngles.y + " " + targetObject.transform.localEulerAngles.z);
            }
        }
        else if (rootCommand.Equals("message_to_remote", System.StringComparison.OrdinalIgnoreCase))
        {
            string message = "";
            for(int i = 1; i < msgArray.Length; i++)
            {
                if (i > 1)
                    message += " ";
                message += msgArray[i];
            }
            PrintUI(message);
        }
        else
        {
            string message = "";
            for (int i = 0; i < msgArray.Length; i++)
            {
                if (i > 0)
                    message += " ";
                message += msgArray[i];
            }
            PrintUI(message);
        }
    }

    public void SendCommandFromUI(string cmd)
    {
        SendCommand(cmd, true);
    }

    public void SendCommand(string cmd, bool useReliable = true)
    {
        //Debug.Log("Sending: '" + cmd + "'");
        if (isServer)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<NetworkConnection> connections = server.connections;

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i] != null)
                {
                    NetworkWriter writer = new NetworkWriter();
                    writer.StartMessage(TerminalMsgID);
                    writer.Write(FormatCmdStr(cmd));
                    writer.FinishMessage();

                    server.SendWriterTo(connections[i].connectionId, writer, useReliable ? reliableChannelId : unreliableChannelId);
                }
            }
        }
        else
        {
            if (client.connection != null)
            {
                NetworkWriter writer = new NetworkWriter();
                writer.StartMessage(TerminalMsgID);
                writer.Write(FormatCmdStr(cmd));
                writer.FinishMessage();

                if(client.SendWriter(writer, useReliable ? reliableChannelId : unreliableChannelId) == false)
                {
                    PrintUI("Failed to send message. Disconnected from server. Reconnecting...");
                    ConnectToServer();

                    // TODO: Create unsent message queue the resend on connection?
                }
            }
        }
    }

    string FormatCmdStr(string cmd)
    {
        string[] msgSplit = cmd.Split(' ');
        cmd = "";
        bool hasQuote = false;
        foreach (string s in msgSplit)
        {
            string workingS = s;

            // Handle object names with spaces (requires '' or "")
            char[] trimChars = { '\"', '\'' };
            if (!hasQuote && s.Contains("\"") || s.Contains("\'"))
            {
                workingS = s.TrimStart(trimChars);
                hasQuote = true;
            }
            else if (hasQuote && s.Contains("\"") || s.Contains("\'"))
            {
                workingS = s.TrimEnd(trimChars);
                hasQuote = false;
            }

            if (!hasQuote)
            {
                cmd += workingS + "|";
            }
            else
            {
                cmd += workingS + " ";
            }
        }
        return cmd;
    }

    // Command Line Terminal ----------------------------------------------------------------------
    public void UpdateTerminal()
    {
        if (commandLine == null)
            return;

        if(commandLine.isFocused && Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentCmdHistoryLine > 0)
                currentCmdHistoryLine--;
            commandLine.text = (string)cmdHistory[currentCmdHistoryLine];
        }
        else if(commandLine.isFocused && Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentCmdHistoryLine < cmdHistory.Count - 1)
                currentCmdHistoryLine++;
            commandLine.text = (string)cmdHistory[currentCmdHistoryLine];
        }
        
    }

    public void OnCommandLineEnter(string cmdString)
    {
        cmdHistory.Add(cmdString);
        currentCmdHistoryLine = cmdHistory.Count;

        if (cmdString.Length > 0)
            SendCommand(cmdString);

        if(commandLine != null)
        {
            commandLine.text = "";

            // Keeps input field in focus after pressing enter
            commandLine.ActivateInputField();
            commandLine.Select();
        }

        int maxHistoryCount = 500;
        if(cmdHistory.Count > maxHistoryCount)
        {
            cmdHistory.RemoveRange(0, maxHistoryCount / 2);
            currentCmdHistoryLine = maxHistoryCount / 2;
        }
    }
}
