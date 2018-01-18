using UnityEngine;
using System.Collections;

public class CAVE2WandInteractor : MonoBehaviour {

    [SerializeField]
    int wandID = 1;

    [SerializeField]
    LayerMask wandLayerMask = -1;

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
            CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(playerID, wandID, CAVE2.Button.None, CAVE2.InteractionType.Pointing);

            // Send a message to the hit object telling it that the wand is hovering over it
            hit.collider.gameObject.SendMessage("OnWandPointing", playerInfo, SendMessageOptions.DontRequireReceiver);

            ProcessButtons(hit.collider.gameObject, playerInfo);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(playerID, wandID, CAVE2.Button.None, CAVE2.InteractionType.Touching);

        // Send a message to the hit object telling it that the wand is hovering over it
        collider.gameObject.SendMessage("OnWandOver", playerInfo, SendMessageOptions.DontRequireReceiver);

        ProcessButtons(collider.gameObject, playerInfo);
    }

    void ProcessButtons(GameObject interactedObject, CAVE2.WandEvent playerInfo)
    {
        foreach(CAVE2.Button currentButton in CAVE2.Button.GetValues(typeof(CAVE2.Button)))
        {
            playerInfo.button = currentButton;

             // OnWandButtonDown
            if (CAVE2Manager.GetButtonDown(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButtonDown", playerInfo, SendMessageOptions.DontRequireReceiver);
            }

            // OnWandButton
            else if (CAVE2Manager.GetButton(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButton", playerInfo, SendMessageOptions.DontRequireReceiver);
            }

            // OnWandButtonUp
            if (CAVE2Manager.GetButtonUp(wandID, currentButton))
            {
                interactedObject.SendMessage("OnWandButtonUp", playerInfo, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
