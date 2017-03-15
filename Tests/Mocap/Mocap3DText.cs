using UnityEngine;
using System.Collections;

public class Mocap3DText : MonoBehaviour {

    OmicronMocapObject mocapObject;

	// Use this for initialization
	void Start () {
        mocapObject = GetComponentInParent<OmicronMocapObject>();
    }
	
	// Update is called once per frame
	void Update () {
	    if(mocapObject)
        {
            GetComponent<TextMesh>().text = mocapObject.sourceID.ToString();
            transform.LookAt(Camera.main.transform.position);
            transform.Rotate(Vector3.up, 180);
        }
	}
}
