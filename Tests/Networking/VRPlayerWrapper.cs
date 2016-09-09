using UnityEngine;
using System.Collections;

public class VRPlayerWrapper : MonoBehaviour {

    public string VRTypeLabel = "";

    public Transform head;

    public Transform[] wands;

    public Transform GetHead()
    {
        return head;
    }

    public Transform GetWand(int wandID)
    {
        if(wandID > 0 && wands.Length > wandID)
            return wands[wandID-1];
        else
        {
            return null;
        }
    }

    public Transform[] GetWands()
    {
        return wands;
    }

    public string GetVRTypeLabel()
    {
        if (CAVE2Manager.GetCAVE2Manager().simulatorMode)
        {
            VRTypeLabel = "CAVE2 Simulator";
        }
#if USING_GETREAL3D
        VRTypeLabel = "CAVE2";
#endif
        return VRTypeLabel;
    }
}
