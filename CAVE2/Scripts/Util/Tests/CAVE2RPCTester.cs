using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CAVE2RPCTester : MonoBehaviour {

    Text uiText;

    int testMode = 0;

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
                if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.Button5))
                {
                    switch(testMode)
                    {
                        case (0):
                            CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent");
                            break;
                        case (1):
                            CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent1", 1);
                            break;
                        case (2):
                            CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent2", 2, "Two");
                            break;
                        case (3):
                            CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent3", 3, "Three", new Vector3(1, 2, 3));
                            break;
                        case (4):
                            CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEvent4", 1, "Two", new Vector3(3, 3.2f, 3.3f), 4.0f);
                            break;
                        case (5):
                            CallRPCFromFunction();
                            break;
                    }
                    testMode++;
                    if (testMode >= 6)
                        testMode = 0;
                }
            }
            
        }
    }

    public void CallRPCFromFunction()
    {
        CAVE2.BroadcastMessage(gameObject.name, "ProcessRPCEventFunc");
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

    void ProcessRPCEventFunc()
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (No Param) from Function()";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (No Param) from Function()";
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

    void ProcessRPCEvent4(object[] param)
    {
        if (CAVE2.IsMaster())
        {
            uiText.text = "Master Node:\n";
            uiText.text += "CAVE2 RPC Sent (4 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
            uiText.text += "param[3]: " + param[3] + "\n";
        }
        else
        {
            uiText.text = "Display Node:\n";
            uiText.text += "CAVE2 RPC Received (4 Param)\n";
            uiText.text += "param[0]: " + param[0] + "\n";
            uiText.text += "param[1]: " + param[1] + "\n";
            uiText.text += "param[2]: " + param[2] + "\n";
            uiText.text += "param[3]: " + param[3] + "\n";
        }
    }
}
