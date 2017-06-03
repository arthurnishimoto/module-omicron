using UnityEngine;
using System.Collections;

public class WandPointer : MonoBehaviour
{
    public int wandID = 1;

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

	public CAVE2.Button laserButton = CAVE2.Button.Button3;

    public bool laserAlwaysOn = false;

    // Use this for initialization
    void Start()
    {
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
        //laserParticle.GetComponent<ParticleSystemRenderer>().material.color = laserColor;
    }

	// Update is called once per frame
    void Update()
    {
        //GetComponent<SphereCollider>().enabled = false; // Disable sphere collider for raycast

		laserActivated = CAVE2.Input.GetButton(wandID,laserButton);

        if (laserAlwaysOn)
            laserActivated = true;
        laser.enabled = laserActivated;

        // Shoot a ray from the wand
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        RaycastHit hit;

        // Get the first collider that was hit by the ray
        wandHit = Physics.Raycast(ray, out hit, 100, wandLayerMask);
        Debug.DrawLine(ray.origin, hit.point); // Draws a line in the editor

        if (wandHit) // The wand is pointed at a collider
        {
            // Laser button is held down
            if (laserActivated)
            {
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
            if (wandHit && laserParticle)
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
		if( alwaysShowLaserParticle && !laserActivated && laserParticle)
		{
			laserParticle.transform.position = laserPosition;
			laserParticle.Emit(1);
		}
        //GetComponent<SphereCollider>().enabled = true; // Enable sphere collider after raycast
    }
}
