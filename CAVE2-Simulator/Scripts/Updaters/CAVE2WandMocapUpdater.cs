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
    }

    void FixedUpdate()
    {
        if (!CAVE2Manager.GetCAVE2Manager().wandMousePointerEmulation && virtualWand && virtualWand.GetComponent<Rigidbody>())
        {
            transform.localPosition = CAVE2Manager.GetWandPosition(wandID);
            transform.localRotation = CAVE2Manager.GetWandRotation(wandID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!CAVE2.GetCAVE2Manager().wandMousePointerEmulation)
        {
            if (virtualWand && !CAVE2.IsSimulatorMode())
            {
                //virtualWand.transform.localPosition = CAVE2Manager.GetWandTrackingOffset(wandID);
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
                    transform.localPosition = CAVE2Manager.GetWandPosition(wandID);
                    transform.localRotation = CAVE2Manager.GetWandRotation(wandID);
                }
            }

        }
        else if (CAVE2.GetCAVE2Manager().wandEmulationMode == CAVE2Manager.TrackerEmulationMode.Pointer) // Mouse pointer mode
        {
            if (GetComponent<Rigidbody>())
            {
                GetComponent<Rigidbody>().MovePosition(CAVE2Manager.GetWandPosition(wandID));
            }
            else
            {
                transform.localPosition = CAVE2Manager.GetWandPosition(wandID);
            }

            // Mouse pointer ray controls rotation
            Vector2 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y);

            // Ray extending from main camera into screen from touch point
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                //transform.LookAt( hit.point );
            }
            else
            {
                //transform.LookAt( ray.GetPoint(1000) );
            }
            transform.LookAt(ray.GetPoint(1000));
            // Update the wandState rotation (opposite of normal since this object is determining the rotation)
            CAVE2.GetCAVE2Manager().simulatorWandRotation = transform.eulerAngles;
        }
        else if (CAVE2.GetCAVE2Manager().wandEmulationMode == CAVE2Manager.TrackerEmulationMode.TranslateVertical) // Mouse pointer mode
        {
            // Translate wand based on mouse position
            Vector3 mouseDeltaPos = CAVE2.GetCAVE2Manager().GetMouseDeltaPos() * Time.deltaTime * 0.05f;
            transform.localPosition += mouseDeltaPos;
            CAVE2.GetCAVE2Manager().simulatorWandPosition = transform.localPosition;
        }
        else if (CAVE2.GetCAVE2Manager().wandEmulationMode == CAVE2Manager.TrackerEmulationMode.TranslateForward) // Wand mouse mode
        {
            // Translate wand based on mouse position
            Vector3 mouseDeltaPos = CAVE2.GetCAVE2Manager().GetMouseDeltaPos() * Time.deltaTime * 0.05f;
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 2.0f;
            transform.localPosition += new Vector3(mouseDeltaPos.x, mouseScroll, mouseDeltaPos.y);
            CAVE2.GetCAVE2Manager().simulatorWandPosition = transform.localPosition;
        }
        else if (CAVE2.GetCAVE2Manager().wandEmulationMode == CAVE2Manager.TrackerEmulationMode.RotatePitchYaw) // Wand mouse mode
        {
            // Translate wand based on mouse position
            Vector3 mouseDeltaPos = CAVE2.GetCAVE2Manager().GetMouseDeltaPos();
            transform.Rotate(new Vector3(-mouseDeltaPos.y, mouseDeltaPos.x, 0));
            CAVE2.GetCAVE2Manager().simulatorWandPosition = transform.eulerAngles;
        }
        else if (CAVE2.GetCAVE2Manager().wandEmulationMode == CAVE2Manager.TrackerEmulationMode.RotateRoll) // Wand mouse mode
        {
            // Translate wand based on mouse position
            Vector3 mouseDeltaPos = CAVE2.GetCAVE2Manager().GetMouseDeltaPos();
            transform.Rotate(new Vector3(0, 0, -mouseDeltaPos.x));
            CAVE2.GetCAVE2Manager().simulatorWandPosition = transform.eulerAngles;
        }
    }
}
