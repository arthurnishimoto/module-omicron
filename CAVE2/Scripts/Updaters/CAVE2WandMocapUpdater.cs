using UnityEngine;
using System.Collections;

public class CAVE2WandMocapUpdater : MonoBehaviour
{
    public int wandID = 1;

    // The wand's center position is likely not the center of the physical wand,
    // but the center of the tracking markers.
    // The WandUpdater will handle the offset between the wand center and the tracking
    // center so that the virtual and physical wand align
    public Transform virtualWand;

    Joint wandJoint;

    void Start()
    {
        if(virtualWand && wandID == 1)
        {
            virtualWand.localPosition = CAVE2.Input.wand1TrackingOffset;
        }

        // Register this gameobject as wand
        CAVE2.RegisterWandObject(wandID, gameObject);
    }

    void FixedUpdate()
    {
        if (virtualWand && virtualWand.GetComponent<Rigidbody>())
        {
            transform.localPosition = CAVE2Manager.GetWandPosition(wandID);
            transform.localRotation = CAVE2Manager.GetWandRotation(wandID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualWand.GetComponent<Rigidbody>() && wandJoint == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            wandJoint = virtualWand.gameObject.AddComponent<FixedJoint>();
            wandJoint.connectedBody = rb;
        }
        else if (virtualWand == null || virtualWand.GetComponent<Rigidbody>() == null)
        {
            transform.localPosition = CAVE2.Input.GetWandPosition(wandID);
            transform.localRotation = CAVE2.Input.GetWandRotation(wandID);
        }
    }
}
