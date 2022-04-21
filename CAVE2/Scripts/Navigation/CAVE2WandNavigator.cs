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

public class CAVE2WandNavigator : MonoBehaviour {

    [Header("Input")]
    [SerializeField] int headID = 1;
    [SerializeField] int wandID = 1;

    public CAVE2.Axis forwardAxis = CAVE2.Axis.LeftAnalogStickUD;
    public CAVE2.Axis strafeAxis = CAVE2.Axis.LeftAnalogStickLR;
    public CAVE2.Axis lookUDAxis = CAVE2.Axis.RightAnalogStickUD;
    public CAVE2.Axis lookLRAxis = CAVE2.Axis.RightAnalogStickLR;
    public CAVE2.Axis verticalAxis = CAVE2.Axis.AnalogTriggerL;

    [SerializeField] float forward;
    [SerializeField] float strafe;
    [SerializeField] float vertical;
    public Vector2 lookAround = new Vector2();

    [Header("Movement Speed")]
    [SerializeField] bool walkUsesFlyGlobalSpeedScale = false;
    public float globalSpeedMod = 1.0f;
    [SerializeField] float movementScale = 5;
    [SerializeField] float flyMovementScale = 5;
    [SerializeField] float turnSpeed = 20;

    [SerializeField] bool smoothMovement = true;
    [SerializeField] float smoothMovementTime = 0.5f;

    Vector3 moveDirection;

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

    Vector3 fly_x, fly_y, fly_z;

    [SerializeField]
    bool updateOnlyOnMaster = false;

    public enum AutoLevelMode { Disabled, OnGroundCollision };

    [Header("Collisions")]
    [SerializeField] AutoLevelMode autoLevelMode = AutoLevelMode.OnGroundCollision;
    // [SerializeField] CAVE2.Button autoLevelButton = CAVE2.Button.Button6;

    [SerializeField]
    CapsuleCollider bodyCollider;

    Vector3 initialPosition;
    Quaternion initialRotation;
    NavigationMode initMode;

    [SerializeField]
    GameObject worldNavigationManager = null;

    public bool hasInput;

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

    public float GetTurnSpeed()
    {
        return turnSpeed;
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
        if (!updateOnlyOnMaster || (updateOnlyOnMaster && CAVE2.IsMaster()))
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
    }

    void SetPosition(Vector3 position)
    {
        if (worldNavigationManager)
        {
            worldNavigationManager.SendMessage("SetWorldTranslation", position);
        }
        else
        {
            transform.position = position;
        }
    }

    public void SetActiveWandID(int id)
    {
        wandID = id;
    }

	// Update is called once per frame
	void Update()
    {
        float speedMod = 1;
        if (walkUsesFlyGlobalSpeedScale)
        {
            speedMod = globalSpeedMod;
        }

        if (wandID == -1) // Special wand ID 1 or 2 case
        {
            forward = CAVE2.Input.GetAxis(1, forwardAxis);
            strafe = CAVE2.GetAxis(strafeAxis, 1);
            lookAround.x = CAVE2.GetAxis(lookUDAxis, 1);
            lookAround.y = CAVE2.GetAxis(lookLRAxis, 1);

            forward += CAVE2.Input.GetAxis(2, forwardAxis);
            strafe += CAVE2.GetAxis(strafeAxis, 2);
            lookAround.x += CAVE2.GetAxis(lookUDAxis, 2);
            lookAround.y += CAVE2.GetAxis(lookLRAxis, 2);
        }
        else // Normal single wandID case
        {
            forward = CAVE2.Input.GetAxis(wandID, forwardAxis);
            strafe = CAVE2.GetAxis(strafeAxis, wandID);
            lookAround.x = CAVE2.GetAxis(lookUDAxis, wandID);
            lookAround.y = CAVE2.GetAxis(lookLRAxis, wandID);
        }
        
        forward *= movementScale * speedMod;
        strafe *= movementScale;
        lookAround.x *= movementScale;
        lookAround.y *= movementScale;

        if(CAVE2.IsSimulatorMode())
        {
            if(Input.GetKey(CAVE2.GetCAVE2Manager().GetComponent<CAVE2InputManager>().simulatorFlyUp))
            {
                vertical = 1;
            }
            else if(Input.GetKey(CAVE2.GetCAVE2Manager().GetComponent<CAVE2InputManager>().simulatorFlyDown))
            {
                vertical = -1;
            }
            else
            {
                vertical = 0;
            }
            vertical *= movementScale * speedMod;
        }
        else
        {
            vertical = CAVE2.GetAxis(verticalAxis, wandID);
            vertical *= movementScale * speedMod;
        }

        freeflyButtonDown = CAVE2.GetButton(freeFlyButton, wandID);

        hasInput = (forward != 0 || strafe != 0 || lookAround.magnitude > 0 || vertical != 0 || freeflyButtonDown);

        if (CAVE2.GetButtonDown(freeFlyToggleButton, wandID))
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
        if (Time.timeSinceLevelLoad > 1)
        { // Prevent Unity 2019.1.2 bug where UI triggers all of these these at start
            navMode = (NavigationMode)val;

            // Update Nav button UI state
            GetComponentInChildren<NavModeUI>().UpdateNavButtons();
        }
    }

