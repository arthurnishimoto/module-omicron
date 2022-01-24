/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2022		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2022, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
 
using UnityEngine;
using System.Collections;

public class CAVE2PlayerIdentity : MonoBehaviour
{
    public int playerID = 1;

    public int headID;
    public Transform headObject;

    public int[] wandIDs;
    public Transform[] wandObjects;

    public Transform cameraController;

    int playerSyncMode;

    private void Start()
    {
        CAVE2.AddPlayerController(playerID, gameObject);

        Transform[] children = GetComponentsInChildren<Transform>();

        // If PlayerController is not unique append ID to separate objects for CAVE2.SendMessage()
        if (GameObject.Find(transform.name) != gameObject)
        {
            foreach (Transform t in children)
            {
                t.name = t.name + " (PlayerID " + playerID + ")";
            }
        }

        if(CAVE2.IsSimulatorMode())
        {
            headObject.parent = cameraController;
            foreach (Transform t in wandObjects)
            {
                t.parent = cameraController;
            }
        }

        // CAVE2 Player Controller Config
        configPath = Application.dataPath + "/cave2player.cfg";

        // Read from config (if it exists, else create on quit)
        try
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(configPath);
            config = JsonUtility.FromJson<CAVE2PlayerConfig>(reader.ReadToEnd());

            playerSyncMode = config.playerSyncMode;

            GameObject playerController = CAVE2.GetPlayerController(1);
            if (playerController && playerController.GetComponent<CAVE2TransformSync>())
            {
                switch (playerSyncMode)
                {
                    case ((int)CAVE2TransformSync.UpdateMode.Adaptive):
                        playerController.GetComponent<CAVE2TransformSync>().SetAdaptiveSync();
                        break;
                    case ((int)CAVE2TransformSync.UpdateMode.Manual):
                        playerController.GetComponent<CAVE2TransformSync>().SetManualSync();
                        break;
                }
            }


        }
        catch
        {

        }
    }

    [System.Serializable]
    public class CAVE2PlayerConfig
    {
        public int playerSyncMode;
    }

    CAVE2PlayerConfig config;
    string configPath;

    void OnApplicationQuit()
    {
        if (config != null)
        {
            config.playerSyncMode = playerSyncMode;

            string sfgJson = JsonUtility.ToJson(config, true);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(configPath);
            writer.WriteLine(sfgJson);
            writer.Close();
        }
    }


    // GUI
    Vector2 GUIOffset;

    public void SetGUIOffSet(Vector2 offset)
    {
        GUIOffset = offset;
    }

    public void OnWindow(int windowID)
    {
        float rowHeight = 18;

        CAVE2TransformSync playerSync = GetComponent<CAVE2TransformSync>();
        if (playerSync)
        {
            GUI.Label(new Rect(GUIOffset.x + 20, GUIOffset.y + rowHeight * 0, 120, 20), "Player Sync:");
            bool playerAdaptiveSync = GUI.Toggle(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 1, 250, 20), playerSync.IsAdaptiveSyncEnabled(), "Adaptive");
            //bool playerManualSync = GUI.Toggle(new Rect(GUIOffset.x + 25, GUIOffset.y + rowHeight * 2, 250, 20), playerSync.IsManualSyncEnabled(), "Manual (Off)");

            if (playerAdaptiveSync && !playerSync.IsAdaptiveSyncEnabled())
            {
                playerSyncMode = (int)CAVE2TransformSync.UpdateMode.Adaptive;
                playerSync.SetAdaptiveSync();

                UpdateConfig();
            }
            else if(!playerAdaptiveSync && playerSync.IsAdaptiveSyncEnabled())
            {
                playerSyncMode = (int)CAVE2TransformSync.UpdateMode.Manual;
                playerSync.SetManualSync();

                UpdateConfig();
            }
            
        }
    }

    void UpdateConfig()
    {
        if(config == null)
        {
            config = new CAVE2PlayerConfig();
            config.playerSyncMode = playerSyncMode;
        }
    }
}
