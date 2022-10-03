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

public class CAVE2CameraController : MonoBehaviour {

    int leftEyeLayer;
    int rightEyeLayer;
    int cameraLayer;

    [SerializeField]
    bool setCameraEyeLayerMasks = false;

    [SerializeField]
    string leftEyeLayerName = "LeftEye";

    [SerializeField]
    string rightEyeLayerName = "RightEye";

    Camera mainCamera;

    [SerializeField]
    Transform leftCameraParent = null;

    [SerializeField]
    Transform rightCameraParent = null;

    CAVE2WandNavigator wandNav = null;

    [SerializeField]
    bool generateGetReal3DCamera = true;

    bool defaultCameraClearFlagsSet = false;
    bool defaultCameraCullingMaskSet = false;
    CameraClearFlags defaultMainCameraClearFlags;
    int defaultMainCameraCullingMask;

    // Use this for initialization
    void Start()
    {
        CAVE2.AddCameraController(this);

        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>();
        }

        defaultMainCameraClearFlags = mainCamera.clearFlags;
        
#if USING_GETREAL3D
        if (generateGetReal3DCamera && mainCamera.GetComponent<getRealCameraUpdater>())
        {
            mainCamera.GetComponent<getRealCameraUpdater>().enabled = true;
        }
        else if(generateGetReal3DCamera)
        {
            mainCamera.gameObject.AddComponent<getRealCameraUpdater>();
        }
#endif
        if (setCameraEyeLayerMasks)
        {
            leftEyeLayer = 1 << LayerMask.NameToLayer(leftEyeLayerName);
            rightEyeLayer = 1 << LayerMask.NameToLayer(rightEyeLayerName);

            cameraLayer = mainCamera.cullingMask;
        }

        wandNav = GetComponentInParent<CAVE2WandNavigator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (setCameraEyeLayerMasks)
        {
            Camera[] cameras = GetComponentsInChildren<Camera>();

            // Found stereo pair
            if (cameras.Length == 2)
            {
                // Use original culling mask minus other eye layer
                cameras[1].cullingMask = cameraLayer - rightEyeLayer;   // Left
                cameras[0].cullingMask = cameraLayer - leftEyeLayer;    // Right

                if(leftCameraParent != null)
                {
                    cameras[1].transform.parent = leftCameraParent;
                }
                if (rightCameraParent != null)
                {
                    cameras[0].transform.parent = rightCameraParent;
                }
            }
        }
    }

    void Update()
    {
        if( CAVE2.IsSimulatorMode() )
        {
#if USING_GETREAL3D
            if (mainCamera.GetComponent<getRealCameraUpdater>())
            {
                mainCamera.GetComponent<getRealCameraUpdater>().enabled = false;
            }
#endif

            float simHeadRotateSpeed = 40;
            if(wandNav)
            {
                simHeadRotateSpeed = wandNav.GetTurnSpeed();
            }
            if( Input.GetKey(CAVE2.Input.simulatorHeadRotateL) )
            {
                transform.Rotate(-Vector3.up * Time.deltaTime * simHeadRotateSpeed);
            }
            else if (Input.GetKey(CAVE2.Input.simulatorHeadRotateR))
            {
                transform.Rotate(Vector3.up * Time.deltaTime * simHeadRotateSpeed);
            }
        }
    }

    public Camera GetMainCamera()
    {
        return mainCamera;
    }

    public void SetCameraCullingMask(int mask)
    {
        if(!defaultCameraCullingMaskSet)
        {
            defaultMainCameraCullingMask = mainCamera.cullingMask;
            defaultCameraCullingMaskSet = true;
        }

        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach( Camera c in cameras )
        {
            c.cullingMask = mask;
        }
    }

    public void SetCameraClearFlags(CameraClearFlags flags)
    {
        if (!defaultCameraClearFlagsSet)
        {
            defaultMainCameraClearFlags = mainCamera.clearFlags;
            defaultCameraClearFlagsSet = true;
        }

        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            c.clearFlags = flags;
        }
    }

    public void SetCameraBackgroundColor(Color color)
    {

        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            c.backgroundColor = color;
        }
    }

    public void SetCameraNearClippingPlane(float value)
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            c.nearClipPlane = value;
        }
    }

    public void SetGenerateGetReal3DCamera(bool enabled)
    {
        generateGetReal3DCamera = enabled;
    }

    public void RestoreDefaultCameraCullingMask()
    {
        if (defaultCameraCullingMaskSet)
        {
            Camera[] cameras = GetComponentsInChildren<Camera>();
            foreach (Camera c in cameras)
            {
                c.cullingMask = defaultMainCameraCullingMask;
            }
        }
    }

    public void RestoreDefaultCameraClearFlags()
    {
        if (defaultCameraClearFlagsSet)
        {
            Camera[] cameras = GetComponentsInChildren<Camera>();
            foreach (Camera c in cameras)
            {
                c.clearFlags = defaultMainCameraClearFlags;
            }
        }
    }
}
