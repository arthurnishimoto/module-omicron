using UnityEngine;
using System.Collections;

public class DestroyOnYPosition : MonoBehaviour {

	public float fallLimit = -100;

	// Update is called once per frame
	void Update () {
		if( transform.position.y < fallLimit )
			Destroy (gameObject);
	}
}
