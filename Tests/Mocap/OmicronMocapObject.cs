using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class OmicronMocapObject : OmicronEventClient
{

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeMocap;
        InitOmicron();
    }

    void OnEvent(EventData e)
    {
        transform.localPosition = new Vector3(e.posx, e.posy, e.posz);
    }
}
