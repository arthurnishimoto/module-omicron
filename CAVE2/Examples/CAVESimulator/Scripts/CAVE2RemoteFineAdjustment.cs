using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DisplayTransform
{
    public Vector3 rootPos;
    public Vector3 rootRot;
    public Vector3[] displayPos;
    public Vector3[] displayRot;
}

public class CAVE2RemoteFineAdjustment : MonoBehaviour
{
    bool initialized;
    CAVE2Display[] cave2Displays;
    Vector3[] origDisplayPos;
    Vector3[] origDisplayRot;
    Vector3 origRootPos;
    Vector3 origRootRot;

    [Header("Physical Arrangement")]
    [SerializeField]
    int currentDisplayIndex = 0;

    [SerializeField]
    Transform currentDisplay = null;

    [SerializeField]
    float translateIncrement = 0.01f;

    [SerializeField]
    float rotateIncrement = 1.00f;

    [Header("Projection Rotation")]
    [SerializeField]
    float displayAngularOffset = 179.5f;

    [SerializeField]
    bool applyAngularOffset = false;

    VRDisplayManager vrDisplayManager;

    [Header("Load/Save Config")]
    [SerializeField]
    bool applySavedOffsets = false;

    [SerializeField]
    bool outputDisplayTransformToFile = false;

    string jsonPath = "Assets/Resources/cave2SimDisplayTransform";

    [Header("Debug")]
    [SerializeField]
    Text uiText = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Initialize()
    {
        GameObject cave2DisplaysRoot = GameObject.Find("CAVE2 Displays");
        cave2Displays = cave2DisplaysRoot.GetComponentsInChildren<CAVE2Display>();
        vrDisplayManager = cave2DisplaysRoot.GetComponentInParent<VRDisplayManager>();

        int i = 0;
        origDisplayPos = new Vector3[cave2Displays.Length];
        origDisplayRot = new Vector3[cave2Displays.Length];
        foreach (CAVE2Display d in cave2Displays)
        {
            origDisplayPos[i] = d.transform.localPosition;
            origDisplayRot[i] = d.transform.localEulerAngles;
            i++;
        }
        if(cave2Displays.Length > 0)
        {
            initialized = true;

            origRootPos = cave2Displays[0].transform.parent.localPosition;
            origRootRot = cave2Displays[0].transform.parent.localEulerAngles;

            if (applySavedOffsets)
            {
                ApplySavedOffsets();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!initialized)
        {
            Initialize();
            return;
        }

        if (currentDisplayIndex != -1 && cave2Displays[currentDisplayIndex] != null) // Index -1 is for the display parent
        {
            currentDisplay = cave2Displays[currentDisplayIndex].transform;
        }
        else if(cave2Displays[0] != null)
        {
            currentDisplay = cave2Displays[0].transform.parent;
        }

        if(uiText && currentDisplay)
        {
            uiText.text = "CurrentDisplayIndex: " + currentDisplayIndex;
            uiText.text += "\n" + currentDisplay.gameObject.name;
        }

        if (currentDisplay == null)
        {
            initialized = false;
        }
        else
        {
            OnInput();
        }

        if(outputDisplayTransformToFile)
        {
            Vector3[] curDisplayPos = new Vector3[cave2Displays.Length];
            Vector3[] curDisplayRot = new Vector3[cave2Displays.Length];

            int i = 0;
            foreach (CAVE2Display d in cave2Displays)
            {
                curDisplayPos[i] = d.transform.localPosition;
                curDisplayRot[i] = d.transform.localEulerAngles;
                i++;
            }

            DisplayTransform displayInfo = new DisplayTransform();
            displayInfo.displayPos = curDisplayPos;
            displayInfo.displayRot = curDisplayRot;

            displayInfo.rootPos = cave2Displays[0].transform.parent.localPosition;
            displayInfo.rootRot = cave2Displays[0].transform.parent.localEulerAngles;

            string json = JsonUtility.ToJson(displayInfo);

            if(File.Exists(jsonPath + ".json"))
            {
                // If file exists, rename appending time
                System.DateTime fileTime = File.GetLastWriteTime(jsonPath + ".json");
                string timeString = fileTime.Year + "-" + fileTime.Month + "-" + fileTime.Day + "-" + fileTime.Hour + "-" + fileTime.Minute + "-" + fileTime.Second + "-" + fileTime.Millisecond;
                File.Move(jsonPath + ".json", jsonPath + "-" + timeString + ".json");
            }

            StreamWriter writer = new StreamWriter(jsonPath + ".json", false);
            writer.WriteLine(json);
            writer.Close();

            Debug.Log("CAVE2 display transforms exported to '" + jsonPath + ".json'");
            outputDisplayTransformToFile = false;
        }
    }

    void OnInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && currentDisplayIndex >= 0)
        {
            currentDisplayIndex--;
            CAVE2.SendMessage(gameObject.name, "ServerSetDisplayIndex", currentDisplayIndex);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && currentDisplayIndex < cave2Displays.Length - 1)
        {
            currentDisplayIndex++;
            CAVE2.SendMessage(gameObject.name, "ServerSetDisplayIndex", currentDisplayIndex);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            //TranslateDisplay(Vector3.left * translateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerTranslateDisplay", Vector3.left * translateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            //TranslateDisplay(Vector3.right * translateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerTranslateDisplay", Vector3.right * translateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            //TranslateDisplay(Vector3.up * translateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerTranslateDisplay", Vector3.up * translateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            //TranslateDisplay(Vector3.down * translateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerTranslateDisplay", Vector3.down * translateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            //TranslateDisplay(Vector3.forward * translateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerTranslateDisplay", Vector3.forward * translateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            //TranslateDisplay(Vector3.back * translateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerTranslateDisplay", Vector3.back * translateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            //RotateDisplay(Vector3.up * rotateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.down * rotateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            //RotateDisplay(Vector3.down * rotateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.up * rotateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            //RotateDisplay(Vector3.up * rotateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.forward * rotateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            //RotateDisplay(Vector3.down * rotateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.back * rotateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            //RotateDisplay(Vector3.up * rotateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.left * rotateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            //RotateDisplay(Vector3.down * rotateIncrement);
            CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.right * rotateIncrement);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = currentDisplay.transform.localPosition;
            Vector3 rot = currentDisplay.transform.localEulerAngles;

