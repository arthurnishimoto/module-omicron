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
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;

[ExecuteInEditMode]
public class CAVE2InputManager : OmicronEventClient
{
    Hashtable mocapSensors = new Hashtable();
    Hashtable wandControllers = new Hashtable();

    // [SerializeField]
    // float axisSensitivity = 1f;

    [Header("Simulator Input Mapping")]
    public int wandIDMappedToSimulator = 1;

    // CAVE2 Simulator to Wand button bindings
    public string wandSimulatorAnalogUD = "Vertical";
    public string wandSimulatorAnalogLR = "Horizontal";
    public string wandSimulatorButton3 = "Fire1"; // PS3 Navigation Cross
    public string wandSimulatorButton2 = "Fire2"; // PS3 Navigation Circle
    public KeyCode wandSimulatorDPadUp = KeyCode.UpArrow;
    public KeyCode wandSimulatorDPadDown = KeyCode.DownArrow;
    public KeyCode wandSimulatorDPadLeft = KeyCode.LeftArrow;
    public KeyCode wandSimulatorDPadRight = KeyCode.RightArrow;
    public string wandSimulatorButton5 = "space"; // PS3 Navigation L1
    public string wandSimulatorButton6 = "Fire3"; // PS3 Navigation L3
    public string wandSimulatorButton7 = "space"; // PS3 Navigation L2

    string simulatorWand2AnalogUD = "Vertical2";
    string simulatorWand2AnalogLR = "Horizontal2";

    string simulatorWand1Trigger = "Trigger L";
    string simulatorWand1Grip = "Grip R";

    string simulatorWand2Trigger = "Trigger R";
    string simulatorWand2Grip = "Grip R";

    public KeyCode simulatorFlyUp = KeyCode.R;
    public KeyCode simulatorFlyDown = KeyCode.F;

#if USING_GETREAL3D
    [SerializeField]
    bool disableGetReal3DControllerInput = false;
#endif

    [Header("Simulator Mocap Mapping")]
    public KeyCode simulatorHeadRotateL = KeyCode.Q;
    public KeyCode simulatorHeadRotateR = KeyCode.E;


    [SerializeField]
    float axisDeadzone = 0.2f;

    public Vector3[] wandTrackingOffset = new Vector3[]{
        new Vector3(-0.007781088f, -0.04959464f, -0.07368752f) // Wand 1
    };

    // Prevent other inputs from wand if it has opened a menu
    bool wand1MenuLock = false;
    bool wand2MenuLock = false;

    Hashtable unityInputToOmicronInput = new Hashtable();

    public enum InputMappingMode { CAVE2, Vive, Oculus };

    [SerializeField]
    public InputMappingMode inputMappingMode = InputMappingMode.CAVE2;

    [SerializeField]
    bool debug = false;

    public enum VRModel { None, Vive, OculusRiftCV1 };

    [SerializeField]
    VRModel vrModel = VRModel.None;

    public static string GetXRDeviceName()
    {
        List<UnityEngine.XR.InputDevice> xrDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(xrDevices);
        if (xrDevices.Count > 0)
        {
            return xrDevices[0].name;
        }
        return "";
    }

    public void Init()
    {
        base.Start();

        unityInputToOmicronInput[wandSimulatorAnalogUD] = CAVE2.Axis.LeftAnalogStickUD;
        unityInputToOmicronInput[wandSimulatorAnalogLR] = CAVE2.Axis.LeftAnalogStickLR;
        unityInputToOmicronInput[wandSimulatorButton3] = CAVE2.Button.Button3;
        unityInputToOmicronInput[wandSimulatorButton2] = CAVE2.Button.Button2;
        unityInputToOmicronInput[wandSimulatorButton6] = CAVE2.Button.Button6;


        string xrDeviceName = GetXRDeviceName();
        if (xrDeviceName == "Vive MV" || xrDeviceName == "Vive. MV")
        {
            vrModel = VRModel.Vive;

            inputMappingMode = InputMappingMode.Vive;
        }
        else if (xrDeviceName == "Oculus Rift CV1")
        {
            vrModel = VRModel.OculusRiftCV1;

            simulatorWand2AnalogUD = "Oculus_CrossPlatform_SecondaryThumbstickVertical";
            simulatorWand2AnalogLR = "Oculus_CrossPlatform_SecondaryThumbstickHorizontal";

            simulatorWand1Trigger = "Oculus_CrossPlatform_PrimaryIndexTrigger";
            simulatorWand1Grip = "Oculus_CrossPlatform_PrimaryHandTrigger";

            simulatorWand2Trigger = "Oculus_CrossPlatform_SecondaryIndexTrigger";
            simulatorWand2Grip = "Oculus_CrossPlatform_SecondaryHandTrigger";

            inputMappingMode = InputMappingMode.Oculus;
        }
        else if(xrDeviceName.Length > 0)
        {
            Debug.Log("CAVE2InputManager: Detected VRDevice '" + xrDeviceName + "'.");
        }

        OmicronMocapSensor[] mocapSensors = GetComponents<OmicronMocapSensor>();
        foreach (OmicronMocapSensor ms in mocapSensors)
        {
            Destroy(ms);
            //mocapSensors[ms.sourceID] = ms;
            Debug.LogWarning("CAVE2InputManager: Found existing mocap sensor id '" + ms.sourceID + "'.");
        }
        OmicronController[] controllers = GetComponents<OmicronController>();
        foreach (OmicronController c in controllers)
        {
            Destroy(c);
            //wandControllers[c.sourceID] = c;
            Debug.LogWarning("CAVE2InputManager: Found existing controllers id '" + c.sourceID + "'.");
        }

        omicronManager = CAVE2.GetCAVE2Manager().GetComponent<OmicronManager>();
    }
    public bool IsWandMenuLocked(int wandID)
    {
        if (wandID == 1)
            return wand1MenuLock;
        else if (wandID == 2)
            return wand2MenuLock;
        else
            return false;
    }

    public void SetWandMenuLock(int wandID, bool value)
    {
        if (wandID == 1)
            wand1MenuLock = value;
        else if (wandID == 2)
            wand2MenuLock = value;
    }

