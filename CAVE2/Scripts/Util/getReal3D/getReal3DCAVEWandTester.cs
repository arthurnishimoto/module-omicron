using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;

public class getReal3DCAVEWandTester : MonoBehaviour {
#if USING_GETREAL3D
    [SerializeField]
    float[] valuator;

    [SerializeField]
    bool[] button;

    // Use this for initialization
    void Start () {
        valuator = new float[6];
        button = new bool[12];
    }
	
	// Update is called once per frame
	void Update () {

        valuator[0] = getReal3D.Input.GetAxis("Yaw");
        valuator[1] = getReal3D.Input.GetAxis("Forward");
        valuator[2] = getReal3D.Input.GetAxis("Strafe");
        valuator[3] = getReal3D.Input.GetAxis("Pitch");
        valuator[4] = getReal3D.Input.GetAxis("DPadLR");
        valuator[5] = getReal3D.Input.GetAxis("DPadUD");

        button[0] = getReal3D.Input.GetButton("NavSpeed");
        button[1] = getReal3D.Input.GetButton("WandButton");
        button[2] = getReal3D.Input.GetButton("ChangeWand");
        button[3] = getReal3D.Input.GetButton("Jump");
        button[4] = getReal3D.Input.GetButton("WandLook");
        button[5] = getReal3D.Input.GetButton("Reset");
        button[6] = getReal3D.Input.GetButton("WandDrive");
        button[7] = getReal3D.Input.GetButton("RT");
        button[8] = getReal3D.Input.GetButton("L3");
        button[9] = getReal3D.Input.GetButton("R3");
        button[10] = getReal3D.Input.GetButton("Back");
        button[11] = getReal3D.Input.GetButton("Start");

    }
#endif
}
