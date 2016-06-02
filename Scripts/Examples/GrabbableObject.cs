using UnityEngine;
using System.Collections;

public class GrabbableObject : MonoBehaviour {

    bool usedGravity;
    public RigidbodyConstraints constraints;

    void OnWandGrab(Transform grabber)
    {

        if (GetComponent<Rigidbody>() && transform.parent != grabber )
        {
            usedGravity = GetComponent<Rigidbody>().useGravity;
            GetComponent<Rigidbody>().useGravity = false;
            constraints = GetComponent<Rigidbody>().constraints;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            transform.parent = grabber;
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
