using UnityEngine;
using System.Collections;

public class CAVE2PlayerControllerID : MonoBehaviour {

    public int playerID = 1;

    public Transform head;
    public Transform[] wands;

    // Use this for initialization
    void Start() {
        if (CAVE2.GetPlayerController(playerID) == null)
        {
            CAVE2.AddPlayerController(playerID, gameObject);
        }
	}
}