    public OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        if(wandControllers != null && wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button);
        }
        return OmicronController.ButtonState.Idle;
    }

    public bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        if (wandControllers != null && wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button) == OmicronController.ButtonState.Down;
        }
        return false;
    }

    public bool GetButton(int wandID, CAVE2.Button button)
    {
        if (wandControllers != null && wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button) == OmicronController.ButtonState.Held;
        }
        return false;
    }

    public bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        if (wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button) == OmicronController.ButtonState.Up;
        }
        return false;
    }

    public bool GetButtonDown(int wandID, string button)
    {
        return GetButtonDown(wandID, (CAVE2.Button)unityInputToOmicronInput[button]);
    }

    public bool GetButton(int wandID, string button)
    {
        return GetButton(wandID, (CAVE2.Button)unityInputToOmicronInput[button]);
    }

    public bool GetButtonUp(int wandID, string button)
    {
        return GetButtonUp(wandID, (CAVE2.Button)unityInputToOmicronInput[button]);
    }

    public int GetButtonFlags(int wandID)
    {
        if (wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.rawFlags;
        }
        return 0;
    }

    public float GetAxis(int wandID, CAVE2.Axis axis)
    {
        if (wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
			float axisValue = wandController.GetAxis (axis);
			if ( Mathf.Abs(axisValue) <= axisDeadzone)
				axisValue = 0;
			return axisValue;
        }
        return 0;
    }

    public float GetAxis(string axis, int wandID = 1)
    {
        return GetAxis(wandID, (CAVE2.Axis)unityInputToOmicronInput[axis]);
    }

    public Vector3 GetMocapPosition(int ID)
    {
        if (mocapSensors.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapSensors[ID];
            return mocap.GetPosition();
        }
        return Vector3.zero;
    }

    public Quaternion GetMocapRotation(int ID)
    {
        if (mocapSensors.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapSensors[ID];
            return mocap.GetOrientation();
        }
        return Quaternion.identity;
    }

    public float GetMocapTimeSinceUpdate(int ID)
    {
        if (mocapSensors.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapSensors[ID];
            return mocap.GetTimeSinceLastUpdate();
        }
        return float.PositiveInfinity;
    }

    public Vector3 GetHeadPosition(int ID)
    {
        int headID = 1;
        switch(ID)
        {
            case (1): headID = CAVE2.GetCAVE2Manager().head1MocapID; break;
            case (2): headID = CAVE2.GetCAVE2Manager().head2MocapID; break;
        }
        return GetMocapPosition(headID);
    }

    public Quaternion GetHeadRotation(int ID)
    {
        int headID = 1;
        switch (ID)
        {
            case (1): headID = CAVE2.GetCAVE2Manager().head1MocapID; break;
            case (2): headID = CAVE2.GetCAVE2Manager().head2MocapID; break;
        }
        return GetMocapRotation(headID);
    }

    public Vector3 GetWandPosition(int ID)
    {
        int wandID = 1;
        switch (ID)
        {
            case (1): wandID = CAVE2.GetCAVE2Manager().wand1MocapID; break;
            case (2): wandID = CAVE2.GetCAVE2Manager().wand2MocapID; break;
        }
        return GetMocapPosition(wandID);
    }

    public Quaternion GetWandRotation(int ID)
    {
        int wandID = 1;
        switch (ID)
        {
            case (1): wandID = CAVE2.GetCAVE2Manager().wand1MocapID; break;
            case (2): wandID = CAVE2.GetCAVE2Manager().wand2MocapID; break;
        }
        return GetMocapRotation(wandID);
    }

    public float GetWandTimeSinceUpdate(int ID)
    {
        int wandID = 1;
        switch (ID)
        {
            case (1): wandID = CAVE2.GetCAVE2Manager().wand1MocapID; break;
            case (2): wandID = CAVE2.GetCAVE2Manager().wand2MocapID; break;
        }
        return GetMocapTimeSinceUpdate(wandID);
    }


    public string[] GetSensorList()
    {
        string[] sensorList = new string[mocapSensors.Count];

        int i = 0;
        foreach (DictionaryEntry s in mocapSensors)
        {
            sensorList[i] = ((OmicronMocapSensor)s.Value).gameObject.name;
            i++;
        }

        return sensorList;
    }

    public void UpdateOmicronSensor(string s)
    {
        string[] nameStr = s.Split(' ');
        int id = -1;
        if (int.TryParse(nameStr[1], out id))
        {
            if (!mocapSensors.ContainsKey(id))
            {
                GameObject g = new GameObject("OmicronMocapSensor " + id);
                g.transform.parent = transform;
                OmicronMocapSensor newSensor = g.AddComponent<OmicronMocapSensor>();
                newSensor.sourceID = id;

                mocapSensors.Add(id, newSensor);
            }
        }
    }

    public void UpdateOmicronController(string s)
    {
        string[] nameStr = s.Split(' ');
        int id = -1;
        if (int.TryParse(nameStr[1], out id))
        {
            if (!wandControllers.ContainsKey(id))
            {
                GameObject g = new GameObject("OmicronController " + id);
                g.transform.parent = transform;
                OmicronController newSensor = g.AddComponent<OmicronController>();
                newSensor.sourceID = id;

                wandControllers.Add(id, newSensor);
            }
        }
    }

    /*
    public void UpdateOmicronSensorList(object[] param)
    {
        string[] sensors = (string[])param[0];
        string[] controllers = (string[])param[1];

        foreach(string s in sensors)
        {
            string[] nameStr = s.Split(' ');
            int id = -1;
            if (int.TryParse(nameStr[1], out id))
            {
                if (!mocapSensors.ContainsKey(id))
                {
                    GameObject g = new GameObject("OmicronMocapSensor " + id);
                    g.transform.parent = transform;
                    OmicronMocapSensor newSensor = g.AddComponent<OmicronMocapSensor>();
                    newSensor.sourceID = id;

                    mocapSensors.Add(id, newSensor);
                }
            }
        }
        foreach (string s in controllers)
        {
            string[] nameStr = s.Split(' ');
            int id = -1;
            if (int.TryParse(nameStr[1], out id))
            {
                if (!wandControllers.ContainsKey(id))
                {
                    GameObject g = new GameObject("OmicronController " + id);
                    g.transform.parent = transform;
                    OmicronController newSensor = g.AddComponent<OmicronController>();
                    newSensor.sourceID = id;

                    wandControllers.Add(id, newSensor);
                }
            }
        }
    }
    */

    public string[] GetWandControllerList()
    {
        string[] list = new string[wandControllers.Count];

        int i = 0;
        foreach (DictionaryEntry s in wandControllers)
        {
            list[i] = ((OmicronController)s.Value).gameObject.name;
            i++;
        }

        return list;
    }

    // Parses Omicron Input Data
    public override void OnEvent(EventData e)
    {
        //Debug.Log("CAVE2Manager_Legacy: '"+name+"' received " + e.serviceType);
        if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap)
        {
            if (!mocapSensors.ContainsKey((int)e.sourceId))
            {
                GameObject g = new GameObject("OmicronMocapSensor " + (int)e.sourceId);
                g.transform.parent = transform;
                OmicronMocapSensor mocapManager = g.AddComponent<OmicronMocapSensor>();
                mocapManager.sourceID = (int)e.sourceId;
                if (CAVE2.GetCAVE2Manager().usingKinectTrackingSimulator)
                {
                    mocapManager.SetPositionMod(new Vector3(1, 1, -1));
                }
                mocapSensors[(int)e.sourceId] = mocapManager;
            }
        }
        else if (e.serviceType == EventBase.ServiceType.ServiceTypeWand)
        {
            if ( !wandControllers.ContainsKey((int)e.sourceId) )
            {
                GameObject g = new GameObject("OmicronController " + (int)e.sourceId);
                g.transform.parent = transform;
                OmicronController wandController = g.AddComponent<OmicronController>();
                wandController.sourceID = (int)e.sourceId;
                wandControllers[(int)e.sourceId] = wandController;
            }
        }
    }

    void FixedUpdate()
    {
        int headMocapID = CAVE2.GetCAVE2Manager().head1MocapID;
        int wandMocapID = CAVE2.GetCAVE2Manager().wand1MocapID;
        int wand2MocapID = CAVE2.GetCAVE2Manager().wand2MocapID;
        int wandID = CAVE2.GetCAVE2Manager().wand1ControllerID;
        int wand2ID = CAVE2.GetCAVE2Manager().wand2ControllerID;

        // Get/Create Primary Head
        OmicronMocapSensor mainHeadSensor;
        if (!mocapSensors.ContainsKey(headMocapID))
        {
            GameObject g = new GameObject("OmicronMocapSensor " + headMocapID);
            g.transform.parent = transform;
            mainHeadSensor = g.AddComponent<OmicronMocapSensor>();
            mainHeadSensor.sourceID = headMocapID;
            mocapSensors[headMocapID] = mainHeadSensor;
        }
        else
        {
            mainHeadSensor = (OmicronMocapSensor)mocapSensors[headMocapID];
        }

        // Get/Create Primary Wand
        OmicronMocapSensor wandMocapSensor;
        if (!mocapSensors.ContainsKey(wandMocapID))
        {
            GameObject g = new GameObject("OmicronMocapSensor " + wandMocapID);
            g.transform.parent = transform;
            wandMocapSensor = g.AddComponent<OmicronMocapSensor>();
            wandMocapSensor.sourceID = wandMocapID;
            mocapSensors[wandMocapID] = wandMocapSensor;
        }
        else
        {
            wandMocapSensor = (OmicronMocapSensor)mocapSensors[wandMocapID];
        }

        // Get Primary Wand (ID 1)
        OmicronController wandController;
        if (!wandControllers.ContainsKey(wandID))
        {
            GameObject g = new GameObject("OmicronController " + wandID);
            g.transform.parent = transform;
            wandController = g.AddComponent<OmicronController>();
            wandController.sourceID = wandID;
            wandController.SetAsWand();
            wandControllers[wandID] = wandController;
        }
        else
        {
            wandController = (OmicronController)wandControllers[wandID];
        }

        // Get/Create Secondary Wand
        OmicronMocapSensor wand2MocapSensor;
        if (!mocapSensors.ContainsKey(wand2MocapID))
        {
            GameObject g = new GameObject("OmicronMocapSensor " + wand2MocapID);
            g.transform.parent = transform;
            wand2MocapSensor = g.AddComponent<OmicronMocapSensor>();
            wand2MocapSensor.sourceID = wand2MocapID;
            mocapSensors[wand2MocapID] = wand2MocapSensor;
        }
        else
        {
            wand2MocapSensor = (OmicronMocapSensor)mocapSensors[wand2MocapID];
        }

        // Get Secondary Wand (ID 1)
        OmicronController wandController2;
        if (!wandControllers.ContainsKey(wand2ID))
        {
            GameObject g = new GameObject("OmicronController " + wand2ID);
            g.transform.parent = transform;
            wandController2 = g.AddComponent<OmicronController>();
            wandController2.sourceID = wand2ID;
            wandController2.SetAsWand();
            wandControllers[wand2ID] = wandController2;
        }
        else
        {
            wandController2 = (OmicronController)wandControllers[wand2ID];
        }
        
        Vector2 wand1_analog1 = Vector2.zero;
        Vector2 wand1_analog2 = Vector2.zero;
        Vector2 wand1_analog3 = Vector2.zero;
        int wand1_flags = 0;

        Vector2 wand2_analog1 = Vector2.zero;
        Vector2 wand2_analog2 = Vector2.zero;
        Vector2 wand2_analog3 = Vector2.zero;
        int wand2_flags = 0;
        
        // Mocap
        if (CAVE2.GetCAVE2Manager().mocapEmulation)
        {
            if (UnityEngine.XR.XRSettings.enabled)
            {
                string xrDeviceName = GetXRDeviceName();
                if (xrDeviceName == "Vive MV")
                {
                    CAVE2.GetCAVE2Manager().simulatorHeadPosition = CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.Head);
                    CAVE2.GetCAVE2Manager().simulatorHeadRotation = CAVE2.GetXRNodeRotation(UnityEngine.XR.XRNode.Head).eulerAngles;
#if UNITY_5_5_OR_NEWER
                    CAVE2.GetCAVE2Manager().simulatorWandPosition = CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.LeftHand);
                    CAVE2.GetCAVE2Manager().simulatorWandRotation = CAVE2.GetXRNodeRotation(UnityEngine.XR.XRNode.LeftHand).eulerAngles;

                    wand2MocapSensor.UpdateTransform(CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.RightHand), CAVE2.GetXRNodeRotation(UnityEngine.XR.XRNode.RightHand));
#endif
                }
                else if(xrDeviceName.Length > 0)
                {
                    // Hack: InputTracking isn't using some offset that the Main Camera is otherwise getting. Calculate the diff here:
                    Vector3 oculusRealHeadPosition = Camera.main.transform.localPosition;
                    Vector3 positionOffset = oculusRealHeadPosition - CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.Head);

                    CAVE2.GetCAVE2Manager().simulatorHeadPosition = CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.Head) + positionOffset;
                    CAVE2.GetCAVE2Manager().simulatorHeadRotation = CAVE2.GetXRNodeRotation(UnityEngine.XR.XRNode.Head).eulerAngles;
