using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2RemoteDisplayManager : MonoBehaviour
{
    public enum DisplayMode { Normal, Overlay_Black, Overlay_Custom };

    // Start is called before the first frame update
    void Start()
    {
        /* // Test routine for ButtonToNodeDisplayIDs()
        for(int i = 0; i < 23; i++)
        {
            int[] nodeDisplayIDs = ButtonToNodeDisplayIDs(i);
            Debug.Log(i + " " + nodeDisplayIDs[0] + " " + nodeDisplayIDs[1]);
        }
        */
    }

    /// <summary>
    /// Converts a buttonID to a node and display ID mapping
    /// </summary>
    /// <param name="buttonID"></param>
    /// <returns></returns>
    int[] ButtonToNodeDisplayIDs(int buttonID)
    {
        int displaysPerNode = 3;

        int nodeID;
        int displayID;

        if (buttonID == -1) // All
        {
            nodeID = -1;
            displayID = -1;
        }
        else if (buttonID < 4) // Backwall
        {
            nodeID = buttonID / 4;
            displayID = buttonID % 4;
        }
        else // Other nodes (Assumed to have 3 displays each)
        {
            nodeID = 1 + ((buttonID - 4) / displaysPerNode);
            displayID = (buttonID - 4) % displaysPerNode;
        }

        return new int[] { nodeID, displayID };
    }

    public void SetDisplayNormal(int buttonID)
    {
        int[] nodeDisplayIDs = ButtonToNodeDisplayIDs(buttonID);
        int nodeID = nodeDisplayIDs[0];
        int displayID = nodeDisplayIDs[1];

        CAVE2.SendMessage(gameObject.name, "SetDisplayMode", nodeID, displayID, DisplayMode.Normal);
    }

    public void SetDisplayBlack(int buttonID)
    {
        int[] nodeDisplayIDs = ButtonToNodeDisplayIDs(buttonID);
        int nodeID = nodeDisplayIDs[0];
        int displayID = nodeDisplayIDs[1];

        CAVE2.SendMessage(gameObject.name, "SetDisplayMode", nodeID, displayID, DisplayMode.Overlay_Black);
    }

    public void SetDisplayCustom(int buttonID, Color color)
    {
        int[] nodeDisplayIDs = ButtonToNodeDisplayIDs(buttonID);
        int nodeID = nodeDisplayIDs[0];
        int displayID = nodeDisplayIDs[1];

        CAVE2.SendMessage(gameObject.name, "SetDisplayMode", nodeID, displayID, DisplayMode.Overlay_Custom, color);
    }

    void SetDisplayMode(object[] param)
    {
        int nodeID = (int)param[0];
        int displayID = (int)param[1];
        DisplayMode mode = (DisplayMode)param[2];

        CAVE2ClusterManager cluster = CAVE2.GetCAVE2Manager().GetComponent<CAVE2ClusterManager>();

        if(mode == DisplayMode.Overlay_Custom)
        {
            Debug.Log("Color " + (Color)param[3]);
        }

        bool allNodes = (nodeID == -1 && displayID == -1);
        if(allNodes || (CheckNodeID(nodeID) && cluster.GetWindowPositionID() == displayID))
        {
            CAVE2CameraController cameraController = CAVE2.GetCameraController();
            switch(mode)
            {
                case (DisplayMode.Normal):
                    cameraController.RestoreDefaultCameraClearFlags();
                    cameraController.RestoreDefaultCameraCullingMask();
                    break;
                case (DisplayMode.Overlay_Black):
                    cameraController.SetCameraClearFlags(CameraClearFlags.SolidColor);
                    cameraController.SetCameraBackgroundColor(Color.black);
                    cameraController.SetCameraCullingMask(0);
                    break;
                case (DisplayMode.Overlay_Custom):
                    Color color = (Color)param[3];
                    cameraController.SetCameraClearFlags(CameraClearFlags.SolidColor);
                    cameraController.SetCameraBackgroundColor(color);
                    cameraController.SetCameraCullingMask(0);
                    break;
            }

        }
    }

    static bool CheckNodeID(int id)
    {
        if(id == 0 && CAVE2Manager.GetMachineName() == "ORION") // Backwall
        {
            return true;
        }
        else if (id == 1 && CAVE2Manager.GetMachineName() == "ORION-01")
        {
            return true;
        }
        else if (id == 2 && CAVE2Manager.GetMachineName() == "ORION-02")
        {
            return true;
        }
        else if (id == 3 && CAVE2Manager.GetMachineName() == "ORION-03")
        {
            return true;
        }
        else if (id == 4 && CAVE2Manager.GetMachineName() == "ORION-04")
        {
            return true;
        }
        else if (id == 5 && CAVE2Manager.GetMachineName() == "ORION-05")
        {
            return true;
        }
        else if (id == 6 && CAVE2Manager.GetMachineName() == "ORION-06")
        {
            return true;
        }
        else if (id == 7 && CAVE2Manager.GetMachineName() == "ORION-07") // Projector
        {
            return true;
        }

        return false;
    }
}
