using UnityEngine;
using System.Collections;

public class OmicronCameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().AddCameraController(gameObject);
	}
}
