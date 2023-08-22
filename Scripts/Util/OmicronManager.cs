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
 
/*
 * OmicronInputScript handles all network connentions to the input server and sends events out to objects tagged as 'OmicronListener'.
 * Currently supported input servers:
 * 		- Omicron oinputserver
 *		- OmicronInputConnector
 */
using UnityEngine;
using System.Collections;

using omicronConnector;
using omicron;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.IO;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
public class TouchPoint{

	Vector3 position;

    int ID;
    int rootID;
	EventBase.Type gesture;
	Ray touchRay = new Ray();
	RaycastHit touchHit;
	long timeStamp;
	GameObject objectTouched;
    GameObject visualObject;

    public PointerEventData pointerEvent;

    public TouchPoint(EventData e)
    {
        position = new Vector3(e.posx, e.posy, e.posz);
        ID = (int)e.sourceId;
        touchRay = Camera.main.ScreenPointToRay(position);
        gesture = (EventBase.Type)e.type;
        timeStamp = (long)Time.time;

        pointerEvent = new PointerEventData(EventSystem.current);
        pointerEvent.pointerId = ID;
    }

    public TouchPoint(Vector2 pos, int id){
		position = pos;
		ID = id;
		touchRay = Camera.main.ScreenPointToRay(position);
		gesture = EventBase.Type.Null;
		timeStamp = (long)Time.time;

        pointerEvent = new PointerEventData(EventSystem.current);
        pointerEvent.pointerId = ID;
    }
	
    public void Update(Vector2 newPosition, EventBase.Type gesture)
    {
        position = newPosition;
        pointerEvent.position = newPosition;

        // Raycast into the Unity Event system
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEvent, raycastResults);

