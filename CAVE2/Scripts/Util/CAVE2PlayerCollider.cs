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

public class CAVE2PlayerCollider : MonoBehaviour {

    // [SerializeField]
    // int headID = 1;

    [SerializeField]
    float bodyRadius = 0.3f;

    [SerializeField]
    CapsuleCollider bodyCollider;

    new Rigidbody rigidbody;
    Vector3 playerHeadPosition;

    [SerializeField]
    Collider[] playerColliders = null;

    // Use this for initialization
    void Start () {

        // Setup body collider
        if(bodyCollider == null)
        {
            bodyCollider = gameObject.AddComponent<CapsuleCollider>();
        }
        rigidbody = bodyCollider.GetComponent<Rigidbody>();
        if (rigidbody == null )
        {
            rigidbody = bodyCollider.gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        // Ignore collisions between body and any listed child coliders
        // as well as between child colliders
        Collider lastCollider = bodyCollider;
        foreach( Collider c in playerColliders )
        {
            Physics.IgnoreCollision(bodyCollider, c);
            Physics.IgnoreCollision(lastCollider, c);
            lastCollider = c; 
        }

        UpdatePlayerCollider();
    }

	void FixedUpdate () {
        UpdatePlayerCollider();
    }

    void UpdatePlayerCollider()
    {
        bodyCollider.radius = bodyRadius;
        playerHeadPosition = CAVE2.GetHeadPosition(GetComponent<CAVE2PlayerIdentity>().headID);

        // Prevent collider from height = 0, which causes falling through floors
        if (playerHeadPosition.y < 0.1f)
        {
            playerHeadPosition.y = 0.1f;
        }

        bodyCollider.height = playerHeadPosition.y;
        bodyCollider.center = new Vector3(playerHeadPosition.x, bodyCollider.height / 2.0f, playerHeadPosition.z);
    }
}
