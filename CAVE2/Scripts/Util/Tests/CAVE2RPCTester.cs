using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CAVE2RPCTester : MonoBehaviour {

    Text uiText;

    // Use this for initialization
    void Start()
    {
        uiText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (uiText != null)
        {
            if(CAVE2.IsMaster())
            {
                if(CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonUp))
                {
                    CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent");
                }
                else if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonLeft))
                {
                    CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent1", 1);
                }
                else if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonRight))
                {
                    CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent2", 2, "Two");
                }
                else if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonDown))
                {
                    CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent3", 3, "Three", new Vector3(1, 2, 3));
                }
            }
            
        }
    }

    void ProcessRPCEvent()
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (No Param)";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (No Param)";
        }
    }

    void ProcessRPCEvent1(int id)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (1 Param)\n";
            uiText.text += "param[0]: " + id;
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (1 Param)\n";
            uiText.text += "param[0]: " + id;
        }
    }

    void ProcessRPCEvent2(object[] param)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (2 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1];
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (2 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1];
        }
    }

    void ProcessRPCEvent3(object[] param)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (3 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (3 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
        }
    }
}