        if (gesture == EventBase.Type.Down)
        {
            if (raycastResults.Count > 0)
            {
                objectTouched = raycastResults[raycastResults.Count - 1].gameObject;
                Debug.Log(objectTouched);
            }

            pointerEvent.Reset();
            pointerEvent.pointerId = ID;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.position = position;

            ExecuteEvents.ExecuteHierarchy(objectTouched, pointerEvent, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(objectTouched, pointerEvent, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.ExecuteHierarchy(objectTouched, pointerEvent, ExecuteEvents.pointerEnterHandler);

            ExecuteEvents.Execute(objectTouched, pointerEvent, ExecuteEvents.beginDragHandler);
            pointerEvent.pointerDrag = objectTouched;
        }
        else if (gesture == EventBase.Type.Move)
        {
            // If pointer has pressed on object, apply drag on all objects hit (initial hit or saved object may not be slider, so hit everything)
            // We prevent further unnecessary processing by only checking while there's an active object that was pressed
            if (pointerEvent.pointerDrag != null)
            { 
                foreach (RaycastResult result in raycastResults)
                {
                    ExecuteEvents.ExecuteHierarchy(result.gameObject, pointerEvent, ExecuteEvents.dragHandler);
                }
            }
        }
        else if (gesture == EventBase.Type.Up)
        {
            ExecuteEvents.ExecuteHierarchy(objectTouched, pointerEvent, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute(objectTouched, pointerEvent, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(objectTouched, pointerEvent, ExecuteEvents.pointerExitHandler);

            ExecuteEvents.Execute(objectTouched, pointerEvent, ExecuteEvents.endDragHandler);
            pointerEvent.pointerDrag = null;
        }
    }

	public Vector3 GetPosition(){
		return position;
	}
	
	public Ray GetRay(){
		return touchRay;
	}
	
	public int GetID(){
		return ID;
	}
	
    public int GetRootID()
    {
        return rootID;
    }

	public long GetTimeStamp(){
		return timeStamp;
	}
	
	public EventBase.Type GetGesture(){
		return gesture;
	}
	
	public RaycastHit GetRaycastHit(){
		 return touchHit;
	}
	
	public GameObject GetObjectTouched(){
		 return visualObject;
	}

    public GameObject GetGameObject()
    {
        return objectTouched;
    }

    public void SetGesture(EventBase.Type value){
		 gesture = value;
	}
	
	public void SetRaycastHit(RaycastHit value){
		 touchHit = value;
	}
	
	public void SetObjectTouched(GameObject value){
        visualObject = value;
	}

    public void SetGameObject(GameObject value)
    {
        objectTouched = value;
    }

    public void SetRootID(int value)
    {
        rootID = value;
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
class EventListener : IOmicronConnectorClientListener
{
	OmicronManager parent;
	
	public EventListener( OmicronManager p )
	{
		parent = p;
	}
	
	public override void onEvent(EventData e)
	{
        parent.AddEvent(e);
    }// onEvent
	
}// EventListener

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if USING_GETREAL3D
public class OmicronManager : getReal3D.MonoBehaviourWithRpc
#else
public class OmicronManager : MonoBehaviour
#endif
{
    static OmicronManager omicronManagerInstance;
    EventListener omicronListener;
    OmicronConnectorClient omicronManager;

    [SerializeField]
    public bool connectToServer = false;

    public enum ConnectionState { NotConnected, Connecting, Connected, FailedToConnect };

    [SerializeField]
    ConnectionState connectionState = ConnectionState.NotConnected;

    public string serverIP = "localhost";
    public int serverMsgPort = 28000;
    public int dataPort = 7013;

    [SerializeField]
    bool debug = false;

    [SerializeField]
    bool receivingDataFromMaster = false;

    // Use mouse clicks to emulate touches
    public bool mouseTouchEmulation = false;

    // List storing events since we have multiple threads
    private ArrayList eventList;

    private ArrayList omicronClients;

    [SerializeField]
    UnityEngine.UI.Text statusCanvasText = null;

    [SerializeField]
    SimpleCanvasTouch testTouchCanvas = null;

    int configLoaded = -1;

    [Header("Requested Service Types")]
    [SerializeField] bool enablePointer = true;
    [SerializeField] bool enableMocap = true;
    [SerializeField] bool enableKeyboard = true;
    [SerializeField] bool enableController = true;
    [SerializeField] bool enableUI = true;
    [SerializeField] bool enableGeneric = true;
    [SerializeField] bool enableBrain = true;
    [SerializeField] bool enableWand = true;
    [SerializeField] bool enableSpeech = true;
    [SerializeField] bool enableImage = false;
    [SerializeField] bool enableAudio = true;

    [Header("Coordinate Conversions")]
    public bool continuum3DXAxis;
    public bool continuumMainInvertX;

    enum ClientFlags
    {
        DataIn = 1 << 0,
        ServiceTypePointer = 1 << 1,
        ServiceTypeMocap = 1 << 2,
        ServiceTypeKeyboard = 1 << 3,
        ServiceTypeController = 1 << 4,
        ServiceTypeUi = 1 << 5,
        ServiceTypeGeneric = 1 << 6,
        ServiceTypeBrain = 1 << 7,
        ServiceTypeWand = 1 << 8,
        ServiceTypeSpeech = 1 << 9,
        ServiceTypeImage = 1 << 10,
        AlwaysTCP = 1 << 11,
        AlwaysUDP = 1 << 12,
        ServiceTypeAudio = 1 << 13
    }

    [Serializable]
    public class OmicronConfig
    {
        public bool connectToServer;
        public string serverIP;
        public int serverMsgPort;
        public int dataPort;
        public bool continuum3DXAxis;
        public bool continuumMainInvertX;
        public bool keyboardMouseWandEmulation;
    }

    OmicronConfig config;

    [Header("Network Analysis")]
    [SerializeField]
    float incomingUpdateLatency;

    float updateTimer;
    int updateEvents;

    [SerializeField]
    int filterByID = -1;

    [Header("UI")]
    [SerializeField]
    HeadNodeDebugManager mainConsoleUI = null;

    public static OmicronManager GetOmicronManager()
    {
        if (omicronManagerInstance != null)
        {
            return omicronManagerInstance;
        }
        else
        {
            //Debug.LogWarning(ERROR_MANAGERNOTFOUND);
            GameObject c2m = GameObject.Find("OmicronManager");
            if( c2m == null )
            {
                // Debug.LogWarning("OmicronManager not found looking for CAVE2Manager");
                c2m = GameObject.Find("CAVE2-Manager");
            }
            omicronManagerInstance = c2m.GetComponent<OmicronManager>();
            // Debug.LogWarning("Reintializing OmicronManager");
            omicronManagerInstance.Awake();
            return omicronManagerInstance;
        }
    }

    // Initializations
    public void Awake()
    {
        omicronManagerInstance = this;
        omicronListener = new EventListener(this);
        omicronManager = new OmicronConnectorClient(omicronListener);

        eventList = new ArrayList();
    }

    // Use this for initialization
    void Start()
    {
        if (connectToServer)
        {
            StartCoroutine("ConnectToServer");
        }

        omicronClients = new ArrayList();
        //DontDestroyOnLoad(gameObject);
    }// start

	public bool ConnectToServer()
	{
        int flags = 0; // Use server defaults

        flags += enablePointer ? (int)ClientFlags.ServiceTypePointer : 0;
        flags += enableMocap ? (int)ClientFlags.ServiceTypeMocap : 0;
        flags += enableKeyboard ? (int)ClientFlags.ServiceTypeKeyboard : 0;
        flags += enableController ? (int)ClientFlags.ServiceTypeController : 0;
        flags += enableUI ? (int)ClientFlags.ServiceTypeUi : 0;
        flags += enableGeneric ? (int)ClientFlags.ServiceTypeGeneric : 0;
        flags += enableBrain ? (int)ClientFlags.ServiceTypeBrain : 0;
        flags += enableWand ? (int)ClientFlags.ServiceTypeWand : 0;
        flags += enableSpeech ? (int)ClientFlags.ServiceTypeSpeech : 0;
        flags += enableImage ? (int)ClientFlags.ServiceTypeImage : 0;
        flags += enableAudio ? (int)ClientFlags.ServiceTypeAudio : 0;

#if !UNITY_WEBGL
        omicronManager.Connect( serverIP, serverMsgPort, dataPort, flags);
#endif
        return connectToServer;
	}

    public bool IsConnectedToServer()
    {
        return connectionState == ConnectionState.Connected;
    }

    public ConnectionState GetConnectionState()
    {
        return connectionState;
    }

    public bool IsReceivingDataFromMaster()
    {
        return receivingDataFromMaster;
    }

    public void DisconnectServer()
	{
		omicronManager.Dispose ();
		connectToServer = false;
        connectionState = ConnectionState.NotConnected;
        Debug.Log("InputService: Disconnected");
	}

	public void AddClient( OmicronEventClient c )
	{
        if (debug)
        {
            Debug.Log("OmicronManager: OmicronEventClient " + c.name + " added of type " + c.GetClientType());
        }
        if (omicronClients != null)
        {
            omicronClients.Add(c);
        }
        else
        {
            // First run case since client may attempt to connect before
            // OmicronManager Start() is called
            omicronClients = new ArrayList();
            omicronClients.Add(c);
        }
	}

	public void AddEvent( EventData e )
	{
        if (filterByID == -1)
        {
            updateEvents++;
        }

        lock (eventList.SyncRoot)
        {
            if (filterByID != -1)
            {
                updateEvents++;
            }

            eventList.Add(e);
            if (debug)
            {
                Debug.Log("OmicronManager: Received New event ID: " + e.sourceId + " of type " + e.serviceType);
                //if( e.serviceType == EventBase.ServiceType.ServiceTypeWand)
                //    Debug.Log("OmicronManager: Received New event ID: " + e.sourceId + " of type " + e.serviceType);
            }
        };
    }

#if USING_GETREAL3D
	[getReal3D.RPC]
	public void AddStringEvent( string evtString )
	{
		EventData e = OmicronConnectorClient.StringToEventData(evtString);
		if(!getReal3D.Cluster.isMaster)
		{
            AddEvent(e);
		}
	}
#endif

    public void Update()
    {
        updateTimer += Time.deltaTime;
        if(updateTimer > 5 && updateEvents > 0)
        {
            incomingUpdateLatency = updateTimer / updateEvents;
            updateTimer = 0;
            updateEvents = 0;
        }

        connectionState = (ConnectionState)omicronManager.GetConnectionState();

        if (mouseTouchEmulation && (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
        {
            Vector2 position = new Vector3(Input.mousePosition.x / Screen.width, 1 - (Input.mousePosition.y / Screen.height));

            // Ray extending from main camera into screen from touch point
            Ray touchRay = Camera.main.ScreenPointToRay(position);
            Debug.DrawRay(touchRay.origin, touchRay.direction * 10, Color.white);

            TouchPoint touch = new TouchPoint(position, -1);

            if (Input.GetMouseButtonDown(0))
                touch.SetGesture(EventBase.Type.Down);
            else if (Input.GetMouseButtonUp(0))
                touch.SetGesture(EventBase.Type.Up);
            else if (Input.GetMouseButton(0))
                touch.SetGesture(EventBase.Type.Move);

            testTouchCanvas.OnEvent(touch);
        }

        StartCoroutine("SendEventsToClients");

        // CAVE2.SendMessage(gameObject.name, "UpdateDebugTextRPC", connectStatus );
        if (statusCanvasText)
        {
            statusCanvasText.text = "OmicronManager\n";
            statusCanvasText.text += "Server: " + serverIP + ":" + serverMsgPort +"\n";

            string statusText = "UNKNOWN";
            switch (connectionState)
            {
                case (ConnectionState.NotConnected): statusText = "Not Connected"; break;
                case (ConnectionState.Connecting): statusText = "Connecting"; break;
                case (ConnectionState.Connected): statusText = "Connected"; break;
                case (ConnectionState.FailedToConnect): statusText = "Failed To Connect"; break;
            }

            statusCanvasText.text += "Status: " + statusText + "\n";
        }

        if(configLoaded == 0)
        {
            if (ConfigurationManager.loadedConfig != null)
            {
                TrackerConfig trackerConfig = ConfigurationManager.loadedConfig.trackerConfig;
                if(trackerConfig.trackerType.ToLower() == "omicron")
                {
                    serverIP = trackerConfig.serverIP;
                    serverMsgPort = trackerConfig.serverMsgPort;
                    dataPort = trackerConfig.dataPort;
                    continuum3DXAxis = trackerConfig.continuum3DCoordinateConversion;
                    continuumMainInvertX = trackerConfig.continuumMainCoordinateConversion;

                    if(trackerConfig.connectToServer)
                    {
                        StartCoroutine("ConnectToServer");
                    }
                    
                    if(mainConsoleUI)
                    {
                        mainConsoleUI.UpdateTrackingUI();
                    }

                    CAVE2Manager c2Manager = GetComponent<CAVE2Manager>();
                    if(c2Manager != null)
                    {
                        if (trackerConfig.trackingSimulatorMode)
                        {
                            c2Manager.simulatorMode = true;
                            c2Manager.mocapEmulation = true;
                            c2Manager.wandMousePointerEmulation = true;
                            c2Manager.keyboardEventEmulation = true;
                        }
                        else
                        {
                            c2Manager.simulatorMode = false;
                            c2Manager.mocapEmulation = false;
                            c2Manager.wandMousePointerEmulation = false;
                            c2Manager.keyboardEventEmulation = false;
                        }
                    }
                }
                configLoaded = 1;
            }
        }
    }

    void UpdateDebugTextRPC(int connectStatus)
    {
        if (statusCanvasText != null)
        {
            string statusText = "UNKNOWN";
            switch (connectStatus)
            {
                case (0): currentStatus = idleStatus; statusText = "Not Connected"; statusCanvasText.color = Color.grey; break;
                case (1): currentStatus = activeStatus; statusText = "Connected"; statusCanvasText.color = Color.green; break;
                case (-1): currentStatus = errorStatus; statusText = "Failed to Connect"; statusCanvasText.color = Color.red; break;
                default: statusCanvasText.color = Color.yellow; break;
            }

            statusCanvasText.text = statusText;
        }

        receivingDataFromMaster = connectStatus == 1;
    }
      	
    IEnumerator SendEventsToClients()
    {
        lock (eventList.SyncRoot)
        {
            foreach (EventData e in eventList)
            {
                // Send Omicron events to display client
                // Note we're doing this before coordinate conversion since display clients will do that calculation
                if (CAVE2.IsMaster())
                {
                    //getReal3D.RpcManager.call("AddStringEvent", OmicronConnectorClient.EventDataToString(e));
                    // Commented out because risk of double events - use CAVE2.SendMessage to sync actions instead
                }


                // -zPos -xRot -yRot for Omicron->Unity coordinate conversion
                if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap || e.serviceType == EventBase.ServiceType.ServiceTypeWand)
                {
                    e.posz = -e.posz;
                    e.orx = -e.orx;
                    e.ory = -e.ory;
                }

                // If Continuum tracking system is calibrated as main wall display out as +Z and 3D wall out as +X
                // I.e. 3D wall faces +X, opposed to standard CAVE2 configuration where this is +Z
                if (continuum3DXAxis)
                {
                    float cave2X = e.posx;
                    float cave2Z = e.posz;

                    e.posz = cave2X;
                    e.posx = -cave2Z;

                    Vector3 euler = new Quaternion(e.orx, e.ory, e.orz, e.orw).eulerAngles;
                    euler.y += 90;

                    Quaternion newQ = Quaternion.Euler(euler);
                    e.orx = -newQ.x;
                    e.ory = newQ.y;
                    e.orz = -newQ.z;
                    e.orw = newQ.w;
                }

                // If Continuum tracking system is calibrated as main wall display out as +Z and 3D wall out as +X
                // CAVE2 (Vicon uses +x to right (when facing main wall), Continuum uses -x to right)
                if (continuumMainInvertX)
                {
                    float cave2X = e.posx;
                    float cave2Z = e.posz;

                    e.posx = -cave2X + 0.2f;
                    e.posz = -cave2Z;
                }

                foreach (OmicronEventClient c in omicronClients)
                {
                    if (c == null)
                        continue;

                    EventBase.ServiceType eType = e.serviceType;
                    EventBase.ServiceType clientType = c.GetClientType();


                    if (!enablePointer && eType == EventBase.ServiceType.ServiceTypePointer)
                    {
                        continue;
                    }
                    else if (!enableMocap && eType == EventBase.ServiceType.ServiceTypeMocap)
                    {
                        continue;
                    }
                    else if (!enableKeyboard && eType == EventBase.ServiceType.ServiceTypeKeyboard)
                    {
                        continue;
                    }
                    else if (!enableController && eType == EventBase.ServiceType.ServiceTypeController)
                    {
                        continue;
                    }
                    else if (!enableUI && eType == EventBase.ServiceType.ServiceTypeUi)
                    {
                        continue;
                    }
                    else if (!enableGeneric && eType == EventBase.ServiceType.ServiceTypeGeneric)
                    {
                        continue;
                    }
                    else if (!enableBrain && eType == EventBase.ServiceType.ServiceTypeBrain)
                    {
                        continue;
                    }
                    else if (!enableWand && eType == EventBase.ServiceType.ServiceTypeWand)
                    {
                        continue;
                    }
                    else if (!enableSpeech && eType == EventBase.ServiceType.ServiceTypeSpeech)
                    {
                        continue;
                    }
                    else if (!enableImage && eType == EventBase.ServiceType.ServiceTypeImage)
                    {
                        continue;
                    }
                    else if (!enableAudio && eType == EventBase.ServiceType.ServiceTypeAudio)
                    {
                        continue;
                    }

                    if (!c.IsFlaggedForRemoval() && (clientType == EventBase.ServiceType.ServiceTypeAny || eType == clientType))
                    {
                        //c.BroadcastMessage("OnEvent", e, SendMessageOptions.DontRequireReceiver);
                        //CAVE2.SendMessage(c.gameObject.name, "OnEvent", e);
                        c.OnEvent(e);
                    }
                }
            }

            // Clear the list (TODO: probably should set the Processed flag instead and cleanup elsewhere)
            eventList.Clear();
        }
        yield return null;
    }

    public bool WandEventsEnabled()
    {
        return enableWand;
    }

	void OnApplicationQuit()
    {
        if (connectToServer)
        {
            DisconnectServer();
        }
    }

	void OnDestroy() {
		if( connectToServer ){
			DisconnectServer();
		}
	}

    void ConfigurationLoaded(DefaultConfig config)
    {
        configLoaded = 0;
    }

    // GUI (Legacy)
    GUIStyle idleStatus = new GUIStyle();
	GUIStyle activeStatus = new GUIStyle();
	GUIStyle errorStatus = new GUIStyle();
	GUIStyle currentStatus;
	Vector2 GUIOffset;
	
	public void SetGUIOffSet( Vector2 offset )
	{
		GUIOffset = offset;
    }

	public void OnWindow(int windowID)
	{
		float rowHeight = 18;

		idleStatus.normal.textColor = Color.white;
		activeStatus.normal.textColor = Color.green;
		errorStatus.normal.textColor = Color.red;

        currentStatus = idleStatus;
        
        string statusText = "UNKNOWN";
        switch (connectionState)
        {
            case(ConnectionState.NotConnected): currentStatus = idleStatus; statusText = "Not Connected"; break;
            case(ConnectionState.Connected): currentStatus = activeStatus; statusText = "Connected"; break;
            case(ConnectionState.FailedToConnect): currentStatus = errorStatus; statusText = "Failed to Connect"; break;
        }

		if( GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 0, 250, 20), connectToServer, "Connect to Server:") )
		{
			if( currentStatus != activeStatus )
				ConnectToServer();
		}
		else
		{
			if( currentStatus == activeStatus )
				DisconnectServer();
        }

        GUI.Label (new Rect (GUIOffset.x + 150, GUIOffset.y + rowHeight * 0 + 3, 250, 200), statusText, currentStatus);

		GUI.Label(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 1, 120, 20), "Omicron Server IP:");
		serverIP = GUI.TextField(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 1, 200, 20), serverIP, 25);

		GUI.Label(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 2, 120, 20), "Server Message Port:");
		serverMsgPort = int.Parse(GUI.TextField(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 2, 200, 20), serverMsgPort.ToString(), 25));

		GUI.Label(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 3, 120, 20), "Data Port:");
		dataPort = int.Parse(GUI.TextField(new Rect(GUIOffset.x + 150, GUIOffset.y + rowHeight * 3, 200, 20), dataPort.ToString(), 25));

        GUI.Label(new Rect(GUIOffset.x + 20, GUIOffset.y + rowHeight * 5, 120, 20), "Tracking Options:");
        continuum3DXAxis = GUI.Toggle(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 6, 250, 20), continuum3DXAxis, "Flip Z and X axis for Continuum 3D Wall");
        continuumMainInvertX = GUI.Toggle(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 7, 250, 20), continuumMainInvertX, "Invert X axis for Continuum Main Wall");
    }
}// class