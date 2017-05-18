using UnityEngine;
using System.Collections;

public class CAVE2WandNavigator : MonoBehaviour {

    public int headID = 1;
    public int wandID = 1;

    public CAVE2.Axis forwardAxis = CAVE2.Axis.LeftAnalogStickUD;
    public CAVE2.Axis strafeAxis = CAVE2.Axis.LeftAnalogStickLR;
    public CAVE2.Axis lookUDAxis = CAVE2.Axis.RightAnalogStickUD;
    public CAVE2.Axis lookLRAxis = CAVE2.Axis.RightAnalogStickLR;
    public CAVE2.Axis verticalAxis = CAVE2.Axis.AnalogTriggerL;

    public float forward;
    public float strafe;
    public float vertical;
    public Vector2 lookAround = new Vector2();

    public float globalSpeedMod = 1.0f;
    public float movementScale = 2;
    public float flyMovementScale = 10;
    public float turnSpeed = 20;

    public Vector3 moveDirection;

    // Walk - Analog stick movement with physics
    // Fly - 6DoF movement with no physics
    // Drive - Same as fly without pitch/roll
    public enum NavigationMode { Walk, Drive, Freefly }

    public NavigationMode navMode = NavigationMode.Walk;
    public CAVE2.Button freeFlyToggleButton = CAVE2.Button.Button5;
    public CAVE2.Button freeFlyButton = CAVE2.Button.Button7;

    public enum HorizonalMovementMode { Strafe, Turn }
    public HorizonalMovementMode horizontalMovementMode = HorizonalMovementMode.Strafe;

    public enum AutoLevelMode { Disabled, OnGroundCollision };
    public AutoLevelMode autoLevelMode = AutoLevelMode.OnGroundCollision;
    public CAVE2.Button autoLevelButton = CAVE2.Button.Button6;

    public enum ForwardRef { CAVEFront, Head, Wand }
    public ForwardRef forwardReference = ForwardRef.Wand;

    public bool freeflyButtonDown;
    public bool freeflyInitVectorSet;
    public Vector3 freeflyInitVector;
    public Vector3 wandPosition;
    public Vector3 movementVector;

    Vector3 fly_x, fly_y, fly_z;

    CAVE2PlayerCollider playerCollider;

    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public NavigationMode initMode;

    public UnityEngine.UI.Text positionUIText;

