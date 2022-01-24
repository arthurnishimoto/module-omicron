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
using UnityEngine.UI;

public class TransformUI : MonoBehaviour {

    public Text positionUIText;
    public Transform targetTransform;

    public bool local;

    protected Vector3 position;
    protected Vector3 eulerAngles;

    [SerializeField]
    bool showScale = false;

    // Use this for initialization
    void Start () {
        if (positionUIText == null && GetComponent<Text>())
            positionUIText = GetComponent<Text>();

        if (targetTransform == null)
            targetTransform = transform;
    }
	
	// Update is called once per frame
	void Update () {
        if (positionUIText)
        {
            if (local)
            {
                position = targetTransform.localPosition;
                eulerAngles = targetTransform.localEulerAngles;
            }
            else
            {
                position = targetTransform.position;
                eulerAngles = targetTransform.eulerAngles;
            }

            positionUIText.text = "Position: " + position.ToString("N3") + "\nRotation: " + eulerAngles.ToString("N3");
            
            if(showScale)
            {
                positionUIText.text += "\nScale: " + targetTransform.localScale.ToString("N3");
            }
        }
    }
}
