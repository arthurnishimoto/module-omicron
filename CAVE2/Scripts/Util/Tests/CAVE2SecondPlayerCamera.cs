using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Crazy transform hack for setting/offsetting CAVE2 camera
public class CAVE2SecondPlayerCamera : MonoBehaviour {

    [SerializeField]
    Transform mainPlayerHead;

    [SerializeField]
    Transform secondPlayerHead;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 camPos = mainPlayerHead.localPosition;
        Vector3 player2HeadPos = secondPlayerHead.localPosition;

        // Re-center camera
        transform.localPosition = new Vector3(-camPos.x, -camPos.y, -camPos.z);

        // Apply second head offset
        transform.localPosition += player2HeadPos;

    }
}
