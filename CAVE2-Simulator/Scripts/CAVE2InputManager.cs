using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;

public class CAVE2InputManager : OmicronEventClient
{
    public Hashtable mocapStates;
    public Hashtable wandStates;

    public float axisSensitivity = 1f;
    public float axisDeadzone = 0.2f;

    // Use this for initialization
    new void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
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
        }
    }

    void UpdateController(uint wandID, uint flags, Vector2 leftAnalogStick, Vector2 rightAnalogStick, Vector2 analogTrigger)
    {
        if (wandStates.ContainsKey((int)wandID))
        {
            ((WandState)wandStates[(int)wandID]).UpdateController(flags, leftAnalogStick, rightAnalogStick, analogTrigger);
        }
        else
        {
            int mocapID = (int)wandID;
            /*
            if ((int)wandID == 1)
            {
                mocapID = Wand1MocapID;
            }
            else if ((int)wandID == 2)
            {
                mocapID = Wand2MocapID;
            }
            else if ((int)wandID == 3)
            {
                mocapID = Wand3MocapID;
            }
            else if ((int)wandID == 4)
            {
                mocapID = Wand4MocapID;
            }
            */
            WandState newWandState = new WandState((int)wandID, mocapID);
            newWandState.UpdateController(flags, leftAnalogStick, rightAnalogStick, analogTrigger);
            wandStates.Add((int)wandID, newWandState);
        }
    }
}
