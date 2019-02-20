/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2018		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2018, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
 
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
            if (GetComponent<TextMesh>())
                GetComponent<TextMesh>().text = "";
            if (GetComponent<UnityEngine.UI.Text>())
                GetComponent<UnityEngine.UI.Text>().text = "";
        }
    }
}
