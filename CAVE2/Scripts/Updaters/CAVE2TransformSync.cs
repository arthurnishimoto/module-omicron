using UnityEngine;
using System.Collections;

public class CAVE2TransformSync : MonoBehaviour {

    public float updateSpeed = 3;
    float updateTimer;

    public bool syncPosition = true;
    public bool syncRotation;

    public Transform testSyncObject;

    public void FixedUpdate()
    {
        if (CAVE2.IsMaster())
        {
            if (updateTimer < 0 )
            {
                if(syncPosition)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncPosition", transform.position);
                }
                if(syncRotation)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncRotation", transform.rotation);
                }
                
                updateTimer = updateSpeed;
            }
            
            updateTimer -= Time.fixedDeltaTime;
        }

        if(testSyncObject)
        {
            transform.localPosition = testSyncObject.localPosition;
            transform.localRotation = testSyncObject.localRotation;
        }
    }

    public void SyncPosition(Vector3 position)
    {
        //Debug.Log("SyncPosition on " + gameObject.name + " to " + position);
        if (!CAVE2Manager.IsMaster())
            transform.position = position;
    }

    public void SyncRotation(Quaternion rotation)
    {
        //Debug.Log("SyncPosition on " + gameObject.name + " to " + position);
        if (!CAVE2Manager.IsMaster())
            transform.rotation = rotation;
    }
}
