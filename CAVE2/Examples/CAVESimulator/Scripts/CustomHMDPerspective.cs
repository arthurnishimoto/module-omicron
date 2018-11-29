using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomHMDPerspective : GeneralizedPerspectiveProjection {

    protected DisplayInfo displayInfo;

    [SerializeField]
    Vector2 displayResolution = new Vector2(1366, 768);

    [SerializeField]
    Vector3 displaySize = new Vector3(0.3f, 0.2f, 0.01f);

    [SerializeField]
    Vector3 displayOffset = new Vector3(0.0f, 0.0f, 0.734f);

    [SerializeField]
    Transform vrDisplay;

    protected GameObject vrCamera;

    public int virtualCameraCullingMask;

    // Set this to true if rendering to a virtual display
    // Disable if say rendering to an HMD display
    [SerializeField]
    protected bool renderTextureToVRCamera = true;

    [SerializeField]
    protected Vector3 HMDScreenUL = new Vector3(-1.0215f, 2.476f, -0.085972f);

    [SerializeField]
    protected Vector3 HMDScreenLL = new Vector3(-1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    protected Vector3 HMDScreenLR = new Vector3(1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    Vector3 headProjectionOffset;

    [SerializeField]
    Vector3 headOriginOffset;

    [SerializeField]
    Transform headOrigin;

    void Start()
    {
        displayInfo = GetComponent<DisplayInfo>();

        screenUL = displayInfo.Px_UpperLeft;
        screenLL = displayInfo.Px_LowerLeft;
        screenLR = displayInfo.Px_LowerRight;

        head = GetComponentInParent<VRDisplayManager>().headTrackedUser;

        vrCamera = new GameObject(gameObject.name + " (VR Camera)");
        vrCamera.transform.parent = GetComponentInParent<VRDisplayManager>().virtualHead;
        vrCamera.transform.localPosition = Vector3.zero;
        vrCamera.transform.localEulerAngles = new Vector3(0, displayInfo.h + GetComponentInParent<VRDisplayManager>().displayAngularOffset, 0);

        virtualCamera = vrCamera.AddComponent<Camera>();
        virtualCamera.cullingMask = virtualCameraCullingMask;

        RenderTexture cameraRT = new RenderTexture((int)displayResolution.x, (int)displayResolution.y, 16);
        if (renderTextureToVRCamera)
            virtualCamera.targetTexture = cameraRT;

        Material displayMat = new Material(Shader.Find("Unlit/Texture"));
        displayMat.name = gameObject.name + " (VR Camera Material)";
        displayMat.mainTexture = cameraRT;

        Transform displaySpace = transform.Find("Borders/PixelSpace");
        displaySpace.GetComponent<MeshRenderer>().material = displayMat;
        //displaySpace.gameObject.layer = GetComponentInParent<VRDisplayManager>().gameObject.layer;
    }

    void LateUpdate () {
        vrDisplay.localScale = displaySize;
        vrDisplay.localPosition = displayOffset;
        headOrigin.localPosition = headOriginOffset;

        displayInfo.UpdateDisplayInfo();
        HMDScreenUL = displayInfo.Px_UpperLeft;
        HMDScreenLL = displayInfo.Px_LowerLeft;
        HMDScreenLR = displayInfo.Px_LowerRight;

        screenLL = headProjectionOffset + HMDScreenLL;
        screenLR = headProjectionOffset + HMDScreenLR;
        screenUL = headProjectionOffset + HMDScreenUL;

        vrCamera.transform.localEulerAngles = head.localEulerAngles;

        if (useProjection)
        {
            Projection(screenLL, screenLR, screenUL, head.localPosition, virtualCamera.nearClipPlane, virtualCamera.farClipPlane);
        }
    }

    public void SetHeadProjectionOffset(object[] data)
    {
        float x = headProjectionOffset.x;
        float y = headProjectionOffset.y;
        float z = headProjectionOffset.z;

        float.TryParse((string)data[0], out x);
        float.TryParse((string)data[1], out y);
        float.TryParse((string)data[2], out z);

        headProjectionOffset = new Vector3(x, y, z);
    }

    public void SetHeadProjectionOffset(Vector3 value)
    {
        headProjectionOffset = value;
    }

    public void SetHeadOriginOffset(Vector3 value)
    {
        headOriginOffset = value;
    }

    public void SetDisplayOffset(object[] data)
    {
        float x = displayOffset.x;
        float y = displayOffset.y;
        float z = displayOffset.z;

        float.TryParse((string)data[0], out x);
        float.TryParse((string)data[1], out y);
        float.TryParse((string)data[2], out z);

        displayOffset = new Vector3(x, y, z);
    }

    public void SetHeadOriginOffset(object[] data)
    {
        float x = displayOffset.x;
        float y = displayOffset.y;
        float z = displayOffset.z;

        float.TryParse((string)data[0], out x);
        float.TryParse((string)data[1], out y);
        float.TryParse((string)data[2], out z);

        headOriginOffset = new Vector3(x, y, z);
    }

    public Vector3 GetHeadProjectionOffset()
    {
        return headProjectionOffset;
    }

    public Vector3 GetHeadOriginOffset()
    {
        return headOriginOffset;
    }
}
