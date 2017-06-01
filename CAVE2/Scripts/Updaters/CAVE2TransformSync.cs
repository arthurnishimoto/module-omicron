using UnityEngine;
using System.Collections;

public class CAVE2TransformSync : MonoBehaviour {

    public float updateSpeed = 3;
    float updateTimer;

    public void FixedUpdate()
    {
        if (CAVE2.IsMaster())
        {
            if (updateTimer < 0 )
            {
                CAVE2.BroadcastMessage(gameObject.name, "SyncPosition", transform.position);
                updateTimer = updateSpeed;
            }
            
            updateTimer -= Time.fixedDeltaTime;
        }
    }

    public void SyncPosition(Vector3 position)
    {
        //Debug.Log("SyncPosition on " + gameObject.name + " to " + position);
        if (!CAVE2Manager.IsMaster())
            transform.position = position;
    }
}
