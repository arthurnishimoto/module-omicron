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
    Material stereoscopicMaterial;

    [SerializeField]
    bool generateCameras = false;

    [SerializeField]
    Vector2 outputResolution = new Vector2(1366, 768);

    [SerializeField]
    float eyeSeparation = 0.065f;

    GameObject leftEye;
    GameObject rightEye;

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
        leftEye.transform.localPosition = Vector3.left * eyeSeparation / 2.0f;
        rightEye.transform.localPosition = Vector3.right * eyeSeparation / 2.0f;

        if (GetComponent<GeneralizedPerspectiveProjection>())
        {
            leftEye.GetComponent<GeneralizedPerspectiveProjection>().SetOffset(leftEye.transform.localPosition);
            rightEye.GetComponent<GeneralizedPerspectiveProjection>().SetOffset(rightEye.transform.localPosition);
        }
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

        // If this object has a GeneralizedPerspectiveProjection
        // Set head position to eyes
        if (GetComponent<GeneralizedPerspectiveProjection>())
        {
            // Disable head offset since eyes since we're calculating that above to include eye separation
            leftEye.GetComponent<GeneralizedPerspectiveProjection>().DisablePosition();
            rightEye.GetComponent<GeneralizedPerspectiveProjection>().DisablePosition();
        }

        // Set the eye separation
        leftEye.transform.localPosition = Vector3.left * eyeSeparation / 2.0f;
        rightEye.transform.localPosition = Vector3.right * eyeSeparation / 2.0f;

        // Cleanup unnecessary duplicated components
        Destroy(leftEye.GetComponent<AudioListener>());
        Destroy(rightEye.GetComponent<AudioListener>());

        // Setup stereo materials and render textures
        leftTexture = new RenderTexture((int)outputResolution.x, (int)outputResolution.y, 24);
        rightTexture = new RenderTexture((int)outputResolution.x, (int)outputResolution.y, 24);

        stereoscopicMaterial.SetTexture("_LeftTex", leftTexture);
        stereoscopicMaterial.SetTexture("_RightTex", rightTexture);

        stereoscopicMaterial.SetFloat("_RenderWidth", outputResolution.x);
        stereoscopicMaterial.SetFloat("_RenderHeight", outputResolution.y);

        // Set render textures to match output screen resolution
        leftTexture.width = (int)outputResolution.x;
        leftTexture.height = (int)outputResolution.y;

        rightTexture.width = (int)outputResolution.x;
        rightTexture.height = (int)outputResolution.y;

        leftEye.GetComponent<Camera>().targetTexture = leftTexture;
        rightEye.GetComponent<Camera>().targetTexture = rightTexture;        
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Copy the source Render Texture to the destination,
        // applying the material along the way.
        Graphics.Blit(src, dest, stereoscopicMaterial);
    }
}
