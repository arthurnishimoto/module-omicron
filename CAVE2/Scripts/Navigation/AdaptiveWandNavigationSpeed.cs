using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveWandNavigationSpeed : MonoBehaviour {

    CAVE2WandNavigator wandNav;

    PlanetaryCoordinateMarker planetCoords;
    AltitudeRadar altitudeRadar;

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
        altitudeRadar = GetComponent<AltitudeRadar>();
    }
	
	// Update is called once per frame
	void Update () {
        if (useAdaptiveSpeed)
        {
            float altitude = planetCoords.altitude;
            float newSpeed = 1;

            if (altitudeRadar && altitudeRadar.HasRadarHit())
            {
                altitude = altitudeRadar.GetRadarAltitude();
            }

            if (mode == NavMode.Planet)
            {
                if (altitude > 5000)
                {
                    newSpeed = Mathf.Lerp(10000, 1000000, altitude / 5000000) * 2;
                }
                else
                {
                    newSpeed = Mathf.Lerp(10, 10000, altitude / 5000) * 2;
                }
            }
            else
            {
                newSpeed = Mathf.Lerp(lowSpeed, highSpeed, altitude / highAltitude) * 2;
            }

            if(CAVE2.IsMaster())
            {
                CAVE2.SendMessage(gameObject.name, "SetSpeed", newSpeed);
            }
        }
    }

    void SetSpeed(object[] value)
    {
        SetSpeed((float)value[0]);
    }

    void SetSpeed(float speed)
    {
        wandNav.globalSpeedMod = speed;
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
