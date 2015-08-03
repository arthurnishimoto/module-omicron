using UnityEngine;
using System.Collections;

public class CubeDrop : MonoBehaviour {

	public CAVE2Manager.Button dropButton;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnWandButtonDown(CAVE2Manager.Button clickButton)
	{
		if( clickButton == dropButton )
			Drop();
	}

	void Drop()
	{
		GetComponent<Rigidbody>().useGravity = true;
	}
}
