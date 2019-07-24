using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    public enum TrackerType { UnityXR, Omicron };

    [SerializeField]
    TrackerType trackerSource = TrackerType.UnityXR;

    [Header("Unity XR Config")]
    [SerializeField]
    UnityEngine.XR.XRNode node;

    [Header("Omicron Config")]
    [SerializeField]
    int sourceID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        switch (trackerSource)
        {
            case (TrackerType.UnityXR):
                transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(node);
                transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(node);
                break;
            case (TrackerType.Omicron):
                Vector3 position = Omicron.GetMocapPosition(sourceID);
                transform.localPosition = new Vector3(position.x, position.y, -position.z);

                Vector3 eulerAngles = Omicron.GetMocapRotation(sourceID).eulerAngles;
                transform.localRotation = Quaternion.Euler(new Vector3(-eulerAngles.x, -eulerAngles.y, eulerAngles.z));
                break;
        }
    }
}
