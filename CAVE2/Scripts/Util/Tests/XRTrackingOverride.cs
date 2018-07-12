using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRTrackingOverride : MonoBehaviour {

    [SerializeField]
    bool disablePositionalTracking;

	// Use this for initialization
	void Start () {
        UnityEngine.XR.InputTracking.disablePositionalTracking = disablePositionalTracking;
	}
}
