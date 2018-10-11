using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class OmicronMocapObject : OmicronEventClient
{
    public int sourceID = 1; // -1 for any

    public Vector3 position;
    public Quaternion orientation;

    public bool hideIfNotTracked = false;
    public float timeSinceLastUpdate;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeMocap;
        InitOmicron();
    }

    public override void OnEvent(EventData e)
    {
        if (e.sourceId == sourceID || sourceID == -1)
        {
            position = new Vector3(e.posx, e.posy, e.posz);
            orientation = new Quaternion(e.orx, e.ory, e.orz, e.orw);
            timeSinceLastUpdate = 0;
        }
    }

    private void Update()
    {
        transform.localPosition = position;
        transform.localRotation = orientation;
        timeSinceLastUpdate += Time.deltaTime;

        if (GetComponent<MeshRenderer>())
        {
            GetComponent<MeshRenderer>().enabled = (hideIfNotTracked && timeSinceLastUpdate > 1) ? false : true;
        }
    }
}
