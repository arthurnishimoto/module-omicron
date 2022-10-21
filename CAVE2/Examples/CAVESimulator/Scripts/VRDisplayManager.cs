using UnityEngine;
using System.Collections;

public class VRDisplayManager : MonoBehaviour {

    [SerializeField]
    Transform headTrackedUser;

    [SerializeField]
    Transform headTrackedUserLeftEye;

    [SerializeField]
    Transform headTrackedUserRightEye;

    public Transform virtualHead;
    public float displayAngularOffset = 1;

    public bool hideScreenBorders;
    int screenBorderState = -1;

    public LayerMask VRDisplayMask;
    LayerMask lastVRDisplayMask;

    public CameraClearFlags vrCameraClearFlag = CameraClearFlags.Skybox;
    public Color vrCameraBGColor = new Color(0.1921569f, 0.3019608f, 0.4745098f); // Default Unity blue

    [SerializeField]
    bool regenerateDisplays;

    private void Update()
    {
        if(hideScreenBorders && screenBorderState != 0)
        {
            BroadcastMessage("HideDisplayBorders");
            screenBorderState = 0;
        }
        else if (!hideScreenBorders && screenBorderState != 1)
        {
            BroadcastMessage("ShowDisplayBorders");
            screenBorderState = 1;
        }

        if(VRDisplayMask != lastVRDisplayMask)
        {
            BroadcastMessage("SetVRDisplayMask", VRDisplayMask);
            lastVRDisplayMask = VRDisplayMask;
        }

        if(regenerateDisplays)
        {
            GetComponentInChildren<ScreenConfigCalc>().RegenerateDisplayWall();
            regenerateDisplays = false;
        }
    }

    public void UpdateDisplayAngularOffset(float offset)
    {
        displayAngularOffset = offset;
        regenerateDisplays = true;
    }

    public Transform GetHeadTrackedUser()
    {
        return headTrackedUser;
    }

    public Transform GetHeadTrackedLeftEye()
    {
        return headTrackedUserLeftEye;
    }

    public Transform GetHeadTrackedRightEye()
    {
        return headTrackedUserRightEye;
    }
}
