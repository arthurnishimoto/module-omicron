using UnityEngine;
using System.Collections;

public class CAVE2CameraController : MonoBehaviour {

    [SerializeField]
    Camera mainCamera;

	// Use this for initialization
	void Start () {
		CAVE2.AddCameraController(this);

        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>();
        }

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
        if( CAVE2.IsSimulatorMode() )
        {
#if USING_GETREAL3D
            if (mainCamera.GetComponent<getRealCameraUpdater>())
            {
                mainCamera.GetComponent<getRealCameraUpdater>().enabled = false;
            }
#endif
        }
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
