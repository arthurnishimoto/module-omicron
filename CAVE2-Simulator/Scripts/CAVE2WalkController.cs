using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2WalkController : MonoBehaviour {

    public float movementSpeed = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float forwardVector = CAVE2.GetAxis(1, CAVE2.Axis.LeftAnalogStickUD);
        float strafeVector = CAVE2.GetAxis(1, CAVE2.Axis.LeftAnalogStickLR);

        transform.Translate(forwardVector * Vector3.forward * movementSpeed * Time.deltaTime);

        transform.Translate(strafeVector * Vector3.right * movementSpeed * Time.deltaTime);
    }
}
