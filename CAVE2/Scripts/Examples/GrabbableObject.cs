using UnityEngine;
using System.Collections;

public class GrabbableObject : CAVE2Interactable {

    [SerializeField]
    CAVE2.Button grabButton = CAVE2.Button.Button3;

    [SerializeField]
    CAVE2.InteractionType grabStyle = CAVE2.InteractionType.Any;

    bool usedGravity;

    [SerializeField]
    RigidbodyConstraints constraints;

    FixedJoint joint;

    [SerializeField]
    bool grabbed;

    bool wasGrabbed;

    Queue previousPositions = new Queue();

    Collider[] grabberColliders;

    int grabbingWandID;

    void Update()
    {
        UpdateWandOverTimer();

        if( CAVE2.Input.GetButtonUp(grabbingWandID, grabButton) && grabbed )
        {
            OnWandGrabRelease();
        }
    }
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

    new void OnWandButtonDown(CAVE2.WandEvent evt)
    {
        if( evt.button == grabButton && (evt.interactionType == grabStyle || grabStyle == CAVE2.InteractionType.Any))
        {
            OnWandGrab(CAVE2.GetWandObject(evt.wandID).transform);
            grabbingWandID = evt.wandID;
        }
    }

    void OnWandGrab(Transform grabber)
    {
        if (GetComponent<Rigidbody>() && transform.parent != grabber )
        {
            usedGravity = GetComponent<Rigidbody>().useGravity;
            GetComponent<Rigidbody>().useGravity = false;
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = grabber.GetComponentInChildren<Rigidbody>();
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
