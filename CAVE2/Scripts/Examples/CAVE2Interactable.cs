using UnityEngine;
using System.Collections;

public class CAVE2Interactable : MonoBehaviour {

    [SerializeField]
    protected bool wandOver = false;

    protected float lastWandOverTime;

    float wandOverTimeout = 0.05f;

    protected void UpdateWandOverTimer()
    {
        if (Time.time - lastWandOverTime > wandOverTimeout)
        {
            wandOver = false;
        }
    }

    public void OnWandButtonDown(CAVE2.WandEvent evt)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt[0];
        //int wandID = (int)evt[1];
        //CAVE2.Button button = (CAVE2.Button)evt[2];


        //Debug.Log("OnWandButtonDown: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandButton(CAVE2.WandEvent evt)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt[0];
        //int wandID = (int)evt[1];
        //CAVE2.Button button = (CAVE2.Button)evt[2];


        //Debug.Log("OnWandButton: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandButtonUp(CAVE2.WandEvent evt)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt[0];
        //int wandID = (int)evt[1];
        //CAVE2.Button button = (CAVE2.Button)evt[2];


        //Debug.Log("OnWandButtonUp: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandOver(CAVE2.WandEvent eventInfo)
    {
        OnWandOverEvent();
    }

    public void OnWandOver()
    {
        OnWandOverEvent();
    }

    protected void OnWandOverEvent()
    {
        lastWandOverTime = Time.time;
        wandOver = true;
    }

    public void OnWandPointing(CAVE2.WandEvent eventInfo)
    {
    }

    /*
    public void OnWandButtonDown(CAVE2.Button button)
    {
        // Deprecated - Legacy Support Only
    }

    public void OnWandButton(CAVE2.Button button)
    {
        // Deprecated - Legacy Support Only
    }

    public void OnWandButtonUp(CAVE2.Button button)
    {
        // Deprecated - Legacy Support Only
    }*/
}
