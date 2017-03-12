using UnityEngine;
using System.Collections;

public class GrabbableObject : MonoBehaviour {

    bool usedGravity;
    public RigidbodyConstraints constraints;

    FixedJoint joint;

    void OnWandGrab(Transform grabber)
    {

        if (GetComponent<Rigidbody>() && transform.parent != grabber )
        {
            usedGravity = GetComponent<Rigidbody>().useGravity;
            GetComponent<Rigidbody>().useGravity = false;
            //constraints = GetComponent<Rigidbody>().constraints;
            //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            //transform.parent = grabber;
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = grabber.GetComponent<Rigidbody>();
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;
        }
        
    }

    void OnWandGrabRelease()
    {
        //transform.parent = null;
        if (GetComponent<Rigidbody>())
        {
            //GetComponent<Rigidbody>().constraints = constraints;
            GetComponent<Rigidbody>().useGravity = usedGravity;
            Destroy(joint);
        }
    }
}
