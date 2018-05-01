using UnityEngine;
using System.Collections;

public class CAVE2CameraController : MonoBehaviour {

    Camera mainCamera;

	// Use this for initialization
	void Start () {
		CAVE2.AddCameraController(this);

        mainCamera = GetComponentInChildren<Camera>();

#if USING_GETREAL3D
        if (mainCamera.GetComponent<getRealCameraUpdater>())
        {
            mainCamera.GetComponent<getRealCameraUpdater>().enabled = true;
        }
        else
        {
            mainCamera.gameObject.AddComponent<getRealCameraUpdater>();
        }
#endif
    }

    void Update()
    {
#if USING_GETREAL3D
        if (mainCamera.GetComponent<getRealCameraUpdater>())
        {
            if (CAVE2.IsSimulatorMode())
            {
                mainCamera.GetComponent<getRealCameraUpdater>().enabled = false;
                mainCamera.transform.localPosition = CAVE2.GetHeadPosition(1);
                mainCamera.transform.localRotation = CAVE2.GetHeadRotation(1);
            }
            else
            {
                mainCamera.GetComponent<getRealCameraUpdater>().enabled = true;
            }
        }
#endif
    }

    public Camera GetMainCamera()
    {
        return mainCamera;
    }

    public void SetCameraCullingMask(int mask)
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach( Camera c in cameras )
        {
            c.cullingMask = mask;
        }
    }

    public void SetCameraNearClippingPlane(float value)
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras)
        {
            c.nearClipPlane = value;
        }
    }

}
