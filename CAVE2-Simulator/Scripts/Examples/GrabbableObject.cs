using UnityEngine;
using System.Collections;

public class GrabbableObject : MonoBehaviour {

    bool usedGravity;
    public RigidbodyConstraints constraints;

    FixedJoint joint;

    public bool grabbed;
    bool wasGrabbed;

    public Queue previousPositions = new Queue();

    Collider[] grabberColliders;

    void FixedUpdate()
    {
        if( grabbed )
        {
            previousPositions.Enqueue(GetComponent<Rigidbody>().position);

            // Save only the last 5 frames of positions
            if(previousPositions.Count > 5)
            {
                previousPositions.Dequeue();
            }
        }
        else if(wasGrabbed)
        {
            Vector3 throwForce = Vector3.zero;
            for (int i = 0; i < previousPositions.Count; i++ )
            {
                Vector3 s1 = (Vector3)previousPositions.Dequeue();
                Vector3 s2 = GetComponent<Rigidbody>().position;
                
                if ( previousPositions.Count > 0 )
                    s2 = (Vector3)previousPositions.Dequeue();
                throwForce += (s2 - s1);
            }
            GetComponent<Rigidbody>().AddForce(throwForce * 10, ForceMode.Impulse);
            wasGrabbed = false;
        }
    }

    void OnWandGrab(Transform grabber)
    {
        if (GetComponent<Rigidbody>() && transform.parent != grabber )
        {
            usedGravity = GetComponent<Rigidbody>().useGravity;
            GetComponent<Rigidbody>().useGravity = false;
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = grabber.GetComponent<Rigidbody>();
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;

            // Disable collisions between grabber and collider while held
            grabberColliders = grabber.root.GetComponentsInChildren<Collider>();
            foreach (Collider c in grabberColliders)
            {
                Physics.IgnoreCollision(c, GetComponent<Collider>(), true);
            }
        }
        grabbed = true;
    }

    void OnWandGrabRelease()
    {
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().useGravity = usedGravity;
            Destroy(joint);
        }

        // Re-enable collisions between grabber and collider after released
        foreach (Collider c in grabberColliders)
        {
            Physics.IgnoreCollision(c, GetComponent<Collider>(), false);
        }

        grabbed = false;
        wasGrabbed = true;
    }
}
