using UnityEngine;
using System.Collections;

public class CAVE2MocapUpdater : MonoBehaviour {

    public int sourceID = 1;

    // Offset to tracking data (ex. object pivot vs tracking marker center)
    [SerializeField] Vector3 posOffset;
    [SerializeField] Vector3 rotOffset;

    [SerializeField]
    bool useLateUpdate;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!useLateUpdate)
        {
            transform.localPosition = CAVE2.GetMocapPosition(sourceID) + posOffset;
            transform.localRotation = CAVE2.GetMocapRotation(sourceID);
            transform.Rotate(rotOffset);
        }
    }

    void LateUpdate()
    {
        if (useLateUpdate)
        {
            transform.localPosition = CAVE2.GetMocapPosition(sourceID) + posOffset;
            transform.localRotation = CAVE2.GetMocapRotation(sourceID);
            transform.Rotate(rotOffset);
        }
    }
}
