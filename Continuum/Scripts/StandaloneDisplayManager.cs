using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StandaloneDisplayManager : MonoBehaviour
{
    [Header("Display UI")]
    [SerializeField]
    Text currentResolution;

    [SerializeField]
    InputField newWidthInput;

    [SerializeField]
    InputField newHeightInput;

    [SerializeField]
    Toggle fullScreenToggle;

    [SerializeField]
    Text currentWindowPosition;

    [SerializeField]
    InputField newXPosInput;

    [SerializeField]
    InputField newYPosInput;

    [SerializeField]
    Dropdown windowModeDropdown;

    int displayConfigLoaded = -1;

    [Header("Stereoscopic UI")]
    [SerializeField]
    Dropdown stereoMode;

    StereoscopicCamera stereoCamera;

    [SerializeField]
    InputField stereoXRes;

    [SerializeField]
    InputField stereoYRes;

    [SerializeField]
    Toggle stereoAutoRes;

    // Start is called before the first frame update
    void Start()
    {
        newWidthInput.text = Screen.width.ToString();
        newHeightInput.text = Screen.height.ToString();
        fullScreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // Calling in FixedUpdate to update less that Update draw loop

        // Standard Display Options
        currentResolution.text = "Current Resolution: " + Screen.width.ToString() + " x " + Screen.height + " @ " + Screen.currentResolution.refreshRateRatio + " Hz";
        // currentResolution.text += "\n" + Screen.currentResolution.ToString();

        currentWindowPosition.text = "Current Window Position: " + Screen.mainWindowPosition.ToString();
        
        if(displayConfigLoaded == 0)
        {
            DisplayConfig displayConfig = ConfigurationManager.loadedConfig.displayConfig;
            SetDisplayResolution(displayConfig.screenWidth, displayConfig.screenHeight, displayConfig.windowMode);

            int newX = displayConfig.screenXPos;
            int newY = displayConfig.screenYPos;

            Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, new Vector2Int(newX, newY));

            windowModeDropdown.value = displayConfig.windowMode;
            displayConfigLoaded = 1;
        }

        // Stereoscopic Options
        if(stereoCamera == null)
        {
            stereoMode.interactable = false;
            stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();

            if(stereoCamera)
            {
                switch(stereoCamera.GetStereoMode())
                {
                    // stereoMode: None = 0, Interleaved, SideBySide, Checkerboard, Left, Right
                    case (StereoscopicCamera.StereoscopicMode.Interleaved):
                        stereoMode.value = 1;
                        break;
                    case (StereoscopicCamera.StereoscopicMode.SideBySide):
                        stereoMode.value = 2;
                        break;
                    case (StereoscopicCamera.StereoscopicMode.Checkerboard):
                        stereoMode.value = 3;
                        break;
                    case (StereoscopicCamera.StereoscopicMode.Left):
                        stereoMode.value = 4;
                        break;
                    case (StereoscopicCamera.StereoscopicMode.Right):
                        stereoMode.value = 5;
                        break;
                }
            }
        }
        else
        {
            stereoMode.interactable = true;

            if(stereoAutoRes.isOn)
            {
                stereoXRes.interactable = false;
                stereoYRes.interactable = false;

                stereoXRes.text = Screen.width.ToString();
                stereoYRes.text = Screen.height.ToString();

                stereoCamera.SetStereoResolution(new Vector2(Screen.width, Screen.height));
            }
            else
            {
                stereoXRes.interactable = true;
                stereoYRes.interactable = true;

                int newWidth = Screen.width;
                int newHeight = Screen.height;
                int.TryParse(stereoXRes.text, out newWidth);
                int.TryParse(stereoYRes.text, out newHeight);

                stereoCamera.SetStereoResolution(new Vector2(newWidth, newHeight));
            }
        }
    }

    public void SetDisplayResolution()
    {
        int newWidth = Screen.width;
        int newHeight = Screen.height;
        int.TryParse(newWidthInput.text, out newWidth);
        int.TryParse(newHeightInput.text, out newHeight);

        SetDisplayResolution(newWidth, newHeight, windowModeDropdown.value);

        ConfigurationManager.loadedConfig.displayConfig.screenWidth = newWidth;
        ConfigurationManager.loadedConfig.displayConfig.screenHeight = newHeight;
        ConfigurationManager.loadedConfig.displayConfig.windowMode = windowModeDropdown.value;
        ConfigurationManager.loadedConfig.displayConfig.fullscreen = windowModeDropdown.value == 1;
    }

    private void SetDisplayResolution(int newWidth, int newHeight, int windowMode)
    {
        switch (windowMode)
        {
            case (0): Screen.SetResolution(newWidth, newHeight, false); break;
            case (1): Screen.SetResolution(newWidth, newHeight, true); break;
            case (2):
                CAVE2ClusterManager.SetPosition(Screen.mainWindowPosition.x, Screen.mainWindowPosition.y, newWidth, newHeight, true);
                break;
        }
    }

    void ConfigurationLoaded(DefaultConfig config)
    {
#if UNITY_STANDALONE
        displayConfigLoaded = 0;   
#endif
    }

    public void SetWindowPosition()
    {
        int newX = Screen.mainWindowPosition.x;
        int newY = Screen.mainWindowPosition.y;
        int.TryParse(newXPosInput.text, out newX);
        int.TryParse(newYPosInput.text, out newY);

        Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, new Vector2Int(newX, newY));

        ConfigurationManager.loadedConfig.displayConfig.screenXPos = newX;
        ConfigurationManager.loadedConfig.displayConfig.screenYPos = newY;
    }

    public void SetStereoMode(int mode)
    {
        if(stereoCamera != null)
        {
            switch (mode)
            {
                // stereoMode: None = 0, Interleaved, SideBySide, Checkerboard, Left, Right
                case (1):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Interleaved);
                    break;
                case (2):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.SideBySide);
                    break;
                case (3):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Checkerboard);
                    break;
                case (4):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Left);
                    break;
                case (5):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Right);
                    break;
                default:
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Left);
                    break;
            }
        }
    }

    public void ToggleAutoStereoRes(bool value)
    {
        if (stereoCamera != null)
        {
            if(value)
            {
                stereoCamera.SetStereoResolution(new Vector2(Screen.width, Screen.height));
            }
        }
    }

    public void InvertStereo(bool value)
    {
        if (stereoCamera != null)
        {
            stereoCamera.SetStereoInverted(value);
        }
    }
}
