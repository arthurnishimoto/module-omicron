using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2LoadSceneWandClick : CAVE2Interactable
{
    [SerializeField]
    string sceneName = null;

    [SerializeField]
    int wandID = 1;

    [SerializeField]
    CAVE2.Button button = CAVE2.Button.Button3;

    new public void OnWandButtonDown(CAVE2.WandEvent evt)
    {
        // CAVE2PlayerIdentity playerID = (CAVE2PlayerIdentity)evt.playerID;
        int wandID = (int)evt.wandID;
        CAVE2.Button button = (CAVE2.Button)evt.button;

        if(wandID == this.wandID && button == this.button)
            CAVE2.LoadScene(sceneName);
    }
}
