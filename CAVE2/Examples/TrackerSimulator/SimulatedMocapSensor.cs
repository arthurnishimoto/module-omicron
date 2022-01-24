using omicronConnector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
public class SimulatedMocapSensor : MonoBehaviour
{
    [SerializeField]
    int sourceID = 0;

    [SerializeField]
    bool addRandomJitter = false;

    [SerializeField]
    Vector3 outputPos = Vector3.zero;

    [SerializeField]
    Vector3 outputRot = Vector3.zero;

    OmicronManager omicronManager;

    // Start is called before the first frame update
    void Start()
    {
        omicronManager = CAVE2.GetCAVE2Manager().GetComponent<OmicronManager>();
    }

    // Update is called once per frame
    void Update()
    {
        outputPos = transform.localPosition;
        outputRot = transform.localEulerAngles;

        if(addRandomJitter)
        {
            outputPos.x += Random.Range(-1000, 1000) / 1000000.0f;
            outputPos.y += Random.Range(-1000, 1000) / 1000000.0f;
            outputPos.z += Random.Range(-1000, 1000) / 1000000.0f;

            outputRot.x += Random.Range(-1000, 1000) / 1000000.0f;
            outputRot.y += Random.Range(-1000, 1000) / 1000000.0f;
            outputRot.z += Random.Range(-1000, 1000) / 1000000.0f;
        }

        Quaternion qrot = Quaternion.Euler(outputRot);

        EventData evt = new EventData();
        evt.timestamp = (uint)Time.time;
        evt.sourceId = (uint)sourceID;
        evt.serviceId = 0;
        evt.serviceType = EventBase.ServiceType.ServiceTypeMocap;
        evt.type = (uint)EventBase.Type.Update;
        evt.flags = 0;
        evt.posx = outputPos.x;
        evt.posy = outputPos.y;
        evt.posz = -outputPos.z;
        evt.orx = -transform.localRotation.x;
        evt.ory = -transform.localRotation.y;
        evt.orz = transform.localRotation.z;
        evt.orw = transform.localRotation.w;

        omicronManager.AddEvent(evt);
    }
}
