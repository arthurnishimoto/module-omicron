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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>CAVE2ExperimentalDisplayMode</c> configures the CAVE2-Manager and CAVE2-PlayerController
/// for an experimental display mode designed to make minimal use of getReal3D and switch from using
/// getReal3D's camera updater to Omicron's GeneralizedPerspectiveProjection and StereoscopicCamera.
/// </summary>
[ExecuteInEditMode]
public class CAVE2ExperimentalDisplayMode : MonoBehaviour
{
    enum DisplayMode { None, Standard_getReal3D, Experimental }

    DisplayMode lastDisplayMode = DisplayMode.None;

    [SerializeField]
    DisplayMode displayMode = DisplayMode.Standard_getReal3D;

    [Header("Modified Scripts")]
    [SerializeField]
    CAVE2ClusterManager clusterManager = null;

    [SerializeField]
    CAVE2RPCManager rpcManager = null;

    [SerializeField]
    CAVE2CameraController cameraController = null;

    // Update is called once per frame
    void Update()
    {
        if(lastDisplayMode != displayMode)
        {
            switch(displayMode)
            {
                case DisplayMode.Standard_getReal3D:
                    clusterManager.SetManageCAVE2GeneralizedPerspectiveProjection(false);
                    rpcManager.EnableMsgServer(false);
                    if (cameraController)
                    {
                        cameraController.SetGenerateGetReal3DCamera(true);
                    }
                    break;
                case DisplayMode.Experimental:
                    clusterManager.SetManageCAVE2GeneralizedPerspectiveProjection(true);
                    rpcManager.EnableMsgServer(true);
                    if (cameraController)
                    {
                        cameraController.SetGenerateGetReal3DCamera(false);
                    }
                    else
                    {
                        Debug.LogWarning("CAVE2 Experimental Mode - CameraController is NOT assigned!");
                    }
                    Debug.LogWarning("CAVE2 Experimental Mode Active - Make sure CAVE2RPCManager ServerIP is assigned!");
                    break;
            }

            lastDisplayMode = displayMode;
        }
    }
}
