using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadNodeDebugManager : MonoBehaviour
{
    Canvas mainCanvas;

    [Header("Menu Panels")]
    [SerializeField]
    GameObject applicationPanel;

    [SerializeField]
    GameObject trackingSystemPanel;

    [Header("Tracking System")]
    //[SerializeField]
    //Button trackingSystemButton;

    [SerializeField]
    Toggle connectToServer;

    [SerializeField]
    InputField serverIP;

    [SerializeField]
    Text connectionStatus;

    [SerializeField]
    InputField msgPort;

    [SerializeField]
    InputField dataPort;

    [SerializeField]
    Text primaryHeadTrackerPosRot;

    [SerializeField]
    Toggle continuum3DMode;

    [SerializeField]
    Toggle continuumMainMode;

    OmicronManager omicronManager;

    // FPS
    [SerializeField]
    Text fpsText;

    public float FPS_updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval
    float curFPS;
    Vector2 minMaxFPS = new Vector2(10000, 0);
    Vector2 minMaxFPSTime = new Vector2();
    Vector2 avgFPS = new Vector2();
    float avgFPSStartTime = 0;

    [SerializeField]
    float avgFPSTimeLimit = 15;

    [SerializeField]
    bool resetAvgFPSTimer = false;


    // Start is called before the first frame update
    void Start()
    {
        mainCanvas = GetComponent<Canvas>();
        mainCanvas.enabled = false;

        omicronManager = GetComponentInParent<OmicronManager>();

        serverIP.text = omicronManager.serverIP;
        msgPort.text = omicronManager.serverMsgPort.ToString();
        dataPort.text = omicronManager.dataPort.ToString();

        continuum3DMode.SetIsOnWithoutNotify(omicronManager.continuum3DXAxis);
        continuumMainMode.SetIsOnWithoutNotify(omicronManager.continuumMainInvertX);
    }

    // Update is called once per frame
    void Update()
    {
        if (CAVE2.IsMaster())
        {
            CalculateFPS();

            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
            {
                mainCanvas.enabled = !mainCanvas.enabled;
            }

            if(mainCanvas.enabled)
            {
                fpsText.text = "FPS Current:\t\t\t" + System.String.Format("{0:F2}", curFPS) + "\t\t";
                fpsText.text += "Time:\t" + System.String.Format("{0:F2}", Time.time) + "\n";
                fpsText.text += "FPS Min (Time): ";
                fpsText.text += "\t\t" + System.String.Format("{0:F2} ({1:F2})", minMaxFPS.x, minMaxFPSTime.x);
                fpsText.text += "\nFPS Max (Time): ";
                fpsText.text += "\t" + System.String.Format("{0:F2} ({1:F2})", minMaxFPS.y, minMaxFPSTime.y);

                connectToServer.SetIsOnWithoutNotify(omicronManager.IsConnectedToServer());

                switch(omicronManager.GetConnectionState())
                {
                    case (OmicronManager.ConnectionState.Connected):
                        connectionStatus.text = "Connected";
                        connectionStatus.color = Color.green;
                        break;
                    case (OmicronManager.ConnectionState.Connecting):
                        connectionStatus.text = "Connecting";
                        connectionStatus.color = Color.white;
                        break;
                    case (OmicronManager.ConnectionState.FailedToConnect):
                        connectionStatus.text = "Failed To Connect";
                        connectionStatus.color = Color.red;
                        break;
                    default: // OmicronManager.ConnectionState.NotConnected
                        connectionStatus.text = "Not Connected";
                        connectionStatus.color = Color.white;
                        break;
                }

                primaryHeadTrackerPosRot.text = CAVE2.GetHeadPosition(1).ToString() + "\n";
                primaryHeadTrackerPosRot.text += CAVE2.GetHeadRotation(1).eulerAngles.ToString();
            }
        }
    }

    void CalculateFPS()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            curFPS = accum / frames;
            // formattedFPS = System.String.Format("{0:F2} FPS",fps);
            if (curFPS < minMaxFPS.x)
            {
                minMaxFPS.x = curFPS;
                minMaxFPSTime.x = Time.time;
            }
            if (Time.time > 1 && curFPS > minMaxFPS.y)
            {
                minMaxFPS.y = curFPS;
                minMaxFPSTime.y = Time.time;
            }

            if (avgFPSTimeLimit > 0 && Time.time - avgFPSStartTime <= avgFPSTimeLimit)
            {
                avgFPS.x += curFPS;
                avgFPS.y++;
            }

            if (resetAvgFPSTimer)
            {
                avgFPSStartTime = Time.time;
            }

            //	DebugConsole.Log(format,level);
            timeleft = FPS_updateInterval;
            accum = 0.0F;
            frames = 0;
        }
    }

    public void ToggleApplicationPanel()
    {
        if (applicationPanel && applicationPanel.activeSelf)
        {
            applicationPanel.SetActive(false);
        }
        else if (applicationPanel)
        {
            applicationPanel.SetActive(true);
        }
    }

    public void ToggleTrackingSystemPanel()
    {
        if(trackingSystemPanel.activeSelf)
        {
            trackingSystemPanel.SetActive(false);
        }
        else
        {
            trackingSystemPanel.SetActive(true);
        }
    }

    public void SetServerIP(string serverIP)
    {
        omicronManager.serverIP = serverIP;
    }

    public void SetMsgPort(string port)
    {
        omicronManager.serverMsgPort = int.Parse(port);
    }

    public void SetDataPort(string port)
    {
        omicronManager.dataPort = int.Parse(port);
    }

    public void ToggleConnectToServer(bool toggle)
    {
        if (toggle == false && omicronManager.IsConnectedToServer())
        {
            omicronManager.DisconnectServer();
        }
        else if (toggle == true && !omicronManager.IsConnectedToServer())
        {
            omicronManager.ConnectToServer();
        }
    }

    public void ToggleContinuum3DMode(bool toggle)
    {
        omicronManager.continuum3DXAxis = toggle;
    }

    public void ToggleContinuumMainMode(bool toggle)
    {
        omicronManager.continuumMainInvertX = toggle;
    }
}
