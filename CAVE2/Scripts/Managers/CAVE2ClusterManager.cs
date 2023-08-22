/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2023		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2023, Electronic Visualization Laboratory, University of Illinois at Chicago
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class CAVE2ClusterManager : MonoBehaviour
{
    [SerializeField]
    OmicronManager omicronManager = null;

    [SerializeField]
    CAVE2RPCManager rpcManager = null;

    [SerializeField]
    bool enableScreenManagement = false;

    bool windowAssignmentDone;

    int myWindowPosID;

    [SerializeField]
    bool useCAVE2GeneralizedPerspectiveProjection = false;

    [SerializeField]
    Transform headTracker = null;

    int headTrackerID = -1;

    [SerializeField]
    bool keepHeadTrackerIDUpdated = true;

    float updateInterval = 2;
    float updateTimer;

    Dictionary<int, CAVE2ClientInfo> cave2Clients = new Dictionary<int, CAVE2ClientInfo>();
    Dictionary<string, int> cave2ClientNames = new Dictionary<string, int>();

    [Header("UI")]
    [SerializeField]
    Text currentHeadTrackerText = null;

    [SerializeField]
    Text currentHeadTrackerTransformText = null;

    [SerializeField]
    Button[] clientStatusButtons = null;

    [SerializeField]
    Text clientStatusText = null;

    [Header("Debug")]
    [SerializeField]
    bool showDebugTextOnDisplayNodes = false;

    [SerializeField]
    Text debugUIText = null;

    [SerializeField]
    InputField displayWidthUI = null;

    [SerializeField]
    InputField displayHeightUI = null;

    [SerializeField]
    InputField displayPosXUI = null;

    [SerializeField]
    InputField displayPosYUI = null;

    Display mainDisplay;

    bool firstUpdate = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!CAVE2.IsMaster())
        {
            omicronManager.connectToServer = false;
            rpcManager.EnableMsgServer(false);
            rpcManager.EnableMsgClient(true);


            if (CAVE2.OnCAVE2Display() && debugUIText)
            {
                debugUIText.gameObject.SetActive(showDebugTextOnDisplayNodes);
            }
        }

        Process currentProcess = Process.GetCurrentProcess();
        Process[] processlist = Process.GetProcesses();

        int matchingProcessIndex = 0;
        for (int i = 0; i < processlist.Length; i++)
        {
            Process process = processlist[i];
            if (process.ProcessName == currentProcess.ProcessName)
            {
                if (process.Id == currentProcess.Id)
                {
                    myWindowPosID = matchingProcessIndex;
                }
                matchingProcessIndex++;
            }
        }

        if (useCAVE2GeneralizedPerspectiveProjection)
        {
            SetCAVE2CameraPerspective();
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        if (headTracker)
        {
            CAVE2MocapUpdater head = headTracker.GetComponent<CAVE2MocapUpdater>();

            if (currentHeadTrackerText)
            {
                if (head != null)
                {
                    currentHeadTrackerText.text = head.sourceID.ToString();
                }
                else
                {
                    currentHeadTrackerText.text = "N/A";
                }
            }
            if (currentHeadTrackerTransformText)
            {
                currentHeadTrackerTransformText.text = headTracker.localPosition.ToString("F3") + "\n" + headTracker.localEulerAngles.ToString("F3");
            }
        }

        if (debugUIText)
        {
            debugUIText.text = CAVE2Manager.GetMachineName() + " as " + (CAVE2.IsMaster() ? "Head Node" : "Display Node") + "\n";

            Process currentProcess = Process.GetCurrentProcess();
            debugUIText.text += "My Process: '" + currentProcess.ProcessName + "' ID: '" + currentProcess.Id + "' Window title: '" + currentProcess.MainWindowTitle + "'\n";
            debugUIText.text += "   Window ID: " + myWindowPosID + "\n";

            debugUIText.text += "ConnID: " + rpcManager.GetConnID() + "\n";

            debugUIText.text += "\nDisplays: \n";
            Display[] displays = Display.displays;
            int displayID = 0;
            foreach (Display d in displays)
            {
                if(firstUpdate && displayID == 0)
                {
                    mainDisplay = d;
                    if (displayWidthUI)
                    {
                        displayWidthUI.SetTextWithoutNotify(d.renderingWidth + "");
                    }
                    if (displayHeightUI)
                    {
                        displayHeightUI.SetTextWithoutNotify(d.renderingHeight + "");
                    }
                    firstUpdate = false;
                }
                debugUIText.text += "  [" + displayID + "] NativeRes: " + d.systemWidth + ", " + d.systemHeight + " RenderDim: " + d.renderingWidth + ", " + d.renderingHeight + "\n";
                debugUIText.text += "    " + d.ToString() + "\n";
                displayID++;
            }

            debugUIText.text += "\nSensors: \n";
            OmicronMocapSensor[] mocapSensors = GetComponentsInChildren<OmicronMocapSensor>();
            foreach(OmicronMocapSensor mo in mocapSensors)
            {
                debugUIText.text += "  [" + mo.sourceID + "] Pos: " + mo.GetPosition().ToString("F3") + " Rot: " + mo.GetOrientation().ToString("F3") + "\n";
            }
            debugUIText.text += "\nControllers: \n";
            OmicronController[] controllerSensors = GetComponentsInChildren<OmicronController>();
            foreach (OmicronController c in controllerSensors)
            {
                debugUIText.text += "  [" + c.sourceID + "] Button Flag: " + c.rawFlags + "\n";
                debugUIText.text += "       Analog 1: " + c.GetAnalogStick(1).ToString("F2") + " Analog 2: " + c.GetAnalogStick(2).ToString("F2") + "\n";
                debugUIText.text += "       Analog 3: " + c.GetAnalogStick(3).ToString("F2") + " Analog 4: " + c.GetAnalogStick(4).ToString("F2") + "\n";
            }
        }

        if (enableScreenManagement && windowAssignmentDone == false)
        {
            int connID = GetComponent<CAVE2RPCManager>().GetConnID();

            var currentProc = System.Diagnostics.Process.GetCurrentProcess();
            for (int i = 1; i < 5; i++)
            {
                if (currentProc.Id == CAVE2ClusterManager.GetWindowProcessId(-i))
                {
                    CAVE2ClusterManager.SetWindowTitle(connID, -i);
                    switch (connID)
                    {
                        case (1):
                            CAVE2ClusterManager.SetPosition(connID, 0, 768 * 0, 5440, 768);
                            break;
                        case (2):
                            CAVE2ClusterManager.SetPosition(connID, 0, 768 * 1, 5440, 768);
                            break;
                        case (3):
                            CAVE2ClusterManager.SetPosition(connID, 0, 768 * 2, 5440, 768);
                            break;
                        case (4):
                            CAVE2ClusterManager.SetPosition(connID, 0, 768 * 3, 5440, 768);
                            break;
                    }
                    windowAssignmentDone = true;
                }
            }
        }

        if (CAVE2.IsMaster() && keepHeadTrackerIDUpdated)
        {
            if (updateTimer >= updateInterval && headTrackerID != -1)
            {
                SetPrimaryHeadTracker(headTrackerID);
                updateTimer = 0;
            }
            else
            {
                updateTimer += Time.deltaTime;
            }
        }
    }

    public void SetPrimaryHeadTracker(int trackerID)
    {
        headTrackerID = trackerID;
        CAVE2.SendMessage(gameObject.name, "SetPrimaryHeadTrackerRPC", trackerID);
    }

    void SetPrimaryHeadTrackerRPC(int trackerID)
    {
        if (headTracker)
        {
            headTracker.GetComponent<CAVE2MocapUpdater>().SetSourceID(trackerID);
        }
    }

    public void SetEnableScreenManagement(bool enabled)
    {
        enableScreenManagement = enabled;
    }

    public void SetManageCAVE2GeneralizedPerspectiveProjection(bool enabled)
    {
        useCAVE2GeneralizedPerspectiveProjection = enabled;
    }

    void SetCAVE2CameraPerspective()
    {
        GeneralizedPerspectiveProjection mainCamera = Camera.main.GetComponent<GeneralizedPerspectiveProjection>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main.gameObject.AddComponent<GeneralizedPerspectiveProjection>();
        }
        else if (mainCamera.enabled == false)
        {
            mainCamera.enabled = true;
        }

        // Stereoscopic first
        StereoscopicCamera stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();
        if (stereoCamera == null)
        {
            stereoCamera = Camera.main.gameObject.AddComponent<StereoscopicCamera>();
        }
        else if (stereoCamera.enabled == false)
        {
            stereoCamera.enabled = true;
        }
        stereoCamera.EnableStereo(true);
        stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Interleaved);

        // Stereo for main CAVE2 columns
        stereoCamera.SetStereoResolution(new Vector2(1366, 3072));
        stereoCamera.ForceSetupStereoCameras();
        stereoCamera.InvertStereo();

        bool mainCameraRotated = false;

        // Setup camera perspective
        GeneralizedPerspectiveProjection[] cameras = Camera.main.GetComponentsInChildren<GeneralizedPerspectiveProjection>();
        foreach (GeneralizedPerspectiveProjection camera in cameras)
        {
            camera.SetHeadTracker(headTracker);
            camera.SetVirtualCamera(camera.GetComponent<Camera>());
            camera.SetApplyHeadOffset(false);

            RemoteTerminal terminal = GameObject.Find("Terminal").GetComponent<RemoteTerminal>();

            terminal.PrintUI("Setting Camera Perspective for " + CAVE2Manager.GetMachineName() + " WindowID: " + myWindowPosID);
            if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME) //Backwall
            {
                // Backwall has a different stereo configuration
                stereoCamera.SetStereoResolution(new Vector2(5440, 768));
                stereoCamera.InvertStereo(); // Invert backwall back to un-inverted

                // Stereoscopic camera is the "root" camera
                if (!mainCameraRotated)
                {
                    mainCamera.transform.Rotate(Vector3.up * 180);
                    mainCameraRotated = true;
                }
                switch (myWindowPosID)
                {
                    case (0): // Top Row
                        camera.SetScreenLL(new Vector3(2.57f, 2.03f, -6.85f));
                        camera.SetScreenLR(new Vector3(-1.6f, 2.03f, -6.85f));
                        camera.SetScreenUL(new Vector3(2.57f, 2.60f, -6.85f));
                        break;
                    case (1):
                        camera.SetScreenLL(new Vector3(2.57f, 1.44f, -6.85f));
                        camera.SetScreenLR(new Vector3(-1.6f, 1.44f, -6.85f));
                        camera.SetScreenUL(new Vector3(2.57f, 2.02f, -6.85f));
                        break;
                    case (2):
                        camera.SetScreenLL(new Vector3(2.57f, 0.86f, -6.85f));
                        camera.SetScreenLR(new Vector3(-1.6f, 0.86f, -6.85f));
                        camera.SetScreenUL(new Vector3(2.57f, 1.44f, -6.85f));
                        break;
                    case (3): // Bottom Row
                        camera.SetScreenLL(new Vector3(2.57f, 0.28f, -6.85f));
                        camera.SetScreenLR(new Vector3(-1.6f, 0.28f, -6.85f));
                        camera.SetScreenUL(new Vector3(2.57f, 0.86f, -6.85f));
                        break;
                }
            }
            else if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME + "-01")
            {
                switch (myWindowPosID)
                {
                    case (0): // Lyra-01
                        camera.SetScreenUL(new Vector3(-0.5437923f, 2.605996f, -3.239587f));
                        camera.SetScreenLL(new Vector3(-0.5437923f, 0.3060037f, -3.239587f));
                        camera.SetScreenLR(new Vector3(-1.512198f, 0.3060037f, -2.916143f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -161.531f);
                        }
                        break;
                    case (1): // Lyra-03
                        camera.SetScreenUL(new Vector3(-1.518851f, 2.605996f, -2.912683f));
                        camera.SetScreenLL(new Vector3(-1.518851f, 0.3060037f, -2.912683f));
                        camera.SetScreenLR(new Vector3(-2.339788f, 0.3060037f, -2.305651f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -143.519f);
                        }
                        break;
                    case (2): // Lyra-05
                        camera.SetScreenUL(new Vector3(-2.345045f, 2.605996f, -2.300303f));
                        camera.SetScreenLL(new Vector3(-2.345045f, 0.3060037f, -2.300303f));
                        camera.SetScreenLR(new Vector3(-2.938052f, 0.3060037f, -1.469178f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -125.508f);
                        }
                        break;
                }
            }
            else if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME + "-02")
            {
                switch (myWindowPosID)
                {
                    case (0): // Lyra-07
                        camera.SetScreenUL(new Vector3(-2.941398f, 2.605996f, -1.462467f));
                        camera.SetScreenLL(new Vector3(-2.941398f, 0.3060037f, -1.462467f));
                        camera.SetScreenLR(new Vector3(-3.248353f, 0.3060037f, -0.4887097f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -107.496f);
                        }
                        break;
                    case (1): // Lyra-09
                        camera.SetScreenUL(new Vector3(-3.24946f, 2.605996f, -0.4812928f));
                        camera.SetScreenLL(new Vector3(-3.24946f, 0.3060037f, -0.4812928f));
                        camera.SetScreenLR(new Vector3(-3.240278f, 0.3060037f, 0.539658f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -89.485f);
                        }
                        break;
                    case (2): // Lyra-11
                        camera.SetScreenUL(new Vector3(-3.239037f, 2.605996f, 0.5470539f));
                        camera.SetScreenLL(new Vector3(-3.239037f, 0.3060037f, 0.5470539f));
                        camera.SetScreenLR(new Vector3(-2.914619f, 0.3060037f, 1.515133f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -71.47301f);
                        }
                        break;
                }
            }
            else if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME + "-03")
            {
                switch (myWindowPosID)
                {
                    case (0): // Lyra-13
                        camera.SetScreenUL(new Vector3(-2.911153f, 2.605996f, 1.521783f));
                        camera.SetScreenLL(new Vector3(-2.911153f, 0.3060037f, 1.521783f));
                        camera.SetScreenLR(new Vector3(-2.303294f, 0.3060037f, 2.342108f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -53.462f);
                        }
                        break;
                    case (1): // Lyra-15
                        camera.SetScreenUL(new Vector3(-2.297941f, 2.605996f, 2.34736f));
                        camera.SetScreenLL(new Vector3(-2.297941f, 0.3060037f, 2.34736f));
                        camera.SetScreenLR(new Vector3(-1.46622f, 0.3060037f, 2.939529f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -35.45f);
                        }
                        break;
                    case (2): // Lyra-17
                        camera.SetScreenLL(new Vector3(-1.459505f, 0.3060037f, 2.942869f));
                        camera.SetScreenLR(new Vector3(-0.4854392f, 0.3060037f, 3.248843f));
                        camera.SetScreenUL(new Vector3(-1.459505f, 2.605996f, 2.942869f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * -17.439f);
                        }
                        break;
                }
            }
            else if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME + "-04")
            {
                switch (myWindowPosID)
                {
                    case (0): // Lyra-19
                        camera.SetScreenUL(new Vector3(-0.478021f, 2.605996f, 3.249943f));
                        camera.SetScreenLL(new Vector3(-0.478021f, 0.3060037f, 3.249943f));
                        camera.SetScreenLR(new Vector3(0.54292f, 0.3060037f, 3.239733f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 0.573f);
                        }
                        break;
                    case (1): // Lyra-21
                        camera.SetScreenUL(new Vector3(0.5503147f, 2.605996f, 3.238485f));
                        camera.SetScreenLL(new Vector3(0.5503147f, 0.3060037f, 3.238485f));
                        camera.SetScreenLR(new Vector3(1.518067f, 0.3060037f, 2.913092f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 18.584f);
                        }
                        break;
                    case (2): // Lyra-23
                        camera.SetScreenUL(new Vector3(1.524713f, 2.605996f, 2.909619f));
                        camera.SetScreenLL(new Vector3(1.524713f, 0.3060037f, 2.909619f));
                        camera.SetScreenLR(new Vector3(2.344425f, 0.3060037f, 2.300935f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 36.596f);
                        }
                        break;
                }
            }
            else if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME + "-05")
            {
                switch (myWindowPosID)
                {
                    case (0): // Lyra-25
                        camera.SetScreenUL(new Vector3(2.349672f, 2.605996f, 2.295577f));
                        camera.SetScreenLL(new Vector3(2.349672f, 0.3060037f, 2.295577f));
                        camera.SetScreenLR(new Vector3(2.941004f, 0.3060037f, 1.463259f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 54.608f);
                        }
                        break;
                    case (1): // Lyra-27
                        camera.SetScreenUL(new Vector3(2.944337f, 2.605996f, 1.456542f));
                        camera.SetScreenLL(new Vector3(2.944337f, 0.3060037f, 1.456542f));
                        camera.SetScreenLR(new Vector3(3.24933f, 0.3060037f, 0.4821681f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 72.619f);
                        }
                        break;
                    case (2): // Lyra-29
                        camera.SetScreenUL(new Vector3(3.250422f, 2.605996f, 0.4747489f));
                        camera.SetScreenLL(new Vector3(3.250422f, 0.3060037f, 0.4747489f));
                        camera.SetScreenLR(new Vector3(3.239185f, 0.3060037f, -0.5461813f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 90.631f);
                        }
                        break;
                }
            }
            else if (CAVE2Manager.GetMachineName() == CAVE2.DISPLAY_NODE_NAME + "-06")
            {
                switch (myWindowPosID)
                {
                    case (0): // Lyra-31
                        camera.SetScreenUL(new Vector3(3.237929f, 2.605996f, -0.5535748f));
                        camera.SetScreenLL(new Vector3(3.237929f, 0.3060037f, -0.5535748f));
                        camera.SetScreenLR(new Vector3(2.911562f, 0.3060037f, -1.520999f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 108.642f);
                        }
                        break;
                    case (1): // Lyra-33
                        camera.SetScreenUL(new Vector3(2.908083f, 2.605996f, -1.527641f));
                        camera.SetScreenLL(new Vector3(2.908083f, 0.3060037f, -1.527641f));
                        camera.SetScreenLR(new Vector3(2.298573f, 0.3060037f, -2.346741f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 126.654f);
                        }
                        break;
                    case (2): // Lyra-35
                        camera.SetScreenUL(new Vector3(2.29321f, 2.605996f, -2.351982f));
                        camera.SetScreenLL(new Vector3(2.29321f, 0.3060037f, -2.351982f));
                        camera.SetScreenLR(new Vector3(1.460298f, 0.3060037f, -2.942476f));

                        if (!mainCameraRotated)
                        {
                            mainCameraRotated = true;
                            // Stereoscopic camera is the "root" camera
                            mainCamera.transform.Rotate(Vector3.up * 144.665f);
                        }
                        break;
                }
            }
            else if (CAVE2Manager.OnCAVE2Master())
            {
                switch (myWindowPosID)
                {
                    case (0):
                        camera.SetScreenLL(new Vector3(-2.3f, 0.29f, 2.36f));
                        camera.SetScreenLR(new Vector3(2.35f, 0.29f, 2.31f));
                        camera.SetScreenUL(new Vector3(-2.3f, 2.61f, 2.36f));
                        break;
                }
                stereoCamera.enabled = false;
            }
        }
    }

    public void OnSetDisplay()
    {
        int newWidth = 1024;
        int newHeight = 768;

        int newXPos = 0;
        int newYPos = 0;

        int.TryParse(displayWidthUI.text, out newWidth);
        int.TryParse(displayHeightUI.text, out newHeight);
        int.TryParse(displayPosXUI.text, out newXPos);
        int.TryParse(displayPosYUI.text, out newYPos);

        //mainDisplay.SetParams(newWidth, newHeight, newXPos, newYPos);
        //mainDisplay.SetRenderingResolution(newWidth, newHeight);
        Screen.SetResolution(newWidth, newHeight, FullScreenMode.Windowed);
        // SetPosition(newXPos, newYPos);
    }

    public int GetWindowPositionID()
    {
        return myWindowPosID;
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    public struct Rect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
    public static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
    public static extern bool GetWindowThreadProcessId(System.IntPtr hwnd, out System.Int32 lpdwProcessId);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
    public static extern long SetWindowLongA(System.IntPtr hwnd, System.Int32 nIndex, System.Int64 dwNewLong);
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

    public static void SetPosition(int x, int y, int resX = 0, int resY = 0, bool borderless = true)
    {
#if !UNITY_EDITOR
        const int SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        IntPtr windowPtr = GetActiveWindow();

        // https://answers.unity.com/questions/946630/borderless-windows-in-standalone-builds.html
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlonga
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles

        if (borderless)
        {
            // Sets window to borderless
            SetWindowLongA(windowPtr, GWL_STYLE, 0x80000000L);
        }
        else
        {
            // Sets window with Title Bar
            SetWindowLongA(windowPtr, GWL_STYLE, 0x00020000L);
        }

        //https://answers.unity.com/questions/13523/is-there-a-way-to-set-the-position-of-a-standalone.html?_ga=2.54933416.2072224053.1643235688-1899960085.1550115936
        // Sets the window position and shows window (last flag required after setting style above)
        SetWindowPos(windowPtr, 0, x, y, resX, resY, SWP_SHOWWINDOW);
#endif
    }

    public static void SetPosition(int connID, int x, int y, int resX = 0, int resY = 0, bool borderless = true)
    {
#if !UNITY_EDITOR
        const int SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        IntPtr windowPtr = FindWindow(null, Application.productName + " " + connID);

        // https://answers.unity.com/questions/946630/borderless-windows-in-standalone-builds.html
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlonga
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles

        if (borderless)
        {
            // Sets window to borderless
            SetWindowLongA(windowPtr, GWL_STYLE, 0x00800000L);
        }
        else
        {
            // Sets window with Title Bar
            SetWindowLongA(windowPtr, GWL_STYLE, 0x00C00000L);
        }

        //https://answers.unity.com/questions/13523/is-there-a-way-to-set-the-position-of-a-standalone.html?_ga=2.54933416.2072224053.1643235688-1899960085.1550115936
        // Sets the window position and shows window (last flag required after setting style above)
        SetWindowPos(windowPtr, 0, x, y, resX, resY, SWP_SHOWWINDOW);
#endif
    }

    public static void SetWindowTitle(int connID, int oldID = 0)
    {
        // Get the window handle by current Process
        // var currentProc = System.Diagnostics.Process.GetCurrentProcess();
        // IntPtr windowPtr = currentProc.MainWindowHandle;

        string oldIDstr = "";
        if(oldID != 0)
        {
            oldIDstr = " " + oldID;
        }

        //Get the window handle by name (does not work for multiple app instances)
        IntPtr windowPtr = FindWindow(null, Application.productName + oldIDstr);

        //Set the title text using the window handle.
        SetWindowText(windowPtr, Application.productName + " " + connID);
    }

    /**
     * Sets the Window Title to 'Product Name [ProcessID]'
     */
    public static void SetWindowTitle()
    {
        IntPtr windowPtr = GetActiveWindow();

        //Set the title text using the window handle.
        SetWindowText(windowPtr, Application.productName + " " + Process.GetCurrentProcess().Id);
    }

    public static int GetWindowProcessId(int connID)
    {
        int id = -1;
        IntPtr windowPtr = FindWindow(null, Application.productName + " " + connID);
        GetWindowThreadProcessId(windowPtr, out id);
        return id;
    }

    public static Rect GetWindowRect(string windowName)
    {
        IntPtr windowPtr = FindWindow(null, windowName);
        Rect windowRect = new Rect();

        GetWindowRect(windowPtr, ref windowRect);

        return windowRect;
    }

#else
    public static void SetPosition(int x, int y, int resX = 0, int resY = 0, bool borderless = true)
    {
        UnityEngine.Debug.LogWarning("Not implemented on current platform");
    }

    public static void SetPosition(int connID, int x, int y, int resX = 0, int resY = 0, bool borderless = true)
    {
        UnityEngine.Debug.LogWarning("Not implemented on current platform");
    }

    public static void SetWindowTitle(int connID, int oldID = 0)
    {
        UnityEngine.Debug.LogWarning("Not implemented on current platform");
    }

    public static void SetWindowTitle()
    {
        UnityEngine.Debug.LogWarning("Not implemented on current platform");
    }

    public static int GetWindowProcessId(int connID)
    {
        UnityEngine.Debug.LogWarning("Not implemented on current platform");
        return -1;
    }

    public static Rect GetWindowRect(string windowName)
    {
        UnityEngine.Debug.LogWarning("Not implemented on current platform");

        return new Rect();
    }
#endif

    public void AddClient(int connID, string hostname, string deviceType)
    {
        string displayName = hostname;
        int clientID = 0;
        int nodeID = -1;
        bool cave2Node = false;

        if (hostname == "ORION-01")
        {
            nodeID = 0;
            cave2Node = true;
        }
        else if (hostname == "ORION-02")
        {
            nodeID = 1;
            cave2Node = true;
        }
        else if (hostname == "ORION-03")
        {
            nodeID = 2;
            cave2Node = true;
        }
        else if (hostname == "ORION-04")
        {
            nodeID = 3;
            cave2Node = true;
        }
        else if (hostname == "ORION-05")
        {
            nodeID = 4;
            cave2Node = true;
        }
        else if (hostname == "ORION-06")
        {
            nodeID = 5;
            cave2Node = true;
        }

        if (deviceType == "Android")
        {
            displayName = "ANDROID";
            nodeID = 0;
            clientID = 19;
        }

        if (deviceType == "WindowsEditor")
        {
            displayName = "LAPTOP";
            nodeID = 0;
            clientID = 20;
        }

        if (deviceType == "WSAPlayerARM")
        {
            displayName = "HOLOLENS-2";
            nodeID = 0;
            clientID = 18;
        }

        if (deviceType == "MetroPlayerX86")
        {
            displayName = "HOLOLENS-1";
            nodeID = 0;
            clientID = 19; // Replaces Android slot
            cave2Node = false;
        }

        if (cave2Node)
        {
            if (cave2ClientNames.ContainsKey(hostname))
            {
                clientID = cave2ClientNames[hostname] + 1;
                cave2ClientNames[hostname] = clientID;
            }
            else
            {
                cave2ClientNames.Add(hostname, 0);
            }
        }

        int buttonID = (nodeID * 3) + clientID;

        UnityEngine.Debug.Log("Client connected: " + connID + " " + hostname);
        if(cave2Clients.ContainsKey(connID))
        {
            CAVE2ClientInfo clientInfo = cave2Clients[connID];
            clientInfo.connectTime = Time.time;
            clientInfo.connected = true;
            clientInfo.reconnects++;

            if (buttonID >= 0 && buttonID < clientStatusButtons.Length)
            {
                clientInfo.assignedButton = clientStatusButtons[buttonID];

                clientInfo.assignedButton.onClick.RemoveAllListeners();
                clientInfo.assignedButton.onClick.AddListener(delegate { ShowClientInfoToUI(connID); });
                clientInfo.assignedButton.GetComponent<Image>().color = Color.yellow;
                clientInfo.assignedButton.GetComponentInChildren<Text>().text = displayName + " " + connID;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Client button index " + buttonID + " is not assigned!");
            }
            cave2Clients[connID] = clientInfo;
        }
        else
        {
            CAVE2ClientInfo clientInfo = new CAVE2ClientInfo();
            clientInfo.connectTime = Time.time;
            clientInfo.connected = true;
            clientInfo.displayName = displayName;
            clientInfo.hostName = hostname;
            clientInfo.connID = connID;

            if (buttonID >= 0 && buttonID < clientStatusButtons.Length)
            {
                clientInfo.assignedButton = clientStatusButtons[buttonID];

                clientInfo.assignedButton.GetComponent<Image>().color = Color.green;
                clientInfo.assignedButton.GetComponentInChildren<Text>().text = displayName + " " + connID;
                clientInfo.assignedButton.onClick.AddListener(delegate { ShowClientInfoToUI(connID); });
            }
            else
            {
                UnityEngine.Debug.LogWarning("Client button index " + buttonID + " is not assigned!");
            }

            cave2Clients.Add(connID, clientInfo);
        }
    }

    public void RemoveClient(int connID)
    {
        UnityEngine.Debug.Log("Client Disconneced: " + connID);

        if (cave2Clients.ContainsKey(connID))
        {
            CAVE2ClientInfo clientInfo = cave2Clients[connID];
            clientInfo.disconnectTime = Time.time;
            clientInfo.connected = false;
            clientInfo.assignedButton.GetComponent<Image>().color = Color.red;
        }
    }

    public void ShowClientInfoToUI(int connID)
    {
        if (cave2Clients.ContainsKey(connID))
        {
            CAVE2ClientInfo clientInfo = cave2Clients[connID];

            clientStatusText.text = "Client: '" + clientInfo.displayName + " " + clientInfo.connID + "'" + "\n";
            clientStatusText.text += (clientInfo.connected ? "CONNECTED" : "NOT CONNECTED") + "\n";
            clientStatusText.text += "Hostname: " + clientInfo.hostName + "\n";
            clientStatusText.text += "ConnID: " + clientInfo.connID + "\n";
            clientStatusText.text += "Connect Time: " + clientInfo.connectTime + "\n";
            clientStatusText.text += "Disconnect Time: " + clientInfo.disconnectTime + "\n";
            clientStatusText.text += "Reconnects: " + clientInfo.reconnects + "\n";
        }
    }
}

public class CAVE2ClientInfo
{
    public int connID;
    public string hostName;
    public string displayName;
    public float connectTime;
    public float disconnectTime;
    public bool connected;
    public Button assignedButton;
    public int reconnects;
}
