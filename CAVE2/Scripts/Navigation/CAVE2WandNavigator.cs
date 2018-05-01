using UnityEngine;
using System.Collections;

public class CAVE2WandNavigator : MonoBehaviour {

    [Header("Input")]
    [SerializeField] int headID = 1;
    [SerializeField] int wandID = 1;

    [SerializeField] CAVE2.Axis forwardAxis = CAVE2.Axis.LeftAnalogStickUD;
    [SerializeField] CAVE2.Axis strafeAxis = CAVE2.Axis.LeftAnalogStickLR;
    [SerializeField] CAVE2.Axis lookUDAxis = CAVE2.Axis.RightAnalogStickUD;
    [SerializeField] CAVE2.Axis lookLRAxis = CAVE2.Axis.RightAnalogStickLR;
    [SerializeField] CAVE2.Axis verticalAxis = CAVE2.Axis.AnalogTriggerL;

    [SerializeField] float forward;
    [SerializeField] float strafe;
    [SerializeField] float vertical;
    public Vector2 lookAround = new Vector2();

    [Header("Movement Speed")]
    [SerializeField] bool walkUsesFlyGlobalSpeedScale = false;
    public float globalSpeedMod = 1.0f;
    [SerializeField] float movementScale = 2;
    [SerializeField] float flyMovementScale = 10;
    [SerializeField] float turnSpeed = 20;

    public Vector3 moveDirection;

    // Walk - Analog stick movement with physics
    // Fly - 6DoF movement with no physics
    // Drive - Same as fly without pitch/roll
    public enum NavigationMode { Disabled, Walk, Drive, Freefly }

    [Header("Navigation Mode")]
    public NavigationMode navMode = NavigationMode.Walk;
    [SerializeField] CAVE2.Button freeFlyToggleButton = CAVE2.Button.Button5;
    [SerializeField] CAVE2.Button freeFlyButton = CAVE2.Button.Button7;
    NavigationMode lastNavMode;

    public enum HorizonalMovementMode { Strafe, Turn }
    public HorizonalMovementMode horizontalMovementMode = HorizonalMovementMode.Strafe;

    

    public enum ForwardRef { CAVEFront, Head, Wand }
    [SerializeField] ForwardRef forwardReference = ForwardRef.Wand;

    bool freeflyButtonDown;
    bool freeflyInitVectorSet;
    Vector3 freeflyInitVector;
    Vector3 wandPosition;
    Vector3 movementVector;

    Quaternion freeflyInitRotation;

    Vector3 fly_x, fly_y, fly_z;

    public enum AutoLevelMode { Disabled, OnGroundCollision };

    [Header("Collisions")]
    [SerializeField] AutoLevelMode autoLevelMode = AutoLevelMode.OnGroundCollision;
    [SerializeField] CAVE2.Button autoLevelButton = CAVE2.Button.Button6;

    [SerializeField]
    CapsuleCollider bodyCollider;

    Vector3 initialPosition;
    Quaternion initialRotation;
    NavigationMode initMode;

