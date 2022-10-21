using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct ObjectStressTestInfo
{
    public int objectCount;
    public Vector2 minFPS;
    public Vector2 maxFPS;
    public Vector2 avgFPS;

    public string GetStats()
    {
        string output = "Object Count: " + objectCount + "\n";
        output += "Min: " + minFPS.x + " (" + minFPS.y + ")" + "\n";
        output += "Max: " + maxFPS.x + " (" + maxFPS.y + ")" + "\n";
        output += "Avg: " + avgFPS.x + " (" + avgFPS.y + ")" + "\n";

        return output;
    }

    public string GetStatsCSV()
    {
        string output = objectCount + ",";
        output += minFPS.x + "," + minFPS.y + ",";
        output += maxFPS.x + "," + maxFPS.y + ",";
        output += avgFPS.x + "," + avgFPS.y;

        return output;
    }
}

public class ObjectCountStressTestCounter : MonoBehaviour
{
    [SerializeField]
    new string tag = "";

    int currentObjectCount;
    int lastObjectCount;

    [SerializeField]
    Vector2 minFPS = new Vector2(float.PositiveInfinity, 0); // minFPS, time

    [SerializeField]
    Vector2 maxFPS = new Vector2(float.NegativeInfinity, 0); // minFPS, time

    [SerializeField]
    Vector2 avgFPS = new Vector2(); // average FPS, duration

    float fpsAvgTotal;
    int fpsCount;

    [SerializeField]
    Text textLog = null;

    float time;

    ArrayList savedStats = new ArrayList();

    [Header("Automation")]
    [SerializeField]
    bool enableAutoTest = false;

    bool autoTestDone;

    [SerializeField]
    int testStage = 0;

    [SerializeField]
    SpawnObjectsTest tester = null;

    [SerializeField]
    Text mainConsoleDebugText = null;

    float testingTimer;

    void UpdateAutoTest()
    {
        if(testStage == 0)
        {
            testStage = 1;
            testingTimer = 15;
        }
        else if (testStage == 1 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 2 && testingTimer < 0) // 1000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 3 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 4 && testingTimer < 0) // 2000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 5 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 6 && testingTimer < 0) // 3000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 7 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 8 && testingTimer < 0) // 4000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 9 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 10 && testingTimer < 0) // 5000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 11 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 12 && testingTimer < 0) // 6000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 13 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 14 && testingTimer < 0) // 7000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 15 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 16 && testingTimer < 0) // 8000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(1000);
        }
        else if (testStage == 17 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 18 && testingTimer < 0) // 10000 Object Test
        {
            SaveCurrentStats();
            testStage++;
            testingTimer = 25;
            tester.SetSpawnCount(2000);
        }
        else if (testStage == 19 && testingTimer < 10)
        {
            ClearCurrentStats();
            testStage++;
        }
        else if (testStage == 20 && testingTimer < 0) // End - Save to file
        {
            SaveCurrentStats();
            testStage++;
            SaveLogToFile();
            autoTestDone = true;
            tester.ClearSpawnedObjectsByTag();
        }

        testingTimer -= Time.deltaTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tag.Length > 0)
        {
            GameObject[] g = GameObject.FindGameObjectsWithTag(tag);
            currentObjectCount = g.Length;
        }

        UpdateFPS();

        if (currentObjectCount != lastObjectCount)
        {
            // Object count changed, reset counter
            ClearCurrentStats();
            time = 0;
            lastObjectCount = currentObjectCount;
        }
        else
        {
            if(fps < minFPS.x)
            {
                minFPS.x = fps;
                minFPS.y = time;
            }
            if (fps > maxFPS.x)
            {
                maxFPS.x = fps;
                maxFPS.y = time;
            }
        }

        if(textLog)
        {
            if (enableAutoTest && tag.Length > 0)
            {
                if (!autoTestDone)
                {
                    textLog.text = "Object Count: " + lastObjectCount + " (Automated Test in Progress)\n";
                }
                else
                {
                    textLog.text = "Object Count: " + lastObjectCount + " (Automated Test Done)\n";
                }
            }
            else if(tag.Length > 0)
            {
                textLog.text = "Object Count: " + lastObjectCount + "\n";
            }
            else
            {
                textLog.text = "FPS (Time)" + "\n";
            }

            textLog.text += "Min: " + minFPS.x + " (" + minFPS.y + ")" + "\n";
            textLog.text += "Max: " + maxFPS.x + " (" + maxFPS.y + ")" + "\n";
            textLog.text += "Avg: " + avgFPS.x + " (" + avgFPS.y + ")" + "\n\n";

            foreach(ObjectStressTestInfo stat in savedStats)
            {
                textLog.text += stat.GetStats() + "\n";
            }    
        }

        if(enableAutoTest && CAVE2.IsMaster())
        {
            UpdateAutoTest();
        }

        time += Time.deltaTime;
    }


    public float FPS_updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval
    private float fps;
    string fpsString;
    Color fpsColor;

    void UpdateFPS()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            fps = accum / frames;
            string format = System.String.Format("{0:F2} FPS", fps);
            fpsString = format;

            if (fps < 30)
                fpsColor = Color.yellow;
            else
                if (fps < 10)
                fpsColor = Color.red;
            else
                fpsColor = Color.green;
            //	DebugConsole.Log(format,level);
            timeleft = FPS_updateInterval;
            accum = 0.0F;
            frames = 0;

            fpsAvgTotal += fps;
            fpsCount++;

            avgFPS.x = fpsAvgTotal / fpsCount;
            avgFPS.y = time;
        }
    }
    public string GetFPSString()
    {
        return fpsString;
    }

    public void SetFPSText(Text text)
    {
        text.text = fpsString;
        text.color = fpsColor;
    }

    public void SaveCurrentStats()
    {
        ObjectStressTestInfo stats;
        stats.objectCount = lastObjectCount;
        stats.minFPS = minFPS;
        stats.maxFPS = maxFPS;
        stats.avgFPS = avgFPS;

        savedStats.Add(stats);
    }

    public void ClearStatLog()
    {
        savedStats.Clear();
    }

    public void ClearCurrentStats()
    {
        minFPS = new Vector2(float.PositiveInfinity, 0); // minFPS, time
        maxFPS = new Vector2(float.NegativeInfinity, 0); // minFPS, time
        avgFPS = new Vector2();
        fpsAvgTotal = 0;
        fpsCount = 0;
    }

    public void SaveLogToFile()
    {
        string dataTimeStr = System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day + "-" + System.DateTime.Now.Hour + "-" + System.DateTime.Now.Minute + "-" + System.DateTime.Now.Second + "-" + System.DateTime.Now.Millisecond;
        System.IO.StreamWriter writer = new System.IO.StreamWriter(Application.dataPath + "/objectCountStressTestLog-" + dataTimeStr + ".cfg");

        writer.WriteLine(Application.productName);
        writer.WriteLine(Application.unityVersion);
        writer.WriteLine(Debug.isDebugBuild ? "Development Build" : "Production Build");
        writer.WriteLine(CAVE2Manager.GetMachineName());

#if USING_GETREAL3D
        writer.WriteLine("USING_GETREAL3D");
#else
#endif
        foreach (ObjectStressTestInfo stat in savedStats)
        {
            writer.WriteLine(stat.GetStatsCSV());
        }
        writer.Close();

        mainConsoleDebugText.text = "Stress Test Log Saved to: " + Application.dataPath + "/objectCountStressTestLog-" + dataTimeStr + ".cfg";
    }
}
