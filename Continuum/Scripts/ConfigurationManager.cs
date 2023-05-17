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
}

[Serializable]
public class DisplayConfig
{
    public int screenWidth = 1024;
    public int screenHeight = 768;
    public bool fullscreen = true;
    public int windowMode;
    public int screenXPos = -1;
    public int screenYPos = -1;
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

        }
        else
        {
            ConfigList defaultConfigList = new ConfigList();

            defaultConfigList.configurations = new string[] { "default.cfg", "cave2.cfg", "continuum3d.cfg" };

            StreamWriter configSelectwriter = new StreamWriter(configSelectionPath);
            configSelectwriter.Write(JsonUtility.ToJson(defaultConfigList, true));
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
            Debug.Log(JsonUtility.ToJson(loadedConfig, true));
            BroadcastMessage("ConfigurationLoaded", loadedConfig);
        }
        else
        {
            DefaultConfig defaultConfig = new DefaultConfig();
            defaultConfig.displayConfig = new DisplayConfig();

            StreamWriter writer = new StreamWriter(defaultConfigPath + "/" + defaultConfigFile);
            writer.Write(JsonUtility.ToJson(defaultConfig, true));
            writer.Close();
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
