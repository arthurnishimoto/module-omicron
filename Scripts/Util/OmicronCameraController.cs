using UnityEngine;
using System.Collections;

public class OmicronCameraController : MonoBehaviour {

    Camera mainCamera;

	// Use this for initialization
	void Start () {
		CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>().AddCameraController(gameObject);

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

    public Camera GetMainCamera()
    {
        return mainCamera;
    }
}
