using UnityEngine;
using System.Collections;

public class CAVE2AdvancedTrackingSimulator : MonoBehaviour {

    public int headID = 1;
    public float headMovementSpeed = 1;
    public float headRotationSpeed = 25;
    public int wandID = 1;

    public bool wandOffsetFollowsHead = true;
    public Vector3 wandPositionOffset = new Vector3(0.15f, 1.5f, 0.4f);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	    if( CAVE2.GetCAVE2Manager().wandMousePointerEmulation )
        {
            MouseWandPointerMode();
        }

        KeyboardHeadTracking();
	}

    void MouseWandPointerMode()
    {
        GameObject wandObject = CAVE2.GetWandObject(wandID);

        if (wandObject)
        {
            if (wandObject.GetComponent<Rigidbody>())
            {
                wandObject.GetComponent<Rigidbody>().MovePosition(CAVE2.Input.GetWandPosition(wandID));
            }
            else
            {
                wandObject.transform.localPosition = CAVE2.Input.GetWandPosition(wandID);
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
            wandObject.transform.LookAt(ray.GetPoint(1000));
            // Update the wandState rotation (opposite of normal since this object is determining the rotation)
            CAVE2.GetCAVE2Manager().simulatorWandRotation = wandObject.transform.eulerAngles;
        }
    }

    void KeyboardHeadTracking()
    {
        Vector3 headPosition = CAVE2.GetCAVE2Manager().simulatorHeadPosition;
        Vector3 headRotation = CAVE2.GetCAVE2Manager().simulatorHeadRotation;

        Vector3 wandPosition = CAVE2.GetCAVE2Manager().simulatorWandPosition;
        Vector3 wandRotation = CAVE2.GetCAVE2Manager().simulatorWandRotation;

        Vector3 translation = Vector3.zero;

        if ( Input.GetKey(KeyCode.I))
        {
            translation.z += headMovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.K))
        {
            translation.z -= headMovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.J))
        {
            translation.x -= headMovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.L))
        {
            translation.x += headMovementSpeed * Time.fixedDeltaTime;
        }

        if (Input.GetKey(KeyCode.H))
        {
            translation.y += headMovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.N))
        {
            if(headPosition.y > 0.1f)
                translation.y -= headMovementSpeed * Time.fixedDeltaTime;
        }

        if (Input.GetKey(KeyCode.U))
        {
            headRotation.y -= headRotationSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.O))
        {
            headRotation.y += headRotationSpeed * Time.fixedDeltaTime;
        }

        headPosition.z += translation.z * Mathf.Cos(Mathf.Deg2Rad * headRotation.y);
        headPosition.x += translation.z * Mathf.Sin(Mathf.Deg2Rad * headRotation.y);

        headPosition.z += translation.x * Mathf.Cos(Mathf.Deg2Rad * (90 + headRotation.y));
        headPosition.x += translation.x * Mathf.Sin(Mathf.Deg2Rad * (90 + headRotation.y));

        headPosition.y += translation.y;

        CAVE2.GetCAVE2Manager().simulatorHeadPosition = headPosition;
        CAVE2.GetCAVE2Manager().simulatorHeadRotation = headRotation;

        CAVE2.GetCAVE2Manager().simulatorWandPosition = wandPositionOffset + new Vector3(headPosition.x, 0, headPosition.z);
    }
}
