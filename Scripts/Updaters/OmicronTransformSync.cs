using UnityEngine;
using System.Collections;

public class OmicronTransformSync : MonoBehaviour {

    public enum TransformSyncMode { None, SyncTransform, SyncRigidBody2D, SyncRigidBody3D };
    public enum TransformRotationSyncMode { None, X, Y, Z, XYZ, XY, XZ, YZ };

    public TransformRotationSyncMode rotationAxis = TransformRotationSyncMode.Y;

    public void FixedUpdate()
    {
        if (CAVE2Manager.IsMaster())
        {
            CAVE2Manager.BroadcastMessage(gameObject.name, "SyncPosition", transform.position);

            if(rotationAxis == TransformRotationSyncMode.X)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationX", transform.localEulerAngles.x);
            }
            else if (rotationAxis == TransformRotationSyncMode.Y)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationY", transform.localEulerAngles.y);
            }
            else if (rotationAxis == TransformRotationSyncMode.Z)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationZ", transform.localEulerAngles.z);
            }
            else if (rotationAxis == TransformRotationSyncMode.XYZ)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationXYZ", transform.rotation);
            }
            else if (rotationAxis == TransformRotationSyncMode.XY)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationXY", new Vector2(transform.localEulerAngles.x, transform.localEulerAngles.y));
            }
            else if (rotationAxis == TransformRotationSyncMode.XZ)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationXZ", new Vector2(transform.localEulerAngles.x, transform.localEulerAngles.z));
            }
            else if (rotationAxis == TransformRotationSyncMode.YZ)
            {
                CAVE2Manager.BroadcastMessage(gameObject.name, "SyncRotationYZ", new Vector2(transform.localEulerAngles.y, transform.localEulerAngles.z));
            }
        }
    }

    public void SyncPosition(Vector3 position)
    {
        if (!CAVE2Manager.IsMaster())
            transform.position = position;
    }

    public void SyncRotation(Quaternion rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localRotation = rot;
    }

    public void SyncRotationX(float rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localEulerAngles = new Vector3(rot, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    public void SyncRotationY(float rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, rot, transform.localEulerAngles.z);
    }

    public void SyncRotationZ(float rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rot);
    }

    public void SyncRotationXY(Vector2 rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localEulerAngles = new Vector3(rot.x, rot.y, transform.localEulerAngles.z);
    }

    public void SyncRotationXZ(Vector2 rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localEulerAngles = new Vector3(rot.x, transform.localEulerAngles.y, rot.y);
    }

    public void SyncRotationYZ(Vector2 rot)
    {
        if (!CAVE2Manager.IsMaster())
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, rot.x, rot.y);
    }
}
