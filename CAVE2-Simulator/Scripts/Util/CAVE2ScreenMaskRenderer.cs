using UnityEngine;
using System.Collections;

public class CAVE2ScreenMaskRenderer : MonoBehaviour {

    public enum RenderMode { None, Background, Overlay }
    public RenderMode renderMode = RenderMode.Background;

	// Update is called once per frame
	void Update () {
	    
        switch(renderMode)
        {
            case (RenderMode.Background): GetComponent<Renderer>().material.SetFloat("_ZTest", 2); break;
            case (RenderMode.None): GetComponent<Renderer>().material.SetFloat("_ZTest", 1); break;
            case (RenderMode.Overlay): GetComponent<Renderer>().material.SetFloat("_ZTest", 0); break;
        }
	}
}
