using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandUI : TransformUI
{
    public int wandID = 1;

	// Update is called once per frame
	void Update () {
        if (positionUIText)
        {
            if (local)
            {
                position = targetTransform.localPosition;
                eulerAngles = targetTransform.localEulerAngles;
            }
            else
            {
                position = targetTransform.position;
                eulerAngles = targetTransform.eulerAngles;
            }

            int flags = CAVE2.Input.GetButtonFlags(wandID);
            positionUIText.text = "Position: " + position.ToString("N3") + "\nRotation: " + eulerAngles.ToString("N3");
            positionUIText.text += "\nButton Flags: " + flags;
        }
    }
}
