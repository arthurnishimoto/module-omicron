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
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class OmicronEditorMode : MonoBehaviour
{
    const string CAVE2SIM_NAME = "Omicron/Configure for CAVE2 Simulator";
    const string CAVE2_NAME = "Omicron/Configure for CAVE2 Build";
    const string OCULUS_NAME = "Omicron/Configure for Oculus";
    const string VIVE_NAME = "Omicron/Configure for Vive";
    const string VR_NAME = "Omicron/Disable VR HMDs";
    const string CONTINUUM_3D = "Omicron/Configure for Continuum 3D Wall";
    const string CONTINUUM_MAIN = "Omicron/Configure for Continuum Main Wall";

    static GameObject c2sm;

    [MenuItem(CAVE2SIM_NAME)]
    static void ConfigCAVE2Simulator()
    {
        CleanupCustomPerspectiveCamera();

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "USING_CAVE2");

        Debug.Log("Configured for CAVE2 simulator");

        Menu.SetChecked(CAVE2SIM_NAME, true);
        Menu.SetChecked(CAVE2_NAME, false);

        if (Camera.main)
        {
            Camera.main.transform.localPosition = Vector3.up * 1.6f;
        }

        c2sm = GameObject.Find("CAVE2ScreenMask");
        if (c2sm && c2sm.GetComponent<CAVE2ScreenMaskRenderer>() && c2sm.GetComponent<CAVE2ScreenMaskRenderer>().renderMode == CAVE2ScreenMaskRenderer.RenderMode.None)
            c2sm.GetComponent<CAVE2ScreenMaskRenderer>().renderMode = CAVE2ScreenMaskRenderer.RenderMode.Background;

        if(CAVE2.GetCAVE2Manager() != null)
        {
            CAVE2.GetCAVE2Manager().GetComponent<CAVE2InputManager>().inputMappingMode = CAVE2InputManager.InputMappingMode.CAVE2;
        }
        else
        {
            Debug.LogWarning("CAVE2Manager / CAVE2InputManager not found in current scene!");
        }
    }

    [MenuItem(CAVE2_NAME)]
    static void ConfigCAVE2()
    {
        CleanupCustomPerspectiveCamera();

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "USING_CAVE2; USING_GETREAL3D");
        PlayerSettings.virtualRealitySupported = false;

        Debug.Log("Configured for CAVE2 deployment");

        Menu.SetChecked(CAVE2SIM_NAME, false);
        Menu.SetChecked(CAVE2_NAME, true);
        Menu.SetChecked(OCULUS_NAME, false);

        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;

        if (CAVE2.GetCAVE2Manager() != null)
        {
            CAVE2.GetCAVE2Manager().GetComponent<CAVE2InputManager>().inputMappingMode = CAVE2InputManager.InputMappingMode.CAVE2;
        }
        else
        {
            Debug.LogWarning("CAVE2Manager / CAVE2InputManager not found in current scene!");
        }
    }

    [MenuItem(OCULUS_NAME)]
    static void ConfigOculus()
    {
        CleanupCustomPerspectiveCamera();

        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;

        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2_NAME, false);
        Menu.SetChecked(OCULUS_NAME, PlayerSettings.virtualRealitySupported);
        Menu.SetChecked(VIVE_NAME, false);

        c2sm = GameObject.Find("CAVE2ScreenMask");
        if (c2sm && c2sm.GetComponent<CAVE2ScreenMaskRenderer>())
            c2sm.GetComponent<CAVE2ScreenMaskRenderer>().renderMode = CAVE2ScreenMaskRenderer.RenderMode.None;

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Oculus VR HMDs" : "VR support disabled");

        if (CAVE2.GetCAVE2Manager() != null)
        {
            CAVE2.GetCAVE2Manager().GetComponent<CAVE2InputManager>().inputMappingMode = CAVE2InputManager.InputMappingMode.Oculus;
        }
        else
        {
            Debug.LogWarning("CAVE2Manager / CAVE2InputManager not found in current scene!");
        }
    }

    [MenuItem(VIVE_NAME)]
    static void ConfigVive()
    {
        CleanupCustomPerspectiveCamera();

        if ( Camera.main )
            Camera.main.transform.localPosition = Vector3.up * 0f;

        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2_NAME, false);
        Menu.SetChecked(OCULUS_NAME, false);
        Menu.SetChecked(VIVE_NAME, PlayerSettings.virtualRealitySupported);

        c2sm = GameObject.Find("CAVE2ScreenMask");
        if (c2sm && c2sm.GetComponent<CAVE2ScreenMaskRenderer>())
            c2sm.GetComponent<CAVE2ScreenMaskRenderer>().renderMode = CAVE2ScreenMaskRenderer.RenderMode.None;

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Vive VR HMDs" : "VR support disabled");

        if (CAVE2.GetCAVE2Manager() != null)
        {
            CAVE2.GetCAVE2Manager().GetComponent<CAVE2InputManager>().inputMappingMode = CAVE2InputManager.InputMappingMode.Vive;
        }
        else
        {
            Debug.LogWarning("CAVE2Manager / CAVE2InputManager not found in current scene!");
        }
    }

    [MenuItem(VR_NAME)]
    static void ConfigVR()
    {
        CleanupCustomPerspectiveCamera();

        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;
        PlayerSettings.virtualRealitySupported = false;
        Menu.SetChecked(VIVE_NAME, false);
        Menu.SetChecked(OCULUS_NAME, false);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Vive VR HMDs" : "VR support disabled");
    }
    /*
    [MenuItem(CONTINUUM_3D)]
    static void ConfigContinuum3D()
    {
        if (Camera.main)
        {
            Camera.main.transform.localEulerAngles = Vector3.up * 0;

            GeneralizedPerspectiveProjection projection = Camera.main.GetComponent<GeneralizedPerspectiveProjection>();
            if (projection == null)
            {
                projection = Camera.main.gameObject.AddComponent<GeneralizedPerspectiveProjection>();
            }

            projection.SetScreenUL(new Vector3(-2.051f, 2.627f, 6.043f));
            projection.SetScreenLL(new Vector3(-2.051f, 0.309f, 6.043f));
            projection.SetScreenLR(new Vector3(2.051f, 0.309f, 6.043f));
            projection.SetVirtualCamera(Camera.main);

            StereoscopicCamera stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();
            if (stereoCamera == null)
            {
                stereoCamera = Camera.main.gameObject.AddComponent<StereoscopicCamera>();
            }

            stereoCamera.EnableStereo(true);
            stereoCamera.SetStereoResolution(new Vector2(7680f, 4320f), false);
            stereoCamera.SetStereoInverted(true);

            projection.SetHeadTracker(GameObject.Find("CAVE2-PlayerController/Head").transform);
        }

        c2sm = GameObject.Find("CAVE2ScreenMask");
        if (c2sm && c2sm.GetComponent<CAVE2ScreenMaskRenderer>())
            c2sm.GetComponent<CAVE2ScreenMaskRenderer>().renderMode = CAVE2ScreenMaskRenderer.RenderMode.None;

        Debug.Log("Configured for Continuum 3D Wall");
    }

    [MenuItem(CONTINUUM_MAIN)]
    static void ConfigContinuumMain()
    {
        if (Camera.main)
        {
            // Camera.main.transform.localEulerAngles = Vector3.up * 90;

            GeneralizedPerspectiveProjection projection = Camera.main.GetComponent<GeneralizedPerspectiveProjection>();
            if (projection == null)
            {
                projection = Camera.main.gameObject.AddComponent<GeneralizedPerspectiveProjection>();
            }

            projection.SetScreenUL(new Vector3(-3.642f, 2.527f, 3.579f));
            projection.SetScreenLL(new Vector3(-3.642f, 0.479f, 3.579f));
            projection.SetScreenLR(new Vector3(3.642f, 0.479f, 3.579f));
            projection.SetVirtualCamera(Camera.main);

            StereoscopicCamera stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();
            if (stereoCamera != null)
            {
                //DestroyImmediate(stereoCamera);
                stereoCamera.enabled = false;
            }
            projection.SetHeadTracker(GameObject.Find("CAVE2-PlayerController/Head").transform);
        }

        c2sm = GameObject.Find("CAVE2ScreenMask");
        if (c2sm && c2sm.GetComponent<CAVE2ScreenMaskRenderer>())
            c2sm.GetComponent<CAVE2ScreenMaskRenderer>().renderMode = CAVE2ScreenMaskRenderer.RenderMode.None;

        Debug.Log("Configured for Continuum Main Wall");
    }
    */

    static void CleanupCustomPerspectiveCamera()
    {
        GeneralizedPerspectiveProjection projection = Camera.main.GetComponent<GeneralizedPerspectiveProjection>();
        if (projection != null)
        {
            //DestroyImmediate(projection);
            projection.enabled = false;
        }

        StereoscopicCamera stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();
        if (stereoCamera != null)
        {
            //DestroyImmediate(stereoCamera);
            projection.enabled = false;
        }
    }
}