    public void SetNavModeWalk(bool val)
    {
        if (Time.timeSinceLevelLoad > 1)
        {
            navMode = NavigationMode.Walk;

            // Update Nav button UI state
            GetComponentInChildren<NavModeUI>().UpdateNavButtons();
        }
    }

    public void SetNavModeDrive(bool val)
    {
        if (Time.timeSinceLevelLoad > 1)
        {
            navMode = NavigationMode.Drive;

            // Update Nav button UI state
            GetComponentInChildren<NavModeUI>().UpdateNavButtons();
        }
    }

    public void SetNavModeFreefly(bool val)
    {
        if (Time.timeSinceLevelLoad > 1 && navMode != NavigationMode.Freefly)
        {
            navMode = NavigationMode.Freefly;

            // Update Nav button UI state
            GetComponentInChildren<NavModeUI>().UpdateNavButtons();
        }
    }

    public void SetNavModeStrafe(bool val)
    {
        if (Time.timeSinceLevelLoad > 1)
            horizontalMovementMode = HorizonalMovementMode.Strafe;
    }

    public void SetNavModeRotate(bool val)
    {
        if (Time.timeSinceLevelLoad > 1)
            horizontalMovementMode = HorizonalMovementMode.Turn;
    }

    private Vector3 velocity = Vector3.zero;

    void UpdateWalkMovement()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        bodyCollider.GetComponent<Rigidbody>().useGravity = true;
        bodyCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 nextPos = transform.position;
        float forwardAngle = transform.eulerAngles.y;

