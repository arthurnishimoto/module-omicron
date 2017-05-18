using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWalkController : MonoBehaviour {

    public int wandID = 1;
    public CAVE2.Axis forwardInputAxis = CAVE2.Axis.LeftAnalogStickUD;
    public CAVE2.Axis strafeInputAxis = CAVE2.Axis.LeftAnalogStickLR;

    public float movementSpeed = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float forwardVector = CAVE2.GetAxis(wandID, forwardInputAxis);
        float strafeVector = CAVE2.GetAxis(wandID, strafeInputAxis);

        transform.Translate(forwardVector * Vector3.forward * movementSpeed * Time.deltaTime);
        transform.Translate(strafeVector * Vector3.right * movementSpeed * Time.deltaTime);
    }
}
