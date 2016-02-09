using UnityEngine;
using System.Collections;

public class WandPointer : OmicronWandUpdater
{

    public bool laserActivated;
    float laserDistance;
    bool wandHit;
    Vector3 laserPosition;

    LineRenderer laser;
    public Material laserMaterial;
    public Color laserColor = Color.red;

    public ParticleSystem laserParticlePrefab;
    ParticleSystem laserParticle;

    public bool drawLaser = true;
	public bool alwaysShowLaserParticle = false;
    public LayerMask wandLayerMask = -1;

	public CAVE2Manager.Button laserButton;

    // Use this for initialization
    new void Start()
    {
        InitOmicron();

        // Laser line
        laser = gameObject.AddComponent<LineRenderer>();
        laser.SetWidth(0.02f, 0.02f);
        laser.useWorldSpace = false;
        laser.material = laserMaterial;
        laser.SetColors(laserColor, laserColor);
		laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laser.receiveShadows = false;

        // Laser impact glow
        laserParticle = Instantiate(laserParticlePrefab) as ParticleSystem;
    }

	// Update is called once per frame
    void Update()
    {
        GetComponent<SphereCollider>().enabled = false; // Disable sphere collider for raycast

		laserActivated = CAVE2Manager.GetButton(wandID,laserButton);
        laser.enabled = laserActivated;

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

            // If the laser button has just been pressed, tell the hit object
            if (CAVE2Manager.GetButtonDown(wandID,CAVE2Manager.Button.Button3))
            {
				hit.collider.gameObject.SendMessage("OnWandButtonDown", laserButton, SendMessageOptions.DontRequireReceiver);
            }

			// If the laser button has just been pressed, tell the hit object
			if (CAVE2Manager.GetButtonUp(wandID,CAVE2Manager.Button.Button3))
			{
				hit.collider.gameObject.SendMessage("OnWandButtonUp", laserButton, SendMessageOptions.DontRequireReceiver);
			}

            if (CAVE2Manager.GetButtonDown(wandID,CAVE2Manager.Button.Button2))
            {
				hit.collider.gameObject.SendMessage("OnWandButtonDown", CAVE2Manager.Button.Button2, SendMessageOptions.DontRequireReceiver);
			}
			if (CAVE2Manager.GetButtonUp(wandID,CAVE2Manager.Button.Button2))
			{
				hit.collider.gameObject.SendMessage("OnWandGrabRelease", SendMessageOptions.DontRequireReceiver);
			}

            if (CAVE2Manager.GetButton(wandID,CAVE2Manager.Button.Button2))
            {
                hit.collider.gameObject.SendMessage("OnWandButtonHold", CAVE2Manager.Button.Button2, SendMessageOptions.DontRequireReceiver);
            }

            // DPad Click
            if (CAVE2Manager.GetButtonDown(wandID,CAVE2Manager.Button.ButtonUp))
            {
				hit.collider.gameObject.SendMessage("OnWandButtonDown", CAVE2Manager.Button.ButtonUp, SendMessageOptions.DontRequireReceiver);
            }
            if (CAVE2Manager.GetButtonDown(wandID,CAVE2Manager.Button.ButtonDown))
            {
				hit.collider.gameObject.SendMessage("OnWandButtonDown", CAVE2Manager.Button.ButtonDown, SendMessageOptions.DontRequireReceiver);
            }
            if (CAVE2Manager.GetButtonDown(wandID,CAVE2Manager.Button.ButtonLeft))
            {
				hit.collider.gameObject.SendMessage("OnWandButtonDown", CAVE2Manager.Button.ButtonLeft, SendMessageOptions.DontRequireReceiver);
            }
            if (CAVE2Manager.GetButtonDown(wandID,CAVE2Manager.Button.ButtonRight))
            {
				hit.collider.gameObject.SendMessage("OnWandButtonDown", CAVE2Manager.Button.ButtonRight, SendMessageOptions.DontRequireReceiver);
            }

            // DPad Hold
            if (CAVE2Manager.GetButton(wandID,CAVE2Manager.Button.ButtonUp))
            {
                hit.collider.gameObject.SendMessage("OnWandButtonHold", CAVE2Manager.Button.ButtonUp, SendMessageOptions.DontRequireReceiver);
            }
            if (CAVE2Manager.GetButton(wandID,CAVE2Manager.Button.ButtonDown))
            {
                hit.collider.gameObject.SendMessage("OnWandButtonHold", CAVE2Manager.Button.ButtonDown, SendMessageOptions.DontRequireReceiver);
            }
            if (CAVE2Manager.GetButton(wandID,CAVE2Manager.Button.ButtonLeft))
            {
                hit.collider.gameObject.SendMessage("OnWandButtonHold", CAVE2Manager.Button.ButtonLeft, SendMessageOptions.DontRequireReceiver);
            }
            if (CAVE2Manager.GetButton(wandID,CAVE2Manager.Button.ButtonRight))
            {
                hit.collider.gameObject.SendMessage("OnWandButtonHold", CAVE2Manager.Button.ButtonRight, SendMessageOptions.DontRequireReceiver);
            }

            // Laser button is held down
            if (laserActivated)
            {
                // Tell hit object laser button is held down
				hit.collider.gameObject.SendMessage("OnWandButtonHold", laserButton, SendMessageOptions.DontRequireReceiver);

                Debug.DrawLine(ray.origin, hit.point);

                // Set the laser distance at the collision point
                laserDistance = hit.distance;
                laserPosition = hit.point;
            }
			laserDistance = hit.distance;
			laserPosition = hit.point;
        }
        else if (laserActivated)
        {
            // The laser button is pressed, but not pointed at valid target
            // Set laser distance far away
            laserDistance = 1000;
        }

        // Do this on all nodes
        laser.enabled = laserActivated;
        if (laserActivated && drawLaser)
        {
            if (wandHit)
            {
                laserParticle.transform.position = laserPosition;
                laserParticle.Emit(1);
            }
            laser.SetPosition(1, new Vector3(0, 0, laserDistance));
        }
        else if (!drawLaser)
        {
            laser.SetPosition(1, new Vector3(0, 0, 0));
        }
		if( alwaysShowLaserParticle && !laserActivated)
		{
			laserParticle.transform.position = laserPosition;
			laserParticle.Emit(1);
		}
        GetComponent<SphereCollider>().enabled = true; // Enable sphere collider after raycast
    }
}
