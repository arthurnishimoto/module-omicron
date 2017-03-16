using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CAVE2ScreenMaskRenderer : MonoBehaviour {

    public enum RenderMode { None, Background, Overlay }
    public RenderMode renderMode = RenderMode.Background;

    void Start()
    {
#if UNITY_EDITOR
#else
        if( CAVE2.OnCAVE2Display() )
        {
            GetComponent<Renderer>().enabled = false;
        }
#endif
    }

	// Update is called once per frame
	void Update () {
	    
        switch(renderMode)
        {
            case (RenderMode.Background): GetComponent<Renderer>().sharedMaterial.SetFloat("_ZTest", 2); break;
            case (RenderMode.None): GetComponent<Renderer>().sharedMaterial.SetFloat("_ZTest", 1); break;
            case (RenderMode.Overlay): GetComponent<Renderer>().sharedMaterial.SetFloat("_ZTest", 0); break;
        }
	}
}