#if UNITY_5_5_OR_NEWER
                    CAVE2.GetCAVE2Manager().simulatorWandPosition = CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.LeftHand) + positionOffset;
                    CAVE2.GetCAVE2Manager().simulatorWandRotation = CAVE2.GetXRNodeRotation(UnityEngine.XR.XRNode.LeftHand).eulerAngles;

                    wand2MocapSensor.UpdateTransform(CAVE2.GetXRNodePosition(UnityEngine.XR.XRNode.RightHand) + positionOffset, CAVE2.GetXRNodeRotation(UnityEngine.XR.XRNode.RightHand));
#endif
                }
            }

            mainHeadSensor.UpdateTransform(CAVE2.GetCAVE2Manager().simulatorHeadPosition, Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorHeadRotation));

            if (wandIDMappedToSimulator == 1)
            {
                wandMocapSensor.UpdateTransform(CAVE2.GetCAVE2Manager().simulatorWandPosition + CAVE2.GetCAVE2Manager().simulatorWandPositionOffset, Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation + CAVE2.GetCAVE2Manager().simulatorWandRotationOffset));
            }
            else
            {
                wand2MocapSensor.UpdateTransform(CAVE2.GetCAVE2Manager().simulatorWandPosition + CAVE2.GetCAVE2Manager().simulatorWandPositionOffset, Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation + CAVE2.GetCAVE2Manager().simulatorWandRotationOffset));
            }
        }
        else if( CAVE2.GetCAVE2Manager().usingKinectTrackingSimulator )
        {
            CAVE2.GetCAVE2Manager().simulatorHeadPosition = GetHeadPosition(1);
            CAVE2.GetCAVE2Manager().simulatorWandPosition = GetWandPosition(1);

            mainHeadSensor.UpdateTransform(CAVE2.GetCAVE2Manager().simulatorHeadPosition, Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorHeadRotation + CAVE2.GetCAVE2Manager().simulatorWandRotationOffset));

            wandMocapSensor.UpdateTransform(CAVE2.GetCAVE2Manager().simulatorWandPosition + CAVE2.GetCAVE2Manager().simulatorWandPositionOffset, Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation + CAVE2.GetCAVE2Manager().simulatorWandRotationOffset));
        }

        // Wand Buttons
        if (CAVE2.GetCAVE2Manager().keyboardEventEmulation)
        {
            float wand1_analogUD = Input.GetAxis(wandSimulatorAnalogUD);
            float wand1_analogLR = Input.GetAxis(wandSimulatorAnalogLR);
            //bool turn = CAVE2.GetCAVE2Manager().simulatorWandStrafeAsTurn;

            wand1_analog1 = new Vector2(wand1_analogLR, wand1_analogUD);

            if (Input.GetKey(wandSimulatorDPadUp))
                wand1_flags += (int)EventBase.Flags.ButtonUp;
            if (Input.GetKey(wandSimulatorDPadDown))
                wand1_flags += (int)EventBase.Flags.ButtonDown;
            if (Input.GetKey(wandSimulatorDPadLeft))
                wand1_flags += (int)EventBase.Flags.ButtonLeft;
            if (Input.GetKey(wandSimulatorDPadRight))
                wand1_flags += (int)EventBase.Flags.ButtonRight;

            // Wand Button 1 (Triangle/Y)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
            //    flags += (int)EventBase.Flags.Button1;
            // F -> Wand Button 2 (Circle/B)
            if (wandSimulatorButton2.Length > 0 && Input.GetButton(wandSimulatorButton2))
                wand1_flags += (int)EventBase.Flags.Button2;
            // R -> Wand Button 3 (Cross/A)
            if (wandSimulatorButton3.Length > 0 && Input.GetButton(wandSimulatorButton3))
                wand1_flags += (int)EventBase.Flags.Button3;
            // Wand Button 4 (Square/X)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
            //    flags += (int)EventBase.Flags.Button4;
            // Wand Button 8 (R1/RB)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
            //    flags += (int)EventBase.Flags.SpecialButton3;
            // Wand Button 5 (L1/LB)
            if (wandSimulatorButton5.Length > 0 && Input.GetKey(wandSimulatorButton5))
                wand1_flags += (int)EventBase.Flags.Button5;
            // Wand Button 6 (L3)
            if (wandSimulatorButton6.Length > 0 && Input.GetButton(wandSimulatorButton6))
                wand1_flags += (int)EventBase.Flags.Button6;
            // Wand Button 7 (L2)
            if (wandSimulatorButton7.Length > 0 && Input.GetKey(wandSimulatorButton7))
                wand1_flags += (int)EventBase.Flags.Button7;
            // Wand Button 8 (R2)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)))
            //    flags += (int)EventBase.Flags.Button8;
            // Wand Button 9 (R3)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)))
            //    flags += (int)EventBase.Flags.Button9;
            // Wand Button SP1 (Back)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)))
            //    flags += (int)EventBase.Flags.SpecialButton1;
            // Wand Button SP2 (Start)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)))
            //    flags += (int)EventBase.Flags.SpecialButton2;
        }

        // getReal3D
