using UnityEngine;
using System.Collections;

public class OmicronPlayerControllerDebugCanvasUI : MonoBehaviour {

    public OmicronPlayerController player;

    public UnityEngine.UI.Text position;
    public UnityEngine.UI.Text headPosition;
    public UnityEngine.UI.Text wandPosition;
    public UnityEngine.UI.Text forwardReference;
    public UnityEngine.UI.Text horizontalMovementMode;
    public UnityEngine.UI.Text movementScale;
    public UnityEngine.UI.Text flyMovementScale;
    public UnityEngine.UI.Text turnSpeed;
    public UnityEngine.UI.Text autoLevelMode;

    // Update is called once per frame
    void Update () {
        string[] debugInfo = player.GetDebugText();

        if (position)
        {
            position.text = debugInfo[0];
        }
        if (headPosition)
        {
            headPosition.text = debugInfo[1];
        }
        if (wandPosition)
        {
            wandPosition.text = debugInfo[2];
        }
        if (forwardReference)
        {
            forwardReference.text = debugInfo[3];
        }
        if (horizontalMovementMode)
        {
            horizontalMovementMode.text = debugInfo[4];
        }
        if (movementScale)
        {
            movementScale.text = debugInfo[5];
        }
    }
}
