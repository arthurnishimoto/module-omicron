using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class OmicronController : OmicronEventClient
{
    public int sourceID = 1; // Controller 1-4, -1 for any

    public int rawFlags;
    public Vector2 rawAnalogInput1;
    public Vector2 rawAnalogInput2;
    public Vector2 rawAnalogInput3;
    public Vector2 rawAnalogInput4;
    public float deadzone = 0.001f;

    public Vector2 analogInput1;
    public Vector2 analogInput2;
    public Vector2 analogInput3;
    public Vector2 analogInput4;

    public enum ButtonState { Idle, Down, Held, Up };
    public ButtonState Button1;
    public ButtonState Button2;
    public ButtonState Button3;
    public ButtonState Button4;
    public ButtonState Button5;
    public ButtonState Button6;
    public ButtonState Button7;
    public ButtonState Button8;
    public ButtonState Button9;

    public ButtonState SpecialButton1;
    public ButtonState SpecialButton2;
    public ButtonState SpecialButton3;

    public ButtonState ButtonUp;
    public ButtonState ButtonDown;
    public ButtonState ButtonLeft;
    public ButtonState ButtonRight;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeWand;
        InitOmicron();
    }

    // Update is called once per frame
    void Update () {
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

    void OnEvent(EventData e)
    {
        if (e.sourceId == sourceID || sourceID == -1)
        {
            //Debug.Log("OmicronEventClient: '" + name + "' received " + e.serviceType + " sourceId: " + e.sourceId);
            
            // Process button flags
            rawFlags = (int)e.flags;

            // Process analog inputs
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
