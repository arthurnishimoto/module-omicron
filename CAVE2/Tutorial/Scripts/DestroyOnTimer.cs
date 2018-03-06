using UnityEngine;
using System.Collections;

public class DestroyOnTimer : MonoBehaviour {

    [SerializeField]
    float timeDelay = 10;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, 10);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
