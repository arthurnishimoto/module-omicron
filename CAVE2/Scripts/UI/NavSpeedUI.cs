using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NavSpeedUI : MonoBehaviour {

    public Text label;
    public Slider slider;

    public CAVE2WandNavigator navController;

    // Use this for initialization
    void Start () {
        navController = GetComponentInParent<CAVE2WandNavigator>();

        float navSpeed = navController.globalSpeedMod;


    }

    public void UpdateNavSpeed()
    {
        float sliderVal = slider.value;
        sliderVal = Mathf.Pow(10,(sliderVal - 5));

        if (navController)
        {
            label.text = "Navigation Speed: " + sliderVal + "x";
            navController.globalSpeedMod = sliderVal;
        }
    }
}
