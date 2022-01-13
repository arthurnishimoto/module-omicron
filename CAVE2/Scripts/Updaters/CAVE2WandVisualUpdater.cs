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
using omicronConnector;

public class CAVE2WandVisualUpdater : MonoBehaviour
{
    public int wandID = 1;

    public Material litMaterial;
    public Material unlitMaterial;

    public Transform buttonCross;
    public Transform buttonCircle;
    public Transform buttonUp;
    public Transform buttonDown;
    public Transform buttonLeft;
    public Transform buttonRight;

    public Transform buttonL1;
    public Transform buttonL2;
    public Transform buttonL3;

    public Vector2 leftAnalogStick;
    public Vector2 analogTriggers;

    // Gamepad
    public Transform buttonTriangle;
    public Transform buttonSquare;

    public Transform buttonR1;
    public Transform buttonR2;
    public Transform buttonR3;

    public Vector2 rightAnalogStick;

    public Transform buttonSelect;
    public Transform buttonStart;

    MeshRenderer[] meshRenderers;

    [SerializeField]
    bool wandVisible;

    // Use this for initialization
    void Start()
    {
        // Wand (Navigation Controller)
        buttonCross = transform.Find("CrossButton");
        buttonCircle = transform.Find("CircleButton");
        buttonDown = transform.Find("ButtonDown");
        buttonLeft = transform.Find("ButtonLeft");
        buttonRight = transform.Find("ButtonRight");
        buttonUp = transform.Find("ButtonUp");
        buttonL1 = transform.Find("L1");
        buttonL2 = transform.Find("L2");
        buttonL3 = transform.Find("AnalogStick-L3");

        // Full Controller
        buttonTriangle = transform.Find("TriangleButton");
        buttonSquare = transform.Find("SquareButton");

        buttonR1 = transform.Find("R1");
        buttonR2 = transform.Find("R2");
        buttonR3 = transform.Find("AnalogStick-R3");

        buttonSelect = transform.Find("Select");
        buttonStart = transform.Find("Start");

        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        if (!CAVE2.OnCAVE2Display())
        {
            SetWandVisible(true);
        }
        else
        {
            SetWandVisible(wandVisible);
        }
    }

    public void SetWandVisible(bool value)
    {
        foreach(MeshRenderer r in meshRenderers)
        {
            r.enabled = value;
        }
        wandVisible = value;
    }

   public void ToggleWandVisible()
    {
        SetWandVisible(!wandVisible);
    }

    // Update is called once per frame
    void Update()
    {
        leftAnalogStick = new Vector2(CAVE2.GetAxis(CAVE2.Axis.LeftAnalogStickLR, wandID), CAVE2.GetAxis(CAVE2.Axis.LeftAnalogStickUD, wandID));
        rightAnalogStick = new Vector2(CAVE2.GetAxis(CAVE2.Axis.RightAnalogStickLR, wandID), CAVE2.GetAxis(CAVE2.Axis.RightAnalogStickUD, wandID));
        analogTriggers = new Vector2(CAVE2.GetAxis(CAVE2.Axis.AnalogTriggerL, wandID), CAVE2.GetAxis(CAVE2.Axis.AnalogTriggerR, wandID));

        if (buttonL3)
            buttonL3.localEulerAngles = new Vector3(leftAnalogStick.y, 0, -leftAnalogStick.x) * 30;
        if (buttonR3)
            buttonR3.localEulerAngles = new Vector3(rightAnalogStick.y, 0, -rightAnalogStick.x) * 30;

        if (buttonL2)
            buttonL2.localEulerAngles = new Vector3(0, 90, analogTriggers.x * 20);
        if (buttonR2)
            buttonR2.localEulerAngles = new Vector3(0, 90, analogTriggers.y * 20);

        // Tests if hold state is working properly (public state varibles should change)
        // Tests if up/down is working (visual buttons should change)
        SetLit(buttonCross, CAVE2.GetButtonState(CAVE2.Button.Button3, wandID));
        SetLit(buttonCircle, CAVE2.GetButtonState(CAVE2.Button.Button2, wandID));
        SetLit(buttonTriangle, CAVE2.GetButtonState(CAVE2.Button.Button1, wandID));
        SetLit(buttonSquare, CAVE2.GetButtonState(CAVE2.Button.Button4, wandID));

        SetLit(buttonUp, CAVE2.GetButtonState(CAVE2.Button.ButtonUp, wandID));
        SetLit(buttonDown, CAVE2.GetButtonState(CAVE2.Button.ButtonDown, wandID));
        SetLit(buttonLeft, CAVE2.GetButtonState(CAVE2.Button.ButtonLeft, wandID));
        SetLit(buttonRight, CAVE2.GetButtonState(CAVE2.Button.ButtonRight, wandID));

        SetLit(buttonL1, CAVE2.GetButtonState(CAVE2.Button.Button5, wandID));
        SetLit(buttonL2, CAVE2.GetButtonState(CAVE2.Button.Button7, wandID));
        SetLit(buttonL3, CAVE2.GetButtonState(CAVE2.Button.Button6, wandID));

        SetLit(buttonR1, CAVE2.GetButtonState(CAVE2.Button.Button8, wandID));
        SetLit(buttonR2, CAVE2.GetButtonState(CAVE2.Button.SpecialButton3, wandID));
        SetLit(buttonR3, CAVE2.GetButtonState(CAVE2.Button.Button9, wandID));

        SetLit(buttonSelect, CAVE2.GetButtonState(CAVE2.Button.SpecialButton1, wandID));
        SetLit(buttonStart, CAVE2.GetButtonState(CAVE2.Button.SpecialButton2, wandID));
    }

    void SetLit(Transform g, OmicronController.ButtonState state)
    {
        if (g)
        {
            if (state == OmicronController.ButtonState.Held)
                g.GetComponent<Renderer>().material = litMaterial;
            else if (state == OmicronController.ButtonState.Idle)
                g.GetComponent<Renderer>().material = unlitMaterial;
        }
    }
}
