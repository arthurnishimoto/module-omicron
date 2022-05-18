using UnityEngine;
using System.Collections;

public class VRDisplayManager : MonoBehaviour {

    public Transform headTrackedUser;
    public Transform virtualHead;
    public float displayAngularOffset = 1;

    public bool hideScreenBorders;
    int screenBorderState = -1;

    public LayerMask VRDisplayMask;
    LayerMask lastVRDisplayMask;

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
}
