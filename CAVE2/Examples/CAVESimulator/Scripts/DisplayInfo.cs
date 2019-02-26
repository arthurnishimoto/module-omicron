using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayInfo : MonoBehaviour {

    // getReal3D screen parameters
    public Vector3 Px_UpperLeft;
    public Vector3 Px_LowerLeft;
    public Vector3 Px_LowerRight;

    // CalVR screen parameters
    public Vector3 origin; // pixel origin
    public float h; // Screen rotation


    Transform trackingOrigin;

    [SerializeField]
    Vector3 trackingOriginPos;

    // Use this for initialization
    void Start() {
        UpdateDisplayInfo();

        if (GetComponentInParent<VRDisplayManager>())
        {
            trackingOrigin = GetComponentInParent<VRDisplayManager>().headTrackedUser.parent;
            trackingOriginPos = trackingOrigin.position;
        }
    }

    public void UpdateDisplayInfo()
    {
        Px_UpperLeft = gameObject.transform.Find("Borders/PixelSpace/Px-UpperLeft").position;
        Px_LowerLeft = gameObject.transform.Find("Borders/PixelSpace/Px-LowerLeft").position;
        Px_LowerRight = gameObject.transform.Find("Borders/PixelSpace/Px-LowerRight").position;

        origin = gameObject.transform.Find("Borders/PixelSpace").position;
        h = gameObject.transform.Find("Borders/PixelSpace").eulerAngles.y;

        // Convert from world space to local tracker space
        if (GetComponentInParent<VRDisplayManager>())
        {
            trackingOrigin = GetComponentInParent<VRDisplayManager>().headTrackedUser.parent;
            trackingOriginPos = trackingOrigin.position;

            Px_UpperLeft = Px_UpperLeft - trackingOriginPos;
            Px_LowerLeft = Px_LowerLeft - trackingOriginPos;
            Px_LowerRight = Px_LowerRight - trackingOriginPos;

            origin = trackingOrigin.InverseTransformPoint(origin);
        }
    }
}
