using UnityEngine;
using System.Collections;

public class VRPlayerWrapper : MonoBehaviour {

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
}
