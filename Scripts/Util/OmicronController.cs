/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2018		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2018, Electronic Visualization Laboratory, University of Illinois at Chicago
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
using omicron;
using omicronConnector;

public class OmicronController : OmicronEventClient
{
    public int sourceID = 1; // Controller 1-4, -1 for any

    public int rawFlags;
    Vector2 rawAnalogInput1;
    Vector2 rawAnalogInput2;
    Vector2 rawAnalogInput3;
    Vector2 rawAnalogInput4;
    float deadzone = 0.001f;

    [SerializeField] Vector2 analogInput1;
    [SerializeField] Vector2 analogInput2;
    [SerializeField] Vector2 analogInput3;
    [SerializeField] Vector2 analogInput4;

    public enum ButtonState { Idle, Down, Held, Up };
    ButtonState Button1;
    ButtonState Button2;
    ButtonState Button3;
    ButtonState Button4;
    ButtonState Button5;
    ButtonState Button6;
    ButtonState Button7;
    ButtonState Button8;
    ButtonState Button9;

    ButtonState SpecialButton1;
    ButtonState SpecialButton2;
    ButtonState SpecialButton3;

    ButtonState ButtonUp;
    ButtonState ButtonDown;
    ButtonState ButtonLeft;
    ButtonState ButtonRight;

    EventBase.ServiceType defaultType = EventBase.ServiceType.ServiceTypeController;

    // bool buttonsChanged;

    public void SetAsWand()
    {
        defaultType = EventBase.ServiceType.ServiceTypeWand;
    }

    // Use this for initialization
    new void Start()
    {
        eventOptions = defaultType;
        InitOmicron();
    }

    // Update is called once per frame
    void Update() {

        if (CAVE2.IsMaster())
        {
            UpdateButtons(rawFlags);

            if (CAVE2.GetCAVE2Manager().sendTrackingData)
            {
                CAVE2.SendMessage(gameObject.name, "UpdateButtonStates", Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, SpecialButton1, SpecialButton2, SpecialButton3, ButtonUp, ButtonDown, ButtonLeft, ButtonRight);
            }
        }
    }

    void LateUpdate()
    {
        UpdateButtons(rawFlags);
    }

    public ButtonState GetButtonState(CAVE2.Button button)
    {
        switch(button)
        {
            case CAVE2.Button.Button1: return Button1;
            case CAVE2.Button.Button2: return Button2;
            case CAVE2.Button.Button3: return Button3;
            case CAVE2.Button.Button4: return Button4;
            case CAVE2.Button.Button5: return Button5;
            case CAVE2.Button.Button6: return Button6;
            case CAVE2.Button.Button7: return Button7;
            case CAVE2.Button.Button8: return Button8;
            case CAVE2.Button.Button9: return Button9;

            case CAVE2.Button.SpecialButton1: return SpecialButton1;
            case CAVE2.Button.SpecialButton2: return SpecialButton2;
            case CAVE2.Button.SpecialButton3: return SpecialButton3;

            case CAVE2.Button.ButtonUp: return ButtonUp;
            case CAVE2.Button.ButtonDown: return ButtonDown;
            case CAVE2.Button.ButtonLeft: return ButtonLeft;
            case CAVE2.Button.ButtonRight: return ButtonRight;
        }
        return ButtonState.Idle;
    }

    public float GetAxis(CAVE2.Axis axis)
    {
        switch (axis)
        {
            case CAVE2.Axis.LeftAnalogStickLR: return analogInput1.x;
            case CAVE2.Axis.LeftAnalogStickUD: return analogInput1.y;
            case CAVE2.Axis.RightAnalogStickLR: return analogInput2.x;
            case CAVE2.Axis.RightAnalogStickUD: return analogInput2.x;
            case CAVE2.Axis.AnalogTriggerL: return analogInput3.x;

            case CAVE2.Axis.LeftAnalogStickLR_Inverted: return -analogInput1.x;
            case CAVE2.Axis.LeftAnalogStickUD_Inverted: return -analogInput1.y;
            case CAVE2.Axis.AnalogTriggerL_Inverted: return -analogInput3.x;
        }
        return 0;
    }

    public Vector2 GetAnalogStick(int ID)
    {
        switch(ID)
        {
            case (1): return analogInput1;
            case (2): return analogInput2;
            case (3): return analogInput3;
            case (4): return analogInput4;
        }
        return Vector2.zero;
    }

    public override void OnEvent(EventData e)
    {
        if (e.sourceId == sourceID || sourceID == -1)
        {
            //Debug.Log("OmicronEventClient: '" + name + "' received " + e.serviceType + " sourceId: " + e.sourceId);
            // if ((int)e.flags != rawFlags)
            // buttonsChanged = true;

            rawFlags = (int)e.flags;
            rawAnalogInput1 = new Vector2(e.getExtraDataFloat(0), e.getExtraDataFloat(1));
            rawAnalogInput2 = new Vector2(e.getExtraDataFloat(2), e.getExtraDataFloat(3));
            rawAnalogInput3 = new Vector2(e.getExtraDataFloat(4), e.getExtraDataFloat(5));
            rawAnalogInput4 = new Vector2(e.getExtraDataFloat(6), e.getExtraDataFloat(7));

            analogInput1 = ApplyDeadzone(rawAnalogInput1);
            analogInput2 = ApplyDeadzone(rawAnalogInput2);
            analogInput3 = rawAnalogInput3;
            analogInput4 = rawAnalogInput4;

            // Flip Up/Down analog stick values
            analogInput1.y *= -1;
            analogInput2.y *= -1;

            CAVE2.SendMessage(gameObject.name, "UpdateAnalogC2", analogInput1, analogInput2, analogInput3, analogInput4);
            CAVE2.SendMessage(gameObject.name, "UpdateRawFlagsC2", (int)e.flags);
        }
    }

