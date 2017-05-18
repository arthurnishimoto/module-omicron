using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NavModeUI : MonoBehaviour {

    public Toggle walkButton;
    public Toggle driveButton;
    public Toggle freeflyButton;

    public CAVE2WandNavigator navController;

    // Use this for initialization
    void Start () {
        navController = GetComponentInParent<CAVE2WandNavigator>();

        if(navController.navMode == CAVE2WandNavigator.NavigationMode.Walk)
        {
            walkButton.isOn = true;
        }
        else if (navController.navMode == CAVE2WandNavigator.NavigationMode.Drive)
        {
            driveButton.isOn = true;
        }
        else if (navController.navMode == CAVE2WandNavigator.NavigationMode.Freefly)
        {
            freeflyButton.isOn = true;
        }
    }

    void Update()
    {
        //UpdateButtons();
    }

    void UpdateNavButtons()
    {
        walkButton.isOn = false;
        driveButton.isOn = false;
        freeflyButton.isOn = false;

        switch (navController.navMode)
        {
            case (CAVE2WandNavigator.NavigationMode.Walk):
                walkButton.isOn = true;
                break;
            case (CAVE2WandNavigator.NavigationMode.Drive):
                driveButton.isOn = true;
                break;
            case (CAVE2WandNavigator.NavigationMode.Freefly):
                freeflyButton.isOn = true;
                break;
        }
    }
}
