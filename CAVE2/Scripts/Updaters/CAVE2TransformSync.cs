using UnityEngine;
using System.Collections;

public class CAVE2TransformSync : MonoBehaviour {

    enum UpdateMode { Update, Fixed, Late};

    [SerializeField]
    UpdateMode updateMode = UpdateMode.Fixed;

    public float updateSpeed = 3;
    float updateTimer;

    public bool syncPosition = true;
    public bool syncRotation;

    public Transform testSyncObject;

    Vector3 nextPosition;
    Quaternion nextRotation;

    public void Update()
    {
        if (updateMode == UpdateMode.Update)
            UpdateSync();
    }
    public void FixedUpdate()
    {
        if (updateMode == UpdateMode.Fixed)
            UpdateSync();
    }

    public void LateUpdate()
    {
        if (updateMode == UpdateMode.Late)
            UpdateSync();
    }

    void UpdateSync()
    {
        if (CAVE2.IsMaster())
        {
            if (updateTimer < 0)
            {
                if (syncPosition)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncPosition", transform.position.x, transform.position.y, transform.position.z);
                }
                if (syncRotation)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SyncRotation", transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                }

                updateTimer = updateSpeed;
            }

            updateTimer -= Time.fixedDeltaTime;
        }
        else
        {
            transform.position = nextPosition;
            transform.rotation = nextRotation;
        }

        if (testSyncObject)
        {
            transform.localPosition = testSyncObject.localPosition;
            transform.localRotation = testSyncObject.localRotation;
        }
    }

    public void SyncPosition(Vector3 position)
    {
        nextPosition = position;
    }

    public void SyncPosition(object[] data)
    {
        SyncPosition(new Vector3((float)data[0], (float)data[1], (float)data[2]));
    }

    public void SyncRotation(Quaternion rotation)
    {
        nextRotation = rotation;
    }

    public void SyncRotation(object[] data)
    {
        SyncRotation(new Quaternion((float)data[0], (float)data[1], (float)data[2], (float)data[3]));
    }
}
