using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class OmicronMocapSensor : OmicronEventClient
{
    public int sourceID = 1; // -1 for any

    public Vector3 position;
    public Quaternion orientation;
    public Vector3 positionMod = Vector3.one;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeMocap;
        InitOmicron();
    }

    public override void OnEvent(EventData e)
    {
        if (CAVE2.IsMaster() && CAVE2.GetCAVE2Manager().mocapEmulation)
            return;

        if (e.sourceId == sourceID || sourceID == -1)
        {
            position = new Vector3(e.posx * positionMod.x, e.posy * positionMod.y, e.posz * positionMod.z);
            orientation = new Quaternion(e.orx, e.ory, e.orz, e.orw);
        }
    }
}
