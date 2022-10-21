using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadNodeDisplayModePanel : MonoBehaviour
{
    [SerializeField]
    int buttonID = 0;

    [SerializeField]
    ColorSelectorUI colorSelector = null;

    // Start is called before the first frame update
    void Start()
    {

    }
    
    public void SetDisplayMode(int displayMode)
    {
        CAVE2CameraController cameraController = CAVE2.GetCameraController();

        switch((CAVE2RemoteDisplayManager.DisplayMode)displayMode)
        {
            case (CAVE2RemoteDisplayManager.DisplayMode.Normal):
                cameraController.GetComponent<CAVE2RemoteDisplayManager>().SetDisplayNormal(buttonID);
                break;
            case (CAVE2RemoteDisplayManager.DisplayMode.Overlay_Black):
                cameraController.GetComponent<CAVE2RemoteDisplayManager>().SetDisplayBlack(buttonID);
                break;
            case (CAVE2RemoteDisplayManager.DisplayMode.Overlay_Custom):
                Color customColor = colorSelector.GetOutputColor();
                cameraController.GetComponent<CAVE2RemoteDisplayManager>().SetDisplayCustom(buttonID, customColor);
                break;
        }
        
    }

}
