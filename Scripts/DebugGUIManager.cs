using UnityEngine;
using System.Collections;

public class DebugGUIManager : MonoBehaviour {

	Rect mainWindow = new Rect(20, 20, 512, 350);

	public enum DebugWindow { App, Omicron, CAVE2, PlayerController };
	string[] windowStrings = new string[] {"App", "OmicronManager", "CAVE2 Simulator", "PlayerController"};

    public DebugWindow currentWindow = DebugWindow.App;

	OmicronManager omgManager;
	CAVE2Manager cave2manager;
	OmicronPlayerController playerController;
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

	void Start()
	{
        GameObject cave2ManagerObj = GameObject.Find("CAVE2-Manager");
        if (cave2ManagerObj)
        {
            omgManager = cave2ManagerObj.GetComponent<OmicronManager>();
            cave2manager = cave2ManagerObj.GetComponent<CAVE2Manager>();

            if (GetComponent<GUIText>() == null)
                gameObject.AddComponent<GUIText>();
            if (GetComponent<TextMesh>() == null)
                transform.position = new Vector3(0.01f, 0.04f, 0);
        }
	}

	void Update()
	{
		if (playerController == null && cave2manager.GetPlayerController(0) != null )
        {
            playerController = cave2manager.GetPlayerController(0).GetComponent<OmicronPlayerController>();
        }
		if ( (Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
			showGUI = !showGUI;

        bool isMaster = CAVE2Manager.IsMaster();

        if( showFPS && ((showOnlyOnMaster && isMaster) || !showOnlyOnMaster) )
		{
			timeleft -= Time.deltaTime;
			accum += Time.timeScale/Time.deltaTime;
			++frames;
			
			// Interval ended - update GUI text and start new interval
			if( timeleft <= 0.0 )
			{
				// display two fractional digits (f2 format)
				float fps = accum/frames;
				string format = System.String.Format("{0:F2} FPS",fps);

				if( GetComponent<GUIText>() )
				{
					GetComponent<GUIText>().text = format;
					
					if(fps < 30)
						GetComponent<GUIText>().material.color = Color.yellow;
					else 
						if(fps < 10)
							GetComponent<GUIText>().material.color = Color.red;
					else
						GetComponent<GUIText>().material.color = Color.green;
				}
				if( GetComponent<TextMesh>() )
				{
					GetComponent<TextMesh>().text = format;
					
					if(fps < 30)
						GetComponent<TextMesh>().color = Color.yellow;
					else 
						if(fps < 10)
							GetComponent<TextMesh>().color = Color.red;
					else
						GetComponent<TextMesh>().color = Color.green;
				}

				//	DebugConsole.Log(format,level);
				timeleft = FPS_updateInterval;
				accum = 0.0F;
				frames = 0;
			}
		}
		else
		{
            if( GetComponent<GUIText>() )
			    GetComponent<GUIText>().text = "";
			if( GetComponent<TextMesh>() )
				GetComponent<TextMesh>().text = "";
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

                showFPS = GUI.Toggle(new Rect(20, 25 * 7, 250, 20), showFPS, "Show FPS");
                showOnlyOnMaster = GUI.Toggle(new Rect(20, 25 * 8, 250, 20), showOnlyOnMaster, "Show FPS only on master");
	        }
	        else
				GUI.Label(new Rect(0,50,256,24), "This Feature is Not Currently Available");
		}
		else if (currentWindow == DebugWindow.CAVE2 )
		{
			if( cave2manager != null )
			{
				cave2manager.SetGUIOffSet(new Vector2(0,50));
				cave2manager.OnWindow(windowID);
			}
			else
				GUI.Label(new Rect(20,50,256,24), "This Feature is Not Currently Available");
		}
		else if (currentWindow == DebugWindow.PlayerController )
		{
			if( playerController != null )
			{
				playerController.SetGUIOffSet(new Vector2(0,25));
				playerController.OnWindow(windowID);
			}
			else
				GUI.Label(new Rect(20,50,256,24), "This Feature is Not Currently Available");
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
