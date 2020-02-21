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
using UnityEngine.VR;
using System.Collections;

public class CAVE2AdvancedTrackingSimulator : MonoBehaviour {

    public int headID = 1;

    public int wandID = 1;

    public bool wandUsesHeadPosition = false;
    public Vector3 wandDefaultPositionOffset = new Vector3(0.15f, 1.5f, 0.4f);

    public enum TrackingSimulatorMode { Head, Wand };

    [SerializeField]
    TrackingSimulatorMode IJKLHM_mode = TrackingSimulatorMode.Head;

    [SerializeField]
    float IJKLHM_MovementSpeed = 1;

    [SerializeField]
    float IJKLHM_RotationSpeed = 25;

    public KeyCode toggleMouseTracking = KeyCode.Tab;

    // bool mouseWandOffsetTriggered;
    public Vector3 mouseTrackingOffset;
    bool mouseInitSet;
    Vector3 mouseLastPosition;

	// Use this for initialization
	void Start () {
	
	}
	
    void Update()
    {
        
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (Input.GetKey(toggleMouseTracking))
        {
            if (!mouseInitSet)
            {
                mouseLastPosition = Input.mousePosition;
                mouseInitSet = true;
            }
            else
            {
                mouseTrackingOffset = (Input.mousePosition - mouseLastPosition) * Time.fixedDeltaTime * 0.1f;
                mouseLastPosition = Input.mousePosition;

                Vector3 wandPosition = CAVE2.GetCAVE2Manager().simulatorWandPosition;
                CAVE2.GetCAVE2Manager().simulatorWandPosition = wandPosition + mouseTrackingOffset;
                // mouseWandOffsetTriggered = true;
            }

        }
        else
        {
            mouseTrackingOffset = Vector3.zero;
            mouseInitSet = false;

            if (UnityEngine.XR.XRSettings.enabled)
            {
                // CAVE2InputManager processes tracking data
            }
            else
            { 
                if (CAVE2.GetCAVE2Manager().mocapEmulation)
                {
                    KeyboardHeadTracking();
                }
                if (CAVE2.GetCAVE2Manager().wandMousePointerEmulation)
                {
                    MouseWandPointerMode();
                }
            }
            
        }
	}

    void MouseWandPointerMode()
    {
        GameObject wandObject = CAVE2.GetWandObject(wandID);

        if (wandObject)
        {
            if (wandObject.GetComponent<Rigidbody>())
            {
                wandObject.GetComponent<Rigidbody>().MovePosition(CAVE2.Input.GetWandPosition(wandID));
            }
            else
            {
                wandObject.transform.localPosition = CAVE2.Input.GetWandPosition(wandID);
            }

            if(wandUsesHeadPosition)
            {
                wandObject.transform.localPosition = CAVE2.Input.GetHeadPosition(1);
            }
            else
            {
                wandObject.transform.localPosition = wandDefaultPositionOffset;
            }

            // Disable head collider if wand is attached to head
            if(CAVE2.GetHeadObject(1) != null)
            {
                SphereCollider headCollider = CAVE2.GetHeadObject(1).GetComponentInChildren<SphereCollider>();
                if(headCollider)
                    headCollider.enabled = !wandUsesHeadPosition;
            }

            // Mouse pointer ray controls rotation
            Vector2 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y);

            // Ray extending from main camera into screen from touch point
            Ray ray = CAVE2.GetCameraController().GetMainCamera().ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                //transform.LookAt( hit.point );
            }
            else
            {
                //transform.LookAt( ray.GetPoint(1000) );
            }
            wandObject.transform.LookAt(ray.GetPoint(1000));
            // Update the wandState rotation (opposite of normal since this object is determining the rotation)
            CAVE2.GetCAVE2Manager().simulatorWandPosition = wandObject.transform.localPosition;
            CAVE2.GetCAVE2Manager().simulatorWandRotation = wandObject.transform.localEulerAngles;
        }
    }

    void KeyboardHeadTracking()
    {
        Vector3 headPosition = CAVE2.GetCAVE2Manager().simulatorHeadPosition;
        Vector3 headRotation = CAVE2.GetCAVE2Manager().simulatorHeadRotation;

        Vector3 wandPosition = CAVE2.GetCAVE2Manager().simulatorWandPosition;
        // Vector3 wandRotation = CAVE2.GetCAVE2Manager().simulatorWandRotation;

        Vector3 translation = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if ( Input.GetKey(KeyCode.I))
        {
            translation.z += IJKLHM_MovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.K))
        {
            translation.z -= IJKLHM_MovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.J))
        {
            translation.x -= IJKLHM_MovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.L))
        {
            translation.x += IJKLHM_MovementSpeed * Time.fixedDeltaTime;
        }

        if (Input.GetKey(KeyCode.H))
        {
            translation.y += IJKLHM_MovementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.N))
        {
            if(headPosition.y > 0.1f)
                translation.y -= IJKLHM_MovementSpeed * Time.fixedDeltaTime;
        }

        if (Input.GetKey(KeyCode.U))
        {
            rotation.y -= IJKLHM_RotationSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.O))
        {
            rotation.y += IJKLHM_RotationSpeed * Time.fixedDeltaTime;
        }

        if (IJKLHM_mode == TrackingSimulatorMode.Head)
        {
            headPosition.z += translation.z * Mathf.Cos(Mathf.Deg2Rad * headRotation.y);
            headPosition.x += translation.z * Mathf.Sin(Mathf.Deg2Rad * headRotation.y);

            headPosition.z += translation.x * Mathf.Cos(Mathf.Deg2Rad * (90 + headRotation.y));
            headPosition.x += translation.x * Mathf.Sin(Mathf.Deg2Rad * (90 + headRotation.y));

            headPosition.y += translation.y;

            headRotation.y += rotation.y;

            CAVE2.GetCAVE2Manager().simulatorHeadPosition = headPosition;
            CAVE2.GetCAVE2Manager().simulatorHeadRotation = headRotation;
        }
        else if (IJKLHM_mode == TrackingSimulatorMode.Wand)
        {
            wandPosition.z += translation.z * Mathf.Cos(Mathf.Deg2Rad * headRotation.y);
            wandPosition.x += translation.z * Mathf.Sin(Mathf.Deg2Rad * headRotation.y);

            wandPosition.z += translation.x * Mathf.Cos(Mathf.Deg2Rad * (90 + headRotation.y));
            wandPosition.x += translation.x * Mathf.Sin(Mathf.Deg2Rad * (90 + headRotation.y));

            wandPosition.y += translation.y;

            wandDefaultPositionOffset = wandPosition;
        }

    }
}
