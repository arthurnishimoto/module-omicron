using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class OmicronVRDisplayManager : MonoBehaviour {

    [Serializable]
    public class OmicronVRDisplayManagerConfig
    {
        public Vector3 screenUL;
        public Vector3 screenLL;
        public Vector3 screenLR;
        public Vector2 screenScale;
        public Vector2 screenOffset;
        public bool stereoInverted;
        public StereoscopicCamera.StereoscopicMode stereoMode;
        public Vector2 stereoResolution;
        public float eyeSeparation;
    }

    OmicronVRDisplayManagerConfig config = new OmicronVRDisplayManagerConfig();

    [SerializeField]
    bool showUI;

    [SerializeField]
    GameObject panel;

    [SerializeField]
    GeneralizedPerspectiveProjection projection;

    [SerializeField]
    StereoscopicCamera stereoScript;


    [Header("Canvas UI")]

    [SerializeField]
    InputField screenUL_x;

    [SerializeField]
    InputField screenUL_y;

    [SerializeField]
    InputField screenUL_z;

    Vector3 screenUL;

    [SerializeField]
    InputField screenLL_x;

    [SerializeField]
    InputField screenLL_y;

    [SerializeField]
    InputField screenLL_z;

    Vector3 screenLL;

    [SerializeField]
    InputField screenLR_x;

    [SerializeField]
    InputField screenLR_y;

    [SerializeField]
    InputField screenLR_z;

    Vector3 screenLR;

    [SerializeField]
    Dropdown stereoModeMenu;

    [SerializeField]
    Button invertStereo;

    [SerializeField]
    InputField screenScale_x;

    [SerializeField]
    InputField screenScale_y;

    [SerializeField]
    InputField screenOffset_x;

    [SerializeField]
    InputField screenOffset_y;

    [SerializeField]
    InputField resolution_x;

    [SerializeField]
    InputField resolution_y;

    Vector2 stereoResolution = new Vector2(1366, 768);

    [SerializeField]
    Button useCurrentResolution;

    [SerializeField]
    InputField eyeSeparation;

    Vector2 screenScale;
    Vector2 screenOffset;
    bool stereoInverted;
    StereoscopicCamera.StereoscopicMode stereoMode;

    string configPath;

    // Use this for initialization
    void Start () {
        configPath = Application.dataPath + "/OmicronVR.cfg";

        string[] cmdArgs = Environment.GetCommandLineArgs();
        for(int i = 0; i < cmdArgs.Length; i++)
        {
            if(cmdArgs[i].Equals("-ovrconfig") && i + 1 < cmdArgs.Length)
            {
                configPath = Environment.CurrentDirectory + "/" + cmdArgs[i + 1];
            }
        }
        

        panel.SetActive(showUI);

        // Read from config (if it exists, else create on quit)
        try
        {
            StreamReader reader = new StreamReader(configPath);
            config = JsonUtility.FromJson<OmicronVRDisplayManagerConfig>(reader.ReadToEnd());
            projection.SetScreenUL(config.screenUL);
            projection.SetScreenLL(config.screenLL);
            projection.SetScreenLR(config.screenLR);

            stereoScript.SetScreenScale(config.screenScale);
            stereoScript.SetScreenOffset(config.screenOffset);

            stereoScript.SetStereoInverted(config.stereoInverted);
            stereoScript.SetStereoMode(config.stereoMode);
            stereoScript.SetStereoResolution(config.stereoResolution);
            stereoScript.SetEyeSeparation(config.eyeSeparation);
        }
        catch(Exception e)
        {
            Debug.LogWarning(e);
            config = new OmicronVRDisplayManagerConfig();
        }

        // Set callbacks for Canvas Objects
        screenUL_x.onEndEdit.AddListener(delegate { projection.UpdateScreenUL_x(screenUL_x.text); });
        screenUL_y.onEndEdit.AddListener(delegate { projection.UpdateScreenUL_y(screenUL_y.text); });
        screenUL_z.onEndEdit.AddListener(delegate { projection.UpdateScreenUL_z(screenUL_z.text); });

        screenLL_x.onEndEdit.AddListener(delegate { projection.UpdateScreenLL_x(screenLL_x.text); });
        screenLL_y.onEndEdit.AddListener(delegate { projection.UpdateScreenLL_y(screenLL_y.text); });
        screenLL_z.onEndEdit.AddListener(delegate { projection.UpdateScreenLL_z(screenLL_z.text); });

        screenLR_x.onEndEdit.AddListener(delegate { projection.UpdateScreenLR_x(screenLR_x.text); });
        screenLR_y.onEndEdit.AddListener(delegate { projection.UpdateScreenLR_y(screenLR_y.text); });
        screenLR_z.onEndEdit.AddListener(delegate { projection.UpdateScreenLR_z(screenLR_z.text); });

        screenOffset_x.onEndEdit.AddListener(delegate { stereoScript.UpdateScreenOffset_x(screenOffset_x.text); });
        screenOffset_y.onEndEdit.AddListener(delegate { stereoScript.UpdateScreenOffset_y(screenOffset_y.text); });
        screenScale_x.onEndEdit.AddListener(delegate { stereoScript.UpdateScreenScale_x(screenScale_x.text); });
        screenScale_y.onEndEdit.AddListener(delegate { stereoScript.UpdateScreenScale_y(screenScale_y.text); });

        stereoModeMenu.onValueChanged.AddListener(delegate { stereoScript.SetStereoMode(stereoModeMenu.value); });
        invertStereo.onClick.AddListener(delegate { stereoScript.InvertStereo(); });
        invertStereo.onClick.AddListener(delegate { CheckState(); });

        resolution_x.onEndEdit.AddListener(delegate { stereoScript.UpdateStereoResolution_x(resolution_x.text); });
        resolution_y.onEndEdit.AddListener(delegate { stereoScript.UpdateStereoResolution_y(resolution_y.text); });

        useCurrentResolution.onClick.AddListener(delegate { stereoScript.SetStereoResolution(new Vector2(Screen.width, Screen.height)); });

        eyeSeparation.onEndEdit.AddListener(delegate { stereoScript.UpdateEyeSeparation(eyeSeparation.text); });

        // Set initial UI values
        screenUL = projection.GetScreenUL();
        screenUL_x.text = screenUL.x.ToString();
        screenUL_y.text = screenUL.y.ToString();
        screenUL_z.text = screenUL.z.ToString();

        screenLL = projection.GetScreenLL();
        screenLL_x.text = screenLL.x.ToString();
        screenLL_y.text = screenLL.y.ToString();
        screenLL_z.text = screenLL.z.ToString();

        screenLR = projection.GetScreenLR();
        screenLR_x.text = screenLR.x.ToString();
        screenLR_y.text = screenLR.y.ToString();
        screenLR_z.text = screenLR.z.ToString();

        screenScale = stereoScript.GetScreenScale();
        screenScale_x.text = screenScale.x.ToString();
        screenScale_y.text = screenScale.y.ToString();

        screenOffset = stereoScript.GetScreenOffset();
        screenOffset_x.text = screenOffset.x.ToString();
        screenOffset_y.text = screenOffset.y.ToString();

        stereoResolution = stereoScript.GetStereoResolution();
        resolution_x.text = stereoResolution.x.ToString();
        resolution_y.text = stereoResolution.y.ToString();

        eyeSeparation.text = stereoScript.GetEyeSeparation().ToString();

        CheckState();
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.F11) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
        {
            showUI = !showUI;
        }
        if(panel)
            panel.SetActive(showUI);
    }

    public void CheckState()
    {
        stereoMode = stereoScript.GetStereoMode();
        stereoModeMenu.value = (int)stereoMode;
        stereoInverted = stereoScript.IsStereoInverted();
        
        if (!stereoInverted)
        {
            invertStereo.GetComponentInChildren<Text>().text = "Left First";
        }
        else
        {
            invertStereo.GetComponentInChildren<Text>().text = "Right First";
        }
    }

    private void OnApplicationQuit()
    {
        config.screenUL = projection.GetScreenUL();
        config.screenLL = projection.GetScreenLL();
        config.screenLR = projection.GetScreenLR();
        config.screenScale = stereoScript.GetScreenScale();
        config.screenOffset = stereoScript.GetScreenOffset();
        config.stereoInverted = stereoScript.IsStereoInverted();
        config.stereoMode = stereoScript.GetStereoMode();
        config.stereoResolution = stereoScript.GetStereoResolution();
        config.eyeSeparation = stereoScript.GetEyeSeparation();

        string sfgJson = JsonUtility.ToJson(config, true);

        StreamWriter writer = new StreamWriter(configPath);
        writer.WriteLine(sfgJson);
        writer.Close();
    }

    Vector2 GUIOffset;

    public void SetGUIOffSet(Vector2 offset)
    {
        GUIOffset = offset;
    }

    void OnWindow(int windowID)
    {
        float rowHeight = 25;

        GUI.Label(new Rect(GUIOffset.x + 20, GUIOffset.y + rowHeight * 1, 250, 200), "Generalized Perspective Projection");

        GUI.Label(new Rect(GUIOffset.x + 40, GUIOffset.y + rowHeight * 2, 120, 20), "Screen UL (x, y, z):");
        screenUL.x = float.Parse(GUI.TextField(new Rect(GUIOffset.x + 40 + 120, GUIOffset.y + rowHeight * 2, 50, 20), screenUL.x.ToString(), 25));
    }

}
