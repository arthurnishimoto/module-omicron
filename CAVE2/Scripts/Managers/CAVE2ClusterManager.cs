using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CAVE2ClusterManager : MonoBehaviour
{
    [SerializeField]
    OmicronManager omicronManager = null;

    [SerializeField]
    CAVE2RPCManager rpcManager = null;

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
            rpcManager.useMsgServer = false;
            rpcManager.useMsgClient = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(debugUIText)
        {
            debugUIText.text = CAVE2Manager.GetMachineName() + "\n";
            debugUIText.text += "ConnID: " + rpcManager.GetConnID() + "\n";

            debugUIText.text += "\nDisplays: \n";
            Display[] displays = Display.displays;
            int displayID = 0;
            foreach (Display d in displays)
            {
                if(firstUpdate && displayID == 0)
                {
                    mainDisplay = d;
                    displayWidthUI.SetTextWithoutNotify(d.renderingWidth+"");
                    displayHeightUI.SetTextWithoutNotify(d.renderingHeight + "");
                    
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

    public static void SetPosition(int connID, int x, int y, int resX = 0, int resY = 0)
    {
        SetWindowPos(FindWindow(null, Application.productName + " " + connID), 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
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

#endif
}
