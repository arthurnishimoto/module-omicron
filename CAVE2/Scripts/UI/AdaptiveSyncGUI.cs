using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdaptiveSyncGUI : MonoBehaviour {

    [SerializeField]
    CAVE2TransformSync transformSync = null;

    [SerializeField]
    Text text;

	// Use this for initialization
	void Start () {
        if(text == null)
           text = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        if (text && transformSync)
        {
            text.text = transformSync.GetAdaptiveDebugText();
        }
    }
}
