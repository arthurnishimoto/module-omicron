/**************************************************************************************************
 * StereoscopicCamera.cs
 * 
 * Generates a stereoscopic camera from a main camera/
 *-------------------------------------------------------------------------------------------------
 * Copyright 2018   		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2018, Electronic Visualization Laboratory, University of Illinois at Chicago
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

public class StereoscopicCamera : MonoBehaviour {

    [SerializeField]
    bool generateCameras = false;

    [SerializeField]
    float eyeSeparation;

    GameObject leftEye;
    GameObject rightEye;

    [SerializeField]
    Material stereoscopicMaterial;

    RenderTexture leftTexture;
    RenderTexture rightTexture;

    // Use this for initialization
    void Start () {
        if (generateCameras && transform.parent.GetComponent<StereoscopicCamera>() == null)
        {
            SetupStereoCameras();

            if (transform.parent.name == "rightEye")
            {
                Destroy(gameObject);
            }

            GetComponent<Camera>().targetTexture = null;
        }
        else
        {
            Destroy(GetComponent<StereoscopicCamera>());
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void SetupStereoCameras()
    {
        // Create the gameobjects for the eyes
        leftEye = Instantiate(gameObject, transform) as GameObject;
        leftEye.name = "leftEye";

        // Cloning the leftEye instead of the source since cloning
        // the source now will add another left eye
        rightEye = Instantiate(leftEye, transform) as GameObject;
        rightEye.name = "rightEye";

        // Set the eye separation
        leftEye.transform.localPosition = Vector3.left * eyeSeparation / 2.0f;
        rightEye.transform.localPosition = Vector3.right * eyeSeparation / 2.0f;

        // Cleanup unnecessary duplicated components
        Destroy(leftEye.GetComponent<AudioListener>());
        Destroy(rightEye.GetComponent<AudioListener>());

        // Setup stereo materials and render textures
        leftTexture = stereoscopicMaterial.GetTexture("_LeftTex") as RenderTexture;
        rightTexture = stereoscopicMaterial.GetTexture("_RightTex") as RenderTexture;

        leftEye.GetComponent<Camera>().targetTexture = leftTexture;
        rightEye.GetComponent<Camera>().targetTexture = rightTexture;

        // Temp
        leftEye.GetComponent<GeneralizedPerspectiveProjection>().UseProjection(false);
        rightEye.GetComponent<GeneralizedPerspectiveProjection>().UseProjection(false);
    }
}
