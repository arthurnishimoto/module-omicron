using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CAVE2AdvancedTrackingSimulator;

public class CAVE2MultiPlayerControllerManager : MonoBehaviour
{
    [SerializeField]
    int activePlayer = 1;

    [SerializeField]
    KeyCode togglePlayer = KeyCode.None;

    int lastActivePlayer = 0;

    [SerializeField]
    GameObject playerController1;

    [SerializeField]
    GameObject playerController2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR // Only run in editor/simulator
        if(lastActivePlayer != activePlayer)
        {
            SetActivePlayer(activePlayer);

            lastActivePlayer = activePlayer;
        }

        if (Input.GetKeyDown(togglePlayer))
        {
            if (activePlayer == 1)
            {
                activePlayer = 2;
            }
            else if (activePlayer == 2)
            {
                activePlayer = 1;
            }
        }
#endif
    }

    void SetActivePlayer(int playerID)
    {
        GameObject newPlayer = playerController1;
        GameObject oldPlayer = playerController2;

        if (playerID == 1)
        {
            newPlayer = playerController1;
            oldPlayer = playerController2;
        }
        else if (playerID == 2)
        {
            newPlayer = playerController2;
            oldPlayer = playerController1;
        }

        // Enable cameras on new player
        Camera[] cameras = newPlayer.GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            cam.enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
        }

        // Disable cameras on old player
        cameras = oldPlayer.GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            cam.enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
        }

        GetComponent<CAVE2InputManager>().wandIDMappedToSimulator = playerID;
        GetComponent<CAVE2AdvancedTrackingSimulator>().headID = playerID;
        GetComponent<CAVE2AdvancedTrackingSimulator>().wandID = playerID;
    }
}
