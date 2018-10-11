using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRTrackingOverride : MonoBehaviour {

    [SerializeField]
    bool disablePositionalTracking;

    Vector3 lastEulerAngles;

	// Use this for initialization
	void Start () {
        UnityEngine.XR.InputTracking.disablePositionalTracking = disablePositionalTracking;
	}

    public void LateUpdate()
    {
        if (disablePositionalTracking)
        {
            //transform.localPosition = -UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
            transform.localRotation = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye));
        }
    }
}
