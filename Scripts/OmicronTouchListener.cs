using UnityEngine;
using System.Collections;
using omicronConnector;
using omicron;

public class OmicronTouchListener : OmicronEventClient
{
    // Use this for initialization
    new void Start () {
        eventOptions = EventBase.ServiceType.ServiceTypePointer;
        InitOmicron();
    }

    void OnEvent(EventData e)
    {
        Debug.Log("OmicronEventClient: '"+name+"' received " + e.serviceType);
    }
}
