using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MocapSensorLatencyUI : MonoBehaviour
{
    [SerializeField]
    string mocapSensorName = null;

    OmicronMocapSensor sensor;
    Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(sensor == null)
        {
            GameObject sensorObj = GameObject.Find(mocapSensorName);
            if(sensorObj != null)
            {
                sensor = sensorObj.GetComponent<OmicronMocapSensor>();
            }
        }
        else
        {
            text.text = mocapSensorName + " latency: " + (1000 * sensor.GetUpdateLatency()).ToString("F1") + " ms";
        }
        
    }
}
