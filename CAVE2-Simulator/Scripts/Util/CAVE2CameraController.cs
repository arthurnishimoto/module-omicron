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
            mainCamera.GetComponent<getRealCameraUpdater>().applyHeadPosition = true;
            mainCamera.GetComponent<getRealCameraUpdater>().applyHeadRotation = true;
            mainCamera.GetComponent<getRealCameraUpdater>().applyCameraProjection = true;
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
                mainCamera.GetComponent<getRealCameraUpdater>().applyHeadPosition = false;
                mainCamera.GetComponent<getRealCameraUpdater>().applyHeadRotation = false;
                mainCamera.GetComponent<getRealCameraUpdater>().applyCameraProjection = false;
            }
#endif
        }
    }

    public Camera GetMainCamera()
    {
        return mainCamera;
    }
}
