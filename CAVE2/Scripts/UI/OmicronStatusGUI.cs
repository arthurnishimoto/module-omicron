using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OmicronStatusGUI : MonoBehaviour {

    [SerializeField]
    OmicronManager omicronManager;

    [SerializeField]
    Text uiText;

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
                if(omicronManager.GetConnectionState() == OmicronManager.ConnectionState.Connected)
                {
                    uiText.text = "Connected to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                    uiText.color = Color.green;
                }
                else if (omicronManager.GetConnectionState() == OmicronManager.ConnectionState.NotConnected)
                {
                    uiText.text = "Not Connected";
                    uiText.color = Color.white;
                }
                else if (omicronManager.GetConnectionState() == OmicronManager.ConnectionState.FailedToConnect)
                {
                    uiText.text = "Failed to connect to " + omicronManager.serverIP + ":" + omicronManager.serverMsgPort;
                    uiText.color = Color.red;
                }
            }
        }
	}
}
