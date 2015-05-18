using UnityEngine;
using System.Collections;

public class GrabbableObject : MonoBehaviour {

    bool usedGravity;
    RigidbodyConstraints constraints;

    void OnWandGrab(Transform grabber)
    {
        transform.parent = grabber;
        if (rigidbody)
        {
            usedGravity = rigidbody.useGravity;
            rigidbody.useGravity = false;
            constraints = rigidbody.constraints;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        }
    }

    void OnWandGrabRelease()
    {
        transform.parent = null;
        if (rigidbody)
        {
            rigidbody.constraints = constraints;
            rigidbody.useGravity = usedGravity;
        }
    }
}
