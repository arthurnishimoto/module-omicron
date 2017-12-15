using UnityEngine;
using System.Collections;

// Forwards CAVE2 interaction events to parentObject
public class CAVE2InteractableChild : CAVE2Interactable
{
    public GameObject parentObject;

    public void OnWandButtonDown(CAVE2.ButtonInfo playerInfo)
    {
        parentObject.SendMessage("OnWandButtonDown", playerInfo, SendMessageOptions.DontRequireReceiver);
    }

    public void OnWandButton(CAVE2.ButtonInfo playerInfo)
    {
        parentObject.SendMessage("OnWandButton", playerInfo, SendMessageOptions.DontRequireReceiver);
    }

    public void OnWandButtonUp(CAVE2.ButtonInfo playerInfo)
    {
        parentObject.SendMessage("OnWandButtonUp", playerInfo, SendMessageOptions.DontRequireReceiver);
    }
}
