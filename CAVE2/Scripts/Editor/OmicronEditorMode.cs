/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2018		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2018, Electronic Visualization Laboratory, University of Illinois at Chicago
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

    [MenuItem(CAVE2SIM_NAME)]
    static void ConfigCAVE2Simulator()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Debug.Log("Configured for CAVE2 simulator");

        Menu.SetChecked(CAVE2SIM_NAME, true);
        Menu.SetChecked(CAVE2_NAME, false);

        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;
    }

    [MenuItem(CAVE2_NAME)]
    static void ConfigCAVE2()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "USING_GETREAL3D");
        PlayerSettings.virtualRealitySupported = false;

        Debug.Log("Configured for CAVE2 deployment");

        Menu.SetChecked(CAVE2SIM_NAME, false);
        Menu.SetChecked(CAVE2_NAME, true);
        Menu.SetChecked(OCULUS_NAME, false);

        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;
    }

    [MenuItem(OCULUS_NAME)]
    static void ConfigOculus()
    {
        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;

        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2_NAME, false);
        Menu.SetChecked(OCULUS_NAME, PlayerSettings.virtualRealitySupported);
        Menu.SetChecked(VIVE_NAME, false);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Oculus VR HMDs" : "VR support disabled");
    }

    [MenuItem(VIVE_NAME)]
    static void ConfigVive()
    {
        if( Camera.main )
            Camera.main.transform.localPosition = Vector3.up * 0f;

        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2_NAME, false);
        Menu.SetChecked(OCULUS_NAME, false);
        Menu.SetChecked(VIVE_NAME, PlayerSettings.virtualRealitySupported);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Vive VR HMDs" : "VR support disabled");
    }

    [MenuItem(VR_NAME)]
    static void ConfigVR()
    {
        if (Camera.main)
            Camera.main.transform.localPosition = Vector3.up * 1.6f;
        PlayerSettings.virtualRealitySupported = false;
        Menu.SetChecked(VIVE_NAME, false);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Vive VR HMDs" : "VR support disabled");
    }
}
