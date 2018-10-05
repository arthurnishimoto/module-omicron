using UnityEngine;
using System.Collections;

public class CAVE2Display : GeneralizedPerspectiveProjection {

    DisplayInfo displayInfo;

    [SerializeField]
    Vector2 displayResolution = new Vector2(1366, 768);

    // Use this for initialization
    void Start () {
        displayInfo = GetComponent<DisplayInfo>();

        screenUL = displayInfo.Px_UpperLeft;
        screenLL = displayInfo.Px_LowerLeft;
        screenLR = displayInfo.Px_LowerRight;

        head = GetComponentInParent<VRDisplayManager>().headTrackedUser;

        GameObject vrCamera = new GameObject(gameObject.name + " (VR Camera)");
        vrCamera.transform.parent = GetComponentInParent<VRDisplayManager>().virtualHead;
        vrCamera.transform.localPosition = Vector3.zero;
        vrCamera.transform.localEulerAngles = new Vector3(0, displayInfo.h + GetComponentInParent<VRDisplayManager>().displayAngularOffset, 0);

        virtualCamera = vrCamera.AddComponent<Camera>();
        RenderTexture cameraRT = new RenderTexture((int)displayResolution.x, (int)displayResolution.y, 16);
        virtualCamera.targetTexture = cameraRT;

        Material displayMat = new Material(Shader.Find("Unlit/Texture"));
        displayMat.name = gameObject.name + " (VR Camera Material)";
        displayMat.mainTexture = cameraRT;

        Transform displaySpace = transform.Find("Borders/PixelSpace");
        displaySpace.GetComponent<MeshRenderer>().material = displayMat;
        displaySpace.gameObject.layer = GetComponentInParent<VRDisplayManager>().gameObject.layer;
    }

}
