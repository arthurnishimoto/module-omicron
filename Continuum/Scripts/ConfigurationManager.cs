using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class ConfigList
{
    public string[] configurations = null;
}

[Serializable]
public class DefaultConfig
{
    public bool showDebugMenu = false;
    public DisplayConfig displayConfig;
    public StereoscopicConfig stereoscopicConfig;
    public TrackerConfig trackerConfig;
    public ClusterConfig clusterConfig;
}

[Serializable]
public class DisplayConfig
{
    public int screenWidth = 1024;
    public int screenHeight = 768;
    public bool fullscreen = true;
    public string windowMode = "Windowed";
    public int screenXPos = -1;
    public int screenYPos = -1;
    public bool useGeneralizedPerspectiveProjection = false;
    public Vector3 screenUL;
    public Vector3 screenLL;
    public Vector3 screenLR;
}

[Serializable]
public class StereoscopicConfig
{
    public string stereoMode = "None";
    public int stereoResolutionX = 1366;
    public int stereoResolutionY = 768;
    public bool autoStereoResolution = true;
    public bool invertStereo = false;
}

[Serializable]
public class TrackerConfig
{
    public string trackerType = "omicron";
    public bool connectToServer = false;
    public string serverIP = "localhost";
    public int serverMsgPort = 28000;
    public int dataPort = 7013;
    public bool continuum3DCoordinateConversion = false;
    public bool continuumMainCoordinateConversion = false;
    public bool trackingSimulatorMode = true;
}

[Serializable]
public class ClusterConfig
{
    public string headNodeName = "CAVE2MASTER";
    public string headNodeIPAddesss = "localhost";
    public string displayNodeName = "ORION";
}

public class ConfigurationManager : MonoBehaviour
{
    public static DefaultConfig loadedConfig;

    // Application.dataPath
    // Unity editor: [project folder]/Assets
    // Standalone: [exe data folder]
    string configSelectionPath = "/config.cfg";
    string configPath = "/Configs";

    [SerializeField]
    [Tooltip("Relative path to Assets folder")]
    string defaultEditorBasePath = "/module-omicron";
    string defaultConfigFile = "default.cfg";

    // Start is called before the first frame update
    void Start()
    {
        // Default editor path: [project folder]/Assets/module-omicron
        defaultEditorBasePath = Application.dataPath + "/" + defaultEditorBasePath;

        // Default standalone paths:
        // [exe_Data folder]/config.cfg
        // [exe_Data folder]/Configs
        // Default editor paths:
        // [project folder]/Assets/module-omicron/config.cfg
        // [project folder]/Assets/module-omicron/Configs
#if UNITY_EDITOR
        configSelectionPath = defaultEditorBasePath + "/" + configSelectionPath;
        configPath = defaultEditorBasePath + "/" + configPath;
#else
        configSelectionPath = Application.dataPath + "/" + configSelectionPath;
        configPath = Application.dataPath + "/" + configPath;
#endif
        // Check for config selection file
        if (File.Exists(configSelectionPath))
        {
            // If exists, read
            StreamReader configSelectReader = new StreamReader(configSelectionPath);
            defaultConfigFile = configSelectReader.ReadLine();
            configSelectReader.Close();
        }
        else
        {
            // If not, generate file
            StreamWriter configSelectwriter = new StreamWriter(configSelectionPath);
            configSelectwriter.WriteLine(defaultConfigFile);
            configSelectwriter.Close();
            Debug.Log("Default config selector not found. Generating '" + configSelectionPath + "'");
        }

        // Creates default config directory if it doesn't exist
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
            Debug.Log("Default config directory not found. Generating '" + configPath + "'");
        }

        if (File.Exists(configPath + "/" + defaultConfigFile))
        {
            StreamReader reader = new StreamReader(configPath + "/" + defaultConfigFile);
            loadedConfig = JsonUtility.FromJson<DefaultConfig>(reader.ReadToEnd());

            Debug.Log("Loaded configuration file '" + configPath + "/" + defaultConfigFile + "':");
            //Debug.Log(JsonUtility.ToJson(loadedConfig, true));
#if USING_CAVE2
            // Specific CAVE2 config function to ONLY set machine name
            // and not set anything else i.e. display (for now)
            BroadcastMessage("CAVE2ConfigurationLoaded", loadedConfig);
#else
            BroadcastMessage("ConfigurationLoaded", loadedConfig);
#endif
        }
        else
        {
            DefaultConfig defaultConfig = new DefaultConfig();
            defaultConfig.displayConfig = new DisplayConfig();
            defaultConfig.stereoscopicConfig = new StereoscopicConfig();
            defaultConfig.trackerConfig = new TrackerConfig();
            defaultConfig.clusterConfig = new ClusterConfig();

            StreamWriter writer = new StreamWriter(configPath + "/" + defaultConfigFile);
            writer.Write(JsonUtility.ToJson(defaultConfig, true));
            writer.Close();

            loadedConfig = defaultConfig;
            Debug.Log("Default config not found. Generating '" + configPath + "/" + defaultConfigFile + "'");
        }
    }

    private void OnApplicationQuit()
    {
        if (File.Exists(configPath + "/" + defaultConfigFile))
        {
            //StreamWriter writer = new StreamWriter(defaultConfigPath + "/" + defaultConfigFile);
            //writer.Write(JsonUtility.ToJson(loadedConfig, true));
            //writer.Close();
        }
    }
}
