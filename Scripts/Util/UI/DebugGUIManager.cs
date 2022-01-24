/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2022		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2022, Electronic Visualization Laboratory, University of Illinois at Chicago
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

public class DebugGUIManager : MonoBehaviour {

	Rect mainWindow = new Rect(20, 20, 512, 350);

	public enum DebugWindow { App, Omicron, CAVE2, PlayerController };
	string[] windowStrings = new string[] {"App", "OmicronManager", "CAVE2 Simulator", "PlayerController"};

    public DebugWindow currentWindow = DebugWindow.App;

    OmicronManager omgManager;
    CAVE2Manager cave2manager;
    CAVE2PlayerIdentity cave2PlayerController;

    public MonoBehaviour appMenu;

	Rect omicronWindow;
	Rect playerWindow;

	public bool showGUI = false;

	public bool showFPS = false;
	public bool showOnlyOnMaster = false;
	public  float FPS_updateInterval = 0.5F;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
    float curFPS;
    Vector2 minMaxFPS = new Vector2(10000, 0);
    Vector2 minMaxFPSTime = new Vector2();
    Vector2 avgFPS = new Vector2();
    float avgFPSStartTime = 0;

    [SerializeField]
    float avgFPSTimeLimit = 15;

    [SerializeField]
    bool resetAvgFPSTimer = false;

    bool cursorVisibleDefault;

    void Start()
	{
		omgManager = GetComponent<OmicronManager>();
        cave2manager = GetComponent<CAVE2Manager>();

        transform.position = new Vector3(0.01f, 0.04f, 0);
        cursorVisibleDefault = Cursor.visible;
    }

	void Update()
	{
        if (CAVE2.IsMaster())
        {
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
            {
                showGUI = !showGUI;
                if(showGUI)
                {
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.visible = cursorVisibleDefault;
                }
            }

            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0.0)
            {
                // display two fractional digits (f2 format)
                curFPS = accum / frames;
                // formattedFPS = System.String.Format("{0:F2} FPS",fps);
                if (curFPS < minMaxFPS.x)
                {
                    minMaxFPS.x = curFPS;
                    minMaxFPSTime.x = Time.time;
                }
                if (Time.time > 1 && curFPS > minMaxFPS.y)
                {
                    minMaxFPS.y = curFPS;
                    minMaxFPSTime.y = Time.time;
                }

                if (avgFPSTimeLimit > 0 && Time.time - avgFPSStartTime <= avgFPSTimeLimit)
                {
                    avgFPS.x += curFPS;
                    avgFPS.y++;
                }

                if(resetAvgFPSTimer)
                {
                    avgFPSStartTime = Time.time;
                }

                //	DebugConsole.Log(format,level);
                timeleft = FPS_updateInterval;
                accum = 0.0F;
                frames = 0;
            }

            if (showFPS)
            {
                
            }
        }
	}

	void OnGUI() {
		if( showGUI )
		{
			mainWindow = GUI.Window(0, mainWindow, OnMainWindow, "Omicron Debug Manager (Alt-F11)");
		}
	}

	void OnMainWindow(int windowID) {
		GUI.DragWindow (new Rect (0, 0, 10000, 20));

		currentWindow = (DebugWindow)GUI.SelectionGrid(new Rect(10, 20, 480, 24), (int)currentWindow, windowStrings, 4);



		if (currentWindow == DebugWindow.Omicron )
		{
			if( omgManager != null )
			{
				omgManager.SetGUIOffSet(new Vector2(0,50));
                omgManager.OnWindow(windowID);

                // showFPS = GUI.Toggle(new Rect(20, 25 * 7, 250, 20), showFPS, "Show FPS");
                // showOnlyOnMaster = GUI.Toggle(new Rect(20, 25 * 8, 250, 20), showOnlyOnMaster, "Show FPS only on master");
                GUI.Label(new Rect(20, 28 * 8.5f, 250, 20), "Time: " + System.String.Format("{0:F2}", Time.time));
                GUI.Label(new Rect(20, 28 * 9, 250, 20), "FPS: " + System.String.Format("{0:F2}", curFPS));
                GUI.Label(new Rect(20, 28 * 9.5f, 250, 20), "   Min (Time): " + System.String.Format("{0:F2} ({1:F2})", minMaxFPS.x, minMaxFPSTime.x));
                GUI.Label(new Rect(20, 28 * 10.0f, 250, 20), "   Avg (Time): " + System.String.Format("{0:F2}", avgFPS.x / avgFPS.y));
                GUI.Label(new Rect(20, 28 * 10.5f, 250, 20), "   Max (Time): " + System.String.Format("{0:F2} ({1:F2})", minMaxFPS.y, minMaxFPSTime.y));
                GUI.Label(new Rect(20, 28 * 10.5f + 18 * 1, 500, 40), "Config Path: " + Application.dataPath);

            }
            else
				GUI.Label(new Rect(0,50,256,24), "This Feature is Not Currently Available");
		}
		else if (currentWindow == DebugWindow.CAVE2 )
		{
            if (cave2manager != null)
            {
                cave2manager.SetGUIOffSet(new Vector2(0, 50));
                cave2manager.OnWindow(windowID);
            }
            else
                GUI.Label(new Rect(20,50,256,24), "This Feature is Not Currently Available");
		}
		else if (currentWindow == DebugWindow.PlayerController )
		{
            if (cave2PlayerController != null)
            {
                cave2PlayerController.SetGUIOffSet(new Vector2(0, 50));
                cave2PlayerController.OnWindow(windowID);
            }
            else if(cave2PlayerController == null && CAVE2.GetPlayerController(1) != null)
            {
                cave2PlayerController = CAVE2.GetPlayerController(1).GetComponent<CAVE2PlayerIdentity>();
            }
            else
                GUI.Label(new Rect(20, 50, 256, 24), "This Feature is Not Currently Available");
        }
        else if (currentWindow == DebugWindow.App )
        {
            if( appMenu != null )
            {
                appMenu.SendMessage("SetGUIOffSet", new Vector2(0,25));
                appMenu.SendMessage("OnWindow",windowID);
            }
            else
                GUI.Label(new Rect(20,50,256,24), "This Feature is Not Currently Available");
        }
    }
}
