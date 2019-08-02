using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraLister : MonoBehaviour {

    Text uiText;

	// Use this for initialization
	void Start () {
        uiText = GetComponent<Text>();

    }
	
	// Update is called once per frame
	void Update () {
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

        int i = 0;
        uiText.text = "";
        foreach (Camera c in cameras)
        {
            uiText.text += "[" + i + "] " + c.name + " " + c.transform.position + "\n";
            uiText.text += "  (" + c.transform.localPosition.x + ", " + c.transform.localPosition.y + ", " + c.transform.localPosition.z + ")" + "\n";
            i++;
        }
	}
}