#if USING_GETREAL3D
        if (!CAVE2.IsSimulatorMode() && !disableGetReal3DControllerInput)
        {
            wand1_analog1 = new Vector2(
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickLR)),
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickUD))
            );
            wand1_analog2 = new Vector2(
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickLR)),
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickUD))
            );

            wand1_flags = 0;
            // wand2_flags = 0;

            float wand1_DPadUD = getReal3D.Input.GetAxis("DPadUD");
            float wand1_DPadLR = getReal3D.Input.GetAxis("DPadLR");

            List<float> valuators = getReal3D.Input.valuators;
            if(valuators.Count > 6)
            {
                wand1_DPadUD = valuators[5];
                wand1_DPadLR = valuators[4];
            }

            if (wand1_DPadUD > 0)
                wand1_flags += (int)EventBase.Flags.ButtonUp;
            else if (wand1_DPadUD < 0)
                wand1_flags += (int)EventBase.Flags.ButtonDown;
            if (wand1_DPadLR < 0)
                wand1_flags += (int)EventBase.Flags.ButtonLeft;
            else if (wand1_DPadLR > 0)
                wand1_flags += (int)EventBase.Flags.ButtonRight;

            // Wand Button 1 (Triangle/Y)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
                wand1_flags += (int)EventBase.Flags.Button1;
            // F -> Wand Button 2 (Circle/B)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button2)))
                wand1_flags += (int)EventBase.Flags.Button2;
            // R -> Wand Button 3 (Cross/A)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button3)))
                wand1_flags += (int)EventBase.Flags.Button3;
            // Wand Button 4 (Square/X)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
                wand1_flags += (int)EventBase.Flags.Button4;
            // Wand Button 8 (R1/RB)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
                wand1_flags += (int)EventBase.Flags.SpecialButton3;
            // Wand Button 5 (L1/LB)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button5)))
                wand1_flags += (int)EventBase.Flags.Button5;
            // Wand Button 6 (L3)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button6)))
                wand1_flags += (int)EventBase.Flags.Button6;
            // Wand Button 7 (L2)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button7)))
                wand1_flags += (int)EventBase.Flags.Button7;
            // Wand Button 8 (R2)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)))
                wand1_flags += (int)EventBase.Flags.Button8;
            // Wand Button 9 (R3)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)))
                wand1_flags += (int)EventBase.Flags.Button9;
            // Wand Button SP1 (Back)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)))
                wand1_flags += (int)EventBase.Flags.SpecialButton1;
            // Wand Button SP2 (Start)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)))
                wand1_flags += (int)EventBase.Flags.SpecialButton2;
        }
        
