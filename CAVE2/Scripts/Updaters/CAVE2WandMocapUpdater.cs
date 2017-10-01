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

            // If position and rotation are zero, wand is not tracking, disable drawing and physics
            if (transform.localPosition == Vector3.zero && transform.localRotation == Quaternion.identity && virtualWand.gameObject.activeSelf)
            {
                virtualWand.gameObject.SetActive(false);
            }
            else if (transform.localPosition != Vector3.zero && transform.localRotation != Quaternion.identity && !virtualWand.gameObject.activeSelf)
            {
                virtualWand.gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualWand == null || virtualWand.GetComponent<Rigidbody>() == null)
        {
            transform.localPosition = CAVE2.Input.GetWandPosition(wandID);
            transform.localRotation = CAVE2.Input.GetWandRotation(wandID);
        }
    }
}
