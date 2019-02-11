using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HoloLensCameraCalibrationUI : MonoBehaviour {

    [SerializeField]
    bool enableKeyboardShortcuts;

    [SerializeField]
    Text cameraSettingText;

    [SerializeField]
    CustomHMDPerspective hololensPerspective;

    [SerializeField]
    RemoteTerminal commandLineTerminal;

    [SerializeField]
    bool hideCAVE2View;

    bool lastHideViewState;

	// Use this for initialization
	void Start () {
        lastHideViewState = hideCAVE2View;
    }
	
	// Update is called once per frame
	void Update () {
        cameraSettingText.text = "Head Projection Offset\n";
        Vector3 hpo = hololensPerspective.GetHeadProjectionOffset();
        cameraSettingText.text += "(" + hpo.x + ", " + hpo.y + ", " + hpo.z + ")\n";
        cameraSettingText.text += "Head Origin Offset\n";
        Vector3 hoo = hololensPerspective.GetHeadOriginOffset();
        cameraSettingText.text += "(" + hoo.x + ", " + hoo.y + ", " + hoo.z + ")\n";

        if(hideCAVE2View != lastHideViewState)
        {
            if(hideCAVE2View)
            {
                commandLineTerminal.SendCommand("SetCAVE2DisplayCover HoloManager true");
            }
            else
            {
                commandLineTerminal.SendCommand("SetCAVE2DisplayCover HoloManager false");
            }
            lastHideViewState = hideCAVE2View;
        }
        if( enableKeyboardShortcuts )
        {
            bool changed = false;
            if(Input.GetKeyDown(KeyCode.Q))
            {
                hpo.x += 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                hpo.x -= 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                hpo.y += 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                hpo.y -= 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                hpo.z += 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                hpo.z -= 0.01f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                hoo.x += 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                hoo.x -= 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                hoo.y += 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                hoo.y -= 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                hoo.z += 0.01f;
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                hoo.z -= 0.01f;
                changed = true;
            }

            if (changed)
            {
                hololensPerspective.SetHeadProjectionOffset(hpo);
                hololensPerspective.SetHeadOriginOffset(hoo);
                commandLineTerminal.SendCommand("SetHeadProjectionOffset HoloManager " + hpo.x + " " + hpo.y + " " + hpo.z);
                commandLineTerminal.SendCommand("SetHeadOriginOffset HoloManager " + hoo.x + " " + hoo.y + " " + hoo.z);
            }
        }
    }
}