            Debug.Log(currentDisplay.gameObject.name);
            Debug.Log("Pos: " + pos.x.ToString("F3") + ", " + pos.y.ToString("F3") + ", " + pos.z.ToString("F3"));
            Debug.Log("Rot: " + rot.x.ToString("F3") + ", " + rot.y.ToString("F3") + ", " + rot.z.ToString("F3"));
        }
        else if (Input.GetKeyDown(KeyCode.U) || applySavedOffsets)
        {
            applySavedOffsets = false;
            ApplySavedOffsets();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            if (currentDisplayIndex == -1)
            {
                CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", currentDisplayIndex, origRootPos);
                CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", currentDisplayIndex, origRootRot);
            }
            else
            {
                CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", currentDisplayIndex, origDisplayPos[currentDisplayIndex]);
                CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", currentDisplayIndex, origDisplayRot[currentDisplayIndex]);
            }
        }

        if(applyAngularOffset)
        {
            CAVE2.SendMessage(vrDisplayManager.gameObject.name, "UpdateDisplayAngularOffset", displayAngularOffset);
            applyAngularOffset = false;
        }
    }
    void ApplySavedOffsets()
    {
        Debug.Log("Importing CAVE2 display transforms from '" + jsonPath + ".json'");

        StreamReader reader = new StreamReader(jsonPath + ".json");
        DisplayTransform displayInfo = JsonUtility.FromJson<DisplayTransform>(reader.ReadLine());

        Debug.Log("Imported " + displayInfo.displayPos.Length + " positions");
        Debug.Log("Imported " + displayInfo.displayRot.Length + " positions");

        int i = 0;
        foreach(Vector3 pos in displayInfo.displayPos)
        {
            CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", i, pos);
        }

        i = 0;
        foreach (Vector3 rot in displayInfo.displayRot)
        {
            CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", i, rot);
        }

        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", -1, displayInfo.rootPos);
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", -1, displayInfo.rootRot);
        /*
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 4, new Vector3(-3.141f, 1.508f, -0.083f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 5, new Vector3(-3.084f, 1.415f, 1.366f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 6, new Vector3(-2.592f, 1.355f, 2.210f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 7, new Vector3(-1.792f, 1.336f, 2.986f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 8, new Vector3(-1.059f, 1.336f, 3.305f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 9, new Vector3(0.064f, 1.336f, 3.440f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 10, new Vector3(1.033f, 1.336f, 3.261f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 11, new Vector3(1.920f, 1.357f, 2.808f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 12, new Vector3(2.602f, 1.387f, 2.070f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 13, new Vector3(3.041f, 1.416f, 1.130f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayPosition", 14, new Vector3(3.182f, 1.454f, 0.186f));

        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 4, new Vector3(0.000f, 270.515f, 358.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 5, new Vector3(359.861f, 288.529f, 358.005f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 6, new Vector3(0.000f, 308.538f, 358.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 7, new Vector3(0.000f, 324.550f, 0.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 8, new Vector3(0.000f, 342.561f, 0.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 9, new Vector3(0.000f, 0.573f, 0.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 10, new Vector3(0.000f, 18.584f, 0.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 11, new Vector3(0.000f, 34.596f, 2.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 12, new Vector3(0.000f, 58.608f, 2.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 13, new Vector3(0.000f, 72.619f, 2.000f));
        CAVE2.SendMessage(gameObject.name, "ServerSetLocalDisplayRotation", 14, new Vector3(0.000f, 90.631f, 2.000f));
        */
    }

    void TranslateDisplay(Vector3 value)
    {
        currentDisplay.transform.Translate(value, Space.Self);
    }


    void RotateDisplay(Vector3 value)
    {
        currentDisplay.transform.Rotate(value, Space.Self);
    }
    void SetDisplayIndex(int value)
    {
        currentDisplayIndex = value;
    }

    void SetLocalDisplayPosition(object[] param)
    {
        int index = (int)param[0];
        Vector3 value = (Vector3)param[1];

        if (index == -1)
        {
            cave2Displays[0].transform.parent.localPosition = value;
        }
        else
        {
            cave2Displays[index].transform.localPosition = value;
        }
    }

    void SetLocalDisplayRotation(object[] param)
    {
        int index = (int)param[0];
        Vector3 value = (Vector3)param[1];

        if (index == -1)
        {
            cave2Displays[0].transform.parent.localEulerAngles = value;
        }
        else
        {
            cave2Displays[index].transform.localEulerAngles = value;
        }
    }

    void ServerSetDisplayIndex(int index)
    {
        CAVE2.SendMessage(gameObject.name, "SetDisplayIndex", index);
    }

    void ServerTranslateDisplay(Vector3 value)
    {
        CAVE2.SendMessage(gameObject.name, "TranslateDisplay", value);
    }

    void ServerRotateDisplay(Vector3 value)
    {
        CAVE2.SendMessage(gameObject.name, "RotateDisplay", value);
    }

    void ServerSetLocalDisplayPosition(object[] param)
    {
        int index = (int)param[0];
        Vector3 value = (Vector3)param[1];
        CAVE2.SendMessage(gameObject.name, "SetLocalDisplayPosition", index, value);
    }

    void ServerSetLocalDisplayRotation(object[] param)
    {
        int index = (int)param[0];
        Vector3 value = (Vector3)param[1];
        CAVE2.SendMessage(gameObject.name, "SetLocalDisplayRotation", index, value);
    }
}