    Vector2 ApplyDeadzone(Vector2 input)
    {
        Vector2 output = input;
        if(Mathf.Abs(input.x) < deadzone)
        {
            output.x = 0;
        }
        if (Mathf.Abs(input.y) < deadzone)
        {
            output.y = 0;
        }
        return output;
    }

    public void UpdateAnalog(Vector2 analog1, Vector2 analog2, Vector2 analog3, Vector2 analog4)
    {
        analogInput1 = analog1;
        analogInput2 = analog2;
        analogInput3 = analog3;
        analogInput4 = analog4;
    }

    public void UpdateRawFlagsC2(int flags)
    {
        rawFlags = flags;
    }

    public void UpdateAnalogC2(object[] param)
    {
        analogInput1 = (Vector2)param[0];
        analogInput2 = (Vector2)param[1];
        analogInput3 = (Vector2)param[2];
        analogInput4 = (Vector2)param[3];
    }

    public void UpdateButtons(int flags)
    {
        UpdateButton(ref Button1, EventBase.Flags.Button1, flags);
        UpdateButton(ref Button2, EventBase.Flags.Button2, flags);
        UpdateButton(ref Button3, EventBase.Flags.Button3, flags);
        UpdateButton(ref Button4, EventBase.Flags.Button4, flags);
        UpdateButton(ref Button5, EventBase.Flags.Button5, flags);
        UpdateButton(ref Button6, EventBase.Flags.Button6, flags);
        UpdateButton(ref Button7, EventBase.Flags.Button7, flags);
        UpdateButton(ref Button8, EventBase.Flags.Button8, flags);
        UpdateButton(ref Button9, EventBase.Flags.Button9, flags);

        UpdateButton(ref SpecialButton1, EventBase.Flags.SpecialButton1, flags);
        UpdateButton(ref SpecialButton2, EventBase.Flags.SpecialButton2, flags);
        UpdateButton(ref SpecialButton3, EventBase.Flags.SpecialButton3, flags);

        UpdateButton(ref ButtonUp, EventBase.Flags.ButtonUp, flags);
        UpdateButton(ref ButtonDown, EventBase.Flags.ButtonDown, flags);
        UpdateButton(ref ButtonLeft, EventBase.Flags.ButtonLeft, flags);
        UpdateButton(ref ButtonRight, EventBase.Flags.ButtonRight, flags);
    }

    void UpdateButtonStates(object[] param)
    {
        Button1 = (ButtonState)param[0];
        Button2 = (ButtonState)param[1];
        Button3 = (ButtonState)param[2];
        Button4 = (ButtonState)param[3];
        Button5 = (ButtonState)param[4];
        Button6 = (ButtonState)param[5];
        Button7 = (ButtonState)param[6];
        Button8 = (ButtonState)param[7];
        Button9 = (ButtonState)param[8];

        SpecialButton1 = (ButtonState)param[9];
        SpecialButton2 = (ButtonState)param[10];
        SpecialButton3 = (ButtonState)param[11];

        ButtonUp = (ButtonState)param[12];
        ButtonDown = (ButtonState)param[13];
        ButtonLeft = (ButtonState)param[14];
        ButtonRight = (ButtonState)param[15];

        // buttonsChanged = true;
    }

    void UpdateButton(ref ButtonState button, EventBase.Flags buttonFlag, int flags)
    {
        //Debug.Log((int)buttonFlag + " " + flags + " " + (int)(flags & (int)buttonFlag));
        if (button == ButtonState.Idle && (int)(flags & (int)buttonFlag) != 0)
        {
            button = ButtonState.Down;
            //Debug.Log("Button " + buttonFlag + " DOWN");
        }
        else if (button == ButtonState.Down && (int)(flags & (int)buttonFlag) != 0)
        {
            button = ButtonState.Held;
            //Debug.Log("Button " + buttonFlag + " HELD");
        }
        else if (button == ButtonState.Down && (int)(flags & (int)buttonFlag) == 0)
        {
            button = ButtonState.Up;
            //Debug.Log("Button " + buttonFlag + " UP");
        }
        else if (button == ButtonState.Held && (int)(flags & (int)buttonFlag) == 0)
        {
            button = ButtonState.Up;
            //Debug.Log("Button " + buttonFlag + " UP");
        }
        else if (button == ButtonState.Up && (int)(flags & (int)buttonFlag) == 0)
        {
            button = ButtonState.Idle;
            //Debug.Log("Button " + buttonFlag + " IDLE");
        }
    }
}
