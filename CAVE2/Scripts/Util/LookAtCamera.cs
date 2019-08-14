using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {

    [SerializeField]
    bool alwaysUpOrientation = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(alwaysUpOrientation)
		    transform.LookAt(Camera.main.transform, Camera.main.transform.TransformDirection(Vector3.up));
        else
            transform.LookAt(Camera.main.transform);
    }
}
