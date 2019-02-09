using UnityEngine;
using System.Collections;

public class getReal3DMocapUpdater : MonoBehaviour {

    public string sensorName;

    // Offset to tracking data (ex. object pivot vs tracking marker center)
    [SerializeField] Vector3 posOffset;
    [SerializeField] Vector3 rotOffset;

    [SerializeField]
    bool useLateUpdate;

    // Use this for initialization
    void Start () {
	
	}
#if USING_GETREAL3D
    // Update is called once per frame
    void Update () {
        if (!useLateUpdate)
        {
            transform.localPosition = getReal3D.Input.GetSensor(sensorName).position + posOffset;
            transform.localRotation = getReal3D.Input.GetSensor(sensorName).rotation;
            transform.Rotate(rotOffset);
        }
    }

    void LateUpdate()
    {
        if (useLateUpdate)
        {
            transform.localPosition = getReal3D.Input.GetSensor(sensorName).position + posOffset;
            transform.localRotation = getReal3D.Input.GetSensor(sensorName).rotation;
            transform.Rotate(rotOffset);
        }
    }
#endif
}
