using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RotateModeUI : MonoBehaviour {

    public Toggle strafeButton;
    public Toggle rotateButton;

    public CAVE2WandNavigator navController;

    // Use this for initialization
    void Start () {
        navController = GetComponentInParent<CAVE2WandNavigator>();

        if(navController.horizontalMovementMode == CAVE2WandNavigator.HorizonalMovementMode.Strafe)
        {
            strafeButton.isOn = true;
        }
        else if (navController.horizontalMovementMode == CAVE2WandNavigator.HorizonalMovementMode.Turn)
        {
            rotateButton.isOn = true;
        }
    }

    void Update()
    {
        //UpdateButtons();
    }

    void UpdateNavButtons()
    {
        strafeButton.isOn = false;
        rotateButton.isOn = false;

        switch (navController.horizontalMovementMode)
        {
            case (CAVE2WandNavigator.HorizonalMovementMode.Strafe):
                strafeButton.isOn = true;
                break;
            case (CAVE2WandNavigator.HorizonalMovementMode.Turn):
                rotateButton.isOn = true;
                break;
        }
    }
}
