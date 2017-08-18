using UnityEngine;
using System.Collections;

public class FramerateUI : MonoBehaviour {

    public bool showFPS = false;
    public bool showOnlyOnMaster = false;
    public float FPS_updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    // Use this for initialization
    void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        bool isMaster = CAVE2.IsMaster();

        if (showFPS && ((showOnlyOnMaster && isMaster) || !showOnlyOnMaster))
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

                if (GetComponent<GUIText>())
                {
                    GUIText text = GetComponent<GUIText>();
                    text.text = format;

                    if (fps < 30)
                        text.material.color = Color.yellow;
                    else
                        if (fps < 10)
                            text.material.color = Color.red;
                    else
                        text.material.color = Color.green;
                }
                if (GetComponent<TextMesh>())
                {
                    TextMesh text = GetComponent<TextMesh>();
                    text.text = format;

                    if (fps < 30)
                        text.color = Color.yellow;
                    else
                        if (fps < 10)
                        text.color = Color.red;
                    else
                        text.color = Color.green;
                }
                if (GetComponent<UnityEngine.UI.Text>())
                {
                    UnityEngine.UI.Text text = GetComponent<UnityEngine.UI.Text>();
                    text.text = format;

                    if (fps < 30)
                        text.color = Color.yellow;
                    else
                        if (fps < 10)
                        text.color = Color.red;
                    else
                        text.color = Color.green;
                }

                //	DebugConsole.Log(format,level);
                timeleft = FPS_updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
        else
        {
            if (GetComponent<GUIText>())
                GetComponent<GUIText>().text = "";
            if (GetComponent<TextMesh>())
                GetComponent<TextMesh>().text = "";
            if (GetComponent<UnityEngine.UI.Text>())
                GetComponent<UnityEngine.UI.Text>().text = "";
        }
    }
}
