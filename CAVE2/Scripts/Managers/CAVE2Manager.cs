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
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static OmicronManager;
using System.IO;

public class CAVE2 : MonoBehaviour
{
    public static string HEAD_NODE_NAME = "CAVE2MASTER";
    public static string HEAD_NODE_NAME_ALT = "ORION-WIN";
    public static string DISPLAY_NODE_NAME = "ORION";

    static float CAVE2_RADIUS = 3.240f;
    static float CAVE2_FLOOR_TO_BOTTOM_DISPLAY = 0.293f;
    static float CAVE2_DISPLAY_W_BORDER_HEIGHT = 0.581f;

    public enum Axis
    {
        None, LeftAnalogStickLR, LeftAnalogStickUD, RightAnalogStickLR, RightAnalogStickUD, AnalogTriggerL, AnalogTriggerR,
        LeftAnalogStickLR_Inverted, LeftAnalogStickUD_Inverted, RightAnalogStickLR_Inverted, RightAnalogStickUD_Inverted, AnalogTriggerL_Inverted, AnalogTriggerR_Inverted
    };
    public enum Button { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, SpecialButton1, SpecialButton2, SpecialButton3, ButtonUp, ButtonDown, ButtonLeft, ButtonRight, None };
    public enum InteractionType { Any, Pointing, Touching }

    public static CAVE2InputManager Input;
    public static CAVE2RPCManager RpcManager;

    public struct WandEvent
    {
        public CAVE2PlayerIdentity playerID;
        public int wandID;
        public Button button;
        public InteractionType interactionType;

        public WandEvent(CAVE2PlayerIdentity id, int wand, Button b, InteractionType t)
        {
            playerID = id;
            wandID = wand;
            button = b;
            interactionType = t;
        }
    };

    // CAVE2 Tracking Management -------------------------------------------------------------------
    public static Vector3 GetHeadPosition(int ID)
    {
        return CAVE2Manager.GetHeadPosition(ID);
    }

    public static Quaternion GetHeadRotation(int ID)
    {
        return CAVE2Manager.GetHeadRotation(ID);
    }

    public static Vector3 GetWandPosition(int ID)
    {
        return CAVE2Manager.GetWandPosition(ID);
    }

    public static Quaternion GetWandRotation(int ID)
    {
        return CAVE2Manager.GetWandRotation(ID);
    }

    public static Vector3 GetMocapPosition(int ID)
    {
        return CAVE2Manager.GetMocapPosition(ID);
    }

    public static Quaternion GetMocapRotation(int ID)
    {
        return CAVE2Manager.GetMocapRotation(ID);
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Input Management ----------------------------------------------------------------------
    public static bool UsingOmicronServer()
    {
        return CAVE2Manager.UsingOmicronServer();
    }

    public static float GetAxis(CAVE2.Axis axis, int wandID = 1)
    {
        return CAVE2Manager.GetAxis(wandID, axis);
    }

    public static bool GetButton(CAVE2.Button button, int wandID = 1)
    {
        return CAVE2Manager.GetButton(wandID, button);
    }

    public static bool GetButtonDown(CAVE2.Button button, int wandID = 1)
    {
        return CAVE2Manager.GetButtonDown(wandID, button);
    }

    public static bool GetButtonUp(CAVE2.Button button, int wandID = 1)
    {
        return CAVE2Manager.GetButtonUp(wandID, button);
    }

    public static OmicronController.ButtonState GetButtonState(CAVE2.Button button, int wandID = 1)
    {
        return CAVE2Manager.GetButtonState(wandID, button);
    }


    //
    [System.Obsolete("GetAxis(int, CAVE2.Axis) is deprecated, please use GetAxis(CAVE2.Axis, int) instead.")]
    public static float GetAxis(int wandID, CAVE2.Axis axis)
    {
        return CAVE2Manager.GetAxis(wandID, axis);
    }

    [System.Obsolete("GetButton(int, CAVE2.Button) is deprecated, please use GetButton(CAVE2.Button, int) instead.")]
    public static bool GetButton(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButton(wandID, button);
    }

    [System.Obsolete("GetButtonDown(int, CAVE2.Button) is deprecated, please use GetButtonDown(CAVE2.Button, int) instead.")]
    public static bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonDown(wandID, button);
    }

