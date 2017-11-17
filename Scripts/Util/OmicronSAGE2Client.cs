using UnityEngine;
using System.Collections;

public class OmicronSAGE2Client : MonoBehaviour {
#if UNITY_WEBGL
    WebSocket ws;

    [SerializeField]
    string sage2Server = "localhost";

    [SerializeField]
    int sage2OmicronPort = 19090;

    OmicronManager omicronManager;

    // Use this for initialization
    IEnumerator Start () {
        omicronManager = GetComponent<OmicronManager>();

        ws = new WebSocket(new System.Uri("ws://"+sage2Server + ":" + sage2OmicronPort));
        yield return StartCoroutine(ws.Connect());

        while (true)
        {
            byte[] reply = ws.Recv();
            if (reply != null)
            {
                omicronManager.AddEvent(omicronConnector.OmicronConnectorClient.ByteArrayToEventData(reply));
            }
            if (ws.error != null)
            {
                Debug.LogError("Error: " + ws.error);
                break;
            }
            yield return 0;
        }
        ws.Close();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
#endif
}
