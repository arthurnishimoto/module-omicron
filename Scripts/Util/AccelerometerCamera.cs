using UnityEngine;
using System.Collections;

public class AccelerometerCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Input.acceleration.x, Input.acceleration.y, 0);
	}
}
