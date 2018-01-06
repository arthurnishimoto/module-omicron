using UnityEngine;
using System.Collections;

public class CAVE2WandInteractor : MonoBehaviour {

    public int wandID = 1;
    public LayerMask wandLayerMask = -1;

    CAVE2PlayerIdentity playerID;

    // Use this for initialization
    void Start () {
        playerID = GetComponentInParent<CAVE2PlayerIdentity>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 raySource = transform.position;

        // Helper override to set wand pointer to mouse cursor
        if( CAVE2.Input.GetComponent<CAVE2AdvancedTrackingSimulator>().wandUsesHeadPosition )
        {
            raySource = transform.parent.position;
        }

        // Shoot a ray from the wand
        Ray ray = new Ray(raySource, transform.TransformDirection(Vector3.forward));
        RaycastHit hit;

        // Get the first collider that was hit by the ray
        bool wandHit = Physics.Raycast(ray, out hit, 100, wandLayerMask);
        Debug.DrawLine(ray.origin, hit.point); // Draws a line in the editor

        if (wandHit) // The wand is pointed at a collider
        {
            CAVE2.ButtonInfo playerInfo = new CAVE2.ButtonInfo(playerID, wandID, CAVE2.Button.None);

            // Send a message to the hit object telling it that the wand is hovering over it
            hit.collider.gameObject.SendMessage("OnWandOver", playerInfo, SendMessageOptions.DontRequireReceiver);

            foreach (CAVE2.Button currentButton in CAVE2.Button.GetValues(typeof(CAVE2.Button)))
            {
                //object[] playerInfo = new object[] { playerID, wandID, currentButton };
                playerInfo = new CAVE2.ButtonInfo(playerID, wandID, currentButton);

                // OnWandButtonDown
                if (CAVE2Manager.GetButtonDown(wandID, currentButton))
                {
                    hit.collider.gameObject.SendMessage("OnWandButtonDown", playerInfo, SendMessageOptions.DontRequireReceiver);

                    // Legacy Support
                    //hit.collider.gameObject.SendMessage("OnWandButtonDown", currentButton, SendMessageOptions.DontRequireReceiver);
                }

                // OnWandButton
                else if (CAVE2Manager.GetButton(wandID, currentButton))
                {
                    hit.collider.gameObject.SendMessage("OnWandButton", playerInfo, SendMessageOptions.DontRequireReceiver);

                    // Legacy Support
                    //hit.collider.gameObject.SendMessage("OnWandButtonDown", currentButton, SendMessageOptions.DontRequireReceiver);
                }
                
                // OnWandButtonUp
                if (CAVE2Manager.GetButtonDown(wandID, currentButton))
                {
                    hit.collider.gameObject.SendMessage("OnWandButtonUp", playerInfo, SendMessageOptions.DontRequireReceiver);

                    // Legacy Support
                    //hit.collider.gameObject.SendMessage("OnWandButtonDown", currentButton, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
