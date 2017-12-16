using UnityEngine;
using System.Collections;

public class MocapAdvUpdater : MonoBehaviour {

    public int sourceID = 1;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (CAVE2.IsMaster())
        {
            //transform.localPosition = CAVE2.GetMocapPosition(sourceID);
            Vector3 rotation = CAVE2.GetMocapRotation(sourceID).eulerAngles;
            rotation.y += 163.351f - 90;
            transform.localEulerAngles = new Vector3(0, 0, rotation.y);
        }
    }
}
