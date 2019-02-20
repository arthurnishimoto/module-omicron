/**************************************************************************************************
 * CAVE2Display.cs
 *
 * Turns the attached object into a virtual reality display. Assumes parent object has a
 * VRDisplayManager to get the tracked head position and the virtual world head position
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

public class CAVE2Display : GeneralizedPerspectiveProjection {

    protected DisplayInfo displayInfo;

    [SerializeField]
    Vector2 displayResolution = new Vector2(1366, 768);

    protected GameObject vrCamera;

    // Set this to true if rendering to a virtual display
    // Disable if say rendering to an HMD display
    [SerializeField]
    protected bool renderTextureToVRCamera = true;

    Material originalMaterial;
    Material displayMat;

    // Use this for initialization
    void Start () {
        displayInfo = GetComponent<DisplayInfo>();

        screenUL = displayInfo.Px_UpperLeft;
        screenLL = displayInfo.Px_LowerLeft;
        screenLR = displayInfo.Px_LowerRight;

        head = GetComponentInParent<VRDisplayManager>().headTrackedUser;

        vrCamera = new GameObject(gameObject.name + " (VR Camera)");
        vrCamera.transform.parent = GetComponentInParent<VRDisplayManager>().virtualHead;
        vrCamera.transform.localPosition = Vector3.zero;
        vrCamera.transform.localEulerAngles = new Vector3(0, displayInfo.h + GetComponentInParent<VRDisplayManager>().displayAngularOffset, 0);

        virtualCamera = vrCamera.AddComponent<Camera>();

        RenderTexture cameraRT = new RenderTexture((int)displayResolution.x, (int)displayResolution.y, 16);
        if (renderTextureToVRCamera)
            virtualCamera.targetTexture = cameraRT;

        displayMat = new Material(Shader.Find("Unlit/Texture"));
        displayMat.name = gameObject.name + " (VR Camera Material)";
        displayMat.mainTexture = cameraRT;

        Transform displaySpace = transform.Find("Borders/PixelSpace");
        originalMaterial = displaySpace.GetComponent<MeshRenderer>().material;
        displaySpace.GetComponent<MeshRenderer>().material = displayMat;
        displaySpace.gameObject.layer = GetComponentInParent<VRDisplayManager>().gameObject.layer;
    }

    public void RemoveDisplayTexture()
    {
        Transform displaySpace = transform.Find("Borders/PixelSpace");
        displaySpace.GetComponent<MeshRenderer>().material = originalMaterial;
        if (vrCamera)
            vrCamera.SetActive(false);
    }

    public void CreateDisplayTexture()
    {
        Transform displaySpace = transform.Find("Borders/PixelSpace");
        displaySpace.GetComponent<MeshRenderer>().material = displayMat;
        if(vrCamera)
            vrCamera.SetActive(true);
    }
}
