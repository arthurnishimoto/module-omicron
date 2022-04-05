﻿using System;
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
    bool enableScreenManagement;

    bool windowAssignmentDone;

    int myWindowPosID;

    [SerializeField]
    bool useCAVE2GeneralizedPerspectiveProjection;

    [Header("Debug")]
    [SerializeField]
    Text debugUIText = null;

    [SerializeField]
    InputField displayWidthUI;

    [SerializeField]
    InputField displayHeightUI;

    [SerializeField]
    InputField displayPosXUI;

    [SerializeField]
    InputField displayPosYUI;

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
        if(debugUIText)
        {
            debugUIText.text = CAVE2Manager.GetMachineName() + "\n";

            Process currentProcess = Process.GetCurrentProcess();
            debugUIText.text += "My Process: '" + currentProcess.ProcessName + "' ID: '" + currentProcess.Id + "' Window title: '" + currentProcess.MainWindowTitle + "'\n";

            /*
            IntPtr windowPtr = FindWindow(null, currentProcess.ProcessName);
            Rect myRect = new Rect();

            GetWindowRect(windowPtr, ref myRect);

            debugUIText.text += "   Window Rect: " + myRect.Left + ", " + myRect.Right + ", " + myRect.Top + ", " + myRect.Bottom + "\n";
            
            
            debugUIText.text += "\nAll Main Window Processes: \n";
            Process[] processlist = Process.GetProcesses();

            int matchingProcessIndex = 0;
            for(int i = 0; i < processlist.Length; i++)
            {
                Process process = processlist[i];
                if (process.ProcessName == currentProcess.ProcessName)
                {
                    debugUIText.text += "Process: '" + process.ProcessName + "' ID: '" + process.Id + "' Window title: '" + process.MainWindowTitle + "'\n";
                    if(process.Id == currentProcess.Id)
                    {
                        myWindowPosID = matchingProcessIndex;
                    }
                    matchingProcessIndex++;
                }
            }
            */

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
    }

    void SetCAVE2CameraPerspective()
    {
        GeneralizedPerspectiveProjection camera = Camera.main.GetComponent<GeneralizedPerspectiveProjection>();
        if(camera == null)
        {
            camera = Camera.main.gameObject.AddComponent<GeneralizedPerspectiveProjection>();
        }
        else if(camera.enabled == false)
        {
            camera.enabled = true;
        }
        camera.SetHeadTracker(GameObject.Find("CAVE2-PlayerController/Head").transform);
        camera.SetVirtualCamera(Camera.main);
        camera.SetApplyHeadOffset(false);

        RemoteTerminal terminal = GameObject.Find("Terminal").GetComponent<RemoteTerminal>();

        terminal.PrintUI("Setting Camera Perspective for " + CAVE2Manager.GetMachineName() + " WindowID: " + myWindowPosID);
        if (CAVE2Manager.GetMachineName() == "ORION") //Backwall
        {
            camera.transform.Rotate(Vector3.up * 180);
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
        else if (CAVE2Manager.GetMachineName() == "ORION-01")
        {
            switch (myWindowPosID)
            {
                case (0): // Lyra-01
                    camera.SetScreenUL(new Vector3(-0.5437923f, 2.605996f, -3.239587f));
                    camera.SetScreenLL(new Vector3(-0.5437923f, 0.3060037f, -3.239587f));
                    camera.SetScreenLR(new Vector3(-1.512198f, 0.3060037f, -2.916143f));
                    camera.transform.Rotate(Vector3.up * -161.531f);
                    break;
                case (1): // Lyra-03
                    camera.SetScreenUL(new Vector3(-1.518851f, 2.605996f, -2.912683f));
                    camera.SetScreenLL(new Vector3(-1.518851f, 0.3060037f, -2.912683f));
                    camera.SetScreenLR(new Vector3(-2.339788f, 0.3060037f, -2.305651f));
                    camera.transform.Rotate(Vector3.up * -143.519f);
                    break;
                case (2): // Lyra-05
                    camera.SetScreenUL(new Vector3(-2.345045f, 2.605996f, -2.300303f));
                    camera.SetScreenLL(new Vector3(-2.345045f, 0.3060037f, -2.300303f));
                    camera.SetScreenLR(new Vector3(-2.938052f, 0.3060037f, -1.469178f));
                    camera.transform.Rotate(Vector3.up * -125.508f);
                    break;
            }
        }
        else if (CAVE2Manager.GetMachineName() == "ORION-02")
        {
            switch (myWindowPosID)
            {
                case (0): // Lyra-07
                    camera.SetScreenUL(new Vector3(-2.941398f, 2.605996f, -1.462467f));
                    camera.SetScreenLL(new Vector3(-2.941398f, 0.3060037f, -1.462467f));
                    camera.SetScreenLR(new Vector3(-3.248353f, 0.3060037f, -0.4887097f));
                    camera.transform.Rotate(Vector3.up * -107.496f);
                    break;
                case (1): // Lyra-09
                    camera.SetScreenUL(new Vector3(-3.24946f, 2.605996f, -0.4812928f));
                    camera.SetScreenLL(new Vector3(-3.24946f, 0.3060037f, -0.4812928f));
                    camera.SetScreenLR(new Vector3(-3.240278f, 0.3060037f, 0.539658f));
                    camera.transform.Rotate(Vector3.up * -89.485f);
                    break;
                case (2): // Lyra-11
                    camera.SetScreenUL(new Vector3(-3.239037f, 2.605996f, 0.5470539f));
                    camera.SetScreenLL(new Vector3(-3.239037f, 0.3060037f, 0.5470539f));
                    camera.SetScreenLR(new Vector3(-2.914619f, 0.3060037f, 1.515133f));
                    camera.transform.Rotate(Vector3.up * -71.47301f);
                    break;
            }
        }
        else if (CAVE2Manager.GetMachineName() == "ORION-03")
        {
            switch (myWindowPosID)
            {
                case (0): // Lyra-13
                    camera.SetScreenUL(new Vector3(-2.911153f, 2.605996f, 1.521783f));
                    camera.SetScreenLL(new Vector3(-2.911153f, 0.3060037f, 1.521783f));
                    camera.SetScreenLR(new Vector3(-2.303294f, 0.3060037f, 2.342108f));
                    camera.transform.Rotate(Vector3.up * -53.462f);
                    break;
                case (1): // Lyra-15
                    camera.SetScreenUL(new Vector3(-2.297941f, 2.605996f, 2.34736f));
                    camera.SetScreenLL(new Vector3(-2.297941f, 0.3060037f, 2.34736f));
                    camera.SetScreenLR(new Vector3(-1.46622f, 0.3060037f, 2.939529f));
                    camera.transform.Rotate(Vector3.up * -35.45f);
                    break;
                case (2): // Lyra-17
                    camera.SetScreenLL(new Vector3(-1.459505f, 0.3060037f, 2.942869f));
                    camera.SetScreenLR(new Vector3(-0.4854392f, 0.3060037f, 3.248843f));
                    camera.SetScreenUL(new Vector3(-1.459505f, 2.605996f, 2.942869f));
                    camera.transform.Rotate(Vector3.up * -17.439f);
                    break;
            }
        }
        else if (CAVE2Manager.GetMachineName() == "ORION-04")
        {
            switch (myWindowPosID)
            {
                case (0): // Lyra-19
                    camera.SetScreenUL(new Vector3(-0.478021f, 2.605996f, 3.249943f));
                    camera.SetScreenLL(new Vector3(-0.478021f, 0.3060037f, 3.249943f));
                    camera.SetScreenLR(new Vector3(0.54292f, 0.3060037f, 3.239733f));
                    camera.transform.Rotate(Vector3.up * 0.573f);
                    break;
                case (1): // Lyra-21
                    camera.SetScreenUL(new Vector3(0.5503147f, 2.605996f, 3.238485f));
                    camera.SetScreenLL(new Vector3(0.5503147f, 0.3060037f, 3.238485f));
                    camera.SetScreenLR(new Vector3(1.518067f, 0.3060037f, 2.913092f));
                    camera.transform.Rotate(Vector3.up * 18.584f);
                    break;
                case (2): // Lyra-23
                    camera.SetScreenUL(new Vector3(1.524713f, 2.605996f, 2.909619f));
                    camera.SetScreenLL(new Vector3(1.524713f, 0.3060037f, 2.909619f));
                    camera.SetScreenLR(new Vector3(2.344425f, 0.3060037f, 2.300935f));
                    camera.transform.Rotate(Vector3.up * 36.596f);
                    break;
            }
        }
        else if (CAVE2Manager.GetMachineName() == "ORION-05")
        {
            switch (myWindowPosID)
            {
                case (0): // Lyra-25
                    camera.SetScreenUL(new Vector3(2.349672f, 2.605996f, 2.295577f));
                    camera.SetScreenLL(new Vector3(2.349672f, 0.3060037f, 2.295577f));
                    camera.SetScreenLR(new Vector3(2.941004f, 0.3060037f, 1.463259f));
                    camera.transform.Rotate(Vector3.up * 54.608f);
                    break;
                case (1): // Lyra-27
                    camera.SetScreenUL(new Vector3(2.944337f, 2.605996f, 1.456542f));
                    camera.SetScreenLL(new Vector3(2.944337f, 0.3060037f, 1.456542f));
                    camera.SetScreenLR(new Vector3(3.24933f, 0.3060037f, 0.4821681f));
                    camera.transform.Rotate(Vector3.up * 72.619f);
                    break;
                case (2): // Lyra-29
                    camera.SetScreenUL(new Vector3(3.250422f, 2.605996f, 0.4747489f));
                    camera.SetScreenLL(new Vector3(3.250422f, 0.3060037f, 0.4747489f));
                    camera.SetScreenLR(new Vector3(3.239185f, 0.3060037f, -0.5461813f));
                    camera.transform.Rotate(Vector3.up * 90.631f);
                    break;
            }
        }
        else if (CAVE2Manager.GetMachineName() == "ORION-06")
        {
            switch (myWindowPosID)
            {
                case (0): // Lyra-31
                    camera.SetScreenUL(new Vector3(3.237929f, 2.605996f, -0.5535748f));
                    camera.SetScreenLL(new Vector3(3.237929f, 0.3060037f, -0.5535748f));
                    camera.SetScreenLR(new Vector3(2.911562f, 0.3060037f, -1.520999f));
                    camera.transform.Rotate(Vector3.up * 108.642f);
                    break;
                case (1): // Lyra-33
                    camera.SetScreenUL(new Vector3(2.908083f, 2.605996f, -1.527641f));
                    camera.SetScreenLL(new Vector3(2.908083f, 0.3060037f, -1.527641f));
                    camera.SetScreenLR(new Vector3(2.298573f, 0.3060037f, -2.346741f));
                    camera.transform.Rotate(Vector3.up * 126.654f);
                    break;
                case (2): // Lyra-35
                    camera.SetScreenUL(new Vector3(2.29321f, 2.605996f, -2.351982f));
                    camera.SetScreenLL(new Vector3(2.29321f, 0.3060037f, -2.351982f));
                    camera.SetScreenLR(new Vector3(1.460298f, 0.3060037f, -2.942476f));
                    camera.transform.Rotate(Vector3.up * 144.665f);
                    break;
            }
        }
        else if (CAVE2Manager.GetMachineName() == "ORION-WIN")
        {
            switch (myWindowPosID)
            {
                case (0):
                    camera.SetScreenLL(new Vector3(-2.3f, 0.29f, 2.36f));
                    camera.SetScreenLR(new Vector3(2.35f, 0.29f, 2.31f));
                    camera.SetScreenUL(new Vector3(-2.3f, 2.61f, 2.36f));
                    break;
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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
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
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
    public static extern bool GetWindowThreadProcessId(System.IntPtr hwnd, out System.Int32 lpdwProcessId);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
    public static extern long SetWindowLongA(System.IntPtr hwnd, System.Int32 nIndex, System.Int64 dwNewLong);
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

    public static void SetPosition(int connID, int x, int y, int resX = 0, int resY = 0)
    {
        const int SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        IntPtr windowPtr = FindWindow(null, Application.productName + " " + connID);

        // https://answers.unity.com/questions/946630/borderless-windows-in-standalone-builds.html
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlonga
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles
        // Sets window to borderless
        SetWindowLongA(windowPtr, GWL_STYLE, 0x00800000L);

        //https://answers.unity.com/questions/13523/is-there-a-way-to-set-the-position-of-a-standalone.html?_ga=2.54933416.2072224053.1643235688-1899960085.1550115936
        // Sets the window position and shows window (last flag required after setting style above)
        SetWindowPos(windowPtr, 0, x, y, resX, resY, SWP_SHOWWINDOW);
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
    public static void SetPosition(int connID, int x, int y, int resX = 0, int resY = 0)
    {
        Debug.LogWarning("Not implemented on current platform");
    }

    public static void SetWindowTitle(int connID, int oldID = 0)
    {
        Debug.LogWarning("Not implemented on current platform");
    }

    public static int GetWindowProcessId(int connID)
    {
        int id = -1;
        Debug.LogWarning("Not implemented on current platform");
        return id;
    }
#endif
}