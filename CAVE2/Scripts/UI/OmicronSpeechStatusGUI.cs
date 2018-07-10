using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using omicronConnector;
using omicron;

public class OmicronSpeechStatusGUI : OmicronEventClient {

    [SerializeField]
    Text statusText;

    [SerializeField]
    Text lastEventText;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeSpeech;
        InitOmicron();
    }

    public override void OnEvent(EventData evt)
    {
        // Speech Event:
        // extraDataString = speech string
        // posX = speech confidence
        if (evt.serviceType == EventBase.ServiceType.ServiceTypeSpeech)
        {
            statusText.text = "Online";
            statusText.color = Color.green;
            float speechConfidence = evt.posx;

            string speechString = evt.getExtraDataString().Trim();
            lastEventText.text = "'" + speechString + "' " + speechConfidence;

            if(CAVE2.IsMaster())
            {
                CAVE2.BroadcastMessage(gameObject.name, "UpdateOmicronSpeechStatus", lastEventText.text);
            }
        }
    }

    void UpdateOmicronSpeechStatus(string text)
    {
        statusText.text = "Online";
        statusText.color = Color.green;
        lastEventText.text = text;
    }
}
