using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StandaloneDisplayManager : MonoBehaviour
{
    [Header("UI")]
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
}