    [System.Obsolete("GetButtonUp(int, CAVE2.Button) is deprecated, please use GetButtonUp(CAVE2.Button, int) instead.")]
    public static bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonUp(wandID, button);
    }

    [System.Obsolete("GetButtonState(int, CAVE2.Button) is deprecated, please use GetButtonState(CAVE2.Button, int) instead.")]
    public static OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonState(wandID, button);
    }


    public static CAVE2.Button GetReal3DToCAVE2Button(string name)
    {
        return CAVE2Manager.GetReal3DToCAVE2Button(name);
    }

    public static CAVE2.Axis GetReal3DToCAVE2Axis(string name)
    {
        return CAVE2Manager.GetReal3DToCAVE2Axis(name);
    }

    public static string CAVE2ToGetReal3DButton(CAVE2.Button name)
    {
        return CAVE2Manager.CAVE2ToGetReal3DButton(name);
    }

    public static string CAVE2ToGetReal3DAxis(CAVE2.Axis name)
    {
        return CAVE2Manager.CAVE2ToGetReal3DAxis(name);
    }

    // ---------------------------------------------------------------------------------------------


    // CAVE2 Cluster Management --------------------------------------------------------------------
    public static CAVE2Manager GetCAVE2Manager()
    {
        return CAVE2Manager.GetCAVE2Manager();
    }

    public static bool IsMaster()
    {
        return CAVE2Manager.IsMaster();
    }

    public static bool OnCAVE2Display()
    {
        return CAVE2Manager.OnCAVE2Display();
    }

    public static bool UsingGetReal3D()
    {
        return CAVE2Manager.UsingGetReal3D();
    }

    public static bool UsingHMDVR()
    {
        return CAVE2Manager.UsingHMDVR();
    }

    public static void SetHMDVREnabled(bool value)
    {
        CAVE2Manager.SetHMDVREnabled(value);
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Simulator Management ------------------------------------------------------------------
    public static bool IsSimulatorMode()
    {
        return CAVE2Manager.IsSimulatorMode();
    }

    public static void RegisterHeadObject(int headID, GameObject gameobject)
    {
        CAVE2Manager.GetCAVE2Manager().RegisterHeadObject(headID, gameobject);
    }

    public static void RegisterWandObject(int wandID, GameObject gameobject)
    {
        CAVE2Manager.GetCAVE2Manager().RegisterWandObject(wandID, gameobject);
    }

    public static bool IsHeadRegistered(int headID, GameObject gameobject)
    {
        return CAVE2Manager.GetCAVE2Manager().IsHeadRegistered(headID, gameobject);
    }

    public static bool IsWandRegistered(int wandID, GameObject gameobject)
    {
        return CAVE2Manager.GetCAVE2Manager().IsWandRegistered(wandID, gameobject);
    }
    // ---------------------------------------------------------------------------------------------

    // CAVE2 Player Management ---------------------------------------------------------------------
    public static GameObject GetHeadObject(int ID)
    {
        return CAVE2Manager.GetCAVE2Manager().GetHeadObject(ID);
    }

    public static GameObject GetWandObject(int ID)
    {
        return CAVE2Manager.GetCAVE2Manager().GetWandObject(ID);
    }

    public static void ShowDebugCanvas(bool value)
    {
        CAVE2Manager.GetCAVE2Manager().showDebugCanvas = value;
    }
    // ---------------------------------------------------------------------------------------------

    public static void AddCameraController(CAVE2CameraController cam)
    {
        CAVE2Manager.AddCameraController(cam);
    }
    public static CAVE2CameraController GetCameraController()
    {
        return GetCAVE2Manager().mainCameraController;
    }

    public static void AddPlayerController(int id, GameObject g)
    {
        GetCAVE2Manager().AddPlayerController(id, g);
    }

    public static GameObject GetPlayerController(int id)
    {
        return GetCAVE2Manager().GetPlayerController(id);
    }

    public static int GetPlayerControllerCount()
    {
        return GetCAVE2Manager().GetPlayerControllerCount();
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Synchronization Management ------------------------------------------------------------
    public static void BroadcastMessage(string targetObjectName, string methodName, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().BroadcastMessage(targetObjectName, methodName, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.BroadcastMessage(methodName, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void BroadcastMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().BroadcastMessage(targetObjectName, methodName, param, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().BroadcastMessage(targetObjectName, methodName, param, param2, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.BroadcastMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, param2, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, new object[] { param, param2 }, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, param2, param3, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, new object[] { param, param2, param3 }, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, param2, param3, param4, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4 }, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, param2, param3, param4, param5, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5}, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7 }, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, new object[] { param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 }, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void SendMessage(string targetObjectName, string methodName, object[] paramArr, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().SendMessage(targetObjectName, methodName, paramArr, msgType);
        }
        else
        {
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObject != null)
            {
                //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
                targetObject.SendMessage(methodName, paramArr, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public static void Destroy(string targetObjectName)
    {
        if (GetCAVE2Manager())
        {
            GetCAVE2Manager().Destroy(targetObjectName);
        }
        else
        {
            Destroy(targetObjectName);
        }
    }

    public static void LoadScene(int id)
    {
        if (RpcManager && GetCAVE2Manager())
        {
            RpcManager.SendMessage(GetCAVE2Manager().name, "CAVE2LoadScene", id);
        }
        else
        {
            LoadScene(id);
        }
    }

    public static void LoadScene(string id)
    {
        if (RpcManager && GetCAVE2Manager())
        {
            RpcManager.SendMessage(GetCAVE2Manager().name, "CAVE2LoadScene", id);
        }
        else
        {
            LoadScene(id);
        }
    }

    public static void LoadSceneAsync(int id)
    {
        if (RpcManager && GetCAVE2Manager())
        {
            RpcManager.SendMessage(GetCAVE2Manager().name, "CAVE2LoadSceneAsync", id);
        }
        else
        {
            LoadSceneAsync(id);
        }
    }

    public static void LoadSceneAsync(string id)
    {
        if (RpcManager && GetCAVE2Manager())
        {
            RpcManager.SendMessage(GetCAVE2Manager().name, "CAVE2LoadSceneAsync", id);
        }
        else
        {
            LoadSceneAsync(id);
        }
    }

    public static void PrintArray(object[] array)
    {
        for(int i = 0; i < array.Length; i++)
        {
            Debug.Log("[" + i + "]: '" + array[i] + "'");
        }
    }

    public static bool IsPointingAtCAVE2Screens(Vector3 position, Quaternion rotation, out Vector3 intersectPoint)
    {
        return IsPointingAtCAVE2Screens(position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, out intersectPoint);
    }

    public static bool IsPointingAtCAVE2Screens(float x, float y, float z, float rx, float ry, float rz, float rw, out Vector3 intersectPoint)
    {
        Vector3 eulerAngles = Vector3.zero;

        // Quaternion to Euler ////////////////////////
        // Rotation matrix Q multiplied by reference vector (0,0,1)
        // 		| 1 - 2y^2 - 2z^2 , 2xy - 2zw, 2xz + 2yw	|		|0	|
        // Q =	| 2xy + 2zw, 1 - 2x^2 - 2z^2, 2yz - 2xw		| * 	|0	|
        // 		| 2xz - 2yw, 2yz + 2xw, 1 - 2x^2 - 2y^2		|		|1  |
        eulerAngles.x = 1 * (2 * rx * rz + 2 * ry * rw);
        eulerAngles.y = 1 * (2 * ry * rz - 2 * rx * rw);
        eulerAngles.z = 1 * (1 - 2 * (rx * rx) - 2 * (ry * ry));

        if (rx * ry + rz * rw == 0.5)
        {
            // North pole
            eulerAngles.x = 2 * Mathf.Atan2(rx, rw);
            eulerAngles.z = 0;
        }
        else if (rx * ry + rz * rw == -0.5)
        {
            // South pole
            eulerAngles.x = -2 * Mathf.Atan2(rx, rw);
            eulerAngles.z = 0;
        }
        // QuaternionToEuler ends ///////////////////

        float h = 0; // x-coordinate of the center of the circle
        float k = 0; // z-coordinate of the center of the circle
        float ox = eulerAngles.x; // parametric slope of x, from orientation vector
        float oy = eulerAngles.y; // parametric slope of y, from orientation vector
        float oz = eulerAngles.z; // parametric slope of z, from orientation vector
        float r = CAVE2_RADIUS; // radius of cylinder

        // A * t^2 + B * t + C
        float A = ox * ox + oz * oz;
        float B = 2 * ox * x + 2 * oz * z - 2 * h * ox - 2 * k * oz;
        float C = x * x + z * z + h * h + k * k - r * r - 2 * h * x - 2 * k * z;

        float t1 = (-B + Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);
        float t2 = (-B - Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);
        float t = 0;
        if (t1 >= 0)
        {
            t = t1;
        }
        else if (t2 >= 0)
        {
            t = t2;
        }

        var x_pos = ox * t + x;
        var y_pos = oy * t + y;
        var z_pos = oz * t + z;
        intersectPoint = new Vector3(x_pos, y_pos, z_pos);

        // Check if over CAVE2 entrance
        float angle = Mathf.Atan2(x_pos, -z_pos);
        float radiansForDoor = 36 * Mathf.PI / 180;
        float max_x_error = 0.005f; // fractional

        if (angle < 0)
        {
            angle += 2 * Mathf.PI;
        }
        angle = 2 * Mathf.PI - angle;
        angle -= radiansForDoor / 2;
        x = angle / (2 * Mathf.PI - radiansForDoor);
        x += 0.02777777777f;
        if (x > 1)
        {
            if (x >= 1 + max_x_error)
            {
                return false;
            }
        }
        if (x < 0)
        {
            if (x <- -max_x_error)
            {
                return false;
            }
        }

        if (intersectPoint.y >= CAVE2_FLOOR_TO_BOTTOM_DISPLAY && intersectPoint.y <= CAVE2_FLOOR_TO_BOTTOM_DISPLAY + CAVE2_DISPLAY_W_BORDER_HEIGHT * 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    // ---------------------------------------------------------------------------------------------

    // CAVE2 XR Helpers
    public static Vector3 GetXRNodePosition(UnityEngine.XR.XRNode type)
    {
        List<UnityEngine.XR.XRNodeState> nodeStates = new List<UnityEngine.XR.XRNodeState>();
        UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);

        foreach (UnityEngine.XR.XRNodeState nodeState in nodeStates)
        {
            if (nodeState.nodeType == type)
            {
                Vector3 value;
                if (nodeState.TryGetPosition(out value))
                {
                    return value;
                }
            }
        }
        return Vector3.zero;
    }

    public static Quaternion GetXRNodeRotation(UnityEngine.XR.XRNode type)
    {
        List<UnityEngine.XR.XRNodeState> nodeStates = new List<UnityEngine.XR.XRNodeState>();
        UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);

        foreach (UnityEngine.XR.XRNodeState nodeState in nodeStates)
        {
            if (nodeState.nodeType == type)
            {
                Quaternion value;
                if (nodeState.TryGetRotation(out value))
                {
                    return value;
                }
            }
        }
        return Quaternion.identity;
    }

    // ---------------------------------------------------------------------------------------------
}

[RequireComponent(typeof(CAVE2AdvancedTrackingSimulator))]
[RequireComponent(typeof(CAVE2InputManager))]
[RequireComponent(typeof(CAVE2RPCManager))]

public class CAVE2Manager : MonoBehaviour {

static CAVE2Manager CAVE2Manager_Instance;
    CAVE2InputManager inputManager;

    static string machineName;

    public int head1MocapID = 0;
    public int wand1MocapID = 1;
    public int wand1ControllerID = 1;

    public int head2MocapID = 2;
    public int wand2MocapID = 3;
    public int wand2ControllerID = 2;

    ArrayList cameraControllers;
    public CAVE2CameraController mainCameraController;
    public Hashtable headObjects = new Hashtable();
    public Hashtable wandObjects = new Hashtable();

    public Hashtable playerControllers = new Hashtable();

    // Simulator
    public bool simulatorMode;
    public bool mocapEmulation;
    public bool keyboardEventEmulation;
    public bool wandMousePointerEmulation;
    public bool usingKinectTrackingSimulator;
    
    public Vector3 simulatorHeadPosition = new Vector3(0.0f, 1.6f, 0.0f);
    public Vector3 simulatorHeadRotation = new Vector3(0.0f, 0.0f, 0.0f);

    public Vector3 simulatorWandPosition = new Vector3(0.16f, 1.43f, 0.4f);
    public Vector3 simulatorWandRotation = new Vector3(0.0f, 0.0f, 0.0f);

    public Vector3 simulatorWandPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 simulatorWandRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);

    public enum TrackerEmulated { CAVE, Head, Wand };
    public enum TrackerEmulationMode { Pointer, Translate, Rotate, TranslateForward, TranslateVertical, RotatePitchYaw, RotateRoll };
    // string[] trackerEmuStrings = { "CAVE", "Head", "Wand1" };
    // string[] trackerEmuModeStrings = { "Pointer", "Translate", "Rotate" };

    // TrackerEmulationMode defaultWandEmulationMode = TrackerEmulationMode.Pointer;
    // TrackerEmulationMode toggleWandEmulationMode = TrackerEmulationMode.TranslateVertical;
    public TrackerEmulationMode wandEmulationMode = TrackerEmulationMode.Pointer;

    // bool wandModeToggled = false;
    Vector3 mouseLastPos;
    // Vector3 mouseDeltaPos;

    public bool showDebugCanvas;
    public GameObject debugCanvas;

    
    CAVE2RPCManager rpcManager;

    [Header("Networking")]
    [SerializeField]
    public bool sendTrackingData;

    [SerializeField]
    public bool simulateAsClient;

    public static bool hmdVREnabled; // HMD mode (non-CAVE2 simulator)

    [Header("Performance")]
    [SerializeField]
    public bool useLowQualityOnMaster;

    [SerializeField]
    int lowQualitySettingIndex = 0;

    [SerializeField]
    int highQualitySettingIndex = -1;

    public void Init()
    {
        CAVE2Manager_Instance = this;
        inputManager = GetComponent<CAVE2InputManager>();
        
        CAVE2.Input = inputManager;
        CAVE2.RpcManager = GetComponent<CAVE2RPCManager>();

        CAVE2.Input.Init();

        cameraControllers = new ArrayList();

        machineName = GetMachineName();
        Debug.Log(this.GetType().Name + ">\t initialized on " + machineName);

        UnityEngine.Random.InitState(1138);
    }

    void Start()
    {
        Init();

        if ( OnCAVE2Display() || OnCAVE2Master() )
        {
#if UNITY_EDITOR
#else
            simulatorMode = false;
            mocapEmulation = false;
            keyboardEventEmulation = false;
            wandMousePointerEmulation = false;
            usingKinectTrackingSimulator = false;
#endif
            if (OnCAVE2Display())
            {
                OmicronManager omg = GetComponent<OmicronManager>();
                omg.connectToServer = false;

                if(highQualitySettingIndex != -1)
                {
                    QualitySettings.SetQualityLevel(highQualitySettingIndex);
                }
            }
            else if(useLowQualityOnMaster)
            {
                QualitySettings.SetQualityLevel(lowQualitySettingIndex);
            }

            if(OnCAVE2Master())
            {
                // Enable mouse cursor or head node
                Cursor.visible = true;
            }
        }
    }

    void ConfigurationLoaded(DefaultConfig config)
    {
        ClusterConfig cConfig = ConfigurationManager.loadedConfig.clusterConfig;
        if (cConfig.headNodeName.Length > 0)
        {
            CAVE2.HEAD_NODE_NAME = cConfig.headNodeName;
            Debug.Log("Config: Using '" + CAVE2.HEAD_NODE_NAME + "' as head node machine name");
        }
        if (cConfig.displayNodeName.Length > 0)
        {
            CAVE2.DISPLAY_NODE_NAME = cConfig.displayNodeName;
            Debug.Log("Config: Using '" + CAVE2.DISPLAY_NODE_NAME + "' as display node machine name");
        }
    }

    void CAVE2ConfigurationLoaded(DefaultConfig config)
    {
        ClusterConfig cConfig = ConfigurationManager.loadedConfig.clusterConfig;
        if (cConfig.headNodeName.Length > 0)
        {
            CAVE2.HEAD_NODE_NAME = cConfig.headNodeName;
            Debug.Log("Config: Using '" + CAVE2.HEAD_NODE_NAME + "' as head node machine name");
        }
        if (cConfig.displayNodeName.Length > 0)
        {
            CAVE2.DISPLAY_NODE_NAME = cConfig.displayNodeName;
            Debug.Log("Config: Using '" + CAVE2.DISPLAY_NODE_NAME + "' as display node machine name");
        }
    }

    void Update()
    {
        if(CAVE2Manager_Instance == null)
        {
            CAVE2Manager_Instance = this;
        }

        if( !UsingGetReal3D() && !UnityEngine.XR.XRSettings.enabled && (mocapEmulation || usingKinectTrackingSimulator) )
        {
            if (mainCameraController)
            {
                mainCameraController.GetMainCamera().transform.localPosition = simulatorHeadPosition;
                mainCameraController.GetMainCamera().transform.localEulerAngles = simulatorHeadRotation;
            }
        }

        if( debugCanvas )
        {
            debugCanvas.SetActive(showDebugCanvas);
        }
    }

    // GUI
    Vector2 GUIOffset;
    bool keyboardMouseWand;

    public void SetKeyboardMouseWand(bool value)
    {
        keyboardMouseWand = value;
    }

    public bool GetKeyboardMouseWand()
    {
        return keyboardMouseWand;
    }

    public void SetGUIOffSet(Vector2 offset)
    {
        GUIOffset = offset;
    }

    public void OnWindow(int windowID)
    {
        float rowHeight = 25;

        keyboardMouseWand = GUI.Toggle(new Rect(GUIOffset.x + 20, GUIOffset.y + rowHeight * 0, 250, 20), keyboardMouseWand, "Simulator Wand");

        mocapEmulation = keyboardMouseWand;
        keyboardEventEmulation = keyboardMouseWand;
        wandMousePointerEmulation = keyboardMouseWand;
    }

    // CAVE2 Tracking Management -------------------------------------------------------------------
    public static Vector3 GetHeadPosition(int ID)
    {
        return CAVE2.Input.GetHeadPosition(ID);
    }

    public static Quaternion GetHeadRotation(int ID)
    {
        return CAVE2.Input.GetHeadRotation(ID);
    }

    public static Vector3 GetWandPosition(int ID)
    {
        return CAVE2.Input.GetWandPosition(ID);
    }

    public static Quaternion GetWandRotation(int ID)
    {
        return CAVE2.Input.GetWandRotation(ID);
    }

    public static float GetWandTimeSinceUpdate(int ID)
    {
        return CAVE2.Input.GetWandTimeSinceUpdate(ID);
    }

    public static Vector3 GetMocapPosition(int ID)
    {
        return CAVE2.Input.GetMocapPosition(ID);
    }

    public static Quaternion GetMocapRotation(int ID)
    {
        return CAVE2.Input.GetMocapRotation(ID);
    }

    public static CAVE2CameraController GetCameraController()
    {
        return CAVE2.GetCAVE2Manager().mainCameraController;
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Input Management ----------------------------------------------------------------------
    public static bool UsingOmicronServer()
    {
        OmicronManager omicronManager = GetCAVE2Manager().GetComponent<OmicronManager>();
        if (omicronManager)
            return omicronManager.IsConnectedToServer() || omicronManager.IsReceivingDataFromMaster();
        else
            return false;
    }

    public static float GetAxis(int wandID, CAVE2.Axis axis)
    {
        return CAVE2.Input.GetAxis(wandID, axis);
    }

    public static bool GetButton(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButton(wandID, button);
    }

    public static bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButtonDown(wandID, button);
    }

    public static bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButtonUp(wandID, button);
    }

    public static OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButtonState(wandID, button);
    }

    /*
          <map_button index = "2" name="WandButton"/> <!-- B / Circle --> 
          <map_button index = "3" name="ChangeWand"/> <!-- X / Square --> 
          <map_button index = "6" name="Reset"/> <!-- A / Cross -->
          <map_button index = "4" name="Jump"/> <!-- Y / Triangle -->
          <map_button index = "5" name="WandLook" /> <!-- RB / R1-->
          <map_button index = "1" name="NavSpeed" /> <!-- LB / L1-->
          <map_button index = "7" name="WandDrive" /> <!-- LT / L2 --> 
          <map_button index = "8" name="RT" /> <!-- RT / R2 --> 
          <map_button index = "9" name="L3" /> <!-- L3(Left analog button)-->
          <map_button index = "10" name="R3" /> <!-- R3(Right analog button)-->
          <map_button index = "11" name="Back" /> <!-- Back / Select-->
          <map_button index = "12" name="Start" /> <!-- Start -->
          
          <map_valuator dead_zone = ".1" index="1" name="Yaw"/>
          <map_valuator dead_zone = ".1" index="2" name="Forward"/>
          <map_valuator dead_zone = ".1" index="3" name="Strafe" />
          <map_valuator dead_zone = ".1" index="4" name="Pitch" />
          <map_valuator dead_zone = ".1" index="5" name="DPadLR" />
          <map_valuator dead_zone = ".1" index="6" name="DPadUD" />

          <map_tracker index = "1" name="Head" />
          <map_tracker index = "3" name="Wand" />
*/
    public static CAVE2.Button GetReal3DToCAVE2Button(string name)
    {
        switch(name)
        {
            case "WandButton": return CAVE2.Button.Button3;
            case "ChangeWand": return CAVE2.Button.Button2;
            case "Reset": return CAVE2.Button.Button8;
            case "Jump": return CAVE2.Button.Button1;
            case "WandLook": return CAVE2.Button.Button5;
            case "NavSpeed": return CAVE2.Button.Button4;
            case "WandDrive": return CAVE2.Button.Button7;
            case "RT": return CAVE2.Button.SpecialButton3;
            case "L3": return CAVE2.Button.Button6;
            case "R3": return CAVE2.Button.Button9;
            case "Back": return CAVE2.Button.SpecialButton1;
            case "Start": return CAVE2.Button.SpecialButton2;
        }
        return CAVE2.Button.None;
    }

    public static CAVE2.Axis GetReal3DToCAVE2Axis(string name)
    {
        switch (name)
        {
            case "Yaw": return CAVE2.Axis.LeftAnalogStickLR;
            case "Forward": return CAVE2.Axis.LeftAnalogStickUD;
            case "Strafe": return CAVE2.Axis.RightAnalogStickLR;
            case "Pitch": return CAVE2.Axis.RightAnalogStickUD;
        }
        return CAVE2.Axis.None;
    }

    public static string CAVE2ToGetReal3DButton(CAVE2.Button name)
    {
        switch (name)
        {
            case CAVE2.Button.Button8: return "WandButton";
            case CAVE2.Button.Button3: return "ChangeWand";
            case CAVE2.Button.Button2: return "Reset";
            case CAVE2.Button.Button1: return "Jump";
            case CAVE2.Button.Button5: return "WandLook";
            case CAVE2.Button.Button4: return "NavSpeed";
            case CAVE2.Button.Button7: return "WandDrive";
            case CAVE2.Button.SpecialButton3: return "RT";
            case CAVE2.Button.Button6: return "L3";
            case CAVE2.Button.Button9: return "R3";
            case CAVE2.Button.SpecialButton1: return "Back";
            case CAVE2.Button.SpecialButton2: return "Start";

            case CAVE2.Button.ButtonUp: return "DPadUD";
            case CAVE2.Button.ButtonDown: return "DPadUD";
            case CAVE2.Button.ButtonLeft: return "DPadLR";
            case CAVE2.Button.ButtonRight: return "DPadLR";
        }
        return "";
    }

    public static string CAVE2ToGetReal3DAxis(CAVE2.Axis name)
    {
        switch (name)
        {
            case CAVE2.Axis.LeftAnalogStickLR: return "Yaw";
            case CAVE2.Axis.LeftAnalogStickUD: return "Forward";
            case CAVE2.Axis.RightAnalogStickLR: return "Strafe";
            case CAVE2.Axis.RightAnalogStickUD: return "Pitch";
        }
        return "";
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Cluster Management --------------------------------------------------------------------
    public static CAVE2Manager GetCAVE2Manager()
    {
        if(CAVE2Manager_Instance == null)
        {
            GameObject cave2Manager = GameObject.Find("CAVE2-Manager");
            if (cave2Manager)
            {
                CAVE2Manager_Instance = cave2Manager.GetComponent<CAVE2Manager>();

                if (CAVE2Manager_Instance == null)
                {
                    Debug.LogWarning("CAVE2Manager_Instance is NULL - SHOULD NOT HAPPEN!");
                }
                else
                {
                    Debug.LogWarning("Reintializing CAVE2Manager_Instance");
                    CAVE2Manager_Instance.Init();
                }
            }
            //GameObject cave2Manager = new GameObject("CAVE2-Manager");
            //cave2Manager.AddComponent<OmicronManager>();
            //CAVE2Manager_Instance = cave2Manager.AddComponent<CAVE2Manager>();
            //cave2Manager.AddComponent<CAVE2InputManager>();
            //cave2Manager.AddComponent<CAVE2AdvancedTrackingSimulator>();
            //cave2Manager.AddComponent<CAVE2RPCManager>();
        }
        return CAVE2Manager_Instance;
    }

    public static bool IsMaster()
    {
        if (CAVE2Manager_Instance != null && CAVE2Manager_Instance.simulateAsClient)
            return false;
#if USING_GETREAL3D
		return getReal3D.Cluster.isMaster;
#else
        machineName = GetMachineName();
        if (machineName.Contains(CAVE2.DISPLAY_NODE_NAME) && !machineName.Equals(CAVE2.HEAD_NODE_NAME))
            return false;
        else // Assumes master or development machine
            return true;
#endif
    }

    public static string GetMachineName()
    {
#if !UNITY_WSA
        return System.Environment.MachineName;
#else
        return "";
#endif
    }

    public static bool OnCAVE2Display()
    {
        if (CAVE2Manager_Instance.simulateAsClient)
            return true;

        machineName = GetMachineName();
        if (machineName.Contains(CAVE2.DISPLAY_NODE_NAME) && !IsMaster())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool OnCAVE2Master()
    {
        if (CAVE2Manager_Instance.simulateAsClient)
            return true;

        machineName = GetMachineName();
        if (machineName.Contains(CAVE2.HEAD_NODE_NAME) || machineName.Contains(CAVE2.HEAD_NODE_NAME_ALT))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool UsingGetReal3D()
    {
#if USING_GETREAL3D
		return true;
#else
        return false;
#endif
    }

    public static bool UsingHMDVR()
    {
        return hmdVREnabled || UnityEngine.XR.XRSettings.enabled;
    }

    public static void SetHMDVREnabled(bool value)
    {
        hmdVREnabled = value;
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Simulator Management ------------------------------------------------------------------
    public static bool IsSimulatorMode()
    {
        return GetCAVE2Manager().simulatorMode;
    }

    // public Vector3 GetMouseDeltaPos()
    // {
    //     return mouseDeltaPos;
    // }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Player Management ---------------------------------------------------------------------
    public static void AddCameraController(CAVE2CameraController cam)
    {
        if (GetCAVE2Manager().cameraControllers == null)
        {
            GetCAVE2Manager().cameraControllers = new ArrayList();
        }

        if(GetCAVE2Manager().cameraControllers.Count == 0)
            GetCAVE2Manager().mainCameraController = cam;

        GetCAVE2Manager().cameraControllers.Add(cam);
    }

    public static CAVE2CameraController GetCameraController(int cam)
    {
        if (cam >= 0 && cam < GetCAVE2Manager().cameraControllers.Count - 1)
            return (CAVE2CameraController)GetCAVE2Manager().cameraControllers[cam];
        else
            return null;
    }

    public void RegisterHeadObject(int ID, GameObject gameObject)
    {
        headObjects[ID] = gameObject;
    }

    public void RegisterWandObject(int ID, GameObject gameObject)
    {
        wandObjects[ID] = gameObject;
    }

    public bool IsHeadRegistered(int ID, GameObject gameObject)
    {
        if ((GameObject)headObjects[ID] == gameObject)
            return true;
        return false;
    }

    public bool IsWandRegistered(int ID, GameObject gameObject)
    {
        if ((GameObject)wandObjects[ID] == gameObject)
            return true;
        return false;
    }

    public GameObject GetHeadObject(int ID)
    {
        return (GameObject)headObjects[ID];
    }

    public GameObject GetWandObject(int ID)
    {
        return (GameObject)wandObjects[ID];
    }

    public void AddPlayerController(int id, GameObject g)
    {
        playerControllers[id] = g;
    }

    public GameObject GetPlayerController(int id)
    {
        GameObject player = (GameObject)playerControllers[id];
        return player;
    }

    public int GetPlayerControllerCount()
    {
        return playerControllers.Count;
    }

    // ---------------------------------------------------------------------------------------------


    // CAVE2 Synchronization Management ------------------------------------------------------------
    public void BroadcastMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.BroadcastMessage(targetObjectName, methodName, param, msgType);
    }

    public void BroadcastMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.BroadcastMessage(targetObjectName, methodName, param, param2, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, param2, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, param2, param3, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, param2, param3, param4, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, param2, param3, param4, param5, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object param, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10, object param11, object param12, object param13, object param14, object param15, object param16, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, param, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, object[] paramArr, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, paramArr, msgType);
    }

    public void SendMessage(string targetObjectName, string methodName, CAVE2RPCManager.MsgType msgType = CAVE2RPCManager.MsgType.Reliable)
    {
        CAVE2.RpcManager.SendMessage(targetObjectName, methodName, 0, msgType);
    }

    public void Destroy(string targetObjectName)
    {
        CAVE2.RpcManager.Destroy(targetObjectName);
    }

    public void CAVE2LoadScene(int id)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(id);
        Debug.Log(this.GetType().Name + ">\t CAVE2 Load Scene: " + id);
    }

    public void CAVE2LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        Debug.Log(this.GetType().Name + ">\t CAVE2 Load Scene: " + sceneName);
    }

    public void CAVE2LoadSceneAsync(int id)
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(id);
        Debug.Log(this.GetType().Name + ">\t CAVE2 Load Scene (Async): " + id);
    }

    public void CAVE2LoadSceneAsync(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        Debug.Log(this.GetType().Name + ">\t CAVE2 Load Scene (Async): " + sceneName);
    }
}
