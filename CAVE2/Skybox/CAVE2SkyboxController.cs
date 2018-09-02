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

public class CAVE2SkyboxController : MonoBehaviour {

    public LayerMask skyboxCullingMask;
    LayerMask lastCullingMask;

    public bool takeAScreenShot;
    public string screenshotPath = "CAVE2SkyboxScreenShots";
    public string screenshotLabel = "";

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if( lastCullingMask != skyboxCullingMask )
        {
            Camera[] cameras = gameObject.GetComponentsInChildren<Camera>();
            foreach( Camera c in cameras )
            {
                c.cullingMask = skyboxCullingMask;
            }
            lastCullingMask = skyboxCullingMask;
        }

        if( takeAScreenShot )
        {
            CubeMapScreenShot();
            takeAScreenShot = false;
        }
	}

    void CubeMapScreenShot()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();

        foreach(Camera c in cameras)
        {
            RenderTexture rc = c.targetTexture;

            RenderTexture tempRT = new RenderTexture(rc.width, rc.height, 24);
            c.targetTexture = tempRT;
            c.Render();

            RenderTexture.active = tempRT;

            Texture2D tx = new Texture2D(rc.width, rc.height, TextureFormat.RGB24, false);
            // false, meaning no need for mipmaps
            tx.ReadPixels(new Rect(0, 0, rc.width, rc.height), 0, 0);

            RenderTexture.active = null; //can help avoid errors 
            c.targetTexture = rc;
            Destroy(tempRT);

            byte[] bytes;
            bytes = tx.EncodeToPNG();

            System.IO.File.WriteAllBytes(Application.dataPath+"/"+ screenshotPath + "/"+ screenshotLabel +"-"+c.name+".png", bytes);
        }
    }
}
