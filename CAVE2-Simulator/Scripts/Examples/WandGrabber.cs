using UnityEngine;
using System.Collections;

public class WandGrabber : MonoBehaviour
{
    public int wandID = 1;
	public CAVE2.Button grabButton = CAVE2.Button.Button2;
	public LayerMask wandLayerMask = -1;

	public bool wandHit;
	public bool overGrabbable;

	ArrayList grabbedObjects;

	void Start()
	{
		grabbedObjects = new ArrayList();
	}

	// Update is called once per frame
	void Update()
	{
		//GetComponent<SphereCollider>().enabled = false; // Disable sphere collider for raycast

		// Shoot a ray from the wand
		Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
		RaycastHit hit;
			
		// Get the first collider that was hit by the ray
		wandHit = Physics.Raycast(ray, out hit, 100, wandLayerMask);
		Debug.DrawLine(ray.origin, hit.point); // Draws a line in the editor
			
		if (wandHit) // The wand is pointed at a collider
		{
			// Send a message to the hit object telling it that the wand is hovering over it
			hit.collider.gameObject.SendMessage("OnWandOver", SendMessageOptions.DontRequireReceiver);

			overGrabbable = (hit.transform.GetComponent<GrabbableObject>() != null);
			if (CAVE2Manager.GetButtonDown(wandID,grabButton))
			{
                if (hit.transform.GetComponent<GrabbableObject>())
                {
                    hit.collider.gameObject.SendMessage("OnWandGrab", transform, SendMessageOptions.DontRequireReceiver);
                    grabbedObjects.Add(hit.collider.transform);
                }
			}
				

		}
		else
		{
			overGrabbable = false;
		}
		if (CAVE2Manager.GetButtonUp(wandID,grabButton))
		{
			foreach( Transform t in grabbedObjects )
			{
                t.SendMessage("OnWandGrabRelease", SendMessageOptions.DontRequireReceiver);
			}
			grabbedObjects.Clear();
		}
		//GetComponent<SphereCollider>().enabled = true; // Enable sphere collider after raycast
	}
}