#endif
        if (UnityEngine.XR.XRSettings.enabled)
        {
            if (inputMappingMode == InputMappingMode.CAVE2)
            {
                object[] mapping = CAVE2ToVRControllerMapping(wand1_analog1, wand2_analog1, wand1_analog3, wand2_analog3);
                wand1_analog1 = (Vector2)mapping[0];
                wand2_analog1 = (Vector2)mapping[1];
                wand1_analog3 = (Vector2)mapping[2];
                wand2_analog3 = (Vector2)mapping[3];
                wand1_flags = (int)mapping[4];
                wand2_flags = (int)mapping[5];
            }
            else if (inputMappingMode == InputMappingMode.Oculus)
            {
                object[] mapping = OculusControllerMapping(wand1_analog1, wand2_analog1, wand1_analog3, wand2_analog3);
                wand1_analog1 = (Vector2)mapping[0];
                wand2_analog1 = (Vector2)mapping[1];
                wand1_analog3 = (Vector2)mapping[2];
                wand2_analog3 = (Vector2)mapping[3];
                wand1_flags = (int)mapping[4];
                wand2_flags = (int)mapping[5];
            }
            else if (inputMappingMode == InputMappingMode.Vive)
            {
                object[] mapping = OculusControllerMapping(wand1_analog1, wand2_analog1, wand1_analog3, wand2_analog3);
                wand1_analog1 = (Vector2)mapping[0];
                wand2_analog1 = (Vector2)mapping[1];
                wand1_analog3 = (Vector2)mapping[2];
                wand2_analog3 = (Vector2)mapping[3];
                wand1_flags = (int)mapping[4];
                wand2_flags = (int)mapping[5];
            }
        }

        // Only apply tracking when not in simulator mode
        if (!CAVE2.IsSimulatorMode())
        {
#if USING_GETREAL3D
            mainHeadSensor.UpdateTransform(getReal3D.Input.head.position, getReal3D.Input.head.rotation);
            wandMocapSensor.UpdateTransform(getReal3D.Input.wand.position, getReal3D.Input.wand.rotation);

            //wand2MocapSensor.position = getReal3D.Input.GetSensor("Wand2").position;
            //wand2MocapSensor.orientation = getReal3D.Input.GetSensor("Wand2").rotation;
#endif
        }
        
        if ( (!CAVE2.UsingOmicronServer() && !CAVE2.IsSimulatorMode()) || CAVE2.GetCAVE2Manager().keyboardEventEmulation || UnityEngine.XR.XRSettings.enabled || (CAVE2.UsingOmicronServer() && !omicronManager.WandEventsEnabled()))
        {
            if (wandIDMappedToSimulator == 1)
            {
                wandController.UpdateAnalog(wand1_analog1, wand1_analog2, wand1_analog3, Vector2.zero);
                wandController.rawFlags = wand1_flags;

                wandController2.UpdateAnalog(wand2_analog1, wand2_analog2, wand2_analog3, Vector2.zero);
                wandController2.rawFlags = wand2_flags;
            }
            else
            {
                wandController2.UpdateAnalog(wand1_analog1, wand1_analog2, wand1_analog3, Vector2.zero);
                wandController2.rawFlags = wand1_flags;

                wandController.UpdateAnalog(wand2_analog1, wand2_analog2, wand2_analog3, Vector2.zero);
                wandController.rawFlags = wand2_flags;
            }
        }

        /*
        // If Omicron server is enabled, let Omicron handle tracker/controller data instead of getReal3D
        // Unless keyboard emulation is enabled
        if (CAVE2.UsingOmicronServer() || CAVE2.GetCAVE2Manager().keyboardEventEmulation)
        {
            wandController.UpdateAnalog(wand1_analog1, wand1_analog2, wand1_analog3, Vector2.zero);
            wandController.rawFlags = wand1_flags;

            wandController2.UpdateAnalog(wand2_analog1, wand2_analog2, wand2_analog3, Vector2.zero);
            wandController2.rawFlags = wand2_flags;
        }
        */
    }

    public void UpdateWandController(int wandID, Vector2 analog1, Vector2 analog2, Vector2 analog3, Vector2 analog4, int buttonFlags)
    {
        OmicronController wand = (OmicronController)wandControllers[wandID];
        if(wand != null)
        {
            wand.UpdateAnalog(analog1, analog2, analog3, analog4);
            wand.rawFlags = buttonFlags;
        }
    }

    object[] CAVE2ToVRControllerMapping(Vector2 wand1_analog1, Vector2 wand2_analog1, Vector2 wand1_analog3, Vector2 wand2_analog3)
    {
        int wand1_flags = 0;
        int wand2_flags = 0;

        // VR Left Controller  ---------------------------------

        // Oculus Touch Left: Button.Three / X (Press)
        // Vive Left: Menu Button
        if (Input.GetKey(KeyCode.JoystickButton2))
        {
            if (vrModel == VRModel.Vive)
            {
                // Ignore left menu button, mapped from right controller
                // since accidental trackpad on left is common
                wand1_flags += (int)EventBase.Flags.Button2;
            }
            else
            {
                wand1_flags += (int)EventBase.Flags.Button2;
            }
            if (debug)
                Debug.Log("JoystickButton2");
        }

        // Oculus Touch Left: Button.Four / Y (Press)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton3))
        {
            if (vrModel == VRModel.Vive)
            {
                // Ignore left menu button, mapped from right controller
                // since accidental trackpad on left is common
                //wand1_flags += (int)EventBase.Flags.Button3;
            }
            else
            {
                wand1_flags += (int)EventBase.Flags.Button3;
            }
            if (debug)
                Debug.Log("JoystickButton3");
        }

        // Oculus Touch Left: Button.Start / Menu
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton7))
        {
            if (debug)
                Debug.Log("JoystickButton7");
        }

        // Oculus Touch Left: Analog Stick (Press)
        // Vive Left: Analog Stick (Press)
        if (Input.GetKey(KeyCode.JoystickButton8))
        {
            wand1_flags += (int)EventBase.Flags.Button6;
            if (debug)
                Debug.Log("JoystickButton8");
        }

        // Oculus Touch Left: Button.Three / X (Touch)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton12))
        {
            if (debug)
                Debug.Log("JoystickButton12");
        }

        // Oculus Touch Left: Button.Four / Y (Touch)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton13))
        {
            if (debug)
                Debug.Log("JoystickButton13");
        }

        // Oculus Touch Left: IndexTrigger (Touch)
        // Vive Left: IndexTrigger (Touch)
        if (Input.GetKey(KeyCode.JoystickButton14))
        {
            if (debug)
                Debug.Log("JoystickButton14");
        }

        // Oculus Touch Left: ThumbRest (Touch)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton18))
        {
            if (debug)
                Debug.Log("JoystickButton18");
        }

        /*
         * Grip L/R are not default Unity InputManager axis, but 
         * are required for proper mapping of VR controller buttons.
         * The following can be added to the Axis. All other fields
         * are blank unless otherwise specified
         * 
         * Name: Trigger L
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 9th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         *
         * Name: Trigger R
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 10th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         * 
         * Name: Grip L
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 11th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         * 
         * Name: Grip R
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 12th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         */
        // Oculus Touch Left: IndexTrigger (Press)
        // Vive Left: IndexTrigger (Press)
        wand1_analog3.y = Input.GetAxis(simulatorWand1Trigger);
        if (Input.GetAxis(simulatorWand1Trigger) > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand1_flags += (int)EventBase.Flags.Button7;
            else
                wand1_flags += (int)EventBase.Flags.Button5;
        }

        // Oculus Touch Left: HandTrigger (Press)
        // Vive Left: HandTrigger (Press)
        wand1_analog3.x = Input.GetAxis(simulatorWand1Grip);
        if (Input.GetAxis(simulatorWand1Grip) > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand1_flags += (int)EventBase.Flags.Button5;
            else
                wand1_flags += (int)EventBase.Flags.Button7;
        }

        // Oculus Touch Right: HandTrigger (Press)
        // Vive Right: HandTrigger (Press)
        wand2_analog3.x = Input.GetAxis(simulatorWand2Grip);
        if (Input.GetAxis(simulatorWand2Grip) > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand1_flags += (int)EventBase.Flags.Button3;
            else
                wand2_flags += (int)EventBase.Flags.Button7;
        }

        // Oculus Touch Right: IndexTrigger (Press)
        // Vive Right: IndexTrigger (Press)
        wand2_analog3.y = Input.GetAxis(simulatorWand2Trigger);
        if (Input.GetAxis(simulatorWand2Trigger) > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand2_flags += (int)EventBase.Flags.Button7;
            else
                wand2_flags += (int)EventBase.Flags.Button5;
        }

        // VR Right Controller ---------------------------------
        // For CAVE2 simulator mode, we map the right controller
        // analog stick to the DPad
        bool rightAnalogDPadActivate = false;

        // Oculus Touch Right: Button.One / A (Press)
        // Vive Right: Menu Button
        if (Input.GetKey(KeyCode.JoystickButton0))
        {
            if (vrModel == VRModel.Vive)
            {
                // Map the right menu button to main wand for Vive
                // since accidental trackpad on left is common
                wand1_flags += (int)EventBase.Flags.Button2;
            }
            else
            {
                wand2_flags += (int)EventBase.Flags.Button2;
            }
            if (debug)
                Debug.Log("JoystickButton0");
        }

        // Oculus Touch Right: Button.Two / B (Press)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton1))
        {
            if (vrModel == VRModel.Vive)
            {
                wand1_flags += (int)EventBase.Flags.Button3;
            }
            else
            {
                wand2_flags += (int)EventBase.Flags.Button3;
            }
            if (debug)
                Debug.Log("JoystickButton1");
        }

        // Oculus Touch Right: Oculus Button Reserved

        // Oculus Touch Right: Analog Stick (Press)
        // Vive Right: Analog Stick (Press)
        if (Input.GetKey(KeyCode.JoystickButton9))
        {
            if (vrModel == VRModel.Vive)
            {
                rightAnalogDPadActivate = true;
            }
            wand2_flags += (int)EventBase.Flags.Button6;
            if (debug)
                Debug.Log("JoystickButton9");
        }

        // Oculus Touch Right: Button.One / A (Touch)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton10))
        {
            if (debug)
                Debug.Log("JoystickButton10");
        }

        // Oculus Touch Right: Button.Two / B (Touch)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton11))
        {
            if (debug)
                Debug.Log("JoystickButton11");
        }

        // Oculus Touch Right: IndexTrigger (Touch)
        // Vive Right: IndexTrigger (Touch)
        if (Input.GetKey(KeyCode.JoystickButton15))
        {
            if (debug)
                Debug.Log("JoystickButton15");
        }

        // Oculus Touch Right: ThumbRest (Touch)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton19))
        {
            if (debug)
                Debug.Log("JoystickButton19");
        }

        wand1_analog1 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        wand2_analog1 = new Vector2(Input.GetAxis(simulatorWand2AnalogLR), Input.GetAxis(simulatorWand2AnalogUD));

        // Oculus Touch Right: Analog (Touch)
        // Vive Right: Analog (Touch)
        if (Input.GetKey(KeyCode.JoystickButton17))
        {
            if (vrModel == VRModel.Vive)
            {
                // Right analog dpad is activated on pad press on Vive
            }
            else
            {
                rightAnalogDPadActivate = true;
            }
        }

        // For CAVE2 simulator purposes, we're treating the right analog as the DPad
        if (rightAnalogDPadActivate &&
        (wand2_analog1.y != 0 ||
        wand2_analog1.x != 0)
        )
        {
            /*
             * Horizontal2/Vertical2 are not default Unity InputManager axis, but 
             * are required for proper mapping of the second VR controller pad/analog stick.
             * The following can be added to the Axis. All other fields
             * are blank unless otherwise specified
             * 
             * Name: Horizontal2
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 4th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             * 
             * Name: Vertical2
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 5th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             */
            float padAngle = Mathf.Rad2Deg * Mathf.Atan2(-wand2_analog1.y, wand2_analog1.x);

            if (padAngle < -45 && padAngle > -135)
            {
                wand1_flags += (int)EventBase.Flags.ButtonUp;
                wand2_flags += (int)EventBase.Flags.ButtonUp;
            }
            else if (padAngle > -45 && padAngle < 45)
            {
                wand1_flags += (int)EventBase.Flags.ButtonRight;
                wand2_flags += (int)EventBase.Flags.ButtonRight;
            }
            else if (padAngle > 45 && padAngle < 135)
            {
                wand1_flags += (int)EventBase.Flags.ButtonDown;
                wand2_flags += (int)EventBase.Flags.ButtonDown;
            }
            else if (padAngle < -135 || padAngle > 135)
            {
                wand1_flags += (int)EventBase.Flags.ButtonLeft;
                wand2_flags += (int)EventBase.Flags.ButtonLeft;
            }
        }

        return new object[] { wand1_analog1, wand2_analog1, wand1_analog3, wand2_analog3, wand1_flags, wand2_flags };
    }

    object[] OculusControllerMapping(Vector2 wand1_analog1, Vector2 wand2_analog1, Vector2 wand1_analog3, Vector2 wand2_analog3)
    {
        int wand1_flags = 0;
        int wand2_flags = 0;

        // VR Left Controller  ---------------------------------

        // Oculus Touch Left: Button.Three / X (Press)
        // Vive Left: Menu Button
        if (Input.GetKey(KeyCode.JoystickButton2))
        {
            if (vrModel == VRModel.Vive)
            {
                // Ignore left menu button, mapped from right controller
                // since accidental trackpad on left is common
                wand1_flags += (int)EventBase.Flags.Button5;
            }
            else
            {
                wand1_flags += (int)EventBase.Flags.Button2;
            }
            if (debug)
                Debug.Log("JoystickButton2");
        }

        // Oculus Touch Left: Button.Four / Y (Press)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton3))
        {
            wand1_flags += (int)EventBase.Flags.Button3;
            if (debug)
                Debug.Log("JoystickButton3");
        }

        // Oculus Touch Left: Button.Start / Menu
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton7))
        {
            if (debug)
                Debug.Log("JoystickButton7");
        }

        // Oculus Touch Left: Analog Stick (Press)
        // Vive Left: Analog Stick (Press)
        if (Input.GetKey(KeyCode.JoystickButton8))
        {
            wand1_flags += (int)EventBase.Flags.Button6;
            if (debug)
                Debug.Log("JoystickButton8");
        }

        // Oculus Touch Left: Button.Three / X (Touch)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton12))
        {
            if (debug)
                Debug.Log("JoystickButton12");
        }

        // Oculus Touch Left: Button.Four / Y (Touch)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton13))
        {
            if (debug)
                Debug.Log("JoystickButton13");
        }

        // Oculus Touch Left: IndexTrigger (Touch)
        // Vive Left: IndexTrigger (Touch)
        if (Input.GetKey(KeyCode.JoystickButton14))
        {
            if (debug)
                Debug.Log("JoystickButton14");
        }

        // Oculus Touch Left: ThumbRest (Touch)
        // Vive Left: N/A
        if (Input.GetKey(KeyCode.JoystickButton18))
        {
            if (debug)
                Debug.Log("JoystickButton18");
        }

        /*
         * Grip L/R are not default Unity InputManager axis, but 
         * are required for proper mapping of VR controller buttons.
         * The following can be added to the Axis. All other fields
         * are blank unless otherwise specified
         * 
         * Name: Trigger L
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 9th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         *
         * Name: Trigger R
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 10th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         * 
         * Name: Grip L
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 11th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         * 
         * Name: Grip R
         * Gravity: 3
         * Dead: 0.001
         * Sensitivity: 3
         * Type: Joystick Axis
         * Axis: 12th axis (Joysticks)
         * JoyNum: Get Motion from all Joysticks
         */
        // Oculus Touch Left: IndexTrigger (Press)
        // Vive Left: IndexTrigger (Press)
        wand1_analog3.y = Input.GetAxis("Trigger L");
        if (Input.GetAxis("Trigger L") > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand1_flags += (int)EventBase.Flags.Button7;
            else
                wand1_flags += (int)EventBase.Flags.Button5;
        }

        // Oculus Touch Left: HandTrigger (Press)
        // Vive Left: HandTrigger (Press)
        wand1_analog3.x = Input.GetAxis("Grip L");
        if (Input.GetAxis("Grip L") > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand1_flags += (int)EventBase.Flags.Button3;
            else
                wand1_flags += (int)EventBase.Flags.Button7;
        }

        // Oculus Touch Right: HandTrigger (Press)
        // Vive Right: HandTrigger (Press)
        wand2_analog3.x = Input.GetAxis("Grip R");
        if (Input.GetAxis("Grip R") > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand1_flags += (int)EventBase.Flags.Button3;
            else
                wand2_flags += (int)EventBase.Flags.Button7;
        }

        // Oculus Touch Right: IndexTrigger (Press)
        // Vive Right: IndexTrigger (Press)
        wand2_analog3.y = Input.GetAxis("Trigger R");
        if (Input.GetAxis("Trigger R") > 0.1f)
        {
            if (vrModel == VRModel.Vive)
                wand2_flags += (int)EventBase.Flags.Button7;
            else
                wand2_flags += (int)EventBase.Flags.Button5;
        }

        // VR Right Controller ---------------------------------
        // For CAVE2 simulator mode, we map the right controller
        // analog stick to the DPad
        bool rightAnalogDPadActivate = false;

        // Oculus Touch Right: Button.One / A (Press)
        // Vive Right: Menu Button
        if (Input.GetKey(KeyCode.JoystickButton0))
        {
            if (vrModel == VRModel.Vive)
            {
                // Map the right menu button to main wand for Vive
                // since accidental trackpad on left is common
                wand1_flags += (int)EventBase.Flags.Button2;
            }
            else
            {
                wand2_flags += (int)EventBase.Flags.Button2;
            }
            if (debug)
                Debug.Log("JoystickButton0");
        }

        // Oculus Touch Right: Button.Two / B (Press)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton1))
        {
            wand2_flags += (int)EventBase.Flags.Button3;
            if (debug)
                Debug.Log("JoystickButton1");
        }

        // Oculus Touch Right: Oculus Button Reserved

        // Oculus Touch Right: Analog Stick (Press)
        // Vive Right: Analog Stick (Press)
        if (Input.GetKey(KeyCode.JoystickButton9))
        {
            if (vrModel == VRModel.Vive)
            {
                rightAnalogDPadActivate = true;
            }
            wand2_flags += (int)EventBase.Flags.Button6;
            if (debug)
                Debug.Log("JoystickButton9");
        }

        // Oculus Touch Right: Button.One / A (Touch)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton10))
        {
            if (debug)
                Debug.Log("JoystickButton10");
        }

        // Oculus Touch Right: Button.Two / B (Touch)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton11))
        {
            if (debug)
                Debug.Log("JoystickButton11");
        }

        // Oculus Touch Right: IndexTrigger (Touch)
        // Vive Right: IndexTrigger (Touch)
        if (Input.GetKey(KeyCode.JoystickButton15))
        {
            if (debug)
                Debug.Log("JoystickButton15");
        }

        // Oculus Touch Right: ThumbRest (Touch)
        // Vive Right: N/A
        if (Input.GetKey(KeyCode.JoystickButton19))
        {
            if (debug)
                Debug.Log("JoystickButton19");
        }

        wand1_analog1 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        wand2_analog1 = new Vector2(Input.GetAxis(simulatorWand2AnalogLR), -Input.GetAxis(simulatorWand2AnalogUD));

        // Oculus Touch Right: Analog (Touch)
        // Vive Right: Analog (Touch)
        if (Input.GetKey(KeyCode.JoystickButton17))
        {
            if (vrModel == VRModel.Vive)
            {
                // Right analog dpad is activated on pad press on Vive
            }
            else
            {
                rightAnalogDPadActivate = true;
            }
        }

        // For CAVE2 simulator purposes, we're treating the right analog as the DPad
        if (rightAnalogDPadActivate &&
        (Input.GetAxis(simulatorWand2AnalogUD) != 0 ||
        Input.GetAxis(simulatorWand2AnalogLR) != 0)
        )
        {
            /*
             * Horizontal2/Vertical2 are not default Unity InputManager axis, but 
             * are required for proper mapping of the second VR controller pad/analog stick.
             * The following can be added to the Axis. All other fields
             * are blank unless otherwise specified
             * 
             * Name: Horizontal2
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 4th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             * 
             * Name: Vertical2
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 5th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             */
            float padAngle = Mathf.Rad2Deg * Mathf.Atan2(Input.GetAxis(simulatorWand2AnalogUD), Input.GetAxis(simulatorWand2AnalogLR));

            if (padAngle < -45 && padAngle > -135)
            {
                wand1_flags += (int)EventBase.Flags.ButtonUp;
            }
            else if (padAngle > -45 && padAngle < 45)
            {
                wand1_flags += (int)EventBase.Flags.ButtonRight;
            }
            else if (padAngle > 45 && padAngle < 135)
            {
                wand1_flags += (int)EventBase.Flags.ButtonDown;
            }
            else if (padAngle < -135 || padAngle > 135)
            {
                wand1_flags += (int)EventBase.Flags.ButtonLeft;
            }
        }

        return new object[] { wand1_analog1, wand2_analog1, wand1_analog3, wand2_analog3, wand1_flags, wand2_flags };
    }
}
