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
                    CAVE2.BroadcastMessage(gameObject.name, "SyncPosition", transform.position.x, transform.position.y, transform.position.z);
                }
                if(syncRotation)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncRotation", transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
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

    public void SyncPosition(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));
    }

    public void SyncRotation(Quaternion rotation)
    {
        //Debug.Log("SyncPosition on " + gameObject.name + " to " + position);
        if (!CAVE2Manager.IsMaster())
            transform.rotation = rotation;
    }

    public void SyncRotation(object[] data)
    {
        SyncRotation(new Quaternion((float)data[0], (float)data[1], (float)data[2], (float)data[3]));
    }
}
