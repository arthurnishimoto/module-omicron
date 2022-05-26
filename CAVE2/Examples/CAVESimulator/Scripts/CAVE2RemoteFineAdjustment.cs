using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2RemoteFineAdjustment : MonoBehaviour
{
    bool initialized;
    CAVE2Display[] cave2Displays;
    Vector3[] origDisplayPos;
    Vector3[] origDisplayRot;

    [SerializeField]
    int currentDisplayIndex = 0;

    [SerializeField]
    CAVE2Display currentDisplay;

    [SerializeField]
    float translateIncrement = 0.01f;

    [SerializeField]
    float rotateIncrement = 1.00f;

    [SerializeField]
    bool applySavedOffsets;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Initialize()
    {
        cave2Displays = GameObject.Find("CAVE2 Displays").GetComponentsInChildren<CAVE2Display>();

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

            if(applySavedOffsets)
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

        if(Input.GetKeyDown(KeyCode.DownArrow) && currentDisplayIndex > 0)
        {
            currentDisplayIndex--;
            CAVE2.SendMessage(gameObject.name, "ServerSetDisplayIndex", currentDisplayIndex);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && currentDisplayIndex < cave2Displays.Length - 1)
        {
            currentDisplayIndex++;
            CAVE2.SendMessage(gameObject.name, "ServerSetDisplayIndex", currentDisplayIndex);
        }

        currentDisplay = cave2Displays[currentDisplayIndex];

        if (currentDisplay == null)
        {
            initialized = false;
        }
        else
        {
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
                CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.up * rotateIncrement);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                //RotateDisplay(Vector3.down * rotateIncrement);
                CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.down * rotateIncrement);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                //RotateDisplay(Vector3.up * rotateIncrement);
                CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.forward * rotateIncrement);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                //RotateDisplay(Vector3.down * rotateIncrement);
                CAVE2.SendMessage(gameObject.name, "ServerRotateDisplay", Vector3.back * rotateIncrement);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 pos = currentDisplay.transform.localPosition;
                Vector3 rot = currentDisplay.transform.localEulerAngles;

                Debug.Log(currentDisplay.gameObject.name);
                Debug.Log("Pos: " + pos.x.ToString("F3") + ", " + pos.y.ToString("F3") + ", " + pos.z.ToString("F3"));
                Debug.Log("Rot: " + rot.x.ToString("F3") + ", " + rot.y.ToString("F3") + ", " + rot.z.ToString("F3"));
            }
        }
    }

    void ApplySavedOffsets()
    {
        ServerSetLocalDisplayPosition(4, new Vector3(-3.237f, 1.435f, 0.348f));
        ServerSetLocalDisplayPosition(5, new Vector3(-3.084f, 1.415f, 1.366f));
        ServerSetLocalDisplayPosition(6, new Vector3(-2.592f, 1.355f, 2.210f));
        ServerSetLocalDisplayPosition(7, new Vector3(-1.792f, 1.336f, 2.986f));
        ServerSetLocalDisplayPosition(8, new Vector3(-1.059f, 1.336f, 3.305f));
        ServerSetLocalDisplayPosition(9, new Vector3(0.064f, 1.336f, 3.440f));
        ServerSetLocalDisplayPosition(10, new Vector3(1.033f, 1.336f, 3.261f));
        ServerSetLocalDisplayPosition(11, new Vector3(1.920f, 1.357f, 2.808f));
        ServerSetLocalDisplayPosition(12, new Vector3(2.602f, 1.387f, 2.070f));
        ServerSetLocalDisplayPosition(13, new Vector3(3.041f, 1.416f, 1.130f));
        ServerSetLocalDisplayPosition(14, new Vector3(3.182f, 1.454f, 0.186f));

        ServerSetLocalDisplayRotation(4, new Vector3(0.000f, 270.515f, 358.000f));
        ServerSetLocalDisplayRotation(5, new Vector3(359.861f, 288.529f, 358.005f));
        ServerSetLocalDisplayRotation(6, new Vector3(0.000f, 308.538f, 358.000f));
        ServerSetLocalDisplayRotation(7, new Vector3(0.000f, 324.550f, 0.000f));
        ServerSetLocalDisplayRotation(8, new Vector3(0.000f, 342.561f, 0.000f));
        ServerSetLocalDisplayRotation(9, new Vector3(0.000f, 0.573f, 0.000f));
        ServerSetLocalDisplayRotation(10, new Vector3(0.000f, 18.584f, 0.000f));
        ServerSetLocalDisplayRotation(11, new Vector3(0.000f, 34.596f, 2.000f));
        ServerSetLocalDisplayRotation(12, new Vector3(0.000f, 58.608f, 2.000f));
        ServerSetLocalDisplayRotation(13, new Vector3(0.000f, 72.619f, 2.000f));
        ServerSetLocalDisplayRotation(14, new Vector3(0.000f, 90.631f, 2.000f));

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
        cave2Displays[index].transform.localPosition = value;
    }

    void SetLocalDisplayRotation(object[] param)
    {
        int index = (int)param[0];
        Vector3 value = (Vector3)param[1];
        cave2Displays[index].transform.localEulerAngles = value;
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

    void ServerSetLocalDisplayPosition(int index, Vector3 value)
    {
        CAVE2.SendMessage(gameObject.name, "SetLocalDisplayPosition", index, value);
    }

    void ServerSetLocalDisplayRotation(int index, Vector3 value)
    {
        CAVE2.SendMessage(gameObject.name, "SetLocalDisplayRotation", index, value);
    }
}
