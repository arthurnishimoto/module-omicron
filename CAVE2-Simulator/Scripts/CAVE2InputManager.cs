using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;

public class CAVE2InputManager : OmicronEventClient
{
    Hashtable mocapManagers;
    Hashtable wandManagers;

    public float axisSensitivity = 1f;
    public float axisDeadzone = 0.2f;

    // Use this for initialization
    new void Start () {
        base.Start();

        mocapManagers = new Hashtable();
        wandManagers = new Hashtable();
    }

    public OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        if(wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronController wandButtonManager = (OmicronController)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button);
        }
        return OmicronController.ButtonState.Idle;
    }

    public bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        if (wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronController wandButtonManager = (OmicronController)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronController.ButtonState.Down;
        }
        return false;
    }

    public bool GetButton(int wandID, CAVE2.Button button)
    {
        if (wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronController wandButtonManager = (OmicronController)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronController.ButtonState.Held;
        }
        return false;
    }

    public bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        if (wandManagers.ContainsKey(wandID))
        {
            OmicronController wandButtonManager = (OmicronController)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronController.ButtonState.Up;
        }
        return false;
    }

    public float GetAxis(int wandID, CAVE2.Axis axis)
    {
        if (wandManagers.ContainsKey(wandID))
        {
            OmicronController wandButtonManager = (OmicronController)wandManagers[wandID];
            return wandButtonManager.GetAxis(axis);
        }
        return 0;
    }

    public Vector3 GetMocapPosition(int ID)
    {
        if (mocapManagers.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapManagers[ID];
            return mocap.position;
        }
        return Vector3.zero;
    }

    public Quaternion GetMocapRotation(int ID)
    {
        if (mocapManagers.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapManagers[ID];
            return mocap.orientation;
        }
        return Quaternion.identity;
    }

    // Parses Omicron Input Data
    void OnEvent(EventData e)
    {
        //Debug.Log("CAVE2Manager_Legacy: '"+name+"' received " + e.serviceType);
        if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap)
        {
            if (!mocapManagers.ContainsKey((int)e.sourceId))
            {
                OmicronMocapSensor mocapManager = gameObject.AddComponent<OmicronMocapSensor>();
                mocapManager.sourceID = (int)e.sourceId;

                mocapManagers[(int)e.sourceId] = mocapManager;
            }
        }
        else if (e.serviceType == EventBase.ServiceType.ServiceTypeWand)
        {
            if ( !wandManagers.ContainsKey((int)e.sourceId) )
            {
                OmicronController wandButtonManager = gameObject.AddComponent<OmicronController>();
                wandButtonManager.sourceID = (int)e.sourceId;

                wandManagers[(int)e.sourceId] = wandButtonManager;
            }
        }
    }

    void Update()
    {
        int wandID = 1;

        OmicronController wandButtonManager;
        if (!wandManagers.ContainsKey(wandID))
        {
            wandButtonManager = gameObject.AddComponent<OmicronController>();
            wandButtonManager.sourceID = wandID;
            wandManagers[wandID] = wandButtonManager;
        }
        else
        {
            wandButtonManager = (OmicronController)wandManagers[wandID];
        }
#if USING_GETREAL3D
        wandButtonManager.analogInput1 = new Vector2(
            getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickLR)),
            getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickUD))
        );
        wandButtonManager.analogInput2 = new Vector2(
            getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickLR)),
            getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickUD))
        );
        //wandButtonManager.analogInput3 = new Vector2(
        //    getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.AnalogTriggerL)),
        //    getReal3D.Input.GetAxis(0)
        //);

        int flags = 0;
        float DPadUD = getReal3D.Input.GetAxis("DPadUD");
        float DPadLR = getReal3D.Input.GetAxis("DPadLR");
        if (DPadUD > 0)
            flags += (int)EventBase.Flags.ButtonUp;
        else if (DPadUD < 0)
            flags += (int)EventBase.Flags.ButtonDown;
        if (DPadLR < 0)
            flags += (int)EventBase.Flags.ButtonLeft;
        else if (DPadLR > 0)
            flags += (int)EventBase.Flags.ButtonRight;

        // Wand Button 1 (Triangle/Y)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
            flags += (int)EventBase.Flags.Button1;
        // F -> Wand Button 2 (Circle/B)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button2)))
            flags += (int)EventBase.Flags.Button2;
        // R -> Wand Button 3 (Cross/A)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button3)))
            flags += (int)EventBase.Flags.Button3;
        // Wand Button 4 (Square/X)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
            flags += (int)EventBase.Flags.Button4;
        // Wand Button 8 (R1/RB)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
            flags += (int)EventBase.Flags.SpecialButton3;
        // Wand Button 5 (L1/LB)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button5)))
            flags += (int)EventBase.Flags.Button5;
        // Wand Button 6 (L3)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button6)))
            flags += (int)EventBase.Flags.Button6;
        // Wand Button 7 (L2)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button7)))
            flags += (int)EventBase.Flags.Button7;
        // Wand Button 8 (R2)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)))
            flags += (int)EventBase.Flags.Button8;
        // Wand Button 9 (R3)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)))
            flags += (int)EventBase.Flags.Button9;
        // Wand Button SP1 (Back)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)))
            flags += (int)EventBase.Flags.SpecialButton1;
        // Wand Button SP2 (Start)
        if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)))
            flags += (int)EventBase.Flags.SpecialButton2;

        wandButtonManager.rawFlags = flags;
#endif
    }
}
