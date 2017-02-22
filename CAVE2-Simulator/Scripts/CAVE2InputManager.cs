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

    public OmicronControllerManager.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        if(wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button);
        }
        else if( CAVE2.UsingGetReal3D() && wandID == 1)
        {
            if (getReal3D.Input.GetButtonDown(CAVE2.CAVE2ToGetReal3DButton(button)))
                return OmicronControllerManager.ButtonState.Down;
            else if (getReal3D.Input.GetButtonUp(CAVE2.CAVE2ToGetReal3DButton(button)))
                return OmicronControllerManager.ButtonState.Up;
            else if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(button)))
                return OmicronControllerManager.ButtonState.Held;
            else
                return OmicronControllerManager.ButtonState.Idle;
        }
        return OmicronControllerManager.ButtonState.Idle;
    }

    public bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        if (wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronControllerManager.ButtonState.Down;
        }
        else if (CAVE2.UsingGetReal3D() && wandID == 1)
        {
            return getReal3D.Input.GetButtonDown(CAVE2.CAVE2ToGetReal3DButton(button));
        }
        return false;
    }

    public bool GetButton(int wandID, CAVE2.Button button)
    {
        if (wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronControllerManager.ButtonState.Held;
        }
        else if (CAVE2.UsingGetReal3D() && wandID == 1)
        {
            // Handle getReal3D DPad mapped to axes
            if (button == CAVE2.Button.ButtonUp)
                return getReal3D.Input.GetAxis("DPadUD") > 0;
            else if (button == CAVE2.Button.ButtonDown)
                return getReal3D.Input.GetAxis("DPadUD") < 0;
            else if (button == CAVE2.Button.ButtonRight)
                return getReal3D.Input.GetAxis("DPadLR") > 0;
            else if (button == CAVE2.Button.ButtonLeft)
                return getReal3D.Input.GetAxis("DPadLR") < 0;

            return getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(button));
        }
        return false;
    }

    public bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        if (wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronControllerManager.ButtonState.Up;
        }
        else if (CAVE2.UsingGetReal3D() && wandID == 1)
        {
            // Handle getReal3D DPad mapped to axes
            if (button == CAVE2.Button.ButtonUp)
                return getReal3D.Input.GetAxis("DPadUD") > 0;
            else if (button == CAVE2.Button.ButtonDown)
                return getReal3D.Input.GetAxis("DPadUD") < 0;
            else if (button == CAVE2.Button.ButtonRight)
                return getReal3D.Input.GetAxis("DPadLR") > 0;
            else if (button == CAVE2.Button.ButtonLeft)
                return getReal3D.Input.GetAxis("DPadLR") < 0;

            return getReal3D.Input.GetButtonUp(CAVE2.CAVE2ToGetReal3DButton(button));
        }
        return false;
    }

    public float GetAxis(int wandID, CAVE2.Axis axis)
    {
        if (wandManagers != null && wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetAxis(axis);
        }
        else if (CAVE2.UsingGetReal3D() && wandID == 1)
        {
            return getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(axis));
        }

        return 0;
    }

    // Parses Omicron Input Data
    void OnEvent(EventData e)
    {
        //Debug.Log("CAVE2Manager_Legacy: '"+name+"' received " + e.serviceType);
        if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap)
        {
            if (!mocapManagers.ContainsKey((int)e.sourceId))
            {
                OmicronMocapManager mocapManager = gameObject.AddComponent<OmicronMocapManager>();
                mocapManager.sourceID = (int)e.sourceId;

                mocapManagers[(int)e.sourceId] = mocapManager;
            }

            /*
            // -zPos -xRot -yRot for Omicron->Unity coordinate conversion)
            Vector3 unityPos = new Vector3(e.posx, e.posy, -e.posz);
            Quaternion unityRot = new Quaternion(-e.orx, -e.ory, e.orz, e.orw);

#if USING_GETREAL3D
			getReal3D.RpcManager.call ("UpdateMocapRPC", e.sourceId, unityPos, unityRot );
#else
            //UpdateMocap(e.sourceId, unityPos, unityRot);
#endif
            */
        }
        else if (e.serviceType == EventBase.ServiceType.ServiceTypeWand)
        {
            if ( !wandManagers.ContainsKey((int)e.sourceId) )
            {
                OmicronControllerManager wandButtonManager = gameObject.AddComponent<OmicronControllerManager>();
                wandButtonManager.sourceID = (int)e.sourceId;

                wandManagers[(int)e.sourceId] = wandButtonManager;
            }

            /*
            // -zPos -xRot -yRot for Omicron->Unity coordinate conversion)
            //Vector3 unityPos = new Vector3(e.posx, e.posy, -e.posz);
            //Quaternion unityRot = new Quaternion(-e.orx, -e.ory, e.orz, e.orw);

            // Flip Up/Down analog stick values
            Vector2 leftAnalogStick = new Vector2(e.getExtraDataFloat(0), -e.getExtraDataFloat(1)) * axisSensitivity;
            Vector2 rightAnalogStick = new Vector2(e.getExtraDataFloat(2), e.getExtraDataFloat(3)) * axisSensitivity;
            Vector2 analogTrigger = new Vector2(e.getExtraDataFloat(4), e.getExtraDataFloat(5));

            if (Mathf.Abs(leftAnalogStick.x) < axisDeadzone)
                leftAnalogStick.x = 0;
            if (Mathf.Abs(leftAnalogStick.y) < axisDeadzone)
                leftAnalogStick.y = 0;
            if (Mathf.Abs(rightAnalogStick.x) < axisDeadzone)
                rightAnalogStick.x = 0;
            if (Mathf.Abs(rightAnalogStick.y) < axisDeadzone)
                rightAnalogStick.y = 0;

#if USING_GETREAL3D
			getReal3D.RpcManager.call ("UpdateControllerRPC", e.sourceId, e.flags, leftAnalogStick, rightAnalogStick, analogTrigger);
#else
            UpdateController(e.sourceId, e.flags, leftAnalogStick, rightAnalogStick, analogTrigger);
#endif
            */
        }
    }
}
