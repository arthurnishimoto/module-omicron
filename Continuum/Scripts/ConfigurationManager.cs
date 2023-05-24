using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class ConfigList
{
    public string[] configurations;
}

[Serializable]
public class DefaultConfig
{
    public DisplayConfig displayConfig;
    public StereoscopicConfig stereoscopicConfig;
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

public class ConfigurationManager : MonoBehaviour
{
    public static DefaultConfig loadedConfig;

    string configSelectionPath = Application.dataPath + "/config.cfg";
    string defaultConfigPath = Application.dataPath + "/Configs";
    string defaultConfigFile = "default.cfg";

    // Start is called before the first frame update
    void Start()
    {
        if (File.Exists(configSelectionPath))
        {
            StreamReader configSelectReader = new StreamReader(configSelectionPath);
            defaultConfigFile = configSelectReader.ReadLine();
            configSelectReader.Close();
        }
        else
        {
            StreamWriter configSelectwriter = new StreamWriter(configSelectionPath);
            configSelectwriter.WriteLine(defaultConfigFile);
            configSelectwriter.Close();
        }

        

        // Creates default config directory if it doesn't exist
        if (!Directory.Exists(defaultConfigPath))
        {
            Directory.CreateDirectory(defaultConfigPath);
        }

        if (File.Exists(defaultConfigPath + "/" + defaultConfigFile))
        {
            StreamReader reader = new StreamReader(defaultConfigPath + "/" + defaultConfigFile);
            loadedConfig = JsonUtility.FromJson<DefaultConfig>(reader.ReadToEnd());

            Debug.Log("Loaded configuration file '" + defaultConfigPath + "/" + defaultConfigFile + "':");
            //Debug.Log(JsonUtility.ToJson(loadedConfig, true));
            BroadcastMessage("ConfigurationLoaded", loadedConfig);
        }
        else
        {
            DefaultConfig defaultConfig = new DefaultConfig();
            defaultConfig.displayConfig = new DisplayConfig();
            defaultConfig.stereoscopicConfig = new StereoscopicConfig();

            StreamWriter writer = new StreamWriter(defaultConfigPath + "/" + defaultConfigFile);
            writer.Write(JsonUtility.ToJson(defaultConfig, true));
            writer.Close();

            loadedConfig = defaultConfig;
        }
    }

    private void OnApplicationQuit()
    {
        if (File.Exists(defaultConfigPath + "/" + defaultConfigFile))
        {
            StreamWriter writer = new StreamWriter(defaultConfigPath + "/" + defaultConfigFile);
            writer.Write(JsonUtility.ToJson(loadedConfig, true));
            writer.Close();
        }
    }
}
