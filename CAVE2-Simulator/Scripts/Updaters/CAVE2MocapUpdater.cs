using UnityEngine;
using System.Collections;

public class CAVE2MocapUpdater : MonoBehaviour {

    public int sourceID = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.localPosition = CAVE2.GetMocapPosition(sourceID);
        transform.localRotation = CAVE2.GetMocapRotation(sourceID);
    }
}
