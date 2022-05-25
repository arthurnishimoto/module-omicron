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
        }
    }

    void TranslateDisplay(Vector3 value)
    {
        currentDisplay.transform.Translate(value, Space.Self);
    }

    void RotateDisplay(Vector3 value)
    {
        currentDisplay.transform.Rotate(value, Space.Self);
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
}