    public void Reset()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        navMode = initMode;
    }

    public void DisableMovement()
    {
        lastNavMode = navMode;
        navMode = NavigationMode.Disabled;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void EnableMovement()
    {
        navMode = lastNavMode;
    }

    // Use this for initialization
    void Start ()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initMode = navMode;
        lastNavMode = navMode;

        if( bodyCollider == null )
            bodyCollider = GetComponent<CapsuleCollider>();

        if( navMode == NavigationMode.Disabled )
        {
            DisableMovement();
            lastNavMode = NavigationMode.Walk;
        }
    }

    void FixedUpdate()
    {
        if (navMode == NavigationMode.Drive || navMode == NavigationMode.Freefly)
        {
            UpdateFreeflyMovement();
        }
        else if (navMode == NavigationMode.Walk)
        {
            UpdateWalkMovement();
        }
    }

	// Update is called once per frame
	void Update()
    {
        float speedMod = 1;
        if(walkUsesFlyGlobalSpeedScale)
        {
            speedMod = globalSpeedMod;
        }

        forward = CAVE2.Input.GetAxis(wandID, forwardAxis);
        forward *= movementScale * speedMod;

        strafe = CAVE2.GetAxis(wandID, strafeAxis);
        strafe *= movementScale;

        lookAround.x = CAVE2.GetAxis(wandID, lookUDAxis);
        lookAround.x *= movementScale;
        lookAround.y = CAVE2.GetAxis(wandID, lookLRAxis);
        lookAround.y *= movementScale;

        vertical = CAVE2.GetAxis(wandID, verticalAxis);
        vertical *= movementScale * speedMod;

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

    public void SetNavModeStrafe(bool val)
    {
        horizontalMovementMode = HorizonalMovementMode.Strafe;
    }

    public void SetNavModeRotate(bool val)
    {
        horizontalMovementMode = HorizonalMovementMode.Turn;
    }

    void UpdateWalkMovement()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        bodyCollider.GetComponent<Rigidbody>().useGravity = true;
        bodyCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 nextPos = transform.position;
        float forwardAngle = transform.eulerAngles.y;

        if (forwardReference == ForwardRef.Head)
            forwardAngle = CAVE2.GetHeadObject(headID).transform.eulerAngles.y;
        else if (forwardReference == ForwardRef.Wand)
        {
            forwardAngle = CAVE2.GetWandObject(wandID).transform.eulerAngles.y;
        }
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
        bodyCollider.GetComponent<Rigidbody>().useGravity = false;
        bodyCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        wandPosition = CAVE2.Input.GetWandPosition(wandID);
        Quaternion wandRotation = CAVE2.Input.GetWandRotation(wandID);

        if (freeflyButtonDown && !freeflyInitVectorSet)
        {
            freeflyInitVector = wandPosition;
            freeflyInitRotation = wandRotation;
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
            freeflyInitRotation = Quaternion.identity;
            freeflyInitVectorSet = false;
        }
        else
        {
            movementVector = (wandPosition - freeflyInitVector);

            // Ported from Electro's Vortex application by Robert Kooima
            Vector3 xVec = new Vector3(1.0f,0.0f,0.0f);
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
            {
                // Old 'Electro' method
                //transform.Rotate(new Vector3(rX, rY, rZ));

                Quaternion quat = Quaternion.Inverse(freeflyInitRotation) * CAVE2.GetWandRotation(wandID);
                transform.Rotate(CAVE2.GetWandObject(wandID).transform.forward, quat.z * Time.deltaTime * turnSpeed);
                transform.Rotate(CAVE2.GetWandObject(wandID).transform.right, quat.x * Time.deltaTime * turnSpeed);
                transform.Rotate(CAVE2.GetWandObject(wandID).transform.up, quat.y * Time.deltaTime * turnSpeed);
            }
        }

        Vector3 nextPos = transform.position;
        float forwardAngle = transform.eulerAngles.y;

        Vector3 directionalVector = Vector3.zero;

        if (forwardReference == ForwardRef.Head)
            directionalVector = CAVE2.GetHeadObject(headID).transform.forward;
        else if (forwardReference == ForwardRef.Wand)
            directionalVector = CAVE2.GetWandObject(wandID).transform.forward;

        //nextPos.z += forward * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * forwardAngle) * flyMovementScale;
        //nextPos.x += forward * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * forwardAngle) * flyMovementScale;
        //nextPos.y += vertical * Time.deltaTime * flyMovementScale * globalSpeedMod;

        nextPos += forward * Time.deltaTime * directionalVector * flyMovementScale;

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
            transform.RotateAround(transform.position + transform.rotation * CAVE2.GetHeadPosition(headID) * 2, Vector3.up, strafe * Time.deltaTime);
            transform.Rotate(new Vector3(0, strafe, 0) * Time.deltaTime * turnSpeed);
        }
    }
}
