using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEyeLayerFilter : MonoBehaviour {

    [SerializeField]
    int leftEyeLayer;

    [SerializeField]
    int rightEyeLayer;

    [SerializeField]
    int cameraLayer;


    // Use this for initialization
    void Start () {
        leftEyeLayer = 1 << LayerMask.NameToLayer("LeftEye");
        rightEyeLayer = 1 << LayerMask.NameToLayer("RightEye");

        cameraLayer = Camera.main.cullingMask;
    }
	
	// Update is called once per frame
	void LateUpdate() {
        Camera[] cameras = FindObjectsOfType<Camera>();

        // Found stereo pair
        if(cameras.Length == 2)
        {
            // Use original culling mask minus other eye layer
            cameras[1].cullingMask = cameraLayer - rightEyeLayer;   // Left
            cameras[0].cullingMask = cameraLayer - leftEyeLayer;    // Right
        }
    }
}
