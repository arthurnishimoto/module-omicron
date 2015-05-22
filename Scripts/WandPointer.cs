using UnityEngine;
using System.Collections;

public class WandPointer : OmicronWandUpdater {
	
    public CAVE2Manager.Button pointerButton = CAVE2Manager.Button.Button3;
    public CAVE2Manager.Button grabButton = CAVE2Manager.Button.Button2;

	bool laserActivated;
	float laserDistance;
	bool wandHit;
	Vector3 laserPosition;

	LineRenderer laser;
	public Material laserMaterial;
	public Color laserColor = Color.red;

	public ParticleSystem laserParticlePrefab;
	ParticleSystem laserParticle;

	public bool drawLaser = true;
    public bool enableGrab = true;
    ArrayList grabbedObjects = new ArrayList();

	public Vector3 forwardVector = Vector3.forward;

	// Use this for initialization
	new void Start () {
		// Laser line
		laser = gameObject.AddComponent<LineRenderer>();
		laser.SetWidth( 0.02f, 0.02f );
		laser.useWorldSpace = false;
		laser.material = laserMaterial;
		laser.SetColors( laserColor, laserColor );
		laser.castShadows = false;
		laser.receiveShadows = false;

		// Laser impact glow
		laserParticle = Instantiate(laserParticlePrefab) as ParticleSystem;
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<SphereCollider>().enabled = false; // Disable sphere collider for raycast

		// Checking inputs should only be done on master node
		laserActivated = CAVE2Manager.GetButton(wandID, pointerButton);
		laser.enabled = laserActivated;

		// Shoot a ray from the wand
		Ray ray = new Ray( transform.position, transform.TransformDirection(forwardVector) );
		RaycastHit hit;

		// Get the first collider that was hit by the ray
		wandHit = Physics.Raycast(ray, out hit, 100);
		Debug.DrawLine(ray.origin, hit.point); // Draws a line in the editor

		if( wandHit ) // The wand is pointed at a collider
		{
			// Send a message to the hit object telling it that the wand is hovering over it
			hit.collider.gameObject.SendMessage("OnWandOver", SendMessageOptions.DontRequireReceiver );
			Debug.DrawRay(hit.point, hit.normal * 0.1f, Color.yellow, 1, false); // Draws a line in the editor

			// If the laser button has just been pressed, tell the hit object
            if( CAVE2Manager.GetButtonDown(wandID, pointerButton) )
			{
                hit.collider.gameObject.SendMessage("OnWandButtonClick", pointerButton, SendMessageOptions.DontRequireReceiver );
			}

			// Laser button is held down
            if( CAVE2Manager.GetButton(wandID, pointerButton) )
			{
				// Tell hit object laser button is held down
                hit.collider.gameObject.SendMessage("OnWandButtonHold", pointerButton, SendMessageOptions.DontRequireReceiver );
				Debug.DrawLine(ray.origin, hit.point);

				// Set the laser distance at the collision point
				laserDistance = hit.distance;
				laserPosition = hit.point;
			}

            if( enableGrab && CAVE2Manager.GetButtonDown(wandID, grabButton) )
            {
                GrabbableObject grabbable = hit.collider.GetComponent<GrabbableObject>();
                if( grabbable != null )
                {
                    hit.collider.gameObject.SendMessage("OnWandGrab", transform, SendMessageOptions.DontRequireReceiver );
                    grabbedObjects.Add(grabbable);
                }
            }
		}
		else if( laserActivated )
		{
			// The laser button is pressed, but not pointed at valid target
			// Set laser distance far away
			laserDistance = 1000;
		}

        if( enableGrab && CAVE2Manager.GetButtonUp(wandID, grabButton) )
        {
            foreach( GrabbableObject g in grabbedObjects )
            {
                g.SendMessage("OnWandGrabRelease", SendMessageOptions.DontRequireReceiver );
            }
            grabbedObjects.Clear();
        }

		// Do this on all nodes
		laser.enabled = laserActivated;
		if( laserActivated && drawLaser )
		{
			if (wandHit)
			{
				laserParticle.transform.position = laserPosition;
				laserParticle.Emit(1);
			}
			laser.SetPosition( 1, forwardVector * laserDistance );
		}
		else
		{
			laser.SetPosition( 1, Vector3.zero );
		}

		GetComponent<SphereCollider>().enabled = true; // Enable sphere collider after raycast
	}
}
