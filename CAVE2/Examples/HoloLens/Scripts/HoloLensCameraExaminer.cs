using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloLensCameraExaminer : MonoBehaviour {

    [SerializeField]
    bool printCameraMatrix;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(printCameraMatrix)
        {
            Debug.Log(GetComponent<Camera>().GetStereoViewMatrix(Camera.StereoscopicEye.Left));
            Debug.Log(GetComponent<Camera>().GetStereoViewMatrix(Camera.StereoscopicEye.Right));
            printCameraMatrix = false;
        }
	}
}
