using UnityEngine;
using System.Collections;

public class CAVE2Interactable : MonoBehaviour {

    public void OnWandButtonDown(CAVE2.WandEvent playerInfo)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)playerInfo[0];
        //int wandID = (int)playerInfo[1];
        //CAVE2.Button button = (CAVE2.Button)playerInfo[2];


        //Debug.Log("OnWandButtonDown: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandButton(CAVE2.WandEvent playerInfo)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)playerInfo[0];
        //int wandID = (int)playerInfo[1];
        //CAVE2.Button button = (CAVE2.Button)playerInfo[2];


        //Debug.Log("OnWandButton: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandButtonUp(CAVE2.WandEvent playerInfo)
    {
        //CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)playerInfo[0];
        //int wandID = (int)playerInfo[1];
        //CAVE2.Button button = (CAVE2.Button)playerInfo[2];


        //Debug.Log("OnWandButtonUp: " + playerID.name + " " + wandID + " " + button);
    }

    public void OnWandOver()
    {
    }

    public void OnWandOver(CAVE2.WandEvent eventInfo)
    {
    }

    public void OnWandPointing()
    {
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
