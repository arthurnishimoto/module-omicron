using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2SphereDisplayManager : MonoBehaviour
{
    static string SPHERE_MACHINE_NAME = "PFAP-2008";

    [SerializeField]
    GameObject cave2Camera;

    [SerializeField]
    GameObject sphereCamera;

    [SerializeField]
    GameObject cave2ScreenMask;

    [Header("Debug")]
    [SerializeField]
    bool forceSphereMode = false;

    // Start is called before the first frame update
    void Start()
    {
        // If sphere display, enable sphere projection camera object
        if(IsCAVE2SphereDisplay() || forceSphereMode)
        {
            //cave2Camera.SetActive(false); // This breaks on CAVE2, but works fine if still enabled on sphere client
            sphereCamera.SetActive(true);
            cave2ScreenMask.SetActive(false);
        }
    }

    public static bool IsCAVE2SphereDisplay()
    {
        return CAVE2Manager.GetMachineName() == SPHERE_MACHINE_NAME;
    }
}
