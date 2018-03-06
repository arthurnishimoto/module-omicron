using UnityEngine;
using System.Collections;

public class DestroyOnTimer : MonoBehaviour {

    [SerializeField]
    float timeDelay = 10;

	// Use this for initialization
	void Start () {
        if( timeDelay > 0 )
            Destroy(gameObject, timeDelay);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
