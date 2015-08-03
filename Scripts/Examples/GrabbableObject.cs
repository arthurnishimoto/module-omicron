using UnityEngine;
using System.Collections;

public class GrabbableObject : MonoBehaviour {

    bool usedGravity;
    RigidbodyConstraints constraints;

    void OnWandGrab(Transform grabber)
    {
        transform.parent = grabber;
        if (GetComponent<Rigidbody>())
        {
            usedGravity = GetComponent<Rigidbody>().useGravity;
            GetComponent<Rigidbody>().useGravity = false;
            constraints = GetComponent<Rigidbody>().constraints;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void OnWandGrabRelease()
    {
        transform.parent = null;
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().constraints = constraints;
            GetComponent<Rigidbody>().useGravity = usedGravity;
        }
    }
}