    public void Reset()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        //navMode = initMode;
    }

    // Use this for initialization
    void Start ()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initMode = navMode;

        playerCollider = GetComponent<CAVE2PlayerCollider>();

    }
	
    void FixedUpdate()
    {
        if (navMode == NavigationMode.Drive || navMode == NavigationMode.Freefly)
        {
            UpdateFreeflyMovement();
        }
        else
        {
            UpdateWalkMovement();
        }
    }

	// Update is called once per frame
	void Update()
    {
        if(positionUIText)
            positionUIText.text = "Position: " + transform.position + "\nRotation: " + transform.eulerAngles;

        forward = CAVE2.Input.GetAxis(wandID, forwardAxis);
        forward *= movementScale;

        strafe = CAVE2.GetAxis(wandID, strafeAxis);
        strafe *= movementScale;

        lookAround.x = CAVE2.GetAxis(wandID, lookUDAxis);
        lookAround.x *= movementScale;
        lookAround.y = CAVE2.GetAxis(wandID, lookLRAxis);
        lookAround.y *= movementScale;

        vertical = CAVE2.GetAxis(wandID, verticalAxis);
        vertical *= movementScale;

        freeflyButtonDown = CAVE2.GetButton(wandID, freeFlyButton);

        if (CAVE2.GetButtonDown(wandID, freeFlyToggleButton))
        {
            if (navMode == NavigationMode.Walk)
                SetNavigationMode((int)NavigationMode.Drive);
            else if (navMode == NavigationMode.Drive)
                SetNavigationMode((int)NavigationMode.Freefly);
            else
                SetNavigationMode((int)NavigationMode.Walk);
        }
    }

    void SetNavigationMode(int val)
    {
        navMode = (NavigationMode)val;
    }

    public void SetNavModeWalk(bool val)
    {
        navMode = NavigationMode.Walk;
    }

    public void SetNavModeDrive(bool val)
    {
        navMode = NavigationMode.Drive;
    }

    public void SetNavModeFreefly(bool val)
    {
        navMode = NavigationMode.Freefly;
    }

    void UpdateWalkMovement()
    {
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 nextPos = transform.position;
        float forwardAngle = transform.eulerAngles.y;

        if (forwardReference == ForwardRef.Head)
            forwardAngle = CAVE2.GetHeadObject(headID).transform.eulerAngles.y;
        else if (forwardReference == ForwardRef.Wand)
            forwardAngle = CAVE2.GetWandObject(wandID).transform.eulerAngles.y;

        if (horizontalMovementMode == HorizonalMovementMode.Strafe)
        {
            nextPos.z += forward * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * forwardAngle);
            nextPos.x += forward * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * forwardAngle);

            nextPos.z += strafe * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * (forwardAngle + 90));
            nextPos.x += strafe * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * (forwardAngle + 90));

            transform.position = nextPos;
            transform.Rotate(new Vector3(0, lookAround.y, 0) * Time.deltaTime * turnSpeed);
        }
        else if (horizontalMovementMode == HorizonalMovementMode.Turn)
        {
            nextPos.z += forward * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * forwardAngle);
            nextPos.x += forward * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * forwardAngle);
            transform.position = nextPos;

            transform.RotateAround(transform.position + transform.rotation * CAVE2.GetHeadPosition(headID) * 2, Vector3.up, strafe * Time.deltaTime * turnSpeed);
            transform.Rotate(new Vector3(0, strafe, 0) * Time.deltaTime * turnSpeed);
        }

        if (autoLevelMode == AutoLevelMode.OnGroundCollision)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    void UpdateFreeflyMovement()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        wandPosition = CAVE2.Input.GetWandPosition(wandID);
        Quaternion wandRotation = CAVE2.Input.GetWandRotation(wandID);

        Vector3 headPosition = CAVE2.Input.GetHeadPosition(headID);
        Quaternion headRotation = CAVE2.Input.GetHeadRotation(headID);

        if (freeflyButtonDown && !freeflyInitVectorSet)
        {
            freeflyInitVector = wandPosition;
            freeflyInitVectorSet = true;

            Vector3 xVec = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 yVec = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 zVec = new Vector3(0.0f, 0.0f, 1.0f);
            fly_x = wandRotation * xVec;
            fly_y = wandRotation * yVec;
            fly_z = wandRotation * zVec;
        }
        else if (!freeflyButtonDown)
        {
            freeflyInitVector = Vector3.zero;
            freeflyInitVectorSet = false;

        }
        else
        {
            movementVector = (wandPosition - freeflyInitVector);

            // Ported from Electro's Vortex application by Robert Kooima
            //Vector3 xVec = new Vector3(1.0f,0.0f,0.0f);
            Vector3 yVec = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 zVec = new Vector3(0.0f, 0.0f, 1.0f);
            //Vector3 x = wandRotation * xVec;
            Vector3 y = wandRotation * yVec;
            Vector3 z = wandRotation * zVec;

            float vx = fly_z.x - z.x;
            float vy = fly_z.y - z.y;
            float vz = fly_z.z - z.z;

            float wx = fly_y.x - y.x;
            float wy = fly_y.y - y.y;
            float wz = fly_y.z - y.z;

            float rX = (vx * fly_y.x + vy * fly_y.y + vz * fly_y.z) * Time.deltaTime * turnSpeed;
            float rY = -(vx * fly_x.x + vy * fly_x.y + vz * fly_x.z) * Time.deltaTime * turnSpeed;
            float rZ = (wx * fly_x.x + wy * fly_x.y + wz * fly_x.z) * Time.deltaTime * turnSpeed;

            transform.Translate(movementVector * flyMovementScale * globalSpeedMod);
            if (navMode == NavigationMode.Freefly)
                transform.Rotate(new Vector3(rX, rY, rZ));
        }

        Vector3 nextPos = transform.position;
        float forwardAngle = transform.eulerAngles.y;

        if (forwardReference == ForwardRef.Head)
            forwardAngle = CAVE2.GetHeadObject(headID).transform.eulerAngles.y;
        else if (forwardReference == ForwardRef.Wand)
            forwardAngle = CAVE2.GetWandObject(wandID).transform.eulerAngles.y;

        nextPos.z += forward * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * forwardAngle) * flyMovementScale;
        nextPos.x += forward * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * forwardAngle) * flyMovementScale;
        nextPos.y += vertical * Time.deltaTime * flyMovementScale * globalSpeedMod;

        if (horizontalMovementMode == HorizonalMovementMode.Strafe)
        {
            nextPos.z += strafe * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * (forwardAngle + 90)) * flyMovementScale;
            nextPos.x += strafe * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * (forwardAngle + 90)) * flyMovementScale;

            transform.position = nextPos;
            transform.Rotate(new Vector3(0, lookAround.y, 0) * Time.deltaTime * turnSpeed);
        }
        else if (horizontalMovementMode == HorizonalMovementMode.Turn)
        {
            transform.position = nextPos;
            transform.RotateAround(transform.position + transform.rotation * CAVE2.GetHeadPosition(headID) * 2, Vector3.up, strafe * Time.deltaTime * turnSpeed);
            transform.Rotate(new Vector3(0, strafe, 0) * Time.deltaTime * turnSpeed);
        }
    }
}
