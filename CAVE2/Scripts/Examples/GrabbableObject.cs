using UnityEngine;
using System.Collections;

public class GrabbableObject : CAVE2Interactable {

    [SerializeField]
    CAVE2.Button grabButton = CAVE2.Button.Button7;

    bool usedGravity;
    public RigidbodyConstraints constraints;

    FixedJoint joint;

    public bool grabbed;
    bool wasGrabbed;

    public Queue previousPositions = new Queue();

    Collider[] grabberColliders;

    int grabbingWandID;

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

        // Case where release button was pressed, but wand may not be directly touching object
        if( CAVE2.GetButtonUp(grabbingWandID, grabButton) )
        {
            ReleaseObject();
        }
    }

    new void OnWandButtonDown(CAVE2.WandEvent evt)
    {
        if( evt.button == grabButton && !grabbed)
        {
            GameObject grabber = CAVE2.GetWandObject(evt.wandID);
            Rigidbody grabberRB = grabber.GetComponentInChildren<Rigidbody>();
            if(grabberRB != null)
            {
                usedGravity = GetComponent<Rigidbody>().useGravity;
                GetComponent<Rigidbody>().useGravity = false;

                joint = gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = grabberRB;
                joint.breakForce = float.PositiveInfinity;
                joint.breakTorque = float.PositiveInfinity;

                grabbed = true;
                grabbingWandID = evt.wandID;
            }
        }
        /*
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
        
        */
    }

    new void OnWandButtonUp(CAVE2.WandEvent evt)
    {
        if (evt.button == grabButton && grabbed)
        {
            ReleaseObject();
        }
    }

    void ReleaseObject()
    {
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().useGravity = usedGravity;
            Destroy(joint);
        }
        /*
        // Re-enable collisions between grabber and collider after released
        foreach (Collider c in grabberColliders)
        {
            Physics.IgnoreCollision(c, GetComponent<Collider>(), false);
        }
        */
        grabbed = false;
        wasGrabbed = true;
    }
}
