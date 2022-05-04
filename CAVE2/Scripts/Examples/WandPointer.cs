/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2022		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2022, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
 
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
    public float particleSize = 0.5f;
    ParticleSystem laserParticle;

    public bool drawLaser = true;
	public bool alwaysShowLaserParticle = false;
    public LayerMask wandLayerMask = -1;

	public CAVE2.Button laserButton = CAVE2.Button.Button3;

    public bool laserAlwaysOn = false;
    public bool laserButtonPressed;

    // Use this for initialization
    void Start()
    {
        // Laser line
        laser = gameObject.AddComponent<LineRenderer>();
#if UNITY_5_5_OR_NEWER
        laser.startWidth = 0.02f;
        laser.endWidth = 0.02f;
#else
        laser.SetWidth(0.02f, 0.02f);
#endif
        laser.useWorldSpace = false;
        laser.material = laserMaterial;
#if UNITY_5_5_OR_NEWER
        laser.startColor = laserColor;
        laser.endColor = laserColor;
#else
        laser.SetColors(laserColor, laserColor);
#endif
        laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laser.receiveShadows = false;

        // Laser impact glow
        laserParticle = Instantiate(laserParticlePrefab);
        ParticleSystem.MainModule particleMain = laserParticle.main;
        particleMain.startSize = particleSize;

        // Set particle color, but ensure color has at least a small r, g, b component to get a white glow in the middle
        particleMain.startColor = new Color(Mathf.Max(laserColor.r, 0.2f) / 2.0f, Mathf.Max(laserColor.g, 0.2f) / 2.0f, Mathf.Max(laserColor.b, 0.2f) / 2.0f);
    }

	// Update is called once per frame
    void Update()
    {
        //GetComponent<SphereCollider>().enabled = false; // Disable sphere collider for raycast

		laserButtonPressed = CAVE2.Input.GetButton(wandID,laserButton);

        bool drawLaserOverride = drawLaser;

        if (laserAlwaysOn)
        {
            laserActivated = true;

            // If wand laser is always on, pressing button will increase width
            // and show wand laser to remote clients
            if (laserButtonPressed)
            {
                laser.startWidth = 0.02f * 4;
                laser.endWidth = 0.02f * 4;

                drawLaserOverride = true;
            }
            else
            {
                laser.startWidth = 0.02f;
                laser.endWidth = 0.02f;
            }
        }
        else
            laserActivated = laserButtonPressed;

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
        if (laserActivated && drawLaserOverride)
        {
            if (wandHit && laserParticle)
            {
                laserParticle.transform.position = laserPosition;
                laserParticle.Emit(1);
            }
            laser.SetPosition(1, new Vector3(0, 0, laserDistance));
        }
        else if (!drawLaserOverride)
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
