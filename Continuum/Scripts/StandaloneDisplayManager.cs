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

    [SerializeField]
    Toggle stereoInvertToggle;

    Vector2Int lastStereoRes;
    int lastStereoMode = 0;

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

            int newX = displayConfig.screenXPos;
            int newY = displayConfig.screenYPos;

            // Don't mess with window in editor
#if !UNITY_EDITOR
             // If -1, use current position instead (config null value)
            if(newX == -1)
            {
                newX = Screen.mainWindowPosition.x;
            }
            if(newY == -1)
            {
                newY = Screen.mainWindowPosition.y;
            }
            Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, new Vector2Int(newX, newY));
#endif
            // windowModeDropdown.value = displayConfig.windowMode;
            if (displayConfig.windowMode.ToLower() == "windowed")
            {
                windowModeDropdown.value = 0;
            }
            else if (displayConfig.windowMode.ToLower() == "fullscreen")
            {
                windowModeDropdown.value = 1;
            }
            else if (displayConfig.windowMode.ToLower() == "borderless" || displayConfig.windowMode.ToLower() == "borderless window")
            {
                windowModeDropdown.value = 2;
            }
            SetDisplayResolution(displayConfig.screenWidth, displayConfig.screenHeight, newX, newY, windowModeDropdown.value);

            // Stereoscopic
            StereoscopicConfig stereoConfig = ConfigurationManager.loadedConfig.stereoscopicConfig;
            if (stereoCamera == null)
            {
                stereoMode.interactable = false;
                stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();
            }
            if (stereoCamera)
            {
                stereoCamera.SetStereoInverted(stereoConfig.invertStereo);
                stereoInvertToggle.SetIsOnWithoutNotify(stereoConfig.invertStereo);
                stereoAutoRes.SetIsOnWithoutNotify(stereoConfig.autoStereoResolution);

                if (stereoConfig.stereoMode.ToLower() == "interleaved")
                {
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Interleaved);
                    stereoMode.value = 1;
                }
                else if (stereoConfig.stereoMode.ToLower() == "sidebyside")
                {
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.SideBySide);
                    stereoMode.value = 2;
                }
                else if (stereoConfig.stereoMode.ToLower() == "checkerboard")
                {
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Checkerboard);
                    stereoMode.value = 3;
                }
                else if (stereoConfig.stereoMode.ToLower() == "left")
                {
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Checkerboard);
                    stereoMode.value = 4;
                }
                else if (stereoConfig.stereoMode.ToLower() == "right")
                {
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Checkerboard);
                    stereoMode.value = 5;
                }
                else if (stereoConfig.stereoMode.ToLower() == "none")
                {
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Left);
                    stereoMode.value = 0;
                }
                lastStereoMode = stereoMode.value;
            }


            // Gemeralized Perspective Projection
            if(displayConfig.useGeneralizedPerspectiveProjection)
            {
                GeneralizedPerspectiveProjection[] gpps = Camera.main.GetComponentsInChildren<GeneralizedPerspectiveProjection>();
                foreach(GeneralizedPerspectiveProjection proj in gpps)
                {
                    proj.enabled = true;
                    proj.SetScreenUL(displayConfig.screenUL);
                    proj.SetScreenLL(displayConfig.screenLL);
                    proj.SetScreenLR(displayConfig.screenLR);
                }
            }

            displayConfigLoaded = 1;
        }

        // Stereoscopic Options
        if(stereoCamera == null)
        {
            stereoMode.interactable = false;
            stereoCamera = Camera.main.GetComponent<StereoscopicCamera>();
        }
        else
        {
            stereoMode.interactable = true;

            if (lastStereoMode != stereoMode.value)
            {
                SetStereoMode(stereoMode.value);    
                lastStereoMode = stereoMode.value;
            }

            if (stereoAutoRes.isOn)
            {
                stereoXRes.interactable = false;
                stereoYRes.interactable = false;

                stereoXRes.text = Screen.width.ToString();
                stereoYRes.text = Screen.height.ToString();

                if (lastStereoRes.x != Screen.width || lastStereoRes.y != Screen.height)
                {
                    stereoCamera.SetStereoResolution(new Vector2(Screen.width, Screen.height));

                    lastStereoRes.x = Screen.width;
                    lastStereoRes.y = Screen.height;
                }
            }
            else
            {
                stereoXRes.interactable = true;
                stereoYRes.interactable = true;

                int newWidth = Screen.width;
                int newHeight = Screen.height;
                int.TryParse(stereoXRes.text, out newWidth);
                int.TryParse(stereoYRes.text, out newHeight);

                if (lastStereoRes.x != newWidth || lastStereoRes.y != newHeight)
                {
                    stereoCamera.SetStereoResolution(new Vector2(newWidth, newHeight));

                    lastStereoRes.x = newWidth;
                    lastStereoRes.y = newHeight;
                }

                
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

        switch(windowModeDropdown.value)
        {
            case (0): ConfigurationManager.loadedConfig.displayConfig.windowMode = "Windowed"; break;
            case (1): ConfigurationManager.loadedConfig.displayConfig.windowMode = "Fullscreen"; break;
            case (2): ConfigurationManager.loadedConfig.displayConfig.windowMode = "Borderless Window"; break;
        }
        
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

    private void SetDisplayResolution(int newWidth, int newHeight, int newX, int newY, int windowMode)
    {
        switch (windowMode)
        {
            case (0): Screen.SetResolution(newWidth, newHeight, false); break;
            case (1): Screen.SetResolution(newWidth, newHeight, true); break;
            case (2):
                CAVE2ClusterManager.SetPosition(newX, newY, newWidth, newHeight, true);
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
                    UpdateStereoConfigMode("Interleaved");
                    break;
                case (2):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.SideBySide);
                    UpdateStereoConfigMode("SideBySide");
                    break;
                case (3):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Checkerboard);
                    UpdateStereoConfigMode("Checkerboard");
                    break;
                case (4):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Left);
                    UpdateStereoConfigMode("Left");
                    break;
                case (5):
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Right);
                    UpdateStereoConfigMode("Right");
                    break;
                default:
                    stereoCamera.SetStereoMode(StereoscopicCamera.StereoscopicMode.Left);
                    UpdateStereoConfigMode("None");
                    break;
            }
        }
    }

    void UpdateStereoConfigMode(string mode)
    {
        if (ConfigurationManager.loadedConfig != null)
        {
            ConfigurationManager.loadedConfig.stereoscopicConfig.stereoMode = mode;
        }
    }

    public void ToggleAutoStereoRes(bool value)
    {
        if (stereoCamera != null)
        {
            if(value)
            {
                stereoCamera.SetStereoResolution(new Vector2(Screen.width, Screen.height));
                ConfigurationManager.loadedConfig.stereoscopicConfig.autoStereoResolution = value;
            }
        }
    }

    public void InvertStereo(bool value)
    {
        if (stereoCamera != null)
        {
            stereoCamera.SetStereoInverted(value);
            ConfigurationManager.loadedConfig.stereoscopicConfig.invertStereo = value;
        }
    }
}
