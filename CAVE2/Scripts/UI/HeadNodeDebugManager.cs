using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadNodeDebugManager : MonoBehaviour
{
    enum MenuMode { Hidden, Visible, Application, Tracking, Performance, Display};

    [SerializeField]
    MenuMode initialMenuState = MenuMode.Hidden;

    Canvas mainCanvas;

    [Header("Menu Panels")]
    [SerializeField]
    GameObject applicationPanel;

    [SerializeField]
    GameObject trackingSystemPanel;

    [SerializeField]
    GameObject performancePanel;

    [SerializeField]
    GameObject displayPanel;

    [Header("Tracking System")]
    //[SerializeField]
    //Button trackingSystemButton;

    [SerializeField]
    Toggle connectToServer;

    [SerializeField]
    InputField serverIPField;

    [SerializeField]
    Text connectionStatus;

    [SerializeField]
    InputField msgPortField;

    [SerializeField]
    InputField dataPortField;

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

    [SerializeField]
    Text timeText;

    ObjectCountStressTestCounter fpsCounter;

    // Start is called before the first frame update
    void Start()
    {
        mainCanvas = GetComponent<Canvas>();
        fpsCounter = GetComponent<ObjectCountStressTestCounter>();

        omicronManager = GetComponentInParent<OmicronManager>();

        serverIPField.text = omicronManager.serverIP;
        msgPortField.text = omicronManager.serverMsgPort.ToString();
        dataPortField.text = omicronManager.dataPort.ToString();

        continuum3DMode.SetIsOnWithoutNotify(omicronManager.continuum3DXAxis);
        continuumMainMode.SetIsOnWithoutNotify(omicronManager.continuumMainInvertX);

        if (applicationPanel)
        {
            applicationPanel.SetActive(false);
        }
        if (trackingSystemPanel)
        {
            trackingSystemPanel.SetActive(false);
        }
        if (performancePanel)
        {
            performancePanel.SetActive(false);
        }
        if (displayPanel)
        {
            displayPanel.SetActive(false);
        }

        switch (initialMenuState)
        {
            case (MenuMode.Hidden):
                mainCanvas.enabled = false;
                break;
            case (MenuMode.Visible):
                mainCanvas.enabled = true;
                break;
            case (MenuMode.Application):
                if (applicationPanel)
                {
                    applicationPanel.SetActive(true);
                }
                break;
            case (MenuMode.Tracking):
                if (trackingSystemPanel)
                {
                    trackingSystemPanel.SetActive(true);
                }
                break;
            case (MenuMode.Performance):
                if (performancePanel)
                {
                    performancePanel.SetActive(true);
                }
                break;
            case (MenuMode.Display):
                if (displayPanel)
                {
                    displayPanel.SetActive(true);
                }
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CAVE2.IsMaster())
        {
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
            {
                mainCanvas.enabled = !mainCanvas.enabled;
            }

            if(mainCanvas.enabled)
            {
                timeText.text = "Time:\t" + System.String.Format("{0:F2}", Time.time) + "\n";
                fpsCounter.SetFPSText(fpsText);

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

    void ConfigurationLoaded(DefaultConfig config)
    {
        mainCanvas.enabled = ConfigurationManager.loadedConfig.showDebugMenu;
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

    public void TogglePerformancePanel()
    {
        if (performancePanel.activeSelf)
        {
            performancePanel.SetActive(false);
        }
        else
        {
            performancePanel.SetActive(true);
        }
    }

    public void ToggleDisplayPanel()
    {
        if (displayPanel.activeSelf)
        {
            displayPanel.SetActive(false);
        }
        else
        {
            displayPanel.SetActive(true);
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

    public void UpdateTrackingUI()
    {
        serverIPField.text = omicronManager.serverIP;
        msgPortField.text = omicronManager.serverMsgPort.ToString();
        dataPortField.text = omicronManager.dataPort.ToString();

        continuumMainMode.SetIsOnWithoutNotify(omicronManager.continuumMainInvertX);
        continuum3DMode.SetIsOnWithoutNotify(omicronManager.continuum3DXAxis);
    }
}
