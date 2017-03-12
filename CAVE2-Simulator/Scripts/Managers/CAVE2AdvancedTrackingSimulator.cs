using UnityEngine;
using System.Collections;

public class CAVE2AdvancedTrackingSimulator : MonoBehaviour {

    public int wandID = 1;

    public bool wandOffsetFollowsHead = true;
    public Vector3 wandPositionOffset = new Vector3(0.15f, 1.5f, 0.4f);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if( CAVE2.GetCAVE2Manager().wandMousePointerEmulation )
        {
            MouseWandPointerMode();
        }
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
}
