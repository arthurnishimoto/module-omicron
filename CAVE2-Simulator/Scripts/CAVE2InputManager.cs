using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;

public class CAVE2InputManager : OmicronEventClient
{
    Hashtable mocapStates;
    Hashtable wandManagers;

    public float axisSensitivity = 1f;
    public float axisDeadzone = 0.2f;

    WandState nullWandState = new WandState(-1, -1);

    // Use this for initialization
    new void Start () {
        base.Start();

        wandManagers = new Hashtable();
    }

    public OmicronControllerManager.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        if(wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button);
        }
        return OmicronControllerManager.ButtonState.Idle;
    }

    public bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        if (wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronControllerManager.ButtonState.Down;
        }
        return false;
    }

    public bool GetButton(int wandID, CAVE2.Button button)
    {
        if (wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronControllerManager.ButtonState.Held;
        }
        return false;
    }

    public bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        if (wandManagers.ContainsKey(wandID))
        {
            OmicronControllerManager wandButtonManager = (OmicronControllerManager)wandManagers[wandID];
            return wandButtonManager.GetButtonState(button) == OmicronControllerManager.ButtonState.Up;
        }
        return false;
    }

    // Parses Omicron Input Data
    void OnEvent(EventData e)
    {
        //Debug.Log("CAVE2Manager_Legacy: '"+name+"' received " + e.serviceType);
        if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap)
        {
            // -zPos -xRot -yRot for Omicron->Unity coordinate conversion)
            Vector3 unityPos = new Vector3(e.posx, e.posy, -e.posz);
            Quaternion unityRot = new Quaternion(-e.orx, -e.ory, e.orz, e.orw);

#if USING_GETREAL3D
			getReal3D.RpcManager.call ("UpdateMocapRPC", e.sourceId, unityPos, unityRot );
#else
            //UpdateMocap(e.sourceId, unityPos, unityRot);
#endif

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
