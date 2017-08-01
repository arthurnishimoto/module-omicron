using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCanvasUIText : MonoBehaviour {

    public float FPS_updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    Text canvasText;

	// Use this for initialization
	void Start () {
        canvasText = GetComponent<Text>();

    }
	
	// Update is called once per frame
	void Update () {
        if (canvasText != null)
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0.0)
            {
                // display two fractional digits (f2 format)
                float fps = accum / frames;
                string format = System.String.Format("{0:F2} FPS", fps);
                canvasText.text = format;

                if (fps < 30)
                    canvasText.color = Color.yellow;
                else
                    if (fps < 10)
                    canvasText.color = Color.red;
                else
                    canvasText.color = Color.green;
                //	DebugConsole.Log(format,level);
                timeleft = FPS_updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
    }
}
