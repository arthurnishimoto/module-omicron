using UnityEngine;
using UnityEditor;
using System.Collections;

public class OmicronEditorMode
{
    const string CAVE2SIMNAME = "Omicron/Enable CAVE2 Simulator";
    const string CAVE2NAME = "Omicron/Enable CAVE2 Build";
    const string VRNAME = "Omicron/Enable VR HMD";

    [MenuItem(CAVE2SIMNAME)]
    static void EnableCAVE2Simulator()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Debug.Log("Configured for CAVE2 simulator");

        Menu.SetChecked(CAVE2SIMNAME, true);
        Menu.SetChecked(CAVE2NAME, false);
    }

    [MenuItem(CAVE2NAME)]
    static void EnableCAVE2()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "USING_GETREAL3D");
        PlayerSettings.virtualRealitySupported = false;

        Debug.Log("Configured for CAVE2 deployment");

        Menu.SetChecked(CAVE2SIMNAME, false);
        Menu.SetChecked(CAVE2NAME, true);
        Menu.SetChecked(VRNAME, false);
    }

    [MenuItem(VRNAME)]
    static void EnableVR()
    {
        PlayerSettings.virtualRealitySupported = !PlayerSettings.virtualRealitySupported;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");

        Menu.SetChecked(CAVE2NAME, false);
        Menu.SetChecked(VRNAME, PlayerSettings.virtualRealitySupported);
    }
}
