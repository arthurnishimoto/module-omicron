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
                transform.localPosition = Omicron.Input.GetMocapPosition(sourceID);
                transform.localRotation = Omicron.Input.GetMocapRotation(sourceID);
                break;
        }
    }
}
