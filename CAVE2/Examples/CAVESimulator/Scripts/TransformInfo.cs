using UnityEngine;
using System.Collections;

public class TransformInfo : MonoBehaviour {

    public Vector3 position;
    public Vector3 localPosition;
	
	// Update is called once per frame
	void Update () {
        position = transform.position;
        localPosition = transform.localPosition;
    }
}
