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

[ExecuteInEditMode]
public class CAVE2ScreenMaskRenderer : MonoBehaviour {

    public enum RenderMode { None, Background, Overlay }
    public RenderMode renderMode = RenderMode.Background;

    [SerializeField]
    bool showInHMDVR = false;

    void Awake()
    {
#if UNITY_EDITOR
        if (!showInHMDVR && CAVE2.UsingHMDVR())
        {
            renderMode = RenderMode.None;
        }
#endif
    }

    void Start()
    {
#if !UNITY_EDITOR
        if ( CAVE2.OnCAVE2Display() )
        {
            GetComponent<Renderer>().enabled = false;
        }
#endif
    }

	// Update is called once per frame
	void Update () {
	    
        switch(renderMode)
        {
            case (RenderMode.Background):
                GetComponent<Renderer>().sharedMaterial.SetFloat("_ZTest", 2);
                GetComponent<Renderer>().sharedMaterial.SetFloat("_Cull", 2);
                break;
            case (RenderMode.None):
                GetComponent<Renderer>().sharedMaterial.SetFloat("_ZTest", 1);
                GetComponent<Renderer>().sharedMaterial.SetFloat("_Cull", 0);
                break;
            case (RenderMode.Overlay):
                GetComponent<Renderer>().sharedMaterial.SetFloat("_ZTest", 0);
                GetComponent<Renderer>().sharedMaterial.SetFloat("_Cull", 0);
                break;
        }
	}
}
