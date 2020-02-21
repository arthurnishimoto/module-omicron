/**************************************************************************************************
 * HoloLensTestBuildManager.cs
 *
 * Editor helper script to toggle a scene between HoloLens+CAVE2 simulator mode and HoloLens build
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HoloLensTestBuildManager : MonoBehaviour {

    enum Mode { Simulator, Build, CAVE2Server, Remote, Playback };

    [Header("Settings")]
    [SerializeField]
    Mode mode = Mode.Simulator;

    [SerializeField]
    bool simulateTracking = false;

    [SerializeField]
    bool showTerminal = false;

    [SerializeField]
    bool enableCommandLine = false;

    [SerializeField]
    bool hideCAVE2View = false;

    [SerializeField]
    bool showHMDTerminal = false;

    int lastShowHMDTerminalState;

    [SerializeField]
    bool showCalibrationObjects;

    [Header("Components")]
    [SerializeField]
    ScreenConfigCalc cave2Screen;

    [SerializeField]
    Camera holoLensCamera;

    // [SerializeField]
    // int currentHoloLensCameraMask;

    [SerializeField]
    int VRProjectionCameraMask;

    [SerializeField]
    CAVE2TransformSync headTracking;

    [SerializeField]
    CAVE2RPCManager cave2RPCManager;

    [SerializeField]
    GameObject CAVE2ScreenCover;

    [SerializeField]
    CAVE2Manager cave2Manager;

    [SerializeField]
    CustomHMDPerspective hmdPerspective;

    [SerializeField]
    getReal3DMocapUpdater serverHeadTracking;

    [SerializeField]
    Camera cave2SimCamera;

    [SerializeField]
    GameObject cave2ScreenMask;

    [SerializeField]
    GameObject commandLineTerminal;

    [SerializeField]
    RemoteTerminal remoteTerminal;

    [SerializeField]
    GameObject hmdTerminalDisplay;

    [SerializeField]
    GameObject remoteUIControls;

    [SerializeField]
    GameObject calibrationObjects;

    private void Start()
    {
        UpdateMode();
    }

    private void Update()
    {
        UpdateMode();
        // currentHoloLensCameraMask = holoLensCamera.cullingMask;

        if (cave2RPCManager.IsReconnecting())
        {
            if (showHMDTerminal && lastShowHMDTerminalState == 0)
            {
                lastShowHMDTerminalState = 2;
            }
            else if (!showHMDTerminal && lastShowHMDTerminalState == 0)
            {
                showHMDTerminal = true;
                lastShowHMDTerminalState = 1;
            }
        } else if (!cave2RPCManager.IsReconnecting() && lastShowHMDTerminalState == 1)
        {
            showHMDTerminal = false;
        }
    }

    void UpdateMode () {

        headTracking.GetComponent<getReal3DMocapUpdater>().enabled = true;
        cave2SimCamera.GetComponentInParent<CAVE2CameraController>().enabled = true;

        if (mode == Mode.Simulator)
        {
            cave2Screen.enabled = true;
            holoLensCamera.cullingMask = -1;
            holoLensCamera.enabled = true;
            headTracking.enabled = !simulateTracking;
            cave2RPCManager.useMsgClient = !simulateTracking;

            cave2RPCManager.useMsgServer = false;
            serverHeadTracking.enabled = false;
            cave2SimCamera.enabled = false;
            cave2ScreenMask.SetActive(false);
            cave2Manager.simulateAsClient = true;
        }
        else if (mode == Mode.Build)
        {
            showTerminal = false;
            enableCommandLine = false;
            cave2Screen.enabled = false;
            headTracking.enabled = true;
            holoLensCamera.cullingMask = 512;
            hmdPerspective.virtualCameraCullingMask = VRProjectionCameraMask;
            holoLensCamera.enabled = true;
            cave2RPCManager.useMsgClient = true;

            cave2RPCManager.useMsgServer = false;
            serverHeadTracking.enabled = false;
            cave2SimCamera.enabled = false;
            cave2ScreenMask.SetActive(false);
            cave2Manager.simulateAsClient = true;
        }
        else if (mode == Mode.CAVE2Server)
        {
            cave2Screen.enabled = false;
            headTracking.enabled = true;
            holoLensCamera.enabled = false;
            cave2RPCManager.useMsgClient = false;

            cave2RPCManager.useMsgServer = true;
            serverHeadTracking.enabled = true;
            cave2SimCamera.enabled = true;
            cave2ScreenMask.SetActive(true);
            cave2Manager.simulateAsClient = false;
            remoteUIControls.SetActive(false);

            showTerminal = true;
        }
        else if (mode == Mode.Remote)
        {
            cave2Screen.enabled = false;
            headTracking.enabled = true;
            holoLensCamera.enabled = true;
            cave2RPCManager.useMsgClient = true;

            cave2RPCManager.useMsgServer = false;
            serverHeadTracking.enabled = true;
            cave2SimCamera.enabled = false;
            cave2ScreenMask.SetActive(false);
            cave2Manager.simulateAsClient = true;
            remoteUIControls.SetActive(true);

            showTerminal = true;
            enableCommandLine = true;
        }
        else if (mode == Mode.Playback)
        {
            cave2Screen.enabled = false;
            headTracking.enabled = true;
            holoLensCamera.enabled = false;
            serverHeadTracking.enabled = true;
            cave2SimCamera.enabled = true;
            cave2ScreenMask.SetActive(true);
            cave2Manager.simulateAsClient = false;

            headTracking.GetComponent<getReal3DMocapUpdater>().enabled = false;
            cave2SimCamera.GetComponentInParent<CAVE2CameraController>().enabled = false;
            cave2Manager.simulatorMode = true;
            cave2Manager.mocapEmulation = true;

            headTracking.enabled = false;

            cave2RPCManager.useMsgClient = false;
            cave2RPCManager.useMsgServer = false;
            remoteUIControls.SetActive(false);

            showTerminal = false;
        }

        remoteTerminal.ShowInputField(enableCommandLine);

        // Ignore ExecuteInEditMode and set local client mode if server
        if (Application.isPlaying && enableCommandLine && mode != Mode.Playback)
            remoteTerminal.StartClient(mode == Mode.CAVE2Server);

        CAVE2ScreenCover.SetActive(hideCAVE2View);

        commandLineTerminal.SetActive(showTerminal);

        hmdTerminalDisplay.SetActive(showHMDTerminal);

        calibrationObjects.SetActive(showCalibrationObjects);
    }

    void SetHeadProjectionOffset(object[] data)
    {
        if(hmdPerspective)
        {
            hmdPerspective.SetHeadProjectionOffset(data);
        }
    }

    void SetDisplayOffset(object[] data)
    {
        if (hmdPerspective)
        {
            hmdPerspective.SetDisplayOffset(data);
        }
    }

    void SetHeadOriginOffset(object[] data)
    {
        if (hmdPerspective)
        {
            hmdPerspective.SetHeadOriginOffset(data);
        }
    }

    void SetCAVE2DisplayCover(object[] data)
    {
        if (data[0].ToString().Equals("true", System.StringComparison.OrdinalIgnoreCase))
            hideCAVE2View = true;
        else if (data[0].ToString().Equals("false", System.StringComparison.OrdinalIgnoreCase))
            hideCAVE2View = false;
    }

    void ShowHMDTerminal(object[] data)
    {
        if (data[0].ToString().Equals("true", System.StringComparison.OrdinalIgnoreCase))
            showHMDTerminal = true;
        else if (data[0].ToString().Equals("false", System.StringComparison.OrdinalIgnoreCase))
            showHMDTerminal = false;
    }

    void ShowCalibrationObjects(object[] data)
    {
        if (data[0].ToString().Equals("true", System.StringComparison.OrdinalIgnoreCase))
            showCalibrationObjects = true;
        else if (data[0].ToString().Equals("false", System.StringComparison.OrdinalIgnoreCase))
            showCalibrationObjects = false;
    }
}
