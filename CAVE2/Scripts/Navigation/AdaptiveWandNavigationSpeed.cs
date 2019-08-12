using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveWandNavigationSpeed : MonoBehaviour {

    CAVE2WandNavigator wandNav;

    PlanetaryCoordinateMarker planetCoords;

    float lowSpeed = 10000;
    float highSpeed = 1000000;

    float highAltitude = 5000000;

    bool useAdaptiveSpeed = true;

    enum NavMode { Planet, Custom };

    NavMode mode = NavMode.Planet;

    // Use this for initialization
    void Start () {
        wandNav = GetComponent<CAVE2WandNavigator>();
        planetCoords = GetComponent<PlanetaryCoordinateMarker>();
    }
	
	// Update is called once per frame
	void Update () {
        if (useAdaptiveSpeed)
        {
            if (mode == NavMode.Planet)
            {
                if (planetCoords.altitude > 20000)
                {
                    wandNav.globalSpeedMod = Mathf.Lerp(10000, 1000000, planetCoords.altitude / 5000000) * 2;
                }
                else
                {
                    wandNav.globalSpeedMod = Mathf.Lerp(10, 10000, planetCoords.altitude / 20000) * 2;
                }
            }
            else
            {
                wandNav.globalSpeedMod = Mathf.Lerp(lowSpeed, highSpeed, planetCoords.altitude / highAltitude) * 2;
            }
        }
    }

    public void UseAdaptiveSpeed(bool value)
    {
        useAdaptiveSpeed = value;

        mode = NavMode.Planet;
    }

    public void UseAdaptiveSpeed(bool value, float lowSpeed, float highSpeed, float highAltitude)
    {
        useAdaptiveSpeed = value;
        this.lowSpeed = lowSpeed;
        this.highSpeed = highSpeed;
        this.highAltitude = highAltitude;

        mode = NavMode.Custom;
    }

    public bool UsingAdaptiveSpeed()
    {
        return useAdaptiveSpeed;
    }
}