        nextPos.y += vertical * Time.deltaTime * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);

        if (forwardReference == ForwardRef.Head)
            forwardAngle = CAVE2.GetHeadObject(headID).transform.eulerAngles.y;
        else if (forwardReference == ForwardRef.Wand)
        {
            forwardAngle = CAVE2.GetWandObject(wandID).transform.eulerAngles.y;
        }
        if (horizontalMovementMode == HorizonalMovementMode.Strafe)
        {
            nextPos.z += forward * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * forwardAngle) * (smoothMovement ? movementScale * 20 : movementScale);
            nextPos.x += forward * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * forwardAngle) * (smoothMovement ? movementScale * 20 : movementScale);

            nextPos.z += strafe * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * (forwardAngle + 90)) * (smoothMovement ? movementScale * 20 : movementScale);
            nextPos.x += strafe * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * (forwardAngle + 90)) * (smoothMovement ? movementScale * 20 : movementScale);

            if (smoothMovement)
            {
                transform.position = Vector3.SmoothDamp(transform.position, nextPos, ref velocity, smoothMovementTime);
            }
            else
            {
                transform.position = nextPos;
            }

            transform.Rotate(new Vector3(lookAround.x, lookAround.y, 0) * Time.deltaTime * turnSpeed);
        }
        else if (horizontalMovementMode == HorizonalMovementMode.Turn)
        {
            nextPos.z += forward * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * forwardAngle) * (smoothMovement ? movementScale * 20 : movementScale);
            nextPos.x += forward * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * forwardAngle) * (smoothMovement ? movementScale * 20 : movementScale);

            if (smoothMovement)
            {
                transform.position = Vector3.SmoothDamp(transform.position, nextPos, ref velocity, smoothMovementTime);
            }
            else
            {
                transform.position = nextPos;
            }

            transform.RotateAround(transform.position + transform.rotation * CAVE2.GetHeadPosition(headID), Vector3.up, strafe * Time.deltaTime * turnSpeed);
        }

        if (autoLevelMode == AutoLevelMode.OnGroundCollision)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    void UpdateFreeflyMovement()
    {
        Vector3 nextPos = transform.position;

        bodyCollider.GetComponent<Rigidbody>().useGravity = false;
        bodyCollider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        wandPosition = CAVE2.Input.GetWandPosition(wandID);
        Quaternion wandRotation = CAVE2.Input.GetWandRotation(wandID);

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

            nextPos += transform.localRotation * movementVector * flyMovementScale * globalSpeedMod;
            if (navMode == NavigationMode.Freefly)
            {
                transform.Rotate( CAVE2.GetWandObject(wandID).transform.localRotation * new Vector3(rX, rY, rZ));
            }
        }

        float forwardAngle = transform.eulerAngles.y;

        if (wandID == -1) // Special either wand 1 or 2 case 
        {
            if (forwardReference == ForwardRef.Head)
                forwardAngle = CAVE2.GetHeadObject(headID).transform.eulerAngles.y;
            else if (forwardReference == ForwardRef.Wand)
            {
                if (CAVE2.Input.GetAxis(1, forwardAxis) != 0)
                {
                    forwardAngle = CAVE2.GetWandObject(1).transform.eulerAngles.y;
                }
                if (CAVE2.Input.GetAxis(2, forwardAxis) != 0)
                {
                    forwardAngle = CAVE2.GetWandObject(2).transform.eulerAngles.y;
                }
            }

            if (CAVE2.Input.GetAxis(1, forwardAxis) != 0)
            {
                nextPos += CAVE2.GetWandObject(1).transform.rotation * Vector3.forward * forward * Time.deltaTime * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);
            }
            if (CAVE2.Input.GetAxis(2, forwardAxis) != 0)
            {
                nextPos += CAVE2.GetWandObject(2).transform.rotation * Vector3.forward * forward * Time.deltaTime * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);
            }
        }
        else // Normal case
        {
            if (forwardReference == ForwardRef.Head)
                forwardAngle = CAVE2.GetHeadObject(headID).transform.eulerAngles.y;
            else if (forwardReference == ForwardRef.Wand)
                forwardAngle = CAVE2.GetWandObject(wandID).transform.eulerAngles.y;

            nextPos += CAVE2.GetWandObject(wandID).transform.rotation * Vector3.forward * forward * Time.deltaTime * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);
        }

        nextPos.y += vertical * Time.deltaTime * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);

        if (horizontalMovementMode == HorizonalMovementMode.Strafe)
        {
            nextPos.z += strafe * Time.deltaTime * Mathf.Cos(Mathf.Deg2Rad * (forwardAngle + 90)) * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);
            nextPos.x += strafe * Time.deltaTime * Mathf.Sin(Mathf.Deg2Rad * (forwardAngle + 90)) * (smoothMovement ? flyMovementScale * 20 : flyMovementScale);

            if (smoothMovement)
            {
                transform.position = Vector3.SmoothDamp(transform.position, nextPos, ref velocity, smoothMovementTime);
            }
            else
            {
                transform.position = nextPos;
            }

            transform.Rotate(new Vector3(lookAround.x, lookAround.y, 0) * Time.deltaTime * turnSpeed);
        }
        else if (horizontalMovementMode == HorizonalMovementMode.Turn)
        {
            if (smoothMovement)
            {
                transform.position = Vector3.SmoothDamp(transform.position, nextPos, ref velocity, smoothMovementTime);
            }
            else
            {
                transform.position = nextPos;
            }

            transform.RotateAround(transform.position + transform.rotation * CAVE2.GetHeadPosition(headID), Vector3.up, strafe * Time.deltaTime * turnSpeed);
        }
    }
}
