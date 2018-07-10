using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OmicronStatusGUI : MonoBehaviour {

    [SerializeField]
    OmicronManager omicronManager;

    [SerializeField]
    Text uiText;

    OmicronManager.ConnectionState connectionState;

    // Use this for initialization
    void Start () {
        omicronManager = CAVE2.GetCAVE2Manager().GetComponent<OmicronManager>();
        uiText = gameObject.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
	    if(uiText != null)
        {
            if(omicronManager == null)
            {
                uiText.text = "Omicron Not Found";
                uiText.color = Color.yellow;
            }
            else
            {
                if (CAVE2.IsMaster())
                {
                    CAVE2.BroadcastMessage(gameObject.name, "UpdateOmicronConnectionState", omicronManager.GetConnectionState());

                    if (connectionState == OmicronManager.ConnectionState.Connected)
                    {
                        uiText.text = "Connected to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.green;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.NotConnected)
                    {
                        uiText.text = "Not Connected";
                        uiText.color = Color.white;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.FailedToConnect)
                    {
                        uiText.text = "Failed to connect to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.red;
                    }
                }
                else
                {
                    if (connectionState == OmicronManager.ConnectionState.Connected)
                    {
                        uiText.text = "Master connected to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.green;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.NotConnected)
                    {
                        uiText.text = "Master Not Connected";
                        uiText.color = Color.white;
                    }
                    else if (connectionState == OmicronManager.ConnectionState.FailedToConnect)
                    {
                        uiText.text = "Master failed to connect to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                        uiText.color = Color.red;
                    }
                }
            }
        }
	}

    void UpdateOmicronConnectionState(OmicronManager.ConnectionState state)
    {
        connectionState = state;
    }
}
