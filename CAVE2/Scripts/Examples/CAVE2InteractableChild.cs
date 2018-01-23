using UnityEngine;
using System.Collections;

// Forwards CAVE2 interaction events to parentObject
public class CAVE2InteractableChild : CAVE2Interactable
{
    public GameObject parentObject;

    public new void OnWandButtonDown(CAVE2.WandEvent eventInfo)
    {
        parentObject.SendMessage("OnWandButtonDown", eventInfo, SendMessageOptions.DontRequireReceiver);
    }

    public new void OnWandButton(CAVE2.WandEvent eventInfo)
    {
        parentObject.SendMessage("OnWandButton", eventInfo, SendMessageOptions.DontRequireReceiver);
    }

    public new void OnWandButtonUp(CAVE2.WandEvent eventInfo)
    {
        parentObject.SendMessage("OnWandButtonUp", eventInfo, SendMessageOptions.DontRequireReceiver);
    }
}
