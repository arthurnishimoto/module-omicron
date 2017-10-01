using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class OmicronEditorMode : MonoBehaviour
{
    const string CAVE2SIM_NAME = "Omicron/Configure for CAVE2 Simulator";
    const string CAVE2_NAME = "Omicron/Configure for CAVE2 Build";
    const string OCULUS_NAME = "Omicron/Configure for Oculus";
    const string VIVE_NAME = "Omicron/Configure for Vive";
    const string VR_NAME = "Omicron/Disable VR HMDs";

    [MenuItem(CAVE2SIM_NAME)]
    static void ConfigCAVE2Simulator()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Debug.Log("Configured for CAVE2 simulator");

        Menu.SetChecked(CAVE2SIM_NAME, true);
        Menu.SetChecked(CAVE2_NAME, false);

        Camera.main.transform.localPosition = Vector3.up * 1.6f;
    }

    [MenuItem(CAVE2_NAME)]
    static void ConfigCAVE2()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "USING_GETREAL3D");
        PlayerSettings.virtualRealitySupported = false;

        Debug.Log("Configured for CAVE2 deployment");

        Menu.SetChecked(CAVE2SIM_NAME, false);
        Menu.SetChecked(CAVE2_NAME, true);
        Menu.SetChecked(OCULUS_NAME, false);

        Camera.main.transform.localPosition = Vector3.up * 1.6f;
    }

    [MenuItem(OCULUS_NAME)]
    static void ConfigOculus()
    {
        Camera.main.transform.localPosition = Vector3.up * 1.6f;

        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2_NAME, false);
        Menu.SetChecked(OCULUS_NAME, PlayerSettings.virtualRealitySupported);
        Menu.SetChecked(VIVE_NAME, false);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Oculus VR HMDs" : "VR support disabled");
    }

    [MenuItem(VIVE_NAME)]
    static void ConfigVive()
    {
        Camera.main.transform.localPosition = Vector3.up * 0f;

        PlayerSettings.virtualRealitySupported = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2_NAME, false);
        Menu.SetChecked(OCULUS_NAME, false);
        Menu.SetChecked(VIVE_NAME, PlayerSettings.virtualRealitySupported);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Vive VR HMDs" : "VR support disabled");
    }

    [MenuItem(VR_NAME)]
    static void ConfigVR()
    {
        Camera.main.transform.localPosition = Vector3.up * 1.6f;
        PlayerSettings.virtualRealitySupported = false;

        Menu.SetChecked(VIVE_NAME, false);

        Debug.Log(PlayerSettings.virtualRealitySupported ? "Configured for Vive VR HMDs" : "VR support disabled");
    }
}
